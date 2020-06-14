using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Net.NetworkInformation;
using TaleWorlds.CampaignSystem;
using Helpers;

namespace CheerReloaded {
	internal class CheerBehaviour : MissionBehaviour {
		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		private readonly Config _config;
		private readonly ActionIndexCache[] _cheerActions = new ActionIndexCache[] {
			ActionIndexCache.Create("act_cheer_1"),
			ActionIndexCache.Create("act_cheer_2"),
			ActionIndexCache.Create("act_cheer_3"),
			ActionIndexCache.Create("act_cheer_4")
		};
		private float _moraleChange;
		private float _effectRadius;
		private int _cheerAmount;
		private bool _canCheer;

		public CheerBehaviour(Config config) {
			_config = config;
			_cheerAmount = config.CheerAmount;
			_canCheer = true;
		}

		/// <summary>
		/// Runs on every tick of the game. Considering it runs extremely often only the lightest things should
		/// appear here, such as user input handling.
		/// </summary>
		public override async void OnMissionTick(float dt) {
			if (Input.IsKeyReleased((InputKey)_config.KeyCode) && _canCheer) {
				_canCheer = false;

				if (!Mission.Current.MissionEnded()) {
					await DoInCombatCheer();
					await Task.Delay(TimeSpan.FromSeconds(3));
				} else {
					await DoVictoryCheer();
					await Task.Delay(TimeSpan.FromSeconds(1));
				}

				_canCheer = true;
			}
		}

		/// <summary>
		/// Calculates effect radius and a list of agents to be affected. Ignores cheer limit.
		/// </summary>
		private async Task DoVictoryCheer() {
			var leadership = Agent.Main.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
			_effectRadius = leadership.Clamp(50, 400);

			try {
				var agentList = Mission.GetAgentsInRange(Agent.Main.Position.AsVec2, _effectRadius)
									.Where(x => x.IsMount == false)
									.Where(x => x.Health > 0)
									.Where(x => x.IsMainAgent == false)
									.Where(x => x.Team.IsFriendOf(Agent.Main.Team))
									.ToList();

				#pragma warning disable CS4014 // don't await, we want everyone to cheer at the same time
				ApplyCheering(Agent.Main);
				await Task.Delay(TimeSpan.FromSeconds(0.65));

				foreach (var a in agentList) {
					var rng = new Random();
					ApplyCheering(a);
					await Task.Delay(rng.Next(0, 200));
				}
				#pragma warning restore CS4014
			} catch (Exception ex) {
				if (_config.DebugMode) {
					Helpers.Log(ex.Message);
					Clipboard.SetText(ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// Calculates morale changed, effect radius, a list of agents to be affected. Handles morale report and applies XP increase.
		/// </summary>
		private async Task DoInCombatCheer() {
			if (_cheerAmount < 1) {
				Helpers.Say("You wanted to perform a war cry, but felt you would only make a fool of yourself.");
				return;
			}

			_cheerAmount--;

			var leadership = Agent.Main.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
			float advantageBonus = (Agent.Main.Team.ActiveAgents.Count
								 - Mission.Current.Teams.GetEnemiesOf(Agent.Main.Team).Count())
								 / 12;

			_moraleChange = (leadership / 18) + advantageBonus;
			_effectRadius = (leadership / 2).Clamp(25, 200);

			if (_config.PreventNegativeMorale) {
				_moraleChange.Clamp(0, 100);
			}

			try {
				var agentsList = Mission.GetAgentsInRange(Agent.Main.Position.AsVec2, _effectRadius)
									.Where(x => x.IsMount == false)
									.Where(x => x.Health > 0)
									.Where(x => x.IsMainAgent == false);

				var friendlyAgentsList = agentsList.Where(x => x.Team.IsFriendOf(Agent.Main.Team)).ToList();
				var enemyAgentsList = agentsList.Where(x => x.Team.IsEnemyOf(Agent.Main.Team)).ToList();

				#pragma warning disable CS4014 // don't await, we want everyone to cheer at the same time
				ApplyCheering(Agent.Main);
				await Task.Delay(TimeSpan.FromSeconds(0.65));

				var totalFriendlyMoraleApplied = 0;
				foreach (var a in friendlyAgentsList) {
					ApplyCheering(a);
					var rng = new Random();					
					await Task.Delay(rng.Next(0, 25));
					totalFriendlyMoraleApplied += ApplyMoraleChange(a);
				}
				#pragma warning restore CS4014

				var totalEnemyMoraleApplied = 0;
				if (leadership >= _config.EnemyMoraleLeadershipThreshold) {
					foreach (var a in enemyAgentsList) {
						totalEnemyMoraleApplied += ApplyMoraleChange(a, true);
						a.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					}
				}

				if (_config.ReportMoraleChange) {
					if (totalFriendlyMoraleApplied > 0) {
						Helpers.Say($"Each party member in the area received {_moraleChange} morale, {totalFriendlyMoraleApplied} in total.");
						if (leadership >= _config.EnemyMoraleLeadershipThreshold && totalEnemyMoraleApplied > 0) {
							Helpers.Say($"In addition, enemies lost {totalEnemyMoraleApplied} morale.");
						}
					} else if (totalFriendlyMoraleApplied < 0) {
						Helpers.Say($"Your own soldiers felt demoralized by your battle cries. {_moraleChange} for each, {totalFriendlyMoraleApplied} in total.");
						if (leadership >= _config.EnemyMoraleLeadershipThreshold && totalEnemyMoraleApplied > 0) {
							Helpers.Say($"This caused nearby enemies to gain {totalEnemyMoraleApplied} morale.");
						}
					} else {
						Helpers.Say("You failed to affect any soldiers' morale.");
					}
				}

				if (!(Campaign.Current is null)) {
					var mainHero = Hero.All.Where(x => x.StringId == Agent.Main.Character.StringId).FirstOrDefault();
					mainHero.AddSkillXp(DefaultSkills.Leadership, totalFriendlyMoraleApplied);
				}

			} catch (Exception ex) {
				if (_config.DebugMode) {
					Helpers.Log(ex.Message);
					Clipboard.SetText(ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		/// <summary>
		/// Applies audio-visual effects.
		/// </summary>
		/// <param name="a">An agent to apply cheering to.</param>
		/// <returns></returns>
		private async Task ApplyCheering(Agent a) {
			// i hate checking for a fucking null mission before i do anything so fucking much
			if (Mission.Current == null) return;

			a.SetActionChannel(1, _cheerActions[MBRandom.RandomInt(_cheerActions.Length)], actionSpeed: 1.5f);
			a.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
			await Task.Delay(TimeSpan.FromSeconds(2.5));
			// you need to ignore priority to break previous animation - took me way too long to figure out
			if (Mission.Current != null) {
				a.SetActionChannel(1, ActionIndexCache.act_none, ignorePriority: true);
			}
		}

		/// <summary>
		/// Applies morale changes.
		/// </summary>
		/// <param name="a">An agent to apply morale changes to.</param>
		/// <param name="inverted">Inverts morale amount, i.e. 3 morale turns into -3 morale</param>
		/// <returns></returns>
		private int ApplyMoraleChange(Agent a, bool inverted = false) {
			var currentMorale = a.GetMorale();
			if (inverted) {
				a.SetMorale((currentMorale + _moraleChange) * -1);
			} else {
				a.SetMorale(currentMorale + _moraleChange);
			}
			return (int)_moraleChange;
		}
	}
}
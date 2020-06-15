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
			ActionIndexCache.Create("act_command_bow"),
			ActionIndexCache.Create("act_command_follow_bow"),
			ActionIndexCache.Create("act_command_unarmed"),
			ActionIndexCache.Create("act_command_unarmed_leftstance"),
			ActionIndexCache.Create("act_command_follow_unarmed"),
			ActionIndexCache.Create("act_command_follow_unarmed_leftstance"),
			ActionIndexCache.Create("act_command_2h"),
			ActionIndexCache.Create("act_command_2h_leftstance"),
			ActionIndexCache.Create("act_command"),
			ActionIndexCache.Create("act_command_follow"),
			ActionIndexCache.Create("act_command_leftstance"),
			ActionIndexCache.Create("act_command_follow_2h"),
			ActionIndexCache.Create("act_command_follow_2h_leftstance"),
			ActionIndexCache.Create("act_command_follow_leftstance")
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
				if (Mission.Current.Mode != MissionMode.Battle) return;

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
			try {
				var leadership = Agent.Main.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
				_effectRadius = leadership.Clamp(50, 400);

				var agentList = Mission.GetAgentsInRange(Agent.Main.Position.AsVec2, _effectRadius)
									.Where(x => x.IsMount == false)
									.Where(x => x.Health > 0)
									.Where(x => x.IsMainAgent == false)
									.Where(x => x.Team.IsFriendOf(Agent.Main.Team))
									.ToList();

				ApplyCheerEffects(Agent.Main);
				await Task.Delay(TimeSpan.FromSeconds(0.65));

				foreach (var a in agentList) {
					ApplyCheerEffects(a);
					await Task.Delay(MBRandom.RandomInt(0, 20));
				}
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

			try {
				var leadership = Agent.Main.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
				float advantageBonus = ((Agent.Main.Team.ActiveAgents.Count
									 - Mission.Current.Teams.GetEnemiesOf(Agent.Main.Team).Count())
									 / 12)
									 .Clamp(-2, 2);

				var mCap = _config.MaximumMoralePerAgent;
				_moraleChange = ((leadership / 18) + advantageBonus).Clamp(mCap * -1, mCap);
				_effectRadius = (leadership / 2).Clamp(25, 200);

				if (_config.PreventNegativeMorale) {
					_moraleChange.Clamp(0, 100);
				}

				var agentsList = Mission.GetAgentsInRange(Agent.Main.Position.AsVec2, _effectRadius)
									.Where(x => x.IsMount == false)
									.Where(x => x.Health > 0)
									.Where(x => x.IsMainAgent == false);

				var friendlyAgentsList = agentsList.Where(x => x.Team.IsFriendOf(Agent.Main.Team)).ToList();
				var enemyAgentsList = agentsList.Where(x => x.Team.IsEnemyOf(Agent.Main.Team)).ToList();

				var totalFriendlyMoraleApplied = 0;
				var totalEnemyMoraleApplied = 0;
				ApplyCheerEffects(Agent.Main);
				await Task.Delay(TimeSpan.FromSeconds(0.65));

				foreach (var a in friendlyAgentsList) {
					ApplyCheerEffects(a);
					await Task.Delay(MBRandom.RandomInt(0, 20));
					totalFriendlyMoraleApplied += ApplyMoraleChange(a);
				}

				if (leadership >= _config.EnemyMoraleLeadershipThreshold) {
					foreach (var a in enemyAgentsList) {
						totalEnemyMoraleApplied += ApplyMoraleChange(a, true);
						ApplyCheerEffects(a, false);
						a.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					}
				}

				var xpToGrant = (totalFriendlyMoraleApplied + (totalEnemyMoraleApplied * -1)).Clamp(0, 6000);

				if (!(Campaign.Current is null)) {
					var mainHero = Hero.All.Where(x => x.StringId == Agent.Main.Character.StringId).FirstOrDefault();
					if (xpToGrant <= 0) {
						xpToGrant += 10;
					}
					mainHero.AddSkillXp(DefaultSkills.Leadership, xpToGrant);
				}


				if (_config.ReportMoraleChange) {
					if (totalFriendlyMoraleApplied > 0) {
						Helpers.Say($"Each party member in the area received {_moraleChange} morale, {totalFriendlyMoraleApplied} in total.");
						if (leadership >= _config.EnemyMoraleLeadershipThreshold && totalEnemyMoraleApplied > 0) {
							Helpers.Say($"In addition, enemies lost {totalEnemyMoraleApplied} morale.");
						}
						_cheerAmount--;
					} else if (totalFriendlyMoraleApplied < 0) {
						Helpers.Say($"Your own soldiers felt demoralized by your battle cries. {_moraleChange} for each, {totalFriendlyMoraleApplied} in total.");
						if (leadership >= _config.EnemyMoraleLeadershipThreshold && totalEnemyMoraleApplied > 0) {
							Helpers.Say($"This caused nearby enemies to gain {totalEnemyMoraleApplied} morale.");
						}
						_cheerAmount--;
					} else {
						Helpers.Say("You failed to affect any soldiers' morale.");
					}
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
		private void ApplyCheerEffects(Agent a, bool doAnim = true, bool doVoice = true) {
			if (Mission.Current == null) return;

			if (doAnim) {
				// additionalFlags: it seems like anything past 2 means "can be cancelled by other actions"
				a.SetActionChannel(1, _cheerActions[MBRandom.RandomInt(_cheerActions.Length)], additionalFlags: 2);
			}

			if (!doVoice) return;

			// i know ugly as hell
			if (_moraleChange >= 0) {
				if (_moraleChange.IsInRange(0, 1)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Grunt, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (_moraleChange.IsInRange(1, 3, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (_moraleChange.IsInRange(3, 5, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.FaceEnemy, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (_moraleChange.IsInRange(5, null, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
			} else {
				if (_moraleChange.IsInRange(-1, 0, true, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.FallBack, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (_moraleChange.IsInRange(-3, -1, true, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Fear, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (_moraleChange.IsInRange(null, -3, true, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Retreat, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
			}


		}

		/// <summary>
		/// Applies morale changes.
		/// </summary>
		/// <param name="a">An agent to apply morale changes to.</param>
		/// <param name="inverted">Inverts morale amount, i.e. 3 morale turns into -3 morale</param>
		/// <returns>Amount of morale that was applied.</returns>
		private int ApplyMoraleChange(Agent a, bool inverted = false) {
			var currentMorale = a.GetMorale();
			if (inverted) {
				var invertedMorale = _moraleChange / 2;
				a.SetMorale(currentMorale - invertedMorale);
				return (int)invertedMorale;
			} else {
				a.SetMorale(currentMorale + _moraleChange);
				return (int)_moraleChange;
			}
		}
	}
}
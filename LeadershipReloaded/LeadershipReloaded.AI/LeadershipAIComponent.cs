using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using LeadershipReloaded.Common;
using LeadershipReloaded.Settings;

namespace LeadershipReloaded.AI {
	internal class LeadershipAIComponent : AgentComponent {
		public int CheerRange;
		public int _cheerAmount;

		private readonly Config _config;
		private readonly Strings _strings;
		private readonly float _initialMorale;
		private readonly int _leadership;
		private readonly Agent _agent;
		private readonly CheerCommonMethods _common;
		private float _moraleChange;
		private float _timerToEnableCheering;
		private bool _canCheer;
		private IEnumerable<Agent> _agentsInArea;
		private readonly BattleSideEnum _playerSide;


		/// <summary>
		/// Allows AI to cheer under almost the same rules as the player.
		/// </summary>
		public LeadershipAIComponent(Config config, Agent agent, CheerCommonMethods common, Strings strings) : base(agent) {
			_config = config;
			_strings = strings;
			_agent = agent;
			_common = common;
			_leadership = agent.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
			_cheerAmount = _config.AI.BaselineCheerAmount;
			_cheerAmount += Math.DivRem(_leadership, _config.Cheering.CheersPerXLeadershipLevels, out _);
			_initialMorale = _agent?.GetMorale() ?? 0;
			CheerRange = (_leadership / 2).Clamp(50, 200);
			_canCheer = false;
			_timerToEnableCheering = MBCommon.TimeType.Mission.GetTime() + MBRandom.RandomInt(8, 13);
			_playerSide = agent.Team.Side;
		}

		protected override void OnTickAsAI(float dt) {
			if (Mission.Current != null && Mission.Current.Mode != MissionMode.Battle) return;
			if (_agent.Health < 1) return;
			if (_agent.Team == null) return;

			if (MBCommon.TimeType.Mission.GetTime() > _timerToEnableCheering) _canCheer = true;

			if (!_canCheer) return;
			if (_cheerAmount == 0) {
				_agent.RemoveComponent(this);
				return;
			};


			var lowestMorale = _initialMorale; // default

			_agentsInArea = Mission.Current.GetAgentsInRange(_agent.Position.AsVec2, CheerRange)
											.Where(x => x != null)
											.Where(x => x.Character != null && x.IsHuman)
											.Where(x => x.IsFriendOf(_agent));

			if (_agentsInArea.Count() > 0) {
				lowestMorale = _agentsInArea.Min(x => x.GetMorale());
				foreach (var a in _agentsInArea) {
					if (!_canCheer) break;
					if (a.IsEnemyOf(_agent)) break;

					if (lowestMorale < _initialMorale - 3f) {					
						Cheer();					
					}
				}
			}
		}

		/// <summary>
		/// Calculates morale changed. This one is stripped down for AI for performance reasons.
		/// </summary>
		private void Cheer() {
			_timerToEnableCheering = MBCommon.TimeType.Mission.GetTime() + MBRandom.RandomInt(4, 7);
			_canCheer = false;

			if (_config.AI.DisplayAnnouncement) {
				Helpers.Announce("{=lord_cheered}" + _strings.Lord.Cheered
								.Replace("$NAME$", _agent.Name)
								.Replace("$HISHERUPPER$", _agent.IsFemale ? "Her" : "His")
								.Replace("$HISHERLOWER$", _agent.IsFemale ? "her" : "his"),
								_agent.Character,
								new Dictionary<string, TextObject> {
									{ "NAME", new TextObject(_agent.Name) },
									{ "HISHERUPPER", new TextObject(_agent.IsFemale ? "Her" : "His") },
									{ "HISHERLOWER", new TextObject(_agent.IsFemale ? "her" : "his") }
								}
				);
			}

			float playerPower = 0;
			float enemyPower = 0;
			int mCap = _config.AI.MaximumMoralePerAgent;
			int aCap = _config.AI.MaximumAdvantageMorale;
			var teams = Mission.Current.Teams;

			foreach (Team t in teams) {
				// formation list may change, have to store it first
				var formations = t.Formations; 
				foreach (Formation f in formations) {
					if (f.Team.Side == _playerSide) {
						playerPower += f?.GetFormationPower() ?? 0f;
					} else {
						enemyPower += f?.GetFormationPower() ?? 0f;
					}
				}
			}

			float advantageBonus = ((playerPower - enemyPower) / 40).Clamp(aCap * -1, aCap);

			_moraleChange = (int)Math.Round(((_leadership / 18) + advantageBonus).Clamp(mCap * -1, mCap));

			if (_config.Cheering.PreventNegativeMorale) {
				_moraleChange.Clamp(0, 100);
			}

			var friendlyAgentsList = _agentsInArea.Where(x => x.Team.IsFriendOf(_agent.Team));
			var enemyAgentsList = _agentsInArea.Where(x => x.Team.IsEnemyOf(_agent.Team));

			var totalFriendlyMoraleApplied = 0;
			var totalEnemyMoraleApplied = 0;

			foreach (var a in friendlyAgentsList) {
				_common.ApplyCheerEffects(a, _moraleChange);
				totalFriendlyMoraleApplied += _common.ApplyMoraleChange(a, _moraleChange, noNegativeMorale: _config.Cheering.PreventNegativeMorale);
			}

			if (_leadership >= _config.Cheering.EnemyMoraleLeadershipThreshold) {
				foreach (var a in enemyAgentsList) {
					totalEnemyMoraleApplied += _common.ApplyMoraleChange(a, _moraleChange, true);
					_common.ApplyCheerEffects(a, _moraleChange, false);
					a.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
				}
			}

			_cheerAmount--;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CheerReloaded {
	internal class CheerAIComponent : AgentComponent {
		public int CheerRange;
		public int _cheerAmount;

		private readonly Config _config;
		private readonly float _initialMorale;
		private readonly int _leadership;
		private readonly Agent _agent;
		private readonly CheerCommonMethods _common;
		private float _moraleChange;
		private float _timerToEnableCheering;
		private bool _canCheer;
		private IEnumerable<Agent> _agentsInArea;


		/// <summary>
		/// Allows AI to cheer under almost the same rules as the player.
		/// </summary>
		public CheerAIComponent(Config config, Agent agent, CheerCommonMethods common) : base(agent) {
			_config = config;
			_agent = agent;
			_common = common;
			_leadership = agent.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
			_cheerAmount = _config.AI.BaselineCheerAmount;
			_cheerAmount += Math.DivRem(_leadership, _config.CheersPerXLeadershipLevels, out _);
			_initialMorale = _agent?.GetMorale() ?? 0;
			CheerRange = (_leadership / 2).Clamp(50, 200);
			_canCheer = false;
			_timerToEnableCheering = MBCommon.TimeType.Mission.GetTime() + MBRandom.RandomInt(8, 13);
		}

		protected override void OnTickAsAI(float dt) {
			if (Mission.Current != null && Mission.Current.Mode != MissionMode.Battle) return;
			if (MBCommon.TimeType.Mission.GetTime() > _timerToEnableCheering) {
				_canCheer = true;
			}
			if (_agent.Team == null) return;

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
				Helpers.Announce($"{_agent.Name} cheers, boosting {(_agent.IsFemale ? "her" : "his")} allies' morale!");
			}

			var mCap = _config.AI.MaximumMoralePerAgent;
			_moraleChange = ((_leadership / 18) + 1f).Clamp(-mCap, mCap);

			if (_config.PreventNegativeMorale) {
				_moraleChange.Clamp(0, 100);
			}

			var friendlyAgentsList = _agentsInArea.Where(x => x.Team.IsFriendOf(_agent.Team)).ToList();
			var enemyAgentsList = _agentsInArea.Where(x => x.Team.IsEnemyOf(_agent.Team)).ToList();

			var totalFriendlyMoraleApplied = 0;
			var totalEnemyMoraleApplied = 0;

			foreach (var a in friendlyAgentsList) {
				_common.ApplyCheerEffects(a, _moraleChange);
				totalFriendlyMoraleApplied += _common.ApplyMoraleChange(a, _moraleChange);
			}

			if (_leadership >= _config.EnemyMoraleLeadershipThreshold) {
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
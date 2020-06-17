using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CheerReloaded {
	internal class CheerAIComponent : AgentComponent {
		public int CheerRange = 50;
		public int _cheerAmount;

		private readonly Config _config;
		private readonly int _leadership;
		private readonly Agent _agent;
		private readonly CheerCommonMethods _common;
		private float _moraleChange;
		private float _missionTimer;
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
			_canCheer = true;
			_timerToEnableCheering = MBCommon.TimeType.Mission.GetTime() + 20f; // start a bit later
		}

		protected override void OnTickAsAI(float dt) {
			_missionTimer = MBCommon.TimeType.Mission.GetTime();
			if (_missionTimer > _timerToEnableCheering)
				_canCheer = true;

			if (!_canCheer) return;
			if (_cheerAmount == 0) {
				_agent.RemoveComponent(this);
				return;
			};

			if (_agent.Team == null) return;

			_agentsInArea = Mission.Current.GetAgentsInRange(_agent.Position.AsVec2, CheerRange)
											.Where(x => x.Character != null);

			foreach (var a in _agentsInArea) {
				if (!_canCheer) break;
				if (!a.IsFriendOf(_agent)) break;

				if (a.GetMorale() < 33f) {
					_timerToEnableCheering = _missionTimer + 5f;
					Cheer();
					_canCheer = false;
					if (_config.AI.DisplayAnnouncement) {
						Helpers.Announce($"{_agent.Name} cheers, boosting {(_agent.IsFemale ? "her" : "his")} allies' morale!");
					}
				}
			}
		}

		/// <summary>
		/// Calculates morale changed. This one is stripped down for AI for performance reasons.
		/// </summary>
		private void Cheer() {
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
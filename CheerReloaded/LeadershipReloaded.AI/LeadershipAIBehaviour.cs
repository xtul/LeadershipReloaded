using LeadershipReloaded.Settings;
using LeadershipReloaded.Common;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using System;
using TaleWorlds.CampaignSystem.Actions;

namespace LeadershipReloaded.AI {
	internal class LeadershipAIBehaviour : MissionBehaviour {
		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		private readonly Config _config;
		private readonly Strings _strings;
		private readonly CheerCommonMethods _common;
		private readonly List<Agent> _personalDeathEffectAgentList;

		public LeadershipAIBehaviour(Config config, CheerCommonMethods common, Strings strings) {
			_config = config;
			_strings = strings;
			_common = common;
			_personalDeathEffectAgentList = new List<Agent>();
		}

		public override void OnAgentCreated(Agent agent) {
			if (agent == null) return;
			if (agent.IsMount) return;
			if (agent.IsMainAgent) return;
			if (agent.Character == null) return;

			if (agent.IsHero) {
				agent.AddComponent(new CheerAIComponent(_config, agent, _common, _strings));
				if (_config.AI.ImpactfulDeath) {
					agent.OnAgentHealthChanged += OnHitPointsChanged;
				}
				if (_config.AI.PersonalEffects.Enabled) {
					_personalDeathEffectAgentList.Add(agent);
				}
			}
 		}

		private void OnHitPointsChanged(Agent agent, float oldHealth, float newHealth) {
			if (newHealth < 1f) {
				var agentsToAffect = Mission.Current.GetAgentsInRange(agent.Position.AsVec2, 75f)
													.Where(x => x != null)
													.Where(x => x.IsMount == false)
													.Where(x => x.Character != null)
													.Where(x => x.Health > 0)
													.Where(x => x.IsMainAgent == false)
													.Where(x => x.IsFriendOf(agent));
				
				foreach (var a in agentsToAffect) {
					a.SetMorale(a.GetMorale() - _config.AI.DeathMoraleDecrease);
				}

				Helpers.Announce("{=lord_died}" + _strings.Lord.Died
								.Replace("$NAME$", agent.Name)
								.Replace("$MORALEHIT$", _config.AI.DeathMoraleDecrease.ToString())
								.Replace("$HISHERUPPER$", agent.IsFemale ? "Her" : "His")
								.Replace("$HISHERLOWER$", agent.IsFemale ? "her" : "his"),
								agent.Character,
								new Dictionary<string, TextObject> {
									{ "NAME", new TextObject(agent.Name) },
									{ "MORALEHIT", new TextObject(_config.AI.DeathMoraleDecrease)},
									{ "HISHERUPPER", new TextObject(agent.IsFemale ? "Her" : "His") },
									{ "HISHERLOWER", new TextObject(agent.IsFemale ? "her" : "his") }
								}

				);
			}
		}

		public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow) {
			if (Campaign.Current == null) return;
			if (affectedAgent.Character == null) return;
			if (!affectedAgent.IsHero) return;

			if (_config.AI.PersonalEffects.Enabled) {
				if (_personalDeathEffectAgentList.Contains(affectedAgent) && affectorAgent == Agent.Main) {
					var killedHero = Hero.FindFirst(x => x.StringId == affectedAgent.Character.StringId);
					var playerHero = Hero.FindFirst(x => x.StringId == affectorAgent.Character.StringId);
					
					ChangeRelationAction.ApplyPlayerRelation(killedHero, _config.AI.PersonalEffects.RelationshipChange, false, true);
					playerHero.Clan.AddRenown(_config.AI.PersonalEffects.RenownGain);

					Helpers.Say("{=death_personal_effect}" + _strings.Lord.DeathPersonalEffect
								.Replace("$NAME$", affectedAgent.Name)
								.Replace("$RENOWN$", _config.AI.PersonalEffects.RenownGain.ToString())
								.Replace("$RELATIONSHIPHIT$", _config.AI.PersonalEffects.RelationshipChange.ToString()),
								new Dictionary<string, TextObject> {
									{ "NAME", new TextObject(affectedAgent.Name) },
									{ "RENOWN", new TextObject(_config.AI.PersonalEffects.RenownGain.ToString()) },
									{ "RELATIONSHIPHIT", new TextObject(_config.AI.PersonalEffects.RelationshipChange.ToString()) }
								});
				}
			}
		}
	}
}
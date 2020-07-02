using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using CheerReloaded.Settings;

namespace CheerReloaded.Responsive {
	internal class CheerResponsiveOrdersBehaviour : MissionBehaviour {
		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		private readonly Config _config;
		private OrderController _orderController;
		private int _playerLeadership;
		private readonly Random _rng;
		private int _affirmativeAgentCounter;
		private int _affirmativeAgentMaxCount;

		private OrderType _orderType;

		public CheerResponsiveOrdersBehaviour(Config config) {
			_config = config;
			_rng = new Random();
		}

		public override async void OnAgentBuild(Agent agent, Banner banner) {
			if (agent == null) return;
			if (banner == null) return;

			if (agent == Agent.Main) {
				_orderController = agent.Team?.PlayerOrderController ?? null;
				_playerLeadership = agent.Character?.GetSkillValue(DefaultSkills.Leadership) ?? 0;
				_affirmativeAgentMaxCount = _config.ResponsiveOrders.BaselineResponseCount 
											+ (int)Math.Round(_playerLeadership / 25f);
				_orderController.OnOrderIssued += ReactToIssuedOrder;
				// we are waiting a while before subscribing to events - otherwise 
				// the initial unit selection will trigger cheering and it feels awkward
				await Task.Delay(1000);
				_orderController.OnSelectedFormationsChanged += ReactToChangedFormations;
			}
		}

		public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow) {
			if (affectedAgent == Agent.Main) {
				_orderController.OnOrderIssued -= ReactToIssuedOrder;
				_orderController.OnSelectedFormationsChanged -= ReactToChangedFormations;
			}
		}

		private async void Affirmative(Agent a) {
			_affirmativeAgentCounter++;
			if (a == Agent.Main) return;

			if (_affirmativeAgentCounter > _affirmativeAgentMaxCount) return;

			var agentPosition = a.Position;
			var distanceToPlayer = Agent.Main.GetPathDistanceToPoint(ref agentPosition);
			var timeToRespond = (int)(_rng.Next(900, 1200) * (distanceToPlayer / 10)).Clamp(900, 2000);

			await Task.Delay(timeToRespond);

			// ;_;
			if (Mission.Current != null && !Mission.Current.MissionEnded()) { 
				switch (_orderType) {
					case OrderType.Advance:
						a.MakeVoice(SkinVoiceManager.VoiceType.Advance, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementCircular:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormCircle, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementColumn:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormColumn, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementLine:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormLine, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementLoose:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormLoose, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementScatter:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormScatter, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementSchiltron:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormShieldWall, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ArrangementVee:
						a.MakeVoice(SkinVoiceManager.VoiceType.FormSkein, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.AttackEntity:
						a.MakeVoice(SkinVoiceManager.VoiceType.MpAttack, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.Charge:
						a.MakeVoice(SkinVoiceManager.VoiceType.Charge, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.ChargeWithTarget:
						a.MakeVoice(SkinVoiceManager.VoiceType.Charge, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.Dismount:
						a.MakeVoice(SkinVoiceManager.VoiceType.Dismount, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.FallBack:
						a.MakeVoice(SkinVoiceManager.VoiceType.FallBack, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.FallBackTenPaces:
						a.MakeVoice(SkinVoiceManager.VoiceType.FallBack, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.FireAtWill:
						a.MakeVoice(SkinVoiceManager.VoiceType.FireAtWill, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.FollowEntity:
						a.MakeVoice(SkinVoiceManager.VoiceType.Follow, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.FollowMe:
						a.MakeVoice(SkinVoiceManager.VoiceType.Follow, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.HoldFire:
						a.MakeVoice(SkinVoiceManager.VoiceType.HoldFire, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.Mount:
						a.MakeVoice(SkinVoiceManager.VoiceType.Mount, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					case OrderType.Move:
						a.MakeVoice(SkinVoiceManager.VoiceType.Follow, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
					default: 
						a.MakeVoice(SkinVoiceManager.VoiceType.Grunt, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
						break;
				}			
			}
		}

		private async void Grunt(Agent a) {
			if (_affirmativeAgentCounter > _affirmativeAgentMaxCount * 2) return;
			if (a == Agent.Main) return;

			try {
				var agentPosition = a.Position;

				var distanceToPlayer = agentPosition.Distance(Agent.Main.Position);
				if (distanceToPlayer > 35f) return;

				_affirmativeAgentCounter++;

				var timeToRespond = (int)(_rng.Next(700, 900) * (distanceToPlayer / 10)).Clamp(500, 1200);

				await Task.Delay(timeToRespond);

				var reactionList = new Action[] {
					() => a.MakeVoice(SkinVoiceManager.VoiceType.Grunt, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction),
					() => a.MakeVoice(SkinVoiceManager.VoiceType.Everyone, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction),
					() => a.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction)
				};

				if (Mission.Current != null && !Mission.Current.MissionEnded()) {
					reactionList[MBRandom.RandomInt(0, 2)].Invoke();
				}
			} catch { }
		}

		private async void Charge(Agent a) {
			if (a == Agent.Main) return;

			await Task.Delay(_rng.Next(600, 2000));

			if (Mission.Current != null && !Mission.Current.MissionEnded()) {
				a.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);				
			}			
		}

		private void ReactToChangedFormations() {
			if (Mission.Current == null) return;
			try {
				foreach (var formation in _orderController.SelectedFormations) {
					formation.ApplyActionOnEachUnit(Grunt);
					_affirmativeAgentCounter = 0;
				}
			} catch { }
		}

		private async void ReactToIssuedOrder(OrderType orderType, IEnumerable<Formation> appliedFormations, params object[] delegateParams) {
			if (Mission.Current == null) return;
			try {
				_orderType = orderType;
				if (_config.ResponsiveOrders.ContinuousChargeYell == true && (orderType == OrderType.Charge || orderType == OrderType.ChargeWithTarget)) {
					for (int i = 0; i < _rng.Next(8, 15); i++) {
						foreach (var formation in appliedFormations) {
							var timeGate = MBCommon.GetTime(MBCommon.TimeType.Mission) + (_rng.Next(3, 9) / 10f);
							while (Mission.Current != null) {
								if (timeGate > MBCommon.GetTime(MBCommon.TimeType.Mission)) {
									await Task.Delay(300);
									continue;
								}
								formation.ApplyActionOnEachUnit(Charge);
								break;
							}
						}
					}
					_affirmativeAgentCounter = 0;
					return;
				}
				// default reaction
				foreach (var formation in appliedFormations) {
					formation.ApplyActionOnEachUnit(Affirmative);
					_affirmativeAgentCounter = 0;
				}
			} catch { }
		}
	}
}
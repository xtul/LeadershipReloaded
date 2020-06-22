using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CheerReloaded {
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
				// we are waiting a while before subscribing to events - otherwise 
				// the initial unit selection will trigger cheering and it feels awkward
				await Task.Delay(1000);
				_orderController.OnOrderIssued += ReactToIssuedOrder;
				_orderController.OnSelectedFormationsChanged += ReactToChangedFormations;
			}
		}

		private async void Affirmative(Agent a) {
			_affirmativeAgentCounter++;
			
			if (_affirmativeAgentCounter > _affirmativeAgentMaxCount) return;

			var agentPosition = a.Position;
			var distanceToPlayer = Agent.Main.GetPathDistanceToPoint(ref agentPosition);
			var timeToRespond = (int)(_rng.Next(900, 1200) * (distanceToPlayer / 10)).Clamp(900, 2000);

			await Task.Delay(timeToRespond);

			// ;_;
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

		private async void Grunt(Agent a) {
			if (_affirmativeAgentCounter > _affirmativeAgentMaxCount*2) return;

			var agentPosition = a.Position;
			var distanceToPlayer = Agent.Main.GetPathDistanceToPoint(ref agentPosition);
			if (distanceToPlayer > 35f) return;

			_affirmativeAgentCounter++;

			var timeToRespond = (int)(_rng.Next(700, 900) * (distanceToPlayer / 10)).Clamp(500, 1200);

			await Task.Delay(timeToRespond);

			a.MakeVoice(SkinVoiceManager.VoiceType.Everyone, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
		}

		private void ReactToChangedFormations() {
			foreach (var formation in _orderController.SelectedFormations) {
				formation.ApplyActionOnEachUnit(Grunt);
				_affirmativeAgentCounter = 0;
			}
		}

		private void ReactToIssuedOrder(OrderType orderType, IEnumerable<Formation> appliedFormations, params object[] delegateParams) {
			_orderType = orderType;
			foreach (var formation in appliedFormations) {
				formation.ApplyActionOnEachUnit(Affirmative);
				_affirmativeAgentCounter = 0;
			}
		}
	}
}
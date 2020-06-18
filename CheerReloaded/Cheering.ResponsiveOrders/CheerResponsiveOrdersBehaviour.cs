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
		private readonly Random _rng;

		private OrderType _orderType;

		public CheerResponsiveOrdersBehaviour(Config config) {
			_config = config;
			_rng = new Random();
		}

		public override async void OnAgentBuild(Agent agent, Banner banner) {
			if (agent == null) return;
			if (banner == null) return;

			if (agent == Agent.Main) {
				_orderController = Agent.Main.Team?.PlayerOrderController ?? null;
				// we are waiting a while before subscribing to events - otherwise 
				// the initial unit selection will trigger cheering and it feels awkward
				await Task.Delay(1000);
				_orderController.OnOrderIssued += ReactToIssuedOrder;
				_orderController.OnSelectedFormationsChanged += ReactToChangedFormations;
			}
		}

		private async void Affirmative(Agent a) {
			// ignore half of the formation
			if (_rng.Next(0, 100).IsInRange(0, 50)) {
				return;
			}

			await Task.Delay(_rng.Next(600, 800));

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
					a.MakeVoice(SkinVoiceManager.VoiceType.Move, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					break;
				default: // eh idk i'll just yell so he stops bothering me
					a.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					break;
			}
		}

		private void Grunt(Agent a) {
			a.MakeVoice(SkinVoiceManager.VoiceType.Everyone, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
		}

		private void ReactToChangedFormations() {
			foreach (var formation in _orderController.SelectedFormations) {
				formation.ApplyActionOnEachUnit(Grunt);
			}
		}

		private void ReactToIssuedOrder(OrderType orderType, IEnumerable<Formation> appliedFormations, params object[] delegateParams) {
			_orderType = orderType;
			foreach (var formation in appliedFormations) {
				formation.ApplyActionOnEachUnit(Affirmative);
			}
		}
	}
}
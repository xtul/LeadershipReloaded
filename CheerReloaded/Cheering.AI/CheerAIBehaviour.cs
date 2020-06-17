using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CheerReloaded {
	internal class CheerAIBehaviour : MissionBehaviour {
		public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;

		private readonly Config _config;
		private readonly CheerCommonMethods _common;

		public CheerAIBehaviour(Config config, CheerCommonMethods common) {
			_config = config;
			_common = common;
		}

		public override void OnAgentCreated(Agent agent) {
			if (agent == null) return;
			if (agent.IsMount) return;
			if (agent.IsMainAgent) return;
			if (agent.Character == null) return;

			if (agent.IsHero) {
				agent.AddComponent(new CheerAIComponent(_config, agent, _common));
			}
 		}
	}
}
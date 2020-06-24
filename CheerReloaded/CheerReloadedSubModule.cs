using System;
using TaleWorlds.MountAndBlade;
using static CheerReloaded.Helpers;
using TaleWorlds.InputSystem;
using SandBox.Source.Missions;

namespace CheerReloaded {
	public class CheerReloadedSubModule : MBSubModuleBase {
		public Config _config;
		public Strings _strings;
		public CheerCommonMethods _common;

		/// <summary>
		/// Entry point of the mod. When a mission is initialized and is a field battle, 
		/// it reads user config, serializes it into Config object and adds Cheer behaviour 
		/// to the mission.
		/// </summary>
		public override void OnMissionBehaviourInitialize(Mission mission) {
			_common = new CheerCommonMethods();
			_config = ReadAndStoreAsType<Config>("config");
			_strings = ReadAndStoreAsType<Strings>("strings");

			if (_config.DebugMode == true) {
				Log("{=debug_loadingsuccessful}" + _strings.Debug.LoadingSuccessful);
			}
			if (mission.GetMissionBehaviour<ArenaDuelMissionBehaviour>() != null) return;

			CorrectConfig();

			if (mission.CombatType == Mission.MissionCombatType.ArenaCombat) return;
			if (mission.CombatType == Mission.MissionCombatType.NoCombat) return;

			mission.AddMissionBehaviour(new CheerBehaviour(_config, _common, _strings));
			if (_config.AI.Enabled) {
				Mission.Current.AddMissionBehaviour(new CheerAIBehaviour(_config, _common, _strings));
			}
			if (_config.ResponsiveOrders.Enabled) {
				mission.AddMissionBehaviour(new CheerResponsiveOrdersBehaviour(_config));
			}
		}

		private void CorrectConfig() {
			_config.CheersPerXLeadershipLevels.Clamp(1, 500);
			_config.MaximumAdvantageMorale.Clamp(0, 100);
			_config.MaximumMoralePerAgent.Clamp(0, 100);
			_config.BaselineCheerAmount.Clamp(0, 9999);

			_config.AI.DeathMoraleDecrease.Clamp(0, 100);
			_config.AI.MaximumAdvantageMorale.Clamp(0, 100);
			_config.AI.MaximumMoralePerAgent.Clamp(0, 100);

			if (!Enum.IsDefined(typeof(InputKey), _config.KeyCode)) {
				Say("{=invalidkeycode}" + _strings.InvalidKeyCode);
				_config.KeyCode = 47;
			}

		}
	}
}

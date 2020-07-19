using System;
using TaleWorlds.MountAndBlade;
using static LeadershipReloaded.Helpers;
using TaleWorlds.InputSystem;
using LeadershipReloaded.Common;
using LeadershipReloaded.Player;
using LeadershipReloaded.AI;
using LeadershipReloaded.Settings;
using LeadershipReloaded.ResponsiveOrders;

namespace LeadershipReloaded {
	public class LeadershipReloadedSubModule : MBSubModuleBase {
		public Config _config;
		public Strings _strings;
		public CheerCommonMethods _common;

		/// <summary>
		/// Entry point of the mod. When a mission is initialized and is a field battle, 
		/// it reads user config, serializes it into Config object and adds Cheer behaviour 
		/// to the mission.
		/// </summary>
		public override void OnMissionBehaviourInitialize(Mission mission) {
			if (MissionState.Current.MissionName == "TournamentFight" ||
				MissionState.Current.MissionName == "ArenaPracticeFight") {
				return;
			}
			_common = new CheerCommonMethods();
			_config = ReadAndStoreAsType<Config>("config");
			_strings = ReadAndStoreAsType<Strings>("strings");
			
			if (_config.Cheering.DebugMode == true) {
				Log("{=debug_loadingsuccessful}" + _strings.Debug.LoadingSuccessful);
				Log(MissionState.Current.MissionName);
			}

			CorrectConfig();

			if (mission.CombatType == Mission.MissionCombatType.ArenaCombat) return;
			if (mission.CombatType == Mission.MissionCombatType.NoCombat) return;

			mission.AddMissionBehaviour(new LeadershipBehaviour(_config, _common, _strings));
			if (_config.AI.Enabled) {
				mission.AddMissionBehaviour(new LeadershipAIBehaviour(_config, _common, _strings));
			}
			if (_config.ResponsiveOrders.Enabled) {
				mission.AddMissionBehaviour(new LeadershipResponsiveOrdersBehaviour(_config));
			}
		}

		private void CorrectConfig() {
			_config.Cheering.CheersPerXLeadershipLevels.Clamp(1, 500);
			_config.Cheering.MaximumAdvantageMorale.Clamp(0, 100);
			_config.Cheering.MaximumMoralePerAgent.Clamp(0, 100);
			_config.Cheering.BaselineCheerAmount.Clamp(0, 9999);

			_config.AI.DeathMoraleDecrease.Clamp(0, 100);
			_config.AI.MaximumAdvantageMorale.Clamp(0, 100);
			_config.AI.MaximumMoralePerAgent.Clamp(0, 100);
			_config.AI.PersonalEffects.RelationshipChange.Clamp(-100, 100);

			if (!Enum.IsDefined(typeof(InputKey), _config.Cheering.KeyCode)) {
				Say("{=invalidkeycode}" + _strings.InvalidKeyCode);
				_config.Cheering.KeyCode = 47;
			}
		}
	}
}

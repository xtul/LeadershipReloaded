using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TaleWorlds.Library;
using TaleWorlds.Core;
using System.IO;

namespace CheerReloaded {
	public class CheerReloadedSubModule : MBSubModuleBase {
		public Config _config;
		public CheerCommonMethods _common;

		/// <summary>
		/// Entry point of the mod. When a mission is initialized, it reads user config,
		/// serializes it into Config object and adds Cheer behaviour to a mission.
		/// </summary>
		public override void OnMissionBehaviourInitialize(Mission mission) {
			_common = new CheerCommonMethods();

			var serializer = new XmlSerializer(typeof(Config));
			var reader = new StreamReader(BasePath.Name + "Modules/CheerReloaded/bin/Win64_Shipping_Client/config.xml");
			_config = (Config)serializer.Deserialize(reader);
			reader.Close();

			if (_config.DebugMode == true) {
				Helpers.Log("Cheer Reloaded activated.");
			}

			CorrectConfig();

			if (mission.CombatType == Mission.MissionCombatType.ArenaCombat) return;
			if (mission.CombatType == Mission.MissionCombatType.NoCombat) return;

			mission.AddMissionBehaviour(new CheerBehaviour(_config, _common));
			if (_config.AI.Enabled) {
				mission.AddMissionBehaviour(new CheerAIBehaviour(_config, _common));
			}
		}

		private void CorrectConfig() {
			_config.CheersPerXLeadershipLevels.Clamp(1, 500);
		}
	}
}

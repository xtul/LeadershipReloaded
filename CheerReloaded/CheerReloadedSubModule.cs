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
		public Config config;

		/// <summary>
		/// Entry point of the mod. When a mission is initialized, it reads user config,
		/// serializes it into Config object and adds Cheer behaviour to a mission.
		/// </summary>
		public override void OnMissionBehaviourInitialize(Mission mission) {
			var serializer = new XmlSerializer(typeof(Config));
			var reader = new StreamReader(BasePath.Name + "Modules/CheerReloaded/bin/Win64_Shipping_Client/config.xml");
			config = (Config)serializer.Deserialize(reader);
			reader.Close();

			if (config.DebugMode == true) {
				Helpers.Log("Cheer Reloaded activated.");
			}

			CorrectConfig();

			if (mission.CombatType == Mission.MissionCombatType.ArenaCombat) return;
			if (mission.CombatType == Mission.MissionCombatType.NoCombat) return;
			mission.AddMissionBehaviour(new CheerBehaviour(config));			
		}

		/// <summary>
		/// Corrects config 
		/// </summary>
		private void CorrectConfig() {
			config.CheersPerXLeadershipLevels.Clamp(1, 500);
		}
	}
}

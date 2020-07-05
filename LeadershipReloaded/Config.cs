namespace LeadershipReloaded.Settings {
	public class Config {
		public int KeyCode { get; set; }
		public int BaselineCheerAmount { get; set; }
		public int CheersPerXLeadershipLevels { get; set; }
		public bool ReportMoraleChange { get; set; }
		public bool PreventNegativeMorale { get; set; }
		public int EnemyMoraleLeadershipThreshold { get; set; }
		public float MaximumMoralePerAgent { get; set; }
		public float MaximumAdvantageMorale { get; set; }
		public bool DebugMode { get; set; }
		public AI AI { get; set; }
		public ResponsiveOrders ResponsiveOrders { get; set; }
	}

	public class AI {
		public bool Enabled { get; set; }
		public int BaselineCheerAmount { get; set; }
		public int CheersPerXLeadershipLevels { get; set; }
		public int MaximumMoralePerAgent { get; set; }
		public int MaximumAdvantageMorale { get; set; }
		public bool DisplayAnnouncement { get; set; }
		public bool ImpactfulDeath { get; set; }
		public int DeathMoraleDecrease { get; set; }
		public PersonalEffects PersonalEffects {get;set;}
	}

	public class PersonalEffects {
		public bool Enabled { get; set; }
		public int RenownGain { get; set; }
		public int RelationshipChange { get; set; }
	}

	public class ResponsiveOrders {
		public bool Enabled { get; set; }
		public int BaselineResponseCount { get; set; }
		public bool ContinuousChargeYell { get; set; }
		public Horn Horn { get; set; }
	}

	public class Horn {
		public bool Enabled { get; set; }
		public float MaxVolume { get; set; }
		public int MinTroopsToEnable { get; set; }
		public int MinDistanceToEnable { get; set; }
		public bool AddHornWhenVictoryCheering { get; set; }
		public bool AlwaysUseDefault { get; set; }
	}
}
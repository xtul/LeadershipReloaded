namespace CheerReloaded {
	public class Config {
		public int KeyCode { get; set; }
		public int BaselineCheerAmount { get; set; }
		public int CheersPerXLeadershipLevels { get; set; }
		public bool ReportMoraleChange { get; set; }
		public bool PreventNegativeMorale { get; set; }
		public int EnemyMoraleLeadershipThreshold { get; set; }
		public float MaximumMoralePerAgent { get; set; }
		public bool DebugMode { get; set; }
		public AI AI { get; set; }
	}

	public class AI {
		public bool Enabled { get; set; }
		public int BaselineCheerAmount { get; set; }
		public int CheersPerXLeadershipLevels { get; set; }
		public int MaximumMoralePerAgent { get; set; }
		public bool DisplayAnnouncement { get; set; }
	}
}
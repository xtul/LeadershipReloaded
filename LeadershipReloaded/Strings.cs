﻿using System.Security.Cryptography;
using System.Security.Policy;

namespace LeadershipReloaded.Settings {
	public class Strings {
		public string Failed { get; set; }
		public string NoEffect { get; set; }
		public string CheerCounter { get; set; }
		public string EnemyMoraleEffect { get; set; }
		public MoraleChange Friendly { get; set; }
		public MoraleChange Enemy { get; set; }
		public Lord Lord { get; set; }
		public Debug Debug { get; set; }
		public string InvalidKeyCode { get; set; }
	}

	public class MoraleChange {
		public string PositiveMorale { get; set; }
		public string NegativeMorale { get; set; }
	}

	public class Lord {
		public string Cheered { get; set; }
		public string Died { get; set; }
		public string DeathPersonalEffect { get; set; }
	}

	public class Debug {
		public string LoadingSuccessful { get; set; }

	}
}

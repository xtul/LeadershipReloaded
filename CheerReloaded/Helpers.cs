using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.TwoDimension;

namespace CheerReloaded {
	public static class Helpers {
		public static void Say(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new TaleWorlds.Library.Color(0.437f, 0.625f, 1f)));
		}

		public static void Log(string text) {
			InformationManager.DisplayMessage(new InformationMessage(text, new TaleWorlds.Library.Color(0.5f, 0.5f, 0.5f)));
		}

		// https://stackoverflow.com/a/2683487/
		/// <summary>
		/// Clamps a value between <paramref name="min"/> and <paramref name="max"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="val"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T> {
			if (val.CompareTo(min) < 0)
				return min;
			else if (val.CompareTo(max) > 0)
				return max;
			else
				return val;
		}
	}
}

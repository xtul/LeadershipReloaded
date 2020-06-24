using System;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace CheerReloaded {
	public static class Helpers {
		public static void Say(string text) {
			text = CleanupText(text);
			InformationManager.DisplayMessage(new InformationMessage(new TextObject(text, null).ToString(), new Color(0.437f, 0.625f, 1f)));
		}

		public static void Log(string text) {
			text = CleanupText(text);
			InformationManager.DisplayMessage(new InformationMessage(new TextObject(text, null).ToString(), new Color(0.5f, 0.5f, 0.5f)));
		}

		public static void Announce(string text, BasicCharacterObject agent = null) {
			InformationManager.AddQuickInformation(new TextObject(text, null), announcerCharacter: agent);
		}

		private static string CleanupText(string t) {
			return t.Trim().Replace("\n", "").Replace("\r", "");
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

		// https://stackoverflow.com/a/62181085/11365088
		/// <summary>
		/// Returns whether specified value is in valid range.
		/// </summary>
		/// <typeparam name="T">The type of data to validate.</typeparam>
		/// <param name="value">The value to validate.</param>
		/// <param name="min">The minimum valid value.</param>
		/// <param name="minInclusive">Whether the minimum value is valid.</param>
		/// <param name="max">The maximum valid value.</param>
		/// <param name="maxInclusive">Whether the maximum value is valid.</param>
		/// <returns>Whether the value is within range.</returns>
		public static bool IsInRange<T>(this T value, T? min, T? max = null, bool minInclusive = true, bool maxInclusive = true)
			where T : struct, IComparable<T> {
			var minValid = min == null || (minInclusive && value.CompareTo(min.Value) >= 0) || (!minInclusive && value.CompareTo(min.Value) > 0);
			var maxValid = max == null || (maxInclusive && value.CompareTo(max.Value) <= 0) || (!maxInclusive && value.CompareTo(max.Value) < 0);
			return minValid && maxValid;
		}

		/// <summary>
		/// Reads provided XML and serializes it into the specified <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">A class to serialize XML into.</typeparam>
		/// <param name="xml">The name of XML file without .xml.</param>
		/// <returns></returns>
		public static T ReadAndStoreAsType<T>(string xml) where T : class {
			var serializer = new XmlSerializer(typeof(T));
			var reader = new StreamReader(BasePath.Name + $"Modules/CheerReloaded/bin/Win64_Shipping_Client/{xml}.xml");
			var result = (T)serializer.Deserialize(reader);
			reader.Close();
			return result;
		}
	}
}

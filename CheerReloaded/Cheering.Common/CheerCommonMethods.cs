using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CheerReloaded.Common {
	public class CheerCommonMethods {
		public readonly ActionIndexCache[] _cheerBowActions = new ActionIndexCache[] {
			ActionIndexCache.Create("act_command_bow"),
			ActionIndexCache.Create("act_command_follow_bow")
		};
		public readonly ActionIndexCache[] _cheer2hActions = new ActionIndexCache[] {
			ActionIndexCache.Create("act_command_2h"),
			ActionIndexCache.Create("act_command_2h_leftstance"),
			ActionIndexCache.Create("act_command_follow_2h"),
			ActionIndexCache.Create("act_command_follow_2h_leftstance")
		};
		public readonly ActionIndexCache[] _cheer1hActions = new ActionIndexCache[] {
			ActionIndexCache.Create("act_command"),
			ActionIndexCache.Create("act_command_follow"),
			ActionIndexCache.Create("act_command_leftstance"),
			ActionIndexCache.Create("act_command_follow_leftstance")
		};
		public readonly ActionIndexCache[] _cheerNoneActions = new ActionIndexCache[] {
			ActionIndexCache.Create("act_command")
		};

		/// <summary>
		/// Applies audio-visual effects.
		/// </summary>
		/// <param name="a">An agent to apply cheering to.</param>
		public void ApplyCheerEffects(Agent a, float moraleChange, bool doAnim = true, bool doVoice = true) {
			if (Mission.Current == null) return;

			if (doAnim) {
				ActionIndexCache[] cheerActions = _cheerNoneActions;
				if (a.Mission == null) return;
				var wieldedWeapons = a.WieldedWeapon.Weapons;
				if (wieldedWeapons.Count > 0) {
					foreach (var weapon in wieldedWeapons) {
						if (weapon.IsRangedWeapon) {
							cheerActions = _cheerBowActions;
							break;
						}
						if (weapon.IsMeleeWeapon) {
							if (weapon.RelevantSkill == DefaultSkills.TwoHanded) {
								cheerActions = _cheer2hActions;
								break;
							}
							if (weapon.RelevantSkill == DefaultSkills.OneHanded || weapon.RelevantSkill == DefaultSkills.Throwing) {
								cheerActions = _cheer1hActions;
								break;
							}
						}
					}
				}

				// additionalFlags: it seems like anything past 2 means "can be cancelled by other actions"
				a.SetActionChannel(1, cheerActions[MBRandom.RandomInt(cheerActions.Length)], additionalFlags: 2);
			}

			if (!doVoice) return;

			// i know ugly as hell
			if (moraleChange >= 0) {
				if (moraleChange.IsInRange(0, 1)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Grunt, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (moraleChange.IsInRange(1, 3, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Yell, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (moraleChange.IsInRange(3, null, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Victory, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
			} else {
				if (moraleChange.IsInRange(-1, 0, true, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Fear, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (moraleChange.IsInRange(-3, -1, true, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.FallBack, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
				if (moraleChange.IsInRange(null, -3, true, false)) {
					a.MakeVoice(SkinVoiceManager.VoiceType.Retreat, SkinVoiceManager.CombatVoiceNetworkPredictionType.NoPrediction);
					return;
				}
			}
		}

		/// <summary>
		/// Applies morale changes.
		/// </summary>
		/// <param name="a">An agent to grant morale to.</param>
		/// <param name="moraleChange">How much morale to add?</param>
		/// <param name="isEnemyOfCheerer">If you grant morale to an enemy, use this. It halves the amount of morale and caps bottom morale value.</param>
		/// <param name="noNegativeMorale">Defines whether </param>
		/// <returns>Amount of morale that was applied to an agent.</returns>
		public int ApplyMoraleChange(Agent a, float moraleChange, bool isEnemyOfCheerer = false, bool noNegativeMorale = false) {
			var currentMorale = a.GetMorale();
			if (isEnemyOfCheerer) {
				if (currentMorale < 38)
					return 0;
				var invertedMorale = moraleChange / 2;
				a.SetMorale(currentMorale - invertedMorale);
				return (int)invertedMorale;
			} else {
				if (noNegativeMorale) {
					if (moraleChange < 0) moraleChange = 0;
				}
				a.SetMorale(currentMorale + moraleChange);
				return (int)moraleChange;
			}
		}
	}
}

using LeadershipReloaded.Settings;
using NAudio.Wave;
using System.IO;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace LeadershipReloaded.Common {
	public class SoundManager {
		private readonly Config _config;
		private bool _canPlayerPlayHorn;

		public SoundManager(Config config) {
			_config = config;
			_canPlayerPlayHorn = true;
		}

		public async Task PlayHorn(string culture, Agent agentPlayingHorn, OrderType orderType, bool victoryCheer = false) {
			if (!_canPlayerPlayHorn) return;
			
			var fullpath = BasePath.Name + $"Modules/LeadershipReloaded/ModuleData/HornAudio/{culture}/{orderType}.wav";
			if (victoryCheer) {
				fullpath = BasePath.Name + $"Modules/LeadershipReloaded/ModuleData/HornAudio/VictoryCheer.wav";
				if (!File.Exists(fullpath)) {
					return;
				}
			} else {
				if (_config.ResponsiveOrders.Horn.AlwaysUseDefault || !File.Exists(fullpath)) {
					fullpath = BasePath.Name + $"Modules/LeadershipReloaded/ModuleData/HornAudio/default/{orderType}.wav";
					if (!File.Exists(fullpath)) {
						return;
					}
				}
			}

			// to do: https://github.com/naudio/NAudio/blob/master/Docs/ConvertBetweenStereoAndMono.md

			_canPlayerPlayHorn = false;

			await Task.Delay(1400 + (int)(agentPlayingHorn.GetTrackDistanceToMainAgent() * 8.5f));

			using (var player = new WaveOutEvent())
			using (var audioFile = new AudioFileReader(fullpath)) {
				audioFile.Volume = CalculateVolume(agentPlayingHorn);
				player.Init(audioFile);
				player.Play();
				while (player.PlaybackState == PlaybackState.Playing) {
					audioFile.Volume = CalculateVolume(agentPlayingHorn);
					await Task.Delay(100);
				}
			}

			await Task.Delay(5000);
			_canPlayerPlayHorn = true;
		}

		private float CalculateVolume(Agent agentPlayingHorn) {
			return (25 / agentPlayingHorn.GetTrackDistanceToMainAgent()).Clamp(0.02f, _config.ResponsiveOrders.Horn.MaxVolume);
		}
	}
}

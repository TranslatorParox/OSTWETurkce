using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode.fmod;

namespace OneShotMG.src.EngineSpecificCode
{
	public class SoundManager
	{
		private FmodManager fMan;

		public const int BGM_VOLUME_MIN = 0;

		public const int BGM_VOLUME_MAX = 100;

		public const int SFX_VOLUME_MIN = 0;

		public const int SFX_VOLUME_MAX = 100;

		private string currentSongName = string.Empty;

		private float currentSongVolume = 1f;

		private float currentSongPitch = 1f;

		private string memorizedSongName = string.Empty;

		private float memorizedSongVolume = 1f;

		private float memorizedSongPitch = 1f;

		private string currentBgsName = string.Empty;

		private float currentBgsVolume = 1f;

		private float currentBgsPitch = 1f;

		private string memorizedBgsName = string.Empty;

		private float memorizedBgsVolume = 1f;

		private float memorizedBgsPitch = 1f;

		private Dictionary<string, MusicPlayerTrack> trackInfo;

		private Queue<string> unloadedSounds;

		public int SFXVol { get; set; } = 100;

		public int BGMVol { get; set; } = 100;

		public int SoundCount { get; private set; }

		public SoundManager()
		{
			fMan = new FmodManager(this);
			trackInfo = LoadSongMetadata();
		}

		public void LoadSoundList()
		{
			unloadedSounds = new Queue<string>();
			string[] files = Directory.GetFiles(Game1.GameDataPath() + "/sfx");
			foreach (string text in files)
			{
				if (text.Contains(".wav"))
				{
					unloadedSounds.Enqueue(text);
				}
			}
			SoundCount = unloadedSounds.Count;
		}

		public bool HasMoreSoundsToLoad()
		{
			return unloadedSounds.Count > 0;
		}

		public string NextSoundToLoad()
		{
			string text = unloadedSounds.Peek();
			return text.Substring(text.LastIndexOfAny(new char[2] { '\\', '/' }) + 1);
		}

		public void LoadNextSound()
		{
			if (HasMoreSoundsToLoad())
			{
				string filename = unloadedSounds.Dequeue();
				fMan.LoadSound(filename);
			}
		}

		public void LoadSound(string soundName)
		{
			fMan.LoadSound(Game1.GameDataPath() + "/sfx/" + soundName + ".wav");
		}

		public void LoadAllSounds()
		{
			if (SoundCount <= 0)
			{
				LoadSoundList();
			}
			while (HasMoreSoundsToLoad())
			{
				LoadNextSound();
			}
		}

		public void Update()
		{
			fMan.Update();
		}

		public void PlaySound(string sfxName, float vol = 1f, float pitch = 1f)
		{
			fMan.PlaySound(sfxName, vol * ((float)SFXVol / 100f), pitch);
		}

		public void PlaySong(string songName, float fadeInTime = 0f, float volume = 1f, float pitch = 1f)
		{
			if (GetTrackInfoById(songName, out var track))
			{
				Game1.windowMan.UnlockMan.UnlockTrack(track);
			}
			currentSongName = songName;
			currentSongVolume = volume;
			currentSongPitch = pitch;
			if (string.IsNullOrEmpty(songName))
			{
				fMan.StopSong();
				return;
			}
			fMan.PlaySong(songName, fadeInTime, volume, pitch);
			fMan.QueueSong(string.Empty);
		}

		public void QueueSong(string songName, float fadeInTime = 0f, float volume = 1f, float pitch = 1f)
		{
			if (GetTrackInfoById(songName, out var track))
			{
				Game1.windowMan.UnlockMan.UnlockTrack(track);
			}
			currentSongName = songName;
			currentSongVolume = volume;
			currentSongPitch = pitch;
			if (songName == fMan.CurrentSong)
			{
				fMan.PlaySong(songName, 0f, volume, pitch);
				fMan.QueueSong(string.Empty);
			}
			else
			{
				fMan.FadeOutBGM(0.5f);
				fMan.QueueSong(songName, fadeInTime, volume, pitch);
			}
		}

		public void SetSongPaused(bool paused)
		{
			fMan.SetSongPaused(paused);
		}

		public void PlayMusicEffect(string meName, float volume = 1f, float pitch = 1f)
		{
			fMan.PlayMusicEffect(meName, volume, pitch);
		}

		public void MemorizeBGMandBGS()
		{
			memorizedSongName = currentSongName;
			memorizedSongPitch = currentSongPitch;
			memorizedSongVolume = currentSongVolume;
			memorizedBgsName = currentBgsName;
			memorizedBgsPitch = currentBgsPitch;
			memorizedBgsVolume = currentBgsVolume;
		}

		public void RestoreBGMandBGS()
		{
			PlaySong(memorizedSongName, 0f, memorizedSongVolume, memorizedSongPitch);
			PlayBGS(memorizedBgsName, memorizedBgsVolume, memorizedBgsPitch);
		}

		public void FadeOutBGM(float fadeOutTime)
		{
			fMan.FadeOutBGM(fadeOutTime);
		}

		public void FadeInBGM(float targetVolume, float fadeInTime)
		{
			fMan.FadeInBGM(targetVolume, fadeInTime);
		}

		public SoundSaveData GetSoundSaveData()
		{
			return new SoundSaveData
			{
				BGMname = fMan.CurrentSong,
				BGMvol = fMan.CurrentSongVolume,
				BGMpitch = fMan.SongSpeed,
				BGSname = currentBgsName,
				BGSvol = currentBgsVolume,
				BGSpitch = currentBgsPitch
			};
		}

		public void LoadSoundSaveData(SoundSaveData soundSaveData)
		{
			if (soundSaveData != null)
			{
				PlaySong(soundSaveData.BGMname, 0f, soundSaveData.BGMvol, soundSaveData.BGMpitch);
				PlayBGS(soundSaveData.BGSname, soundSaveData.BGSvol, soundSaveData.BGSpitch);
			}
		}

		public void PlayBGS(string sfxName, float vol, float pitch)
		{
			StopBGS();
			currentBgsName = sfxName;
			currentBgsVolume = vol;
			currentBgsPitch = pitch;
			if (!string.IsNullOrEmpty(sfxName))
			{
				fMan.PlayBGS(sfxName, vol, pitch);
			}
		}

		public void FadeOutBGS(float durationInSeconds)
		{
			fMan.FadeOutBGS(durationInSeconds);
			currentBgsName = string.Empty;
			currentBgsPitch = 0f;
			currentBgsVolume = 1f;
		}

		public void StopBGS()
		{
			fMan.StopBGS();
			currentBgsName = string.Empty;
			currentBgsPitch = 0f;
			currentBgsVolume = 1f;
		}

		public bool GetTrackInfoById(string id, out MusicPlayerTrack track)
		{
			track = null;
			if (id != null)
			{
				return trackInfo.TryGetValue(id, out track);
			}
			return false;
		}

		public List<MusicPlayerTrack> GetAllTracks()
		{
			return trackInfo.Values.ToList();
		}

		private Dictionary<string, MusicPlayerTrack> LoadSongMetadata()
		{
			Dictionary<string, MusicPlayerTrack> dictionary = new Dictionary<string, MusicPlayerTrack>();
			MusicPlayerTrack[] tracklist = JsonConvert.DeserializeObject<MusicTrackMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "music/tracklist.json"))).tracklist;
			foreach (MusicPlayerTrack musicPlayerTrack in tracklist)
			{
				dictionary[musicPlayerTrack.trackId] = musicPlayerTrack;
			}
			return dictionary;
		}
	}
}

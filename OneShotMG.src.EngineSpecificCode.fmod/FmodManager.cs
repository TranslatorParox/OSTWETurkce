using System;
using System.Collections.Generic;
using FMOD;
using OneShotMG.src.Util;

namespace OneShotMG.src.EngineSpecificCode.fmod
{
	public class FmodManager
	{
		private SoundManager owner;

		private FMOD.System fSystem;

		private ChannelGroup fMainChannel;

		private Sound bgmSound;

		private Channel bgmChannel;

		private Channel bgsChannel;

		private Sound meSound;

		private Channel meChannel;

		private Dictionary<string, Sound> sfxDictionary;

		private List<Channel> activeSFXChannels;

		private float fadeVol = 1f;

		private int fadeTimeTotal;

		private int fadeTimer;

		private float fadeStartVol;

		private float fadeEndVol = 1f;

		private string queuedSongName = string.Empty;

		private float queuedSongVol = 1f;

		private float queuedSongPitch = 1f;

		private float queuedSongFadeInTime;

		private int bgsFadeOutTimer;

		private int bgsFadeOutTimeTotal;

		private List<Channel> channelsToRemove = new List<Channel>();

		public string CurrentSong { get; private set; }

		public float CurrentSongVolume { get; private set; }

		public string CurrentBGSTitle { get; private set; }

		public float CurrentBGSVolume { get; private set; }

		public float SongSpeed
		{
			get
			{
				if (bgmChannel.hasHandle())
				{
					bgmChannel.getPitch(out var pitch);
					return pitch;
				}
				return 1f;
			}
		}

		public FmodManager(SoundManager owner)
		{
			this.owner = owner;
			activeSFXChannels = new List<Channel>();
			Factory.System_Create(out fSystem);
			fSystem.init(32, INITFLAGS.NORMAL, (IntPtr)0);
			fSystem.createChannelGroup("main", out fMainChannel);
			bgmSound.clearHandle();
			bgmChannel.clearHandle();
			bgsChannel.clearHandle();
			meSound.clearHandle();
			meChannel.clearHandle();
			sfxDictionary = new Dictionary<string, Sound>();
		}

		public void LoadSound(string filename)
		{
			string text = filename.Substring(filename.LastIndexOfAny(new char[2] { '\\', '/' }) + 1);
			text = text.Substring(0, text.IndexOf(".wav"));
			text = text.ToLowerInvariant();
			if (!sfxDictionary.ContainsKey(text))
			{
				fSystem.createSound(filename, MODE.CREATESAMPLE, out var sound);
				sfxDictionary.Add(text, sound);
			}
		}

		public void PlaySound(string sfxName, float vol = 1f, float pitch = 0f)
		{
			sfxName = sfxName.ToLowerInvariant();
			if (sfxDictionary.TryGetValue(sfxName, out var value))
			{
				fSystem.playSound(value, fMainChannel, paused: true, out var channel);
				channel.setVolume(vol);
				channel.setPitch(pitch);
				channel.setLoopCount(0);
				channel.setPaused(paused: false);
				activeSFXChannels.Add(channel);
			}
		}

		public void Update()
		{
			channelsToRemove.Clear();
			foreach (Channel activeSFXChannel in activeSFXChannels)
			{
				activeSFXChannel.isPlaying(out var isplaying);
				if (!isplaying)
				{
					activeSFXChannel.stop();
					channelsToRemove.Add(activeSFXChannel);
				}
			}
			foreach (Channel item in channelsToRemove)
			{
				activeSFXChannels.Remove(item);
			}
			if (fadeTimer < fadeTimeTotal && fadeTimeTotal > 0)
			{
				fadeTimer++;
				float speedRatio = (float)fadeTimer / (float)fadeTimeTotal;
				fadeVol = MathHelper.ApproachFloat(fadeEndVol, fadeStartVol, speedRatio);
			}
			else
			{
				fadeVol = fadeEndVol;
				if (fadeVol <= 0f)
				{
					StopSong();
					if (!string.IsNullOrEmpty(queuedSongName))
					{
						dequeueSong();
					}
				}
			}
			if (meChannel.hasHandle())
			{
				meChannel.isPlaying(out var isplaying2);
				if (!isplaying2)
				{
					StopMusicEffect();
					if (bgmChannel.hasHandle())
					{
						if (!string.IsNullOrEmpty(queuedSongName))
						{
							dequeueSong();
						}
						fadeVol = 0f;
						fadeTimer = 0;
						fadeTimeTotal = 60;
						fadeStartVol = 0f;
						fadeEndVol = 1f;
						bgmChannel.setVolume(0f);
					}
				}
				else if (bgmChannel.hasHandle())
				{
					bgmChannel.setVolume(0f);
				}
			}
			else if (bgmChannel.hasHandle())
			{
				bgmChannel.setVolume(CurrentSongVolume * fadeVol * ((float)owner.BGMVol / 100f));
			}
			if (!bgsChannel.hasHandle())
			{
				return;
			}
			if (bgsFadeOutTimeTotal > 0)
			{
				bgsFadeOutTimer++;
				if (bgsFadeOutTimer >= bgsFadeOutTimeTotal)
				{
					StopBGS();
				}
				else
				{
					bgsChannel.setVolume(CurrentBGSVolume * (((float)bgsFadeOutTimeTotal - (float)bgsFadeOutTimer) / (float)bgsFadeOutTimeTotal) * ((float)owner.SFXVol / 100f));
				}
			}
			else
			{
				bgsChannel.setVolume(CurrentBGSVolume * ((float)owner.SFXVol / 100f));
			}
		}

		public void PlayMusicEffect(string meName, float volume, float pitch)
		{
			StopMusicEffect();
			if (bgmChannel.hasHandle())
			{
				bgmChannel.setVolume(0f);
			}
			string name = Game1.GameDataPath() + "/music_effects/" + meName + ".ogg";
			fSystem.createStream(name, MODE.LOOP_NORMAL, out meSound);
			fSystem.playSound(meSound, fMainChannel, paused: true, out meChannel);
			meChannel.setMode(MODE.LOOP_NORMAL);
			meChannel.setLoopCount(0);
			meChannel.setPriority(1);
			meChannel.setVolume(volume * ((float)owner.SFXVol / 100f));
			meChannel.setPitch(pitch);
			meChannel.setPaused(paused: false);
		}

		public void QueueSong(string title, float fadeInTime = 0f, float volume = 1f, float pitch = 1f)
		{
			queuedSongName = title;
			queuedSongFadeInTime = fadeInTime;
			queuedSongVol = volume;
			queuedSongPitch = pitch;
		}

		public void dequeueSong()
		{
			PlaySong(queuedSongName, queuedSongFadeInTime, queuedSongVol, queuedSongPitch);
			queuedSongName = string.Empty;
		}

		public void PlaySong(string title, float fadeInTime = 0f, float volume = 1f, float pitch = 1f)
		{
			if (title != CurrentSong || !bgmChannel.hasHandle())
			{
				StopSong();
				CurrentSong = title;
				string name = Game1.GameDataPath() + "/music/" + title + ".ogg";
				fSystem.createStream(name, MODE.LOOP_NORMAL, out bgmSound);
				fSystem.playSound(bgmSound, fMainChannel, paused: true, out bgmChannel);
				bgmChannel.setMode(MODE.LOOP_NORMAL);
				bgmChannel.setLoopCount(-1);
				bgmChannel.setPriority(0);
			}
			bgmChannel.setPitch(pitch);
			fadeTimer = 0;
			CurrentSongVolume = volume;
			if (fadeInTime <= 0f)
			{
				fadeVol = 1f;
				fadeTimeTotal = 0;
				fadeStartVol = 1f;
				fadeEndVol = 1f;
			}
			else
			{
				fadeVol = 0f;
				fadeTimeTotal = (int)(fadeInTime * 60f);
				fadeStartVol = 0f;
				fadeEndVol = 1f;
			}
			bgmChannel.setVolume(CurrentSongVolume * fadeVol * ((float)owner.BGMVol / 100f));
			if (meChannel.hasHandle())
			{
				meChannel.isPlaying(out var isplaying);
				if (isplaying)
				{
					bgmChannel.setVolume(0f);
				}
			}
			bgmChannel.setPaused(paused: false);
		}

		public void PlayBGS(string sfxName, float volume = 1f, float pitch = 1f)
		{
			sfxName = sfxName.ToLowerInvariant();
			if (sfxName != CurrentBGSTitle || !bgsChannel.hasHandle())
			{
				StopBGS();
				CurrentBGSTitle = sfxName;
				if (sfxDictionary.TryGetValue(sfxName, out var value))
				{
					fSystem.playSound(value, fMainChannel, paused: true, out bgsChannel);
					bgsChannel.setVolume(volume * ((float)owner.SFXVol / 100f));
					bgsChannel.setPitch(pitch);
					bgsChannel.setMode(MODE.LOOP_NORMAL);
					bgsChannel.setLoopCount(-1);
					bgsChannel.setPaused(paused: false);
				}
			}
			CurrentBGSVolume = volume;
		}

		public void StopSong()
		{
			CurrentSong = string.Empty;
			CurrentSongVolume = 0f;
			if (bgmChannel.hasHandle())
			{
				bgmChannel.stop();
				bgmChannel.clearHandle();
			}
			if (bgmSound.hasHandle())
			{
				bgmSound.release();
				bgmSound.clearHandle();
			}
		}

		public void StopBGS()
		{
			if (bgsChannel.hasHandle())
			{
				bgsChannel.stop();
				bgsChannel.clearHandle();
			}
			bgsFadeOutTimeTotal = 0;
			bgsFadeOutTimer = 0;
		}

		private void StopMusicEffect()
		{
			if (meChannel.hasHandle())
			{
				meChannel.stop();
				meChannel.clearHandle();
			}
			if (meSound.hasHandle())
			{
				meSound.release();
				meSound.clearHandle();
			}
		}

		public void FadeInBGM(float targetVolume, float fadeInTime)
		{
			fadeStartVol = CurrentSongVolume;
			CurrentSongVolume = targetVolume;
			fadeStartVol = 0f;
			fadeEndVol = targetVolume;
			fadeTimer = 0;
			fadeTimeTotal = (int)(fadeInTime * 60f);
		}

		public void FadeOutBGM(float fadeOutTime)
		{
			fadeTimer = 0;
			fadeTimeTotal = (int)(fadeOutTime * 60f);
			fadeStartVol = fadeVol;
			fadeEndVol = 0f;
		}

		public void FadeOutBGS(float fadeOutTime)
		{
			bgsFadeOutTimer = 0;
			bgsFadeOutTimeTotal = (int)(fadeOutTime * 60f);
		}

		public void SetSongPaused(bool paused)
		{
			if (bgmChannel.hasHandle())
			{
				bgmChannel.setPaused(paused);
			}
		}
	}
}

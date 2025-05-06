using System;
using System.Collections.Generic;
using System.Linq;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class JukeboxWindow : TWMWindow
	{
		private enum NikoState
		{
			Sleep,
			Jamming,
			Bopping,
			Blinking
		}

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private const int TEXT_OFFSET = 2;

		private const int CONTENT_MARGIN = 6;

		private const int TEXT_SIZE = 16;

		private const int BUTTON_SIZE = 16;

		private const int SLIDER_SIZE = 32;

		private const int CONTROL_BUTTON_SIZE = 32;

		private const int TRACK_CHOOSER_Y = 122;

		private const int SLIDER_Y = 144;

		private const int CONTROL_Y = 182;

		private const int GRAMMAPHONE_X_OFFSET = 110;

		private const int GRAMMAPHONE_WIDTH = 50;

		private const int GRAMMAPHONE_HEIGHT = 50;

		private const int SCENE_HEIGHT = 100;

		private readonly SliderControl speedSlider;

		private readonly SliderControl volumeSlider;

		private readonly List<MusicPlayerTrack> trackList;

		private readonly List<IconButton> buttons;

		private TempTexture currentTrackTexture;

		private static string lastTrack;

		private ChooserControl trackChooser;

		private bool isPlaying;

		private bool isPaused;

		private float gramphoneAnimTimer;

		private int gramphoneAnimIndex;

		private const float GRAMPHONE_FRAME_TIME = 5f;

		private readonly int[] GRAMPHONE_FRAME_INDEXES = new int[8] { 0, 1, 2, 3, 4, 3, 2, 1 };

		private float musicNoteSpawnTimer;

		private const float MUSIC_NOTE_SPAWN_TIME = 30f;

		private float nikoAnimTimer;

		private int nikoAnimIndex;

		private int nikoFrameIndex;

		private const float NIKO_PAUSE_FRAME_TIME = 60f;

		private const int NIKO_PAUSE_TOTAL_FRAMES = 2;

		private const float NIKO_JAM_FRAME_TIME = 15f;

		private const int NIKO_JAM_TOTAL_FRAMES = 4;

		private readonly int[] NIKO_BLINK_FRAME_TIMES = new int[4] { 110, 10, 50, 10 };

		private readonly int[] NIKO_BLINK_FRAME_INDEXES = new int[4] { 0, 1, 2, 1 };

		private readonly int[] NIKO_BOP_FRAME_TIMES = new int[4] { 10, 10, 10, 10 };

		private readonly int[] NIKO_BOP_FRAME_INDEXES = new int[4] { 6, 5, 4, 5 };

		private const int NIKO_ANIM_WIDTH = 50;

		private const int NIKO_ANIM_HEIGHT = 50;

		private const int BULB_WIDTH = 18;

		private const int BULB_HEIGHT = 18;

		private const int MUSIC_NOTE_SPAWN_X = 152;

		private const int MUSIC_NOTE_SPAWN_Y = 47;

		private readonly NikoState[] NIKO_PLAY_STATES = new NikoState[3]
		{
			NikoState.Jamming,
			NikoState.Bopping,
			NikoState.Blinking
		};

		private NikoState nikoState;

		private MusicNoteManager musicNoteMan;

		private bool isGlitched;

		public JukeboxWindow(List<string> unlockedTracks)
		{
			if (Game1.windowMan.Desktop.inSolstice && MathHelper.Random(1, 16) == 16)
			{
				isGlitched = true;
				unlockedTracks = new List<string>();
				unlockedTracks.Add("MyBurdenIsLight");
				lastTrack = null;
			}
			trackList = new List<MusicPlayerTrack>();
			foreach (string unlockedTrack in unlockedTracks)
			{
				if (Game1.soundMan.GetTrackInfoById(unlockedTrack, out var track))
				{
					trackList.Add(track);
				}
			}
			trackList.Sort((MusicPlayerTrack a, MusicPlayerTrack b) => a.trackNumber - b.trackNumber);
			base.ContentsSize = new Vec2(320, 220);
			base.WindowIcon = "jukebox";
			base.WindowTitle = "musicbox_appname";
			AddButton(TWMWindowButtonType.Close);
			onClose = (Action<TWMWindow>)Delegate.Combine(onClose, (Action<TWMWindow>)delegate
			{
				OnMusicStop();
			});
			AddButton(TWMWindowButtonType.Minimize);
			int num = (base.ContentsSize.X - 18) / 2;
			speedSlider = new SliderControl("musicbox_playback_speed_slider_label", 50, 150, new Vec2(6, 144), num, useButtons: true, vertical: false, "%");
			speedSlider.Increment = 5;
			speedSlider.OnValueChanged = SetSpeed;
			volumeSlider = new SliderControl("musicbox_volume_slider_label", 0, 100, new Vec2(12 + num, 144), num, useButtons: true, vertical: false, "%");
			volumeSlider.Increment = 5;
			volumeSlider.OnValueChanged = SetVolume;
			volumeSlider.Value = 100;
			buttons = InitButtons();
			lastTrack = trackChooser.Value;
			SwitchTrack(lastTrack);
			musicNoteMan = new MusicNoteManager();
		}

		private List<IconButton> InitButtons()
		{
			int y = 122;
			List<(string, string)> items = trackList.Select((MusicPlayerTrack t) => (trackId: t.trackId, displayName: t.displayName)).ToList();
			trackChooser = new ChooserControl(new Vec2(6, y), base.ContentsSize.X - 12, items, lastTrack, GraphicsManager.FontType.OS, Game1.languageMan.GetMusicLocString);
			trackChooser.OnItemChange = delegate(string t)
			{
				lastTrack = t;
				SwitchTrack(t);
			};
			if (string.IsNullOrEmpty(lastTrack) && trackList.Count > 0)
			{
				lastTrack = trackChooser.Value;
			}
			Vec2 relativePos = default(Vec2);
			relativePos.Y = 182;
			relativePos.X = (base.ContentsSize.X - 12 - 96) / 2;
			IconButton item = new IconButton("the_world_machine/window_icons/stop", relativePos, OnMusicStop);
			relativePos.X += 38;
			IconButton item2 = new IconButton("the_world_machine/window_icons/play", relativePos, OnMusicPlayOrPause);
			relativePos.X += 38;
			IconButton item3 = new IconButton("the_world_machine/window_icons/restart_track", relativePos, OnMusicRestart);
			return new List<IconButton> { item, item2, item3 };
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gameColor = theme.Primary(alpha);
			GameColor gColor = theme.Background(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gColor);
			Vec2 vec = new Vec2(screenPos.X + 110, screenPos.Y) * 2;
			Game1.gMan.MainBlit($"the_world_machine/jukebox/grammaphone{GRAMPHONE_FRAME_INDEXES[gramphoneAnimIndex] + 1}", vec, new Rect(0, 0, 50, 50), 4f, 4f, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.TheWorldMachine);
			if (!isGlitched)
			{
				string textureName;
				switch (nikoState)
				{
				default:
					textureName = "the_world_machine/jukebox/niko_sleep";
					break;
				case NikoState.Jamming:
					textureName = "the_world_machine/jukebox/niko_jam";
					break;
				case NikoState.Bopping:
				case NikoState.Blinking:
					textureName = "the_world_machine/jukebox/niko_stand";
					break;
				}
				Rect srcRect = new Rect(50 * nikoFrameIndex, 0, 50, 50);
				Vec2 pixelPos = vec;
				pixelPos.X += 160;
				Game1.gMan.MainBlit(textureName, pixelPos, srcRect, 4f, 4f, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.TheWorldMachine);
				Vec2 pixelPos2 = vec;
				pixelPos2.X -= 80;
				pixelPos2.Y += 128;
				Game1.gMan.MainBlit("the_world_machine/jukebox/small_sun", pixelPos2, new Rect(0, 0, 18, 18), 4f, 4f, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.TheWorldMachine);
			}
			Rect boxRect2 = new Rect(6 + screenPos.X, 122 + screenPos.Y, base.ContentsSize.X - 12, 16);
			Game1.gMan.ColorBoxBlit(boxRect2, gameColor);
			Rect boxRect3 = boxRect2.Shrink(1);
			Game1.gMan.ColorBoxBlit(boxRect3, gColor);
			speedSlider.Draw(theme, screenPos, alpha);
			volumeSlider.Draw(theme, screenPos, alpha);
			trackChooser.Draw(screenPos, theme, alpha);
			foreach (IconButton button in buttons)
			{
				button.Draw(screenPos, theme, alpha);
			}
			Vec2 vec2 = new Vec2(base.ContentsSize.X / 2, 108) + screenPos;
			Game1.gMan.MainBlit(currentTrackTexture, vec2 * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			vec2.Y += 16;
			musicNoteMan.Draw(screenPos);
		}

		public override bool Update(bool cursorOccluded)
		{
			if (currentTrackTexture == null || !currentTrackTexture.isValid)
			{
				currentTrackTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("musicbox_current_track_chooser_label"));
			}
			currentTrackTexture.KeepAlive();
			bool canInteract = !cursorOccluded && !base.IsMinimized;
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			speedSlider.Update(parentPos, canInteract);
			volumeSlider.Update(parentPos, canInteract);
			trackChooser.Update(parentPos, canInteract);
			buttons[1].Icon = ((!isPlaying || isPaused) ? "the_world_machine/window_icons/play" : "the_world_machine/window_icons/pause");
			foreach (IconButton button in buttons)
			{
				button.Update(parentPos, canInteract);
				button.Disabled = trackList.Count <= 0;
			}
			if (isPlaying && !isPaused)
			{
				gramphoneAnimTimer += (float)speedSlider.Value / 100f;
				if (gramphoneAnimTimer >= 5f)
				{
					gramphoneAnimTimer = 0f;
					gramphoneAnimIndex++;
					if (gramphoneAnimIndex >= GRAMPHONE_FRAME_INDEXES.Length)
					{
						gramphoneAnimIndex = 0;
					}
				}
				musicNoteSpawnTimer += (float)speedSlider.Value / 100f;
				if (musicNoteSpawnTimer >= 30f)
				{
					musicNoteSpawnTimer = 0f;
					if (!isGlitched)
					{
						musicNoteMan.SpawnNote(new Vec2(152, 47));
					}
				}
			}
			UpdateNikoAnim();
			musicNoteMan.Update();
			return base.Update(cursorOccluded);
		}

		private void UpdateNikoAnim()
		{
			switch (nikoState)
			{
			case NikoState.Sleep:
				nikoAnimTimer += 1f;
				if (nikoAnimTimer >= 60f)
				{
					nikoAnimTimer = 0f;
					nikoAnimIndex++;
					if (nikoAnimIndex >= 2)
					{
						nikoAnimIndex = 0;
					}
					nikoFrameIndex = nikoAnimIndex;
				}
				break;
			case NikoState.Jamming:
				nikoAnimTimer += (float)speedSlider.Value / 100f;
				if (nikoAnimTimer >= 15f)
				{
					nikoAnimTimer = 0f;
					nikoAnimIndex++;
					if (nikoAnimIndex >= 4)
					{
						nikoAnimIndex = 0;
					}
					nikoFrameIndex = nikoAnimIndex;
				}
				break;
			case NikoState.Blinking:
				nikoAnimTimer += (float)speedSlider.Value / 100f;
				if (nikoAnimTimer >= (float)NIKO_BLINK_FRAME_TIMES[nikoAnimIndex])
				{
					nikoAnimTimer = 0f;
					nikoAnimIndex++;
					if (nikoAnimIndex >= NIKO_BLINK_FRAME_INDEXES.Length)
					{
						nikoAnimIndex = 0;
					}
					nikoFrameIndex = NIKO_BLINK_FRAME_INDEXES[nikoAnimIndex];
				}
				break;
			case NikoState.Bopping:
				nikoAnimTimer += (float)speedSlider.Value / 100f;
				if (nikoAnimTimer >= (float)NIKO_BOP_FRAME_TIMES[nikoAnimIndex])
				{
					nikoAnimTimer = 0f;
					nikoAnimIndex++;
					if (nikoAnimIndex >= NIKO_BOP_FRAME_INDEXES.Length)
					{
						nikoAnimIndex = 0;
					}
					nikoFrameIndex = NIKO_BOP_FRAME_INDEXES[nikoAnimIndex];
				}
				break;
			}
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is JukeboxWindow;
		}

		public void AddTrack(string id)
		{
			if (!isGlitched)
			{
				if (Game1.soundMan.GetTrackInfoById(id, out var track))
				{
					trackList.Add(track);
				}
				trackList.Sort((MusicPlayerTrack a, MusicPlayerTrack b) => a.trackNumber - b.trackNumber);
				trackChooser.SetItems(trackList.Select((MusicPlayerTrack t) => (trackId: t.trackId, displayName: t.displayName)).ToList(), lastTrack);
				if (string.IsNullOrEmpty(lastTrack) && trackList.Count > 0)
				{
					lastTrack = trackChooser.Value;
					SwitchTrack(lastTrack);
				}
			}
		}

		private void resetNikoAnim()
		{
			nikoAnimIndex = 0;
			nikoAnimTimer = 0f;
			nikoFrameIndex = 0;
		}

		private void pickNextNikoPlayState()
		{
			nikoState = MathHelper.RandomChoice(NIKO_PLAY_STATES);
		}

		private void pickNextNikoPauseState()
		{
			nikoState = NikoState.Sleep;
		}

		public void OnMusicStop()
		{
			Game1.soundMan.PlaySong(null);
			isPlaying = false;
			isPaused = false;
			pickNextNikoPauseState();
			resetNikoAnim();
		}

		public void OnMusicPlayOrPause()
		{
			if (isPlaying && !isPaused)
			{
				Game1.soundMan.SetSongPaused(paused: true);
				isPaused = true;
				pickNextNikoPauseState();
			}
			else
			{
				if (isPaused)
				{
					Game1.soundMan.SetSongPaused(paused: false);
					isPaused = false;
				}
				else
				{
					PlayCurrentSong();
				}
				pickNextNikoPlayState();
			}
			resetNikoAnim();
		}

		public void OnMusicRestart()
		{
			bool num = isPlaying && !isPaused;
			OnMusicStop();
			if (num)
			{
				PlayCurrentSong();
				pickNextNikoPlayState();
			}
			resetNikoAnim();
		}

		private void SetSpeed(int speed)
		{
			if (!isGlitched && isPlaying)
			{
				PlayCurrentSong();
			}
		}

		private void SetVolume(int volume)
		{
			if (!isGlitched && isPlaying)
			{
				PlayCurrentSong();
			}
		}

		private void PlayCurrentSong()
		{
			MusicPlayerTrack musicPlayerTrack = trackList.Find((MusicPlayerTrack t) => t.trackId == lastTrack);
			if (musicPlayerTrack != null)
			{
				string songName = musicPlayerTrack.trackId;
				float volume = (float)volumeSlider.Value / 100f;
				float pitch = (float)speedSlider.Value / 100f;
				if (isGlitched)
				{
					songName = "MyBurdenIsDead";
					volume = 1f;
					pitch = 1f;
				}
				Game1.soundMan.PlaySong(songName, 0f, volume, pitch);
				isPlaying = true;
			}
		}

		private void SwitchTrack(string trackId)
		{
			MusicPlayerTrack musicPlayerTrack = trackList.Find((MusicPlayerTrack t) => t.trackId == trackId);
			if (musicPlayerTrack != null)
			{
				speedSlider.Value = musicPlayerTrack.defaultSpeed;
				if (isPlaying)
				{
					PlayCurrentSong();
					if (isPaused)
					{
						Game1.soundMan.SetSongPaused(paused: true);
					}
				}
			}
			else
			{
				OnMusicStop();
			}
		}
	}
}

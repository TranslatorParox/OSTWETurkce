using System;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Entities;
using OneShotMG.src.Map;
using OneShotMG.src.Menus;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class OneshotWindow : TWMWindow
	{
		private Vec2 oldMaximizeButtonPos;

		private int fullscreenBorderFrameIndex;

		private int fullscreenBorderFrameTimer;

		private const int FULLSCREEN_BORDER_FRAME_TIME = 6;

		private const int FULLSCREEN_BORDER_TOTAL_FRAMES = 2;

		private const int EXIT_GAME_COMMON_EVENT_ID = 35;

		public FollowerManager followerMan { get; private set; }

		public FlashManager flashMan { get; private set; }

		public PictureManager pictureMan { get; private set; }

		public SelfSwitchManager selfSwitchMan { get; private set; }

		public FilmPuzzleManager filmPuzzleMan { get; private set; }

		public WindowShakeManager windowShakeMan { get; private set; }

		public GameSaveManager gameSaveMan { get; private set; }

		public MenuManager menuMan { get; private set; }

		public FlagManager flagMan { get; private set; }

		public VariableManager varMan { get; private set; }

		public TileMapManager tileMapMan { get; private set; }

		public FastTravelManager fastTravelMan { get; private set; }

		public TitleScreenManager titleScreenMan { get; private set; }

		public GlitchEffectManager glitchEffectMan { get; private set; }

		public FullscreenBorderManager fullscreenBorderMan { get; private set; }

		public bool canMouseInteract { get; private set; }

		public OneshotWindow(Game1 game)
		{
			base.WindowIcon = "oneshot";
			base.WindowTitle = "oneshot_appname";
			base.ContentsSize = new Vec2(320, 240);
			AddButton(TWMWindowButtonType.Close, delegate
			{
				CloseGame();
			});
			AddButton(TWMWindowButtonType.Maximize, delegate
			{
				if (!IsModalWindowOpen() && !flagMan.IsFlagSet(123))
				{
					ToggleMaximize();
				}
			});
			AddButton(TWMWindowButtonType.Minimize);
			filmPuzzleMan = new FilmPuzzleManager(this);
			windowShakeMan = new WindowShakeManager(this);
			followerMan = new FollowerManager(this);
			flashMan = new FlashManager();
			pictureMan = new PictureManager();
			fastTravelMan = new FastTravelManager();
			flagMan = new FlagManager(this);
			varMan = new VariableManager();
			selfSwitchMan = new SelfSwitchManager(this);
			int num = 0;
			while (Game1.masterSaveMan.IsWritingFile() && num < 100)
			{
				num++;
				Thread.Sleep(100);
			}
			gameSaveMan = new GameSaveManager(this);
			menuMan = new MenuManager(this);
			gameSaveMan.LoadSettings();
			gameSaveMan.LoadPermaSave();
			tileMapMan = new TileMapManager(this);
			titleScreenMan = new TitleScreenManager(this);
			glitchEffectMan = new GlitchEffectManager();
			fullscreenBorderMan = new FullscreenBorderManager(this);
			if (gameSaveMan.SaveExists())
			{
				StartGame();
				return;
			}
			Game1.windowMan.Desktop.inSolstice = false;
			titleScreenMan.Open();
		}

		public override void ToggleMaximize()
		{
			base.ToggleMaximize();
			if (base.IsMaximized)
			{
				TWMWindowButton tWMWindowButton = windowButtons.First((TWMWindowButton b) => b.WindowButtonType == TWMWindowButtonType.Maximize);
				oldMaximizeButtonPos = tWMWindowButton.Position;
			}
			else
			{
				windowButtons.First((TWMWindowButton b) => b.WindowButtonType == TWMWindowButtonType.Maximize).Position = oldMaximizeButtonPos;
			}
			menuMan.SettingsMenu.UpdateSettingOptionsValues();
		}

		public void StartGame()
		{
			tileMapMan.LoadMap(1, 17f, 17f);
		}

		public void CloseGame()
		{
			if (titleScreenMan.IsOpen())
			{
				ExitGame();
			}
			else if (!tileMapMan.IsInScript())
			{
				tileMapMan.StartCommonEvent(35);
			}
			else if (!IsModalWindowOpen())
			{
				ShowModalWindow(ModalWindow.ModalType.Info, "oneshot_window_no_close_during_cutscene", null, playModalNoise: true, canAutomash: true);
			}
		}

		public void ExitGame()
		{
			Game1.windowMan.Desktop.RestoreWallpaper();
			Game1.soundMan.StopBGS();
			Game1.soundMan.PlaySong(string.Empty, 0f, 0f);
			Game1.gMan.clearTextureCache(TextureCache.CacheType.Game);
			onClose(this);
		}

		public override bool Update(bool mouseAlreadyOnOtherWindow)
		{
			fullscreenBorderFrameTimer++;
			if (fullscreenBorderFrameTimer >= 6)
			{
				fullscreenBorderFrameTimer = 0;
				fullscreenBorderFrameIndex++;
				if (fullscreenBorderFrameIndex >= 2)
				{
					fullscreenBorderFrameIndex = 0;
				}
			}
			canMouseInteract = !mouseAlreadyOnOtherWindow && !Game1.windowMan.IsModalWindowOpen();
			if (!IsModalWindowOpen() && !Game1.windowMan.IsModalWindowOpen())
			{
				GameLogic();
			}
			if (!base.IsMaximized)
			{
				return base.Update(mouseAlreadyOnOtherWindow);
			}
			TWMWindowButton tWMWindowButton = windowButtons.First((TWMWindowButton b) => b.WindowButtonType == TWMWindowButtonType.Maximize);
			tWMWindowButton.Position = new Vec2(Game1.gMan.DrawScreenSize.X / 2 - 2 - 16, 2);
			return tWMWindowButton.Update(Vec2.Zero, !Game1.windowMan.IsModalWindowOpen());
		}

		private void GameLogic()
		{
			if (gameSaveMan.PlayTimeFrameCount < long.MaxValue)
			{
				gameSaveMan.PlayTimeFrameCount++;
			}
			menuMan.Update();
			menuMan.ItemMan.UpdateSelectedItemIcon();
			glitchEffectMan.Update();
			fullscreenBorderMan.Update();
			if (!titleScreenMan.IsOpen())
			{
				tileMapMan.Update();
				filmPuzzleMan.Update();
				windowShakeMan.Update();
				tileMapMan.EventRunnerUpdate();
				pictureMan.Update();
				flashMan.Update();
			}
			else
			{
				titleScreenMan.Update();
			}
		}

		private void checkScripts()
		{
			for (int i = 1; i <= 263; i++)
			{
				string text = $"map{i}";
				Event[] events = JsonConvert.DeserializeObject<MapEvents>(File.ReadAllText(Game1.GameDataPath() + "/maps/events_" + text + ".json")).events;
				foreach (Event @event in events)
				{
					int num = 0;
					Event.Page[] pages = @event.pages;
					foreach (Event.Page page in pages)
					{
						new EventRunner(this, page.list, @event.id, num);
						num++;
					}
				}
			}
			CommonEvent[] common_events = JsonConvert.DeserializeObject<CommonEvents>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_common_events.json")).common_events;
			foreach (CommonEvent commonEvent in common_events)
			{
				new EventRunner(this, commonEvent.list, commonEvent.id, -1);
			}
			Game1.logMan.Log(LogManager.LogLevel.Info, "checked all unlocks");
		}

		public bool IsCloverEquipped()
		{
			return varMan.GetVariable(1) == 58;
		}

		public override void PreDraw()
		{
			Game1.gMan.BeginDrawToOneshotTexture();
			if (!titleScreenMan.IsOpen())
			{
				tileMapMan.Draw();
				menuMan.ItemMan.DrawSelectedItemIcon();
				menuMan.Draw();
				pictureMan.Draw();
				filmPuzzleMan.Draw();
				flashMan.Draw();
				tileMapMan.DrawEventRunners();
				tileMapMan.DrawMapTransition();
			}
			else
			{
				titleScreenMan.Draw();
				menuMan.Draw();
			}
			Game1.gMan.EndDrawToOneshotTexture();
			if (glitchEffectMan.GlitchSegmentsEnabled)
			{
				glitchEffectMan.Draw();
			}
		}

		public override void Draw(TWMTheme theme)
		{
			if (!base.IsMaximized)
			{
				base.Draw(theme);
				return;
			}
			Vec2 drawScreenSize = Game1.gMan.DrawScreenSize;
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, drawScreenSize.X / 2, drawScreenSize.Y / 2), GameColor.Black);
			GameColor currentBorderColor = fullscreenBorderMan.CurrentBorderColor;
			float num = (float)drawScreenSize.X / (float)drawScreenSize.Y;
			Rect fullscreenRect = GetFullscreenRect();
			Game1.gMan.DrawOneshotTexture(fullscreenRect, byte.MaxValue, menuMan.SettingsMenu.FullscreenScaling == SettingsMenu.ScalingMode.Smooth);
			if (1.3333334f <= num)
			{
				if (menuMan.SettingsMenu.IsFullscreenBorderEnabled)
				{
					Vec2 vec = Game1.gMan.TextureSize("the_world_machine/fullscreen_border_left", TextureCache.CacheType.TheWorldMachine);
					vec.X /= 2;
					float num2 = (float)drawScreenSize.Y / (float)vec.Y;
					int num3 = (int)Math.Round((float)vec.X * num2);
					Vec2 pixelPos = new Vec2(fullscreenRect.X - num3, 0);
					Rect srcRect = new Rect(fullscreenBorderFrameIndex * vec.X, 0, vec.X, vec.Y);
					GraphicsManager gMan = Game1.gMan;
					float rf = currentBorderColor.rf;
					float gf = currentBorderColor.gf;
					float bf = currentBorderColor.bf;
					gMan.MainBlit("the_world_machine/fullscreen_border_left", pixelPos, srcRect, num2, num2, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), rf, gf, bf, 0f, TextureCache.CacheType.TheWorldMachine);
					Vec2 vec2 = Game1.gMan.TextureSize("the_world_machine/fullscreen_border_right", TextureCache.CacheType.TheWorldMachine);
					vec2.X /= 2;
					float num4 = (float)drawScreenSize.Y / (float)vec2.Y;
					Vec2 pixelPos2 = new Vec2(fullscreenRect.X + fullscreenRect.W, 0);
					Rect srcRect2 = new Rect(fullscreenBorderFrameIndex * vec2.X, 0, vec2.X, vec2.Y);
					GraphicsManager gMan2 = Game1.gMan;
					bf = currentBorderColor.rf;
					gf = currentBorderColor.gf;
					rf = currentBorderColor.bf;
					gMan2.MainBlit("the_world_machine/fullscreen_border_right", pixelPos2, srcRect2, num4, num4, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), bf, gf, rf, 0f, TextureCache.CacheType.TheWorldMachine);
				}
			}
			else if (menuMan.SettingsMenu.IsFullscreenBorderEnabled)
			{
				Vec2 vec3 = Game1.gMan.TextureSize("the_world_machine/fullscreen_border_up", TextureCache.CacheType.TheWorldMachine);
				vec3.Y /= 2;
				float num5 = (float)drawScreenSize.X / (float)vec3.X;
				int num6 = (int)Math.Round((float)vec3.Y * num5);
				Vec2 pixelPos3 = new Vec2(0, fullscreenRect.Y - num6);
				Rect srcRect3 = new Rect(0, fullscreenBorderFrameIndex * vec3.Y, vec3.X, vec3.Y);
				GraphicsManager gMan3 = Game1.gMan;
				float rf = currentBorderColor.rf;
				float gf = currentBorderColor.gf;
				float bf = currentBorderColor.bf;
				gMan3.MainBlit("the_world_machine/fullscreen_border_up", pixelPos3, srcRect3, num5, num5, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), rf, gf, bf, 0f, TextureCache.CacheType.TheWorldMachine);
				Vec2 vec4 = Game1.gMan.TextureSize("the_world_machine/fullscreen_border_down", TextureCache.CacheType.TheWorldMachine);
				vec4.Y /= 2;
				float num7 = (float)drawScreenSize.X / (float)vec4.X;
				Vec2 pixelPos4 = new Vec2(0, fullscreenRect.Y + fullscreenRect.H);
				Rect srcRect4 = new Rect(0, fullscreenBorderFrameIndex * vec4.Y, vec4.X, vec4.Y);
				GraphicsManager gMan4 = Game1.gMan;
				bf = currentBorderColor.rf;
				gf = currentBorderColor.gf;
				rf = currentBorderColor.bf;
				gMan4.MainBlit("the_world_machine/fullscreen_border_down", pixelPos4, srcRect4, num7, num7, 1f, 0, GraphicsManager.BlendMode.Normal, default(GameTone), bf, gf, rf, 0f, TextureCache.CacheType.TheWorldMachine);
			}
			if (Game1.steamMan.IsTimedDemo)
			{
				int num8 = 24 + Game1.gMan.TextSize(GraphicsManager.FontType.OS, "00:00").X;
				int num9 = 24;
				Rect boxRect = new Rect(drawScreenSize.X / 2 - num8 - 4, drawScreenSize.Y / 2 - num9 - 4, num8, num9);
				Game1.gMan.ColorBoxBlit(boxRect, currentBorderColor);
				Game1.gMan.ColorBoxBlit(new Rect(boxRect.X + 2, boxRect.Y + 2, num8 - 4, num9 - 4), theme.Background());
				Vec2 pixelPos5 = new Vec2(boxRect.X + 4, boxRect.Y + 4);
				Game1.gMan.MainBlit("the_world_machine/window_icons/timer", pixelPos5, currentBorderColor, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, new Vec2(pixelPos5.X + 16, pixelPos5.Y - 1), Game1.steamMan.GetDemoTimeString(), currentBorderColor);
			}
			windowButtons.First((TWMWindowButton b) => b.WindowButtonType == TWMWindowButtonType.Maximize).Draw(Vec2.Zero, theme, byte.MaxValue);
		}

		public Rect GetFullscreenRect()
		{
			Vec2 drawScreenSize = Game1.gMan.DrawScreenSize;
			float num = (float)drawScreenSize.X / (float)drawScreenSize.Y;
			Rect result;
			if (1.3333334f <= num)
			{
				result = new Rect(0, 0, 320 * drawScreenSize.Y / 240, drawScreenSize.Y);
				if (menuMan.SettingsMenu.FullscreenScaling == SettingsMenu.ScalingMode.Integer)
				{
					int num2 = drawScreenSize.Y / 480;
					if (num2 >= 1)
					{
						result.W = 640 * num2;
						result.H = 480 * num2;
					}
				}
			}
			else
			{
				result = new Rect(0, 0, drawScreenSize.X, 240 * drawScreenSize.X / 320);
				if (menuMan.SettingsMenu.FullscreenScaling == SettingsMenu.ScalingMode.Integer)
				{
					int num3 = drawScreenSize.X / 640;
					if (num3 >= 1)
					{
						result.W = 640 * num3;
						result.H = 480 * num3;
					}
				}
			}
			result.Y = (drawScreenSize.Y - result.H) / 2;
			result.X = (drawScreenSize.X - result.W) / 2;
			return result;
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			Rect dstRect = new Rect(screenPos.X * 2, screenPos.Y * 2, 640, 480);
			Game1.gMan.DrawOneshotTexture(dstRect, alpha);
		}

		public override bool IsSameContent(TWMWindow other)
		{
			return other is OneshotWindow;
		}

		internal void CloseModalWindow()
		{
			modalWindow = null;
		}
	}
}

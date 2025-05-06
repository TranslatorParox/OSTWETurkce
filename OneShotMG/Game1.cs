using System;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using OneShotMG.src;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;

namespace OneShotMG
{
	public class Game1 : Game
	{
		public static LogManager logMan;

		public static GraphicsManager gMan;

		public static SoundManager soundMan;

		public static InputManager inputMan;

		public static WindowManager windowMan;

		public static MouseCursorManager mouseCursorMan;

		public static BootManager bootMan;

		public static LanguageManager languageMan;

		public static MasterSaveManager masterSaveMan;

		public static SteamManager steamMan;

		public static IniData Config;

		private static bool shutdownCalled = false;

		public static string VersionString = "v1.24.12.22.0";

		private bool debugFrameAdvanceMode;

		private bool debugAdvanceFrame;

		public static int GlobalFrameCounter { get; private set; }

		public static string SaveFolderName { get; private set; } = "OneShotWME";

		public Game1()
		{
			logMan = new LogManager();
			try
			{
				steamMan = new SteamManager();
				if (steamMan.IsTimedDemo)
				{
					SaveFolderName = "OneShotWMEDemo";
				}
				Config = new FileIniDataParser().ReadFile("config.ini");
				base.IsFixedTimeStep = true;
				base.MaxElapsedTime = TimeSpan.FromSeconds(0.20000000298023224);
				base.InactiveSleepTime = TimeSpan.FromSeconds(0.0);
				base.Content.RootDirectory = Config["paths"]["content"];
				gMan = new GraphicsManager(this);
				soundMan = new SoundManager();
				masterSaveMan = new MasterSaveManager();
				languageMan = new LanguageManager();
				inputMan = new InputManager();
				bootMan = new BootManager(this);
				mouseCursorMan = new MouseCursorManager();
			}
			catch (Exception ex)
			{
				logMan.Log(LogManager.LogLevel.Error, ex.Message);
				logMan.Log(LogManager.LogLevel.StackDump, ex.StackTrace);
				logMan.Dispose();
				Environment.Exit(0);
			}
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
		}

		protected override void UnloadContent()
		{
			logMan.Dispose();
			steamMan.ShutDown();
		}

		public static string GameDataPath()
		{
			return Config["paths"]["gamedata"];
		}

		public void InitWindowManager()
		{
			windowMan = new WindowManager(this);
		}

		public static void ShutDown()
		{
			shutdownCalled = true;
		}

		protected override void Update(GameTime gameTime)
		{
			try
			{
				GlobalFrameCounter++;
				if (GlobalFrameCounter >= int.MaxValue)
				{
					GlobalFrameCounter = 0;
				}
				inputMan.Update();
				soundMan.Update();
				if (inputMan.IsButtonPressed(InputManager.Button.DEBUG_FRAME_ADVANCE_MODE))
				{
					debugFrameAdvanceMode = !debugFrameAdvanceMode;
				}
				if (inputMan.IsButtonPressed(InputManager.Button.DEBUG_FRAME_ADVANCE))
				{
					debugAdvanceFrame = true;
				}
				if (!debugFrameAdvanceMode || debugAdvanceFrame)
				{
					debugAdvanceFrame = false;
					gMan.TempTexMan.Update();
					masterSaveMan.Update();
					bootMan.Update();
					if (bootMan.SequenceComplete)
					{
						mouseCursorMan.Update();
						windowMan.Update();
					}
				}
				if (inputMan.IsButtonPressed(InputManager.Button.FullScreen))
				{
					gMan.ToggleFullscreen();
					if (windowMan != null)
					{
						windowMan.UpdateCustomizationWindowFullscreen(gMan.IsFullscreen());
					}
				}
				base.Update(gameTime);
			}
			catch (ContentLoadException ex)
			{
				logMan.Log(LogManager.LogLevel.Error, ex.InnerException?.Message);
				logMan.Log(LogManager.LogLevel.StackDump, ex.StackTrace);
				logMan.Dispose();
				Environment.Exit(0);
			}
			catch (Exception ex2)
			{
				logMan.Log(LogManager.LogLevel.Error, ex2.Message);
				logMan.Log(LogManager.LogLevel.StackDump, ex2.StackTrace);
				logMan.Dispose();
				Environment.Exit(0);
			}
			if (shutdownCalled && !masterSaveMan.IsWritingFile())
			{
				Exit();
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			try
			{
				gMan.StartDrawCycle();
				if (bootMan.ShowOs)
				{
					windowMan.Draw();
					mouseCursorMan.Draw();
				}
				bootMan.Draw();
				masterSaveMan.Draw();
				gMan.EndDrawCycle(windowMan == null || windowMan.IsSystemScalingSmooth, gMan.DrawScreenSize);
				base.Draw(gameTime);
			}
			catch (ContentLoadException ex)
			{
				logMan.Log(LogManager.LogLevel.Error, ex.InnerException?.Message);
				logMan.Log(LogManager.LogLevel.StackDump, ex.StackTrace);
				logMan.Dispose();
				Environment.Exit(0);
			}
			catch (Exception ex2)
			{
				logMan.Log(LogManager.LogLevel.Error, ex2.Message);
				logMan.Log(LogManager.LogLevel.StackDump, ex2.StackTrace);
				logMan.Dispose();
				Environment.Exit(0);
			}
		}
	}
}

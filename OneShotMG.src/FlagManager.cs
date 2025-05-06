using OneShotMG.src.TWM;

namespace OneShotMG.src
{
	public class FlagManager
	{
		private OneshotWindow oneshotWindow;

		public readonly int TotalFlags = 500;

		private ulong[] flagData;

		private const int FLAG_ALWAYS_TRUE = 25;

		public const int FLAG_ON_ROOMBA = 79;

		public const int FLAG_LOCK_CAMERA_TO_MAP = 98;

		public const int FLAG_LOCK_CAMERA = 100;

		public const int MADE_FINAL_CHOICE = 147;

		public const int BEAT_SOLSTICE_FLAG = 160;

		private const int FLAG_COLORBLIND_MODE = 252;

		private const int GLITCHES_APPEARED_ON_SILVER_IN_MINES = 353;

		private const int FLAG_ENCOUNTER_OVER = 317;

		private const int NO_DANGEN_CREDITS = 390;

		public const int FLAG_TIMED_DEMO = 395;

		public const int FLAG_DEMO_MODE = 396;

		private const int FLAG_COLLECTIBLE_WP_LAMP = 401;

		private const int FLAG_COLLECTIBLE_WP_FACTORY = 402;

		private const int FLAG_COLLECTIBLE_WP_NAVIGATE = 403;

		private const int FLAG_COLLECTIBLE_WP_COURTYARD = 404;

		private const int FLAG_COLLECTIBLE_WP_RUINS = 405;

		private const int FLAG_COLLECTIBLE_WP_CATWALKS = 406;

		private const int FLAG_COLLECTIBLE_WP_LAMPLIGHTER = 407;

		private const int FLAG_COLLECTIBLE_WP_SECRET = 408;

		private const int FLAG_COLLECTIBLE_WP_LIBRARY = 409;

		private const int FLAG_COLLECTIBLE_WP_CAFE = 410;

		private const int FLAG_COLLECTIBLE_WP_PLANT = 411;

		private const int FLAG_COLLECTIBLE_WP_TOWER = 412;

		private const int FLAG_COLLECTIBLE_WP_MINECART = 413;

		private const int FLAG_COLLECTIBLE_WP_SIBLINGS = 415;

		private const int FLAG_COLLECTIBLE_WP_WORLD_MACHINE = 416;

		private const int FLAG_COLLECTIBLE_PROF_PROPHETBOT = 426;

		private const int FLAG_COLLECTIBLE_PROF_SILVER = 427;

		private const int FLAG_COLLECTIBLE_PROF_ROWBOT = 428;

		private const int FLAG_COLLECTIBLE_PROF_SHEPHERD = 429;

		private const int FLAG_COLLECTIBLE_PROF_MAGPIE = 430;

		private const int FLAG_COLLECTIBLE_PROF_CALAMUS = 431;

		private const int FLAG_COLLECTIBLE_PROF_ALULA = 432;

		private const int FLAG_COLLECTIBLE_PROF_MAIZE = 433;

		private const int FLAG_COLLECTIBLE_PROF_LING = 434;

		private const int FLAG_COLLECTIBLE_PROF_WATCHER = 435;

		private const int FLAG_COLLECTIBLE_PROF_MASON = 436;

		private const int FLAG_COLLECTIBLE_PROF_LAMPLIGHTER = 437;

		private const int FLAG_COLLECTIBLE_PROF_KELVIN = 438;

		private const int FLAG_COLLECTIBLE_PROF_KIP = 439;

		private const int FLAG_COLLECTIBLE_PROF_GEORGE1 = 440;

		private const int FLAG_COLLECTIBLE_PROF_GEORGE2 = 441;

		private const int FLAG_COLLECTIBLE_PROF_GEORGE3 = 442;

		private const int FLAG_COLLECTIBLE_PROF_GEORGE4 = 443;

		private const int FLAG_COLLECTIBLE_PROF_GEORGE5 = 444;

		private const int FLAG_COLLECTIBLE_PROF_GEORGE6 = 445;

		private const int FLAG_COLLECTIBLE_PROF_PROTOTYPE = 446;

		private const int FLAG_COLLECTIBLE_PROF_CEDRIC = 447;

		private const int FLAG_COLLECTIBLE_PROF_RUE = 448;

		private const int FLAG_COLLECTIBLE_PROF_WORLD_MACHINE = 449;

		private const int FLAG_COLLECTIBLE_PROF_AUTHOR = 450;

		private const int FLAG_COLLECTIBLE_PROF_NIKO = 451;

		public const int FLAG_COLLECTIBLE_THEME_BLUE = 461;

		public const int FLAG_COLLECTIBLE_THEME_TEAL = 462;

		public const int FLAG_COLLECTIBLE_THEME_GREEN = 463;

		public const int FLAG_COLLECTIBLE_THEME_YELLOW = 464;

		public const int FLAG_COLLECTIBLE_THEME_RED = 465;

		public const int FLAG_COLLECTIBLE_THEME_PINK = 466;

		public const int FLAG_COLLECTIBLE_THEME_ORANGE = 467;

		public const int FLAG_COLLECTIBLE_THEME_WHITE = 468;

		public const int FLAG_COLLECTIBLE_THEME_RAINBOW = 469;

		public FlagManager(OneshotWindow osWindow, bool permaFlags = false)
		{
			oneshotWindow = osWindow;
			if (permaFlags)
			{
				TotalFlags = 25;
			}
			flagData = new ulong[TotalFlags / 64 + 1];
		}

		public bool SetFlag(int flagIndex)
		{
			int num = flagIndex / 64;
			if (num >= flagData.Length || num < 0)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"(SetFlag) flagIndex {flagIndex} is out of bounds!");
				return false;
			}
			int num2 = flagIndex % 64;
			flagData[num] |= (ulong)(1L << num2);
			return true;
		}

		public bool UnsetFlag(int flagIndex)
		{
			int num = flagIndex / 64;
			if (num >= flagData.Length || num < 0)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"(UnsetFlag) flagIndex {flagIndex} is out of bounds!");
				return false;
			}
			int num2 = flagIndex % 64;
			flagData[num] &= (ulong)((1L << num2) ^ -1);
			return true;
		}

		public bool IsFlagSet(int flagIndex)
		{
			switch (flagIndex)
			{
			case 390:
				return true;
			case 395:
				return Game1.steamMan.IsTimedDemo;
			case 396:
				return Game1.steamMan.IsDemoMode;
			case 25:
				return true;
			case 252:
				if (oneshotWindow != null)
				{
					return oneshotWindow.menuMan.SettingsMenu.IsColorBlindMode;
				}
				return false;
			case 401:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("lamp");
			case 402:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("factory");
			case 403:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("navigate");
			case 404:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("courtyard");
			case 405:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("ruins");
			case 406:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("catwalks");
			case 407:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("lamplighter");
			case 408:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("secret");
			case 409:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("library");
			case 410:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("cafe");
			case 411:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("plant");
			case 412:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("tower");
			case 413:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("minecart");
			case 415:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("siblings");
			case 416:
				return Game1.windowMan.UnlockMan.IsWallpaperUnlocked("world_machine");
			case 426:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("prophetbot");
			case 427:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("silver");
			case 428:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("rowbot");
			case 429:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("shepherd");
			case 430:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("magpie");
			case 431:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("calamus");
			case 432:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("alula");
			case 433:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("maize");
			case 434:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("ling");
			case 435:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("watcher");
			case 436:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("mason");
			case 437:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("lamplighter");
			case 438:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("kelvin");
			case 439:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("kip");
			case 440:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("george1");
			case 441:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("george2");
			case 442:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("george3");
			case 443:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("george4");
			case 444:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("george5");
			case 445:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("george6");
			case 446:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("prototype");
			case 447:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("cedric");
			case 448:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("rue");
			case 449:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("world_machine");
			case 450:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("author");
			case 451:
				return Game1.windowMan.UnlockMan.IsProfileUnlocked("niko");
			case 461:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("blue");
			case 462:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("teal");
			case 463:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("green");
			case 464:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("yellow");
			case 465:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("red");
			case 466:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("pink");
			case 467:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("orange");
			case 468:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("white");
			case 469:
				return Game1.windowMan.UnlockMan.IsThemeUnlocked("rainbow");
			default:
			{
				int num = flagIndex / 64;
				if (num >= flagData.Length || num < 0)
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, $"(CheckFlag) flagIndex {flagIndex} is out of bounds!");
					return false;
				}
				int num2 = flagIndex % 64;
				return (flagData[num] & (ulong)(1L << num2)) != 0;
			}
			}
		}

		public ulong[] GetRawFlagData()
		{
			return flagData;
		}

		public void SetRawFlagData(ulong[] newFlagData)
		{
			flagData = newFlagData;
		}

		public bool IsSolticeGlitchTime()
		{
			if (IsFlagSet(353))
			{
				return !IsFlagSet(317);
			}
			return false;
		}
	}
}

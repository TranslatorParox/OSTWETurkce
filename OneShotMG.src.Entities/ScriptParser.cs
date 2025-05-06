using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Menus;
using OneShotMG.src.TWM;
using OneShotMG.src.TWM.Filesystem;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class ScriptParser
	{
		public enum ScriptConditionalResult
		{
			No,
			Yes,
			EdText
		}

		public const int TEMP_VAR_1_VAR_ID = 22;

		public const int TEMP_VAR_2_VAR_ID = 23;

		public const int TEMP_FLAG_1_VAR_ID = 22;

		public const int TEMP_FLAG_2_VAR_ID = 23;

		public const int OUTDOOR_TRANSITION_FLAG = 11;

		public const string JOURNAL_NAME = "app_journal";

		private const string DOCUMENT_NAME = "app_safe_document";

		public const string BIG_PORTAL_FOLDERNAME = "big_portal_foldername";

		public const string PORTAL_1_FOLDERNAME = "portal_1_foldername";

		public const string PORTAL_2_FOLDERNAME = "portal_2_foldername";

		public const string PORTAL_3_FOLDERNAME = "portal_3_foldername";

		public const string KEY_B_FILENAME = "key_b_filename";

		public const string KEY_R_FILENAME = "key_r_filename";

		public const string KEY_G_FILENAME = "key_g_filename";

		public const string PROTOTYPE_NPCSHEET_FILENAME = "prototype_npcsheet_filename";

		public const string PROTOTYPE_FACEPIC_FILENAME = "prototype_facepic_filename";

		public const string CEDRIC_NPCSHEET_FILENAME = "cedric_npcsheet_filename";

		public const string CEDRIC_FACEPIC_FILENAME = "cedric_facepic_filename";

		public const string RUE_NPCSHEET_FILENAME = "rue_npcsheet_filename";

		public const string RUE_FACEPIC_FILENAME = "rue_facepic_filename";

		private static readonly byte[] GLEN_PUZZLE_SOLUTION = new byte[30]
		{
			0, 1, 1, 1, 0, 1, 0, 0, 0, 1,
			1, 0, 1, 0, 1, 1, 0, 0, 0, 1,
			0, 1, 1, 1, 0, 0, 0, 1, 0, 0
		};

		private static readonly byte[] TOWER_PUZZLE_SOLUTION_1 = new byte[121]
		{
			0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 1, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 1, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			1, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 1, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 1, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 1, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
			0
		};

		private static readonly byte[] TOWER_PUZZLE_SOLUTION_3 = new byte[121]
		{
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			0, 1, 1, 1, 1, 1, 1, 1, 1, 1,
			1, 0, 1, 1, 1, 1, 1, 1, 0, 0,
			0, 0, 0, 1, 1, 1, 1, 1, 1, 0,
			0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
			0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
			1, 0, 0, 0, 0, 0, 1, 1, 1, 1,
			1, 1, 0, 0, 0, 0, 0, 1, 1, 1,
			1, 1, 1, 0, 0, 0, 0, 0, 1, 1,
			1, 1, 1, 1, 0, 0, 0, 0, 0, 1,
			1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
			1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
			0
		};

		private static readonly byte[] TOWER_PUZZLE_SOLUTION_4;

		private static readonly byte[] TOWER_PUZZLE_SOLUTION_5;

		public static bool HandleScript(OneshotWindow oneshotWindow, string script, EventRunner caller, int pageId)
		{
			if (script.StartsWith("check_exit"))
			{
				if (oneshotWindow.flagMan.IsFlagSet(11))
				{
					oneshotWindow.flagMan.UnsetFlag(22);
					return false;
				}
				script = script.Substring("check_exit ".Length);
				string[] array = script.Split(' ');
				if (array.Length != 4)
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, "improperly formatted check exit command - check_exit " + script);
					return false;
				}
				array[0] = array[0].Substring(0, array[0].Length - 1);
				array[1] = array[1].Substring(0, array[1].Length - 1);
				int num;
				int num2;
				int num3;
				int num4;
				if (array[2] == "x:")
				{
					num = int.Parse(array[3], CultureInfo.InvariantCulture);
					num2 = int.Parse(array[3], CultureInfo.InvariantCulture);
					num3 = int.Parse(array[0], CultureInfo.InvariantCulture);
					num4 = int.Parse(array[1], CultureInfo.InvariantCulture);
				}
				else
				{
					num = int.Parse(array[0], CultureInfo.InvariantCulture);
					num2 = int.Parse(array[1], CultureInfo.InvariantCulture);
					num3 = int.Parse(array[3], CultureInfo.InvariantCulture);
					num4 = int.Parse(array[3], CultureInfo.InvariantCulture);
				}
				Vec2 currentTile = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				if (currentTile.X >= num && currentTile.X <= num2 && currentTile.Y >= num3 && currentTile.Y <= num4)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				return false;
			}
			if (script.StartsWith("EdText"))
			{
				HandleEdText(oneshotWindow, script, pageId);
				return true;
			}
			if (script.StartsWith("UnlockProfile"))
			{
				Match match = Regex.Match(script, ".+?['\"](.+?)['\"]");
				if (match.Success)
				{
					Game1.windowMan.UnlockMan.UnlockProfile(match.Groups[1].Value);
				}
				else
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "Failed to match profile unlock: " + script);
				}
				return false;
			}
			if (script.StartsWith("UnlockWallpaper"))
			{
				Match match2 = Regex.Match(script, ".+?['\"](.+?)['\"]");
				if (match2.Success)
				{
					Game1.windowMan.UnlockMan.UnlockWallpaper(match2.Groups[1].Value);
				}
				else
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "Failed to match wallpaper unlock: " + script);
				}
				return false;
			}
			if (script.StartsWith("UnlockTheme"))
			{
				Match match3 = Regex.Match(script, ".+?['\"](.+?)['\"]");
				if (match3.Success)
				{
					Game1.windowMan.UnlockMan.UnlockTheme(match3.Groups[1].Value);
				}
				else
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "Failed to match theme unlock: " + script);
				}
				return false;
			}
			if (script.StartsWith("Steam.unlock"))
			{
				Match match4 = Regex.Match(script, ".+?['\"](.+?)['\"]");
				if (match4.Success)
				{
					Game1.windowMan.UnlockMan.UnlockAchievement(match4.Groups[1].Value);
				}
				else
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "Failed to match achievement " + script);
				}
				return false;
			}
			if (script.StartsWith("unlock_map "))
			{
				UnlockMap(oneshotWindow, script, caller);
				return false;
			}
			if (script.StartsWith("Journal.set "))
			{
				script = script.Substring("Journal.set ".Length);
				string journalPage = script.Substring(1, script.Length - 2);
				Game1.windowMan.SetJournalPage(journalPage);
				return false;
			}
			if (script.StartsWith("Script.fix_footsplashes("))
			{
				script = script.Substring("Script.fix_footsplashes(".Length);
				script = script.Substring(0, script.IndexOf(')'));
				string[] array2 = script.Split(',');
				int x = int.Parse(array2[0], CultureInfo.InvariantCulture);
				int y = int.Parse(array2[1], CultureInfo.InvariantCulture);
				oneshotWindow.tileMapMan.MoveFootsplashesRelative(x, y);
				return false;
			}
			if (script.StartsWith("Script.move_player_relative("))
			{
				script = script.Substring("Script.move_player_relative(".Length);
				script = script.Substring(0, script.IndexOf(')'));
				string[] array3 = script.Split(',');
				int x2 = int.Parse(array3[0], CultureInfo.InvariantCulture);
				int y2 = int.Parse(array3[1], CultureInfo.InvariantCulture);
				oneshotWindow.tileMapMan.MovePlayerRelative(x2, y2);
				return false;
			}
			if (script.StartsWith("change_map "))
			{
				script = script.Substring("change_map ".Length);
				int mapId = int.Parse(script, CultureInfo.InvariantCulture);
				float playerXTile = oneshotWindow.tileMapMan.GetPlayerXTile();
				float playerYTile = oneshotWindow.tileMapMan.GetPlayerYTile();
				oneshotWindow.tileMapMan.ChangeMap(mapId, playerXTile, playerYTile, 0f, Entity.Direction.None);
				return false;
			}
			switch (script)
			{
			case "demo_end_check":
				if (Game1.steamMan.IsDemoMode)
				{
					Game1.windowMan.EndDemo();
				}
				break;
			case "create_author_prof_file":
				if (!Game1.windowMan.UnlockMan.IsProfileUnlocked("author") && !Game1.windowMan.FileSystem.FileTypeExists(LaunchableWindowType.AUTHOR_PROFILE))
				{
					TWMFile tWMFile = new TWMFile("contacts", "author_prof_filename", LaunchableWindowType.AUTHOR_PROFILE);
					tWMFile.deleteRestricted = true;
					Game1.windowMan.FileSystem.WriteFile("/docs_foldername/mygames_foldername/oneshot_foldername/", tWMFile);
				}
				if (!Game1.windowMan.UnlockMan.IsProfileUnlocked("niko") && !Game1.windowMan.FileSystem.FileTypeExists(LaunchableWindowType.NIKO_PROFILE))
				{
					TWMFile tWMFile2 = new TWMFile("contacts", "niko_prof_filename", LaunchableWindowType.NIKO_PROFILE);
					tWMFile2.deleteRestricted = true;
					Game1.windowMan.FileSystem.WriteFile("/docs_foldername/", tWMFile2);
				}
				if (!Game1.windowMan.UnlockMan.IsWallpaperUnlocked("from_niko") && !Game1.windowMan.FileSystem.FileTypeExists(LaunchableWindowType.FROMNIKO_WALLPAPER))
				{
					TWMFile tWMFile3 = new TWMFile("photo", "fromniko_wallpaper_filename", LaunchableWindowType.FROMNIKO_WALLPAPER);
					tWMFile3.deleteRestricted = true;
					Game1.windowMan.FileSystem.WriteFile("/docs_foldername/", tWMFile3);
				}
				break;
			case "GlitchPixelEffect()":
				oneshotWindow.glitchEffectMan.StartGlitch(oneshotWindow, 20);
				break;
			case "GlitchPixelEffectLong()":
				oneshotWindow.glitchEffectMan.StartGlitch(oneshotWindow, 40);
				break;
			case "GlitchPixelEffectWeak()":
				oneshotWindow.glitchEffectMan.StartGlitch(oneshotWindow, 20, 0.1f);
				break;
			case "GlitchPixelEffectWeakest()":
				oneshotWindow.glitchEffectMan.StartGlitch(oneshotWindow, 40, 0.02f);
				break;
			case "GlitchPixelEffectWeakLong()":
				oneshotWindow.glitchEffectMan.StartGlitch(oneshotWindow, 40, 0.1f);
				break;
			case "watcher_tell_time":
			{
				int hour = DateTime.Now.Hour;
				if (hour >= 6 && hour < 12)
				{
					oneshotWindow.varMan.SetVariable(22, 0);
				}
				else if (hour >= 12 && hour < 17)
				{
					oneshotWindow.varMan.SetVariable(22, 1);
				}
				else if (hour >= 17)
				{
					oneshotWindow.varMan.SetVariable(22, 2);
				}
				else
				{
					oneshotWindow.varMan.SetVariable(22, 3);
				}
				break;
			}
			case "$scene = Scene_Map.new":
				oneshotWindow.menuMan.ItemMan.Close();
				break;
			case "disable_travel":
				oneshotWindow.fastTravelMan.DisableFastTravel();
				break;
			case "enable_travel":
				oneshotWindow.fastTravelMan.EnableFastTravel();
				break;
			case "save":
				oneshotWindow.gameSaveMan.MakeSave();
				break;
			case "Script.niko_reflection_enc_update":
				if (caller.TriggeringEntity != null)
				{
					Entity player2 = oneshotWindow.tileMapMan.GetPlayer();
					Vec2 pos2 = player2.GetPos();
					pos2.Y = 83968 - (pos2.Y - 83968);
					if (pos2.Y > 79872)
					{
						pos2.Y = 79872;
					}
					caller.TriggeringEntity.SetPos(pos2);
					caller.TriggeringEntity.SetFrameIndex(player2.GetFrameIndex());
					switch (player2.GetDirection())
					{
					case Entity.Direction.Left:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Left);
						break;
					case Entity.Direction.Right:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Right);
						break;
					case Entity.Direction.Down:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Up);
						break;
					case Entity.Direction.Up:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Down);
						break;
					}
				}
				break;
			case "Script.niko_reflection_peng_update":
				if (caller.TriggeringEntity != null)
				{
					Vec2 pos3 = oneshotWindow.tileMapMan.GetPlayer().GetPos();
					pos3.Y = 83968 - (pos3.Y - 83968);
					if (pos3.Y > 79872)
					{
						pos3.Y = 79872;
					}
					caller.TriggeringEntity.SetPos(pos3);
				}
				break;
			case "Script.niko_reflection_update":
				if (caller.TriggeringEntity != null)
				{
					Entity player = oneshotWindow.tileMapMan.GetPlayer();
					Vec2 pos = player.GetPos();
					pos.Y = 57344 - (pos.Y - 57344);
					caller.TriggeringEntity.SetPos(pos);
					caller.TriggeringEntity.SetFrameIndex(player.GetFrameIndex());
					caller.TriggeringEntity.SetNPCSheet(player.GetNPCSheet());
					switch (player.GetDirection())
					{
					case Entity.Direction.Left:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Left);
						break;
					case Entity.Direction.Right:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Right);
						break;
					case Entity.Direction.Down:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Up);
						break;
					case Entity.Direction.Up:
						caller.TriggeringEntity.SetDirection(Entity.Direction.Down);
						break;
					}
				}
				break;
			case "house_ambient\nadd_light :eyes, 'niko_eyes', 1.0, \n14 * 32, 16 * 32":
				HouseAmbient(oneshotWindow);
				break;
			case "house_ambient":
				HouseAmbient(oneshotWindow);
				break;
			case "blue_ambient":
				BlueAmbient(oneshotWindow);
				break;
			case "quit_game_bed":
				oneshotWindow.gameSaveMan.MakeSave();
				oneshotWindow.ExitGame();
				return true;
			case "quit_game_no_save":
				oneshotWindow.ExitGame();
				return true;
			case "real_load":
				if (!oneshotWindow.gameSaveMan.LoadSave())
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, Game1.languageMan.GetTWMLocString("game_load_failed"));
					oneshotWindow.gameSaveMan.ClearCorruptSave();
					oneshotWindow.ExitGame();
				}
				caller.EndEvent();
				break;
			case "$game_variables[22] = $game_player.x\n$game_variables[23] = $game_player.y":
			{
				Vec2 currentTile10 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				oneshotWindow.varMan.SetVariable(22, currentTile10.X);
				oneshotWindow.varMan.SetVariable(23, currentTile10.Y);
				break;
			}
			case "Script.tmp_s1 = Script.px == 26 && \nScript.py >= 23":
			{
				Vec2 currentTile8 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				if (currentTile8.X == 26 && currentTile8.Y >= 23)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.px == 33":
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X == 33)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = Script.py >= 29":
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y >= 29)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_v2 = Script.py + 2":
			{
				Vec2 currentTile5 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				oneshotWindow.varMan.SetVariable(23, currentTile5.Y + 2);
				break;
			}
			case "Script.tmp_s1 = Script.py >= 29\nScript.tmp_v2 = Script.py":
			{
				Vec2 currentTile2 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				if (currentTile2.Y >= 29)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				oneshotWindow.varMan.SetVariable(23, currentTile2.Y);
				break;
			}
			case "Script.tmp_s1 = Script.px == 59 && \nScript.py >= 34":
			{
				Vec2 currentTile12 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				if (currentTile12.Y >= 34 && currentTile12.X == 59)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.py < \nScript.eve_y(\"calamus npc\") - 2":
			{
				Vec2 currentTile11 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				Vec2 vec8 = oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero;
				if (currentTile11.Y < vec8.Y - 2)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.py > \nScript.eve_y(\"calamus npc\") + 2":
			{
				Vec2 currentTile9 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				Vec2 vec7 = oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero;
				if (currentTile9.Y > vec7.Y + 2)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.px > \nScript.eve_x(\"calamus npc\") + 2":
			{
				Vec2 currentTile7 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				Vec2 vec6 = oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero;
				if (currentTile7.X > vec6.X + 2)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.px < \nScript.eve_x(\"calamus npc\") - 2":
			{
				Vec2 currentTile6 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				Vec2 vec5 = oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero;
				if (currentTile6.X < vec5.X - 2)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = 46 > \nScript.eve_y(\"calamus npc\") ":
				if (46 > (oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero).Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = 46 < \nScript.eve_y(\"calamus npc\")":
				if (46 < (oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero).Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = 60 > \nScript.eve_x(\"calamus npc\") ":
				if (60 > (oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero).X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = 60 < \nScript.eve_x(\"calamus npc\")":
				if (60 < (oneshotWindow.tileMapMan.GetEntityByID(37)?.GetCurrentTile() ?? Vec2.Zero).X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = \nScript.eve_x(\"silver walk\") >= 47":
			{
				Vec2 obj = oneshotWindow.tileMapMan.GetEntityByID(23)?.GetCurrentTile() ?? Vec2.Zero;
				if (obj.X >= 47)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.py > \nScript.eve_y(\"cedric npc\")":
			{
				Vec2 vec4 = oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y > vec4.Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.py < \nScript.eve_y(\"cedric npc\")":
			{
				Vec2 vec2 = oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y < vec2.Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.px > \nScript.eve_x(\"cedric npc\") + 2":
			{
				Vec2 vec12 = oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X > vec12.X + 2)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.px < \nScript.eve_x(\"cedric npc\") - 2":
			{
				Vec2 vec10 = oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X < vec10.X - 2)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = 57 > \nScript.eve_y(\"cedric npc\") ":
				if (57 > (oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero).Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = 57 < \nScript.eve_y(\"cedric npc\")":
				if (57 < (oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero).Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = 73 > \nScript.eve_x(\"cedric npc\") ":
				if (73 > (oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero).X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = 73 < \nScript.eve_x(\"cedric npc\")":
				if (73 < (oneshotWindow.tileMapMan.GetEntityByID(129)?.GetCurrentTile() ?? Vec2.Zero).X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = Script.px == \nScript.eve_x(\"rue\")":
			{
				Vec2 vec3 = oneshotWindow.tileMapMan.GetEntityByID(12)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X == vec3.X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.py == \nScript.eve_y(\"rue\")":
			{
				Vec2 vec = oneshotWindow.tileMapMan.GetEntityByID(12)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y == vec.Y)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.py >= 14":
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y >= 14)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "Script.tmp_s1 = Script.px > \nScript.eve_x(\"silver\")":
			{
				Vec2 vec11 = oneshotWindow.tileMapMan.GetEntityByID(63)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X > vec11.X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "Script.tmp_s1 = Script.px < \nScript.eve_x(\"silver\")":
			{
				Vec2 vec9 = oneshotWindow.tileMapMan.GetEntityByID(63)?.GetCurrentTile() ?? Vec2.Zero;
				if (oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X < vec9.X)
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			}
			case "enter_name":
				caller.OpenNameInput();
				return true;
			case "Graphics.smooth = true":
				Game1.gMan.SmoothScaleWindow = true;
				break;
			case "Graphics.smooth = false":
				Game1.gMan.SmoothScaleWindow = false;
				break;
			case "blue_ambient\nbg 'blue_mine'\n$game_temp.footstep_sfx = \n['step_gravel']":
				BlueAmbient(oneshotWindow);
				oneshotWindow.tileMapMan.SetBackground("blue_mine");
				oneshotWindow.tileMapMan.OverrideStepSounds(new List<List<string>>
				{
					new List<string> { "step_gravel" }
				});
				break;
			case "$game_temp.footstep_sfx = \\\n['step_tile_soft',\\\n   'step_wood',\\\n   'step_grate_soft']":
				oneshotWindow.tileMapMan.OverrideStepSounds(new List<List<string>>
				{
					new List<string> { "step_tile_soft01", "step_tile_soft02", "step_tile_soft03", "step_tile_soft04" },
					new List<string> { "step_wood" },
					new List<string> { "step_grate_soft" }
				});
				break;
			case "ambient -30, -30, -30\nbg 'green_maize'\n$game_temp.footstep_sfx = ['step_grass']":
				oneshotWindow.tileMapMan.SetAmbientTone(new GameTone(-30, -30, -30));
				oneshotWindow.tileMapMan.SetBackground("green_maize");
				oneshotWindow.tileMapMan.OverrideStepSounds(new List<List<string>>
				{
					new List<string> { "step_grass" }
				});
				break;
			case "safe_puzzle_write":
			{
				string text4 = $"{oneshotWindow.varMan.GetVariable(20)}";
				TWMFile file4 = new TWMFile("doc", "app_safe_document", LaunchableWindowType.DOCUMENT, "document", text4);
				Game1.windowMan.FileSystem.WriteFile("/docs_foldername/", file4);
				break;
			}
			case "safe_puzzle_postgame_write":
			{
				string text3 = $"{oneshotWindow.varMan.GetVariable(20)}";
				TWMFile file3 = new TWMFile("doc", "app_safe_document", LaunchableWindowType.DOCUMENT, "document_postgame", text3);
				Game1.windowMan.FileSystem.WriteFile("/docs_foldername/", file3);
				break;
			}
			case "Script.copy_journal":
			{
				string path4 = "/docs_foldername/mygames_foldername/oneshot_foldername/";
				Game1.windowMan.FileSystem.AddDir(path4);
				TWMFile tWMFile7 = new TWMFile("clover", "app_journal", LaunchableWindowType.JOURNAL);
				tWMFile7.deleteRestricted = true;
				tWMFile7.moveRestricted = true;
				Game1.windowMan.FileSystem.WriteFile(path4, tWMFile7);
				break;
			}
			case "Script.create_boxes":
			{
				string text2 = "/portals_foldername/";
				Game1.windowMan.FileSystem.AddDir(text2 + "portal_1_foldername/");
				Game1.windowMan.FileSystem.AddDir(text2 + "portal_2_foldername/");
				Game1.windowMan.FileSystem.AddDir(text2 + "portal_3_foldername/");
				Game1.windowMan.FileSystem.AddDir(text2 + "big_portal_foldername/");
				break;
			}
			case "Script.clear_boxes":
			{
				string text = "/portals_foldername/";
				TWMFilesystem fileSystem = Game1.windowMan.FileSystem;
				fileSystem.Delete(text + "portal_1_foldername/prototype_npcsheet_filename");
				fileSystem.Delete(text + "portal_1_foldername/prototype_facepic_filename");
				fileSystem.Delete(text + "portal_1_foldername/key_b_filename");
				fileSystem.Delete(text + "portal_2_foldername/cedric_npcsheet_filename");
				fileSystem.Delete(text + "portal_2_foldername/cedric_facepic_filename");
				fileSystem.Delete(text + "portal_2_foldername/key_g_filename");
				fileSystem.Delete(text + "portal_3_foldername/rue_npcsheet_filename");
				fileSystem.Delete(text + "portal_3_foldername/rue_facepic_filename");
				fileSystem.Delete(text + "portal_3_foldername/key_r_filename");
				fileSystem.Delete(text + "big_portal_foldername/key_b_filename");
				fileSystem.Delete(text + "big_portal_foldername/key_g_filename");
				fileSystem.Delete(text + "big_portal_foldername/key_r_filename");
				break;
			}
			case "Script.put_key_in_box(1)":
			{
				string path3 = "/portals_foldername/portal_1_foldername/";
				TWMFile[] array4 = new TWMFile[3]
				{
					new TWMFile("photo", "prototype_npcsheet_filename", LaunchableWindowType.PHOTO, "blue_npc_prototype", "2"),
					new TWMFile("photo", "prototype_facepic_filename", LaunchableWindowType.PHOTO, "proto1", "2"),
					new TWMFile("doc", "key_b_filename", LaunchableWindowType.DOCUMENT, "keyB")
				};
				foreach (TWMFile tWMFile6 in array4)
				{
					tWMFile6.deleteRestricted = true;
					Game1.windowMan.FileSystem.WriteFile(path3, tWMFile6);
				}
				break;
			}
			case "Script.put_key_in_box(2)":
			{
				string path2 = "/portals_foldername/portal_2_foldername/";
				TWMFile[] array4 = new TWMFile[3]
				{
					new TWMFile("photo", "cedric_npcsheet_filename", LaunchableWindowType.PHOTO, "green_npc_cedric", "2"),
					new TWMFile("photo", "cedric_facepic_filename", LaunchableWindowType.PHOTO, "cedric", "2"),
					new TWMFile("doc", "key_g_filename", LaunchableWindowType.DOCUMENT, "keyG")
				};
				foreach (TWMFile tWMFile5 in array4)
				{
					tWMFile5.deleteRestricted = true;
					Game1.windowMan.FileSystem.WriteFile(path2, tWMFile5);
				}
				break;
			}
			case "Script.put_key_in_box(3)":
			{
				string path = "/portals_foldername/portal_3_foldername/";
				TWMFile[] array4 = new TWMFile[3]
				{
					new TWMFile("photo", "rue_npcsheet_filename", LaunchableWindowType.PHOTO, "red_rue", "2"),
					new TWMFile("photo", "rue_facepic_filename", LaunchableWindowType.PHOTO, "rue", "2"),
					new TWMFile("doc", "key_r_filename", LaunchableWindowType.DOCUMENT, "keyR")
				};
				foreach (TWMFile tWMFile4 in array4)
				{
					tWMFile4.deleteRestricted = true;
					Game1.windowMan.FileSystem.WriteFile(path, tWMFile4);
				}
				break;
			}
			case "Script.fadein_bgm(90,5)":
				Game1.soundMan.FadeInBGM(0.9f, 5f);
				break;
			case "Script.fadein_bgm(90,3)":
				Game1.soundMan.FadeInBGM(0.9f, 3f);
				break;
			case "particles :fireflies":
				oneshotWindow.tileMapMan.SpawnFireflyParticles();
				break;
			case "particles :fireflies\ngreen_ambient":
				GreenAmbient(oneshotWindow);
				oneshotWindow.tileMapMan.SpawnFireflyParticles();
				break;
			case "particles :fireflies\n#green_ambient":
				oneshotWindow.tileMapMan.SpawnFireflyParticles();
				break;
			case "ram_integrity_check":
				if (oneshotWindow.tileMapMan.RamPuzzleCheck())
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "green_ambient":
				GreenAmbient(oneshotWindow);
				break;
			case "ambient -100, -100, -100":
				oneshotWindow.tileMapMan.SetAmbientTone(new GameTone(-100, -100, -100));
				break;
			case "pixel_puzzle_check":
				if (oneshotWindow.tileMapMan.PixelPuzzleCheck(GLEN_PUZZLE_SOLUTION, new Vec2(31, 34), new Vec2(5, 6)))
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "puzzle_check(\"s1\")":
				if (oneshotWindow.tileMapMan.PixelPuzzleCheck(TOWER_PUZZLE_SOLUTION_1, new Vec2(5, 2), new Vec2(11, 11)))
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "puzzle_check(\"s3\")":
				if (oneshotWindow.tileMapMan.PixelPuzzleCheck(TOWER_PUZZLE_SOLUTION_3, new Vec2(5, 2), new Vec2(11, 11)))
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "puzzle_check(\"s4\")":
				if (oneshotWindow.tileMapMan.PixelPuzzleCheck(TOWER_PUZZLE_SOLUTION_4, new Vec2(5, 2), new Vec2(11, 11)))
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "puzzle_check(\"s5\")":
				if (oneshotWindow.tileMapMan.PixelPuzzleCheck(TOWER_PUZZLE_SOLUTION_5, new Vec2(5, 2), new Vec2(11, 12)))
				{
					oneshotWindow.flagMan.SetFlag(22);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(22);
				}
				break;
			case "pixel_puzzle_reset":
				oneshotWindow.tileMapMan.PixelPuzzleReset();
				break;
			case "Wallpaper.reset_persistent":
				Game1.windowMan.RestoreWallpaper();
				break;
			case "Script.move_player(64,46)":
				oneshotWindow.tileMapMan.GetPlayer().SetPosTile(new Vec2(64, 46));
				break;
			case "Script.move_player(35,15)":
				oneshotWindow.tileMapMan.GetPlayer().SetPosTile(new Vec2(35, 15));
				break;
			case "Script.move_player(31,7)":
				oneshotWindow.tileMapMan.GetPlayer().SetPosTile(new Vec2(31, 7));
				break;
			case "plight_start_timer":
				oneshotWindow.gameSaveMan.StartPlightTimer();
				break;
			case "plight_update_timer":
			{
				TimeSpan timeSpan = DateTime.Now - oneshotWindow.gameSaveMan.PlightTimeStart;
				oneshotWindow.varMan.SetVariable(22, (int)timeSpan.TotalMinutes);
				break;
			}
			case "pan_offset_y -64\nwrap_map":
				oneshotWindow.tileMapMan.EnableWrapping();
				break;
			case "film_puzzle_begin":
				oneshotWindow.filmPuzzleMan.PuzzleBegin();
				break;
			case "Graphics.fullscreen = false\nfilm_puzzle_begin":
				if (oneshotWindow.IsMaximized)
				{
					oneshotWindow.ToggleMaximize();
				}
				oneshotWindow.filmPuzzleMan.PuzzleBegin();
				break;
			case "Graphics.fullscreen = true":
				if (!oneshotWindow.IsMaximized)
				{
					oneshotWindow.ToggleMaximize();
				}
				break;
			case "Graphics.fullscreen = false":
				if (oneshotWindow.IsMaximized)
				{
					oneshotWindow.ToggleMaximize();
				}
				break;
			case "film_puzzle_end":
				oneshotWindow.filmPuzzleMan.PuzzleEnd();
				break;
			case "Interpreter.take_a_chill_pill":
				return true;
			case "Script.start_bruteforce":
				oneshotWindow.gameSaveMan.PlightBruteForceStartFrame = oneshotWindow.gameSaveMan.PlayTimeFrameCount;
				break;
			case "Script.bruteforce_vars":
			{
				if (!oneshotWindow.gameSaveMan.PlightBruteForceStartFrame.HasValue)
				{
					oneshotWindow.gameSaveMan.PlightBruteForceStartFrame = oneshotWindow.gameSaveMan.PlayTimeFrameCount;
				}
				int num10 = (int)(oneshotWindow.gameSaveMan.PlayTimeFrameCount - oneshotWindow.gameSaveMan.PlightBruteForceStartFrame.Value) / 120;
				int num11 = num10 % 10;
				int num12 = (num10 % 100 - num11) / 10;
				int num13 = (num10 % 1000 - num12 * 10 - num11) / 100;
				int num14 = (num10 % 10000 - num13 * 100 - num12 * 10 - num11) / 1000;
				int newVal = (num10 % 100000 - num14 * 1000 - num13 * 100 - num12 * 10 - num11) / 10000;
				oneshotWindow.varMan.SetVariable(26, newVal);
				oneshotWindow.varMan.SetVariable(27, num14);
				oneshotWindow.varMan.SetVariable(28, num13);
				oneshotWindow.varMan.SetVariable(29, num12);
				oneshotWindow.varMan.SetVariable(30, num11);
				break;
			}
			case "Script.lose_all_items":
				oneshotWindow.menuMan.ItemMan.LoseAllItems();
				break;
			case "Script.set_cam(0,0)":
				oneshotWindow.tileMapMan.SetCamPos(new Vec2(0, 0));
				break;
			case "Script.set_cam(64,0)":
				oneshotWindow.tileMapMan.SetCamPos(new Vec2(8, 0));
				break;
			case "Script.set_cam(48,0)":
				oneshotWindow.tileMapMan.SetCamPos(new Vec2(6, 0));
				break;
			case "bg 'black'":
				oneshotWindow.tileMapMan.SetBackground("black");
				break;
			case "bg 'summit'":
				oneshotWindow.tileMapMan.SetBackground("summit");
				break;
			case "pixel_puzzle_niko_correct(\"s5\")":
			{
				Vec2 currentTile4 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				currentTile4.X -= 5;
				currentTile4.Y -= 2;
				if (currentTile4.X < 0)
				{
					currentTile4.X = 0;
				}
				if (currentTile4.X > 10)
				{
					currentTile4.X = 10;
				}
				if (currentTile4.Y < 0)
				{
					currentTile4.Y = 0;
				}
				if (currentTile4.Y > 11)
				{
					currentTile4.Y = 11;
				}
				int num5 = currentTile4.X;
				int num6 = currentTile4.X;
				int num7 = currentTile4.Y;
				int num8 = currentTile4.Y;
				int num9 = 0;
				while (num9 < 20)
				{
					if (TOWER_PUZZLE_SOLUTION_5[currentTile4.Y * 11 + num5] != 0)
					{
						oneshotWindow.varMan.SetVariable(7, num5 + 5);
						oneshotWindow.varMan.SetVariable(8, currentTile4.Y + 2);
						break;
					}
					if (TOWER_PUZZLE_SOLUTION_5[currentTile4.Y * 11 + num6] != 0)
					{
						oneshotWindow.varMan.SetVariable(7, num6 + 5);
						oneshotWindow.varMan.SetVariable(8, currentTile4.Y + 2);
						break;
					}
					if (TOWER_PUZZLE_SOLUTION_5[num7 * 11 + currentTile4.X] != 0)
					{
						oneshotWindow.varMan.SetVariable(7, currentTile4.X + 5);
						oneshotWindow.varMan.SetVariable(8, num7 + 2);
						break;
					}
					if (TOWER_PUZZLE_SOLUTION_5[num8 * 11 + currentTile4.X] != 0)
					{
						oneshotWindow.varMan.SetVariable(7, currentTile4.X + 5);
						oneshotWindow.varMan.SetVariable(8, num8 + 2);
						break;
					}
					num9++;
					num5--;
					if (num5 < 0)
					{
						num5 = 0;
					}
					num6++;
					if (num6 > 10)
					{
						num6 = 10;
					}
					num7--;
					if (num7 < 0)
					{
						num7 = 0;
					}
					num8++;
					if (num8 > 11)
					{
						num8 = 11;
					}
				}
				break;
			}
			case "lightbulb_room_fix":
			{
				Vec2 currentTile3 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
				if (currentTile3.X < 5 || currentTile3.X > 15 || currentTile3.Y < 2 || currentTile3.Y > 13)
				{
					oneshotWindow.tileMapMan.GetPlayer().SetPosTile(new Vec2(10, 13));
				}
				break;
			}
			case "Oneshot.shake":
				oneshotWindow.windowShakeMan.Shake(60);
				break;
			case "Niko.do_your_thing":
				Game1.windowMan.DeployNiko(oneshotWindow.tileMapMan.GetMapID() == 255, oneshotWindow.flagMan.IsFlagSet(160));
				break;
			case "erase_game\nload_perma_flags":
				oneshotWindow.gameSaveMan.EraseSave();
				break;
			case "Script.password1":
			{
				for (int j = 1; j <= 4; j++)
				{
					TWMFile file2 = new TWMFile("photo", $"oneshot_password_image_{j}_filename", LaunchableWindowType.PHOTO, $"scenario1/pw{j}");
					Game1.windowMan.FileSystem.WriteFile("/docs_foldername/", file2);
				}
				break;
			}
			case "Script.password2":
			{
				for (int i = 1; i <= 4; i++)
				{
					TWMFile file = new TWMFile("photo", $"oneshot_password_image_{i}_filename", LaunchableWindowType.PHOTO, $"scenario2/pw{i}");
					Game1.windowMan.FileSystem.WriteFile("/docs_foldername/", file);
				}
				break;
			}
			case "kill_perma_flags":
				oneshotWindow.gameSaveMan.KillPermaFlags();
				break;
			case "$scene = Scene_Load.new":
				oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.DebugSaveMenu);
				oneshotWindow.menuMan.DebugSaveMenu.LoadSaves = true;
				break;
			case "open_debug_flag_menu":
				oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.DebugFlagMenu);
				break;
			case "open_var_flag_menu":
				oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.DebugVarMenu);
				break;
			case "plight_bruteforce_skip":
				oneshotWindow.gameSaveMan.PlightBruteForceStartFrame = oneshotWindow.gameSaveMan.PlayTimeFrameCount - 7561680;
				break;
			case "set_wallpaper_black":
				Game1.windowMan.Desktop.SetWallpaper("black");
				break;
			case "stop_speedrun_timer":
				Game1.windowMan.StopCurrentPlaythroughTimer();
				break;
			default:
				Game1.logMan.Log(LogManager.LogLevel.Error, "Unrecognized script command \"" + script + "\"");
				break;
			case "Screen.start":
			case "Screen.finish":
			case "Screen.set 'test1'":
			case "Screen.set 'test2'":
			case "Wallpaper.set 'test'":
			case "Wallpaper.reset":
			case "clamp_panorama":
			case "loadQASave(\"Save12_redsky_done\")":
			case "loadQASave \\\n(\"Save2_RightBeforeEnteringMines\")":
			case "loadQASave(\"Save2_tutorial_done\")":
			case "loadQASave(\"Save9_ReachedGlen\")":
			case "loadQASave(\"Save4_barrens_done\")":
			case "loadQASave(\"Save14_ReachedRefugeSky\")":
			case "loadQASave(\"Save9_glen_done\")":
			case "loadQASave(\"Save16_ReachedRedGround\")":
			case "add_light :window, \n'start_window', 2.0, 680, 370":
			case "add_light :bulb,\n'bulb_ground', 0.70, 304, 688":
			case "del_light :eyes":
			case "Window_Settings.load_settings":
			case "fake_save":
				break;
			}
			return false;
		}

		public static ScriptConditionalResult HandleScriptConditional(OneshotWindow oneshotWindow, string script, EventRunner caller, int pageId)
		{
			bool flag = false;
			if (script.StartsWith("unlock_map "))
			{
				UnlockMap(oneshotWindow, script, caller);
				return ScriptConditionalResult.No;
			}
			if (script.StartsWith("Script.px ") || script.StartsWith("Script.py "))
			{
				string[] array = script.Split(' ');
				int num = 0;
				num = ((!(array[0] == "Script.px")) ? oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y : oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().X);
				int num2 = int.Parse(array[2], CultureInfo.InvariantCulture);
				switch (array[1])
				{
				case "==":
					flag = num == num2;
					break;
				case ">":
					flag = num > num2;
					break;
				case "<":
					flag = num < num2;
					break;
				case ">=":
					flag = num >= num2;
					break;
				case "<=":
					flag = num <= num2;
					break;
				case "!=":
					flag = num != num2;
					break;
				}
			}
			else
			{
				if (script.StartsWith("EdText"))
				{
					HandleEdText(oneshotWindow, script, pageId);
					return ScriptConditionalResult.EdText;
				}
				switch (script)
				{
				case "$debug":
					flag = false;
					break;
				case "save_exists":
					flag = oneshotWindow.gameSaveMan.SaveExists();
					break;
				case "$persistent.langcode != 'zh_CN'":
					flag = true;
					break;
				case "$persistent.langcode == 'ja'":
					flag = false;
					break;
				case "$game_player.y == 17":
					flag = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile().Y == 17;
					break;
				case "Game_Oneshot.get_user_name == $game_oneshot.player_name":
					flag = oneshotWindow.gameSaveMan.GetPlayerName() == oneshotWindow.gameSaveMan.GetPlayerNameFromPC();
					break;
				case "Script.is_name_swear":
					flag = Game1.languageMan.GetTWMLocString("namecheck_is_swear_in_name").Split(' ').Any((string s) => oneshotWindow.gameSaveMan.GetPlayerName().ToLowerInvariant().Contains(s));
					break;
				case "Script.is_name_niko":
					flag = oneshotWindow.gameSaveMan.GetPlayerName().ToLowerInvariant() == Game1.languageMan.GetTWMLocString("namecheck_is_name_niko");
					break;
				case "Script.is_name_like_niko":
					flag = Game1.languageMan.GetTWMLocString("namecheck_is_name_like_niko").Split(' ').Contains(oneshotWindow.gameSaveMan.GetPlayerName().ToLowerInvariant());
					break;
				case "Script.is_name_like_mom_dad":
					flag = Game1.languageMan.GetTWMLocString("namecheck_is_name_like_mom_or_dad").Split(' ').Contains(oneshotWindow.gameSaveMan.GetPlayerName().ToLowerInvariant());
					break;
				case "Script.is_name_gross":
					flag = Game1.languageMan.GetTWMLocString("namecheck_is_name_gross").Split(' ').Contains(oneshotWindow.gameSaveMan.GetPlayerName().ToLowerInvariant());
					break;
				case "$console":
					flag = oneshotWindow.IsMaximized;
					break;
				case "Wallpaper.set_persistent('desktop', 0x1a041f)":
					flag = true;
					Game1.windowMan.OverrideWallpaper(new WallpaperInfo(new WallpaperInfoSaveData
					{
						imageFile = "desktop",
						displayName = "asdf",
						mode = WallpaperInfo.DisplayMode.CENTERED,
						bgColor = new GameColor(26, 4, 31, byte.MaxValue)
					}));
					break;
				case "Wallpaper.set_persistent('save_w32', 0x0)":
					flag = true;
					Game1.windowMan.OverrideWallpaper(new WallpaperInfo(new WallpaperInfoSaveData
					{
						imageFile = "save_w32",
						displayName = "asdf",
						mode = WallpaperInfo.DisplayMode.CENTERED,
						bgColor = GameColor.Black
					}));
					break;
				case "activate_balcony? 17":
				{
					Vec2 currentTile = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
					Entity.Direction direction = oneshotWindow.tileMapMan.GetPlayer().GetDirection();
					flag = oneshotWindow.tileMapMan.GetPlayer().PlayerHasControl() && currentTile.Y == 17 && direction == Entity.Direction.Up && Game1.inputMan.IsButtonPressed(InputManager.Button.OK);
					break;
				}
				case "Oneshot.obscured_cleared?":
					flag = oneshotWindow.filmPuzzleMan.HasPuzzleBeenCleared();
					break;
				case "button_pressed?":
					flag = Game1.inputMan.IsButtonPressed(InputManager.Button.OK) || Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel);
					break;
				case "Script.check_bruteforce":
					if (!oneshotWindow.gameSaveMan.PlightBruteForceStartFrame.HasValue)
					{
						oneshotWindow.gameSaveMan.PlightBruteForceStartFrame = oneshotWindow.gameSaveMan.PlayTimeFrameCount;
					}
					flag = oneshotWindow.gameSaveMan.PlayTimeFrameCount - oneshotWindow.gameSaveMan.PlightBruteForceStartFrame.Value > 7561680;
					break;
				case "Journal.active?":
					flag = Game1.windowMan.IsJournalOpen();
					break;
				case "fake_save_exists":
					flag = true;
					break;
				case "$game_temp.countdown_password.downcase == \"solstice\"":
					flag = !string.IsNullOrEmpty(caller.PasswordInput) && caller.PasswordInput.ToLowerInvariant() == "solstice";
					break;
				case "$game_temp.countdown_password.downcase == \"hello penguin\"":
					flag = !string.IsNullOrEmpty(caller.PasswordInput) && caller.PasswordInput.ToLowerInvariant() == "hello penguin";
					break;
				case "false":
					flag = false;
					break;
				case "Script.countdown_update == true":
				{
					bool flag2 = false;
					DateTime now = DateTime.Now;
					int num3 = now.Second % 10;
					int num4 = now.Second / 10;
					int num5 = now.Minute % 10;
					int num6 = now.Minute / 10;
					int num7 = now.Hour % 10;
					int num8 = now.Hour / 10;
					int num9 = now.DayOfYear % 10;
					int num10 = now.DayOfYear / 10 % 10;
					int num11 = now.DayOfYear / 100;
					if (oneshotWindow.varMan.GetVariable(101) != num3)
					{
						oneshotWindow.varMan.SetVariable(101, num3);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(102) != num4)
					{
						oneshotWindow.varMan.SetVariable(102, num4);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(103) != num5)
					{
						oneshotWindow.varMan.SetVariable(103, num5);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(104) != num6)
					{
						oneshotWindow.varMan.SetVariable(104, num6);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(105) != num7)
					{
						oneshotWindow.varMan.SetVariable(105, num7);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(106) != num8)
					{
						oneshotWindow.varMan.SetVariable(106, num8);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(107) != num9)
					{
						oneshotWindow.varMan.SetVariable(107, num9);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(108) != num10)
					{
						oneshotWindow.varMan.SetVariable(108, num10);
						flag2 = true;
					}
					if (oneshotWindow.varMan.GetVariable(109) != num11)
					{
						oneshotWindow.varMan.SetVariable(109, num11);
						flag2 = true;
					}
					flag = flag2;
					break;
				}
				case "Script.countdown_over == true":
					flag = true;
					break;
				case "Script.is_key_in_bigbox(1)":
				{
					string text6 = "/portals_foldername/big_portal_foldername/";
					flag = Game1.windowMan.FileSystem.FileExists(text6 + "key_b_filename");
					break;
				}
				case "Script.is_key_in_bigbox(2)":
				{
					string text5 = "/portals_foldername/big_portal_foldername/";
					flag = Game1.windowMan.FileSystem.FileExists(text5 + "key_g_filename");
					break;
				}
				case "Script.is_key_in_bigbox(3)":
				{
					string text4 = "/portals_foldername/big_portal_foldername/";
					flag = Game1.windowMan.FileSystem.FileExists(text4 + "key_r_filename");
					break;
				}
				case "Script.is_key_in_box(1)":
				{
					string text3 = "/portals_foldername/portal_1_foldername/";
					flag = Game1.windowMan.FileSystem.FileExists(text3 + "key_b_filename");
					break;
				}
				case "Script.is_key_in_box(2)":
				{
					string text2 = "/portals_foldername/portal_2_foldername/";
					flag = Game1.windowMan.FileSystem.FileExists(text2 + "key_g_filename");
					break;
				}
				case "Script.is_key_in_box(3)":
				{
					string text = "/portals_foldername/portal_3_foldername/";
					flag = Game1.windowMan.FileSystem.FileExists(text + "key_r_filename");
					break;
				}
				default:
					Game1.logMan.Log(LogManager.LogLevel.Error, "unimplemented script '" + script + "' in conditional branch!");
					break;
				}
			}
			if (!flag)
			{
				return ScriptConditionalResult.No;
			}
			return ScriptConditionalResult.Yes;
		}

		private static void HandleEdText(OneshotWindow oneshotWindow, string script, int pageId)
		{
			script = script.Substring("EdText.".Length);
			ModalWindow.ModalType modalType = ModalWindow.ModalType.Info;
			if (script.StartsWith("info("))
			{
				script = script.Substring("info(".Length);
				modalType = ModalWindow.ModalType.Info;
			}
			else if (script.StartsWith("err("))
			{
				script = script.Substring("err(".Length);
				modalType = ModalWindow.ModalType.Error;
			}
			else
			{
				if (!script.StartsWith("yesno("))
				{
					return;
				}
				script = script.Substring("yesno(".Length);
				modalType = ModalWindow.ModalType.YesNo;
			}
			string value = script.Substring(0, script.IndexOf(')'));
			value = JsonConvert.DeserializeObject<string>(value);
			value = value.Replace("\n", " ");
			while (value.Contains("  "))
			{
				value = value.Replace("  ", " ");
			}
			value = value.Trim();
			value = Game1.languageMan.GetMapLocString(pageId, value);
			value = value.Replace("\\p", oneshotWindow.gameSaveMan.GetPlayerName());
			oneshotWindow.ShowModalWindow(modalType, value, null, playModalNoise: true, canAutomash: true);
		}

		private static void HouseAmbient(OneshotWindow oneshotWindow)
		{
			oneshotWindow.tileMapMan.SetAmbientTone(new GameTone(-8, -8, -8));
		}

		private static void BlueAmbient(OneshotWindow oneshotWindow)
		{
			oneshotWindow.tileMapMan.SetAmbientTone(new GameTone(-40, -40, -40));
		}

		private static void GreenAmbient(OneshotWindow oneshotWindow)
		{
			oneshotWindow.tileMapMan.SetAmbientTone(new GameTone(-30, -20, -30));
		}

		private static void UnlockMap(OneshotWindow oneshotWindow, string script, EventRunner caller)
		{
			script = script.Substring("unlock_map ".Length);
			string[] array = script.Split(',');
			FastTravelManager.FastTravelLocation fastTravelLocation = new FastTravelManager.FastTravelLocation();
			switch (array[0].Trim().ToLowerInvariant())
			{
			case ":blue":
				fastTravelLocation.zone = FastTravelManager.FastTravelZone.Blue;
				break;
			case ":green":
				fastTravelLocation.zone = FastTravelManager.FastTravelZone.Green;
				break;
			case ":red":
				fastTravelLocation.zone = FastTravelManager.FastTravelZone.Red;
				break;
			case ":red_ground":
				fastTravelLocation.zone = FastTravelManager.FastTravelZone.RedGround;
				break;
			}
			fastTravelLocation.mapId = oneshotWindow.tileMapMan.GetMapID();
			fastTravelLocation.tilePos = ((caller.TriggeringEntity != null) ? caller.TriggeringEntity.GetCurrentTile() : Vec2.Zero);
			switch (array[2].Trim().ToLowerInvariant())
			{
			case ":down":
				fastTravelLocation.direction = Entity.Direction.Down;
				break;
			case ":up":
				fastTravelLocation.direction = Entity.Direction.Up;
				break;
			case ":left":
				fastTravelLocation.direction = Entity.Direction.Left;
				break;
			case ":right":
				fastTravelLocation.direction = Entity.Direction.Right;
				break;
			}
			oneshotWindow.fastTravelMan.UnlockFastTravelLocation(fastTravelLocation);
		}

		static ScriptParser()
		{
			byte[] array = new byte[121];
			array[60] = 1;
			TOWER_PUZZLE_SOLUTION_4 = array;
			TOWER_PUZZLE_SOLUTION_5 = new byte[132]
			{
				0, 0, 1, 1, 1, 1, 1, 1, 1, 0,
				0, 0, 1, 1, 0, 0, 0, 0, 0, 1,
				1, 0, 1, 1, 0, 1, 1, 1, 1, 1,
				0, 1, 1, 1, 0, 1, 1, 0, 0, 0,
				1, 1, 0, 1, 1, 0, 1, 0, 0, 0,
				0, 0, 1, 0, 1, 1, 0, 1, 0, 0,
				0, 0, 0, 1, 0, 1, 1, 0, 1, 0,
				0, 0, 0, 0, 1, 0, 1, 1, 0, 1,
				1, 0, 0, 0, 1, 1, 0, 1, 1, 1,
				0, 1, 1, 1, 1, 1, 0, 1, 1, 0,
				1, 1, 0, 0, 0, 0, 0, 1, 1, 0,
				0, 0, 1, 1, 1, 1, 1, 1, 1, 0,
				0, 0, 0, 0, 0, 1, 1, 1, 0, 0,
				0, 0
			};
		}
	}
}

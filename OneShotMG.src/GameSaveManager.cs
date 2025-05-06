using System;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.Menus;
using OneShotMG.src.TWM;
using OneShotMG.src.TWM.Filesystem;

namespace OneShotMG.src
{
	public class GameSaveManager
	{
		private readonly OneshotWindow oneshotWindow;

		private static string _saveFolder;

		public const string DEBUG_SAVE_FOLDER_NAME = "OneShotWME/debug_saves";

		private const string saveName = "save.dat";

		private const string permaSaveName = "p-settings.dat";

		private const string SettingsFileName = "settings.conf";

		public const string TWMSaveFileName = "fakesave_filename";

		private string playerName = string.Empty;

		public long PlayTimeFrameCount;

		public long? PlightBruteForceStartFrame;

		private DateTime? plightTimeStart;

		public const int PERMA_FLAGS_START_FLAG = 151;

		private const int PERMA_VARS_START_VAR = 76;

		private static string saveFolder
		{
			get
			{
				if (_saveFolder == null)
				{
					InitializeSaveFolder();
				}
				return _saveFolder;
			}
		}

		public DateTime PlightTimeStart
		{
			get
			{
				if (!plightTimeStart.HasValue)
				{
					StartPlightTimer();
				}
				return plightTimeStart.Value;
			}
		}

		public GameSaveManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			InitializeSaveFolder();
			playerName = GetPlayerNameFromPC();
		}

		private static void InitializeSaveFolder()
		{
			_saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Game1.SaveFolderName);
			Directory.CreateDirectory(_saveFolder);
		}

		public string GetPlayerName()
		{
			return playerName;
		}

		public string GetPlayerNameFromPC()
		{
			string result = Environment.UserName;
			if (Game1.steamMan.IsTimedDemo)
			{
				result = string.Empty;
			}
			else if (!string.IsNullOrEmpty(Game1.steamMan.SteamUserName))
			{
				result = Game1.steamMan.SteamUserName;
			}
			return result;
		}

		public void SetPlayerName(string newName)
		{
			playerName = newName;
		}

		public void StartPlightTimer()
		{
			plightTimeStart = DateTime.Now;
		}

		public void MakeSave()
		{
			WriteSaveFile("save.dat");
			WritePermaSave();
			SaveSettings();
			if (!Game1.windowMan.FileSystem.FileExists("/docs_foldername/mygames_foldername/oneshot_foldername/fakesave_filename"))
			{
				TWMFile tWMFile = new TWMFile("save", "fakesave_filename", LaunchableWindowType.DOCUMENT, "save_progress_text");
				tWMFile.moveRestricted = true;
				Game1.windowMan.FileSystem.WriteFile("/docs_foldername/mygames_foldername/oneshot_foldername/", tWMFile);
			}
			Game1.windowMan.SaveDesktopAndFileSystem();
			Game1.logMan.Log(LogManager.LogLevel.Info, "Game Saved!");
		}

		public void EraseSave()
		{
			string path = Path.Combine(saveFolder, "save.dat");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			Game1.windowMan.FileSystem.Delete("/docs_foldername/mygames_foldername/oneshot_foldername/app_journal");
		}

		public void ClearCorruptSave()
		{
			Game1.masterSaveMan.DeleteCorruptFile("save.dat");
			Game1.masterSaveMan.DeleteCorruptFile("p-settings.dat");
		}

		public void LoadSettings()
		{
			SettingsMenu.SettingsSaveData data = Game1.masterSaveMan.LoadFile("settings.conf", LoadSettingsFromString, VerifySettingsSaveData, new SettingsMenu.SettingsSaveData());
			oneshotWindow.menuMan.SettingsMenu.LoadSettingsSaveData(data);
		}

		private SettingsMenu.SettingsSaveData LoadSettingsFromString(string data)
		{
			return JsonConvert.DeserializeObject<SettingsMenu.SettingsSaveData>(data);
		}

		private bool VerifySettingsSaveData(SettingsMenu.SettingsSaveData settingsSave)
		{
			if (settingsSave == null)
			{
				return false;
			}
			return true;
		}

		public void SaveSettings()
		{
			SettingsMenu.SettingsSaveData settingsSaveData = oneshotWindow.menuMan.SettingsMenu.GetSettingsSaveData();
			Game1.masterSaveMan.WriteFile(new SaveRequest("settings.conf", JsonConvert.SerializeObject(settingsSaveData)));
		}

		public static void DeleteAllOneshotData()
		{
			File.Delete(Path.Combine(saveFolder, "settings.conf"));
			File.Delete(Path.Combine(saveFolder, "save.dat"));
			File.Delete(Path.Combine(saveFolder, "p-settings.dat"));
		}

		public static PermaSaveData LoadPermaSaveData()
		{
			return Game1.masterSaveMan.LoadFile("p-settings.dat", LoadPermaSaveDataFromString, ValidatePermaSaveData, null);
		}

		private static PermaSaveData LoadPermaSaveDataFromString(string data)
		{
			return JsonConvert.DeserializeObject<PermaSaveData>(data);
		}

		private static bool ValidatePermaSaveData(PermaSaveData permaSave)
		{
			if (permaSave == null)
			{
				return false;
			}
			if (permaSave.permaFlagData == null)
			{
				return false;
			}
			if (permaSave.permaVarData == null)
			{
				return false;
			}
			return true;
		}

		public bool LoadPermaSave()
		{
			PermaSaveData permaSaveData = LoadPermaSaveData();
			if (permaSaveData == null)
			{
				return false;
			}
			FlagManager flagManager = new FlagManager(oneshotWindow, permaFlags: true);
			VariableManager variableManager = new VariableManager(permaVars: true);
			flagManager.SetRawFlagData(permaSaveData.permaFlagData);
			variableManager.SetRawVarData(permaSaveData.permaVarData);
			for (int i = 0; i < 25; i++)
			{
				oneshotWindow.varMan.SetVariable(i + 76, variableManager.GetVariable(i));
				if (flagManager.IsFlagSet(i))
				{
					oneshotWindow.flagMan.SetFlag(i + 151);
				}
				else
				{
					oneshotWindow.flagMan.UnsetFlag(i + 151);
				}
			}
			playerName = permaSaveData.playerName;
			return true;
		}

		private void WritePermaSave()
		{
			FlagManager flagManager = new FlagManager(oneshotWindow, permaFlags: true);
			VariableManager variableManager = new VariableManager(permaVars: true);
			for (int i = 0; i < 25; i++)
			{
				variableManager.SetVariable(i, oneshotWindow.varMan.GetVariable(i + 76));
				if (oneshotWindow.flagMan.IsFlagSet(i + 151))
				{
					flagManager.SetFlag(i);
				}
				else
				{
					flagManager.UnsetFlag(i);
				}
			}
			PermaSaveData value = new PermaSaveData
			{
				permaFlagData = flagManager.GetRawFlagData(),
				permaVarData = variableManager.GetRawVarData(),
				playerName = playerName
			};
			Game1.masterSaveMan.WriteFile(new SaveRequest("p-settings.dat", JsonConvert.SerializeObject(value)));
		}

		private void WriteSaveFile(string fileName)
		{
			SaveData value = new SaveData
			{
				version = 1,
				flagData = oneshotWindow.flagMan.GetRawFlagData(),
				varData = oneshotWindow.varMan.GetRawVarData(),
				pictureData = oneshotWindow.pictureMan.GetPictureSaveDatas(),
				plightTimeStart = plightTimeStart,
				playTimeFrameCount = PlayTimeFrameCount,
				plightBruteForceStartFrame = PlightBruteForceStartFrame,
				currentOverridingWallpaper = (Game1.windowMan.Desktop.IsWallpaperOverridden() ? Game1.windowMan.Desktop.wallpaper.GetWallpaperInfoSaveData() : null),
				soundSaveData = Game1.soundMan.GetSoundSaveData(),
				inventoryData = oneshotWindow.menuMan.ItemMan.GetRawInventoryData(),
				selfSwitchData = oneshotWindow.selfSwitchMan.GetSelfSwitchesData(),
				fastTravelData = oneshotWindow.fastTravelMan.GetSaveData(),
				tileMapSaveData = oneshotWindow.tileMapMan.GetSaveData(),
				followers = oneshotWindow.followerMan.GetFollowerSaveData()
			};
			Game1.masterSaveMan.WriteFile(new SaveRequest(fileName, JsonConvert.SerializeObject(value)));
		}

		private string makeDebugSaveName(int slot)
		{
			return $"debug_save{slot}.dat";
		}

		public void WriteDebugSave(int slot)
		{
			WriteSaveFile(makeDebugSaveName(slot));
			Game1.windowMan.SaveDesktopAndFileSystem();
		}

		public SaveData LoadDebugSaveData(int slot)
		{
			return LoadSaveData(makeDebugSaveName(slot));
		}

		private static SaveData LoadSaveData(string fileName)
		{
			return Game1.masterSaveMan.LoadFile(fileName, LoadSaveDataFromString, VerifySaveData, null);
		}

		private static SaveData LoadSaveDataFromString(string data)
		{
			return JsonConvert.DeserializeObject<SaveData>(data);
		}

		private static bool VerifySaveData(SaveData saveData)
		{
			if (saveData == null)
			{
				return false;
			}
			if (saveData.fastTravelData == null)
			{
				return false;
			}
			if (saveData.flagData == null)
			{
				return false;
			}
			if (saveData.followers == null)
			{
				return false;
			}
			if (saveData.inventoryData == null)
			{
				return false;
			}
			if (saveData.pictureData == null)
			{
				return false;
			}
			if (saveData.selfSwitchData == null)
			{
				return false;
			}
			if (saveData.soundSaveData == null)
			{
				return false;
			}
			if (saveData.tileMapSaveData == null)
			{
				return false;
			}
			if (saveData.varData == null)
			{
				return false;
			}
			return true;
		}

		public void LoadDebugSave(int slot)
		{
			LoadAndApplySaveFile(makeDebugSaveName(slot), debugSave: true);
		}

		private bool LoadAndApplySaveFile(string fileName, bool debugSave = false)
		{
			SaveData saveData = LoadSaveData(fileName);
			if (saveData == null)
			{
				return false;
			}
			oneshotWindow.flagMan.SetRawFlagData(saveData.flagData);
			oneshotWindow.varMan.SetRawVarData(saveData.varData);
			oneshotWindow.selfSwitchMan.LoadSelfSwitchesData(saveData.selfSwitchData);
			oneshotWindow.fastTravelMan.LoadSaveData(saveData.fastTravelData);
			oneshotWindow.pictureMan.LoadPictureSaveDatas(saveData.pictureData);
			plightTimeStart = saveData.plightTimeStart;
			PlightBruteForceStartFrame = saveData.plightBruteForceStartFrame;
			PlayTimeFrameCount = saveData.playTimeFrameCount;
			oneshotWindow.menuMan.ItemMan.SetRawInventoryData(saveData.inventoryData);
			oneshotWindow.followerMan.LoadFollowersSaveData(saveData.followers);
			oneshotWindow.tileMapMan.LoadSaveData(saveData.tileMapSaveData, saveData.version);
			if (saveData.currentOverridingWallpaper != null)
			{
				Game1.windowMan.Desktop.OverrideWallpaper(new WallpaperInfo(saveData.currentOverridingWallpaper));
			}
			if (!oneshotWindow.flagMan.IsFlagSet(181) && !oneshotWindow.flagMan.IsFlagSet(183) && !oneshotWindow.flagMan.IsFlagSet(186) && !oneshotWindow.flagMan.IsFlagSet(188) && !oneshotWindow.flagMan.IsFlagSet(190))
			{
				Game1.soundMan.LoadSoundSaveData(saveData.soundSaveData);
			}
			if (!oneshotWindow.flagMan.IsFlagSet(198) && !debugSave)
			{
				oneshotWindow.tileMapMan.StartCommonEvent(42);
			}
			oneshotWindow.fullscreenBorderMan.ShiftColorInstantly();
			Game1.windowMan.Desktop.inSolstice = oneshotWindow.flagMan.IsSolticeGlitchTime();
			Game1.logMan.Log(LogManager.LogLevel.Info, "Save Loaded!");
			return true;
		}

		public bool LoadSave()
		{
			if (!SaveExists())
			{
				return true;
			}
			if (LoadAndApplySaveFile("save.dat"))
			{
				return LoadPermaSave();
			}
			return false;
		}

		public static SaveData LoadSaveData()
		{
			return LoadSaveData("save.dat");
		}

		public bool SaveExists()
		{
			if (!Game1.windowMan.FileSystem.FileExists("/docs_foldername/mygames_foldername/oneshot_foldername/fakesave_filename"))
			{
				EraseSave();
			}
			return File.Exists(Path.Combine(saveFolder, "save.dat"));
		}

		public void KillPermaFlags()
		{
			for (int i = 0; i < 25; i++)
			{
				oneshotWindow.varMan.SetVariable(i + 76, 0);
				oneshotWindow.flagMan.UnsetFlag(i + 151);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;

namespace OneShotMG.src.TWM.Filesystem
{
	public class FilesystemSaveManager
	{
		public class FilesystemSaveData
		{
			public List<TWMFile> files;

			public List<string> folders;
		}

		public class DesktopSaveData
		{
			public Dictionary<string, Vec2> iconPositions;

			public string currentWallpaper;

			public string currentTheme;

			public string currentResolution;

			public List<string> unlockedWallpapers;

			public List<string> unlockedThemes;

			public List<string> unlockedMusicTracks;

			public List<string> unlockedCgs;

			public List<string> unlockedAchievements;

			public List<string> unlockedProfiles;

			public bool tutorialCompleted;

			public int Gamma = 100;

			public int BGMVolume = 100;

			public int SFXVolume = 100;

			public bool unlockedFirstWallpaper;

			public bool unlockedFirstProfile;

			public bool unlockedFirstTheme;

			public bool inSolstice;

			public bool isSystemScalingSmooth = true;

			public bool isVsyncEnabled;

			public bool isBadgeToastsEnabled = true;

			public bool isFullscreen;

			public Vec2 DrawResolution = Vec2.Zero;

			public Vec2 DrawResolutionSteamDeck = Vec2.Zero;

			public void SetDrawResolution(Vec2 res)
			{
				if (Game1.steamMan.IsOnSteamDeck)
				{
					DrawResolutionSteamDeck = res;
				}
				else
				{
					DrawResolution = res;
				}
			}

			public Vec2 GetDrawResolution()
			{
				if (Game1.steamMan.IsOnSteamDeck)
				{
					if (DrawResolutionSteamDeck.X > 0 && DrawResolutionSteamDeck.Y > 0)
					{
						return DrawResolutionSteamDeck;
					}
					return GraphicsManager.RES_800P;
				}
				if (DrawResolution.X > 0 && DrawResolution.Y > 0)
				{
					return DrawResolution;
				}
				if (!string.IsNullOrEmpty(currentResolution))
				{
					return Game1.gMan.GetDrawResolutionFromString(currentResolution);
				}
				return GraphicsManager.RES_720P;
			}
		}

		private readonly string saveFolder;

		private const string FilesystemFileName = "fs.dat";

		private const string DesktopFileName = "desktop.dat";

		private FilesystemSaveData fsSaveDataToWrite;

		private DesktopSaveData dsSaveDataToWrite;

		public FilesystemSaveManager()
		{
			saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Game1.SaveFolderName);
		}

		public bool FsSaveExists()
		{
			return File.Exists(Path.Combine(saveFolder, "fs.dat"));
		}

		public bool DesktopSaveExists()
		{
			return File.Exists(Path.Combine(saveFolder, "desktop.dat"));
		}

		public bool LoadFsSave(out FilesystemSaveData data)
		{
			data = Game1.masterSaveMan.LoadFile("fs.dat", LoadFsSaveFromString, VerifyFsSave, null);
			return data != null;
		}

		private FilesystemSaveData LoadFsSaveFromString(string data)
		{
			return JsonConvert.DeserializeObject<FilesystemSaveData>(data);
		}

		private bool VerifyFsSave(FilesystemSaveData filesystemSave)
		{
			if (filesystemSave == null)
			{
				return false;
			}
			if (filesystemSave.files == null)
			{
				return false;
			}
			if (filesystemSave.folders == null)
			{
				return false;
			}
			return true;
		}

		public void DeleteFSAndDesktopSave()
		{
			File.Delete(Path.Combine(saveFolder, "desktop.dat"));
			File.Delete(Path.Combine(saveFolder, "fs.dat"));
		}

		public void WriteFsSave(FilesystemSaveData data)
		{
			fsSaveDataToWrite = data;
		}

		public bool LoadDesktopSave(out DesktopSaveData data)
		{
			data = Game1.masterSaveMan.LoadFile("desktop.dat", LoadDesktopSaveFromString, VerifyDesktopSave, null);
			return data != null;
		}

		private DesktopSaveData LoadDesktopSaveFromString(string data)
		{
			return JsonConvert.DeserializeObject<DesktopSaveData>(data);
		}

		private bool VerifyDesktopSave(DesktopSaveData desktopSave)
		{
			if (desktopSave == null)
			{
				return false;
			}
			if (desktopSave.BGMVolume < 0 || desktopSave.BGMVolume > 100)
			{
				return false;
			}
			if (desktopSave.SFXVolume < 0 || desktopSave.SFXVolume > 100)
			{
				return false;
			}
			if (string.IsNullOrEmpty(desktopSave.currentTheme))
			{
				return false;
			}
			if (string.IsNullOrEmpty(desktopSave.currentWallpaper))
			{
				return false;
			}
			if (desktopSave.Gamma < 50 || desktopSave.Gamma > 150)
			{
				return false;
			}
			if (desktopSave.iconPositions == null)
			{
				return false;
			}
			if (desktopSave.unlockedAchievements == null)
			{
				return false;
			}
			if (desktopSave.unlockedCgs == null)
			{
				return false;
			}
			if (desktopSave.unlockedMusicTracks == null)
			{
				return false;
			}
			if (desktopSave.unlockedProfiles == null)
			{
				return false;
			}
			if (desktopSave.unlockedThemes == null)
			{
				return false;
			}
			if (desktopSave.unlockedWallpapers == null)
			{
				return false;
			}
			return true;
		}

		public void WriteDesktopSave(DesktopSaveData data)
		{
			dsSaveDataToWrite = data;
		}

		public void Update()
		{
			if (fsSaveDataToWrite != null)
			{
				Game1.masterSaveMan.WriteFile(new SaveRequest("fs.dat", JsonConvert.SerializeObject(fsSaveDataToWrite)));
				fsSaveDataToWrite = null;
			}
			if (dsSaveDataToWrite != null)
			{
				Game1.masterSaveMan.WriteFile(new SaveRequest("desktop.dat", JsonConvert.SerializeObject(dsSaveDataToWrite)));
				dsSaveDataToWrite = null;
			}
		}

		public void DeleteCorruptFSAndDesktopSaves()
		{
			Game1.masterSaveMan.DeleteCorruptFile("fs.dat");
			Game1.masterSaveMan.DeleteCorruptFile("desktop.dat");
		}
	}
}

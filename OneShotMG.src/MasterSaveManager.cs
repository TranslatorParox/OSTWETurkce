using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OneShotMG.src
{
	public class MasterSaveManager
	{
		public delegate T LoadObjectFromString<T>(string data);

		public delegate bool VerifyData<T>(T data);

		private SpinningBulbIcon lightbulb;

		private const int RIGHT_OFFSET = 20;

		private const int BOTTOM_OFFSET = 20;

		private byte bulbAlpha;

		private int bulbFadeTime;

		private const int BULB_ALIVE_TIME = 90;

		private const int BULB_TOTAL_FADE_TIME = 60;

		private List<SaveRequest> saveRequests;

		private Task saveThread;

		private bool isWritingFile;

		private readonly object writingFileLock = new object();

		private string saveFolder;

		private string backupFolder;

		private const string BACKUP_FOLDER_NAME = "backups";

		private const int NUMBER_OF_BACKUPS = 2;

		public MasterSaveManager()
		{
			lightbulb = new SpinningBulbIcon();
			saveRequests = new List<SaveRequest>();
			saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Game1.SaveFolderName);
			Directory.CreateDirectory(saveFolder);
			backupFolder = Path.Combine(saveFolder, "backups");
			Directory.CreateDirectory(backupFolder);
			saveThread = new Task(async delegate
			{
				while (true)
				{
					await Task.Delay(100);
					lock (saveRequests)
					{
						if (saveRequests.Count > 0)
						{
							lock (writingFileLock)
							{
								isWritingFile = true;
							}
							while (saveRequests.Count > 0)
							{
								SaveRequest saveRequest = saveRequests[0];
								writeSaveRequestAndBackup(saveRequest);
								saveRequests.RemoveAt(0);
							}
							lock (writingFileLock)
							{
								isWritingFile = false;
							}
						}
					}
				}
			});
			saveThread.Start();
		}

		public void DeleteCorruptFile(string fileName)
		{
			string text = Path.Combine(saveFolder, fileName);
			if (File.Exists(text))
			{
				string corruptFilename = getCorruptFilename(fileName);
				string text2 = Path.Combine(saveFolder, corruptFilename);
				File.Delete(text2);
				File.Move(text, text2);
			}
		}

		private void writeSaveRequestAndBackup(SaveRequest saveRequest)
		{
			File.WriteAllText(Path.Combine(saveFolder, saveRequest.fileName), saveRequest.data);
			if (Game1.steamMan.IsTimedDemo || !saveRequest.backupsEnabled)
			{
				return;
			}
			string path = Path.Combine(backupFolder, getFileBackupName(saveRequest.fileName, 2));
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			for (int num = 1; num >= 1; num--)
			{
				string text = Path.Combine(backupFolder, getFileBackupName(saveRequest.fileName, num));
				string destFileName = Path.Combine(backupFolder, getFileBackupName(saveRequest.fileName, num + 1));
				if (File.Exists(text))
				{
					File.Move(text, destFileName);
				}
			}
			File.WriteAllText(Path.Combine(backupFolder, getFileBackupName(saveRequest.fileName, 1)), saveRequest.data);
		}

		private string getCorruptFilename(string fileName)
		{
			string[] array = fileName.Split('.');
			if (array.Length == 2)
			{
				return array[0] + "_corrupt.dat";
			}
			return fileName + "_corrupt.dat";
		}

		private string getFileBackupName(string fileName, int slot)
		{
			string[] array = fileName.Split('.');
			if (array.Length == 2)
			{
				return array[0] + "_backup_" + slot + "." + array[1];
			}
			return fileName + slot;
		}

		public bool IsWritingFile()
		{
			lock (writingFileLock)
			{
				return isWritingFile;
			}
		}

		public void WriteFile(SaveRequest saveRequest)
		{
			lock (saveRequests)
			{
				lock (writingFileLock)
				{
					isWritingFile = true;
				}
				saveRequests.Add(saveRequest);
			}
			bulbFadeTime = 90;
		}

		public T LoadFile<T>(string fileName, LoadObjectFromString<T> loadObjectFromString, VerifyData<T> verifyData, T defaultObject) where T : class
		{
			string text = Path.Combine(saveFolder, fileName);
			if (!File.Exists(text))
			{
				return defaultObject;
			}
			T val = tryLoadFile(text, loadObjectFromString, verifyData);
			if (val == null)
			{
				string message = "error loading " + fileName + ", attempting to load a backup.";
				if (Game1.languageMan != null)
				{
					message = string.Format(Game1.languageMan.GetTWMLocString("file_load_fail"), fileName);
				}
				Game1.logMan.Log(LogManager.LogLevel.Error, message);
				DeleteCorruptFile(fileName);
				for (int i = 1; i <= 2; i++)
				{
					string fileBackupName = getFileBackupName(fileName, i);
					string text2 = Path.Combine(backupFolder, fileBackupName);
					if (!File.Exists(text2))
					{
						message = $"backup {i} not found for {fileName}";
						if (Game1.languageMan != null)
						{
							message = string.Format(Game1.languageMan.GetTWMLocString("backup_not_found"), i, fileName);
						}
						Game1.logMan.Log(LogManager.LogLevel.Error, message);
						continue;
					}
					val = tryLoadFile(text2, loadObjectFromString, verifyData);
					if (val == null)
					{
						message = $"error loading backup {i} for {fileName}.";
						if (Game1.languageMan != null)
						{
							message = string.Format(Game1.languageMan.GetTWMLocString("backup_load_failed"), i, fileName);
						}
						Game1.logMan.Log(LogManager.LogLevel.Error, message);
						continue;
					}
					File.Move(text2, text);
					break;
				}
				if (val == null)
				{
					message = "error loading backups for " + fileName + ", using default instead.";
					if (Game1.languageMan != null)
					{
						message = string.Format(Game1.languageMan.GetTWMLocString("all_backups_failed"), fileName);
					}
					Game1.logMan.Log(LogManager.LogLevel.Error, message);
					val = defaultObject;
				}
			}
			return val;
		}

		private T tryLoadFile<T>(string filePath, LoadObjectFromString<T> loadObjectFromString, VerifyData<T> verifyData) where T : class
		{
			try
			{
				string data = File.ReadAllText(filePath);
				T val = loadObjectFromString(data);
				if (val == null)
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "there was an error parsing " + filePath + " on load.");
					return null;
				}
				if (!verifyData(val))
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "there was an error verifying " + filePath + " on load.");
					return null;
				}
				return val;
			}
			catch (Exception ex)
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, "there was an exception when loading " + filePath + " - " + ex.Message);
				Game1.logMan.Log(LogManager.LogLevel.Warning, ex.StackTrace);
			}
			return null;
		}

		public void Update()
		{
			lightbulb.Update();
			if (IsWritingFile())
			{
				bulbFadeTime = 90;
			}
			else if (bulbFadeTime > 0)
			{
				bulbFadeTime--;
			}
			else
			{
				bulbFadeTime = 0;
			}
			int num = Math.Min(bulbFadeTime, 60);
			bulbAlpha = (byte)(num * 255 / 60);
		}

		public void Draw()
		{
			if (bulbAlpha > 0)
			{
				Vec2 drawPos = new Vec2(Game1.gMan.DrawScreenSize.X / 2 - 48 - 20, Game1.gMan.DrawScreenSize.Y / 2 - 48 - 20);
				lightbulb.Draw(drawPos, bulbAlpha);
			}
		}
	}
}

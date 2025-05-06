using System;
using System.Collections.Generic;
using System.Linq;

namespace OneShotMG.src.TWM.Filesystem
{
	public class TWMFilesystem
	{
		public delegate void FsNotify(TWMFilesystem fs, string path);

		public const string FOLDER_DESKTOP = "/";

		public const string FOLDER_MYDOCUMENTS = "/docs_foldername/";

		public const string FOLDER_MYGAMES = "/docs_foldername/mygames_foldername/";

		public const string FOLDER_MYGAMESONESHOT = "/docs_foldername/mygames_foldername/oneshot_foldername/";

		public const string FOLDER_HELP = "/help_foldername/";

		public const string FOLDER_PORTALS = "/portals_foldername/";

		public const string FOLDER_WALLPAPERS = "/wallpapers_foldername/";

		public const string FOLDER_THEMES = "/themes_foldername/";

		public const string FOLDER_MOVEFILE_FOLDER1 = "/movefile_tutorial_folder1_name/";

		public const string FOLDER_MOVEFILE_FOLDER2 = "/movefile_tutorial_folder2_name/";

		public const string TUTORIAL_DUMMY_FILE_NAME = "tutorial_dummyfile_filename";

		public static readonly char[] separator = new char[1] { '/' };

		public FsNotify NotifyWriteOrDelete;

		public FsNotify NotifyMove;

		private Dictionary<string, TWMFolder> directories = new Dictionary<string, TWMFolder>();

		public static string ParentPath(string path)
		{
			string text = path.Substring(0, path.Length - 1);
			return text.Substring(0, text.LastIndexOf('/') + 1);
		}

		public static string FileNameFromPath(string path)
		{
			return path.Substring(path.LastIndexOf('/') + 1);
		}

		public TWMFolder GetDir(string path)
		{
			directories.TryGetValue(path, out var value);
			return value;
		}

		public bool FileExists(string path)
		{
			string key = ParentPath(path);
			if (directories.TryGetValue(key, out var value))
			{
				string fileName = FileNameFromPath(path);
				return value.contents.Any((TWMFileNode fn) => fn.name == fileName);
			}
			return false;
		}

		public bool IsPathMoveRestricted(string path)
		{
			if (!(path == "/wallpapers_foldername/"))
			{
				return path == "/themes_foldername/";
			}
			return true;
		}

		public void Delete(string path)
		{
			if (directories.TryGetValue(path, out var value))
			{
				directories.Remove(path);
			}
			else
			{
				value = null;
			}
			string text = ParentPath(path);
			if (directories.TryGetValue(text, out var value2))
			{
				string fileName = ((value != null) ? value.name : FileNameFromPath(path));
				TWMFileNode tWMFileNode = value2.contents.FirstOrDefault((TWMFileNode fn) => fn.name == fileName);
				if (tWMFileNode != null)
				{
					value2.contents.Remove(tWMFileNode);
					NotifyWriteOrDelete?.Invoke(this, text);
				}
			}
		}

		public void Delete(TWMFileNode node)
		{
			string parentPath = node.parentPath;
			if (directories.TryGetValue(parentPath, out var value))
			{
				value.contents.Remove(node);
				NotifyWriteOrDelete?.Invoke(this, parentPath);
			}
		}

		public void MoveFile(TWMFile file, string to)
		{
			string parentPath = file.parentPath;
			directories.TryGetValue(parentPath, out var value);
			directories.TryGetValue(to, out var value2);
			if (value != null && value2 != null)
			{
				value.contents.Remove(file);
				value2.contents.Add(file);
				file.parentPath = to;
				NotifyMove?.Invoke(this, parentPath);
				NotifyMove?.Invoke(this, to);
			}
		}

		public void CreateDefaultFiles()
		{
			directories = new Dictionary<string, TWMFolder>();
			AddDir("/");
		}

		public void CreateDocumentsFolder()
		{
			AddDir("/docs_foldername/mygames_foldername/oneshot_foldername/");
		}

		public void CreateOneshotFile()
		{
			Game1.windowMan.Desktop.MoveDocumentsToTopLeft();
			TWMFile tWMFile = new TWMFile("oneshot", "oneshot_appname", LaunchableWindowType.ONESHOT);
			tWMFile.deleteRestricted = true;
			WriteFile("/", tWMFile);
		}

		public void CreateTutorialCompleteFiles()
		{
			TWMFile tWMFile = new TWMFile("customize", "customize_app_name", LaunchableWindowType.CUSTOMIZE);
			tWMFile.deleteRestricted = true;
			WriteFile("/", tWMFile);
			CreateHelpFiles();
			CreateWallpaperFiles(Game1.windowMan.UnlockMan.UnlockedWallpapers);
			CreateThemeFiles(Game1.windowMan.UnlockMan.UnlockedThemes);
			CreateExtraSystemFiles();
		}

		public void CreateExtraSystemFiles()
		{
			if (!FileTypeExists(LaunchableWindowType.TIMER))
			{
				TWMFile tWMFile = new TWMFile("timer", "timer_app_filename", LaunchableWindowType.TIMER);
				tWMFile.deleteRestricted = true;
				WriteFile("/docs_foldername/mygames_foldername/oneshot_foldername/", tWMFile);
			}
			if (!FileTypeExists(LaunchableWindowType.CONTROLS))
			{
				TWMFile tWMFile2 = new TWMFile("controls", "controls_app_filename", LaunchableWindowType.CONTROLS);
				tWMFile2.deleteRestricted = true;
				WriteFile("/", tWMFile2);
			}
			if (!FileTypeExists(LaunchableWindowType.SHUTDOWN))
			{
				TWMFile tWMFile3 = new TWMFile("exit", "shutdown_app_filename", LaunchableWindowType.SHUTDOWN);
				tWMFile3.deleteRestricted = true;
				tWMFile3.moveRestricted = true;
				WriteFile("/", tWMFile3);
			}
		}

		public void CreateHelpFiles()
		{
			AddDir("/help_foldername/");
			TWMFile tWMFile = new TWMFile("help", "tutorial_delete_file_filename", LaunchableWindowType.TUTORIAL_DELETE_FILE);
			tWMFile.deleteRestricted = true;
			tWMFile.moveRestricted = true;
			WriteFile("/help_foldername/", tWMFile);
			TWMFile tWMFile2 = new TWMFile("help", "tutorial_move_file_filename", LaunchableWindowType.TUTORIAL_MOVE_FILE);
			tWMFile2.deleteRestricted = true;
			tWMFile2.moveRestricted = true;
			WriteFile("/help_foldername/", tWMFile2);
			TWMFile tWMFile3 = new TWMFile("help", "tutorial_introduction_filename", LaunchableWindowType.TUTORIAL_INTRO_FILE);
			tWMFile3.deleteRestricted = true;
			tWMFile3.moveRestricted = true;
			WriteFile("/help_foldername/", tWMFile3);
			TWMFile tWMFile4 = new TWMFile("doc", "credits_filename", LaunchableWindowType.DOCUMENT, "credits");
			tWMFile4.deleteRestricted = true;
			WriteFile("/help_foldername/", tWMFile4);
			TWMFile tWMFile5 = new TWMFile("doc", "readme_filename", LaunchableWindowType.DOCUMENT, "readme");
			tWMFile5.deleteRestricted = true;
			WriteFile("/docs_foldername/", tWMFile5);
		}

		public void CreateDeleteTutorialFiles()
		{
			AddDir("/docs_foldername/");
			TWMFile tWMFile = new TWMFile("doc", "tutorial_dummyfile_filename", LaunchableWindowType.DUMMY_FILE_FOR_TUTORIALS, "readme");
			tWMFile.moveRestricted = true;
			WriteFile("/docs_foldername/", tWMFile);
		}

		public void CreateMoveTutorialFiles()
		{
			AddDir("/movefile_tutorial_folder1_name/");
			AddDir("/movefile_tutorial_folder2_name/");
			TWMFile tWMFile = new TWMFile("doc", "tutorial_dummyfile_filename", LaunchableWindowType.DUMMY_FILE_FOR_TUTORIALS, "readme");
			tWMFile.deleteRestricted = true;
			WriteFile("/movefile_tutorial_folder1_name/", tWMFile);
		}

		public void CreateWallpaperFiles(List<string> unlockedWallpapers)
		{
			Delete("/wallpapers_foldername/");
			AddDir("/wallpapers_foldername/");
			foreach (string unlockedWallpaper in unlockedWallpapers)
			{
				CreateWallpaperFile(unlockedWallpaper);
			}
		}

		public void CreateWallpaperFile(string wallpaperId)
		{
			TWMFile tWMFile = new TWMFile("wallpaper_thumbs/" + wallpaperId, wallpaperId, LaunchableWindowType.WALLPAPER, wallpaperId);
			tWMFile.deleteRestricted = true;
			tWMFile.moveRestricted = true;
			WriteFile("/wallpapers_foldername/", tWMFile);
		}

		public void CreateThemeFiles(List<string> unlockedThemes)
		{
			Delete("/themes_foldername/");
			AddDir("/themes_foldername/");
			foreach (string unlockedTheme in unlockedThemes)
			{
				CreateThemeFile(unlockedTheme);
			}
		}

		public void CreateThemeFile(string themeId)
		{
			TWMFile tWMFile = new TWMFile("theme", themeId, LaunchableWindowType.THEME, themeId);
			tWMFile.deleteRestricted = true;
			tWMFile.moveRestricted = true;
			WriteFile("/themes_foldername/", tWMFile);
		}

		public void AddDir(string path)
		{
			if (!directories.ContainsKey(path))
			{
				string[] array = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				string folderName = ((array.Length == 0) ? "desktop_foldername" : array[array.Length - 1]);
				TWMFolder tWMFolder = new TWMFolder(folderName);
				directories.Add(path, tWMFolder);
				string text = ParentPath(path);
				if (text.Length > 0)
				{
					AddDir(text);
				}
				directories.TryGetValue(text, out var value);
				if (value != null)
				{
					value.contents.Add(tWMFolder);
					tWMFolder.parentPath = text;
					NotifyWriteOrDelete?.Invoke(this, text);
				}
			}
		}

		public int ItemCount(string path)
		{
			if (!directories.ContainsKey(path))
			{
				return 0;
			}
			return directories[path].contents.Count;
		}

		public void WriteFile(string path, TWMFile file)
		{
			TWMFile tWMFile = FindNamedFile(file);
			if (tWMFile != null)
			{
				Delete(tWMFile);
			}
			directories.TryGetValue(path, out var value);
			if (value != null)
			{
				value.contents.Add(file);
				file.parentPath = path;
				NotifyWriteOrDelete?.Invoke(this, path);
			}
			else
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, "could not find path when creating '" + file.name + "': " + path);
			}
		}

		public FilesystemSaveManager.FilesystemSaveData GetSaveData()
		{
			FilesystemSaveManager.FilesystemSaveData filesystemSaveData = new FilesystemSaveManager.FilesystemSaveData
			{
				folders = directories.Keys.ToList(),
				files = new List<TWMFile>()
			};
			foreach (string folder in filesystemSaveData.folders)
			{
				foreach (TWMFileNode content in directories[folder].contents)
				{
					if (content is TWMFile item)
					{
						filesystemSaveData.files.Add(item);
					}
				}
			}
			return filesystemSaveData;
		}

		public void LoadSaveData(FilesystemSaveManager.FilesystemSaveData data)
		{
			DeleteEntireFileSystem();
			directories = new Dictionary<string, TWMFolder>();
			foreach (string folder in data.folders)
			{
				AddDir(folder);
			}
			foreach (TWMFile file in data.files)
			{
				WriteFile(file.parentPath, file);
			}
		}

		private void DeleteEntireFileSystem()
		{
			if (directories == null)
			{
				return;
			}
			List<TWMFileNode> list = new List<TWMFileNode>();
			foreach (TWMFolder value in directories.Values)
			{
				foreach (TWMFileNode content in value.contents)
				{
					list.Add(content);
				}
			}
			foreach (TWMFileNode item in list)
			{
				Delete(item);
			}
		}

		public bool FileTypeExists(LaunchableWindowType fileType)
		{
			foreach (TWMFolder value in directories.Values)
			{
				foreach (TWMFileNode content in value.contents)
				{
					if (content is TWMFile && (content as TWMFile).program == fileType)
					{
						return true;
					}
				}
			}
			return false;
		}

		public TWMFile FindNamedFile(TWMFile fileToFind)
		{
			foreach (TWMFolder value in directories.Values)
			{
				foreach (TWMFileNode content in value.contents)
				{
					if (content is TWMFile)
					{
						TWMFile tWMFile = content as TWMFile;
						if (tWMFile.program == fileToFind.program && tWMFile.name == fileToFind.name)
						{
							return tWMFile;
						}
					}
				}
			}
			return null;
		}

		public void MoveFilesOffDesktop(int desktopCapacity)
		{
			TWMFolder tWMFolder = directories["/"];
			int num = ItemCount("/");
			List<TWMFile> list = new List<TWMFile>();
			foreach (TWMFileNode content in tWMFolder.contents)
			{
				if (content is TWMFile tWMFile && !tWMFile.moveRestricted)
				{
					LaunchableWindowType program = tWMFile.program;
					if ((uint)(program - 1) <= 1u || (uint)(program - 9) <= 2u)
					{
						list.Add(tWMFile);
					}
				}
			}
			while (num > desktopCapacity && list.Count > 0)
			{
				int index = list.Count - 1;
				TWMFile file = list[index];
				MoveFile(file, "/docs_foldername/");
				list.RemoveAt(index);
				num--;
			}
		}
	}
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM.Filesystem;

namespace OneShotMG.src.TWM
{
	public class TWMDesktopManager : IconGrid
	{
		public delegate void DesktopLayoutChangeHandler();

		public WallpaperInfo wallpaper;

		public WallpaperInfo rememberedWallpaper;

		private Dictionary<string, WallpaperInfoSaveData> wallpaperMetadata;

		public bool inSolstice;

		public TWMDesktopManager(string folderPath, TWMFilesystem fs, Vec2 size, bool leftToRight = false)
			: base(folderPath, fs, size, leftToRight)
		{
			LoadWallpaperMetadata();
		}

		public void LoadWallpaperMetadata()
		{
			wallpaperMetadata = new Dictionary<string, WallpaperInfoSaveData>();
			WallpaperInfoSaveData[] wallpapers = JsonConvert.DeserializeObject<WallpaperMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/wallpapers_metadata.json"))).wallpapers;
			foreach (WallpaperInfoSaveData wallpaperInfoSaveData in wallpapers)
			{
				if (!wallpaperMetadata.TryGetValue(wallpaperInfoSaveData.imageFile, out var _))
				{
					wallpaperMetadata.Add(wallpaperInfoSaveData.imageFile, wallpaperInfoSaveData);
				}
			}
		}

		public FilesystemSaveManager.DesktopSaveData GetLayout()
		{
			FilesystemSaveManager.DesktopSaveData desktopSaveData = new FilesystemSaveManager.DesktopSaveData
			{
				iconPositions = new Dictionary<string, Vec2>()
			};
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					if (grid[i, j] != null)
					{
						desktopSaveData.iconPositions[grid[i, j].file.name] = new Vec2(i, j);
					}
				}
			}
			desktopSaveData.currentWallpaper = wallpaper.imageFile;
			return desktopSaveData;
		}

		public void DrawDesktop(TWMTheme theme, Vec2 offset, byte alpha = byte.MaxValue, bool drawIcons = true)
		{
			if (wallpaper != null)
			{
				Rect area = new Rect(offset.X, offset.Y, Game1.windowMan.ScreenSize.X, Game1.windowMan.ScreenSize.Y);
				wallpaper.Draw(area);
			}
			if (drawIcons)
			{
				base.Draw(theme, offset, alpha);
			}
		}

		public void SetLayout(FilesystemSaveManager.DesktopSaveData data)
		{
			foreach (string key in data.iconPositions.Keys)
			{
				Vec2 vec = data.iconPositions[key];
				if (vec.X >= grid.GetLength(0) || vec.Y >= grid.GetLength(1))
				{
					continue;
				}
				FileIcon fileIcon = grid[vec.X, vec.Y];
				if (fileIcon != null && fileIcon.file != null && fileIcon.file.name == key)
				{
					continue;
				}
				for (int i = 0; i < grid.GetLength(0); i++)
				{
					for (int j = 0; j < grid.GetLength(1); j++)
					{
						FileIcon fileIcon2 = grid[i, j];
						if (fileIcon2 != null && fileIcon2.file.name == key)
						{
							grid[i, j] = grid[vec.X, vec.Y];
							grid[vec.X, vec.Y] = fileIcon2;
							j = grid.GetLength(1);
							i = grid.GetLength(0);
						}
					}
				}
			}
			WallpaperInfoSaveData wallpaperInfoSaveDataFromId = GetWallpaperInfoSaveDataFromId(data.currentWallpaper);
			if (wallpaperInfoSaveDataFromId == null)
			{
				wallpaperInfoSaveDataFromId = GetWallpaperInfoSaveDataFromId(WallpaperInfo.DEFAULT_WALLPAPERS[0]);
			}
			wallpaper = new WallpaperInfo(wallpaperInfoSaveDataFromId);
			inSolstice = data.inSolstice;
		}

		public List<WallpaperInfoSaveData> GetAllWallpaperMetadata()
		{
			return wallpaperMetadata.Values.ToList();
		}

		public WallpaperInfoSaveData GetWallpaperInfoSaveDataFromId(string id)
		{
			if (wallpaperMetadata.TryGetValue(id, out var value))
			{
				return value;
			}
			return null;
		}

		public void SetWallpaper(string id)
		{
			WallpaperInfoSaveData wallpaperInfoSaveDataFromId = GetWallpaperInfoSaveDataFromId(id);
			if (wallpaperInfoSaveDataFromId != null)
			{
				Game1.gMan.clearTextureCache(TextureCache.CacheType.DesktopWallpaper);
				wallpaper = new WallpaperInfo(wallpaperInfoSaveDataFromId);
			}
		}

		public void OverrideWallpaper(WallpaperInfo wp)
		{
			Game1.gMan.clearTextureCache(TextureCache.CacheType.DesktopWallpaper);
			rememberedWallpaper = wallpaper;
			wallpaper = wp;
		}

		public void MoveDocumentsToTopLeft()
		{
			Vec2 vec = new Vec2(-1, -1);
			bool flag = false;
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				if (flag)
				{
					break;
				}
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					if (flag)
					{
						break;
					}
					FileIcon fileIcon = grid[i, j];
					if (fileIcon != null && fileIcon.file is TWMFolder tWMFolder && tWMFolder.Path == "/docs_foldername/")
					{
						vec.X = i;
						vec.Y = j;
						flag = true;
					}
				}
			}
			if (flag && (vec.X != 0 || vec.Y != 0))
			{
				FileIcon fileIcon2 = grid[vec.X, vec.Y];
				grid[vec.X, vec.Y] = grid[0, 0];
				grid[0, 0] = fileIcon2;
			}
		}

		public void RestoreWallpaper()
		{
			if (rememberedWallpaper != null)
			{
				wallpaper = rememberedWallpaper;
				rememberedWallpaper = null;
			}
		}

		public bool IsWallpaperOverridden()
		{
			return rememberedWallpaper != null;
		}

		public WallpaperInfo GetCurrentWallpaper()
		{
			if (rememberedWallpaper != null)
			{
				return rememberedWallpaper;
			}
			return wallpaper;
		}

		protected override void OnNotifyFsChange(TWMFilesystem fs, string path)
		{
			base.OnNotifyFsChange(fs, path);
			if (path == fsPath)
			{
				Game1.windowMan?.SaveDesktopAndFileSystem();
			}
		}

		public override void OnDragDropped(DraggedItem item, Vec2 pos)
		{
			if (item.Icon != null && !CheckPreventMoveFolder(item) && !CheckPreventMoveRestricted(item) && !CheckForFolderDrop(item, pos) && !CheckBlockedByTutorial(item) && !CheckTooManyFiles(item) && !CheckPathMoveRestricted(item) && !Game1.windowMan.IsModalWindowOpen())
			{
				Vec2 vec = PointToGridPos(pos);
				if (item.DragSource == this)
				{
					SwapIcons(clickGridPos, vec);
				}
				else
				{
					InsertIcon(item.Icon, vec);
				}
				heldIcon = null;
				item.OnDropComplete?.Invoke(obj: true);
				if (item.DragSource != this)
				{
					fs.MoveFile(item.Icon.file as TWMFile, fsPath);
				}
				UnselectIcon();
			}
		}

		public bool IsDesktopFull()
		{
			return fs.ItemCount("/") >= grid.Length;
		}

		private bool CheckTooManyFiles(DraggedItem item)
		{
			if (IsDesktopFull() && item.Icon.file.parentPath != "/")
			{
				CreateModal(ModalWindow.ModalType.Error, "error_too_many_items");
				return true;
			}
			return false;
		}

		private void SwapIcons(Vec2 from, Vec2 to)
		{
			FileIcon fileIcon = grid[to.X, to.Y];
			grid[to.X, to.Y] = grid[from.X, from.Y];
			grid[from.X, from.Y] = fileIcon;
		}
	}
}

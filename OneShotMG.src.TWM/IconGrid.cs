using System;
using System.Collections.Generic;
using OneShotMG.src.TWM.Filesystem;

namespace OneShotMG.src.TWM
{
	public class IconGrid : IDropTarget
	{
		public const string E_MOVE_FOLDER = "error_move_folder";

		public const string E_MOVE_RESTRICTED = "error_move_restricted";

		public const string E_MOVE_TOO_MANY_ITEMS = "error_too_many_items";

		protected const int BORDER_MARGIN = 4;

		protected const int DOUBLECLICK_TIME = 30;

		private const int ICON_MOVE_TOLERANCE = 3;

		protected FileIcon[,] grid;

		protected FileIcon clickedIcon;

		protected int framesSinceClick = 30;

		protected Vec2 clickGridPos = new Vec2(-1, -1);

		protected bool cursorWasFree;

		protected TWMFilesystem fs;

		protected string fsPath;

		protected Vec2 bounds;

		private Vec2 clickPos;

		protected FileIcon heldIcon;

		public int GridDisplayRowStart;

		private int gridDisplayHeight;

		public BrowserWindow.SetBrowserPath FolderOverride;

		public ModalWindow.CreateModalHandler CreateModal;

		public Action<DraggedItem> OnBeginDrag;

		private readonly bool BrowserIconGrid;

		private Action onItemDropped;

		public IconGrid(string folderPath, TWMFilesystem fs, Vec2 size, bool leftToRight = false, Action onItemDropped = null)
		{
			this.fs = fs;
			BrowserIconGrid = leftToRight;
			fsPath = folderPath;
			SetIconArea(size);
			fs.NotifyWriteOrDelete = (TWMFilesystem.FsNotify)Delegate.Combine(fs.NotifyWriteOrDelete, new TWMFilesystem.FsNotify(OnNotifyFsChange));
			fs.NotifyMove = (TWMFilesystem.FsNotify)Delegate.Combine(fs.NotifyMove, new TWMFilesystem.FsNotify(OnNotifyFsChange));
			OnNotifyFsChange(fs, folderPath);
			this.onItemDropped = onItemDropped;
		}

		public void UnregisterDelegate(TWMFilesystem fs)
		{
			fs.NotifyWriteOrDelete = (TWMFilesystem.FsNotify)Delegate.Remove(fs.NotifyWriteOrDelete, new TWMFilesystem.FsNotify(OnNotifyFsChange));
			fs.NotifyMove = (TWMFilesystem.FsNotify)Delegate.Remove(fs.NotifyMove, new TWMFilesystem.FsNotify(OnNotifyFsChange));
		}

		public virtual void Update(Vec2 offset, bool inputConsumed)
		{
			FileIcon[,] array = grid;
			int upperBound = array.GetUpperBound(0);
			int upperBound2 = array.GetUpperBound(1);
			for (int i = array.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = array.GetLowerBound(1); j <= upperBound2; j++)
				{
					array[i, j]?.Update();
				}
			}
			cursorWasFree = !inputConsumed;
			if (framesSinceClick <= 30)
			{
				framesSinceClick++;
			}
			Vec2 vec = Game1.mouseCursorMan.MousePos - offset;
			if (clickedIcon != null)
			{
				if (Game1.mouseCursorMan.MouseHeld)
				{
					if ((Game1.windowMan.TutorialStep == TutorialStep.DRAG_FILE || (Game1.windowMan.TutorialStep == TutorialStep.MOVEFILE_DRAG_FILE && clickedIcon.file is TWMFile tWMFile && tWMFile.program == LaunchableWindowType.DUMMY_FILE_FOR_TUTORIALS) || Game1.windowMan.TutorialStep == TutorialStep.COMPLETE) && OnBeginDrag != null && (Math.Abs(vec.X - clickPos.X) > 3 || Math.Abs(vec.Y - clickPos.Y) > 3))
					{
						heldIcon = clickedIcon;
						clickedIcon = null;
						DraggedItem obj = new DraggedItem(heldIcon, delegate
						{
							heldIcon = null;
						}, this);
						OnBeginDrag(obj);
					}
				}
				else
				{
					clickedIcon = null;
				}
			}
			else
			{
				CheckForClick(offset, inputConsumed);
			}
		}

		public virtual void Draw(TWMTheme theme, Vec2 offset, byte alpha = byte.MaxValue)
		{
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = GridDisplayRowStart; j < GridDisplayRowStart + gridDisplayHeight && j < grid.GetLength(1); j++)
				{
					FileIcon fileIcon = grid[i, j];
					if (fileIcon != null)
					{
						bool focus = i == clickGridPos.X && j == clickGridPos.Y;
						Vec2 pos = GridPosition(i, j) + offset;
						float num = (float)(int)alpha / 255f;
						if (fileIcon == heldIcon)
						{
							num /= 2f;
						}
						fileIcon.Draw(theme, pos, focus, cursorWasFree, num);
					}
				}
			}
		}

		private void CheckForClick(Vec2 offset, bool inputConsumed)
		{
			bool flag = Game1.windowMan.TutorialStep == TutorialStep.COMPLETE || Game1.windowMan.TutorialStep == TutorialStep.DRAG_FILE || Game1.windowMan.TutorialStep == TutorialStep.LAUNCH_APPLICATION || Game1.windowMan.TutorialStep == TutorialStep.DELETE_CLICK_FILE || Game1.windowMan.TutorialStep == TutorialStep.MOVEFILE_DRAG_FILE;
			if (!(!inputConsumed && flag) || !Game1.mouseCursorMan.MouseClicked)
			{
				return;
			}
			Vec2 v = Game1.mouseCursorMan.MousePos - offset;
			if (!new Rect(0, 0, bounds.X, bounds.Y).IsVec2InRect(v))
			{
				return;
			}
			int num = Math.Min(v.X / 84, grid.GetLength(0) - 1);
			int num2 = Math.Min(v.Y / 72, gridDisplayHeight - 1) + GridDisplayRowStart;
			bool flag2 = false;
			FileIcon fileIcon = grid[num, num2];
			if (fileIcon != null && fileIcon.ClickAreaForIcon(GridPosition(num, num2)).IsVec2InRect(v))
			{
				flag2 = true;
				Vec2 vec = new Vec2(num, num2);
				if (framesSinceClick < 30 && vec.X == clickGridPos.X && vec.Y == clickGridPos.Y)
				{
					framesSinceClick = 30;
					if (Game1.windowMan.AllowLaunchApplications())
					{
						if (FolderOverride != null && fileIcon.file is TWMFolder tWMFolder)
						{
							FolderOverride(tWMFolder.Path);
						}
						else
						{
							Game1.windowMan.RunFile(fileIcon.file);
						}
					}
				}
				else
				{
					framesSinceClick = 0;
					clickedIcon = fileIcon;
					clickGridPos = vec;
					clickPos = v;
				}
			}
			if (!flag2)
			{
				UnselectIcon();
			}
		}

		public void UnselectIcon()
		{
			clickGridPos = new Vec2(-1, -1);
		}

		protected virtual void OnNotifyFsChange(TWMFilesystem fs, string path)
		{
			if (path != fsPath)
			{
				return;
			}
			TWMFolder dir = fs.GetDir(path);
			if (dir != null)
			{
				if (BrowserIconGrid)
				{
					SetIconArea(bounds);
				}
				HashSet<TWMFileNode> hashSet = new HashSet<TWMFileNode>();
				FileIcon[,] array = grid;
				foreach (FileIcon fileIcon in array)
				{
					if (fileIcon != null)
					{
						hashSet.Add(fileIcon.file);
					}
				}
				foreach (TWMFileNode content in dir.contents)
				{
					if (hashSet.Contains(content))
					{
						hashSet.Remove(content);
					}
				}
				if (hashSet.Count > 0)
				{
					for (int k = 0; k < grid.GetLength(0); k++)
					{
						for (int l = 0; l < grid.GetLength(1); l++)
						{
							if (hashSet.Contains(grid[k, l]?.file))
							{
								grid[k, l] = null;
							}
						}
					}
				}
				HashSet<TWMFileNode> hashSet2 = new HashSet<TWMFileNode>();
				array = grid;
				foreach (FileIcon fileIcon2 in array)
				{
					if (fileIcon2 != null)
					{
						hashSet2.Add(fileIcon2.file);
					}
				}
				foreach (TWMFileNode content2 in dir.contents)
				{
					if (!hashSet2.Contains(content2))
					{
						AddIcon(content2);
					}
				}
				if (!BrowserIconGrid)
				{
					return;
				}
				for (int m = 0; m < grid.Length; m++)
				{
					int num = m % grid.GetLength(0);
					int num2 = m / grid.GetLength(0);
					if (grid[num, num2] != null)
					{
						continue;
					}
					bool flag = false;
					for (int n = m + 1; n < grid.Length; n++)
					{
						int num3 = n % grid.GetLength(0);
						int num4 = n / grid.GetLength(0);
						if (grid[num3, num4] != null)
						{
							flag = true;
							grid[num, num2] = grid[num3, num4];
							grid[num3, num4] = null;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
				}
			}
			else
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, "No folder found for icon grid at path " + path);
			}
		}

		public FileIcon GetSelected()
		{
			if (clickGridPos.X >= 0 && clickGridPos.Y >= 0)
			{
				return grid[clickGridPos.X, clickGridPos.Y];
			}
			return null;
		}

		public int GetNumberOfGridRows()
		{
			return grid.GetLength(1);
		}

		public void SetIconArea(Vec2 size)
		{
			bounds = size;
			int num = (size.X - 8) / 84;
			int num2 = (gridDisplayHeight = (size.Y - 8) / 72);
			int num3 = fs.ItemCount(fsPath);
			if (BrowserIconGrid && num * num2 < num3)
			{
				num2 = num3 / num + ((num3 % num > 0) ? 1 : 0);
			}
			if (grid == null)
			{
				grid = new FileIcon[num, num2];
			}
			else
			{
				if (grid.GetLength(0) == num && grid.GetLength(1) == num2)
				{
					return;
				}
				if (!BrowserIconGrid && fsPath == "/" && num2 * num < num3)
				{
					fs.MoveFilesOffDesktop(num2 * num);
				}
				FileIcon[,] oldIcons = grid;
				grid = new FileIcon[num, num2];
				TransferResizedIconArea(oldIcons);
				if (clickGridPos.X >= num || clickGridPos.Y >= num2)
				{
					UnselectIcon();
				}
				if (GridDisplayRowStart + gridDisplayHeight > num2)
				{
					GridDisplayRowStart = num2 - gridDisplayHeight;
					if (GridDisplayRowStart < 0)
					{
						GridDisplayRowStart = 0;
					}
				}
			}
		}

		protected void AddIcon(TWMFileNode node)
		{
			int num = (BrowserIconGrid ? grid.GetLength(1) : grid.GetLength(0));
			int num2 = (BrowserIconGrid ? grid.GetLength(0) : grid.GetLength(1));
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					int num3 = (BrowserIconGrid ? j : i);
					int num4 = (BrowserIconGrid ? i : j);
					if (grid[num3, num4] == null)
					{
						grid[num3, num4] = new FileIcon(node);
						return;
					}
				}
			}
		}

		private void TransferResizedIconArea(FileIcon[,] oldIcons)
		{
			for (int i = 0; i < oldIcons.GetLength(0); i++)
			{
				for (int j = 0; j < oldIcons.GetLength(1); j++)
				{
					if (oldIcons[i, j] != null)
					{
						if (BrowserIconGrid && (i >= grid.GetLength(0) || j >= grid.GetLength(1)))
						{
							return;
						}
						int x = Math.Min(i, grid.GetLength(0) - 1);
						int y = Math.Min(j, grid.GetLength(1) - 1);
						InsertIcon(oldIcons[i, j], new Vec2(x, y));
					}
				}
			}
		}

		protected void InsertIcon(FileIcon icon, Vec2 position)
		{
			if (grid[position.X, position.Y] == icon)
			{
				return;
			}
			bool flag = true;
			FileIcon fileIcon = icon;
			for (int num = position.X; num < grid.GetLength(0); num = (num + 1) % grid.GetLength(0))
			{
				for (int i = (flag ? position.Y : 0); i < grid.GetLength(1); i++)
				{
					if (!flag && num == position.X && i == position.Y)
					{
						if (BrowserIconGrid)
						{
							return;
						}
						throw new InvalidOperationException("Attempted to insert an icon, but the desktop has no available space.");
					}
					FileIcon fileIcon2 = grid[num, i];
					grid[num, i] = fileIcon;
					if (fileIcon2 == null)
					{
						return;
					}
					fileIcon = fileIcon2;
				}
				flag = false;
			}
		}

		protected Vec2 GridPosition(int gridX, int gridY)
		{
			return new Vec2(gridX * 84 + 4, (gridY - GridDisplayRowStart) * 72 + 4);
		}

		public virtual void OnDragDropped(DraggedItem item, Vec2 pos)
		{
			if (CheckPreventMoveFolder(item) || CheckPreventMoveRestricted(item) || CheckForFolderDrop(item, pos) || CheckBlockedByTutorial(item) || CheckPathMoveRestricted(item))
			{
				return;
			}
			if (item.DragSource != this)
			{
				if (item.Icon?.file is TWMFile file)
				{
					fs.MoveFile(file, fsPath);
				}
				item.OnDropComplete?.Invoke(obj: true);
				onItemDropped?.Invoke();
			}
			else
			{
				item.OnDropComplete?.Invoke(obj: false);
			}
		}

		protected Vec2 PointToGridPos(Vec2 pos)
		{
			Vec2 result = default(Vec2);
			result.X = pos.X / 84;
			result.Y = pos.Y / 72;
			result.X = Math.Min(result.X, grid.GetLength(0) - 1);
			result.Y = Math.Min(result.Y, grid.GetLength(1) - 1);
			return result;
		}

		protected bool CheckForFolderDrop(DraggedItem item, Vec2 cursorPos)
		{
			Vec2 vec = PointToGridPos(cursorPos);
			FileIcon fileIcon = grid[vec.X, vec.Y];
			if (fileIcon == null)
			{
				return false;
			}
			Vec2 pos = GridPosition(vec.X, vec.Y);
			if (fileIcon.ClickAreaForIcon(pos).IsVec2InRect(cursorPos) && fileIcon.file is TWMFolder tWMFolder)
			{
				if (item.Icon?.file is TWMFolder)
				{
					if (fileIcon.file != item.Icon?.file && fileIcon.file.parentPath != "/")
					{
						CreateModal(ModalWindow.ModalType.Error, "error_move_folder");
					}
					item.OnDropComplete?.Invoke(obj: false);
					return false;
				}
				if (item.Icon?.file is TWMFile tWMFile)
				{
					if (tWMFile.moveRestricted || fs.IsPathMoveRestricted(tWMFolder.Path))
					{
						CreateModal(ModalWindow.ModalType.Error, "error_move_restricted");
						item.OnDropComplete?.Invoke(obj: false);
						return false;
					}
					fs.MoveFile(tWMFile, tWMFolder.Path);
					item.OnDropComplete?.Invoke(obj: true);
					return true;
				}
			}
			return false;
		}

		protected bool CheckPreventMoveFolder(DraggedItem item)
		{
			if (item.Icon?.file is TWMFolder && item.DragSource != this)
			{
				CreateModal(ModalWindow.ModalType.Error, "error_move_folder");
				item.OnDropComplete?.Invoke(obj: false);
				return true;
			}
			return false;
		}

		protected bool CheckPreventMoveRestricted(DraggedItem item)
		{
			if (item.Icon.file.moveRestricted && item.DragSource != this)
			{
				CreateModal(ModalWindow.ModalType.Error, "error_move_restricted");
				item.OnDropComplete?.Invoke(obj: false);
				return true;
			}
			return false;
		}

		protected bool CheckPathMoveRestricted(DraggedItem item)
		{
			if (fs.IsPathMoveRestricted(fsPath) && item.DragSource != this)
			{
				CreateModal(ModalWindow.ModalType.Error, "error_move_restricted");
				item.OnDropComplete?.Invoke(obj: false);
				return true;
			}
			return false;
		}

		protected bool CheckBlockedByTutorial(DraggedItem item)
		{
			if (Game1.windowMan.TutorialStep != TutorialStep.COMPLETE && item.Icon.file is TWMFile tWMFile && tWMFile.program == LaunchableWindowType.DUMMY_FILE_FOR_TUTORIALS && fsPath != "/movefile_tutorial_folder2_name/")
			{
				item.OnDropComplete?.Invoke(obj: false);
				return true;
			}
			return false;
		}

		public Vec2 GetIconPosition(Predicate<FileIcon> predicate, Vec2 offset)
		{
			for (int i = 0; i < grid.GetLength(0); i++)
			{
				for (int j = 0; j < grid.GetLength(1); j++)
				{
					FileIcon fileIcon = grid[i, j];
					if (fileIcon != null && predicate(fileIcon))
					{
						Rect rect = fileIcon.ClickAreaForIcon(GridPosition(i, j));
						return rect.XY + new Vec2(rect.W / 2, rect.H / 2) + offset;
					}
				}
			}
			return Vec2.Zero;
		}

		public bool IsIconSelected(Func<FileIcon, bool> iconPredicate)
		{
			if (clickGridPos.X >= 0 && clickGridPos.X < grid.GetLength(0) && clickGridPos.Y >= 0 && clickGridPos.Y < grid.GetLength(1))
			{
				FileIcon fileIcon = grid[clickGridPos.X, clickGridPos.Y];
				if (fileIcon != null)
				{
					return iconPredicate(fileIcon);
				}
				return false;
			}
			return false;
		}
	}
}

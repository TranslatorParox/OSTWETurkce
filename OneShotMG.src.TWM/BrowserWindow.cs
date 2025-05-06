using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM.Filesystem;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class BrowserWindow : TWMWindow
	{
		public delegate void SetBrowserPath(string path);

		private class BreadCrumb
		{
			public string folderName;

			public string displayName;

			public Rect bound;

			public bool hovered;

			public TempTexture titleTexture;
		}

		public const int BROWSER_WIDTH = 360;

		public const int BROWSER_HEIGHT = 200;

		public const int ROWS_IN_BROWSER = 2;

		private const int BREADCRUMB_LEFT_MARGIN = 24;

		private const string ICON_FOLDERUP = "the_world_machine/window_icons/folder_up";

		private const string ICON_DELETE = "the_world_machine/window_icons/trash";

		private int BREADCRUMB_BAR_HEIGHT = 22;

		private TWMFilesystem fs;

		private IconGrid iconSpace;

		private Action<DraggedItem> dragHandler;

		private readonly IconButton folderUpButton;

		private readonly IconButton deleteButton;

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private BreadCrumb[] breadcrumbs;

		private SliderControl slider;

		public string Path { get; private set; }

		public BrowserWindow(TWMFilesystem fs, string path, Action<DraggedItem> dragHandler)
		{
			BrowserWindow browserWindow = this;
			this.fs = fs;
			this.dragHandler = dragHandler;
			base.WindowIcon = "folder";
			base.ContentsSize = new Vec2(360, 200);
			NavigateTo(path);
			AddButton(TWMWindowButtonType.Close, delegate
			{
				if (Game1.windowMan.TutorialStep == TutorialStep.CLOSE_WINDOW || Game1.windowMan.TutorialStep == TutorialStep.COMPLETE)
				{
					browserWindow.iconSpace.UnregisterDelegate(fs);
					browserWindow.onClose(browserWindow);
				}
			});
			AddButton(TWMWindowButtonType.Minimize);
			int y = (BREADCRUMB_BAR_HEIGHT - 16) / 2;
			folderUpButton = new IconButton("the_world_machine/window_icons/folder_up", new Vec2(4, y), OnFolderUp);
			deleteButton = new IconButton("the_world_machine/window_icons/trash", new Vec2(base.ContentsSize.X - 20, y), OnFileDelete);
			slider = new SliderControl(string.Empty, 0, 1, new Vec2(343, BREADCRUMB_BAR_HEIGHT + 1), 200 - BREADCRUMB_BAR_HEIGHT - 2, useButtons: true, vertical: true);
			slider.OnValueChanged = OnSliderChange;
			slider.Active = iconSpace.GetNumberOfGridRows() > 2;
			if (slider.Active)
			{
				slider.Max = iconSpace.GetNumberOfGridRows() - 2;
			}
			Vec2 contentsSize = base.ContentsSize;
			contentsSize.Y -= BREADCRUMB_BAR_HEIGHT;
			slider.ScrollTriggerZone = new Rect(0, BREADCRUMB_BAR_HEIGHT, contentsSize.X, contentsSize.Y);
		}

		private void OnSliderChange(int newVal)
		{
			iconSpace.GridDisplayRowStart = newVal;
		}

		public override bool Update(bool mouseInputWasConsumed)
		{
			BreadCrumb[] array = breadcrumbs;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].titleTexture.KeepAlive();
			}
			Vec2 vec = new Vec2(Pos.X + 2, Pos.Y + 26);
			Vec2 offset = vec;
			offset.Y += BREADCRUMB_BAR_HEIGHT;
			bool flag = !mouseInputWasConsumed && !IsModalWindowOpen() && !base.IsMinimized;
			iconSpace.Update(offset, !flag);
			if (base.IsMinimized)
			{
				return base.Update(mouseInputWasConsumed);
			}
			folderUpButton.Update(vec, flag);
			deleteButton.Update(vec, flag);
			slider.Active = iconSpace.GetNumberOfGridRows() > 2;
			if (slider.Active)
			{
				slider.Max = iconSpace.GetNumberOfGridRows() - 2;
				if (slider.Value > slider.Max)
				{
					slider.Value = slider.Max;
				}
			}
			slider.Update(vec, flag);
			if (flag && Game1.windowMan.TutorialStep == TutorialStep.COMPLETE)
			{
				string text = "/";
				array = breadcrumbs;
				foreach (BreadCrumb breadCrumb in array)
				{
					text = text + breadCrumb.folderName + "/";
					Rect bound = breadCrumb.bound;
					bound.X += vec.X;
					bound.Y += vec.Y;
					breadCrumb.hovered = bound.IsVec2InRect(Game1.mouseCursorMan.MousePos);
					if (breadCrumb.hovered && Game1.mouseCursorMan.MouseClicked)
					{
						NavigateTo(text);
						break;
					}
				}
			}
			folderUpButton.Disabled = breadcrumbs.Length <= 1;
			deleteButton.Disabled = iconSpace.GetSelected() == null;
			return base.Update(mouseInputWasConsumed);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			GameColor gColor2 = theme.Primary(alpha);
			GameColor gColor3 = theme.Variant(alpha);
			Rect rect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(rect, gColor);
			Rect boxRect = rect;
			boxRect.Y = screenPos.Y + BREADCRUMB_BAR_HEIGHT - 2;
			boxRect.H = 2;
			Game1.gMan.ColorBoxBlit(boxRect, gColor2);
			DrawBreadcrumbs(theme, screenPos, alpha);
			if (Path == "/wallpapers_foldername/")
			{
				int count = Game1.windowMan.UnlockMan.UnlockedWallpapers.Count;
				int count2 = Game1.windowMan.Desktop.GetAllWallpaperMetadata().Count;
				string text = $"{count}/{count2}";
				Vec2 pixelPos = screenPos;
				pixelPos.X += base.ContentsSize.X - 24 - Game1.gMan.TextSize(GraphicsManager.FontType.OS, text).X;
				pixelPos.Y += base.ContentsSize.Y - 20;
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, pixelPos, text, gColor3);
			}
			else if (Path == "/themes_foldername/")
			{
				int count3 = Game1.windowMan.UnlockMan.UnlockedThemes.Count;
				int count4 = Game1.windowMan.LoadedThemes.Count;
				string text2 = $"{count3}/{count4}";
				Vec2 pixelPos2 = screenPos;
				pixelPos2.X += base.ContentsSize.X - 24 - Game1.gMan.TextSize(GraphicsManager.FontType.OS, text2).X;
				pixelPos2.Y += base.ContentsSize.Y - 20;
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, pixelPos2, text2, gColor3);
			}
			folderUpButton.Draw(screenPos, theme, alpha);
			byte b = alpha;
			if (Game1.windowMan.TutorialStep == TutorialStep.DELETE_CLICK_DELETE)
			{
				float num = (float)Math.Abs(objectHighlightTimer - 40) / 40f;
				num *= 0.6f;
				num += 0.4f;
				b = (byte)((float)(int)b * num);
			}
			deleteButton.Draw(screenPos, theme, b);
			slider.Draw(theme, screenPos, alpha);
			screenPos.Y += BREADCRUMB_BAR_HEIGHT;
			iconSpace.Draw(theme, screenPos, alpha);
		}

		private void CreateBreadcrumbs(string path)
		{
			string[] array = path.Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			Vec2 vec = Game1.gMan.TextSize(GraphicsManager.FontType.OS, "/");
			Vec2 vec2 = new Vec2(24 + vec.X + 2, 2);
			breadcrumbs = new BreadCrumb[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				string tWMLocString = Game1.languageMan.GetTWMLocString(text);
				Vec2 vec3 = Game1.gMan.TextSize(GraphicsManager.FontType.OS, tWMLocString);
				BreadCrumb breadCrumb = new BreadCrumb
				{
					folderName = text,
					displayName = tWMLocString,
					bound = new Rect(vec2.X, vec2.Y, vec3.X + 4, 16),
					hovered = false
				};
				breadCrumb.titleTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, tWMLocString);
				vec2.X += breadCrumb.bound.W + vec.X + 2;
				breadcrumbs[i] = breadCrumb;
			}
		}

		private void DrawBreadcrumbs(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gameColor = theme.Primary(alpha);
			GameColor gameColor2 = theme.Background(alpha);
			Vec2 vec = Game1.gMan.TextSize(GraphicsManager.FontType.OS, "/");
			BreadCrumb[] array = breadcrumbs;
			foreach (BreadCrumb breadCrumb in array)
			{
				GameColor gColor = (breadCrumb.hovered ? gameColor : gameColor2);
				GameColor gameColor3 = (breadCrumb.hovered ? gameColor2 : gameColor);
				Rect bound = breadCrumb.bound;
				bound.X += screenPos.X;
				bound.Y += screenPos.Y;
				Vec2 vec2 = new Vec2(bound.X - vec.X - 1, bound.Y - 1);
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, vec2, "/", gameColor);
				Game1.gMan.ColorBoxBlit(bound, gameColor);
				bound.X++;
				bound.Y++;
				bound.W -= 2;
				bound.H -= 2;
				Game1.gMan.ColorBoxBlit(bound, gColor);
				vec2.X = bound.X - 1;
				vec2.Y += 4;
				Game1.gMan.MainBlit(breadCrumb.titleTexture, vec2 * 2, gameColor3, 0, GraphicsManager.BlendMode.Normal, 1);
			}
		}

		public override bool IsSameContent(TWMWindow window)
		{
			if (window is BrowserWindow browserWindow && browserWindow.Path == Path)
			{
				return TWMFilesystem.ParentPath(Path) != "/";
			}
			return false;
		}

		protected override void HandleDragDropped(DraggedItem item, Vec2 contentPos)
		{
			if (contentPos.Y <= BREADCRUMB_BAR_HEIGHT)
			{
				string text = "/";
				BreadCrumb[] array = breadcrumbs;
				foreach (BreadCrumb breadCrumb in array)
				{
					text = text + breadCrumb.folderName + "/";
					if (breadCrumb.bound.IsVec2InRect(contentPos))
					{
						DropFolder(item, text);
						return;
					}
				}
			}
			contentPos.Y = Math.Max(0, contentPos.Y - BREADCRUMB_BAR_HEIGHT);
			iconSpace.OnDragDropped(item, contentPos);
		}

		private void DropFolder(DraggedItem item, string path)
		{
			if (item.Icon?.file.parentPath != path)
			{
				if (item.Icon?.file is TWMFile tWMFile)
				{
					if (!tWMFile.moveRestricted && !fs.IsPathMoveRestricted(path))
					{
						fs.MoveFile(tWMFile, path);
						item.OnDropComplete?.Invoke(obj: true);
						return;
					}
					ShowModalWindow(ModalWindow.ModalType.Error, "error_move_restricted");
				}
				else
				{
					ShowModalWindow(ModalWindow.ModalType.Error, "error_move_folder");
				}
			}
			item.OnDropComplete?.Invoke(obj: false);
		}

		private void InitIconGrid()
		{
			Vec2 contentsSize = base.ContentsSize;
			contentsSize.Y -= BREADCRUMB_BAR_HEIGHT;
			iconSpace?.UnregisterDelegate(fs);
			iconSpace = new IconGrid(Path, fs, contentsSize, leftToRight: true, onItemDropped);
			iconSpace.OnBeginDrag = dragHandler;
			iconSpace.FolderOverride = NavigateTo;
			iconSpace.CreateModal = base.ShowModalWindow;
		}

		private void onItemDropped()
		{
			grabFocus(this);
		}

		private void NavigateTo(string folderPath)
		{
			Path = folderPath;
			CreateBreadcrumbs(Path);
			base.WindowTitle = Path.TrimEnd(TWMFilesystem.separator);
			string[] array = base.WindowTitle.Split(TWMFilesystem.separator);
			for (int i = 0; i < array.Length; i++)
			{
				string id = array[i];
				id = Game1.languageMan.GetTWMLocString(id);
				array[i] = id;
			}
			base.WindowTitle = string.Join(TWMFilesystem.separator[0].ToString(), array);
			InitIconGrid();
		}

		private void OnFolderUp()
		{
			if (Game1.windowMan.TutorialStep == TutorialStep.COMPLETE)
			{
				string folderPath = TWMFilesystem.ParentPath(Path);
				NavigateTo(folderPath);
			}
		}

		private void OnFileDelete()
		{
			TWMFileNode selected = iconSpace.GetSelected()?.file;
			if (selected != null)
			{
				if (selected.deleteRestricted)
				{
					ShowModalWindow(ModalWindow.ModalType.Error, "delete_restricted_modal");
					return;
				}
				ShowModalWindow(ModalWindow.ModalType.YesNo, string.Format(Game1.languageMan.GetTWMLocString("delete_confirmation_modal"), Game1.languageMan.GetTWMLocString(selected.name)), delegate(ModalWindow.ModalResponse res)
				{
					if (res == ModalWindow.ModalResponse.Yes)
					{
						fs.Delete(selected);
					}
				});
			}
			else
			{
				ShowModalWindow(ModalWindow.ModalType.Error, "delete_no_file_selected_modal");
			}
		}

		public Vec2 GetIconPos(LaunchableWindowType iconType)
		{
			return Pos + iconSpace.GetIconPosition(iconPredicate, Vec2.Zero);
			bool iconPredicate(FileIcon icon)
			{
				if (icon?.file is TWMFile tWMFile)
				{
					return tWMFile.program == iconType;
				}
				return false;
			}
		}

		public void UnselectIcon()
		{
			iconSpace.UnselectIcon();
		}

		public bool IsIconSelected(LaunchableWindowType iconType)
		{
			return iconSpace.IsIconSelected(iconPredicate);
			bool iconPredicate(FileIcon icon)
			{
				if (icon?.file is TWMFile tWMFile)
				{
					return tWMFile.program == iconType;
				}
				return false;
			}
		}

		public Vec2 GetDeleteButtonPos()
		{
			int num = (BREADCRUMB_BAR_HEIGHT - 16) / 2;
			return new Vec2(base.ContentsSize.X - 20, num + 26) + Pos;
		}

		public bool IsShowingDeleteConfirmModal()
		{
			if (modalWindow != null)
			{
				return modalWindow.Type == ModalWindow.ModalType.YesNo;
			}
			return false;
		}

		public Vec2 GetModalButtonsPos()
		{
			if (modalWindow != null)
			{
				return modalWindow.GetButtonsPos();
			}
			return Vec2.Zero;
		}
	}
}

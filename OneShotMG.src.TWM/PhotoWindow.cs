using OneShotMG.src.EngineSpecificCode;

namespace OneShotMG.src.TWM
{
	public class PhotoWindow : TWMWindow
	{
		private string fullPicturePath;

		private int scale;

		private bool isPictureHeld;

		private Vec2 pictureHeldMousePos = Vec2.Zero;

		public PhotoWindow(string sourceName, string displayName, int scale = 1)
		{
			this.scale = scale;
			base.WindowIcon = "photo";
			base.WindowTitle = Game1.languageMan.GetTWMLocString(displayName);
			fullPicturePath = "the_world_machine/photos/" + sourceName;
			base.ContentsSize = Game1.gMan.TextureSize(fullPicturePath) / (2 / scale);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			Game1.gMan.MainBlit(fullPicturePath, screenPos * (2 / scale), (float)(int)alpha / 255f, 0, GraphicsManager.BlendMode.Normal, scale);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			if (window is PhotoWindow photoWindow)
			{
				return photoWindow.fullPicturePath == fullPicturePath;
			}
			return false;
		}

		public override bool Update(bool mouseInputWasConsumed)
		{
			mouseInputWasConsumed |= base.Update(mouseInputWasConsumed);
			if (!mouseInputWasConsumed && !base.IsMinimized)
			{
				Vec2 mousePos = Game1.mouseCursorMan.MousePos;
				mousePos.X -= Pos.X + 2;
				mousePos.Y -= Pos.Y + 26;
				mousePos.X *= 2;
				mousePos.Y *= 2;
				if (mousePos.X >= 0 && mousePos.Y >= 0 && mousePos.X < base.ContentsSize.X * 2 && mousePos.Y < base.ContentsSize.Y * 2)
				{
					mouseInputWasConsumed = true;
					Game1.mouseCursorMan.SetState(MouseCursorManager.State.Grabbable);
					if (isPictureHeld)
					{
						if (Game1.mouseCursorMan.MouseHeld)
						{
							Game1.mouseCursorMan.SetState(MouseCursorManager.State.Holding);
							Vec2 vec = new Vec2(Game1.mouseCursorMan.MousePos.X - pictureHeldMousePos.X, Game1.mouseCursorMan.MousePos.Y - pictureHeldMousePos.Y);
							Pos = new Vec2(Pos.X + vec.X, Pos.Y + vec.Y);
							pictureHeldMousePos = Game1.mouseCursorMan.MousePos;
						}
						else
						{
							isPictureHeld = false;
						}
					}
					else if (Game1.mouseCursorMan.MouseClicked)
					{
						isPictureHeld = true;
						pictureHeldMousePos = Game1.mouseCursorMan.MousePos;
						grabFocus(this);
					}
				}
			}
			else
			{
				isPictureHeld = false;
			}
			return mouseInputWasConsumed;
		}
	}
}

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;

namespace OneShotMG.src.TWM
{
	public class JournalWindow : TWMWindow
	{
		private string currentPicture;

		private string currentPictureId;

		private const int PICTURE_WIDTH = 400;

		private const int PICTURE_HEIGHT = 300;

		private bool isPictureHeld;

		private Vec2 pictureHeldMousePos = Vec2.Zero;

		private Dictionary<string, CloverLocation> cloverLocations;

		private const int CLOVER_WIDTH = 23;

		private const int CLOVER_HEIGHT = 25;

		private const int ONESHOT_CLOVER_X = 294;

		private const int ONESHOT_CLOVER_Y = 239;

		public JournalWindow()
		{
			base.WindowIcon = "clover";
			SetPicture("default");
			showWindowBackground = false;
			base.ContentsSize = new Vec2(400, 300);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
			LoadCloverLocations();
			CheckForSaveInstructions();
		}

		private void CheckForSaveInstructions()
		{
			if (!Game1.windowMan.FileSystem.FileExists("/docs_foldername/mygames_foldername/oneshot_foldername/fakesave_filename"))
			{
				return;
			}
			SaveData saveData = GameSaveManager.LoadSaveData();
			if (saveData != null)
			{
				FlagManager flagManager = new FlagManager(null);
				flagManager.SetRawFlagData(saveData.flagData);
				if (flagManager.IsFlagSet(147))
				{
					SetPicture("save");
				}
			}
		}

		private void LoadCloverLocations()
		{
			cloverLocations = new Dictionary<string, CloverLocation>();
			CloverLocation[] array = JsonConvert.DeserializeObject<CloverLocations>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/journal_clover_data.json"))).cloverLocations;
			foreach (CloverLocation cloverLocation in array)
			{
				if (!cloverLocations.TryGetValue(cloverLocation.id, out var _))
				{
					cloverLocations.Add(cloverLocation.id, cloverLocation);
				}
			}
		}

		public void SetPicture(string picture)
		{
			if (string.IsNullOrEmpty(picture))
			{
				onClose(this);
				return;
			}
			currentPictureId = picture;
			currentPicture = "the_world_machine/journal/" + currentPictureId;
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			screenPos.X *= 2;
			screenPos.Y *= 2;
			if (cloverLocations.TryGetValue(currentPictureId, out var value))
			{
				screenPos += value.drawOffset;
			}
			Game1.gMan.MainBlit(currentPicture, screenPos, (float)(int)alpha / 255f, 0, GraphicsManager.BlendMode.Normal, 1);
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
				if (Game1.gMan.IsPixelSolid(currentPicture, mousePos))
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
			if (Game1.windowMan.IsWindowFocused(this) && cloverLocations.TryGetValue(currentPictureId, out var value) && Game1.windowMan.IsCloverEquipped())
			{
				Vec2 cloverPos = value.cloverPos;
				cloverPos /= 2;
				cloverPos += Pos;
				cloverPos.Y += 26;
				cloverPos.X += 2;
				Vec2 oneshotPos = Game1.windowMan.GetOneshotPos();
				oneshotPos.X += 294;
				oneshotPos.Y += 239;
				if (cloverPos.X >= oneshotPos.X - 11 && cloverPos.X <= oneshotPos.X + 11 && cloverPos.Y >= oneshotPos.Y - 12 && cloverPos.Y <= oneshotPos.Y + 12)
				{
					Vec2 pos = Pos;
					if (cloverPos.X < oneshotPos.X)
					{
						pos.X++;
					}
					else if (cloverPos.X > oneshotPos.X)
					{
						pos.X--;
					}
					if (cloverPos.Y < oneshotPos.Y)
					{
						pos.Y++;
					}
					else if (cloverPos.Y > oneshotPos.Y)
					{
						pos.Y--;
					}
					Pos = pos;
				}
			}
			return mouseInputWasConsumed;
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is JournalWindow;
		}
	}
}

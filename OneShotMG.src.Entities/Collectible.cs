using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class Collectible : Entity
	{
		private bool isMouseHoveringOnUs;

		private const int ICON_WIDTH = 24;

		private const int ICON_HEIGHT = 20;

		private string themeId;

		public Collectible(OneshotWindow osWindow, Event e)
			: base(osWindow, e)
		{
			if (e.pages.Length >= 3 && e.pages[2].condition.switch1_valid)
			{
				switch (e.pages[2].condition.switch1_id)
				{
				case 461:
					themeId = "blue";
					break;
				case 462:
					themeId = "teal";
					break;
				case 463:
					themeId = "green";
					break;
				case 464:
					themeId = "yellow";
					break;
				case 465:
					themeId = "red";
					break;
				case 466:
					themeId = "pink";
					break;
				case 467:
					themeId = "orange";
					break;
				case 468:
					themeId = "white";
					break;
				case 469:
					themeId = "rainbow";
					break;
				}
			}
		}

		public override void Update()
		{
			CommonUpdate();
			if (!active)
			{
				return;
			}
			MathHelper.HandleAbberateUpdate(ref redAbberateTimer, ref redOffset);
			MathHelper.HandleAbberateUpdate(ref blueAbberateTimer, ref blueOffset);
			MathHelper.HandleAbberateUpdate(ref greenAbberateTimer, ref greenOffset);
			isMouseHoveringOnUs = false;
			if (oneshotWindow.tileMapMan.IsInScript())
			{
				return;
			}
			if (!oneshotWindow.IsMaximized)
			{
				if (oneshotWindow.canMouseInteract)
				{
					Vec2 zero = Vec2.Zero;
					Vec2 camPos = oneshotWindow.tileMapMan.GetCamPos();
					zero.X = pos.X / 256 - camPos.X - 12;
					zero.Y = pos.Y / 256 - camPos.Y - 10;
					Rect rect = new Rect(zero.X, zero.Y, 24, 20);
					if (rect.X + rect.W > 0 && rect.X <= 320 && rect.Y + rect.H > 0 && rect.Y <= 240)
					{
						Vec2 vec = new Vec2(oneshotWindow.Pos.X + 2, oneshotWindow.Pos.Y + 26);
						Vec2 mousePos = Game1.mouseCursorMan.MousePos;
						if (mousePos.X >= vec.X && mousePos.X <= vec.X + oneshotWindow.ContentsSize.X && mousePos.Y >= vec.Y && mousePos.Y <= vec.Y + oneshotWindow.ContentsSize.Y)
						{
							rect.X += vec.X;
							rect.Y += vec.Y;
							if (mousePos.X >= rect.X && mousePos.X <= rect.X + rect.W && mousePos.Y >= rect.Y && mousePos.Y <= rect.Y + rect.H)
							{
								isMouseHoveringOnUs = true;
							}
						}
					}
				}
			}
			else
			{
				Vec2 zero2 = Vec2.Zero;
				Vec2 camPos2 = oneshotWindow.tileMapMan.GetCamPos();
				zero2.X = pos.X / 256 - camPos2.X - 12;
				zero2.Y = pos.Y / 256 - camPos2.Y - 10;
				Rect rect2 = new Rect(zero2.X, zero2.Y, 24, 20);
				if (rect2.X + rect2.W > 0 && rect2.X <= 320 && rect2.Y + rect2.H > 0 && rect2.Y <= 240)
				{
					Vec2 mousePos2 = Game1.mouseCursorMan.MousePos;
					Rect fullscreenRect = oneshotWindow.GetFullscreenRect();
					mousePos2.X -= fullscreenRect.X / 2;
					mousePos2.Y -= fullscreenRect.Y / 2;
					mousePos2.X = mousePos2.X * 320 * 2 / fullscreenRect.W;
					mousePos2.Y = mousePos2.Y * 240 * 2 / fullscreenRect.H;
					if (mousePos2.X >= 0 && mousePos2.X <= oneshotWindow.ContentsSize.X && mousePos2.Y >= 0 && mousePos2.Y <= oneshotWindow.ContentsSize.Y && mousePos2.X >= rect2.X && mousePos2.X <= rect2.X + rect2.W && mousePos2.Y >= rect2.Y && mousePos2.Y <= rect2.Y + rect2.H)
					{
						isMouseHoveringOnUs = true;
					}
				}
			}
			if (isMouseHoveringOnUs && Game1.mouseCursorMan.MouseClicked)
			{
				SetActivePage(0);
				oneshotWindow.tileMapMan.StartEvent(this);
			}
		}

		public override void Draw(Vec2 camPos, GameTone tone)
		{
			if (!active)
			{
				return;
			}
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X;
			zero.Y = pos.Y / 256 - camPos.Y;
			if (jumping)
			{
				int num = jumpCount - jumpPeak;
				zero.Y -= (jumpPeak * jumpPeak - num * num) / 4;
			}
			float num2 = (float)opacity / 255f;
			TWMTheme tWMTheme = Game1.windowMan.GetCurrentTheme();
			if (themeId != null)
			{
				tWMTheme = Game1.windowMan.GetThemeById(themeId);
			}
			if (!string.IsNullOrEmpty(npcSheet))
			{
				string textureName = "npc/" + npcSheet;
				Rect srcRect = new Rect(frameIndex * spriteSize.X, (int)(direction - 2) / 2 * spriteSize.Y, spriteSize.X, spriteSize.Y);
				zero.X -= spriteSize.X / 2;
				zero.Y -= spriteSize.Y - 8;
				if (isMouseHoveringOnUs)
				{
					GameColor gColor = tWMTheme.Primary((byte)(opacity / 2));
					GameColor gColor2 = tWMTheme.Background((byte)(opacity / 2));
					Rect boxRect = new Rect(pos.X / 256 - camPos.X - 12, pos.Y / 256 - camPos.Y - 10, 24, 20);
					Game1.gMan.ColorBoxBlit(boxRect, gColor);
					boxRect = boxRect.Shrink(1);
					Game1.gMan.ColorBoxBlit(boxRect, gColor2);
				}
				zero.X++;
				zero.Y++;
				Game1.gMan.MainBlit(textureName, zero, srcRect, num2, hue, blendMode, 2, tone, 0f, 0f, 0f);
				zero.X--;
				zero.Y--;
				GameColor gameColor = tWMTheme.Primary();
				if (oneshotWindow.menuMan.SettingsMenu.IsChromaAberrationEnabled)
				{
					Game1.gMan.MainBlit(textureName, zero + redOffset, srcRect, num2 / 2f, hue, GraphicsManager.BlendMode.Additive, 2, tone, gameColor.rf, 0f, 0f);
					Game1.gMan.MainBlit(textureName, zero + greenOffset, srcRect, num2 / 2f, hue, GraphicsManager.BlendMode.Additive, 2, tone, 0f, gameColor.gf, 0f);
					Game1.gMan.MainBlit(textureName, zero + blueOffset, srcRect, num2 / 2f, hue, GraphicsManager.BlendMode.Additive, 2, tone, 0f, 0f, gameColor.bf);
				}
				Game1.gMan.MainBlit(textureName, zero, srcRect, num2, hue, blendMode, 2, tone, gameColor.rf, gameColor.gf, gameColor.bf);
			}
		}
	}
}

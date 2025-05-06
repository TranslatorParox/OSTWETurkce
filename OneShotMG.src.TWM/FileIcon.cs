using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM.Filesystem;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class FileIcon
	{
		public delegate void DesktopIconAction();

		private string iconText1;

		private string iconText2;

		private TempTexture text1Texture;

		private TempTexture text2Texture;

		private int objectHighlightTimer;

		private const int OBJECT_HIGHLIGHT_PERIOD = 80;

		private const string baseIconPath = "the_world_machine/desktop_icons/";

		public const int ICON_TEXT_OFFSET = 36;

		public const int GRID_ITEM_W = 84;

		public const int GRID_ITEM_H = 72;

		public const int ITEM_MARGIN = 2;

		public static Vec2 ICON_OFFSET = new Vec2(26, 6);

		public static Vec2 WALLPAPER_ICON_OFFSET = new Vec2(21, 10);

		private const int TEXT_LEFT_MARGIN = 4;

		private const int MAX_TEXT_WIDTH = 76;

		public readonly string iconImagePath;

		public readonly TWMFileNode file;

		public Rect textOffset1;

		public Rect textOffset2;

		public int textHeight;

		public const GraphicsManager.FontType iconFont = GraphicsManager.FontType.OS;

		private GlitchEffect glitchEffect;

		public FileIcon(TWMFileNode file)
		{
			this.file = file;
			iconImagePath = "the_world_machine/desktop_icons/" + file.icon;
			processFileName();
			Vec2 size = new Vec2(32, 32);
			if (isWallpaperIcon())
			{
				size = new Vec2(84, 48);
			}
			glitchEffect = new GlitchEffect(size, 100, 900, 900, 20);
		}

		private void processFileName()
		{
			if (isWallpaperIcon())
			{
				iconText1 = Game1.languageMan.GetWallpaperLocString(file.name, file.name);
			}
			else if (isThemeIcon())
			{
				iconText1 = Game1.languageMan.GetThemesLocString(file.name, file.name);
			}
			else
			{
				iconText1 = Game1.languageMan.GetTWMLocString(file.name);
			}
			iconText2 = null;
			List<string> list = MathHelper.WordWrap(GraphicsManager.FontType.OS, iconText1, 76);
			if (list.Count <= 1)
			{
				iconText1 = MathHelper.Truncate(GraphicsManager.FontType.OS, iconText1, 76);
				Vec2 vec = Game1.gMan.TextSize(GraphicsManager.FontType.OS, iconText1);
				textOffset1 = new Rect((76 - vec.X) / 2 + 4, 36, vec.X + 2, 12);
				textHeight = 12;
			}
			else
			{
				iconText1 = MathHelper.Truncate(GraphicsManager.FontType.OS, list[0], 76);
				iconText1 = iconText1.Trim();
				Vec2 vec2 = Game1.gMan.TextSize(GraphicsManager.FontType.OS, iconText1);
				textOffset1 = new Rect((76 - vec2.X) / 2 + 4, 36, vec2.X + 2, 12);
				textHeight = 12;
				iconText2 = MathHelper.Truncate(GraphicsManager.FontType.OS, list[1], 76);
				vec2 = Game1.gMan.TextSize(GraphicsManager.FontType.OS, iconText2);
				textOffset2 = new Rect((76 - vec2.X) / 2 + 4, 36 + textHeight, vec2.X + 2, 12);
				textHeight += 12;
			}
			GenerateTextTempTextures();
		}

		private void GenerateTextTempTextures()
		{
			if (!string.IsNullOrEmpty(iconText1))
			{
				text1Texture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, iconText1);
				text1Texture.KeepAlive(3600);
			}
			if (!string.IsNullOrEmpty(iconText2))
			{
				text2Texture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, iconText2);
				text2Texture.KeepAlive(3600);
			}
		}

		public void Update()
		{
			if (Game1.windowMan.TutorialStep != TutorialStep.COMPLETE)
			{
				objectHighlightTimer++;
				if (objectHighlightTimer >= 80)
				{
					objectHighlightTimer = 0;
				}
			}
			if (Game1.windowMan.Desktop.inSolstice)
			{
				glitchEffect.Update();
			}
			if (text1Texture != null && !text1Texture.isValid)
			{
				GenerateTextTempTextures();
			}
			text1Texture?.KeepAlive();
			text2Texture?.KeepAlive();
		}

		public void Draw(TWMTheme theme, Vec2 pos, bool focus = false, bool canHover = false, float alpha = 1f)
		{
			if (isThemeIcon())
			{
				TWMFile tWMFile = file as TWMFile;
				theme = Game1.windowMan.GetThemeById(tWMFile.argument[0]);
			}
			GameColor gColor = theme.Background();
			GameColor gColor2 = (focus ? theme.Primary() : theme.Variant());
			GameColor gameColor = theme.Primary();
			GameColor gameColor2 = theme.Background();
			if (alpha != 1f)
			{
				gameColor.a = (gColor2.a = (gColor.a = (byte)(255f * alpha)));
			}
			if (Game1.windowMan.TutorialStep == TutorialStep.DRAG_FILE || (Game1.windowMan.TutorialStep == TutorialStep.MOVEFILE_DRAG_FILE && file is TWMFile tWMFile2 && tWMFile2.program == LaunchableWindowType.DUMMY_FILE_FOR_TUTORIALS) || Game1.windowMan.TutorialStep == TutorialStep.LAUNCH_APPLICATION || (Game1.windowMan.TutorialStep == TutorialStep.DELETE_CLICK_FILE && file is TWMFile tWMFile3 && tWMFile3.program == LaunchableWindowType.DUMMY_FILE_FOR_TUTORIALS))
			{
				float num = (float)Math.Abs(objectHighlightTimer - 40) / 40f;
				num *= 0.6f;
				num += 0.4f;
				gameColor.a = (byte)((float)(int)gameColor.a * num);
			}
			int borderSize = 2;
			Rect boxRect = ClickAreaForIcon(pos);
			if (canHover && boxRect.IsVec2InRect(Game1.mouseCursorMan.MousePos))
			{
				focus = true;
				Game1.mouseCursorMan.SetState(MouseCursorManager.State.Clickable);
			}
			if (focus)
			{
				gColor2.a /= 2;
				gColor.a /= 2;
				Game1.gMan.ColorBoxBlit(boxRect, gColor2);
				boxRect = boxRect.Shrink(borderSize);
				Game1.gMan.ColorBoxBlit(boxRect, gColor);
			}
			Vec2 vec = pos + ICON_OFFSET;
			if (isWallpaperIcon())
			{
				vec = pos + WALLPAPER_ICON_OFFSET;
				vec *= 2;
				if (Game1.windowMan.Desktop.inSolstice)
				{
					glitchEffect.Draw(iconImagePath, vec + new Vec2(2, 2), gameColor2.af, gameColor2.rf, gameColor2.gf, gameColor2.bf, GraphicsManager.BlendMode.Normal, TextureCache.CacheType.Game, 1);
					glitchEffect.Draw(iconImagePath, vec, 1f, 1f, 1f, 1f, GraphicsManager.BlendMode.Normal, TextureCache.CacheType.Game, 1);
				}
				else
				{
					Game1.gMan.MainBlit(iconImagePath, vec + new Vec2(2, 2), gameColor2, 0, GraphicsManager.BlendMode.Normal, 1);
					Game1.gMan.MainBlit(iconImagePath, vec, GameColor.White, 0, GraphicsManager.BlendMode.Normal, 1);
				}
			}
			else if (Game1.windowMan.Desktop.inSolstice)
			{
				glitchEffect.Draw(iconImagePath, vec + new Vec2(1, 1), gameColor2.af, gameColor2.rf, gameColor2.gf, gameColor2.bf, GraphicsManager.BlendMode.Normal, TextureCache.CacheType.TheWorldMachine);
				glitchEffect.Draw(iconImagePath, vec, gameColor.af, gameColor.rf, gameColor.gf, gameColor.bf, GraphicsManager.BlendMode.Normal, TextureCache.CacheType.TheWorldMachine);
			}
			else
			{
				Game1.gMan.MainBlit(iconImagePath, vec + new Vec2(1, 1), gameColor2, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
				Game1.gMan.MainBlit(iconImagePath, vec, gameColor, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
			}
			Vec2 vec2 = new Vec2(-1, 4);
			Rect boxRect2 = textOffset1.Translated(pos + vec2);
			Game1.gMan.ColorBoxBlit(boxRect2, GameColor.Black);
			Vec2 xY = boxRect2.XY;
			xY.X--;
			Game1.gMan.MainBlit(text1Texture, xY * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
			if (text2Texture != null && text2Texture.isValid)
			{
				Rect boxRect3 = textOffset2.Translated(pos + vec2);
				Vec2 xY2 = boxRect3.XY;
				xY2.X--;
				Game1.gMan.ColorBoxBlit(boxRect3, GameColor.Black);
				Game1.gMan.MainBlit(text2Texture, xY2 * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
			}
		}

		private bool isWallpaperIcon()
		{
			if (file is TWMFile tWMFile)
			{
				return tWMFile.program == LaunchableWindowType.WALLPAPER;
			}
			return false;
		}

		private bool isThemeIcon()
		{
			if (file is TWMFile tWMFile)
			{
				return tWMFile.program == LaunchableWindowType.THEME;
			}
			return false;
		}

		public Rect ClickAreaForIcon(Vec2 pos)
		{
			return new Rect(pos.X + 2, pos.Y + 2, 80, 36 + textHeight + 2 + 4);
		}
	}
}

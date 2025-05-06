using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OneShotMG.src.Util;

namespace OneShotMG.src.EngineSpecificCode
{
	public class TempTextureManager
	{
		private List<TempTexture> tempTextures;

		private Game monoGame;

		private List<TempTexture> texturesToDestroy = new List<TempTexture>();

		public TempTextureManager(Game game)
		{
			monoGame = game;
			tempTextures = new List<TempTexture>();
		}

		public TempTexture GetSingleLineTexture(GraphicsManager.FontType fontType, string text, int maxWidth = -1)
		{
			GameColor white = GameColor.White;
			int num = Game1.gMan.TextSize(fontType, text, checkForGlyphes: true).X;
			if (num % 2 == 1)
			{
				num--;
			}
			Vec2 pixelPos = new Vec2(2, 0);
			Vec2 size = Vec2.Zero;
			int num2 = 1;
			switch (fontType)
			{
			case GraphicsManager.FontType.Game:
				size = new Vec2(num + 4, 24);
				break;
			case GraphicsManager.FontType.OS:
				num2 = 2;
				pixelPos.Y = -4;
				size = new Vec2((num + 4) * 2, 24);
				break;
			}
			TempTexture tempTexture = Game1.gMan.TempTexMan.GetTempTexture(size);
			Game1.gMan.BeginDrawToTempTexture(tempTexture);
			Game1.gMan.TextBlit(fontType, pixelPos, text, white, GraphicsManager.BlendMode.Normal, num2, GraphicsManager.TextBlitMode.OnlyText);
			Game1.gMan.EndDrawToTempTexture();
			if (maxWidth > 0 && size.X > maxWidth * num2)
			{
				TempTexture tempTexture2 = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(maxWidth * num2, size.Y));
				Game1.gMan.BeginDrawToTempTexture(tempTexture2, clearTexture: true, smooth: true);
				float xScale = (float)(maxWidth * num2) / (float)size.X;
				Game1.gMan.MainBlitStretch(tempTexture, Vec2.Zero, GameColor.White, 0, GraphicsManager.BlendMode.Normal, xScale);
				Game1.gMan.EndDrawToTempTexture();
				tempTexture.framesLeftToPersist = 0;
				return tempTexture2;
			}
			return tempTexture;
		}

		public TempTexture GetTempTexture(Vec2 size)
		{
			if (size.X % 2 == 1)
			{
				size.X++;
			}
			if (size.Y % 2 == 1)
			{
				size.Y++;
			}
			TempTexture tempTexture = new TempTexture();
			tempTexture.renderTarget = new RenderTarget2D(monoGame.GraphicsDevice, size.X, size.Y, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			tempTexture.isValid = true;
			tempTexture.framesLeftToPersist = 5;
			tempTextures.Add(tempTexture);
			Game1.logMan.Log(LogManager.LogLevel.Info, $"Created temp tex size {size.X},{size.Y}");
			return tempTexture;
		}

		public void Update()
		{
			texturesToDestroy.Clear();
			foreach (TempTexture tempTexture in tempTextures)
			{
				tempTexture.framesLeftToPersist--;
				if (tempTexture.framesLeftToPersist <= 0)
				{
					texturesToDestroy.Add(tempTexture);
				}
			}
			foreach (TempTexture item in texturesToDestroy)
			{
				Game1.logMan.Log(LogManager.LogLevel.Info, $"Deleting temp tex size {item.renderTarget.Width},{item.renderTarget.Height}");
				item.isValid = false;
				item.renderTarget.Dispose();
				item.renderTarget = null;
				tempTextures.Remove(item);
			}
		}
	}
}

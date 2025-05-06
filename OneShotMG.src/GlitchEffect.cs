using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class GlitchEffect
	{
		private Vec2 size;

		private int glitchStrength = 10;

		private int timeBetweenGlitches = 60;

		private int timeVariance = 60;

		private int glitchTime = 20;

		private int glitchOverallTimer;

		private int glitchCycleTimer;

		private const int GLITCH_CYCLE_TIME = 5;

		private List<Vec2> glitchSegmentHW;

		private List<GameColor> glitchSegmentColors;

		private int glitchNextTimer = 60;

		public GlitchEffect(Vec2 size, int glitchStrength, int timeBetweenGlitches, int timeVariance, int glitchTime)
		{
			this.size = size;
			this.glitchStrength = glitchStrength;
			if (this.glitchStrength > size.X)
			{
				this.glitchStrength = size.X;
			}
			this.timeBetweenGlitches = timeBetweenGlitches;
			this.timeVariance = timeVariance;
			this.glitchTime = glitchTime;
			glitchNextTimer = OneShotMG.src.Util.MathHelper.Random(0, timeBetweenGlitches + timeVariance);
		}

		public void Update()
		{
			if (glitchOverallTimer > 0)
			{
				glitchOverallTimer--;
				glitchCycleTimer++;
				if (glitchCycleTimer >= 5)
				{
					glitchCycleTimer = 0;
					generateGlitchSegments();
				}
				return;
			}
			glitchOverallTimer = 0;
			glitchNextTimer--;
			if (glitchNextTimer <= 0)
			{
				glitchNextTimer = OneShotMG.src.Util.MathHelper.Random(timeBetweenGlitches, timeBetweenGlitches + timeVariance);
				glitchOverallTimer = glitchTime;
				glitchCycleTimer = 0;
				generateGlitchSegments();
			}
		}

		private void generateGlitchSegments()
		{
			glitchSegmentHW = new List<Vec2>();
			glitchSegmentColors = new List<GameColor>();
			int num = size.Y;
			while (num > 0)
			{
				Vec2 item = new Vec2(generateSegmentWidth(), OneShotMG.src.Util.MathHelper.Random(size.Y / 4, size.Y / 2));
				if (item.Y > num)
				{
					item.Y = num;
				}
				glitchSegmentHW.Add(item);
				glitchSegmentColors.Add(generateGlitchColor());
				num -= item.Y;
			}
		}

		private GameColor generateGlitchColor()
		{
			Vector3 col = new Vector3(1f, OneShotMG.src.Util.MathHelper.FRandom(0.75f, 1f), 1f);
			col = OneShotMG.src.Util.MathHelper.ApplyHue(col, OneShotMG.src.Util.MathHelper.FRandom(0f, (float)Math.PI * 2f));
			col.X = Math.Max(0f, Math.Min(1f, col.X));
			col.Y = Math.Max(0f, Math.Min(1f, col.Y));
			col.Z = Math.Max(0f, Math.Min(1f, col.Z));
			return new GameColor((byte)(col.X * 255f), (byte)(col.Y * 255f), (byte)(col.Z * 255f), byte.MaxValue);
		}

		private int generateSegmentWidth()
		{
			int i;
			for (i = OneShotMG.src.Util.MathHelper.Random(-glitchStrength, glitchStrength); i < 0; i += size.X)
			{
			}
			while (i >= size.X)
			{
				i -= size.X;
			}
			return i;
		}

		public void Draw(string textureName, Vec2 pos, float alpha = 1f, float red = 1f, float green = 1f, float blue = 1f, GraphicsManager.BlendMode blendMode = GraphicsManager.BlendMode.Normal, TextureCache.CacheType cacheType = TextureCache.CacheType.Game, int scale = 2)
		{
			if (glitchOverallTimer > 0 && glitchSegmentHW != null && glitchSegmentColors != null)
			{
				Vec2 vec = pos;
				int num = 0;
				for (int i = 0; i < glitchSegmentHW.Count; i++)
				{
					Vec2 vec2 = glitchSegmentHW[i];
					GameColor gameColor = glitchSegmentColors[i];
					float num2 = (float)(int)gameColor.r / 255f * red;
					float num3 = (float)(int)gameColor.g / 255f * green;
					float num4 = (float)(int)gameColor.b / 255f * blue;
					GraphicsManager gMan = Game1.gMan;
					Vec2 pixelPos = vec;
					Rect srcRect = new Rect(vec2.X, num, size.X - vec2.X, vec2.Y);
					float red2 = num2;
					float blue2 = num4;
					float green2 = num3;
					TextureCache.CacheType cacheType2 = cacheType;
					gMan.MainBlit(textureName, pixelPos, srcRect, alpha, 0, blendMode, scale, default(GameTone), red2, green2, blue2, 0f, cacheType2);
					vec.X += size.X - vec2.X;
					GraphicsManager gMan2 = Game1.gMan;
					Vec2 pixelPos2 = vec;
					Rect srcRect2 = new Rect(0, num, vec2.X, vec2.Y);
					green2 = num2;
					blue2 = num4;
					red2 = num3;
					cacheType2 = cacheType;
					gMan2.MainBlit(textureName, pixelPos2, srcRect2, alpha, 0, blendMode, scale, default(GameTone), green2, red2, blue2, 0f, cacheType2);
					num += vec2.Y;
					vec.X = pos.X;
					vec.Y += vec2.Y;
				}
			}
			else
			{
				GraphicsManager gMan3 = Game1.gMan;
				Vec2 pixelPos3 = pos;
				float red2 = red;
				float blue2 = green;
				float green2 = blue;
				TextureCache.CacheType cacheType2 = cacheType;
				gMan3.MainBlit(textureName, pixelPos3, alpha, 0, blendMode, scale, default(GameTone), red2, blue2, green2, cacheType2);
			}
		}
	}
}

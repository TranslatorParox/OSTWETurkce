using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class HintBox
	{
		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private const int DRIFT_TIME = 360;

		private const int DRIFT_MAX = 6;

		private const float DRIFT_TIMESCALE = 2f;

		private const int QUADRANT_DEADZONE = 30;

		private const byte ALPHA_STEP = 5;

		private const int TEXT_OFFSET = 6;

		private const int BORDER_SIZE = 2;

		private const int MARGIN = 4;

		private const int MAX_WIDTH = 400;

		private const int LINE_HEIGHT = 32;

		private const int SCREEN_SAFETY_MARGIN = 20;

		public Vec2 ScreenCenter;

		public Func<Vec2> GetTargetPos;

		private Vec2 targetPos;

		public Action OnFadeOutComplete;

		private readonly Vec2 boxSize;

		private Vec2 targetOffset;

		private int driftTimer;

		private Vec2 lastQuadrant;

		private Vector2 currentOffset;

		private byte alpha;

		private List<string> lines;

		private List<TempTexture> lineTextures;

		public bool FadeOut;

		public HintBox(Vec2 screenCenter, string text)
		{
			ScreenCenter = screenCenter;
			lineTextures = new List<TempTexture>();
			int x = 412;
			lines = OneShotMG.src.Util.MathHelper.WordWrap(GraphicsManager.FontType.OS, text, 400);
			if (lines.Count <= 1)
			{
				x = Game1.gMan.TextSize(GraphicsManager.FontType.OS, lines[0], checkForGlyphes: true).X + 12;
			}
			foreach (string line in lines)
			{
				lineTextures.Add(Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, line));
			}
			boxSize = new Vec2(x, 12 + 32 * lines.Count);
		}

		public void Update()
		{
			foreach (TempTexture lineTexture in lineTextures)
			{
				lineTexture.KeepAlive();
			}
			if (GetTargetPos == null)
			{
				return;
			}
			if (FadeOut && alpha > 0)
			{
				alpha = (byte)Math.Max(0, alpha - 5);
				if (alpha == 0)
				{
					OnFadeOutComplete?.Invoke();
				}
			}
			else if (alpha < byte.MaxValue)
			{
				alpha = (byte)Math.Min(255, alpha + 5);
			}
			if (!FadeOut)
			{
				Vec2 vec = GetTargetPos();
				if (vec.X != 0 || vec.Y != 0)
				{
					targetPos = vec;
				}
			}
			Vec2 vec2 = new Vec2(25, 25) * TargetQuadrant(targetPos);
			if (vec2.X < 0)
			{
				vec2.X -= boxSize.X;
			}
			if (vec2.Y < 0)
			{
				vec2.Y -= boxSize.Y;
			}
			targetOffset = vec2 + GetDrift(driftTimer);
			Vec2 vec3 = Game1.gMan.DrawScreenSize / 2;
			if (targetOffset.X + targetPos.X < 20)
			{
				targetOffset.X = 20 - targetPos.X;
			}
			else if (targetOffset.X + targetPos.X > vec3.X - 20 - boxSize.X)
			{
				targetOffset.X = vec3.X - 20 - boxSize.X - targetPos.X;
			}
			Vector2 vector = new Vector2((float)targetOffset.X - currentOffset.X, (float)targetOffset.Y - currentOffset.Y);
			float num = ((vector.Length() > 0f) ? 0.12f : 0.02f);
			currentOffset += vector * num;
			if (++driftTimer > 360)
			{
				driftTimer = 0;
			}
		}

		public void Draw(TWMTheme theme)
		{
			if (GetTargetPos != null)
			{
				GameColor gColor = theme.Background(alpha);
				GameColor gameColor = theme.Primary(alpha);
				Vec2 vec = targetPos + new Vec2((int)currentOffset.X, (int)currentOffset.Y);
				Rect boxRect = new Rect(vec.X, vec.Y, boxSize.X, boxSize.Y);
				Game1.gMan.ColorBoxBlit(boxRect, gameColor);
				Game1.gMan.ColorBoxBlit(boxRect.Shrink(2), gColor);
				Vec2 vec2 = boxRect.XY + new Vec2(6, 12);
				for (int i = 0; i < lines.Count; i++)
				{
					Game1.gMan.MainBlit(lineTextures[i], (vec2 + new Vec2(-2, 4)) * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
					Game1.gMan.TextBlit(GraphicsManager.FontType.OS, vec2, lines[i], gameColor, GraphicsManager.BlendMode.Normal, 2, GraphicsManager.TextBlitMode.OnlyGlyphes);
					vec2.Y += 32;
				}
			}
		}

		private Vec2 TargetQuadrant(Vec2 targetPos)
		{
			Vec2 vec = targetPos - ScreenCenter;
			Vec2 result = lastQuadrant;
			if (result.X == 0)
			{
				result.X = ((vec.X < 0) ? 1 : (-1));
			}
			if (result.Y == 0)
			{
				result.Y = ((vec.Y < 0) ? 1 : (-1));
			}
			if (FadeOut)
			{
				return result;
			}
			if (vec.X > 30)
			{
				result.X = -1;
			}
			else if (vec.X < -30)
			{
				result.X = 1;
			}
			if (vec.Y > 30)
			{
				result.Y = -1;
			}
			else if (vec.Y < -30)
			{
				result.Y = 1;
			}
			lastQuadrant = result;
			return result;
		}

		private Vec2 GetDrift(int timer)
		{
			return new Vec2(0, (int)(Math.Sin((double)timer * Math.PI / 180.0 * 2.0) * 6.0));
		}

		public bool IsFadeInComplete()
		{
			if (alpha != byte.MaxValue)
			{
				return FadeOut;
			}
			return true;
		}
	}
}

using System;
using Microsoft.Xna.Framework;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class MusicNote
	{
		private const int NOTE_HEIGHT = 16;

		private const int NOTE_WIDTH = 16;

		private const int PIXEL_UNITS = 80;

		private const int WAVE_MAGNITUDE = 640;

		private int frame;

		private Vec2 pos;

		private Vec2 vel;

		private float xWavePos;

		private float yWavePos;

		private float waveSpeed = 0.2f;

		private int alpha = 255;

		private GameColor noteColor;

		public MusicNote(Vec2 spawnPos)
		{
			frame = OneShotMG.src.Util.MathHelper.Random(0, 2);
			pos = spawnPos * 80;
			vel = new Vec2(OneShotMG.src.Util.MathHelper.Random(-100, -10), OneShotMG.src.Util.MathHelper.Random(-100, -75));
			xWavePos = OneShotMG.src.Util.MathHelper.FRandom(0f, (float)Math.PI * 2f);
			yWavePos = OneShotMG.src.Util.MathHelper.FRandom(0f, (float)Math.PI * 2f);
			waveSpeed = OneShotMG.src.Util.MathHelper.FRandom(0.075f, 0.11f);
			pos.X -= (int)(Math.Sin(xWavePos) * 640.0);
			pos.Y -= (int)(Math.Sin(yWavePos) * 640.0);
			Vector3 col = new Vector3(1f, 0.75f, 0.75f);
			col = OneShotMG.src.Util.MathHelper.ApplyHue(col, OneShotMG.src.Util.MathHelper.FRandom(0f, (float)Math.PI * 2f));
			col.X = Math.Max(0f, Math.Min(1f, col.X));
			col.Y = Math.Max(0f, Math.Min(1f, col.Y));
			col.Z = Math.Max(0f, Math.Min(1f, col.Z));
			noteColor = new GameColor((byte)(col.X * 255f), (byte)(col.Y * 255f), (byte)(col.Z * 255f), byte.MaxValue);
		}

		public bool IsAlive()
		{
			return alpha > 0;
		}

		public void Update()
		{
			pos += vel;
			xWavePos += waveSpeed;
			if ((double)xWavePos >= Math.PI * 2.0)
			{
				xWavePos -= (float)Math.PI * 2f;
			}
			yWavePos += waveSpeed;
			if ((double)yWavePos >= Math.PI * 2.0)
			{
				yWavePos -= (float)Math.PI * 2f;
			}
			alpha -= 2;
			if (alpha < 0)
			{
				alpha = 0;
			}
		}

		public void Draw(Vec2 windowPos)
		{
			Vec2 vec = pos;
			vec.X += (int)(Math.Sin(xWavePos) * 640.0);
			vec.Y += (int)(Math.Sin(yWavePos) * 640.0);
			vec /= 80;
			vec += windowPos;
			vec.X -= 8;
			vec.Y -= 8;
			Rect srcRect = new Rect(frame * 16, 0, 16, 16);
			GameColor gameColor = noteColor;
			gameColor.a = (byte)alpha;
			GameColor black = GameColor.Black;
			black.a = (byte)alpha;
			Game1.gMan.MainBlit("the_world_machine/jukebox/notes", vec + new Vec2(1, 1), srcRect, black, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
			Game1.gMan.MainBlit("the_world_machine/jukebox/notes", vec, srcRect, gameColor, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
		}
	}
}

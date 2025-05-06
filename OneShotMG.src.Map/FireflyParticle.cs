using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.Map
{
	public class FireflyParticle
	{
		private Vec2 pos;

		private Vec2 vel;

		private float size = 1f;

		private Vec2 scaledSpriteSize;

		private Rect drawRect;

		private int phase;

		private int wavelength;

		private float alpha;

		private const string SPRITE_NAME = "pictures/firefly";

		public FireflyParticle()
		{
			size = MathHelper.FRandom(0.25f, 1f);
			Vec2 vec = Game1.gMan.TextureSize("pictures/firefly");
			drawRect = new Rect(0, 0, vec.X, vec.Y);
			scaledSpriteSize = new Vec2((int)((float)vec.X * size), (int)((float)vec.Y * size));
			vel = new Vec2(MathHelper.Random(-512, 512), MathHelper.Random(-512, 512));
			pos = new Vec2(MathHelper.Random(0, 163840), MathHelper.Random(0, 122880));
			wavelength = MathHelper.Random(120, 240);
			phase = MathHelper.Random(0, wavelength);
			Update(Vec2.Zero);
		}

		public void Update(Vec2 camDelta)
		{
			float num = size * (2f / 3f) + 5f / 6f;
			pos.X += vel.X - (int)((float)(camDelta.X * 2 * 256) * num);
			pos.Y += vel.Y - (int)((float)(camDelta.Y * 2 * 256) * num);
			if (pos.X > 163840)
			{
				pos.X -= (640 + scaledSpriteSize.X) * 256;
			}
			else if (pos.X < -scaledSpriteSize.X * 256)
			{
				pos.X += (640 + scaledSpriteSize.X) * 256;
			}
			if (pos.Y > 122880)
			{
				pos.Y -= (480 + scaledSpriteSize.Y) * 256;
			}
			else if (pos.Y < -scaledSpriteSize.Y * 256)
			{
				pos.Y += (480 + scaledSpriteSize.Y) * 256;
			}
			phase++;
			if (phase >= wavelength)
			{
				phase = 0;
			}
			alpha = (float)Math.Sin((double)((float)phase / (float)wavelength) * Math.PI);
		}

		public void Draw()
		{
			Vec2 pixelPos = new Vec2(pos.X / 256, pos.Y / 256);
			Game1.gMan.MainBlit("pictures/firefly", pixelPos, drawRect, size, size, alpha, 0, GraphicsManager.BlendMode.Additive);
		}
	}
}

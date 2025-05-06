using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class SpinningBulbIcon
	{
		private int frameTimer;

		private int frameIndex;

		private const int FRAME_TIME = 3;

		private const int TOTAL_FRAMES = 24;

		public const int BULB_ANIM_WIDTH = 48;

		public const int BULB_ANIM_HEIGHT = 48;

		private const int BULB_ANIM_SHEET_COLUMNS = 5;

		private Rect bulbRect;

		private byte lightOpacity = 128;

		private int lightCycle;

		private const int LIGHT_PERIOD = 60;

		private const byte LIGHT_LUMINANCE_MIN = 128;

		private const byte LIGHT_LUMINANCE_MAX = byte.MaxValue;

		public SpinningBulbIcon()
		{
			bulbRect = new Rect(0, 0, 48, 48);
		}

		public void Update()
		{
			frameTimer++;
			if (frameTimer >= 3)
			{
				frameTimer = 0;
				frameIndex++;
				if (frameIndex >= 24)
				{
					frameIndex = 0;
				}
			}
			int num = frameIndex % 5;
			int num2 = frameIndex / 5;
			bulbRect.X = 48 * num;
			bulbRect.Y = 48 * num2;
			lightCycle++;
			if (lightCycle >= 60)
			{
				lightCycle = 0;
			}
			double num3 = (Math.Sin((double)((float)lightCycle / 60f) * Math.PI * 2.0) + 1.0) / 2.0;
			lightOpacity = (byte)(128.0 + 127.0 * num3);
		}

		public void Draw(Vec2 drawPos, byte alpha, int scale = 2)
		{
			GameColor white = GameColor.White;
			white.a = alpha;
			Game1.gMan.MainBlit("the_world_machine/bulb_anim_24", drawPos + new Vec2(1, 1), bulbRect, new GameColor(0, 0, 0, alpha), 0, GraphicsManager.BlendMode.Normal, scale, TextureCache.CacheType.TheWorldMachine);
			Game1.gMan.MainBlit("the_world_machine/bulb_anim_24", drawPos, bulbRect, white, 0, GraphicsManager.BlendMode.Normal, scale, TextureCache.CacheType.TheWorldMachine);
			white.a = (byte)(lightOpacity * alpha / 255);
			Game1.gMan.MainBlit("the_world_machine/bulb_anim_24_lightmap", drawPos, bulbRect, white, 0, GraphicsManager.BlendMode.Additive, scale, TextureCache.CacheType.TheWorldMachine);
		}
	}
}

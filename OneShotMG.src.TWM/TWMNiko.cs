using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class TWMNiko
	{
		private Vec2 pos;

		private int[] frameIdArray = new int[4] { 1, 2, 3, 2 };

		private int frameIndex;

		private int frameTimer;

		private bool movedLastFrame;

		private bool isSolstice;

		private bool isEntityNiko;

		private const int FRAME_TIME = 16;

		public bool Active { get; private set; } = true;

		public TWMNiko(Vec2 pos, bool isSolstice, bool isEntityNiko)
		{
			this.pos = pos;
			this.isSolstice = isSolstice;
			this.isEntityNiko = isEntityNiko;
		}

		public void Update()
		{
			if (!movedLastFrame)
			{
				pos.Y++;
				movedLastFrame = true;
			}
			else
			{
				movedLastFrame = false;
			}
			frameTimer++;
			if (frameTimer >= 16)
			{
				frameTimer = 0;
				frameIndex++;
				if (frameIndex >= frameIdArray.Length)
				{
					frameIndex = 0;
				}
			}
			if (pos.Y > Game1.windowMan.ScreenSize.Y)
			{
				Active = false;
			}
		}

		public void Draw()
		{
			if (!isSolstice)
			{
				if (isEntityNiko)
				{
					Game1.gMan.MainBlit("the_world_machine/journal/niko/en" + frameIdArray[frameIndex], pos, 1f, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.TheWorldMachine);
				}
				else
				{
					Game1.gMan.MainBlit("the_world_machine/journal/niko/niko" + frameIdArray[frameIndex], pos, 1f, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.TheWorldMachine);
				}
				Game1.gMan.MainBlit("the_world_machine/journal/niko/niko_lightmap" + frameIdArray[frameIndex], pos * 2, 1f, 0, GraphicsManager.BlendMode.Additive, 1, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.TheWorldMachine);
			}
			else
			{
				Game1.gMan.MainBlit("the_world_machine/journal/niko/niko_finale" + frameIdArray[frameIndex], pos, 1f, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.TheWorldMachine);
			}
		}
	}
}

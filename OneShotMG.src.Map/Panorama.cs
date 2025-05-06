using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.Map
{
	public class Panorama
	{
		public enum Type
		{
			Normal,
			Clamped,
			AnimatedWater,
			Scroll
		}

		protected string sourceImageName;

		protected Vec2 size;

		protected string backImageName;

		protected string frontImageName;

		protected int frontAlpha;

		protected int frameIndex;

		private Type type;

		public bool clamped_x;

		public bool clamped_y;

		public int offset_y;

		private Vec2 scrollOffset = new Vec2(0, 0);

		private const int scrollTicksPerPixel = 4;

		public Panorama(string imageName, Vec2 size)
		{
			sourceImageName = imageName;
			this.size = size;
			switch (imageName.Substring(imageName.IndexOf('/') + 1).ToLowerInvariant())
			{
			case "red":
			case "red_distort":
				type = Type.Clamped;
				break;
			case "codebg":
				type = Type.Scroll;
				break;
			case "green_water":
			case "dark_water":
				type = Type.AnimatedWater;
				backImageName = sourceImageName + "3";
				frontImageName = sourceImageName + "1";
				break;
			case "skyline":
				type = Type.Normal;
				offset_y = 32;
				break;
			default:
				type = Type.Normal;
				break;
			}
		}

		public void Update()
		{
			switch (type)
			{
			case Type.AnimatedWater:
				frontAlpha += 3;
				if (frontAlpha > 255)
				{
					frontAlpha %= 255;
					frameIndex = (frameIndex + 1) % 3;
					backImageName = frontImageName;
					frontImageName = $"{sourceImageName}{frameIndex + 1}";
				}
				break;
			case Type.Scroll:
				scrollOffset.X++;
				scrollOffset.X %= size.X * 4;
				scrollOffset.Y++;
				scrollOffset.Y %= size.Y * 4;
				break;
			}
		}

		public PanoramaSaveData GetSaveData()
		{
			return new PanoramaSaveData
			{
				name = sourceImageName,
				size = size
			};
		}

		public virtual void Draw(Vec2 camPos, Vec2 playerPos, Vec2 mapSize, Vec2 tileSize, GameTone tone)
		{
			Vec2 zero = Vec2.Zero;
			switch (type)
			{
			case Type.Scroll:
				zero.X = -scrollOffset.X / 4;
				zero.Y = -scrollOffset.Y / 4;
				zero.X -= camPos.X;
				zero.Y -= camPos.Y;
				zero.X %= size.X;
				zero.Y %= size.Y;
				Game1.gMan.FillScreen(sourceImageName, zero, new Rect(0, 0, size.X, size.Y), 1f, GraphicsManager.BlendMode.Normal, tone);
				break;
			case Type.Clamped:
				zero.X = -(playerPos.X * (size.X - 320)) / (mapSize.X * tileSize.X);
				zero.Y = -(playerPos.Y * (size.Y - 240)) / (mapSize.Y * tileSize.Y);
				if (zero.X > 0)
				{
					zero.X -= size.X;
				}
				if (zero.Y > 0)
				{
					zero.Y -= size.Y;
				}
				Game1.gMan.FillScreen(sourceImageName, zero, new Rect(0, 0, size.X, size.Y), 1f, GraphicsManager.BlendMode.Normal, tone);
				break;
			case Type.Normal:
				zero.X = -camPos.X / 2;
				zero.Y = -camPos.Y / 2;
				zero.Y += offset_y;
				zero.X %= size.X;
				zero.Y %= size.Y;
				if (zero.X > 0)
				{
					zero.X -= size.X;
				}
				if (zero.Y > 0)
				{
					zero.Y -= size.Y;
				}
				Game1.gMan.FillScreen(sourceImageName, zero, new Rect(0, 0, size.X, size.Y), 1f, GraphicsManager.BlendMode.Normal, tone);
				break;
			case Type.AnimatedWater:
				zero.X = -camPos.X;
				zero.Y = -camPos.Y;
				zero.X %= size.X;
				zero.Y %= size.Y;
				if (zero.X > 0)
				{
					zero.X -= size.X;
				}
				if (zero.Y > 0)
				{
					zero.Y -= size.Y;
				}
				Game1.gMan.FillScreen(backImageName, zero, new Rect(0, 0, size.X, size.Y), 1f, GraphicsManager.BlendMode.Normal, tone);
				Game1.gMan.FillScreen(frontImageName, zero, new Rect(0, 0, size.X, size.Y), (float)frontAlpha / 255f, GraphicsManager.BlendMode.Normal, tone);
				break;
			}
		}
	}
}

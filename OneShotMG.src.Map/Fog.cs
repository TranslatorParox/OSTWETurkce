using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.Map
{
	public class Fog
	{
		protected string sourceImageName;

		protected Vec2 size;

		private int hue;

		private float opacity;

		private GraphicsManager.BlendMode blendMode;

		private float scrollX;

		private float scrollY;

		private float offsetX;

		private float offsetY;

		public Fog(string imageName, Vec2 fogSize, int fogHue, float fogOpacity, GraphicsManager.BlendMode fogBlendMode, int fogScrollX, int fogScrollY)
		{
			sourceImageName = imageName;
			size = fogSize;
			hue = fogHue;
			opacity = fogOpacity;
			blendMode = fogBlendMode;
			scrollX = (float)fogScrollX / 8f;
			scrollY = (float)fogScrollY / 8f;
		}

		public void Update()
		{
			offsetX += scrollX;
			offsetY += scrollY;
			if (offsetX < 0f)
			{
				offsetX += size.X;
			}
			else if (offsetX > (float)size.X)
			{
				offsetX -= size.X;
			}
			if (offsetY < 0f)
			{
				offsetY += size.Y;
			}
			else if (offsetY > (float)size.Y)
			{
				offsetY -= size.Y;
			}
		}

		public virtual void Draw(Vec2 camPos, GameTone tone)
		{
			Vec2 zero = Vec2.Zero;
			zero.X = -camPos.X * 2;
			zero.Y = -camPos.Y * 2;
			zero.X += (int)offsetX;
			zero.Y += (int)offsetY;
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
			Game1.gMan.FillScreen(sourceImageName, zero, new Rect(0, 0, size.X, size.Y), opacity, blendMode, tone, hue, 1);
		}
	}
}

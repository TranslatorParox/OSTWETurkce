using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class Footprint : Entity
	{
		private Rect drawRect;

		public Footprint(OneshotWindow osWindow, Direction d, Vec2 p)
			: base(osWindow)
		{
			id = -1;
			direction = d;
			opacity = 255;
			alwaysOnBottom = true;
			base.NeverHash = true;
			pos = p;
			oldPos = pos;
			collisionRect = new Rect(0, 0, 0, 0);
			ignoreMapCollision = true;
			ignoreNpcCollision = true;
			eventData = new Event();
			drawRect = new Rect(0, 0, 16, 16);
			switch (direction)
			{
			case Direction.Down:
				drawRect.Y = 0;
				break;
			case Direction.Left:
				drawRect.Y = 16;
				break;
			case Direction.Right:
				drawRect.Y = 32;
				break;
			case Direction.Up:
				drawRect.Y = 64;
				break;
			case (Direction)3:
			case (Direction)5:
			case (Direction)7:
				break;
			}
		}

		public override void Update()
		{
			opacity -= 4;
			if (opacity <= 0)
			{
				opacity = 0;
				KillEntityAfterUpdate = true;
			}
		}

		public override void Draw(Vec2 camPos, GameTone tone)
		{
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X - 8;
			zero.Y = pos.Y / 256 - camPos.Y - 8;
			float alpha = (float)opacity / 255f;
			Game1.gMan.MainBlit("footprints/footprints", zero, drawRect, alpha, 0, GraphicsManager.BlendMode.Normal, 2, tone);
		}
	}
}

using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class Footsplash : Entity
	{
		public Footsplash(OneshotWindow osWindow, Direction d, Vec2 p)
			: base(osWindow)
		{
			id = -1;
			opacity = 255;
			alwaysOnBottom = true;
			frameIndex = 0;
			base.NeverHash = true;
			pos = p;
			oldPos = pos;
			pos.Y += 1536;
			collisionRect = new Rect(0, 0, 0, 0);
			ignoreMapCollision = true;
			ignoreNpcCollision = true;
			eventData = new Event();
			switch (d)
			{
			case Direction.Down:
				pos.Y -= 1024;
				break;
			case Direction.Left:
				pos.X += 1024;
				break;
			case Direction.Right:
				pos.X -= 1024;
				break;
			case Direction.Up:
				pos.Y += 1024;
				break;
			case (Direction)3:
			case (Direction)5:
			case (Direction)7:
				break;
			}
		}

		public override void Update()
		{
			opacity -= 6;
			frameIndex = 20 - opacity / 13;
			if (opacity <= 0)
			{
				opacity = 0;
				KillEntityAfterUpdate = true;
			}
		}

		public override void Draw(Vec2 camPos, GameTone tone)
		{
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X - 40;
			zero.Y = pos.Y / 256 - camPos.Y - 40;
			float alpha = (float)opacity / 255f;
			Rect srcRect = new Rect(frameIndex % 4 * 80, frameIndex / 4 * 80, 80, 80);
			Game1.gMan.MainBlit("footprints/foot_splash", zero, srcRect, alpha, 0, GraphicsManager.BlendMode.Normal, 2, tone);
		}
	}
}

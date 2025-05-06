using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class Moodbubble : Entity
	{
		public enum BubbleType
		{
			Question = 1,
			Exclamation,
			Love,
			Sweat,
			Frustration
		}

		private BubbleType bubbleType;

		private Rect drawRect;

		private Entity parent;

		private int parentOffsetY;

		public Moodbubble(OneshotWindow osWindow, BubbleType bType, Entity parentE)
			: base(osWindow)
		{
			id = -1;
			alwaysOnTop = true;
			frameTimer = 0;
			base.NeverHash = true;
			bubbleType = bType;
			parent = parentE;
			parentOffsetY = -(parent.GetSpriteSize().Y / 4 + 24);
			pos.X = parent.GetPos().X;
			pos.Y = parent.GetPos().Y + parentOffsetY * 256;
			collisionRect = new Rect(0, 0, 0, 0);
			ignoreMapCollision = true;
			ignoreNpcCollision = true;
			eventData = new Event();
			drawRect = new Rect(0, 0, 16, 16);
			switch (bubbleType)
			{
			case BubbleType.Question:
				drawRect.Y = 0;
				break;
			case BubbleType.Exclamation:
				drawRect.Y = 16;
				break;
			case BubbleType.Love:
				drawRect.Y = 80;
				break;
			case BubbleType.Sweat:
				drawRect.Y = 96;
				break;
			case BubbleType.Frustration:
				drawRect.Y = 32;
				break;
			}
		}

		public override void Update()
		{
			oldPos = pos;
			frameTimer++;
			switch (frameTimer)
			{
			case 2:
				parentOffsetY -= 3;
				break;
			case 4:
				parentOffsetY -= 2;
				break;
			case 6:
				parentOffsetY--;
				break;
			case 10:
				parentOffsetY++;
				break;
			case 12:
				parentOffsetY += 2;
				break;
			case 14:
				parentOffsetY += 3;
				break;
			}
			pos.X = parent.GetPos().X;
			pos.Y = parent.GetPos().Y + parentOffsetY * 256;
			if (bubbleType == BubbleType.Frustration)
			{
				drawRect.Y = 32 + frameTimer / 4 % 3 * 16;
			}
			if (frameTimer > 40)
			{
				KillEntityAfterUpdate = true;
			}
		}

		public override void Draw(Vec2 camPos, GameTone tone)
		{
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X - 8;
			zero.Y = pos.Y / 256 - camPos.Y - 8;
			Game1.gMan.MainBlit("npc/mood_bubbles", zero, drawRect, 1f, 0, GraphicsManager.BlendMode.Normal, 2, tone);
		}
	}
}

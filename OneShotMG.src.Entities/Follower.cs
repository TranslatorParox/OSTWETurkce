using System;
using System.Collections.Generic;
using OneShotMG.src.Map;
using OneShotMG.src.TWM;

namespace OneShotMG.src.Entities
{
	public class Follower : Entity
	{
		public class FollowerState
		{
			public int frameIndex;

			public Vec2 pos;

			public Direction direction;

			public FollowerState(Entity e)
			{
				frameIndex = e.GetFrameIndex();
				pos = e.GetPos();
				direction = e.GetDirection();
			}

			public bool IsEqualState(FollowerState otherState)
			{
				int num = Math.Abs(pos.X - otherState.pos.X) + Math.Abs(pos.Y - otherState.pos.Y);
				if (otherState != null)
				{
					if (frameIndex == otherState.frameIndex && num < 128)
					{
						return direction == otherState.direction;
					}
					return false;
				}
				return true;
			}
		}

		private FollowerManager.FollowerType type;

		private Queue<FollowerState> trackedStates;

		private const int FRAMES_TO_LAG_BEHIND = 120;

		private const int DISTANCE_TO_LAG_BEHIND = 6144;

		private Entity followTarget;

		private FollowerState lastObservedState;

		public Follower(FollowerManager.FollowerType followerType, Entity entityToFollow, OneshotWindow osWindow)
			: base(osWindow)
		{
			id = oneshotWindow.tileMapMan.GetNextEntityID();
			type = followerType;
			followTarget = entityToFollow;
			ignoreMapCollision = true;
			ignoreNpcCollision = true;
			SetNPCSheet(oneshotWindow.followerMan.GetFollowerSheet(type));
			opacity = 255;
			collisionRect = new Rect(-8, -8, 16, 16);
			trackedStates = new Queue<FollowerState>();
			pos = entityToFollow.GetPos();
			direction = entityToFollow.GetDirection();
			frameIndex = 0;
			base.NeverHash = true;
		}

		public override void Update()
		{
			oldPos = pos;
			if (followTarget == null)
			{
				return;
			}
			FollowerState followerState = new FollowerState(followTarget);
			if (trackedStates.Count <= 0 || !followerState.IsEqualState(lastObservedState))
			{
				trackedStates.Enqueue(followerState);
				lastObservedState = followerState;
			}
			long num = Math.Abs(pos.X - followTarget.GetPos().X);
			long num2 = Math.Abs(pos.Y - followTarget.GetPos().Y);
			int num3 = (int)Math.Sqrt(num * num + num2 * num2);
			if (num3 > 6144 || trackedStates.Count > 120)
			{
				FollowerState followerState2 = trackedStates.Dequeue();
				if (num3 > 12288 && trackedStates.Count > 40)
				{
					followerState2 = trackedStates.Dequeue();
				}
				frameIndex = followerState2.frameIndex;
				direction = followerState2.direction;
				pos = followerState2.pos;
			}
			else
			{
				frameIndex = 0;
			}
		}

		public void InheritPosition(Follower followerWeInherit)
		{
			followTarget = followerWeInherit.followTarget;
			while (followerWeInherit.trackedStates.Count > 0)
			{
				trackedStates.Enqueue(followerWeInherit.trackedStates.Dequeue());
			}
		}
	}
}

using OneShotMG.src.Entities;

namespace OneShotMG.src
{
	public class EntitySaveData
	{
		public Entity.Direction direction;

		public int frameIndex;

		public int frameTimer;

		public int collisionDirection;

		public int currentPageIndex;

		public Vec2 pos;

		public Vec2 vel;

		public bool active;

		public int stopTimer;

		public int moveRouteIndex;

		public int waitTimer;

		public int moveStuckTimer;

		public bool jumping;

		public bool moving;

		public Vec2 moveTarget;

		public int id;

		public bool ignoreMapCollision;

		public bool ignoreNpcCollision;

		public MoveRoute moveRoute;

		public MoveRoute storedMoveRoute;

		public bool moveRouteForcing;
	}
}

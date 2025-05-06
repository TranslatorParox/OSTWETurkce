using System;
using System.Collections.Generic;
using System.Globalization;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Entities;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class Entity
	{
		public enum Direction
		{
			Down = 2,
			Left = 4,
			Right = 6,
			Up = 8,
			None = 0
		}

		public enum MoveType
		{
			None,
			Random,
			Approach,
			Custom
		}

		public enum EventTrigger
		{
			ActionButton,
			PlayerTouch,
			EventTouch,
			AutoRun,
			ParallelProcess
		}

		public enum MoveCommandCode
		{
			End,
			MoveDown,
			MoveLeft,
			MoveRight,
			MoveUp,
			MoveLowerLeft,
			MoveLowerRight,
			MoveUpperLeft,
			MoveUpperRight,
			MoveRandom,
			MoveTowardPlayer,
			MoveAwayFromPlayer,
			MoveForward,
			MoveBackward,
			MoveJump,
			Wait,
			TurnDown,
			TurnLeft,
			TurnRight,
			TurnUp,
			TurnRight90,
			TurnLeft90,
			Turn180,
			TurnRightOrLeft90,
			TurnRandom,
			TurnTowardPlayer,
			TurnAwayFromPlayer,
			FlagOn,
			FlagOff,
			ChangeSpeed,
			ChangeFreq,
			MoveAnimOn,
			MoveAnimOff,
			StopAnimOn,
			StopAnimOff,
			DirectionFixOn,
			DirectionFixOff,
			ThroughOn,
			ThroughOff,
			AlwaysOnTopOn,
			AlwaysOnTopOff,
			ChangeGraphic,
			ChangeOpacity,
			ChangeBlending,
			PlaySE,
			Script
		}

		public const int ENTITY_PIXEL_SCALE = 256;

		protected OneshotWindow oneshotWindow;

		protected MoveType moveType;

		protected Direction direction;

		protected Direction originalDirection;

		protected int frameIndex;

		protected int frameTimer;

		protected int originalFrameIndex;

		protected int collisionDirection;

		protected int tileId;

		protected string npcSheet = string.Empty;

		protected bool hasLightmap;

		protected Vec2 spriteSize;

		protected int hue;

		protected int opacity;

		protected GraphicsManager.BlendMode blendMode = GraphicsManager.BlendMode.Normal;

		protected int currentPageIndex = -1;

		protected Vec2 oldPos;

		protected Direction oldDirection;

		protected Vec2 pos;

		protected Vec2 vel;

		protected bool active = true;

		protected Rect collisionRect;

		protected bool ignoreMapCollision = true;

		protected bool ignoreNpcCollision = true;

		protected bool alwaysOnTop;

		protected bool alwaysOnBottom;

		protected bool spawnedOnMapCollision;

		protected int stopTimer;

		protected int moveFrequency;

		protected int moveSpeed;

		protected bool moveAnimation;

		protected bool stopAnimation;

		protected MoveRoute moveRoute;

		protected int moveRouteIndex;

		protected bool moveRouteForcing;

		private bool locked;

		private Direction prelockDirection;

		protected MoveRoute storedMoveRoute;

		protected int storedMoveRouteIndex;

		protected int waitTimer;

		protected int moveStuckTimer;

		protected const int MOVE_STUCK_RESET_TIME = 30;

		protected bool jumping;

		protected bool moving;

		protected int jumpCount;

		protected int jumpPeak;

		protected bool ignoreNpcCollisionBeforeJump;

		protected bool ignoreMapCollisionBeforeJump;

		protected Vec2 moveTarget = Vec2.Zero;

		protected bool directionFix;

		protected Event eventData;

		public LinkedListNode<Entity> sortedNode;

		public bool KillEntityAfterUpdate;

		public bool HasBeenHashed;

		protected int id;

		public bool isTouchingPlayerAndTriggeredEvent;

		private bool AlwaysActiveAndOnlyHasOnePage;

		protected bool colorAbberate;

		protected int redAbberateTimer;

		protected int greenAbberateTimer;

		protected int blueAbberateTimer;

		protected Vec2 redOffset = Vec2.Zero;

		protected Vec2 greenOffset = Vec2.Zero;

		protected Vec2 blueOffset = Vec2.Zero;

		private bool wheelSqueak;

		public bool NeverHash { get; protected set; }

		public EventTrigger eventTrigger { get; private set; }

		public EventCommand[] list { get; private set; }

		public bool HandlesMapEventVar { get; private set; }

		public int GetID()
		{
			return id;
		}

		public void SetID(int newID)
		{
			id = newID;
		}

		public Entity(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
		}

		public Entity(OneshotWindow osWindow, Event eventData)
		{
			oneshotWindow = osWindow;
			this.eventData = eventData;
			Initialize();
		}

		public bool IgnoreMapCollision()
		{
			return ignoreMapCollision;
		}

		public bool IgnoreNpcCollision()
		{
			return ignoreNpcCollision;
		}

		public bool Active()
		{
			return active;
		}

		public string GetName()
		{
			if (eventData == null || eventData.name == null)
			{
				return string.Empty;
			}
			return eventData.name;
		}

		public bool HasScript()
		{
			if (eventTrigger != EventTrigger.ParallelProcess && eventTrigger != EventTrigger.AutoRun && list != null)
			{
				return list.Length > 1;
			}
			return false;
		}

		public virtual void Movement()
		{
			oldPos = pos;
			pos.X += vel.X;
			pos.Y += vel.Y;
		}

		protected void CheckForActivePage()
		{
			if (AlwaysActiveAndOnlyHasOnePage)
			{
				return;
			}
			bool flag = false;
			for (int num = eventData.pages.Length - 1; num >= 0; num--)
			{
				Event.Page page = eventData.pages[num];
				if (isPageActive(page))
				{
					SetActivePage(num);
					flag = true;
					break;
				}
			}
			active = flag;
		}

		protected void CommonUpdate()
		{
			if (KillEntityAfterUpdate)
			{
				return;
			}
			CheckForActivePage();
			if (!active)
			{
				return;
			}
			if (eventTrigger == EventTrigger.AutoRun)
			{
				oneshotWindow.tileMapMan.StartEvent(this);
			}
			else if (eventTrigger == EventTrigger.ParallelProcess)
			{
				oneshotWindow.tileMapMan.StartEvent(this, parallelProcess: true);
			}
			if (jumping)
			{
				UpdateJump();
				Movement();
			}
			else if (moving)
			{
				UpdateMovement();
			}
			else
			{
				vel = Vec2.Zero;
				frameTimer++;
				stopTimer++;
				Movement();
			}
			int num = 18 - moveSpeed * 2;
			if (!stopAnimation)
			{
				num = num * 2 / 3;
			}
			if (frameTimer > num)
			{
				frameTimer = 0;
				if (!stopAnimation && stopTimer > 0)
				{
					frameIndex = originalFrameIndex;
				}
				else
				{
					frameIndex = (frameIndex + 1) % 4;
					if (this is Player)
					{
						CheckIfPlayFootstepSound();
					}
				}
			}
			if (waitTimer > 0)
			{
				waitTimer--;
			}
			else if (moveRouteForcing)
			{
				MoveTypeCustom();
			}
			else if (!locked && stopTimer > (40 - moveFrequency * 2) * (6 - moveFrequency))
			{
				DoNextMove();
			}
			ResetCollisionDirection();
		}

		private void UpdateMovement()
		{
			if (moveAnimation || stopAnimation)
			{
				frameTimer++;
			}
			int num = 16 * (2 << moveSpeed);
			if (oldPos.X == pos.X && oldPos.Y == pos.Y)
			{
				moveStuckTimer++;
				stopTimer = 1;
			}
			else
			{
				moveStuckTimer = 0;
				stopTimer = 0;
			}
			if (pos.X + num <= moveTarget.X)
			{
				vel.X = num;
			}
			else if (pos.X - num >= moveTarget.X)
			{
				vel.X = -num;
			}
			else
			{
				vel.X = 0;
				pos.X = moveTarget.X;
			}
			if (pos.Y + num <= moveTarget.Y)
			{
				vel.Y = num;
			}
			else if (pos.Y - num >= moveTarget.Y)
			{
				vel.Y = -num;
			}
			else
			{
				vel.Y = 0;
				pos.Y = moveTarget.Y;
			}
			Movement();
			if ((pos.X == moveTarget.X && pos.Y == moveTarget.Y) || (moveStuckTimer > 30 && !moveRouteForcing && moveType != MoveType.Custom))
			{
				CancelMoveDueToMoveStuck();
			}
		}

		private void CancelMoveDueToMoveStuck()
		{
			moveStuckTimer = 0;
			moving = false;
			vel = Vec2.Zero;
			stopTimer = 0;
		}

		private void UpdateJump()
		{
			jumpCount--;
			if (jumpCount <= 0)
			{
				jumpPeak = 0;
				jumping = false;
				pos = moveTarget;
				ignoreMapCollision = ignoreMapCollisionBeforeJump;
				ignoreNpcCollision = ignoreNpcCollisionBeforeJump;
			}
			else
			{
				pos.X = (pos.X * jumpCount + moveTarget.X) / (jumpCount + 1);
				pos.Y = (pos.Y * jumpCount + moveTarget.Y) / (jumpCount + 1);
			}
		}

		private void DoNextMove()
		{
			switch (moveType)
			{
			case MoveType.Approach:
			{
				Vec2 playerPos = oneshotWindow.tileMapMan.GetPlayerPos();
				if (Math.Abs(playerPos.X - pos.X) + Math.Abs(playerPos.Y - pos.Y) > 81920)
				{
					MoveRandom();
					break;
				}
				switch (MathHelper.Random(0, 5))
				{
				case 0:
				case 1:
				case 2:
				case 3:
					MoveTowardPlayer();
					break;
				case 4:
					MoveRandom();
					break;
				case 5:
					MoveInDirection(direction);
					break;
				}
				break;
			}
			case MoveType.Random:
				switch (MathHelper.Random(0, 5))
				{
				case 0:
				case 1:
				case 2:
				case 3:
					MoveRandom();
					break;
				case 4:
					MoveInDirection(direction);
					break;
				case 5:
					stopTimer = 0;
					break;
				}
				break;
			case MoveType.Custom:
				MoveTypeCustom();
				break;
			case MoveType.None:
				break;
			}
		}

		private void MoveTypeCustom()
		{
			if (moving || jumping)
			{
				return;
			}
			while (moveRouteIndex < moveRoute.list.Length)
			{
				MoveCommand moveCommand = moveRoute.list[moveRouteIndex];
				MoveCommandCode code = (MoveCommandCode)moveCommand.code;
				if (code == MoveCommandCode.End)
				{
					if (moveRoute.repeat)
					{
						moveRouteIndex = 0;
						break;
					}
					if (moveRouteForcing)
					{
						moveRouteForcing = false;
						moveRoute = storedMoveRoute;
						moveRouteIndex = storedMoveRouteIndex;
						storedMoveRoute = null;
					}
					stopTimer = 0;
					break;
				}
				if (code <= MoveCommandCode.MoveJump)
				{
					switch (code)
					{
					case MoveCommandCode.MoveDown:
						MoveInDirection(Direction.Down);
						break;
					case MoveCommandCode.MoveLeft:
						MoveInDirection(Direction.Left);
						break;
					case MoveCommandCode.MoveRight:
						MoveInDirection(Direction.Right);
						break;
					case MoveCommandCode.MoveUp:
						MoveInDirection(Direction.Up);
						break;
					case MoveCommandCode.MoveLowerLeft:
						MoveLowerLeft();
						break;
					case MoveCommandCode.MoveLowerRight:
						MoveLowerRight();
						break;
					case MoveCommandCode.MoveUpperLeft:
						MoveUpperLeft();
						break;
					case MoveCommandCode.MoveUpperRight:
						MoveUpperRight();
						break;
					case MoveCommandCode.MoveJump:
						MoveJump(int.Parse(moveCommand.parameters[0]), int.Parse(moveCommand.parameters[1], CultureInfo.InvariantCulture));
						break;
					default:
						Game1.logMan.Log(LogManager.LogLevel.Error, "move command " + code.ToString() + " not implemented!");
						break;
					case MoveCommandCode.MoveRandom:
						MoveRandom();
						break;
					case MoveCommandCode.MoveTowardPlayer:
						MoveTowardPlayer();
						break;
					case MoveCommandCode.MoveAwayFromPlayer:
						MoveAwayFromPlayer();
						break;
					case MoveCommandCode.MoveBackward:
						MoveBackward();
						break;
					case MoveCommandCode.MoveForward:
						MoveInDirection(direction);
						break;
					}
					if (moveRoute.skippable || moving || jumping)
					{
						moveRouteIndex++;
					}
					break;
				}
				if (code == MoveCommandCode.Wait)
				{
					waitTimer = int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture) * 2 - 1;
					moveRouteIndex++;
					break;
				}
				if (code <= MoveCommandCode.TurnAwayFromPlayer)
				{
					switch (code)
					{
					case MoveCommandCode.TurnDown:
						Turn(Direction.Down);
						break;
					case MoveCommandCode.TurnLeft:
						Turn(Direction.Left);
						break;
					case MoveCommandCode.TurnRight:
						Turn(Direction.Right);
						break;
					case MoveCommandCode.TurnUp:
						Turn(Direction.Up);
						break;
					case MoveCommandCode.TurnRight90:
						TurnRight90();
						break;
					case MoveCommandCode.TurnLeft90:
						TurnLeft90();
						break;
					case MoveCommandCode.Turn180:
						Turn180();
						break;
					case MoveCommandCode.TurnRightOrLeft90:
						TurnRightOrLeft90();
						break;
					case MoveCommandCode.TurnRandom:
						TurnRandom();
						break;
					case MoveCommandCode.TurnTowardPlayer:
						TurnTowardPlayer();
						break;
					case MoveCommandCode.TurnAwayFromPlayer:
						TurnAwayFromPlayer();
						break;
					default:
						Game1.logMan.Log(LogManager.LogLevel.Error, "move command " + code.ToString() + " not implemented!");
						break;
					}
					moveRouteIndex++;
					break;
				}
				switch (code)
				{
				case MoveCommandCode.FlagOn:
					oneshotWindow.flagMan.SetFlag(int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture));
					break;
				case MoveCommandCode.FlagOff:
					oneshotWindow.flagMan.UnsetFlag(int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture));
					break;
				case MoveCommandCode.ChangeSpeed:
					moveSpeed = int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture);
					break;
				case MoveCommandCode.ChangeFreq:
					moveFrequency = int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture);
					break;
				case MoveCommandCode.MoveAnimOn:
					moveAnimation = true;
					break;
				case MoveCommandCode.MoveAnimOff:
					moveAnimation = false;
					if (!stopAnimation && frameIndex != originalFrameIndex)
					{
						frameIndex = originalFrameIndex;
					}
					break;
				case MoveCommandCode.StopAnimOn:
					stopAnimation = true;
					break;
				case MoveCommandCode.StopAnimOff:
					stopAnimation = false;
					if (!moveAnimation && frameIndex != originalFrameIndex)
					{
						frameIndex = originalFrameIndex;
					}
					break;
				case MoveCommandCode.DirectionFixOn:
					directionFix = true;
					break;
				case MoveCommandCode.DirectionFixOff:
					directionFix = false;
					break;
				case MoveCommandCode.ThroughOn:
					ignoreMapCollision = true;
					ignoreNpcCollision = true;
					break;
				case MoveCommandCode.ThroughOff:
					ignoreMapCollision = spawnedOnMapCollision;
					ignoreNpcCollision = false;
					break;
				case MoveCommandCode.AlwaysOnTopOn:
					alwaysOnTop = true;
					break;
				case MoveCommandCode.AlwaysOnTopOff:
					alwaysOnTop = false;
					break;
				case MoveCommandCode.ChangeGraphic:
				{
					tileId = 0;
					SetNPCSheet(moveCommand.parameters[0], int.Parse(moveCommand.parameters[1], CultureInfo.InvariantCulture));
					direction = (Direction)int.Parse(moveCommand.parameters[2], CultureInfo.InvariantCulture);
					int num = int.Parse(moveCommand.parameters[3], CultureInfo.InvariantCulture);
					if (originalFrameIndex != num)
					{
						frameIndex = num;
						originalFrameIndex = num;
					}
					break;
				}
				case MoveCommandCode.ChangeOpacity:
					opacity = int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture);
					break;
				case MoveCommandCode.ChangeBlending:
					if (int.Parse(moveCommand.parameters[0], CultureInfo.InvariantCulture) == 1)
					{
						blendMode = GraphicsManager.BlendMode.Additive;
					}
					else
					{
						blendMode = GraphicsManager.BlendMode.Normal;
					}
					break;
				case MoveCommandCode.PlaySE:
				{
					float pitch = (float)moveCommand.audio_file.pitch / 100f;
					float vol = (float)moveCommand.audio_file.volume / 100f;
					Game1.soundMan.PlaySound(moveCommand.audio_file.name, vol, pitch);
					break;
				}
				case MoveCommandCode.Script:
					switch (moveCommand.parameters[0])
					{
					case "straighten":
						Straighten();
						moveRouteIndex++;
						return;
					case "step_fwd_no_snap":
						StepForwardNoSnap();
						moveRouteIndex++;
						return;
					case "move_to_this_npc":
						MoveToNpc();
						moveRouteIndex++;
						return;
					}
					break;
				default:
					Game1.logMan.Log(LogManager.LogLevel.Error, "move command " + code.ToString() + " not implemented!");
					break;
				}
				moveRouteIndex++;
			}
		}

		private void MoveJump(int xDelta, int yDelta)
		{
			if (xDelta != 0 || yDelta != 0)
			{
				if (Math.Abs(xDelta) > Math.Abs(yDelta))
				{
					if (xDelta < 0)
					{
						Turn(Direction.Left);
					}
					else
					{
						Turn(Direction.Right);
					}
				}
				else if (yDelta < 0)
				{
					Turn(Direction.Up);
				}
				else
				{
					Turn(Direction.Down);
				}
			}
			Vec2 currentTile = GetCurrentTile();
			Vec2 currentTile2 = new Vec2(currentTile.X + xDelta, currentTile.Y + yDelta);
			if ((xDelta == 0 && yDelta == 0) || Passable(currentTile2, Direction.None))
			{
				moveTarget = new Vec2(currentTile2.X * 16 * 256 + 2048, currentTile2.Y * 16 * 256 + 2048);
				int num = (int)Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
				jumpPeak = 10 + num - moveSpeed;
				jumpCount = jumpPeak * 2;
				stopTimer = 0;
				jumping = true;
				ignoreMapCollisionBeforeJump = ignoreMapCollision;
				ignoreNpcCollisionBeforeJump = ignoreNpcCollision;
				ignoreMapCollision = true;
				ignoreNpcCollision = true;
			}
		}

		public void Lock()
		{
			if (!locked)
			{
				prelockDirection = direction;
				TurnTowardPlayer();
				locked = true;
			}
		}

		public void Unlock()
		{
			if (locked)
			{
				locked = false;
				if (!directionFix && prelockDirection != 0)
				{
					direction = prelockDirection;
					prelockDirection = Direction.None;
				}
			}
		}

		public Direction GetOldDirection()
		{
			return oldDirection;
		}

		private Vec2 TargetTileFromDirection(Vec2 currentTile, Direction desiredDirection)
		{
			Vec2 result = currentTile;
			switch (desiredDirection)
			{
			case Direction.Down:
				result.Y++;
				break;
			case Direction.Up:
				result.Y--;
				break;
			case Direction.Left:
				result.X--;
				break;
			case Direction.Right:
				result.X++;
				break;
			}
			return result;
		}

		private bool Passable(Vec2 currentTile, Direction desiredDirection)
		{
			if (ignoreMapCollision && ignoreNpcCollision)
			{
				return true;
			}
			Vec2 vec = TargetTileFromDirection(currentTile, desiredDirection);
			if (!ignoreMapCollision && oneshotWindow.tileMapMan.IsTileSolid(vec.X, vec.Y))
			{
				return false;
			}
			if (!ignoreNpcCollision)
			{
				Entity entity = new Entity(oneshotWindow);
				entity.collisionRect = new Rect(-8, -8, 16, 16);
				entity.pos = new Vec2(vec.X * 256 * 16 + 2048, vec.Y * 256 * 16 + 2048);
				entity.ResetCollisionDirection();
				oneshotWindow.tileMapMan.HashedEntityCollision(entity, this, isTestCollisionEntity: true);
				if (entity.collisionDirection != 0)
				{
					return false;
				}
			}
			return true;
		}

		private void Turn(Direction d)
		{
			if (!directionFix)
			{
				stopTimer = 0;
				direction = d;
			}
		}

		private void TurnRight90()
		{
			switch (direction)
			{
			case Direction.Left:
				Turn(Direction.Up);
				break;
			case Direction.Up:
				Turn(Direction.Right);
				break;
			case Direction.Right:
				Turn(Direction.Down);
				break;
			case Direction.Down:
				Turn(Direction.Left);
				break;
			case (Direction)3:
			case (Direction)5:
			case (Direction)7:
				break;
			}
		}

		private void TurnLeft90()
		{
			switch (direction)
			{
			case Direction.Left:
				Turn(Direction.Down);
				break;
			case Direction.Up:
				Turn(Direction.Left);
				break;
			case Direction.Right:
				Turn(Direction.Up);
				break;
			case Direction.Down:
				Turn(Direction.Right);
				break;
			case (Direction)3:
			case (Direction)5:
			case (Direction)7:
				break;
			}
		}

		private void Turn180()
		{
			switch (direction)
			{
			case Direction.Left:
				Turn(Direction.Right);
				break;
			case Direction.Up:
				Turn(Direction.Down);
				break;
			case Direction.Right:
				Turn(Direction.Left);
				break;
			case Direction.Down:
				Turn(Direction.Up);
				break;
			case (Direction)3:
			case (Direction)5:
			case (Direction)7:
				break;
			}
		}

		public string GetNPCSheet()
		{
			return npcSheet;
		}

		public int GetCurrentPageNumber()
		{
			return currentPageIndex;
		}

		public int GetFrameIndex()
		{
			return frameIndex;
		}

		public void SetFrameIndex(int newIndex)
		{
			frameIndex = newIndex;
		}

		private void TurnRightOrLeft90()
		{
			switch (MathHelper.Random(0, 1))
			{
			case 0:
				TurnLeft90();
				break;
			case 1:
				TurnRight90();
				break;
			}
		}

		private void TurnRandom()
		{
			switch (MathHelper.Random(0, 3))
			{
			case 0:
				Turn(Direction.Left);
				break;
			case 1:
				Turn(Direction.Right);
				break;
			case 2:
				Turn(Direction.Up);
				break;
			case 3:
				Turn(Direction.Down);
				break;
			}
		}

		private void TurnTowardPlayer()
		{
			Vec2 playerPos = oneshotWindow.tileMapMan.GetPlayerPos();
			int num = pos.X - playerPos.X;
			int num2 = pos.Y - playerPos.Y;
			if (Math.Abs(num) > Math.Abs(num2))
			{
				if (num > 0)
				{
					Turn(Direction.Left);
				}
				else
				{
					Turn(Direction.Right);
				}
			}
			else if (num2 > 0)
			{
				Turn(Direction.Up);
			}
			else
			{
				Turn(Direction.Down);
			}
		}

		private void TurnAwayFromPlayer()
		{
			Vec2 playerPos = oneshotWindow.tileMapMan.GetPlayerPos();
			int num = pos.X - playerPos.X;
			int num2 = pos.Y - playerPos.Y;
			if (Math.Abs(num) > Math.Abs(num2))
			{
				if (num > 0)
				{
					Turn(Direction.Right);
				}
				else
				{
					Turn(Direction.Left);
				}
			}
			else if (num2 > 0)
			{
				Turn(Direction.Down);
			}
			else
			{
				Turn(Direction.Up);
			}
		}

		private void MoveToNpc()
		{
			Vec2 currentTile = GetCurrentTile();
			Entity nPCTriggeringEvent = oneshotWindow.tileMapMan.GetNPCTriggeringEvent();
			if (nPCTriggeringEvent == null)
			{
				return;
			}
			Vec2 currentTile2 = nPCTriggeringEvent.GetCurrentTile();
			if (currentTile.X == currentTile2.X && currentTile.Y == currentTile2.Y)
			{
				return;
			}
			int num = currentTile.X - currentTile2.X;
			int num2 = currentTile.Y - currentTile2.Y;
			if (Math.Abs(num) > Math.Abs(num2))
			{
				if (num > 0)
				{
					Turn(Direction.Left);
				}
				else if (num < 0)
				{
					Turn(Direction.Right);
				}
			}
			else if (num2 > 0)
			{
				Turn(Direction.Up);
			}
			else if (num2 < 0)
			{
				Turn(Direction.Down);
			}
			moveTarget = new Vec2(currentTile2.X * 16 * 256 + 2048, currentTile2.Y * 16 * 256 + 2048);
			moving = true;
			stopTimer = 0;
		}

		private void MoveTowardPlayer()
		{
			Vec2 currentTile = GetCurrentTile();
			Vec2 currentTile2 = oneshotWindow.tileMapMan.GetPlayer().GetCurrentTile();
			if (currentTile.X == currentTile2.X && currentTile.Y == currentTile2.Y)
			{
				return;
			}
			int num = currentTile.X - currentTile2.X;
			int num2 = currentTile.Y - currentTile2.Y;
			if (Math.Abs(num) > Math.Abs(num2))
			{
				if (num > 0)
				{
					MoveInDirection(Direction.Left);
				}
				else if (num < 0)
				{
					MoveInDirection(Direction.Right);
				}
				if (!moving)
				{
					if (num2 > 0)
					{
						MoveInDirection(Direction.Up);
					}
					else if (num2 < 0)
					{
						MoveInDirection(Direction.Down);
					}
				}
				return;
			}
			if (num2 > 0)
			{
				MoveInDirection(Direction.Up);
			}
			else if (num2 < 0)
			{
				MoveInDirection(Direction.Down);
			}
			if (!moving)
			{
				if (num > 0)
				{
					MoveInDirection(Direction.Left);
				}
				else if (num < 0)
				{
					MoveInDirection(Direction.Right);
				}
			}
		}

		private void MoveAwayFromPlayer()
		{
			Vec2 playerPos = oneshotWindow.tileMapMan.GetPlayerPos();
			int num = pos.X - playerPos.X;
			int num2 = pos.Y - playerPos.Y;
			if (Math.Abs(num) > Math.Abs(num2))
			{
				if (num > 0)
				{
					MoveInDirection(Direction.Right);
				}
				else
				{
					MoveInDirection(Direction.Left);
				}
				if (!moving)
				{
					if (num2 > 0)
					{
						MoveInDirection(Direction.Down);
					}
					else
					{
						MoveInDirection(Direction.Up);
					}
				}
				return;
			}
			if (num2 > 0)
			{
				MoveInDirection(Direction.Down);
			}
			else
			{
				MoveInDirection(Direction.Up);
			}
			if (!moving)
			{
				if (num > 0)
				{
					MoveInDirection(Direction.Right);
				}
				else
				{
					MoveInDirection(Direction.Left);
				}
			}
		}

		private void MoveBackward()
		{
			switch (direction)
			{
			case Direction.Left:
				MoveInDirection(Direction.Right);
				Turn(Direction.Left);
				break;
			case Direction.Right:
				MoveInDirection(Direction.Left);
				Turn(Direction.Right);
				break;
			case Direction.Up:
				MoveInDirection(Direction.Down);
				Turn(Direction.Up);
				break;
			case Direction.Down:
				MoveInDirection(Direction.Up);
				Turn(Direction.Down);
				break;
			case (Direction)3:
			case (Direction)5:
			case (Direction)7:
				break;
			}
		}

		private void MoveInDirection(Direction d, bool turnEnabled = true)
		{
			if (turnEnabled)
			{
				Turn(d);
			}
			Vec2 currentTile = GetCurrentTile();
			if (Passable(currentTile, d))
			{
				Turn(d);
				Vec2 vec = TargetTileFromDirection(currentTile, d);
				moveTarget = new Vec2(vec.X * 16 * 256 + 2048, vec.Y * 16 * 256 + 2048);
				moving = true;
				stopTimer = 0;
				EmitFootprint();
			}
		}

		private void Straighten()
		{
			Vec2 currentTile = GetCurrentTile();
			moveTarget = new Vec2(currentTile.X * 16 * 256 + 2048, currentTile.Y * 16 * 256 + 2048);
			moving = true;
			stopTimer = 0;
		}

		private void StepForwardNoSnap()
		{
			moveTarget = pos;
			switch (direction)
			{
			case Direction.Down:
				moveTarget.Y += 4096;
				break;
			case Direction.Up:
				moveTarget.Y -= 4096;
				break;
			case Direction.Left:
				moveTarget.X -= 4096;
				break;
			case Direction.Right:
				moveTarget.X += 4096;
				break;
			}
			moving = true;
			stopTimer = 0;
			EmitFootprint();
		}

		private void MoveLowerLeft()
		{
			if (!directionFix)
			{
				switch (direction)
				{
				case Direction.Up:
					direction = Direction.Down;
					break;
				case Direction.Right:
					direction = Direction.Left;
					break;
				}
			}
			Vec2 currentTile = GetCurrentTile();
			Vec2 currentTile2 = new Vec2(currentTile.X - 1, currentTile.Y);
			Vec2 currentTile3 = new Vec2(currentTile.X, currentTile.Y + 1);
			if ((Passable(currentTile, Direction.Left) && Passable(currentTile2, Direction.Down)) || (Passable(currentTile, Direction.Down) && Passable(currentTile3, Direction.Left)))
			{
				Vec2 vec = new Vec2(currentTile.X - 1, currentTile.Y + 1);
				moveTarget = new Vec2(vec.X * 16 * 256 + 2048, vec.Y * 16 * 256 + 2048);
				moving = true;
				stopTimer = 0;
			}
		}

		private void MoveLowerRight()
		{
			if (!directionFix)
			{
				switch (direction)
				{
				case Direction.Up:
					direction = Direction.Down;
					break;
				case Direction.Left:
					direction = Direction.Right;
					break;
				}
			}
			Vec2 currentTile = GetCurrentTile();
			Vec2 currentTile2 = new Vec2(currentTile.X + 1, currentTile.Y);
			Vec2 currentTile3 = new Vec2(currentTile.X, currentTile.Y + 1);
			if ((Passable(currentTile, Direction.Right) && Passable(currentTile2, Direction.Down)) || (Passable(currentTile, Direction.Down) && Passable(currentTile3, Direction.Right)))
			{
				Vec2 vec = new Vec2(currentTile.X + 1, currentTile.Y + 1);
				moveTarget = new Vec2(vec.X * 16 * 256 + 2048, vec.Y * 16 * 256 + 2048);
				moving = true;
				stopTimer = 0;
			}
		}

		private void MoveUpperLeft()
		{
			if (!directionFix)
			{
				switch (direction)
				{
				case Direction.Down:
					direction = Direction.Up;
					break;
				case Direction.Right:
					direction = Direction.Left;
					break;
				}
			}
			Vec2 currentTile = GetCurrentTile();
			Vec2 currentTile2 = new Vec2(currentTile.X - 1, currentTile.Y);
			Vec2 currentTile3 = new Vec2(currentTile.X, currentTile.Y - 1);
			if ((Passable(currentTile, Direction.Left) && Passable(currentTile2, Direction.Up)) || (Passable(currentTile, Direction.Up) && Passable(currentTile3, Direction.Left)))
			{
				Vec2 vec = new Vec2(currentTile.X - 1, currentTile.Y - 1);
				moveTarget = new Vec2(vec.X * 16 * 256 + 2048, vec.Y * 16 * 256 + 2048);
				moving = true;
				stopTimer = 0;
			}
		}

		private void MoveUpperRight()
		{
			if (!directionFix)
			{
				switch (direction)
				{
				case Direction.Down:
					direction = Direction.Up;
					break;
				case Direction.Left:
					direction = Direction.Right;
					break;
				}
			}
			Vec2 currentTile = GetCurrentTile();
			Vec2 currentTile2 = new Vec2(currentTile.X + 1, currentTile.Y);
			Vec2 currentTile3 = new Vec2(currentTile.X, currentTile.Y - 1);
			if ((Passable(currentTile, Direction.Right) && Passable(currentTile2, Direction.Up)) || (Passable(currentTile, Direction.Up) && Passable(currentTile3, Direction.Right)))
			{
				Vec2 vec = new Vec2(currentTile.X + 1, currentTile.Y - 1);
				moveTarget = new Vec2(vec.X * 16 * 256 + 2048, vec.Y * 16 * 256 + 2048);
				moving = true;
				stopTimer = 0;
			}
		}

		protected void EmitFootprint()
		{
			Vec2 currentTile = GetCurrentTile();
			if (oneshotWindow.tileMapMan.IsTileCounter(currentTile.X, currentTile.Y) && !oneshotWindow.flagMan.IsFlagSet(101) && !oneshotWindow.flagMan.IsFlagSet(111))
			{
				oneshotWindow.tileMapMan.EntityAddEntity(new Footprint(oneshotWindow, direction, pos));
			}
		}

		protected void EmitFootsplash()
		{
			Vec2 currentTile = GetCurrentTile();
			if (oneshotWindow.tileMapMan.IsTileCounter(currentTile.X, currentTile.Y) && oneshotWindow.flagMan.IsFlagSet(101) && !oneshotWindow.flagMan.IsFlagSet(111))
			{
				oneshotWindow.tileMapMan.EntityAddEntity(new Footsplash(oneshotWindow, direction, pos));
			}
		}

		private void MoveRandom()
		{
			switch (MathHelper.Random(0, 3))
			{
			case 0:
				MoveInDirection(Direction.Up, turnEnabled: false);
				break;
			case 1:
				MoveInDirection(Direction.Left, turnEnabled: false);
				break;
			case 2:
				MoveInDirection(Direction.Right, turnEnabled: false);
				break;
			case 3:
				MoveInDirection(Direction.Down, turnEnabled: false);
				break;
			}
		}

		public void SetNPCSheet(string newSheet, int hue = 0)
		{
			npcSheet = newSheet.ToLowerInvariant();
			if (oneshotWindow.flagMan.IsFlagSet(160) && npcSheet.StartsWith("niko"))
			{
				npcSheet = npcSheet.Replace("niko", "en");
			}
			if (npcSheet.StartsWith("en"))
			{
				colorAbberate = true;
			}
			else
			{
				colorAbberate = false;
			}
			this.hue = hue;
			if (!string.IsNullOrEmpty(npcSheet))
			{
				Vec2 vec = Game1.gMan.TextureSize("npc/" + npcSheet);
				spriteSize = new Vec2(vec.X / 4, vec.Y / 4);
				hasLightmap = Game1.gMan.DoesLightmapExist("lightmaps/" + npcSheet);
			}
			else if (tileId > 384)
			{
				spriteSize = new Vec2(16, 16);
				hasLightmap = false;
			}
			else
			{
				spriteSize = Vec2.Zero;
				hasLightmap = false;
			}
			if (npcSheet == "green_scenery1" && eventData.name == "maize")
			{
				alwaysOnBottom = true;
			}
		}

		protected void SetActivePage(int newPageIndex)
		{
			if (currentPageIndex == newPageIndex)
			{
				return;
			}
			currentPageIndex = newPageIndex;
			if (newPageIndex >= 0 && newPageIndex < eventData.pages.Length)
			{
				Event.Page page = eventData.pages[currentPageIndex];
				alwaysOnTop = page.always_on_top;
				alwaysOnBottom = page.always_on_bottom;
				tileId = page.graphic.tile_id;
				SetNPCSheet(page.graphic.character_name, page.graphic.character_hue);
				if (tileId > 384)
				{
					page.through |= !oneshotWindow.tileMapMan.IsTileSolid(tileId);
				}
				if (originalDirection != (Direction)page.graphic.direction || page.direction_fix)
				{
					direction = (Direction)page.graphic.direction;
					originalDirection = (Direction)page.graphic.direction;
					prelockDirection = Direction.None;
				}
				if (page.graphic.pattern != originalFrameIndex)
				{
					frameIndex = page.graphic.pattern;
					originalFrameIndex = frameIndex;
				}
				opacity = page.graphic.opacity;
				ignoreNpcCollision = page.through;
				ignoreMapCollision = page.through || spawnedOnMapCollision;
				moveType = (MoveType)page.move_type;
				moveSpeed = page.move_speed;
				moveFrequency = page.move_frequency;
				moveRoute = page.move_route;
				moveRouteForcing = false;
				moveRouteIndex = 0;
				directionFix = page.direction_fix;
				moveAnimation = page.walk_anime;
				stopAnimation = page.step_anime;
				if (page.graphic.blend_type == 1)
				{
					blendMode = GraphicsManager.BlendMode.Additive;
				}
				else
				{
					blendMode = GraphicsManager.BlendMode.Normal;
				}
				eventTrigger = (EventTrigger)page.trigger;
				list = page.list;
			}
		}

		private bool isPageActive(Event.Page page)
		{
			Event.Page.Condition condition = page.condition;
			if ((!condition.switch1_valid || oneshotWindow.flagMan.IsFlagSet(condition.switch1_id)) && (!condition.switch2_valid || oneshotWindow.flagMan.IsFlagSet(condition.switch2_id)) && (!condition.variable_valid || oneshotWindow.varMan.GetVariable(condition.variable_id) >= condition.variable_value))
			{
				if (condition.self_switch_valid)
				{
					return oneshotWindow.selfSwitchMan.IsSelfSwitchSet(id, condition.self_switch_ch);
				}
				return true;
			}
			return false;
		}

		public Rect GetButtonPressRect()
		{
			Rect result = default(Rect);
			switch (direction)
			{
			case Direction.Left:
				result.X = pos.X + (collisionRect.X - 1) * 256;
				result.Y = pos.Y - 1;
				result.W = 256;
				result.H = 2;
				break;
			case Direction.Right:
				result.X = pos.X + (collisionRect.X + collisionRect.W) * 256;
				result.Y = pos.Y - 1;
				result.W = 256;
				result.H = 2;
				break;
			case Direction.Up:
				result.X = pos.X - 1;
				result.Y = pos.Y + (collisionRect.Y - 1) * 256;
				result.W = 2;
				result.H = 256;
				break;
			default:
				result.X = pos.X - 1;
				result.Y = pos.Y + (collisionRect.Y + collisionRect.H) * 256;
				result.W = 2;
				result.H = 256;
				break;
			}
			return result;
		}

		public virtual void Update()
		{
			if (colorAbberate)
			{
				MathHelper.HandleAbberateUpdate(ref redAbberateTimer, ref redOffset);
				MathHelper.HandleAbberateUpdate(ref blueAbberateTimer, ref blueOffset);
				MathHelper.HandleAbberateUpdate(ref greenAbberateTimer, ref greenOffset);
			}
			if (isTouchingPlayerAndTriggeredEvent)
			{
				Entity player = oneshotWindow.tileMapMan.GetPlayer();
				if (StartScriptOnPlayerTouch())
				{
					Rect otherCollision = new Rect(player.pos.X - 1, player.pos.Y - 1, 2, 2);
					Rect buttonPressRect = player.GetButtonPressRect();
					if (!IgnoreNpcCollision())
					{
						isTouchingPlayerAndTriggeredEvent = TouchingRect(buttonPressRect);
					}
					else
					{
						isTouchingPlayerAndTriggeredEvent = TouchingRect(otherCollision);
					}
				}
				else if (StartScriptOnEventTouch())
				{
					Rect otherCollision2 = new Rect(player.pos.X + player.collisionRect.X * 256, player.pos.Y + player.collisionRect.Y * 256, player.collisionRect.W * 256, player.collisionRect.H * 256);
					isTouchingPlayerAndTriggeredEvent = TouchingRect(otherCollision2);
				}
				else
				{
					isTouchingPlayerAndTriggeredEvent = false;
				}
			}
			CommonUpdate();
		}

		public int GetPixelBottom()
		{
			if (tileId > 384)
			{
				byte tilePriority = oneshotWindow.tileMapMan.GetTilePriority(tileId);
				if (tilePriority == 0)
				{
					return int.MinValue;
				}
				return pos.Y / 256 + collisionRect.Y + collisionRect.H + (tilePriority - 1) * 16;
			}
			if (alwaysOnBottom)
			{
				return int.MinValue;
			}
			if (alwaysOnTop)
			{
				return int.MaxValue;
			}
			return pos.Y / 256 + collisionRect.Y + collisionRect.H;
		}

		public Vec2 GetPixelPos()
		{
			return new Vec2(pos.X / 256, pos.Y / 256);
		}

		public bool HasMoved()
		{
			if (oldPos.X == pos.X)
			{
				return oldPos.Y != pos.Y;
			}
			return true;
		}

		public Vec2 GetCurrentTile()
		{
			return new Vec2(pos.X / 4096, pos.Y / 4096);
		}

		public Direction GetDirection()
		{
			return direction;
		}

		public void SetDirection(Direction dir)
		{
			direction = dir;
		}

		public Vec2 GetPos()
		{
			return pos;
		}

		public Vec2 GetOldPos()
		{
			return oldPos;
		}

		public void SetPos(Vec2 newPos)
		{
			pos = newPos;
		}

		public void SetPosTile(Vec2 tile)
		{
			pos = new Vec2(tile.X * 16 * 256 + 2048, tile.Y * 16 * 256 + 2048);
			moving = false;
			moveTarget = pos;
			vel = Vec2.Zero;
		}

		public Vec2 GetVel()
		{
			return vel;
		}

		public void SetVel(Vec2 newVel)
		{
			vel = newVel;
		}

		public Rect GetCollisionRect()
		{
			return collisionRect;
		}

		public void ResetCollisionDirection()
		{
			collisionDirection = 0;
		}

		public static int CollisionFlagMask(Direction d)
		{
			return 1 << (int)d / 2;
		}

		public void SetCollisionDirection(Direction d)
		{
			collisionDirection |= CollisionFlagMask(d);
		}

		public bool isCollisionDirectionSet(Direction d)
		{
			return (collisionDirection & CollisionFlagMask(d)) != 0;
		}

		public virtual void Draw(Vec2 camPos, GameTone tone)
		{
			if (!active)
			{
				return;
			}
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X;
			zero.Y = pos.Y / 256 - camPos.Y;
			zero.X -= spriteSize.X / 2;
			zero.Y -= spriteSize.Y - 8;
			if (jumping)
			{
				int num = jumpCount - jumpPeak;
				zero.Y -= (jumpPeak * jumpPeak - num * num) / 4;
			}
			if (zero.X > 320 || zero.X + spriteSize.X < 0 || zero.Y > 240 || zero.Y + spriteSize.Y < 0)
			{
				return;
			}
			float num2 = (float)opacity / 255f;
			if (!string.IsNullOrEmpty(npcSheet))
			{
				string textureName = "npc/" + npcSheet;
				Rect srcRect = new Rect(frameIndex * spriteSize.X, (int)(direction - 2) / 2 * spriteSize.Y, spriteSize.X, spriteSize.Y);
				Game1.gMan.MainBlit(textureName, zero, srcRect, num2, hue, blendMode, 2, tone);
				if (colorAbberate && oneshotWindow.menuMan.SettingsMenu.IsChromaAberrationEnabled)
				{
					Game1.gMan.MainBlit(textureName, zero + redOffset, srcRect, num2 * 0.5f, hue, GraphicsManager.BlendMode.Additive, 2, tone, 1f, 0f, 0f);
					Game1.gMan.MainBlit(textureName, zero + greenOffset, srcRect, num2 * 0.5f, hue, GraphicsManager.BlendMode.Additive, 2, tone, 0f, 1f, 0f);
					Game1.gMan.MainBlit(textureName, zero + blueOffset, srcRect, num2 * 0.5f, hue, GraphicsManager.BlendMode.Additive, 2, tone, 0f, 0f);
				}
				if (hasLightmap)
				{
					Vec2 vec = spriteSize;
					vec.X *= 2;
					vec.Y *= 2;
					zero.X *= 2;
					zero.Y *= 2;
					Game1.gMan.MainBlit("lightmaps/" + npcSheet, zero, new Rect(frameIndex * vec.X, (int)(direction - 2) / 2 * vec.Y, vec.X, vec.Y), num2, hue, GraphicsManager.BlendMode.Additive, 1);
				}
			}
			else if (tileId > 384)
			{
				oneshotWindow.tileMapMan.DrawTile(tileId - 384, zero, num2, tone);
			}
		}

		public void DrawCollision(Vec2 camPos)
		{
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X;
			zero.Y = pos.Y / 256 - camPos.Y;
			Rect boxRect = new Rect(zero.X + collisionRect.X, zero.Y + collisionRect.Y, collisionRect.W, collisionRect.H);
			Game1.gMan.ColorBoxBlit(boxRect, new GameColor(0, byte.MaxValue, byte.MaxValue, 120));
		}

		public void Initialize()
		{
			id = eventData.id;
			pos = new Vec2(eventData.x * 16 * 256 + 2048, eventData.y * 16 * 256 + 2048);
			vel = Vec2.Zero;
			collisionRect = new Rect(-8, -8, 16, 16);
			if (oneshotWindow.tileMapMan.IsTileSolid(eventData.x, eventData.y) && !(this is Player))
			{
				spawnedOnMapCollision = true;
				ignoreMapCollision = true;
			}
			Event.Page[] pages;
			switch (eventData.name.ToLowerInvariant())
			{
			case "!bigpool":
			case "!smallpool":
			case "!generator":
			case "!bed":
			{
				alwaysOnBottom = true;
				pages = eventData.pages;
				for (int i = 0; i < pages.Length; i++)
				{
					pages[i].always_on_bottom = true;
				}
				break;
			}
			case "!minecart":
				collisionRect = new Rect(-16, -8, 32, 16);
				break;
			default:
				alwaysOnBottom = false;
				break;
			}
			if (eventData.name.ToLowerInvariant().StartsWith("pixel "))
			{
				alwaysOnBottom = true;
				pages = eventData.pages;
				for (int i = 0; i < pages.Length; i++)
				{
					pages[i].always_on_bottom = true;
				}
			}
			HandleSpecialCollision();
			pages = eventData.pages;
			foreach (Event.Page page in pages)
			{
				if (page.trigger == 3 && page.condition.variable_valid && page.condition.variable_id == 4)
				{
					HandlesMapEventVar = true;
					break;
				}
			}
			if (eventData.pages.Length == 1)
			{
				Event.Page.Condition condition = eventData.pages[0].condition;
				if (!condition.self_switch_valid && !condition.switch1_valid && !condition.switch2_valid && !condition.variable_valid)
				{
					AlwaysActiveAndOnlyHasOnePage = true;
					active = true;
					SetActivePage(0);
				}
			}
			if (AlwaysActiveAndOnlyHasOnePage)
			{
				switch (eventData.pages[0].graphic.character_name.ToLowerInvariant())
				{
				case "sparkle_blue1":
				case "sparkle_blue2":
				case "water_waves_blue":
				case "water_waves_blue2":
				case "water_waves_blue_wide":
				{
					if (!ignoreNpcCollision)
					{
						Vec2 currentTile = GetCurrentTile();
						oneshotWindow.tileMapMan.SetTileSolid(currentTile.X, currentTile.Y);
					}
					ignoreMapCollision = true;
					ignoreNpcCollision = true;
					pages = eventData.pages;
					for (int i = 0; i < pages.Length; i++)
					{
						pages[i].through = true;
					}
					break;
				}
				}
				if (!HasScript() && !(this is Player) && !MovesEver())
				{
					NeverHash = true;
					if (!ignoreNpcCollision)
					{
						Vec2 currentTile2 = GetCurrentTile();
						oneshotWindow.tileMapMan.SetTileSolid(currentTile2.X, currentTile2.Y);
						ignoreNpcCollision = true;
						ignoreMapCollision = true;
					}
				}
			}
			else
			{
				CheckForActivePage();
			}
		}

		private bool MovesEver()
		{
			Event.Page[] pages = eventData.pages;
			foreach (Event.Page page in pages)
			{
				switch ((MoveType)page.move_type)
				{
				case MoveType.Random:
				case MoveType.Approach:
					return true;
				case MoveType.Custom:
				{
					MoveCommand[] array = page.move_route.list;
					for (int j = 0; j < array.Length; j++)
					{
						MoveCommandCode code = (MoveCommandCode)array[j].code;
						if ((uint)(code - 1) <= 13u)
						{
							return true;
						}
					}
					break;
				}
				}
			}
			return false;
		}

		public void HandleSpecialCollision()
		{
			float[,] array = null;
			int[,] array2 = null;
			switch (eventData.name.ToLowerInvariant())
			{
			case "!bigpool":
				array = new float[0, 2];
				array2 = new int[24, 2]
				{
					{ -4, -7 },
					{ -3, -7 },
					{ -2, -7 },
					{ -1, -7 },
					{ 0, -7 },
					{ 1, -7 },
					{ 2, -7 },
					{ 3, -7 },
					{ 4, -7 },
					{ -5, -6 },
					{ -5, -5 },
					{ -5, -4 },
					{ 5, -6 },
					{ 5, -5 },
					{ 5, -4 },
					{ -4, -3 },
					{ -3, -3 },
					{ 3, -3 },
					{ 4, -3 },
					{ -2, -2 },
					{ -1, -2 },
					{ 0, -2 },
					{ 1, -2 },
					{ 2, -2 }
				};
				break;
			case "!smallpool":
				array = new float[0, 2];
				array2 = new int[6, 2]
				{
					{ -1, 0 },
					{ 0, 0 },
					{ 1, 0 },
					{ -1, -1 },
					{ 0, -1 },
					{ 1, -1 }
				};
				pos.Y += 1024;
				break;
			case "!specialpool":
				array = new float[6, 2]
				{
					{ -1f, 0f },
					{ 0f, 0f },
					{ 1f, 0f },
					{ -1f, -1f },
					{ 0f, -1f },
					{ 1f, -1f }
				};
				break;
			case "!vendor":
				array = new float[3, 2]
				{
					{ -1f, 0f },
					{ 0f, 0f },
					{ 1f, 0f }
				};
				break;
			case "!glitch":
				array = new float[2, 2]
				{
					{ 0f, 0f },
					{ 0f, -1f }
				};
				break;
			case "!bed":
				array = new float[2, 2]
				{
					{ 0f, 0f },
					{ 0f, -1f }
				};
				break;
			case "!lens":
				array = new float[2, 2]
				{
					{ 0f, 0f },
					{ 0f, -1f }
				};
				break;
			}
			if (array != null)
			{
				for (int i = 0; i < array.Length / 2; i++)
				{
					Event @event = new Event();
					@event.id = oneshotWindow.tileMapMan.GetNextEntityID();
					@event.name = eventData.name + "_clone";
					@event.pages = new Event.Page[eventData.pages.Length];
					@event.x = eventData.x;
					@event.y = eventData.y;
					for (int j = 0; j < eventData.pages.Length; j++)
					{
						Event.Page page = new Event.Page();
						Event.Page page2 = eventData.pages[j];
						page.always_on_top = page2.always_on_top;
						page.condition = page2.condition;
						page.direction_fix = page2.direction_fix;
						page.list = page2.list;
						page.move_frequency = page2.move_frequency;
						page.move_route = page2.move_route;
						page.move_speed = page2.move_speed;
						page.move_type = 0;
						page.step_anime = page2.step_anime;
						page.through = page2.through;
						page.trigger = page2.trigger;
						page.walk_anime = page2.walk_anime;
						page.graphic = new Event.Page.Graphic();
						page.graphic.blend_type = 0;
						page.graphic.character_hue = 0;
						page.graphic.character_name = string.Empty;
						page.graphic.direction = 0;
						page.graphic.opacity = 255;
						page.graphic.pattern = 0;
						page.graphic.tile_id = 0;
						@event.pages[j] = page;
					}
					Entity entity = new Entity(oneshotWindow, @event);
					entity.pos.X = (int)(((float)eventData.x + array[i, 0] + 0.5f) * 256f * 16f);
					entity.pos.Y = (int)(((float)eventData.y + array[i, 1] + 0.5f) * 256f * 16f);
					oneshotWindow.tileMapMan.AddEntity(entity);
				}
				ignoreMapCollision = true;
				ignoreNpcCollision = true;
				for (int k = 0; k < eventData.pages.Length; k++)
				{
					eventData.pages[k].through = true;
				}
			}
			if (array2 != null)
			{
				Vec2 currentTile = GetCurrentTile();
				for (int l = 0; l < array2.Length / 2; l++)
				{
					oneshotWindow.tileMapMan.SetTileSolid(currentTile.X + array2[l, 0], currentTile.Y + array2[l, 1]);
				}
			}
		}

		public bool IsInForcedRoute()
		{
			if (active)
			{
				return moveRouteForcing;
			}
			return false;
		}

		public void ForceMoveRoute(MoveRoute newRoute)
		{
			if (storedMoveRoute == null)
			{
				storedMoveRoute = moveRoute;
				storedMoveRouteIndex = moveRouteIndex;
			}
			moveRoute = newRoute;
			moveRouteIndex = 0;
			moveRouteForcing = true;
			prelockDirection = Direction.None;
			waitTimer = 0;
			if (moveStuckTimer > 0)
			{
				CancelMoveDueToMoveStuck();
			}
			MoveTypeCustom();
		}

		public void CheckIfPlayFootstepSound()
		{
			if (frameIndex % 2 != 1)
			{
				return;
			}
			Tuple<List<string>, float> stepSounds = oneshotWindow.tileMapMan.GetStepSounds(new Vec2(pos.X / 256, pos.Y / 256));
			if (stepSounds.Item1 != null && stepSounds.Item1.Count > 0)
			{
				int index = MathHelper.Random(0, stepSounds.Item1.Count - 1);
				string sfxName = stepSounds.Item1[index];
				float vol = (float)MathHelper.Random(70, 90) * stepSounds.Item2 / 100f;
				float num = (float)MathHelper.Random(85, 115) / 100f;
				if (oneshotWindow.flagMan.IsFlagSet(112))
				{
					sfxName = "wheel_squeak1";
					num = (float)MathHelper.Random(120, 130) / 100f;
					if (wheelSqueak)
					{
						num += 0.1f;
					}
					wheelSqueak = !wheelSqueak;
				}
				Game1.soundMan.PlaySound(sfxName, vol, num);
			}
			EmitFootsplash();
		}

		public void NotifyScriptStarted(Entity entityThatTriggeredUs)
		{
			Lock();
		}

		public bool TouchingOtherEntity(Entity otherEntity)
		{
			Rect otherCollision = new Rect(otherEntity.pos.X + otherEntity.collisionRect.X * 256, otherEntity.pos.Y + otherEntity.collisionRect.Y * 256, otherEntity.collisionRect.W * 256, otherEntity.collisionRect.H * 256);
			return TouchingRect(otherCollision);
		}

		public bool TouchingRect(Rect otherCollision)
		{
			Rect rect = new Rect(pos.X + collisionRect.X * 256, pos.Y + collisionRect.Y * 256, collisionRect.W * 256, collisionRect.H * 256);
			if (rect.X <= otherCollision.X + otherCollision.W && rect.X + rect.W >= otherCollision.X && rect.Y <= otherCollision.Y + otherCollision.H)
			{
				return rect.Y + rect.H >= otherCollision.Y;
			}
			return false;
		}

		public bool TouchingOldRect(Rect otherCollision)
		{
			Rect rect = new Rect(oldPos.X + collisionRect.X * 256, oldPos.Y + collisionRect.Y * 256, collisionRect.W * 256, collisionRect.H * 256);
			if (rect.X <= otherCollision.X + otherCollision.W && rect.X + rect.W >= otherCollision.X && rect.Y <= otherCollision.Y + otherCollision.H)
			{
				return rect.Y + rect.H >= otherCollision.Y;
			}
			return false;
		}

		public void EntityCollision(Entity otherEntity)
		{
			Rect rect = new Rect(otherEntity.pos.X + otherEntity.collisionRect.X * 256, otherEntity.pos.Y + otherEntity.collisionRect.Y * 256, otherEntity.collisionRect.W * 256, otherEntity.collisionRect.H * 256);
			if (TouchingRect(rect))
			{
				oneshotWindow.tileMapMan.TileCollision(rect, this);
			}
		}

		public Vec2 GetSpriteSize()
		{
			return spriteSize;
		}

		public bool StartScriptOnPlayerTouch()
		{
			return eventTrigger == EventTrigger.PlayerTouch;
		}

		public bool StartScriptOnEventTouch()
		{
			return eventTrigger == EventTrigger.EventTouch;
		}

		public bool StartScriptOnPlayerAction()
		{
			return eventTrigger == EventTrigger.ActionButton;
		}

		public EntitySaveData GetEntitySaveData()
		{
			return new EntitySaveData
			{
				direction = direction,
				frameIndex = frameIndex,
				frameTimer = frameTimer,
				collisionDirection = collisionDirection,
				currentPageIndex = currentPageIndex,
				pos = pos,
				vel = vel,
				active = active,
				stopTimer = stopTimer,
				moveRouteIndex = moveRouteIndex,
				waitTimer = waitTimer,
				moveStuckTimer = moveStuckTimer,
				jumping = jumping,
				moving = moving,
				moveTarget = moveTarget,
				id = id,
				ignoreMapCollision = ignoreMapCollision,
				ignoreNpcCollision = ignoreNpcCollision,
				moveRoute = moveRoute,
				moveRouteForcing = moveRouteForcing,
				storedMoveRoute = storedMoveRoute
			};
		}

		public void LoadEntitySaveData(EntitySaveData esd, int? version)
		{
			if (!(this is Follower))
			{
				SetActivePage(esd.currentPageIndex);
				direction = esd.direction;
				if (!(this is Player))
				{
					frameIndex = esd.frameIndex;
					frameTimer = esd.frameTimer;
				}
				collisionDirection = esd.collisionDirection;
				pos = esd.pos;
				vel = esd.vel;
				active = esd.active;
				stopTimer = esd.stopTimer;
				moveRouteIndex = esd.moveRouteIndex;
				waitTimer = esd.waitTimer;
				moveStuckTimer = esd.moveStuckTimer;
				jumping = esd.jumping;
				moving = esd.moving;
				moveTarget = esd.moveTarget;
				ignoreMapCollision = esd.ignoreMapCollision;
				ignoreNpcCollision = esd.ignoreNpcCollision;
				if (version.HasValue && version.Value >= 1)
				{
					moveRoute = esd.moveRoute;
					moveRouteForcing = esd.moveRouteForcing;
					storedMoveRoute = esd.storedMoveRoute;
				}
			}
		}
	}
}

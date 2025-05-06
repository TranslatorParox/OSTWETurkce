using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Entities;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class Player : Entity
	{
		private const int WALK_SPEED = 256;

		private const int RUN_SPEED = 512;

		private int footprintEmitTimer;

		public const int NIKO_IN_MINECART_FLAG = 112;

		public const int NIKO_ONLY_MOVE_LEFT_RIGHT_FLAG = 146;

		private int roombaForceChangeDirTimer;

		private int roombaAutoChangeDirTimer;

		private const int ROOMBA_CHANGE_DIR_TIME = 5;

		private const int ROOMBA_AUTOCHANGE_DIR_TIME = 30;

		public Player(OneshotWindow osWindow, int id, float xPixel, float yPixel)
			: base(osWindow)
		{
			eventData = new Event
			{
				name = "player",
				pages = new Event.Page[1]
			};
			eventData.pages[0] = new Event.Page();
			currentPageIndex = 0;
			eventData.pages[0].condition = new Event.Page.Condition();
			eventData.pages[0].graphic = new Event.Page.Graphic();
			eventData.pages[0].move_speed = 2;
			eventData.pages[0].walk_anime = true;
			moveSpeed = 2;
			moveAnimation = true;
			Initialize();
			base.id = id;
			pos = new Vec2((int)(xPixel * 256f), (int)(yPixel * 256f));
			ignoreMapCollision = false;
			ignoreNpcCollision = false;
			opacity = 255;
		}

		public void MapChanged(int eId, float xPixel, float yPixel)
		{
			SetID(eId);
			pos = new Vec2((int)(xPixel * 256f), (int)(yPixel * 256f));
			if (moveRouteForcing && moving)
			{
				moving = false;
				moveTarget = pos;
			}
			HasBeenHashed = false;
		}

		public bool PlayerHasControl()
		{
			if (!oneshotWindow.tileMapMan.IsInScript() && !oneshotWindow.tileMapMan.IsMapTransitioning())
			{
				return !oneshotWindow.menuMan.IsMenuOpen();
			}
			return false;
		}

		public override void Update()
		{
			if (colorAbberate)
			{
				MathHelper.HandleAbberateUpdate(ref redAbberateTimer, ref redOffset);
				MathHelper.HandleAbberateUpdate(ref blueAbberateTimer, ref blueOffset);
				MathHelper.HandleAbberateUpdate(ref greenAbberateTimer, ref greenOffset);
			}
			if (moveRouteForcing)
			{
				CommonUpdate();
				return;
			}
			if (PlayerHasControl())
			{
				if (oneshotWindow.tileMapMan.CheckIfStartScript(this, Game1.inputMan.IsButtonPressed(InputManager.Button.OK), out var interactableEntity))
				{
					if (interactableEntity.StartScriptOnPlayerTouch() && interactableEntity.IgnoreNpcCollision())
					{
						Vec2 currentTile = GetCurrentTile();
						Vec2 currentTile2 = interactableEntity.GetCurrentTile();
						while (currentTile.X > currentTile2.X)
						{
							pos.X -= 256;
							currentTile = GetCurrentTile();
						}
						while (currentTile.X < currentTile2.X)
						{
							pos.X += 256;
							currentTile = GetCurrentTile();
						}
						while (currentTile.Y > currentTile2.Y)
						{
							pos.Y -= 256;
							currentTile = GetCurrentTile();
						}
						while (currentTile.Y < currentTile2.Y)
						{
							pos.Y += 256;
							currentTile = GetCurrentTile();
						}
					}
					else if (!interactableEntity.IgnoreNpcCollision() && (interactableEntity.StartScriptOnPlayerAction() || interactableEntity.StartScriptOnPlayerTouch()) && interactableEntity.GetName() != "!minecart")
					{
						Vec2 currentTile3 = GetCurrentTile();
						Vec2 currentTile4 = interactableEntity.GetCurrentTile();
						switch (direction)
						{
						case Direction.Down:
						case Direction.Up:
							while (currentTile3.X > currentTile4.X)
							{
								pos.X -= 256;
								currentTile3 = GetCurrentTile();
							}
							while (currentTile3.X < currentTile4.X)
							{
								pos.X += 256;
								currentTile3 = GetCurrentTile();
							}
							break;
						case Direction.Left:
						case Direction.Right:
							while (currentTile3.Y > currentTile4.Y)
							{
								pos.Y -= 256;
								currentTile3 = GetCurrentTile();
							}
							while (currentTile3.Y < currentTile4.Y)
							{
								pos.Y += 256;
								currentTile3 = GetCurrentTile();
							}
							break;
						}
					}
					interactableEntity.NotifyScriptStarted(this);
					oneshotWindow.tileMapMan.StartEvent(interactableEntity);
					vel = Vec2.Zero;
				}
				if (!oneshotWindow.tileMapMan.IsInScript() && !oneshotWindow.flagMan.IsFlagSet(11))
				{
					oldDirection = direction;
					bool flag = Game1.inputMan.IsButtonHeld(InputManager.Button.Run) ^ oneshotWindow.menuMan.SettingsMenu.IsDefaultMovementRun;
					bool flag2 = directionFix && direction == Direction.Down;
					bool flag3 = oneshotWindow.flagMan.IsFlagSet(112) || oneshotWindow.flagMan.IsFlagSet(146);
					Vec2 vec = pos - oldPos;
					if (Math.Abs(Game1.inputMan.MoveStickPos.X) > 0 || Math.Abs(Game1.inputMan.MoveStickPos.Y) > 0)
					{
						float num = (float)(-Game1.inputMan.MoveStickPos.Y) / 100f;
						float num2 = (float)Game1.inputMan.MoveStickPos.X / 100f;
						if (num < 0f && !flag2)
						{
							if (Math.Abs(num) > Math.Abs(num2))
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Up;
							}
							if (!flag3)
							{
								if (flag)
								{
									vel.Y = (int)(512f * num);
								}
								else
								{
									vel.Y = (int)(256f * num);
								}
							}
						}
						else if (num > 0f)
						{
							if (Math.Abs(num) > Math.Abs(num2))
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Down;
							}
							if (!flag3)
							{
								if (flag)
								{
									vel.Y = (int)(512f * num);
								}
								else
								{
									vel.Y = (int)(256f * num);
								}
							}
						}
						else
						{
							vel.Y = 0;
						}
						if (num2 < 0f && !flag2)
						{
							if (Math.Abs(num2) > Math.Abs(num))
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Left;
							}
							if (flag)
							{
								vel.X = (int)(512f * num2);
							}
							else
							{
								vel.X = (int)(256f * num2);
							}
						}
						else if (num2 > 0f && !flag2)
						{
							if (Math.Abs(num2) > Math.Abs(num))
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Right;
							}
							if (flag)
							{
								vel.X = (int)(512f * num2);
							}
							else
							{
								vel.X = (int)(256f * num2);
							}
						}
						else
						{
							vel.X = 0;
						}
					}
					else
					{
						if (Game1.inputMan.IsButtonHeld(InputManager.Button.Up) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down) && !flag2)
						{
							if (!isCollisionDirectionSet(Direction.Up) || vel.X == 0)
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Up;
							}
							if (!flag3)
							{
								if (flag)
								{
									vel.Y = -512;
								}
								else
								{
									vel.Y = -256;
								}
							}
						}
						else if (Game1.inputMan.IsButtonHeld(InputManager.Button.Down) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up))
						{
							if (!isCollisionDirectionSet(Direction.Down) || vel.X == 0)
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Down;
							}
							if (!flag3)
							{
								if (flag)
								{
									vel.Y = 512;
								}
								else
								{
									vel.Y = 256;
								}
							}
						}
						else
						{
							vel.Y = 0;
						}
						if (Game1.inputMan.IsButtonHeld(InputManager.Button.Left) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Right) && !flag2)
						{
							if (!isCollisionDirectionSet(Direction.Left) || vel.Y == 0)
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Left;
							}
							if (flag)
							{
								vel.X = -512;
							}
							else
							{
								vel.X = -256;
							}
						}
						else if (Game1.inputMan.IsButtonHeld(InputManager.Button.Right) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Left) && !flag2)
						{
							if (!isCollisionDirectionSet(Direction.Right) || vel.Y == 0)
							{
								roombaForceChangeDirTimer = 0;
								roombaAutoChangeDirTimer = 0;
								direction = Direction.Right;
							}
							if (flag)
							{
								vel.X = 512;
							}
							else
							{
								vel.X = 256;
							}
						}
						else
						{
							vel.X = 0;
						}
					}
					NormalizeWalkRunSpeed();
					if (oneshotWindow.flagMan.IsFlagSet(79))
					{
						switch (direction)
						{
						case Direction.Down:
							vel.Y = 512;
							vel.X = 0;
							if (isCollisionDirectionSet(Direction.Down))
							{
								roombaForceChangeDirTimer++;
								if (roombaForceChangeDirTimer > 5)
								{
									roombaForceChangeDirTimer = 0;
									roombaAutoChangeDirTimer = 0;
									direction = MathHelper.RandomChoice(new Direction[2]
									{
										Direction.Left,
										Direction.Right
									});
								}
							}
							break;
						case Direction.Up:
							vel.Y = -512;
							vel.X = 0;
							if (isCollisionDirectionSet(Direction.Up))
							{
								roombaForceChangeDirTimer++;
								if (roombaForceChangeDirTimer > 5)
								{
									roombaForceChangeDirTimer = 0;
									roombaAutoChangeDirTimer = 0;
									direction = MathHelper.RandomChoice(new Direction[2]
									{
										Direction.Left,
										Direction.Right
									});
								}
							}
							break;
						case Direction.Right:
							vel.X = 512;
							vel.Y = 0;
							if (isCollisionDirectionSet(Direction.Right))
							{
								roombaForceChangeDirTimer++;
								if (roombaForceChangeDirTimer > 5)
								{
									roombaForceChangeDirTimer = 0;
									roombaAutoChangeDirTimer = 0;
									direction = MathHelper.RandomChoice(new Direction[2]
									{
										Direction.Up,
										Direction.Down
									});
								}
							}
							break;
						case Direction.Left:
							vel.X = -512;
							vel.Y = 0;
							if (isCollisionDirectionSet(Direction.Left))
							{
								roombaForceChangeDirTimer++;
								if (roombaForceChangeDirTimer > 5)
								{
									roombaForceChangeDirTimer = 0;
									roombaAutoChangeDirTimer = 0;
									direction = MathHelper.RandomChoice(new Direction[2]
									{
										Direction.Up,
										Direction.Down
									});
								}
							}
							break;
						}
						roombaAutoChangeDirTimer++;
						if (roombaAutoChangeDirTimer > 30)
						{
							roombaAutoChangeDirTimer = 0;
							direction = MathHelper.RandomChoice(new Direction[4]
							{
								Direction.Up,
								Direction.Down,
								Direction.Left,
								Direction.Right
							});
						}
					}
					if (oldPos.X == pos.X && oldPos.Y == pos.Y)
					{
						frameIndex = 0;
						frameTimer = 0;
						footprintEmitTimer = 0;
					}
					else
					{
						frameTimer++;
						footprintEmitTimer++;
						int num3 = Math.Min((int)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y), 512);
						int num4 = 10 - (int)Math.Round((float)num3 * 4f / 512f);
						if (frameTimer >= num4)
						{
							frameIndex = (frameIndex + 1) % 4;
							frameTimer = 0;
							CheckIfPlayFootstepSound();
						}
						int num5 = (flag ? 8 : 16);
						if (footprintEmitTimer >= num5)
						{
							footprintEmitTimer = 0;
							EmitFootprint();
						}
					}
				}
				else
				{
					vel = Vec2.Zero;
				}
			}
			else
			{
				oldDirection = direction;
				if (moveAnimation)
				{
					frameIndex = 0;
				}
				vel = Vec2.Zero;
			}
			Movement();
			ResetCollisionDirection();
			if (!oneshotWindow.tileMapMan.Wrapping)
			{
				oneshotWindow.tileMapMan.KeepEntityOnMap(this);
			}
		}

		private void NormalizeWalkRunSpeed()
		{
			int num = ((Game1.inputMan.IsButtonHeld(InputManager.Button.Run) ^ oneshotWindow.menuMan.SettingsMenu.IsDefaultMovementRun) ? 512 : 256);
			int num2 = (int)Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);
			if (num2 > num)
			{
				vel.X = vel.X * num / num2;
				vel.Y = vel.Y * num / num2;
			}
		}
	}
}

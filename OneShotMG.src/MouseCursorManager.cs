using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class MouseCursorManager
	{
		public enum State
		{
			Normal = 0,
			Clickable = 1,
			Grabbable = 3,
			Holding = 4,
			Move = 5,
			NotAllowed = 6
		}

		private State state;

		private Rect frameRect;

		private int frameIndex;

		private int frameTimer;

		private Vec2 offset;

		private static int[] clickableWaitTimes = new int[2] { 20, 10 };

		public bool MouseHidden;

		private readonly Dictionary<State, Vec2> offsets = new Dictionary<State, Vec2>
		{
			{
				State.Normal,
				new Vec2(0, 0)
			},
			{
				State.Clickable,
				new Vec2(-3, 0)
			},
			{
				State.Grabbable,
				new Vec2(-3, -4)
			},
			{
				State.Holding,
				new Vec2(-3, -4)
			},
			{
				State.Move,
				new Vec2(-6, -6)
			},
			{
				State.NotAllowed,
				new Vec2(-5, -5)
			}
		};

		public Vec2 MousePos { get; private set; } = Vec2.Zero;

		public bool MouseClicked { get; private set; }

		public bool MouseHeld { get; private set; }

		public int MouseScrollSpeed { get; private set; }

		public MouseCursorManager()
		{
			frameRect = new Rect(0, 0, 16, 16);
		}

		public void SetMousePos(Vec2 mousePos)
		{
			MousePos = mousePos;
		}

		public void SetMouseScrollSpeed(int speed)
		{
			MouseScrollSpeed = speed;
		}

		public void SetState(State s)
		{
			if (state != State.Holding)
			{
				state = s;
			}
		}

		public void Update()
		{
			double num = (float)Game1.inputMan.MouseStickPos.X / 100f;
			num = ((!(num > 0.0)) ? Math.Floor(num) : Math.Ceiling(num));
			double num2 = (float)(-Game1.inputMan.MouseStickPos.Y) / 100f;
			num2 = ((!(num2 > 0.0)) ? Math.Floor(num2) : Math.Ceiling(num2));
			Vec2 mousePos = new Vec2(MousePos.X + (int)num, MousePos.Y + (int)num2);
			Vec2 vec = Game1.gMan.DrawScreenSize / 2;
			if (mousePos.X < 0)
			{
				mousePos.X = 0;
			}
			else if (mousePos.X >= vec.X)
			{
				mousePos.X = vec.X - 1;
			}
			if (mousePos.Y < 0)
			{
				mousePos.Y = 0;
			}
			else if (mousePos.Y >= vec.Y)
			{
				mousePos.Y = vec.Y - 1;
			}
			MousePos = mousePos;
			MouseClicked = Game1.inputMan.IsButtonPressed(InputManager.Button.MouseButton);
			if (MouseClicked)
			{
				Game1.soundMan.PlaySound("twm_mouse_click");
			}
			bool mouseHeld = MouseHeld;
			MouseHeld = Game1.inputMan.IsButtonHeld(InputManager.Button.MouseButton);
			if (!MouseHeld && mouseHeld)
			{
				Game1.soundMan.PlaySound("twm_mouse_unclick");
			}
			offset = offsets[state];
			if (state == State.Clickable)
			{
				frameTimer++;
				if (frameTimer >= clickableWaitTimes[frameIndex])
				{
					frameTimer = 0;
					frameIndex++;
				}
				if (frameIndex >= clickableWaitTimes.Length)
				{
					frameIndex = 0;
				}
			}
			else
			{
				frameTimer = 0;
				frameIndex = 0;
			}
			frameRect.X = 16 * (int)(frameIndex + state);
			state = State.Normal;
		}

		public void Draw()
		{
			if (!MouseHidden && (Game1.windowMan == null || !Game1.windowMan.HideAllUI))
			{
				Rect srcRect = frameRect;
				srcRect.Y += 16;
				Rect srcRect2 = frameRect;
				srcRect2.Y += 32;
				Game1.gMan.MainBlit("the_world_machine/cursor", MousePos + offset, srcRect, GameColor.Black, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
				Game1.gMan.MainBlit("the_world_machine/cursor", MousePos + offset, srcRect2, GameColor.White, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
			}
		}
	}
}

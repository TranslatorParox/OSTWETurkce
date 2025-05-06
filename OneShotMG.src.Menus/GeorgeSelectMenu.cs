using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class GeorgeSelectMenu : AbstractMenu
	{
		private OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 48;

		private int georgeIndex = 1;

		private const int GEORGE_MAX = 6;

		private const GraphicsManager.FontType MainMenuFont = GraphicsManager.FontType.GameSmall;

		public GeorgeSelectMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			float alpha = (float)opacity / 255f;
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(opacity * 155 / 255)));
			Game1.gMan.TextBlitCentered(GraphicsManager.FontType.GameSmall, new Vec2(160, 37), "PICK YOUR GEORGE (left/right)", white);
			Game1.gMan.MainBlit($"facepics/george{georgeIndex}", new Vec2(136, 120), alpha);
		}

		public override void Update()
		{
			switch (state)
			{
			case MenuState.Opening:
				opacity += 48;
				if (opacity >= 255)
				{
					opacity = 255;
					state = MenuState.Open;
				}
				break;
			case MenuState.Closing:
				opacity -= 48;
				if (opacity <= 0)
				{
					opacity = 0;
					state = MenuState.Closed;
				}
				break;
			case MenuState.Open:
				if (oneshotWindow.tileMapMan.IsInScript())
				{
					break;
				}
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left))
				{
					georgeIndex--;
					if (georgeIndex < 1)
					{
						georgeIndex = 1;
					}
					else
					{
						Game1.soundMan.PlaySound("menu_cursor");
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Right))
				{
					georgeIndex++;
					if (georgeIndex > 6)
					{
						georgeIndex = 6;
					}
					else
					{
						Game1.soundMan.PlaySound("menu_cursor");
					}
				}
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					Close();
				}
				break;
			}
		}

		public override void Close()
		{
			oneshotWindow.varMan.SetVariable(47, georgeIndex);
			state = MenuState.Closing;
			opacity = 255;
		}

		public override void Open()
		{
			Game1.soundMan.PlaySound("menu_decision");
			georgeIndex = oneshotWindow.varMan.GetVariable(47);
			if (georgeIndex <= 0)
			{
				georgeIndex = 1;
			}
			if (georgeIndex > 6)
			{
				georgeIndex = 6;
			}
			state = MenuState.Opening;
			opacity = 0;
		}
	}
}

using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.MessageBox;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class MainMenu : AbstractMenu
	{
		private OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 48;

		private int cursorAnimTimer;

		private const int CURSOR_ANIM_CYCLE_FRAMES = 32;

		private float cursorAlpha = 0.5f;

		private int cursorIndex;

		private const int CURSOR_MAX = 2;

		private const GraphicsManager.FontType MainMenuFont = GraphicsManager.FontType.Game;

		private const int CURSOR_MOVE_X = 202;

		private const int COMMON_EVENT_INSTRUCTIONS = 15;

		private TempTexture fastTravelTexture;

		private TempTexture notesTexture;

		private TempTexture settingsTexture;

		public MainMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			GameColor gameColor = white;
			gameColor.a /= 2;
			float num = (float)opacity / 255f;
			Game1.gMan.ColorBoxBlit(new Rect(10, 10, 300, 28), new GameColor(24, 12, 30, (byte)(opacity * 200 / 255)));
			TextBox.DrawWindowBorder(new Rect(8, 8, 304, 32), GraphicsManager.BlendMode.Normal, num);
			Game1.gMan.MainBlit("ui/main_menu_cursor", new Vec2(32 + cursorIndex * 202, 32), num * cursorAlpha, 0, GraphicsManager.BlendMode.Normal, 1);
			Game1.gMan.MainBlit(fastTravelTexture, new Vec2(116, 37), oneshotWindow.fastTravelMan.Unlocked ? white : gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			Game1.gMan.MainBlit(notesTexture, new Vec2(318, 37), white, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			Game1.gMan.MainBlit(settingsTexture, new Vec2(520, 37), white, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
		}

		public override void Update()
		{
			if (IsOpen())
			{
				if (fastTravelTexture == null || !fastTravelTexture.isValid)
				{
					fastTravelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("mainmenu_fasttravel"));
				}
				if (notesTexture == null || !notesTexture.isValid)
				{
					notesTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("mainmenu_notes"));
				}
				if (settingsTexture == null || !settingsTexture.isValid)
				{
					settingsTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("mainmenu_settings"));
				}
				fastTravelTexture.KeepAlive();
				notesTexture.KeepAlive();
				settingsTexture.KeepAlive();
			}
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
				cursorAnimTimer++;
				if (cursorAnimTimer >= 32)
				{
					cursorAnimTimer = 0;
				}
				cursorAlpha = 0.5f + (float)Math.Abs(16 - cursorAnimTimer) / 32f;
				if (oneshotWindow.tileMapMan.IsInScript())
				{
					break;
				}
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left))
				{
					cursorIndex--;
					if (cursorIndex < 0)
					{
						cursorIndex = 0;
					}
					else
					{
						Game1.soundMan.PlaySound("menu_cursor");
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Right))
				{
					cursorIndex++;
					if (cursorIndex > 2)
					{
						cursorIndex = 2;
					}
					else
					{
						Game1.soundMan.PlaySound("menu_cursor");
					}
				}
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					switch (cursorIndex)
					{
					case 0:
						if (oneshotWindow.fastTravelMan.Unlocked)
						{
							oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.FastTravelMenu);
							break;
						}
						oneshotWindow.tileMapMan.StartCommonEvent(4);
						Game1.soundMan.PlaySound("menu_buzzer");
						break;
					case 1:
						oneshotWindow.tileMapMan.StartCommonEvent(15);
						Close();
						break;
					case 2:
						oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.SettingsMenu);
						break;
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.MainMenu) || Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
				{
					Game1.soundMan.PlaySound("menu_cancel");
					Close();
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Inventory))
				{
					Close();
					oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.ItemMenu);
				}
				break;
			}
		}

		public override void Close()
		{
			state = MenuState.Closing;
			opacity = 255;
		}

		public override void Open()
		{
			Game1.soundMan.PlaySound("menu_decision");
			state = MenuState.Opening;
			opacity = 0;
			cursorAlpha = 0.5f;
			fastTravelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("mainmenu_fasttravel"));
			notesTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("mainmenu_notes"));
			settingsTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("mainmenu_settings"));
		}
	}
}

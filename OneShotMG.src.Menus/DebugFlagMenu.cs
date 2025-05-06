using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class DebugFlagMenu : AbstractMenu
	{
		private class FlagNamesData
		{
			public FlagName[] flags;
		}

		private class FlagName
		{
			public int id;

			public string name = string.Empty;
		}

		private Dictionary<int, string> flagNames;

		private OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 48;

		private int flagDrawIndex;

		private int flagSelectIndex;

		private const GraphicsManager.FontType MainMenuFont = GraphicsManager.FontType.Game;

		private const int FLAGS_X_POS = 20;

		private const int FLAGS_Y_POS = 60;

		private const int FLAG_HEIGHT = 20;

		private const int SELECTED_FLAG_X_OFFSET = 20;

		private const int ON_OFF_X_POS = 400;

		private const int FLAGS_PER_PAGE = 20;

		private const int CURSOR_TIME_TO_AUTO_TRIGGER = 20;

		private const int CURSOR_TIME_BETWEEN_AUTO_TRIGGER = 4;

		public DebugFlagMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			FlagNamesData flagNamesData = JsonConvert.DeserializeObject<FlagNamesData>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_flag_names.json"));
			flagNames = new Dictionary<int, string>();
			FlagName[] flags = flagNamesData.flags;
			foreach (FlagName flagName in flags)
			{
				flagNames[flagName.id] = flagName.name;
			}
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			GameColor white2 = GameColor.White;
			white2.a = (byte)(opacity / 2);
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(opacity * 155 / 255)));
			Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, new Vec2(320, 30), "Flag Manager", white, GraphicsManager.BlendMode.Normal, 1);
			for (int i = 0; i < 20; i++)
			{
				int num = i + flagDrawIndex;
				if (num < oneshotWindow.flagMan.TotalFlags)
				{
					Vec2 pixelPos = new Vec2(20, 60 + 20 * i);
					GameColor gColor = white2;
					if (num == flagSelectIndex)
					{
						pixelPos.X += 20;
						gColor = white;
					}
					string text = $"FLAG {num}";
					if (flagNames.TryGetValue(num, out var value))
					{
						text = text + " - " + value;
					}
					Game1.gMan.TextBlit(GraphicsManager.FontType.Game, pixelPos, text, gColor, GraphicsManager.BlendMode.Normal, 1);
					pixelPos.X += 400;
					Game1.gMan.TextBlit(GraphicsManager.FontType.Game, pixelPos, oneshotWindow.flagMan.IsFlagSet(num) ? "[ON]" : "[OFF]", gColor, GraphicsManager.BlendMode.Normal, 1);
					continue;
				}
				break;
			}
		}

		public override void Update()
		{
			bool flag = Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Right, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Left));
			bool flag2 = Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Left, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Right));
			bool flag3 = Game1.inputMan.IsButtonPressed(InputManager.Button.Up) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Up, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down));
			bool flag4 = Game1.inputMan.IsButtonPressed(InputManager.Button.Down) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Down, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up));
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
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
				{
					Game1.soundMan.PlaySound("menu_cancel");
					Close();
				}
				else if (flag4)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					flagSelectIndex++;
					if (flagSelectIndex >= oneshotWindow.flagMan.TotalFlags)
					{
						flagSelectIndex = 0;
					}
					UpdateFlagDrawIndex();
				}
				else if (flag3)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					flagSelectIndex--;
					if (flagSelectIndex < 0)
					{
						flagSelectIndex = oneshotWindow.flagMan.TotalFlags - 1;
					}
					UpdateFlagDrawIndex();
				}
				else if (flag2)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					flagSelectIndex -= 20;
					if (flagSelectIndex < 0)
					{
						flagSelectIndex = oneshotWindow.flagMan.TotalFlags - 1;
					}
					UpdateFlagDrawIndex();
				}
				else if (flag)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					flagSelectIndex += 20;
					if (flagSelectIndex >= oneshotWindow.flagMan.TotalFlags)
					{
						flagSelectIndex = 0;
					}
					UpdateFlagDrawIndex();
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					if (oneshotWindow.flagMan.IsFlagSet(flagSelectIndex))
					{
						oneshotWindow.flagMan.UnsetFlag(flagSelectIndex);
					}
					else
					{
						oneshotWindow.flagMan.SetFlag(flagSelectIndex);
					}
				}
				break;
			}
		}

		private void UpdateFlagDrawIndex()
		{
			flagDrawIndex = flagSelectIndex / 20 * 20;
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
		}
	}
}

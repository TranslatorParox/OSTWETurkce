using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class DebugVarMenu : AbstractMenu
	{
		private class VarNamesData
		{
			public VarName[] vars;
		}

		private class VarName
		{
			public int id;

			public string name = string.Empty;
		}

		private Dictionary<int, string> varNames;

		private OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 48;

		private int varDrawIndex;

		private int varSelectIndex;

		private const GraphicsManager.FontType MainMenuFont = GraphicsManager.FontType.Game;

		private const int VARS_X_POS = 20;

		private const int VARS_Y_POS = 60;

		private const int VARS_HEIGHT = 20;

		private const int SELECTED_VAR_X_OFFSET = 20;

		private const int VAR_VAL_X_POS = 400;

		private const int VARS_PER_PAGE = 20;

		private const int CURSOR_TIME_TO_AUTO_TRIGGER = 20;

		private const int CURSOR_TIME_BETWEEN_AUTO_TRIGGER = 4;

		private bool editingVar;

		public DebugVarMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			VarNamesData varNamesData = JsonConvert.DeserializeObject<VarNamesData>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_var_names.json"));
			varNames = new Dictionary<int, string>();
			VarName[] vars = varNamesData.vars;
			foreach (VarName varName in vars)
			{
				varNames[varName.id] = varName.name;
			}
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			GameColor white2 = GameColor.White;
			white2.a = (byte)(opacity / 2);
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(opacity * 155 / 255)));
			Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, new Vec2(320, 30), "Variable Manager", white, GraphicsManager.BlendMode.Normal, 1);
			for (int i = 0; i < 20; i++)
			{
				int num = i + varDrawIndex;
				if (num < oneshotWindow.varMan.TotalVariables)
				{
					Vec2 pixelPos = new Vec2(20, 60 + 20 * i);
					GameColor gColor = white2;
					if (num == varSelectIndex)
					{
						pixelPos.X += 20;
						gColor = ((!editingVar) ? white : white2);
					}
					string text = $"VAR {num}";
					if (varNames.TryGetValue(num, out var value))
					{
						text = text + " - " + value;
					}
					Game1.gMan.TextBlit(GraphicsManager.FontType.Game, pixelPos, text, gColor, GraphicsManager.BlendMode.Normal, 1);
					pixelPos.X += 400;
					bool flag = editingVar && num == varSelectIndex;
					if (flag)
					{
						gColor = white;
					}
					Game1.gMan.TextBlit(GraphicsManager.FontType.Game, pixelPos, flag ? ("<" + oneshotWindow.varMan.GetVariable(num) + ">") : oneshotWindow.varMan.GetVariable(num).ToString(), gColor, GraphicsManager.BlendMode.Normal, 1);
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
					if (editingVar)
					{
						editingVar = false;
					}
					else
					{
						Close();
					}
				}
				else if (flag4)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					if (editingVar)
					{
						oneshotWindow.varMan.SetVariable(varSelectIndex, oneshotWindow.varMan.GetVariable(varSelectIndex) - 10);
						break;
					}
					varSelectIndex++;
					if (varSelectIndex >= oneshotWindow.varMan.TotalVariables)
					{
						varSelectIndex = 0;
					}
					UpdateVarDrawIndex();
				}
				else if (flag3)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					if (editingVar)
					{
						oneshotWindow.varMan.SetVariable(varSelectIndex, oneshotWindow.varMan.GetVariable(varSelectIndex) + 10);
						break;
					}
					varSelectIndex--;
					if (varSelectIndex < 0)
					{
						varSelectIndex = oneshotWindow.varMan.TotalVariables - 1;
					}
					UpdateVarDrawIndex();
				}
				else if (flag2)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					if (editingVar)
					{
						oneshotWindow.varMan.SetVariable(varSelectIndex, oneshotWindow.varMan.GetVariable(varSelectIndex) - 1);
						break;
					}
					varSelectIndex -= 20;
					if (varSelectIndex < 0)
					{
						varSelectIndex = oneshotWindow.varMan.TotalVariables - 1;
					}
					UpdateVarDrawIndex();
				}
				else if (flag)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					if (editingVar)
					{
						oneshotWindow.varMan.SetVariable(varSelectIndex, oneshotWindow.varMan.GetVariable(varSelectIndex) + 1);
						break;
					}
					varSelectIndex += 20;
					if (varSelectIndex >= oneshotWindow.varMan.TotalVariables)
					{
						varSelectIndex = 0;
					}
					UpdateVarDrawIndex();
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					editingVar = !editingVar;
				}
				break;
			}
		}

		private void UpdateVarDrawIndex()
		{
			varDrawIndex = varSelectIndex / 20 * 20;
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

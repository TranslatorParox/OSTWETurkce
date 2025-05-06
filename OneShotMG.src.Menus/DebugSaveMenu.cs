using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.MessageBox;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class DebugSaveMenu : AbstractMenu
	{
		private class DebugSaveSlot
		{
			public int slot;

			public string name = string.Empty;

			public string npcsheet = string.Empty;

			public string playtime = string.Empty;

			public string mapName = string.Empty;

			public bool exists;
		}

		private List<DebugSaveSlot> saveSlots;

		private OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 48;

		private const int MAX_SAVES = 100;

		private const int SAVES_ON_SCREEN = 4;

		private int saveDrawIndex;

		private int saveSelectIndex;

		private const GraphicsManager.FontType MainMenuFont = GraphicsManager.FontType.Game;

		private const int SAVE_SLOT_HEIGHT = 50;

		private const int SAVE_SLOT_WIDTH = 300;

		private const int CURSOR_TIME_TO_AUTO_TRIGGER = 20;

		private const int CURSOR_TIME_BETWEEN_AUTO_TRIGGER = 4;

		public bool LoadSaves = true;

		public DebugSaveMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
		}

		private void updateSaveSlots()
		{
			saveSlots = new List<DebugSaveSlot>(100);
			for (int i = 1; i <= 100; i++)
			{
				DebugSaveSlot debugSaveSlot = new DebugSaveSlot();
				debugSaveSlot.slot = i;
				debugSaveSlot.name = $"File {i}";
				SaveData saveData = oneshotWindow.gameSaveMan.LoadDebugSaveData(i);
				if (saveData != null)
				{
					debugSaveSlot.exists = true;
					debugSaveSlot.mapName = oneshotWindow.tileMapMan.GetMapName(saveData.tileMapSaveData.currentMap);
					debugSaveSlot.npcsheet = saveData.tileMapSaveData.playerSheet;
					debugSaveSlot.playtime = TimeSpan.FromSeconds((float)saveData.playTimeFrameCount / 60f).ToString("hh\\:mm\\:ss");
				}
				saveSlots.Add(debugSaveSlot);
			}
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			float alpha = (float)opacity / 255f;
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(opacity * 155 / 255)));
			TextBox.DrawWindowBorder(new Rect(0, 0, 320, 40), GraphicsManager.BlendMode.Normal, alpha);
			Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, new Vec2(320, 30), LoadSaves ? "Debug Load" : "Debug Save", white, GraphicsManager.BlendMode.Normal, 1);
			for (int i = saveDrawIndex; i < saveDrawIndex + 4; i++)
			{
				DrawSaveSlot(new Vec2(0, 40 + 50 * (i - saveDrawIndex)), saveSlots[i]);
			}
		}

		private void DrawSaveSlot(Vec2 pos, DebugSaveSlot saveSlot)
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			float num = (float)opacity / 255f;
			if (saveSlot.slot != saveSelectIndex + 1)
			{
				num /= 2f;
				white.a /= 2;
				pos.X += 20;
			}
			Vec2 vec = pos * 2;
			TextBox.DrawWindowBorder(new Rect(pos.X, pos.Y, 300, 50), GraphicsManager.BlendMode.Normal, num);
			Game1.gMan.TextBlit(GraphicsManager.FontType.Game, vec + new Vec2(20, 10), saveSlot.name, white, GraphicsManager.BlendMode.Normal, 1);
			if (saveSlot.exists)
			{
				Game1.gMan.TextBlit(GraphicsManager.FontType.Game, vec + new Vec2(20, 35), saveSlot.mapName, white, GraphicsManager.BlendMode.Normal, 1);
				Game1.gMan.TextBlit(GraphicsManager.FontType.Game, vec + new Vec2(20, 60), saveSlot.playtime, white, GraphicsManager.BlendMode.Normal, 1);
				string textureName = "npc/" + saveSlot.npcsheet;
				Vec2 vec2 = Game1.gMan.TextureSize(textureName);
				Rect srcRect = new Rect(0, 0, vec2.X / 4, vec2.Y / 4);
				Game1.gMan.MainBlit(textureName, pos + new Vec2(290 - vec2.X / 4, 10), srcRect, num);
			}
			else
			{
				Game1.gMan.TextBlit(GraphicsManager.FontType.Game, vec + new Vec2(20, 40), "EMPTY", white, GraphicsManager.BlendMode.Normal, 1);
			}
		}

		public override void Update()
		{
			bool flag = Game1.inputMan.IsButtonPressed(InputManager.Button.Up) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Up, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down));
			bool flag2 = Game1.inputMan.IsButtonPressed(InputManager.Button.Down) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Down, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up));
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
				else if (flag2)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					saveSelectIndex++;
					if (saveSelectIndex >= 100)
					{
						saveSelectIndex = 0;
						saveDrawIndex = 0;
					}
					else if (saveSelectIndex - saveDrawIndex >= 4)
					{
						saveDrawIndex = saveSelectIndex - 4 + 1;
					}
				}
				else if (flag)
				{
					Game1.soundMan.PlaySound("menu_cursor");
					saveSelectIndex--;
					if (saveSelectIndex < 0)
					{
						saveSelectIndex = 99;
						saveDrawIndex = saveSelectIndex - 4 + 1;
					}
					else if (saveSelectIndex < saveDrawIndex)
					{
						saveDrawIndex = saveSelectIndex;
					}
				}
				else
				{
					if (!Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
					{
						break;
					}
					if (LoadSaves)
					{
						if (saveSlots[saveSelectIndex].exists)
						{
							oneshotWindow.gameSaveMan.LoadDebugSave(saveSlots[saveSelectIndex].slot);
							Game1.soundMan.PlaySound("menu_decision");
							Close();
						}
						else
						{
							Game1.soundMan.PlaySound("menu_buzzer");
						}
					}
					else
					{
						oneshotWindow.gameSaveMan.WriteDebugSave(saveSlots[saveSelectIndex].slot);
						Game1.soundMan.PlaySound("menu_decision");
						Close();
					}
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
			updateSaveSlots();
			Game1.soundMan.PlaySound("menu_decision");
			state = MenuState.Opening;
			opacity = 0;
		}
	}
}

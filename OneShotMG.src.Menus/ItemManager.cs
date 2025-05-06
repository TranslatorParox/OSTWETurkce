using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.MessageBox;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class ItemManager : AbstractMenu
	{
		private class DisplayedItem
		{
			public int itemId;

			public TempTexture textTexture;
		}

		private OneshotWindow oneshotWindow;

		private Dictionary<int, ItemData> itemsData;

		private bool[] currentInventory;

		private List<DisplayedItem> currentDisplayedItems;

		private const int INVENTORY_SIZE = 81;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 48;

		private const int ITEM_SLOT_WIDTH = 136;

		private const int ITEM_SLOT_HEIGHT = 16;

		private const int ITEM_GRID_START_X = 16;

		private const int ITEM_GRID_START_Y = 56;

		private const int ITEM_GRID_HORIZONTAL_PADDING = 16;

		private const int MAX_ITEMS_PER_PAGE = 12;

		private int firstDisplayedItemIndex;

		private int cursorAnimTimer;

		private const int CURSOR_ANIM_CYCLE_FRAMES = 32;

		private float cursorAlpha = 0.5f;

		private const int CURSOR_TIME_TO_AUTO_TRIGGER = 20;

		private const int CURSOR_TIME_BETWEEN_AUTO_TRIGGER = 4;

		public const int CURRENT_ACTIVE_ITEM_VAR_ID = 1;

		public const int COMBINED_ITEM_VAR_ID = 2;

		public const int DEBUG_SAVE_ITEM_ID = 54;

		public const int DEBUG_FLAG_VAR_ITEM_ID = 79;

		public const int DEBUG_PLIGHT_SKIP_ITEM_ID = 80;

		public const int CLOVER_ITEM_ID = 58;

		private Dictionary<Vec2, int> itemCombos;

		public const int COMBINE_ITEM_COMMON_EVENT = 1;

		private int cursorIndex = -1;

		private int currentItemId = -1;

		private int arrowTimer;

		private const int ARROW_TIMER_PERIOD = 20;

		private bool displaceArrows;

		private const GraphicsManager.FontType InventoryFont = GraphicsManager.FontType.Game;

		private int itemFlashOpacity;

		private int itemFlashWait = 160;

		private bool itemFlashFadeIn = true;

		private Vec2 itemComboSelection = new Vec2(-1, -1);

		private TempTexture itemDescTexture;

		private const int ITEM_DESC_TEXTURE_WIDTH = 580;

		private const int ITEM_NAME_TEXTURE_WIDTH = 232;

		public ItemManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			itemsData = new Dictionary<int, ItemData>();
			currentInventory = new bool[81];
			currentDisplayedItems = new List<DisplayedItem>();
			for (int i = 0; i < 81; i++)
			{
				currentInventory[i] = false;
			}
			ItemData[] items = JsonConvert.DeserializeObject<ItemsData>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_items.json")).items;
			foreach (ItemData ıtemData in items)
			{
				itemsData.Add(ıtemData.id, ıtemData);
			}
			initItemCombos();
		}

		private void initItemCombos()
		{
			itemCombos = new Dictionary<Vec2, int>();
			itemCombos.Add(new Vec2(3, 4), 5);
			itemCombos.Add(new Vec2(8, 9), 10);
			itemCombos.Add(new Vec2(10, 12), 13);
			itemCombos.Add(new Vec2(1, 13), 14);
			itemCombos.Add(new Vec2(1, 12), 102);
			itemCombos.Add(new Vec2(16, 20), 17);
			itemCombos.Add(new Vec2(11, 22), 101);
			itemCombos.Add(new Vec2(15, 22), 16);
			itemCombos.Add(new Vec2(11, 18), 103);
			itemCombos.Add(new Vec2(11, 19), 103);
			itemCombos.Add(new Vec2(1, 18), 104);
			itemCombos.Add(new Vec2(1, 19), 104);
			itemCombos.Add(new Vec2(25, 26), 30);
			itemCombos.Add(new Vec2(27, 56), 57);
			itemCombos.Add(new Vec2(32, 36), 33);
			itemCombos.Add(new Vec2(37, 38), 32);
			itemCombos.Add(new Vec2(36, 37), 100);
			itemCombos.Add(new Vec2(44, 46), 52);
			itemCombos.Add(new Vec2(46, 53), 51);
			itemCombos.Add(new Vec2(44, 45), 53);
			itemCombos.Add(new Vec2(44, 61), 71);
			itemCombos.Add(new Vec2(44, 62), 72);
			itemCombos.Add(new Vec2(44, 63), 73);
			itemCombos.Add(new Vec2(44, 64), 74);
			itemCombos.Add(new Vec2(44, 65), 75);
			itemCombos.Add(new Vec2(44, 66), 54);
			itemCombos.Add(new Vec2(44, 67), 77);
			itemCombos.Add(new Vec2(44, 68), 78);
			itemCombos.Add(new Vec2(44, 69), 79);
			itemCombos.Add(new Vec2(44, 70), 80);
			itemCombos.Add(new Vec2(45, 52), 51);
			itemCombos.Add(new Vec2(52, 61), 61);
			itemCombos.Add(new Vec2(52, 62), 62);
			itemCombos.Add(new Vec2(52, 63), 63);
			itemCombos.Add(new Vec2(52, 64), 64);
			itemCombos.Add(new Vec2(52, 65), 65);
			itemCombos.Add(new Vec2(52, 66), 54);
			itemCombos.Add(new Vec2(52, 67), 67);
			itemCombos.Add(new Vec2(52, 68), 68);
			itemCombos.Add(new Vec2(52, 69), 69);
			itemCombos.Add(new Vec2(52, 70), 70);
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			float alpha = (float)opacity / 255f;
			if (state == MenuState.Closed)
			{
				return;
			}
			Game1.gMan.ColorBoxBlit(new Rect(10, 10, 300, 28), new GameColor(24, 12, 30, (byte)(opacity * 200 / 255)));
			TextBox.DrawWindowBorder(new Rect(8, 8, 304, 32), GraphicsManager.BlendMode.Normal, alpha);
			Game1.gMan.MainBlit(itemDescTexture, new Vec2(320 - itemDescTexture.renderTarget.Width / 2, 36), white, 0, GraphicsManager.BlendMode.Normal, 1);
			Game1.gMan.ColorBoxBlit(new Rect(10, 50, 300, 108), new GameColor(24, 12, 30, (byte)(opacity * 200 / 255)));
			TextBox.DrawWindowBorder(new Rect(8, 48, 304, 112), GraphicsManager.BlendMode.Normal, alpha);
			if (firstDisplayedItemIndex > 0)
			{
				Game1.gMan.MainBlit("ui/item_box_arrows", new Vec2(152, displaceArrows ? 50 : 49), new Rect(0, 0, 16, 8), alpha);
			}
			if (firstDisplayedItemIndex + 12 < currentDisplayedItems.Count)
			{
				Game1.gMan.MainBlit("ui/item_box_arrows", new Vec2(152, displaceArrows ? 151 : 152), new Rect(0, 8, 16, 8), alpha);
			}
			for (int i = 0; i < 12; i++)
			{
				int num = i + firstDisplayedItemIndex;
				if (num < currentDisplayedItems.Count)
				{
					Vec2 pixelPos = new Vec2(16 + i % 2 * 152, 56 + i / 2 * 16);
					DrawItemSlot(pixelPos, currentDisplayedItems[num], alpha, white, num == cursorIndex);
					continue;
				}
				break;
			}
		}

		private void DrawItemSlot(Vec2 pixelPos, DisplayedItem dItem, float alpha, GameColor textColor, bool selected)
		{
			int itemId = dItem.itemId;
			if (selected)
			{
				Game1.gMan.MainBlit("ui/item_cursor", new Vec2(pixelPos.X * 2, pixelPos.Y * 2), alpha * cursorAlpha, 0, GraphicsManager.BlendMode.Normal, 1);
			}
			ItemData ıtemData = itemsData[itemId];
			if (!string.IsNullOrEmpty(ıtemData.icon_name))
			{
				Game1.gMan.MainBlit("item_icons/" + ıtemData.icon_name, new Vec2(pixelPos.X * 2, pixelPos.Y * 2), alpha, 0, GraphicsManager.BlendMode.Normal, 1);
			}
			if (ıtemData.common_event_id > 0)
			{
				textColor.r = 128;
				textColor.b = 128;
			}
			if (itemId == oneshotWindow.varMan.GetVariable(1) || itemId == itemComboSelection.X || itemId == itemComboSelection.Y)
			{
				textColor.r = 222;
				textColor.g = 134;
				textColor.b = 0;
			}
			Vec2 pixelPos2 = pixelPos;
			pixelPos2.X += 76;
			pixelPos2.X *= 2;
			pixelPos2.Y *= 2;
			pixelPos2.Y += 4;
			Game1.gMan.MainBlit(dItem.textTexture, pixelPos2, textColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
		}

		public override void Update()
		{
			if (IsOpen())
			{
				if (currentDisplayedItems.Count <= 0)
				{
					cursorIndex = -1;
				}
				else if (cursorIndex < 0)
				{
					cursorIndex = 0;
				}
				else if (cursorIndex >= currentDisplayedItems.Count)
				{
					cursorIndex = currentDisplayedItems.Count - 1;
				}
				int num = -1;
				if (cursorIndex >= 0)
				{
					num = currentDisplayedItems[cursorIndex].itemId;
				}
				if (currentItemId != num)
				{
					currentItemId = num;
					DrawItemDescriptionTexture();
				}
				itemDescTexture?.KeepAlive();
				foreach (DisplayedItem currentDisplayedItem in currentDisplayedItems)
				{
					if (currentDisplayedItem.textTexture == null || !currentDisplayedItem.textTexture.isValid)
					{
						DrawItemNameTexture(currentDisplayedItem);
					}
					currentDisplayedItem.textTexture?.KeepAlive();
				}
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
					currentDisplayedItems.Clear();
				}
				break;
			case MenuState.Open:
				arrowTimer++;
				if (arrowTimer >= 20)
				{
					arrowTimer = 0;
					displaceArrows = !displaceArrows;
				}
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
				itemComboSelection = new Vec2(-1, -1);
				HandleCursorInput();
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					if (cursorIndex < 0 || cursorIndex >= currentDisplayedItems.Count)
					{
						Game1.soundMan.PlaySound("menu_buzzer");
						break;
					}
					int itemId = currentDisplayedItems[cursorIndex].itemId;
					if (itemsData[itemId].common_event_id > 0)
					{
						Game1.soundMan.PlaySound("menu_decision");
						oneshotWindow.tileMapMan.StartCommonEvent(itemsData[itemId].common_event_id);
						break;
					}
					int variable = oneshotWindow.varMan.GetVariable(1);
					if (itemId == variable)
					{
						Game1.soundMan.PlaySound("menu_cancel");
						oneshotWindow.varMan.SetVariable(1, 0);
						break;
					}
					Game1.soundMan.PlaySound("menu_decision");
					if (variable == 0)
					{
						oneshotWindow.varMan.SetVariable(1, itemId);
						break;
					}
					Vec2 key = ((variable > itemId) ? new Vec2(itemId, variable) : new Vec2(variable, itemId));
					if (!itemCombos.TryGetValue(key, out var value))
					{
						value = 0;
					}
					itemComboSelection = key;
					oneshotWindow.varMan.SetVariable(2, value);
					oneshotWindow.varMan.SetVariable(1, 0);
					oneshotWindow.tileMapMan.StartCommonEvent(1);
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Inventory) || Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
				{
					Game1.soundMan.PlaySound("menu_cancel");
					Close();
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.MainMenu))
				{
					Close();
					oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.MainMenu);
				}
				break;
			}
		}

		private void HandleCursorInput()
		{
			if (currentDisplayedItems.Count > 0)
			{
				int num = cursorIndex;
				bool num2 = Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Right, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Left));
				bool flag = Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Left, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Right));
				bool flag2 = Game1.inputMan.IsButtonPressed(InputManager.Button.Up) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Up, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down));
				bool flag3 = Game1.inputMan.IsButtonPressed(InputManager.Button.Down) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Down, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up));
				if (num2)
				{
					num++;
				}
				else if (flag)
				{
					num--;
				}
				if (flag2)
				{
					num -= 2;
				}
				else if (flag3)
				{
					num += 2;
				}
				if (num < 0)
				{
					num = 0;
				}
				else if (num >= currentDisplayedItems.Count)
				{
					num = currentDisplayedItems.Count - 1;
				}
				if (num != cursorIndex)
				{
					cursorIndex = num;
					Game1.soundMan.PlaySound("menu_cursor");
				}
				while (cursorIndex < firstDisplayedItemIndex)
				{
					firstDisplayedItemIndex -= 2;
				}
				while (cursorIndex >= firstDisplayedItemIndex + 12)
				{
					firstDisplayedItemIndex += 2;
				}
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
			currentDisplayedItems = new List<DisplayedItem>();
			for (int i = 0; i < 81; i++)
			{
				if (currentInventory[i])
				{
					AddDisplayedItem(i);
				}
			}
			DrawItemDescriptionTexture();
		}

		private void AddDisplayedItem(int id, bool insertion = false)
		{
			DisplayedItem displayedItem = new DisplayedItem
			{
				itemId = id
			};
			DrawItemNameTexture(displayedItem);
			if (insertion)
			{
				int num = -1;
				for (int i = 0; i < currentDisplayedItems.Count; i++)
				{
					if (currentDisplayedItems[i].itemId > id)
					{
						num = i;
						break;
					}
				}
				if (num >= 0)
				{
					currentDisplayedItems.Insert(num, displayedItem);
				}
				else
				{
					currentDisplayedItems.Add(displayedItem);
				}
			}
			else
			{
				currentDisplayedItems.Add(displayedItem);
			}
		}

		private void RemoveDisplayedItem(int id)
		{
			int num = -1;
			for (int i = 0; i < currentDisplayedItems.Count; i++)
			{
				if (currentDisplayedItems[i].itemId == id)
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				currentDisplayedItems.RemoveAt(num);
			}
		}

		private void DrawItemDescriptionTexture()
		{
			string text = Game1.languageMan.GetTWMLocString("inventory_no_items_item_description");
			if (currentItemId >= 0)
			{
				text = Game1.languageMan.GetItemLocString(currentItemId, LanguageManager.ItemStringType.description, itemsData[currentItemId].description);
			}
			itemDescTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, text, 570);
		}

		private void DrawItemNameTexture(DisplayedItem dItem)
		{
			string ıtemLocString = Game1.languageMan.GetItemLocString(dItem.itemId, LanguageManager.ItemStringType.name, itemsData[dItem.itemId].name);
			dItem.textTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, ıtemLocString, 232);
		}

		public bool HasItem(int itemId)
		{
			if (itemId < 0 || itemId >= 81)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"Tried to check item with bad item id {itemId}");
			}
			return currentInventory[itemId];
		}

		public void RemoveItem(int itemId)
		{
			if (itemId < 0 || itemId >= 81)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"Tried to remove item with bad item id {itemId}");
			}
			if (currentInventory[itemId])
			{
				currentInventory[itemId] = false;
				if (IsOpen())
				{
					RemoveDisplayedItem(itemId);
				}
			}
		}

		public void AddItem(int itemId)
		{
			if (itemId < 0 || itemId >= 81)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"Tried to add item with bad item id {itemId}");
			}
			if (!currentInventory[itemId])
			{
				currentInventory[itemId] = true;
				if (IsOpen())
				{
					AddDisplayedItem(itemId, insertion: true);
				}
			}
		}

		public void UpdateSelectedItemIcon()
		{
			if (oneshotWindow.varMan.GetVariable(1) != 58)
			{
				return;
			}
			if (itemFlashWait > 0)
			{
				itemFlashWait--;
			}
			else if (itemFlashFadeIn)
			{
				itemFlashOpacity += 6;
				if (itemFlashOpacity >= 255)
				{
					itemFlashOpacity = 255;
					itemFlashFadeIn = false;
					itemFlashWait = 40;
				}
			}
			else
			{
				itemFlashOpacity -= 6;
				if (itemFlashOpacity <= 0)
				{
					itemFlashOpacity = 0;
					itemFlashFadeIn = true;
					itemFlashWait = 160;
				}
			}
		}

		public void DrawSelectedItemIcon()
		{
			int variable = oneshotWindow.varMan.GetVariable(1);
			if (variable <= 0)
			{
				return;
			}
			ItemData ıtemData = itemsData[variable];
			if (!string.IsNullOrEmpty(ıtemData.icon_name))
			{
				string text = "item_icons/" + ıtemData.icon_name;
				Vec2 vec = Game1.gMan.TextureSize(text);
				Game1.gMan.MainBlit(text, new Vec2(320 - vec.X, 240 - vec.Y));
				if (variable == 58)
				{
					string textureName = text + "2";
					float alpha = (float)itemFlashOpacity / 255f;
					Game1.gMan.MainBlit(textureName, new Vec2(320 - vec.X, 240 - vec.Y), alpha);
				}
			}
		}

		public bool[] GetRawInventoryData()
		{
			return currentInventory;
		}

		public void SetRawInventoryData(bool[] newInventory)
		{
			currentInventory = newInventory;
		}

		public void LoseAllItems()
		{
			for (int i = 0; i < 81; i++)
			{
				if (i != 54 && i != 79 && i != 80)
				{
					currentInventory[i] = false;
				}
			}
		}
	}
}

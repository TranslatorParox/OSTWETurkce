using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;

namespace OneShotMG.src.Menus
{
	public class MenuManager
	{
		public enum Menus
		{
			ItemMenu,
			MainMenu,
			FastTravelMenu,
			SettingsMenu,
			GeorgeMenu,
			DebugSaveMenu,
			DebugFlagMenu,
			DebugVarMenu
		}

		private readonly OneshotWindow oneshotWindow;

		private AbstractMenu currentMenu;

		private AbstractMenu nextMenu;

		public ItemManager ItemMan { get; private set; }

		private MainMenu MainMenu { get; }

		private FastTravelMenu FastTravelMenu { get; }

		private GeorgeSelectMenu GeorgeSelectMenu { get; }

		public DebugSaveMenu DebugSaveMenu { get; private set; }

		private DebugFlagMenu DebugFlagMenu { get; }

		private DebugVarMenu DebugVarMenu { get; }

		public SettingsMenu SettingsMenu { get; private set; }

		public MenuManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			ItemMan = new ItemManager(oneshotWindow);
			MainMenu = new MainMenu(oneshotWindow);
			FastTravelMenu = new FastTravelMenu(oneshotWindow);
			SettingsMenu = new SettingsMenu(oneshotWindow);
			GeorgeSelectMenu = new GeorgeSelectMenu(oneshotWindow);
			DebugSaveMenu = new DebugSaveMenu(oneshotWindow);
			DebugFlagMenu = new DebugFlagMenu(oneshotWindow);
			DebugVarMenu = new DebugVarMenu(oneshotWindow);
			currentMenu = null;
			nextMenu = null;
		}

		public void ShowMenu(Menus selection)
		{
			switch (selection)
			{
			case Menus.ItemMenu:
				nextMenu = ItemMan;
				break;
			case Menus.MainMenu:
				nextMenu = MainMenu;
				break;
			case Menus.FastTravelMenu:
				nextMenu = FastTravelMenu;
				break;
			case Menus.SettingsMenu:
				nextMenu = SettingsMenu;
				break;
			case Menus.GeorgeMenu:
				nextMenu = GeorgeSelectMenu;
				break;
			case Menus.DebugSaveMenu:
				nextMenu = DebugSaveMenu;
				break;
			case Menus.DebugFlagMenu:
				nextMenu = DebugFlagMenu;
				break;
			case Menus.DebugVarMenu:
				nextMenu = DebugVarMenu;
				break;
			default:
				throw new ArgumentException($"No menu available for {selection}");
			}
			if (currentMenu == nextMenu)
			{
				nextMenu = null;
				return;
			}
			currentMenu?.Close();
			nextMenu.Open();
		}

		public void Update()
		{
			currentMenu?.Update();
			nextMenu?.Update();
			if (currentMenu != null && currentMenu.IsOpen())
			{
				return;
			}
			if (nextMenu != null)
			{
				currentMenu = nextMenu;
				nextMenu = null;
				return;
			}
			currentMenu = null;
			if (!oneshotWindow.tileMapMan.IsInScript() && !oneshotWindow.titleScreenMan.IsOpen())
			{
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.DEBUG_CHANGE_GEORGE))
				{
					ShowMenu(Menus.DebugVarMenu);
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Inventory))
				{
					ShowMenu(Menus.ItemMenu);
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.MainMenu))
				{
					ShowMenu(Menus.MainMenu);
				}
			}
		}

		public void Draw()
		{
			currentMenu?.Draw();
			nextMenu?.Draw();
		}

		public bool IsMenuOpen()
		{
			if (currentMenu == null || !currentMenu.IsOpen())
			{
				if (nextMenu != null)
				{
					return nextMenu.IsOpen();
				}
				return false;
			}
			return true;
		}
	}
}

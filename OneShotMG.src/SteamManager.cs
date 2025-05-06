using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.TWM.Filesystem;
using Steamworks;

namespace OneShotMG.src
{
	public class SteamManager
	{
		private InputHandle_t[] connectedControllers;

		private int numberOfConnectedControllers;

		private InputActionSetHandle_t actionSetHandle;

		private InputActionSetHandle_t emptyActionSetHandle;

		private Dictionary<InputManager.Button, InputDigitalActionHandle_t> buttonHandleMap;

		private Dictionary<InputManager.Button, InputAnalogActionHandle_t> stickHandleMap;

		private const string DEMO_INI_FILENAME = "demo.ini";

		public bool IsOnSteamDeck { get; private set; }

		public string SteamUserName { get; private set; } = "steamUserName";

		public bool IsDemoMode { get; private set; }

		public bool IsTimedDemo { get; private set; }

		public int DemoTimeLimitMinutes { get; private set; } = 20;

		public int DemoTimeRemaining { get; private set; }

		public SteamManager()
		{
			SteamAPI.Init();
			IsOnSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
			SteamUserName = SteamFriends.GetPersonaName();
			SteamUserStats.RequestCurrentStats();
			inputInit();
			if (!File.Exists("demo.ini"))
			{
				return;
			}
			IniData ıniData = new FileIniDataParser().ReadFile("demo.ini");
			if (!bool.TryParse(ıniData["demo"]["enabled"], out var result))
			{
				return;
			}
			IsDemoMode = result;
			if (int.TryParse(ıniData["demo"]["minutes"], out var result2))
			{
				DemoTimeLimitMinutes = result2;
				if (DemoTimeLimitMinutes > 0)
				{
					IsTimedDemo = true;
				}
				else
				{
					DemoTimeLimitMinutes = 0;
					IsTimedDemo = false;
				}
				ResetDemoTimer();
			}
		}

		private void ResetDemoTimer()
		{
			DemoTimeRemaining = DemoTimeLimitMinutes * 60 * 60;
		}

		public void ShutDown()
		{
			SteamAPI.Shutdown();
		}

		private void inputInit()
		{
			SteamInput.Init(bExplicitlyCallRunFrame: false);
			SteamInput.EnableDeviceCallbacks();
			actionSetHandle = SteamInput.GetActionSetHandle("GameControls");
			connectedControllers = new InputHandle_t[16];
			numberOfConnectedControllers = SteamInput.GetConnectedControllers(connectedControllers);
			if (numberOfConnectedControllers >= 1)
			{
				SteamInput.ActivateActionSet(connectedControllers[0], actionSetHandle);
			}
			buttonHandleMap = new Dictionary<InputManager.Button, InputDigitalActionHandle_t>();
			stickHandleMap = new Dictionary<InputManager.Button, InputAnalogActionHandle_t>();
		}

		private InputDigitalActionHandle_t ButtonToDigitalActionHandle(InputManager.Button b)
		{
			if (buttonHandleMap.TryGetValue(b, out var value))
			{
				return value;
			}
			string empty = string.Empty;
			switch (b)
			{
			case InputManager.Button.Up:
				empty = "up";
				break;
			case InputManager.Button.Down:
				empty = "down";
				break;
			case InputManager.Button.Left:
				empty = "left";
				break;
			case InputManager.Button.Right:
				empty = "right";
				break;
			case InputManager.Button.MouseButton:
				empty = "click_mouse";
				break;
			case InputManager.Button.OK:
				empty = "ok";
				break;
			case InputManager.Button.Cancel:
				empty = "cancel";
				break;
			case InputManager.Button.MainMenu:
				empty = "main_menu";
				break;
			case InputManager.Button.Inventory:
				empty = "inventory";
				break;
			case InputManager.Button.Run:
				empty = "run";
				break;
			case InputManager.Button.MashText:
				empty = "skip_text";
				break;
			case InputManager.Button.FullScreen:
				empty = "fullscreen";
				break;
			default:
				return new InputDigitalActionHandle_t(0uL);
			}
			value = SteamInput.GetDigitalActionHandle(empty);
			if (value.m_InputDigitalActionHandle != 0L)
			{
				buttonHandleMap.Add(b, value);
			}
			return value;
		}

		public string GetLangCode()
		{
			switch (SteamApps.GetCurrentGameLanguage())
			{
			case "english":
				return "en";
			case "spanish":
			case "latam":
				return "es";
			case "french":
				return "fr";
			case "italian":
				return "it";
			case "japanese":
				return "ja";
			case "koreana":
				return "ko";
			case "portuguese":
			case "brazilian":
				return "pt_br";
			case "russian":
				return "ru";
			case "schinese":
				return "zh_cn";
			case "tchinese":
				return "zh_cht";
			default:
				return string.Empty;
			}
		}

		private InputAnalogActionHandle_t ButtonToAnalogActionHandle(InputManager.Button b)
		{
			if (stickHandleMap.TryGetValue(b, out var value))
			{
				return value;
			}
			string empty = string.Empty;
			switch (b)
			{
			case InputManager.Button.Move:
				empty = "MoveCharacter";
				break;
			case InputManager.Button.MoveMouse:
				empty = "MoveMouse";
				break;
			default:
				return new InputAnalogActionHandle_t(0uL);
			}
			value = SteamInput.GetAnalogActionHandle(empty);
			if (value.m_InputAnalogActionHandle != 0L)
			{
				stickHandleMap.Add(b, value);
			}
			return value;
		}

		public void DemoUpdate()
		{
			if (IsTimedDemo && Game1.bootMan.SequenceComplete)
			{
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.DEBUG_END_DEMO))
				{
					DemoTimeRemaining = 300;
				}
				if (DemoTimeRemaining > 0)
				{
					DemoTimeRemaining--;
				}
				else if (DemoTimeRemaining == 0)
				{
					Game1.windowMan.EndDemo();
				}
			}
		}

		public void DepleteDemoTimer()
		{
			DemoTimeRemaining = -1;
		}

		public void RunFrame()
		{
			int num = numberOfConnectedControllers;
			if (actionSetHandle == emptyActionSetHandle)
			{
				actionSetHandle = SteamInput.GetActionSetHandle("GameControls");
			}
			SteamAPI.RunCallbacks();
			SteamInput.RunFrame();
			numberOfConnectedControllers = SteamInput.GetConnectedControllers(connectedControllers);
			if (numberOfConnectedControllers >= 1)
			{
				SteamInput.ActivateActionSet(connectedControllers[0], actionSetHandle);
			}
			else if (num >= 1 && Game1.windowMan != null)
			{
				Game1.windowMan.ShowModalWindow(ModalWindow.ModalType.Error, "error_controller_disconnect");
			}
		}

		public bool IsSteamInputControllerConnected()
		{
			return numberOfConnectedControllers >= 1;
		}

		public InputManager.ButtonGlyphInfo GetGlyphInfo(InputManager.Button b)
		{
			List<EInputActionOrigin> originsForButton = GetOriginsForButton(b);
			if (originsForButton.Count > 0)
			{
				int index = Game1.GlobalFrameCounter / 60 % originsForButton.Count;
				EInputActionOrigin origin = originsForButton[index];
				return originToGlyphInfo(origin);
			}
			return Game1.inputMan.UnknownButton;
		}

		private List<EInputActionOrigin> GetOriginsForButton(InputManager.Button b)
		{
			List<EInputActionOrigin> list = new List<EInputActionOrigin>();
			EInputActionOrigin[] array = new EInputActionOrigin[8];
			InputDigitalActionHandle_t digitalActionHandle = ButtonToDigitalActionHandle(b);
			if (digitalActionHandle.m_InputDigitalActionHandle != 0L)
			{
				int digitalActionOrigins = SteamInput.GetDigitalActionOrigins(connectedControllers[0], actionSetHandle, digitalActionHandle, array);
				for (int i = 0; i < digitalActionOrigins; i++)
				{
					list.Add(array[i]);
				}
			}
			else
			{
				InputAnalogActionHandle_t analogActionHandle = ButtonToAnalogActionHandle(b);
				if (analogActionHandle.m_InputAnalogActionHandle != 0L)
				{
					int analogActionOrigins = SteamInput.GetAnalogActionOrigins(connectedControllers[0], actionSetHandle, analogActionHandle, array);
					for (int j = 0; j < analogActionOrigins; j++)
					{
						list.Add(array[j]);
					}
				}
			}
			return list;
		}

		public void DemoResetCheck()
		{
			if (IsTimedDemo)
			{
				new FilesystemSaveManager().DeleteFSAndDesktopSave();
				InputMapping.DeleteInputMapping();
				Game1.inputMan.LoadDefaultInputMap();
				GameSaveManager.DeleteAllOneshotData();
				Game1.gMan.SetDemoDefaults();
				Game1.soundMan.SFXVol = 100;
				Game1.soundMan.BGMVol = 100;
				ResetDemoTimer();
			}
		}

		private InputManager.ButtonGlyphInfo originToGlyphInfo(EInputActionOrigin origin)
		{
			string text = string.Empty;
			switch (origin)
			{
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_PS4_LeftStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_LeftStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_LeftStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_LeftStick_Move:
				text = "pad_lstick";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_RightStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_RightStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_RightStick_Move:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_RightStick_Move:
				text = "pad_rstick";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_A:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_A:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_A:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_A:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_A:
				text = "pad_a";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_B:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_B:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_B:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_B:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_B:
				text = "pad_b";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_X:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_X:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_X:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_X:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_X:
				text = "pad_x";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_Y:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_Y:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_Y:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_Y:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_Y:
				text = "pad_y";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_X:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_X:
				text = "pad_x";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_Circle:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_Circle:
				text = "pad_circle";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_Triangle:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_Triangle:
				text = "pad_triangle";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_Square:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_Square:
				text = "pad_square";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_LeftBumper:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_LeftBumper:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_L1:
				text = "pad_l1";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_RightBumper:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_RightBumper:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_R1:
				text = "pad_r1";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_LeftTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_PS4_LeftTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_LeftTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_LeftTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_L2_SoftPull:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_L2:
				text = "pad_l2";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_RightTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_PS4_RightTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_RightTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_PS5_RightTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_R2_SoftPull:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_R2:
				text = "pad_r2";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftBumper:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftBumper:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftBumper:
				text = "pad_lbutton";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_RightBumper:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightBumper:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_RightBumper:
				text = "pad_rbutton";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_LeftTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_LeftTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftTrigger_Click:
				text = "pad_ltrigger";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_RightTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_SteamController_RightTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_RightTrigger_Click:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_RightTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_XBox360_RightTrigger_Click:
				text = "pad_rtrigger";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_Switch_LeftBumper:
				text = "pad_nnx_l";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_Switch_RightBumper:
				text = "pad_nnx_r";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_Switch_LeftTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_LeftTrigger_Click:
				text = "pad_zl";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_Switch_RightTrigger_Pull:
			case EInputActionOrigin.k_EInputActionOrigin_Switch_RightTrigger_Click:
				text = "pad_zr";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_Share:
				text = "pad_ps4_share";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_PS4_Options:
				text = "pad_ps4_options";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_View:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_View:
				text = "pad_back";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_XBoxOne_Menu:
			case EInputActionOrigin.k_EInputActionOrigin_SteamDeck_Menu:
				text = "pad_burger";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_Switch_Plus:
				text = "pad_plus";
				break;
			case EInputActionOrigin.k_EInputActionOrigin_Switch_Minus:
				text = "pad_minus";
				break;
			}
			if (text == string.Empty)
			{
				string glyphPNGForActionOrigin = SteamInput.GetGlyphPNGForActionOrigin(origin, ESteamInputGlyphSize.k_ESteamInputGlyphSize_Medium, 0u);
				if (!string.IsNullOrEmpty(glyphPNGForActionOrigin) && Game1.gMan.TextureExists(glyphPNGForActionOrigin, TextureCache.CacheType.SteamGlyphes))
				{
					return new InputManager.ButtonGlyphInfo(glyphPNGForActionOrigin, TextureCache.CacheType.SteamGlyphes);
				}
				return Game1.inputMan.UnknownButton;
			}
			return new InputManager.ButtonGlyphInfo("glyphs/" + text, TextureCache.CacheType.TheWorldMachine);
		}

		public string GetDemoTimeString()
		{
			if (DemoTimeRemaining <= 0)
			{
				return "0:00";
			}
			int num = DemoTimeRemaining / 3600;
			int num2 = DemoTimeRemaining / 60 % 60;
			return $"{num}:{num2:00}";
		}

		public bool IsButtonPressed(InputManager.Button b)
		{
			InputDigitalActionHandle_t digitalActionHandle = ButtonToDigitalActionHandle(b);
			if (digitalActionHandle.m_InputDigitalActionHandle != 0L)
			{
				InputDigitalActionData_t digitalActionData = SteamInput.GetDigitalActionData(connectedControllers[0], digitalActionHandle);
				if (digitalActionData.bActive != 0)
				{
					return digitalActionData.bState != 0;
				}
				return false;
			}
			return false;
		}

		public Vec2 GetStickPos(InputManager.Button b)
		{
			InputAnalogActionHandle_t analogActionHandle = ButtonToAnalogActionHandle(b);
			if (analogActionHandle.m_InputAnalogActionHandle != 0L)
			{
				InputAnalogActionData_t analogActionData = SteamInput.GetAnalogActionData(connectedControllers[0], analogActionHandle);
				if (analogActionData.bActive != 0)
				{
					if (analogActionData.eMode == EInputSourceMode.k_EInputSourceMode_AbsoluteMouse)
					{
						return new Vec2((int)analogActionData.x, (int)(0f - analogActionData.y));
					}
					return new Vec2((int)(analogActionData.x * 100f), (int)(analogActionData.y * 100f));
				}
			}
			return Vec2.Zero;
		}

		public void OpenSteamInputConfig()
		{
			if (numberOfConnectedControllers >= 1)
			{
				if (!IsOnSteamDeck && Game1.gMan.IsFullscreen())
				{
					Game1.gMan.SetFullscreen(fullscreen: false);
				}
				SteamInput.ShowBindingPanel(connectedControllers[0]);
			}
		}

		public void UnlockAchievement(string achievement)
		{
			SteamUserStats.GetAchievement(achievement, out var pbAchieved);
			if (!pbAchieved)
			{
				SteamUserStats.SetAchievement(achievement);
				SteamUserStats.StoreStats();
			}
		}
	}
}

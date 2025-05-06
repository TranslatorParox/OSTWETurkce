using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OneShotMG.src.EngineSpecificCode
{
	public class InputManager
	{
		public class ButtonGlyphInfo
		{
			public string texturePath;

			public TextureCache.CacheType cacheType;

			public ButtonGlyphInfo(string path, TextureCache.CacheType cache)
			{
				texturePath = path;
				cacheType = cache;
			}
		}

		public enum Button
		{
			None = 0,
			OK = 11,
			Cancel = 12,
			Left = 4,
			Up = 8,
			Right = 6,
			Down = 2,
			Run = 15,
			Inventory = 14,
			MainMenu = 13,
			MashText = 50,
			ONESHOW_WINDOW_RESERVED_BUTTONS = 100,
			FullScreen = 101,
			MouseButton = 200,
			MoveMouse = 201,
			Move = 202,
			DEBUG_COLLISION = 1000,
			DEBUG_CHANGE_MAP = 1001,
			DEBUG_CHANGE_GEORGE = 1003,
			DEBUG_END_DEMO = 1004,
			DEBUG_FRAME_ADVANCE_MODE = 1005,
			DEBUG_FRAME_ADVANCE = 1006
		}

		public enum GlyphMode
		{
			None,
			Gamepad,
			Keyboard
		}

		public readonly ButtonGlyphInfo UnknownButton = new ButtonGlyphInfo("glyphs/pad_unknown", TextureCache.CacheType.TheWorldMachine);

		private ButtonGlyphMap buttonGlyphMap;

		private KeyGlyphMap keyGlyphMap;

		private Dictionary<Keys, List<Keys>> keysDupesMap;

		private Dictionary<Button, Keys> keyboardButtonMap;

		private Dictionary<Button, Buttons> gamepadButtonMap;

		private Dictionary<Button, bool> isButtonDown;

		private Dictionary<Button, bool> isButtonPressed;

		private Dictionary<Button, int> holdTime;

		private Dictionary<Button, string> buttonToGlyphStringsMap;

		private Dictionary<string, Button> glyphStringToButtonsMap;

		public Vec2 MoveStickPos = Vec2.Zero;

		public Vec2 MouseStickPos = Vec2.Zero;

		public int MoveStickDeadZone = 10;

		public int MouseStickDeadZone = 10;

		public int MouseCursorSpeed = 5;

		public const int MOUSESTICK_DEADZONE_MIN = 0;

		public const int MOUSESTICK_DEADZONE_MAX = 50;

		public const int MOVESTICK_DEADZONE_MIN = 0;

		public const int MOVESTICK_DEADZONE_MAX = 50;

		public const int MOUSESPEED_MIN = 2;

		public const int MOUSESPEED_MAX = 8;

		public bool IsAutoMashingEnabled;

		public bool IsControllerVibrationEnabled = true;

		public bool SuppressOneshotwindowInputs;

		private int assignModeGraceTimer;

		private int assignModeTimeoutTimer;

		private const int BUTTON_ASSIGN_MODE_GRACE_TIME = 10;

		private const int STICK_ASSIGN_MODE_GRACE_TIME = 30;

		private const int ASSIGN_MODE_TIMEOUT_TIME = 600;

		private readonly Buttons[] AssignableButtons = new Buttons[10]
		{
			Buttons.A,
			Buttons.B,
			Buttons.X,
			Buttons.Y,
			Buttons.LeftTrigger,
			Buttons.LeftShoulder,
			Buttons.RightTrigger,
			Buttons.RightShoulder,
			Buttons.Start,
			Buttons.Back
		};

		private readonly Keys[] AssignableKeys;

		private static string _glyphPattern;

		private float shakeStrength;

		private float prevSetShakeStrength;

		private int shakeFrameTimer;

		private int shakeFrameTotal;

		private Vec2 oldMousePos;

		private int oldMouseScrollPos;

		private bool oldMouseDown;

		public GlyphMode CurrentGlyphMode { get; private set; }

		public bool InButtonAssignMode { get; private set; }

		public bool InStickAssignMode { get; private set; }

		public bool InKeyAssignMode { get; private set; }

		public Buttons LastAssignedButton { get; private set; }

		public Keys LastAssignedKey { get; private set; }

		public static string GlyphPattern
		{
			get
			{
				if (_glyphPattern == null)
				{
					_glyphPattern = "(" + string.Join("|", Game1.inputMan.GlyphSubstitutionKeys()) + ")";
				}
				return _glyphPattern;
			}
		}

		public InputManager()
		{
			CurrentGlyphMode = GlyphMode.Keyboard;
			if (Game1.steamMan.IsOnSteamDeck)
			{
				CurrentGlyphMode = GlyphMode.Gamepad;
			}
			buttonGlyphMap = ButtonGlyphMap.LoadGlyphMap();
			keyGlyphMap = KeyGlyphMap.LoadGlyphMap();
			AssignableKeys = keyGlyphMap.KeysToGlyphes.Keys.ToArray();
			keysDupesMap = MakeKeyDupesMap();
			keyboardButtonMap = new Dictionary<Button, Keys>();
			gamepadButtonMap = new Dictionary<Button, Buttons>();
			isButtonDown = new Dictionary<Button, bool>();
			isButtonPressed = new Dictionary<Button, bool>();
			holdTime = new Dictionary<Button, int>();
			foreach (Button value in Enum.GetValues(typeof(Button)))
			{
				isButtonDown.Add(value, value: false);
				isButtonPressed.Add(value, value: false);
				holdTime.Add(value, 0);
			}
			InputMapping map = InputMapping.LoadInputMapping();
			ApplyInputMap(map);
			PopulateButtonGlyphStringMaps();
		}

		private Dictionary<Keys, List<Keys>> MakeKeyDupesMap()
		{
			Dictionary<Keys, List<Keys>> dictionary = new Dictionary<Keys, List<Keys>>();
			Dictionary<string, List<Keys>> dictionary2 = new Dictionary<string, List<Keys>>();
			foreach (KeyValuePair<Keys, string> keysToGlyphe in keyGlyphMap.KeysToGlyphes)
			{
				if (!dictionary2.TryGetValue(keysToGlyphe.Value, out var value))
				{
					value = new List<Keys>();
					value.Add(keysToGlyphe.Key);
					dictionary2.Add(keysToGlyphe.Value, value);
				}
				else
				{
					value.Add(keysToGlyphe.Key);
				}
			}
			foreach (KeyValuePair<string, List<Keys>> item in dictionary2)
			{
				if (item.Value.Count <= 1)
				{
					continue;
				}
				foreach (Keys item2 in item.Value)
				{
					dictionary.Add(item2, item.Value);
				}
			}
			return dictionary;
		}

		public void LoadDefaultInputMap()
		{
			ApplyInputMap(InputMapping.GetDefaultMapping());
		}

		private void ApplyInputMap(InputMapping map)
		{
			gamepadButtonMap = map.GamepadButtonMap;
			keyboardButtonMap = map.KeyboardButtonMap;
			MoveStickDeadZone = map.MoveStickDeadZone;
			MouseStickDeadZone = map.MouseStickDeadZone;
			MouseCursorSpeed = map.MouseSpeed;
			InputMapping defaultMapping = InputMapping.GetDefaultMapping();
			foreach (Button key in defaultMapping.GamepadButtonMap.Keys)
			{
				if (!gamepadButtonMap.ContainsKey(key))
				{
					SetButton(key, defaultMapping.GamepadButtonMap[key]);
				}
			}
			foreach (Button key2 in defaultMapping.KeyboardButtonMap.Keys)
			{
				if (!keyboardButtonMap.ContainsKey(key2))
				{
					SetKey(key2, defaultMapping.KeyboardButtonMap[key2]);
				}
			}
		}

		public void SaveCurrentInputMap()
		{
			InputMapping.SaveInputMapping(new InputMapping
			{
				GamepadButtonMap = gamepadButtonMap,
				KeyboardButtonMap = keyboardButtonMap,
				MouseSpeed = MouseCursorSpeed,
				MouseStickDeadZone = MouseStickDeadZone,
				MoveStickDeadZone = MoveStickDeadZone
			});
		}

		private void PopulateButtonGlyphStringMaps()
		{
			buttonToGlyphStringsMap = new Dictionary<Button, string>();
			glyphStringToButtonsMap = new Dictionary<string, Button>();
			AddButtonGlyphStringMapping(Button.OK, "@OK");
			AddButtonGlyphStringMapping(Button.Cancel, "@CANCEL");
			AddButtonGlyphStringMapping(Button.Run, "@RUN");
			AddButtonGlyphStringMapping(Button.Inventory, "@INVENTORY");
			AddButtonGlyphStringMapping(Button.MainMenu, "@MAINMENU");
			AddButtonGlyphStringMapping(Button.MashText, "@MASHTEXT");
			AddButtonGlyphStringMapping(Button.MouseButton, "@CLICK");
			AddButtonGlyphStringMapping(Button.MoveMouse, "@MOVEMOUSE");
			AddButtonGlyphStringMapping(Button.Move, "@MOVE");
			AddButtonGlyphStringMapping(Button.Up, "@UP");
			AddButtonGlyphStringMapping(Button.Down, "@DOWN");
			AddButtonGlyphStringMapping(Button.Left, "@LEFT");
			AddButtonGlyphStringMapping(Button.Right, "@RIGHT");
			AddButtonGlyphStringMapping(Button.FullScreen, "@FULLSCREEN");
		}

		private void AddButtonGlyphStringMapping(Button b, string s)
		{
			buttonToGlyphStringsMap.Add(b, s);
			glyphStringToButtonsMap.Add(s, b);
		}

		public void Update()
		{
			bool flag = false;
			bool flag2 = false;
			KeyboardState state = Keyboard.GetState();
			GamePadState state2 = GamePad.GetState(0);
			Game1.steamMan.RunFrame();
			if (Game1.steamMan.IsSteamInputControllerConnected())
			{
				MouseStickPos = Game1.steamMan.GetStickPos(Button.MoveMouse);
				MouseStickPos *= 8;
			}
			else
			{
				MouseStickPos = GetThumbstickPos(state2, Button.MoveMouse);
				MouseStickPos = ApplyStickDeadzone(MouseStickPos, MouseStickDeadZone, 90);
				MouseStickPos *= MouseCursorSpeed;
			}
			MouseState state3 = Mouse.GetState();
			Point position = state3.Position;
			Vec2 vec = Game1.gMan.ConvertWindowPosToLogicalPos(new Vec2(position.X, position.Y)) / 2;
			Vec2 vec2 = Game1.gMan.DrawScreenSize / 2;
			Rect rect = new Rect(0, 0, vec2.X, vec2.Y);
			if (rect.IsVec2InRect(vec) && (!vec.Equals(oldMousePos) || state3.LeftButton == ButtonState.Pressed))
			{
				MouseStickPos = Vec2.Zero;
				Game1.mouseCursorMan.SetMousePos(vec);
			}
			oldMousePos = vec;
			int scrollWheelValue = state3.ScrollWheelValue;
			Game1.mouseCursorMan.SetMouseScrollSpeed(scrollWheelValue - oldMouseScrollPos);
			oldMouseScrollPos = scrollWheelValue;
			if (InButtonAssignMode)
			{
				ButtonAssignModeUpdate(state2);
			}
			else if (InStickAssignMode)
			{
				StickAssignModeUpdate(state2);
			}
			else if (InKeyAssignMode)
			{
				KeyAssignModeUpdate(state);
			}
			else
			{
				if (!SuppressOneshotwindowInputs)
				{
					if (Game1.steamMan.IsSteamInputControllerConnected())
					{
						MoveStickPos = Game1.steamMan.GetStickPos(Button.Move);
						MoveStickPos = ApplyStickDeadzone(MoveStickPos, 0, 100);
					}
					else
					{
						MoveStickPos = GetThumbstickPos(state2, Button.Move);
						MoveStickPos = ApplyStickDeadzone(MoveStickPos, MoveStickDeadZone, 90);
					}
				}
				else
				{
					MoveStickPos = Vec2.Zero;
				}
				foreach (Button value3 in Enum.GetValues(typeof(Button)))
				{
					if (SuppressOneshotwindowInputs && Game1.bootMan.SequenceComplete && value3 < Button.ONESHOW_WINDOW_RESERVED_BUTTONS)
					{
						isButtonPressed[value3] = false;
						isButtonDown[value3] = false;
						holdTime[value3] = 0;
						continue;
					}
					bool flag3 = false;
					if (keyboardButtonMap.TryGetValue(value3, out var value))
					{
						bool flag4 = IsKeyDown(state, value);
						flag3 = flag3 || flag4;
						flag = flag || flag4;
					}
					bool flag5 = false;
					Buttons value2;
					if (Game1.steamMan.IsSteamInputControllerConnected())
					{
						flag5 = Game1.steamMan.IsButtonPressed(value3);
					}
					else if (gamepadButtonMap.TryGetValue(value3, out value2))
					{
						flag5 = state2.IsButtonDown(value2);
					}
					flag3 = flag3 || flag5;
					flag2 = flag2 || flag5;
					switch (value3)
					{
					case Button.Down:
						flag3 |= MoveStickPos.Y < -MoveStickDeadZone;
						break;
					case Button.Up:
						flag3 |= MoveStickPos.Y > MoveStickDeadZone;
						break;
					case Button.Left:
						flag3 |= MoveStickPos.X < -MoveStickDeadZone;
						break;
					case Button.Right:
						flag3 |= MoveStickPos.X > MoveStickDeadZone;
						break;
					case Button.MouseButton:
					{
						bool flag6 = state3.LeftButton == ButtonState.Pressed;
						if (Game1.steamMan.IsOnSteamDeck)
						{
							bool flag7 = flag6;
							flag6 = oldMouseDown;
							oldMouseDown = flag7;
						}
						if (rect.IsVec2InRect(vec))
						{
							flag3 = flag3 || flag6;
							flag = flag || flag6;
						}
						break;
					}
					}
					isButtonPressed[value3] = flag3 && !isButtonDown[value3];
					isButtonDown[value3] = flag3;
					if (flag3)
					{
						if (holdTime[value3] < int.MaxValue)
						{
							holdTime[value3]++;
						}
					}
					else
					{
						holdTime[value3] = 0;
					}
				}
			}
			if (flag && !flag2)
			{
				CurrentGlyphMode = GlyphMode.Keyboard;
			}
			else if (flag2 && !flag)
			{
				CurrentGlyphMode = GlyphMode.Gamepad;
			}
			if (Game1.steamMan.IsOnSteamDeck)
			{
				CurrentGlyphMode = GlyphMode.Gamepad;
			}
			ControllerVibrationUpdate();
		}

		private Vec2 GetThumbstickPos(GamePadState padState, Button b)
		{
			Vec2 zero = Vec2.Zero;
			switch (gamepadButtonMap[b])
			{
			case Buttons.LeftStick:
				zero.X = (int)(padState.ThumbSticks.Left.X * 100f);
				zero.Y = (int)(padState.ThumbSticks.Left.Y * 100f);
				break;
			case Buttons.RightStick:
				zero.X = (int)(padState.ThumbSticks.Right.X * 100f);
				zero.Y = (int)(padState.ThumbSticks.Right.Y * 100f);
				break;
			}
			return zero;
		}

		private Vec2 ApplyStickDeadzone(Vec2 stickPos, int stickDeadZone, int upperDeadZone)
		{
			double num = Math.Atan2(stickPos.X, stickPos.Y);
			double num2 = Math.Sqrt(stickPos.X * stickPos.X + stickPos.Y * stickPos.Y) - (double)stickDeadZone;
			num2 = num2 * 100.0 / (double)(upperDeadZone - stickDeadZone);
			if (num2 < 0.0)
			{
				num2 = 0.0;
			}
			if (num2 > 100.0)
			{
				num2 = 100.0;
			}
			return new Vec2((int)(Math.Sin(num) * num2), (int)(Math.Cos(num) * num2));
		}

		private void ExitAssignMode()
		{
			InButtonAssignMode = false;
			InStickAssignMode = false;
			InKeyAssignMode = false;
			Game1.mouseCursorMan.MouseHidden = false;
		}

		public void StartButtonAssignMode()
		{
			InButtonAssignMode = true;
			LastAssignedButton = (Buttons)0;
			assignModeGraceTimer = 0;
			assignModeTimeoutTimer = 0;
			Game1.mouseCursorMan.MouseHidden = true;
		}

		public void StartStickAssignMode()
		{
			InStickAssignMode = true;
			LastAssignedButton = (Buttons)0;
			assignModeGraceTimer = 0;
			assignModeTimeoutTimer = 0;
			Game1.mouseCursorMan.MouseHidden = true;
		}

		public void StartKeyAssignMode()
		{
			InKeyAssignMode = true;
			LastAssignedKey = Keys.None;
			assignModeGraceTimer = 0;
			assignModeTimeoutTimer = 0;
			Game1.mouseCursorMan.MouseHidden = true;
		}

		private void ControllerVibrationUpdate()
		{
			if (shakeStrength > 0f || shakeStrength != prevSetShakeStrength)
			{
				GamePad.SetVibration(PlayerIndex.One, shakeStrength, shakeStrength);
				prevSetShakeStrength = shakeStrength;
			}
			if (shakeFrameTotal > 0)
			{
				shakeFrameTimer++;
				if (shakeFrameTimer >= shakeFrameTotal)
				{
					shakeFrameTotal = 0;
					shakeFrameTimer = 0;
					shakeStrength = 0f;
				}
			}
		}

		private void ButtonAssignModeUpdate(GamePadState padState)
		{
			MoveStickPos = Vec2.Zero;
			MouseStickPos = Vec2.Zero;
			foreach (Button value in Enum.GetValues(typeof(Button)))
			{
				isButtonPressed[value] = false;
				isButtonDown[value] = false;
				holdTime[value] = 0;
			}
			assignModeTimeoutTimer++;
			if (assignModeTimeoutTimer >= 600)
			{
				LastAssignedButton = (Buttons)0;
				ExitAssignMode();
				return;
			}
			if (assignModeGraceTimer < 10)
			{
				assignModeGraceTimer++;
				return;
			}
			Buttons[] assignableButtons = AssignableButtons;
			foreach (Buttons buttons in assignableButtons)
			{
				if (padState.IsButtonDown(buttons))
				{
					LastAssignedButton = buttons;
					ExitAssignMode();
					break;
				}
			}
		}

		private void StickAssignModeUpdate(GamePadState padState)
		{
			MoveStickPos = Vec2.Zero;
			MouseStickPos = Vec2.Zero;
			foreach (Button value in Enum.GetValues(typeof(Button)))
			{
				isButtonPressed[value] = false;
				isButtonDown[value] = false;
				holdTime[value] = 0;
			}
			assignModeTimeoutTimer++;
			if (assignModeTimeoutTimer >= 600)
			{
				LastAssignedButton = (Buttons)0;
				ExitAssignMode();
			}
			else if (assignModeGraceTimer < 30)
			{
				assignModeGraceTimer++;
			}
			else if (Math.Max(Math.Abs(padState.ThumbSticks.Left.X), Math.Abs(padState.ThumbSticks.Left.Y)) > 0.5f)
			{
				LastAssignedButton = Buttons.LeftStick;
				ExitAssignMode();
			}
			else if (Math.Max(Math.Abs(padState.ThumbSticks.Right.X), Math.Abs(padState.ThumbSticks.Right.Y)) > 0.5f)
			{
				LastAssignedButton = Buttons.RightStick;
				ExitAssignMode();
			}
		}

		private void KeyAssignModeUpdate(KeyboardState kbState)
		{
			MoveStickPos = Vec2.Zero;
			MouseStickPos = Vec2.Zero;
			foreach (Button value in Enum.GetValues(typeof(Button)))
			{
				isButtonPressed[value] = false;
				isButtonDown[value] = false;
				holdTime[value] = 0;
			}
			assignModeTimeoutTimer++;
			if (assignModeTimeoutTimer >= 600)
			{
				LastAssignedKey = Keys.None;
				ExitAssignMode();
				return;
			}
			if (assignModeGraceTimer < 10)
			{
				assignModeGraceTimer++;
				return;
			}
			Keys[] assignableKeys = AssignableKeys;
			foreach (Keys keys in assignableKeys)
			{
				if (kbState.IsKeyDown(keys))
				{
					LastAssignedKey = keys;
					ExitAssignMode();
					break;
				}
			}
		}

		public void VibrateController(float shakeStrength, int shakeDuration)
		{
			if (IsControllerVibrationEnabled)
			{
				shakeFrameTotal = shakeDuration;
				shakeFrameTimer = 0;
				this.shakeStrength = shakeStrength;
			}
		}

		public bool IsButtonPressed(Button b)
		{
			return isButtonPressed[b];
		}

		public bool IsButtonHeld(Button b)
		{
			return isButtonDown[b];
		}

		public bool IsAutoMashing()
		{
			if (IsAutoMashingEnabled)
			{
				return isButtonDown[Button.MashText];
			}
			return false;
		}

		public bool HoldAutoTriggerInput(Button b, int timeToAutoTrigger, int autoTriggerInterval)
		{
			int num = holdTime[b];
			if (num >= timeToAutoTrigger)
			{
				return num % autoTriggerInterval == 0;
			}
			return false;
		}

		private bool IsKeyDown(KeyboardState kbState, Keys keys)
		{
			bool result = false;
			if (keysDupesMap != null && keysDupesMap.TryGetValue(keys, out var value))
			{
				foreach (Keys item in value)
				{
					if (kbState.IsKeyDown(item))
					{
						result = true;
						break;
					}
				}
			}
			else
			{
				result = kbState.IsKeyDown(keys);
			}
			return result;
		}

		private bool IsKeyEqual(Keys k1, Keys k2)
		{
			if (keysDupesMap != null && keysDupesMap.TryGetValue(k1, out var value))
			{
				foreach (Keys item in value)
				{
					if (item == k2)
					{
						return true;
					}
				}
				return false;
			}
			return k1 == k2;
		}

		public void SetKey(Button gameButton, Keys keys)
		{
			if (keys == Keys.None)
			{
				return;
			}
			if (keyboardButtonMap.TryGetValue(gameButton, out var value))
			{
				foreach (Button value3 in Enum.GetValues(typeof(Button)))
				{
					if (keyboardButtonMap.TryGetValue(value3, out var value2) && IsKeyEqual(value2, keys))
					{
						keyboardButtonMap[value3] = value;
					}
				}
			}
			keyboardButtonMap[gameButton] = keys;
			isButtonDown[gameButton] = true;
		}

		public void SetButton(Button gameButton, Buttons padButton)
		{
			if (padButton == (Buttons)0)
			{
				return;
			}
			if (gamepadButtonMap.TryGetValue(gameButton, out var value))
			{
				foreach (Button value3 in Enum.GetValues(typeof(Button)))
				{
					if (gamepadButtonMap.TryGetValue(value3, out var value2) && value2 == padButton)
					{
						gamepadButtonMap[value3] = value;
					}
				}
			}
			gamepadButtonMap[gameButton] = padButton;
			isButtonDown[gameButton] = true;
		}

		public List<string> GlyphSubstitutionKeys()
		{
			return glyphStringToButtonsMap.Keys.ToList();
		}

		public List<ButtonGlyphInfo> GetGlyph(string subText, GlyphMode glyphMode = GlyphMode.None)
		{
			List<ButtonGlyphInfo> list = new List<ButtonGlyphInfo>();
			if (glyphStringToButtonsMap.TryGetValue(subText, out var value))
			{
				if (glyphMode == GlyphMode.None)
				{
					glyphMode = CurrentGlyphMode;
				}
				switch (glyphMode)
				{
				case GlyphMode.Gamepad:
				{
					Buttons value6;
					string value7;
					if (Game1.steamMan.IsSteamInputControllerConnected())
					{
						list.Add(Game1.steamMan.GetGlyphInfo(value));
					}
					else if (gamepadButtonMap.TryGetValue(value, out value6) && buttonGlyphMap.ButtonsToGlyphes.TryGetValue(value6, out value7))
					{
						list.Add(new ButtonGlyphInfo("glyphs/" + value7, TextureCache.CacheType.TheWorldMachine));
					}
					else
					{
						list.Add(UnknownButton);
					}
					break;
				}
				case GlyphMode.Keyboard:
					switch (value)
					{
					case Button.MoveMouse:
						list.Add(new ButtonGlyphInfo("glyphs/keys/mouse_move", TextureCache.CacheType.TheWorldMachine));
						break;
					case Button.MouseButton:
						list.Add(new ButtonGlyphInfo("glyphs/keys/mouse_leftclick", TextureCache.CacheType.TheWorldMachine));
						break;
					case Button.Move:
					{
						Button[] array = new Button[4]
						{
							Button.Up,
							Button.Down,
							Button.Left,
							Button.Right
						};
						foreach (Button key in array)
						{
							if (keyboardButtonMap.TryGetValue(key, out var value4) && keyGlyphMap.KeysToGlyphes.TryGetValue(value4, out var value5))
							{
								list.Add(new ButtonGlyphInfo("glyphs/keys/" + value5, TextureCache.CacheType.TheWorldMachine));
							}
							else
							{
								list.Add(new ButtonGlyphInfo("glyphs/keys/key_unknown", TextureCache.CacheType.TheWorldMachine));
							}
						}
						break;
					}
					default:
					{
						if (keyboardButtonMap.TryGetValue(value, out var value2) && keyGlyphMap.KeysToGlyphes.TryGetValue(value2, out var value3))
						{
							list.Add(new ButtonGlyphInfo("glyphs/keys/" + value3, TextureCache.CacheType.TheWorldMachine));
						}
						else
						{
							list.Add(new ButtonGlyphInfo("glyphs/keys/key_unknown", TextureCache.CacheType.TheWorldMachine));
						}
						break;
					}
					}
					break;
				}
			}
			return list;
		}

		public bool IsTutorialButtonPressed()
		{
			if (!IsButtonPressed(Button.OK) && !IsButtonPressed(Button.Cancel) && !IsButtonPressed(Button.MouseButton))
			{
				if (IsButtonHeld(Button.MashText))
				{
					return IsButtonHeld(Button.MouseButton);
				}
				return false;
			}
			return true;
		}

		public string ButtonToGlyphString(Button b)
		{
			if (!buttonToGlyphStringsMap.TryGetValue(b, out var value))
			{
				return string.Empty;
			}
			return value;
		}

		public Button GlyphStringToButton(string s)
		{
			if (!glyphStringToButtonsMap.TryGetValue(s, out var value))
			{
				return Button.None;
			}
			return value;
		}
	}
}

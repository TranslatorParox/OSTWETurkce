using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace OneShotMG.src.EngineSpecificCode
{
	public class InputMapping
	{
		[JsonIgnore]
		private const string INPUT_MAP_FILE_NAME = "input_mapping.dat";

		[JsonProperty]
		public Dictionary<InputManager.Button, Buttons> GamepadButtonMap;

		public Dictionary<InputManager.Button, Keys> KeyboardButtonMap;

		[JsonProperty]
		public int MoveStickDeadZone = 10;

		[JsonProperty]
		public int MouseStickDeadZone = 10;

		[JsonProperty]
		public int MouseSpeed = 5;

		public InputMapping()
		{
			GamepadButtonMap = new Dictionary<InputManager.Button, Buttons>();
			KeyboardButtonMap = new Dictionary<InputManager.Button, Keys>();
		}

		public static InputMapping LoadInputMapping()
		{
			return Game1.masterSaveMan.LoadFile("input_mapping.dat", LoadInputMappingFromString, VerifyMapping, GetDefaultMapping());
		}

		private static InputMapping LoadInputMappingFromString(string data)
		{
			return JsonConvert.DeserializeObject<InputMapping>(data);
		}

		private static bool VerifyMapping(InputMapping map)
		{
			if (map == null)
			{
				return false;
			}
			if (map.GamepadButtonMap == null || map.GamepadButtonMap.Count < 13)
			{
				return false;
			}
			if (map.KeyboardButtonMap == null || map.KeyboardButtonMap.Count < 11)
			{
				return false;
			}
			if (map.MouseStickDeadZone < 0 || map.MouseStickDeadZone > 50)
			{
				return false;
			}
			if (map.MoveStickDeadZone < 0 || map.MoveStickDeadZone > 50)
			{
				return false;
			}
			if (map.MouseSpeed < 2 || map.MouseSpeed > 8)
			{
				return false;
			}
			return true;
		}

		public static void SaveInputMapping(InputMapping map)
		{
			string fData = JsonConvert.SerializeObject(map);
			Game1.masterSaveMan.WriteFile(new SaveRequest("input_mapping.dat", fData));
		}

		public static void DeleteInputMapping()
		{
			File.Delete(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Game1.SaveFolderName), "input_mapping.dat"));
		}

		public static InputMapping GetDefaultMapping()
		{
			return new InputMapping
			{
				GamepadButtonMap = 
				{
					{
						InputManager.Button.OK,
						Buttons.A
					},
					{
						InputManager.Button.Cancel,
						Buttons.B
					},
					{
						InputManager.Button.MashText,
						Buttons.LeftTrigger
					},
					{
						InputManager.Button.MainMenu,
						Buttons.Start
					},
					{
						InputManager.Button.Inventory,
						Buttons.Y
					},
					{
						InputManager.Button.Left,
						Buttons.DPadLeft
					},
					{
						InputManager.Button.Up,
						Buttons.DPadUp
					},
					{
						InputManager.Button.Right,
						Buttons.DPadRight
					},
					{
						InputManager.Button.Down,
						Buttons.DPadDown
					},
					{
						InputManager.Button.Run,
						Buttons.X
					},
					{
						InputManager.Button.MouseButton,
						Buttons.RightTrigger
					},
					{
						InputManager.Button.MoveMouse,
						Buttons.RightStick
					},
					{
						InputManager.Button.Move,
						Buttons.LeftStick
					}
				},
				KeyboardButtonMap = 
				{
					{
						InputManager.Button.OK,
						Keys.Z
					},
					{
						InputManager.Button.Cancel,
						Keys.X
					},
					{
						InputManager.Button.MashText,
						Keys.C
					},
					{
						InputManager.Button.MainMenu,
						Keys.A
					},
					{
						InputManager.Button.Inventory,
						Keys.S
					},
					{
						InputManager.Button.Left,
						Keys.Left
					},
					{
						InputManager.Button.Up,
						Keys.Up
					},
					{
						InputManager.Button.Right,
						Keys.Right
					},
					{
						InputManager.Button.Down,
						Keys.Down
					},
					{
						InputManager.Button.Run,
						Keys.LeftShift
					},
					{
						InputManager.Button.FullScreen,
						Keys.F
					}
				}
			};
		}
	}
}

using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class SettingsMenu : AbstractMenu
	{
		public enum ScalingMode
		{
			None = 0,
			Smooth = 1,
			Sharp = 2,
			Integer = 3,
			Min = 1,
			Max = 3
		}

		private enum Settings
		{
			Fullscreen,
			DefaultMovement,
			ColorblindMode,
			ControllerVibration,
			ScreenTearEffects,
			ChromaAberrationEffects,
			FullscreenScaling,
			FullscreenBorder,
			Automash
		}

		private class SettingsOption
		{
			public string name = string.Empty;

			public int offsetX;

			public int opacity = 128;

			public TempTexture nameTexture;

			public TempTexture valueTexture;

			public string value { get; private set; } = string.Empty;

			public void SetValue(string newValue, bool updateTexture)
			{
				if (newValue != value)
				{
					value = newValue;
					if (updateTexture)
					{
						DrawValueTexture();
					}
				}
			}

			public void DrawValueTexture()
			{
				valueTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, value);
			}
		}

		public class SettingsSaveData
		{
			public bool fullscreen;

			public bool isDefaultMovementRun = true;

			public bool isColorblindMode;

			public bool isAutomashEnabled;

			public bool isControllerVibrationEnabled = true;

			public bool isScreenTearEnabled = true;

			public bool isChromaticAberrationEnabled = true;

			public bool isFullscreenScalingSmooth = true;

			public bool isFullscreenBorderEnabled = true;

			public ScalingMode fullscreenScaling;
		}

		private OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 20;

		private const GraphicsManager.FontType MenuFont = GraphicsManager.FontType.Game;

		private int selectedOptionIndex;

		private TempTexture settingsTexture;

		private const int LEFT_MARGIN = 30;

		private const int TITLE_TOP_MARGIN = 48;

		private const int ITEMS_TOP_MARGIN = 118;

		private const int ITEM_SPACING = 32;

		private const int ITEM_VALUE_MARGIN = 370;

		private const int OFFSET_NORMAL_MARGIN = 30;

		private const int OFFSET_SELECTED_MARGIN = 50;

		private const int OFFSET_MOVE_SPEED = 6;

		private const int OPTION_UNSELECTED_OPACITY = 128;

		private const int OPTION_OPACITY_CHANGE_SPEED = 10;

		private const int SETTING_NAME_MAX_WIDTH = 360;

		private Dictionary<Settings, SettingsOption> settingsOptions;

		public bool IsDefaultMovementRun { get; private set; } = true;

		public bool IsColorBlindMode { get; private set; }

		public bool IsAutomashEnabled
		{
			get
			{
				return Game1.inputMan.IsAutoMashingEnabled;
			}
			private set
			{
				Game1.inputMan.IsAutoMashingEnabled = value;
			}
		}

		public bool IsControllerVibrationEnabled
		{
			get
			{
				return Game1.inputMan.IsControllerVibrationEnabled;
			}
			private set
			{
				Game1.inputMan.IsControllerVibrationEnabled = value;
			}
		}

		public bool IsScreenTearEnabled { get; private set; } = true;

		public bool IsChromaAberrationEnabled { get; private set; } = true;

		public ScalingMode FullscreenScaling { get; private set; } = ScalingMode.Smooth;

		public bool IsFullscreenBorderEnabled { get; private set; } = true;

		public SettingsMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			settingsOptions = new Dictionary<Settings, SettingsOption>();
			settingsOptions.Add(Settings.Fullscreen, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_fullscreen")
			});
			settingsOptions.Add(Settings.DefaultMovement, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_movement")
			});
			settingsOptions.Add(Settings.ColorblindMode, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_colorblind")
			});
			settingsOptions.Add(Settings.ControllerVibration, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_vibration")
			});
			settingsOptions.Add(Settings.ScreenTearEffects, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_screentear")
			});
			settingsOptions.Add(Settings.ChromaAberrationEffects, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_chromaaberration")
			});
			settingsOptions.Add(Settings.FullscreenScaling, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_fullscreen_scaling")
			});
			settingsOptions.Add(Settings.FullscreenBorder, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_fullscreen_border")
			});
			settingsOptions.Add(Settings.Automash, new SettingsOption
			{
				name = Game1.languageMan.GetTWMLocString("settings_automash")
			});
			UpdateSettingOptionsValues(updateTextures: false);
		}

		public void UpdateSettingOptionsValues(bool updateTextures = true)
		{
			settingsOptions[Settings.Fullscreen].SetValue(Game1.languageMan.GetTWMLocString(oneshotWindow.IsMaximized ? "settings_on" : "settings_off"), updateTextures);
			settingsOptions[Settings.DefaultMovement].SetValue(Game1.languageMan.GetTWMLocString(IsDefaultMovementRun ? "settings_run" : "settings_walk"), updateTextures);
			settingsOptions[Settings.ColorblindMode].SetValue(Game1.languageMan.GetTWMLocString(IsColorBlindMode ? "settings_on" : "settings_off"), updateTextures);
			settingsOptions[Settings.Automash].SetValue(Game1.languageMan.GetTWMLocString(IsAutomashEnabled ? "settings_on" : "settings_off"), updateTextures);
			settingsOptions[Settings.ControllerVibration].SetValue(Game1.languageMan.GetTWMLocString(IsControllerVibrationEnabled ? "settings_on" : "settings_off"), updateTextures);
			settingsOptions[Settings.ScreenTearEffects].SetValue(Game1.languageMan.GetTWMLocString(IsScreenTearEnabled ? "settings_on" : "settings_off"), updateTextures);
			settingsOptions[Settings.ChromaAberrationEffects].SetValue(Game1.languageMan.GetTWMLocString(IsChromaAberrationEnabled ? "settings_on" : "settings_off"), updateTextures);
			string id;
			switch (FullscreenScaling)
			{
			case ScalingMode.Smooth:
				id = "settings_fullscreen_scaling_smooth";
				break;
			case ScalingMode.Sharp:
				id = "settings_fullscreen_scaling_sharp";
				break;
			case ScalingMode.Integer:
				id = "settings_fullscreen_scaling_integer";
				break;
			default:
				id = "settings_fullscreen_scaling_none";
				break;
			}
			settingsOptions[Settings.FullscreenScaling].SetValue(Game1.languageMan.GetTWMLocString(id), updateTextures);
			settingsOptions[Settings.FullscreenBorder].SetValue(Game1.languageMan.GetTWMLocString(IsFullscreenBorderEnabled ? "settings_on" : "settings_off"), updateTextures);
		}

		private bool skipFullscreenRelatedSettings(Settings s)
		{
			if (oneshotWindow.IsMaximized)
			{
				return false;
			}
			if ((uint)(s - 6) <= 1u)
			{
				return true;
			}
			return false;
		}

		public override void Draw()
		{
			GameColor gameColor = new GameColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)opacity);
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(opacity * 180 / 255)));
			Game1.gMan.MainBlit(settingsTexture, new Vec2(13, 24), gameColor);
			int num = 118;
			for (int i = 0; i < settingsOptions.Count; i++)
			{
				if (!skipFullscreenRelatedSettings((Settings)i))
				{
					SettingsOption settingsOption = settingsOptions[(Settings)i];
					gameColor.a = (byte)(int)((float)settingsOption.opacity * ((float)opacity / 255f));
					if (i == 8)
					{
						num += 32;
					}
					Game1.gMan.MainBlit(settingsOption.nameTexture, new Vec2(30 + settingsOption.offsetX - 2, num), gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
					if (i == 8)
					{
						Game1.gMan.TextBlit(GraphicsManager.FontType.Game, new Vec2(30 + settingsOption.offsetX, num), settingsOption.name, gameColor, GraphicsManager.BlendMode.Normal, 1, GraphicsManager.TextBlitMode.OnlyGlyphes);
					}
					Game1.gMan.MainBlit(settingsOption.valueTexture, new Vec2(400 + settingsOption.offsetX - 2, num), gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
					num += 32;
				}
			}
		}

		public override void Update()
		{
			if (IsOpen())
			{
				if (settingsTexture == null || !settingsTexture.isValid)
				{
					settingsTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("settings_title"));
				}
				settingsTexture.KeepAlive();
				foreach (SettingsOption value in settingsOptions.Values)
				{
					if (value.nameTexture == null || !value.nameTexture.isValid)
					{
						value.nameTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, value.name, 360);
					}
					value.nameTexture.KeepAlive();
					if (value.valueTexture == null || !value.valueTexture.isValid)
					{
						value.DrawValueTexture();
					}
					value.valueTexture.KeepAlive();
				}
			}
			switch (state)
			{
			case MenuState.Opening:
			{
				opacity += 20;
				if (opacity >= 255)
				{
					opacity = 255;
				}
				bool flag = false;
				foreach (KeyValuePair<Settings, SettingsOption> settingsOption2 in settingsOptions)
				{
					if (settingsOption2.Value.offsetX < 30)
					{
						flag = true;
						settingsOption2.Value.offsetX += 6;
						if (settingsOption2.Value.offsetX > 30)
						{
							settingsOption2.Value.offsetX = 30;
						}
						break;
					}
				}
				if (!flag && opacity >= 255)
				{
					state = MenuState.Open;
				}
				break;
			}
			case MenuState.Closing:
				opacity -= 20;
				if (opacity <= 0)
				{
					opacity = 0;
					state = MenuState.Closed;
				}
				break;
			case MenuState.Open:
			{
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Up))
				{
					Game1.soundMan.PlaySound("menu_cursor");
					selectedOptionIndex--;
					while (skipFullscreenRelatedSettings((Settings)selectedOptionIndex))
					{
						selectedOptionIndex--;
					}
					if (selectedOptionIndex < 0)
					{
						selectedOptionIndex = settingsOptions.Count - 1;
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Down))
				{
					Game1.soundMan.PlaySound("menu_cursor");
					selectedOptionIndex++;
					while (skipFullscreenRelatedSettings((Settings)selectedOptionIndex))
					{
						selectedOptionIndex++;
					}
					if (selectedOptionIndex >= settingsOptions.Count)
					{
						selectedOptionIndex = 0;
					}
				}
				while (skipFullscreenRelatedSettings((Settings)selectedOptionIndex))
				{
					selectedOptionIndex--;
				}
				for (int i = 0; i < settingsOptions.Count; i++)
				{
					SettingsOption settingsOption = settingsOptions[(Settings)i];
					if (i == selectedOptionIndex)
					{
						settingsOption.offsetX += 6;
						if (settingsOption.offsetX > 50)
						{
							settingsOption.offsetX = 50;
						}
						settingsOption.opacity += 10;
						if (settingsOption.opacity > 255)
						{
							settingsOption.opacity = 255;
						}
					}
					else
					{
						settingsOption.offsetX -= 6;
						if (settingsOption.offsetX < 30)
						{
							settingsOption.offsetX = 30;
						}
						settingsOption.opacity -= 10;
						if (settingsOption.opacity < 128)
						{
							settingsOption.opacity = 128;
						}
					}
				}
				PerOptionInput();
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
				{
					Game1.soundMan.PlaySound("menu_cancel");
					Close();
				}
				break;
			}
			}
		}

		private void PerOptionInput()
		{
			switch ((Settings)selectedOptionIndex)
			{
			case Settings.Fullscreen:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					oneshotWindow.ToggleMaximize();
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.DefaultMovement:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsDefaultMovementRun = !IsDefaultMovementRun;
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.ColorblindMode:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsColorBlindMode = !IsColorBlindMode;
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.Automash:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsAutomashEnabled = !IsAutomashEnabled;
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.ControllerVibration:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsControllerVibrationEnabled = !IsControllerVibrationEnabled;
					if (IsControllerVibrationEnabled)
					{
						Game1.inputMan.VibrateController(0.3f, 20);
					}
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.ScreenTearEffects:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsScreenTearEnabled = !IsScreenTearEnabled;
					if (IsScreenTearEnabled)
					{
						oneshotWindow.glitchEffectMan.StartGlitch(oneshotWindow, 20, 0.05f, vibrateController: false);
					}
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.ChromaAberrationEffects:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsChromaAberrationEnabled = !IsChromaAberrationEnabled;
					UpdateSettingOptionsValues();
				}
				break;
			case Settings.FullscreenScaling:
			{
				bool flag = false;
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left))
				{
					FullscreenScaling--;
					flag = true;
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					FullscreenScaling++;
					flag = true;
				}
				if (FullscreenScaling > ScalingMode.Integer)
				{
					FullscreenScaling = ScalingMode.Smooth;
				}
				else if (FullscreenScaling < ScalingMode.Smooth)
				{
					FullscreenScaling = ScalingMode.Integer;
				}
				if (flag)
				{
					Game1.soundMan.PlaySound("menu_decision");
					UpdateSettingOptionsValues();
				}
				break;
			}
			case Settings.FullscreenBorder:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision");
					IsFullscreenBorderEnabled = !IsFullscreenBorderEnabled;
					UpdateSettingOptionsValues();
				}
				break;
			}
		}

		public override void Close()
		{
			oneshotWindow.gameSaveMan.SaveSettings();
			state = MenuState.Closing;
			opacity = 255;
		}

		public override void Open()
		{
			Game1.soundMan.PlaySound("menu_decision");
			state = MenuState.Opening;
			opacity = 0;
			selectedOptionIndex = 0;
			UpdateSettingOptionsValues();
			foreach (KeyValuePair<Settings, SettingsOption> settingsOption in settingsOptions)
			{
				settingsOption.Value.offsetX = 0;
				settingsOption.Value.opacity = 128;
			}
		}

		public SettingsSaveData GetSettingsSaveData()
		{
			return new SettingsSaveData
			{
				fullscreen = oneshotWindow.IsMaximized,
				isDefaultMovementRun = IsDefaultMovementRun,
				isColorblindMode = IsColorBlindMode,
				isAutomashEnabled = IsAutomashEnabled,
				isControllerVibrationEnabled = IsControllerVibrationEnabled,
				isScreenTearEnabled = IsScreenTearEnabled,
				isChromaticAberrationEnabled = IsChromaAberrationEnabled,
				fullscreenScaling = FullscreenScaling,
				isFullscreenBorderEnabled = IsFullscreenBorderEnabled
			};
		}

		public void LoadSettingsSaveData(SettingsSaveData data)
		{
			if (data.fullscreen != oneshotWindow.IsMaximized)
			{
				oneshotWindow.ToggleMaximize();
			}
			IsDefaultMovementRun = data.isDefaultMovementRun;
			IsColorBlindMode = data.isColorblindMode;
			IsAutomashEnabled = data.isAutomashEnabled;
			IsControllerVibrationEnabled = data.isControllerVibrationEnabled;
			IsScreenTearEnabled = data.isScreenTearEnabled;
			IsChromaAberrationEnabled = data.isChromaticAberrationEnabled;
			if (data.fullscreenScaling == ScalingMode.None)
			{
				FullscreenScaling = (data.isFullscreenScalingSmooth ? ScalingMode.Smooth : ScalingMode.Sharp);
			}
			else
			{
				FullscreenScaling = data.fullscreenScaling;
			}
			IsFullscreenBorderEnabled = data.isFullscreenBorderEnabled;
		}
	}
}

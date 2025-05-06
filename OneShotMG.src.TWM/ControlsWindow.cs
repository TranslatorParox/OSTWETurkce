using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class ControlsWindow : TWMWindow
	{
		private class ButtonMappingDisplay
		{
			public InputManager.Button button;

			public TempTexture buttonNameTex;

			public ButtonMappingDisplay(InputManager.Button b)
			{
				button = b;
			}
		}

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private readonly InputManager.Button[] gamepadButtonsToMap = new InputManager.Button[9]
		{
			InputManager.Button.MouseButton,
			InputManager.Button.MoveMouse,
			InputManager.Button.Move,
			InputManager.Button.OK,
			InputManager.Button.Cancel,
			InputManager.Button.MainMenu,
			InputManager.Button.Inventory,
			InputManager.Button.Run,
			InputManager.Button.MashText
		};

		private readonly InputManager.Button[] keyboardButtonsToMap = new InputManager.Button[11]
		{
			InputManager.Button.Up,
			InputManager.Button.Down,
			InputManager.Button.Left,
			InputManager.Button.Right,
			InputManager.Button.OK,
			InputManager.Button.Cancel,
			InputManager.Button.MainMenu,
			InputManager.Button.Inventory,
			InputManager.Button.Run,
			InputManager.Button.MashText,
			InputManager.Button.FullScreen
		};

		private Dictionary<InputManager.Button, ButtonMappingDisplay> buttonNameTextures = new Dictionary<InputManager.Button, ButtonMappingDisplay>();

		private SliderControl editButtonsSlider;

		private SliderControl moveStickDeadzoneSlider;

		private SliderControl mouseStickDeadzoneSlider;

		private SliderControl mouseCursorSpeedSlider;

		private TextButton defaultControlsButton;

		private int displayedButtonsStart;

		private const int TAB_BUTTONS_HEIGHT = 32;

		private const int TAB_BUTTONS_AREA_HEIGHT = 34;

		private IconButton gamepadTabButton;

		private IconButton keyboardTabButton;

		private const int BUTTON_MAPPINGS_DISPLAYED = 5;

		private const int BUTTON_MAPPING_HEIGHT = 32;

		private const int BUTTON_NAME_WIDTH = 150;

		private const int TEXT_PADDING = 2;

		private const int BUTTON_GLYPH_WIDTH = 32;

		private const int EDIT_BUTTON_WIDTH = 32;

		private const int SLIDER_MARGIN = 2;

		private const int VERTICAL_DIVIDER_WIDTH = 2;

		private const int SLIDERS_WIDTH = 200;

		private const int SLIDERS_X_PADDING = 4;

		private const int SLIDERS_Y_PADDING = 8;

		private const int LEFT_PANE_WIDTH = 240;

		private const int RIGHT_PANE_WIDTH = 208;

		private const int WINDOW_CONTENT_WIDTH = 450;

		private const int WINDOW_CONTENT_HEIGHT = 202;

		private const int BUTTON_MAPPING_SEPARATOR_HEIGHT = 2;

		private readonly List<IconButton> editButtons;

		private InputManager.GlyphMode displayedGlyphMode;

		private bool hideKeyboardControls;

		public ControlsWindow()
		{
			base.WindowIcon = "controls";
			base.WindowTitle = "controls_app_name";
			base.ContentsSize = new Vec2(450, 202);
			gamepadTabButton = new IconButton("the_world_machine/window_icons/gamepad_tab", new Vec2(8, 4), delegate
			{
				if (Game1.steamMan.IsSteamInputControllerConnected())
				{
					Game1.steamMan.OpenSteamInputConfig();
				}
				else
				{
					changeDisplayedGlyphMode(InputManager.GlyphMode.Gamepad);
				}
			});
			keyboardTabButton = new IconButton("the_world_machine/window_icons/keyboard_tab", new Vec2(56, 4), delegate
			{
				changeDisplayedGlyphMode(InputManager.GlyphMode.Keyboard);
			});
			if (Game1.steamMan.IsOnSteamDeck && Game1.inputMan.CurrentGlyphMode == InputManager.GlyphMode.Gamepad)
			{
				hideKeyboardControls = true;
			}
			Vec2 pos = new Vec2(246, 8);
			moveStickDeadzoneSlider = new SliderControl("controls_app_move_deadzone_slider_label", 0, 50, pos, 200);
			moveStickDeadzoneSlider.Increment = 2;
			moveStickDeadzoneSlider.Value = Game1.inputMan.MoveStickDeadZone;
			SliderControl sliderControl = moveStickDeadzoneSlider;
			sliderControl.OnValueChanged = (Action<int>)Delegate.Combine(sliderControl.OnValueChanged, (Action<int>)delegate(int val)
			{
				Game1.logMan.Log(LogManager.LogLevel.Info, $"Move Deadzone Value is now {val}!");
				Game1.inputMan.MoveStickDeadZone = val;
			});
			Vec2 pos2 = new Vec2(246, pos.Y + 8 + 32);
			mouseStickDeadzoneSlider = new SliderControl("controls_app_mouse_deadzone_slider_label", 0, 50, pos2, 200);
			mouseStickDeadzoneSlider.Increment = 2;
			mouseStickDeadzoneSlider.Value = Game1.inputMan.MouseStickDeadZone;
			SliderControl sliderControl2 = mouseStickDeadzoneSlider;
			sliderControl2.OnValueChanged = (Action<int>)Delegate.Combine(sliderControl2.OnValueChanged, (Action<int>)delegate(int val)
			{
				Game1.logMan.Log(LogManager.LogLevel.Info, $"Mouse Deadzone Value is now {val}!");
				Game1.inputMan.MouseStickDeadZone = val;
			});
			Vec2 pos3 = new Vec2(246, pos2.Y + 8 + 32);
			mouseCursorSpeedSlider = new SliderControl("controls_app_mouse_speed_slider_label", 2, 8, pos3, 200);
			mouseCursorSpeedSlider.Value = Game1.inputMan.MouseCursorSpeed;
			SliderControl sliderControl3 = mouseCursorSpeedSlider;
			sliderControl3.OnValueChanged = (Action<int>)Delegate.Combine(sliderControl3.OnValueChanged, (Action<int>)delegate(int val)
			{
				Game1.logMan.Log(LogManager.LogLevel.Info, $"Mouse Cursor Speed is now {val}!");
				Game1.inputMan.MouseCursorSpeed = val;
			});
			defaultControlsButton = new TextButton(relativePos: new Vec2(246, 178), label: Game1.languageMan.GetTWMLocString("controls_app_default_controls_button_label"), action: delegate
			{
				ShowModalWindow(ModalWindow.ModalType.YesNo, "controls_app_default_controls_confirm_popup", delegate(ModalWindow.ModalResponse res)
				{
					if (res == ModalWindow.ModalResponse.Yes)
					{
						Game1.inputMan.LoadDefaultInputMap();
						mouseCursorSpeedSlider.Value = Game1.inputMan.MouseCursorSpeed;
						mouseStickDeadzoneSlider.Value = Game1.inputMan.MouseStickDeadZone;
						moveStickDeadzoneSlider.Value = Game1.inputMan.MoveStickDeadZone;
					}
				});
			}, buttonWidth: 200);
			editButtonsSlider = new SliderControl("", 0, gamepadButtonsToMap.Length - 5, new Vec2(222, 36), 164, useButtons: true, vertical: true);
			editButtonsSlider.OnValueChanged = OnSliderChange;
			editButtonsSlider.ScrollTriggerZone = new Rect(0, 34, 240, 168);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
			DrawButtonNames();
			editButtons = new List<IconButton>();
			for (int i = 0; i < 5; i++)
			{
				Vec2 relativePos2 = new Vec2(188, i * 34 + 34);
				int buttonIndex = i;
				editButtons.Add(new IconButton("the_world_machine/window_icons/edit", relativePos2, delegate
				{
					OnEditButton(buttonIndex);
				}));
			}
			onClose = (Action<TWMWindow>)Delegate.Combine(onClose, (Action<TWMWindow>)delegate
			{
				Game1.inputMan.SaveCurrentInputMap();
			});
			if (Game1.steamMan.IsSteamInputControllerConnected() && !Game1.steamMan.IsOnSteamDeck)
			{
				changeDisplayedGlyphMode(InputManager.GlyphMode.Keyboard);
			}
			else
			{
				changeDisplayedGlyphMode(Game1.inputMan.CurrentGlyphMode);
			}
		}

		private void changeDisplayedGlyphMode(InputManager.GlyphMode newGlyphMode)
		{
			if (displayedGlyphMode != newGlyphMode)
			{
				displayedGlyphMode = newGlyphMode;
				displayedButtonsStart = 0;
				editButtonsSlider.Value = 0;
				switch (displayedGlyphMode)
				{
				case InputManager.GlyphMode.Gamepad:
					gamepadTabButton.Clickable = false;
					gamepadTabButton.Icon = "the_world_machine/window_icons/gamepad_tab_selected";
					keyboardTabButton.Clickable = true;
					keyboardTabButton.Icon = "the_world_machine/window_icons/keyboard_tab";
					editButtonsSlider.Max = gamepadButtonsToMap.Length - 5;
					break;
				case InputManager.GlyphMode.Keyboard:
					gamepadTabButton.Clickable = true;
					gamepadTabButton.Icon = "the_world_machine/window_icons/gamepad_tab";
					keyboardTabButton.Clickable = false;
					keyboardTabButton.Icon = "the_world_machine/window_icons/keyboard_tab_selected";
					editButtonsSlider.Max = keyboardButtonsToMap.Length - 5;
					break;
				}
			}
		}

		public void OnEditButton(int index)
		{
			int num = index + displayedButtonsStart;
			InputManager.Button buttonToEdit = ((displayedGlyphMode == InputManager.GlyphMode.Gamepad) ? gamepadButtonsToMap[num] : keyboardButtonsToMap[num]);
			Game1.logMan.Log(LogManager.LogLevel.Info, "EDIT BUTTON PRESSED for " + buttonToEdit.ToString() + "!");
			switch (displayedGlyphMode)
			{
			case InputManager.GlyphMode.Gamepad:
			{
				InputManager.Button button = buttonToEdit;
				if ((uint)(button - 201) <= 1u)
				{
					string message2 = string.Format(Game1.languageMan.GetTWMLocString("controls_stick_assign_prompt"), Game1.languageMan.GetTWMLocString("controls_button_" + buttonToEdit.ToString().ToLowerInvariant()));
					ShowModalWindow(ModalWindow.ModalType.StickAssign, message2, delegate
					{
						Game1.inputMan.SetButton(buttonToEdit, Game1.inputMan.LastAssignedButton);
					});
				}
				else
				{
					string message3 = string.Format(Game1.languageMan.GetTWMLocString("controls_button_assign_prompt"), Game1.languageMan.GetTWMLocString("controls_button_" + buttonToEdit.ToString().ToLowerInvariant()));
					ShowModalWindow(ModalWindow.ModalType.ButtonAssign, message3, delegate
					{
						Game1.inputMan.SetButton(buttonToEdit, Game1.inputMan.LastAssignedButton);
					});
				}
				break;
			}
			case InputManager.GlyphMode.Keyboard:
			{
				string message = string.Format(Game1.languageMan.GetTWMLocString("controls_key_assign_prompt"), Game1.languageMan.GetTWMLocString("controls_button_" + buttonToEdit.ToString().ToLowerInvariant()));
				ShowModalWindow(ModalWindow.ModalType.KeyAssign, message, delegate
				{
					Game1.inputMan.SetKey(buttonToEdit, Game1.inputMan.LastAssignedKey);
				});
				break;
			}
			}
		}

		private void DrawButtonNames()
		{
			InputManager.Button[] array = gamepadButtonsToMap;
			foreach (InputManager.Button button in array)
			{
				ButtonMappingDisplay buttonMappingDisplay = new ButtonMappingDisplay(button);
				DrawButtonName(buttonMappingDisplay);
				buttonNameTextures.Add(button, buttonMappingDisplay);
			}
			array = keyboardButtonsToMap;
			foreach (InputManager.Button button2 in array)
			{
				if (!buttonNameTextures.ContainsKey(button2))
				{
					ButtonMappingDisplay buttonMappingDisplay2 = new ButtonMappingDisplay(button2);
					DrawButtonName(buttonMappingDisplay2);
					buttonNameTextures.Add(button2, buttonMappingDisplay2);
				}
			}
		}

		private void DrawButtonName(ButtonMappingDisplay bmd)
		{
			bmd.buttonNameTex = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("controls_button_" + bmd.button.ToString().ToLowerInvariant()), 150);
		}

		private void OnSliderChange(int newVal)
		{
			displayedButtonsStart = newVal;
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			GameColor gameColor = theme.Primary(alpha);
			GameColor gColor2 = theme.Variant(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gColor);
			Game1.gMan.ColorBoxBlit(new Rect(screenPos.X, screenPos.Y + 32, 240, 2), gameColor);
			editButtonsSlider.Draw(theme, screenPos, alpha);
			gamepadTabButton.Draw(screenPos, theme, alpha);
			if (!hideKeyboardControls)
			{
				keyboardTabButton.Draw(screenPos, theme, alpha);
			}
			for (int i = 0; i < 5; i++)
			{
				Vec2 vec = screenPos + new Vec2(0, i * 34 + 34);
				int num = i + displayedButtonsStart;
				if (num >= ((displayedGlyphMode == InputManager.GlyphMode.Gamepad) ? gamepadButtonsToMap.Length : keyboardButtonsToMap.Length))
				{
					break;
				}
				InputManager.Button key = ((displayedGlyphMode == InputManager.GlyphMode.Gamepad) ? gamepadButtonsToMap[num] : keyboardButtonsToMap[num]);
				ButtonMappingDisplay buttonMappingDisplay = buttonNameTextures[key];
				Vec2 pixelPos = vec * 2;
				pixelPos.Y += 24;
				pixelPos.X += 300 - buttonMappingDisplay.buttonNameTex.renderTarget.Width;
				Game1.gMan.MainBlit(buttonMappingDisplay.buttonNameTex, pixelPos, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				Vec2 pixelPos2 = vec;
				pixelPos2.X += 154;
				pixelPos2.Y += 6;
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, pixelPos2, Game1.inputMan.ButtonToGlyphString(buttonMappingDisplay.button), gameColor, GraphicsManager.BlendMode.Normal, 2, GraphicsManager.TextBlitMode.Normal, displayedGlyphMode);
				if (i != 4)
				{
					Game1.gMan.ColorBoxBlit(new Rect(vec.X + 2, vec.Y + 32, 218, 2), gColor2);
				}
			}
			foreach (IconButton editButton in editButtons)
			{
				editButton.Draw(screenPos, theme, alpha);
			}
			Game1.gMan.ColorBoxBlit(new Rect(screenPos.X + 240, screenPos.Y, 2, 202), gameColor);
			if (displayedGlyphMode == InputManager.GlyphMode.Gamepad)
			{
				moveStickDeadzoneSlider.Draw(theme, screenPos, alpha);
				mouseStickDeadzoneSlider.Draw(theme, screenPos, alpha);
				mouseCursorSpeedSlider.Draw(theme, screenPos, alpha);
			}
			defaultControlsButton.Draw(screenPos, theme, alpha);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is ControlsWindow;
		}

		public override bool Update(bool mouseInputWasConsumed)
		{
			foreach (ButtonMappingDisplay value in buttonNameTextures.Values)
			{
				if (!value.buttonNameTex.isValid)
				{
					DrawButtonName(value);
				}
				value.buttonNameTex.KeepAlive();
			}
			bool canInteract = !mouseInputWasConsumed && !base.IsMinimized && !IsModalWindowOpen();
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			gamepadTabButton.Update(parentPos, canInteract);
			if (!hideKeyboardControls)
			{
				keyboardTabButton.Update(parentPos, canInteract);
			}
			editButtonsSlider.Update(parentPos, canInteract);
			foreach (IconButton editButton in editButtons)
			{
				editButton.Update(parentPos, canInteract);
			}
			if (displayedGlyphMode == InputManager.GlyphMode.Gamepad)
			{
				moveStickDeadzoneSlider.Update(parentPos, canInteract);
				mouseStickDeadzoneSlider.Update(parentPos, canInteract);
				mouseCursorSpeedSlider.Update(parentPos, canInteract);
			}
			defaultControlsButton.Update(parentPos, canInteract);
			mouseInputWasConsumed |= base.Update(mouseInputWasConsumed);
			return mouseInputWasConsumed;
		}
	}
}

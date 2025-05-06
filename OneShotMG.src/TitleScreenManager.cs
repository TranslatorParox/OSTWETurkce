using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Menus;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class TitleScreenManager : AbstractMenu
	{
		private OneshotWindow oneshotWindow;

		private int fadeOpacity = 255;

		private const int OPEN_CLOSE_FADE_STEP = 5;

		private List<TempTexture> options;

		private int selectedOptionIndex;

		private const int OPTIONS_DRAW_X = 245;

		private const int OPTIONS_DRAW_Y = 190;

		private const int OPTION_START = 0;

		private const int OPTION_SETTINGS = 1;

		private const int OPTION_EXIT = 2;

		private const int OPTION_MEMORY = 3;

		private const int PICKED_MEMORY_AT_TITLE_FLAG = 157;

		private int inputHelpTextTimer;

		private const int INPUT_HELP_TEXT_WAIT_TIME_NO_MOVEMENT = 600;

		private const int INPUT_HELP_TEXT_WAIT_TIME_NO_SELECTION = 1200;

		private bool hasAnyOptionBeenSelected;

		private bool hasMenuCursorBeenMoved;

		private bool showInputHelpText;

		private int helpTextFadeInTimer;

		private const int HELP_TEXT_FADE_IN_TIME = 60;

		private const int HELP_TEXT_X = 620;

		private const int HELP_TEXT_Y = 16;

		private const byte HELP_TEXT_ALPHA = 180;

		private TempTexture helpTextTexture1;

		private TempTexture helpTextTexture2;

		private InputManager.GlyphMode helpTextGlyphMode;

		public TitleScreenManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			DrawOptionsTextures();
		}

		private void DrawOptionsTextures()
		{
			options = new List<TempTexture>
			{
				Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("title_option_start")),
				Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("title_option_settings")),
				Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("title_option_exit"))
			};
			if (oneshotWindow.flagMan.IsFlagSet(160) && oneshotWindow.flagMan.IsFlagSet(152))
			{
				options.Add(Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("title_option_dots")));
			}
		}

		public override void Update()
		{
			if (IsOpen())
			{
				bool flag = false;
				foreach (TempTexture option in options)
				{
					if (option == null || !option.isValid)
					{
						flag = true;
						break;
					}
					option.KeepAlive();
				}
				if (flag)
				{
					DrawOptionsTextures();
				}
				if (helpTextTexture1 == null || !helpTextTexture1.isValid || helpTextGlyphMode != Game1.inputMan.CurrentGlyphMode)
				{
					helpTextTexture1 = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("title_help_text_1"));
					helpTextGlyphMode = Game1.inputMan.CurrentGlyphMode;
				}
				helpTextTexture1.KeepAlive();
				if (helpTextTexture2 == null || !helpTextTexture2.isValid)
				{
					helpTextTexture2 = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("title_help_text_2"));
				}
				helpTextTexture2.KeepAlive();
			}
			if (!hasAnyOptionBeenSelected && !showInputHelpText)
			{
				inputHelpTextTimer++;
				if ((!hasMenuCursorBeenMoved && inputHelpTextTimer >= 600) || (!hasAnyOptionBeenSelected && inputHelpTextTimer >= 1200))
				{
					showInputHelpText = true;
				}
			}
			if (showInputHelpText && helpTextFadeInTimer < 60)
			{
				helpTextFadeInTimer++;
			}
			switch (state)
			{
			case MenuState.Opening:
				fadeOpacity -= 5;
				if (fadeOpacity <= 0)
				{
					fadeOpacity = 0;
					state = MenuState.Open;
				}
				break;
			case MenuState.Closing:
				fadeOpacity += 5;
				if (fadeOpacity >= 255)
				{
					fadeOpacity = 255;
					state = MenuState.Closed;
					switch (selectedOptionIndex)
					{
					case 0:
						oneshotWindow.flagMan.UnsetFlag(157);
						oneshotWindow.StartGame();
						break;
					case 2:
						oneshotWindow.ExitGame();
						break;
					case 3:
						oneshotWindow.flagMan.SetFlag(157);
						oneshotWindow.StartGame();
						break;
					case 1:
						break;
					}
				}
				break;
			case MenuState.Open:
			{
				if (oneshotWindow.menuMan.IsMenuOpen())
				{
					break;
				}
				int num = selectedOptionIndex;
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Up))
				{
					num--;
					if (num < 0)
					{
						num = 0;
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Down))
				{
					num++;
					if (num > options.Count - 1)
					{
						num = options.Count - 1;
					}
				}
				if (num != selectedOptionIndex)
				{
					hasMenuCursorBeenMoved = true;
					selectedOptionIndex = num;
					Game1.soundMan.PlaySound("title_cursor", 0.5f);
				}
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					hasAnyOptionBeenSelected = true;
					switch (selectedOptionIndex)
					{
					case 0:
						Game1.windowMan.StartCurrentPlaythroughTimer();
						Game1.soundMan.PlaySound("title_decision");
						Close();
						break;
					case 1:
						oneshotWindow.menuMan.ShowMenu(MenuManager.Menus.SettingsMenu);
						break;
					case 2:
						Game1.soundMan.PlaySound("title_decision");
						Close();
						break;
					case 3:
						Game1.soundMan.PlaySound("title_decision");
						Close();
						break;
					}
				}
				break;
			}
			}
		}

		public override void Draw()
		{
			if (!IsOpen())
			{
				return;
			}
			Game1.gMan.MainBlit("titles/normal", Vec2.Zero);
			Vec2 vec = new Vec2(488, 380);
			for (int i = 0; i < options.Count; i++)
			{
				TempTexture tempTexture = options[i];
				Game1.gMan.MainBlit(tempTexture, vec, GameColor.White, 0, GraphicsManager.BlendMode.Normal, 1);
				vec.Y += 25;
				if (selectedOptionIndex == i)
				{
					Vec2 pixelPos = vec;
					pixelPos.X /= 2;
					pixelPos.Y /= 2;
					pixelPos.X -= 5;
					pixelPos.Y -= 10;
					Game1.gMan.MainBlit("ui/title_cursor", pixelPos);
				}
			}
			if (helpTextTexture1 != null && helpTextTexture2 != null)
			{
				Vec2 vec2 = new Vec2(620 - helpTextTexture1.renderTarget.Width, 32);
				byte a = (byte)(helpTextFadeInTimer * 180 / 60);
				GameColor gameColor = new GameColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, a);
				Game1.gMan.MainBlit(helpTextTexture1, vec2 + new Vec2(-2, 0), gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				Game1.gMan.TextBlit(GraphicsManager.FontType.Game, vec2, Game1.languageMan.GetTWMLocString("title_help_text_1"), gameColor, GraphicsManager.BlendMode.Normal, 1, GraphicsManager.TextBlitMode.OnlyGlyphes);
				vec2.X = 620 - helpTextTexture2.renderTarget.Width;
				vec2.Y += 52;
				Game1.gMan.MainBlit(helpTextTexture2, vec2 + new Vec2(-2, 0), gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				Game1.gMan.TextBlit(GraphicsManager.FontType.Game, vec2, Game1.languageMan.GetTWMLocString("title_help_text_2"), gameColor, GraphicsManager.BlendMode.Normal, 1, GraphicsManager.TextBlitMode.OnlyGlyphes);
			}
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)fadeOpacity));
		}

		public override void Open()
		{
			Game1.soundMan.PlaySong("MyBurdenIsLight", 1f);
			fadeOpacity = 255;
			state = MenuState.Opening;
			Game1.windowMan.ResetCurrentPlaythroughTimer();
		}

		public override void Close()
		{
			Game1.soundMan.FadeOutBGM(0.5f);
			fadeOpacity = 0;
			state = MenuState.Closing;
		}
	}
}

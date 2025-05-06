using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Entities;
using OneShotMG.src.MessageBox;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class NameInputMenu : AbstractMenu
	{
		private readonly OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 30;

		private int cursorAnimTimer;

		private const int CURSOR_ANIM_CYCLE_FRAMES = 32;

		private float cursorAlpha = 0.5f;

		private const int CURSOR_TIME_TO_AUTO_TRIGGER = 20;

		private const int CURSOR_TIME_BETWEEN_AUTO_TRIGGER = 4;

		private Vec2 cursorIndex = Vec2.Zero;

		private const GraphicsManager.FontType InputFont = GraphicsManager.FontType.Game;

		private const int INPUT_MAX_CHARACTERS = 16;

		private const int LETTER_CURSOR_WIDTH = 28;

		private const int LETTER_CURSOR_HEIGHT = 32;

		private const int LETTER_TOP_BUFFER = 5;

		private const int INPUT_NAME_Y = 48;

		private const int LETTERS_Y = 180;

		private const int LETTERS_OPTIONS_Y_MARGIN = 8;

		private const int LETTERS_OPTIONS_CURSOR_WIDTH = 80;

		private static readonly string[] UpperCaseLetters = new string[50]
		{
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
			"K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
			"U", "V", "W", "X", "Y", "Z", " ", ".", ",", "-",
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
			"!", "?", "\"", "'", "/", "&", "[", "]", "(", ")"
		};

		private static readonly string[] LowerCaseLetters = new string[50]
		{
			"a", "b", "c", "d", "e", "f", "g", "h", "i", "j",
			"k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
			"u", "v", "w", "x", "y", "z", " ", ".", ",", "-",
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
			"!", "?", "\"", "'", "/", "&", "[", "]", "(", ")"
		};

		private static readonly string[] Hiragana = new string[90]
		{
			"ー", "っ", "ぁ", "ぱ", "ば", "だ", "ざ", "が", "わ", "ら",
			"や", "ま", "は", "な", "た", "さ", "か", "あ", "～", "ゃ",
			"ぃ", "ぴ", "び", "ぢ", "じ", "ぎ", " ", "り", " ", "み",
			"ひ", "に", "ち", "し", "き", "い", "・", "ゅ", "ぅ", "ぷ",
			"ぶ", "づ", "ず", "ぐ", "を", "る", "ゆ", "む", "ふ", "ぬ",
			"つ", "す", "く", "う", "＝", "ょ", "ぇ", "ぺ", "べ", "で",
			"ぜ", "げ", " ", "れ", " ", "め", "へ", "ね", "て", "せ",
			"け", "え", "☆", "ゎ", "ぉ", "ぽ", "ぼ", "ど", "ぞ", "ご",
			"ん", "ろ", "よ", "も", "ほ", "の", "と", "そ", "こ", "お"
		};

		private static readonly string[] Katakana = new string[90]
		{
			"ー", "ッ", "ァ", "パ", "バ", "ダ", "ザ", "ガ", "ワ", "ラ",
			"ヤ", "マ", "ハ", "ナ", "タ", "サ", "カ", "ア", "～", "ャ",
			"ィ", "ピ", "ビ", "ヂ", "ジ", "ギ", " ", "リ", " ", "ミ",
			"ヒ", "ニ", "チ", "シ", "キ", "イ", "・", "ュ", "ゥ", "プ",
			"ブ", "ヅ", "ズ", "グ", "ヲ", "ル", "ユ", "ム", "フ", "ヌ",
			"ツ", "ス", "ク", "ウ", "＝", "ョ", "ェ", "ペ", "ベ", "デ",
			"ゼ", "ゲ", " ", "レ", " ", "メ", "ヘ", "ネ", "テ", "セ",
			"ケ", "エ", "☆", "ヮ", "ォ", "ポ", "ボ", "ド", "ゾ", "ゴ",
			"ン", "ロ", "ヨ", "モ", "ホ", "ノ", "ト", "ソ", "コ", "オ"
		};

		private static readonly string[] Romaji = new string[90]
		{
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "a",
			"b", "c", "d", "e", "f", "g", "h", "i", "J", "K",
			"L", "M", "N", "O", "P", "Q", "R", "j", "k", "l",
			"m", "n", "o", "p", "q", "r", "S", "T", "U", "V",
			"W", "X", "Y", "Z", " ", "s", "t", "u", "v", "w",
			"x", "y", "z", " ", "0", "1", "2", "3", "4", "+",
			"-", "=", "&", "!", "?", "'", "\"", ".", ",", ":",
			";", "$", "5", "6", "7", "8", "9", "*", "/", "#",
			"|", "[", "]", "(", ")", "<", ">", "{", "}", "@"
		};

		private static readonly string[] RuUpper = new string[90]
		{
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "А",
			"Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "J", "K",
			"L", "M", "N", "O", "P", "Q", "R", "И", "Й", "К",
			"Л", "М", "Н", "О", "П", "Р", "S", "T", "U", "V",
			"W", "X", "Y", "Z", " ", "С", "Т", "У", "Ф", "Х",
			"Ц", "Ч", "Ш", " ", "0", "1", "2", "3", "4", "+",
			"-", "=", "&", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я",
			" ", " ", "5", "6", "7", "8", "9", "*", "/", "#",
			"|", "^", "%", "~", " ", " ", " ", " ", " ", " "
		};

		private static readonly string[] RuLower = new string[90]
		{
			"a", "b", "c", "d", "e", "f", "g", "h", "i", "а",
			"б", "в", "г", "д", "е", "ё", "ж", "з", "j", "k",
			"l", "m", "n", "o", "p", "q", "r", "и", "й", "к",
			"л", "м", "н", "о", "п", "р", "s", "t", "u", "v",
			"w", "x", "y", "z", " ", "с", "т", "у", "ф", "х",
			"ц", "ч", "ш", " ", "!", "?", "'", "\"", ".", ",",
			":", ";", "$", "щ", "ъ", "ы", "ь", "э", "ю", "я",
			" ", " ", "[", "]", "(", ")", "<", ">", "{", "}",
			"@", " ", " ", " ", " ", " ", " ", " ", " ", " "
		};

		private static readonly string[] EuUpper = new string[90]
		{
			"A", "B", "C", "D", "E", "F", "G", "H", "I", "À",
			"Á", "Â", "Ã", "Ä", "Å", "Æ", "Ç", "È", "J", "K",
			"L", "M", "N", "O", "P", "Q", "R", "É", "Ê", "Ë",
			"Ì", "Í", "Î", "Ï", "Ñ", "Ò", "S", "T", "U", "V",
			"W", "X", "Y", "Z", " ", "Ó", "Ô", "Õ", "Ö", "Ù",
			"Ú", "Û", "Ü", "Ý", "0", "1", "2", "3", "4", "+",
			"-", "=", "&", "!", "?", "'", "\"", ".", ",", ":",
			";", "$", "5", "6", "7", "8", "9", "*", "/", "#",
			"|", "[", "]", "(", ")", "<", ">", "{", "}", "@"
		};

		private static readonly string[] EuLower = new string[90]
		{
			"a", "b", "c", "d", "e", "f", "g", "h", "i", "à",
			"á", "â", "ã", "ä", "å", "æ", "ç", "è", "j", "k",
			"l", "m", "n", "o", "p", "q", "r", "é", "ê", "ë",
			"ì", "í", "î", "ï", "ñ", "ò", "s", "t", "u", "v",
			"w", "x", "y", "z", " ", "ó", "ô", "õ", "ö", "ù",
			"ú", "û", "ü", "ý", "0", "1", "2", "3", "4", "+",
			"-", "=", "&", "¡", "¿", "'", "\"", ".", ",", ":",
			";", "$", "5", "6", "7", "8", "9", "*", "/", "#",
			"|", "[", "]", "(", ")", "<", ">", "{", "}", "@"
		};

		private const int DELETE_INSTRUCTION_Y = 420;

		private int letterGridWidth = 10;

		private int letterGridHeight = 5;

		private const int RU_GRID_WIDTH = 18;

		private const int RU_GRID_HEIGHT = 5;

		private const int EU_GRID_WIDTH = 18;

		private const int EU_GRID_HEIGHT = 5;

		private const int KANA_GRID_WIDTH = 18;

		private const int KANA_GRID_HEIGHT = 5;

		private const int LETTER_GRID_WIDTH = 10;

		private const int LETTER_GRID_HEIGHT = 5;

		private string inputName = string.Empty;

		private string[] currentLetters = UpperCaseLetters;

		private const int INPUT_IS_PASSWORD_FLAG = 91;

		private EventRunner owner;

		private TempTexture instructionTexture;

		private TempTexture okLabelTexture;

		private TempTexture characterSwitchTexture;

		private TempTexture lettersTexture;

		public NameInputMenu(EventRunner owner, OneshotWindow osWindow)
		{
			this.owner = owner;
			oneshotWindow = osWindow;
			switch (Game1.languageMan.GetCurrentLangCode())
			{
			case "ja":
				currentLetters = Romaji;
				letterGridWidth = 18;
				letterGridHeight = 5;
				break;
			case "ru":
				currentLetters = RuUpper;
				letterGridWidth = 18;
				letterGridHeight = 5;
				break;
			case "es":
			case "fr":
			case "it":
			case "pt_br":
				currentLetters = EuUpper;
				letterGridWidth = 18;
				letterGridHeight = 5;
				break;
			default:
				currentLetters = UpperCaseLetters;
				letterGridWidth = 10;
				letterGridHeight = 5;
				break;
			}
		}

		public override void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)opacity;
			float num = (float)opacity / 255f;
			Game1.gMan.ColorBoxBlit(new Rect(2, 2, 316, 236), new GameColor(24, 12, 30, (byte)(opacity * 200 / 255)));
			TextBox.DrawWindowBorder(new Rect(0, 0, 320, 64), GraphicsManager.BlendMode.Normal, num);
			TextBox.DrawWindowBorder(new Rect(0, 64, 320, 176), GraphicsManager.BlendMode.Normal, num);
			Vec2 pixelPos = new Vec2(96, 48);
			for (int i = 0; i < 16; i++)
			{
				string text = "_";
				if (inputName.Length > i)
				{
					text = inputName[i].ToString();
				}
				else if (inputName.Length == i)
				{
					Game1.gMan.MainBlit("ui/name_select_letter_cursor", pixelPos, cursorAlpha * num, 0, GraphicsManager.BlendMode.Normal, 1);
				}
				Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, new Vec2(pixelPos.X + 14 + 1, pixelPos.Y + 5), text, white, GraphicsManager.BlendMode.Normal, 1);
				pixelPos.X += 28;
			}
			if (cursorIndex.Y < letterGridHeight)
			{
				Vec2 pixelPos2 = new Vec2(320 - letterGridWidth * 28 / 2 + cursorIndex.X * 28, 180 + cursorIndex.Y * 32);
				Game1.gMan.MainBlit("ui/name_select_letter_cursor", pixelPos2, cursorAlpha * num, 0, GraphicsManager.BlendMode.Normal, 1);
			}
			Vec2 pixelPos3 = new Vec2(320 - letterGridWidth * 28 / 2, 180);
			Game1.gMan.MainBlit(lettersTexture, pixelPos3, white, 0, GraphicsManager.BlendMode.Normal, 1);
			if (cursorIndex.Y == letterGridHeight)
			{
				Vec2 pixelPos4 = new Vec2(320, 180 + letterGridHeight * 32 + 8);
				if (cursorIndex.X == 0)
				{
					pixelPos4.X -= letterGridWidth * 28 / 2;
				}
				else
				{
					pixelPos4.X += letterGridWidth * 28 / 2 - 80;
				}
				Game1.gMan.MainBlit("ui/name_select_bottom_cursor", pixelPos4, cursorAlpha * num, 0, GraphicsManager.BlendMode.Normal, 1);
			}
			Vec2 pixelPos5 = new Vec2(320 - letterGridWidth * 28 / 2 + 40, 180 + letterGridHeight * 32 + 8 + 5);
			Game1.gMan.MainBlit(characterSwitchTexture, pixelPos5, white, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			Vec2 pixelPos6 = new Vec2(320 + letterGridWidth * 28 / 2 - 40, 180 + letterGridHeight * 32 + 8 + 5);
			Game1.gMan.MainBlit(okLabelTexture, pixelPos6, white, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			Game1.gMan.MainBlit(instructionTexture, new Vec2(320, 420), white, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, new Vec2(320, 420), Game1.languageMan.GetTWMLocString("name_input_delete_instruction"), white, GraphicsManager.BlendMode.Normal, 1, checkForGlyphes: false, GraphicsManager.TextBlitMode.OnlyGlyphes);
		}

		private void DrawLettersTexture()
		{
			if (lettersTexture == null || !lettersTexture.isValid)
			{
				lettersTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(letterGridWidth * 28, letterGridHeight * 32));
			}
			Game1.gMan.BeginDrawToTempTexture(lettersTexture);
			Vec2 vec = new Vec2(0, 0);
			for (int i = 0; i < letterGridWidth; i++)
			{
				for (int j = 0; j < letterGridHeight; j++)
				{
					int num = i + j * letterGridWidth;
					Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, new Vec2(vec.X + 14 + 1, vec.Y + 5), currentLetters[num], GameColor.White, GraphicsManager.BlendMode.Normal, 1);
					vec.Y += 32;
				}
				vec.Y = 0;
				vec.X += 28;
			}
			Game1.gMan.EndDrawToTempTexture();
		}

		private void DrawCharacterSwitchTexture()
		{
			string tWMLocString = Game1.languageMan.GetTWMLocString("name_input_upper");
			switch (Game1.languageMan.GetCurrentLangCode())
			{
			default:
				if (currentLetters == UpperCaseLetters)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_lower");
				}
				else if (currentLetters == LowerCaseLetters)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_upper");
				}
				break;
			case "ja":
				if (currentLetters == Romaji)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_hiragana");
				}
				else if (currentLetters == Hiragana)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_katakana");
				}
				else if (currentLetters == Katakana)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_romaji");
				}
				break;
			case "ru":
				if (currentLetters == RuUpper)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_lower");
				}
				else if (currentLetters == RuLower)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_upper");
				}
				break;
			case "es":
			case "fr":
			case "it":
			case "pt_br":
				if (currentLetters == EuUpper)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_lower");
				}
				else if (currentLetters == EuLower)
				{
					tWMLocString = Game1.languageMan.GetTWMLocString("name_input_upper");
				}
				break;
			}
			characterSwitchTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, tWMLocString, 76);
		}

		public override void Update()
		{
			if (IsOpen())
			{
				if (instructionTexture == null || !instructionTexture.isValid)
				{
					instructionTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("name_input_delete_instruction"));
				}
				instructionTexture.KeepAlive();
				if (okLabelTexture == null || !okLabelTexture.isValid)
				{
					okLabelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, Game1.languageMan.GetTWMLocString("name_input_ok"));
				}
				okLabelTexture.KeepAlive();
				if (characterSwitchTexture == null || !characterSwitchTexture.isValid)
				{
					DrawCharacterSwitchTexture();
				}
				characterSwitchTexture.KeepAlive();
				if (lettersTexture == null || !lettersTexture.isValid)
				{
					DrawLettersTexture();
				}
				lettersTexture.KeepAlive();
			}
			switch (state)
			{
			case MenuState.Opening:
				opacity += 30;
				if (opacity >= 255)
				{
					opacity = 255;
					state = MenuState.Open;
				}
				break;
			case MenuState.Closing:
				opacity -= 30;
				if (opacity <= 0)
				{
					opacity = 0;
					state = MenuState.Closed;
				}
				break;
			case MenuState.Open:
				cursorAnimTimer++;
				if (cursorAnimTimer >= 32)
				{
					cursorAnimTimer = 0;
				}
				cursorAlpha = 0.5f + (float)Math.Abs(16 - cursorAnimTimer) / 32f;
				HandleCursorInput();
				break;
			}
		}

		private void HandleCursorInput()
		{
			Vec2 vec = cursorIndex;
			bool num = Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Right, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Left));
			bool flag = Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Left, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Right));
			bool flag2 = Game1.inputMan.IsButtonPressed(InputManager.Button.Up) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Up, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down));
			bool flag3 = Game1.inputMan.IsButtonPressed(InputManager.Button.Down) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Down, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up));
			if (num)
			{
				cursorIndex.X++;
				if (cursorIndex.Y < letterGridHeight)
				{
					if (cursorIndex.X >= letterGridWidth)
					{
						cursorIndex.X = 0;
					}
				}
				else if (cursorIndex.X > 1)
				{
					cursorIndex.X = 0;
				}
			}
			else if (flag)
			{
				cursorIndex.X--;
				if (cursorIndex.Y < letterGridHeight)
				{
					if (cursorIndex.X < 0)
					{
						cursorIndex.X = letterGridWidth - 1;
					}
				}
				else if (cursorIndex.X < 0)
				{
					cursorIndex.X = 1;
				}
			}
			if (flag2)
			{
				if (cursorIndex.Y == letterGridHeight && cursorIndex.X > 0)
				{
					cursorIndex.X = letterGridWidth - 1;
				}
				cursorIndex.Y--;
				if (cursorIndex.Y < 0)
				{
					cursorIndex.Y = 0;
				}
			}
			else if (flag3)
			{
				cursorIndex.Y++;
				if (cursorIndex.Y == letterGridHeight)
				{
					cursorIndex.X = ((cursorIndex.X >= letterGridWidth / 2) ? 1 : 0);
				}
				if (cursorIndex.Y > letterGridHeight)
				{
					cursorIndex.Y = letterGridHeight;
				}
			}
			if (vec.X != cursorIndex.X || vec.Y != cursorIndex.Y)
			{
				Game1.soundMan.PlaySound("menu_cursor");
			}
			if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
			{
				if (cursorIndex.Y == letterGridHeight)
				{
					if (cursorIndex.X == 0)
					{
						switch (Game1.languageMan.GetCurrentLangCode())
						{
						default:
							if (currentLetters == UpperCaseLetters)
							{
								currentLetters = LowerCaseLetters;
							}
							else if (currentLetters == LowerCaseLetters)
							{
								currentLetters = UpperCaseLetters;
							}
							break;
						case "ja":
							if (currentLetters == Romaji)
							{
								currentLetters = Hiragana;
							}
							else if (currentLetters == Hiragana)
							{
								currentLetters = Katakana;
							}
							else if (currentLetters == Katakana)
							{
								currentLetters = Romaji;
							}
							break;
						case "ru":
							if (currentLetters == RuUpper)
							{
								currentLetters = RuLower;
							}
							else if (currentLetters == RuLower)
							{
								currentLetters = RuUpper;
							}
							break;
						case "es":
						case "fr":
						case "it":
						case "pt_br":
							if (currentLetters == EuUpper)
							{
								currentLetters = EuLower;
							}
							else if (currentLetters == EuLower)
							{
								currentLetters = EuUpper;
							}
							break;
						}
						DrawCharacterSwitchTexture();
						DrawLettersTexture();
						Game1.soundMan.PlaySound("menu_decision");
					}
					else if (inputName.Length > 0)
					{
						Game1.soundMan.PlaySound("menu_decision");
						if (oneshotWindow.flagMan.IsFlagSet(91))
						{
							owner.PasswordInput = inputName;
						}
						else
						{
							oneshotWindow.gameSaveMan.SetPlayerName(inputName);
						}
						Close();
					}
				}
				else if (inputName.Length >= 16)
				{
					Game1.soundMan.PlaySound("menu_buzzer");
				}
				else
				{
					Game1.soundMan.PlaySound("menu_decision");
					inputName += currentLetters[cursorIndex.X + cursorIndex.Y * letterGridWidth];
				}
			}
			else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel) && inputName.Length > 0)
			{
				Game1.soundMan.PlaySound("menu_cancel");
				inputName = inputName.Substring(0, inputName.Length - 1);
			}
		}

		public override void Close()
		{
			state = MenuState.Closing;
			opacity = 255;
		}

		public override void Open()
		{
			if (oneshotWindow.flagMan.IsFlagSet(91))
			{
				inputName = string.Empty;
			}
			else
			{
				inputName = oneshotWindow.gameSaveMan.GetPlayerName();
			}
			state = MenuState.Opening;
			opacity = 0;
			cursorAlpha = 0.5f;
		}
	}
}

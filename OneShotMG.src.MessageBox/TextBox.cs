using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.MessageBox
{
	public class TextBox : IMessageBox
	{
		public enum TextBoxStyle
		{
			GoldBorder,
			NoBorder
		}

		public enum TextBoxArea
		{
			Up,
			Middle,
			Down
		}

		private readonly OneshotWindow oneshotWindow;

		private MessageBoxState state;

		private float textboxAlpha;

		private float scrollSpeed = 1f;

		private float scrollTimer;

		private int scrollSoundTimer;

		private const int SCROLL_SOUND_INTERVAL = 4;

		private string scrollSoundName = "text";

		private int nodTimer;

		private const int NOD_TIMER_INTERVAL = 8;

		private int nodOffsetIndex;

		private readonly int[] nodOffsets = new int[4] { 0, 1, 2, 1 };

		private Rect containingBox;

		private int transitionTimer;

		private int totalTransitionTime;

		private const int OPEN_TIME = 6;

		private const int CLOSE_TIME = 6;

		private GameColor boxColor;

		private GameColor textColor;

		private string inputText;

		private List<string> displayedLines;

		private int currentLinePxLength;

		private GraphicsManager.FontType font = GraphicsManager.FontType.Game;

		private Rect textPadding;

		public const int TEXT_LINE_HEIGHT = 12;

		private string portraitName = string.Empty;

		private const int PORTRAIT_WIDTH = 48;

		private bool forcedNod;

		private int textPauseTimer;

		private const int TEXT_PAUSE_TIME = 10;

		private const int TEXT_PAUSE_TIME_LONG = 40;

		private int inputNumberVar = -1;

		private int[] inputNumberDigits;

		private int inputDigitCursorPos;

		private const int INPUT_COLOR_NUMBERS_FLAG = 179;

		private const int INPUT_NUM_LINE_EXTRA_MARGIN = 4;

		private int cursorAnimTimer;

		private const int CURSOR_ANIM_CYCLE_FRAMES = 32;

		private float cursorAlpha = 0.5f;

		private const int CURSOR_TIME_TO_AUTO_TRIGGER = 20;

		private const int CURSOR_TIME_BETWEEN_AUTO_TRIGGER = 4;

		private List<string> choices;

		private int cancelChoice = -1;

		private Action<int> choiceCallback;

		private int choiceCursorIndex;

		private int choiceBeginsOnLine;

		private bool showChoicesFollows;

		private TempTexture textTexture;

		private Vec2 currentDrawPos;

		private GameColor currentDrawColor = GameColor.White;

		public TextBoxStyle CurrentStyle { get; private set; }

		public TextBoxArea CurrentArea { get; private set; } = TextBoxArea.Down;

		public TextBox(OneshotWindow osWindow, TextBoxStyle style, TextBoxArea area)
		{
			oneshotWindow = osWindow;
			CurrentStyle = style;
			CurrentArea = area;
			int y = 0;
			switch (CurrentArea)
			{
			case TextBoxArea.Down:
				y = 168;
				break;
			case TextBoxArea.Middle:
				y = 80;
				break;
			case TextBoxArea.Up:
				y = 8;
				break;
			}
			Rect targetBox = new Rect(8, y, 304, 64);
			Initialize(targetBox, new GameColor(24, 12, 30, 210), GameColor.White, new Rect(12, 8, 12, 8));
		}

		public void Initialize(Rect targetBox, GameColor bColor, GameColor tColor, Rect padding)
		{
			containingBox = targetBox;
			boxColor = bColor;
			textColor = tColor;
			inputText = string.Empty;
			displayedLines = new List<string>();
			textboxAlpha = 0f;
			textPadding = padding;
			state = MessageBoxState.Opening;
			transitionTimer = 0;
			totalTransitionTime = 6;
			CreateTextTexture();
			ClearTextTexture();
		}

		public void SizeBoxHeightToText()
		{
			containingBox.H = textPadding.Y + textPadding.H + 12 * displayedLines.Count;
		}

		public bool IsRollingText()
		{
			if (!string.IsNullOrEmpty(inputText) && !forcedNod)
			{
				return textPauseTimer <= 0;
			}
			return false;
		}

		public bool IsReadyForMoreInput()
		{
			return state == MessageBoxState.Closing;
		}

		public bool IsFinished()
		{
			return state == MessageBoxState.Closed;
		}

		public void RollOutAllText()
		{
			while (IsRollingText())
			{
				ProcessInputText(fastCycle: true);
			}
		}

		public void SetInputNumberToVar()
		{
			int num = 0;
			for (int num2 = inputNumberDigits.Length - 1; num2 >= 0; num2--)
			{
				num += inputNumberDigits[num2] * (int)Math.Pow(10.0, inputNumberDigits.Length - num2 - 1);
			}
			oneshotWindow.varMan.SetVariable(inputNumberVar, num);
		}

		public void Close()
		{
			state = MessageBoxState.Closing;
			totalTransitionTime = 6;
			transitionTimer = 0;
			if (choices != null)
			{
				choices = null;
				choiceCallback = null;
				choiceBeginsOnLine = 0;
				choiceCursorIndex = 0;
			}
		}

		public void Open()
		{
			state = MessageBoxState.Opening;
			totalTransitionTime = 6;
			transitionTimer = 6 - transitionTimer;
			CreateTextTexture();
		}

		public void ClearText()
		{
			currentLinePxLength = 0;
			inputText = string.Empty;
			inputNumberVar = -1;
			displayedLines.Clear();
			portraitName = string.Empty;
			showChoicesFollows = false;
			ClearTextTexture();
		}

		private void CreateTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(containingBox.W * 2, containingBox.H * 2));
			}
		}

		private void ClearTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				CreateTextTexture();
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			Game1.gMan.EndDrawToTempTexture();
			currentDrawPos = Vec2.Zero;
			currentDrawColor = GameColor.White;
		}

		public void Update()
		{
			if (state != MessageBoxState.Closed)
			{
				textTexture?.KeepAlive();
			}
			switch (state)
			{
			case MessageBoxState.Closing:
				transitionTimer++;
				if (transitionTimer < totalTransitionTime)
				{
					textboxAlpha = 1f - (float)transitionTimer / (float)totalTransitionTime;
					break;
				}
				state = MessageBoxState.Closed;
				inputNumberVar = -1;
				transitionTimer = 0;
				textboxAlpha = 0f;
				break;
			case MessageBoxState.Opening:
				transitionTimer++;
				if (transitionTimer < totalTransitionTime)
				{
					textboxAlpha = (float)transitionTimer / (float)totalTransitionTime;
					break;
				}
				state = MessageBoxState.Opened;
				transitionTimer = 0;
				textboxAlpha = 1f;
				break;
			case MessageBoxState.Opened:
				cursorAnimTimer++;
				if (cursorAnimTimer >= 32)
				{
					cursorAnimTimer = 0;
				}
				cursorAlpha = 0.5f + (float)Math.Abs(16 - cursorAnimTimer) / 32f;
				if (textPauseTimer > 0)
				{
					textPauseTimer--;
					if (textPauseTimer > 0)
					{
						break;
					}
				}
				if (!string.IsNullOrEmpty(inputText) && !forcedNod)
				{
					if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK) || Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel) || Game1.inputMan.IsAutoMashing())
					{
						RollOutAllText();
					}
					else
					{
						ProcessInputText();
					}
					break;
				}
				if (IsHandlingChoice())
				{
					HandleChoiceInput();
					break;
				}
				if (IsInputtingNumber())
				{
					HandleNumberInput();
					break;
				}
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK) || Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel) || Game1.inputMan.IsAutoMashing() || showChoicesFollows)
				{
					if (!forcedNod)
					{
						Close();
					}
					else
					{
						forcedNod = false;
					}
				}
				nodTimer++;
				if (nodTimer >= 8)
				{
					nodTimer = 0;
					nodOffsetIndex++;
					if (nodOffsetIndex >= nodOffsets.Length)
					{
						nodOffsetIndex = 0;
					}
				}
				break;
			}
		}

		private void HandleChoiceInput()
		{
			if (choices.Count > 1)
			{
				bool num = Game1.inputMan.IsButtonPressed(InputManager.Button.Up) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Up, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down));
				bool flag = Game1.inputMan.IsButtonPressed(InputManager.Button.Down) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Down, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up));
				if (num)
				{
					choiceCursorIndex--;
					if (choiceCursorIndex < 0)
					{
						choiceCursorIndex = choices.Count - 1;
					}
					Game1.soundMan.PlaySound("menu_cursor");
				}
				else if (flag)
				{
					choiceCursorIndex++;
					if (choiceCursorIndex >= choices.Count)
					{
						choiceCursorIndex = 0;
					}
					Game1.soundMan.PlaySound("menu_cursor");
				}
			}
			if (cancelChoice > 0 && Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
			{
				Game1.soundMan.PlaySound("menu_cancel");
				choiceCallback(cancelChoice - 1);
				Close();
			}
			else if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
			{
				Game1.soundMan.PlaySound("menu_decision");
				choiceCallback(choiceCursorIndex);
				Close();
			}
		}

		private void HandleNumberInput()
		{
			bool flag = Game1.inputMan.IsButtonPressed(InputManager.Button.Right) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Right, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Left));
			bool flag2 = Game1.inputMan.IsButtonPressed(InputManager.Button.Left) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Left, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Right));
			bool num = Game1.inputMan.IsButtonPressed(InputManager.Button.Up) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Up, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Down));
			bool flag3 = Game1.inputMan.IsButtonPressed(InputManager.Button.Down) || (Game1.inputMan.HoldAutoTriggerInput(InputManager.Button.Down, 20, 4) && !Game1.inputMan.IsButtonHeld(InputManager.Button.Up));
			if (inputNumberDigits.Length > 1)
			{
				if (flag2)
				{
					inputDigitCursorPos--;
					if (inputDigitCursorPos < 0)
					{
						inputDigitCursorPos = inputNumberDigits.Length - 1;
					}
					Game1.soundMan.PlaySound("menu_cursor");
				}
				else if (flag)
				{
					inputDigitCursorPos++;
					if (inputDigitCursorPos >= inputNumberDigits.Length)
					{
						inputDigitCursorPos = 0;
					}
					Game1.soundMan.PlaySound("menu_cursor");
				}
			}
			if (num)
			{
				inputNumberDigits[inputDigitCursorPos]++;
				if (inputNumberDigits[inputDigitCursorPos] >= 10)
				{
					inputNumberDigits[inputDigitCursorPos] = 0;
				}
				Game1.soundMan.PlaySound("menu_cursor");
			}
			else if (flag3)
			{
				inputNumberDigits[inputDigitCursorPos]--;
				if (inputNumberDigits[inputDigitCursorPos] < 0)
				{
					inputNumberDigits[inputDigitCursorPos] = 9;
				}
				Game1.soundMan.PlaySound("menu_cursor");
			}
			if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
			{
				Game1.soundMan.PlaySound("menu_decision");
				SetInputNumberToVar();
				Close();
			}
		}

		private void SetPortrait(string newPortrait)
		{
			portraitName = newPortrait.ToLowerInvariant();
			switch (oneshotWindow.tileMapMan.GetPlayer().GetNPCSheet())
			{
			case "niko_gasmask":
			case "niko_bulb_gasmask":
			case "en_gasmask":
			case "en_bulb_gasmask":
				if (portraitName.StartsWith("niko"))
				{
					portraitName = "niko_gasmask";
				}
				break;
			}
			if (IsAprilFools() && portraitName.StartsWith("niko"))
			{
				portraitName = "af";
			}
			if (oneshotWindow.flagMan.IsFlagSet(160) && portraitName.StartsWith("niko"))
			{
				portraitName = portraitName.Replace("niko", "en");
			}
		}

		private bool IsAprilFools()
		{
			DateTime now = DateTime.Now;
			if (now.Month == 4)
			{
				return now.Day == 1;
			}
			return false;
		}

		private void ProcessInputText(bool fastCycle = false)
		{
			if (!fastCycle || (displayedLines.Count <= 0 && choices == null))
			{
				scrollSoundTimer++;
				if (scrollSoundTimer > 4)
				{
					scrollSoundTimer = 0;
					if (!string.IsNullOrEmpty(scrollSoundName))
					{
						float pitch = 1f;
						if (portraitName == "af")
						{
							pitch = MathHelper.FRandom(1f, 1.25f);
						}
						Game1.soundMan.PlaySound(scrollSoundName, 0.5f, pitch);
					}
				}
			}
			if (displayedLines.Count <= 0)
			{
				displayedLines.Add(string.Empty);
			}
			scrollTimer += scrollSpeed;
			while (scrollTimer >= 1f && !string.IsNullOrEmpty(inputText) && !forcedNod && textPauseTimer <= 0)
			{
				char c = inputText[0];
				scrollTimer -= 1f;
				switch (c)
				{
				case '\n':
					addNewLine();
					break;
				case '\\':
					inputText = inputText.Substring(1);
					if (string.IsNullOrEmpty(inputText))
					{
						break;
					}
					c = inputText[0];
					switch (c)
					{
					case '@':
						SetPortrait(inputText.Substring(1, inputText.IndexOf(' ') - 1));
						inputText = inputText.Substring(inputText.IndexOf(' '));
						break;
					case 'n':
						addNewLine();
						break;
					case 'c':
						if (inputText.Length >= 4)
						{
							int colorCode = int.Parse(inputText.Substring(2, 1), CultureInfo.InvariantCulture);
							currentDrawColor = GetTextColor(colorCode);
							inputText = inputText.Substring(3);
						}
						break;
					case '>':
						forcedNod = true;
						break;
					case '.':
						if (!Game1.inputMan.IsAutoMashing())
						{
							textPauseTimer = 10;
							scrollSoundTimer = 0;
						}
						break;
					case '|':
						if (!Game1.inputMan.IsAutoMashing())
						{
							textPauseTimer = 40;
							scrollSoundTimer = 0;
						}
						break;
					default:
						Game1.logMan.Log(LogManager.LogLevel.Warning, $"unrecognized escape character in textbox: \\{c}");
						addNewChar(c);
						break;
					}
					break;
				default:
				{
					bool flag = true;
					if (c == ' ')
					{
						string text = inputText.Substring(1);
						if (!string.IsNullOrEmpty(text))
						{
							string[] array = text.Split(' ', '\n');
							if (array.Length != 0)
							{
								string text2 = " " + array[0];
								text2 = text2.Replace("\\>", "");
								text2 = text2.Replace("\\.", "");
								text2 = text2.Replace("\\|", "");
								if (GetWordWidth(text2) + currentLinePxLength > TextAreaWidth() * 2)
								{
									addNewLine();
									flag = false;
								}
							}
						}
					}
					if (flag)
					{
						if (Game1.gMan.TextSize(font, c.ToString()).X + currentLinePxLength > TextAreaWidth() * 2)
						{
							addNewLine();
						}
						addNewChar(c);
					}
					break;
				}
				}
				inputText = inputText.Substring(1);
			}
		}

		public void ShowChoicesIsNextCommand(bool showChoices)
		{
			showChoicesFollows = showChoices;
		}

		public void SetChoices(List<string> newChoices, int cancelValue, Action<int> choiceComplete, string playerName)
		{
			choices = newChoices;
			cancelChoice = cancelValue;
			choiceCallback = choiceComplete;
			choiceBeginsOnLine = displayedLines.Count;
			string text = string.Join("\n", choices);
			if (displayedLines.Count > 0)
			{
				text = "\n" + text;
			}
			FeedText(text, playerName);
			RollOutAllText();
		}

		private int GetWordWidth(string nextWord)
		{
			int num = 0;
			foreach (char c in nextWord)
			{
				num += Game1.gMan.TextSize(font, c.ToString()).X;
			}
			return num;
		}

		private void addNewChar(char nextChar)
		{
			string text = displayedLines.Last();
			text += nextChar;
			displayedLines[displayedLines.Count - 1] = text;
			int x = Game1.gMan.TextSize(font, nextChar.ToString()).X;
			currentLinePxLength += x;
			Game1.gMan.BeginDrawToTempTexture(textTexture, clearTexture: false);
			Game1.gMan.TextBlit(font, currentDrawPos + textPadding.XY * 2, nextChar.ToString(), currentDrawColor, GraphicsManager.BlendMode.Normal, 1);
			Game1.gMan.EndDrawToTempTexture();
			currentDrawPos.X += x;
		}

		private void addNewLine()
		{
			displayedLines.Add(string.Empty);
			currentLinePxLength = 0;
			currentDrawPos.X = 0;
			currentDrawPos.Y += 24;
		}

		public int TextAreaWidth()
		{
			return containingBox.W - textPadding.X - textPadding.W - ((!string.IsNullOrEmpty(portraitName)) ? 48 : 0);
		}

		public void FeedText(string text, string playerName)
		{
			text = text.Replace("\\\\", "\\");
			text = text.Replace("\\n", "\n");
			text = ReplaceVariableMarkup(oneshotWindow, text);
			text = text.Replace("\\p", playerName);
			if (string.IsNullOrEmpty(inputText))
			{
				bool flag = false;
				if (text.IndexOf('\n') == 0)
				{
					text = text.Substring(1);
					flag = true;
				}
				if (text.IndexOf('@') == 0)
				{
					if (text.Contains(' '))
					{
						SetPortrait(text.Substring(1, text.IndexOf(' ') - 1));
						text = text.Substring(text.IndexOf(' ') + 1);
					}
					else
					{
						SetPortrait(text.Substring(1));
						text = string.Empty;
					}
				}
				if (flag)
				{
					text = "\n" + text;
				}
				if (portraitName == "af")
				{
					scrollSoundName = "cat_2";
				}
				else if (text.StartsWith("["))
				{
					scrollSoundName = "text_robot";
				}
				else
				{
					scrollSoundName = "text";
				}
				inputText = text;
				scrollSoundTimer = 4;
			}
			else
			{
				inputText += text;
			}
		}

		private bool IsHandlingChoice()
		{
			if (choices != null)
			{
				return choices.Count > 0;
			}
			return false;
		}

		private bool IsInputtingNumber()
		{
			return inputNumberVar >= 0;
		}

		public static void DrawWindowBorder(Rect windowBox, GraphicsManager.BlendMode blendMode, float alpha)
		{
			int w = Math.Min(8, windowBox.W / 2);
			int num = Math.Min(8, (windowBox.W + 1) / 2);
			int h = Math.Min(8, windowBox.H / 2);
			int num2 = Math.Min(8, (windowBox.H + 1) / 2);
			Game1.gMan.MainBlit("ui/window_frame", new Vec2(windowBox.X, windowBox.Y), new Rect(0, 0, w, h), alpha, 0, blendMode);
			Game1.gMan.MainBlit("ui/window_frame", new Vec2(windowBox.X, windowBox.Y + windowBox.H - num2), new Rect(0, 24 - num2, w, num2), alpha, 0, blendMode);
			Game1.gMan.MainBlit("ui/window_frame", new Vec2(windowBox.X + windowBox.W - num, windowBox.Y), new Rect(24 - num, 0, num, h), alpha, 0, blendMode);
			Game1.gMan.MainBlit("ui/window_frame", new Vec2(windowBox.X + windowBox.W - num, windowBox.Y + windowBox.H - num2), new Rect(24 - num, 24 - num2, num, num2), alpha, 0, blendMode);
			for (int i = windowBox.X + 8; i < windowBox.X + windowBox.W - 8; i += 8)
			{
				int w2 = 8;
				if (windowBox.X + windowBox.W - 8 - i < 8)
				{
					w2 = windowBox.X + windowBox.W - 8 - i;
				}
				Game1.gMan.MainBlit("ui/window_frame", new Vec2(i, windowBox.Y), new Rect(8, 0, w2, 8), alpha, 0, blendMode);
				Game1.gMan.MainBlit("ui/window_frame", new Vec2(i, windowBox.Y + windowBox.H - 8), new Rect(8, 16, w2, 8), alpha, 0, blendMode);
			}
			for (int j = windowBox.Y + 8; j < windowBox.Y + windowBox.H - 8; j += 8)
			{
				int h2 = 8;
				if (windowBox.Y + windowBox.H - 8 - j < 8)
				{
					h2 = windowBox.Y + windowBox.H - 8 - j;
				}
				Game1.gMan.MainBlit("ui/window_frame", new Vec2(windowBox.X, j), new Rect(0, 8, 8, h2), alpha, 0, blendMode);
				Game1.gMan.MainBlit("ui/window_frame", new Vec2(windowBox.X + windowBox.W - 8, j), new Rect(16, 8, 8, h2), alpha, 0, blendMode);
			}
		}

		public void DrawText(GameColor currentColor)
		{
			Game1.gMan.MainBlit(textTexture, containingBox.XY * 2, currentColor, 0, GraphicsManager.BlendMode.Normal, 1);
		}

		public void Draw()
		{
			if (state == MessageBoxState.Closed)
			{
				return;
			}
			if (CurrentStyle == TextBoxStyle.GoldBorder)
			{
				Game1.gMan.ColorBoxBlit(new Rect(containingBox.X + 4, containingBox.Y + 4, containingBox.W - 8, containingBox.H - 8), new GameColor(boxColor.r, boxColor.g, boxColor.b, (byte)((float)(int)boxColor.a * textboxAlpha)));
				DrawWindowBorder(containingBox, GraphicsManager.BlendMode.Normal, textboxAlpha);
			}
			if (!string.IsNullOrEmpty(portraitName))
			{
				Vec2 pixelPos = new Vec2(containingBox.X + containingBox.W - 48 - 8, containingBox.Y + 8);
				int scale = 2;
				if (portraitName == "af")
				{
					pixelPos *= 2;
					scale = 1;
				}
				string textureName = "facepics/" + portraitName;
				Game1.gMan.MainBlit(textureName, pixelPos, textboxAlpha, 0, GraphicsManager.BlendMode.Normal, scale);
			}
			Vec2 vec = new Vec2(containingBox.X + textPadding.X, containingBox.Y + textPadding.Y);
			if (IsInputtingNumber() && !IsRollingText())
			{
				string textureName2 = "ui/number_cursor";
				int num = 5;
				if (Game1.languageMan.GetCurrentLangCode() == "ja")
				{
					textureName2 = "ui/name_select_letter_cursor";
					num = 0;
				}
				int x = Game1.gMan.TextureSize(textureName2).X;
				Vec2 pixelPos2 = vec;
				pixelPos2.X *= 2;
				pixelPos2.Y *= 2;
				pixelPos2.Y += displayedLines.Count * 12 * 2 - 1;
				for (int i = 0; i < inputDigitCursorPos; i++)
				{
					pixelPos2.X += x + num;
				}
				Game1.gMan.MainBlit(textureName2, pixelPos2, cursorAlpha * textboxAlpha, 0, GraphicsManager.BlendMode.Normal, 1);
				Vec2 pixelPos3 = vec;
				pixelPos3.X *= 2;
				pixelPos3.Y *= 2;
				pixelPos3.X += x / 2 + 1;
				pixelPos3.Y += displayedLines.Count * 12 * 2 + 4;
				GameColor gColor = textColor;
				for (int j = 0; j < inputNumberDigits.Length; j++)
				{
					if (oneshotWindow.flagMan.IsFlagSet(179))
					{
						switch (j)
						{
						case 0:
							gColor = GetTextColor(4);
							break;
						case 1:
							gColor = GetTextColor(2);
							break;
						case 2:
							gColor = GetTextColor(1);
							break;
						case 3:
							gColor = GetTextColor(3);
							break;
						}
					}
					Game1.gMan.TextBlitCentered(font, pixelPos3, inputNumberDigits[j].ToString(), gColor, GraphicsManager.BlendMode.Normal, 1);
					pixelPos3.X += x + num;
				}
			}
			if (IsHandlingChoice() && !IsRollingText())
			{
				Vec2 pixelPos4 = vec;
				pixelPos4.X *= 2;
				pixelPos4.Y *= 2;
				pixelPos4.X -= 4;
				pixelPos4.Y += (choiceBeginsOnLine + choiceCursorIndex) * 12 * 2;
				string textureName3 = "ui/choice_cursor";
				if (!string.IsNullOrEmpty(portraitName))
				{
					textureName3 = "ui/choice_cursor_w_facepic";
				}
				Game1.gMan.MainBlit(textureName3, pixelPos4, cursorAlpha * textboxAlpha, 0, GraphicsManager.BlendMode.Normal, 1);
			}
			DrawText(new GameColor(textColor.r, textColor.g, textColor.b, (byte)((float)(int)textColor.a * textboxAlpha)));
			if (!IsRollingText() && !IsHandlingChoice() && !IsInputtingNumber() && (string.IsNullOrEmpty(inputText) || forcedNod) && !IsReadyForMoreInput())
			{
				Game1.gMan.MainBlit("ui/item_box_arrows", new Vec2((containingBox.X + containingBox.W / 2) * 2 - 8, (containingBox.Y + containingBox.H) * 2 - 13 + nodOffsets[nodOffsetIndex]), new Rect(0, 8, 16, 8), textboxAlpha, 0, GraphicsManager.BlendMode.Normal, 1);
			}
		}

		public static GameColor GetTextColor(int colorCode)
		{
			switch (colorCode)
			{
			case 0:
				return new GameColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			case 1:
				return new GameColor(byte.MaxValue, 64, 64, byte.MaxValue);
			case 2:
				return new GameColor(0, 224, 0, byte.MaxValue);
			case 3:
				return new GameColor(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue);
			case 4:
				return new GameColor(64, 64, byte.MaxValue, byte.MaxValue);
			case 5:
				return new GameColor(byte.MaxValue, 64, byte.MaxValue, byte.MaxValue);
			case 6:
				return new GameColor(64, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			case 7:
				return new GameColor(128, 128, 128, byte.MaxValue);
			default:
				return GameColor.White;
			}
		}

		public static string ReplaceVariableMarkup(OneshotWindow osWindow, string text)
		{
			Match match = Regex.Match(text, "\\\\v\\[(\\d+)\\]");
			while (match.Success)
			{
				int num = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
				int variable = osWindow.varMan.GetVariable(num);
				text = text.Replace($"\\v[{num}]", variable.ToString());
				match = Regex.Match(text, "\\\\v\\[(\\d+)\\]");
			}
			return text;
		}

		public void InputNumberSetup(int var, int digits)
		{
			inputNumberVar = var;
			inputNumberDigits = new int[digits];
			int variable = oneshotWindow.varMan.GetVariable(var);
			for (int i = 0; i < inputNumberDigits.Length; i++)
			{
				inputNumberDigits[i] = variable / (int)Math.Pow(10.0, digits - i - 1) % 10;
			}
		}
	}
}

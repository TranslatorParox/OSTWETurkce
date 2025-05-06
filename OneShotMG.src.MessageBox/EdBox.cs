using System;
using System.Collections.Generic;
using System.Globalization;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.MessageBox
{
	public class EdBox : IMessageBox
	{
		private readonly OneshotWindow oneshotWindow;

		private MessageBoxState state;

		private int transitionTimer;

		private int totalTransitionTime;

		private const int OPEN_TIME = 13;

		private const int CLOSE_TIME = 13;

		private float alpha;

		private List<string> displayedLines;

		private List<int> displayedLinesWidth;

		private TempTexture textTexture;

		private GraphicsManager.FontType font = GraphicsManager.FontType.Game;

		public EdBox(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			Open();
		}

		public void ClearText()
		{
			displayedLines.Clear();
		}

		public void Draw()
		{
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(128f * alpha)));
			if (textTexture != null && textTexture.isValid)
			{
				Vec2 pixelPos = new Vec2(320 - textTexture.renderTarget.Width / 2, 240 - textTexture.renderTarget.Height / 2);
				GameColor white = GameColor.White;
				white.a = (byte)(255f * alpha);
				Game1.gMan.MainBlit(textTexture, pixelPos, white, 0, GraphicsManager.BlendMode.Normal, 1);
			}
		}

		public void FeedText(string text, string playerName)
		{
			text = text.Replace("\\n", "\n");
			text = text.Replace("\\p", playerName);
			text = TextBox.ReplaceVariableMarkup(oneshotWindow, text);
			displayedLines = MathHelper.WordWrap(font, text, 608);
			displayedLinesWidth = new List<int>();
			foreach (string displayedLine in displayedLines)
			{
				string text2 = displayedLine;
				for (int i = 0; i < 8; i++)
				{
					text2 = text2.Replace($"\\c[{i}]", "");
				}
				displayedLinesWidth.Add(Game1.gMan.TextSize(font, text2).X);
			}
			DrawTextTexture();
		}

		private void DrawTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				int num = 0;
				foreach (int item in displayedLinesWidth)
				{
					if (item > num)
					{
						num = item;
					}
				}
				Vec2 size = new Vec2(num + 4, 24 * displayedLines.Count);
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(size);
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			Vec2 pixelPos = new Vec2(0, 0);
			GameColor gColor = GameColor.White;
			for (int i = 0; i < displayedLines.Count; i++)
			{
				pixelPos.X = (textTexture.renderTarget.Width - displayedLinesWidth[i]) / 2;
				string text = displayedLines[i];
				while (!string.IsNullOrEmpty(text))
				{
					int num2 = text.IndexOf("\\c[");
					if (num2 >= 0)
					{
						string text2 = text.Substring(0, num2);
						Game1.gMan.TextBlit(font, pixelPos, text2, gColor, GraphicsManager.BlendMode.Normal, 1);
						pixelPos.X += Game1.gMan.TextSize(font, text2).X;
						text = text.Substring(num2 + "\\c[".Length);
						gColor = TextBox.GetTextColor(int.Parse(text.Substring(0, 1), CultureInfo.InvariantCulture));
						text = text.Substring("x]".Length);
					}
					else
					{
						Game1.gMan.TextBlit(font, pixelPos, text, gColor, GraphicsManager.BlendMode.Normal, 1);
						text = string.Empty;
					}
				}
				pixelPos.Y += 24;
			}
			Game1.gMan.EndDrawToTempTexture();
		}

		public bool IsFinished()
		{
			return state == MessageBoxState.Closed;
		}

		public bool IsReadyForMoreInput()
		{
			return false;
		}

		public void Open()
		{
			state = MessageBoxState.Opening;
			totalTransitionTime = 13;
			transitionTimer = 0;
		}

		public void Close()
		{
			state = MessageBoxState.Closing;
			totalTransitionTime = 13;
			transitionTimer = 0;
		}

		public void Update()
		{
			if (state != MessageBoxState.Closed)
			{
				if (textTexture == null || !textTexture.isValid)
				{
					DrawTextTexture();
				}
				textTexture.KeepAlive();
			}
			switch (state)
			{
			case MessageBoxState.Closing:
				transitionTimer++;
				if (transitionTimer < totalTransitionTime)
				{
					alpha = 1f - (float)transitionTimer / (float)totalTransitionTime;
					break;
				}
				state = MessageBoxState.Closed;
				transitionTimer = 0;
				alpha = 0f;
				break;
			case MessageBoxState.Opening:
				transitionTimer++;
				if (transitionTimer < totalTransitionTime)
				{
					alpha = (float)transitionTimer / (float)totalTransitionTime;
					break;
				}
				state = MessageBoxState.Opened;
				transitionTimer = 0;
				alpha = 1f;
				break;
			case MessageBoxState.Opened:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK) || Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel) || Game1.inputMan.IsAutoMashing())
				{
					Close();
				}
				break;
			}
		}

		public void InputNumberSetup(int inputNumberVar, int inputNumberDigits)
		{
			throw new NotImplementedException();
		}
	}
}

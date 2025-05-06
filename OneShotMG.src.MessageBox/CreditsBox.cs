using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.MessageBox
{
	public class CreditsBox : IMessageBox
	{
		private class CreditLine
		{
			public string center;

			public string left;

			public string right;

			public bool isHeader;
		}

		private readonly OneshotWindow oneshotWindow;

		private MessageBoxState state;

		private int transitionTimer;

		private int totalTransitionTime;

		private const int OPEN_TIME = 51;

		private const int CLOSE_TIME = 51;

		private const int STAY_OPEN_TIME = 150;

		private const int STAY_OPEN_TIME_SMALL = 250;

		private const int LINE_HEIGHT = 24;

		private const int LINE_HEIGHT_SMALL = 18;

		private float alpha;

		private List<CreditLine> creditLines;

		private const int BLACK_CREDITS_FLAG = 104;

		private const GraphicsManager.FontType HeaderFont = GraphicsManager.FontType.GameSmall;

		private const GraphicsManager.FontType NormalFont = GraphicsManager.FontType.Game;

		private bool isSmall;

		private TempTexture textTexture;

		public CreditsBox(OneshotWindow osWindow, bool small = false)
		{
			oneshotWindow = osWindow;
			isSmall = small;
			Open();
		}

		public void ClearText()
		{
			creditLines.Clear();
		}

		public void Draw()
		{
			GameColor gColor = GameColor.White;
			GameColor gameColor = GameColor.Black;
			if (oneshotWindow.flagMan.IsFlagSet(104))
			{
				gColor = GameColor.Black;
				gameColor = GameColor.White;
			}
			gColor.a = (byte)(255f * alpha);
			gameColor.a = (byte)(255f * alpha);
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), gColor);
			Game1.gMan.MainBlit(textTexture, Vec2.Zero, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
		}

		public void FeedText(string text, string playerName)
		{
			creditLines = new List<CreditLine>();
			text = text.Replace("\\n", "\n");
			string[] array = text.Split('\n');
			bool isHeader = false;
			string[] array2 = array;
			foreach (string obj in array2)
			{
				string text2 = string.Empty;
				string text3 = string.Empty;
				string text4 = string.Empty;
				string text5 = obj;
				if (text5.StartsWith("\\h"))
				{
					text5 = text5.Substring(2);
					isHeader = true;
				}
				if (text5.StartsWith("\\l"))
				{
					if (text5.Contains("\\r"))
					{
						text3 = text5.Substring("\\l".Length, text5.IndexOf("\\r") - 2);
						text4 = text5.Substring(text5.IndexOf("\\r") + 2);
					}
					else
					{
						text3 = text5.Substring(2);
					}
				}
				else
				{
					text2 = text5;
				}
				if (string.IsNullOrEmpty(text3) && string.IsNullOrEmpty(text4) && string.IsNullOrEmpty(text2))
				{
					isHeader = true;
				}
				creditLines.Add(new CreditLine
				{
					center = text2,
					left = text3,
					right = text4,
					isHeader = isHeader
				});
				isHeader = false;
			}
			DrawTextTexture();
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
			totalTransitionTime = 51;
			transitionTimer = 0;
			CreateTextTexture();
		}

		public void Close()
		{
			state = MessageBoxState.Closing;
			totalTransitionTime = 51;
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
				totalTransitionTime = (isSmall ? 250 : 150);
				alpha = 1f;
				break;
			case MessageBoxState.Opened:
				transitionTimer++;
				if (transitionTimer > totalTransitionTime)
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

		private void CreateTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(640, 480));
			}
		}

		private void DrawTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				CreateTextTexture();
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			int scale = (isSmall ? 1 : 2);
			int num = ((!isSmall) ? 1 : 2);
			int num2 = 0;
			foreach (CreditLine creditLine in creditLines)
			{
				num2 += (creditLine.isHeader ? 18 : 24);
			}
			int num3 = 240 * num / 2 - num2 / 2;
			foreach (CreditLine creditLine2 in creditLines)
			{
				GraphicsManager.FontType fontType = ((!creditLine2.isHeader) ? GraphicsManager.FontType.Game : GraphicsManager.FontType.GameSmall);
				if (!string.IsNullOrEmpty(creditLine2.left))
				{
					Game1.gMan.TextBlit(fontType, new Vec2(20 * num, num3), creditLine2.left, GameColor.White, GraphicsManager.BlendMode.Normal, scale);
				}
				if (!string.IsNullOrEmpty(creditLine2.center))
				{
					Game1.gMan.TextBlitCentered(fontType, new Vec2(320 * num / 2, num3), creditLine2.center, GameColor.White, GraphicsManager.BlendMode.Normal, scale);
				}
				if (!string.IsNullOrEmpty(creditLine2.right))
				{
					Vec2 vec = Game1.gMan.TextSize(fontType, creditLine2.right);
					Game1.gMan.TextBlit(fontType, new Vec2(300 * num - vec.X, num3), creditLine2.right, GameColor.White, GraphicsManager.BlendMode.Normal, scale);
				}
				num3 += (creditLine2.isHeader ? 18 : 24);
			}
			Game1.gMan.EndDrawToTempTexture();
		}
	}
}

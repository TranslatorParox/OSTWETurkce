using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.MessageBox
{
	public class DocBox : IMessageBox
	{
		private readonly OneshotWindow oneshotWindow;

		private MessageBoxState state;

		private int transitionTimer;

		private int totalTransitionTime;

		private const int OPEN_TIME = 10;

		private const int CLOSE_TIME = 10;

		private float alpha;

		private const int RED_DOCUMENT_FLAG_ID = 124;

		private List<string> displayedLines;

		private const GraphicsManager.FontType docFont = GraphicsManager.FontType.GameSmall;

		private int startLineIndex;

		private const int MAX_LINES_DISPLAYED = 11;

		private TempTexture textTexture;

		public DocBox(OneshotWindow osWindow)
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
			GameColor gameColor = new GameColor(100, 92, byte.MaxValue, (byte)(180f * alpha));
			if (oneshotWindow.flagMan.IsFlagSet(124))
			{
				gameColor = new GameColor(byte.MaxValue, 92, 100, (byte)(180f * alpha));
				Game1.gMan.MainBlit("pictures/lined_paper_red", new Vec2(60, 12), alpha * 0.86f);
				if (startLineIndex > 0)
				{
					Game1.gMan.MainBlit("pictures/scroll_up_red", new Vec2(262, 12), alpha);
				}
				if (startLineIndex + 11 < displayedLines.Count)
				{
					Game1.gMan.MainBlit("pictures/scroll_down_red", new Vec2(262, 212), alpha);
				}
			}
			else
			{
				Game1.gMan.MainBlit("pictures/lined_paper_blue", new Vec2(60, 12), alpha * 0.86f);
				if (startLineIndex > 0)
				{
					Game1.gMan.MainBlit("pictures/scroll_up_blue", new Vec2(262, 12), alpha);
				}
				if (startLineIndex + 11 < displayedLines.Count)
				{
					Game1.gMan.MainBlit("pictures/scroll_down_blue", new Vec2(262, 212), alpha);
				}
			}
			Game1.gMan.MainBlit(textTexture, new Vec2(60, 12), gameColor);
		}

		public void FeedText(string text, string playerName)
		{
			text = text.Replace("\\n", "\n");
			text = text.Replace("\\p", playerName);
			displayedLines = MathHelper.WordWrap(GraphicsManager.FontType.GameSmall, text, 184);
			DrawTextTexture();
		}

		private void DrawTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				CreateTextTexture();
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			Vec2 pixelPos = new Vec2(8, 8);
			for (int i = startLineIndex; i < startLineIndex + 11 && i < displayedLines.Count; i++)
			{
				string text = displayedLines[i];
				Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, pixelPos, text, GameColor.White, GraphicsManager.BlendMode.Normal, 1);
				pixelPos.Y += 18;
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
			totalTransitionTime = 10;
			transitionTimer = 0;
			Game1.soundMan.PlaySound("page");
			CreateTextTexture();
		}

		private void CreateTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(200, 216));
			}
		}

		public void Close()
		{
			state = MessageBoxState.Closing;
			totalTransitionTime = 10;
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
				if (startLineIndex > 0 && Game1.inputMan.IsButtonPressed(InputManager.Button.Up))
				{
					startLineIndex--;
					DrawTextTexture();
				}
				else if (startLineIndex + 11 < displayedLines.Count && Game1.inputMan.IsButtonPressed(InputManager.Button.Down))
				{
					startLineIndex++;
					DrawTextTexture();
				}
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

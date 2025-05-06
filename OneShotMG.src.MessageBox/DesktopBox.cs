using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.MessageBox
{
	public class DesktopBox : IMessageBox
	{
		private GraphicsManager.FontType font = GraphicsManager.FontType.Game;

		private MessageBoxState state;

		private int transitionTimer;

		private int totalTransitionTime;

		private const int OPEN_TIME = 2;

		private const int CLOSE_TIME = 2;

		private float alpha;

		private List<string> displayedLines;

		private TempTexture textTexture;

		public DesktopBox()
		{
			Open();
		}

		public void ClearText()
		{
			displayedLines.Clear();
		}

		public void Draw()
		{
			Game1.gMan.MainBlit("pictures/cg_desktop_messagebox", Vec2.Zero, alpha);
			if (textTexture != null && textTexture.isValid)
			{
				GameColor black = GameColor.Black;
				black.a = (byte)(255f * alpha);
				Vec2 pixelPos = new Vec2(320 - textTexture.renderTarget.Width / 2, 240 - textTexture.renderTarget.Height / 2);
				Game1.gMan.MainBlit(textTexture, pixelPos, black, 0, GraphicsManager.BlendMode.Normal, 1);
			}
		}

		private void DrawTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				int num = 0;
				foreach (string displayedLine in displayedLines)
				{
					int x = Game1.gMan.TextSize(font, displayedLine).X;
					if (x > num)
					{
						num = x;
					}
				}
				Vec2 size = new Vec2(num + 4, 24 * displayedLines.Count);
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(size);
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			Vec2 pixelPos = new Vec2(textTexture.renderTarget.Width / 2, 0);
			for (int i = 0; i < displayedLines.Count; i++)
			{
				Game1.gMan.TextBlitCentered(font, pixelPos, displayedLines[i], GameColor.White, GraphicsManager.BlendMode.Normal, 1);
				pixelPos.Y += 24;
			}
			Game1.gMan.EndDrawToTempTexture();
		}

		public void FeedText(string text, string playerName)
		{
			text = text.Replace("\\n", "\n");
			text = text.Replace("\\p", playerName);
			displayedLines = MathHelper.WordWrap(font, text, 608);
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
			totalTransitionTime = 2;
			transitionTimer = 0;
			Game1.soundMan.PlaySound("pc_messagebox", 0.9f, 1.5f);
		}

		public void Close()
		{
			state = MessageBoxState.Closing;
			totalTransitionTime = 2;
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

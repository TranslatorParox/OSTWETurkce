using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.MessageBox
{
	public class TutorialBox : IMessageBox
	{
		private MessageBoxState state;

		private int transitionTimer;

		private int totalTransitionTime;

		private const int OPEN_TIME = 30;

		private const int CLOSE_TIME = 30;

		private const int PAGE_TEXT_WIDTH = 272;

		private const int RIGHT_ALIGNED_BUTTONS_OFFSET = 60;

		private float alpha;

		private bool isRightAligned;

		private List<string> displayedLines;

		private List<int> linePixelLengths;

		private const GraphicsManager.FontType FONT = GraphicsManager.FontType.GameSmall;

		private const GraphicsManager.FontType TITLE_FONT = GraphicsManager.FontType.Game;

		private OneshotWindow oneshotWindow;

		private TempTexture textTexture;

		private InputManager.GlyphMode textDrawnGlyphMode;

		private TempTexture titleTexture;

		public TutorialBox(OneshotWindow osWindow, bool rightAligned)
		{
			isRightAligned = rightAligned;
			oneshotWindow = osWindow;
			Open();
		}

		public void ClearText()
		{
			displayedLines.Clear();
			linePixelLengths.Clear();
		}

		public void Draw()
		{
			GameColor white = GameColor.White;
			white.a = (byte)(255f * alpha * alpha);
			GameColor gameColor = new GameColor(134, 65, 7, (byte)(255f * alpha * alpha));
			GameColor gameColor2 = new GameColor(251, 130, 0, (byte)(255f * alpha * alpha));
			Game1.gMan.MainBlit("pictures/instruction_bg", Vec2.Zero, alpha);
			Vec2 vec = new Vec2(160, 20);
			Vec2 vec2 = new Vec2(0, 0);
			Game1.gMan.MainBlit(textTexture, vec2 + new Vec2(1, 0), gameColor);
			Game1.gMan.MainBlit(textTexture, vec2 + new Vec2(-1, 0), gameColor);
			Game1.gMan.MainBlit(textTexture, vec2 + new Vec2(0, 1), gameColor);
			Game1.gMan.MainBlit(textTexture, vec2 + new Vec2(0, -1), gameColor);
			Game1.gMan.MainBlit(textTexture, vec2, white);
			for (int i = 0; i < displayedLines.Count; i++)
			{
				string text = displayedLines[i];
				if (i == 0)
				{
					Vec2 vec3 = vec;
					vec3.X -= linePixelLengths[i] / 2;
					vec3.X -= 2;
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(2, 0), gameColor2);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(-2, 0), gameColor2);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(0, 2), gameColor2);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(0, -2), gameColor2);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(1, 0), white);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(-1, 0), white);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(0, 1), white);
					Game1.gMan.MainBlit(titleTexture, vec3 + new Vec2(0, -1), white);
					Game1.gMan.MainBlit(titleTexture, vec3, gameColor);
				}
				else if (isRightAligned)
				{
					Vec2 pixelPos = vec;
					pixelPos.X += 60;
					pixelPos.X -= linePixelLengths[i];
					if (!text.Contains("@MOVEMOUSE") && text.Contains("@MOVE") && Game1.inputMan.CurrentGlyphMode == InputManager.GlyphMode.Keyboard)
					{
						pixelPos.X += 96;
					}
					Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, pixelPos, text, white, GraphicsManager.BlendMode.Normal, 2, GraphicsManager.TextBlitMode.OnlyGlyphes);
				}
				else
				{
					Vec2 pixelPos2 = vec;
					pixelPos2.X -= linePixelLengths[i] / 2;
					Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, pixelPos2, text, white, GraphicsManager.BlendMode.Normal, 2, GraphicsManager.TextBlitMode.OnlyGlyphes);
				}
				vec.Y += GetLineHeight();
			}
		}

		public void FeedText(string text, string playerName)
		{
			text = text.Replace("\\n", "\n");
			text = text.Replace("\\p", playerName);
			displayedLines = MathHelper.WordWrap(GraphicsManager.FontType.GameSmall, text, 272);
			linePixelLengths = new List<int>();
			DrawTextTexture();
		}

		private void MeasureLineLengths()
		{
			linePixelLengths.Clear();
			GraphicsManager.FontType fontType = GraphicsManager.FontType.Game;
			foreach (string displayedLine in displayedLines)
			{
				linePixelLengths.Add(Game1.gMan.TextSize(fontType, displayedLine, checkForGlyphes: true).X);
				fontType = GraphicsManager.FontType.GameSmall;
			}
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
			totalTransitionTime = 30;
			transitionTimer = 0;
		}

		public void Close()
		{
			state = MessageBoxState.Closing;
			totalTransitionTime = 30;
			transitionTimer = 0;
		}

		public void Update()
		{
			if (textTexture == null || !textTexture.isValid || (isRightAligned && textDrawnGlyphMode != Game1.inputMan.CurrentGlyphMode))
			{
				DrawTextTexture();
			}
			textTexture?.KeepAlive();
			titleTexture?.KeepAlive();
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

		private int GetLineHeight()
		{
			if (isRightAligned)
			{
				return 15;
			}
			switch (Game1.languageMan.GetCurrentLangCode())
			{
			default:
				return 13;
			case "ko":
			case "ja":
			case "zh_cn":
			case "zh_cht":
				return 17;
			}
		}

		private void DrawTextTexture()
		{
			MeasureLineLengths();
			if (textTexture == null || !textTexture.isValid)
			{
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(320, 240));
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			GameColor white = GameColor.White;
			Vec2 vec = new Vec2(160, 20);
			for (int i = 0; i < displayedLines.Count; i++)
			{
				string text = displayedLines[i];
				if (i != 0)
				{
					if (isRightAligned)
					{
						Vec2 pixelPos = vec;
						pixelPos.X += 60;
						pixelPos.X -= linePixelLengths[i];
						if (!text.Contains("@MOVEMOUSE") && text.Contains("@MOVE") && Game1.inputMan.CurrentGlyphMode == InputManager.GlyphMode.Keyboard)
						{
							pixelPos.X += 96;
						}
						Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, pixelPos, text, white, GraphicsManager.BlendMode.Normal, 1, GraphicsManager.TextBlitMode.OnlyText);
					}
					else
					{
						Vec2 pixelPos2 = vec;
						pixelPos2.X -= linePixelLengths[i] / 2;
						Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, pixelPos2, text, white, GraphicsManager.BlendMode.Normal, 1, GraphicsManager.TextBlitMode.OnlyText);
					}
				}
				vec.Y += GetLineHeight();
			}
			Game1.gMan.EndDrawToTempTexture();
			textDrawnGlyphMode = Game1.inputMan.CurrentGlyphMode;
			titleTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, displayedLines[0]);
		}
	}
}

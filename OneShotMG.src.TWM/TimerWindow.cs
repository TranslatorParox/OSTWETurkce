using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class TimerWindow : TWMWindow
	{
		private const int TEXT_OFFSET = 2;

		private const int TEXT_HEIGHT = 16;

		private const int WINDOW_WIDTH = 120;

		private const int WINDOW_HEIGHT = 86;

		private const int TIMER_WIDTH = 116;

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private int twoDigitWidth;

		private int threeDigitWidth;

		private int periodWidth;

		private int colonWidth;

		private TempTexture systemActiveTimeLabel;

		private TempTexture currentRunTimeLabel;

		private readonly IconButton helpButton;

		private const string ICON_HELP = "the_world_machine/window_icons/question";

		public TimerWindow()
		{
			base.WindowIcon = "timer";
			base.WindowTitle = "timer_app_name";
			base.ContentsSize = new Vec2(120, 86);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
			twoDigitWidth = Game1.gMan.TextSize(GraphicsManager.FontType.OS, "00").X;
			threeDigitWidth = Game1.gMan.TextSize(GraphicsManager.FontType.OS, "000").X;
			periodWidth = Game1.gMan.TextSize(GraphicsManager.FontType.OS, ".").X;
			colonWidth = Game1.gMan.TextSize(GraphicsManager.FontType.OS, ":").X;
			DrawCurrentRunTimeLabel();
			DrawSystemActiveTimeLabel();
			helpButton = new IconButton("the_world_machine/window_icons/question", new Vec2(4, 68), OnHelp);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gameColor = theme.Background(alpha);
			GameColor gameColor2 = theme.Primary(alpha);
			GameColor secondCol = theme.Variant(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gameColor);
			Vec2 vec = new Vec2(2, 4) + screenPos;
			Game1.gMan.MainBlit(systemActiveTimeLabel, vec * 2, gameColor2, 0, GraphicsManager.BlendMode.Normal, 1);
			bool flashColons = Game1.windowMan.SystemActiveTimer / 20 % 2 == 1;
			DrawTimerString(screenPos + new Vec2(2, 18), Game1.windowMan.SystemActiveTimer, gameColor, secondCol, gameColor2, flashColons, 116);
			vec.Y += 32;
			Game1.gMan.MainBlit(currentRunTimeLabel, vec * 2, gameColor2, 0, GraphicsManager.BlendMode.Normal, 1);
			DrawTimerString(screenPos + new Vec2(2, 50), Game1.windowMan.CurrentPlaythroughTimer, gameColor, secondCol, gameColor2, flashColons, 116, Game1.windowMan.IsCurrentPlaythroughTimerActive);
			helpButton.Draw(screenPos, theme, alpha);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is TimerWindow;
		}

		public override bool Update(bool mouseInputWasConsumed)
		{
			if (systemActiveTimeLabel == null || !systemActiveTimeLabel.isValid)
			{
				DrawSystemActiveTimeLabel();
			}
			if (currentRunTimeLabel == null || !currentRunTimeLabel.isValid)
			{
				DrawCurrentRunTimeLabel();
			}
			systemActiveTimeLabel.KeepAlive();
			currentRunTimeLabel.KeepAlive();
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			bool canInteract = !mouseInputWasConsumed && !IsModalWindowOpen();
			helpButton.Update(parentPos, canInteract);
			mouseInputWasConsumed |= base.Update(mouseInputWasConsumed);
			return mouseInputWasConsumed;
		}

		private void DrawSystemActiveTimeLabel()
		{
			systemActiveTimeLabel = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("timer_system_active_time"), 116);
		}

		private void DrawCurrentRunTimeLabel()
		{
			currentRunTimeLabel = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("timer_current_playthrough"), 116);
		}

		private void DrawTimerString(Vec2 drawPos, int timer, GameColor bgCol, GameColor secondCol, GameColor textCol, bool flashColons, int width, bool active = true)
		{
			Game1.gMan.ColorBoxBlit(new Rect(drawPos.X, drawPos.Y, width, 16), textCol);
			Game1.gMan.ColorBoxBlit(new Rect(drawPos.X + 1, drawPos.Y + 1, width - 2, 14), bgCol);
			drawPos.X += width - 2;
			drawPos.Y--;
			int num = timer / 60;
			int num2 = num / 60 / 60;
			int num3 = num / 60 % 60;
			int num4 = num % 60;
			int num5 = (int)((float)timer * 1000f / 60f) % 1000;
			if (num2 > 999)
			{
				num2 = 999;
				num3 = 59;
				num4 = 59;
				num5 = 999;
			}
			drawPos.X -= threeDigitWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, num5.ToString("D3"), active ? textCol : secondCol);
			drawPos.X -= periodWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, ".", (flashColons || !active) ? secondCol : textCol);
			drawPos.X -= twoDigitWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, num4.ToString("D2"), active ? textCol : secondCol);
			drawPos.X -= colonWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, ":", (flashColons || !active) ? secondCol : textCol);
			drawPos.X -= twoDigitWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, num3.ToString("D2"), active ? textCol : secondCol);
			drawPos.X -= colonWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, ":", (flashColons || !active) ? secondCol : textCol);
			drawPos.X -= threeDigitWidth;
			Game1.gMan.TextBlit(GraphicsManager.FontType.OS, drawPos, num2.ToString("D3"), active ? textCol : secondCol);
		}

		private void OnHelp()
		{
			ShowModalWindow(ModalWindow.ModalType.Info, Game1.languageMan.GetTWMLocString("timer_explanation_1"), delegate
			{
				ShowModalWindow(ModalWindow.ModalType.Info, Game1.languageMan.GetTWMLocString("timer_explanation_2"), delegate
				{
					ShowModalWindow(ModalWindow.ModalType.Info, Game1.languageMan.GetTWMLocString("timer_explanation_3"));
				});
			});
		}
	}
}

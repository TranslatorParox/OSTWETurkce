using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class ModalWindow : TWMWindow
	{
		public delegate void CreateModalHandler(ModalType type, string msg, ModalResponseCallback callback = null, bool playModalNoise = true, bool canAutomash = false);

		public delegate void ModalResponseCallback(ModalResponse response);

		public enum ModalType
		{
			Info,
			Error,
			YesNo,
			ButtonAssign,
			StickAssign,
			KeyAssign
		}

		public enum ModalResponse
		{
			None,
			OK,
			Yes,
			No,
			Cancel
		}

		protected List<string> displayedLines;

		protected const GraphicsManager.FontType MODAL_FONT = GraphicsManager.FontType.OS;

		protected const int MODAL_WIDTH = 160;

		protected const int TEXT_MARGIN = 4;

		protected const int TEXT_AREA_WIDTH = 152;

		protected const int TEXT_LINE_HEIGHT = 12;

		protected List<TextButton> modalButtons;

		public ModalResponseCallback callback;

		private readonly int[] notifNoiseOrder = new int[4] { 2, 3, 4, 2 };

		private static int notifNoiseIndex = 0;

		private static DateTime lastTimeModalWasOpened = DateTime.MinValue;

		private bool hasUpdatedOnce;

		private TempTexture textTexture;

		private bool automashable;

		public ModalType Type { get; protected set; }

		public ModalResponse Response { get; protected set; }

		public ModalWindow(ModalType type, string message, bool playModalNoise = true, bool canAutomash = false)
		{
			if (DateTime.Now - lastTimeModalWasOpened > TimeSpan.FromSeconds(30.0))
			{
				notifNoiseIndex = 0;
			}
			lastTimeModalWasOpened = DateTime.Now;
			Type = type;
			ProcessText(message);
			AddButtons();
			automashable = canAutomash;
			switch (Type)
			{
			case ModalType.Info:
				base.WindowIcon = "info";
				break;
			case ModalType.ButtonAssign:
				base.WindowIcon = "info";
				Game1.inputMan.StartButtonAssignMode();
				break;
			case ModalType.StickAssign:
				base.WindowIcon = "info";
				Game1.inputMan.StartStickAssignMode();
				break;
			case ModalType.KeyAssign:
				base.WindowIcon = "info";
				Game1.inputMan.StartKeyAssignMode();
				break;
			case ModalType.Error:
				base.WindowIcon = "error";
				break;
			case ModalType.YesNo:
				base.WindowIcon = "question";
				break;
			}
			if (playModalNoise)
			{
				int num = notifNoiseOrder[notifNoiseIndex];
				Game1.soundMan.PlaySound($"twm_notif{num}", 0.5f);
				notifNoiseIndex++;
				if (notifNoiseIndex >= notifNoiseOrder.Length)
				{
					notifNoiseIndex = 0;
				}
			}
			base.ContentsSize = new Vec2(160, displayedLines.Count * 12 + 8 + ((modalButtons.Count > 0) ? 20 : 0));
		}

		protected void onButtonClick(ModalResponse buttonResponse)
		{
			if (Game1.windowMan.TutorialStep != TutorialStep.DELETE_CONFIRM_DELETION || buttonResponse != ModalResponse.No)
			{
				Response = buttonResponse;
				callback?.Invoke(buttonResponse);
				onClose(this);
			}
		}

		protected virtual void AddButtons()
		{
			modalButtons = new List<TextButton>();
			switch (Type)
			{
			case ModalType.Info:
			case ModalType.Error:
			{
				Vec2 relativePos3 = new Vec2(52, displayedLines.Count * 12 + 8);
				modalButtons.Add(new TextButton(Game1.languageMan.GetTWMLocString("dialog_ok"), relativePos3, delegate
				{
					onButtonClick(ModalResponse.OK);
				}));
				break;
			}
			case ModalType.YesNo:
			{
				Vec2 relativePos = new Vec2(20, displayedLines.Count * 12 + 8);
				Vec2 relativePos2 = new Vec2(84, displayedLines.Count * 12 + 8);
				modalButtons.Add(new TextButton(Game1.languageMan.GetTWMLocString("dialog_yes"), relativePos, delegate
				{
					onButtonClick(ModalResponse.Yes);
				}));
				modalButtons.Add(new TextButton(Game1.languageMan.GetTWMLocString("dialog_no"), relativePos2, delegate
				{
					onButtonClick(ModalResponse.No);
				}));
				break;
			}
			case ModalType.ButtonAssign:
			case ModalType.StickAssign:
			case ModalType.KeyAssign:
				break;
			}
		}

		private void ProcessText(string text)
		{
			text = Game1.languageMan.GetTWMLocString(text);
			text = text.Replace("\\n", "\n");
			displayedLines = MathHelper.WordWrap(GraphicsManager.FontType.OS, text, 152);
			GenerateTempTexture();
		}

		private void GenerateTempTexture()
		{
			Vec2 size = new Vec2(160, displayedLines.Count * 12 + 8);
			size.X *= 2;
			size.Y *= 2;
			textTexture = Game1.gMan.TempTexMan.GetTempTexture(size);
			Vec2 pixelPos = new Vec2(80, 1);
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			foreach (string displayedLine in displayedLines)
			{
				Game1.gMan.TextBlitCentered(GraphicsManager.FontType.OS, pixelPos, displayedLine, GameColor.White);
				pixelPos.Y += 12;
			}
			Game1.gMan.EndDrawToTempTexture();
		}

		public override bool Update(bool mouseAlreadyOnOtherWindow)
		{
			if (textTexture == null || !textTexture.isValid)
			{
				GenerateTempTexture();
			}
			textTexture?.KeepAlive();
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			foreach (TextButton modalButton in modalButtons)
			{
				modalButton.Update(parentPos, !mouseAlreadyOnOtherWindow);
			}
			if (hasUpdatedOnce && Game1.inputMan.IsAutoMashing() && automashable && modalButtons.Count == 1)
			{
				modalButtons[0].action();
			}
			if (Type == ModalType.ButtonAssign && !Game1.inputMan.InButtonAssignMode)
			{
				onButtonClick(ModalResponse.OK);
			}
			if (Type == ModalType.StickAssign && !Game1.inputMan.InStickAssignMode)
			{
				onButtonClick(ModalResponse.OK);
			}
			if (Type == ModalType.KeyAssign && !Game1.inputMan.InKeyAssignMode)
			{
				onButtonClick(ModalResponse.OK);
			}
			hasUpdatedOnce = true;
			return base.Update(mouseAlreadyOnOtherWindow);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			Game1.gMan.ColorBoxBlit(new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y), gColor);
			Vec2 vec = screenPos;
			GameColor gameColor = theme.Primary(alpha);
			Game1.gMan.MainBlit(textTexture, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
			foreach (TextButton modalButton in modalButtons)
			{
				modalButton.Draw(screenPos, theme, alpha);
				if (Game1.windowMan.TutorialStep == TutorialStep.DELETE_CONFIRM_DELETION && modalButton == modalButtons[0])
				{
					byte b = alpha;
					float num = (float)Math.Abs(objectHighlightTimer - 40) / 40f;
					num *= 0.6f;
					b = (byte)((float)(int)b * num);
					modalButton.DrawHighlight(screenPos, theme, b);
				}
			}
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return false;
		}

		public Vec2 GetButtonsPos()
		{
			return Pos + new Vec2(80, modalButtons[0].GetPos().Y + 26);
		}
	}
}

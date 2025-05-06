namespace OneShotMG.src.TWM
{
	public class LanguageSelectModalWindow : ModalWindow
	{
		private ChooserControl languageChooser;

		public LanguageSelectModalWindow()
			: base(ModalType.Info, "select_language_modal_window")
		{
			Vec2 pos = new Vec2(4, displayedLines.Count * 12 + 8);
			languageChooser = new ChooserControl(pos, 152, Game1.languageMan.GetLanguageOptions(), Game1.languageMan.GetCurrentLangCode());
			languageChooser.DrawLanguagesAsOptions = true;
			base.ContentsSize = new Vec2(160, displayedLines.Count * 12 + 8 + 16 + 8 + 20);
			foreach (TextButton modalButton in modalButtons)
			{
				modalButton.ShiftPos(new Vec2(0, 24));
			}
		}

		public override bool Update(bool mouseAlreadyOnOtherWindow)
		{
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			languageChooser.Update(parentPos, !mouseAlreadyOnOtherWindow);
			return base.Update(mouseAlreadyOnOtherWindow);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			base.DrawContents(theme, screenPos, alpha);
			languageChooser.Draw(screenPos, theme, alpha);
		}

		public string GetSelectedLanguage()
		{
			return languageChooser.Value;
		}
	}
}

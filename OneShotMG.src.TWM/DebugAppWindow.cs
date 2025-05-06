using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class DebugAppWindow : TWMWindow
	{
		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private const int CONTENT_MARGIN = 4;

		private const int BUTTON_HEIGHT = 16;

		private TextButton unlockAllWallpapersButton;

		private TextButton unlockAllThemesButton;

		private TextButton unlockAllFriendsButton;

		private TextButton unlockAllCGsButton;

		private TextButton unlockAllSongsButton;

		private TextButton unlockAllBadgesButton;

		private const int BUTTONS_COUNT = 6;

		public const int CONTENT_WIDTH = 240;

		public DebugAppWindow()
		{
			base.ContentsSize = new Vec2(240, 124);
			base.WindowIcon = "customize";
			base.WindowTitle = "DEBUG CHEAT APP";
			int buttonWidth = 232;
			Vec2 relativePos = new Vec2(4, 4);
			unlockAllWallpapersButton = new TextButton("Unlock all wallpapers", relativePos, delegate
			{
				onUnlockAllWallpapersButtonClicked();
			}, buttonWidth);
			Vec2 relativePos2 = new Vec2(4, 20 + relativePos.Y);
			unlockAllThemesButton = new TextButton("Unlock all themes", relativePos2, delegate
			{
				onUnlockAllThemesButtonClicked();
			}, buttonWidth);
			Vec2 relativePos3 = new Vec2(4, 20 + relativePos2.Y);
			unlockAllFriendsButton = new TextButton("Unlock all friends", relativePos3, delegate
			{
				onUnlockAllFriendsButtonClicked();
			}, buttonWidth);
			Vec2 relativePos4 = new Vec2(4, 20 + relativePos3.Y);
			unlockAllCGsButton = new TextButton("Unlock all CGs", relativePos4, delegate
			{
				onUnlockAllCGsButtonClicked();
			}, buttonWidth);
			Vec2 relativePos5 = new Vec2(4, 20 + relativePos4.Y);
			unlockAllSongsButton = new TextButton("Unlock all Songs", relativePos5, delegate
			{
				onUnlockAllSongsButtonClicked();
			}, buttonWidth);
			Vec2 relativePos6 = new Vec2(4, 20 + relativePos5.Y);
			unlockAllBadgesButton = new TextButton("Unlock all Badges", relativePos6, delegate
			{
				onUnlockAllBadgesButtonClicked();
			}, buttonWidth);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
		}

		public void onUnlockAllWallpapersButtonClicked()
		{
			Game1.windowMan.UnlockMan.UnlockAllWallpapers();
		}

		public void onUnlockAllThemesButtonClicked()
		{
			Game1.windowMan.UnlockMan.UnlockAllThemes();
		}

		public void onUnlockAllFriendsButtonClicked()
		{
			Game1.windowMan.UnlockMan.UnlockAllProfiles();
		}

		public void onUnlockAllCGsButtonClicked()
		{
			Game1.windowMan.UnlockMan.UnlockAllCgs();
		}

		public void onUnlockAllSongsButtonClicked()
		{
			Game1.windowMan.UnlockMan.UnlockAllTracks();
		}

		public void onUnlockAllBadgesButtonClicked()
		{
			Game1.windowMan.UnlockMan.UnlockAllAchievements();
		}

		public override bool Update(bool cursorOccluded)
		{
			if (!IsModalWindowOpen())
			{
				bool canInteract = !cursorOccluded && !base.IsMinimized;
				Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
				unlockAllWallpapersButton.Update(parentPos, canInteract);
				unlockAllThemesButton.Update(parentPos, canInteract);
				unlockAllFriendsButton.Update(parentPos, canInteract);
				unlockAllCGsButton.Update(parentPos, canInteract);
				unlockAllSongsButton.Update(parentPos, canInteract);
				unlockAllBadgesButton.Update(parentPos, canInteract);
			}
			return base.Update(cursorOccluded);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gColor);
			unlockAllWallpapersButton.Draw(screenPos, theme, alpha);
			unlockAllThemesButton.Draw(screenPos, theme, alpha);
			unlockAllFriendsButton.Draw(screenPos, theme, alpha);
			unlockAllCGsButton.Draw(screenPos, theme, alpha);
			unlockAllSongsButton.Draw(screenPos, theme, alpha);
			unlockAllBadgesButton.Draw(screenPos, theme, alpha);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is DebugAppWindow;
		}
	}
}

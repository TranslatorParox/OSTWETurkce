using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class FlashManager
	{
		private GameColor flashColor = GameColor.White;

		private int flashDuration = 1;

		private int flashTimeRemaining;

		public void Update()
		{
			if (flashTimeRemaining > 0)
			{
				flashTimeRemaining--;
			}
		}

		public void Draw()
		{
			if (flashTimeRemaining > 0)
			{
				GameColor gColor = flashColor;
				gColor.a = (byte)(gColor.a * flashTimeRemaining / flashDuration);
				Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), gColor);
			}
		}

		public void StartFlash(GameColor color, int time)
		{
			flashDuration = time;
			flashTimeRemaining = time;
			flashColor = color;
		}
	}
}

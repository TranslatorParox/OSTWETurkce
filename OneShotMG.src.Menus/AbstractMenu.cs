using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public abstract class AbstractMenu
	{
		private const int BACKGROUND_FINAL_ALPHA = 180;

		private int backgroundAlpha;

		protected const int DEFAULT_FADE_TIME = 10;

		protected int fadeTimer;

		protected MenuState state;

		protected readonly Rect fullscreenRect = new Rect(0, 0, 320, 240);

		public abstract void Draw();

		public abstract void Update();

		public abstract void Open();

		public abstract void Close();

		public bool IsOpen()
		{
			return state != MenuState.Closed;
		}

		protected void FadeIn(int transitionCurrent, int transitionEnd)
		{
			Fade(transitionCurrent, transitionEnd, fadeIn: true);
		}

		protected void FadeOut(int transitionCurrent, int transitionEnd)
		{
			Fade(transitionCurrent, transitionEnd, fadeIn: false);
		}

		private void Fade(int transitionCurrent, int transitionEnd, bool fadeIn)
		{
			int target;
			int start;
			if (fadeIn)
			{
				target = 180;
				start = 0;
			}
			else
			{
				target = 0;
				start = 180;
			}
			if (transitionCurrent >= transitionEnd)
			{
				backgroundAlpha = target;
			}
			else
			{
				backgroundAlpha = MathHelper.EaseIn(start, target, transitionCurrent, transitionEnd);
			}
		}

		protected GameColor getBackgroundColor()
		{
			return new GameColor(0, 0, 0, (byte)backgroundAlpha);
		}
	}
}

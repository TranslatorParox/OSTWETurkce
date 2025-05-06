using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;

namespace OneShotMG.src
{
	public class FilmPuzzleManager
	{
		private const int FILM_WIDTH = 320;

		private const int FILM_HEIGHT = 240;

		private bool active;

		private int leftBound;

		private int upperBound;

		private int rightBound = 320;

		private int downBound = 240;

		private OneshotWindow oneshotWindow;

		public FilmPuzzleManager(OneshotWindow oneshotWindow)
		{
			this.oneshotWindow = oneshotWindow;
		}

		public void Update()
		{
			if (active)
			{
				Vec2 pos = oneshotWindow.Pos;
				pos.X += 2;
				pos.Y += 26;
				if (-pos.X > leftBound)
				{
					leftBound = -pos.X;
				}
				if (-pos.Y > upperBound)
				{
					upperBound = -pos.Y;
				}
				int num = Game1.windowMan.ScreenSize.X - pos.X;
				if (num < rightBound)
				{
					rightBound = num;
				}
				int num2 = Game1.windowMan.ScreenSize.Y - 30 - pos.Y;
				if (num2 < downBound)
				{
					downBound = num2;
				}
			}
		}

		public bool HasPuzzleBeenCleared()
		{
			Rect rect = new Rect(leftBound, upperBound, rightBound - leftBound, downBound - upperBound);
			if (rect.W > 0)
			{
				return rect.H <= 0;
			}
			return true;
		}

		public void PuzzleBegin()
		{
			leftBound = 0;
			upperBound = 0;
			rightBound = 320;
			downBound = 240;
			active = true;
		}

		public void PuzzleEnd()
		{
			active = false;
		}

		public void Draw()
		{
			if (active)
			{
				Vec2 pixelPos = new Vec2(leftBound * 2, upperBound * 2);
				Rect srcRect = new Rect(leftBound * 2, upperBound * 2, (rightBound - leftBound) * 2, (downBound - upperBound) * 2);
				if (srcRect.W > 0 && srcRect.H > 0)
				{
					Game1.gMan.MainBlit("pictures/numbersheet", pixelPos, srcRect, 1f, 0, GraphicsManager.BlendMode.Normal, 1);
				}
			}
		}
	}
}

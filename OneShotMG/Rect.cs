namespace OneShotMG
{
	public struct Rect
	{
		public int X;

		public int Y;

		public int W;

		public int H;

		public static readonly Rect Zero = new Rect(0, 0, 0, 0);

		public Vec2 XY => new Vec2(X, Y);

		public int X2 => X + W;

		public int Y2 => Y + H;

		public Rect(int x, int y, int w, int h)
		{
			X = x;
			Y = y;
			W = w;
			H = h;
		}

		public bool IsVec2InRect(Vec2 v)
		{
			if (v.X >= X && v.X <= X + W && v.Y >= Y)
			{
				return v.Y <= Y + H;
			}
			return false;
		}

		public Rect Translated(Vec2 b)
		{
			return new Rect(X + b.X, Y + b.Y, W, H);
		}

		public Rect Shrink(int borderSize)
		{
			return new Rect(X + borderSize, Y + borderSize, W - 2 * borderSize, H - 2 * borderSize);
		}

		public override bool Equals(object obj)
		{
			if (obj is Rect rect)
			{
				return Equals(rect);
			}
			return false;
		}

		public bool Equals(Rect rect)
		{
			if (X == rect.X && Y == rect.Y && W == rect.W)
			{
				return H == rect.H;
			}
			return false;
		}
	}
}

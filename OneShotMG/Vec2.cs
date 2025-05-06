namespace OneShotMG
{
	public struct Vec2
	{
		public int X;

		public int Y;

		public static readonly Vec2 Zero = new Vec2(0, 0);

		public Vec2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vec2 vec)
			{
				return Equals(vec);
			}
			return false;
		}

		public bool Equals(Vec2 vec)
		{
			if (X == vec.X)
			{
				return Y == vec.Y;
			}
			return false;
		}

		public static Vec2 operator +(Vec2 a, Vec2 b)
		{
			return new Vec2(a.X + b.X, a.Y + b.Y);
		}

		public static Vec2 operator -(Vec2 a, Vec2 b)
		{
			return new Vec2(a.X - b.X, a.Y - b.Y);
		}

		public static Vec2 operator *(Vec2 a, Vec2 b)
		{
			return new Vec2(a.X * b.X, a.Y * b.Y);
		}

		public static Vec2 operator /(Vec2 a, int b)
		{
			return new Vec2(a.X / b, a.Y / b);
		}

		public static Vec2 operator *(Vec2 a, int b)
		{
			return new Vec2(a.X * b, a.Y * b);
		}
	}
}

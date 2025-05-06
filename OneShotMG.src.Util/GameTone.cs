namespace OneShotMG.src.Util
{
	public struct GameTone
	{
		public short r;

		public short g;

		public short b;

		public static readonly GameTone Zero = new GameTone(0, 0, 0);

		public static readonly GameTone White = new GameTone(255, 255, 255);

		public static readonly GameTone Black = new GameTone(-255, -255, -255);

		public GameTone(short r, short g, short b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		public static GameTone operator +(GameTone left, GameTone right)
		{
			return new GameTone((short)(left.r + right.r), (short)(left.g + right.g), (short)(left.b + right.b));
		}

		public static GameTone operator *(GameTone left, float right)
		{
			return new GameTone((short)((float)left.r * right), (short)((float)left.g * right), (short)((float)left.b * right));
		}

		public override bool Equals(object obj)
		{
			if (obj is GameTone gTone)
			{
				return Equals(gTone);
			}
			return false;
		}

		public bool Equals(GameTone gTone)
		{
			if (r == gTone.r && b == gTone.b)
			{
				return g == gTone.g;
			}
			return false;
		}
	}
}

namespace OneShotMG.src.Util
{
	public struct GameColor
	{
		public byte r;

		public byte g;

		public byte b;

		public byte a;

		public static readonly GameColor White = new GameColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static readonly GameColor Black = new GameColor(0, 0, 0, byte.MaxValue);

		public static readonly GameColor Zero = new GameColor(0, 0, 0, 0);

		public float rf => (float)(int)r / 255f;

		public float gf => (float)(int)g / 255f;

		public float bf => (float)(int)b / 255f;

		public float af => (float)(int)a / 255f;

		public GameColor(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}

		public override bool Equals(object obj)
		{
			if (obj is GameColor gc)
			{
				return Equals(gc);
			}
			return false;
		}

		public bool Equals(GameColor gc)
		{
			if (r == gc.r && g == gc.g && b == gc.b)
			{
				return a == gc.a;
			}
			return false;
		}
	}
}

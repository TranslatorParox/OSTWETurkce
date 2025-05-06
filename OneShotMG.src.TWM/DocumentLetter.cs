using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class DocumentLetter
	{
		private string originalLetter;

		private readonly GraphicsManager.FontType font;

		public int Width;

		private int changeTimer;

		private const int CHANGE_TIME_MIN = 15;

		private const int CHANGE_TIME_MAX = 60;

		public string DisplayLetter { get; private set; }

		public bool IsGlitched { get; private set; }

		public DocumentLetter(GraphicsManager.FontType font, string letter, bool glitched)
		{
			this.font = font;
			if (letter == " ")
			{
				glitched = false;
			}
			IsGlitched = glitched;
			originalLetter = letter;
			DisplayLetter = letter;
			Width = Game1.gMan.TextSize(font, letter).X;
			if (IsGlitched)
			{
				scrambleLetter();
			}
		}

		public void Draw(TWMTheme theme, Vec2 textDrawPos, byte alpha, int scale = 2)
		{
			GameColor textColor = (IsGlitched ? theme.Primary(alpha) : theme.Variant(alpha));
			Draw(textColor, textDrawPos, scale);
		}

		public void Draw(GameColor textColor, Vec2 textDrawPos, int scale = 2)
		{
			Game1.gMan.TextBlit(font, textDrawPos, DisplayLetter, textColor, GraphicsManager.BlendMode.Normal, scale);
		}

		private void scrambleLetter()
		{
			int num = MathHelper.Random(1, 100);
			changeTimer = MathHelper.Random(15, 60);
			if (num <= 75)
			{
				DisplayLetter = DocumentWindow.GetRandomLetter();
			}
			else
			{
				DisplayLetter = originalLetter;
			}
		}

		public void Update()
		{
			if (IsGlitched)
			{
				changeTimer--;
				if (changeTimer <= 0)
				{
					scrambleLetter();
				}
			}
		}
	}
}

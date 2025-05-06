using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class CreditsTextEntity : Entity
	{
		private string text;

		private GraphicsManager.FontType font = GraphicsManager.FontType.GameSmall;

		private int scale = 2;

		private TempTexture textTexture;

		private const int TEXTURE_X_MARGIN = 2;

		public CreditsTextEntity(OneshotWindow osWindow, Event e, GraphicsManager.FontType f = GraphicsManager.FontType.GameSmall, int drawScale = 2)
			: base(osWindow, e)
		{
			font = f;
			scale = drawScale;
			alwaysOnBottom = true;
			Event.Page[] pages = eventData.pages;
			for (int i = 0; i < pages.Length; i++)
			{
				pages[i].always_on_bottom = true;
			}
			CheckForActivePage();
			if (eventData.pages.Length != 0 && active)
			{
				EventCommand[] array = eventData.pages[currentPageIndex].list;
				if (array.Length != 0 && array[0].code == 101)
				{
					text = array[0].parameters[0];
				}
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				KillEntityAfterUpdate = true;
			}
			base.NeverHash = true;
		}

		public override void Update()
		{
			if (!KillEntityAfterUpdate)
			{
				if (textTexture == null || !textTexture.isValid)
				{
					DrawTextTexture();
				}
				textTexture?.KeepAlive();
				base.Update();
			}
		}

		public override void Draw(Vec2 camPos, GameTone tone)
		{
			if (KillEntityAfterUpdate || !active || string.IsNullOrEmpty(text))
			{
				return;
			}
			Vec2 zero = Vec2.Zero;
			zero.X = pos.X / 256 - camPos.X;
			zero.Y = pos.Y / 256 - camPos.Y;
			zero.Y -= 8;
			if (zero.Y < -16 || zero.Y > 256)
			{
				return;
			}
			if (scale == 1)
			{
				zero.X *= 2;
				zero.Y *= 2;
				zero.Y += 12;
				if (font == GraphicsManager.FontType.GameSmall)
				{
					zero.Y += 10;
				}
			}
			if (scale == 2)
			{
				zero.X *= 2;
				zero.Y *= 2;
			}
			Game1.gMan.MainBlit(textTexture, zero, new GameColor(81, 33, 129, byte.MaxValue), 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
		}

		private void DrawTextTexture()
		{
			string mapLocString = Game1.languageMan.GetMapLocString(0, text);
			if (textTexture == null || !textTexture.isValid)
			{
				int num = Game1.gMan.TextSize(font, mapLocString).X * scale;
				num += 4;
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(num, 32));
			}
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			Game1.gMan.TextBlit(font, new Vec2(2 / scale, 0), mapLocString, GameColor.White, GraphicsManager.BlendMode.Normal, scale);
			Game1.gMan.EndDrawToTempTexture();
		}
	}
}

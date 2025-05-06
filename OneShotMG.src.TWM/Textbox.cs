using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class Textbox
	{
		private const int TEXT_OFFSET = 5;

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private Rect contentArea;

		private SliderControl slider;

		private readonly int rowHeight;

		private readonly int margin;

		private readonly int visibleLines;

		private List<string> lines;

		private int lineOffset;

		private TempTexture linesTexture;

		public Textbox(Rect placement, int rowHeight, int margin)
		{
			contentArea = placement;
			this.rowHeight = rowHeight;
			this.margin = margin;
			visibleLines = (contentArea.H - 2 * margin) / rowHeight;
			slider = new SliderControl("", 0, 1, new Vec2(contentArea.W - 16, 0) + contentArea.XY, contentArea.H, useButtons: true, vertical: true);
			slider.ScrollTriggerZone = placement;
			slider.OnValueChanged = OnSliderChange;
			slider.Active = false;
			lines = new List<string>();
		}

		public void Draw(TWMTheme theme, Vec2 parentPos, byte alpha)
		{
			Rect rect = contentArea.Translated(parentPos);
			Game1.gMan.ColorBoxBlit(rect.Shrink(1), new GameColor(0, 0, 0, 179));
			Game1.gMan.MainBlit(linesTexture, rect.XY * 2, GameColor.White, 0, GraphicsManager.BlendMode.Normal, 1);
			slider.Draw(theme, parentPos, alpha);
		}

		public bool Update(Vec2 parentPos, bool canInteract)
		{
			if (linesTexture == null || !linesTexture.isValid)
			{
				RedrawLinesTexture();
			}
			linesTexture.KeepAlive();
			return slider.Update(parentPos, canInteract);
		}

		public void SetText(string text)
		{
			int num = contentArea.W - 2 * margin - 2;
			lines = MathHelper.WordWrap(GraphicsManager.FontType.OS, text, num);
			if (lines.Count > visibleLines)
			{
				num -= 16;
				lines = MathHelper.WordWrap(GraphicsManager.FontType.OS, text, num);
				slider.Max = lines.Count - visibleLines;
				slider.Active = true;
			}
			else
			{
				slider.Active = false;
			}
			lineOffset = 0;
			slider.Value = 0;
			RedrawLinesTexture();
		}

		private void OnSliderChange(int newVal)
		{
			lineOffset = newVal;
			RedrawLinesTexture();
		}

		private void GenerateLinesTexture()
		{
			linesTexture = Game1.gMan.TempTexMan.GetTempTexture(new Vec2(contentArea.W * 2, contentArea.H * 2));
		}

		private void RedrawLinesTexture()
		{
			if (linesTexture == null || !linesTexture.isValid)
			{
				GenerateLinesTexture();
			}
			Game1.gMan.BeginDrawToTempTexture(linesTexture);
			for (int i = lineOffset; i < lineOffset + visibleLines && i < lines.Count; i++)
			{
				string text = lines[i];
				Vec2 pixelPos = new Vec2(margin, margin + (i - lineOffset) * rowHeight - 5);
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, pixelPos, text, GameColor.White);
			}
			Game1.gMan.EndDrawToTempTexture();
		}
	}
}

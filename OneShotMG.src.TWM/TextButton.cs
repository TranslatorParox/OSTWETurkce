using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class TextButton
	{
		public delegate void ButtonAction();

		public const int BUTTON_WIDTH = 56;

		public const int BUTTON_HEIGHT = 16;

		private string label = string.Empty;

		private Rect buttonClickRect;

		public readonly ButtonAction action;

		private bool hovering;

		private bool isPressed;

		private TempTexture labelTexture;

		public TextButton(string label, Vec2 relativePos, ButtonAction action, int buttonWidth = 56, int buttonHeight = 16)
		{
			this.label = label;
			this.action = action;
			buttonClickRect = new Rect(relativePos.X, relativePos.Y, buttonWidth, buttonHeight);
			GenerateLabelTexture();
		}

		public void ShiftPos(Vec2 shift)
		{
			buttonClickRect.X += shift.X;
			buttonClickRect.Y += shift.Y;
		}

		public Vec2 GetPos()
		{
			return new Vec2(buttonClickRect.X + 28, buttonClickRect.Y + 8);
		}

		public bool Update(Vec2 parentPos, bool canInteract)
		{
			if (labelTexture == null || !labelTexture.isValid)
			{
				GenerateLabelTexture();
			}
			labelTexture.KeepAlive();
			if (canInteract)
			{
				Vec2 v = Game1.mouseCursorMan.MousePos - parentPos;
				hovering = buttonClickRect.IsVec2InRect(v);
				if (hovering)
				{
					Game1.mouseCursorMan.SetState(MouseCursorManager.State.Clickable);
				}
				if (hovering && Game1.mouseCursorMan.MouseClicked)
				{
					isPressed = true;
				}
				else if (isPressed && !Game1.mouseCursorMan.MouseHeld)
				{
					if (hovering && action != null)
					{
						action();
					}
					isPressed = false;
				}
				return hovering;
			}
			return false;
		}

		public void Draw(Vec2 parentPos, TWMTheme theme, byte alpha)
		{
			GameColor gameColor = (hovering ? theme.Variant(alpha) : theme.Primary(alpha));
			GameColor gColor = theme.Background(alpha);
			Vec2 vec = new Vec2(parentPos.X + buttonClickRect.X, parentPos.Y + buttonClickRect.Y);
			Game1.gMan.ColorBoxBlit(new Rect(vec.X, vec.Y, buttonClickRect.W, buttonClickRect.H), gameColor);
			Game1.gMan.ColorBoxBlit(new Rect(vec.X + 1, vec.Y + 1, buttonClickRect.W - 2, buttonClickRect.H - 2), gColor);
			Game1.gMan.MainBlit(labelTexture, new Vec2(vec.X * 2 + buttonClickRect.W, (vec.Y + 2) * 2), gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
		}

		public void DrawHighlight(Vec2 parentPos, TWMTheme theme, byte alpha)
		{
			GameColor gColor = (hovering ? theme.Variant(alpha) : theme.Primary(alpha));
			Vec2 vec = new Vec2(parentPos.X + buttonClickRect.X, parentPos.Y + buttonClickRect.Y);
			Game1.gMan.ColorBoxBlit(new Rect(vec.X, vec.Y, buttonClickRect.W, buttonClickRect.H), gColor);
		}

		private void GenerateLabelTexture()
		{
			labelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, label, buttonClickRect.W - 4);
		}
	}
}

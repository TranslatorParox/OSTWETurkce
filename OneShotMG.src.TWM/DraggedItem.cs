using System;

namespace OneShotMG.src.TWM
{
	public class DraggedItem
	{
		private Vec2 HELD_ICON_OFFSET = new Vec2(-42, -36);

		public readonly FileIcon Icon;

		public readonly Action<bool> OnDropComplete;

		public readonly object DragSource;

		public DraggedItem(FileIcon icon, Action<bool> onDropComplete, object dragSource)
		{
			Icon = icon;
			OnDropComplete = onDropComplete;
			DragSource = dragSource;
		}

		public void Draw(TWMTheme theme, Vec2 mousePos)
		{
			if (Icon != null)
			{
				Vec2 pos = mousePos + HELD_ICON_OFFSET;
				Icon.Draw(theme, pos, focus: true, canHover: false, 0.5f);
			}
		}
	}
}

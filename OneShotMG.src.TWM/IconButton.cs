using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class IconButton
	{
		public delegate void ButtonAction();

		public const string ICON_ARROWLEFT = "the_world_machine/window_icons/arrow_left";

		public const string ICON_ARROWRIGHT = "the_world_machine/window_icons/arrow_right";

		public const string ICON_ARROWUP = "the_world_machine/window_icons/arrow_up";

		public const string ICON_ARROWDOWN = "the_world_machine/window_icons/arrow_down";

		public const string ICON_MUSIC_PLAY = "the_world_machine/window_icons/play";

		public const string ICON_MUSIC_PAUSE = "the_world_machine/window_icons/pause";

		public const string ICON_MUSIC_STOP = "the_world_machine/window_icons/stop";

		public const string ICON_MUSIC_RESTART = "the_world_machine/window_icons/restart_track";

		public const string ICON_EDIT = "the_world_machine/window_icons/edit";

		public const string ICON_GAMEPAD_TAB = "the_world_machine/window_icons/gamepad_tab";

		public const string ICON_GAMEPAD_TAB_SELECTED = "the_world_machine/window_icons/gamepad_tab_selected";

		public const string ICON_KEYBOARD_TAB = "the_world_machine/window_icons/keyboard_tab";

		public const string ICON_KEYBOARD_TAB_SELECTED = "the_world_machine/window_icons/keyboard_tab_selected";

		public string Icon;

		public bool Disabled;

		public bool Clickable = true;

		private TextureCache.CacheType iconTextureCache = TextureCache.CacheType.TheWorldMachine;

		public int AutoRepeatTriggerDelay = 20;

		public int AutoRepeatDelay;

		public int BorderWidth;

		public bool Tint = true;

		protected readonly ButtonAction action;

		protected Rect buttonClickRect;

		protected bool hovering;

		protected bool isPressed;

		protected bool isAutoRepeating;

		protected int autoRepeatTimer;

		public Vec2 Position
		{
			get
			{
				return buttonClickRect.XY;
			}
			set
			{
				buttonClickRect.X = value.X;
				buttonClickRect.Y = value.Y;
			}
		}

		public IconButton(string iconPath, Vec2 buttonSize, Vec2 relativePos, ButtonAction action, TextureCache.CacheType cacheType = TextureCache.CacheType.TheWorldMachine)
		{
			Icon = iconPath;
			iconTextureCache = cacheType;
			this.action = action;
			buttonClickRect = new Rect(relativePos.X, relativePos.Y, buttonSize.X, buttonSize.Y);
		}

		public IconButton(string iconPath, Vec2 relativePos, ButtonAction action, TextureCache.CacheType cacheType = TextureCache.CacheType.TheWorldMachine)
			: this(iconPath, Game1.gMan.TextureSize(iconPath, cacheType), relativePos, action, cacheType)
		{
		}

		public virtual void Draw(Vec2 parentPos, TWMTheme theme, byte alpha, bool dropShadow = false)
		{
			GameColor gameColor = (((hovering | Disabled) && Clickable) ? theme.Variant(alpha) : theme.Primary(alpha));
			GameColor gameColor2 = theme.Background(alpha);
			if (BorderWidth > 0)
			{
				Rect boxRect = buttonClickRect.Translated(parentPos).Shrink(-BorderWidth);
				Game1.gMan.ColorBoxBlit(boxRect, gameColor);
				Game1.gMan.ColorBoxBlit(buttonClickRect.Translated(parentPos), theme.Background(alpha));
			}
			Vec2 vec = parentPos + buttonClickRect.XY;
			if (!Tint)
			{
				gameColor = GameColor.White;
				gameColor.a = alpha;
			}
			if (dropShadow)
			{
				Game1.gMan.MainBlit(Icon, vec + new Vec2(1, 1), gameColor2, 0, GraphicsManager.BlendMode.Normal, 2, iconTextureCache);
			}
			Game1.gMan.MainBlit(Icon, vec, gameColor, 0, GraphicsManager.BlendMode.Normal, 2, iconTextureCache);
		}

		public virtual bool Update(Vec2 parentPos, bool canInteract)
		{
			if (!canInteract)
			{
				hovering = false;
				isAutoRepeating = false;
				autoRepeatTimer = 0;
			}
			else
			{
				Vec2 v = Game1.mouseCursorMan.MousePos - parentPos;
				hovering = buttonClickRect.IsVec2InRect(v);
				if (hovering && Clickable)
				{
					MouseCursorManager.State state = ((!Disabled) ? MouseCursorManager.State.Clickable : MouseCursorManager.State.NotAllowed);
					Game1.mouseCursorMan.SetState(state);
				}
				if (hovering && Clickable && !Disabled && Game1.mouseCursorMan.MouseClicked)
				{
					isPressed = true;
					isAutoRepeating = false;
				}
				else if (isPressed)
				{
					if (!Game1.mouseCursorMan.MouseHeld)
					{
						if (hovering && action != null && !isAutoRepeating)
						{
							action();
						}
						isPressed = false;
						autoRepeatTimer = 0;
					}
					else if (AutoRepeatDelay > 0 && hovering)
					{
						autoRepeatTimer++;
						int num = (isAutoRepeating ? AutoRepeatDelay : AutoRepeatTriggerDelay);
						if (autoRepeatTimer > num)
						{
							action();
							autoRepeatTimer = 0;
							isAutoRepeating = true;
						}
					}
				}
			}
			return hovering;
		}
	}
}

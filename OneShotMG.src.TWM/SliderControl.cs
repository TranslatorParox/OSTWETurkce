using System;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class SliderControl
	{
		public const int VERTICAL_SLIDER_WIDTH = 16;

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private const string ICON_KNOB = "the_world_machine/window_icons/knob";

		private const int TEXT_Y_OFFSET = 2;

		private const int KNOB_SIZE = 16;

		private string label;

		private TempTexture labelTexture;

		private Vec2 pos;

		private int length;

		private bool sliderGrabbed;

		private Vec2 knobGrabOffset;

		private bool vertical;

		private bool useButtons;

		private int _value;

		public int Increment = 1;

		public int DragIncrement;

		private string labelSuffix = "";

		public bool Active = true;

		public int Min;

		public int Max;

		public Action<int> OnValueChanged;

		public Rect? ScrollTriggerZone;

		private Rect sliderArea;

		private Rect sliderScrollArea = Rect.Zero;

		private IconButton arrowLeft;

		private IconButton arrowRight;

		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				int value2 = _value;
				_value = value;
				if (value2 != value)
				{
					DrawLabelTexture();
				}
			}
		}

		public Vec2 Position
		{
			get
			{
				return pos;
			}
			set
			{
				pos = value;
				SetControlPositions();
			}
		}

		public int Length
		{
			get
			{
				return length;
			}
			set
			{
				length = value;
				SetControlPositions();
			}
		}

		private Vec2 KnobPos
		{
			get
			{
				float num = (float)(Value - Min) / (float)(Max - Min);
				Vec2 result = default(Vec2);
				if (vertical)
				{
					result.X = sliderArea.X - 6;
					result.Y = sliderArea.Y + (int)((float)(sliderArea.H - 16) * num);
				}
				else
				{
					result.X = sliderArea.X + (int)((float)(sliderArea.W - 16) * num);
					result.Y = sliderArea.Y - 6;
				}
				return result;
			}
		}

		public SliderControl(string label, int min, int max, Vec2 pos, int length, bool useButtons = true, bool vertical = false, string lblSuffix = "")
		{
			this.label = label;
			Min = min;
			Max = max;
			this.pos = pos;
			this.length = length;
			this.vertical = vertical;
			this.useButtons = useButtons;
			labelSuffix = lblSuffix;
			SetControlPositions();
			Value = min;
		}

		private void SetControlPositions()
		{
			int num = 16;
			if (string.IsNullOrEmpty(label))
			{
				num = 0;
			}
			if (vertical)
			{
				sliderArea = new Rect(pos.X + 6, pos.Y, 4, length);
			}
			else
			{
				sliderArea = new Rect(pos.X, pos.Y + num + 6, length, 4);
			}
			if (useButtons)
			{
				if (vertical)
				{
					sliderArea.Y += 16;
					sliderArea.H -= 32;
					arrowLeft = new IconButton("the_world_machine/window_icons/arrow_up", pos, OnArrowLeft);
					arrowRight = new IconButton("the_world_machine/window_icons/arrow_down", new Vec2(pos.X, pos.Y + length - 16), OnArrowRight);
					sliderScrollArea = new Rect(pos.X, pos.Y, 16, length);
				}
				else
				{
					Vec2 vec = new Vec2(0, num);
					sliderArea.X += 16;
					sliderArea.W -= 32;
					arrowLeft = new IconButton("the_world_machine/window_icons/arrow_left", pos + vec, OnArrowLeft);
					arrowRight = new IconButton("the_world_machine/window_icons/arrow_right", new Vec2(pos.X + length - 16, pos.Y) + vec, OnArrowRight);
				}
				arrowLeft.AutoRepeatDelay = 6;
				arrowRight.AutoRepeatDelay = 6;
			}
		}

		public void Draw(TWMTheme theme, Vec2 parentPos, byte alpha)
		{
			if (Active)
			{
				GameColor gameColor = theme.Primary(alpha);
				GameColor gColor = theme.Background(alpha);
				GameColor gameColor2 = theme.Variant(alpha);
				Rect boxRect = sliderArea.Translated(parentPos);
				Game1.gMan.ColorBoxBlit(boxRect, gameColor);
				Rect boxRect2 = boxRect.Shrink(1);
				if (vertical)
				{
					int num = (int)((float)(Value - Min) / (float)(Max - Min) * (float)boxRect2.H);
					boxRect2.Y += num;
					boxRect2.H -= num;
				}
				else
				{
					int num2 = (int)((float)(Value - Min) / (float)(Max - Min) * (float)boxRect2.W);
					boxRect2.X += num2;
					boxRect2.W -= num2;
				}
				Game1.gMan.ColorBoxBlit(boxRect2, gColor);
				arrowLeft.Draw(parentPos, theme, alpha);
				arrowRight.Draw(parentPos, theme, alpha);
				if (!string.IsNullOrEmpty(label) && !vertical && labelTexture != null && labelTexture.isValid)
				{
					Vec2 vec = new Vec2(boxRect.X - 16 + (sliderArea.W + 32 - labelTexture.renderTarget.Width / 2) / 2, parentPos.Y + pos.Y + 2);
					Game1.gMan.MainBlit(labelTexture, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				}
				Game1.gMan.MainBlit("the_world_machine/window_icons/knob", KnobPos + parentPos, sliderGrabbed ? gameColor2 : gameColor, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.TheWorldMachine);
			}
		}

		private void DrawLabelTexture()
		{
			if (!vertical)
			{
				string tWMLocString = Game1.languageMan.GetTWMLocString(label);
				string text = $"{tWMLocString}: {Value}{labelSuffix}";
				labelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, text, sliderArea.W + 32);
			}
		}

		public bool Update(Vec2 parentPos, bool canInteract)
		{
			if (!vertical)
			{
				if (labelTexture == null || !labelTexture.isValid)
				{
					DrawLabelTexture();
				}
				labelTexture.KeepAlive();
			}
			bool flag = !canInteract;
			if (!Active)
			{
				return false;
			}
			flag |= arrowLeft.Update(parentPos, canInteract);
			flag |= arrowRight.Update(parentPos, canInteract);
			Vec2 mousePos = Game1.mouseCursorMan.MousePos;
			if (!Game1.mouseCursorMan.MouseHeld)
			{
				sliderGrabbed = false;
			}
			if (!flag)
			{
				if (sliderGrabbed)
				{
					flag = true;
					Game1.mouseCursorMan.SetState(MouseCursorManager.State.Holding);
					int value = _value;
					float num = ((!vertical) ? ((float)(mousePos.X - knobGrabOffset.X - (parentPos.X + sliderArea.X)) / (float)(sliderArea.W - 16)) : ((float)(mousePos.Y - knobGrabOffset.Y - (parentPos.Y + sliderArea.Y)) / (float)(sliderArea.H - 16)));
					_value = Min + (int)Math.Ceiling(num * (float)(Max - Min));
					int num2 = ((DragIncrement > 0) ? DragIncrement : Increment);
					if (num2 > 1)
					{
						_value -= _value % num2;
					}
					_value = Math.Max(Math.Min(_value, Max), Min);
					if (_value != value)
					{
						DrawLabelTexture();
						OnValueChanged?.Invoke(Value);
					}
				}
				else
				{
					Vec2 vec = KnobPos + parentPos;
					if (new Rect(vec.X, vec.Y, 16, 16).IsVec2InRect(mousePos))
					{
						flag = true;
						Game1.mouseCursorMan.SetState(MouseCursorManager.State.Grabbable);
						if (Game1.mouseCursorMan.MouseClicked)
						{
							sliderGrabbed = true;
							knobGrabOffset = mousePos - vec;
						}
					}
					HandleMouseScrollWheelInput(parentPos, mousePos);
				}
			}
			return flag;
		}

		private void HandleMouseScrollWheelInput(Vec2 parentPos, Vec2 mousePos)
		{
			if (!vertical)
			{
				return;
			}
			bool flag = new Rect(parentPos.X + sliderScrollArea.X, parentPos.Y + sliderScrollArea.Y, sliderScrollArea.W, sliderScrollArea.H).IsVec2InRect(mousePos);
			if (ScrollTriggerZone.HasValue)
			{
				Rect value = ScrollTriggerZone.Value;
				value.X += parentPos.X;
				value.Y += parentPos.Y;
				flag |= value.IsVec2InRect(mousePos);
			}
			if (!flag)
			{
				return;
			}
			int mouseScrollSpeed = Game1.mouseCursorMan.MouseScrollSpeed;
			if (mouseScrollSpeed < 0)
			{
				for (float num = (float)mouseScrollSpeed / 120f; num < 0f; num += 1f)
				{
					OnArrowRight();
				}
			}
			else if (mouseScrollSpeed > 0)
			{
				for (float num2 = (float)mouseScrollSpeed / 120f; num2 > 0f; num2 -= 1f)
				{
					OnArrowLeft();
				}
			}
		}

		private void OnArrowLeft()
		{
			if (Value > Min)
			{
				Value = Math.Max(Min, Value - Increment);
				OnValueChanged?.Invoke(Value);
			}
		}

		private void OnArrowRight()
		{
			if (Value < Max)
			{
				Value = Math.Min(Max, Value + Increment);
				OnValueChanged?.Invoke(Value);
			}
		}
	}
}

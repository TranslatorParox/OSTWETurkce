using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class ResolutionSelectModalWindow : ModalWindow
	{
		private const int DISPLAYED_RESOLUTIONS = 5;

		private const int RES_CHOICE_HEIGHT = 20;

		private const int RES_CHOICE_WIDTH = 132;

		private const int PICKER_HEIGHT = 100;

		private List<Vec2> resolutions;

		private List<TempTexture> resTextures;

		private int selectedIndex;

		private int hoveredIndex;

		private int displayStartIndex;

		private SliderControl resSlider;

		public ResolutionSelectModalWindow()
			: base(ModalType.Info, "select_resolution_modal_window")
		{
			base.ContentsSize = new Vec2(160, displayedLines.Count * 12 + 8 + 100 + 8 + 20);
			resolutions = Game1.gMan.AvailableResolutions;
			for (int i = 0; i < resolutions.Count; i++)
			{
				if (resolutions[i].Equals(Game1.gMan.DrawScreenSize))
				{
					selectedIndex = i;
					break;
				}
			}
			int num = selectedIndex - 5 + 1;
			if (num > displayStartIndex)
			{
				displayStartIndex = num;
			}
			generateResTextures();
			bool flag = resolutions.Count > 5;
			resSlider = new SliderControl(null, 0, flag ? (resolutions.Count - 5) : 0, new Vec2(140, displayedLines.Count * 12 + 8), 100, useButtons: true, vertical: true);
			resSlider.Active = flag;
			resSlider.Value = displayStartIndex;
			SliderControl sliderControl = resSlider;
			sliderControl.OnValueChanged = (Action<int>)Delegate.Combine(sliderControl.OnValueChanged, (Action<int>)delegate(int val)
			{
				displayStartIndex = val;
			});
			resSlider.ScrollTriggerZone = new Rect(4, displayedLines.Count * 12 + 8, 132, 100);
		}

		private void generateResTextures()
		{
			resTextures = new List<TempTexture>();
			foreach (Vec2 resolution in resolutions)
			{
				int x = resolution.X;
				string text = x.ToString();
				x = resolution.Y;
				string text2 = text + "x" + x;
				resTextures.Add(Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, text2, 124));
			}
		}

		protected override void AddButtons()
		{
			modalButtons = new List<TextButton>();
			Vec2 relativePos = new Vec2(20, displayedLines.Count * 12 + 8 + 100 + 8);
			Vec2 relativePos2 = new Vec2(84, relativePos.Y);
			modalButtons.Add(new TextButton(Game1.languageMan.GetTWMLocString("dialog_ok"), relativePos, delegate
			{
				onButtonClick(ModalResponse.OK);
			}));
			modalButtons.Add(new TextButton(Game1.languageMan.GetTWMLocString("dialog_cancel"), relativePos2, delegate
			{
				onButtonClick(ModalResponse.Cancel);
			}));
		}

		public override bool Update(bool mouseAlreadyOnOtherWindow)
		{
			bool flag = false;
			foreach (TempTexture resTexture in resTextures)
			{
				if (resTexture == null || !resTexture.isValid)
				{
					flag = true;
					break;
				}
				resTexture.KeepAlive();
			}
			if (flag)
			{
				generateResTextures();
			}
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			hoveredIndex = -1;
			if (!mouseAlreadyOnOtherWindow && !base.IsMinimized)
			{
				resSlider.Update(parentPos, !mouseAlreadyOnOtherWindow);
				Rect rect = new Rect(parentPos.X + 4, parentPos.Y + displayedLines.Count * 12 + 8, 132, 100);
				for (int i = 0; i < 5; i++)
				{
					int num = i + displayStartIndex;
					if (num >= resolutions.Count)
					{
						break;
					}
					if (new Rect(rect.X, rect.Y + 20 * i, 132, 19).IsVec2InRect(Game1.mouseCursorMan.MousePos))
					{
						if (Game1.mouseCursorMan.MouseClicked)
						{
							selectedIndex = num;
						}
						else if (num != selectedIndex)
						{
							hoveredIndex = num;
							Game1.mouseCursorMan.SetState(MouseCursorManager.State.Clickable);
						}
					}
				}
			}
			return base.Update(mouseAlreadyOnOtherWindow);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			base.DrawContents(theme, screenPos, alpha);
			GameColor gameColor = theme.Primary(alpha);
			GameColor gColor = theme.Variant(alpha);
			GameColor gameColor2 = theme.Background(alpha);
			Rect boxRect = new Rect(screenPos.X + 4, screenPos.Y + displayedLines.Count * 12 + 8, 132, 100);
			Rect boxRect2 = new Rect(boxRect.X - 1, boxRect.Y - 1, boxRect.W + 2, boxRect.H + 2);
			Game1.gMan.ColorBoxBlit(boxRect2, gameColor);
			Game1.gMan.ColorBoxBlit(boxRect, gameColor2);
			for (int i = displayStartIndex; i < displayStartIndex + 5 && i < resTextures.Count; i++)
			{
				TempTexture tempTexture = resTextures[i];
				if (tempTexture == null || !tempTexture.isValid)
				{
					continue;
				}
				Vec2 pixelPos = new Vec2(boxRect.X + 4, boxRect.Y + 20 * (i - displayStartIndex) + 4);
				pixelPos *= 2;
				if (i == selectedIndex)
				{
					Game1.gMan.ColorBoxBlit(new Rect(boxRect.X, boxRect.Y + 20 * (i - displayStartIndex), boxRect.W, 20), gameColor);
					Game1.gMan.MainBlit(tempTexture, pixelPos, gameColor2, 0, GraphicsManager.BlendMode.Normal, 1);
					continue;
				}
				if (i == hoveredIndex)
				{
					Game1.gMan.ColorBoxBlit(new Rect(boxRect.X, boxRect.Y + 20 * (i - displayStartIndex), boxRect.W, 20), gColor);
				}
				Game1.gMan.MainBlit(tempTexture, pixelPos, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
			}
			resSlider.Draw(theme, screenPos, alpha);
		}

		public Vec2 GetSelectedResolution()
		{
			return resolutions[selectedIndex];
		}
	}
}

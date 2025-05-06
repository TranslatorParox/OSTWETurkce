using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class CustomizationWindow : TWMWindow
	{
		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private const int CONTENT_MARGIN = 4;

		private const int TEXT_OFFSET = 2;

		private const int TEXT_HEIGHT = 16;

		private const int BUTTON_SIZE = 16;

		private const int ROWS_COUNT = 5;

		private ChooserControl systemScalingChooser;

		public Action<string> OnChangeSystemScaling;

		private TempTexture systemScalingLabelTexture;

		private ChooserControl vsyncChooser;

		private ChooserControl badgeToastChooser;

		private SliderControl gammaSlider;

		private SliderControl bgmSlider;

		private SliderControl sfxSlider;

		private TextButton viewWallpaperButton;

		private TextButton changeLanguageButton;

		private TextButton changeResolutionButton;

		public Action<string> OnChangeBadgeToastsEnabled;

		public Action<string> OnChangeVsync;

		public Action<int> OnChangeGamma;

		public Action<int> OnChangeBGMVol;

		public Action<int> OnChangeSFXVol;

		public const int COLUMN_WIDTH = 240;

		private TempTexture vsyncLabelTexture;

		private TempTexture badgeToastsLabel;

		private TempTexture resolutionLabelTexture;

		private TempTexture fullscreenLabelTexture;

		private ChooserControl fullscreenChooser;

		public Action<string> OnChangeFullscreen;

		private bool hideFullscreenChooser;

		public CustomizationWindow(bool vsyncEnabled, bool toastsEnabled, bool isSystemScalingSmooth)
		{
			base.ContentsSize = new Vec2(480, 204);
			base.WindowIcon = "customize";
			base.WindowTitle = "customize_app_name";
			Func<string, string, string> locFunc = (string value, string label) => Game1.languageMan.GetTWMLocString(label);
			Vec2 pos = new Vec2(4, 24);
			List<(string, string)> items = new List<(string, string)>
			{
				("ON", "settings_on"),
				("OFF", "settings_off")
			};
			vsyncChooser = new ChooserControl(pos, 232, items, vsyncEnabled ? "ON" : "OFF", GraphicsManager.FontType.OS, locFunc);
			vsyncChooser.OnItemChange = delegate(string s)
			{
				OnChangeVsync?.Invoke(s);
			};
			Vec2 pos2 = new Vec2(4, pos.Y + 8 + 16 + 16);
			List<(string, string)> items2 = new List<(string, string)>
			{
				("ON", "settings_on"),
				("OFF", "settings_off")
			};
			badgeToastChooser = new ChooserControl(pos2, 232, items2, toastsEnabled ? "ON" : "OFF", GraphicsManager.FontType.OS, locFunc);
			badgeToastChooser.OnItemChange = delegate(string s)
			{
				OnChangeBadgeToastsEnabled?.Invoke(s);
			};
			Vec2 relativePos = new Vec2(4, pos2.Y + 8 + 16 + 16);
			changeResolutionButton = new TextButton(Game1.languageMan.GetTWMLocString("change_resolution_button"), relativePos, delegate
			{
				onChangeResolutionButtonClicked();
			}, 232);
			Vec2 pos3 = new Vec2(4, relativePos.Y + 8 + 16 + 16);
			List<(string, string)> items3 = new List<(string, string)>
			{
				("SMOOTH", "customize_system_scaling_smooth"),
				("SHARP", "customize_system_scaling_sharp")
			};
			systemScalingChooser = new ChooserControl(pos3, 232, items3, isSystemScalingSmooth ? "SMOOTH" : "SHARP", GraphicsManager.FontType.OS, locFunc);
			systemScalingChooser.OnItemChange = delegate(string s)
			{
				OnChangeSystemScaling?.Invoke(s);
			};
			Vec2 pos4 = new Vec2(244, relativePos.Y + 8 + 16 + 16);
			new List<(string, string)>
			{
				("ON", "settings_on"),
				("OFF", "settings_off")
			};
			fullscreenChooser = new ChooserControl(pos4, 232, items, Game1.gMan.IsFullscreen() ? "ON" : "OFF", GraphicsManager.FontType.OS, locFunc);
			fullscreenChooser.OnItemChange = delegate(string s)
			{
				OnChangeFullscreen?.Invoke(s);
			};
			if (Game1.steamMan.IsOnSteamDeck)
			{
				hideFullscreenChooser = true;
			}
			Vec2 relativePos2 = new Vec2(4, pos3.Y + 8 + 16);
			viewWallpaperButton = new TextButton(Game1.languageMan.GetTWMLocString("customize_view_wallpaper_button"), relativePos2, delegate
			{
				onViewWallpaperButtonClicked();
			}, 232);
			Vec2 pos5 = new Vec2(244, 8);
			gammaSlider = new SliderControl("customize_slider_label_brightness", 50, 150, pos5, 232, useButtons: true, vertical: false, "%");
			gammaSlider.Value = Game1.gMan.Gamma;
			gammaSlider.OnValueChanged = delegate(int i)
			{
				OnChangeGamma?.Invoke(i);
			};
			Vec2 pos6 = new Vec2(244, pos5.Y + 8 + 16 + 16);
			bgmSlider = new SliderControl("customize_slider_label_musicvolume", 0, 100, pos6, 232, useButtons: true, vertical: false, "%");
			bgmSlider.Value = Game1.soundMan.BGMVol;
			bgmSlider.OnValueChanged = delegate(int i)
			{
				OnChangeBGMVol?.Invoke(i);
			};
			Vec2 pos7 = new Vec2(244, pos6.Y + 8 + 16 + 16);
			sfxSlider = new SliderControl("customize_slider_label_soundvolume", 0, 100, pos7, 232, useButtons: true, vertical: false, "%");
			sfxSlider.Value = Game1.soundMan.SFXVol;
			sfxSlider.OnValueChanged = delegate(int i)
			{
				OnChangeSFXVol?.Invoke(i);
			};
			changeLanguageButton = new TextButton(relativePos: new Vec2(244, relativePos2.Y), label: Game1.languageMan.GetTWMLocString("change_language_button"), action: delegate
			{
				onChangeLanguageButtonClicked();
			}, buttonWidth: 232);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
		}

		private void DrawVSyncLabelTexture()
		{
			vsyncLabelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("customize_chooser_label_vsync"), 240);
		}

		private void DrawBadgeToastsLabelTexture()
		{
			badgeToastsLabel = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("customize_chooser_label_badgetoasts"), 240);
		}

		private void DrawResolutionLabelTexture()
		{
			string text = $": {Game1.gMan.TWMDrawScreenSize.X}x{Game1.gMan.TWMDrawScreenSize.Y}";
			resolutionLabelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("customize_chooser_label_resolution") + text, 240);
		}

		private void DrawSystemScalingLabelTexture()
		{
			systemScalingLabelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("customize_chooser_label_system_scaling"), 240);
		}

		private void DrawFullscreenLabelTexture()
		{
			fullscreenLabelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, Game1.languageMan.GetTWMLocString("customize_chooser_label_fullscreen"));
		}

		public void onChangeResolutionButtonClicked()
		{
			ShowModalWindow(new ResolutionSelectModalWindow(), delegate(ModalWindow.ModalResponse res)
			{
				if (res == ModalWindow.ModalResponse.OK && modalWindow is ResolutionSelectModalWindow resolutionSelectModalWindow)
				{
					Game1.gMan.SetDrawResolution(resolutionSelectModalWindow.GetSelectedResolution());
					DrawResolutionLabelTexture();
				}
			});
		}

		public void onChangeLanguageButtonClicked()
		{
			if (Game1.windowMan.IsOneshotWindowOpen())
			{
				ShowModalWindow(ModalWindow.ModalType.Error, "cant_change_language_when_oneshot_running");
				return;
			}
			Game1.windowMan.ShowModalWindow(ModalWindow.ModalType.YesNo, "change_language_yesno", delegate(ModalWindow.ModalResponse res)
			{
				if (res == ModalWindow.ModalResponse.Yes)
				{
					Game1.windowMan.ShowLanguageSelect();
				}
			});
		}

		public void onViewWallpaperButtonClicked()
		{
			Game1.windowMan.HideAllUI = true;
		}

		public override bool Update(bool cursorOccluded)
		{
			if (vsyncLabelTexture == null || !vsyncLabelTexture.isValid)
			{
				DrawVSyncLabelTexture();
			}
			if (badgeToastsLabel == null || !badgeToastsLabel.isValid)
			{
				DrawBadgeToastsLabelTexture();
			}
			if (resolutionLabelTexture == null || !resolutionLabelTexture.isValid)
			{
				DrawResolutionLabelTexture();
			}
			if (systemScalingLabelTexture == null || !systemScalingLabelTexture.isValid)
			{
				DrawSystemScalingLabelTexture();
			}
			if (fullscreenLabelTexture == null || !fullscreenLabelTexture.isValid)
			{
				DrawFullscreenLabelTexture();
			}
			fullscreenLabelTexture.KeepAlive();
			vsyncLabelTexture.KeepAlive();
			badgeToastsLabel.KeepAlive();
			resolutionLabelTexture.KeepAlive();
			systemScalingLabelTexture.KeepAlive();
			if (!IsModalWindowOpen())
			{
				bool canInteract = !cursorOccluded && !base.IsMinimized;
				Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
				vsyncChooser.Update(parentPos, canInteract);
				badgeToastChooser.Update(parentPos, canInteract);
				changeResolutionButton.Update(parentPos, canInteract);
				if (!hideFullscreenChooser)
				{
					fullscreenChooser.Update(parentPos, canInteract);
				}
				gammaSlider.Update(parentPos, canInteract);
				bgmSlider.Update(parentPos, canInteract);
				sfxSlider.Update(parentPos, canInteract);
				systemScalingChooser.Update(parentPos, canInteract);
				changeLanguageButton.Update(parentPos, canInteract);
				viewWallpaperButton.Update(parentPos, canInteract);
			}
			return base.Update(cursorOccluded);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			GameColor gameColor = theme.Primary(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gColor);
			vsyncChooser.Draw(screenPos, theme, alpha);
			badgeToastChooser.Draw(screenPos, theme, alpha);
			changeResolutionButton.Draw(screenPos, theme, alpha);
			viewWallpaperButton.Draw(screenPos, theme, alpha);
			gammaSlider.Draw(theme, screenPos, alpha);
			bgmSlider.Draw(theme, screenPos, alpha);
			sfxSlider.Draw(theme, screenPos, alpha);
			systemScalingChooser.Draw(screenPos, theme, alpha);
			changeLanguageButton.Draw(screenPos, theme, alpha);
			Vec2 vec = new Vec2(120, 6) + screenPos;
			Game1.gMan.MainBlit(vsyncLabelTexture, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			vec.Y += 40;
			Game1.gMan.MainBlit(badgeToastsLabel, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			vec.Y += 40;
			Game1.gMan.MainBlit(resolutionLabelTexture, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			vec.Y += 40;
			Game1.gMan.MainBlit(systemScalingLabelTexture, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			if (!hideFullscreenChooser)
			{
				fullscreenChooser.Draw(screenPos, theme, alpha);
				vec.X += 240;
				Game1.gMan.MainBlit(fullscreenLabelTexture, vec * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1, xCentered: true);
			}
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is CustomizationWindow;
		}

		public void SetFullscreenValue(bool isFullscreen)
		{
			fullscreenChooser.Value = (isFullscreen ? "ON" : "OFF");
		}
	}
}

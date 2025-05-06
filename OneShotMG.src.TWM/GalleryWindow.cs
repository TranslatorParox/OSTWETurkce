using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class GalleryWindow : TWMWindow
	{
		private const int SCROLLBAR_SIZE = 16;

		private const int FULLSCREEN_CONTROL_HEIGHT = 40;

		private const int CHOOSER_WIDTH = 240;

		private const int SCROLLBAR_MAX_LEN = 320;

		private const string EMPTY_MESSAGE = "gallery_empty_message";

		private static Dictionary<string, GalleryInfo> _galleryInfo;

		private readonly List<GalleryInfo> imageList;

		private readonly ChooserControl imageChooser;

		private readonly SliderControl hScroll;

		private readonly SliderControl vScroll;

		private readonly TWMWindowButton bExitFullscreen;

		private readonly string baseTitle;

		private Vec2 DISPLAY_SIZE = new Vec2(320, 240);

		private Vec2 chooserPos = new Vec2(40, 244);

		private Vec2 rememberedWindowPos;

		private bool isGlitched;

		public static Dictionary<string, GalleryInfo> GalleryInfo
		{
			get
			{
				if (_galleryInfo == null)
				{
					_galleryInfo = LoadGalleryInfo();
				}
				return _galleryInfo;
			}
		}

		public GalleryWindow(List<string> unlockedCgs)
		{
			if (Game1.windowMan.Desktop.inSolstice && MathHelper.Random(1, 16) == 16)
			{
				isGlitched = true;
				string item = unlockedCgs[0];
				unlockedCgs = new List<string>();
				unlockedCgs.Add(item);
			}
			baseTitle = "gallery_app";
			base.WindowTitle = baseTitle;
			base.WindowIcon = "gallery";
			imageList = new List<GalleryInfo>();
			AddButton(TWMWindowButtonType.Close, delegate
			{
				Game1.gMan.clearTextureCache(TextureCache.CacheType.GalleryWindow);
				onClose(this);
			});
			AddButton(TWMWindowButtonType.Maximize, OnMaximize);
			AddButton(TWMWindowButtonType.Minimize);
			Func<string, string, string> locFunc = (string value, string originalText) => isGlitched ? "???" : Game1.languageMan.GetCgNameLocString(imageChooser.Value, originalText);
			imageChooser = new ChooserControl(chooserPos, 240, new List<(string, string)>(), null, GraphicsManager.FontType.OS, locFunc);
			bExitFullscreen = new TWMWindowButton(TWMWindowButtonType.Maximize, Vec2.Zero, delegate
			{
				OnMaximize();
			});
			hScroll = new SliderControl("", 0, 100, new Vec2(0, base.ContentsSize.Y + 24), 100);
			vScroll = new SliderControl("", 0, 100, new Vec2(base.ContentsSize.X, 0), 100, useButtons: true, vertical: true);
			hScroll.DragIncrement = 1;
			vScroll.DragIncrement = 1;
			foreach (string unlockedCg in unlockedCgs)
			{
				AddImage(unlockedCg, doSort: false);
			}
			imageList.Sort((GalleryInfo a, GalleryInfo b) => a.displayOrder - b.displayOrder);
			List<(string, string)> items = imageList.Select((GalleryInfo i) => (imageId: i.imageId, displayName: i.displayName)).ToList();
			imageChooser.SetItems(items, imageList.FirstOrDefault()?.imageId);
			imageChooser.OnItemChange = delegate
			{
				ConfigCurrentCg();
			};
			ConfigCurrentCg();
		}

		private static Dictionary<string, GalleryInfo> LoadGalleryInfo()
		{
			GalleryInfoMetadata galleryInfoMetadata = JsonConvert.DeserializeObject<GalleryInfoMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/cg_unlocks.json")));
			Dictionary<string, GalleryInfo> dictionary = new Dictionary<string, GalleryInfo>();
			foreach (GalleryInfo cg in galleryInfoMetadata.cgList)
			{
				dictionary[cg.imageId] = cg;
			}
			return dictionary;
		}

		private void OnMaximize()
		{
			ToggleMaximize();
			if (base.IsMaximized)
			{
				rememberedWindowPos = Pos;
				Pos = Vec2.Zero;
				ConfigCurrentCg();
			}
			else
			{
				Pos = rememberedWindowPos;
				ConfigCurrentCg();
			}
		}

		public override bool Update(bool cursorOccluded)
		{
			Vec2 parentPos;
			if (base.IsMaximized)
			{
				cursorOccluded = false;
				parentPos = Vec2.Zero;
				bExitFullscreen.Update(Vec2.Zero, canInteract: true);
			}
			else
			{
				parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			}
			bool canInteract = !cursorOccluded && !base.IsMinimized;
			imageChooser.Update(parentPos, canInteract);
			hScroll.Update(parentPos, canInteract);
			vScroll.Update(parentPos, canInteract);
			return base.Update(cursorOccluded);
		}

		public override void Draw(TWMTheme theme)
		{
			if (base.IsMaximized)
			{
				DrawContents(theme, Vec2.Zero, byte.MaxValue);
			}
			else
			{
				base.Draw(theme);
			}
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			theme.Primary(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gColor);
			DrawCurrentCg(theme, screenPos, alpha);
			hScroll.Draw(theme, screenPos, alpha);
			vScroll.Draw(theme, screenPos, alpha);
			imageChooser.Draw(screenPos, theme, alpha);
			if (base.IsMaximized)
			{
				bExitFullscreen.Draw(screenPos, theme, alpha);
			}
		}

		private void DrawCurrentCg(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GalleryInfo cg = GetCurrentCg();
			if (cg != null)
			{
				string textureName = "pictures/" + cg.imageId;
				if (isGlitched)
				{
					textureName = "pictures/ohno";
				}
				Vec2 cgPos;
				Rect sourceRect;
				if (cg.oversize && !base.IsMaximized)
				{
					Vec2 vec = Game1.gMan.TextureSize(textureName, TextureCache.CacheType.GalleryWindow);
					float num = (float)DISPLAY_SIZE.Y / (float)vec.Y * 2f;
					Vec2 pixelPos = screenPos * 2;
					Rect srcRect = new Rect(0, 0, vec.X, vec.Y);
					Game1.gMan.MainBlit(textureName, pixelPos, srcRect, num, num, 1f, 0, GraphicsManager.BlendMode.LinearStretch, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.GalleryWindow);
				}
				else if (base.IsMaximized)
				{
					(Vec2, Vec2, Vec2, float) tuple = ImageMetricsBestFit(cg);
					Vec2 ıtem = tuple.Item2;
					Vec2 ıtem2 = tuple.Item3;
					float scale = tuple.Item4;
					Vec2 fullscreenDrawSpace = GetFullscreenDrawSpace(cg);
					cgPos = new Vec2(Math.Max(0, (fullscreenDrawSpace.X - ıtem2.X) / 2), Math.Max(0, (fullscreenDrawSpace.Y - ıtem2.Y) / 2));
					Game1.gMan.TextureSize(textureName, TextureCache.CacheType.GalleryWindow);
					sourceRect = new Rect(hScroll.Value, vScroll.Value, ıtem.X, ıtem.Y);
					GraphicsManager.BlendMode blend = ((ıtem.Y <= 240) ? GraphicsManager.BlendMode.Normal : GraphicsManager.BlendMode.LinearStretch);
					if (!string.IsNullOrEmpty(cg.fullscreenBlendMode) && Enum.TryParse<GraphicsManager.BlendMode>(cg.fullscreenBlendMode, out var result))
					{
						blend = result;
					}
					Game1.gMan.MainBlit(textureName, cgPos, sourceRect, scale, scale, (float)(int)alpha / 255f, 0, blend, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.GalleryWindow);
					cg.additionalLayers?.ForEach(delegate(string layer)
					{
						string textureName2 = "pictures/" + layer;
						Game1.gMan.MainBlit(textureName2, cgPos, sourceRect, scale, scale, (float)(int)alpha / 255f, 0, blend, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.GalleryWindow);
					});
				}
				else
				{
					int scale2 = cg.scale;
					Vec2 vec2 = ((!cg.HasCustomWindowSize) ? DISPLAY_SIZE : cg.overrideWindowSize);
					int num2 = 2 / cg.scale;
					cgPos = screenPos * num2;
					sourceRect = new Rect(hScroll.Value, vScroll.Value, vec2.X * num2, vec2.Y * num2);
					Game1.gMan.MainBlit(textureName, cgPos, sourceRect, (float)(int)alpha / 255f, 0, GraphicsManager.BlendMode.Normal, scale2);
					cg.additionalLayers?.ForEach(delegate(string layer)
					{
						string textureName3 = "pictures/" + layer;
						Game1.gMan.MainBlit(textureName3, cgPos, sourceRect, (float)(int)alpha / 255f, 0, GraphicsManager.BlendMode.Normal, cg.scale, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.GalleryWindow);
					});
				}
			}
			else
			{
				string tWMLocString = Game1.languageMan.GetTWMLocString("gallery_empty_message");
				GameColor gColor = theme.Primary(alpha);
				Vec2 vec3 = base.ContentsSize / 2;
				Game1.gMan.TextBlitCentered(GraphicsManager.FontType.OS, vec3 + screenPos, tWMLocString, gColor);
			}
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is GalleryWindow;
		}

		public override void AdjustForNewScreenResolution(Vec2 oldScreenSize, Vec2 newScreenSize)
		{
			base.AdjustForNewScreenResolution(oldScreenSize, newScreenSize);
			ConfigCurrentCg();
		}

		public void AddImage(string imageId, bool doSort = true)
		{
			if (isGlitched && doSort)
			{
				return;
			}
			if (GalleryInfo.TryGetValue(imageId, out var value))
			{
				if (value.additionalLayers == null || value.additionalLayers.Count <= 1)
				{
					imageList.Add(value);
				}
				else
				{
					int num = 0;
					foreach (string additionalLayer in value.additionalLayers)
					{
						GalleryInfo item = new GalleryInfo(value.imageId, new List<string> { additionalLayer }, value.displayName, value.displayOrder + num, value.scale);
						imageList.Add(item);
						num++;
					}
				}
			}
			if (doSort)
			{
				imageList.Sort((GalleryInfo a, GalleryInfo b) => a.displayOrder - b.displayOrder);
				List<(string, string)> items = imageList.Select((GalleryInfo i) => (imageId: i.imageId, displayName: i.displayName)).ToList();
				imageChooser.SetItems(items, imageChooser.Value);
			}
			imageChooser.Disabled = false;
		}

		private void ConfigCurrentCg()
		{
			Game1.gMan.clearTextureCache(TextureCache.CacheType.GalleryWindow);
			GalleryInfo currentCg = GetCurrentCg();
			Vec2 contentsSize = base.ContentsSize;
			Vec2 imageSize = DISPLAY_SIZE;
			if (currentCg != null)
			{
				string textureName = "pictures/" + currentCg.imageId;
				imageSize = Game1.gMan.TextureSize(textureName, TextureCache.CacheType.GalleryWindow);
				string text = Game1.languageMan.GetCgNameLocString(currentCg.imageId, currentCg.displayName);
				if (isGlitched)
				{
					text = "???";
				}
				base.WindowTitle = Game1.languageMan.GetTWMLocString(baseTitle) + " - " + text;
			}
			else
			{
				hScroll.Active = false;
				vScroll.Active = false;
				imageChooser.Disabled = true;
			}
			if (!base.IsMaximized)
			{
				ConfigCgWindowed(currentCg, imageSize);
			}
			else
			{
				ConfigCgFullscreen(currentCg);
			}
			if (contentsSize.X != base.ContentsSize.X || contentsSize.Y != base.ContentsSize.Y)
			{
				PositionScrollbars();
			}
		}

		private void ConfigCgWindowed(GalleryInfo cg, Vec2 imageSize)
		{
			Vec2 vec = DISPLAY_SIZE;
			if (cg != null)
			{
				if (cg.HasCustomWindowSize)
				{
					vec = cg.overrideWindowSize;
				}
				int num = 2 / cg.scale;
				imageSize /= num;
				if (cg.oversize)
				{
					imageSize = DISPLAY_SIZE;
				}
			}
			Vec2 vec2 = new Vec2(0, 24);
			Vec2 vec3 = ConfigScrollbarsFromImageSize(cg, imageSize, vec);
			base.ContentsSize = vec + vec2 + vec3;
		}

		private void ConfigCgFullscreen(GalleryInfo cg)
		{
			base.ContentsSize = Game1.windowMan.ScreenSize;
			(Vec2, Vec2, Vec2, float) tuple = ImageMetricsBestFit(cg);
			ConfigScrollbarsFromImageSize(cg, tuple.Item1, tuple.Item2);
		}

		private Vec2 ConfigScrollbarsFromImageSize(GalleryInfo cg, Vec2 imageSize, Vec2 viewportSize)
		{
			Vec2 zero = Vec2.Zero;
			if (cg != null && imageSize.X > viewportSize.X)
			{
				hScroll.Active = true;
				hScroll.Max = imageSize.X - viewportSize.X;
				hScroll.Value = (int)(cg.scrollbarX * (float)hScroll.Max);
				hScroll.Increment = Math.Max(1, hScroll.Max / 10);
				zero.Y += 16;
			}
			else
			{
				hScroll.Active = false;
				hScroll.Value = 0;
			}
			if (cg != null && imageSize.Y > viewportSize.Y)
			{
				vScroll.Active = true;
				vScroll.Max = imageSize.Y - viewportSize.Y;
				vScroll.Value = (int)(cg.scrollbarY * (float)vScroll.Max);
				vScroll.Increment = Math.Max(1, vScroll.Max / 10);
				zero.X += 16;
			}
			else
			{
				vScroll.Active = false;
				vScroll.Value = 0;
			}
			return zero;
		}

		private void PositionScrollbars()
		{
			if (base.IsMaximized)
			{
				vScroll.ScrollTriggerZone = new Rect(0, 0, base.ContentsSize.X, base.ContentsSize.Y - 40);
				imageChooser.Position = new Vec2((base.ContentsSize.X - 240) / 2, base.ContentsSize.Y - 40 + 4);
				vScroll.Length = 320;
				vScroll.Position = new Vec2(base.ContentsSize.X - 16, (base.ContentsSize.Y - vScroll.Length) / 2);
				hScroll.Length = 320;
				hScroll.Position = new Vec2((base.ContentsSize.X - hScroll.Length) / 2, base.ContentsSize.Y - 16);
				bExitFullscreen.Position = new Vec2(250, 0) + imageChooser.Position;
			}
			else
			{
				vScroll.ScrollTriggerZone = new Rect(0, 0, base.ContentsSize.X, base.ContentsSize.Y - 24);
				vScroll.Position = new Vec2(base.ContentsSize.X - 16, 0);
				vScroll.Length = (hScroll.Active ? hScroll.Position.Y : base.ContentsSize.Y);
				hScroll.Position = new Vec2(0, base.ContentsSize.Y - 16);
				hScroll.Length = (vScroll.Active ? vScroll.Position.X : base.ContentsSize.X);
				imageChooser.Position = chooserPos;
			}
		}

		private GalleryInfo GetCurrentCg()
		{
			int ındex = imageChooser.Index;
			if (ındex >= 0 && ındex < imageList.Count)
			{
				return imageList[ındex];
			}
			return null;
		}

		private Vec2 GetFullscreenDrawSpace(GalleryInfo cg)
		{
			Vec2 vec = new Vec2(base.ContentsSize.X, base.ContentsSize.Y - 40);
			if (cg != null && cg.fitHorizontal)
			{
				vec.X -= 16;
			}
			return vec * 2;
		}

		private (Vec2 imgSize, Vec2 sourceSize, Vec2 destSize, float scale) ImageMetricsBestFit(GalleryInfo cg)
		{
			Vec2 fullscreenDrawSpace = GetFullscreenDrawSpace(cg);
			float num = (float)fullscreenDrawSpace.X / (float)fullscreenDrawSpace.Y;
			Vec2 item = ((cg != null) ? Game1.gMan.TextureSize("pictures/" + cg.imageId, TextureCache.CacheType.GalleryWindow) : DISPLAY_SIZE);
			float num2 = (float)item.X / (float)item.Y;
			float num3;
			Vec2 item2 = default(Vec2);
			if (cg == null || cg.fitHorizontal || ((double)num < 1.33 && (double)num2 < 1.4))
			{
				num3 = (float)fullscreenDrawSpace.X / (float)item.X;
				item2.X = item.X;
				item2.Y = (int)Math.Min(item.Y, (float)fullscreenDrawSpace.Y / num3);
			}
			else
			{
				num3 = (float)fullscreenDrawSpace.Y / (float)item.Y;
				item2.X = (int)Math.Min(item.X, (float)fullscreenDrawSpace.X / num3);
				item2.Y = item.Y;
			}
			return new ValueTuple<Vec2, Vec2, Vec2, float>(item3: new Vec2((int)((float)item.X * num3), (int)((float)item.Y * num3)), item1: item, item2: item2, item4: num3);
		}
	}
}

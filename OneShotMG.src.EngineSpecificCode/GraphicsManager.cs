using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OneShotMG.src.Util;

namespace OneShotMG.src.EngineSpecificCode
{
	public class GraphicsManager
	{
		public enum BlendMode
		{
			None,
			Normal,
			Additive,
			SpecialTransition,
			LinearStretch,
			Dither
		}

		public enum FontType
		{
			OS,
			Game,
			GameSmall
		}

		public class GameFont
		{
			public SpriteFont spriteFont;

			public HashSet<char> supportedCharacters;

			public string name;
		}

		public enum Resolution
		{
			Res1024x600,
			Res1280x720,
			Res1600x900,
			Res1920x1080
		}

		public enum TextBlitMode
		{
			Normal,
			OnlyText,
			OnlyGlyphes
		}

		public const int GAME_PX_WIDTH = 320;

		public const int GAME_PX_HEIGHT = 240;

		public const int GAME_PX_SCALE = 2;

		public const string NO_GLYPH_FOUND = "[?]";

		private BlendMode currentBlendMode;

		private GameTone previousUsedTone = GameTone.White;

		private int previousUsedHue;

		private RenderTarget2D oneshotWindowScreen;

		private RenderTarget2D twmTempScreen;

		private RenderTarget2D mapTransitionTexture;

		private GraphicsDeviceManager graphics;

		private SpriteBatch spriteBatch;

		private Game monoGame;

		private ContentManager baseTextureContent;

		private Effect hueEffect;

		private Effect transitionEffect;

		private Effect ditherEffect;

		public string TransitionTextureName = "transitions/mosaic";

		public string DitherTextureName = "transitions/BayerDither8x8";

		private TextureCache textureCache;

		private Dictionary<FontType, GameFont> fontCache;

		private ContentManager fontContentManager;

		private const string BACKUP_FONT1_NAME = "noto_sans";

		private const string BACKUP_FONT1_SMALL_NAME = "noto_sans_small";

		private const string BACKUP_FONT2_NAME = "noto_sans_jp";

		private const string BACKUP_FONT2_SMALL_NAME = "noto_sans_jp_small";

		private GameFont backupFont1;

		private GameFont backupFont2;

		private GameFont backupFont1small;

		private GameFont backupFont2small;

		private Texture2D colorBoxTexture;

		private HashSet<string> existingLightmaps;

		public bool SmoothScaleWindow;

		public int Gamma = 100;

		public const int GAMMA_MIN = 50;

		public const int GAMMA_MAX = 150;

		public static readonly Vec2 RES_600P = new Vec2(1024, 600);

		public static readonly Vec2 RES_720P = new Vec2(1280, 720);

		public static readonly Vec2 RES_900P = new Vec2(1600, 900);

		public static readonly Vec2 RES_1080P = new Vec2(1920, 1080);

		public static readonly Vec2 RES_720P_4x3 = new Vec2(960, 720);

		public static readonly Vec2 RES_900P_4x3 = new Vec2(1200, 900);

		public static readonly Vec2 RES_1080P_4x3 = new Vec2(1440, 1080);

		public static readonly Vec2 RES_800P = new Vec2(1280, 800);

		public Action OnUpdateDrawScreenSize;

		public TempTextureManager TempTexMan { get; private set; }

		public Vec2 DrawScreenSize
		{
			get
			{
				if (Game1.bootMan != null && Game1.bootMan.ForceBootManagerScreenSize())
				{
					return GetBootScreenSize();
				}
				if (IsFullscreen() && Game1.windowMan != null && Game1.windowMan.IsOneshotMaximized())
				{
					return GetDrawResolutionForOneshotMaximized();
				}
				return TWMDrawScreenSize;
			}
			set
			{
				TWMDrawScreenSize = value;
			}
		}

		public Vec2 TWMDrawScreenSize { get; private set; } = RES_720P;

		public List<Vec2> AvailableResolutions { get; private set; }

		public Vec2 OutputScreenSize { get; private set; }

		public bool vsyncEnabled
		{
			get
			{
				return graphics.SynchronizeWithVerticalRetrace;
			}
			set
			{
				if (value != graphics.SynchronizeWithVerticalRetrace)
				{
					graphics.SynchronizeWithVerticalRetrace = value;
					graphics.ApplyChanges();
				}
			}
		}

		public Vec2 ConvertWindowPosToLogicalPos(Vec2 windowPos)
		{
			return new Vec2(windowPos.X * DrawScreenSize.X / OutputScreenSize.X, windowPos.Y * DrawScreenSize.Y / OutputScreenSize.Y);
		}

		public GraphicsManager(Game mGame)
		{
			monoGame = mGame;
			graphics = new GraphicsDeviceManager(monoGame);
			TempTexMan = new TempTextureManager(mGame);
			graphics.SynchronizeWithVerticalRetrace = false;
			SetDrawResolution(RES_720P);
			OutputScreenSize = RES_720P;
			graphics.PreferredBackBufferWidth = OutputScreenSize.X;
			graphics.PreferredBackBufferHeight = OutputScreenSize.Y;
			graphics.HardwareModeSwitch = false;
			graphics.ApplyChanges();
			generateAvailableResolutions();
			monoGame.Window.AllowUserResizing = false;
			monoGame.Window.ClientSizeChanged += OnResize;
			spriteBatch = new SpriteBatch(monoGame.GraphicsDevice);
			fontCache = new Dictionary<FontType, GameFont>();
			oneshotWindowScreen = new RenderTarget2D(monoGame.GraphicsDevice, 640, 480, mipMap: false, monoGame.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);
			Vec2 maxScreenAreaFromAvailableResolutions = getMaxScreenAreaFromAvailableResolutions();
			CreateTWMTempScreen(maxScreenAreaFromAvailableResolutions);
			mapTransitionTexture = new RenderTarget2D(monoGame.GraphicsDevice, 640, 480, mipMap: false, monoGame.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
			baseTextureContent = new ContentManager(monoGame.Content.ServiceProvider, monoGame.Content.RootDirectory);
			textureCache = new TextureCache(monoGame);
			colorBoxTexture = baseTextureContent.Load<Texture2D>("white16x16");
			hueEffect = baseTextureContent.Load<Effect>("shaders/HueShader");
			transitionEffect = baseTextureContent.Load<Effect>("shaders/TransitionShader");
			ditherEffect = baseTextureContent.Load<Effect>("shaders/DitherShader");
			fontContentManager = new ContentManager(monoGame.Content.ServiceProvider, monoGame.Content.RootDirectory);
			existingLightmaps = new HashSet<string>();
			string[] files = Directory.GetFiles(monoGame.Content.RootDirectory + "/lightmaps");
			foreach (string obj in files)
			{
				string text = obj.Substring(obj.LastIndexOfAny(new char[2] { '\\', '/' }) + 1);
				text = text.Substring(0, text.IndexOf(".xnb"));
				existingLightmaps.Add("lightmaps/" + text);
			}
			backupFont1 = createGameFont(baseTextureContent.Load<SpriteFont>("noto_sans"), "noto_sans");
			backupFont2 = createGameFont(baseTextureContent.Load<SpriteFont>("noto_sans_jp"), "noto_sans_jp");
			backupFont1small = createGameFont(baseTextureContent.Load<SpriteFont>("noto_sans_small"), "noto_sans_small");
			backupFont2small = createGameFont(baseTextureContent.Load<SpriteFont>("noto_sans_jp_small"), "noto_sans_jp_small");
			if (Game1.steamMan.IsOnSteamDeck)
			{
				AddResolution(RES_800P);
				SetDrawResolution(RES_800P);
				SetFullscreen(fullscreen: true);
			}
		}

		public bool TextureExists(string path, TextureCache.CacheType cacheType)
		{
			return textureCache.TextureExists(path, cacheType);
		}

		private Vec2 getMaxScreenAreaFromAvailableResolutions()
		{
			Vec2 rES_1080P = RES_1080P;
			foreach (Vec2 availableResolution in AvailableResolutions)
			{
				if (availableResolution.X > rES_1080P.X)
				{
					rES_1080P.X = availableResolution.X;
				}
				if (availableResolution.Y > rES_1080P.Y)
				{
					rES_1080P.Y = availableResolution.Y;
				}
			}
			return rES_1080P;
		}

		public void AddResolution(Vec2 newRes)
		{
			if (!AvailableResolutions.Contains(newRes))
			{
				AvailableResolutions.Add(newRes);
				if (twmTempScreen != null && (newRes.X > twmTempScreen.Width || newRes.Y > twmTempScreen.Height))
				{
					Vec2 size = new Vec2(Math.Max(newRes.X, twmTempScreen.Width), Math.Max(newRes.Y, twmTempScreen.Height));
					CreateTWMTempScreen(size);
				}
			}
		}

		private void CreateTWMTempScreen(Vec2 size)
		{
			if (twmTempScreen != null)
			{
				twmTempScreen.Dispose();
			}
			twmTempScreen = new RenderTarget2D(monoGame.GraphicsDevice, size.X, size.Y, mipMap: false, monoGame.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
		}

		public void SetDemoDefaults()
		{
			if (Game1.steamMan.IsOnSteamDeck)
			{
				SetDrawResolution(RES_800P);
				SetFullscreen(fullscreen: true);
			}
			else
			{
				SetDrawResolution(RES_720P);
				SetFullscreen(fullscreen: false);
			}
			Gamma = 100;
		}

		private void generateAvailableResolutions()
		{
			AvailableResolutions = new List<Vec2>();
			AvailableResolutions.Add(RES_720P);
			AvailableResolutions.Add(RES_900P);
			AvailableResolutions.Add(RES_1080P);
			AvailableResolutions.Add(RES_720P_4x3);
			AvailableResolutions.Add(RES_900P_4x3);
			AvailableResolutions.Add(RES_1080P_4x3);
			bool flag = false;
			Screen[] allScreens = Screen.AllScreens;
			foreach (Screen screen in allScreens)
			{
				double num = (double)screen.Bounds.Width / (double)screen.Bounds.Height;
				int[] array;
				if (num >= 1.0)
				{
					if (screen.Bounds.Height <= RES_1080P.Y && screen.Bounds.Width <= 4096)
					{
						Vec2 newRes = new Vec2(screen.Bounds.Width, screen.Bounds.Height);
						AddResolution(newRes);
					}
					else if (screen.Bounds.Height / 2 >= RES_720P.Y)
					{
						Vec2 newRes2 = new Vec2(screen.Bounds.Width / 2, screen.Bounds.Height / 2);
						AddResolution(newRes2);
					}
					array = new int[3] { 720, 900, 1080 };
					foreach (int num2 in array)
					{
						int x = (int)Math.Round((double)num2 * num);
						Vec2 vec = new Vec2(x, num2);
						AddResolution(vec);
						if (num2 == 720 && !flag)
						{
							SetDrawResolution(vec);
							flag = true;
						}
					}
					continue;
				}
				if (screen.Bounds.Width <= RES_1080P.Y && screen.Bounds.Height <= 4096)
				{
					Vec2 newRes3 = new Vec2(screen.Bounds.Width, screen.Bounds.Height);
					AddResolution(newRes3);
				}
				else if (screen.Bounds.Width / 2 >= RES_720P.Y)
				{
					Vec2 newRes4 = new Vec2(screen.Bounds.Width / 2, screen.Bounds.Height / 2);
					AddResolution(newRes4);
				}
				array = new int[3] { 720, 900, 1080 };
				foreach (int num3 in array)
				{
					int y = (int)Math.Round((double)num3 / num);
					Vec2 vec2 = new Vec2(num3, y);
					AddResolution(vec2);
					if (num3 == 720 && !flag)
					{
						SetDrawResolution(vec2);
						flag = true;
					}
				}
			}
		}

		public bool IsFullscreen()
		{
			return graphics.IsFullScreen;
		}

		public void SetFullscreen(bool fullscreen)
		{
			if (!IsFullscreen() == fullscreen)
			{
				ToggleFullscreen();
			}
		}

		public bool DoesLightmapExist(string lightmapName)
		{
			return existingLightmaps.Contains(lightmapName);
		}

		internal void ToggleFullscreen()
		{
			if (!graphics.IsFullScreen)
			{
				graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
				graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
				graphics.ApplyChanges();
			}
			else
			{
				graphics.PreferredBackBufferWidth = TWMDrawScreenSize.X;
				graphics.PreferredBackBufferHeight = TWMDrawScreenSize.Y;
			}
			graphics.ToggleFullScreen();
			updateOutputScreenSize();
		}

		private void updateOutputScreenSize()
		{
			OutputScreenSize = new Vec2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
		}

		public void SetDrawResolution(Vec2 newRes)
		{
			if (newRes.X < 640)
			{
				newRes.X = 640;
			}
			if (newRes.Y < 480)
			{
				newRes.Y = 480;
			}
			DrawScreenSize = newRes;
			if (!IsFullscreen())
			{
				graphics.PreferredBackBufferWidth = TWMDrawScreenSize.X;
				graphics.PreferredBackBufferHeight = TWMDrawScreenSize.Y;
				graphics.ApplyChanges();
				updateOutputScreenSize();
			}
			OnUpdateDrawScreenSize?.Invoke();
		}

		public void LoadFont(FontType fontType)
		{
			if (fontCache.TryGetValue(fontType, out var _))
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, $"tried to load already loaded font: {fontType}");
				return;
			}
			string text = FontTypeToFontName(fontType);
			SpriteFont font = baseTextureContent.Load<SpriteFont>(text);
			fontCache.Add(fontType, createGameFont(font, text));
		}

		private GameFont createGameFont(SpriteFont font, string fontName)
		{
			font.DefaultCharacter = '?';
			HashSet<char> hashSet = new HashSet<char>();
			foreach (char character in font.Characters)
			{
				hashSet.Add(character);
			}
			return new GameFont
			{
				spriteFont = font,
				supportedCharacters = hashSet,
				name = fontName
			};
		}

		public void clearTextureCache(TextureCache.CacheType cacheType)
		{
			textureCache.UnloadCache(cacheType);
		}

		public bool IsPixelSolid(string textureName, Vec2 pixOnTexture, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			Texture2D texture = textureCache.GetTexture(textureName, cacheType);
			if (pixOnTexture.X < 0 || pixOnTexture.Y < 0 || pixOnTexture.X >= texture.Width || pixOnTexture.Y >= texture.Height)
			{
				return false;
			}
			Color[] array = new Color[1];
			texture.GetData(0, new Rectangle(pixOnTexture.X, pixOnTexture.Y, 1, 1), array, 0, 1);
			return array[0].A > 128;
		}

		public void MainBlit(string textureName, Vec2 pixelPos, Rect srcRect, GameColor gameColor, int hue = 0, BlendMode blendMode = BlendMode.Normal, int scale = 2, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			float alpha = (float)(int)gameColor.a / 255f;
			float red = (float)(int)gameColor.r / 255f;
			float green = (float)(int)gameColor.g / 255f;
			float blue = (float)(int)gameColor.b / 255f;
			MainBlit(textureName, pixelPos, srcRect, alpha, hue, blendMode, scale, GameTone.Zero, red, green, blue, 0f, cacheType);
		}

		public void MainBlit(string textureName, Vec2 pixelPos, GameColor gameColor, int hue = 0, BlendMode blendMode = BlendMode.Normal, int scale = 2, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			float alpha = (float)(int)gameColor.a / 255f;
			float red = (float)(int)gameColor.r / 255f;
			float green = (float)(int)gameColor.g / 255f;
			float blue = (float)(int)gameColor.b / 255f;
			MainBlit(textureName, pixelPos, alpha, hue, blendMode, scale, GameTone.Zero, red, green, blue, cacheType);
		}

		public void MainBlit(TempTexture tempTexture, Vec2 pixelPos, GameColor gameColor, int hue = 0, BlendMode blendMode = BlendMode.Normal, int scale = 2, bool xCentered = false)
		{
			if (tempTexture != null && tempTexture.isValid)
			{
				tempTexture.KeepAlive();
				Texture2D renderTarget = tempTexture.renderTarget;
				if (xCentered)
				{
					pixelPos.X -= renderTarget.Width / 2;
				}
				float alpha = (float)(int)gameColor.a / 255f;
				float red = (float)(int)gameColor.r / 255f;
				float green = (float)(int)gameColor.g / 255f;
				float blue = (float)(int)gameColor.b / 255f;
				pixelPos *= scale;
				MainBlit(srcRect: new Rect(0, 0, renderTarget.Width, renderTarget.Height), tex: renderTarget, pixelPos: pixelPos, xScale: scale, yScale: scale, alpha: alpha, hue: hue, blendMode: blendMode, tone: GameTone.Zero, red: red, green: green, blue: blue);
			}
		}

		public void MainBlitStretch(TempTexture tempTexture, Vec2 pixelPos, GameColor gameColor, int hue = 0, BlendMode blendMode = BlendMode.Normal, float xScale = 1f, float yScale = 1f, bool xCentered = false)
		{
			if (tempTexture != null && tempTexture.isValid)
			{
				tempTexture.KeepAlive();
				Texture2D renderTarget = tempTexture.renderTarget;
				if (xCentered)
				{
					pixelPos.X -= renderTarget.Width / 2;
				}
				float alpha = (float)(int)gameColor.a / 255f;
				float red = (float)(int)gameColor.r / 255f;
				float green = (float)(int)gameColor.g / 255f;
				float blue = (float)(int)gameColor.b / 255f;
				MainBlit(srcRect: new Rect(0, 0, renderTarget.Width, renderTarget.Height), tex: renderTarget, pixelPos: pixelPos, xScale: xScale, yScale: yScale, alpha: alpha, hue: hue, blendMode: blendMode, tone: GameTone.Zero, red: red, green: green, blue: blue);
			}
		}

		public void MainBlit(string textureName, Vec2 pixelPos, float alpha = 1f, int hue = 0, BlendMode blendMode = BlendMode.Normal, int scale = 2, GameTone tone = default(GameTone), float red = 1f, float green = 1f, float blue = 1f, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			Texture2D texture = textureCache.GetTexture(textureName, cacheType);
			MainBlit(textureName, pixelPos, new Rect(0, 0, texture.Width, texture.Height), alpha, hue, blendMode, scale, tone, red, green, blue, 0f, cacheType);
		}

		public void MainBlit(string textureName, Vec2 pixelPos, Rect srcRect, float alpha = 1f, int hue = 0, BlendMode blendMode = BlendMode.Normal, int scale = 2, GameTone tone = default(GameTone), float red = 1f, float green = 1f, float blue = 1f, float angle = 0f, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			pixelPos.X *= scale;
			pixelPos.Y *= scale;
			MainBlit(textureName, pixelPos, srcRect, scale, scale, alpha, hue, blendMode, tone, red, green, blue, angle, cacheType);
		}

		public void MainBlit(string textureName, Vec2 pixelPos, Rect srcRect, float xScale, float yScale, float alpha = 1f, int hue = 0, BlendMode blendMode = BlendMode.Normal, GameTone tone = default(GameTone), float red = 1f, float green = 1f, float blue = 1f, float angle = 0f, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			Texture2D texture = textureCache.GetTexture(textureName, cacheType);
			MainBlit(texture, pixelPos, srcRect, xScale, yScale, alpha, hue, blendMode, tone, red, green, blue, angle);
		}

		public void MainBlit(Texture2D tex, Vec2 pixelPos, Rect srcRect, float xScale, float yScale, float alpha = 1f, int hue = 0, BlendMode blendMode = BlendMode.Normal, GameTone tone = default(GameTone), float red = 1f, float green = 1f, float blue = 1f, float angle = 0f)
		{
			if (!tone.Equals(previousUsedTone) || previousUsedHue != hue)
			{
				applyBlendMode(BlendMode.None, xScale);
				previousUsedTone = tone;
				previousUsedHue = hue;
			}
			applyBlendMode(blendMode, xScale);
			Vector2 origin = new Vector2((float)srcRect.W / 2f, (float)srcRect.H / 2f);
			Rectangle destinationRectangle = new Rectangle(pixelPos.X, pixelPos.Y, (int)((float)srcRect.W * xScale), (int)((float)srcRect.H * yScale));
			destinationRectangle.X += destinationRectangle.Width / 2;
			destinationRectangle.Y += destinationRectangle.Height / 2;
			if (blendMode != BlendMode.Dither)
			{
				float value = (float)Math.PI / 180f * (float)hue;
				hueEffect.Parameters["hueShiftInRads"].SetValue(value);
				hueEffect.Parameters["tone"].SetValue(new Vector3((float)tone.r / 255f, (float)tone.g / 255f, (float)tone.b / 255f));
				hueEffect.CurrentTechnique.Passes[0].Apply();
			}
			spriteBatch.Draw(tex, destinationRectangle, new Rectangle(srcRect.X, srcRect.Y, srcRect.W, srcRect.H), new Color(red, green, blue, alpha), angle, origin, SpriteEffects.None, 0f);
		}

		public void LoadTexture(string texture, TextureCache.CacheType cache)
		{
			textureCache.LoadTexture(texture, cache);
		}

		private void applyBlendMode(BlendMode mode, float scale = 1f)
		{
			if (currentBlendMode != mode)
			{
				currentBlendMode = mode;
				spriteBatch.End();
				switch (currentBlendMode)
				{
				case BlendMode.None:
				case BlendMode.Normal:
					spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, hueEffect, null);
					hueEffect.CurrentTechnique.Passes[0].Apply();
					hueEffect.Parameters["gamma"].SetValue((float)Gamma / 100f);
					break;
				case BlendMode.LinearStretch:
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, null, null, null, null);
					hueEffect.CurrentTechnique.Passes[0].Apply();
					hueEffect.Parameters["gamma"].SetValue((float)Gamma / 100f);
					break;
				case BlendMode.Additive:
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, null, null, null, null);
					hueEffect.CurrentTechnique.Passes[0].Apply();
					hueEffect.Parameters["gamma"].SetValue((float)Gamma / 100f);
					break;
				case BlendMode.SpecialTransition:
				{
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
					Texture2D texture2 = textureCache.GetTexture(TransitionTextureName, TextureCache.CacheType.Game);
					transitionEffect.Parameters["transMap"].SetValue(texture2);
					transitionEffect.CurrentTechnique.Passes[0].Apply();
					break;
				}
				case BlendMode.Dither:
				{
					spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
					Texture2D texture = textureCache.GetTexture(DitherTextureName, TextureCache.CacheType.BootScreen);
					ditherEffect.Parameters["ditherTx"].SetValue(texture);
					ditherEffect.Parameters["screenSize"].SetValue(new Vector2(DrawScreenSize.X, DrawScreenSize.Y));
					ditherEffect.Parameters["ditherSize"].SetValue(new Vector2(texture.Bounds.Width, texture.Bounds.Height));
					ditherEffect.Parameters["scale"].SetValue(4f);
					ditherEffect.Parameters["flipYPos"].SetValue(useTempTextureForOSDraw() ? 1f : 0f);
					ditherEffect.CurrentTechnique.Passes[0].Apply();
					break;
				}
				}
			}
		}

		public void FillScreen(string textureName, Vec2 pixelPos, Rect srcRect, float alpha = 1f, BlendMode blendMode = BlendMode.Normal, GameTone tone = default(GameTone), int hue = 0, int scale = 2, Vec2 screenSize = default(Vec2), TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			if (screenSize.X == 0 || screenSize.Y == 0)
			{
				screenSize.X = 320;
				screenSize.Y = 240;
			}
			for (int i = pixelPos.X; i < screenSize.X * 2 / scale; i += srcRect.W)
			{
				for (int j = pixelPos.Y; j < screenSize.Y * 2 / scale; j += srcRect.H)
				{
					MainBlit(textureName, new Vec2(i, j), srcRect, alpha, hue, blendMode, scale, tone, 1f, 1f, 1f, 0f, cacheType);
				}
			}
		}

		public void TextBlitCentered(FontType fontType, Vec2 pixelPos, string text, GameColor gColor, BlendMode blendMode = BlendMode.Normal, int scale = 2, bool checkForGlyphes = false, TextBlitMode textMode = TextBlitMode.Normal, InputManager.GlyphMode glyphMode = InputManager.GlyphMode.None)
		{
			Vec2 vec = TextSize(fontType, text, checkForGlyphes, osFontNoResize: false, glyphMode);
			pixelPos.X -= vec.X / 2;
			TextBlit(fontType, pixelPos, text, gColor, blendMode, scale, textMode, glyphMode);
		}

		public void TextBlit(FontType fontType, Vec2 pixelPos, string text, GameColor gColor, BlendMode blendMode = BlendMode.Normal, int scale = 2, TextBlitMode textMode = TextBlitMode.Normal, InputManager.GlyphMode glyphMode = InputManager.GlyphMode.None)
		{
			if (fontType == FontType.OS && Game1.languageMan.GetCurrentFontOSScale() == 1 && scale >= 2)
			{
				scale /= 2;
				pixelPos.X *= 2;
				pixelPos.Y *= 2;
			}
			string[] array = Regex.Split(text, InputManager.GlyphPattern);
			if (array.Length > 1)
			{
				TextBlitWithGlyphs(fontType, pixelPos, array, gColor, blendMode, scale, textMode, glyphMode);
			}
			else if (textMode != TextBlitMode.OnlyGlyphes)
			{
				TextBlitPlain(fontType, pixelPos, text, gColor, blendMode, scale);
			}
		}

		private void TextBlitWithGlyphs(FontType fontType, Vec2 pixelPos, string[] parts, GameColor gColor, BlendMode blendMode, int scale, TextBlitMode textMode, InputManager.GlyphMode glyphMode)
		{
			bool flag = false;
			foreach (string text in parts)
			{
				if (flag)
				{
					List<InputManager.ButtonGlyphInfo> glyph = Game1.inputMan.GetGlyph(text, glyphMode);
					if (glyph == null || glyph.Count <= 0)
					{
						pixelPos.X += TextBlitPlain(fontType, pixelPos, "[?]", gColor, blendMode, scale);
					}
					else
					{
						foreach (InputManager.ButtonGlyphInfo item in glyph)
						{
							Vec2 vec = TextureSize(item.texturePath, item.cacheType);
							int num = vec.Y / vec.X;
							if (num < 1)
							{
								num = 1;
							}
							int num2 = Game1.GlobalFrameCounter / 20 % num;
							Rect rect = new Rect(0, vec.X * num2, vec.X, vec.X);
							if (item.cacheType == TextureCache.CacheType.SteamGlyphes)
							{
								rect.X = 0;
								rect.Y = 0;
								vec = new Vec2(32, 32);
							}
							Vec2 vec2 = pixelPos;
							switch (fontType)
							{
							default:
								if (Game1.languageMan.GetCurrentFontOSScale() == 2)
								{
									vec2.Y -= 6;
								}
								else
								{
									vec2.Y -= 12;
								}
								break;
							case FontType.Game:
								vec2.Y -= 24;
								break;
							case FontType.GameSmall:
								vec2.Y -= 8;
								break;
							}
							if (scale != 2)
							{
								vec2 /= 2;
							}
							if (textMode != TextBlitMode.OnlyText)
							{
								if (item.cacheType == TextureCache.CacheType.SteamGlyphes)
								{
									float xScale = 64f / (float)rect.W;
									float yScale = 64f / (float)rect.H;
									string texturePath = item.texturePath;
									Vec2 pixelPos2 = vec2 * 2;
									Rect srcRect = rect;
									float alpha = (float)(int)gColor.a / 255f;
									TextureCache.CacheType cacheType = item.cacheType;
									MainBlit(texturePath, pixelPos2, srcRect, xScale, yScale, alpha, 0, BlendMode.LinearStretch, default(GameTone), 1f, 1f, 1f, 0f, cacheType);
								}
								else
								{
									MainBlit(item.texturePath, vec2, rect, gColor, 0, blendMode, 2, item.cacheType);
								}
							}
							pixelPos.X += vec.X * (2 / scale);
						}
					}
				}
				else if (textMode != TextBlitMode.OnlyGlyphes)
				{
					pixelPos.X += TextBlitPlain(fontType, pixelPos, text, gColor, blendMode, scale);
				}
				else
				{
					pixelPos.X += TextSize(fontType, text, checkForGlyphes: false, osFontNoResize: true).X;
				}
				flag = !flag;
			}
		}

		private int TextBlitPlain(FontType fontType, Vec2 pixelPos, string text, GameColor gColor, BlendMode blendMode, int scale)
		{
			int num = 0;
			for (int i = 0; i < text.Length; i++)
			{
				char textCharacter = text[i];
				string text2 = textCharacter.ToString();
				Vec2 vec = TextSize(fontType, text2, checkForGlyphes: false, osFontNoResize: true);
				drawText(fontType, pixelPos, textCharacter, gColor, blendMode, scale);
				pixelPos.X += vec.X;
				num += vec.X;
			}
			return num;
		}

		private void drawText(FontType fontType, Vec2 pixelPos, char textCharacter, GameColor gColor, BlendMode blendMode = BlendMode.Normal, int scale = 2)
		{
			GameFont fontForChar = getFontForChar(fontType, textCharacter);
			switch (fontType)
			{
			case FontType.OS:
				switch (fontForChar.name)
				{
				case "higashi_ome":
					pixelPos.Y += 8;
					break;
				case "noto_sans":
				case "noto_sans_jp":
					if (scale == 2)
					{
						pixelPos *= 2;
						pixelPos.Y += 7;
						scale = 1;
					}
					else
					{
						pixelPos.Y += 8;
					}
					break;
				case "terminus":
					pixelPos.Y += 8;
					break;
				case "wqy-microhei_ko":
				case "wqy-microhei_zh_cn":
				case "wqy-microhei_zh_cht":
					pixelPos.Y += 8;
					break;
				default:
					pixelPos.Y += 5;
					break;
				case "supertext":
					break;
				}
				break;
			case FontType.GameSmall:
				switch (fontForChar.name)
				{
				case "higashi_ome_doc":
					pixelPos.Y -= 2;
					break;
				case "wqy-microhei_doc_ko":
				case "wqy-microhei_doc_zh_cn":
				case "wqy-microhei_doc_zh_cht":
					pixelPos.Y--;
					break;
				case "noto_sans_small":
				case "noto_sans_jp_small":
					pixelPos.Y++;
					break;
				}
				break;
			case FontType.Game:
			{
				string name = fontForChar.name;
				if (name == "noto_sans" || name == "noto_sans_jp")
				{
					pixelPos.Y -= 2;
				}
				break;
			}
			}
			applyBlendMode(blendMode, scale);
			SpriteBatch obj = spriteBatch;
			SpriteFont spriteFont = fontForChar.spriteFont;
			string text = textCharacter.ToString();
			Vector2 position = new Vector2(pixelPos.X * scale, pixelPos.Y * scale);
			Color color = new Color(gColor.r, gColor.g, gColor.b, gColor.a);
			float scale2 = scale;
			obj.DrawString(spriteFont, text, position, color, 0f, Vector2.Zero, scale2, SpriteEffects.None, 0f);
		}

		public Vec2 TextureSize(string textureName, TextureCache.CacheType cacheType = TextureCache.CacheType.Game)
		{
			Texture2D texture = textureCache.GetTexture(textureName, cacheType);
			return new Vec2(texture.Width, texture.Height);
		}

		private string FontTypeToFontName(FontType fontType)
		{
			switch (fontType)
			{
			case FontType.OS:
				return Game1.languageMan.GetCurrentFontOS();
			case FontType.Game:
				return Game1.languageMan.GetCurrentFontGame();
			case FontType.GameSmall:
				return Game1.languageMan.GetCurrentFontGameSmall();
			default:
				return string.Empty;
			}
		}

		private GameFont getFontForChar(FontType fontType, char c)
		{
			if (!fontCache.TryGetValue(fontType, out var value))
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, $"tried to get font '{fontType} ' without loading it first!");
				LoadFont(fontType);
				value = fontCache[fontType];
			}
			if (!value.supportedCharacters.Contains(c))
			{
				value = ((fontType == FontType.GameSmall) ? backupFont1small : backupFont1);
				if (!value.supportedCharacters.Contains(c))
				{
					value = ((fontType == FontType.GameSmall) ? backupFont2small : backupFont2);
				}
			}
			return value;
		}

		public Vec2 TextSize(FontType fontType, string text, bool checkForGlyphes = false, bool osFontNoResize = false, InputManager.GlyphMode glyphMode = InputManager.GlyphMode.None)
		{
			Vec2 zero = Vec2.Zero;
			int num = 0;
			int num2 = 0;
			if (checkForGlyphes)
			{
				MatchCollection matchCollection = Regex.Matches(text, ".*" + InputManager.GlyphPattern + ".*");
				while (matchCollection.Count > 0)
				{
					string value = matchCollection[0].Groups[1].Value;
					List<InputManager.ButtonGlyphInfo> glyph = Game1.inputMan.GetGlyph(value, glyphMode);
					if (glyph != null)
					{
						foreach (InputManager.ButtonGlyphInfo item in glyph)
						{
							Vec2 vec = TextureSize(item.texturePath, item.cacheType);
							if (item.cacheType == TextureCache.CacheType.SteamGlyphes)
							{
								vec = new Vec2(32, 32);
							}
							if (fontType == FontType.Game)
							{
								zero.X += vec.X * 2;
							}
							else
							{
								zero.X += vec.X;
							}
							if (vec.Y > zero.Y)
							{
								zero.Y = vec.Y;
							}
						}
					}
					text = text.Replace(value, string.Empty);
					matchCollection = Regex.Matches(text, ".*" + InputManager.GlyphPattern + ".*");
				}
			}
			string text2 = text;
			for (int i = 0; i < text2.Length; i++)
			{
				char c = text2[i];
				GameFont fontForChar = getFontForChar(fontType, c);
				switch (fontForChar.name)
				{
				case "noto_sans":
				case "noto_sans_jp":
					num2 = 10;
					num = 2;
					break;
				case "terminus":
					num2 = 10;
					num = 2;
					break;
				case "noto_sans_small":
				case "noto_sans_jp_small":
					num2 = 8;
					num = 1;
					break;
				case "terminus_doc":
					num2 = 8;
					num = 1;
					break;
				case "volter":
					num2 = 4;
					num = 1;
					break;
				case "higashi_ome":
					num2 = 10;
					num = 3;
					break;
				case "wqy-microhei_ko":
				case "wqy-microhei_zh_cn":
				case "wqy-microhei_zh_cht":
					num2 = 4;
					num = 2;
					break;
				case "wqy-microhei_doc_ko":
				case "wqy-microhei_doc_zh_cn":
				case "wqy-microhei_doc_zh_cht":
					num2 = 4;
					num = 2;
					break;
				default:
					num2 = 6;
					num = 1;
					break;
				}
				string text3 = c.ToString();
				Vector2 vector = fontForChar.spriteFont.MeasureString(text3);
				if (c == ' ' || c == '\u00a0')
				{
					vector.X = num2;
				}
				int num3 = (int)Math.Ceiling(vector.X) + num;
				if (fontType == FontType.OS && Game1.languageMan.GetCurrentFontOSScale() == 2 && (fontForChar.name == "noto_sans" || fontForChar.name == "noto_sans_jp"))
				{
					num3++;
					num3 /= 2;
					vector.Y /= 2f;
				}
				if (!osFontNoResize && fontType == FontType.OS && Game1.languageMan.GetCurrentFontOSScale() == 1)
				{
					num3++;
					num3 /= 2;
					vector.Y /= 2f;
				}
				zero.X += num3;
				if (vector.Y > (float)zero.Y)
				{
					zero.Y = (int)vector.Y;
				}
			}
			return zero;
		}

		public void ColorBoxBlit(Rect boxRect, GameColor gColor, BlendMode blendMode = BlendMode.Normal, int scale = 2)
		{
			if (boxRect.H > 0 && boxRect.W > 0)
			{
				applyBlendMode(blendMode, scale);
				spriteBatch.Draw(colorBoxTexture, new Rectangle(boxRect.X * scale, boxRect.Y * scale, boxRect.W * scale, boxRect.H * scale), new Color(gColor.r, gColor.g, gColor.b, gColor.a));
			}
		}

		public void LineBlit(Vec2 startPoint, Vec2 endPoint, GameColor gColor, int drawScale = 2)
		{
			int num = endPoint.X - startPoint.X;
			int num2 = endPoint.Y - startPoint.Y;
			float x = (float)Math.Sqrt(num * num + num2 * num2);
			float rotation = (float)Math.Atan2(num2, num);
			Vector2 origin = new Vector2(0f, (float)drawScale / 2f);
			Vector2 scale = new Vector2(x, 1f);
			spriteBatch.Draw(colorBoxTexture, new Vector2(startPoint.X * drawScale, startPoint.Y * drawScale), new Rectangle(0, 0, drawScale, drawScale), new Color(gColor.r, gColor.g, gColor.b, gColor.a), rotation, origin, scale, SpriteEffects.None, 0f);
		}

		public void Initialize()
		{
		}

		public void OnResize(object sender, EventArgs e)
		{
			if (!graphics.IsFullScreen)
			{
				bool flag = false;
				if (graphics.PreferredBackBufferHeight != monoGame.Window.ClientBounds.Height)
				{
					graphics.PreferredBackBufferWidth = monoGame.Window.ClientBounds.Height * 320 / 240;
					graphics.PreferredBackBufferHeight = monoGame.Window.ClientBounds.Height;
					flag = true;
				}
				else if (graphics.PreferredBackBufferWidth != monoGame.Window.ClientBounds.Width)
				{
					graphics.PreferredBackBufferWidth = monoGame.Window.ClientBounds.Width;
					graphics.PreferredBackBufferHeight = monoGame.Window.ClientBounds.Width * 240 / 320;
					flag = true;
				}
				int num = 320;
				int num2 = 240;
				if (graphics.PreferredBackBufferWidth < num || graphics.PreferredBackBufferHeight < num2)
				{
					graphics.PreferredBackBufferWidth = num;
					graphics.PreferredBackBufferHeight = num2;
					flag = true;
				}
				if (flag)
				{
					graphics.ApplyChanges();
				}
			}
		}

		public void SetUpMapTransitionFrame()
		{
			monoGame.GraphicsDevice.SetRenderTarget(mapTransitionTexture);
			monoGame.GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointWrap, null, null, null, null);
			spriteBatch.Draw(oneshotWindowScreen, new Rectangle(0, 0, 640, 480), Color.White);
			spriteBatch.End();
		}

		public void DrawMapTransitionFrame(float alpha, BlendMode blendMode)
		{
			if (Gamma != 100)
			{
				applyBlendMode(BlendMode.None);
			}
			applyBlendMode(blendMode);
			if (blendMode == BlendMode.Normal)
			{
				hueEffect.Parameters["gamma"].SetValue(1f);
				hueEffect.CurrentTechnique.Passes[0].Apply();
				spriteBatch.Draw(mapTransitionTexture, new Rectangle(0, 0, 640, 480), new Color(Color.White, alpha));
				if (Gamma != 100)
				{
					applyBlendMode(BlendMode.None);
					applyBlendMode(blendMode);
				}
				hueEffect.Parameters["gamma"].SetValue((float)Gamma / 100f);
				hueEffect.CurrentTechnique.Passes[0].Apply();
			}
			else
			{
				spriteBatch.Draw(mapTransitionTexture, new Rectangle(0, 0, 640, 480), new Color(Color.White, alpha));
			}
		}

		public void BeginDrawToOneshotTexture()
		{
			spriteBatch.End();
			currentBlendMode = BlendMode.None;
			monoGame.GraphicsDevice.SetRenderTarget(oneshotWindowScreen);
			monoGame.GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
		}

		public void EndDrawToOneshotTexture()
		{
			spriteBatch.End();
			monoGame.GraphicsDevice.SetRenderTarget(useTempTextureForOSDraw() ? twmTempScreen : null);
			monoGame.GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
			currentBlendMode = BlendMode.None;
		}

		public void DrawGlitchSegments(List<Vec2> glitchSegmentHW, List<GameColor> glitchSegmentColors)
		{
			spriteBatch.End();
			SetUpMapTransitionFrame();
			monoGame.GraphicsDevice.SetRenderTarget(oneshotWindowScreen);
			monoGame.GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
			int num = 0;
			for (int i = 0; i < glitchSegmentHW.Count; i++)
			{
				Vec2 vec = glitchSegmentHW[i];
				GameColor gameColor = glitchSegmentColors[i];
				Color color = new Color(gameColor.r, gameColor.g, gameColor.b, gameColor.a);
				spriteBatch.Draw(mapTransitionTexture, new Rectangle(0, num, 640 - vec.X, vec.Y), new Rectangle(vec.X, num, 640 - vec.X, vec.Y), color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				spriteBatch.Draw(mapTransitionTexture, new Rectangle(640 - vec.X, num, vec.X, vec.Y), new Rectangle(0, num, vec.X, vec.Y), color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
				num += vec.Y;
			}
			EndDrawToOneshotTexture();
		}

		public void DrawOneshotTexture(Rect dstRect, byte alpha, bool smoothScaling = false)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, smoothScaling ? SamplerState.LinearWrap : SamplerState.PointWrap, null, null, null, null);
			spriteBatch.Draw(oneshotWindowScreen, new Rectangle(dstRect.X, dstRect.Y, dstRect.W, dstRect.H), Color.White);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
			hueEffect.CurrentTechnique.Passes[0].Apply();
			hueEffect.Parameters["gamma"].SetValue((float)Gamma / 100f);
		}

		public void StartDrawCycle()
		{
			monoGame.GraphicsDevice.SetRenderTarget(useTempTextureForOSDraw() ? twmTempScreen : null);
			monoGame.GraphicsDevice.Clear(Color.Black);
			currentBlendMode = BlendMode.None;
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap, null, null, null, null);
		}

		public void EndDrawCycle(bool isSystemScalingSmooth, Vec2 screenSize)
		{
			spriteBatch.End();
			if (useTempTextureForOSDraw())
			{
				monoGame.GraphicsDevice.SetRenderTarget(null);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, isSystemScalingSmooth ? SamplerState.LinearWrap : SamplerState.PointWrap, null, null, null, null);
				Rectangle value = new Rectangle(0, 0, screenSize.X, screenSize.Y);
				Rectangle destinationRectangle = new Rectangle(0, 0, OutputScreenSize.X, OutputScreenSize.Y);
				spriteBatch.Draw(twmTempScreen, destinationRectangle, value, Color.White);
				spriteBatch.End();
			}
		}

		private bool useTempTextureForOSDraw()
		{
			return !DrawScreenSize.Equals(OutputScreenSize);
		}

		public void ClearFontCache()
		{
			fontCache = new Dictionary<FontType, GameFont>();
			fontContentManager.Unload();
		}

		public Vec2 GetDrawResolutionForOneshotMaximized()
		{
			if ((double)OutputScreenSize.X / (double)OutputScreenSize.Y >= 1.333)
			{
				if (AvailableResolutions.Contains(OutputScreenSize) && OutputScreenSize.Y <= RES_1080P.Y)
				{
					return OutputScreenSize;
				}
				double num = 960.0 / (double)OutputScreenSize.Y;
				return new Vec2((int)Math.Round((double)OutputScreenSize.X * num), 960);
			}
			if (AvailableResolutions.Contains(OutputScreenSize) && OutputScreenSize.X <= RES_1080P.Y)
			{
				return OutputScreenSize;
			}
			double num2 = 640.0 / (double)OutputScreenSize.X;
			return new Vec2(640, (int)Math.Round((double)OutputScreenSize.Y * num2));
		}

		public Vec2 GetDrawResolutionFromString(string res_id)
		{
			Vec2 result = RES_720P;
			if (Enum.TryParse<Resolution>(res_id, out var result2))
			{
				switch (result2)
				{
				case Resolution.Res1024x600:
					result = RES_600P;
					break;
				default:
					result = RES_720P;
					break;
				case Resolution.Res1600x900:
					result = RES_900P;
					break;
				case Resolution.Res1920x1080:
					result = RES_1080P;
					break;
				}
			}
			else
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, "Tried to get non-existent resolution: " + res_id);
			}
			return result;
		}

		public void BeginDrawToTempTexture(TempTexture tempTexture, bool clearTexture = true, bool smooth = false)
		{
			RenderTargetBinding[] renderTargets = monoGame.GraphicsDevice.GetRenderTargets();
			if (renderTargets.Length != 1 || renderTargets[0].RenderTarget != tempTexture.renderTarget)
			{
				monoGame.GraphicsDevice.SetRenderTarget(tempTexture.renderTarget);
			}
			if (clearTexture)
			{
				monoGame.GraphicsDevice.Clear(Color.Transparent);
			}
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, null, null, null, null);
			ColorBoxBlit(new Rect(0, 0, 1, 1), GameColor.Zero);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, smooth ? SamplerState.LinearWrap : SamplerState.PointWrap, null, null, null, null);
		}

		public void EndDrawToTempTexture()
		{
			spriteBatch.End();
		}

		public static string ResolutionToString(Vec2 res)
		{
			return res.X + ":" + res.Y;
		}

		public static Vec2 StringToResolution(string resString)
		{
			if (resString != null)
			{
				string[] array = resString.Split(':');
				if (array.Length == 2 && int.TryParse(array[0], out var result) && int.TryParse(array[1], out var result2))
				{
					return new Vec2(result, result2);
				}
				Game1.logMan.Log(LogManager.LogLevel.Warning, "failed to parse string into resolution: '" + resString + "'");
			}
			else
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, "failed to parse string into resolution: string was null");
			}
			return Vec2.Zero;
		}

		public Vec2 GetBootScreenSize()
		{
			double num = (double)TWMDrawScreenSize.X / (double)TWMDrawScreenSize.Y;
			double num2 = 1.3333330154418945;
			int y = RES_720P.Y;
			if (Game1.steamMan.IsOnSteamDeck)
			{
				y = RES_800P.Y;
			}
			if (num >= num2)
			{
				if (TWMDrawScreenSize.Y == y)
				{
					return TWMDrawScreenSize;
				}
				double num3 = (double)y / (double)TWMDrawScreenSize.Y;
				return new Vec2((int)Math.Round((double)TWMDrawScreenSize.X * num3), y);
			}
			if (TWMDrawScreenSize.X == y)
			{
				return TWMDrawScreenSize;
			}
			double num4 = (double)y / (double)TWMDrawScreenSize.X;
			return new Vec2(y, (int)Math.Round((double)TWMDrawScreenSize.Y * num4));
		}
	}
}

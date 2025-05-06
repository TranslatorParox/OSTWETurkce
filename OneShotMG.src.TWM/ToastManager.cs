using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class ToastManager
	{
		private class Toast
		{
			public int timer;

			public string icon;

			public string[] text;

			public TempTexture[] textTextures;
		}

		private const GraphicsManager.FontType font = GraphicsManager.FontType.OS;

		private const int TOAST_W = 180;

		private const int TOAST_H = 44;

		private const int TOAST_BORDER = 2;

		private const int TOAST_PADDING = 4;

		private const int RELEASE_RATE = 10;

		private const int TOAST_DURATION = 200;

		private const float TOAST_FADE_BEGIN = 20f;

		private const int LINE_HEIGHT = 12;

		private readonly Queue<Toast> toastList;

		private readonly Queue<Toast> pendingToastList;

		private int cardOffset;

		private int releaseTimer;

		public ToastManager()
		{
			toastList = new Queue<Toast>();
			pendingToastList = new Queue<Toast>();
		}

		public void Update()
		{
			foreach (Toast toast in toastList)
			{
				toast.timer--;
				TempTexture[] textTextures = toast.textTextures;
				for (int i = 0; i < textTextures.Length; i++)
				{
					textTextures[i]?.KeepAlive();
				}
			}
			foreach (Toast pendingToast in pendingToastList)
			{
				TempTexture[] textTextures = pendingToast.textTextures;
				for (int i = 0; i < textTextures.Length; i++)
				{
					textTextures[i]?.KeepAlive();
				}
			}
			while (toastList.Count > 0 && toastList.Peek().timer <= 0)
			{
				toastList.Dequeue();
				cardOffset -= 44;
				if (toastList.Count <= 0)
				{
					Game1.gMan.clearTextureCache(TextureCache.CacheType.Toasts);
				}
			}
			if (releaseTimer > 0)
			{
				releaseTimer--;
			}
			else if (pendingToastList.Count > 0)
			{
				toastList.Enqueue(pendingToastList.Dequeue());
				releaseTimer = 10;
			}
			int num = toastList.Count * 44;
			if (cardOffset < num)
			{
				cardOffset += Math.Max((num - cardOffset) / 10, 1);
			}
			else
			{
				cardOffset = num;
			}
		}

		public void Draw(TWMTheme theme, Vec2 screenSize)
		{
			Rect boxRect = new Rect(screenSize.X - 180 - 4, 0, 180, 44);
			int num = cardOffset;
			foreach (Toast toast in toastList)
			{
				num = (boxRect.Y = num - 44);
				byte b = byte.MaxValue;
				if ((float)toast.timer < 20f)
				{
					b = (byte)(255f * ((float)toast.timer / 20f));
				}
				Game1.gMan.ColorBoxBlit(boxRect, theme.Primary(b));
				Game1.gMan.ColorBoxBlit(boxRect.Shrink(2), theme.Background(b));
				Vec2 vec = new Vec2(boxRect.X + 4, (44 - toast.text.Length * 12) / 2 + boxRect.Y - 4);
				if (toast.icon != null)
				{
					GameColor white = GameColor.White;
					white.a = b;
					Vec2 vec2 = Game1.gMan.TextureSize(toast.icon);
					Vec2 pixelPos = new Vec2(vec.X, (44 - vec2.Y) / 2 + boxRect.Y);
					Game1.gMan.MainBlit(toast.icon, pixelPos, white, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.Toasts);
					vec.X += vec2.X + 4;
				}
				else
				{
					vec.X += 4;
				}
				TempTexture[] textTextures = toast.textTextures;
				foreach (TempTexture tempTexture in textTextures)
				{
					Game1.gMan.MainBlit(tempTexture, (vec + new Vec2(-2, 4)) * 2, theme.Primary(b), 0, GraphicsManager.BlendMode.Normal, 1);
					vec.Y += 12;
				}
			}
		}

		public void AddToast(string icon, string toastText)
		{
			Toast toast = new Toast();
			toast.timer = 200;
			toast.icon = (string.IsNullOrEmpty(icon) ? null : ("the_world_machine/" + icon));
			toast.text = toastText.Split('\n');
			Toast toast2 = toast;
			toast2.textTextures = new TempTexture[toast2.text.Length];
			for (int i = 0; i < toast2.text.Length; i++)
			{
				toast2.textTextures[i] = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, toast2.text[i]);
			}
			pendingToastList.Enqueue(toast2);
		}

		public int ToastCount()
		{
			return toastList.Count + pendingToastList.Count;
		}
	}
}

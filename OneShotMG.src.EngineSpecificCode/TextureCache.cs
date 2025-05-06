using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OneShotMG.src.EngineSpecificCode
{
	public class TextureCache
	{
		public enum CacheType
		{
			Game,
			TheWorldMachine,
			CharacterProfile,
			GalleryWindow,
			DesktopWallpaper,
			BootScreen,
			Toasts,
			Achievements,
			SteamGlyphes
		}

		private int CacheCount = Enum.GetValues(typeof(CacheType)).Length;

		private CacheAndManager[] contentCaches;

		public TextureCache(Game monoGame)
		{
			contentCaches = new CacheAndManager[CacheCount];
			for (int i = 0; i < CacheCount; i++)
			{
				if (i == 8)
				{
					contentCaches[i] = new UncookedTextureCache(monoGame);
				}
				else
				{
					contentCaches[i] = new CacheAndManager(monoGame);
				}
			}
		}

		public Texture2D GetTexture(string textureName, CacheType cacheType)
		{
			return contentCaches[(int)cacheType].GetTexture(textureName);
		}

		public void UnloadCache(CacheType cacheType)
		{
			contentCaches[(int)cacheType].Unload();
		}

		public void LoadTexture(string textureName, CacheType cacheType)
		{
			contentCaches[(int)cacheType].LoadTexture(textureName);
		}

		internal bool TextureExists(string path, CacheType cacheType)
		{
			return contentCaches[(int)cacheType].TextureExists(path);
		}
	}
}

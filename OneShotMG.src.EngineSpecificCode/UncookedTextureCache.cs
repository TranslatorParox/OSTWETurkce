using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OneShotMG.src.EngineSpecificCode
{
	internal class UncookedTextureCache : CacheAndManager
	{
		private GraphicsDevice graphicsDevice;

		public UncookedTextureCache(Game monoGame)
			: base(monoGame)
		{
			graphicsDevice = monoGame.GraphicsDevice;
		}

		public override void Unload()
		{
			foreach (Texture2D value in cache.Values)
			{
				value.Dispose();
			}
			base.Unload();
		}

		public override void LoadTexture(string textureName)
		{
			if (cache.TryGetValue(textureName, out var value))
			{
				return;
			}
			Game1.logMan.Log(LogManager.LogLevel.Info, "Loading texture '" + textureName + "'");
			FileStream fileStream = null;
			try
			{
				fileStream = new FileStream(textureName, FileMode.Open);
				value = Texture2D.FromStream(graphicsDevice, fileStream);
				cache.Add(textureName, value);
			}
			catch (Exception ex)
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, "Error loading texture '" + textureName + "' : " + ex.Message);
			}
			finally
			{
				fileStream?.Dispose();
			}
		}

		public override Texture2D GetTexture(string textureName)
		{
			LoadTexture(textureName);
			if (!cache.TryGetValue(textureName, out var value))
			{
				return null;
			}
			return value;
		}
	}
}

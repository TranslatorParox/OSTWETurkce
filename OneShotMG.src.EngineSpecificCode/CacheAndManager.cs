using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace OneShotMG.src.EngineSpecificCode
{
	internal class CacheAndManager
	{
		protected ContentManager contentManager;

		protected Dictionary<string, Texture2D> cache;

		public CacheAndManager(Game monoGame)
		{
			cache = new Dictionary<string, Texture2D>();
			contentManager = new ContentManager(monoGame.Content.ServiceProvider, monoGame.Content.RootDirectory);
		}

		public virtual Texture2D GetTexture(string textureName)
		{
			if (!cache.TryGetValue(textureName, out var value))
			{
				Game1.logMan.Log(LogManager.LogLevel.Info, "Loading texture '" + textureName + "'");
				string text = textureName.Replace("\\", "/");
				string text4;
				if (text.Contains("/"))
				{
					string text2 = text.Substring(text.LastIndexOf("/") + 1);
					string text3 = text.Substring(0, text.LastIndexOf("/"));
					text4 = text3 + "/" + Game1.languageMan.GetCurrentLangCode() + "/" + text2;
				}
				else
				{
					text4 = Game1.languageMan.GetCurrentLangCode() + text;
				}
				if (File.Exists(contentManager.RootDirectory + "/" + text4 + ".xnb"))
				{
					Game1.logMan.Log(LogManager.LogLevel.Info, "localized texture found, loading that '" + text4 + "'");
					value = contentManager.Load<Texture2D>(text4);
				}
				else
				{
					value = contentManager.Load<Texture2D>(textureName);
				}
				cache.Add(textureName, value);
			}
			return value;
		}

		public virtual void Unload()
		{
			cache = new Dictionary<string, Texture2D>();
			contentManager.Unload();
		}

		public virtual void LoadTexture(string textureName)
		{
			if (!cache.TryGetValue(textureName, out var value))
			{
				Game1.logMan.Log(LogManager.LogLevel.Info, "Loading texture '" + textureName + "'");
				value = contentManager.Load<Texture2D>(textureName);
				cache.Add(textureName, value);
			}
		}

		public bool TextureExists(string path)
		{
			if (cache.TryGetValue(path, out var value))
			{
				return true;
			}
			LoadTexture(path);
			return cache.TryGetValue(path, out value);
		}
	}
}

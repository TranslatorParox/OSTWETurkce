using System.IO;
using System.Xml.Serialization;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;
using Tiled;

namespace OneShotMG.src.Map
{
	public class TileSet
	{
		private int firstTileId;

		private string sourceFile;

		private Vec2 tileSize;

		private int totalTiles;

		private int tileColumns;

		private TileInfo[] tileInfos;

		private string imageFileName;

		private int animAreaRows;

		private int animFrames;

		private int animFrameTime;

		private bool animated;

		private int animTimer;

		private int frameIndex;

		private const int AUTOTILE_NORMAL_HEIGHT = 96;

		private const int AUTOTILE_ANIM_FRAME_TIME = 16;

		private const int AUTOTILE_NORMAL_ROWS = 6;

		public TileSet(int firstTileId, string sourceFile)
		{
			this.firstTileId = firstTileId;
			this.sourceFile = sourceFile;
			Read(sourceFile);
		}

		private void Read(string fileName)
		{
			bool flag = false;
			int num = fileName.IndexOf("tilesets/");
			if (num < 0)
			{
				num = fileName.IndexOf("autotiles/");
				flag = true;
				if (num < 0)
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, "file '" + fileName + "' is not in the tilesets folder!");
					return;
				}
			}
			string text = fileName.Substring(num);
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Tiled.TileSet));
			Tiled.TileSet tileSet;
			using (Stream stream = new FileStream(Game1.GameDataPath() + "/" + text, FileMode.Open))
			{
				tileSet = (Tiled.TileSet)xmlSerializer.Deserialize(stream);
			}
			tileSize.X = tileSet.tilewidth;
			tileSize.Y = tileSet.tileheight;
			totalTiles = tileSet.tilecount;
			tileColumns = tileSet.columns;
			tileInfos = new TileInfo[totalTiles];
			for (int i = 0; i < totalTiles; i++)
			{
				tileInfos[i] = new TileInfo();
			}
			imageFileName = tileSet.image.source;
			int num2 = imageFileName.IndexOf("Content/");
			if (num2 < 0)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, "image '" + imageFileName + "' in tileset '" + fileName + "' is not in the Content folder!");
			}
			else
			{
				num2 += 8;
				imageFileName = imageFileName.Substring(num2);
				int length = imageFileName.IndexOf(".png");
				imageFileName = imageFileName.Substring(0, length);
			}
			if (flag)
			{
				Vec2 vec = Game1.gMan.TextureSize(imageFileName);
				if (vec.Y > 96)
				{
					animated = true;
					animAreaRows = 6;
					animFrames = vec.Y / 96;
					animFrameTime = 16;
					animTimer = 0;
					frameIndex = 0;
				}
			}
		}

		public int GetFirstTileID()
		{
			return firstTileId;
		}

		public TileInfo GetTile(int id)
		{
			return tileInfos[id - firstTileId];
		}

		public void DrawTile(Vec2 drawPos, int tileID, GameTone tone, float alpha = 1f)
		{
			int num = tileID - firstTileId + frameIndex * animAreaRows * tileColumns;
			if (num < 0 || num >= totalTiles)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"Tried to draw tile {num}, out of range on tileset {imageFileName}");
			}
			int x = num % tileColumns * tileSize.X;
			int y = num / tileColumns * tileSize.Y;
			Rect srcRect = new Rect(x, y, tileSize.X, tileSize.Y);
			Game1.gMan.MainBlit(imageFileName, drawPos, srcRect, alpha, 0, GraphicsManager.BlendMode.Normal, 2, tone);
		}

		public void Update()
		{
			if (!animated)
			{
				return;
			}
			animTimer++;
			if (animTimer > animFrameTime)
			{
				animTimer = 0;
				frameIndex++;
				if (frameIndex >= animFrames)
				{
					frameIndex = 0;
				}
			}
		}
	}
}

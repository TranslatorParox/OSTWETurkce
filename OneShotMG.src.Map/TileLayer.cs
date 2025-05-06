using System;
using System.Collections.Generic;
using System.Globalization;
using OneShotMG.src.Util;
using Tiled;

namespace OneShotMG.src.Map
{
	public class TileLayer
	{
		private short[] tiles;

		private string name = string.Empty;

		private Vec2 layerSize = Vec2.Zero;

		public TileLayer(Vec2 mapSize)
		{
			layerSize = mapSize;
			int num = mapSize.X * mapSize.Y;
			tiles = new short[num];
			for (int i = 0; i < num; i++)
			{
				tiles[i] = 0;
			}
		}

		public TileLayer(Vec2 mapSize, Tiled.TileLayer layer)
		{
			int num = mapSize.X * mapSize.Y;
			tiles = new short[num];
			name = layer.name;
			layerSize.X = layer.width;
			layerSize.Y = layer.height;
			Data data = layer.data;
			switch (data.encoding)
			{
			case Encoding.csv:
			{
				string[] array = data.Value.Split(',');
				for (int i = 0; i < num; i++)
				{
					tiles[i] = short.Parse(array[i].Trim(), CultureInfo.InvariantCulture);
				}
				break;
			}
			case Encoding.base64:
				throw new Exception("Unsupported base64 tiledata encoding in $" + name);
			}
		}

		public string GetName()
		{
			return name;
		}

		public TileInfo GetTile(int x, int y, List<TileSet> tilesets)
		{
			int num = tiles[x + y * layerSize.X];
			if (num == 0)
			{
				return null;
			}
			TileSet tileSet = tilesets[0];
			foreach (TileSet tileset in tilesets)
			{
				if (tileset.GetFirstTileID() > num)
				{
					break;
				}
				tileSet = tileset;
			}
			return tileSet.GetTile(num);
		}

		public short GetTileId(int x, int y)
		{
			return tiles[x + y * layerSize.X];
		}

		public short GetRPGMakerTileId(int x, int y, List<TileSet> tilesets)
		{
			short tileId = GetTileId(x, y);
			if (tileId == 0)
			{
				return tileId;
			}
			short num = tileId;
			int num2 = 0;
			bool flag = true;
			TileSet tileSet = tilesets[0];
			foreach (TileSet tileset in tilesets)
			{
				if (flag)
				{
					flag = false;
					continue;
				}
				if (tileset.GetFirstTileID() > tileId)
				{
					break;
				}
				num2++;
				tileSet = tileset;
			}
			num -= (short)tileSet.GetFirstTileID();
			return (short)(num + (short)(num2 * 48));
		}

		public void SetTileId(int x, int y, short tileId)
		{
			tiles[x + y * layerSize.X] = tileId;
		}

		public void DrawTile(Vec2 drawPos, int xIndex, int yIndex, List<TileSet> tilesets, int height, GameTone tone)
		{
			yIndex -= height;
			if (yIndex < 0)
			{
				yIndex += layerSize.Y;
			}
			int num = tiles[xIndex + yIndex * layerSize.X];
			if (num == 0)
			{
				return;
			}
			TileSet tileSet = tilesets[0];
			foreach (TileSet tileset in tilesets)
			{
				if (tileset.GetFirstTileID() > num)
				{
					break;
				}
				tileSet = tileset;
			}
			tileSet.DrawTile(drawPos, num, tone);
		}

		public void Draw(List<TileSet> tilesets, Vec2 camPos, Vec2 tileSize, bool wrapping, GameTone tone, float alpha = 1f)
		{
			Vec2 zero = Vec2.Zero;
			int num = camPos.X / tileSize.X;
			int num2 = camPos.Y / tileSize.Y;
			int num3 = (camPos.X + 320) / tileSize.X + 1;
			int num4 = (camPos.Y + 240) / tileSize.Y + 1;
			if (!wrapping)
			{
				if (num < 0)
				{
					num = 0;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num3 >= layerSize.X)
				{
					num3 = layerSize.X - 1;
				}
				if (num4 >= layerSize.Y)
				{
					num4 = layerSize.Y - 1;
				}
			}
			else
			{
				if (num <= 0)
				{
					num--;
				}
				if (num2 <= 0)
				{
					num2--;
				}
			}
			int x = (camPos.X - num * tileSize.X) * -1;
			int y = (camPos.Y - num2 * tileSize.Y) * -1;
			zero.X = x;
			zero.Y = y;
			for (int i = num2; i <= num4; i++)
			{
				int num5 = i;
				if (num5 < 0)
				{
					num5 += layerSize.Y;
				}
				else if (num5 >= layerSize.Y)
				{
					num5 -= layerSize.Y;
				}
				for (int j = num; j <= num3; j++)
				{
					int num6 = j;
					if (num6 < 0)
					{
						num6 += layerSize.X;
					}
					else if (num6 >= layerSize.X)
					{
						num6 -= layerSize.X;
					}
					int num7 = tiles[num6 + num5 * layerSize.X];
					if (num7 != 0)
					{
						TileSet tileSet = tilesets[0];
						foreach (TileSet tileset in tilesets)
						{
							if (tileset.GetFirstTileID() > num7)
							{
								break;
							}
							tileSet = tileset;
						}
						tileSet.DrawTile(zero, num7, tone, alpha);
					}
					zero.X += tileSize.X;
				}
				zero.Y += tileSize.Y;
				zero.X = x;
			}
		}
	}
}

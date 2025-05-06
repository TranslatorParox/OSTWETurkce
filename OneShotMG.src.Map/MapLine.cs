using System;
using System.Collections.Generic;
using OneShotMG.src.Util;

namespace OneShotMG.src.Map
{
	public class MapLine
	{
		public enum Slope
		{
			Floor,
			Ceiling,
			EastWall,
			WestWall
		}

		private Slope slope;

		private Vec2 startPoint;

		private Vec2 endPoint;

		private int id;

		public MapLine(Vec2 start, Vec2 end, int lineID)
		{
			startPoint = start;
			endPoint = end;
			slopeClassification();
			id = lineID;
		}

		public int GetID()
		{
			return id;
		}

		private void slopeClassification()
		{
			int num = endPoint.X - startPoint.X;
			int num2 = endPoint.Y - startPoint.Y;
			if (num == 0)
			{
				if (num2 > 0)
				{
					slope = Slope.WestWall;
				}
				else
				{
					slope = Slope.EastWall;
				}
				return;
			}
			float num3 = (float)num2 / (float)num;
			if ((double)num3 <= 1.0 && (double)num3 >= -1.0)
			{
				if (num > 0)
				{
					slope = Slope.Floor;
				}
				else
				{
					slope = Slope.Ceiling;
				}
			}
			else if (num2 > 0)
			{
				slope = Slope.WestWall;
			}
			else
			{
				slope = Slope.EastWall;
			}
		}

		public void hashIntoTiles(Dictionary<int, List<MapLine>> hashedLines, Vec2 tileSize, Vec2 mapSize)
		{
			int num = endPoint.X - startPoint.X;
			int num2 = endPoint.Y - startPoint.Y;
			tileSize.X *= 256;
			tileSize.Y *= 256;
			float num3;
			int x;
			int y;
			int y2;
			int num5;
			int num6;
			if (Math.Abs(num) >= Math.Abs(num2))
			{
				num3 = (float)num2 / (float)num;
				int num4;
				if (num > 0)
				{
					num4 = endPoint.X + tileSize.X / 2;
					x = startPoint.X;
					y = endPoint.Y;
					y2 = startPoint.Y;
				}
				else
				{
					num4 = startPoint.X + tileSize.X / 2;
					x = endPoint.X;
					y = startPoint.Y;
					y2 = endPoint.Y;
				}
				num5 = x / tileSize.X;
				num6 = y2 / tileSize.Y;
				hashIntoTile(num6 * mapSize.X + num5, hashedLines);
				int i = x + 1;
				if (i % tileSize.X != 0)
				{
					i = (i / tileSize.X + 1) * tileSize.X;
				}
				for (; i <= num4; i += tileSize.X)
				{
					int num7 = i / tileSize.X;
					int num8 = (int)((float)(i - x) * num3 + (float)y2) / tileSize.Y;
					hashIntoTile(num8 * mapSize.X + num7, hashedLines);
					if (num8 != num6)
					{
						hashIntoTile(num8 * mapSize.X + (num7 - 1), hashedLines);
					}
					num6 = num8;
					num5 = num7;
				}
				return;
			}
			num3 = (float)num / (float)num2;
			if (num2 > 0)
			{
				int num4 = endPoint.X;
				x = startPoint.X;
				y = endPoint.Y + tileSize.Y / 2;
				y2 = startPoint.Y;
			}
			else
			{
				int num4 = startPoint.X;
				x = endPoint.X;
				y = startPoint.Y + tileSize.Y / 2;
				y2 = endPoint.Y;
			}
			num5 = x / tileSize.X;
			num6 = y2 / tileSize.Y;
			hashIntoTile(num6 * mapSize.X + num5, hashedLines);
			int j = y2 + 1;
			if (j % tileSize.Y != 0)
			{
				j = (j / tileSize.Y + 1) * tileSize.Y;
			}
			for (; j <= y; j += tileSize.Y)
			{
				int num8 = j / tileSize.Y;
				int num7 = (int)((float)(j - y2) * num3 + (float)x) / tileSize.X;
				hashIntoTile(num8 * mapSize.X + num7, hashedLines);
				if (num7 != num5)
				{
					hashIntoTile((num8 - 1) * mapSize.X + num7, hashedLines);
				}
				num6 = num8;
				num5 = num7;
			}
		}

		private void hashIntoTile(int index, Dictionary<int, List<MapLine>> hashedLines)
		{
			if (!hashedLines.TryGetValue(index, out var value))
			{
				value = new List<MapLine>(1);
				hashedLines.Add(index, value);
			}
			value.Add(this);
		}

		public void Collision(Entity e)
		{
			Vec2 pos = e.GetPos();
			Vec2 oldPos = e.GetOldPos();
			Rect collisionRect = e.GetCollisionRect();
			Vec2 zero = Vec2.Zero;
			Vec2 zero2 = Vec2.Zero;
			Vec2 zero3 = Vec2.Zero;
			Vec2 zero4 = Vec2.Zero;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			switch (slope)
			{
			default:
				return;
			case Slope.Floor:
				num = pos.X + collisionRect.X * 256;
				num2 = pos.X + (collisionRect.X + collisionRect.W) * 256;
				num3 = pos.Y + collisionRect.Y * 256;
				num4 = pos.Y + (collisionRect.Y + collisionRect.H) * 256;
				num5 = oldPos.Y + (collisionRect.Y + collisionRect.H) * 256;
				zero3.X = pos.X;
				zero3.Y = pos.Y;
				zero4.X = oldPos.X;
				zero4.Y = oldPos.Y;
				zero = new Vec2(startPoint.X, startPoint.Y);
				zero2 = new Vec2(endPoint.X, endPoint.Y);
				break;
			case Slope.Ceiling:
				num = pos.X + collisionRect.X * 256;
				num2 = pos.X + (collisionRect.X + collisionRect.W) * 256;
				num3 = -(pos.Y + (collisionRect.Y + collisionRect.H) * 256);
				num4 = -(pos.Y + collisionRect.Y * 256);
				num5 = -(oldPos.Y + collisionRect.Y * 256);
				zero3.X = pos.X;
				zero3.Y = -pos.Y;
				zero4.X = oldPos.X;
				zero4.Y = -oldPos.Y;
				zero = new Vec2(endPoint.X, -endPoint.Y);
				zero2 = new Vec2(startPoint.X, -startPoint.Y);
				break;
			case Slope.EastWall:
				num = -(pos.Y + (collisionRect.Y + collisionRect.H) * 256);
				num2 = -(pos.Y + collisionRect.Y * 256);
				num3 = pos.X + collisionRect.X * 256;
				num4 = pos.X + (collisionRect.X + collisionRect.W) * 256;
				num5 = oldPos.X + (collisionRect.X + collisionRect.W) * 256;
				zero3.X = -pos.Y;
				zero3.Y = pos.X;
				zero4.X = -oldPos.Y;
				zero4.Y = oldPos.X;
				zero = new Vec2(-startPoint.Y, startPoint.X);
				zero2 = new Vec2(-endPoint.Y, endPoint.X);
				break;
			case Slope.WestWall:
				num = pos.Y + collisionRect.Y * 256;
				num2 = pos.Y + (collisionRect.Y + collisionRect.H) * 256;
				num3 = -(pos.X + (collisionRect.X + collisionRect.W) * 256);
				num4 = -(pos.X + collisionRect.X * 256);
				num5 = -(oldPos.X + collisionRect.X * 256);
				zero3.X = pos.Y;
				zero3.Y = -pos.X;
				zero4.X = oldPos.Y;
				zero4.Y = -oldPos.X;
				zero = new Vec2(startPoint.Y, -startPoint.X);
				zero2 = new Vec2(endPoint.Y, -endPoint.X);
				break;
			}
			if (num2 <= zero.X || num >= zero2.X)
			{
				return;
			}
			float num6 = (float)(zero2.Y - zero.Y) / (float)(zero2.X - zero.X);
			int num7 = (int)((float)(zero3.X - zero.X) * num6 + (float)zero.Y);
			int num8 = (int)((float)(zero4.X - zero.X) * num6 + (float)zero.Y);
			Math.Max(zero.Y, zero2.Y);
			Math.Min(zero.Y, zero2.Y);
			if (num4 > num7 && num3 < num7 && num5 <= num8 + 512)
			{
				int num9 = num7 - num4;
				double num10 = Math.Atan2(zero2.Y - zero.Y, zero2.X - zero.X);
				int num11 = (int)(Math.Sin(num10) * (double)num9);
				int num12 = (int)(Math.Cos(num10) * (double)num9);
				switch (slope)
				{
				case Slope.Floor:
					e.SetCollisionDirection(Entity.Direction.Down);
					e.SetVel(Vec2.Zero);
					e.SetPos(new Vec2(pos.X - num11, pos.Y + num12));
					break;
				case Slope.Ceiling:
					e.SetCollisionDirection(Entity.Direction.Up);
					e.SetVel(Vec2.Zero);
					e.SetPos(new Vec2(pos.X - num11, pos.Y - num12));
					break;
				case Slope.EastWall:
					e.SetCollisionDirection(Entity.Direction.Right);
					e.SetVel(Vec2.Zero);
					e.SetPos(new Vec2(pos.X + num12, pos.Y + num11));
					break;
				case Slope.WestWall:
					e.SetCollisionDirection(Entity.Direction.Right);
					e.SetVel(Vec2.Zero);
					e.SetPos(new Vec2(pos.X - num12, pos.Y - num11));
					break;
				}
			}
		}

		public void Draw(Vec2 camPos)
		{
			Game1.gMan.LineBlit(new Vec2(startPoint.X / 256 - camPos.X, startPoint.Y / 256 - camPos.Y), new Vec2(endPoint.X / 256 - camPos.X, endPoint.Y / 256 - camPos.Y), GameColor.White);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using OneShotMG.src.EngineSpecificCode;

namespace OneShotMG.src.Util
{
	public class MathHelper
	{
		private static Random rnd = new Random();

		private static readonly char[] WORD_SEPARATORS = new char[2] { ' ', '\n' };

		public static int ApproachInt(int target, int start, float speedRatio)
		{
			int num = target - start;
			int num2 = (int)((float)num * speedRatio);
			if (num2 == 0)
			{
				num2 = ((num >= 0) ? 1 : (-1));
			}
			if (num == 0)
			{
				return target;
			}
			return start + num2;
		}

		public static float ApproachFloat(float target, float start, float speedRatio)
		{
			float num = (target - start) * speedRatio;
			return start + num;
		}

		public static int EaseIn(int start, int target, int currentTime, int totalTime)
		{
			float num = (float)currentTime / (float)totalTime;
			float num2 = 1f - (1f - num) * (1f - num) * (1f - num);
			return start + (int)Math.Round((float)(target - start) * num2);
		}

		public static int EaseOut(int start, int target, int currentTime, int totalTime)
		{
			float num = (float)currentTime / (float)totalTime;
			float num2 = num * num * num;
			return start + (int)Math.Round((float)(target - start) * num2);
		}

		public static int Random(int min, int max)
		{
			return rnd.Next(min, max + 1);
		}

		public static float FRandom(float min, float max)
		{
			float num = (float)rnd.NextDouble();
			return min + num * (max - min);
		}

		public static T RandomChoice<T>(T[] options)
		{
			if (options == null || options.Length == 0)
			{
				return default(T);
			}
			return options[rnd.Next(options.Length)];
		}

		public static bool RectOverlap(Rect a, Rect b)
		{
			int num = a.X + a.W;
			int num2 = a.Y + a.H;
			int num3 = b.X + b.W;
			int num4 = b.Y + b.H;
			if (num >= b.X && num2 >= b.Y && a.X <= num3)
			{
				return a.Y <= num4;
			}
			return false;
		}

		public static string Truncate(GraphicsManager.FontType font, string s, int maxLen)
		{
			if (Game1.gMan.TextSize(font, s).X < maxLen)
			{
				return s;
			}
			int num = Math.Max(s.Length / 2, 1);
			return TruncateBinarySearch(font, s, maxLen, s.Length, -num);
		}

		private static string TruncateBinarySearch(GraphicsManager.FontType font, string s, int maxLen, int lastCheck, int step)
		{
			int num = lastCheck + step;
			if (num <= 0)
			{
				return "";
			}
			string text = s.Substring(0, num) + "..";
			int x = Game1.gMan.TextSize(font, text).X;
			if (x > maxLen && step == 1)
			{
				return s.Substring(0, lastCheck) + "..";
			}
			int num2 = Math.Abs(step);
			int step2 = ((x <= maxLen) ? Math.Max(num2 / 2, 1) : Math.Min(-num2 / 2, -1));
			return TruncateBinarySearch(font, s, maxLen, num, step2);
		}

		public static List<string> WordWrap(GraphicsManager.FontType font, string input, int width)
		{
			List<string> list = new List<string>();
			string text = string.Empty;
			string text2 = string.Empty;
			int num = 0;
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];
				switch (c)
				{
				case '@':
				{
					int num2 = input.IndexOfAny(WORD_SEPARATORS, i);
					if (num2 < 0)
					{
						num2 = input.Length - 1;
					}
					string text3 = input.Substring(i, num2 - i);
					if (!Regex.IsMatch(text3, InputManager.GlyphPattern))
					{
						text2 += c;
						num += Game1.gMan.TextSize(font, c.ToString()).X;
						break;
					}
					List<InputManager.ButtonGlyphInfo> glyph = Game1.inputMan.GetGlyph(text3);
					if (glyph == null || glyph.Count <= 0)
					{
						text2 += c;
						num += Game1.gMan.TextSize(font, c.ToString()).X;
						break;
					}
					foreach (InputManager.ButtonGlyphInfo item in glyph)
					{
						num = ((item.cacheType != TextureCache.CacheType.SteamGlyphes) ? (num + Game1.gMan.TextureSize(item.texturePath, item.cacheType).X) : (num + 32));
					}
					text2 += text3;
					i = num2 - 1;
					break;
				}
				case '\n':
					text += text2;
					text2 = string.Empty;
					list.Add(text);
					text = string.Empty;
					num = 0;
					break;
				case ' ':
					text = text + text2 + c;
					text2 = string.Empty;
					num += Game1.gMan.TextSize(font, c.ToString()).X;
					break;
				default:
					text2 += c;
					num += Game1.gMan.TextSize(font, c.ToString()).X;
					break;
				}
				if (num <= width)
				{
					continue;
				}
				if (!string.IsNullOrEmpty(text))
				{
					if (text.EndsWith(" "))
					{
						text = text.Substring(0, text.Length - 1);
					}
					list.Add(text);
					text = string.Empty;
					num = Game1.gMan.TextSize(font, text2).X;
				}
				else
				{
					string text4 = text2[text2.Length - 1].ToString();
					text2 = text2.Substring(0, text2.Length - 1);
					text = text2;
					list.Add(text);
					text2 = text4;
					num = Game1.gMan.TextSize(font, text2).X;
					text = string.Empty;
				}
			}
			if (!string.IsNullOrEmpty(text) || !string.IsNullOrEmpty(text2))
			{
				text += text2;
				list.Add(text);
			}
			return list;
		}

		public static Vector3 ApplyHue(Vector3 col, float hueAdjust)
		{
			Vector3 vector = new Vector3(0.57735f, 0.57735f, 0.57735f);
			float num = (float)Math.Cos(hueAdjust);
			return col * num + Vector3.Cross(vector, col) * (float)Math.Sin(hueAdjust) + vector * Vector3.Dot(vector, col) * (1f - num);
		}

		public static void HandleAbberateUpdate(ref int channelTimer, ref Vec2 channelOffset, int strength = 1, int odds = 33)
		{
			if (channelTimer > 0)
			{
				channelTimer--;
				if (channelTimer <= 0)
				{
					channelTimer = 0;
					channelOffset = Vec2.Zero;
				}
			}
			else if (Random(1, 100) < odds)
			{
				channelTimer = Random(2, 8);
				channelOffset = new Vec2(Random(-strength, strength), Random(-strength, strength));
			}
		}
	}
}

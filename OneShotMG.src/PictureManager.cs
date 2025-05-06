using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class PictureManager
	{
		public class PictureSaveData
		{
			public string name;

			public Vec2 pos;

			public bool isPosCenter;

			public float xScale;

			public float yScale;

			public int opacity;

			public GraphicsManager.BlendMode blendMode = GraphicsManager.BlendMode.Normal;

			public int id;
		}

		private class Picture
		{
			private string name;

			private Vec2 pos;

			private bool isPosCenter;

			private float xScale;

			private float yScale;

			private int opacity;

			private GraphicsManager.BlendMode blendMode;

			private Vec2 picSize;

			private int moveTimer;

			private int totalMoveTime;

			private Vec2 startPos;

			private Vec2 targetPos;

			private float startXScale;

			private float targetXScale;

			private float startYScale;

			private float targetYScale;

			private int startOpacity;

			private int targetOpacity;

			private GameTone tone = GameTone.Zero;

			private GameTone startTone = GameTone.Zero;

			private GameTone targetTone = GameTone.Zero;

			private int toneChangeTimer;

			private int totalToneChangeTime;

			public Picture(string name, Vec2 pos, bool isPosCenter, float xScale, float yScale, int opacity, GraphicsManager.BlendMode blendMode)
			{
				this.name = name;
				this.pos = pos;
				this.isPosCenter = isPosCenter;
				this.xScale = xScale;
				this.yScale = yScale;
				this.opacity = opacity;
				this.blendMode = blendMode;
				picSize = Game1.gMan.TextureSize("pictures/" + name);
			}

			public void Draw()
			{
				if (opacity > 0)
				{
					Vec2 pixelPos = pos;
					if (isPosCenter)
					{
						pixelPos.X -= (int)((float)picSize.X * xScale / 2f);
						pixelPos.Y -= (int)((float)picSize.Y * xScale / 2f);
					}
					float alpha = (float)opacity / 255f;
					Game1.gMan.MainBlit("pictures/" + name, pixelPos, new Rect(0, 0, picSize.X, picSize.Y), xScale, yScale, alpha, 0, blendMode, tone);
				}
			}

			public void Update()
			{
				if (totalMoveTime > 0)
				{
					moveTimer++;
					if (moveTimer >= totalMoveTime)
					{
						pos = targetPos;
						xScale = targetXScale;
						yScale = targetYScale;
						opacity = targetOpacity;
						totalMoveTime = 0;
						moveTimer = 0;
					}
					else
					{
						float amount = (float)moveTimer / (float)totalMoveTime;
						pos.X = (int)Microsoft.Xna.Framework.MathHelper.Lerp(startPos.X, targetPos.X, amount);
						pos.Y = (int)Microsoft.Xna.Framework.MathHelper.Lerp(startPos.Y, targetPos.Y, amount);
						xScale = Microsoft.Xna.Framework.MathHelper.Lerp(startXScale, targetXScale, amount);
						yScale = Microsoft.Xna.Framework.MathHelper.Lerp(startYScale, targetYScale, amount);
						opacity = (int)Microsoft.Xna.Framework.MathHelper.Lerp(startOpacity, targetOpacity, amount);
					}
				}
				if (totalToneChangeTime > 0)
				{
					toneChangeTimer++;
					if (toneChangeTimer >= totalToneChangeTime)
					{
						tone = targetTone;
						totalToneChangeTime = 0;
						toneChangeTimer = 0;
					}
					else
					{
						float num = (float)toneChangeTimer / (float)totalToneChangeTime;
						tone = startTone * (1f - num) + targetTone * num;
					}
				}
			}

			public void Move(int duration, Vec2 pos, bool isPosCenter, float xScale, float yScale, int opacity, GraphicsManager.BlendMode blendMode)
			{
				startPos = this.pos;
				startXScale = this.xScale;
				startYScale = this.yScale;
				startOpacity = this.opacity;
				targetPos = pos;
				this.isPosCenter = isPosCenter;
				targetXScale = xScale;
				targetYScale = yScale;
				targetOpacity = opacity;
				this.blendMode = blendMode;
				moveTimer = 0;
				totalMoveTime = duration;
			}

			public void StartToneShift(GameTone newTone, int shiftTime)
			{
				toneChangeTimer = 0;
				if (shiftTime <= 0)
				{
					totalToneChangeTime = 0;
					tone = newTone;
				}
				else
				{
					totalToneChangeTime = shiftTime;
					startTone = tone;
					targetTone = newTone;
				}
			}

			public PictureSaveData GetPictureSaveData(int pictureId)
			{
				return new PictureSaveData
				{
					id = pictureId,
					name = name,
					pos = pos,
					isPosCenter = isPosCenter,
					xScale = xScale,
					yScale = yScale,
					opacity = opacity,
					blendMode = blendMode
				};
			}
		}

		private SortedDictionary<int, Picture> pictures;

		public PictureManager()
		{
			pictures = new SortedDictionary<int, Picture>();
		}

		public void Update()
		{
			foreach (Picture value in pictures.Values)
			{
				value.Update();
			}
		}

		public void Draw()
		{
			foreach (Picture value in pictures.Values)
			{
				value.Draw();
			}
		}

		public List<PictureSaveData> GetPictureSaveDatas()
		{
			List<PictureSaveData> list = new List<PictureSaveData>();
			foreach (KeyValuePair<int, Picture> picture in pictures)
			{
				list.Add(picture.Value.GetPictureSaveData(picture.Key));
			}
			return list;
		}

		public void LoadPictureSaveDatas(List<PictureSaveData> pictureSaveDatas)
		{
			pictures.Clear();
			if (pictureSaveDatas == null)
			{
				return;
			}
			foreach (PictureSaveData pictureSaveData in pictureSaveDatas)
			{
				ShowPicture(pictureSaveData.id, pictureSaveData.name, pictureSaveData.isPosCenter, pictureSaveData.pos, pictureSaveData.xScale, pictureSaveData.yScale, pictureSaveData.opacity, pictureSaveData.blendMode);
			}
		}

		public void ShowPicture(int pictureSlot, string pictureName, bool isPosCenter, Vec2 position, float xScale, float yScale, int opacity, GraphicsManager.BlendMode blendMode)
		{
			if (pictures.ContainsKey(pictureSlot))
			{
				pictures.Remove(pictureSlot);
			}
			Game1.windowMan.UnlockMan.UnlockCg(pictureName);
			pictures.Add(pictureSlot, new Picture(pictureName, position, isPosCenter, xScale, yScale, opacity, blendMode));
		}

		public void ErasePicture(int pictureSlot)
		{
			pictures.Remove(pictureSlot);
		}

		public void MovePicture(int pictureSlot, int duration, bool isPosCenter, Vec2 position, float xScale, float yScale, int opacity, GraphicsManager.BlendMode blendMode)
		{
			if (pictures.TryGetValue(pictureSlot, out var value))
			{
				value.Move(duration, position, isPosCenter, xScale, yScale, opacity, blendMode);
			}
		}

		public void ChangePictureTone(int pictureSlot, GameTone tone, int duration)
		{
			if (pictures.TryGetValue(pictureSlot, out var value))
			{
				value.StartToneShift(tone, duration);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class GlitchEffectManager
	{
		private List<Vec2> glitchSegmentHW;

		private List<GameColor> glitchSegmentColors;

		private int glitchOverallTimer;

		private int glitchCycleTimer;

		private const int GLITCH_CYCLE_TIME = 5;

		private float glitchStrength = 1f;

		public bool GlitchSegmentsEnabled => glitchOverallTimer > 0;

		public void Update()
		{
			if (glitchOverallTimer > 0)
			{
				glitchOverallTimer--;
				glitchCycleTimer++;
				if (glitchCycleTimer >= 5)
				{
					glitchCycleTimer = 0;
					generateGlitchSegments();
				}
			}
			else
			{
				glitchOverallTimer = 0;
			}
		}

		public void StartGlitch(OneshotWindow osWindow, int frames, float strength = 1f, bool vibrateController = true)
		{
			if (osWindow.menuMan.SettingsMenu.IsScreenTearEnabled)
			{
				glitchOverallTimer = frames;
				glitchCycleTimer = 0;
				glitchStrength = strength;
				generateGlitchSegments();
			}
			if (vibrateController)
			{
				Game1.inputMan.VibrateController(strength, frames);
			}
		}

		private void generateGlitchSegments()
		{
			glitchSegmentHW = new List<Vec2>();
			glitchSegmentColors = new List<GameColor>();
			int num = 480;
			while (num > 0)
			{
				Vec2 item = new Vec2(generateSegmentWidth(), OneShotMG.src.Util.MathHelper.Random(15, 120) * 2);
				if (item.Y > num)
				{
					item.Y = num;
				}
				glitchSegmentHW.Add(item);
				glitchSegmentColors.Add(generateGlitchColor());
				num -= item.Y;
			}
		}

		private GameColor generateGlitchColor()
		{
			Vector3 col = new Vector3(1f, OneShotMG.src.Util.MathHelper.FRandom(1f - 0.5f * glitchStrength, 1f), 1f);
			col = OneShotMG.src.Util.MathHelper.ApplyHue(col, OneShotMG.src.Util.MathHelper.FRandom(0f, (float)Math.PI * 2f));
			col.X = Math.Max(0f, Math.Min(1f, col.X));
			col.Y = Math.Max(0f, Math.Min(1f, col.Y));
			col.Z = Math.Max(0f, Math.Min(1f, col.Z));
			return new GameColor((byte)(col.X * 255f), (byte)(col.Y * 255f), (byte)(col.Z * 255f), byte.MaxValue);
		}

		private int generateSegmentWidth()
		{
			int num = (int)((float)(OneShotMG.src.Util.MathHelper.Random(-160, 160) * 2) * glitchStrength);
			if (num < 0)
			{
				num += 640;
			}
			return num;
		}

		public void Draw()
		{
			Game1.gMan.DrawGlitchSegments(glitchSegmentHW, glitchSegmentColors);
		}
	}
}

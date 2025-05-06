using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class FullscreenBorderManager
	{
		private Dictionary<int, GameColor> mapIdToColor;

		private int currentMapId = -1;

		private OneshotWindow oneshotWindow;

		private GameColor prevBorderColor;

		private GameColor nextBorderColor;

		private int colorShiftTimer;

		private const int COLOR_SHIFT_TIME = 120;

		private bool isColorShifting;

		public GameColor CurrentBorderColor { get; private set; }

		public FullscreenBorderManager(OneshotWindow osw)
		{
			oneshotWindow = osw;
			mapIdToColor = new Dictionary<int, GameColor>();
			BorderColorData[] mapColors = JsonConvert.DeserializeObject<BorderColorsData>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_map_colors.json")).mapColors;
			foreach (BorderColorData borderColorData in mapColors)
			{
				int[] maps = borderColorData.maps;
				foreach (int key in maps)
				{
					mapIdToColor[key] = borderColorData.color;
				}
			}
			Game1.logMan.Log(LogManager.LogLevel.Info, "loaded border colors");
		}

		public void Update()
		{
			if (currentMapId < 0)
			{
				currentMapId = oneshotWindow.tileMapMan.GetMapID();
				CurrentBorderColor = getBorderColor(currentMapId);
			}
			if (currentMapId != oneshotWindow.tileMapMan.GetMapID())
			{
				currentMapId = oneshotWindow.tileMapMan.GetMapID();
				nextBorderColor = getBorderColor(currentMapId);
				if (!CurrentBorderColor.Equals(nextBorderColor))
				{
					prevBorderColor = CurrentBorderColor;
					colorShiftTimer = 0;
					isColorShifting = true;
				}
			}
			if (isColorShifting)
			{
				colorShiftTimer++;
				if (colorShiftTimer >= 120)
				{
					CurrentBorderColor = nextBorderColor;
					isColorShifting = false;
				}
				else
				{
					float speedRatio = (float)colorShiftTimer / 120f;
					CurrentBorderColor = new GameColor((byte)MathHelper.ApproachInt(nextBorderColor.r, prevBorderColor.r, speedRatio), (byte)MathHelper.ApproachInt(nextBorderColor.g, prevBorderColor.g, speedRatio), (byte)MathHelper.ApproachInt(nextBorderColor.b, prevBorderColor.b, speedRatio), byte.MaxValue);
				}
			}
		}

		private GameColor getBorderColor(int id)
		{
			if (mapIdToColor.TryGetValue(id, out var value))
			{
				return value;
			}
			return mapIdToColor[1];
		}

		public void ShiftColorInstantly()
		{
			if (currentMapId != oneshotWindow.tileMapMan.GetMapID())
			{
				currentMapId = oneshotWindow.tileMapMan.GetMapID();
				CurrentBorderColor = getBorderColor(currentMapId);
			}
			if (isColorShifting)
			{
				CurrentBorderColor = nextBorderColor;
				isColorShifting = false;
			}
		}
	}
}

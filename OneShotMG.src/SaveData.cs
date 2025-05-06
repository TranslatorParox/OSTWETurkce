using System;
using System.Collections.Generic;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Map;
using OneShotMG.src.TWM;

namespace OneShotMG.src
{
	public class SaveData
	{
		public int? version;

		public string playerName;

		public WallpaperInfoSaveData currentOverridingWallpaper;

		public long playTimeFrameCount;

		public long? plightBruteForceStartFrame;

		public DateTime? plightTimeStart;

		public ulong[] flagData;

		public int[] varData;

		public List<PictureManager.PictureSaveData> pictureData;

		public SoundSaveData soundSaveData;

		public bool[] inventoryData;

		public List<string> selfSwitchData;

		public FastTravelManager.FastTravelSaveData fastTravelData;

		public TileMapSaveData tileMapSaveData;

		public List<FollowerManager.FollowerType> followers;
	}
}

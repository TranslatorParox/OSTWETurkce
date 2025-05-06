using System.Collections.Generic;
using OneShotMG.src.Entities;
using OneShotMG.src.Util;

namespace OneShotMG.src.Map
{
	public class TileMapSaveData
	{
		public float playerTileX;

		public float playerTileY;

		public Entity.Direction playerDirection;

		public string playerSheet;

		public int currentMap;

		public List<EntitySaveData> entitySaveDatas;

		public GameTone mapAmbientTone;

		public string mapBackground;

		public bool hasFireflyParticles;

		public EventRunnerData mainEventRunnerData;

		public PanoramaSaveData panoramaSaveData;

		public List<List<string>> overrideStepSounds;
	}
}

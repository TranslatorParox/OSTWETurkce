using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace OneShotMG.src
{
	public class FastTravelManager
	{
		public enum FastTravelZone
		{
			Blue = 1,
			Green,
			Red,
			RedGround
		}

		public class FastTravelLocation
		{
			public FastTravelZone zone;

			public int mapId;

			public Vec2 tilePos;

			public Entity.Direction direction;
		}

		public class FastTravelSaveData
		{
			public List<FastTravelLocation> unlockedLocations;

			public bool unlocked;

			public FastTravelZone currentZone;
		}

		private Dictionary<FastTravelZone, List<FastTravelLocation>> unlockedFastTravels;

		private Dictionary<FastTravelZone, string> zoneNames;

		private Dictionary<FastTravelZone, List<MinimapInfo>> minimapZones;

		private Dictionary<FastTravelZone, Dictionary<int, List<MinimapEdge>>> minimapZoneEdges;

		public bool Unlocked { get; private set; }

		public FastTravelZone CurrentZone { get; private set; }

		public FastTravelManager()
		{
			unlockedFastTravels = new Dictionary<FastTravelZone, List<FastTravelLocation>>();
			Unlocked = false;
			populateZoneNames();
			LoadMinimapInfo();
			LoadMinimapNodes();
		}

		private void populateZoneNames()
		{
			zoneNames = JsonConvert.DeserializeObject<Dictionary<FastTravelZone, string>>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_map_zone_names.json"));
		}

		private void LoadMinimapInfo()
		{
			MinimapMetadata minimapMetadata = JsonConvert.DeserializeObject<MinimapMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "oneshot_minimap_info.json")));
			minimapZones = minimapMetadata.zones;
		}

		private void LoadMinimapNodes()
		{
			MinimapNodeData minimapNodeData = JsonConvert.DeserializeObject<MinimapNodeData>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "oneshot_minimap_nodes.json")));
			minimapZoneEdges = minimapNodeData.zones;
		}

		public void DisableFastTravel()
		{
			Unlocked = false;
		}

		public void EnableFastTravel()
		{
			Unlocked = true;
		}

		public void UnlockFastTravelLocation(FastTravelLocation ftl)
		{
			unlockFastTravelLocation(ftl);
			Unlocked = true;
			CurrentZone = ftl.zone;
		}

		private void unlockFastTravelLocation(FastTravelLocation ftl)
		{
			if (!unlockedFastTravels.TryGetValue(ftl.zone, out var value))
			{
				value = new List<FastTravelLocation>();
				unlockedFastTravels.Add(ftl.zone, value);
			}
			if (!value.Any((FastTravelLocation f) => f.mapId == ftl.mapId))
			{
				value.Add(ftl);
			}
		}

		public string GetCurrentZoneName()
		{
			if (zoneNames.TryGetValue(CurrentZone, out var value))
			{
				return Game1.languageMan.GetZoneLocString(CurrentZone, value);
			}
			return Game1.languageMan.GetTWMLocString("fasttravel_no_zone");
		}

		public List<FastTravelLocation> GetCurrentFastTravels()
		{
			if (unlockedFastTravels.TryGetValue(CurrentZone, out var value))
			{
				return value;
			}
			return new List<FastTravelLocation>();
		}

		public List<MinimapInfo> GetCurrentMinimap()
		{
			if (minimapZones.TryGetValue(CurrentZone, out var value))
			{
				return value;
			}
			return new List<MinimapInfo>();
		}

		public Dictionary<int, List<MinimapEdge>> GetCurrentMinimapEdges()
		{
			if (minimapZoneEdges.TryGetValue(CurrentZone, out var value))
			{
				return value;
			}
			return new Dictionary<int, List<MinimapEdge>>();
		}

		public FastTravelSaveData GetSaveData()
		{
			FastTravelSaveData fastTravelSaveData = new FastTravelSaveData();
			fastTravelSaveData.unlockedLocations = new List<FastTravelLocation>();
			foreach (List<FastTravelLocation> value in unlockedFastTravels.Values)
			{
				fastTravelSaveData.unlockedLocations.AddRange(value);
			}
			fastTravelSaveData.currentZone = CurrentZone;
			fastTravelSaveData.unlocked = Unlocked;
			return fastTravelSaveData;
		}

		public void LoadSaveData(FastTravelSaveData data)
		{
			unlockedFastTravels.Clear();
			if (data == null)
			{
				return;
			}
			foreach (FastTravelLocation unlockedLocation in data.unlockedLocations)
			{
				unlockFastTravelLocation(unlockedLocation);
			}
			Unlocked = data.unlocked;
			CurrentZone = data.currentZone;
		}
	}
}

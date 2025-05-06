using System.Collections.Generic;

namespace OneShotMG.src
{
	internal class MinimapMetadata
	{
		public const string PATH = "oneshot_minimap_info.json";

		public Dictionary<FastTravelManager.FastTravelZone, List<MinimapInfo>> zones;
	}
}

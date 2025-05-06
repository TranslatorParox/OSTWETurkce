using System.Collections.Generic;

namespace OneShotMG.src
{
	public class MinimapNodeData
	{
		public const string PATH = "oneshot_minimap_nodes.json";

		public Dictionary<FastTravelManager.FastTravelZone, Dictionary<int, List<MinimapEdge>>> zones;
	}
}

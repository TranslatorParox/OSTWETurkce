using Newtonsoft.Json;

namespace OneShotMG.src
{
	public class MinimapEdge
	{
		[JsonProperty]
		public readonly int id;

		[JsonProperty]
		public readonly EdgeDirection direction;
	}
}

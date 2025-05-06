using Newtonsoft.Json;

namespace OneShotMG.src
{
	public class MinimapInfo
	{
		[JsonProperty]
		public readonly int MapId;

		[JsonProperty]
		public readonly Vec2 Offset;

		[JsonProperty]
		public readonly string Image;

		[JsonProperty]
		public readonly Vec2 WarpPoint;

		[JsonIgnore]
		public bool HasWarp
		{
			get
			{
				if (WarpPoint.X != 0)
				{
					return WarpPoint.Y != 0;
				}
				return false;
			}
		}
	}
}

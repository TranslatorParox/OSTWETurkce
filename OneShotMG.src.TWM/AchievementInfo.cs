using Newtonsoft.Json;

namespace OneShotMG.src.TWM
{
	public class AchievementInfo
	{
		[JsonProperty]
		public readonly string id;

		[JsonProperty]
		public readonly string title;

		[JsonProperty]
		public readonly string description;

		[JsonProperty]
		public readonly bool shownByDefault = true;
	}
}

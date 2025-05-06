using System.Collections.Generic;
using Newtonsoft.Json;

namespace OneShotMG.src.TWM
{
	public class CharProfileInfo
	{
		[JsonProperty]
		public readonly string unlockId;

		[JsonProperty]
		public readonly string title;

		[JsonProperty]
		public readonly string subtitle;

		[JsonProperty]
		public readonly string bgImage;

		[JsonProperty]
		public readonly string walkspriteId;

		[JsonProperty]
		public readonly List<string> facepics;

		[JsonProperty]
		public readonly string infoText;

		[JsonProperty]
		public readonly bool singleSprite;

		[JsonProperty]
		public readonly bool walkAnimation = true;

		[JsonProperty]
		public readonly string uxColorTheme = "white";

		[JsonProperty]
		public readonly bool textDropShadow = true;
	}
}

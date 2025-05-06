using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace OneShotMG.src.TWM
{
	public class GalleryInfo
	{
		[JsonProperty(Required = Required.Always)]
		public readonly string imageId;

		[JsonProperty]
		public readonly List<string> additionalLayers;

		[JsonProperty(Required = Required.Always)]
		public readonly string displayName;

		[JsonProperty(Required = Required.Always)]
		public readonly int displayOrder;

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(2)]
		public readonly int scale;

		[JsonProperty]
		public readonly float scrollbarX;

		[JsonProperty]
		public readonly float scrollbarY;

		[JsonProperty]
		public readonly bool oversize;

		[JsonProperty]
		public Vec2 overrideWindowSize;

		[JsonProperty]
		public bool fitHorizontal;

		[JsonProperty]
		public string fullscreenBlendMode;

		public bool HasCustomWindowSize => overrideWindowSize.X > 0;

		public GalleryInfo(string imageId, List<string> additionalLayers, string displayName, int displayOrder, int scale)
		{
			this.imageId = imageId;
			this.additionalLayers = additionalLayers;
			this.displayName = displayName;
			this.displayOrder = displayOrder;
			this.scale = scale;
		}
	}
}

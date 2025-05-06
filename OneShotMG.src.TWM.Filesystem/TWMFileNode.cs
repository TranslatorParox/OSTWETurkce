using Newtonsoft.Json;

namespace OneShotMG.src.TWM.Filesystem
{
	public abstract class TWMFileNode
	{
		public string parentPath;

		[JsonProperty]
		public readonly string name;

		[JsonProperty]
		public readonly string icon;

		public bool deleteRestricted;

		public bool moveRestricted;

		protected TWMFileNode(string icon, string name)
		{
			this.name = name;
			this.icon = icon;
		}
	}
}

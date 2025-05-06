using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OneShotMG.src.TWM.Filesystem
{
	public class TWMFile : TWMFileNode
	{
		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public readonly LaunchableWindowType program;

		[JsonProperty]
		public readonly string[] argument;

		public TWMFile(string icon, string name, LaunchableWindowType type, params string[] args)
			: base(icon, name)
		{
			program = type;
			argument = args;
		}
	}
}

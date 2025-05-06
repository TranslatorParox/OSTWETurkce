using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace OneShotMG.src.EngineSpecificCode
{
	public class KeyGlyphMap
	{
		[JsonProperty]
		public Dictionary<Keys, string> KeysToGlyphes { get; private set; }

		public static KeyGlyphMap LoadGlyphMap()
		{
			return JsonConvert.DeserializeObject<KeyGlyphMap>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "key_glyph_map.json")));
		}
	}
}

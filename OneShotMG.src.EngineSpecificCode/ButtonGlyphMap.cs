using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace OneShotMG.src.EngineSpecificCode
{
	public class ButtonGlyphMap
	{
		[JsonProperty]
		public Dictionary<Buttons, string> ButtonsToGlyphes { get; private set; }

		public static ButtonGlyphMap LoadGlyphMap()
		{
			string path = "button_glyph_map_xbox.json";
			if (Game1.steamMan.IsOnSteamDeck)
			{
				path = "button_glyph_map_steamdeck.json";
			}
			return JsonConvert.DeserializeObject<ButtonGlyphMap>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), path)));
		}
	}
}

using System.Collections.Generic;
using Tiled;

namespace OneShotMG.src.Map
{
	public class TileInfo
	{
		public bool collision;

		public List<string> stepSounds;

		public int height;

		public void Read(Tile tile)
		{
			Property[] properties = tile.properties;
			foreach (Property property in properties)
			{
				string text = property.value;
				if (property.value == null)
				{
					text = property.text;
				}
				string text2 = property.name.ToLowerInvariant();
				if (!(text2 == "collision"))
				{
					if (text2 == "step_sounds")
					{
						stepSounds = new List<string>(text.Split('\n'));
					}
				}
				else
				{
					collision = bool.Parse(text);
				}
			}
		}
	}
}

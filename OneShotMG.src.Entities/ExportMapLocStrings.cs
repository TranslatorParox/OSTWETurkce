using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Map;
using OneShotMG.src.Menus;
using OneShotMG.src.TWM;

namespace OneShotMG.src.Entities
{
	public class ExportMapLocStrings
	{
		private static readonly int[] excludedMaps = new int[9] { 11, 244, 248, 256, 250, 251, 252, 257, 96 };

		public static void ExportMapStrings()
		{
			ExportMapEventStrings();
			ExportCommonEventStrings();
			ExportItemStrings();
			ExportMapNameStrings();
			ExportMapZoneNameStrings();
			ExportCGNameStrings();
			ExportContactStrings();
			ExportAchievementStrings();
			ExportThemeStrings();
			ExportWallpaperStrings();
			ExportMusicStrings();
			Game1.logMan.Log(LogManager.LogLevel.Info, "exported loc strings");
		}

		public static void ExportMapEventStrings()
		{
			HashSet<int> hashSet = new HashSet<int>(excludedMaps);
			for (int i = 1; i <= 263; i++)
			{
				if (hashSet.Contains(i))
				{
					continue;
				}
				string text = $"map{i}";
				MapEvents mapEvents = JsonConvert.DeserializeObject<MapEvents>(File.ReadAllText(Game1.GameDataPath() + "/maps/events_" + text + ".json"));
				string text2 = "";
				Event[] events = mapEvents.events;
				foreach (Event @event in events)
				{
					for (int k = 0; k < @event.pages.Length; k++)
					{
						Event.Page page = @event.pages[k];
						text2 += generateStringsForList(page.list);
					}
				}
				if (!string.IsNullOrEmpty(text2))
				{
					File.WriteAllText(Game1.GameDataPath() + "/loc/template/maps/eventstrs_" + text + ".po", text2);
				}
			}
		}

		public static void ExportCommonEventStrings()
		{
			CommonEvents commonEvents = JsonConvert.DeserializeObject<CommonEvents>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_common_events.json"));
			string text = "";
			CommonEvent[] common_events = commonEvents.common_events;
			foreach (CommonEvent commonEvent in common_events)
			{
				text += generateStringsForList(commonEvent.list);
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/common_event_strs.po", text);
		}

		public static void ExportItemStrings()
		{
			ItemsData ıtemsData = JsonConvert.DeserializeObject<ItemsData>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_items.json"));
			string text = "";
			ItemData[] items = ıtemsData.items;
			foreach (ItemData ıtemData in items)
			{
				string text2 = $"{ıtemData.id}:name";
				text = text + "msgid " + JsonConvert.ToString(text2 + "=" + ıtemData.name) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(ıtemData.name) + "\n\n";
				string text3 = $"{ıtemData.id}:description";
				text = text + "msgid " + JsonConvert.ToString(text3 + "=" + ıtemData.description) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(ıtemData.description) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/item_strs.po", text);
		}

		public static void ExportMapNameStrings()
		{
			string text = "";
			HashSet<int> hashSet = new HashSet<int>();
			foreach (List<MinimapInfo> value in JsonConvert.DeserializeObject<MinimapMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "oneshot_minimap_info.json"))).zones.Values)
			{
				foreach (MinimapInfo item in value)
				{
					hashSet.Add(item.MapId);
				}
			}
			foreach (MapNameJson map_name in JsonConvert.DeserializeObject<MapNamesJson>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_map_names.json")).map_names)
			{
				if (hashSet.Contains(map_name.id))
				{
					text = text + "msgid " + JsonConvert.ToString(map_name.id + "=" + map_name.name) + "\n";
					text = text + "msgstr " + JsonConvert.ToString(map_name.name) + "\n\n";
				}
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/map_name_strs.po", text);
		}

		public static void ExportMapZoneNameStrings()
		{
			string text = "";
			foreach (KeyValuePair<FastTravelManager.FastTravelZone, string> item in JsonConvert.DeserializeObject<Dictionary<FastTravelManager.FastTravelZone, string>>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_map_zone_names.json")))
			{
				text = text + "msgid " + JsonConvert.ToString(item.Key.ToString() + "=" + item.Value) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(item.Value) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/map_zone_name_strs.po", text);
		}

		public static void ExportCGNameStrings()
		{
			string text = "";
			foreach (GalleryInfo cg in JsonConvert.DeserializeObject<GalleryInfoMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/cg_unlocks.json"))).cgList)
			{
				text = text + "msgid " + JsonConvert.ToString(cg.imageId + "=" + cg.displayName) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(cg.displayName) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/twm/cg_name_strs.po", text);
		}

		public static void ExportContactStrings()
		{
			string text = "";
			foreach (CharProfileInfo profile in JsonConvert.DeserializeObject<CharacterProfileMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/contacts_metadata.json"))).profiles)
			{
				text = text + "msgid " + JsonConvert.ToString(profile.unlockId + ":title=" + profile.title) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(profile.title) + "\n\n";
				text = text + "msgid " + JsonConvert.ToString(profile.unlockId + ":subtitle=" + profile.subtitle) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(profile.subtitle) + "\n\n";
				text = text + "msgid " + JsonConvert.ToString(profile.unlockId + ":infoText=" + profile.infoText) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(profile.infoText) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/twm/contacts_strs.po", text);
		}

		public static void ExportAchievementStrings()
		{
			string text = "";
			foreach (AchievementInfo achievement in JsonConvert.DeserializeObject<AchievementsMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/achievements_metadata.json"))).achievements)
			{
				text = text + "msgid " + JsonConvert.ToString(achievement.id + ":title=" + achievement.title) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(achievement.title) + "\n\n";
				text = text + "msgid " + JsonConvert.ToString(achievement.id + ":description=" + achievement.description) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(achievement.description) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/twm/achievements_strs.po", text);
		}

		public static void ExportThemeStrings()
		{
			string text = "";
			TWMTheme[] themes = JsonConvert.DeserializeObject<TWMThemeData>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/themes_metadata.json"))).themes;
			foreach (TWMTheme tWMTheme in themes)
			{
				text = text + "msgid " + JsonConvert.ToString(tWMTheme.id + "=" + tWMTheme.displayName) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(tWMTheme.displayName) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/twm/themes_strs.po", text);
		}

		public static void ExportWallpaperStrings()
		{
			string text = "";
			WallpaperInfoSaveData[] wallpapers = JsonConvert.DeserializeObject<WallpaperMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/wallpapers_metadata.json"))).wallpapers;
			foreach (WallpaperInfoSaveData wallpaperInfoSaveData in wallpapers)
			{
				text = text + "msgid " + JsonConvert.ToString(wallpaperInfoSaveData.imageFile + "=" + wallpaperInfoSaveData.displayName) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(wallpaperInfoSaveData.displayName) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/twm/wallpaper_strs.po", text);
		}

		public static void ExportMusicStrings()
		{
			string text = "";
			MusicPlayerTrack[] tracklist = JsonConvert.DeserializeObject<MusicTrackMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "music/tracklist.json"))).tracklist;
			foreach (MusicPlayerTrack musicPlayerTrack in tracklist)
			{
				text = text + "msgid " + JsonConvert.ToString(musicPlayerTrack.trackId + "=" + musicPlayerTrack.displayName) + "\n";
				text = text + "msgstr " + JsonConvert.ToString(musicPlayerTrack.displayName) + "\n\n";
			}
			File.WriteAllText(Game1.GameDataPath() + "/loc/template/music/music_strs.po", text);
		}

		private static string extractEdTextString(string script)
		{
			script = script.Substring("EdText.".Length);
			if (script.StartsWith("info("))
			{
				script = script.Substring("info(".Length);
			}
			else if (script.StartsWith("err("))
			{
				script = script.Substring("err(".Length);
			}
			else if (script.StartsWith("yesno("))
			{
				script = script.Substring("yesno(".Length);
			}
			script = script.Substring(0, script.LastIndexOf(")"));
			script = JsonConvert.DeserializeObject<string>(script);
			return script.Trim();
		}

		private static string generateStringsForList(EventCommand[] list)
		{
			string text = "";
			for (int i = 0; i < list.Length; i++)
			{
				EventCommand eventCommand = list[i];
				bool flag = false;
				string text2 = "";
				switch ((EventRunner.EventCommandCode)eventCommand.code)
				{
				case EventRunner.EventCommandCode.ShowText:
				case EventRunner.EventCommandCode.ShowChoices:
				case EventRunner.EventCommandCode.MoreText:
					flag = true;
					text2 = eventCommand.parameters[0];
					break;
				case EventRunner.EventCommandCode.Script:
					if (eventCommand.parameters[0].StartsWith("EdText"))
					{
						flag = true;
						text2 = eventCommand.parameters[0].Trim();
						int num = i;
						while (list.Length > num + 1 && list[num + 1].code == 655)
						{
							num++;
							eventCommand = list[num];
							text2 = text2 + " " + eventCommand.parameters[0].Trim();
						}
						text2 = extractEdTextString(text2);
					}
					break;
				case EventRunner.EventCommandCode.ConditionalBranch:
					if (int.Parse(eventCommand.parameters[0], CultureInfo.InvariantCulture) == 12 && eventCommand.parameters[1].StartsWith("EdText"))
					{
						flag = true;
						text2 = eventCommand.parameters[1];
						text2 = extractEdTextString(text2);
					}
					break;
				}
				if (flag && !string.IsNullOrEmpty(text2))
				{
					text = text + "msgid " + JsonConvert.ToString(text2) + "\n";
					text = text + "msgstr " + JsonConvert.ToString(text2) + "\n\n";
				}
			}
			return text;
		}
	}
}

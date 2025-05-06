using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class LanguageManager
	{
		public enum ItemStringType
		{
			name,
			description
		}

		public enum ContactStringType
		{
			title,
			subtitle,
			infoText
		}

		public enum AchievementStringType
		{
			title,
			description
		}

		private Dictionary<string, LanguageMetaData> languageMetadatas;

		private LanguageMetaData currentLanguageMetadata;

		private Dictionary<string, string> mapLocStrings;

		private Dictionary<string, string> mapLocStringsPrevMap;

		private Dictionary<string, string> commonEventLocStrings;

		private Dictionary<string, string> itemLocStrings;

		private Dictionary<string, string> mapNameLocStrings;

		private Dictionary<string, string> zoneLocStrings;

		private Dictionary<string, string> cgNameLocStrings;

		private Dictionary<string, string> contactsLocStrings;

		private Dictionary<string, string> themesLocStrings;

		private Dictionary<string, string> wallpaperLocStrings;

		private Dictionary<string, string> musicLocStrings;

		private Dictionary<string, string> achievementLocStrings;

		private Dictionary<string, string> twmLocStrings;

		private const string LANGUAGE_SETTING_SAVE_FILE = "language.dat";

		public bool WasLanguageSettingPresentOnStartup { get; private set; } = true;

		public LanguageManager()
		{
			LoadLanguageMetaData();
			string newLangCode = LoadSavedLanguageCode();
			SetCurrentLangCode(newLangCode, saveCodeToFile: false);
		}

		private string LoadSavedLanguageCode()
		{
			string text = Game1.masterSaveMan.LoadFile("language.dat", LoadLangcodeFromString, verifyLangCode, null);
			if (text == null)
			{
				WasLanguageSettingPresentOnStartup = false;
				return GetDefaultLanguageCode();
			}
			return text;
		}

		private static string LoadLangcodeFromString(string data)
		{
			return data.Trim();
		}

		private bool verifyLangCode(string langCode)
		{
			return languageMetadatas.ContainsKey(langCode);
		}

		private void SaveLanguageSetting()
		{
			Game1.masterSaveMan.WriteFile(new SaveRequest("language.dat", GetCurrentLangCode()));
		}

		private string GetDefaultLanguageCode()
		{
			string langCode = Game1.steamMan.GetLangCode();
			if (!string.IsNullOrEmpty(langCode))
			{
				return langCode;
			}
			string[] array = CultureInfo.CurrentUICulture.Name.Split('-');
			if (array.Length >= 1)
			{
				switch (array[0].ToLowerInvariant())
				{
				default:
					return "en";
				case "es":
					return "es";
				case "fr":
					return "fr";
				case "it":
					return "it";
				case "ja":
					return "ja";
				case "ko":
					return "ko";
				case "pt":
					return "pt_br";
				case "ru":
					return "ru";
				case "zh":
					if (array.Length == 2)
					{
						switch (array[1].ToLowerInvariant())
						{
						default:
							return "zh_cn";
						case "tw":
						case "cht":
							return "zh_cht";
						}
					}
					return "zh_cn";
				}
			}
			return languageMetadatas.Values.First().lang_code;
		}

		public string GetCurrentFontOS()
		{
			return currentLanguageMetadata.font_os;
		}

		public string GetCurrentFontGame()
		{
			return currentLanguageMetadata.font_game;
		}

		public string GetCurrentFontGameSmall()
		{
			return currentLanguageMetadata.font_game_small;
		}

		public int GetCurrentFontOSScale()
		{
			return currentLanguageMetadata.font_os_scale;
		}

		public string GetCurrentLangCode()
		{
			return currentLanguageMetadata.lang_code;
		}

		public void SetCurrentLangCode(string newLangCode, bool saveCodeToFile = true)
		{
			if (languageMetadatas.TryGetValue(newLangCode, out var value))
			{
				currentLanguageMetadata = value;
				LoadLanguageFiles();
				if (saveCodeToFile)
				{
					SaveLanguageSetting();
				}
			}
			else
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, "tried to set language code that's not found in language metadata: " + newLangCode);
			}
		}

		private void LoadLanguageFiles()
		{
			string text = "/loc/" + currentLanguageMetadata.lang_code;
			commonEventLocStrings = LoadLocFile(text + "/common_event_strs.po", parseKeys: false);
			itemLocStrings = LoadLocFile(text + "/item_strs.po");
			mapNameLocStrings = LoadLocFile(text + "/map_name_strs.po");
			zoneLocStrings = LoadLocFile(text + "/map_zone_name_strs.po");
			cgNameLocStrings = LoadLocFile(text + "/twm/cg_name_strs.po");
			contactsLocStrings = LoadLocFile(text + "/twm/contacts_strs.po");
			themesLocStrings = LoadLocFile(text + "/twm/themes_strs.po");
			wallpaperLocStrings = LoadLocFile(text + "/twm/wallpaper_strs.po");
			musicLocStrings = LoadLocFile(text + "/music/music_strs.po");
			achievementLocStrings = LoadLocFile(text + "/twm/achievements_strs.po");
			twmLocStrings = LoadLocFile(text + "/twm/twm_strs.po");
			mapLocStrings = null;
			mapLocStringsPrevMap = null;
		}

		private void LoadLanguageMetaData()
		{
			LanguageMetaDatas languageMetaDatas = JsonConvert.DeserializeObject<LanguageMetaDatas>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "loc/language_metadata.json")));
			languageMetadatas = new Dictionary<string, LanguageMetaData>();
			LanguageMetaData[] languages = languageMetaDatas.languages;
			foreach (LanguageMetaData languageMetaData in languages)
			{
				languageMetadatas.Add(languageMetaData.lang_code, languageMetaData);
			}
		}

		private Dictionary<string, string> LoadLocFile(string filePath, bool parseKeys = true)
		{
			string text = Game1.GameDataPath() + filePath;
			if (!File.Exists(text))
			{
				return null;
			}
			return SimplePOReader.ReadPOFile(text, parseKeys);
		}

		public List<(string k, string displayName)> GetLanguageOptions()
		{
			List<(string, string)> list = new List<(string, string)>();
			foreach (LanguageMetaData value in languageMetadatas.Values)
			{
				list.Add((value.lang_code, value.display_name));
			}
			return list;
		}

		public void LoadMapLocFile(int currentMapId)
		{
			mapLocStringsPrevMap = mapLocStrings;
			string filePath = $"/loc/{currentLanguageMetadata.lang_code}/maps/eventstrs_map{currentMapId}.po";
			mapLocStrings = LoadLocFile(filePath, parseKeys: false);
		}

		public string GetItemLocString(int item_id, ItemStringType itemStringType, string originalString)
		{
			string key = $"{item_id}:{itemStringType}";
			return GetLocString(itemLocStrings, key, originalString);
		}

		public string GetMapLocString(int pageId, string originalString)
		{
			if (pageId >= 0)
			{
				string locString = GetLocString(mapLocStrings, originalString, originalString);
				if (locString == originalString && mapLocStringsPrevMap != null)
				{
					locString = GetLocString(mapLocStringsPrevMap, originalString, originalString);
				}
				return locString;
			}
			return GetLocString(commonEventLocStrings, originalString, originalString);
		}

		public string GetLocString(Dictionary<string, string> locMap, string key, string originalString)
		{
			if (locMap != null && locMap.TryGetValue(key, out var value))
			{
				return value;
			}
			return originalString;
		}

		public string GetMapNameLocString(int mapId, string originalString)
		{
			return GetLocString(mapNameLocStrings, mapId.ToString(), originalString);
		}

		public string GetZoneLocString(FastTravelManager.FastTravelZone zone, string originalString)
		{
			return GetLocString(zoneLocStrings, zone.ToString(), originalString);
		}

		public string GetCgNameLocString(string imageId, string originalString)
		{
			return GetLocString(cgNameLocStrings, imageId, originalString);
		}

		public string GetContactLocString(string unlockId, ContactStringType type, string originalString)
		{
			string key = $"{unlockId}:{type}";
			return GetLocString(contactsLocStrings, key, originalString);
		}

		public string GetAchievementLocString(string id, AchievementStringType type, string originalString)
		{
			string key = $"{id}:{type}";
			return GetLocString(achievementLocStrings, key, originalString);
		}

		public string GetThemesLocString(string id, string originalString)
		{
			return GetLocString(themesLocStrings, id, originalString);
		}

		public string GetWallpaperLocString(string imageFile, string originalString)
		{
			return GetLocString(wallpaperLocStrings, imageFile, originalString);
		}

		public string GetMusicLocString(string trackId, string originalString)
		{
			return GetLocString(musicLocStrings, trackId, originalString);
		}

		public string GetTWMLocString(string id)
		{
			return GetLocString(twmLocStrings, id, id);
		}
	}
}

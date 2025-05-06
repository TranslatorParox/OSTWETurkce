using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Newtonsoft.Json;
using OneShotMG.src.Util;

namespace OneShotMG.src.Entities
{
	public class MapOldPoToNewPoFiles
	{
		private const string templateMapsFolder = "/loc/template/maps";

		private const string editedStringsPoFilePath = "C:/Users/GIR/Dropbox/OneshotMG/mapForEditedStrings.po";

		public static void DoPartialMapping(string oldPoFile)
		{
			Dictionary<string, string> dictionary = SimplePOReader.ReadPOFile("C:/Users/GIR/Dropbox/OneshotMG/mapForEditedStrings.po", parseMsgId: false);
			Dictionary<string, string> dictionary2 = SimplePOReader.ReadPOFile(oldPoFile, parseMsgId: false);
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
			foreach (string key2 in dictionary2.Keys)
			{
				dictionary3[key2.Replace(" ", "")] = key2;
			}
			string text = "";
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				text = text + "msgid " + JsonConvert.SerializeObject(item.Key) + "\n";
				if (item.Value != "a")
				{
					text = text + "msgstr " + JsonConvert.SerializeObject(item.Value) + "\n\n";
					continue;
				}
				string key = item.Key.Replace(" ", "");
				text = ((!dictionary3.TryGetValue(key, out var value)) ? (text + "msgstr " + JsonConvert.SerializeObject("asdf") + "\n\n") : (text + "msgstr " + JsonConvert.SerializeObject(value) + "\n\n"));
			}
			File.WriteAllText("C:/Users/GIR/Dropbox/OneshotMG/mapForEditedStringsPatched.po", text);
		}

		public static void DoMapping(string languageCode, string oldPoFile)
		{
			string text = "/loc/" + languageCode + "/maps";
			Directory.CreateDirectory(Game1.GameDataPath() + text);
			Dictionary<string, string> editedStrsMap = SimplePOReader.ReadPOFile("C:/Users/GIR/Dropbox/OneshotMG/mapForEditedStrings.po", parseMsgId: false);
			Dictionary<string, string> oldPoMap = SimplePOReader.ReadPOFile(oldPoFile, parseMsgId: false);
			string[] files = Directory.GetFiles(Game1.GameDataPath() + "/loc/template/maps");
			foreach (string obj in files)
			{
				string text2 = obj.Replace("\\", "/");
				string destPoFilePath = string.Concat(str3: text2.Substring(text2.LastIndexOf("/") + 1), str0: Game1.GameDataPath(), str1: text, str2: "/");
				patchPoFile(obj, destPoFilePath, editedStrsMap, oldPoMap);
			}
			patchPoFile(Game1.GameDataPath() + "/loc/template/common_event_strs.po", Game1.GameDataPath() + "/loc/" + languageCode + "/common_event_strs.po", editedStrsMap, oldPoMap);
			patchPoFile(Game1.GameDataPath() + "/loc/template/item_strs.po", Game1.GameDataPath() + "/loc/" + languageCode + "/item_strs.po", editedStrsMap, oldPoMap);
			patchPoFile(Game1.GameDataPath() + "/loc/template/map_name_strs.po", Game1.GameDataPath() + "/loc/" + languageCode + "/map_name_strs.po", editedStrsMap, oldPoMap, lookForLowercaseKeys: true);
		}

		public static void SanityCheckPoFiles(string lang_code)
		{
			string[] files = Directory.GetFiles(Game1.GameDataPath() + "/loc/" + lang_code, "*.po", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				try
				{
					SimplePOReader.ReadPOFile(text, parseMsgId: false);
				}
				catch (Exception ex)
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, "Error reading file " + text + " : " + ex.Message);
				}
			}
		}

		public static void BuildResxFile(string lang_code)
		{
			string text = Game1.GameDataPath() + "/loc/" + lang_code;
			HashSet<char> hashSet = new HashSet<char>();
			List<string> list = new List<string>();
			string[] files = Directory.GetFiles(text, "*.po", SearchOption.AllDirectories);
			string[] files2 = Directory.GetFiles(text, "*.txt", SearchOption.AllDirectories);
			list.AddRange(files);
			list.AddRange(files2);
			foreach (string item2 in list)
			{
				using (StreamReader streamReader = new StreamReader(item2))
				{
					for (string text2 = streamReader.ReadLine(); text2 != null; text2 = streamReader.ReadLine())
					{
						string text3 = text2;
						foreach (char item in text3)
						{
							hashSet.Add(item);
						}
					}
				}
			}
			List<char> list2 = hashSet.ToList();
			list2.Sort();
			string str = new string(list2.ToArray());
			str = SecurityElement.Escape(str);
			string text4 = "<?xml version=\"1.0\" encoding=\"utf-8\"?><root><data name=\"alltext\" xml:space=\"preserve\"><value>";
			text4 += str;
			text4 += "</value></data></root>";
			File.WriteAllText(text + "/chars.resx", text4);
		}

		private static void patchPoFile(string tmpltPoFilePath, string destPoFilePath, Dictionary<string, string> editedStrsMap, Dictionary<string, string> oldPoMap, bool lookForLowercaseKeys = false)
		{
			string text = "";
			Dictionary<string, string> dictionary = SimplePOReader.ReadPOFile(tmpltPoFilePath, parseMsgId: false);
			foreach (string key in dictionary.Keys)
			{
				string text2 = dictionary[key];
				if (string.IsNullOrEmpty(text2))
				{
					continue;
				}
				string value = string.Empty;
				if (text2.StartsWith("[\"") && text2.EndsWith("\"]"))
				{
					List<string> list = JsonConvert.DeserializeObject<List<string>>(text2);
					List<string> list2 = new List<string>();
					foreach (string item in list)
					{
						if (!oldPoMap.TryGetValue(item, out var value2))
						{
							if (editedStrsMap.TryGetValue(item, out var value3))
							{
								if (!oldPoMap.TryGetValue(value3, out value2))
								{
									Game1.logMan.Log(LogManager.LogLevel.Info, "in " + tmpltPoFilePath + ", couldn't find value for choice: " + JsonConvert.SerializeObject(item));
									value2 = item;
								}
							}
							else
							{
								Game1.logMan.Log(LogManager.LogLevel.Info, "in " + tmpltPoFilePath + ", couldn't find value for choice: " + JsonConvert.SerializeObject(item));
								value2 = item;
							}
						}
						list2.Add(value2);
					}
					value = JsonConvert.SerializeObject(list2);
				}
				else
				{
					bool flag = false;
					if (!oldPoMap.TryGetValue(text2, out value))
					{
						if (editedStrsMap.TryGetValue(text2, out var value4) && oldPoMap.TryGetValue(value4, out value))
						{
							flag = true;
						}
					}
					else
					{
						flag = true;
					}
					if (!flag && lookForLowercaseKeys && oldPoMap.TryGetValue(text2.ToLowerInvariant(), out value))
					{
						flag = true;
					}
					if (!flag)
					{
						Game1.logMan.Log(LogManager.LogLevel.Info, "in " + tmpltPoFilePath + ", couldn't find value for: " + JsonConvert.SerializeObject(text2));
						value = string.Empty;
					}
				}
				text = text + "msgid " + JsonConvert.SerializeObject(key) + "\n";
				text = text + "msgstr " + JsonConvert.SerializeObject(value) + "\n\n";
			}
			File.WriteAllText(destPoFilePath, text);
		}

		public static void MapPartialPo(string srcPoPath, string partialPoPath, string destPoPath)
		{
			Dictionary<string, string> dictionary = SimplePOReader.ReadPOFile(partialPoPath, parseMsgId: false);
			string text = string.Empty;
			using (StreamReader streamReader = new StreamReader(srcPoPath))
			{
				for (string text2 = streamReader.ReadLine(); text2 != null; text2 = streamReader.ReadLine())
				{
					if (string.IsNullOrWhiteSpace(text2))
					{
						text += "\n";
					}
					else if (text2.StartsWith("#"))
					{
						text = text + text2 + "\n";
					}
					else if (text2.StartsWith("msgid "))
					{
						text = text + text2 + "\n";
						string value = text2.Substring("msgid ".Length);
						value = JsonConvert.DeserializeObject<string>(value);
						if (!dictionary.TryGetValue(value, out var value2))
						{
							value2 = string.Empty;
						}
						text = text + "msgstr " + JsonConvert.SerializeObject(value2) + "\n";
					}
				}
			}
			File.WriteAllText(destPoPath, text);
		}

		public static void CheckPoFacepics(string srcPoPath)
		{
			foreach (string value in SimplePOReader.ReadPOFile(srcPoPath, parseMsgId: false).Values)
			{
				string text = value;
				if (string.IsNullOrEmpty(text) || !text.StartsWith("@"))
				{
					continue;
				}
				text = text.Substring(1);
				int num = text.IndexOf(' ');
				if (num >= 1)
				{
					string face = text.Substring(0, num);
					text = text.Substring(num);
					CheckIfFacepicExists(face, value);
				}
				for (int num2 = text.IndexOf("\\@"); num2 >= 0; num2 = text.IndexOf("\\@"))
				{
					text = text.Substring(num2 + 2);
					num = text.IndexOf(' ');
					if (num >= 1)
					{
						string face2 = text.Substring(0, num);
						text = text.Substring(num);
						CheckIfFacepicExists(face2, value);
					}
				}
			}
		}

		public static void CheckIfFacepicExists(string face, string originalLine)
		{
			switch (face.ToLowerInvariant())
			{
			case "desktop":
				return;
			case "credits":
				return;
			case "tut":
				return;
			}
			if (!File.Exists(Game1.Config["paths"]["content"] + "/facepics/" + face + ".xnb"))
			{
				Game1.logMan.Log(LogManager.LogLevel.Warning, "bad facepic '" + face + "' in line: " + originalLine);
			}
		}
	}
}

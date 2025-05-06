using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace OneShotMG.src.Util
{
	public class SimplePOReader
	{
		public static Dictionary<string, string> ReadPOFile(string filePath, bool parseMsgId = true)
		{
			int num = -1;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			try
			{
				using (StreamReader streamReader = new StreamReader(filePath))
				{
					string text = null;
					for (string text2 = streamReader.ReadLine(); text2 != null; text2 = streamReader.ReadLine())
					{
						num++;
						if (!text2.StartsWith("#"))
						{
							if (text2.StartsWith("msgid "))
							{
								if (!string.IsNullOrEmpty(text))
								{
									throw new Exception($"Line {num} - 2 keys found in a row, likely missing a msgstr: " + text2);
								}
								text2 = text2.Substring("msgid ".Length);
								string text3 = JsonConvert.DeserializeObject<string>(text2);
								while ((ushort)streamReader.Peek() == 34)
								{
									num++;
									text3 += JsonConvert.DeserializeObject<string>(streamReader.ReadLine());
								}
								if (parseMsgId)
								{
									int num2 = text3.IndexOf('=');
									if (num2 < 0)
									{
										if (!(text3 == ""))
										{
											throw new Exception($"Line {num} - improperly formatted msgid: " + text2);
										}
									}
									else
									{
										text = text3.Substring(0, num2);
									}
								}
								else if (!(text3 == ""))
								{
									text = text3;
								}
							}
							else if (text2.StartsWith("msgstr "))
							{
								string text4 = JsonConvert.DeserializeObject<string>(text2.Substring("msgstr ".Length));
								while ((ushort)streamReader.Peek() == 34)
								{
									num++;
									text4 += JsonConvert.DeserializeObject<string>(streamReader.ReadLine());
								}
								if (!string.IsNullOrEmpty(text))
								{
									dictionary[text] = text4;
									text = null;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, "Failed to read PO file " + filePath + ": " + ex.Message);
			}
			return dictionary;
		}
	}
}

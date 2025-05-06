using System.Collections.Generic;
using System.IO;
using System.Linq;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	public class DocumentWindow : TWMWindow
	{
		private class DocumentLine
		{
			public bool HasGlitchedLetters;

			public string line;

			public List<DocumentLetter> letters;
		}

		public const string DOC_SAFE_PUZZLE = "document";

		public const string DOC_SAFE_POSTGAME = "document_postgame";

		private const string DOC_RANDOM_LETTERS_FILE = "random_letters";

		private const GraphicsManager.FontType DocumentFont = GraphicsManager.FontType.GameSmall;

		private const string DOC_CODE_PLACEHOLDER = "{CODE}";

		private const int DOC_WINDOW_CONTENT_WIDTH = 450;

		private const int DOC_WINDOW_CONTENT_HEIGHT = 232;

		private const int DOC_WINDOW_TEXT_MARGIN_X = 4;

		private const int DOC_WINDOW_TEXT_MARGIN_Y = 8;

		private const int DOC_WINDOW_TEXT_VERTICAL_SPACING = 18;

		private const int DOC_WINDOW_TEXT_AREA_WIDTH = 424;

		private const int MAX_DISPLAYED_LINES = 12;

		private const int SLIDER_MARGIN = 2;

		private readonly string docFile;

		private List<DocumentLine> documentLines;

		private SliderControl slider;

		private int displayedLinesStart;

		private static string randomLetters;

		private TempTexture textTexture;

		public DocumentWindow(string name, string file, string safeCode = "")
		{
			if (file == "credits")
			{
				file = "credits_pc";
			}
			docFile = file;
			base.WindowIcon = "doc";
			base.WindowTitle = name;
			base.ContentsSize = new Vec2(450, 232);
			AddButton(TWMWindowButtonType.Close);
			AddButton(TWMWindowButtonType.Minimize);
			slider = new SliderControl("", 0, 1, new Vec2(432, 2), 228, useButtons: true, vertical: true);
			slider.ScrollTriggerZone = new Rect(0, 0, 450, 232);
			slider.OnValueChanged = OnSliderChange;
			slider.Active = false;
			LoadDocText(safeCode);
			if (documentLines.Count > 12)
			{
				slider.Active = true;
				slider.Max = documentLines.Count - 12;
			}
		}

		private void OnSliderChange(int newVal)
		{
			displayedLinesStart = newVal;
			DrawTextTexture();
		}

		public override bool Update(bool mouseAlreadyOnOtherWindow)
		{
			if (textTexture == null || !textTexture.isValid)
			{
				DrawTextTexture();
			}
			textTexture?.KeepAlive();
			foreach (DocumentLine documentLine in documentLines)
			{
				if (!documentLine.HasGlitchedLetters || documentLine.letters == null)
				{
					continue;
				}
				foreach (DocumentLetter letter in documentLine.letters)
				{
					letter.Update();
				}
			}
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			slider.Update(parentPos, !mouseAlreadyOnOtherWindow && !base.IsMinimized);
			return base.Update(mouseAlreadyOnOtherWindow);
		}

		private void LoadDocText(string safeCode)
		{
			documentLines = new List<DocumentLine>();
			if (string.IsNullOrEmpty(randomLetters))
			{
				randomLetters = GetFileTxt("random_letters");
			}
			string text = GetFileTxt(docFile);
			if (docFile == "document_postgame" || docFile == "document")
			{
				text = text.Replace("{CODE}", safeCode);
			}
			string[] array = text.Split('\n');
			foreach (string obj in array)
			{
				int num = 0;
				List<DocumentLetter> list = new List<DocumentLetter>();
				bool glitched = false;
				bool flag = false;
				string text2 = obj;
				for (int j = 0; j < text2.Length; j++)
				{
					char c = text2[j];
					switch (c)
					{
					case '\\':
						flag = true;
						continue;
					case '[':
						if (!flag)
						{
							glitched = true;
							break;
						}
						goto default;
					case ']':
						if (!flag)
						{
							glitched = false;
							break;
						}
						goto default;
					default:
					{
						DocumentLetter documentLetter = new DocumentLetter(GraphicsManager.FontType.GameSmall, c.ToString(), glitched);
						num += documentLetter.Width;
						if (num > 424)
						{
							List<DocumentLetter> list2 = new List<DocumentLetter>();
							while (documentLetter.DisplayLetter != " " && list.Count > 0)
							{
								list2.Add(documentLetter);
								documentLetter = list.Last();
								list.RemoveAt(list.Count - 1);
							}
							if (list.Count <= 0)
							{
								list2.Add(documentLetter);
								list = list2;
								list.Reverse();
								documentLetter = list.Last();
								list.RemoveAt(list.Count - 1);
								AddLettersToDocumentLines(list);
								list = new List<DocumentLetter>();
								list.Add(documentLetter);
								num = documentLetter.Width;
								break;
							}
							AddLettersToDocumentLines(list);
							list = list2;
							list.Reverse();
							num = 0;
							foreach (DocumentLetter item in list)
							{
								num += item.Width;
							}
						}
						else
						{
							list.Add(documentLetter);
						}
						break;
					}
					}
					flag = false;
				}
				AddLettersToDocumentLines(list);
			}
			DrawTextTexture();
		}

		private void DrawTextTexture()
		{
			if (textTexture == null || !textTexture.isValid)
			{
				Vec2 size = new Vec2(450, 232);
				textTexture = Game1.gMan.TempTexMan.GetTempTexture(size);
			}
			Vec2 vec = new Vec2(4, 8);
			Game1.gMan.BeginDrawToTempTexture(textTexture);
			for (int i = displayedLinesStart; i < displayedLinesStart + 12 && i < documentLines.Count; i++)
			{
				DocumentLine documentLine = documentLines[i];
				if (documentLine.HasGlitchedLetters && documentLine.letters != null)
				{
					foreach (DocumentLetter letter in documentLines[i].letters)
					{
						if (!letter.IsGlitched)
						{
							letter.Draw(GameColor.White, vec, 1);
						}
						vec.X += letter.Width;
					}
					vec.X = 4;
				}
				else
				{
					Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, vec, documentLine.line, GameColor.White, GraphicsManager.BlendMode.Normal, 1, GraphicsManager.TextBlitMode.OnlyText);
				}
				vec.Y += 18;
			}
			Game1.gMan.EndDrawToTempTexture();
		}

		private static string GetFileTxt(string docFile)
		{
			string path = Path.Combine(Game1.GameDataPath(), "loc/" + Game1.languageMan.GetCurrentLangCode() + "/txt/" + docFile + ".txt");
			if (!File.Exists(path))
			{
				path = Path.Combine(Game1.GameDataPath(), "txt/" + docFile + ".txt");
			}
			return File.ReadAllText(path);
		}

		private void AddLettersToDocumentLines(List<DocumentLetter> letters)
		{
			DocumentLine documentLine = new DocumentLine();
			if (letters.Any((DocumentLetter l2) => l2.IsGlitched))
			{
				documentLine.letters = letters;
				documentLine.HasGlitchedLetters = true;
			}
			else
			{
				documentLine.HasGlitchedLetters = false;
				documentLine.line = string.Empty;
				foreach (DocumentLetter letter in letters)
				{
					documentLine.line += letter.DisplayLetter;
				}
			}
			documentLines.Add(documentLine);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gColor = theme.Background(alpha);
			GameColor gameColor = theme.Variant(alpha);
			Game1.gMan.ColorBoxBlit(new Rect(screenPos.X, screenPos.Y, 450, 232), gColor);
			Game1.gMan.MainBlit(textTexture, screenPos, gameColor);
			Vec2 vec = screenPos;
			vec.X += 4;
			vec.Y += 8;
			for (int i = displayedLinesStart; i < displayedLinesStart + 12 && i < documentLines.Count; i++)
			{
				DocumentLine documentLine = documentLines[i];
				if (documentLine.HasGlitchedLetters && documentLine.letters != null)
				{
					foreach (DocumentLetter letter in documentLines[i].letters)
					{
						if (letter.IsGlitched)
						{
							letter.Draw(theme, vec, alpha);
						}
						vec.X += letter.Width;
					}
					vec.X = screenPos.X + 4;
				}
				else
				{
					Game1.gMan.TextBlit(GraphicsManager.FontType.GameSmall, vec, documentLine.line, gameColor, GraphicsManager.BlendMode.Normal, 2, GraphicsManager.TextBlitMode.OnlyGlyphes);
				}
				vec.Y += 18;
			}
			slider.Draw(theme, screenPos, alpha);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			if (window is DocumentWindow documentWindow)
			{
				return docFile == documentWindow.docFile;
			}
			return false;
		}

		public static string GetRandomLetter()
		{
			if (string.IsNullOrEmpty(randomLetters))
			{
				randomLetters = GetFileTxt("random_letters");
			}
			return randomLetters[MathHelper.Random(0, randomLetters.Length - 1)].ToString();
		}

		public static void ClearRandomLetters()
		{
			randomLetters = null;
		}
	}
}

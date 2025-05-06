using System;
using System.Collections.Generic;
using System.Linq;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class ChooserControl
	{
		public const int BUTTON_SIZE = 16;

		public Action<string> OnItemChange;

		private Func<string, string, string> localizeText;

		private GraphicsManager.FontType font;

		public bool ShowBorder = true;

		public bool DrawLanguagesAsOptions;

		private Rect controlArea;

		private List<(string key, string name)> items;

		private bool disabled;

		private IconButton bLeft;

		private IconButton bRight;

		private List<DocumentLetter> glitchLetters;

		private string label;

		private TempTexture labelTexture;

		public Vec2 Position
		{
			get
			{
				return controlArea.XY;
			}
			set
			{
				SetPosition(value);
			}
		}

		public bool Disabled
		{
			get
			{
				return disabled;
			}
			set
			{
				bLeft.Disabled = value;
				bRight.Disabled = value;
				disabled = value;
			}
		}

		public bool GlitchText
		{
			get
			{
				return glitchLetters != null;
			}
			set
			{
				if (value)
				{
					SetGlitchText();
				}
				else
				{
					glitchLetters = null;
				}
			}
		}

		public string Value
		{
			get
			{
				if (items.Count > 0 && CurrentIndex >= 0)
				{
					return items[CurrentIndex].key;
				}
				return null;
			}
			set
			{
				if (items == null || items.Count <= 0)
				{
					return;
				}
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i].key == value)
					{
						CurrentIndex = i;
						SetLabel();
						break;
					}
				}
			}
		}

		public int Index => CurrentIndex;

		public int CurrentIndex { get; private set; }

		public ChooserControl(Vec2 pos, int length, List<(string, string)> items, string selectedKey = null, GraphicsManager.FontType fontType = GraphicsManager.FontType.OS, Func<string, string, string> locFunc = null)
		{
			controlArea = new Rect(pos.X, pos.Y, length, 16);
			bLeft = new IconButton("the_world_machine/window_icons/arrow_left", pos, OnButtonLeft);
			Vec2 relativePos = pos + new Vec2(length - 16, 0);
			bRight = new IconButton("the_world_machine/window_icons/arrow_right", relativePos, OnButtonRight);
			bLeft.Disabled = items.Count < 1;
			bRight.Disabled = items.Count < 1;
			font = fontType;
			localizeText = locFunc;
			SetItems(items, selectedKey);
		}

		public bool Update(Vec2 parentPos, bool canInteract)
		{
			if (labelTexture == null || !labelTexture.isValid)
			{
				DrawLabelTexture();
			}
			labelTexture.KeepAlive();
			bool result = !canInteract | bLeft.Update(parentPos, canInteract) | bRight.Update(parentPos, canInteract);
			List<DocumentLetter> list = glitchLetters;
			if (list != null)
			{
				list.ForEach(delegate(DocumentLetter l)
				{
					l.Update();
				});
				return result;
			}
			return result;
		}

		public void Draw(Vec2 parentPos, TWMTheme theme, byte alpha, bool dropShadows = false)
		{
			GameColor gameColor = (disabled ? theme.Variant(alpha) : theme.Primary(alpha));
			GameColor gameColor2 = theme.Background(alpha);
			if (ShowBorder)
			{
				Rect boxRect = controlArea.Translated(parentPos);
				Game1.gMan.ColorBoxBlit(boxRect, gameColor);
				Game1.gMan.ColorBoxBlit(boxRect.Shrink(1), gameColor2);
			}
			bLeft.Draw(parentPos, theme, alpha, dropShadows);
			bRight.Draw(parentPos, theme, alpha, dropShadows);
			Vec2 vec = new Vec2(controlArea.W / 2, 2) + parentPos + controlArea.XY;
			if (DrawLanguagesAsOptions)
			{
				string textureName = "the_world_machine/languages/" + Value;
				Vec2 vec2 = Game1.gMan.TextureSize(textureName);
				vec.X *= 2;
				vec.Y *= 2;
				vec.X -= vec2.X / 2;
				vec.Y -= 4;
				Game1.gMan.MainBlit(textureName, vec, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				return;
			}
			if (glitchLetters == null)
			{
				int num = ((font == GraphicsManager.FontType.OS) ? 1 : 2);
				if (font == GraphicsManager.FontType.Game)
				{
					vec.Y -= 4;
				}
				if (dropShadows)
				{
					Game1.gMan.MainBlit(labelTexture, (vec + new Vec2(1, 1)) * (2 / num), gameColor2, 0, GraphicsManager.BlendMode.Normal, num, xCentered: true);
				}
				Game1.gMan.MainBlit(labelTexture, vec * (2 / num), gameColor, 0, GraphicsManager.BlendMode.Normal, num, xCentered: true);
				return;
			}
			int x = Game1.gMan.TextSize(font, label).X;
			vec.X -= x / 2;
			vec.Y -= 4;
			foreach (DocumentLetter glitchLetter in glitchLetters)
			{
				glitchLetter.Draw(theme, vec, alpha);
				vec.X += glitchLetter.Width;
			}
		}

		public void AddItem((string key, string name) item)
		{
			items.Add(item);
			bLeft.Disabled = disabled;
			bRight.Disabled = disabled;
		}

		public void SetItems(List<(string, string)> items, string selectedKey = null)
		{
			this.items = items;
			if (selectedKey != null)
			{
				CurrentIndex = this.items.FindIndex(((string key, string name) pair) => pair.key == selectedKey);
				if (CurrentIndex < 0)
				{
					CurrentIndex = 0;
				}
			}
			else if (CurrentIndex >= items.Count)
			{
				CurrentIndex = items.Count - 1;
			}
			SetLabel();
			bLeft.Disabled = items.Count < 1;
			bRight.Disabled = items.Count < 1;
		}

		private void OnButtonLeft()
		{
			CurrentIndex--;
			if (CurrentIndex < 0)
			{
				CurrentIndex = items.Count - 1;
			}
			OnItemChange?.Invoke(items[CurrentIndex].key);
			SetLabel();
		}

		private void OnButtonRight()
		{
			CurrentIndex++;
			if (CurrentIndex >= items.Count)
			{
				CurrentIndex = 0;
			}
			OnItemChange?.Invoke(items[CurrentIndex].key);
			SetLabel();
		}

		private void SetLabel()
		{
			if (CurrentIndex >= 0 && CurrentIndex < items.Count)
			{
				label = items[CurrentIndex].name;
			}
			else
			{
				label = "";
			}
			if (glitchLetters != null)
			{
				SetGlitchText();
			}
			DrawLabelTexture();
		}

		private void DrawLabelTexture()
		{
			string text = label;
			if (localizeText != null && Value != null)
			{
				text = localizeText(Value, text);
			}
			labelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(font, text);
		}

		private void SetGlitchText()
		{
			glitchLetters = label.Select((char c) => new DocumentLetter(font, $"{c}", glitched: true)).ToList();
		}

		private void SetPosition(Vec2 pos)
		{
			controlArea.X = pos.X;
			controlArea.Y = pos.Y;
			bLeft.Position = pos;
			Vec2 position = pos + new Vec2(controlArea.W - 16, 0);
			bRight.Position = position;
		}
	}
}

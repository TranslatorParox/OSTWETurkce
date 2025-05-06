using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class ContactsWindow : TWMWindow
	{
		private const GraphicsManager.FontType titleFont = GraphicsManager.FontType.Game;

		private const GraphicsManager.FontType textFont = GraphicsManager.FontType.OS;

		private const int MARGIN = 4;

		private const int WALK_MARGIN = 10;

		private const int TITLE_H = 20;

		private const int SUBTITLE_H = 14;

		private const int TEXT_W = 240;

		private const int TEXT_ROWS = 11;

		private const int TEXT_ROW_HEIGHT = 12;

		private const int FACEPIC_SIZE = 48;

		private const int SCROLL_W = 16;

		private const int FACE_BORDER = 2;

		private const int WALK_FRAME_LEN = 12;

		private const int MARGIN_BETWEEN_TEXT_AND_FACEPIC = 30;

		private const int PORTRAIT_OFFSET = 160;

		private Vec2 spriteSize;

		private static Dictionary<string, CharProfileInfo> _profiles;

		private CharProfileInfo currentProfile;

		private Textbox textbox;

		private ChooserControl charChooser;

		private IconButton faceButton;

		private int faceIndex;

		private int walkTimer;

		private bool isWindowHeld;

		private Vec2 windowHeldMousePos;

		private TempTexture subtitleTexture;

		public static Dictionary<string, CharProfileInfo> ProfileInfo
		{
			get
			{
				if (_profiles == null)
				{
					_profiles = LoadProfiles();
				}
				return _profiles;
			}
		}

		public ContactsWindow(List<string> unlockedProfiles)
		{
			base.WindowIcon = "contacts";
			base.WindowTitle = "contacts_app";
			Rect placement = new Rect(4, 46, 240, 140);
			textbox = new Textbox(placement, 12, 4);
			base.ContentsSize = new Vec2(424, 20 + placement.H + 20 + 14 + 48 + 4 + 30);
			AddButton(TWMWindowButtonType.Close, delegate
			{
				Game1.gMan.clearTextureCache(TextureCache.CacheType.CharacterProfile);
				onClose(this);
			});
			AddButton(TWMWindowButtonType.Minimize);
			unlockedProfiles = SortProfiles(unlockedProfiles);
			List<(string, string)> items = unlockedProfiles.Select((string id) => (id: id, title: ProfileInfo[id].title)).ToList();
			Func<string, string, string> locFunc = (string value, string label) => Game1.languageMan.GetContactLocString(value, LanguageManager.ContactStringType.title, label);
			charChooser = new ChooserControl(new Vec2(4, 6), base.ContentsSize.X - 8, items, null, GraphicsManager.FontType.Game, locFunc)
			{
				ShowBorder = false,
				OnItemChange = OnProfileChange
			};
			faceButton = new IconButton(relativePos: new Vec2(6, placement.Y + placement.H + 4 + 2 + 30), iconPath: "", buttonSize: new Vec2(48, 48), action: OnFaceButtonPress, cacheType: TextureCache.CacheType.CharacterProfile)
			{
				Tint = false,
				BorderWidth = 2
			};
			OnProfileChange(unlockedProfiles[0]);
		}

		private List<string> SortProfiles(List<string> unlockedProfiles)
		{
			List<string> list = new List<string>();
			Dictionary<string, string> dictionary = unlockedProfiles.ToDictionary((string s) => s);
			foreach (CharProfileInfo value in ProfileInfo.Values)
			{
				if (dictionary.ContainsKey(value.unlockId))
				{
					list.Add(value.unlockId);
				}
			}
			return list;
		}

		private static Dictionary<string, CharProfileInfo> LoadProfiles()
		{
			CharacterProfileMetadata characterProfileMetadata = JsonConvert.DeserializeObject<CharacterProfileMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/contacts_metadata.json")));
			Dictionary<string, CharProfileInfo> dictionary = new Dictionary<string, CharProfileInfo>();
			foreach (CharProfileInfo profile in characterProfileMetadata.profiles)
			{
				dictionary.Add(profile.unlockId, profile);
			}
			return dictionary;
		}

		public override bool Update(bool mouseInputConsumed)
		{
			if (subtitleTexture == null || !subtitleTexture.isValid)
			{
				DrawSubtitleTexture();
			}
			subtitleTexture.KeepAlive();
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			bool canInteract = !mouseInputConsumed && !base.IsMinimized;
			mouseInputConsumed |= charChooser.Update(parentPos, canInteract);
			mouseInputConsumed |= faceButton.Update(parentPos, canInteract);
			mouseInputConsumed |= textbox.Update(parentPos, canInteract);
			walkTimer++;
			if (walkTimer > 48)
			{
				walkTimer = 0;
			}
			if (!mouseInputConsumed && !base.IsMinimized)
			{
				Vec2 mousePos = Game1.mouseCursorMan.MousePos;
				mousePos.X -= Pos.X + 2;
				mousePos.Y -= Pos.Y + 26;
				mousePos.X *= 2;
				mousePos.Y *= 2;
				if (mousePos.X >= 0 && mousePos.Y >= 0 && mousePos.X < base.ContentsSize.X * 2 && mousePos.Y < base.ContentsSize.Y * 2)
				{
					mouseInputConsumed = true;
					Game1.mouseCursorMan.SetState(MouseCursorManager.State.Grabbable);
					if (isWindowHeld)
					{
						if (Game1.mouseCursorMan.MouseHeld)
						{
							Game1.mouseCursorMan.SetState(MouseCursorManager.State.Holding);
							Vec2 vec = new Vec2(Game1.mouseCursorMan.MousePos.X - windowHeldMousePos.X, Game1.mouseCursorMan.MousePos.Y - windowHeldMousePos.Y);
							Pos = new Vec2(Pos.X + vec.X, Pos.Y + vec.Y);
							windowHeldMousePos = Game1.mouseCursorMan.MousePos;
						}
						else
						{
							isWindowHeld = false;
						}
					}
					else if (Game1.mouseCursorMan.MouseClicked)
					{
						isWindowHeld = true;
						windowHeldMousePos = Game1.mouseCursorMan.MousePos;
						grabFocus(this);
					}
				}
			}
			else
			{
				isWindowHeld = false;
			}
			if (base.Update(mouseInputConsumed) || mouseInputConsumed)
			{
				return !base.IsMinimized;
			}
			return false;
		}

		private void DrawSubtitleTexture()
		{
			string text = "";
			if (currentProfile != null)
			{
				text = Game1.languageMan.GetContactLocString(currentProfile.unlockId, LanguageManager.ContactStringType.subtitle, currentProfile.subtitle);
			}
			subtitleTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.OS, text);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			theme.Primary(alpha);
			GameColor gameColor = theme.Background(alpha);
			TWMTheme themeById = Game1.windowMan.GetThemeById(currentProfile.uxColorTheme);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gameColor);
			if (currentProfile != null)
			{
				float alpha2 = (float)(int)alpha / 255f;
				string textureName = "the_world_machine/portraits/" + currentProfile.bgImage;
				Game1.gMan.MainBlit(textureName, screenPos, alpha2, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.CharacterProfile);
				Vec2 vec = screenPos + new Vec2(2, 32);
				if (currentProfile.textDropShadow)
				{
					Game1.gMan.MainBlit(subtitleTexture, (vec + new Vec2(1, 1)) * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				}
				Game1.gMan.MainBlit(subtitleTexture, vec * 2, themeById.Primary(alpha), 0, GraphicsManager.BlendMode.Normal, 1);
				int y = base.ContentsSize.Y - 4 - Math.Max(0, (52 - spriteSize.Y) / 2) - spriteSize.Y;
				int num = faceButton.Position.X + 2 + 48 + 10;
				string text = "npc/" + currentProfile.walkspriteId;
				if (currentProfile.singleSprite)
				{
					Vec2 vec2 = new Vec2(num, y);
					Game1.gMan.MainBlit(text, vec2 + screenPos, (float)(int)alpha / 255f, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.CharacterProfile);
				}
				else
				{
					for (int i = 0; i < 4; i++)
					{
						Vec2 vec3 = new Vec2(num + i * (spriteSize.X + 10), y);
						Vec2 framePos = new Vec2(walkTimer / 12, i);
						if (!currentProfile.walkAnimation)
						{
							framePos.X = 0;
						}
						DrawWalkSprite(vec3 + screenPos, framePos, text, alpha);
					}
				}
			}
			charChooser.Draw(screenPos, themeById, alpha, currentProfile.textDropShadow);
			faceButton.Draw(screenPos, themeById, alpha);
			textbox.Draw(themeById, screenPos, alpha);
		}

		private void DrawWalkSprite(Vec2 screenPos, Vec2 framePos, string spritesheet, byte alpha)
		{
			Rect srcRect = new Rect(framePos.X * spriteSize.X, framePos.Y * spriteSize.Y, spriteSize.X, spriteSize.Y);
			GameColor white = GameColor.White;
			white.a = alpha;
			Game1.gMan.MainBlit(spritesheet, screenPos, srcRect, white, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.CharacterProfile);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is ContactsWindow;
		}

		private void OnFaceButtonPress()
		{
			if (currentProfile != null)
			{
				faceIndex++;
				if (faceIndex >= currentProfile.facepics.Count)
				{
					faceIndex = 0;
				}
				faceButton.Icon = "facepics/" + currentProfile.facepics[faceIndex];
			}
		}

		private void OnProfileChange(string newProfileId)
		{
			Game1.gMan.clearTextureCache(TextureCache.CacheType.CharacterProfile);
			currentProfile = ProfileInfo[newProfileId];
			faceIndex = 0;
			faceButton.Icon = "facepics/" + currentProfile.facepics[faceIndex];
			string contactLocString = Game1.languageMan.GetContactLocString(newProfileId, LanguageManager.ContactStringType.infoText, currentProfile.infoText);
			textbox.SetText(contactLocString);
			Vec2 vec = Game1.gMan.TextureSize("npc/" + currentProfile.walkspriteId, TextureCache.CacheType.CharacterProfile);
			if (currentProfile.singleSprite)
			{
				spriteSize = vec;
			}
			else
			{
				spriteSize = new Vec2(vec.X / 4, vec.Y / 4);
			}
			DrawSubtitleTexture();
		}

		public void UpdateProfiles(List<string> unlockedProfiles)
		{
			string value = charChooser.Value;
			unlockedProfiles = SortProfiles(unlockedProfiles);
			List<(string, string)> items = unlockedProfiles.Select((string id) => (id: id, title: ProfileInfo[id].title)).ToList();
			charChooser.SetItems(items, value);
		}
	}
}

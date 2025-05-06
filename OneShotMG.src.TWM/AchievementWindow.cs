using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Util;

namespace OneShotMG.src.TWM
{
	internal class AchievementWindow : TWMWindow
	{
		private class ChevoItem
		{
			private static GraphicsManager.FontType title_font = GraphicsManager.FontType.Game;

			private static GraphicsManager.FontType font = GraphicsManager.FontType.OS;

			public string id;

			public string icon;

			public string title;

			public string label;

			public bool unlocked;

			public TempTexture titleTexture;

			public TempTexture labelTexture;

			public void DrawTitleTexture()
			{
				titleTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(title_font, title);
			}

			public void DrawLabelTexture()
			{
				labelTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(font, label);
			}
		}

		private static GraphicsManager.FontType font;

		private const int TITLE_YOFF = 8;

		private const int LABEL_YOFF = 32;

		private const int PADDING = 4;

		private const int SPACER_H = 8;

		private const int MAX_ITEMS = 4;

		private const int ITEM_H = 50;

		private const int ITEM_W = 300;

		private const string ICON_UNKNOWN = "the_world_machine/achievements/unknown";

		private List<ChevoItem> achievements;

		private readonly SliderControl scrollbar;

		private HashSet<string> currentUnlockedAchievements;

		private static Dictionary<string, AchievementInfo> _achievementMetadata;

		public static Dictionary<string, AchievementInfo> AchievementMetadata
		{
			get
			{
				if (_achievementMetadata == null)
				{
					_achievementMetadata = LoadAchievements();
				}
				return _achievementMetadata;
			}
		}

		private static Dictionary<string, AchievementInfo> LoadAchievements()
		{
			AchievementsMetadata achievementsMetadata = JsonConvert.DeserializeObject<AchievementsMetadata>(File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/achievements_metadata.json")));
			Dictionary<string, AchievementInfo> dictionary = new Dictionary<string, AchievementInfo>();
			foreach (AchievementInfo achievement in achievementsMetadata.achievements)
			{
				dictionary.Add(achievement.id, achievement);
			}
			return dictionary;
		}

		public AchievementWindow(List<string> unlockedAchievements)
		{
			base.WindowIcon = "achievements";
			base.WindowTitle = "achievements_app_name";
			currentUnlockedAchievements = new HashSet<string>();
			foreach (string unlockedAchievement in unlockedAchievements)
			{
				currentUnlockedAchievements.Add(unlockedAchievement);
			}
			GenerateDisplayedAchievements();
			base.ContentsSize = new Vec2(300, 224);
			int num = Math.Max(achievements.Count - 4, 0);
			scrollbar = new SliderControl("", 0, num, new Vec2(299, 1), base.ContentsSize.Y - 2, useButtons: true, vertical: true);
			scrollbar.Active = num > 0;
			if (scrollbar.Active)
			{
				base.ContentsSize = new Vec2(316, base.ContentsSize.Y);
			}
			scrollbar.ScrollTriggerZone = new Rect(0, 0, base.ContentsSize.X, base.ContentsSize.Y);
			AddButton(TWMWindowButtonType.Close, delegate
			{
				Game1.gMan.clearTextureCache(TextureCache.CacheType.Achievements);
				onClose(this);
			});
			AddButton(TWMWindowButtonType.Minimize);
		}

		public void GenerateDisplayedAchievements()
		{
			achievements = new List<ChevoItem>();
			foreach (AchievementInfo value in AchievementMetadata.Values)
			{
				if (currentUnlockedAchievements.Contains(value.id))
				{
					achievements.Add(GetItem(value.id, unlocked: true));
				}
				else
				{
					achievements.Add(GetItem(value.id, unlocked: false));
				}
			}
		}

		public override bool Update(bool cursorOccluded)
		{
			foreach (ChevoItem achievement in achievements)
			{
				if (achievement.titleTexture == null || !achievement.titleTexture.isValid)
				{
					achievement.DrawTitleTexture();
				}
				achievement.titleTexture.KeepAlive();
				if (achievement.labelTexture == null || !achievement.labelTexture.isValid)
				{
					achievement.DrawLabelTexture();
				}
				achievement.labelTexture.KeepAlive();
			}
			Vec2 parentPos = new Vec2(Pos.X + 2, Pos.Y + 26);
			bool canInteract = !cursorOccluded && !base.IsMinimized;
			scrollbar.Update(parentPos, canInteract);
			return base.Update(cursorOccluded);
		}

		public override void DrawContents(TWMTheme theme, Vec2 screenPos, byte alpha)
		{
			GameColor gameColor = theme.Primary(alpha);
			GameColor gameColor2 = theme.Variant(alpha);
			GameColor gColor = theme.Background(alpha);
			Rect boxRect = new Rect(screenPos.X, screenPos.Y, base.ContentsSize.X, base.ContentsSize.Y);
			Game1.gMan.ColorBoxBlit(boxRect, gColor);
			int num = Math.Min(achievements.Count, scrollbar.Value + 4);
			for (int i = scrollbar.Value; i < num; i++)
			{
				bool num2 = i == num - 1;
				ChevoItem chevoItem = achievements[i];
				int num3 = (i - scrollbar.Value) * 58;
				string textureName = (chevoItem.unlocked ? chevoItem.icon.ToLowerInvariant() : "the_world_machine/achievements/unknown");
				Vec2 vec = Game1.gMan.TextureSize(textureName, TextureCache.CacheType.Achievements);
				Vec2 vec2 = new Vec2(4, num3 + (50 - vec.Y) / 2);
				Game1.gMan.MainBlit(textureName, vec2 + screenPos, chevoItem.unlocked ? 1f : 0.5f, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), 1f, 1f, 1f, TextureCache.CacheType.Achievements);
				Vec2 vec3 = new Vec2(8 + vec.X - 2, 8 + num3);
				Game1.gMan.MainBlit(chevoItem.titleTexture, vec3 + screenPos, chevoItem.unlocked ? gameColor : gameColor2);
				if (chevoItem.unlocked)
				{
					vec3.Y = 32 + num3;
					Game1.gMan.MainBlit(chevoItem.labelTexture, (vec3 + screenPos) * 2, gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
				}
				if (!num2)
				{
					Vec2 pixelPos = new Vec2(0, num3 + 50) + screenPos;
					Game1.gMan.MainBlit("the_world_machine/achievements/separator", pixelPos, gameColor, 0, GraphicsManager.BlendMode.Normal, 2, TextureCache.CacheType.Achievements);
				}
			}
			scrollbar.Draw(theme, screenPos, alpha);
		}

		public override bool IsSameContent(TWMWindow window)
		{
			return window is AchievementWindow;
		}

		private ChevoItem GetItem(string id, bool unlocked)
		{
			AchievementInfo achievementInfo = AchievementMetadata[id];
			ChevoItem chevoItem = new ChevoItem();
			chevoItem.id = id;
			chevoItem.icon = "the_world_machine/achievements/" + id;
			chevoItem.title = Game1.languageMan.GetAchievementLocString(achievementInfo.id, LanguageManager.AchievementStringType.title, achievementInfo.title);
			chevoItem.label = Game1.languageMan.GetAchievementLocString(achievementInfo.id, LanguageManager.AchievementStringType.description, achievementInfo.description);
			chevoItem.unlocked = unlocked;
			chevoItem.DrawTitleTexture();
			chevoItem.DrawLabelTexture();
			return chevoItem;
		}

		public void AddUnlock(string id)
		{
			if (currentUnlockedAchievements.Contains(id))
			{
				return;
			}
			currentUnlockedAchievements.Add(id);
			foreach (ChevoItem achievement in achievements)
			{
				if (achievement.id == id)
				{
					achievement.unlocked = true;
					break;
				}
			}
			int num = Math.Max(achievements.Count - 4, 0);
			scrollbar.Max = num;
			scrollbar.Active = num > 0;
			if (scrollbar.Active)
			{
				base.ContentsSize = new Vec2(316, base.ContentsSize.Y);
			}
		}
	}
}

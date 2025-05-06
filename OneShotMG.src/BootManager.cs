using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM.Filesystem;
using OneShotMG.src.Util;

namespace OneShotMG.src
{
	public class BootManager
	{
		public enum BootStep
		{
			BEGIN,
			BREAK_BEFORE_FELIX,
			FELIX_FADEIN,
			FELIX_DISPLAY,
			FELIX_FADEOUT,
			FUTURECAT_LOGO_FADEIN,
			FUTURECAT_LOGO_DISPLAY,
			FUTURECAT_LOGO_FADEOUT,
			KOMODO_LOGO_FADEIN,
			KOMODO_LOGO_DISPLAY,
			KOMODO_LOGO_FADEOUT,
			FMOD_LOGO_FADEIN,
			FMOD_LOGO_DISPLAY,
			FMOD_LOGO_FADEOUT,
			LANGUAGE_SELECT_FADEIN,
			LANGUAGE_SELECT,
			LANGUAGE_SELECT_FADEOUT,
			AUTOSAVE_EXPLAIN_FADEIN,
			AUTOSAVE_EXPLAIN,
			AUTOSAVE_EXPLAIN_FADEOUT,
			DEMO_TITLE_FADEIN,
			DEMO_TITLE_DISPLAY,
			DEMO_TITLE_FADEOUT,
			DEMO_EXPLAIN_FADEIN,
			DEMO_EXPLAIN_DISPLAY,
			DEMO_EXPLAIN_FADEOUT,
			DEMO_END_FADEIN,
			DEMO_END_DISPLAY,
			DEMO_END_FADEOUT,
			BREAK_BEFORE_CONSOLE,
			CONSOLE_SHOW_BIOS_LOGO,
			CONSOLE_TEXT,
			CONSOLE_LOAD_SOUNDS,
			CONSOLE_TEXT2,
			CONSOLE_LOAD_TEXTURES,
			CONSOLE_TEXT3,
			BREAK_BEFORE_BIG_LOGO,
			BIG_LOGO_BEFORE_SOUND,
			BIG_LOGO_AFTER_SOUND,
			TWM_LOGO_FADEOUT,
			COMPLETE
		}

		private const TextureCache.CacheType cache = TextureCache.CacheType.BootScreen;

		private const int LINE_HEIGHT = 22;

		private const int TEXT_DELAY_INITIAL = 12;

		private const GraphicsManager.FontType font = GraphicsManager.FontType.Game;

		private const string FUTURECAT_LOGO_PATH = "partner_logos/futurecat_white";

		private const string KOMODO_LOGO_PATH = "partner_logos/komodo_white";

		private const string DEMO_TITLE_PATH = "the_world_machine/demo_title";

		private const string DEMO_END_PATH = "the_world_machine/demo_end";

		private const string FMOD_LOGO_PATH = "partner_logos/fmod_white";

		private const string FELIX_PATH = "the_world_machine/felix";

		private const string TWM_LOGO_PATH = "the_world_machine/logo";

		private const string TWM_LOGO_FULL_PATH = "the_world_machine/logo_full";

		private const int BREAK_BEFORE_FMOD_LOGO_TIME = 30;

		private const int FMOD_LOGO_FADEIN_TIME = 30;

		private const int FMOD_LOGO_DISPLAY_TIME = 120;

		private const int FMOD_LOGO_FADEOUT_TIME = 30;

		private const int FUTURECAT_LOGO_FADEIN_TIME = 30;

		private const int FUTURECAT_LOGO_DISPLAY_TIME = 120;

		private const int FUTURECAT_LOGO_FADEOUT_TIME = 30;

		private const int KOMODO_LOGO_FADEIN_TIME = 30;

		private const int KOMODO_LOGO_DISPLAY_TIME = 120;

		private const int KOMODO_LOGO_FADEOUT_TIME = 30;

		private const int DEMO_TITLE_FADEIN_TIME = 30;

		private const int DEMO_TITLE_FADEOUT_TIME = 30;

		private const int DEMO_EXPLAIN_FADEIN_TIME = 30;

		private const int DEMO_EXPLAIN_DISPLAY_TIME = 600;

		private const int DEMO_EXPLAIN_FADEOUT_TIME = 30;

		private const int DEMO_END_FADEIN_TIME = 30;

		private const int DEMO_END_DISPLAY_TIME = 900;

		private const int DEMO_END_FADEOUT_TIME = 30;

		private const int FELIX_FADEIN_TIME = 30;

		private const int FELIX_DISPLAY_TIME = 120;

		private const int FELIX_FADEOUT_TIME = 30;

		private const int BREAK_BEFORE_CONSOLE_TIME = 60;

		private const int CONSOLE_SHOW_BIOS_LOGO_TIME = 60;

		private const int BREAK_BEFORE_BIG_LOGO_TIME = 60;

		private const int TWM_LOGO_TIME_BEFORE_SOUND = 60;

		private const int TWM_LOGO_TIME_AFTER_SOUND = 120;

		private const int TWM_LOGO_FADEOUT_TIME = 120;

		private const int LANG_SELECT_FADEIN_TIME = 30;

		private const int LANG_SELECT_FADEOUT_TIME = 30;

		private const int AUTOSAVE_EXPLAIN_FADEIN_TIME = 30;

		private const int AUTOSAVE_EXPLAIN_DISPLAY_TIME = 300;

		private const int AUTOSAVE_EXPLAIN_FADEOUT_TIME = 30;

		private readonly GameColor bioTextColor = new GameColor(161, 91, byte.MaxValue, byte.MaxValue);

		private SpinningBulbIcon bulbIcon;

		private BootStep currentStep;

		private float opacity;

		private List<string> postText;

		private string srcText;

		private int textDelay = 12;

		private int stateTimer;

		private Queue<string> unloadedSystemTextures;

		private bool skipBootEntirely;

		private Game1 gameRef;

		private List<(string k, string displayName)> languageOptions;

		private int selectedLanguageIndex;

		private List<string> autosaveLines;

		private List<string> demoTitleLines;

		private List<string> demoExplainLines;

		private List<string> demoEndLines;

		public bool ShowOs
		{
			get
			{
				if (currentStep != BootStep.TWM_LOGO_FADEOUT)
				{
					return currentStep == BootStep.COMPLETE;
				}
				return true;
			}
		}

		public bool SequenceComplete => currentStep == BootStep.COMPLETE;

		public BootManager(Game1 game)
		{
			gameRef = game;
			setupSystemTextureQueue();
			bulbIcon = new SpinningBulbIcon();
			Game1.gMan.LoadTexture("the_world_machine/bulb_anim_24", TextureCache.CacheType.TheWorldMachine);
			Game1.gMan.LoadTexture("the_world_machine/bulb_anim_24_lightmap", TextureCache.CacheType.TheWorldMachine);
			LoadBootmanTextures();
			new FilesystemSaveManager().LoadDesktopSave(out var data);
			if (data != null)
			{
				Vec2 drawResolution = data.GetDrawResolution();
				Game1.gMan.SetDrawResolution(drawResolution);
				Game1.gMan.AddResolution(drawResolution);
				Game1.gMan.SetFullscreen(data.isFullscreen);
				Game1.gMan.vsyncEnabled = data.isVsyncEnabled;
			}
			Game1.steamMan.DemoResetCheck();
		}

		private void LoadBootmanTextures()
		{
			Game1.gMan.LoadTexture("the_world_machine/logo_full", TextureCache.CacheType.BootScreen);
			Game1.gMan.LoadTexture("the_world_machine/felix", TextureCache.CacheType.BootScreen);
			Game1.gMan.LoadTexture("partner_logos/fmod_white", TextureCache.CacheType.BootScreen);
			Game1.gMan.LoadTexture("partner_logos/futurecat_white", TextureCache.CacheType.BootScreen);
			Game1.gMan.LoadTexture("partner_logos/komodo_white", TextureCache.CacheType.BootScreen);
		}

		private void setupSystemTextureQueue()
		{
			unloadedSystemTextures = new Queue<string>();
			string[] array = File.ReadAllText(Path.Combine(Game1.GameDataPath(), "twm/system_tex_list.txt")).Split('\n');
			foreach (string text in array)
			{
				unloadedSystemTextures.Enqueue(text.Replace("\r", string.Empty));
			}
		}

		public void EndDemo()
		{
			RestartBootSequence();
			Game1.steamMan.DemoResetCheck();
			populateDemoTitleLines();
			populateDemoEndLines();
			currentStep = BootStep.DEMO_END_FADEIN;
		}

		public void RestartBootSequence()
		{
			Game1.gMan.clearTextureCache(TextureCache.CacheType.Game);
			setupSystemTextureQueue();
			currentStep = BootStep.BREAK_BEFORE_CONSOLE;
			stateTimer = 0;
			skipBootEntirely = false;
			postText = new List<string>();
			Game1.gMan.LoadTexture("the_world_machine/logo_full", TextureCache.CacheType.BootScreen);
		}

		private bool isButtonPressedToSkipSection()
		{
			if (!Game1.inputMan.IsButtonPressed(InputManager.Button.OK) && !Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
			{
				return Game1.inputMan.IsButtonPressed(InputManager.Button.MouseButton);
			}
			return true;
		}

		public void Update()
		{
			if (currentStep == BootStep.COMPLETE)
			{
				return;
			}
			stateTimer++;
			bulbIcon.Update();
			switch (currentStep)
			{
			case BootStep.BEGIN:
				currentStep = BootStep.BREAK_BEFORE_FELIX;
				postText = new List<string>();
				stateTimer = 0;
				populateAutosaveLines();
				break;
			case BootStep.BREAK_BEFORE_FELIX:
				if (stateTimer >= 30 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.FELIX_FADEIN;
				}
				break;
			case BootStep.FELIX_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.FELIX_DISPLAY;
				}
				break;
			case BootStep.FELIX_DISPLAY:
				if (stateTimer >= 120 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.FELIX_FADEOUT;
				}
				break;
			case BootStep.FELIX_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					currentStep = BootStep.FUTURECAT_LOGO_FADEIN;
				}
				break;
			case BootStep.FUTURECAT_LOGO_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.FUTURECAT_LOGO_DISPLAY;
				}
				break;
			case BootStep.FUTURECAT_LOGO_DISPLAY:
				if (stateTimer >= 120 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.FUTURECAT_LOGO_FADEOUT;
				}
				break;
			case BootStep.FUTURECAT_LOGO_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					currentStep = BootStep.KOMODO_LOGO_FADEIN;
				}
				break;
			case BootStep.KOMODO_LOGO_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.KOMODO_LOGO_DISPLAY;
				}
				break;
			case BootStep.KOMODO_LOGO_DISPLAY:
				if (stateTimer >= 120 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.KOMODO_LOGO_FADEOUT;
				}
				break;
			case BootStep.KOMODO_LOGO_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					currentStep = BootStep.FMOD_LOGO_FADEIN;
				}
				break;
			case BootStep.FMOD_LOGO_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.FMOD_LOGO_DISPLAY;
				}
				break;
			case BootStep.FMOD_LOGO_DISPLAY:
				if (stateTimer >= 120 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.FMOD_LOGO_FADEOUT;
				}
				break;
			case BootStep.FMOD_LOGO_FADEOUT:
			{
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer < 30)
				{
					break;
				}
				opacity = 0f;
				stateTimer = 0;
				if (Game1.languageMan.WasLanguageSettingPresentOnStartup)
				{
					currentStep = BootStep.AUTOSAVE_EXPLAIN_FADEIN;
					break;
				}
				languageOptions = Game1.languageMan.GetLanguageOptions();
				for (int i = 0; i < languageOptions.Count; i++)
				{
					if (languageOptions[i].k == Game1.languageMan.GetCurrentLangCode())
					{
						selectedLanguageIndex = i;
						break;
					}
				}
				Game1.soundMan.LoadSound("menu_cursor");
				Game1.soundMan.LoadSound("menu_decision");
				currentStep = BootStep.LANGUAGE_SELECT_FADEIN;
				break;
			}
			case BootStep.LANGUAGE_SELECT_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.LANGUAGE_SELECT;
				}
				break;
			case BootStep.LANGUAGE_SELECT:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left))
				{
					Game1.soundMan.PlaySound("menu_cursor", 0.8f);
					selectedLanguageIndex--;
					if (selectedLanguageIndex < 0)
					{
						selectedLanguageIndex = languageOptions.Count - 1;
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Right))
				{
					Game1.soundMan.PlaySound("menu_cursor", 0.8f);
					selectedLanguageIndex++;
					if (selectedLanguageIndex >= languageOptions.Count)
					{
						selectedLanguageIndex = 0;
					}
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					stateTimer = 0;
					Game1.soundMan.PlaySound("menu_decision", 0.8f);
					currentStep = BootStep.LANGUAGE_SELECT_FADEOUT;
				}
				break;
			case BootStep.LANGUAGE_SELECT_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					Game1.languageMan.SetCurrentLangCode(languageOptions[selectedLanguageIndex].k);
					Game1.gMan.ClearFontCache();
					populateAutosaveLines();
					currentStep = BootStep.AUTOSAVE_EXPLAIN_FADEIN;
				}
				break;
			case BootStep.AUTOSAVE_EXPLAIN_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.AUTOSAVE_EXPLAIN;
				}
				break;
			case BootStep.AUTOSAVE_EXPLAIN:
				if (stateTimer >= 300 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.AUTOSAVE_EXPLAIN_FADEOUT;
				}
				break;
			case BootStep.AUTOSAVE_EXPLAIN_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					if (Game1.steamMan.IsTimedDemo)
					{
						currentStep = BootStep.DEMO_TITLE_FADEIN;
						populateDemoTitleLines();
						Game1.soundMan.LoadSound("menu_decision");
					}
					else
					{
						currentStep = BootStep.BREAK_BEFORE_CONSOLE;
					}
				}
				break;
			case BootStep.DEMO_TITLE_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.DEMO_TITLE_DISPLAY;
				}
				break;
			case BootStep.DEMO_TITLE_DISPLAY:
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					Game1.soundMan.PlaySound("menu_decision", 0.8f);
					stateTimer = 0;
					currentStep = BootStep.DEMO_TITLE_FADEOUT;
				}
				break;
			case BootStep.DEMO_TITLE_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					currentStep = BootStep.DEMO_EXPLAIN_FADEIN;
				}
				break;
			case BootStep.DEMO_EXPLAIN_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.DEMO_EXPLAIN_DISPLAY;
				}
				break;
			case BootStep.DEMO_EXPLAIN_DISPLAY:
				if (stateTimer >= 600 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.DEMO_EXPLAIN_FADEOUT;
				}
				break;
			case BootStep.DEMO_EXPLAIN_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 0f;
					stateTimer = 0;
					currentStep = BootStep.BREAK_BEFORE_CONSOLE;
				}
				break;
			case BootStep.DEMO_END_FADEIN:
				opacity = (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					opacity = 1f;
					stateTimer = 0;
					currentStep = BootStep.DEMO_END_DISPLAY;
				}
				break;
			case BootStep.DEMO_END_DISPLAY:
				if (stateTimer >= 900 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.DEMO_END_FADEOUT;
				}
				break;
			case BootStep.DEMO_END_FADEOUT:
				opacity = 1f - (float)stateTimer / 30f;
				if (stateTimer >= 30)
				{
					if (Game1.steamMan.IsTimedDemo)
					{
						opacity = 0f;
						stateTimer = 0;
						currentStep = BootStep.BEGIN;
					}
					else
					{
						Game1.ShutDown();
					}
				}
				break;
			case BootStep.BREAK_BEFORE_CONSOLE:
				if (stateTimer >= 60 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.CONSOLE_SHOW_BIOS_LOGO;
					Game1.soundMan.PlaySong("computer_startup");
				}
				break;
			case BootStep.CONSOLE_SHOW_BIOS_LOGO:
				opacity = 1f;
				if (isButtonPressedToSkipSection())
				{
					SkipConsole();
				}
				else if (stateTimer >= 60)
				{
					stateTimer = 0;
					currentStep = BootStep.CONSOLE_TEXT;
					SetSrcText("bios");
				}
				break;
			case BootStep.CONSOLE_TEXT:
			case BootStep.CONSOLE_TEXT2:
			case BootStep.CONSOLE_TEXT3:
				if (isButtonPressedToSkipSection())
				{
					SkipConsole();
				}
				else
				{
					if (stateTimer < textDelay)
					{
						break;
					}
					stateTimer = 0;
					textDelay = 0;
					if (!string.IsNullOrEmpty(srcText))
					{
						ProcessText();
						break;
					}
					switch (currentStep)
					{
					case BootStep.CONSOLE_TEXT:
						stateTimer = 0;
						currentStep = BootStep.CONSOLE_LOAD_SOUNDS;
						break;
					case BootStep.CONSOLE_TEXT2:
						stateTimer = 0;
						currentStep = BootStep.CONSOLE_LOAD_TEXTURES;
						break;
					default:
						stateTimer = 0;
						opacity = 0f;
						Game1.soundMan.FadeOutBGM(1f);
						currentStep = BootStep.BREAK_BEFORE_BIG_LOGO;
						break;
					}
				}
				break;
			case BootStep.CONSOLE_LOAD_SOUNDS:
				if (isButtonPressedToSkipSection())
				{
					SkipConsole();
					break;
				}
				if (Game1.soundMan.HasMoreSoundsToLoad())
				{
					postText[postText.Count - 1] = Game1.soundMan.NextSoundToLoad();
					Game1.soundMan.LoadNextSound();
					break;
				}
				postText[postText.Count - 1] = string.Empty;
				SetSrcText("bios2");
				stateTimer = 0;
				currentStep = BootStep.CONSOLE_TEXT2;
				break;
			case BootStep.CONSOLE_LOAD_TEXTURES:
				if (isButtonPressedToSkipSection())
				{
					SkipConsole();
				}
				else
				{
					if (stateTimer <= 1)
					{
						break;
					}
					stateTimer = 0;
					if (unloadedSystemTextures.Count > 0)
					{
						string text = unloadedSystemTextures.Dequeue();
						Game1.gMan.LoadTexture(text, TextureCache.CacheType.TheWorldMachine);
						if (text.StartsWith("the_world_machine/"))
						{
							text = text.Substring("the_world_machine/".Length) + ".xnb";
						}
						postText[postText.Count - 1] = text;
					}
					else
					{
						postText[postText.Count - 1] = string.Empty;
						SetSrcText("bios3");
						currentStep = BootStep.CONSOLE_TEXT3;
					}
				}
				break;
			case BootStep.BREAK_BEFORE_BIG_LOGO:
				if (stateTimer >= 60 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					opacity = 0f;
					currentStep = BootStep.BIG_LOGO_BEFORE_SOUND;
					Game1.soundMan.PlaySong(string.Empty);
					gameRef.InitWindowManager();
				}
				break;
			case BootStep.BIG_LOGO_BEFORE_SOUND:
				opacity = (float)(stateTimer / 3) / 20f;
				if (stateTimer == 30)
				{
					Game1.soundMan.PlaySound("twm_startup");
				}
				if (stateTimer >= 60)
				{
					stateTimer = 0;
					opacity = 1f;
					currentStep = BootStep.BIG_LOGO_AFTER_SOUND;
				}
				break;
			case BootStep.BIG_LOGO_AFTER_SOUND:
				if (stateTimer >= 120 || isButtonPressedToSkipSection())
				{
					stateTimer = 0;
					currentStep = BootStep.TWM_LOGO_FADEOUT;
				}
				break;
			case BootStep.TWM_LOGO_FADEOUT:
				opacity = 1f - (float)(stateTimer / 3) / 40f;
				if (stateTimer >= 120)
				{
					stateTimer = 0;
					opacity = 0f;
					currentStep = BootStep.COMPLETE;
					Game1.gMan.clearTextureCache(TextureCache.CacheType.BootScreen);
				}
				break;
			}
		}

		private void populateAutosaveLines()
		{
			autosaveLines = MathHelper.WordWrap(GraphicsManager.FontType.GameSmall, Game1.languageMan.GetTWMLocString("boot_autosave_explain1") + "\n" + Game1.languageMan.GetTWMLocString("boot_autosave_explain2"), Game1.gMan.DrawScreenSize.X / 2 - 20);
		}

		private void populateDemoTitleLines()
		{
			demoTitleLines = MathHelper.WordWrap(GraphicsManager.FontType.GameSmall, Game1.languageMan.GetTWMLocString("boot_demo_title"), Game1.gMan.DrawScreenSize.X / 2 - 20);
			string input = string.Format(Game1.languageMan.GetTWMLocString("boot_demo_explain"), Game1.steamMan.DemoTimeLimitMinutes);
			demoExplainLines = MathHelper.WordWrap(GraphicsManager.FontType.GameSmall, input, Game1.gMan.DrawScreenSize.X / 2 - 20);
		}

		private void populateDemoEndLines()
		{
			demoEndLines = MathHelper.WordWrap(GraphicsManager.FontType.GameSmall, Game1.languageMan.GetTWMLocString("boot_demo_end"), Game1.gMan.DrawScreenSize.X / 2 - 20);
		}

		private void SetSrcText(string biosTextFileName)
		{
			string path = Path.Combine(Game1.GameDataPath(), "loc/" + Game1.languageMan.GetCurrentLangCode() + "/twm/" + biosTextFileName + ".txt");
			if (!File.Exists(path))
			{
				path = Path.Combine(Game1.GameDataPath(), "loc/en/twm/" + biosTextFileName + ".txt");
			}
			srcText = File.ReadAllText(path);
		}

		private void SkipConsole()
		{
			stateTimer = 0;
			opacity = 0f;
			Game1.soundMan.FadeOutBGM(1f);
			currentStep = BootStep.BREAK_BEFORE_BIG_LOGO;
			Game1.soundMan.LoadAllSounds();
			while (unloadedSystemTextures.Count > 0)
			{
				string texture = unloadedSystemTextures.Dequeue();
				Game1.gMan.LoadTexture(texture, TextureCache.CacheType.TheWorldMachine);
			}
		}

		private void ProcessText()
		{
			while (!string.IsNullOrEmpty(srcText) && textDelay <= 0)
			{
				switch (srcText[0])
				{
				case '@':
				{
					int num = srcText.IndexOf(' ');
					if (num > 0)
					{
						string text = srcText.Substring(1, num - 1);
						srcText = srcText.Substring(num + 1);
						switch (text)
						{
						case "WAIT":
						{
							num = srcText.IndexOf(' ');
							float num2 = float.Parse(srcText.Substring(0, num), CultureInfo.InvariantCulture);
							textDelay = (int)(num2 * 60f);
							srcText = srcText.Substring(num + 1);
							break;
						}
						case "SOUNDS":
							AddStringToConsole(Game1.soundMan.SoundCount.ToString());
							break;
						case "SCANSOUNDS":
							Game1.soundMan.LoadSoundList();
							break;
						default:
							AddStringToConsole("@" + text);
							break;
						}
					}
					else
					{
						AddStringToConsole(srcText[0].ToString());
						srcText = srcText.Substring(1);
					}
					break;
				}
				case '\n':
					postText.Add(string.Empty);
					srcText = srcText.Substring(1);
					textDelay = 12;
					break;
				default:
					AddStringToConsole(srcText[0].ToString());
					srcText = srcText.Substring(1);
					break;
				}
			}
		}

		private void AddStringToConsole(string s)
		{
			if (postText.Count <= 0)
			{
				postText.Add(string.Empty);
			}
			postText[postText.Count - 1] += s;
		}

		public bool ForceBootManagerScreenSize()
		{
			return currentStep < BootStep.BREAK_BEFORE_BIG_LOGO;
		}

		public void Draw()
		{
			if (currentStep == BootStep.COMPLETE)
			{
				return;
			}
			Vec2 drawScreenSize = Game1.gMan.DrawScreenSize;
			Rect boxRect = new Rect(0, 0, drawScreenSize.X, drawScreenSize.Y);
			double num = (double)drawScreenSize.X / (double)drawScreenSize.Y;
			int num2 = 2;
			GameColor white = GameColor.White;
			GameColor black = GameColor.Black;
			white.a = (byte)(255f * opacity);
			switch (currentStep)
			{
			case BootStep.AUTOSAVE_EXPLAIN_FADEIN:
			case BootStep.AUTOSAVE_EXPLAIN:
			case BootStep.AUTOSAVE_EXPLAIN_FADEOUT:
			{
				byte alpha = (byte)(255f * opacity);
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Vec2 drawPos = new Vec2(boxRect.W / 8 - 24, boxRect.H / 8 - 24);
				bulbIcon.Draw(drawPos, alpha, 4);
				Vec2 pixelPos10 = new Vec2(boxRect.W / 4, boxRect.H / 4 + 48);
				{
					foreach (string autosaveLine in autosaveLines)
					{
						Game1.gMan.TextBlitCentered(GraphicsManager.FontType.GameSmall, pixelPos10, autosaveLine, white);
						pixelPos10.Y += 18;
					}
					break;
				}
			}
			case BootStep.LANGUAGE_SELECT_FADEIN:
			case BootStep.LANGUAGE_SELECT:
			case BootStep.LANGUAGE_SELECT_FADEOUT:
			{
				Game1.gMan.ColorBoxBlit(boxRect, black);
				GameColor gameColor = new GameColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)(255f * opacity));
				Vec2 vec5 = new Vec2(Game1.gMan.DrawScreenSize.X / 4, Game1.gMan.DrawScreenSize.Y / 4 - 16);
				Game1.gMan.TextBlitCentered(GraphicsManager.FontType.Game, vec5, Game1.languageMan.GetTWMLocString("boot_lang_select"), gameColor);
				vec5.Y += 20;
				string textureName = "the_world_machine/languages/" + languageOptions[selectedLanguageIndex].k;
				Vec2 vec6 = Game1.gMan.TextureSize(textureName);
				Vec2 vec7 = vec5;
				vec7.X -= vec6.X / 2;
				Game1.gMan.MainBlit(textureName, vec7, gameColor);
				Game1.gMan.MainBlit("the_world_machine/window_icons/arrow_left", vec7 + new Vec2(-16, 8), gameColor);
				Game1.gMan.MainBlit("the_world_machine/window_icons/arrow_right", vec7 + new Vec2(vec6.X, 8), gameColor);
				vec5.Y += 32;
				Game1.gMan.TextBlitCentered(GraphicsManager.FontType.GameSmall, vec5, Game1.languageMan.GetTWMLocString("boot_lang_select_hint"), gameColor, GraphicsManager.BlendMode.Normal, 2, checkForGlyphes: true);
				break;
			}
			case BootStep.FUTURECAT_LOGO_FADEIN:
			case BootStep.FUTURECAT_LOGO_DISPLAY:
			case BootStep.FUTURECAT_LOGO_FADEOUT:
			{
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Vec2 vec2 = Game1.gMan.TextureSize("partner_logos/futurecat_white", TextureCache.CacheType.BootScreen);
				Vec2 pixelPos = new Vec2(boxRect.W - vec2.X, boxRect.H - vec2.Y) / 2;
				Game1.gMan.MainBlit("partner_logos/futurecat_white", pixelPos, white, 0, GraphicsManager.BlendMode.Normal, 1, TextureCache.CacheType.BootScreen);
				break;
			}
			case BootStep.FMOD_LOGO_FADEIN:
			case BootStep.FMOD_LOGO_DISPLAY:
			case BootStep.FMOD_LOGO_FADEOUT:
			{
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Vec2 vec4 = Game1.gMan.TextureSize("partner_logos/fmod_white", TextureCache.CacheType.BootScreen);
				Vec2 pixelPos = new Vec2(boxRect.W - vec4.X, boxRect.H - vec4.Y) / 2;
				float num9 = 1f;
				if (vec4.X > boxRect.W - 60)
				{
					num9 = (float)(boxRect.W - 60) / (float)vec4.X;
					pixelPos = new Vec2(boxRect.W - (int)((float)vec4.X * num9), boxRect.H - (int)((float)vec4.Y * num9)) / 2;
				}
				Game1.gMan.MainBlit("partner_logos/fmod_white", pixelPos, new Rect(0, 0, vec4.X, vec4.Y), num9, num9, opacity, 0, GraphicsManager.BlendMode.LinearStretch, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.BootScreen);
				break;
			}
			case BootStep.FELIX_FADEIN:
			case BootStep.FELIX_DISPLAY:
			case BootStep.FELIX_FADEOUT:
			{
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Vec2 vec3 = Game1.gMan.TextureSize("the_world_machine/felix", TextureCache.CacheType.BootScreen);
				float num7;
				Vec2 pixelPos2;
				if ((double)vec3.X / (double)vec3.Y <= num)
				{
					int num6 = vec3.X * boxRect.H / vec3.Y;
					num7 = (float)num6 / (float)vec3.X;
					pixelPos2 = new Vec2((boxRect.W - num6) / 2, 0);
				}
				else
				{
					int num8 = vec3.Y * boxRect.W / vec3.X;
					num7 = (float)num8 / (float)vec3.Y;
					pixelPos2 = new Vec2(0, (boxRect.H - num8) / 2);
				}
				Game1.gMan.MainBlit("the_world_machine/felix", pixelPos2, new Rect(0, 0, vec3.X, vec3.Y), num7, num7, opacity, 0, GraphicsManager.BlendMode.LinearStretch, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.BootScreen);
				break;
			}
			case BootStep.KOMODO_LOGO_FADEIN:
			case BootStep.KOMODO_LOGO_DISPLAY:
			case BootStep.KOMODO_LOGO_FADEOUT:
			{
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Vec2 vec13 = Game1.gMan.TextureSize("partner_logos/komodo_white", TextureCache.CacheType.BootScreen);
				float num18;
				Vec2 pixelPos9;
				if ((double)vec13.X / (double)vec13.Y <= num)
				{
					int num17 = vec13.X * boxRect.H / vec13.Y;
					num18 = (float)num17 / (float)vec13.X;
					pixelPos9 = new Vec2((boxRect.W - num17) / 2, 0);
				}
				else
				{
					int num19 = vec13.Y * boxRect.W / vec13.X;
					num18 = (float)num19 / (float)vec13.Y;
					pixelPos9 = new Vec2(0, (boxRect.H - num19) / 2);
				}
				Game1.gMan.MainBlit("partner_logos/komodo_white", pixelPos9, new Rect(0, 0, vec13.X, vec13.Y), num18, num18, opacity, 0, GraphicsManager.BlendMode.LinearStretch, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.BootScreen);
				break;
			}
			case BootStep.DEMO_TITLE_FADEIN:
			case BootStep.DEMO_TITLE_DISPLAY:
			case BootStep.DEMO_TITLE_FADEOUT:
			{
				Vec2 vec12 = Game1.gMan.TextureSize("the_world_machine/demo_title");
				int num16 = 36 + demoTitleLines.Count * 18 * 2;
				Vec2 pixelPos7 = new Vec2((boxRect.W - vec12.X) / 2, (boxRect.H - (vec12.Y + num16)) / 2);
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Game1.gMan.MainBlit("the_world_machine/demo_title", pixelPos7, opacity, 0, GraphicsManager.BlendMode.Normal, 1);
				Vec2 pixelPos8 = new Vec2(boxRect.W / 4, (pixelPos7.Y + vec12.Y) / 2 + 18);
				{
					foreach (string demoTitleLine in demoTitleLines)
					{
						Game1.gMan.TextBlitCentered(GraphicsManager.FontType.GameSmall, pixelPos8, demoTitleLine, white);
						pixelPos8.Y += 18;
					}
					break;
				}
			}
			case BootStep.DEMO_EXPLAIN_FADEIN:
			case BootStep.DEMO_EXPLAIN_DISPLAY:
			case BootStep.DEMO_EXPLAIN_FADEOUT:
			{
				string textureName2 = "the_world_machine/window_icons/timer";
				string demoTimeString = Game1.steamMan.GetDemoTimeString();
				Vec2 vec9 = Game1.gMan.TextureSize(textureName2, TextureCache.CacheType.TheWorldMachine) * 4;
				int num14 = Game1.gMan.TextSize(GraphicsManager.FontType.OS, demoTimeString).X * 4;
				int num15 = 36 + demoTitleLines.Count * 18 * 2;
				Vec2 vec10 = new Vec2((boxRect.W - vec9.X - num14) / 2, (boxRect.H - (vec9.Y + num15)) / 2);
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Game1.gMan.MainBlit(textureName2, vec10 / 4, opacity, 0, GraphicsManager.BlendMode.Normal, 4);
				Vec2 vec11 = new Vec2(vec10.X + vec9.X, vec10.Y);
				Game1.gMan.TextBlit(GraphicsManager.FontType.OS, vec11 / 4, demoTimeString, white, GraphicsManager.BlendMode.Normal, 4);
				Vec2 pixelPos6 = new Vec2(boxRect.W / 4, (vec10.Y + vec9.Y) / 2 + 18);
				{
					foreach (string demoExplainLine in demoExplainLines)
					{
						Game1.gMan.TextBlitCentered(GraphicsManager.FontType.GameSmall, pixelPos6, demoExplainLine, white);
						pixelPos6.Y += 18;
					}
					break;
				}
			}
			case BootStep.DEMO_END_FADEIN:
			case BootStep.DEMO_END_DISPLAY:
			case BootStep.DEMO_END_FADEOUT:
			{
				Vec2 vec8 = Game1.gMan.TextureSize("the_world_machine/demo_end");
				int num12 = 36 + demoEndLines.Count * 18 * 2;
				int num13 = (boxRect.H - (vec8.Y + num12)) / 2;
				Vec2 pixelPos4 = new Vec2((boxRect.W - vec8.X) / 2, num13 + num12);
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Game1.gMan.MainBlit("the_world_machine/demo_end", pixelPos4, opacity, 0, GraphicsManager.BlendMode.Normal, 1);
				Vec2 pixelPos5 = new Vec2(boxRect.W / 4, num13 / 2);
				{
					foreach (string demoEndLine in demoEndLines)
					{
						Game1.gMan.TextBlitCentered(GraphicsManager.FontType.GameSmall, pixelPos5, demoEndLine, white);
						pixelPos5.Y += 18;
					}
					break;
				}
			}
			case BootStep.CONSOLE_SHOW_BIOS_LOGO:
			case BootStep.CONSOLE_TEXT:
			case BootStep.CONSOLE_LOAD_SOUNDS:
			case BootStep.CONSOLE_TEXT2:
			case BootStep.CONSOLE_LOAD_TEXTURES:
			case BootStep.CONSOLE_TEXT3:
			{
				Game1.gMan.ColorBoxBlit(boxRect, black);
				Vec2 pixelPos = Vec2.Zero;
				Game1.gMan.MainBlit("the_world_machine/logo", pixelPos, white, 0, GraphicsManager.BlendMode.Normal, num2, TextureCache.CacheType.BootScreen);
				GameColor gColor = bioTextColor;
				gColor.a = white.a;
				int x = Game1.gMan.TextSize(GraphicsManager.FontType.Game, Game1.VersionString).X;
				Game1.gMan.TextBlit(GraphicsManager.FontType.Game, new Vec2(boxRect.W / num2 - x - 10, 4), Game1.VersionString, gColor, GraphicsManager.BlendMode.Normal, num2);
				int num10 = 100;
				int num11 = (boxRect.H / num2 - num10 - 4) / 22;
				for (int i = Math.Max(0, postText.Count - num11); i < postText.Count; i++)
				{
					string text = postText[i];
					Vec2 pixelPos3 = new Vec2(10, num10);
					Game1.gMan.TextBlit(GraphicsManager.FontType.Game, pixelPos3, text, gColor, GraphicsManager.BlendMode.Normal, num2);
					num10 += 22;
				}
				break;
			}
			case BootStep.BIG_LOGO_BEFORE_SOUND:
			case BootStep.BIG_LOGO_AFTER_SOUND:
			case BootStep.TWM_LOGO_FADEOUT:
			{
				Game1.gMan.ColorBoxBlit(boxRect, new GameColor(black.r, black.g, black.b, (byte)(opacity * 255f)), GraphicsManager.BlendMode.Dither);
				Vec2 vec = Game1.gMan.TextureSize("the_world_machine/logo_full", TextureCache.CacheType.BootScreen);
				float num4;
				Vec2 pixelPos;
				if ((double)vec.X / (double)vec.Y <= num)
				{
					int num3 = vec.X * boxRect.H / vec.Y;
					num4 = (float)num3 / (float)vec.X;
					pixelPos = new Vec2((boxRect.W - num3) / 2, 0);
				}
				else
				{
					int num5 = vec.Y * boxRect.W / vec.X;
					num4 = (float)num5 / (float)vec.Y;
					pixelPos = new Vec2(0, (boxRect.H - num5) / 2);
				}
				Game1.gMan.MainBlit("the_world_machine/logo_full", pixelPos, new Rect(0, 0, vec.X, vec.Y), num4, num4, opacity, 0, GraphicsManager.BlendMode.Dither, default(GameTone), 1f, 1f, 1f, 0f, TextureCache.CacheType.BootScreen);
				break;
			}
			case BootStep.BREAK_BEFORE_CONSOLE:
			case BootStep.BREAK_BEFORE_BIG_LOGO:
				break;
			}
		}
	}
}

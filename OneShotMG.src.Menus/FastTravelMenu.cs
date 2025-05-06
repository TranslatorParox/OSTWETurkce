using System.Collections.Generic;
using System.Linq;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;

namespace OneShotMG.src.Menus
{
	public class FastTravelMenu : AbstractMenu
	{
		private readonly OneshotWindow oneshotWindow;

		private int opacity;

		private const int OPEN_CLOSE_OPACITY_STEP = 20;

		private const GraphicsManager.FontType MenuFont = GraphicsManager.FontType.Game;

		private const int TITLE_LEFT_MARGIN = 13;

		private const int TITLE_TOP_MARGIN = 30;

		private const int MAP_NAME_BOTTOM_MARGIN = 60;

		private const string CurrentPosIcon = "ui/minimap/niko_icon";

		private List<FastTravelManager.FastTravelLocation> fastTravelOptions;

		private List<MinimapInfo> minimap;

		private Dictionary<int, List<MinimapEdge>> minimapEdges;

		private List<GlitchEffect> minimapGlitches;

		private int currentOptionId;

		private List<MinimapEdge> currentEdges;

		private FastTravelManager.FastTravelLocation selectedOption;

		private const int FADE_TIME_TOTAL = 30;

		private Vec2 redOffset = Vec2.Zero;

		private Vec2 greenOffset = Vec2.Zero;

		private Vec2 blueOffset = Vec2.Zero;

		private int redAbberateTimer;

		private int greenAbberateTimer;

		private int blueAbberateTimer;

		private Vec2 cursorPos = Vec2.Zero;

		private Vec2 cursorPosTarget = Vec2.Zero;

		private Vec2 cursorPosStart = Vec2.Zero;

		private int cursorPosMoveTimer;

		private const int CURSOR_POS_MOVE_TIME = 20;

		private int mapSelectMoveShakeTimer;

		private Vec2 selectedMapMoveShake = Vec2.Zero;

		private const int MAP_SELECT_MOVE_SHAKE_TIME = 4;

		private int arrowAnimTimer;

		private const int ARROW_ANIM_CYCLE = 40;

		private TempTexture zoneTexture;

		private TempTexture mapNameTexture;

		public FastTravelMenu(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
		}

		public override void Draw()
		{
			GameColor gameColor = new GameColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)opacity);
			Game1.gMan.ColorBoxBlit(new Rect(0, 0, 320, 240), new GameColor(0, 0, 0, (byte)(opacity * 180 / 255)));
			DrawMinimap();
			Game1.gMan.MainBlit(zoneTexture, new Vec2(13, 30), gameColor);
			Game1.gMan.MainBlit(mapNameTexture, new Vec2(320 - mapNameTexture.renderTarget.Width / 2, 420), gameColor, 0, GraphicsManager.BlendMode.Normal, 1);
		}

		private void DrawZoneNameTexture()
		{
			string currentZoneName = oneshotWindow.fastTravelMan.GetCurrentZoneName();
			zoneTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, currentZoneName);
		}

		private void DrawMapNameTexture()
		{
			string mapName = oneshotWindow.tileMapMan.GetMapName(currentOptionId);
			mapNameTexture = Game1.gMan.TempTexMan.GetSingleLineTexture(GraphicsManager.FontType.Game, mapName);
		}

		private void DrawMinimap()
		{
			float num = (float)opacity / 255f;
			oneshotWindow.tileMapMan.GetMapID();
			if (oneshotWindow.menuMan.SettingsMenu.IsChromaAberrationEnabled)
			{
				for (int i = 0; i < minimap.Count; i++)
				{
					MinimapInfo mapPiece = minimap[i];
					if (!fastTravelOptions.Exists((FastTravelManager.FastTravelLocation fto) => fto.mapId == mapPiece.MapId))
					{
						continue;
					}
					bool num2 = fastTravelOptions.Count > 0 && currentOptionId == mapPiece.MapId;
					string textureName = "ui/minimap/" + mapPiece.Image;
					float num3 = 1f;
					float num4 = 1f;
					float num5 = 1f;
					if (!num2)
					{
						switch (oneshotWindow.fastTravelMan.CurrentZone)
						{
						default:
							num3 = 0.33f;
							num4 = 0.3f;
							num5 = 0.82f;
							break;
						case FastTravelManager.FastTravelZone.Green:
							num3 = 0.2f;
							num4 = 0.66f;
							num5 = 0.45f;
							break;
						case FastTravelManager.FastTravelZone.Red:
						case FastTravelManager.FastTravelZone.RedGround:
							num3 = 0.84f;
							num4 = 0.13f;
							num5 = 0.4f;
							break;
						}
					}
					if (oneshotWindow.flagMan.IsSolticeGlitchTime() && minimapGlitches.Count > i)
					{
						GlitchEffect glitchEffect = minimapGlitches[i];
						glitchEffect.Draw(textureName, mapPiece.Offset + redOffset, num / 2f, num3, 0f, 0f, GraphicsManager.BlendMode.Additive);
						glitchEffect.Draw(textureName, mapPiece.Offset + greenOffset, num / 2f, 0f, num4, 0f, GraphicsManager.BlendMode.Additive);
						glitchEffect.Draw(textureName, mapPiece.Offset + blueOffset, num / 2f, 0f, 0f, num5, GraphicsManager.BlendMode.Additive);
						continue;
					}
					GraphicsManager gMan = Game1.gMan;
					Vec2 pixelPos = mapPiece.Offset + redOffset;
					float alpha = num / 2f;
					float red = num3;
					gMan.MainBlit(textureName, pixelPos, alpha, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), red, 0f, 0f);
					GraphicsManager gMan2 = Game1.gMan;
					Vec2 pixelPos2 = mapPiece.Offset + greenOffset;
					float alpha2 = num / 2f;
					red = num4;
					gMan2.MainBlit(textureName, pixelPos2, alpha2, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 0f, red, 0f);
					GraphicsManager gMan3 = Game1.gMan;
					Vec2 pixelPos3 = mapPiece.Offset + blueOffset;
					float alpha3 = num / 2f;
					red = num5;
					gMan3.MainBlit(textureName, pixelPos3, alpha3, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 0f, 0f, red);
				}
			}
			for (int j = 0; j < minimap.Count; j++)
			{
				MinimapInfo mapPiece2 = minimap[j];
				if (!fastTravelOptions.Exists((FastTravelManager.FastTravelLocation fto) => fto.mapId == mapPiece2.MapId))
				{
					continue;
				}
				bool flag = fastTravelOptions.Count > 0 && currentOptionId == mapPiece2.MapId;
				string textureName2 = "ui/minimap/" + mapPiece2.Image;
				float num6 = 1f;
				float num7 = 1f;
				float num8 = 1f;
				if (!flag)
				{
					switch (oneshotWindow.fastTravelMan.CurrentZone)
					{
					default:
						num6 = 0.33f;
						num7 = 0.3f;
						num8 = 0.82f;
						break;
					case FastTravelManager.FastTravelZone.Green:
						num6 = 0.2f;
						num7 = 0.66f;
						num8 = 0.45f;
						break;
					case FastTravelManager.FastTravelZone.Red:
					case FastTravelManager.FastTravelZone.RedGround:
						num6 = 0.84f;
						num7 = 0.13f;
						num8 = 0.4f;
						break;
					}
				}
				if (oneshotWindow.flagMan.IsSolticeGlitchTime() && minimapGlitches.Count > j)
				{
					minimapGlitches[j].Draw(textureName2, flag ? (mapPiece2.Offset + selectedMapMoveShake) : mapPiece2.Offset, num, num6, num7, num8);
					continue;
				}
				GraphicsManager gMan4 = Game1.gMan;
				Vec2 pixelPos4 = (flag ? (mapPiece2.Offset + selectedMapMoveShake) : mapPiece2.Offset);
				float red = num6;
				float green = num7;
				float blue = num8;
				gMan4.MainBlit(textureName2, pixelPos4, num, 0, GraphicsManager.BlendMode.Normal, 2, default(GameTone), red, green, blue);
			}
			Vec2 vec = new Vec2(-12, -12);
			Game1.gMan.MainBlit("ui/minimap/niko_icon", vec + cursorPos, num);
			if (oneshotWindow.menuMan.SettingsMenu.IsChromaAberrationEnabled)
			{
				Game1.gMan.MainBlit("ui/minimap/niko_icon", vec + cursorPos + redOffset, num / 3f, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 1f, 0f, 0f);
				Game1.gMan.MainBlit("ui/minimap/niko_icon", vec + cursorPos + greenOffset, num / 3f, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 0f, 1f, 0f);
				Game1.gMan.MainBlit("ui/minimap/niko_icon", vec + cursorPos + blueOffset, num / 3f, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 0f, 0f);
			}
			int num9 = arrowAnimTimer / 20;
			using (List<MinimapEdge>.Enumerator enumerator = currentEdges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Rect srcRect;
					Vec2 vec2;
					switch (enumerator.Current.direction)
					{
					default:
						srcRect = new Rect(0, 0, 16, 16);
						vec2 = new Vec2(-8, -24 - num9);
						break;
					case EdgeDirection.down:
						srcRect = new Rect(16, 0, 16, 16);
						vec2 = new Vec2(-8, num9);
						break;
					case EdgeDirection.left:
						srcRect = new Rect(32, 0, 16, 16);
						vec2 = new Vec2(-20 - num9, -12);
						break;
					case EdgeDirection.right:
						srcRect = new Rect(48, 0, 16, 16);
						vec2 = new Vec2(4 + num9, -12);
						break;
					}
					Game1.gMan.MainBlit("ui/minimap/small_arrows", vec2 + cursorPos, srcRect, num);
					if (oneshotWindow.menuMan.SettingsMenu.IsChromaAberrationEnabled)
					{
						Game1.gMan.MainBlit("ui/minimap/small_arrows", vec2 + cursorPos + redOffset, srcRect, num / 2f, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 1f, 0f, 0f);
						Game1.gMan.MainBlit("ui/minimap/small_arrows", vec2 + cursorPos + greenOffset, srcRect, num / 2f, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 0f, 1f, 0f);
						Game1.gMan.MainBlit("ui/minimap/small_arrows", vec2 + cursorPos + blueOffset, srcRect, num / 2f, 0, GraphicsManager.BlendMode.Additive, 2, default(GameTone), 0f, 0f);
					}
				}
			}
		}

		public override void Update()
		{
			if (IsOpen())
			{
				if (zoneTexture == null || !zoneTexture.isValid)
				{
					DrawZoneNameTexture();
				}
				zoneTexture.KeepAlive();
				if (mapNameTexture == null || !mapNameTexture.isValid)
				{
					DrawMapNameTexture();
				}
				mapNameTexture.KeepAlive();
			}
			switch (state)
			{
			case MenuState.Opening:
				opacity += 20;
				if (opacity >= 255)
				{
					opacity = 255;
					state = MenuState.Open;
				}
				break;
			case MenuState.Closing:
				opacity -= 20;
				if (opacity <= 0)
				{
					opacity = 0;
					if (selectedOption != null && selectedOption.mapId != oneshotWindow.tileMapMan.GetMapID())
					{
						oneshotWindow.tileMapMan.SetScreenTone(GameTone.Black, 30);
						state = MenuState.FastTravelFadeOut;
						fadeTimer = 0;
					}
					else
					{
						state = MenuState.Closed;
					}
				}
				break;
			case MenuState.FastTravelFadeOut:
				fadeTimer++;
				if (fadeTimer >= 30)
				{
					oneshotWindow.tileMapMan.ChangeMap(selectedOption.mapId, selectedOption.tilePos.X, selectedOption.tilePos.Y, 0f, selectedOption.direction);
					oneshotWindow.tileMapMan.SetScreenTone(GameTone.Zero, 30);
					state = MenuState.FastTravelFadeIn;
					fadeTimer = 0;
				}
				break;
			case MenuState.FastTravelFadeIn:
				fadeTimer++;
				if (fadeTimer >= 30)
				{
					state = MenuState.Closed;
					fadeTimer = 0;
				}
				break;
			case MenuState.Open:
			{
				int id = currentOptionId;
				if (Game1.inputMan.IsButtonPressed(InputManager.Button.Up) && currentEdges.Any((MinimapEdge e) => e.direction == EdgeDirection.up))
				{
					id = currentEdges.First((MinimapEdge e) => e.direction == EdgeDirection.up).id;
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Down) && currentEdges.Any((MinimapEdge e) => e.direction == EdgeDirection.down))
				{
					id = currentEdges.First((MinimapEdge e) => e.direction == EdgeDirection.down).id;
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Left) && currentEdges.Any((MinimapEdge e) => e.direction == EdgeDirection.left))
				{
					id = currentEdges.First((MinimapEdge e) => e.direction == EdgeDirection.left).id;
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Right) && currentEdges.Any((MinimapEdge e) => e.direction == EdgeDirection.right))
				{
					id = currentEdges.First((MinimapEdge e) => e.direction == EdgeDirection.right).id;
				}
				if (id != currentOptionId)
				{
					currentOptionId = id;
					Game1.soundMan.PlaySound("menu_cursor");
					SetMinimapTarget(currentOptionId);
					DrawMapNameTexture();
				}
				if (fastTravelOptions.Count > 0 && Game1.inputMan.IsButtonPressed(InputManager.Button.OK))
				{
					selectedOption = fastTravelOptions.First((FastTravelManager.FastTravelLocation fto) => fto.mapId == currentOptionId);
					Game1.soundMan.PlaySound("menu_decision");
					Close();
				}
				else if (Game1.inputMan.IsButtonPressed(InputManager.Button.Cancel))
				{
					Game1.soundMan.PlaySound("menu_cancel");
					Close();
				}
				break;
			}
			}
			if (state == MenuState.Closed)
			{
				return;
			}
			int strength = ((!oneshotWindow.flagMan.IsSolticeGlitchTime()) ? 1 : 2);
			MathHelper.HandleAbberateUpdate(ref redAbberateTimer, ref redOffset, strength);
			MathHelper.HandleAbberateUpdate(ref blueAbberateTimer, ref blueOffset, strength);
			MathHelper.HandleAbberateUpdate(ref greenAbberateTimer, ref greenOffset, strength);
			if (cursorPosMoveTimer > 0)
			{
				cursorPosMoveTimer--;
				cursorPos = new Vec2(MathHelper.EaseIn(cursorPosStart.X, cursorPosTarget.X, 20 - cursorPosMoveTimer, 20), MathHelper.EaseIn(cursorPosStart.Y, cursorPosTarget.Y, 20 - cursorPosMoveTimer, 20));
			}
			if (mapSelectMoveShakeTimer > 0)
			{
				mapSelectMoveShakeTimer--;
				selectedMapMoveShake = new Vec2(0, mapSelectMoveShakeTimer % 2 * 2 - 1);
			}
			else
			{
				selectedMapMoveShake = Vec2.Zero;
			}
			arrowAnimTimer++;
			if (arrowAnimTimer >= 40)
			{
				arrowAnimTimer = 0;
			}
			if (!oneshotWindow.flagMan.IsSolticeGlitchTime())
			{
				return;
			}
			foreach (GlitchEffect minimapGlitch in minimapGlitches)
			{
				minimapGlitch.Update();
			}
		}

		private void SetMinimapTarget(int currentMapId)
		{
			MinimapInfo minimapInfo = minimap.Find((MinimapInfo mp) => mp.MapId == currentMapId);
			if (minimapInfo != null)
			{
				Vec2 vec;
				if (minimapInfo.HasWarp)
				{
					vec = minimapInfo.WarpPoint;
				}
				else
				{
					Vec2 vec2 = Game1.gMan.TextureSize("ui/minimap/" + minimapInfo.Image);
					vec = minimapInfo.Offset + vec2 / 2;
				}
				cursorPosStart = cursorPos;
				cursorPosTarget = vec;
				cursorPosMoveTimer = 20;
				mapSelectMoveShakeTimer = 4;
			}
			currentEdges = new List<MinimapEdge>();
			if (!minimapEdges.TryGetValue(currentOptionId, out var value))
			{
				return;
			}
			foreach (MinimapEdge e in value)
			{
				if (fastTravelOptions.Any((FastTravelManager.FastTravelLocation fto) => fto.mapId == e.id))
				{
					currentEdges.Add(e);
				}
			}
		}

		public override void Close()
		{
			state = MenuState.Closing;
			opacity = 255;
		}

		public override void Open()
		{
			Game1.soundMan.PlaySound("menu_decision");
			state = MenuState.Opening;
			opacity = 0;
			selectedOption = null;
			fastTravelOptions = oneshotWindow.fastTravelMan.GetCurrentFastTravels();
			minimap = oneshotWindow.fastTravelMan.GetCurrentMinimap();
			minimapEdges = oneshotWindow.fastTravelMan.GetCurrentMinimapEdges();
			minimapGlitches = new List<GlitchEffect>();
			foreach (MinimapInfo item in minimap)
			{
				string textureName = "ui/minimap/" + item.Image;
				Vec2 size = ((item.MapId > 0) ? Game1.gMan.TextureSize(textureName) : new Vec2(16, 16));
				minimapGlitches.Add(new GlitchEffect(size, 50, 60, 180, 20));
			}
			currentOptionId = oneshotWindow.tileMapMan.GetMapID();
			if (!fastTravelOptions.Any((FastTravelManager.FastTravelLocation fto) => fto.mapId == currentOptionId))
			{
				currentOptionId = fastTravelOptions[0].mapId;
			}
			if (fastTravelOptions.Count > 0)
			{
				SetMinimapTarget(currentOptionId);
				cursorPos = cursorPosTarget;
				cursorPosMoveTimer = 0;
			}
			DrawZoneNameTexture();
			DrawMapNameTexture();
		}
	}
}

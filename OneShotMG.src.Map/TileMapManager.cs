using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using OneShotMG.src.EngineSpecificCode;
using OneShotMG.src.Entities;
using OneShotMG.src.TWM;
using OneShotMG.src.Util;
using Tiled;

namespace OneShotMG.src.Map
{
	public class TileMapManager
	{
		private class EntityWithDistance
		{
			public Entity e;

			public int distance;
		}

		private readonly OneshotWindow oneshotWindow;

		private const int MAX_LAYER_HEIGHT = 6;

		private const float HASH_TILE_SCALAR = 1f;

		private const int PLAYER_ENTITY_ID = 999999;

		private string currentMapName;

		private int currentMapId;

		private Vec2 mapSize;

		private Vec2 tileSize;

		private Vec2 hashTileSize;

		private Vec2 hashMapSize;

		private Panorama mapPanorama;

		private Fog mapFog;

		private Dictionary<int, List<TileLayer>> heightsToLayers;

		private List<TileSet> tilesets;

		private List<Entity> entities;

		private List<Entity> entitiesToSpawn;

		private Dictionary<int, Entity> entitiesById;

		private List<MapLine> mapLines;

		private Dictionary<int, List<MapLine>> hashedMapLines;

		private Dictionary<int, List<Entity>> hashedEntities;

		private Player player;

		private Entity camFocusEntity;

		private Vec2 oldCamPos = Vec2.Zero;

		private Vec2 camPos = Vec2.Zero;

		private LinkedList<Entity> entitiesByHeight;

		private bool debugDrawCollision;

		private int queuedCommonEventId = -1;

		private EventRunner mainEventRunner;

		private Dictionary<int, EventRunner> parallelEventRunners;

		private const int COMMON_EVENT_PARALLEL_EVENT_START_INDEX = 10000;

		private Dictionary<int, TilesetInfoJson> tilesetInfos;

		private int currentTilesetId;

		private bool[] collisionTiles;

		private byte[] terrainTiles;

		private bool[] counterTiles;

		private int changeMapTimer;

		private int totalChangeMapTime;

		private GraphicsManager.BlendMode mapTransitionBlendMode = GraphicsManager.BlendMode.Normal;

		private int entityIDcounter;

		private Dictionary<int, CommonEvent> commonEvents;

		private Dictionary<int, string> mapNames;

		private Entity.Direction scrollDirection;

		private int scrollSpeed;

		private int scrollDistanceRemaining;

		private Vec2 scrollOffset = Vec2.Zero;

		private GameTone ambientTone = GameTone.Zero;

		private GameTone screenTone = GameTone.Zero;

		private GameTone startScreenTone = GameTone.Zero;

		private GameTone targetScreenTone = GameTone.Zero;

		private int screenToneShiftTimer;

		private int screenToneShiftTotalTime;

		private int bulbLightTimer;

		private const int BULB_LIGHT_TIME_TOTAL = 120;

		private const int LIGHTBULB_ITEM_ID = 1;

		private List<List<string>> overrideStepSounds;

		private string background;

		private int shakePower;

		private int shakeSpeed;

		private int shakeDuration;

		private int shakeDirection = 1;

		private float shakeOffset;

		private List<FireflyParticle> particles;

		private bool wasMapChangedDuringUpdate;

		private Dictionary<int, Entity> entitiesToCheck = new Dictionary<int, Entity>();

		private List<Entity> entitiesToKill = new List<Entity>();

		private List<int> parallelEventsToKill = new List<int>();

		private List<EntityWithDistance> entitiesToCollideWith = new List<EntityWithDistance>();

		private readonly Tuple<List<string>, float> emptyStepSounds = new Tuple<List<string>, float>(null, 1f);

		private List<Vec2> tilesToCollideWith = new List<Vec2>();

		private Dictionary<int, MapLine> linesToCollideWith = new Dictionary<int, MapLine>();

		private Vec2[] correctRamPositions = new Vec2[5]
		{
			new Vec2(80, 59),
			new Vec2(79, 63),
			new Vec2(80, 65),
			new Vec2(83, 59),
			new Vec2(85, 61)
		};

		public bool Wrapping { get; private set; }

		public string GetMapName(int mapId)
		{
			if (mapId >= 0 && mapId < mapNames.Count)
			{
				return Game1.languageMan.GetMapNameLocString(mapId, mapNames[mapId]);
			}
			return Game1.languageMan.GetTWMLocString("mapname_no_map_name");
		}

		public TileMapManager(OneshotWindow osWindow)
		{
			oneshotWindow = osWindow;
			parallelEventRunners = new Dictionary<int, EventRunner>();
			tilesetInfos = new Dictionary<int, TilesetInfoJson>();
			foreach (TilesetInfoJson tileset in JsonConvert.DeserializeObject<TilesetsInfoJson>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_tilesets.json")).tilesets)
			{
				tilesetInfos.Add(tileset.id, tileset);
			}
			commonEvents = new Dictionary<int, CommonEvent>();
			CommonEvent[] common_events = JsonConvert.DeserializeObject<CommonEvents>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_common_events.json")).common_events;
			foreach (CommonEvent commonEvent in common_events)
			{
				commonEvents.Add(commonEvent.id, commonEvent);
			}
			mapNames = new Dictionary<int, string>();
			foreach (MapNameJson map_name in JsonConvert.DeserializeObject<MapNamesJson>(File.ReadAllText(Game1.GameDataPath() + "/oneshot_map_names.json")).map_names)
			{
				mapNames.Add(map_name.id, map_name.name);
			}
		}

		public void LoadMap(int mapId, float playerTileX, float playerTileY)
		{
			wasMapChangedDuringUpdate = true;
			heightsToLayers = new Dictionary<int, List<TileLayer>>();
			mapPanorama = null;
			mapFog = null;
			entitiesByHeight = new LinkedList<Entity>(new List<Entity>());
			tilesets = new List<TileSet>();
			entities = new List<Entity>();
			entitiesToSpawn = new List<Entity>();
			mapLines = new List<MapLine>();
			hashedMapLines = new Dictionary<int, List<MapLine>>();
			hashedEntities = new Dictionary<int, List<Entity>>();
			entitiesById = new Dictionary<int, Entity>();
			overrideStepSounds = null;
			background = null;
			particles = null;
			entityIDcounter = 0;
			currentMapName = $"map{mapId}";
			currentMapId = mapId;
			ambientTone = GameTone.Zero;
			scrollOffset = Vec2.Zero;
			scrollDistanceRemaining = 0;
			scrollSpeed = 0;
			scrollDirection = Entity.Direction.None;
			Wrapping = false;
			List<TileLayer> list = new List<TileLayer>();
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Tiled.Map));
			Tiled.Map map;
			using (Stream stream = new FileStream(Game1.GameDataPath() + "/maps/" + currentMapName + ".tmx", FileMode.Open))
			{
				map = (Tiled.Map)xmlSerializer.Deserialize(stream);
			}
			mapSize.X = map.width;
			mapSize.Y = map.height;
			tileSize.X = map.tilewidth;
			tileSize.Y = map.tileheight;
			hashTileSize.X = (int)((float)tileSize.X * 1f);
			hashTileSize.Y = (int)((float)tileSize.Y * 1f);
			hashMapSize.X = (int)((float)mapSize.X / 1f) + 1;
			hashMapSize.Y = (int)((float)mapSize.Y / 1f) + 1;
			Property property = map.properties.FirstOrDefault((Property p) => p.name == "tileset_id");
			if (property != null)
			{
				currentTilesetId = int.Parse(property.value, CultureInfo.InvariantCulture);
			}
			Tiled.TileSet[] tileset = map.tileset;
			foreach (Tiled.TileSet tileSet in tileset)
			{
				tilesets.Add(new TileSet(tileSet.firstgid, tileSet.source));
			}
			Layer[] ıtems = map.Items;
			foreach (object obj in ıtems)
			{
				if (obj.GetType() == typeof(Tiled.TileLayer))
				{
					Tiled.TileLayer layer = (Tiled.TileLayer)obj;
					TileLayer item = new TileLayer(mapSize, layer);
					list.Add(item);
				}
				else if (obj.GetType() == typeof(ObjectGroup))
				{
					ObjectGroup objectGroup = (ObjectGroup)obj;
					if (objectGroup.name.ToLowerInvariant() == "poly_collision")
					{
						ReadCollisionObjectGroup(objectGroup);
					}
				}
			}
			string panorama_name = tilesetInfos[currentTilesetId].panorama_name;
			SetPanorama(panorama_name);
			counterTiles = new bool[mapSize.X * mapSize.Y];
			for (int j = 0; j < mapSize.Y; j++)
			{
				for (int k = 0; k < mapSize.X; k++)
				{
					bool flag = false;
					int num = 1;
					foreach (TileLayer item2 in list)
					{
						short rPGMakerTileId = item2.GetRPGMakerTileId(k, j, tilesets);
						if ((tilesetInfos[currentTilesetId].passages[rPGMakerTileId] & 0x80) == 128)
						{
							flag = true;
							break;
						}
						num++;
					}
					counterTiles[k + j * mapSize.X] = flag;
				}
			}
			list.Reverse();
			collisionTiles = new bool[mapSize.X * mapSize.Y];
			for (int l = 0; l < mapSize.Y; l++)
			{
				for (int m = 0; m < mapSize.X; m++)
				{
					bool flag2 = false;
					int num2 = 1;
					foreach (TileLayer item3 in list)
					{
						short rPGMakerTileId2 = item3.GetRPGMakerTileId(m, l, tilesets);
						if (rPGMakerTileId2 >= 48 || num2 >= 3)
						{
							if ((tilesetInfos[currentTilesetId].passages[rPGMakerTileId2] & 0xF) == 15)
							{
								flag2 = true;
								break;
							}
							if (tilesetInfos[currentTilesetId].priorities[rPGMakerTileId2] == 0)
							{
								flag2 = false;
								break;
							}
						}
						num2++;
					}
					collisionTiles[m + l * mapSize.X] = flag2;
				}
			}
			list.Reverse();
			terrainTiles = new byte[mapSize.X * mapSize.Y];
			for (int n = 0; n < mapSize.Y; n++)
			{
				for (int num3 = 0; num3 < mapSize.X; num3++)
				{
					byte b = 0;
					foreach (TileLayer item4 in list)
					{
						short rPGMakerTileId3 = item4.GetRPGMakerTileId(num3, n, tilesets);
						if (tilesetInfos[currentTilesetId].terrain_tags[rPGMakerTileId3] != 0)
						{
							b = tilesetInfos[currentTilesetId].terrain_tags[rPGMakerTileId3];
						}
					}
					terrainTiles[num3 + n * mapSize.X] = b;
				}
			}
			TilesetInfoJson tilesetInfoJson = tilesetInfos[currentTilesetId];
			heightsToLayers.Add(0, list);
			foreach (TileLayer item5 in list)
			{
				for (int num4 = 0; num4 < mapSize.Y; num4++)
				{
					for (int num5 = 0; num5 < mapSize.X; num5++)
					{
						short rPGMakerTileId4 = item5.GetRPGMakerTileId(num5, num4, tilesets);
						int num6 = tilesetInfoJson.priorities[rPGMakerTileId4];
						if (num6 == 0)
						{
							continue;
						}
						if (!heightsToLayers.TryGetValue(num6, out var value))
						{
							value = new List<TileLayer>();
							heightsToLayers.Add(num6, value);
						}
						TileLayer tileLayer = null;
						foreach (TileLayer item6 in value)
						{
							if (item6.GetTileId(num5, num4) == 0)
							{
								tileLayer = item6;
								break;
							}
						}
						if (tileLayer == null)
						{
							tileLayer = new TileLayer(mapSize);
							value.Add(tileLayer);
						}
						tileLayer.SetTileId(num5, num4, item5.GetTileId(num5, num4));
						item5.SetTileId(num5, num4, 0);
					}
				}
			}
			foreach (MapLine mapLine in mapLines)
			{
				mapLine.hashIntoTiles(hashedMapLines, tileSize, mapSize);
			}
			MapEvents mapEvents = JsonConvert.DeserializeObject<MapEvents>(File.ReadAllText(Game1.GameDataPath() + "/maps/events_" + currentMapName + ".json"));
			if (mapEvents.events.Length != 0)
			{
				entityIDcounter = mapEvents.events.Select((Event e) => e.id).Max();
			}
			else
			{
				entityIDcounter = 10;
			}
			Event[] events = mapEvents.events;
			foreach (Event @event in events)
			{
				Entity e2;
				switch (@event.name)
				{
				case "!collectible":
					e2 = new Collectible(oneshotWindow, @event);
					break;
				case "@text":
					e2 = new CreditsTextEntity(oneshotWindow, @event, GraphicsManager.FontType.Game, 1);
					break;
				case "@text_small":
					e2 = new CreditsTextEntity(oneshotWindow, @event, GraphicsManager.FontType.GameSmall, 1);
					break;
				default:
					e2 = new Entity(oneshotWindow, @event);
					break;
				}
				AddEntity(e2);
			}
			MapMusic mapMusic = JsonConvert.DeserializeObject<MapMusic>(File.ReadAllText(Game1.GameDataPath() + "/maps/music_" + currentMapName + ".json"));
			if (mapMusic.autoplay_bgm)
			{
				Game1.soundMan.QueueSong(mapMusic.bgm.name, 0.25f, (float)mapMusic.bgm.volume / 100f, (float)mapMusic.bgm.pitch / 100f);
			}
			Game1.languageMan.LoadMapLocFile(currentMapId);
			if (player == null)
			{
				player = new Player(oneshotWindow, 999999, (playerTileX + 0.5f) * (float)tileSize.X, (playerTileY + 0.5f) * (float)tileSize.Y);
			}
			else
			{
				player.MapChanged(999999, (playerTileX + 0.5f) * (float)tileSize.X, (playerTileY + 0.5f) * (float)tileSize.Y);
			}
			camFocusEntity = player;
			AddEntity(player);
			foreach (KeyValuePair<int, EventRunner> parallelEventRunner in parallelEventRunners)
			{
				if (parallelEventRunner.Key < 10000)
				{
					parallelEventRunner.Value.KeepAlive = false;
				}
			}
			oneshotWindow.followerMan.SpawnAllFollowers();
			foreach (Entity entity in entities)
			{
				if (entity.eventTrigger == Entity.EventTrigger.ParallelProcess)
				{
					entity.Update();
				}
			}
		}

		public void KeepEntityOnMap(Entity e)
		{
			Vec2 pos = e.GetPos();
			Vec2 vel = e.GetVel();
			if (pos.X < 0)
			{
				pos.X = 0;
				vel.X = 0;
			}
			else if (pos.X > mapSize.X * tileSize.X * 256)
			{
				pos.X = mapSize.X * tileSize.X * 256;
				vel.X = 0;
			}
			if (pos.Y < 0)
			{
				pos.Y = 0;
				vel.Y = 0;
			}
			else if (pos.Y > mapSize.Y * tileSize.Y * 256)
			{
				pos.Y = mapSize.Y * tileSize.Y * 256;
				vel.Y = 0;
			}
			e.SetPos(pos);
			e.SetVel(vel);
		}

		public int GetMapID()
		{
			return currentMapId;
		}

		public Entity GetEntityByID(int entityId)
		{
			if (entitiesById.TryGetValue(entityId, out var value))
			{
				return value;
			}
			return null;
		}

		public Player GetPlayer()
		{
			return player;
		}

		public Entity GetNPCTriggeringEvent()
		{
			if (mainEventRunner != null && mainEventRunner.TriggeringEntity != null)
			{
				return mainEventRunner.TriggeringEntity;
			}
			return null;
		}

		private void ReadCollisionObjectGroup(ObjectGroup group)
		{
			Tiled.Object[] @object = group.@object;
			foreach (Tiled.Object obj in @object)
			{
				ReadCollisionObject(obj);
			}
		}

		private void ReadCollisionObject(Tiled.Object obj)
		{
			float num = (float)obj.x;
			float num2 = (float)obj.y;
			if (obj.polygon == null)
			{
				return;
			}
			string[] array = obj.polygon.points.Split(' ');
			Vec2[] array2 = new Vec2[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string[] array3 = array[i].Split(',');
				float num3 = float.Parse(array3[0], CultureInfo.InvariantCulture) + num;
				float num4 = float.Parse(array3[1], CultureInfo.InvariantCulture) + num2;
				array2[i].X = (int)num3 * 256;
				array2[i].Y = (int)num4 * 256;
			}
			for (int j = 0; j < array2.Length; j++)
			{
				int num5 = j + 1;
				if (num5 >= array2.Length)
				{
					num5 = 0;
				}
				mapLines.Add(new MapLine(array2[j], array2[num5], mapLines.Count));
			}
		}

		public int GetNextEntityID()
		{
			entityIDcounter++;
			return entityIDcounter;
		}

		public bool CheckIfStartScript(Entity e, bool investigateButtonPressed, out Entity interactableEntity)
		{
			interactableEntity = null;
			Vec2 pos = e.GetPos();
			Rect collisionRect = e.GetCollisionRect();
			Rect buttonPressRect = e.GetButtonPressRect();
			Rect otherCollision = new Rect(pos.X - 1, pos.Y - 1, 2, 2);
			entitiesToCheck.Clear();
			Vec2 zero = Vec2.Zero;
			zero.X = buttonPressRect.X + buttonPressRect.W / 2;
			zero.Y = buttonPressRect.Y + buttonPressRect.H / 2;
			Rect otherCollision2 = new Rect(pos.X + collisionRect.X * 256, pos.Y + collisionRect.Y * 256, collisionRect.W * 256, collisionRect.H * 256);
			Rect entityHashRect = GetEntityHashRect(e);
			for (int i = entityHashRect.X; i <= entityHashRect.X + entityHashRect.W; i++)
			{
				for (int j = entityHashRect.Y; j <= entityHashRect.Y + entityHashRect.H; j++)
				{
					int key = j * hashMapSize.X + i;
					if (!hashedEntities.TryGetValue(key, out var value))
					{
						continue;
					}
					foreach (Entity item in value)
					{
						if (!entitiesToCheck.TryGetValue(item.GetID(), out var _))
						{
							entitiesToCheck.Add(item.GetID(), item);
						}
					}
				}
			}
			int num = int.MaxValue;
			foreach (Entity value3 in entitiesToCheck.Values)
			{
				bool flag = false;
				if (value3.Active() && value3.HasScript())
				{
					if (investigateButtonPressed && value3.StartScriptOnPlayerAction())
					{
						if (value3.TouchingRect(buttonPressRect))
						{
							flag = true;
						}
						if (value3.IgnoreNpcCollision() && value3.TouchingRect(otherCollision))
						{
							flag = true;
						}
					}
					if (value3.StartScriptOnPlayerTouch())
					{
						if (!value3.IgnoreNpcCollision() && value3.TouchingRect(buttonPressRect) && !value3.isTouchingPlayerAndTriggeredEvent)
						{
							flag = true;
						}
						if (value3.IgnoreNpcCollision() && value3.TouchingRect(otherCollision) && !value3.isTouchingPlayerAndTriggeredEvent)
						{
							flag = true;
						}
					}
					if (value3.StartScriptOnEventTouch() && value3.TouchingRect(otherCollision2) && !value3.isTouchingPlayerAndTriggeredEvent)
					{
						flag = true;
					}
				}
				if (flag)
				{
					Vec2 pos2 = value3.GetPos();
					int num2 = Math.Abs(pos2.X - zero.X) + Math.Abs(pos2.Y - zero.Y);
					if (interactableEntity == null)
					{
						interactableEntity = value3;
						num = num2;
					}
					else if (num2 < num)
					{
						interactableEntity = value3;
						num = num2;
					}
				}
			}
			if (interactableEntity != null && (interactableEntity.StartScriptOnPlayerTouch() || interactableEntity.StartScriptOnEventTouch()))
			{
				interactableEntity.isTouchingPlayerAndTriggeredEvent = true;
			}
			return interactableEntity != null;
		}

		private bool IsEntityMovingTowardsOtherEntity(Entity e, Entity otherEntity)
		{
			Vec2 pos = e.GetPos();
			Vec2 vel = e.GetVel();
			Vec2 pos2 = otherEntity.GetPos();
			if (Math.Abs(pos2.X - pos.X) > Math.Abs(pos2.Y - pos.Y))
			{
				if ((pos.X < pos2.X && vel.X > 0) || (pos.X > pos2.X && vel.X < 0))
				{
					return true;
				}
			}
			else if ((pos.Y < pos2.Y && vel.Y > 0) || (pos.Y > pos2.Y && vel.Y < 0))
			{
				return true;
			}
			return false;
		}

		public void StartEvent(Entity triggeringEntity, bool parallelProcess = false)
		{
			if (parallelProcess)
			{
				if (!parallelEventRunners.TryGetValue(triggeringEntity.GetID(), out var value))
				{
					value = new EventRunner(oneshotWindow, triggeringEntity.list, triggeringEntity.GetID(), triggeringEntity.GetCurrentPageNumber(), triggeringEntity);
					parallelEventRunners.Add(triggeringEntity.GetID(), value);
				}
				value.KeepAlive = true;
			}
			else
			{
				if (mainEventRunner != null || IsMapTransitioning())
				{
					return;
				}
				mainEventRunner = new EventRunner(oneshotWindow, triggeringEntity.list, triggeringEntity.GetID(), triggeringEntity.GetCurrentPageNumber(), triggeringEntity);
				mainEventRunner.Update();
				if (!mainEventRunner.IsFinished())
				{
					return;
				}
				mainEventRunner = null;
				if (oneshotWindow.varMan.GetVariable(4) == 0)
				{
					return;
				}
				foreach (Entity entity in entities)
				{
					if (entity.HandlesMapEventVar)
					{
						entity.Update();
						break;
					}
				}
			}
		}

		public void StartCommonEvent(int commonEventID, bool parallelProcess = false)
		{
			if (!commonEvents.TryGetValue(commonEventID, out var value))
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"No common event exists with id {commonEventID}");
			}
			else if (parallelProcess)
			{
				int key = 10000 + commonEventID;
				if (!parallelEventRunners.TryGetValue(key, out var value2))
				{
					value2 = new EventRunner(oneshotWindow, value.list, value.id, -1);
					parallelEventRunners.Add(key, value2);
				}
				value2.KeepAlive = true;
			}
			else if (mainEventRunner == null)
			{
				mainEventRunner = new EventRunner(oneshotWindow, value.list, value.id, -1);
				queuedCommonEventId = -1;
			}
			else
			{
				queuedCommonEventId = commonEventID;
			}
		}

		public CommonEvent GetCommonEvent(int commonEventId)
		{
			if (!commonEvents.TryGetValue(commonEventId, out var value))
			{
				Game1.logMan.Log(LogManager.LogLevel.Error, $"No common event exists with id {commonEventId}");
				return null;
			}
			return value;
		}

		public void ScrollMap(int direction, int distance, int speed)
		{
			scrollDirection = (Entity.Direction)direction;
			scrollDistanceRemaining = distance * 16 * 256;
			scrollSpeed = speed;
		}

		public bool IsMapScrolling()
		{
			return scrollDistanceRemaining > 0;
		}

		public void ChangeMap(string mapName)
		{
			if (Regex.IsMatch(mapName, "map([0-9]+)"))
			{
				int mapId = int.Parse(Regex.Match(mapName, "map([0-9]+)").Groups[1].Value, CultureInfo.InvariantCulture);
				ChangeMap(mapId, 0f, 0f, 1f, Entity.Direction.None);
			}
		}

		public void ChangeMap(int mapId, float xTile, float yTile, float time, Entity.Direction direction)
		{
			Game1.gMan.clearTextureCache(TextureCache.CacheType.Game);
			Entity.Direction direction2 = player.GetDirection();
			changeMapTimer = 0;
			totalChangeMapTime = (int)(time * 60f);
			mapTransitionBlendMode = GraphicsManager.BlendMode.Normal;
			LoadMap(mapId, xTile, yTile);
			UpdateCamera();
			if (direction == Entity.Direction.None)
			{
				player.SetDirection(direction2);
			}
			else
			{
				player.SetDirection(direction);
			}
			Game1.gMan.SetUpMapTransitionFrame();
		}

		public bool IsChangingMap()
		{
			return changeMapTimer < totalChangeMapTime;
		}

		public void DrawTile(int tileId, Vec2 drawPos, float alpha, GameTone tone)
		{
			TileSet tileSet = tilesets[tilesets.Count - 1];
			tileSet.DrawTile(drawPos, tileId + tileSet.GetFirstTileID(), tone, alpha);
		}

		public void Draw()
		{
			float num = (float)bulbLightTimer / 120f;
			GameTone tone = ambientTone * (1f - num) + screenTone;
			Vec2 zero = Vec2.Zero;
			int num2 = camPos.X / tileSize.X;
			int num3 = camPos.Y / tileSize.Y;
			int num4 = (camPos.X + 320) / tileSize.X + 1;
			int num5 = (camPos.Y + 240) / tileSize.Y + 1;
			if (!Wrapping)
			{
				if (num2 < 0)
				{
					num2 = 0;
				}
				if (num3 < 0)
				{
					num3 = 0;
				}
				if (num4 >= mapSize.X)
				{
					num4 = mapSize.X - 1;
				}
				if (num5 >= mapSize.Y)
				{
					num5 = mapSize.Y - 1;
				}
			}
			else
			{
				if (num2 <= 0)
				{
					num2--;
				}
				if (num3 <= 0)
				{
					num3--;
				}
			}
			int x = (camPos.X - num2 * tileSize.X) * -1;
			int y = (camPos.Y - num3 * tileSize.Y) * -1;
			zero.X = x;
			zero.Y = y;
			LinkedListNode<Entity> linkedListNode = entitiesByHeight.First;
			mapPanorama?.Draw(camPos, player.GetPixelPos(), mapSize, tileSize, tone);
			List<TileLayer> value;
			if (!string.IsNullOrEmpty(background))
			{
				Vec2 pixelPos = new Vec2(-camPos.X, -camPos.Y);
				Game1.gMan.MainBlit("panoramas/" + background, pixelPos, 1f, 0, GraphicsManager.BlendMode.Normal, 2, tone);
			}
			else if (heightsToLayers.TryGetValue(0, out value))
			{
				foreach (TileLayer item in value)
				{
					item.Draw(tilesets, camPos, tileSize, Wrapping, tone);
				}
			}
			for (int i = num3; i <= num5 + 6; i++)
			{
				int num6 = i;
				if (num6 < 0)
				{
					num6 += mapSize.Y;
				}
				else if (num6 >= mapSize.Y)
				{
					num6 -= mapSize.Y;
				}
				if (string.IsNullOrEmpty(background))
				{
					for (int j = 1; j < 6 && j <= i - num3; j++)
					{
						if (i - j > num5)
						{
							continue;
						}
						Vec2 drawPos = zero;
						drawPos.Y -= j * tileSize.Y;
						if (!heightsToLayers.TryGetValue(j, out var value2))
						{
							continue;
						}
						foreach (TileLayer item2 in value2)
						{
							for (int k = num2; k <= num4; k++)
							{
								int num7 = k;
								if (num7 < 0)
								{
									num7 += mapSize.X;
								}
								else if (num7 >= mapSize.X)
								{
									num7 -= mapSize.X;
								}
								item2.DrawTile(drawPos, num7, num6, tilesets, j, tone);
								drawPos.X += tileSize.X;
							}
							drawPos.X = x;
						}
					}
				}
				zero.Y += tileSize.Y;
				while (linkedListNode != null && linkedListNode.Value.GetPixelBottom() <= (i + 1) * tileSize.Y)
				{
					DrawEntity(linkedListNode.Value, tone);
					linkedListNode = linkedListNode.Next;
				}
			}
			while (linkedListNode != null)
			{
				DrawEntity(linkedListNode.Value, tone);
				linkedListNode = linkedListNode.Next;
			}
			mapFog?.Draw(camPos, tone);
			if (particles != null)
			{
				foreach (FireflyParticle particle in particles)
				{
					particle.Draw();
				}
			}
			Vec2 vec = Game1.gMan.TextureSize("lightmaps/bulb");
			Game1.gMan.MainBlit("lightmaps/bulb", new Vec2((320 - vec.X) / 2 - (int)(shakeOffset / 2f), 0), num);
			if (!debugDrawCollision)
			{
				return;
			}
			zero.Y = y;
			for (int l = num3; l <= num5; l++)
			{
				zero.X = x;
				for (int m = num2; m <= num4; m++)
				{
					if (IsTileSolid(m, l))
					{
						Game1.gMan.MainBlit("collision_tile", zero, 0.25f);
					}
					zero.X += tileSize.X;
				}
				zero.Y += tileSize.Y;
			}
			foreach (MapLine mapLine in mapLines)
			{
				mapLine.Draw(camPos);
			}
			foreach (Entity entity in entities)
			{
				entity.DrawCollision(camPos);
			}
			Game1.gMan.TextBlit(GraphicsManager.FontType.Game, new Vec2(1, 1), $"Map {currentMapId}", GameColor.Black);
			Game1.gMan.TextBlit(GraphicsManager.FontType.Game, Vec2.Zero, $"Map {currentMapId}", GameColor.White);
		}

		private void DrawEntity(Entity e, GameTone tone)
		{
			e.Draw(camPos, tone);
			if (Wrapping)
			{
				Vec2 vec = camPos;
				vec.X -= mapSize.X * tileSize.X;
				e.Draw(vec, tone);
				Vec2 vec2 = camPos;
				vec2.X += mapSize.X * tileSize.X;
				e.Draw(vec2, tone);
			}
		}

		public void SetAmbientTone(GameTone newTone)
		{
			ambientTone = newTone;
		}

		public void SetScreenTone(GameTone newTone, int shiftTime)
		{
			screenToneShiftTimer = 0;
			if (shiftTime <= 0)
			{
				screenToneShiftTotalTime = 0;
				screenTone = newTone;
			}
			else
			{
				startScreenTone = screenTone;
				targetScreenTone = newTone;
				screenToneShiftTotalTime = shiftTime;
			}
		}

		public GameTone GetAmbientTone()
		{
			return ambientTone;
		}

		public void DrawEventRunners()
		{
			if (mainEventRunner != null)
			{
				mainEventRunner.Draw();
			}
			foreach (KeyValuePair<int, EventRunner> parallelEventRunner in parallelEventRunners)
			{
				parallelEventRunner.Value.Draw();
			}
		}

		public void DrawMapTransition()
		{
			if (totalChangeMapTime > 0)
			{
				float alpha = (float)(totalChangeMapTime - changeMapTimer) / (float)totalChangeMapTime;
				Game1.gMan.DrawMapTransitionFrame(alpha, mapTransitionBlendMode);
			}
		}

		private void UpdateCamera()
		{
			oldCamPos = camPos;
			if (oneshotWindow.flagMan.IsFlagSet(100))
			{
				return;
			}
			Vec2 pixelPos = camFocusEntity.GetPixelPos();
			camPos.X = pixelPos.X - 160;
			camPos.Y = pixelPos.Y - 120;
			camPos.X += scrollOffset.X / 256;
			camPos.Y += scrollOffset.Y / 256;
			camPos.X += (int)Math.Round(shakeOffset) / 2;
			if (oneshotWindow.flagMan.IsFlagSet(98))
			{
				if (camPos.X < 0)
				{
					camPos.X = 0;
				}
				if (camPos.Y < 0)
				{
					camPos.Y = 0;
				}
				Vec2 maxCamPos = GetMaxCamPos();
				if (camPos.X > maxCamPos.X)
				{
					camPos.X = maxCamPos.X;
				}
				if (camPos.Y > maxCamPos.Y)
				{
					camPos.Y = maxCamPos.Y;
				}
			}
		}

		public Vec2 GetCamPos()
		{
			return camPos;
		}

		public Vec2 GetMaxCamPos()
		{
			Vec2 result = new Vec2(mapSize.X * tileSize.X - 320, mapSize.Y * tileSize.Y - 240);
			if (result.X < 0)
			{
				result.X = 0;
			}
			if (result.Y < 0)
			{
				result.Y = 0;
			}
			return result;
		}

		public void Update()
		{
			wasMapChangedDuringUpdate = false;
			if (Game1.inputMan.IsButtonPressed(InputManager.Button.DEBUG_COLLISION))
			{
				debugDrawCollision = !debugDrawCollision;
			}
			if (particles != null)
			{
				Vec2 camDelta = new Vec2(camPos.X - oldCamPos.X, camPos.Y - oldCamPos.Y);
				foreach (FireflyParticle particle in particles)
				{
					particle.Update(camDelta);
				}
			}
			if (shakeDuration > 0 || shakeOffset != 0f)
			{
				float num = (float)(shakePower * shakeSpeed * shakeDirection) / 10f;
				if (shakeDuration <= 0 && Math.Abs(shakeOffset) < Math.Abs(num))
				{
					shakeOffset = 0f;
				}
				else
				{
					shakeOffset += num;
				}
				if (shakeOffset > (float)(shakePower * 2))
				{
					shakeDirection = -1;
				}
				else if (shakeOffset < (float)(-shakePower * 2))
				{
					shakeDirection = 1;
				}
				if (shakeDuration > 0)
				{
					shakeDuration--;
				}
				else
				{
					shakeDuration = 0;
				}
			}
			if (oneshotWindow.menuMan.ItemMan.HasItem(1))
			{
				bulbLightTimer++;
				if (bulbLightTimer > 120)
				{
					bulbLightTimer = 120;
				}
			}
			else
			{
				bulbLightTimer--;
				if (bulbLightTimer < 0)
				{
					bulbLightTimer = 0;
				}
			}
			if (screenToneShiftTotalTime > 0)
			{
				screenToneShiftTimer++;
				if (screenToneShiftTimer >= screenToneShiftTotalTime)
				{
					screenToneShiftTimer = 0;
					screenToneShiftTotalTime = 0;
					screenTone = targetScreenTone;
				}
				else
				{
					float num2 = (float)screenToneShiftTimer / (float)screenToneShiftTotalTime;
					screenTone = startScreenTone * (1f - num2) + targetScreenTone * num2;
				}
			}
			if (scrollDistanceRemaining > 0)
			{
				int num3 = 16 * (2 << scrollSpeed);
				switch (scrollDirection)
				{
				case Entity.Direction.Up:
					scrollOffset.Y -= num3;
					break;
				case Entity.Direction.Down:
					scrollOffset.Y += num3;
					break;
				case Entity.Direction.Left:
					scrollOffset.X -= num3;
					break;
				case Entity.Direction.Right:
					scrollOffset.X += num3;
					break;
				}
				scrollDistanceRemaining -= num3;
			}
			if (totalChangeMapTime > 0)
			{
				changeMapTimer++;
				if (changeMapTimer >= totalChangeMapTime)
				{
					changeMapTimer = 0;
					totalChangeMapTime = 0;
				}
			}
			HashEntities();
			foreach (Entity entity in entities)
			{
				entity.Update();
				if (wasMapChangedDuringUpdate)
				{
					break;
				}
				if (Wrapping)
				{
					Vec2 pos = entity.GetPos();
					int num4 = mapSize.X * tileSize.X * 256;
					int num5 = mapSize.Y * tileSize.Y * 256;
					if (pos.X < 0)
					{
						pos.X += num4;
					}
					else if (pos.X > num4)
					{
						pos.X -= num4;
					}
					if (pos.Y < 0)
					{
						pos.Y += num5;
					}
					else if (pos.Y > num5)
					{
						pos.Y -= num5;
					}
					entity.SetPos(pos);
				}
				if (entity.Active())
				{
					if (!entity.IgnoreMapCollision())
					{
						MapCollision(entity);
					}
					if (!entity.IgnoreNpcCollision())
					{
						HashedEntityCollision(entity);
					}
				}
				UpdateEntityHeight(entity);
				if (entity.KillEntityAfterUpdate)
				{
					entitiesToKill.Add(entity);
				}
			}
			foreach (Entity item in entitiesToKill)
			{
				RemoveEntity(item);
			}
			entitiesToKill.Clear();
			foreach (Entity item2 in entitiesToSpawn)
			{
				AddEntity(item2);
			}
			entitiesToSpawn.Clear();
			UpdateCamera();
			UpdateCommonEvents();
			foreach (TileSet tileset in tilesets)
			{
				tileset.Update();
			}
			mapPanorama?.Update();
			mapFog?.Update();
		}

		public void EventRunnerUpdate()
		{
			if (mainEventRunner != null)
			{
				mainEventRunner.Update();
				if (mainEventRunner.IsFinished())
				{
					mainEventRunner = null;
				}
			}
			else if (queuedCommonEventId > 0)
			{
				StartCommonEvent(queuedCommonEventId);
			}
			foreach (KeyValuePair<int, EventRunner> parallelEventRunner in parallelEventRunners)
			{
				EventRunner value = parallelEventRunner.Value;
				if (value.KeepAlive)
				{
					value.Update();
					value.KeepAlive = false;
					if (value.IsFinished())
					{
						parallelEventsToKill.Add(parallelEventRunner.Key);
					}
				}
				else
				{
					parallelEventsToKill.Add(parallelEventRunner.Key);
				}
			}
			foreach (int item in parallelEventsToKill)
			{
				parallelEventRunners.Remove(item);
			}
			parallelEventsToKill.Clear();
		}

		public void SetPanorama(string panoramaName)
		{
			if (!string.IsNullOrEmpty(panoramaName))
			{
				string text = "panoramas/" + panoramaName;
				Vec2 size = Game1.gMan.TextureSize(text);
				mapPanorama = new Panorama(text, size);
			}
		}

		public void SetFog(string fogName, int fogHue, float fogOpacity, GraphicsManager.BlendMode fogBlendMode, int fogScrollX, int fogScrollY)
		{
			if (!string.IsNullOrEmpty(fogName))
			{
				string text = "fogs/" + fogName;
				Vec2 fogSize = Game1.gMan.TextureSize(text);
				mapFog = new Fog(text, fogSize, fogHue, fogOpacity, fogBlendMode, fogScrollX, fogScrollY);
			}
		}

		private void UpdateCommonEvents()
		{
			foreach (CommonEvent value in commonEvents.Values)
			{
				switch (value.trigger)
				{
				case 1:
					if (!IsInScript() && oneshotWindow.flagMan.IsFlagSet(value.switch_id))
					{
						StartCommonEvent(value.id);
					}
					break;
				case 2:
					if (oneshotWindow.flagMan.IsFlagSet(value.switch_id))
					{
						StartCommonEvent(value.id, parallelProcess: true);
					}
					break;
				}
			}
		}

		public bool IsInScript()
		{
			return mainEventRunner != null;
		}

		public bool IsMapTransitioning()
		{
			return changeMapTimer > 0;
		}

		private Vec2 GetEntityHashPos(Entity e)
		{
			Vec2 pos = e.GetPos();
			return new Vec2(pos.X / 256 / hashTileSize.X, pos.Y / 256 / hashTileSize.Y);
		}

		private Rect GetEntityHashRect(Entity e)
		{
			return GetEntityHashRect(e, e.GetPos());
		}

		private Rect GetEntityOldHashRect(Entity e)
		{
			return GetEntityHashRect(e, e.GetOldPos());
		}

		private Rect GetEntityHashRect(Entity e, Vec2 ePos)
		{
			Rect collisionRect = e.GetCollisionRect();
			int num = ((ePos.X + collisionRect.X * 256) / 256 - hashTileSize.X / 2) / hashTileSize.X;
			int num2 = ((ePos.Y + collisionRect.Y * 256) / 256 - hashTileSize.Y / 2) / hashTileSize.Y;
			int num3 = ((ePos.X + (collisionRect.X + collisionRect.W) * 256) / 256 + hashTileSize.X / 2) / hashTileSize.X;
			int num4 = ((ePos.Y + (collisionRect.Y + collisionRect.H) * 256) / 256 + hashTileSize.Y / 2) / hashTileSize.Y;
			if (num < 0)
			{
				num = 0;
			}
			if (num2 < 0)
			{
				num2 = 0;
			}
			if (num3 < 0)
			{
				num3 = 0;
			}
			if (num4 < 0)
			{
				num4 = 0;
			}
			if (num >= hashMapSize.X)
			{
				num = hashMapSize.X;
			}
			if (num2 >= hashMapSize.Y)
			{
				num2 = hashMapSize.Y;
			}
			if (num3 >= hashMapSize.X)
			{
				num3 = hashMapSize.X;
			}
			if (num4 >= hashMapSize.Y)
			{
				num4 = hashMapSize.Y;
			}
			return new Rect(num, num2, num3 - num, num4 - num2);
		}

		private void RemoveEntityFromHash(Entity e)
		{
			Rect entityOldHashRect = GetEntityOldHashRect(e);
			for (int i = entityOldHashRect.X; i <= entityOldHashRect.X + entityOldHashRect.W; i++)
			{
				for (int j = entityOldHashRect.Y; j <= entityOldHashRect.Y + entityOldHashRect.H; j++)
				{
					int key = j * hashMapSize.X + i;
					if (hashedEntities.TryGetValue(key, out var value))
					{
						value.Remove(e);
					}
				}
			}
		}

		private void HashEntities()
		{
			foreach (Entity entity in entities)
			{
				if (entity.NeverHash || (!entity.HasMoved() && entity.HasBeenHashed))
				{
					continue;
				}
				Rect entityOldHashRect = GetEntityOldHashRect(entity);
				Rect entityHashRect = GetEntityHashRect(entity);
				if (entityOldHashRect.Equals(entityHashRect) && entity.HasBeenHashed)
				{
					continue;
				}
				RemoveEntityFromHash(entity);
				for (int i = entityHashRect.X; i <= entityHashRect.X + entityHashRect.W; i++)
				{
					for (int j = entityHashRect.Y; j <= entityHashRect.Y + entityHashRect.H; j++)
					{
						int key = j * hashMapSize.X + i;
						if (!hashedEntities.TryGetValue(key, out var value))
						{
							value = new List<Entity>();
							hashedEntities.Add(key, value);
						}
						if (!value.Contains(entity))
						{
							value.Add(entity);
						}
					}
				}
				entity.HasBeenHashed = true;
			}
		}

		public bool AreAnyEntitiesInForcedRoutes()
		{
			if (player.IsInForcedRoute())
			{
				return true;
			}
			foreach (Entity entity in entities)
			{
				if (entity.IsInForcedRoute())
				{
					return true;
				}
			}
			return false;
		}

		public void HashedEntityCollision(Entity e, Entity ignoredEntity = null, bool isTestCollisionEntity = false)
		{
			Vec2 entityHashPos = GetEntityHashPos(e);
			entitiesToCollideWith.Clear();
			Vec2 pos = e.GetPos();
			int key = entityHashPos.Y * hashMapSize.X + entityHashPos.X;
			if (hashedEntities.TryGetValue(key, out var value))
			{
				foreach (Entity item2 in value)
				{
					if (item2 != e && item2 != ignoredEntity && !item2.IgnoreNpcCollision() && item2.Active())
					{
						Vec2 pos2 = item2.GetPos();
						int distance = Math.Abs(pos.X - pos2.X) + Math.Abs(pos.Y - pos2.Y);
						EntityWithDistance item = new EntityWithDistance
						{
							e = item2,
							distance = distance
						};
						entitiesToCollideWith.Add(item);
					}
				}
			}
			entitiesToCollideWith.Sort((EntityWithDistance e1, EntityWithDistance e2) => e1.distance - e2.distance);
			foreach (EntityWithDistance item3 in entitiesToCollideWith)
			{
				if (item3.e is Player player && e.GetVel().X == 0 && e.GetVel().Y == 0 && !isTestCollisionEntity)
				{
					player.EntityCollision(e);
				}
				else
				{
					e.EntityCollision(item3.e);
				}
			}
		}

		public byte GetTilePriority(int tileid)
		{
			return tilesetInfos[currentTilesetId].priorities[tileid];
		}

		public bool IsTileSolid(int tileid)
		{
			return (tilesetInfos[currentTilesetId].passages[tileid] & 0xF) == 15;
		}

		public void OverrideStepSounds(List<List<string>> stepSounds)
		{
			overrideStepSounds = stepSounds;
		}

		public void SetBackground(string bg)
		{
			background = bg;
		}

		public Tuple<List<string>, float> GetStepSounds(Vec2 mapPos)
		{
			int num = mapPos.X / tileSize.X;
			int num2 = mapPos.Y / tileSize.Y;
			if (num < 0 || num >= mapSize.X || num2 < 0 || num2 >= mapSize.Y)
			{
				return emptyStepSounds;
			}
			byte b = terrainTiles[num + num2 * mapSize.X];
			if (b > 0)
			{
				if (overrideStepSounds != null)
				{
					return new Tuple<List<string>, float>(overrideStepSounds[b - 1], 1f);
				}
				return new Tuple<List<string>, float>(new List<string>(tilesetInfos[currentTilesetId].step_sounds[b - 1]), tilesetInfos[currentTilesetId].step_volumes[b - 1]);
			}
			return emptyStepSounds;
		}

		public bool IsTileSolid(int x, int y)
		{
			if (x < 0 || x >= mapSize.X || y < 0 || y >= mapSize.Y)
			{
				return false;
			}
			return collisionTiles[x + y * mapSize.X];
		}

		public void SetTileSolid(int x, int y)
		{
			if (x >= 0 && x < mapSize.X && y >= 0 && y < mapSize.Y)
			{
				collisionTiles[x + y * mapSize.X] = true;
			}
		}

		public bool IsTileCounter(int x, int y)
		{
			if (x < 0 || x >= mapSize.X || y < 0 || y >= mapSize.Y)
			{
				return false;
			}
			return counterTiles[x + y * mapSize.X];
		}

		public void MapCollision(Entity entity)
		{
			if (debugDrawCollision)
			{
				return;
			}
			tilesToCollideWith.Clear();
			linesToCollideWith.Clear();
			Vec2 entityPos = entity.GetPos();
			Rect collisionRect = entity.GetCollisionRect();
			Vec2 vec = new Vec2((entityPos.X / 256 + collisionRect.X) / tileSize.X, (entityPos.Y / 256 + collisionRect.Y) / tileSize.Y);
			Vec2 vec2 = new Vec2((entityPos.X / 256 + collisionRect.X + collisionRect.W) / tileSize.X, (entityPos.Y / 256 + collisionRect.Y + collisionRect.H) / tileSize.Y);
			if (vec.X < 0)
			{
				vec.X = 0;
			}
			else if (vec.X >= mapSize.X)
			{
				vec.X = mapSize.X - 1;
			}
			if (vec.Y < 0)
			{
				vec.Y = 0;
			}
			else if (vec.Y >= mapSize.Y)
			{
				vec.Y = mapSize.Y - 1;
			}
			if (vec2.X < 0)
			{
				vec2.X = 0;
			}
			else if (vec2.X >= mapSize.X)
			{
				vec2.X = mapSize.X - 1;
			}
			if (vec2.Y < 0)
			{
				vec2.Y = 0;
			}
			else if (vec2.Y >= mapSize.Y)
			{
				vec2.Y = mapSize.Y - 1;
			}
			for (int i = vec.X; i <= vec2.X; i++)
			{
				for (int j = vec.Y; j <= vec2.Y; j++)
				{
					if (hashedMapLines.TryGetValue(j * mapSize.X + i, out var value))
					{
						foreach (MapLine item in value)
						{
							if (!linesToCollideWith.TryGetValue(item.GetID(), out var _))
							{
								linesToCollideWith.Add(item.GetID(), item);
							}
						}
					}
					if (IsTileSolid(i, j))
					{
						tilesToCollideWith.Add(new Vec2(i * tileSize.X * 256 + tileSize.X * 256 / 2, j * tileSize.Y * 256 + tileSize.Y * 256 / 2));
					}
				}
			}
			tilesToCollideWith.Sort((Vec2 t1, Vec2 t2) => Math.Abs(entityPos.X - t1.X) + Math.Abs(entityPos.Y - t1.Y) - (Math.Abs(entityPos.X - t2.X) + Math.Abs(entityPos.Y - t2.Y)));
			foreach (Vec2 item2 in tilesToCollideWith)
			{
				Rect tileCollision = new Rect(item2.X - tileSize.X * 256 / 2, item2.Y - tileSize.Y * 256 / 2, tileSize.X * 256, tileSize.Y * 256);
				TileCollision(tileCollision, entity, tileCollision.X / (tileSize.X * 256), tileCollision.Y / (tileSize.Y * 256), isMapTile: true);
			}
			foreach (MapLine value3 in linesToCollideWith.Values)
			{
				value3.Collision(entity);
			}
		}

		public void TileCollision(Rect tileCollision, Entity entity, int tileX = 0, int tileY = 0, bool isMapTile = false)
		{
			Vec2 pos = entity.GetPos();
			Rect collisionRect = entity.GetCollisionRect();
			Rect rect = new Rect(pos.X + collisionRect.X * 256, pos.Y + collisionRect.Y * 256, collisionRect.W * 256, collisionRect.H * 256);
			if (rect.X >= tileCollision.X + tileCollision.W || rect.X + rect.W <= tileCollision.X || rect.Y >= tileCollision.Y + tileCollision.H || rect.Y + rect.H <= tileCollision.Y)
			{
				return;
			}
			Vec2 vec = new Vec2(tileCollision.X + tileCollision.W / 2, tileCollision.Y + tileCollision.H / 2);
			Vec2 vec2 = new Vec2(rect.X + rect.W / 2, rect.Y + rect.H / 2);
			Vec2 vel = entity.GetVel();
			Entity.Direction direction = Entity.Direction.None;
			int num = Math.Abs(vec2.X - vec.X);
			int num2 = Math.Abs(vec2.Y - vec.Y);
			if (tileCollision.W > tileCollision.H)
			{
				num2 += (tileCollision.W - tileCollision.H) / 2;
			}
			else
			{
				num += (tileCollision.H - tileCollision.W) / 2;
			}
			int num3 = 0;
			if (Math.Abs(Math.Abs(vel.X) - Math.Abs(vel.Y)) < 64)
			{
				num3 = 0;
			}
			if (num - num2 > num3)
			{
				direction = ((vec2.X >= vec.X) ? Entity.Direction.Left : Entity.Direction.Right);
				if (vel.Y == 0)
				{
					if (vec2.Y < tileCollision.Y)
					{
						pos.Y -= Math.Abs(vel.X / 2);
					}
					else if (vec2.Y > tileCollision.Y + tileCollision.H)
					{
						pos.Y += Math.Abs(vel.X / 2);
					}
				}
			}
			else if (num - num2 < -num3)
			{
				direction = ((vec2.Y >= vec.Y) ? Entity.Direction.Up : Entity.Direction.Down);
				if (vel.X == 0)
				{
					if (vec2.X < tileCollision.X)
					{
						pos.X -= Math.Abs(vel.Y / 2);
					}
					else if (vec2.X > tileCollision.X + tileCollision.W)
					{
						pos.X += Math.Abs(vel.Y / 2);
					}
				}
			}
			else if (vec2.X < vec.X)
			{
				if (vec2.Y < vec.Y)
				{
					bool flag = false;
					if (vel.X > 0 && vel.X > vel.Y)
					{
						if (isMapTile && tileY > 0)
						{
							flag = IsTileSolid(tileX, tileY - 1);
						}
						direction = (flag ? Entity.Direction.Right : Entity.Direction.Down);
					}
					else
					{
						if (isMapTile && tileX > 0)
						{
							flag = IsTileSolid(tileX - 1, tileY);
						}
						direction = (flag ? Entity.Direction.Down : Entity.Direction.Right);
					}
				}
				else
				{
					bool flag2 = false;
					if (vel.X > 0 && vel.X > -vel.Y)
					{
						if (isMapTile && tileY < mapSize.Y - 1)
						{
							flag2 = IsTileSolid(tileX, tileY + 1);
						}
						direction = (flag2 ? Entity.Direction.Right : Entity.Direction.Up);
					}
					else
					{
						if (isMapTile && tileX > 0)
						{
							flag2 = IsTileSolid(tileX - 1, tileY);
						}
						direction = (flag2 ? Entity.Direction.Up : Entity.Direction.Right);
					}
				}
			}
			else if (vec2.Y < vec.Y)
			{
				bool flag3 = false;
				if (vel.X < 0 && vel.X < -vel.Y)
				{
					if (isMapTile && tileY > 0)
					{
						flag3 = IsTileSolid(tileX, tileY - 1);
					}
					direction = (flag3 ? Entity.Direction.Left : Entity.Direction.Down);
				}
				else
				{
					if (isMapTile && tileX < mapSize.X - 1)
					{
						flag3 = IsTileSolid(tileX + 1, tileY);
					}
					direction = (flag3 ? Entity.Direction.Down : Entity.Direction.Left);
				}
			}
			else
			{
				bool flag4 = false;
				if (vel.X < 0 && vel.X < vel.Y)
				{
					if (isMapTile && tileY < mapSize.Y - 1)
					{
						flag4 = IsTileSolid(tileX, tileY + 1);
					}
					direction = (flag4 ? Entity.Direction.Left : Entity.Direction.Up);
				}
				else
				{
					if (isMapTile && tileX < mapSize.X - 1)
					{
						flag4 = IsTileSolid(tileX + 1, tileY);
					}
					direction = (flag4 ? Entity.Direction.Up : Entity.Direction.Left);
				}
			}
			switch (direction)
			{
			case Entity.Direction.Right:
				pos.X = tileCollision.X - rect.W - collisionRect.X * 256;
				break;
			case Entity.Direction.Left:
				pos.X = tileCollision.X + tileCollision.W - collisionRect.X * 256;
				break;
			case Entity.Direction.Down:
				pos.Y = tileCollision.Y - rect.H - collisionRect.Y * 256;
				break;
			case Entity.Direction.Up:
				pos.Y = tileCollision.Y + tileCollision.H - collisionRect.Y * 256;
				break;
			}
			entity.SetCollisionDirection(direction);
			entity.SetPos(pos);
		}

		public void SetCamPos(Vec2 newCam)
		{
			camPos = newCam;
		}

		public void EnableWrapping()
		{
			Wrapping = true;
		}

		public void PixelPuzzleReset()
		{
			foreach (Entity entity in entities)
			{
				if (entity.GetName().StartsWith("pixel "))
				{
					oneshotWindow.selfSwitchMan.UnsetSelfSwitch(entity.GetID(), "A");
				}
			}
		}

		public void MoveFootsplashesRelative(int x, int y)
		{
			foreach (Entity entity in entities)
			{
				if (entity is Footsplash)
				{
					Vec2 pos = entity.GetPos();
					pos.X += x * tileSize.X * 256;
					pos.Y += y * tileSize.Y * 256;
					entity.SetPos(pos);
				}
			}
		}

		public void MovePlayerRelative(int x, int y)
		{
			Vec2 pos = player.GetPos();
			pos.X += x * tileSize.X * 256;
			pos.Y += y * tileSize.Y * 256;
			player.SetPos(pos);
			UpdateCamera();
		}

		public bool RamPuzzleCheck()
		{
			foreach (Entity entity in entities)
			{
				if (entity.GetName().StartsWith("sokoram "))
				{
					Vec2 eTile = entity.GetCurrentTile();
					if (!correctRamPositions.Any((Vec2 pos) => pos.X == eTile.X && pos.Y == eTile.Y))
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool PixelPuzzleCheck(byte[] solution, Vec2 startPoint, Vec2 size)
		{
			foreach (Entity entity in entities)
			{
				if (!entity.GetName().StartsWith("pixel "))
				{
					continue;
				}
				Vec2 currentTile = entity.GetCurrentTile();
				Vec2 vec = new Vec2(currentTile.X - startPoint.X, currentTile.Y - startPoint.Y);
				if (vec.X >= 0 && vec.X < size.X && vec.Y >= 0 && vec.Y < size.Y)
				{
					int num = vec.X + vec.Y * size.X;
					if (solution[num] != 0 != oneshotWindow.selfSwitchMan.IsSelfSwitchSet(entity.GetID(), "A"))
					{
						return false;
					}
				}
			}
			return true;
		}

		public void ScreenShake(int power, int speed, int duration, bool vibeController)
		{
			shakePower = power;
			shakeSpeed = speed;
			shakeDuration = duration;
			if (vibeController)
			{
				float num = (float)Math.Sqrt(shakePower * shakeSpeed) / 5f;
				if (num > 1f)
				{
					num = 1f;
				}
				Game1.inputMan.VibrateController(num, shakeDuration);
			}
		}

		public void RemoveEntity(Entity e)
		{
			entities.Remove(e);
			if (entitiesById.TryGetValue(e.GetID(), out var _))
			{
				entitiesById.Remove(e.GetID());
			}
			entitiesByHeight.Remove(e);
			if (!e.NeverHash)
			{
				RemoveEntityFromHash(e);
			}
		}

		public void RemoveEntity(int id)
		{
			Entity entityByID = GetEntityByID(id);
			if (entityByID != null)
			{
				RemoveEntity(entityByID);
			}
		}

		public void EntityAddEntity(Entity e)
		{
			entitiesToSpawn.Add(e);
		}

		public int AddFollower(FollowerManager.FollowerType followerType, int entityToFollowId)
		{
			Entity entityByID = GetEntityByID(entityToFollowId);
			if (entityByID == null)
			{
				entityByID = player;
			}
			Follower follower = new Follower(followerType, entityByID, oneshotWindow);
			AddEntity(follower);
			return follower.GetID();
		}

		public void AddEntity(Entity e)
		{
			entities.Add(e);
			if (e.GetID() >= 0)
			{
				if (entitiesById.TryGetValue(e.GetID(), out var _))
				{
					Game1.logMan.Log(LogManager.LogLevel.Error, $"Tried to add entity with ID {e.GetID()}, but entity with that ID already exists!");
					return;
				}
				entitiesById.Add(e.GetID(), e);
			}
			if (entitiesByHeight.Count <= 0)
			{
				e.sortedNode = entitiesByHeight.AddFirst(e);
				return;
			}
			int pixelBottom = e.GetPixelBottom();
			LinkedListNode<Entity> linkedListNode = entitiesByHeight.First;
			while (linkedListNode.Value.GetPixelBottom() < pixelBottom)
			{
				linkedListNode = linkedListNode.Next;
				if (linkedListNode == null)
				{
					e.sortedNode = entitiesByHeight.AddLast(e);
					break;
				}
			}
			if (linkedListNode != null)
			{
				e.sortedNode = entitiesByHeight.AddAfter(linkedListNode, e);
			}
		}

		private void UpdateEntityHeight(Entity e)
		{
			int pixelBottom = e.GetPixelBottom();
			LinkedListNode<Entity> previous = e.sortedNode.Previous;
			while (previous != null && previous.Value.GetPixelBottom() > pixelBottom)
			{
				previous = previous.Previous;
			}
			if (previous != e.sortedNode.Previous)
			{
				entitiesByHeight.Remove(e.sortedNode);
				if (previous == null)
				{
					e.sortedNode = entitiesByHeight.AddFirst(e);
				}
				else
				{
					e.sortedNode = entitiesByHeight.AddAfter(previous, e);
				}
			}
			LinkedListNode<Entity> next = e.sortedNode.Next;
			while (next != null && (next.Value.GetPixelBottom() < pixelBottom || (e is Player && next.Value.GetPixelBottom() <= pixelBottom)))
			{
				next = next.Next;
			}
			if (next != e.sortedNode.Next)
			{
				entitiesByHeight.Remove(e.sortedNode);
				if (next == null)
				{
					e.sortedNode = entitiesByHeight.AddLast(e);
				}
				else
				{
					e.sortedNode = entitiesByHeight.AddBefore(next, e);
				}
			}
		}

		public float GetPlayerXTile()
		{
			return (float)player.GetPixelPos().X / (float)tileSize.X - 0.5f;
		}

		public float GetPlayerYTile()
		{
			return (float)player.GetPixelPos().Y / (float)tileSize.Y - 0.5f;
		}

		public Vec2 GetPlayerPos()
		{
			return player.GetPos();
		}

		public string GetMapFileName()
		{
			return currentMapName;
		}

		public void SpawnFireflyParticles()
		{
			particles = new List<FireflyParticle>();
			for (int i = 0; i < 30; i++)
			{
				particles.Add(new FireflyParticle());
			}
		}

		private List<EntitySaveData> GetEntitySaveDatas()
		{
			List<EntitySaveData> list = new List<EntitySaveData>();
			foreach (Entity value in entitiesById.Values)
			{
				if (value.GetID() >= 0)
				{
					list.Add(value.GetEntitySaveData());
				}
			}
			return list;
		}

		private void LoadEntitySaveDatas(List<EntitySaveData> entitySaveDatas, int? version)
		{
			Dictionary<int, EntitySaveData> dictionary = new Dictionary<int, EntitySaveData>();
			foreach (EntitySaveData entitySaveData in entitySaveDatas)
			{
				if (dictionary.ContainsKey(entitySaveData.id))
				{
					Game1.logMan.Log(LogManager.LogLevel.Warning, "Entity save datas has 2 entities with same id!");
				}
				else
				{
					dictionary.Add(entitySaveData.id, entitySaveData);
				}
			}
			List<Entity> list = new List<Entity>();
			foreach (Entity value in entitiesById.Values)
			{
				if (!dictionary.ContainsKey(value.GetID()))
				{
					if (!(value is Follower) && !(value is Player))
					{
						list.Add(value);
					}
				}
				else
				{
					value.LoadEntitySaveData(dictionary[value.GetID()], version);
				}
			}
			foreach (Entity item in list)
			{
				RemoveEntity(item);
			}
		}

		public TileMapSaveData GetSaveData()
		{
			return new TileMapSaveData
			{
				playerTileX = GetPlayerXTile(),
				playerTileY = GetPlayerYTile(),
				playerDirection = player.GetDirection(),
				playerSheet = player.GetNPCSheet(),
				currentMap = currentMapId,
				entitySaveDatas = GetEntitySaveDatas(),
				mapAmbientTone = ambientTone,
				mapBackground = background,
				hasFireflyParticles = (particles != null),
				mainEventRunnerData = ((mainEventRunner != null) ? mainEventRunner.GetEventRunnerData() : null),
				panoramaSaveData = ((mapPanorama != null) ? mapPanorama.GetSaveData() : null),
				overrideStepSounds = overrideStepSounds
			};
		}

		public void LoadSaveData(TileMapSaveData tileMapSaveData, int? version)
		{
			ChangeMap(tileMapSaveData.currentMap, tileMapSaveData.playerTileX, tileMapSaveData.playerTileY, 0f, tileMapSaveData.playerDirection);
			LoadEntitySaveDatas(tileMapSaveData.entitySaveDatas, version);
			player.SetNPCSheet(tileMapSaveData.playerSheet);
			ambientTone = tileMapSaveData.mapAmbientTone;
			background = tileMapSaveData.mapBackground;
			if (tileMapSaveData.panoramaSaveData != null)
			{
				mapPanorama = new Panorama(tileMapSaveData.panoramaSaveData.name, tileMapSaveData.panoramaSaveData.size);
			}
			if (tileMapSaveData.hasFireflyParticles)
			{
				SpawnFireflyParticles();
			}
			if (tileMapSaveData.mainEventRunnerData != null)
			{
				mainEventRunner = new EventRunner(oneshotWindow, tileMapSaveData.mainEventRunnerData);
			}
			overrideStepSounds = tileMapSaveData.overrideStepSounds;
		}

		public void ExecuteMapTransition(string transitionName)
		{
			Game1.gMan.TransitionTextureName = "transitions/" + transitionName;
			changeMapTimer = 0;
			totalChangeMapTime = 40;
			mapTransitionBlendMode = GraphicsManager.BlendMode.SpecialTransition;
		}
	}
}

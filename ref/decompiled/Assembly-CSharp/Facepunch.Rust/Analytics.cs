using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ConVar;
using Epic.OnlineServices.Version;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

namespace Facepunch.Rust;

public static class Analytics
{
	public static class Azure
	{
		[JsonModel]
		private struct EntitySumItem
		{
			public uint PrefabId;

			public int Count;

			public int Grade;
		}

		private struct EntityKey : IEquatable<EntityKey>
		{
			public uint PrefabId;

			public int Grade;

			public bool Equals(EntityKey other)
			{
				if (PrefabId == other.PrefabId)
				{
					return Grade == other.Grade;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (17 * 23 + PrefabId.GetHashCode()) * 31 + Grade.GetHashCode();
			}
		}

		private class PendingItemsData : Pool.IPooled
		{
			public PendingItemsKey Key;

			public int amount;

			public string category;

			public void EnterPool()
			{
				Key = default(PendingItemsKey);
				amount = 0;
				category = null;
			}

			public void LeavePool()
			{
			}
		}

		private struct PendingItemsKey : IEquatable<PendingItemsKey>
		{
			public string Item;

			public bool Consumed;

			public string Entity;

			public string Category;

			public NetworkableId EntityId;

			public bool Equals(PendingItemsKey other)
			{
				if (Item == other.Item && Entity == other.Entity && EntityId == other.EntityId && Consumed == other.Consumed)
				{
					return Category == other.Category;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return ((((17 * 23 + Item.GetHashCode()) * 31 + Consumed.GetHashCode()) * 37 + Entity.GetHashCode()) * 47 + Category.GetHashCode()) * 53 + EntityId.GetHashCode();
			}
		}

		[JsonModel]
		private class PlayerAggregate : Pool.IPooled
		{
			public string UserId;

			public Vector3 Position;

			public Vector3 Direction;

			public List<string> Hotbar = new List<string>();

			public List<string> Worn = new List<string>();

			public string ActiveItem;

			public string Biome;

			public void EnterPool()
			{
				UserId = null;
				Position = default(Vector3);
				Direction = default(Vector3);
				Hotbar.Clear();
				Worn.Clear();
				ActiveItem = null;
				Biome = null;
			}

			public void LeavePool()
			{
			}
		}

		[JsonModel]
		private class TeamInfo : Pool.IPooled
		{
			public List<string> online = new List<string>();

			public List<string> offline = new List<string>();

			public int member_count;

			public void EnterPool()
			{
				online.Clear();
				offline.Clear();
				member_count = 0;
			}

			public void LeavePool()
			{
			}
		}

		public enum ResourceMode
		{
			Produced,
			Consumed
		}

		private static class EventIds
		{
			public const string EntityBuilt = "entity_built";

			public const string EntityPickup = "entity_pickup";

			public const string EntityDamage = "entity_damage";

			public const string PlayerRespawn = "player_respawn";

			public const string ExplosiveLaunched = "explosive_launch";

			public const string Explosion = "explosion";

			public const string ItemEvent = "item_event";

			public const string EntitySum = "entity_sum";

			public const string ItemSum = "item_sum";

			public const string ItemDespawn = "item_despawn";

			public const string ItemDropped = "item_drop";

			public const string ItemPickup = "item_pickup";

			public const string AntihackViolation = "antihack_violation";

			public const string AntihackViolationDetailed = "antihack_violation_detailed";

			public const string PlayerConnect = "player_connect";

			public const string PlayerDisconnect = "player_disconnect";

			public const string ConsumableUsed = "consumeable_used";

			public const string MedUsed = "med_used";

			public const string ResearchStarted = "research_start";

			public const string BlueprintLearned = "blueprint_learned";

			public const string TeamChanged = "team_change";

			public const string EntityAuthChange = "auth_change";

			public const string VendingOrderChanged = "vending_changed";

			public const string VendingSale = "vending_sale";

			public const string ChatMessage = "chat";

			public const string BlockUpgrade = "block_upgrade";

			public const string BlockDemolish = "block_demolish";

			public const string ItemRepair = "item_repair";

			public const string EntityRepair = "entity_repair";

			public const string ItemSkinned = "item_skinned";

			public const string EntitySkinned = "entity_skinned";

			public const string ItemAggregate = "item_aggregate";

			public const string CodelockChanged = "code_change";

			public const string CodelockEntered = "code_enter";

			public const string SleepingBagAssign = "sleeping_bag_assign";

			public const string FallDamage = "fall_damage";

			public const string PlayerWipeIdSet = "player_wipe_id_set";

			public const string ServerInfo = "server_info";

			public const string UnderwaterCrateUntied = "crate_untied";

			public const string VehiclePurchased = "vehicle_purchase";

			public const string NPCVendor = "npc_vendor";

			public const string BlueprintsOnline = "blueprint_aggregate_online";

			public const string PlayerPositions = "player_positions";

			public const string ProjectileInvalid = "projectile_invalid";

			public const string ItemDefinitions = "item_definitions";

			public const string KeycardSwiped = "keycard_swiped";

			public const string EntitySpawned = "entity_spawned";

			public const string EntityKilled = "entity_killed";

			public const string HackableCrateStarted = "hackable_crate_started";

			public const string HackableCrateEnded = "hackable_crate_ended";

			public const string StashHidden = "stash_hidden";

			public const string StashRevealed = "stash_reveal";

			public const string EntityManifest = "entity_manifest";

			public const string LootEntity = "loot_entity";

			public const string OnlineTeams = "online_teams";

			public const string Gambling = "gambing";

			public const string BuildingBlockColor = "building_block_color";

			public const string MissionComplete = "mission_complete";

			public const string PlayerPinged = "player_pinged";

			public const string BagUnclaim = "bag_unclaim";

			public const string SteamAuth = "steam_auth";

			public const string ParachuteUsed = "parachute_used";

			public const string MountEntity = "mount";

			public const string DismountEntity = "dismount";

			public const string BurstToggle = "burst_toggle";

			public const string TutorialStarted = "tutorial_started";

			public const string TutorialCompleted = "tutorial_completed";

			public const string TutorialQuit = "tutorial_quit";

			public const string BaseInteraction = "base_interaction";

			public const string PlayerDeath = "player_death";

			public const string CarShredded = "car_shredded";

			public const string PlayerTick = "player_tick";

			public const string WallpaperPlaced = "wallpaper_placed";

			public const string StartFish = "fishing_start";

			public const string FailedFish = "fishing_failed";

			public const string CaughtFish = "fishing_caught";

			public const string InjureStateChange = "injure_state";

			public const string LifeStoryEnd = "life_story_end";

			public const string ServerRPC = "server_rpc";

			public const string AdminCommand = "admin_command";

			public const string PuzzleReset = "puzzle_reset";

			public const string DeepSeaTraverse = "deep_sea_traverse";

			public const string DeepSeaToggle = "deep_sea_toggle";

			public const string PlayerBoatFinish = "player_boat_finish";

			public const string ApartmentCheckIn = "apartment_checkin";

			public const string ApartmentCheckOut = "apartment_checkout";

			public const string ApartmentUpgrade = "apartment_upgrade";

			public const string ApartmentBreakIn = "apartment_breakin";

			public const string ShopOpened = "shop_opened";

			public const string ShopClosed = "shop_closed";

			public const string ShopBreakIn = "shop_breakin";

			public const string ClanCreated = "clan_created";

			public const string ClanDisbanded = "clan_disbanded";

			public const string ClanMemberAdded = "clan_member_added";

			public const string ClanMemberRemoved = "clan_member_removed";

			public const string ClanScoreEvent = "clan_score_event";

			public const string PowerGridStageChanged = "power_grid_stage_changed";

			public const string PowerGridFuseInserted = "power_grid_fuse_inserted";

			public const string CarOpenEditing = "car_open_editing";

			public const string AirfieldChinookCall = "airfield_chinook_call";

			public const string SatelliteLockTrajectory = "satellite_lock_trajectory";

			public const string SatelliteCrashed = "satellite_crashed";

			public const string IOEntityConnected = "io_entity_connected";

			public const string WaterTreatmentPlantEnabled = "water_treatment_plant_enabled";

			public const string OilRigFuelSwitchStarted = "oil_rig_fuel_switch_started";
		}

		private struct SimpleItemAmount
		{
			public string ItemName;

			public int Amount;

			public ulong Skin;

			public float Condition;

			public SimpleItemAmount(Item item)
			{
				ItemName = item.info.shortname;
				Amount = item.amount;
				Skin = item.skin;
				Condition = item.conditionNormalized;
			}
		}

		private struct FiredProjectileKey : IEquatable<FiredProjectileKey>
		{
			public ulong UserId;

			public int ProjectileId;

			public FiredProjectileKey(ulong userId, int projectileId)
			{
				UserId = userId;
				ProjectileId = projectileId;
			}

			public bool Equals(FiredProjectileKey other)
			{
				if (other.UserId == UserId)
				{
					return other.ProjectileId == ProjectileId;
				}
				return false;
			}
		}

		private class PendingFiredProjectile : Pool.IPooled
		{
			public EventRecord Record;

			public BasePlayer.FiredProjectile FiredProjectile;

			public bool Hit;

			public void EnterPool()
			{
				Hit = false;
				Record = null;
				FiredProjectile = null;
			}

			public void LeavePool()
			{
			}
		}

		private static Dictionary<int, string> geneCache = new Dictionary<int, string>();

		private static readonly HashSet<string> RpcLoggingExclusions = new HashSet<string> { "performancereport", "broadcastsignalfromclient", "moveitem", "updatesteaminventory", "rpc_openloot", "receiveclientrotation", "rpc_opendoor", "rpc_closedoor" };

		public static int MaxMSPerFrame = 5;

		private static Dictionary<PendingItemsKey, PendingItemsData> pendingItems = new Dictionary<PendingItemsKey, PendingItemsData>();

		private static Dictionary<FiredProjectileKey, PendingFiredProjectile> trackedProjectiles = new Dictionary<FiredProjectileKey, PendingFiredProjectile>();

		public static bool GameplayAnalytics => GameplayAnalyticsConVar;

		private static string GetGenesAsString(GrowableEntity plant)
		{
			int key = GrowableGeneEncoding.EncodeGenesToInt(plant.Genes);
			if (!geneCache.TryGetValue(key, out var value))
			{
				return string.Join("", from x in plant.Genes.Genes
					group x by x.GetDisplayCharacter() into x
					orderby x.Key
					select x.Count() + x.Key);
			}
			return value;
		}

		private static string GetMonument(BaseEntity entity)
		{
			if (entity == null)
			{
				return null;
			}
			SpawnGroup spawnGroup = null;
			if (entity is BaseCorpse baseCorpse)
			{
				spawnGroup = baseCorpse.spawnGroup;
			}
			if (spawnGroup == null)
			{
				SpawnPointInstance component = entity.GetComponent<SpawnPointInstance>();
				if (component != null)
				{
					spawnGroup = component.parentSpawnPointUser as SpawnGroup;
				}
			}
			if (spawnGroup != null)
			{
				if (!string.IsNullOrEmpty(spawnGroup.category))
				{
					return spawnGroup.category;
				}
				if (spawnGroup.Monument != null)
				{
					return spawnGroup.Monument.name;
				}
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.FindMonumentWithBoundsOverlap(entity.transform.position);
			if (monumentInfo != null)
			{
				return monumentInfo.name;
			}
			return null;
		}

		private static string GetBiome(Vector3 position)
		{
			string result = null;
			switch ((TerrainBiome.Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(position))
			{
			case TerrainBiome.Enum.Arid:
				result = "arid";
				break;
			case TerrainBiome.Enum.Temperate:
				result = "grass";
				break;
			case TerrainBiome.Enum.Tundra:
				result = "tundra";
				break;
			case TerrainBiome.Enum.Arctic:
				result = "arctic";
				break;
			case TerrainBiome.Enum.Jungle:
				result = "jungle";
				break;
			case TerrainBiome.Enum.DeepSea:
				result = "deepsea";
				break;
			}
			return result;
		}

		private static bool IsOcean(Vector3 position)
		{
			return TerrainMeta.TopologyMap.GetTopology(position) == 128;
		}

		public static Dictionary<string, string> GetHardwareData()
		{
			string value;
			using (SHA256 sHA = SHA256.Create())
			{
				value = Convert.ToBase64String(sHA.ComputeHash(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier)));
			}
			return new Dictionary<string, string>
			{
				["device_name"] = SystemInfo.deviceName,
				["device_hash"] = value,
				["gpu_name"] = SystemInfo.graphicsDeviceName,
				["gpu_ram"] = SystemInfo.graphicsMemorySize.ToString(),
				["gpu_vendor"] = SystemInfo.graphicsDeviceVendor,
				["gpu_version"] = SystemInfo.graphicsDeviceVersion,
				["gpu_shader_level"] = SystemInfo.graphicsShaderLevel.ToString(),
				["gpu_max_buffer_size"] = SystemInfo.maxGraphicsBufferSize.ToString(),
				["gpu_device_version"] = SystemInfo.graphicsDeviceVersion.ToString(),
				["cpu_cores"] = SystemInfo.processorCount.ToString(),
				["max_compute_work_size"] = SystemInfo.maxComputeWorkGroupSize.ToString(),
				["max_compute_work_size_x"] = SystemInfo.maxComputeWorkGroupSizeX.ToString(),
				["max_compute_work_size_y"] = SystemInfo.maxComputeWorkGroupSizeY.ToString(),
				["max_compute_work_size_z"] = SystemInfo.maxComputeWorkGroupSizeZ.ToString(),
				["cpu_frequency"] = SystemInfo.processorFrequency.ToString(),
				["gpu_max_texture_size"] = SystemInfo.maxTextureSize.ToString(),
				["cpu_name"] = SystemInfo.processorType.Trim(),
				["system_memory"] = SystemInfo.systemMemorySize.ToString(),
				["os"] = SystemInfo.operatingSystem,
				["supports_compute_shaders"] = SystemInfo.supportsComputeShaders.ToString(),
				["supports_async_compute"] = SystemInfo.supportsAsyncCompute.ToString(),
				["supports_async_gpu_readback"] = SystemInfo.supportsAsyncGPUReadback.ToString(),
				["supports_3d_textures"] = SystemInfo.supports3DTextures.ToString(),
				["supports_instancing"] = SystemInfo.supportsInstancing.ToString()
			};
		}

		public static Dictionary<string, string> GetApplicationData()
		{
			return new Dictionary<string, string>
			{
				["unity"] = UnityEngine.Application.unityVersion ?? "editor",
				["changeset"] = BuildInfo.Current?.Scm.ChangeId ?? "editor",
				["branch"] = BuildInfo.Current?.Scm.Branch ?? "editor",
				["network_version"] = 2633.ToString(),
				["eos_sdk"] = VersionInterface.GetVersion()?.ToString() ?? "disabled"
			};
		}

		public static bool ShouldLogRPC(string rpc)
		{
			if (RpcLoggingExclusions.Contains(rpc))
			{
				return false;
			}
			return true;
		}

		private static IEnumerator AggregateLoop()
		{
			int loop = 0;
			while (!global::Rust.Application.isQuitting)
			{
				yield return CoroutineEx.waitForSecondsRealtime(60f);
				if (GameplayAnalytics)
				{
					yield return TryCatch(AggregatePlayers(blueprints: false, positions: true));
					if (loop % 60 == 0)
					{
						PushServerInfo();
						yield return TryCatch(AggregateEntitiesAndItems());
						yield return TryCatch(AggregatePlayers(blueprints: true));
						yield return TryCatch(AggregateTeams());
						Dictionary<PendingItemsKey, PendingItemsData> dict = pendingItems;
						pendingItems = new Dictionary<PendingItemsKey, PendingItemsData>();
						yield return PushPendingItemsLoopAsync(dict);
					}
					loop++;
				}
			}
		}

		private static IEnumerator TryCatch(IEnumerator coroutine)
		{
			while (true)
			{
				try
				{
					if (!coroutine.MoveNext())
					{
						break;
					}
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					break;
				}
				yield return coroutine.Current;
			}
		}

		private static IEnumerator AggregateEntitiesAndItems()
		{
			List<BaseNetworkable> entityQueue = new List<BaseNetworkable>();
			entityQueue.Clear();
			int totalCount = BaseNetworkable.serverEntities.Count;
			entityQueue.AddRange(BaseNetworkable.serverEntities);
			Dictionary<string, int> itemDict = new Dictionary<string, int>();
			Dictionary<EntityKey, int> entityDict = new Dictionary<EntityKey, int>();
			yield return null;
			UnityEngine.Debug.Log("Starting to aggregate entities & items...");
			DateTime startTime = DateTime.UtcNow;
			Stopwatch watch = Stopwatch.StartNew();
			foreach (BaseNetworkable entity in entityQueue)
			{
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
				if (entity == null || entity.IsDestroyed)
				{
					continue;
				}
				EntityKey entityKey = default(EntityKey);
				entityKey.PrefabId = entity.prefabID;
				EntityKey key = entityKey;
				if (entity is BuildingBlock buildingBlock)
				{
					key.Grade = (int)(buildingBlock.grade + 1);
				}
				entityDict.TryGetValue(key, out var value);
				entityDict[key] = value + 1;
				if (!(entity is LootContainer) && !(entity is BasePlayer { IsNpc: not false }) && !(entity is NPCPlayer))
				{
					if (entity is BasePlayer basePlayer2)
					{
						AddItemsToDict(basePlayer2.inventory.containerMain, itemDict);
						AddItemsToDict(basePlayer2.inventory.containerBelt, itemDict);
						AddItemsToDict(basePlayer2.inventory.containerWear, itemDict);
					}
					else if (entity is IItemContainerEntity itemContainerEntity)
					{
						AddItemsToDict(itemContainerEntity.inventory, itemDict);
					}
					else if (entity is DroppedItemContainer { inventory: not null } droppedItemContainer)
					{
						AddItemsToDict(droppedItemContainer.inventory, itemDict);
					}
				}
			}
			UnityEngine.Debug.Log($"Took {System.Math.Round(DateTime.UtcNow.Subtract(startTime).TotalSeconds, 1)}s to aggregate {totalCount} entities & items...");
			_ = DateTime.UtcNow;
			SubmitPoint(EventRecord.New("entity_sum").AddObject("counts", entityDict.Select(delegate(KeyValuePair<EntityKey, int> x)
			{
				EntitySumItem result = default(EntitySumItem);
				result.PrefabId = x.Key.PrefabId;
				result.Grade = x.Key.Grade;
				result.Count = x.Value;
				return result;
			})));
			yield return null;
			SubmitPoint(EventRecord.New("item_sum").AddObject("counts", itemDict));
			yield return null;
		}

		private static void AddItemsToDict(ItemContainer container, Dictionary<string, int> dict)
		{
			if (container == null || container.itemList == null)
			{
				return;
			}
			foreach (Item item in container.itemList)
			{
				string shortname = item.info.shortname;
				dict.TryGetValue(shortname, out var value);
				dict[shortname] = value + item.amount;
				if (item.contents != null)
				{
					AddItemsToDict(item.contents, dict);
				}
			}
		}

		private static IEnumerator PushPendingItemsLoopAsync(Dictionary<PendingItemsKey, PendingItemsData> dict)
		{
			Stopwatch watch = Stopwatch.StartNew();
			foreach (PendingItemsData value in dict.Values)
			{
				try
				{
					LogResource(value.Key.Consumed ? ResourceMode.Consumed : ResourceMode.Produced, value.category, value.Key.Item, value.amount, null, null, safezone: false, null, 0uL, value.Key.Entity, null, null, 0uL);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				PendingItemsData obj = value;
				Pool.Free(ref obj);
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			dict.Clear();
		}

		public static void AddPendingItems(BaseEntity entity, string itemName, int amount, string category, bool consumed = true, bool perEntity = false)
		{
			PendingItemsKey pendingItemsKey = default(PendingItemsKey);
			pendingItemsKey.Entity = entity.ShortPrefabName;
			pendingItemsKey.Category = category;
			pendingItemsKey.Item = itemName;
			pendingItemsKey.Consumed = consumed;
			pendingItemsKey.EntityId = (perEntity ? entity.net.ID : default(NetworkableId));
			PendingItemsKey key = pendingItemsKey;
			if (!pendingItems.TryGetValue(key, out var value))
			{
				value = Pool.Get<PendingItemsData>();
				value.Key = key;
				value.category = category;
				pendingItems[key] = value;
			}
			value.amount += amount;
		}

		private static IEnumerator AggregatePlayers(bool blueprints = false, bool positions = false)
		{
			Stopwatch watch = Stopwatch.StartNew();
			Dictionary<int, int> playerBps = (blueprints ? new Dictionary<int, int>() : null);
			List<PlayerAggregate> playerPositions = (positions ? Pool.Get<List<PlayerAggregate>>() : null);
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				if (activePlayer == null || activePlayer.IsDestroyed)
				{
					continue;
				}
				if (blueprints)
				{
					foreach (int unlockedItem in activePlayer.PersistantPlayerInfo.unlockedItems)
					{
						playerBps.TryGetValue(unlockedItem, out var value);
						playerBps[unlockedItem] = value + 1;
					}
				}
				if (positions)
				{
					PlayerAggregate playerAggregate = Pool.Get<PlayerAggregate>();
					playerAggregate.UserId = activePlayer.UserIDString;
					playerAggregate.Position = activePlayer.transform.position;
					playerAggregate.Direction = activePlayer.eyes.bodyRotation.eulerAngles;
					foreach (Item item in activePlayer.inventory.containerBelt.itemList)
					{
						playerAggregate.Hotbar.Add(item.info.shortname);
					}
					foreach (Item item2 in activePlayer.inventory.containerWear.itemList)
					{
						playerAggregate.Worn.Add(item2.info.shortname);
					}
					playerAggregate.ActiveItem = activePlayer.GetActiveItem()?.info.shortname;
					playerAggregate.Biome = GetBiome(activePlayer.transform.position);
					playerPositions.Add(playerAggregate);
				}
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			if (blueprints)
			{
				SubmitPoint(EventRecord.New("blueprint_aggregate_online").AddObject("blueprints", playerBps.Select((KeyValuePair<int, int> x) => new
				{
					Key = ItemManager.FindItemDefinition(x.Key).shortname,
					value = x.Value
				})));
			}
			if (positions)
			{
				SubmitPoint(EventRecord.New("player_positions").AddObject("positions", playerPositions).AddObject("player_count", playerPositions.Count));
				Pool.Free(ref playerPositions, freeElements: true);
			}
		}

		private static IEnumerator AggregateTeams()
		{
			yield return null;
			HashSet<ulong> teamIds = new HashSet<ulong>();
			int inTeam = 0;
			int notInTeam = 0;
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				if (activePlayer != null && !activePlayer.IsDestroyed && activePlayer.currentTeam != 0L)
				{
					teamIds.Add(activePlayer.currentTeam);
					inTeam++;
				}
				else
				{
					notInTeam++;
				}
			}
			yield return null;
			Stopwatch watch = Stopwatch.StartNew();
			List<TeamInfo> teams = Pool.Get<List<TeamInfo>>();
			foreach (ulong item in teamIds)
			{
				RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(item);
				if (playerTeam == null || !((playerTeam.members != null) & (playerTeam.members.Count > 0)))
				{
					continue;
				}
				TeamInfo teamInfo = Pool.Get<TeamInfo>();
				teams.Add(teamInfo);
				foreach (ulong member in playerTeam.members)
				{
					BasePlayer basePlayer = RelationshipManager.FindByID(member);
					if (basePlayer != null && !basePlayer.IsDestroyed && basePlayer.IsConnected && !basePlayer.IsSleeping())
					{
						teamInfo.online.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
					else
					{
						teamInfo.offline.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
				}
				teamInfo.member_count = teamInfo.online.Count + teamInfo.offline.Count;
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			SubmitPoint(EventRecord.New("online_teams").AddObject("teams", teams).AddField("users_in_team", inTeam)
				.AddField("users_not_in_team", notInTeam));
			foreach (TeamInfo item2 in teams)
			{
				TeamInfo obj = item2;
				Pool.Free(ref obj);
			}
			Pool.Free(ref teams, freeElements: false);
		}

		public static void Initialize()
		{
			TickLogging.RegisterForAnalytics(Manager);
			Manager.AddTable(AnalyticsManager.GameplayEventsTableServer, Manager.ServerUploader);
			PushItemDefinitions();
			PushEntityManifest();
			SingletonComponent<ServerMgr>.Instance.StartCoroutine(AggregateLoop());
		}

		private static void PushServerInfo()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("server_info").AddField("seed", World.Seed).AddField("size", World.Size)
					.AddField("url", World.Url)
					.AddField("ip_convar", global::Network.Net.sv.ip)
					.AddField("port_convar", global::Network.Net.sv.port)
					.AddField("net_protocol", global::Network.Net.sv.ProtocolId)
					.AddField("protocol_network", 2633)
					.AddField("protocol_save", 288)
					.AddField("changeset", BuildInfo.Current?.Scm.ChangeId ?? "0")
					.AddField("unity_version", UnityEngine.Application.unityVersion)
					.AddField("branch", BuildInfo.Current?.Scm.Branch ?? "empty")
					.AddField("server_tags", ConVar.Server.tags)
					.AddField("device_id", SystemInfo.deviceUniqueIdentifier)
					.AddField("network_id", global::Network.Net.sv.GetLastUIDGiven()));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void PushItemDefinitions()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (GameManifest.Current == null || BuildInfo.Current?.Scm?.ChangeId == null)
				{
					return;
				}
				SubmitPoint(EventRecord.New("item_definitions").AddObject("items", from x in ItemManager.itemDictionary
					select x.Value into x
					select new
					{
						item_id = x.itemid,
						shortname = x.shortname,
						craft_time = (x.Blueprint?.GetCraftTime() ?? 0f),
						workbench = (x.Blueprint?.GetWorkbenchLevel() ?? 0),
						category = x.category.ToString(),
						display_name = x.displayName.english,
						despawn_rarity = x.despawnRarity,
						ingredients = (from y in x.Blueprint?.GetIngredients()
							select new
							{
								shortname = y.itemDef.shortname,
								amount = (int)y.amount
							}),
						wm_tris = x.GetWorldModelTriCount()
					}).AddField("changeset", BuildInfo.Current.Scm.ChangeId));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void PushEntityManifest()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (!(GameManifest.Current == null) && BuildInfo.Current?.Scm?.ChangeId != null)
				{
					SubmitPoint(EventRecord.New("entity_manifest").AddObject("entities", GameManifest.Current.entities.Select((string x) => new
					{
						shortname = Path.GetFileNameWithoutExtension(x),
						prefab_id = StringPool.Get(x.ToLower())
					})).AddField("changeset", BuildInfo.Current?.Scm.ChangeId ?? "editor"));
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void SubmitPoint(EventRecord point)
		{
			point.Submit();
		}

		public static void OnFiredProjectile(BasePlayer player, BasePlayer.FiredProjectile projectile, Guid projectileGroupId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord record = EventRecord.New("entity_damage").AddField("start_pos", projectile.position).AddField("start_vel", projectile.initialVelocity)
					.AddField("velocity_inherit", projectile.inheritedVelocity)
					.AddField("ammo_item", projectile.itemDef?.shortname)
					.AddField("weapon", projectile.weaponSource)
					.AddField("projectile_group", projectileGroupId)
					.AddField("projectile_id", projectile.id)
					.AddField("attacker", player)
					.AddField("look_dir", player.tickViewAngles)
					.AddField("model_state", (player.modelStateTick ?? player.modelState).flags)
					.AddField("burst_mode", projectile.weaponSource?.HasFlag(BaseEntity.Flags.Reserved6) ?? false);
				PendingFiredProjectile pendingFiredProjectile = Pool.Get<PendingFiredProjectile>();
				pendingFiredProjectile.Record = record;
				pendingFiredProjectile.FiredProjectile = projectile;
				trackedProjectiles[new FiredProjectileKey(player.userID, projectile.id)] = pendingFiredProjectile;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFiredProjectileRemoved(BasePlayer player, BasePlayer.FiredProjectile projectile)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				FiredProjectileKey key = new FiredProjectileKey(player.userID, projectile.id);
				if (!trackedProjectiles.TryGetValue(key, out var value))
				{
					return;
				}
				if (!value.Hit)
				{
					EventRecord record = value.Record;
					if (projectile.updates.Count > 0)
					{
						record.AddObject("projectile_updates", projectile.updates);
					}
					SubmitPoint(record);
				}
				Pool.Free(ref value);
				trackedProjectiles.Remove(key);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnQuarryItem(ResourceMode mode, string item, int amount, MiningQuarry sourceEntity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				AddPendingItems(sourceEntity, item, amount, "quarry", mode == ResourceMode.Consumed);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExcavatorProduceItem(Item item, BaseEntity sourceEntity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				AddPendingItems(sourceEntity, item.info.shortname, item.amount, "excavator", consumed: false);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExcavatorConsumeFuel(Item item, int amount, BaseEntity dieselEngine)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Consumed, "excavator", item.info.shortname, amount, dieselEngine, null, safezone: false, null, 0uL, null, null, null, 0uL);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCraftItem(string item, int amount, BasePlayer player, BaseEntity workbench, bool inSafezone, ulong skin = 0uL)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "craft", item, amount, null, null, inSafezone, workbench, player?.userID ?? ((EncryptedValue<ulong>)0uL), null, null, null, skin);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCraftMaterialConsumed(string item, int amount, BasePlayer player, BaseEntity workbench, bool inSafezone, string targetItem)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(safezone: inSafezone, workbench: workbench, targetItem: targetItem, mode: ResourceMode.Consumed, category: "craft", itemName: item, amount: amount, sourceEntity: null, tool: null, steamId: player?.userID ?? ((EncryptedValue<ulong>)0uL), sourceEntityPrefab: null, sourceItem: null, skinId: 0uL);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnConsumableUsed(BasePlayer player, Item item)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("consumeable_used").AddField("player", player).AddField("item", item));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntitySpawned(BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_spawned").AddShortEntityField("entity", entity, includePos: true));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityDestroyed(BaseEntity entity)
		{
			if (!GameplayAnalytics || !entity.IsValid())
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_killed").AddShortEntityField("entity", entity, entity.syncPosition));
				if (!(entity is LootContainer { FirstLooterId: 0uL } lootContainer))
				{
					return;
				}
				foreach (Item item in lootContainer.inventory.itemList)
				{
					OnItemDespawn(lootContainer, item, 3, lootContainer.LastLootedBy);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnMedUsed(string itemName, BasePlayer player, BaseEntity target)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("med_used").AddField("player", player).AddField("target", target)
					.AddField("item_name", itemName));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCodelockChanged(BasePlayer player, CodeLock codeLock, string oldCode, string newCode, bool isGuest)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("code_change").AddField("player", player).AddField("codelock", codeLock)
					.AddField("old_code", oldCode)
					.AddField("new_code", newCode)
					.AddField("is_guest", isGuest));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCodeLockEntered(BasePlayer player, CodeLock codeLock, bool isGuest)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("code_enter").AddField("player", player).AddField("codelock", codeLock)
					.AddField("is_guest", isGuest));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTeamChanged(string change, ulong teamId, ulong teamLeader, ulong user, List<ulong> members)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			List<ulong> obj = Pool.Get<List<ulong>>();
			try
			{
				if (members != null)
				{
					foreach (ulong member in members)
					{
						obj.Add(member);
					}
				}
				SubmitPoint(EventRecord.New("team_change").AddField("team_leader", teamLeader).AddField("team", teamId)
					.AddField("target_user", user)
					.AddField("change", change)
					.AddObject("users", obj)
					.AddField("member_count", members.Count));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			Pool.FreeUnmanaged(ref obj);
		}

		public static void OnEntityAuthChanged(BaseEntity entity, BasePlayer player, IEnumerable<ulong> authedList, string change, ulong targetUser)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("auth_change").AddField("entity", entity).AddField("player", player)
					.AddField("target", targetUser)
					.AddObject("auth_list", authedList)
					.AddField("change", change));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSleepingBagAssigned(BasePlayer player, SleepingBag bag, ulong targetUser)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("sleeping_bag_assign").AddField("entity", bag).AddField("player", player)
					.AddField("target", targetUser));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFallDamage(BasePlayer player, float velocity, float damage)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fall_damage").AddField("player", player).AddField("velocity", velocity)
					.AddField("damage", damage));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnResearchStarted(BasePlayer player, BaseEntity entity, Item item, int scrapCost)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("research_start").AddField("player", player).AddField("item", item.info.shortname)
					.AddField("scrap", scrapCost)
					.AddField("entity", entity));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBlueprintLearned(BasePlayer player, ItemDefinition item, string reason, int scrapCost, BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("blueprint_learned").AddField("player", player).AddField("item", item.shortname)
					.AddField("reason", reason)
					.AddField("entity", entity)
					.AddField("scrap_cost", scrapCost));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemRecycled(string item, int amount, Recycler recycler)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Consumed, "recycler", item, amount, recycler, null, safezone: false, null, recycler.LastLootedBy, null, null, null, 0uL);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnRecyclerItemProduced(string item, int amount, Recycler recycler, Item sourceItem)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "recycler", item, amount, recycler, null, safezone: false, null, recycler.LastLootedBy, null, sourceItem, null, 0uL);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnGatherItem(string item, int amount, BaseEntity sourceEntity, BasePlayer player, AttackEntity weapon = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "gather", item, amount, sourceEntity, weapon, safezone: false, null, player.userID, null, null, null, 0uL);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFirstLooted(BaseEntity entity, BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (entity is LootContainer lootContainer)
				{
					LogContainerLooted(lootContainer.inventory, entity, player);
				}
				else if (entity is RespawnableLootFridge respawnableLootFridge)
				{
					LogContainerLooted(respawnableLootFridge.inventory, entity, player);
				}
				else if (entity is LootableCorpse { containers: var containers })
				{
					foreach (ItemContainer container in containers)
					{
						LogItemsLooted(player, entity, container);
					}
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void LogContainerLooted(ItemContainer container, BaseEntity entity, BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				LogItemsLooted(player, entity, container);
				SubmitPoint(EventRecord.New("loot_entity").AddField("entity", entity).AddField("player", player)
					.AddField("monument", GetMonument(entity))
					.AddField("biome", GetBiome(entity.transform.position)));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnLootContainerDestroyed(LootContainer entity, BasePlayer player, AttackEntity weapon)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (entity.DropsLoot && player != null && Vector3.Distance(entity.transform.position, player.transform.position) < 50f && entity.inventory?.itemList != null && entity.inventory.itemList.Count > 0)
				{
					LogItemsLooted(player, entity, entity.inventory, weapon);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityBuilt(BaseEntity entity, BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("entity_built").AddField("player", player).AddField("entity", entity);
				if (entity is SleepingBag)
				{
					int sleepingBagCount = SleepingBag.GetSleepingBagCount(player.userID);
					eventRecord.AddField("bags_active", sleepingBagCount);
					eventRecord.AddField("max_sleeping_bags", ConVar.Server.max_sleeping_bags);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnMountEntity(BasePlayer player, BaseEntity seat, BaseEntity vehicle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("mount").AddField("player", player).AddField("vehicle", vehicle)
					.AddField("seat", seat));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnDismountEntity(BasePlayer player, BaseEntity seat, BaseEntity vehicle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("dismount").AddField("player", player).AddField("vehicle", vehicle)
					.AddField("seat", seat));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnKeycardSwiped(BasePlayer player, CardReader cardReader)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("keycard_swiped").AddField("player", player).AddField("card_level", cardReader.accessLevel)
					.AddField("entity", cardReader));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnLockedCrateStarted(BasePlayer player, HackableLockedCrate crate)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("hackable_crate_started").AddField("player", player).AddField("entity", crate));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnLockedCrateFinished(ulong player, HackableLockedCrate crate)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("hackable_crate_ended").AddField("player_steamid", player).AddField("entity", crate));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnStashHidden(BasePlayer player, StashContainer entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("stash_hidden").AddField("player", player).AddField("entity", entity)
					.AddField("owner", entity.OwnerID));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnStashRevealed(BasePlayer player, StashContainer entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("stash_reveal").AddField("player", player).AddField("entity", entity)
					.AddField("owner", entity.OwnerID));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnAntihackViolation(BasePlayer player, AntiHackType type, string message)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("antihack_violation").AddField("player", player).AddField("violation_type", (int)type)
					.AddField("violation", type.ToString())
					.AddField("message", message);
				if (BuildInfo.Current != null)
				{
					eventRecord.AddField("changeset", BuildInfo.Current.Scm.ChangeId).AddField("network", 2633);
				}
				switch (type)
				{
				case AntiHackType.SpeedHack:
					eventRecord.AddField("speedhack_protection", ConVar.AntiHack.speedhack_protection).AddField("speedhack_forgiveness", ConVar.AntiHack.speedhack_forgiveness).AddField("speedhack_forgiveness_inertia", ConVar.AntiHack.speedhack_forgiveness_inertia)
						.AddField("speedhack_penalty", ConVar.AntiHack.speedhack_penalty)
						.AddField("speedhack_penalty", ConVar.AntiHack.speedhack_reject)
						.AddField("speedhack_slopespeed", ConVar.AntiHack.speedhack_slopespeed);
					break;
				case AntiHackType.NoClip:
					eventRecord.AddField("noclip_protection", ConVar.AntiHack.noclip_protection).AddField("noclip_penalty", ConVar.AntiHack.noclip_penalty).AddField("noclip_maxsteps", ConVar.AntiHack.noclip_maxsteps)
						.AddField("noclip_margin_dismount", ConVar.AntiHack.noclip_margin_dismount)
						.AddField("noclip_margin", ConVar.AntiHack.noclip_margin)
						.AddField("noclip_backtracking", ConVar.AntiHack.noclip_backtracking)
						.AddField("noclip_reject", ConVar.AntiHack.noclip_reject)
						.AddField("noclip_stepsize", ConVar.AntiHack.noclip_stepsize);
					break;
				case AntiHackType.ProjectileHack:
					eventRecord.AddField("projectile_anglechange", ConVar.AntiHack.projectile_anglechange).AddField("projectile_backtracking", ConVar.AntiHack.projectile_backtracking).AddField("projectile_clientframes", ConVar.AntiHack.projectile_clientframes)
						.AddField("projectile_damagedepth", ConVar.AntiHack.projectile_damagedepth)
						.AddField("projectile_desync", ConVar.AntiHack.projectile_desync)
						.AddField("projectile_forgiveness", ConVar.AntiHack.projectile_forgiveness)
						.AddField("projectile_impactspawndepth", ConVar.AntiHack.projectile_impactspawndepth)
						.AddField("projectile_losforgiveness", ConVar.AntiHack.projectile_losforgiveness)
						.AddField("projectile_penalty", ConVar.AntiHack.projectile_penalty)
						.AddField("projectile_positionoffset", ConVar.AntiHack.projectile_positionoffset)
						.AddField("projectile_protection", ConVar.AntiHack.projectile_protection)
						.AddField("projectile_serverframes", ConVar.AntiHack.projectile_serverframes)
						.AddField("projectile_terraincheck", ConVar.AntiHack.projectile_terraincheck)
						.AddField("projectile_trajectory", ConVar.AntiHack.projectile_trajectory)
						.AddField("projectile_vehiclecheck", ConVar.AntiHack.projectile_vehiclecheck)
						.AddField("projectile_velocitychange", ConVar.AntiHack.projectile_velocitychange);
					break;
				case AntiHackType.InsideTerrain:
					eventRecord.AddField("terrain_check_geometry", ConVar.AntiHack.terrain_check_geometry).AddField("terrain_kill", ConVar.AntiHack.terrain_kill).AddField("terrain_padding", ConVar.AntiHack.terrain_padding)
						.AddField("terrain_penalty", ConVar.AntiHack.terrain_penalty)
						.AddField("terrain_protection", ConVar.AntiHack.terrain_protection)
						.AddField("terrain_timeslice", ConVar.AntiHack.terrain_timeslice);
					break;
				case AntiHackType.MeleeHack:
					eventRecord.AddField("melee_backtracking", ConVar.AntiHack.melee_backtracking).AddField("melee_clientframes", ConVar.AntiHack.melee_clientframes).AddField("melee_forgiveness", ConVar.AntiHack.melee_forgiveness)
						.AddField("melee_losforgiveness", ConVar.AntiHack.melee_losforgiveness)
						.AddField("melee_penalty", ConVar.AntiHack.melee_penalty)
						.AddField("melee_protection", ConVar.AntiHack.melee_protection)
						.AddField("melee_serverframes", ConVar.AntiHack.melee_serverframes)
						.AddField("melee_terraincheck", ConVar.AntiHack.melee_terraincheck)
						.AddField("melee_vehiclecheck", ConVar.AntiHack.melee_vehiclecheck);
					break;
				case AntiHackType.FlyHack:
					eventRecord.AddField("flyhack_extrusion", ConVar.AntiHack.flyhack_extrusion).AddField("flyhack_forgiveness_horizontal", ConVar.AntiHack.flyhack_forgiveness_horizontal).AddField("flyhack_forgiveness_horizontal_inertia", ConVar.AntiHack.flyhack_forgiveness_horizontal_inertia)
						.AddField("flyhack_forgiveness_vertical", ConVar.AntiHack.flyhack_forgiveness_vertical)
						.AddField("flyhack_forgiveness_vertical_inertia", ConVar.AntiHack.flyhack_forgiveness_vertical_inertia)
						.AddField("flyhack_margin", ConVar.AntiHack.flyhack_margin)
						.AddField("flyhack_maxsteps", ConVar.AntiHack.flyhack_maxsteps)
						.AddField("flyhack_penalty", ConVar.AntiHack.flyhack_penalty)
						.AddField("flyhack_protection", ConVar.AntiHack.flyhack_protection)
						.AddField("flyhack_reject", ConVar.AntiHack.flyhack_reject);
					break;
				case AntiHackType.EyeHack:
					eventRecord.AddField("eye_clientframes", ConVar.AntiHack.eye_clientframes).AddField("eye_forgiveness", ConVar.AntiHack.eye_forgiveness).AddField("eye_history_forgiveness", ConVar.AntiHack.eye_history_forgiveness)
						.AddField("eye_history_penalty", ConVar.AntiHack.eye_history_penalty)
						.AddField("eye_losradius", ConVar.AntiHack.eye_losradius)
						.AddField("eye_noclip_backtracking", ConVar.AntiHack.eye_noclip_backtracking)
						.AddField("eye_noclip_cutoff", ConVar.AntiHack.eye_noclip_cutoff)
						.AddField("eye_penalty", ConVar.AntiHack.eye_penalty)
						.AddField("eye_protection", ConVar.AntiHack.eye_protection)
						.AddField("eye_serverframes", ConVar.AntiHack.eye_serverframes)
						.AddField("eye_terraincheck", ConVar.AntiHack.eye_terraincheck)
						.AddField("eye_vehiclecheck", ConVar.AntiHack.eye_vehiclecheck);
					break;
				case AntiHackType.AttackHack:
					eventRecord.AddField("maxdesync", ConVar.AntiHack.maxdesync);
					break;
				case AntiHackType.Ticks:
					eventRecord.AddField("max_distance", ConVar.AntiHack.tick_max_distance).AddField("max_distance_falling", ConVar.AntiHack.tick_max_distance_falling).AddField("max_distance_parented", ConVar.AntiHack.tick_max_distance_parented)
						.AddField("tick_buffer_noclip_threshold", ConVar.AntiHack.tick_buffer_noclip_threshold)
						.AddField("tick_buffer_reject_threshold", ConVar.AntiHack.tick_buffer_reject_threshold);
					break;
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEyehackViolation(BasePlayer player, Vector3 eyePos)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 6)
					.AddField("eye_pos", eyePos));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnNoclipViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount, Collider collider)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 1)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("collider_name", collider.name));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFlyhackViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 3)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnProjectileHackViolation(BasePlayer.FiredProjectile projectile)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (!projectile.invalid)
				{
					FiredProjectileKey key = new FiredProjectileKey(projectile.attacker.userID, projectile.id);
					if (trackedProjectiles.TryGetValue(key, out var value))
					{
						projectile.invalid = true;
						value.Record.AddField("projectile_invalid", value: true).AddObject("updates", projectile.updates);
					}
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSpeedhackViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 2)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("distance", Vector3.Distance(startPos, endPos))
					.AddField("distance_2d", Vector3Ex.Distance2D(startPos, endPos)));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTickViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 13)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("distance", Vector3.Distance(startPos, endPos))
					.AddField("distance_2d", Vector3Ex.Distance2D(startPos, endPos)));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTerrainHackViolation(BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 10)
					.AddField("seed", World.Seed)
					.AddField("size", World.Size)
					.AddField("map_url", World.Url)
					.AddField("map_checksum", World.Checksum));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityTakeDamage(HitInfo info, bool isDeath)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				BasePlayer initiatorPlayer = info.InitiatorPlayer;
				BasePlayer basePlayer = info.HitEntity as BasePlayer;
				if ((info.Initiator == null && !isDeath) || ((initiatorPlayer == null || initiatorPlayer.IsNpc || initiatorPlayer.IsBot) && (basePlayer == null || basePlayer.IsNpc || basePlayer.IsBot)))
				{
					return;
				}
				EventRecord eventRecord = null;
				float value = -1f;
				float value2 = -1f;
				if (initiatorPlayer != null)
				{
					if (info.IsProjectile())
					{
						FiredProjectileKey key = new FiredProjectileKey(initiatorPlayer.userID, info.ProjectileID);
						if (trackedProjectiles.TryGetValue(key, out var value3))
						{
							eventRecord = value3.Record;
							value = Vector3.Distance(info.HitPositionWorld, value3.FiredProjectile.initialPosition);
							value = Vector3Ex.Distance2D(info.HitPositionWorld, value3.FiredProjectile.initialPosition);
							value3.Hit = info.DidHit;
							if (eventRecord != null && value3.FiredProjectile.updates.Count > 0)
							{
								eventRecord.AddObject("projectile_updates", value3.FiredProjectile.updates);
							}
							if (eventRecord != null && value3.FiredProjectile.simulatedPositions.Count > 0)
							{
								eventRecord.AddObject("simulated_position", value3.FiredProjectile.simulatedPositions);
							}
							if (eventRecord != null)
							{
								eventRecord.AddField("partial_time", value3.FiredProjectile.partialTime);
								eventRecord.AddField("desync_lifetime", value3.FiredProjectile.desyncLifeTime);
								eventRecord.AddField("startpoint_mismatch", value3.FiredProjectile.startPointMismatch);
								eventRecord.AddField("endpoint_mismatch", value3.FiredProjectile.endPointMismatch);
								eventRecord.AddField("entity_distance", value3.FiredProjectile.entityDistance);
								eventRecord.AddField("position_offset", value3.FiredProjectile.initialPositionOffset);
							}
							trackedProjectiles.Remove(key);
							Pool.Free(ref value3);
						}
					}
					else
					{
						value = Vector3.Distance(info.HitNormalWorld, initiatorPlayer.eyes.position);
						value2 = Vector3Ex.Distance2D(info.HitNormalWorld, initiatorPlayer.eyes.position);
					}
				}
				if (eventRecord == null)
				{
					eventRecord = EventRecord.New("entity_damage");
				}
				eventRecord.AddField("is_headshot", info.isHeadshot).AddField("victim", info.HitEntity).AddField("damage", info.damageTypes.Total())
					.AddField("damage_type", info.damageTypes.GetMajorityDamageType().ToString())
					.AddField("pos_world", info.HitPositionWorld)
					.AddField("pos_local", info.HitPositionLocal)
					.AddField("point_start", info.PointStart)
					.AddField("point_end", info.PointEnd)
					.AddField("normal_world", info.HitNormalWorld)
					.AddField("normal_local", info.HitNormalLocal)
					.AddField("distance_cl", info.ProjectileDistance)
					.AddField("distance", value)
					.AddField("distance_2d", value2);
				if (info.HitEntity != null && info.HitEntity.model != null)
				{
					eventRecord.AddField("pos_local_model", info.HitEntity.model.transform.InverseTransformPoint(info.HitPositionWorld));
				}
				if (!info.IsProjectile())
				{
					eventRecord.AddField("weapon", info.Weapon);
					eventRecord.AddField("attacker", info.Initiator);
				}
				if (info.HitBone != 0)
				{
					eventRecord.AddField("bone", info.HitBone).AddField("bone_name", info.boneName).AddField("hit_area", (int)info.boneArea);
				}
				if (info.ProjectileID != 0)
				{
					eventRecord.AddField("projectile_integrity", info.ProjectileIntegrity).AddField("projectile_hits", info.ProjectileHits).AddField("trajectory_mismatch", info.ProjectileTrajectoryMismatch)
						.AddField("travel_time", info.ProjectileTravelTime)
						.AddField("projectile_velocity", info.ProjectileVelocity)
						.AddField("projectile_prefab", info.ProjectilePrefab.name);
				}
				if (initiatorPlayer != null && !info.IsProjectile())
				{
					eventRecord.AddField("attacker_eye_pos", initiatorPlayer.eyes.position);
					eventRecord.AddField("attacker_eye_dir", initiatorPlayer.eyes.BodyForward());
					if (initiatorPlayer.GetType() == typeof(BasePlayer))
					{
						eventRecord.AddField("attacker_life", initiatorPlayer.respawnId);
					}
				}
				else if (initiatorPlayer != null)
				{
					eventRecord.AddObject("attacker_worn", initiatorPlayer.inventory.containerWear.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddObject("attacker_hotbar", initiatorPlayer.inventory.containerBelt.itemList.Select((Item x) => new SimpleItemAmount(x)));
				}
				if (basePlayer != null)
				{
					eventRecord.AddField("victim_life", basePlayer.respawnId);
					eventRecord.AddObject("victim_worn", basePlayer.inventory.containerWear.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddObject("victim_hotbar", basePlayer.inventory.containerBelt.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddField("victim_view_dir", basePlayer.tickViewAngles);
					eventRecord.AddField("victim_eye_pos", basePlayer.eyes.position);
					eventRecord.AddField("victim_eye_dir", basePlayer.eyes.BodyForward());
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerRespawned(BasePlayer player, BaseEntity targetEntity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_respawn").AddField("player", player).AddField("bag", targetEntity)
					.AddField("life_id", player.respawnId));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExplosiveLaunched(BasePlayer player, BaseEntity explosive, BaseEntity launcher = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("explosive_launch").AddField("player", player).AddField("explosive", explosive)
					.AddField("explosive_velocity", explosive.GetWorldVelocity())
					.AddField("explosive_direction", explosive.GetWorldVelocity().normalized);
				if (launcher != null)
				{
					eventRecord.AddField("launcher", launcher);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExplosion(TimedExplosive explosive)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("explosion").AddField("entity", explosive));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemDespawn(BaseEntity itemContainer, Item item, int dropReason, ulong userId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("item_despawn").AddField("entity", itemContainer).AddField("item", item)
					.AddField("drop_reason", dropReason);
				if (userId != 0L)
				{
					eventRecord.AddField("player_userid", userId);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemDropped(BasePlayer player, WorldItem entity, DroppedItem.DropReasonEnum dropReason)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_drop").AddField("player", player).AddField("entity", entity)
					.AddField("item", entity.GetItem())
					.AddField("drop_reason", (int)dropReason));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemPickup(BasePlayer player, WorldItem entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_pickup").AddField("player", player).AddField("entity", entity)
					.AddField("item", entity.GetItem()));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerConnected(Connection connection)
		{
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(connection.userid);
				SubmitPoint(EventRecord.New("player_connect").AddField("player_userid", userWipeId).AddField("steam_id", connection.userid)
					.AddField("username", connection.username)
					.AddField("ip", connection.ipaddress));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerDisconnected(Connection connection, string reason)
		{
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(connection.userid);
				SubmitPoint(EventRecord.New("player_disconnect").AddField("player_userid", userWipeId).AddField("steam_id", connection.userid)
					.AddField("username", connection.username)
					.AddField("reason", reason));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityPickedUp(BasePlayer player, BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_pickup").AddField("player", player).AddField("entity", entity));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnChatMessage(BasePlayer player, string message, int channel)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("chat").AddField("player", player).AddField("message", message)
					.AddField("channel", channel));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnVendingMachineOrderChanged(BasePlayer player, VendingMachine vendingMachine, int sellItemId, int sellAmount, bool sellingBp, int buyItemId, int buyAmount, bool buyingBp, bool added)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellItemId);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(buyItemId);
				SubmitPoint(EventRecord.New("vending_changed").AddField("player", player).AddField("entity", vendingMachine)
					.AddField("sell_item", itemDefinition.shortname)
					.AddField("sell_amount", sellAmount)
					.AddField("buy_item", itemDefinition2.shortname)
					.AddField("buy_amount", buyAmount)
					.AddField("is_selling_bp", sellingBp)
					.AddField("is_buying_bp", buyingBp)
					.AddField("change", added ? "added" : "removed"));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBuyFromVendingMachine(BasePlayer player, VendingMachine vendingMachine, int sellItemId, int sellAmount, bool sellingBp, int buyItemId, int buyAmount, bool buyingBp, int numberOfTransactions, float discount, BaseEntity drone = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellItemId);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(buyItemId);
				SubmitPoint(EventRecord.New("vending_sale").AddField("player", player).AddField("entity", vendingMachine)
					.AddField("sell_item", itemDefinition.shortname)
					.AddField("sell_amount", sellAmount)
					.AddField("buy_item", itemDefinition2.shortname)
					.AddField("buy_amount", buyAmount)
					.AddField("transactions", numberOfTransactions)
					.AddField("is_selling_bp", sellingBp)
					.AddField("is_buying_bp", buyingBp)
					.AddField("drone_terminal", drone)
					.AddField("discount", discount));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnNPCVendor(BasePlayer player, NPCTalking vendor, int scrapCost, string action)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("npc_vendor").AddField("player", player).AddField("vendor", vendor)
					.AddField("scrap_amount", scrapCost)
					.AddField("action", action));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void LogItemsLooted(BasePlayer looter, BaseEntity entity, ItemContainer container, AttackEntity tool = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (entity == null || container == null)
				{
					return;
				}
				foreach (Item item in container.itemList)
				{
					if (item != null)
					{
						string shortname = item.info.shortname;
						int amount = item.amount;
						ulong steamId = looter?.userID ?? ((EncryptedValue<ulong>)0uL);
						LogResource(ResourceMode.Produced, "loot", shortname, amount, entity, tool, safezone: false, null, steamId, null, null, null, 0uL);
					}
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void LogResource(ResourceMode mode, string category, string itemName, int amount, BaseEntity sourceEntity = null, AttackEntity tool = null, bool safezone = false, BaseEntity workbench = null, ulong steamId = 0uL, string sourceEntityPrefab = null, Item sourceItem = null, string targetItem = null, ulong skinId = 0uL)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				string value = ((mode == ResourceMode.Produced) ? "Produced" : "Consumed");
				EventRecord eventRecord = EventRecord.New("item_event").AddField("item_mode", value).AddField("category", category)
					.AddField("item_name", itemName)
					.AddField("amount", amount);
				if (sourceEntity != null)
				{
					eventRecord.AddField("entity", sourceEntity);
					string biome = GetBiome(sourceEntity.transform.position);
					if (biome != null)
					{
						eventRecord.AddField("biome", biome);
					}
					if (IsOcean(sourceEntity.transform.position))
					{
						eventRecord.AddField("ocean", value: true);
					}
					string monument = GetMonument(sourceEntity);
					if (monument != null)
					{
						eventRecord.AddField("monument", monument);
					}
				}
				if (sourceEntityPrefab != null)
				{
					eventRecord.AddField("entity_prefab", sourceEntityPrefab);
				}
				if (tool != null)
				{
					eventRecord.AddField("tool", tool);
				}
				if (safezone)
				{
					eventRecord.AddField("safezone", value: true);
				}
				if (workbench != null)
				{
					eventRecord.AddField("workbench", workbench);
				}
				if (sourceEntity is GrowableEntity plant)
				{
					eventRecord.AddField("genes", GetGenesAsString(plant));
				}
				if (sourceItem != null)
				{
					eventRecord.AddField("source_item", sourceItem.info.shortname);
				}
				if (targetItem != null)
				{
					eventRecord.AddField("target_item", targetItem);
				}
				if (steamId != 0L)
				{
					string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(steamId);
					eventRecord.AddField("player_userid", userWipeId);
					eventRecord.AddField("player_steamid", steamId);
				}
				if (skinId != 0L)
				{
					eventRecord.AddField("item_skin_id", skinId);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSkinChanged(BasePlayer player, RepairBench repairBench, Item item, ulong workshopId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_skinned").AddField("player", player).AddField("entity", repairBench)
					.AddField("item", item)
					.AddField("new_skin", workshopId));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntitySkinChanged(BasePlayer player, BaseNetworkable entity, int newSkin)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_skinned").AddField("player", player).AddField("entity", entity)
					.AddField("new_skin", newSkin));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemRepaired(BasePlayer player, BaseEntity repairBench, Item itemToRepair, float conditionBefore, float maxConditionBefore)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("item_repair").AddField("player", player).AddField("entity", repairBench)
					.AddField("item", itemToRepair)
					.AddField("old_condition", conditionBefore)
					.AddField("old_max_condition", maxConditionBefore)
					.AddField("max_condition", itemToRepair.maxConditionNormalized));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityRepaired(BasePlayer player, BaseEntity entity, float healthBefore, float healthAfter)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("entity_repair").AddField("player", player).AddField("entity", entity)
					.AddField("healing", healthAfter - healthBefore)
					.AddField("health_before", healthBefore)
					.AddField("health_after", healthAfter));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBuildingBlockUpgraded(BasePlayer player, BuildingBlock buildingBlock, BuildingGrade.Enum targetGrade, uint targetColor, ulong targetSkin, bool includeColour)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("block_upgrade").AddField("player", player).AddField("entity", buildingBlock)
					.AddField("old_grade", (int)buildingBlock.grade)
					.AddField("new_grade", (int)targetGrade)
					.AddField("biome", GetBiome(buildingBlock.transform.position))
					.AddField("skin_old", buildingBlock.skinID)
					.AddField("skin", targetSkin);
				if (includeColour)
				{
					eventRecord.AddField("color", targetColor);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBuildingBlockDemolished(BasePlayer player, StabilityEntity buildingBlock)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("block_demolish").AddField("player", player).AddField("entity", buildingBlock));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerInitializedWipeId(ulong userId, string wipeId)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_wipe_id_set").AddField("user_id", userId).AddField("player_wipe_id", wipeId));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFreeUnderwaterCrate(BasePlayer player, FreeableLootContainer freeableLootContainer)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("crate_untied").AddField("player", player).AddField("entity", freeableLootContainer));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnVehiclePurchased(BasePlayer player, BaseEntity vehicle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("vehicle_purchase").AddField("player", player).AddField("entity", vehicle)
					.AddField("price", vehicle));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnMissionComplete(BasePlayer player, BaseMission mission, BaseMission.MissionFailReason? failReason = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("mission_complete").AddField("player", player).AddField("mission", mission.shortname);
				if (failReason.HasValue)
				{
					eventRecord.AddField("mission_succeed", value: false).AddField("fail_reason", failReason.Value.ToString());
				}
				else
				{
					eventRecord.AddField("mission_succeed", value: true);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnGamblingResult(BasePlayer player, BaseEntity entity, int scrapPaid, int scrapRecieved, Guid? gambleGroupId = null)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("gambing").AddField("player", player).AddField("entity", entity)
					.AddField("scrap_input", scrapPaid)
					.AddField("scrap_output", scrapRecieved);
				if (gambleGroupId.HasValue)
				{
					eventRecord.AddField("gamble_grouping", gambleGroupId.Value);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerPinged(BasePlayer player, BasePlayer.PingType type, bool wasViaWheel)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_pinged").AddField("player", player).AddField("pingType", (int)type)
					.AddField("viaWheel", wasViaWheel));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBagUnclaimed(BasePlayer player, SleepingBag bag)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("bag_unclaim").AddField("player", player).AddField("entity", bag));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSteamAuth(ulong userId, ulong ownerUserId, string authResponse)
		{
			try
			{
				SubmitPoint(EventRecord.New("steam_auth").AddField("user", userId).AddField("owner", ownerUserId)
					.AddField("response", authResponse)
					.AddField("server_port", global::Network.Net.sv.port)
					.AddField("network_mode", global::Network.Net.sv.ProtocolId)
					.AddField("player_count", BasePlayer.activePlayerList.Count)
					.AddField("max_players", ConVar.Server.maxplayers)
					.AddField("hostname", ConVar.Server.hostname)
					.AddField("secure", global::Network.Net.sv.secure));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityColorChanged(BasePlayer player, BaseEntity entity, uint oldColor, uint newColor)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_pinged").AddField("player", player).AddField("entity", entity)
					.AddField("color_old", oldColor)
					.AddField("color_new", newColor)
					.AddField("biome", GetBiome(entity.transform.position)));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBurstModeToggled(BasePlayer player, BaseProjectile gun, bool state)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("burst_toggle").AddField("player", player).AddField("weapon", gun)
					.AddField("enabled", state));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnParachuteUsed(BasePlayer player, float distanceTravelled, float deployHeight, float timeInAir)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("parachute_used").AddField("player", player).AddField("distanceTravelled", distanceTravelled)
					.AddField("deployHeight", deployHeight)
					.AddField("timeInAir", timeInAir));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTutorialStarted(BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("tutorial_started").AddField("player", player));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTutorialCompleted(BasePlayer player, float timeElapsed)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("tutorial_completed").AddField("player", player).AddLegacyTimespan("duration", TimeSpan.FromSeconds(timeElapsed)));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTutorialQuit(BasePlayer player, string activeMissionName)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("tutorial_quit").AddField("player", player).AddField("activeMissionName", activeMissionName));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBaseInteract(BasePlayer player, BaseEntity entity)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("base_interaction").AddField("player", player).AddField("entity", entity));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerDeath(BasePlayer player, BasePlayer killer)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_death").AddField("player", player).AddField("killer", killer));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCarShredded(MagnetLiftable car, List<Item> produced)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("car_shredded").AddField("player", car.associatedPlayer).AddField("car", car.GetBaseEntity());
				foreach (Item item in produced)
				{
					eventRecord.AddField("item_" + item.info.shortname, item);
				}
				SubmitPoint(eventRecord);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerTick(BasePlayer player, Vector3 pos, in BasePlayer.CachedState tickState, bool isMounted)
		{
			if (GameplayTickAnalyticsConVar)
			{
				EventRecord record = EventRecord.New("player_tick").AddField("player_steamid", player.UserIDString).AddField("modelstate", (player.modelStateTick ?? player.modelState).flags)
					.AddField("heldentity", (player.GetHeldEntity() != null) ? player.GetHeldEntity().ShortPrefabName : "")
					.AddField("pitch", player.tickViewAngles.x)
					.AddField("yaw", player.tickViewAngles.y)
					.AddField("pos_x", pos.x)
					.AddField("pos_y", pos.y)
					.AddField("pos_z", pos.z)
					.AddField("eye_pos_x", tickState.EyePos.x)
					.AddField("eye_pos_y", tickState.EyePos.y)
					.AddField("eye_pos_z", tickState.EyePos.z)
					.AddField("mouse_delta_x", player.tickMouseDelta.x)
					.AddField("mouse_delta_y", player.tickMouseDelta.y)
					.AddField("mouse_delta_z", player.tickMouseDelta.z)
					.AddField("parented", player.HasParent())
					.AddField("mounted", isMounted)
					.AddField("admin", player.IsAdmin || player.IsDeveloper)
					.AddField("water_factor", tickState.WaterFactor)
					.AddField("Timestamp", DateTime.UtcNow);
				TickLogging.TickTable.Append(record);
			}
		}

		public static void OnWallpaperPlaced(BasePlayer player, BuildingBlock buildingBlock, ulong skinID, int side, bool reskin)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("wallpaper_placed").AddField("player", player).AddField("buildingBlock", buildingBlock)
					.AddField("skin", skinID)
					.AddField("side", side)
					.AddField("reskin", reskin));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnStartFish(BasePlayer player, Item lure, Vector3 targetPos)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fishing_start").AddField("player", player).AddField("lure", lure)
					.AddField("target_pos", targetPos));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFailedFish(BasePlayer player, BaseFishingRod.FailReason reason)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fishing_failed").AddField("player", player).AddField("fail_reason", (int)reason));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCaughtFish(BasePlayer player, Item item)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("fishing_caught").AddField("player", player).AddField("item", item));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerChangeInjureState(BasePlayer player, BasePlayer.InjureState oldState, BasePlayer.InjureState newState)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("injure_state").AddField("player", player).AddField("old_state", (int)oldState)
					.AddField("new_state", (int)newState));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerLifeStoryEnd(BasePlayer player, PlayerLifeStory lifeStory)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("life_story_end").AddField("player", player).AddField("time_born", lifeStory.timeBorn)
					.AddField("time_died", lifeStory.timeDied)
					.AddField("meters_walked", lifeStory.metersWalked)
					.AddField("meters_ran", lifeStory.metersRun)
					.AddField("seconds_alive", lifeStory.secondsAlive)
					.AddField("seconds_in_base", lifeStory.secondsInBase)
					.AddField("seconds_in_wilderness", lifeStory.secondsWilderness)
					.AddField("seconds_in_monument", lifeStory.secondsInMonument)
					.AddField("seconds_driving", lifeStory.secondsDriving)
					.AddField("seconds_flying", lifeStory.secondsFlying)
					.AddField("seconds_boating", lifeStory.secondsBoating)
					.AddField("seconds_sleeping", lifeStory.secondsSleeping)
					.AddField("seconds_swimming", lifeStory.secondsSwimming)
					.AddField("total_damage_taken", lifeStory.totalDamageTaken)
					.AddField("total_healed", lifeStory.totalHealing)
					.AddField("killed_players", lifeStory.killedPlayers)
					.AddField("killed_animals", lifeStory.killedAnimals)
					.AddField("killed_scientists", lifeStory.killedScientists)
					.AddObject("death_info", lifeStory.deathInfo)
					.AddObject("weapon_stats", lifeStory.weaponStats));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnServerRPC(BasePlayer player, uint nameID, byte[] data, int length)
		{
			if (!GameplayAnalytics || !GameplayRpcAnalyticsConVar)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("server_rpc").AddField("player", player).AddField("rpc", StringPool.Get(nameID))
					.AddField("data", data)
					.AddField("length", length));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnClientRanCommand(Connection connection, string command)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("admin_command").AddField("steam_id", connection.userid).AddField("owner_id", connection.ownerid)
					.AddField("ip", connection.ipaddress)
					.AddField("auth_level", connection.authLevel)
					.AddField("connected_time", connection.connectionTime)
					.AddField("username", connection.username)
					.AddField("command", command));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPuzzleReset(PuzzleReset reset, float timeTaken, float timeSpentBlocked, float timeBlockedRadiation, float timeBeforeLooted)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				if (timeSpentBlocked == 0f || !reset.playersBlockReset || reset.timeBetweenResets <= 0f || reset.timeBetweenResets >= 100000f)
				{
					return;
				}
				List<SpawnGroup> spawnGroups = reset.GetSpawnGroups();
				if (spawnGroups.Count != 0)
				{
					string value = string.Join("|", from x in spawnGroups
						where x != null
						select x.name into x
						orderby x
						select x);
					EventRecord eventRecord = EventRecord.New("puzzle_reset").AddField("time_blocked_total", timeSpentBlocked).AddField("time_blocked_radiation", timeBlockedRadiation)
						.AddField("time_intended", reset.timeBetweenResets)
						.AddField("time_taken", timeTaken)
						.AddField("time_until_looted", timeBeforeLooted)
						.AddField("ai_zone_enabled", reset.CheckSleepingAIZForPlayers)
						.AddField("player_radius", reset.playerDetectionRadius)
						.AddField("ignore_above_ground", reset.ignoreAboveGroundPlayers)
						.AddField("scales_with_population", reset.scaleWithServerPopulation)
						.AddField("spawn_groups", value);
					AIInformationZone aIZone = reset.GetAIZone();
					if (aIZone != null)
					{
						eventRecord.AddField("zone_size", aIZone.bounds.size);
						eventRecord.AddField("zone_size_magnitude", aIZone.bounds.size.magnitude);
					}
					MonumentInfo monumentInfo = (from x in spawnGroups
						where x != null
						select x.GetComponentInParent<MonumentInfo>()).FirstOrDefault((MonumentInfo x) => x != null);
					if (monumentInfo != null)
					{
						eventRecord.AddField("monument", monumentInfo.name);
					}
					SubmitPoint(eventRecord);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnDeepSeaTraverse(BasePlayer entity, bool entering, float timeToWipe)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("deep_sea_traverse").AddField("player", entity).AddField("entering", entering)
					.AddField("time_to_wipe", timeToWipe));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnDeepSeaToggled(bool toggle)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("deep_sea_toggle").AddField("open", toggle));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerBoatFinish(BasePlayer player, int blockCount, int deployableCount)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("player_boat_finish").AddField("player", player).AddField("block_count", blockCount)
					.AddField("deployable_count", deployableCount));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnApartmentCheckIn(BasePlayer entity, ApartmentRoom room, int scrapCost)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("apartment_checkin").AddField("player_steamid", entity.UserIDString).AddField("apartment_number", room.RoomNumber)
					.AddField("apartment_size", (int)room.Size)
					.AddField("apartment_scrap_cost", scrapCost));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnApartmentCheckOut(BasePlayer entity, ApartmentRoom room)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("apartment_checkout").AddField("player_steamid", entity.UserIDString).AddObject("apartment_owners", room.Owners)
					.AddField("apartment_number", room.RoomNumber)
					.AddField("apartment_size", (int)room.Size));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnApartmentUpgrade(BasePlayer entity, ApartmentRoom oldRoom, ApartmentRoom newRoom, int scrapCost)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("apartment_upgrade").AddField("player_steamid", entity.UserIDString).AddField("old_apartment_number", oldRoom.RoomNumber)
					.AddField("new_apartment_number", newRoom.RoomNumber)
					.AddField("old_apartment_size", (int)oldRoom.Size)
					.AddField("new_apartment_size", (int)newRoom.Size)
					.AddField("upgrade_scrap_cost", scrapCost));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnApartmentBreakIn(BasePlayer entity, ApartmentRoom room)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("apartment_breakin").AddField("player_steamid", entity.UserIDString).AddObject("apartment_owners", room.Owners)
					.AddField("apartment_number", room.RoomNumber)
					.AddField("apartment_size", (int)room.Size));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnShopOpened(RentableShop shop, int openScrapCost, int upkeepScrapCost)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("shop_opened").AddField("player_steamid", shop.ShopOwnerId).AddField("shop_number", shop.ShopNumberId)
					.AddField("shop_open_scrap_cost", openScrapCost)
					.AddField("shop_upkeep_scrap_cost", upkeepScrapCost));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnShopClosed(RentableShop shop)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("shop_closed").AddField("player_steamid", shop.ShopOwnerId).AddField("shop_number", shop.ShopNumberId));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnShopBreakIn(BasePlayer entity, RentableShop shop)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("shop_breakin").AddField("player_steamid", entity.UserIDString).AddField("shop_owner", shop.ShopOwnerId)
					.AddField("shop_number", shop.ShopNumberId));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnClanCreated(ulong playerSteamId, IClan clan)
		{
			OnClanUpdate("clan_created", playerSteamId, playerSteamId, clan);
		}

		public static void OnClanDisbanded(ulong playerSteamId, IClan clan)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			using PooledList<ulong> pooledList = Pool.Get<PooledList<ulong>>();
			try
			{
				foreach (ClanMember member in clan.Members)
				{
					pooledList.Add(member.SteamId);
				}
				SubmitPoint(EventRecord.New("clan_disbanded").AddField("player_steamid", playerSteamId).AddObject("impacted_steamids", pooledList)
					.AddField("clan_id", clan.ClanId)
					.AddField("clan_name", clan.Name));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnClanMemberAdded(ulong playerSteamId, IClan clan)
		{
			OnClanUpdate("clan_member_added", playerSteamId, playerSteamId, clan);
		}

		public static void OnClanMemberRemoved(ulong impactedSteamId, ulong playerSteamId, IClan clan)
		{
			if (clan.Members.Count == 0 || (clan.Members.Count == 1 && clan.Members[0].SteamId == impactedSteamId))
			{
				OnClanDisbanded(playerSteamId, clan);
			}
			else
			{
				OnClanUpdate("clan_member_removed", playerSteamId, impactedSteamId, clan);
			}
		}

		private static void OnClanUpdate(string eventId, ulong playerSteamId, ulong impactedSteamId, IClan clan)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			using PooledList<ulong> pooledList = Pool.Get<PooledList<ulong>>();
			try
			{
				foreach (ClanMember member in clan.Members)
				{
					pooledList.Add(member.SteamId);
				}
				SubmitPoint(EventRecord.New(eventId).AddField("player_steamid", playerSteamId).AddField("impacted_steamid", impactedSteamId)
					.AddField("clan_id", clan.ClanId)
					.AddField("clan_name", clan.Name)
					.AddObject("clan_members", pooledList));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnClanScoreEvent(IClan clan, ClanScoreEvent entry)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("clan_score_event").AddField("player_steamid", entry.SteamId.ToString()).AddField("clan_id", clan.ClanId)
					.AddField("event_type", entry.Type.ToString())
					.AddField("event_score", entry.Score)
					.AddField("event_multiplier", entry.Multiplier));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPowerGridStageChanged(int prevStage, int newStage)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("power_grid_stage_changed").AddField("previous_stage", prevStage).AddField("new_stage", newStage));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPowerGridFuseInserted(BasePlayer player, Item fuse, int fuses)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("power_grid_fuse_inserted").AddField("player_steamid", player).AddField("fuses_inserted", fuses));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCarOpenEditing(BasePlayer player, ModularCarGarage garage)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("car_open_editing").AddField("player_steamid", player.UserIDString).AddField("garage", garage));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnAirfieldChinookCall()
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("airfield_chinook_call"));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSatelliteLockTrajectory(BasePlayer player, Vector3 targetPos)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("satellite_lock_trajectory").AddField("player_steamid", player.UserIDString).AddField("target_position", targetPos));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSatelliteCrashed(bool crashed, Vector3 crashPos)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("satellite_crashed").AddField("crashed", crashed).AddField("crash_position", crashPos));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnIOEntityConnected(BasePlayer player, IOEntity inputIOEntity, IOEntity outputIOEntity, WireTool.WireColour wireColour)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("io_entity_connected").AddField("player", player).AddField("input_io_entity", inputIOEntity)
					.AddField("output_io_entity", outputIOEntity)
					.AddField("wire_colour", (int)wireColour));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnWaterTreatmentPlantEnabled(BasePlayer player, Item item)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("water_treatment_plant_enabled").AddField("player", player).AddField("item", item));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnOilRigFuelSwitchStarted(BasePlayer player)
		{
			if (!GameplayAnalytics)
			{
				return;
			}
			try
			{
				SubmitPoint(EventRecord.New("oil_rig_fuel_switch_started").AddField("player", player));
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}

	private const int ShutdownTimeoutMs = 10000;

	public static AnalyticsManager Manager = new AnalyticsManager();

	private static bool started;

	private const string DryRunDocs = "Executes entire flow without actually sending out anything";

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true, Help = "Executes entire flow without actually sending out anything")]
	public static bool DryRun;

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static bool Log;

	public static string ClientAnalyticsUrl { get; set; } = "https://rust-api.facepunch.com/api/public/analytics/rust/client";


	[RconVar(Name = "server_analytics_url")]
	public static string ServerAnalyticsUrl { get; set; } = "https://rust-api.facepunch.com/api/public/analytics/rust/server";


	[RconVar(Name = "analytics_header", Saved = true, Help = "Header key of secret when uploading analytics")]
	public static string AnalyticsHeader { get; set; } = "X-API-KEY";


	[RconVar(Name = "analytics_secret", Saved = true, Help = "Header secret value when uploading analytics")]
	public static string AnalyticsSecret { get; set; } = "";


	public static string AnalyticsPublicKey { get; set; } = "pub878ABLezSB6onshSwBCRGYDCpEI";


	[RconVar(Name = "analytics_bulk_upload_url", Saved = true, Help = "Azure blob container url + SAS token, enables a more efficient upload method")]
	public static string BulkUploadConnectionString { get; set; }

	[RconVar(Name = "analytics_bulk_container_url", Saved = true, Help = "Azure blob container url for use with client secret authentication")]
	public static string BulkContainerUrl { get; set; }

	[RconVar(Name = "azure_tenant_id", Saved = true, Help = "Azure tenant id for authentication")]
	public static string AzureTenantId { get; set; }

	[RconVar(Name = "azure_client_id", Saved = true, Help = "Azure client id for authentication")]
	public static string AzureClientId { get; set; }

	[RconVar(Name = "azure_client_secret", Saved = true, Help = "Azure client secret for authentication")]
	public static string AzureClientSecret { get; set; }

	[RconVar(Name = "performance_analytics", Saved = true, Help = "Toggle to turn off server performance collection")]
	public static bool ServerPerformanceConVar { get; set; } = true;


	[RconVar(Name = "gameplay_analytics", Saved = true, Help = "Toggle whether gameplay analytics is collected")]
	public static bool GameplayAnalyticsConVar { get; set; }

	[RconVar(Name = "gameplay_tick_analytics", Saved = true, Help = "Toggle whether gameplay tick analytics is collected")]
	public static bool GameplayTickAnalyticsConVar { get; set; }

	[RconVar(Name = "gameplay_rpc_analytics", Saved = true, Help = "Toggle whether gameplay rpc logging is collected")]
	public static bool GameplayRpcAnalyticsConVar { get; set; } = true;


	public static void StartForServer()
	{
		if (!started)
		{
			Manager.StartThead();
			started = true;
		}
		Azure.Initialize();
	}

	public static void ShutdownForServer()
	{
		if (started)
		{
			Manager.Shutdown(10000);
		}
		started = false;
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void Stats(ConsoleSystem.Arg arg)
	{
		AnalyticsManager.TelemStats telemStats = Manager.GatherStats();
		string empty = string.Empty;
		empty += $"Total Events: {telemStats.SerializedCount}\n";
		empty += $"Total Bytes: {telemStats.SerializedSize}\n";
		empty += $"Uploaded Bytes: {telemStats.UploadedSize}\n";
		empty += $"Queue Count: {telemStats.QueueCount}\n";
		empty += $"Max Queue Count: {telemStats.MaxQueueCount}";
		arg.ReplyWith(empty);
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void TableStats(ConsoleSystem.Arg arg)
	{
		TextTable obj = Pool.Get<TextTable>();
		obj.ResizeColumns(8);
		obj.AddColumn("Table Name");
		obj.AddColumn("Uploader Name");
		obj.AddColumn("Compressed");
		obj.AddColumn("Upload interval");
		obj.AddColumn("Total Events");
		obj.AddColumn("Total Bytes");
		obj.AddColumn("Avg Event/s");
		obj.AddColumn("Avg Bytes/s");
		double totalSeconds = (DateTime.Now - Manager.StatsStartTime).TotalSeconds;
		ReadOnlySpan<AnalyticsManager.UploadingTable> tables = Manager.Tables;
		obj.ResizeRows(tables.Length);
		ReadOnlySpan<AnalyticsManager.UploadingTable> readOnlySpan = tables;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			AnalyticsManager.UploadingTable uploadingTable = readOnlySpan[i];
			AnalyticsTable table = uploadingTable.Table;
			AnalyticsManager.IUploader uploader = uploadingTable.Uploader.Resolve();
			AnalyticsManager.IAccumulator obj2 = uploader?.GetAccumulatorFor(table) ?? null;
			obj.AddValue(table.Name);
			obj.AddValue(uploader?.Name ?? "None");
			obj.AddValue(uploader?.IsCompressed ?? false);
			obj.AddValue(table.UploadInterval.ToString());
			int num = obj2?.ItemsWritten ?? 0;
			obj.AddValue(num);
			long num2 = obj2?.BytesWritten ?? 0;
			obj.AddValue(num2);
			float num3 = (float)((double)num / totalSeconds);
			obj.AddValue($"{num3:F2}");
			float num4 = (float)((double)num2 / totalSeconds);
			obj.AddValue($"{num4:F2}");
		}
		arg.ReplyWith(obj.ToString());
		Pool.Free(ref obj);
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void UploaderStats(ConsoleSystem.Arg arg)
	{
		TextTable obj = Pool.Get<TextTable>();
		obj.ResizeColumns(7);
		obj.AddColumn("Uploader Name");
		obj.AddColumn("Is Enabled");
		obj.AddColumn("Compressed");
		obj.AddColumn("Total Events");
		obj.AddColumn("Total Bytes");
		obj.AddColumn("Avg Event/s");
		obj.AddColumn("Avg Bytes/s");
		double totalSeconds = (DateTime.Now - Manager.StatsStartTime).TotalSeconds;
		ReadOnlySpan<AnalyticsManager.IUploader> uploaders = Manager.Uploaders;
		for (int i = 0; i < uploaders.Length; i++)
		{
			AnalyticsManager.IUploader uploader = uploaders[i];
			obj.AddValue(uploader.Name);
			obj.AddValue(uploader.Enabled);
			obj.AddValue(uploader.IsCompressed);
			int itemsSerialized = uploader.ItemsSerialized;
			obj.AddValue(itemsSerialized);
			long bytesSerialized = uploader.BytesSerialized;
			obj.AddValue(bytesSerialized);
			float num = (float)((double)itemsSerialized / totalSeconds);
			obj.AddValue($"{num:F2}");
			float num2 = (float)((double)bytesSerialized / totalSeconds);
			obj.AddValue($"{num2:F2}");
		}
		arg.ReplyWith(obj.ToString());
		Pool.Free(ref obj);
	}

	[ConsoleVar(Clientside = true, Serverside = true, ServerAdmin = true)]
	public static void ResetStats(ConsoleSystem.Arg arg)
	{
		Manager.ResetStats();
	}
}

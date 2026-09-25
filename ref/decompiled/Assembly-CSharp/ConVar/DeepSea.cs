using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facepunch;
using UnityEngine;

namespace ConVar;

public class DeepSea : ConsoleSystem
{
	[ClientVar(ClientAdmin = true, Help = "Scale multiplier for the main island billboard seen from the deep sea")]
	public static float mainisland_billboard_scale = 1f;

	[ClientVar(ClientAdmin = true, Help = "Scale multiplier for the deep sea island billboards")]
	public static float islands_billboard_scale = 1f;

	[ReplicatedVar]
	public static bool block_building = true;

	[ReplicatedVar(Saved = true, Help = "Applies the deep sea terrain config and foliage to the whole map, not just inside the deep sea")]
	public static bool terrain_everywhere = false;

	[ServerVar(Help = "When enabled, logs each deep sea portal spawn attempt to the console including whether it succeeded or failed")]
	public static bool debug_portal_spawnattempts = false;

	[ServerVar(Help = "Allow all vehicles to travel to the deep sea, instead of just the whitelisted vehicles")]
	public static bool allow_all_vehicles = false;

	[ServerVar(Help = "Allow players to swim to the deep sea")]
	public static bool allow_swimmers = false;

	[ServerVar(Help = "Distance in metres from the main island shore at which deep sea entrance portals are placed")]
	public static float island_portal_terrain_distance = 750f;

	private static bool _enabled = true;

	[ServerVar(Help = "(Generated) When enabled, outputs verbose deep sea system log messages (portal transitions, wipe events, entity moves) to the server log for debugging")]
	public static bool logs = false;

	private static float _entities_spawninterval = 5f;

	private static float _spawngroups_spawninterval = 0.15f;

	private static float _navmesh_spawninterval = 5f;

	[ServerVar(Help = "Population multiplier applied to the loot the deep sea spawns with. 1.0 = unchanged, 0.5 = half, 2.0 = double")]
	public static float loot_scale = 1f;

	[ServerVar(Help = "Population multiplier applied to the loot the deep sea will respawn over time. 1.0 = unchanged, 0.5 = half, 2.0 = double")]
	public static float loot_respawn_scale = 1f;

	[ServerVar(Help = "Number of floating cities to spawn in the deep sea")]
	public static int floatingcity_count = 1;

	[ServerVar(Help = "Exclusion radius in metres around floating cities")]
	public static float floatingcity_radius = 500f;

	[ServerVar(Help = "Minimum distance in metres floating cities must be from the deep sea boundary edge when spawning")]
	public static float floatingcity_edgeMargin = 1500f;

	[ServerVar(Help = "Minimum distance in metres required between floating city")]
	public static float floatingcity_minDist = 1500f;

	[ServerVar(Help = "Number of islands to spawn in the deep sea zone")]
	public static int island_count = 6;

	[ServerVar(Help = "Exclusion radius in metres around islands")]
	public static float island_radius = 300f;

	[ServerVar(Help = "Minimum distance in metres islands must be from the deep sea boundary edge when spawning")]
	public static float island_edgeMargin = 750f;

	[ServerVar(Help = "Minimum distance in metres required between islands")]
	public static float island_minDist = 600f;

	[ServerVar(Help = "Number of ghost ship to spawn in the deep sea zone")]
	public static int ghostship_count = 4;

	[ServerVar(Help = "Exclusion radius in metres around ghost ship")]
	public static float ghostship_radius = 80f;

	[ServerVar(Help = "Minimum distance in metres ghost ships must be from the deep sea boundary edge when spawning")]
	public static float ghostship_edgeMargin = 450f;

	[ServerVar(Help = "Minimum distance in metres required between ghost ships")]
	public static float ghostship_minDist = 400f;

	[ServerVar(Help = "Number of RHIB boat groups to spawn in the deep sea")]
	public static int rhib_count = 4;

	[ServerVar(Help = "Exclusion radius in metres around each RHIB group")]
	public static float rhib_radius = 10f;

	[ServerVar(Help = "Minimum distance in metres RHIB groups must be from the deep sea boundary edge when spawning")]
	public static float rhib_edgeMargin = 1150f;

	[ServerVar(Help = "Minimum distance in metres required between RHIB groups")]
	public static float rhib_minDist = 300f;

	[ServerVar(Help = "Number of hackable crates to spawn in the deep sea")]
	public static int hackablecrate_count = 1;

	[ServerVar(Help = "Should the deep sea open as soon as the server wiped?")]
	public static bool openOnServerWipe = false;

	[ServerVar(Help = "0 Random, 1 North, 2 East, 3 South, 4 West")]
	public static int forceEntrancePortalDirection = 0;

	[ServerVar(Help = "Duration in seconds of the deep sea wipe")]
	public static float wipeDuration = 10800f;

	[ServerVar(Help = "Minimum seconds before a deep sea re-opens after closing")]
	public static float wipeCooldownMin = 5400f;

	[ServerVar(Help = "Maximum seconds before a deep sea re-opens after closing")]
	public static float wipeCooldownMax = 5400f;

	[ReplicatedVar(Help = "Duration in seconds of the final wipe phase, bad weather kicking in etc")]
	public static float wipeEndPhaseDuration = 1800f;

	[ServerVar(Help = "Seconds before radiation starts to ramp in before the deep sea wipe")]
	public static float wipeRadiationPhaseDuration = 300f;

	[ReplicatedVar(Default = "True", Help = "Toggles the deep sea. Needs a server restart to take effect. Any saved deep sea entities will be destroyed at the next startup")]
	public static bool enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
		}
	}

	[ServerVar(Help = "When generating, the interval in seconds in between each entity spawn (island, ghost ship and floating city). Increase if you're experiencing lag when the deep sea is opening.")]
	public static float entities_spawninterval
	{
		get
		{
			return _entities_spawninterval;
		}
		set
		{
			_entities_spawninterval = value;
			if (PointEntity<DeepSeaManager>.ServerInstance != null)
			{
				PointEntity<DeepSeaManager>.ServerInstance.CacheWaitForSeconds();
			}
		}
	}

	[ServerVar(Help = "When generating, the interval in seconds in between each spawn groups fill (dwellings/crates/scientists on island, ghost ships). Increase if you're experiencing lag when the deep sea is opening.")]
	public static float spawngroups_spawninterval
	{
		get
		{
			return _spawngroups_spawninterval;
		}
		set
		{
			_spawngroups_spawninterval = value;
			if (PointEntity<DeepSeaManager>.ServerInstance != null)
			{
				PointEntity<DeepSeaManager>.ServerInstance.CacheWaitForSeconds();
			}
		}
	}

	[ServerVar(Help = "When generating, the interval in seconds in between each island navmesh bake. Increase if you're experiencing lag when the deep sea is opening.")]
	public static float navmesh_spawninterval
	{
		get
		{
			return _navmesh_spawninterval;
		}
		set
		{
			_navmesh_spawninterval = value;
			if (PointEntity<DeepSeaManager>.ServerInstance != null)
			{
				PointEntity<DeepSeaManager>.ServerInstance.CacheWaitForSeconds();
			}
		}
	}

	[ServerVar(Help = "Teleports the player (or their mounted vehicle) into the deep sea")]
	public static void enterdeepsea(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			arg.ReplyWith("Deep sea not active, run deepsea.createdeepsea first");
			return;
		}
		BasePlayer player = ArgEx.Player(arg);
		PointEntity<DeepSeaManager>.ServerInstance.MoveToDeepSea(GetEntityToTeleport(player));
	}

	[ServerVar(Help = "Teleports the player (or their mounted vehicle) back to the main island from the deep sea.")]
	public static void leavedeepsea(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		if (!DeepSeaManager.IsInsideDeepSea(basePlayer))
		{
			arg.ReplyWith("Not in the deep sea");
			return;
		}
		BaseEntity entityToTeleport = GetEntityToTeleport(basePlayer);
		if (entityToTeleport is BasePlayer && entityToTeleport.HasParent())
		{
			entityToTeleport.SetParent(null, worldPositionStays: true);
		}
		PointEntity<DeepSeaManager>.ServerInstance.MoveToMainIsland(entityToTeleport);
	}

	[ServerVar(Help = "Creates the deep sea manager entity on the server")]
	public static void createdeepsea(Arg arg)
	{
		if (!enabled)
		{
			arg.ReplyWith("Can't create deep sea, its disabled. Run deepsea.enabled true first");
			return;
		}
		if (PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			arg.ReplyWith("Killing existing deep sea manager");
			PointEntity<DeepSeaManager>.ServerInstance.Kill();
		}
		SingletonComponent<ServerMgr>.Instance.CreateDeepSea();
		arg.ReplyWith("Deep sea manager entity created");
	}

	[ServerVar(Help = "(Generated) Tests whether the calling player's current position is a valid portal spawn location; draws a green sphere if valid or red sphere if invalid with the given radius and duration in seconds")]
	public static void testportalspawnlocation(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			arg.ReplyWith("Deep sea not active, run deepsea.createdeepsea first");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		Vector3 position = basePlayer.transform.position;
		float @float = arg.GetFloat(0, 25f);
		float float2 = arg.GetFloat(1, 10f);
		bool flag = PointEntity<DeepSeaManager>.ServerInstance.IsValidPortalSpawnLocation(basePlayer.transform.position, @float, basePlayer);
		basePlayer.SendConsoleCommand(DDrawCommand.Sphere(position, float2, flag ? Color.green : Color.red, @float, distanceFade: false));
	}

	public static BaseEntity GetEntityToTeleport(BasePlayer player)
	{
		if (player.isMounted)
		{
			BaseVehicle mountedVehicle = player.GetMountedVehicle();
			if (mountedVehicle != null)
			{
				return mountedVehicle;
			}
		}
		BaseEntity parentEntity = player.GetParentEntity();
		if (parentEntity != null)
		{
			if (parentEntity is BaseVehicle)
			{
				return parentEntity;
			}
			if (parentEntity is SmallRamp || parentEntity is Plank || parentEntity is BaseLadder)
			{
				PlayerBoat playerBoat = parentEntity.GetParentEntity() as PlayerBoat;
				if (playerBoat != null)
				{
					return playerBoat;
				}
			}
		}
		return player;
	}

	[ServerVar(Help = "Prints what players on the player's boat will be teleported to the deep sea")]
	public static void printboatteleports(Arg arg)
	{
		BaseEntity entityToTeleport = GetEntityToTeleport(ArgEx.Player(arg));
		if (entityToTeleport == null || !(entityToTeleport is BaseBoat))
		{
			arg.ReplyWith("Not on a boat");
			return;
		}
		List<BasePlayer> list = new List<BasePlayer>();
		BaseVehicle.GetPassengersForVehicle(entityToTeleport, list);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Boat: {entityToTeleport.ShortPrefabName} ({entityToTeleport.net.ID})");
		stringBuilder.AppendLine($"Passengers: {list.Count}");
		foreach (BasePlayer item in list)
		{
			stringBuilder.AppendLine($"  {item.displayName} ({item.net.ID})");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Spawns a random deep sea island prefab at the player position")]
	public static void spawnislandhere(Arg arg)
	{
		Vector3 position = ArgEx.Player(arg).transform.position.WithY(0f);
		GameObjectRef random = ArrayEx.GetRandom(PointEntity<DeepSeaManager>.ServerInstance.islandRefs);
		PointEntity<DeepSeaManager>.ServerInstance.SpawnEntityAt(random.resourcePath, position, Quaternion.identity);
	}

	[ServerVar(Help = "Spawns a random ghost ship prefab at the player position")]
	public static void spawnghostshiphere(Arg arg)
	{
		Vector3 position = ArgEx.Player(arg).transform.position.WithY(0f);
		GameObjectRef random = ArrayEx.GetRandom(PointEntity<DeepSeaManager>.ServerInstance.ghostShipRefs);
		PointEntity<DeepSeaManager>.ServerInstance.SpawnEntityAt(random.resourcePath, position, Quaternion.identity);
	}

	[ServerVar(Help = "Spawns a random floating city prefab at the player position")]
	public static void spawnfloatingcityhere(Arg arg)
	{
		Vector3 position = ArgEx.Player(arg).transform.position.WithY(0f);
		GameObjectRef random = ArrayEx.GetRandom(PointEntity<DeepSeaManager>.ServerInstance.floatingCityRefs);
		PointEntity<DeepSeaManager>.ServerInstance.SpawnEntityAt(random.resourcePath, position, Quaternion.identity);
	}

	[ServerVar(Help = "Spawns a RHIB patrol boat group at the player position")]
	public static void spawnrhibgrouphere(Arg arg)
	{
		Vector3 vector = ArgEx.Player(arg).transform.position.WithY(0f);
		Vector3 normalized = new Vector3(vector.x, 0f, vector.y).normalized;
		if (PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			normalized = (DeepSeaManager.DeepSeaBounds.center - new Vector3(vector.x, 0f, vector.y)).normalized;
		}
		Quaternion rot = Quaternion.LookRotation(normalized);
		BoatAI.SpawnBoatGroup(new Vector2(vector.x, vector.z), rot, null, registerWithDeepSea: true);
	}

	[ServerVar(Help = "Initiates the deep sea closing sequence")]
	public static void close(Arg arg)
	{
		DeepSeaManager serverInstance = PointEntity<DeepSeaManager>.ServerInstance;
		if (serverInstance == null)
		{
			arg.ReplyWith("Deep sea not active, run deepsea.createdeepsea first");
		}
		else if (serverInstance != null)
		{
			if (serverInstance.IsBusy())
			{
				arg.ReplyWith("Deep sea is busy (opening or closing)");
				return;
			}
			if (!serverInstance.IsOpen())
			{
				arg.ReplyWith("Deep sea already closed");
				return;
			}
			serverInstance.CloseDeepSea();
			arg.ReplyWith("Closing deep sea...");
		}
	}

	[ServerVar(Help = "Initiates the deep sea opening sequence")]
	public static void open(Arg arg)
	{
		DeepSeaManager serverInstance = PointEntity<DeepSeaManager>.ServerInstance;
		if (serverInstance == null)
		{
			arg.ReplyWith("Deep sea not active, run deepsea.createdeepsea first");
		}
		else if (serverInstance != null)
		{
			if (serverInstance.IsBusy())
			{
				arg.ReplyWith("Deep sea is busy (opening or closing)");
				return;
			}
			if (serverInstance.IsOpen())
			{
				arg.ReplyWith("Deep sea already opened");
				return;
			}
			serverInstance.OpenDeepSea();
			arg.ReplyWith("Opening deep sea...");
		}
	}

	[ServerVar(Help = "Prints the current deep sea status")]
	public static void status(Arg arg)
	{
		DeepSeaManager serverInstance = PointEntity<DeepSeaManager>.ServerInstance;
		if (serverInstance == null)
		{
			arg.ReplyWith("Deep sea not active, run deepsea.createdeepsea first");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Open: {serverInstance.IsOpen()}");
		stringBuilder.AppendLine($"Busy: {serverInstance.IsBusy()}");
		stringBuilder.AppendLine($"TimeToNextOpening: {serverInstance.TimeToNextOpening}");
		stringBuilder.AppendLine($"TimeToWipe: {serverInstance.TimeToWipe}");
		stringBuilder.AppendLine($"Radiations: {serverInstance.GetWipeRadiationAmount()}");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine($"Portal Direction: {DeepSeaManager.GetEntrancePortalDirection()}");
		stringBuilder.AppendLine($"Entrance Portal Transform: {DeepSeaManager.PortalEntranceTransform}, {DeepSeaManager.PortalEntranceTransform?.position ?? Vector3.zero}");
		stringBuilder.AppendLine("Entrance Portal Bounds: pos:" + DeepSeaManager.PortalEntranceBounds.position.ToString() + ", extents:" + DeepSeaManager.PortalEntranceBounds.extents);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Portals:");
		foreach (DeepSeaPortal serverPortal in DeepSeaManager.ServerPortals)
		{
			stringBuilder.AppendLine($"    {serverPortal.PortalMode} {serverPortal.PortalDirection} {serverPortal.name} {serverPortal.transform.position}");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Sets the time in seconds until the deep sea wipe triggers")]
	public static void settimetowipe(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			float @float = arg.GetFloat(0);
			PointEntity<DeepSeaManager>.ServerInstance.SetTimeToWipe(@float);
			arg.ReplyWith($"Time to wipe set to {@float} seconds");
		}
	}

	[ServerVar(Help = "Sets the time in seconds until the deep sea re-opens after a wipe")]
	public static void settimetonextopening(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			float @float = arg.GetFloat(0);
			PointEntity<DeepSeaManager>.ServerInstance.SetTimeToNextOpening(@float);
			arg.ReplyWith($"Time to next opening set to {@float} seconds");
		}
	}

	[ServerVar(Help = "Prints the current time remaining until the deep sea wipes")]
	public static void printtimetowipe(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			arg.ReplyWith(PointEntity<DeepSeaManager>.ServerInstance.GetTimeToWipe());
		}
	}

	[ServerVar(Help = "Prints a breakdown of all entities currently in the deep sea")]
	public static void printentitycount(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			arg.ReplyWith("Deep sea not active");
			return;
		}
		HashSet<BaseEntity> obj = Facepunch.Pool.Get<HashSet<BaseEntity>>();
		PointEntity<DeepSeaManager>.ServerInstance.GetAllDeepSeaEntities(obj, skipParented: false);
		int count = obj.Count;
		int num = obj.OfType<BasePlayer>().Count((BasePlayer p) => !p.IsNpc);
		int num2 = obj.Count((BaseEntity e) => e.IsNpc);
		int num3 = obj.Count((BaseEntity x) => x is BaseVehicle);
		int num4 = obj.Count((BaseEntity x) => x is TreeEntity);
		int num5 = obj.Count((BaseEntity x) => x is MetalDetectorSource);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumns("Category", "Count");
		textTable.AddRow("Total Entities", count.ToString());
		textTable.AddRow("Islands", DeepSeaManager.ServerIslands.Count.ToString());
		textTable.AddRow("Ghost Ships", DeepSeaManager.ServerGhostShips.Count.ToString());
		textTable.AddRow("Floating Cities", DeepSeaManager.ServerFloatingCities.Count.ToString());
		textTable.AddRow("Trees", num4.ToString());
		textTable.AddRow("Players", num.ToString());
		textTable.AddRow("Metal Detector Sources", num5.ToString());
		IOrderedEnumerable<IGrouping<string, BaseEntity>> orderedEnumerable = from x in obj
			where x is BaseVehicle
			group x by (!string.IsNullOrEmpty(x.ShortPrefabName)) ? x.ShortPrefabName : "unknown" into g
			orderby g.Count() descending
			select g;
		textTable.AddRow("Vehicles", num3.ToString());
		foreach (IGrouping<string, BaseEntity> item in orderedEnumerable)
		{
			textTable.AddRow("  " + item.Key, item.Count().ToString());
		}
		IOrderedEnumerable<IGrouping<string, BaseEntity>> orderedEnumerable2 = from x in obj
			where x.IsNpc
			group x by (!string.IsNullOrEmpty(x.ShortPrefabName)) ? x.ShortPrefabName : "unknown" into g
			orderby g.Count() descending
			select g;
		textTable.AddRow("NPC", num2.ToString());
		foreach (IGrouping<string, BaseEntity> item2 in orderedEnumerable2)
		{
			textTable.AddRow("  " + item2.Key, item2.Count().ToString());
		}
		arg.ReplyWith(textTable.ToString());
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "Prints a breakdown of all loot containers, trees, and ore nodes currently in the deep sea")]
	public static void printloot(Arg arg)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			arg.ReplyWith("Deep sea not active");
			return;
		}
		HashSet<BaseEntity> obj = Facepunch.Pool.Get<HashSet<BaseEntity>>();
		PointEntity<DeepSeaManager>.ServerInstance.GetAllDeepSeaEntities(obj, skipParented: false);
		int num = obj.Count((BaseEntity x) => x is LootContainer);
		int num2 = obj.Count((BaseEntity x) => x is TreeEntity);
		int num3 = obj.Count((BaseEntity x) => x is OreResourceEntity);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumns("Category", "Count");
		IOrderedEnumerable<IGrouping<string, BaseEntity>> orderedEnumerable = from x in obj
			where x is LootContainer
			group x by (!string.IsNullOrEmpty(x.ShortPrefabName)) ? x.ShortPrefabName : "unknown" into g
			orderby g.Count() descending
			select g;
		textTable.AddRow("Loot Containers", num.ToString());
		foreach (IGrouping<string, BaseEntity> item in orderedEnumerable)
		{
			textTable.AddRow("  " + item.Key, item.Count().ToString());
		}
		textTable.AddRow("Trees", num2.ToString());
		textTable.AddRow("Ores", num3.ToString());
		arg.ReplyWith(textTable.ToString());
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}
}

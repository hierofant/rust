using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("entity")]
public class Entity : ConsoleSystem
{
	private struct EntityInfo
	{
		public BaseNetworkable entity;

		public NetworkableId entityID;

		public uint groupID;

		public NetworkableId parentID;

		public string status;

		public EntityInfo(BaseNetworkable src)
		{
			entity = src;
			BaseEntity baseEntity = entity as BaseEntity;
			BaseEntity baseEntity2 = ((baseEntity != null) ? baseEntity.GetParentEntity() : null);
			entityID = ((entity != null && entity.net != null) ? entity.net.ID : default(NetworkableId));
			groupID = ((entity != null && entity.net != null && entity.net.group != null) ? entity.net.group.ID : 0u);
			parentID = ((baseEntity != null) ? baseEntity.parentEntity.uid : default(NetworkableId));
			if (baseEntity != null && baseEntity.parentEntity.uid.IsValid)
			{
				if (baseEntity2 == null)
				{
					status = "orphan";
				}
				else
				{
					status = "child";
				}
			}
			else
			{
				status = string.Empty;
			}
		}
	}

	public struct EntitySpawnRequest
	{
		public string PrefabName;

		public string Error;

		public bool Valid => string.IsNullOrEmpty(Error);
	}

	private struct VendorDefinition
	{
		public string VendingMachinePrefab;

		public string ShopKeeperPrefab;
	}

	private static readonly Dictionary<string, VendorDefinition> VendorDefinitions = new Dictionary<string, VendorDefinition>(StringComparer.OrdinalIgnoreCase) { ["waterwell"] = new VendorDefinition
	{
		VendingMachinePrefab = "assets/prefabs/deployable/vendingmachine/npcvendingmachines/shopkeeper_vm_invis_waterwell.prefab",
		ShopKeeperPrefab = "assets/prefabs/npc/waterwell/waterwell_shopkeeper.prefab"
	} };

	private static void GetEntityTable(TextTable table, Func<EntityInfo, bool> filter)
	{
		table.AddColumn("realm");
		table.AddColumn("entity");
		table.AddColumn("group");
		table.AddColumn("parent");
		table.AddColumn("name");
		table.AddColumn("position");
		table.AddColumn("local");
		table.AddColumn("rotation");
		table.AddColumn("local");
		table.AddColumn("status");
		table.AddColumn("invokes");
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (!(serverEntity == null))
			{
				EntityInfo arg = new EntityInfo(serverEntity);
				if (filter(arg))
				{
					table.AddRow("sv", arg.entityID.Value.ToString(), arg.groupID.ToString(), arg.parentID.Value.ToString(), arg.entity.ShortPrefabName, arg.entity.transform.position.ToString(), arg.entity.transform.localPosition.ToString(), arg.entity.transform.rotation.eulerAngles.ToString(), arg.entity.transform.localRotation.eulerAngles.ToString(), arg.status, arg.entity.InvokeString());
				}
			}
		}
	}

	[ServerVar(Help = "(Generated) Lists all networked entities whose prefab path contains the given filter string in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities whose prefab path contains the given filter string in a formatted table; admin-only on client")]
	public static void find_entity(Arg args)
	{
		string filter = args.GetString(0);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => string.IsNullOrEmpty(filter) || info.entity.PrefabName.Contains(filter));
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Lists the networked entity with the given network entity ID in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists the networked entity with the given network entity ID in a formatted table; admin-only on client")]
	public static void find_id(Arg args)
	{
		NetworkableId filter = ArgEx.GetEntityID(args, 0);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => info.entityID == filter);
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Lists all networked entities belonging to the given network group ID in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities belonging to the given network group ID in a formatted table; admin-only on client")]
	public static void find_group(Arg args)
	{
		uint filter = args.GetUInt(0);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => info.groupID == filter);
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Lists all networked entities that have the given network entity ID as their parent in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities that have the given network entity ID as their parent in a formatted table; admin-only on client")]
	public static void find_parent(Arg args)
	{
		NetworkableId filter = ArgEx.GetEntityID(args, 0);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => info.parentID == filter);
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Lists all networked entities whose status string contains the given filter text in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities whose status string contains the given filter text in a formatted table; admin-only on client")]
	public static void find_status(Arg args)
	{
		string filter = args.GetString(0);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => string.IsNullOrEmpty(filter) || info.status.Contains(filter));
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Lists all networked entities within the given radius in metres of the calling player in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities within the given radius in metres of the calling player in a formatted table; admin-only on client")]
	public static void find_radius(Arg args)
	{
		BasePlayer player = ArgEx.Player(args);
		if (player == null)
		{
			return;
		}
		uint filter = args.GetUInt(0, 10u);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => Vector3.Distance(info.entity.transform.position, player.transform.position) <= (float)filter);
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Lists all networked entities owned by the calling player (matched by network ID) in a formatted table; admin-only on client")]
	[ClientVar(Help = "(Generated) Lists all networked entities owned by the calling player (matched by network ID) in a formatted table; admin-only on client")]
	public static void find_self(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (basePlayer == null || basePlayer.net == null)
		{
			return;
		}
		NetworkableId filter = basePlayer.net.ID;
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		GetEntityTable(textTable, (EntityInfo info) => info.entityID == filter);
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Toggles the debug info overlay for an entity by net ID, showing position, velocity, health, and network state in the world")]
	public static void debug_toggle(Arg args)
	{
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (!entityID.IsValid)
		{
			return;
		}
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityID) as BaseEntity;
		if (!(baseEntity == null))
		{
			using (BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(BaseEntity.Flags.Debugging, !baseEntity.IsDebugging());
			}
			if (baseEntity.IsDebugging())
			{
				baseEntity.OnDebugStart();
			}
			NetworkableId iD = baseEntity.net.ID;
			args.ReplyWith("Debugging for " + iD.ToString() + " " + (baseEntity.IsDebugging() ? "enabled" : "disabled"));
		}
	}

	[ServerVar(Help = "(Generated) Applies a small positional nudge to an entity by net ID, useful for unsticking entities that are clipping into geometry")]
	public static void nudge(Arg args)
	{
		NetworkableId entityID = ArgEx.GetEntityID(args, 0);
		if (entityID.IsValid)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityID) as BaseEntity;
			if (!(baseEntity == null))
			{
				baseEntity.BroadcastMessage("DebugNudge", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static EntitySpawnRequest GetSpawnEntityFromName(string name)
	{
		EntitySpawnRequest result;
		if (string.IsNullOrEmpty(name))
		{
			result = default(EntitySpawnRequest);
			result.Error = "No entity name provided";
			return result;
		}
		string[] array = (from x in GameManifest.Current.entities
			where Path.GetFileNameWithoutExtension(x).Contains(name, CompareOptions.IgnoreCase)
			select x.ToLower()).ToArray();
		if (array.Length == 0)
		{
			result = default(EntitySpawnRequest);
			result.Error = "Entity type not found";
			return result;
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(Path.GetFileNameWithoutExtension(x), name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				result = default(EntitySpawnRequest);
				result.Error = "Unknown entity - could be:\n\n" + string.Join("\n", array.Select(Path.GetFileNameWithoutExtension).ToArray());
				return result;
			}
			array[0] = text;
		}
		result = default(EntitySpawnRequest);
		result.PrefabName = array[0];
		return result;
	}

	[ServerVar(Name = "spawn", Help = "(Generated) Spawns a server entity by prefab name at a given world position and direction; returns the spawned entity net ID")]
	public static string svspawn(string name, Vector3 pos, Vector3 dir, int forceUp = 1)
	{
		BasePlayer arg = ArgEx.Player(ConsoleSystem.CurrentArgs);
		EntitySpawnRequest spawnEntityFromName = GetSpawnEntityFromName(name);
		if (!spawnEntityFromName.Valid)
		{
			return spawnEntityFromName.Error;
		}
		bool flag = forceUp == 1;
		BaseEntity baseEntity = GameManager.server.CreateEntity(spawnEntityFromName.PrefabName, pos, flag ? Quaternion.LookRotation(dir, Vector3.up) : Quaternion.Euler(dir));
		if (baseEntity == null)
		{
			Debug.Log($"{arg} failed to spawn \"{spawnEntityFromName.PrefabName}\" (tried to spawn \"{name}\")");
			return "Couldn't spawn " + name;
		}
		BasePlayer basePlayer = baseEntity as BasePlayer;
		if (basePlayer != null)
		{
			if (flag)
			{
				basePlayer.OverrideViewAngles(Quaternion.LookRotation(dir, Vector3.up).eulerAngles);
			}
			else
			{
				basePlayer.OverrideViewAngles(dir);
			}
		}
		baseEntity.Spawn();
		if (baseEntity.TryGetComponent<EntityParentSettings>(out var component))
		{
			component.TryDetachChildren(baseEntity);
		}
		baseEntity.UpdateNetworkGroup();
		Debug.Log($"{arg} spawned \"{baseEntity}\" at {pos}");
		return "spawned " + baseEntity?.ToString() + " at " + pos;
	}

	private static string UnknownVendorMessage(string name)
	{
		return "Unknown vendor \"" + name + "\" - known vendors: " + string.Join(", ", VendorDefinitions.Keys);
	}

	[ServerVar(Name = "spawnvendor", Help = "(Generated) Spawns a complete NPC vendor by vendor name - both the shopkeeper NPC and the invisible vending machine it needs - at the position the calling player is looking at")]
	public static string svspawnvendor(string name)
	{
		BasePlayer basePlayer = ArgEx.Player(ConsoleSystem.CurrentArgs);
		if (string.IsNullOrEmpty(name) || !VendorDefinitions.TryGetValue(name, out var value))
		{
			return UnknownVendorMessage(name);
		}
		if (basePlayer == null)
		{
			return "spawnvendor has to be run by a player - it spawns the vendor wherever you're looking";
		}
		Ray ray = basePlayer.eyes.HeadRay();
		if (!UnityEngine.Physics.Raycast(ray, out var hitInfo, 100f, 1218652417, QueryTriggerInteraction.Ignore))
		{
			return "Nothing to place the vendor on - look at the ground and try again";
		}
		Vector3 point = hitInfo.point;
		Vector3 vector = -ray.direction.XZ3D();
		Quaternion rot = Quaternion.LookRotation((vector.sqrMagnitude > 0.001f) ? vector : Vector3.forward, Vector3.up);
		InvisibleVendingMachine invisibleVendingMachine = SpawnVendorEntity<InvisibleVendingMachine>(value.VendingMachinePrefab, point, rot);
		if (invisibleVendingMachine == null)
		{
			Debug.Log($"{basePlayer} failed to spawn \"{value.VendingMachinePrefab}\" for the \"{name}\" vendor");
			return "Couldn't spawn the vending machine for the \"" + name + "\" vendor";
		}
		NPCShopKeeper nPCShopKeeper = SpawnVendorEntity<NPCShopKeeper>(value.ShopKeeperPrefab, point, rot);
		if (nPCShopKeeper == null)
		{
			invisibleVendingMachine.Kill();
			Debug.Log($"{basePlayer} failed to spawn \"{value.ShopKeeperPrefab}\" for the \"{name}\" vendor");
			return "Couldn't spawn the shopkeeper for the \"" + name + "\" vendor";
		}
		if (nPCShopKeeper.GetVendingMachine() != invisibleVendingMachine)
		{
			Debug.LogWarning("Spawned the \"" + name + "\" vendor but the shopkeeper didn't pair up with its vending machine - the shop won't be interactable");
		}
		Debug.Log($"{basePlayer} spawned the \"{name}\" vendor at {point}");
		return $"spawned the \"{name}\" vendor at {point}";
	}

	private static T SpawnVendorEntity<T>(string prefabName, Vector3 pos, Quaternion rot) where T : BaseEntity
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabName, pos, rot);
		if (baseEntity == null)
		{
			return null;
		}
		if (!(baseEntity is T val))
		{
			Debug.LogError("\"" + prefabName + "\" is not a " + typeof(T).Name + " - the vendor definition is wrong");
			GameManager.Destroy(baseEntity.gameObject);
			return null;
		}
		if (val is BasePlayer basePlayer)
		{
			basePlayer.OverrideViewAngles(rot.eulerAngles);
		}
		val.Spawn();
		if (val.TryGetComponent<EntityParentSettings>(out var component))
		{
			component.TryDetachChildren(val);
		}
		val.UpdateNetworkGroup();
		return val;
	}

	[ServerVar(Name = "spawnitem", Help = "(Generated) Spawns a dropped item entity server-side by item short name at a given world position")]
	public static string svspawnitem(string name, Vector3 pos)
	{
		BasePlayer basePlayer = ArgEx.Player(ConsoleSystem.CurrentArgs);
		if (string.IsNullOrEmpty(name))
		{
			return "No entity name provided";
		}
		string[] array = (from x in ItemManager.itemList
			select x.shortname into x
			where x.Contains(name, CompareOptions.IgnoreCase)
			select x).ToArray();
		if (array.Length == 0)
		{
			return "Entity type not found";
		}
		if (array.Length > 1)
		{
			string text = array.FirstOrDefault((string x) => string.Compare(x, name, StringComparison.OrdinalIgnoreCase) == 0);
			if (text == null)
			{
				Debug.Log($"{basePlayer} failed to spawn \"{name}\"");
				return "Unknown entity - could be:\n\n" + string.Join("\n", array);
			}
			array[0] = text;
		}
		Item item = ItemManager.CreateByName(array[0], 1, 0uL);
		if (item == null)
		{
			Debug.Log($"{basePlayer} failed to spawn \"{array[0]}\" (tried to spawnitem \"{name}\")");
			return "Couldn't spawn " + name;
		}
		item?.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		BaseEntity arg = item.CreateWorldObject(pos);
		Debug.Log($"{basePlayer} spawned \"{arg}\" at {pos} (via spawnitem)");
		return "spawned " + item?.ToString() + " at " + pos;
	}

	[ServerVar(Name = "spawngrid", Help = "(Generated) Spawns a grid of server entities by prefab name centred at a position; useful for stress-testing entity counts")]
	public static string svspawngrid(string name, int width = 5, int height = 5, float spacing = 5f)
	{
		BasePlayer basePlayer = ArgEx.Player(ConsoleSystem.CurrentArgs);
		EntitySpawnRequest spawnEntityFromName = GetSpawnEntityFromName(name);
		if (!spawnEntityFromName.Valid)
		{
			return spawnEntityFromName.Error;
		}
		Quaternion rotation = basePlayer.transform.rotation;
		rotation.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
		Matrix4x4 matrix4x = Matrix4x4.TRS(basePlayer.transform.position, basePlayer.transform.rotation, Vector3.one);
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Vector3 pos = matrix4x.MultiplyPoint(new Vector3((float)i * spacing, 0f, (float)j * spacing));
				BaseEntity baseEntity = GameManager.server.CreateEntity(spawnEntityFromName.PrefabName, pos, rotation);
				if (baseEntity == null)
				{
					Debug.Log($"{basePlayer} failed to spawn \"{spawnEntityFromName.PrefabName}\" (tried to spawn \"{name}\")");
					return "Couldn't spawn " + name;
				}
				baseEntity.Spawn();
			}
		}
		Debug.Log($"{basePlayer} spawned ({width * height}) " + spawnEntityFromName.PrefabName);
		return $"spawned ({width * height}) " + spawnEntityFromName.PrefabName;
	}

	[ServerVar(Name = "spawnplants", Help = "Spawn every stage of every plant inside it's own planter, with an optional filter")]
	public static void spawnplants(Arg args)
	{
		string @string = args.GetString(0);
		int @int = args.GetInt(0, 1);
		BasePlayer basePlayer = ArgEx.Player(args);
		List<PlanterBox> list = SpawnPlants(basePlayer.transform.position, basePlayer.ServerRotation, @string, @int);
		args.ReplyWith($"Spawned {list.Count} planters");
	}

	public static List<PlanterBox> SpawnPlants(Vector3 position, Quaternion rotation, string filter = "", int height = 1)
	{
		List<PlanterBox> list = new List<PlanterBox>();
		GrowableEntity[] source = (from x in GameManifest.Current.entities
			where x.StartsWith("assets/prefabs/plants/", StringComparison.OrdinalIgnoreCase)
			select GameManager.server.FindPrefab(x) into x
			select x.GetComponent<GrowableEntity>() into x
			where x != null
			select x).ToArray();
		int num = 0;
		string strPrefab = "Assets/Prefabs/Deployable/Planters/planter.large.deployed.prefab";
		foreach (GrowableEntity item in source.OrderBy((GrowableEntity x) => x.ShortPrefabName))
		{
			if (!string.IsNullOrEmpty(filter) && !item.ShortPrefabName.Contains(filter))
			{
				continue;
			}
			for (int i = 0; i <= 7; i++)
			{
				for (int j = 0; j < height; j++)
				{
					Vector3 pos = position + new Vector3((float)num * 3f, 0f, (float)i * 3f);
					PlanterBox planterBox = GameManager.server.CreateEntity(strPrefab, pos, rotation) as PlanterBox;
					planterBox.soilSaturation = planterBox.soilSaturationMax;
					planterBox.Spawn();
					list.Add(planterBox);
					Socket_Specific_Female[] array = (from x in PrefabAttribute.server.FindAll<Socket_Base>(planterBox.prefabID).OfType<Socket_Specific_Female>()
						where x.allowedMaleSockets.Contains("planter_slot")
						select x).ToArray();
					foreach (Socket_Specific_Female socket_Specific_Female in array)
					{
						GrowableEntity obj = GameManager.server.CreateEntity(item.PrefabName, socket_Specific_Female.localPosition, socket_Specific_Female.localRotation) as GrowableEntity;
						obj.ChangeState((PlantProperties.State)i, resetAge: true, loading: true);
						obj.Spawn();
						obj.SetParent(planterBox);
						obj.SetGrowing(state: false);
					}
				}
			}
			num++;
		}
		return list;
	}

	[ServerVar(Help = "(Generated) Spawns a copy of the loot table from one container prefab into the world at the calling player position")]
	public static void spawnlootfrom(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		string @string = args.GetString(0, string.Empty);
		int @int = args.GetInt(1, 1);
		Vector3 vector = args.GetVector3(1, basePlayer ? basePlayer.CenterPoint() : Vector3.zero);
		if (string.IsNullOrEmpty(@string))
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(@string, vector);
		if (baseEntity == null)
		{
			return;
		}
		baseEntity.Spawn();
		basePlayer.ChatMessage("Contents of " + @string + " spawned " + @int + " times");
		LootContainer component = baseEntity.GetComponent<LootContainer>();
		if (component != null)
		{
			for (int i = 0; i < @int * component.maxDefinitionsToSpawn; i++)
			{
				component.lootDefinition.SpawnIntoContainer(basePlayer.inventory.containerMain);
			}
		}
		baseEntity.Kill();
	}

	public static int DeleteBy(ulong id)
	{
		List<ulong> obj = Facepunch.Pool.Get<List<ulong>>();
		obj.Add(id);
		int result = DeleteBy(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	[ServerVar(Help = "Destroy all entities created by provided users (separate users by space)")]
	public static int DeleteBy(Arg arg)
	{
		if (!arg.HasArgs())
		{
			return 0;
		}
		List<ulong> obj = Facepunch.Pool.Get<List<ulong>>();
		StringView[] args = arg.Args;
		for (int i = 0; i < args.Length; i++)
		{
			if (ulong.TryParse(args[i], out var result))
			{
				obj.Add(result);
			}
		}
		int result2 = DeleteBy(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result2;
	}

	private static int DeleteBy(List<ulong> ids)
	{
		int num = 0;
		foreach (BaseEntity serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity == null)
			{
				continue;
			}
			bool flag = false;
			foreach (ulong id in ids)
			{
				if (serverEntity.OwnerID == id)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				serverEntity.Invoke(serverEntity.KillMessage, (float)num * 0.2f);
				num++;
			}
		}
		return num;
	}

	[ServerVar(Help = "Destroy all entities created by users in the provided text block (can use with copied results from ent auth)")]
	public static void DeleteByTextBlock(Arg arg)
	{
		if (arg.Args.Length != 1)
		{
			arg.ReplyWith("Invalid arguments, provide a text block surrounded by \" and listing player id's at the start of each line");
			return;
		}
		MatchCollection matchCollection = Regex.Matches(arg.GetString(0), "^\\b\\d{17}", RegexOptions.Multiline);
		List<ulong> obj = Facepunch.Pool.Get<List<ulong>>();
		foreach (Match item in matchCollection)
		{
			if (ulong.TryParse(item.Value, out var result))
			{
				obj.Add(result);
			}
		}
		int num = DeleteBy(obj);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		arg.ReplyWith($"Destroyed {num} entities");
	}

	[ServerVar(Help = "(Generated) Sets the charge level of an electric battery entity by net ID to the given percentage (0-100)")]
	public static void set_battery_charge(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Usage: set_battery_charge <charge>");
			return;
		}
		float @float = arg.GetFloat(0);
		ElectricBattery electricBattery = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 5f) as ElectricBattery;
		if (electricBattery == null)
		{
			arg.ReplyWith("Not looking at battery");
			return;
		}
		electricBattery.SetCharge(@float);
		arg.ReplyWith($"Set battery charge to {@float}");
	}

	[ServerVar(EditorOnly = true, Help = "(Generated) Editor only: stress-tests the entity pool system by rapidly spawning and despawning a named prefab many times")]
	public static void test_pooling(Arg args)
	{
	}
}

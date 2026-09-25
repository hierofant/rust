using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2;
using UnityEngine;

namespace ConVar;

[Factory("copypaste")]
public class CopyPaste : ConsoleSystem
{
	private class EntityWrapper
	{
		public BaseEntity Entity;

		public ProtoBuf.Entity Protobuf;

		public Vector3 Position;

		public Quaternion Rotation;

		public bool HasParent;
	}

	public class PasteOptions
	{
		public const string Argument_NPCs = "--npcs";

		public const string Argument_Resources = "--resources";

		public const string Argument_Vehicles = "--vehicles";

		public const string Argument_Deployables = "--deployables";

		public const string Argument_FoundationsOnly = "--foundations-only";

		public const string Argument_BuildingBlocksOnly = "--building-only";

		public const string Argument_SnapToTerrain = "--autosnap-terrain";

		public const string Argument_SnapToZeroHeight = "--autosnap-zero";

		public const string Argument_PastePlayers = "--players";

		public const string Argument_AutoAuth = "--auto-auth";

		public bool Resources;

		public bool NPCs;

		public bool Vehicles;

		public bool Deployables;

		public bool FoundationsOnly;

		public bool BuildingBlocksOnly;

		public bool SnapToTerrain;

		public bool SnapToZero;

		public bool Players;

		public bool AutoAuth;

		public Vector3 Origin;

		public Quaternion PlayerRotation;

		public Vector3 HeightOffset;

		public PasteOptions(Arg arg)
		{
			Resources = arg.HasArg("--resources", remove: true);
			NPCs = arg.HasArg("--npcs", remove: true);
			Vehicles = arg.HasArg("--vehicles", remove: true);
			Deployables = arg.HasArg("--deployables", remove: true);
			FoundationsOnly = arg.HasArg("--foundations-only", remove: true);
			BuildingBlocksOnly = arg.HasArg("--building-only", remove: true);
			SnapToTerrain = arg.HasArg("--autosnap-terrain", remove: true);
			SnapToZero = arg.HasArg("--autosnap-zero", remove: true);
			Players = arg.HasArg("--players", remove: true);
			AutoAuth = arg.HasArg("--auto-auth", remove: true);
		}

		public PasteOptions(PasteRequest request)
		{
			Resources = request.resources;
			NPCs = request.npcs;
			Vehicles = request.vehicles;
			Deployables = request.deployables;
			FoundationsOnly = request.foundationsOnly;
			BuildingBlocksOnly = request.buildingBlocksOnly;
			SnapToTerrain = request.snapToTerrain;
			SnapToZero = request.snapToZero;
			Players = request.players;
			AutoAuth = request.autoAuth;
			Origin = request.origin;
			PlayerRotation = Quaternion.Euler(request.playerRotation);
			HeightOffset = request.heightOffset;
		}

		public PasteOptions()
		{
		}
	}

	private const string ClipboardFileName = "clipboard";

	private const string OverwriteFlag = "--overwrite";

	public static CopyPasteHistoryManager playerHistory = new CopyPasteHistoryManager();

	private static void PrintPasteNames(StringBuilder builder, string directory)
	{
		if (!Directory.Exists(directory))
		{
			builder.AppendLine("No pastes found");
			return;
		}
		string[] files = Directory.GetFiles(directory, "*.data");
		builder.AppendLine($"Found {files.Length} pastes");
		foreach (string item in files.OrderBy((string x) => x))
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
			builder.AppendLine(fileNameWithoutExtension);
		}
	}

	private static void CopyEntities(BasePlayer player, List<BaseEntity> entities, string name, Vector3 originPos, Quaternion originRot)
	{
		using CopyPasteEntityInfo arg = BuildCopyPaste(entities, originPos, originRot);
		CopyPasteEntity.ServerInstance?.ClientRPC(RpcTarget.Player("CLIENT_ReceivePaste", player), name, arg);
	}

	public static byte[] BuildCopyPasteBytes(List<BaseEntity> entities, Vector3 originPos, Quaternion originRot, List<BaseEntity> savedOrdered)
	{
		using CopyPasteEntityInfo proto = BuildCopyPaste(entities, originPos, originRot, savedOrdered);
		return proto.ToProtoBytes();
	}

	public static CopyPasteEntityInfo BuildCopyPaste(List<BaseEntity> entities, Vector3 originPos, Quaternion originRot, List<BaseEntity> savedOrdered = null)
	{
		OrderEntitiesForSave(entities);
		CopyPasteEntityInfo copyPasteEntityInfo = Facepunch.Pool.Get<CopyPasteEntityInfo>();
		copyPasteEntityInfo.entities = Facepunch.Pool.Get<List<ProtoBuf.Entity>>();
		Transform transform = new GameObject("Align").transform;
		transform.position = originPos;
		transform.rotation = originRot;
		foreach (BaseEntity entity in entities)
		{
			if (!entity.isClient && entity.enableSaving)
			{
				BaseEntity baseEntity = entity.parentEntity.Get(serverside: true);
				if (baseEntity != null && (!entities.Contains(baseEntity) || !baseEntity.enableSaving))
				{
					Debug.LogWarning("Skipping " + entity.ShortPrefabName + " as it is parented to an entity not included in the copy (it would become orphaned)");
					continue;
				}
				SaveEntity(entity, copyPasteEntityInfo, baseEntity, transform);
				savedOrdered?.Add(entity);
			}
		}
		copyPasteEntityInfo.entityCount = copyPasteEntityInfo.entities.Count;
		UnityEngine.Object.Destroy(transform.gameObject);
		return copyPasteEntityInfo;
	}

	private static List<EntityWrapper> PrepareEntityProtos(CopyPasteEntityInfo toLoad, PasteOptions options, bool assignNewUids)
	{
		toLoad = toLoad.Copy();
		HashSet<NetworkableId> hashSet = new HashSet<NetworkableId>();
		for (int i = 0; i < toLoad.entities.Count; i++)
		{
			ProtoBuf.Entity entity = toLoad.entities[i];
			if (!hashSet.Add(entity.baseNetworkable.uid))
			{
				BaseEntity baseEntity = GameManager.server.FindPrefab(entity.baseNetworkable.prefabID)?.GetComponent<BaseEntity>();
				Debug.LogWarning(string.Format("Skipping entity [{0}/{1}]: duplicate entity in paste, please re-save", entity.baseNetworkable.uid, (baseEntity == null) ? "unknown" : baseEntity.ShortPrefabName));
				toLoad.entities.RemoveAt(i);
				i--;
			}
		}
		Transform transform = new GameObject("Align").transform;
		transform.position = options.Origin;
		transform.rotation = options.PlayerRotation;
		List<EntityWrapper> list = new List<EntityWrapper>();
		Dictionary<ulong, ulong> remapping = new Dictionary<ulong, ulong>();
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		if (assignNewUids)
		{
			remapping = new Dictionary<ulong, ulong>();
		}
		foreach (ProtoBuf.Entity entity2 in toLoad.entities)
		{
			if (assignNewUids)
			{
				entity2.InspectUids(UpdateWithNewUid);
			}
			EntityWrapper item = new EntityWrapper
			{
				Protobuf = entity2,
				HasParent = (entity2.parent != null && entity2.parent.uid != default(NetworkableId))
			};
			list.Add(item);
			if (entity2.decayEntity != null)
			{
				if (!dictionary.TryGetValue(entity2.decayEntity.buildingID, out var value))
				{
					value = BuildingManager.server.NewBuildingID();
					dictionary.Add(entity2.decayEntity.buildingID, value);
				}
				entity2.decayEntity.buildingID = value;
			}
		}
		foreach (EntityWrapper item2 in list)
		{
			item2.Position = item2.Protobuf.baseEntity.pos;
			item2.Rotation = Quaternion.Euler(item2.Protobuf.baseEntity.rot);
			if (!item2.HasParent)
			{
				item2.Protobuf.baseEntity.pos = transform.TransformPoint(item2.Protobuf.baseEntity.pos);
				item2.Protobuf.baseEntity.rot = (transform.rotation * Quaternion.Euler(item2.Protobuf.baseEntity.rot)).eulerAngles;
			}
		}
		if (UnityEngine.Application.isPlaying)
		{
			UnityEngine.Object.Destroy(transform.gameObject);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(transform.gameObject);
		}
		return list;
		void UpdateWithNewUid(UidType type, ref ulong prevUid)
		{
			if (type == UidType.Clear)
			{
				prevUid = 0uL;
			}
			else if (prevUid != 0L && remapping != null)
			{
				if (!remapping.TryGetValue(prevUid, out var value2))
				{
					value2 = Network.Net.sv.TakeUID();
					remapping.Add(prevUid, value2);
				}
				prevUid = value2;
			}
		}
	}

	public static float ComputeAutoSnapOffsetY(IList<BaseEntity> entities, PasteOptions options, Vector3 pasteOrigin)
	{
		if (!options.SnapToTerrain && !options.SnapToZero)
		{
			return 0f;
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		foreach (BaseEntity entity in entities)
		{
			if ((!(entity.parentEntity.Get(serverside: true) == null) || !(entity.ShortPrefabName == "foundation")) && !(entity.ShortPrefabName == "foundation.triangle") && !(entity is PlayerBoat))
			{
				continue;
			}
			Vector3 position = entity.transform.position;
			float num3 = position.y - pasteOrigin.y;
			float num4;
			if (options.SnapToZero)
			{
				num4 = 0f;
			}
			else
			{
				num4 = ((!UnityEngine.Application.isPlaying) ? 0f : TerrainMeta.HeightMap.GetHeight(position));
				if (GamePhysics.Trace(new Ray(new Vector3(position.x, num4, position.z) + new Vector3(0f, 100f, 0f), Vector3.down), 0f, out var hitInfo, 100f, 8454160))
				{
					num4 = hitInfo.point.y;
				}
			}
			if (num3 > num4)
			{
				num = Mathf.Min(num, num3 - num4);
			}
			if (num4 > num3)
			{
				num2 = Mathf.Max(num2, num4 - num3);
			}
		}
		if (num == float.MaxValue && num2 == float.MinValue)
		{
			return 0f;
		}
		if (!(num < num2) && num2 != float.MinValue)
		{
			return num2;
		}
		return 0f - num;
	}

	private static void ApplyAutoSnap(List<BaseEntity> entities, PasteOptions options)
	{
		Vector3 vector = new Vector3(0f, ComputeAutoSnapOffsetY(entities, options, Vector3.zero), 0f);
		vector += options.HeightOffset;
		if (!(vector != Vector3.zero))
		{
			return;
		}
		foreach (BaseEntity entity in entities)
		{
			if (entity.parentEntity.Get(serverside: true) == null)
			{
				entity.transform.position += vector;
			}
			if (!(entity is IOEntity iOEntity))
			{
				continue;
			}
			if (iOEntity.inputs != null)
			{
				IOEntity.IOSlot[] inputs = iOEntity.inputs;
				for (int i = 0; i < inputs.Length; i++)
				{
					inputs[i].originPosition += vector;
				}
			}
			if (iOEntity.outputs != null)
			{
				IOEntity.IOSlot[] inputs = iOEntity.outputs;
				for (int i = 0; i < inputs.Length; i++)
				{
					inputs[i].originPosition += vector;
				}
			}
		}
	}

	public static List<BaseEntity> PasteEntitiesInternal(CopyPasteEntityInfo toLoad, PasteOptions options, ulong admin)
	{
		List<EntityWrapper> list = PrepareEntityProtos(toLoad, options, assignNewUids: true);
		List<BaseEntity> list2 = new List<BaseEntity>();
		foreach (EntityWrapper item in list)
		{
			if (CanPrefabBePasted(item.Protobuf.baseNetworkable.prefabID, options))
			{
				item.Entity = GameManager.server.CreateEntity(StringPool.Get(item.Protobuf.baseNetworkable.prefabID), item.Protobuf.baseEntity.pos, Quaternion.Euler(item.Protobuf.baseEntity.rot));
				if (item.Protobuf.basePlayer != null && item.Protobuf.basePlayer.userid > 10000000)
				{
					ulong userid = 10000000uL + (ulong)UnityEngine.Random.Range(1, int.MaxValue);
					item.Protobuf.basePlayer.userid = userid;
				}
				item.Entity.InitLoad(item.Protobuf.baseNetworkable.uid);
				item.Entity.PreServerLoad();
				list2.Add(item.Entity);
			}
		}
		list.RemoveAll((EntityWrapper x) => x.Entity == null);
		for (int i = 0; i < list.Count; i++)
		{
			EntityWrapper entityWrapper = list[i];
			BaseNetworkable.LoadInfo info = default(BaseNetworkable.LoadInfo);
			info.fromDisk = true;
			info.fromCopy = true;
			info.msg = entityWrapper.Protobuf;
			try
			{
				entityWrapper.Entity.Spawn();
				bool flag = false;
				if (!flag && entityWrapper.Protobuf.parent != null && entityWrapper.Protobuf.parent.uid != default(NetworkableId))
				{
					BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityWrapper.Protobuf.parent.uid) as BaseEntity;
					if (baseEntity == null || baseEntity.net == null)
					{
						flag = true;
					}
				}
				if (flag)
				{
					entityWrapper.Entity.Kill();
					list.RemoveAt(i);
					i--;
				}
				else
				{
					entityWrapper.Entity.Load(info);
				}
			}
			catch (Exception exception)
			{
				Debug.LogError("Failed to spawn entity '" + entityWrapper.Entity?.PrefabName + "' while pasting");
				Debug.LogException(exception);
				try
				{
					entityWrapper.Entity.Kill();
				}
				catch
				{
				}
			}
		}
		ApplyAutoSnap(list2, options);
		foreach (EntityWrapper item2 in list)
		{
			item2.Entity.PostServerLoad();
			item2.Entity.UpdateNetworkGroup();
		}
		foreach (EntityWrapper item3 in list)
		{
			item3.Entity.RefreshEntityLinks();
		}
		foreach (EntityWrapper item4 in list)
		{
			if (item4.Entity is BuildingBlock buildingBlock)
			{
				buildingBlock.UpdateSkin(force: true);
			}
		}
		foreach (EntityWrapper item5 in list)
		{
			if (item5.Entity is BaseMountable baseMountable)
			{
				baseMountable.UpdateMountFlags();
			}
			if (options.AutoAuth)
			{
				Admin.SetUserAuthorized(item5.Entity, admin, state: true);
			}
		}
		return (from x in list
			select x.Entity into x
			where x != null
			select x).ToList();
	}

	public static CopyPasteEntityInfo LoadFileFromBundles(string fullPath)
	{
		CopyPasteDataAsset copyPasteDataAsset = FileSystem.Load<CopyPasteDataAsset>(fullPath);
		if (copyPasteDataAsset == null)
		{
			Debug.LogWarning("Missing file: " + fullPath);
			return null;
		}
		return LoadFromAsset(copyPasteDataAsset);
	}

	public static CopyPasteEntityInfo LoadFromAsset(CopyPasteDataAsset copyPasteAsset)
	{
		if (copyPasteAsset == null)
		{
			return null;
		}
		byte[] data = copyPasteAsset.GetData();
		if (data == null || data.Length == 0)
		{
			return null;
		}
		return CopyPasteEntityInfo.Deserialize(data);
	}

	private static void SaveEntity(BaseEntity baseEntity, CopyPasteEntityInfo toSave, BaseEntity parent, Transform alignObject)
	{
		BaseNetworkable.SaveInfo saveInfo = default(BaseNetworkable.SaveInfo);
		saveInfo.forDisk = true;
		saveInfo.msg = Facepunch.Pool.Get<ProtoBuf.Entity>();
		saveInfo.cachedTime = BaseNetworkable.ThreadSafeTime.TakeSnapshot();
		BaseNetworkable.SaveInfo info = saveInfo;
		baseEntity.Save(info);
		if (parent == null)
		{
			info.msg.baseEntity.pos = alignObject.InverseTransformPoint(info.msg.baseEntity.pos);
			_ = alignObject.rotation * baseEntity.transform.rotation;
			info.msg.baseEntity.rot = (Quaternion.Inverse(alignObject.transform.rotation) * baseEntity.transform.rotation).eulerAngles;
		}
		toSave.entities.Add(info.msg);
	}

	private static void GetEntitiesLookingAt(Vector3 originPoint, Vector3 direction, List<BaseEntity> entityList)
	{
		entityList.Clear();
		BuildingBlock buildingBlock = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, new Ray(originPoint, direction), 0f, 100f, 136315136) as BuildingBlock;
		if (buildingBlock == null)
		{
			return;
		}
		ListHashSet<DecayEntity> listHashSet = buildingBlock.GetBuilding()?.decayEntities;
		if (listHashSet != null)
		{
			BaseEntity rootParentEntity = buildingBlock.GetRootParentEntity();
			if (rootParentEntity is PlayerBoat)
			{
				entityList.Add(rootParentEntity);
			}
			else
			{
				entityList.AddRange(listHashSet);
			}
		}
	}

	private static void GetEntitiesInRadius(Vector3 originPoint, float radius, List<BaseEntity> entityList)
	{
		if (radius <= 0f)
		{
			return;
		}
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		global::Vis.Entities(originPoint, radius, obj);
		foreach (BaseEntity item in obj)
		{
			if (!item.isClient)
			{
				entityList.Add(item);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static void GetEntitiesInBounds(Bounds bounds, List<BaseEntity> entityList)
	{
		OBB bounds2 = new OBB(bounds);
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		global::Vis.Entities(bounds2, obj);
		foreach (BaseEntity item in obj)
		{
			if (!item.isClient)
			{
				entityList.Add(item);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static bool CanPrefabBePasted(uint prefabId, PasteOptions options)
	{
		GameObject gameObject = GameManager.server.FindPrefab(prefabId);
		if (gameObject == null)
		{
			return false;
		}
		BaseEntity component = gameObject.GetComponent<BaseEntity>();
		if (component == null)
		{
			return false;
		}
		if (options.FoundationsOnly && component.ShortPrefabName != "foundation" && component.ShortPrefabName != "foundation.triangle")
		{
			return false;
		}
		if (options.BuildingBlocksOnly && !(component is BuildingBlock))
		{
			return false;
		}
		if (component is DecayEntity && !(component is BuildingBlock) && !options.Deployables)
		{
			return false;
		}
		if (component is BasePlayer { IsNpc: false } && !options.Players)
		{
			return false;
		}
		if (component is PointEntity || component is RelationshipManager)
		{
			return false;
		}
		if ((component is ResourceEntity || component is BushEntity) && !options.Resources)
		{
			return false;
		}
		if ((component is BaseNpc || component is RidableHorse) && !options.NPCs)
		{
			return false;
		}
		if (component is BaseVehicle && !(component is RidableHorse) && !options.Vehicles)
		{
			return false;
		}
		return true;
	}

	private static void OrderEntitiesForSave(List<BaseEntity> entities)
	{
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		obj.AddRange(entities);
		entities.Clear();
		HashSet<BaseEntity> hash = new HashSet<BaseEntity>();
		foreach (BaseEntity item in obj.OrderBy((BaseEntity x) => x.net.ID.Value))
		{
			AddRecursive(item);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		void AddRecursive(BaseEntity current)
		{
			if (hash.Add(current))
			{
				entities.Add(current);
				if (current.children != null)
				{
					foreach (BaseEntity child in current.children)
					{
						AddRecursive(child);
					}
				}
			}
		}
	}

	[ServerVar(Name = "copybox_sv", Help = "(Generated) Server-side handler that copies all entities within the specified bounding box (center + size) into a named paste file; called from copybox client command")]
	public static void copybox_sv(Arg args)
	{
		if (!args.HasArgs(3))
		{
			args.ReplyWith("Missing args: copybox_sv <name> <center> <size> <rotation>");
			return;
		}
		string @string = args.GetString(0);
		Vector3 vector = args.GetVector3(1);
		Vector3 vector2 = args.GetVector3(2);
		Quaternion originRot = Quaternion.Euler(args.GetVector3(3));
		Bounds bounds = new Bounds(vector, vector2);
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		GetEntitiesInBounds(bounds, obj);
		CopyEntities(ArgEx.Player(args), obj, @string, vector, originRot);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static void copyboat_sv(Arg args)
	{
	}

	public static List<BaseEntity> PasteEntities(CopyPasteEntityInfo data, PasteOptions options, ulong steamId)
	{
		List<BaseEntity> list;
		try
		{
			Rust.Application.isLoadingSave = true;
			Rust.Application.isLoading = true;
			list = PasteEntitiesInternal(data, options, steamId);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return new List<BaseEntity>();
		}
		finally
		{
			Rust.Application.isLoadingSave = false;
			Rust.Application.isLoading = false;
		}
		foreach (BaseEntity item in list)
		{
			if (!(item == null) && item is StabilityEntity stabilityEntity)
			{
				stabilityEntity.UpdateStability();
			}
		}
		return list;
	}

	[ServerVar(Help = "(Generated) Undoes the most recent paste operation for the calling player by destroying all entities that were spawned in that paste; replies with 'History empty' if nothing to undo")]
	public static void undopaste_sv(Arg args)
	{
		ulong steamId = ArgEx.Player(args)?.userID ?? ((EncryptedValue<ulong>)0uL);
		PasteResult pasteResult = playerHistory.Undo(steamId);
		if (pasteResult == null)
		{
			args.ReplyWith("History empty");
			return;
		}
		foreach (BaseEntity entity in pasteResult.Entities)
		{
			entity.Kill();
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that copies all entities within the specified radius around a position into a named paste file; called from the copyradius client command")]
	public static void copyradius_sv(Arg args)
	{
		string @string = args.GetString(0);
		Vector3 vector = args.GetVector3(1);
		float @float = args.GetFloat(2);
		Quaternion originRot = Quaternion.Euler(args.GetVector3(3));
		if (@float <= 0f)
		{
			args.ReplyWith("Invalid radius: must be greater than zero");
			return;
		}
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		GetEntitiesInRadius(vector, @float, obj);
		CopyEntities(ArgEx.Player(args), obj, @string, vector, originRot);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Server-side handler that copies all entities belonging to the building the player is looking at into a named paste file; called from the copybuilding client command")]
	public static void copybuilding_sv(Arg args)
	{
		string @string = args.GetString(0);
		Vector3 vector = args.GetVector3(1);
		Vector3 vector2 = args.GetVector3(2);
		Quaternion originRot = Quaternion.Euler(args.GetVector3(3));
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		GetEntitiesLookingAt(vector, vector2, obj);
		CopyEntities(ArgEx.Player(args), obj, @string, vector, originRot);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Server-side handler that prints the names of all entities within the current selection bounds; used to preview what would be included in a copy operation")]
	public static void printselection_sv(Arg args)
	{
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		Vector3 vector = args.GetVector3(0);
		Vector3 vector2 = args.GetVector3(1);
		args.GetVector3(2);
		GetEntitiesInBounds(new Bounds(vector, vector2), obj);
		StringBuilder stringBuilder = new StringBuilder();
		if (obj.Count == 0)
		{
			stringBuilder.AppendLine("Empty");
		}
		else
		{
			foreach (BaseEntity item in obj)
			{
				if (!item.isClient)
				{
					stringBuilder.AppendLine(item.name);
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		args.ReplyWith(stringBuilder.ToString());
	}

	private static string GetLegacyServerDirectory()
	{
		return Server.GetServerFolder("copypaste");
	}

	private static string GetLegacyServerPath(string name)
	{
		return GetLegacyServerDirectory() + "/" + name + ".data";
	}

	[ServerVar(Help = "Downloads a paste file stored on the server (legacy server-side storage) by name and sends its entity data to the requesting client for local storage")]
	public static void download_paste_sv(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Missing args: download_paste_sv <name>");
			return;
		}
		string @string = arg.GetString(0);
		string legacyServerPath = GetLegacyServerPath(arg.GetString(0));
		if (!File.Exists(legacyServerPath))
		{
			arg.ReplyWith("Paste '" + @string + "' not found");
			return;
		}
		using CopyPasteEntityInfo arg2 = CopyPasteEntityInfo.Deserialize(File.ReadAllBytes(legacyServerPath));
		CopyPasteEntity.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_ReceivePaste", arg.Connection), @string, arg2);
	}

	[ServerVar(Help = "(Generated) Lists all paste files stored in the legacy server-side copypaste directory and prints their names to the console")]
	public static void list_pastes_sv(Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		PrintPasteNames(stringBuilder, GetLegacyServerDirectory());
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Server-side handler that destroys all entities within the current selection bounds that match the active paste filter options (NPCs, vehicles, deployables etc.)")]
	public static void killbox_sv(Arg args)
	{
		Vector3 vector = args.GetVector3(0);
		Vector3 vector2 = args.GetVector3(1);
		PasteOptions options = new PasteOptions(args);
		Bounds bounds = new Bounds(vector, vector2);
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		GetEntitiesInBounds(bounds, obj);
		foreach (BaseEntity item in obj)
		{
			if (!item.isClient && CanPrefabBePasted(item.prefabID, options) && (!(item is BasePlayer entity) || entity.IsNpcPlayer()))
			{
				item.Kill();
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static Quaternion GetPlayerRotation(BasePlayer ply)
	{
		Vector3 forward = ply.eyes.BodyForward();
		forward.y = 0f;
		return Quaternion.LookRotation(forward, Vector3.up);
	}
}

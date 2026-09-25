#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.MarchingCubes;
using Network;
using ProtoBuf;
using Rust;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseSculpture : BaseCombatEntity, IUGCBrowserEntity, IServerFileReceiver, IMarchingCubesTarget, IDisposable
{
	private static readonly ListHashSet<BaseSculpture> ServerUpdateProcessQueue = new ListHashSet<BaseSculpture>();

	private bool sculptureDirty;

	private Action resetBlockExcludeLayersAction;

	[Header("BaseSculpture")]
	[SerializeField]
	private MeshFilter targetMesh;

	[SerializeField]
	private MeshCollider sharedMeshCollider;

	[SerializeField]
	private MeshLOD meshLOD;

	[SerializeField]
	private Renderer clientBlockRenderer;

	[SerializeField]
	private DamageType carvingDamageType;

	[SerializeField]
	private Vector3Int gridResolution = new Vector3Int(32, 32, 32);

	[SerializeField]
	private Vector3 gridOffset;

	[SerializeField]
	private float gridScale;

	[SerializeField]
	public SDFSet SDFSet;

	[SerializeField]
	private TriggerPlayerForce playerPushTrigger;

	[SerializeField]
	private CapsuleCollider playerPushCollider;

	[SerializeField]
	[Header("Mesh Painting")]
	private GameObjectRef meshPaintDialogueRef;

	[SerializeField]
	public GameObjectRef BasePlate;

	[SerializeField]
	private MeshPaintableSource[] meshPaintableSources;

	private UnityEngine.Mesh generationMesh;

	private UnityEngine.Mesh generationCollisionMesh;

	private UnityEngine.Mesh[] generationLodMeshes;

	[ClientVar(Default = "false", Help = "(Generated) When enabled, logs mesh vertex and triangle count statistics when a sculpture mesh is applied or modified")]
	public static bool LogMeshStats = false;

	[ReplicatedVar(Default = "false", Help = "Use convex colliders for generated blocks on both the client and server - slower to generate but blocks holes, only effects future modifications")]
	public static bool UseConvexColliders = false;

	private uint __sync_crc;

	public uint[] GetContentCRCs => new uint[1] { crc };

	public UGCType ContentType => UGCType.Sculpt;

	public List<ulong> EditingHistory => new List<ulong> { base.OwnerID };

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	private static string SculpturePath => ConVar.Server.GetServerFolder("sculptures");

	public Vector3Int GridResolution => gridResolution;

	public float3 GridOffset => gridOffset;

	public float GridScale => gridScale;

	public Renderer ClientBlockRenderer => clientBlockRenderer;

	public Bounds GridBounds => new Bounds(-gridOffset * gridScale, (Vector3)gridResolution * gridScale);

	[Sync(Autosave = true, RequireChange = false, InvalidateCache = true, Pack = false)]
	private uint crc
	{
		[CompilerGenerated]
		get
		{
			return __sync_crc;
		}
		[CompilerGenerated]
		set
		{
			__sync_crc = value;
			byte nameID = __GetWeaverID("crc");
			SV_SyncVarSend(nameID);
		}
	}

	UnityEngine.Mesh IMarchingCubesTarget.TargetMesh => generationMesh;

	UnityEngine.Mesh IMarchingCubesTarget.TargetMeshForCollision => generationCollisionMesh;

	MeshCollider IMarchingCubesTarget.TargetMeshCollider => sharedMeshCollider;

	SDFSet IMarchingCubesTarget.SDFSet => SDFSet;

	Vector3 IMarchingCubesTarget.VertexOffset => gridOffset;

	float IMarchingCubesTarget.VertexScale => gridScale;

	bool IMarchingCubesTarget.WantsConvexCollider => UseConvexColliders;

	int IMarchingCubesTarget.LodMeshCount
	{
		get
		{
			UnityEngine.Mesh[] array = generationLodMeshes;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseSculpture.OnRpcMessage"))
		{
			if (rpc == 4267718869u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_LockSculpture");
				}
				using (TimeWarning.New("SV_LockSculpture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4267718869u, "SV_LockSculpture", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							SV_LockSculpture(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SV_LockSculpture");
					}
				}
				return true;
			}
			if (rpc == 2509595789u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_SendSculptureUpdate");
				}
				using (TimeWarning.New("SV_SendSculptureUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2509595789u, "SV_SendSculptureUpdate", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2509595789u, "SV_SendSculptureUpdate", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							SV_SendSculptureUpdate(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SV_SendSculptureUpdate");
					}
				}
				return true;
			}
			if (rpc == 1358295833 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_UnlockSculpture");
				}
				using (TimeWarning.New("SV_UnlockSculpture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1358295833u, "SV_UnlockSculpture", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							SV_UnlockSculpture(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in SV_UnlockSculpture");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (crc == 0)
		{
			ResetSet(SDFSet);
			MarkServerSculptureDirty();
		}
	}

	public void LoadFromData(byte[] arr)
	{
		using Sculpt sculpt = SculptFormat.LoadDisposableSculptFromStorage(arr);
		SDFSet.Chunks[0].CopyFromByteArray(sculpt.data);
		MarkServerSculptureDirty();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SV_LockSculpture(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || !CanUpdateSculpture(msg.player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Locked, b: true);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SV_UnlockSculpture(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || !CanUpdateSculpture(msg.player, ignoreLock: true))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Locked, b: false);
	}

	private void MarkServerSculptureDirty()
	{
		if (!sculptureDirty)
		{
			sculptureDirty = true;
			ServerUpdateProcessQueue.Add(this);
		}
	}

	public static void ProcessSculptureUpdates()
	{
		if (ServerUpdateProcessQueue.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("FileUpdates"))
		{
			for (int i = 0; i < ServerUpdateProcessQueue.Count; i++)
			{
				BaseSculpture baseSculpture = ServerUpdateProcessQueue[i];
				if (!(baseSculpture == null))
				{
					Debug.Assert(baseSculpture.isServer, "Added client sculpture to server process queue");
					baseSculpture.ServerSculptureUpdate();
				}
			}
		}
		ServerUpdateProcessQueue.Clear();
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	private void SV_SendSculptureUpdate(RPCMessage msg)
	{
		if (msg.read.Length > 2000000 || !CanUpdateSculpture(msg.player) || !msg.read.TemporaryBytesWithSize(out var buffer, out var size))
		{
			return;
		}
		using Sculpt sculpt = SculptFormat.LoadDisposableSculptFromStorage(new ArraySegment<byte>(buffer, 0, size));
		if (sculpt?.data.Array != null && sculpt.data.Count != 0)
		{
			SDFSet.Chunks[0].CopyFromByteArray(sculpt.data);
			MarkServerSculptureDirty();
		}
	}

	private void ServerSculptureUpdate()
	{
		using (TimeWarning.New("ServerSculptureUpdate"))
		{
			using BufferStream bufferStream = Facepunch.Pool.Get<BufferStream>().Initialize();
			bufferStream.Clear();
			SculptFormat.SerializeToSculpt(this, bufferStream);
			EnqueueMarchingCubesUpdate();
			byte[] storageReadyBuffer = SculptFormat.GetStorageReadyBuffer(bufferStream);
			FileStorage.server.Remove(crc, FileStorage.Type.sculpt, net.ID);
			crc = FileStorage.server.Store(storageReadyBuffer, FileStorage.Type.sculpt, net.ID);
			AdjustCollidersForPlayerIntersection();
			sculptureDirty = false;
		}
	}

	private void AdjustCollidersForPlayerIntersection()
	{
		if (!playerPushCollider)
		{
			return;
		}
		using (TimeWarning.New("AdjustCollidersForPlayerIntersection"))
		{
			int maxYLayer = SDFSet.GetMaxYLayer();
			Vector3 localScale = playerPushCollider.transform.localScale;
			localScale.y = (float)maxYLayer / (float)GridResolution.y;
			playerPushCollider.transform.localScale = localScale;
			MeshCollider meshCollider = sharedMeshCollider;
			meshCollider.excludeLayers = (int)meshCollider.excludeLayers | 0x1000;
			playerPushTrigger.pushVelocity = 5f;
			if (resetBlockExcludeLayersAction == null)
			{
				resetBlockExcludeLayersAction = ResetBlockExcludeLayers;
			}
			Invoke(resetBlockExcludeLayersAction, 3f);
		}
	}

	private void ResetBlockExcludeLayers()
	{
		if ((bool)sharedMeshCollider)
		{
			sharedMeshCollider.excludeLayers = 0;
			playerPushTrigger.pushVelocity = 0.25f;
		}
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
		if (crc != 0 && createdItem.info.TryGetComponent<ItemModSculpture>(out var component))
		{
			component.OnSculpturePickUp(net.ID, crc, createdItem);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (!ComponentEx.HasComponent<ItemModSculpture>(fromItem.info))
		{
			return;
		}
		AssociatedSculptureStorage associatedEntity = ItemModAssociatedEntity<AssociatedSculptureStorage>.GetAssociatedEntity(fromItem);
		if (associatedEntity != null)
		{
			crc = associatedEntity.Crc;
			FileStorage.server.ReassignEntityId(associatedEntity.net.ID, net.ID);
			byte[] array = FileStorage.server.Get(crc, FileStorage.Type.sculpt, net.ID);
			if (array == null)
			{
				Debug.LogError("[SCULPT] Missing sculpt data on-disk - fill with default");
				ClearContent();
			}
			else
			{
				PopulateSculptureFromEncodedData(array);
				MarkServerSculptureDirty();
			}
		}
		else
		{
			ClearContent();
		}
	}

	public void ClearContent()
	{
		ResetSet(SDFSet);
		MarkServerSculptureDirty();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		FileStorage.server.RemoveAllByEntity(net.ID);
	}

	[ServerVar(ServerAdmin = true)]
	public static void ListSavedSculptures(ConsoleSystem.Arg arg)
	{
		string sculpturePath = SculpturePath;
		TextTable obj = Facepunch.Pool.Get<TextTable>();
		obj.Clear();
		obj.AddColumn("Sculptures");
		foreach (string item in Directory.EnumerateFiles(sculpturePath))
		{
			obj.AddRow(Path.GetRelativePath(sculpturePath, item));
		}
		arg.ReplyWith(obj.ToString());
		Facepunch.Pool.Free(ref obj);
	}

	[ServerVar(ServerAdmin = true)]
	public static void SaveSculpture(ConsoleSystem.Arg arg)
	{
		string text = Path.ChangeExtension(Path.Combine(SculpturePath, arg.GetString(0)), ".sculpt");
		if (!string.IsNullOrEmpty(text))
		{
			if (!Directory.Exists(SculpturePath))
			{
				Directory.CreateDirectory(SculpturePath);
			}
			BaseSculpture baseSculpture = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ArgEx.Player(arg).eyes.HeadRay()) as BaseSculpture;
			if (!(baseSculpture == null))
			{
				baseSculpture.SaveToFile(text);
				arg.ReplyWith("[SCULPTING] Saved to " + text.Replace('\\', '/'));
			}
		}
	}

	private void SaveToFile(string path)
	{
		using BufferStream bufferStream = Facepunch.Pool.Get<BufferStream>().Initialize();
		bufferStream.Clear();
		SculptFormat.SerializeToSculpt(this, bufferStream);
		byte[] storageReadyBuffer = SculptFormat.GetStorageReadyBuffer(bufferStream);
		File.WriteAllBytes(path, storageReadyBuffer);
	}

	[ServerVar(ClientAdmin = true)]
	public static void LoadSculpture(ConsoleSystem.Arg arg)
	{
		string path = Path.ChangeExtension(Path.Combine(SculpturePath, arg.GetString(0)), ".sculpt");
		if (!File.Exists(path))
		{
			return;
		}
		using PooledList<BaseSculpture> pooledList = Facepunch.Pool.Get<PooledList<BaseSculpture>>();
		BasePlayer basePlayer = ArgEx.Player(arg);
		float @float = arg.GetFloat(1, -1f);
		if (@float < 0f)
		{
			Ray ray = basePlayer.eyes.HeadRay();
			if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ray) is BaseSculpture item)
			{
				pooledList.Add(item);
			}
		}
		else
		{
			Vis.Components(basePlayer.transform.position, @float, pooledList);
		}
		if (pooledList.Count == 0)
		{
			return;
		}
		byte[] arr = Array.Empty<byte>();
		using (FileStream fileStream = File.OpenRead(path))
		{
			using BinaryReader binaryReader = new BinaryReader(fileStream);
			if (fileStream.Length > 16000000)
			{
				return;
			}
			arr = binaryReader.ReadBytes((int)fileStream.Length);
		}
		HashSet<NetworkableId> obj = Facepunch.Pool.Get<HashSet<NetworkableId>>();
		int num = 0;
		foreach (BaseSculpture item2 in pooledList)
		{
			if (item2.isServer && obj.Add(item2.net.ID))
			{
				num++;
				item2.LoadFromData(arr);
			}
		}
		arg.ReplyWith($"Sent data to {num} sculptures");
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(ClientAdmin = true)]
	public static void ApplyRandomShapes(ConsoleSystem.Arg arg)
	{
		using PooledList<BaseSculpture> pooledList = Facepunch.Pool.Get<PooledList<BaseSculpture>>();
		BasePlayer basePlayer = ArgEx.Player(arg);
		float @float = arg.GetFloat(0, -1f);
		if (@float < 0f)
		{
			Ray ray = basePlayer.eyes.HeadRay();
			if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ray) is BaseSculpture item)
			{
				pooledList.Add(item);
			}
		}
		else
		{
			Vis.Components(basePlayer.transform.position, @float, pooledList);
		}
		if (pooledList.Count == 0)
		{
			return;
		}
		HashSet<NetworkableId> obj = Facepunch.Pool.Get<HashSet<NetworkableId>>();
		int num = 0;
		foreach (BaseSculpture item2 in pooledList)
		{
			if (!item2.isClient && obj.Add(item2.net.ID))
			{
				num++;
				item2.PopulateWithRandomShapes();
			}
		}
		arg.ReplyWith($"Applied random shapes to {num} sculptures");
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void PopulateWithRandomShapes()
	{
		Bounds chunkBoundsSetSpace = SDFSet.Chunks[0].ChunkBoundsSetSpace;
		float3 @float = chunkBoundsSetSpace.center;
		float3 x = chunkBoundsSetSpace.extents;
		float3 float2 = chunkBoundsSetSpace.min;
		float num = math.cmin(x);
		SDFSet.ClearAllMods();
		float num2 = chunkBoundsSetSpace.size.y - 10f;
		SDFSet.AddAABBMod(new float3(@float.x, float2.y + 10f + num2, @float.z), new float3(x.x * 2f, num2, x.z * 2f), isAdditive: false);
		int num3 = UnityEngine.Random.Range(6, 12);
		for (int i = 0; i < num3; i++)
		{
			float3 blockSpacePos = new float3(@float.x + UnityEngine.Random.Range(-0.7f, 0.7f) * x.x, float2.y + UnityEngine.Random.Range(2f, 10f + x.y * 0.5f), @float.z + UnityEngine.Random.Range(-0.7f, 0.7f) * x.z);
			float3 float3 = new float3(UnityEngine.Random.Range(0.15f, 0.4f), UnityEngine.Random.Range(0.15f, 0.4f), UnityEngine.Random.Range(0.15f, 0.4f)) * num;
			quaternion rotation = UnityEngine.Random.rotationUniform;
			float smoothing = SDFSet.SmoothingForRadius(UnityEngine.Random.value, math.cmin(float3));
			switch (UnityEngine.Random.Range(0, 7))
			{
			case 0:
				SDFSet.AddSphereMod(blockSpacePos, float3.x, isAdditive: true, smoothing);
				break;
			case 1:
				SDFSet.AddAABBMod(blockSpacePos, float3, isAdditive: true, smoothing);
				break;
			case 2:
				SDFSet.AddOBBMod(blockSpacePos, float3, rotation, isAdditive: true, smoothing);
				break;
			case 3:
				SDFSet.AddCylinderMod(blockSpacePos, float3, rotation, isAdditive: true, smoothing);
				break;
			case 4:
				SDFSet.AddCapsuleMod(blockSpacePos, float3, rotation, isAdditive: true, smoothing);
				break;
			case 5:
				SDFSet.AddConeMod(blockSpacePos, float3, rotation, isAdditive: true, smoothing);
				break;
			case 6:
				SDFSet.AddHexPrismMod(blockSpacePos, float3, rotation, isAdditive: true, smoothing);
				break;
			}
		}
		SDFSet.ScheduleRegenerateAllChunks();
		MarkServerSculptureDirty();
	}

	[ServerVar(ClientAdmin = true)]
	public static void PrintCrc(ConsoleSystem.Arg arg)
	{
		using PooledList<BaseSculpture> pooledList = Facepunch.Pool.Get<PooledList<BaseSculpture>>();
		BasePlayer basePlayer = ArgEx.Player(arg);
		float @float = arg.GetFloat(0, -1f);
		if (@float < 0f)
		{
			Ray ray = basePlayer.eyes.HeadRay();
			if (GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ray) is BaseSculpture item)
			{
				pooledList.Add(item);
			}
		}
		else
		{
			Vis.Components(basePlayer.transform.position, @float, pooledList);
		}
		if (pooledList.Count == 0)
		{
			return;
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumns("netID", "crc", "__sync_crc", "matches");
		HashSet<NetworkableId> obj = Facepunch.Pool.Get<HashSet<NetworkableId>>();
		foreach (BaseSculpture item2 in pooledList)
		{
			if (!item2.isClient && obj.Add(item2.net.ID))
			{
				textTable.AddRow(item2.net.ID.ToString(), item2.crc.ToString(), item2.__sync_crc.ToString(), (item2.crc == item2.__sync_crc).ToString());
			}
		}
		arg.ReplyWith(textTable.ToString());
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	UnityEngine.Mesh IMarchingCubesTarget.GetLodMesh(int level)
	{
		int num = level - 1;
		if (generationLodMeshes == null || num < 0 || num >= generationLodMeshes.Length)
		{
			return null;
		}
		return generationLodMeshes[num];
	}

	void IMarchingCubesTarget.OnRenderMeshesUpdated()
	{
	}

	public override void InitShared()
	{
		base.InitShared();
		Bounds gridBounds = GridBounds;
		generationMesh = new UnityEngine.Mesh
		{
			name = base.name,
			bounds = gridBounds
		};
		generationCollisionMesh = new UnityEngine.Mesh
		{
			name = base.name + "_collision",
			bounds = gridBounds
		};
		if (base.isClient)
		{
			targetMesh.sharedMesh = generationMesh;
			InitLodMeshes();
		}
		sharedMeshCollider.sharedMesh = generationCollisionMesh;
		SDFSet.Init();
		SDFSet.AddChunk(int3.zero, new int3(gridResolution.x, gridResolution.y, gridResolution.z));
	}

	private void InitLodMeshes()
	{
		if (meshLOD == null || meshLOD.States == null || meshLOD.States.Length == 0)
		{
			return;
		}
		meshLOD.States[0].mesh = generationMesh;
		int num = Mathf.Min(meshLOD.States.Length - 2, 2);
		if (num > 0)
		{
			Bounds gridBounds = GridBounds;
			generationLodMeshes = new UnityEngine.Mesh[num];
			for (int i = 0; i < num; i++)
			{
				generationLodMeshes[i] = new UnityEngine.Mesh
				{
					name = $"{base.name}_lod{i + 1}",
					bounds = gridBounds
				};
				meshLOD.States[i + 1].mesh = generationLodMeshes[i];
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public bool CanUpdateSculpture(BasePlayer player, bool ignoreLock = false)
	{
		if (!ignoreLock && IsLocked())
		{
			return false;
		}
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		return true;
	}

	private void EnqueueMarchingCubesUpdate()
	{
		MarchingCubesManager.Instance.Enqueue(this);
	}

	private void PopulateSculptureFromEncodedData(byte[] encoded)
	{
		using Sculpt sculpt = SculptFormat.LoadDisposableSculptFromStorage(encoded);
		SDFSet.Chunks[0].CopyFromByteArray(sculpt.data);
	}

	public static void ResetSet(SDFSet set)
	{
		Debug.Assert(set.Chunks.Count == 1);
		set.ClearChunks();
		set.ClearAllMods();
		set.AddAABBMod(set.Chunks[0].ChunkBoundsSetSpace.center, set.Chunks[0].ChunkBoundsSetSpace.extents * 2f, isAdditive: true);
		set.ScheduleRegenerateAllChunks();
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		Dispose();
	}

	public void Dispose()
	{
		SDFSet.Dispose();
		if ((bool)generationMesh)
		{
			UnityEngine.Object.Destroy(generationMesh);
		}
		if ((bool)generationCollisionMesh)
		{
			UnityEngine.Object.Destroy(generationCollisionMesh);
		}
		if (generationLodMeshes == null)
		{
			return;
		}
		for (int i = 0; i < generationLodMeshes.Length; i++)
		{
			if ((bool)generationLodMeshes[i])
			{
				UnityEngine.Object.Destroy(generationLodMeshes[i]);
			}
		}
		generationLodMeshes = null;
	}

	private void OnSyncVar_crc(uint? oldValue, uint newValue)
	{
		using (TimeWarning.New("BaseSculpture.OnSyncVar_crc"))
		{
			if (base.isServer && !oldValue.HasValue)
			{
				byte[] array = FileStorage.server.Get(newValue, FileStorage.Type.sculpt, net.ID);
				if (array == null || array.Length == 0)
				{
					Debug.LogWarning($"[SCULPTING] ({net.ID}) Missing sculpt data on-disk for - fill with default");
					ResetSet(SDFSet);
				}
				else
				{
					PopulateSculptureFromEncodedData(array);
				}
				MarkServerSculptureDirty();
			}
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: crc for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_crc);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				uint? oldValue = __sync_crc;
				uint newValue = (__sync_crc = reader.UInt32());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_crc(oldValue, newValue);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "crc")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite obj = Network.Net.sv.StartWrite();
		WriteAutoSaveSyncVars(obj);
		var (src, num) = obj.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Facepunch.Pool.Free(ref obj);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead obj = Facepunch.Pool.Get<NetRead>();
			obj.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(obj);
			Facepunch.Pool.Free(ref obj);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_crc = 0u;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PowergridManager : PointEntity<PowergridManager>
{
	public struct PowergridEntityEntry
	{
		public IPowergridEntity Entity;

		public float SqrDistanceToPowerPlant;
	}

	private struct InsertedFuseEntry
	{
		public PowergridFuseBox FuseBox;

		public Item Fuse;
	}

	private static Vector3 powerPlantPosition;

	private static bool hasCachedPowerPlantPosition;

	private const string powerlineAccessPointPrefabPath = "assets/prefabs/io/electric/generators/powergrid_powerline_io.static.prefab";

	private static readonly List<PowergridEntityEntry> powergridEntities = new List<PowergridEntityEntry>();

	public static readonly PowergridStageChangeWorkQueue stageChangeWorkQueue = new PowergridStageChangeWorkQueue(powergridEntities);

	private static ListHashSet<PowergridFuseBox> fuseBoxes = new ListHashSet<PowergridFuseBox>();

	private static ListHashSet<PowerlinePowergridAccessPointSpawn> powerlineAccessPointSpawns = new ListHashSet<PowerlinePowergridAccessPointSpawn>();

	private static ListHashSet<PowergridIOAccessPoint> spawnedPowergridAccessPoints = new ListHashSet<PowergridIOAccessPoint>();

	private static int noOfAccessPoints;

	private static List<InsertedFuseEntry> insertedFuses = new List<InsertedFuseEntry>();

	private static int currentInsertedFusesCount;

	private static int fuseSocketsCount;

	private List<ItemId> loadedFuseInsertionOrder;

	private int previousInsertedFusesCount;

	private bool hasTickedOnce;

	private float lastFuseDeteriorationTickTime;

	private Action _actionServerTick;

	private Action _actionServerFuseDeteriorationTick;

	private int __sync_CurrentStage;

	private float __sync_LastProcessedStateChangeSqrDistance;

	[Sync(Autosave = true)]
	public int CurrentStage
	{
		[CompilerGenerated]
		get
		{
			return __sync_CurrentStage;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_CurrentStage, value))
			{
				__sync_CurrentStage = value;
				byte nameID = __GetWeaverID("CurrentStage");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync]
	public float LastProcessedStateChangeSqrDistance
	{
		[CompilerGenerated]
		get
		{
			return __sync_LastProcessedStateChangeSqrDistance;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_LastProcessedStateChangeSqrDistance, value))
			{
				__sync_LastProcessedStateChangeSqrDistance = value;
				byte nameID = __GetWeaverID("LastProcessedStateChangeSqrDistance");
				QueueSyncVar(nameID);
			}
		}
	}

	private Action actionServerTick => ServerTick;

	private Action actionServerFuseDeteriorationTick => ServerFuseDeteriorationTick;

	public static int GetCurrentStage(bool isServer)
	{
		if (isServer)
		{
			if (PointEntity<PowergridManager>.ServerInstance == null)
			{
				return 0;
			}
			return PointEntity<PowergridManager>.ServerInstance.CurrentStage;
		}
		return 0;
	}

	public static Vector3 GetPowerPlantPosition()
	{
		if (!hasCachedPowerPlantPosition)
		{
			if (TerrainMeta.Path == null || TerrainMeta.Path.Monuments == null)
			{
				return Vector3.zero;
			}
			using PooledList<Vector3> pooledList = Facepunch.Pool.Get<PooledList<Vector3>>();
			List<MonumentInfo> monuments = TerrainMeta.Path.Monuments;
			int i = 0;
			for (int count = monuments.Count; i < count; i++)
			{
				MonumentInfo monumentInfo = monuments[i];
				if (monumentInfo.IsPowerPlant())
				{
					pooledList.Add(monumentInfo.transform.position);
				}
			}
			int count2 = pooledList.Count;
			if (count2 > 0)
			{
				if (count2 == 1)
				{
					powerPlantPosition = pooledList[0];
				}
				else
				{
					Vector3 zero = Vector3.zero;
					for (int j = 0; j < count2; j++)
					{
						zero += pooledList[j];
					}
					powerPlantPosition = zero / count2;
				}
			}
			else
			{
				powerPlantPosition = Vector3.zero;
			}
			hasCachedPowerPlantPosition = true;
		}
		return powerPlantPosition;
	}

	public static void Server_AddPowergridEntity(IPowergridEntity powergridEntity)
	{
		if (powergridEntity.Server_ShouldConnectToPowergrid())
		{
			float num = Vector3.SqrMagnitude(powergridEntity.GetEntity().transform.position - GetPowerPlantPosition());
			int index = FindSortedInsertIndex(num);
			powergridEntities.Insert(index, new PowergridEntityEntry
			{
				Entity = powergridEntity,
				SqrDistanceToPowerPlant = num
			});
			stageChangeWorkQueue.OnEntityInserted(index, powergridEntity);
			if (Rust.Application.isServerStarted && PointEntity<PowergridManager>.ServerInstance != null)
			{
				powergridEntity.Server_OnPowergridStageChanged(PointEntity<PowergridManager>.ServerInstance.CurrentStage);
			}
		}
	}

	public static void Server_RemovePowergridEntity(IPowergridEntity powergridEntity)
	{
		int i = 0;
		for (int count = powergridEntities.Count; i < count; i++)
		{
			if (powergridEntities[i].Entity == powergridEntity)
			{
				powergridEntities.RemoveAt(i);
				stageChangeWorkQueue.OnEntityRemoved(i, powergridEntity);
				break;
			}
		}
	}

	private static int FindSortedInsertIndex(float sqrDistance)
	{
		int num = 0;
		int num2 = powergridEntities.Count;
		while (num < num2)
		{
			int num3 = num + num2 >> 1;
			if (powergridEntities[num3].SqrDistanceToPowerPlant <= sqrDistance)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		return num;
	}

	public static void Server_AddPowergridFuseBox(PowergridFuseBox fuseBox)
	{
		if (fuseBoxes.TryAdd(fuseBox))
		{
			fuseSocketsCount += fuseBox.GetMaxNoOfFuses();
		}
	}

	public static void Server_RemovePowergridFuseBox(PowergridFuseBox fuseBox)
	{
		if (fuseBoxes.Remove(fuseBox))
		{
			fuseSocketsCount -= fuseBox.GetMaxNoOfFuses();
			if (fuseSocketsCount < 0)
			{
				fuseSocketsCount = 0;
			}
		}
		for (int num = insertedFuses.Count - 1; num >= 0; num--)
		{
			if (insertedFuses[num].FuseBox == fuseBox)
			{
				insertedFuses.RemoveAt(num);
			}
		}
	}

	public static void Server_OnFuseInsertedIntoFuseBox(PowergridFuseBox fuseBox, Item fuse, BasePlayer byPlayer)
	{
		InsertedFuseEntry insertedFuseEntry = default(InsertedFuseEntry);
		insertedFuseEntry.FuseBox = fuseBox;
		insertedFuseEntry.Fuse = fuse;
		InsertedFuseEntry item = insertedFuseEntry;
		insertedFuses.Add(item);
		currentInsertedFusesCount++;
		if (Rust.Application.isServerStarted)
		{
			Facepunch.Rust.Analytics.Azure.OnPowerGridFuseInserted(byPlayer, fuse, currentInsertedFusesCount);
		}
	}

	public static void Server_OnFuseRemovedFromFuseBox(Item fuse)
	{
		int i = 0;
		for (int count = insertedFuses.Count; i < count; i++)
		{
			if (insertedFuses[i].Fuse == fuse)
			{
				insertedFuses.RemoveAt(i);
				currentInsertedFusesCount--;
				break;
			}
		}
	}

	public static void Server_AddPowerlineAccessPointSpawn(PowerlinePowergridAccessPointSpawn accessPointSpawn)
	{
		powerlineAccessPointSpawns.TryAdd(accessPointSpawn);
	}

	public static void Server_RemovePowerlineAccessPointSpawn(PowerlinePowergridAccessPointSpawn accessPointSpawn)
	{
		powerlineAccessPointSpawns.Remove(accessPointSpawn);
	}

	public static void Server_AddPowergridAccessPoint(PowergridIOAccessPoint accessPoint)
	{
		spawnedPowergridAccessPoints.TryAdd(accessPoint);
		noOfAccessPoints++;
	}

	public static void Server_RemovePowergridAccessPoint(PowergridIOAccessPoint accessPoint)
	{
		spawnedPowergridAccessPoints.Remove(accessPoint);
		noOfAccessPoints--;
	}

	public static void SpawnPowerlineAccessPoints()
	{
		int i = 0;
		for (int count = powerlineAccessPointSpawns.Count; i < count; i++)
		{
			powerlineAccessPointSpawns[i].transform.GetPositionAndRotation(out var position, out var rotation);
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/io/electric/generators/powergrid_powerline_io.static.prefab", position, rotation);
			if (baseEntity == null)
			{
				Debug.LogError("Failed to spawn entity from assets/prefabs/io/electric/generators/powergrid_powerline_io.static.prefab");
			}
			else
			{
				baseEntity.Spawn();
			}
		}
	}

	public static int GetNoOfPowergridAccessPoints()
	{
		return noOfAccessPoints;
	}

	public static int GetNoOfPowergridEntities()
	{
		return powergridEntities.Count;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!World.LoadedFromSave)
		{
			SpawnPowerlineAccessPoints();
		}
		InvokeRepeating(actionServerTick, 0f, 0f);
		lastFuseDeteriorationTickTime = UnityEngine.Time.time;
		InvokeRandomized(actionServerFuseDeteriorationTick, 1f, 1f, 0.015f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		CancelInvoke(actionServerTick);
		CancelInvoke(actionServerFuseDeteriorationTick);
		stageChangeWorkQueue.StopWorkQueue();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.powergridManager = Facepunch.Pool.Get<ProtoBuf.PowergridManager>();
			info.msg.powergridManager.fuseItemIds = Facepunch.Pool.Get<List<ItemId>>();
			int i = 0;
			for (int count = insertedFuses.Count; i < count; i++)
			{
				info.msg.powergridManager.fuseItemIds.Add(insertedFuses[i].Fuse.uid);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.powergridManager?.fuseItemIds != null)
		{
			if (loadedFuseInsertionOrder == null)
			{
				loadedFuseInsertionOrder = new List<ItemId>();
			}
			loadedFuseInsertionOrder.AddRange(info.msg.powergridManager.fuseItemIds);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Server_RestoreFuseInsertionOrder();
	}

	private void Server_RestoreFuseInsertionOrder()
	{
		if (loadedFuseInsertionOrder == null || loadedFuseInsertionOrder.Count == 0)
		{
			return;
		}
		using PooledList<InsertedFuseEntry> pooledList = Facepunch.Pool.Get<PooledList<InsertedFuseEntry>>();
		int i = 0;
		for (int count = loadedFuseInsertionOrder.Count; i < count; i++)
		{
			ItemId itemId = loadedFuseInsertionOrder[i];
			for (int j = 0; j < insertedFuses.Count; j++)
			{
				if (insertedFuses[j].Fuse.uid == itemId)
				{
					pooledList.Add(insertedFuses[j]);
					insertedFuses.RemoveAt(j);
					break;
				}
			}
		}
		int k = 0;
		for (int count2 = insertedFuses.Count; k < count2; k++)
		{
			pooledList.Add(insertedFuses[k]);
		}
		insertedFuses.Clear();
		insertedFuses.AddRange(pooledList);
		loadedFuseInsertionOrder = null;
	}

	public int Server_GetFuseSocketsCount()
	{
		return fuseSocketsCount;
	}

	public int Server_GetPowerPlantInsertedFuses()
	{
		int num = currentInsertedFusesCount + Powergrid.simulatePowerPlantFuses;
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public int CalculateCurrentStage()
	{
		if (!Powergrid.enabled)
		{
			return 0;
		}
		return PowergridStageConfig.instance.GetStageForFuseCount(Server_GetPowerPlantInsertedFuses());
	}

	private void ServerTick()
	{
		using (TimeWarning.New("PowergridManager.ServerTick"))
		{
			if (!Powergrid.enabled || !Rust.Application.isServerStarted)
			{
				return;
			}
			int num = Server_GetPowerPlantInsertedFuses();
			if (num != previousInsertedFusesCount || !hasTickedOnce)
			{
				previousInsertedFusesCount = num;
				int num2 = CalculateCurrentStage();
				if (CurrentStage != num2 || !hasTickedOnce)
				{
					if (Interface.CallHook("OnPowergridStageChange", this, num2) != null)
					{
						return;
					}
					Facepunch.Rust.Analytics.Azure.OnPowerGridStageChanged(CurrentStage, num2);
					CurrentStage = num2;
					bool b = num2 > 0;
					using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
					{
						flagsUpdateScope.Set(Flags.On, b);
					}
					Interface.CallHook("OnPowergridStageChanged", this, num2);
					stageChangeWorkQueue.RestartWorkQueue();
					int i = 0;
					for (int count = fuseBoxes.Count; i < count; i++)
					{
						fuseBoxes[i].Server_OnPowergridStageChanged();
					}
				}
			}
			hasTickedOnce = true;
		}
	}

	private static bool Server_IsActiveFuse(InsertedFuseEntry entry)
	{
		if (entry.FuseBox != null && !entry.FuseBox.IsDestroyed && entry.Fuse != null)
		{
			return entry.Fuse.hasCondition;
		}
		return false;
	}

	public static void Server_GatherInsertedFuses(List<Item> fuses)
	{
		int i = 0;
		for (int count = insertedFuses.Count; i < count; i++)
		{
			InsertedFuseEntry entry = insertedFuses[i];
			if (Server_IsActiveFuse(entry))
			{
				fuses.Add(entry.Fuse);
			}
		}
	}

	public static void Server_GatherFullDecayFuses(List<Item> fullDecayFuses)
	{
		int fuseFullDecayCount = Powergrid.fuseFullDecayCount;
		if (fuseFullDecayCount <= 0)
		{
			return;
		}
		int i = 0;
		for (int count = insertedFuses.Count; i < count; i++)
		{
			InsertedFuseEntry entry = insertedFuses[i];
			if (!Server_IsActiveFuse(entry))
			{
				continue;
			}
			float condition = entry.Fuse.condition;
			int num = fullDecayFuses.Count;
			for (int j = 0; j < fullDecayFuses.Count; j++)
			{
				if (condition < fullDecayFuses[j].condition)
				{
					num = j;
					break;
				}
			}
			if (num < fuseFullDecayCount)
			{
				fullDecayFuses.Insert(num, entry.Fuse);
				if (fullDecayFuses.Count > fuseFullDecayCount)
				{
					fullDecayFuses.RemoveAt(fullDecayFuses.Count - 1);
				}
			}
		}
	}

	public static float Server_GetSlowDecayRateScale(Item fuse)
	{
		float num = Mathf.Max(Powergrid.fuseSlowDecayFractionMin, 0f);
		float num2 = Mathf.Max(Powergrid.fuseSlowDecayFractionMax, num);
		if (num2 <= 0f)
		{
			return 0f;
		}
		if (num >= num2)
		{
			return num;
		}
		ulong value = fuse.uid.Value;
		uint x = (uint)(value ^ (value >> 32));
		return Mathf.Lerp(num, num2, SeedRandom.Wanghash01(ref x));
	}

	private void ServerFuseDeteriorationTick()
	{
		using (TimeWarning.New("PowergridManager.ServerFuseDeteriorationTick"))
		{
			float time = UnityEngine.Time.time;
			float deltaTime = time - lastFuseDeteriorationTickTime;
			lastFuseDeteriorationTickTime = time;
			if (!Powergrid.enabled || Powergrid.fuseLifespanSeconds <= 0f || !Rust.Application.isServerStarted)
			{
				return;
			}
			using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
			Server_GatherFullDecayFuses(pooledList);
			for (int num = insertedFuses.Count - 1; num >= 0; num--)
			{
				if (num < insertedFuses.Count)
				{
					InsertedFuseEntry entry = insertedFuses[num];
					if (Server_IsActiveFuse(entry))
					{
						float decayRateScale = (pooledList.Contains(entry.Fuse) ? 1f : Server_GetSlowDecayRateScale(entry.Fuse));
						entry.FuseBox.Server_DeteriorateFuse(entry.Fuse, deltaTime, decayRateScale);
					}
				}
			}
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 0:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: CurrentStage for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_CurrentStage);
			return true;
		case 1:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: LastProcessedStateChangeSqrDistance for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_LastProcessedStateChangeSqrDistance);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_CurrentStage;
				int _sync_CurrentStage = reader.Int32();
				__sync_CurrentStage = _sync_CurrentStage;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_LastProcessedStateChangeSqrDistance;
				float _sync_LastProcessedStateChangeSqrDistance = reader.Float();
				__sync_LastProcessedStateChangeSqrDistance = _sync_LastProcessedStateChangeSqrDistance;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (!(propertyName == "CurrentStage"))
		{
			if (propertyName == "LastProcessedStateChangeSqrDistance")
			{
				return 1;
			}
			return byte.MaxValue;
		}
		return 0;
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
		__sync_CurrentStage = 0;
		__sync_LastProcessedStateChangeSqrDistance = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}

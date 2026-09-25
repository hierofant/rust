using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class Marketplace : BaseEntity
{
	[Header("Marketplace")]
	public GameObjectRef terminalPrefab;

	public Transform[] terminalPoints;

	public Transform droneLaunchPoint;

	public GameObjectRef deliveryDronePrefab;

	public static readonly List<Marketplace> serverMarketplaces = new List<Marketplace>();

	[NonSerialized]
	public EntityRef<MarketTerminal>[] terminalEntities;

	private float currentCharge;

	private float lastChargeTickTime;

	private bool acceptingOrders;

	private bool hasAppliedAcceptingOrders;

	private bool hasRestoredCharge;

	private Action _actionChargeTick;

	private float __sync_ChargeFraction;

	[Sync(Autosave = true)]
	public float ChargeFraction
	{
		[CompilerGenerated]
		get
		{
			return __sync_ChargeFraction;
		}
		[CompilerGenerated]
		private set
		{
			if (!IsSyncVarEqual(__sync_ChargeFraction, value))
			{
				__sync_ChargeFraction = value;
				byte nameID = __GetWeaverID("ChargeFraction");
				QueueSyncVar(nameID);
			}
		}
	}

	private Action actionChargeTick => ServerChargeTick;

	public override void ServerInit()
	{
		base.ServerInit();
		serverMarketplaces.Add(this);
		lastChargeTickTime = UnityEngine.Time.time;
		InvokeRandomized(actionChargeTick, 1f, 1f, 0.1f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		serverMarketplaces.Remove(this);
		CancelInvoke(actionChargeTick);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Server_RestoreCharge();
	}

	public bool Server_CanAcceptOrder()
	{
		if (Powergrid.enabled)
		{
			return acceptingOrders;
		}
		return true;
	}

	public float Server_GetCurrentCharge()
	{
		return currentCharge;
	}

	private static float GetChargeCapacity()
	{
		return Mathf.Max(Powergrid.marketplaceChargeCapacity, 1f);
	}

	private void Server_RestoreCharge()
	{
		if (!hasRestoredCharge)
		{
			currentCharge = Mathf.Clamp01(ChargeFraction) * GetChargeCapacity();
			hasRestoredCharge = true;
			Server_RefreshChargeState();
		}
	}

	private void ServerChargeTick()
	{
		using (TimeWarning.New("Marketplace.ServerChargeTick"))
		{
			float time = UnityEngine.Time.time;
			float num = time - lastChargeTickTime;
			lastChargeTickTime = time;
			if (Rust.Application.isServerStarted && !Rust.Application.isLoadingSave)
			{
				Server_RestoreCharge();
				if (!Powergrid.enabled)
				{
					Server_SetAcceptingOrders(accepting: true);
					return;
				}
				currentCharge = Mathf.Clamp(currentCharge + Server_GetChargeRate() * num, 0f, GetChargeCapacity());
				Server_RefreshChargeState();
			}
		}
	}

	public float Server_GetChargeRate()
	{
		int num = ((PointEntity<PowergridManager>.ServerInstance != null) ? PointEntity<PowergridManager>.ServerInstance.Server_GetPowerPlantInsertedFuses() : 0);
		if (num <= 0)
		{
			return 0f - Mathf.Max(Powergrid.marketplaceDrainRate, 0f);
		}
		if (num < Powergrid.marketplaceMinimumFusesToCharge)
		{
			return 0f;
		}
		return (float)num * Mathf.Max(Powergrid.marketplaceChargePerFuse, 0f);
	}

	private void Server_RefreshChargeState()
	{
		if (!Powergrid.enabled)
		{
			Server_SetAcceptingOrders(accepting: true);
			return;
		}
		float num = GetChargeCapacity() * Mathf.Clamp01(Powergrid.marketplaceRequiredChargeFraction);
		Server_SetAcceptingOrders(currentCharge >= num);
		float num2 = Mathf.Round(Mathf.Clamp01(currentCharge / GetChargeCapacity()) * 100f) / 100f;
		if (!Mathf.Approximately(num2, ChargeFraction))
		{
			ChargeFraction = num2;
		}
	}

	private void Server_SetAcceptingOrders(bool accepting)
	{
		if (!hasAppliedAcceptingOrders || acceptingOrders != accepting)
		{
			acceptingOrders = accepting;
			hasAppliedAcceptingOrders = true;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags))
			{
				flagsUpdateScope.Set(Flags.On, accepting);
			}
			Server_RefreshTerminalPowerFlags();
		}
	}

	private void Server_RefreshTerminalPowerFlags()
	{
		if (terminalEntities == null)
		{
			return;
		}
		bool hasPower = Server_CanAcceptOrder();
		for (int i = 0; i < terminalEntities.Length; i++)
		{
			if (terminalEntities[i].TryGet(serverside: true, out var entity))
			{
				entity.Server_SetMarketplaceHasPower(hasPower);
			}
		}
	}

	public NetworkableId SendDrone(BasePlayer player, MarketTerminal sourceTerminal, VendingMachine vendingMachine)
	{
		if (sourceTerminal == null || vendingMachine == null)
		{
			return default(NetworkableId);
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(deliveryDronePrefab?.resourcePath, droneLaunchPoint.position, droneLaunchPoint.rotation);
		if (!(baseEntity is DeliveryDrone deliveryDrone))
		{
			baseEntity.Kill();
			return default(NetworkableId);
		}
		deliveryDrone.OwnerID = player.userID;
		deliveryDrone.Spawn();
		deliveryDrone.Setup(this, sourceTerminal, vendingMachine);
		return deliveryDrone.net.ID;
	}

	public void ReturnDrone(DeliveryDrone deliveryDrone)
	{
		if (deliveryDrone.sourceTerminal.TryGet(serverside: true, out var entity))
		{
			entity.CompleteOrder(deliveryDrone.targetVendingMachine.uid);
		}
		deliveryDrone.Kill();
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave)
		{
			SpawnSubEntities();
		}
	}

	private void SpawnSubEntities()
	{
		if (!base.isServer)
		{
			return;
		}
		if (terminalEntities != null && terminalEntities.Length > terminalPoints.Length)
		{
			for (int i = terminalPoints.Length; i < terminalEntities.Length; i++)
			{
				if (terminalEntities[i].TryGet(serverside: true, out var entity))
				{
					entity.Kill();
				}
			}
		}
		Array.Resize(ref terminalEntities, terminalPoints.Length);
		for (int j = 0; j < terminalPoints.Length; j++)
		{
			Transform transform = terminalPoints[j];
			if (!terminalEntities[j].TryGet(serverside: true, out var _))
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(terminalPrefab?.resourcePath, transform.position, transform.rotation);
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
				if (!(baseEntity is MarketTerminal marketTerminal))
				{
					Debug.LogError("Marketplace.terminalPrefab did not spawn a MarketTerminal (it spawned " + baseEntity.GetType().FullName + ")");
					baseEntity.Kill();
				}
				else
				{
					marketTerminal.Setup(this);
					terminalEntities[j].Set(marketTerminal);
				}
			}
		}
		Server_RefreshTerminalPowerFlags();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.subEntityList != null)
		{
			List<NetworkableId> subEntityIds = info.msg.subEntityList.subEntityIds;
			Array.Resize(ref terminalEntities, subEntityIds.Count);
			for (int i = 0; i < subEntityIds.Count; i++)
			{
				terminalEntities[i] = new EntityRef<MarketTerminal>(subEntityIds[i]);
			}
		}
		SpawnSubEntities();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.subEntityList = Facepunch.Pool.Get<SubEntityList>();
		info.msg.subEntityList.subEntityIds = Facepunch.Pool.Get<List<NetworkableId>>();
		if (terminalEntities != null)
		{
			for (int i = 0; i < terminalEntities.Length; i++)
			{
				info.msg.subEntityList.subEntityIds.Add(terminalEntities[i].uid);
			}
		}
	}

	private void OnSyncVar_ChargeFraction(float? oldValue, float newValue)
	{
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: ChargeFraction for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_ChargeFraction);
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
				float? oldValue = __sync_ChargeFraction;
				float newValue = (__sync_ChargeFraction = reader.Float());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_ChargeFraction(oldValue, newValue);
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
		if (propertyName == "ChargeFraction")
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
		__sync_ChargeFraction = 0f;
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

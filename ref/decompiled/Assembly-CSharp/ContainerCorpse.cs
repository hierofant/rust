#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ContainerCorpse : ConstructableEntity
{
	[Header("Container Corpse")]
	public Collider mainCollider;

	public const Flags Flag_Empty = Flags.Reserved13;

	private ProtoBuf.CodeLock codelockData;

	private ProtoBuf.KeyLock keylockData;

	private ulong lockOwnerId;

	private BuildingPrivlidge _cachedTc;

	private float _cacheTimeout;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ContainerCorpse.OnRpcMessage"))
		{
			if (rpc == 1735184033 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_RequestOwnerData");
				}
				using (TimeWarning.New("SERVER_RequestOwnerData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1735184033u, "SERVER_RequestOwnerData", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1735184033u, "SERVER_RequestOwnerData", this, player, 3f))
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
							SERVER_RequestOwnerData(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_RequestOwnerData");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static bool IsValidPointForEntity(uint prefabID, Vector3 point, Quaternion rotation, BaseEntity ignoredEntity = null, int mask = 536870912, bool ignoreChildrenOfEntity = false)
	{
		DeployVolume[] array = null;
		array = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
		using (PooledList<Type> pooledList = Facepunch.Pool.Get<PooledList<Type>>())
		{
			pooledList.Add(typeof(DebrisEntity));
			if (DeployVolume.Check(point, rotation, array, pooledList, DeployVolume.TypeFilterMode.Ignore, ignoredEntity, mask, ignoreChildrenOfEntity))
			{
				return false;
			}
		}
		Socket_Base[] array2 = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
		Construction.Target target = new Construction.Target
		{
			position = point,
			rotation = rotation.eulerAngles,
			onTerrain = (Mathf.Abs(TerrainMeta.HeightMap.GetHeight(point) - point.y) < 0.05f)
		};
		Construction.Placement placement = new Construction.Placement(target)
		{
			position = target.position,
			rotation = rotation
		};
		Socket_Base[] array3 = array2;
		foreach (Socket_Base socket_Base in array3)
		{
			if (socket_Base.male && !socket_Base.CheckSocketMods(ref placement))
			{
				return false;
			}
		}
		return true;
	}

	public bool InValidPosition()
	{
		int mask = (IsNearlyBuilt() ? 537001984 : 536870912);
		return IsValidPointForEntity(entityToSpawn.resourceID, base.transform.position, base.transform.rotation, this, mask);
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		bool flag = false;
		if (base.isServer)
		{
			flag = IsOwner(player);
		}
		if (flag && pickup.enabled)
		{
			if (pickup.requireHammer)
			{
				return player.IsHoldingEntity<Hammer>();
			}
			return true;
		}
		return false;
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock)
		{
			return false;
		}
		return base.HasSlot(slot);
	}

	public override void OnInventoryFirstCreated(ItemContainer container)
	{
		base.OnInventoryFirstCreated(container);
		base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
	}

	public void TakeFrom(ItemContainer[] source, float savePercent = 0f)
	{
		DroppedItemContainer.TakeFractionOfItems(source, base.inventory, savePercent);
		base.inventory.capacity = base.inventory.itemList.Count;
	}

	protected override bool CanRepair(BasePlayer player)
	{
		if (!RunBuildingChecks(player))
		{
			return false;
		}
		bool flag = IsOwner(player);
		if (flag && !InValidPosition())
		{
			if (DeployVolume.LastDeployHit != null)
			{
				BaseEntity baseEntity = GameObjectEx.ToBaseEntity(DeployVolume.LastDeployHit);
				if (baseEntity != null)
				{
					player.ShowBlockedByEntityToast(baseEntity, Construction.lastPlacementError);
				}
			}
			else
			{
				player.ShowToast(GameTip.Styles.Error, Construction.lastPlacementError, false);
			}
			return false;
		}
		if (flag)
		{
			return base.CanRepair(player);
		}
		return false;
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (isLootable)
		{
			return IsOwner(player);
		}
		return false;
	}

	private bool RunBuildingChecks(BasePlayer player)
	{
		BaseEntity baseEntity = entityToSpawn.Get()?.GetComponent<BaseEntity>();
		if (baseEntity == null)
		{
			Debug.LogError("Prefab not found for '" + entityToSpawn.resourcePath + "'");
			return false;
		}
		Construction construction = PrefabAttribute.server.Find<Construction>(baseEntity.prefabID);
		if (construction == null)
		{
			Debug.LogError("Construction not found on '" + entityToSpawn.resourcePath + "'");
			return false;
		}
		if (construction.isBuildingPrivilege)
		{
			BuildingPrivlidge componentInChildren = GetComponentInChildren<BuildingPrivlidge>();
			if (!player.CanPlaceBuildingPrivilege(base.transform.position, base.transform.rotation, construction.bounds, componentInChildren))
			{
				player.ShowToast(GameTip.Styles.Red_Normal, "Can't stack building privileges", false);
				return false;
			}
		}
		return true;
	}

	public override void OnRepairFinished(BasePlayer player)
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(entityToSpawn.resourcePath, base.transform.position, base.transform.rotation);
		if (HasParent())
		{
			baseEntity.SetParent(GetParentEntity(), worldPositionStays: true);
		}
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.skinID = skinID;
		baseEntity.Spawn();
		if (baseEntity is StorageContainer storageContainer)
		{
			storageContainer.MoveAllInventoryItems(base.inventory);
		}
		else if (baseEntity is ContainerIOEntity containerIOEntity)
		{
			containerIOEntity.MoveAllInventoryItems(base.inventory);
		}
		if (baseEntity is BuildingPrivlidge buildingPrivlidge)
		{
			BuildingPrivlidge componentInChildren = GetComponentInChildren<BuildingPrivlidge>();
			if (componentInChildren == null)
			{
				Debug.LogError("Can't copy auth list from corpse to TC: no BuildingPrivilege found in '" + base.PrefabName + "'");
			}
			else
			{
				buildingPrivlidge.SetAuthListFrom(componentInChildren);
			}
		}
		if (baseEntity is DecayEntity decayEntity)
		{
			decayEntity.AttachToBuilding(null);
		}
		SpawnLock(baseEntity);
		Kill();
		baseEntity.SendNetworkUpdateImmediate();
		if (spawnEffect.isValid)
		{
			Effect.server.Run(spawnEffect.resourcePath, base.transform.position, Vector3.up);
		}
	}

	public void SaveLock(BaseLock lockEntity)
	{
		SaveInfo saveInfo = default(SaveInfo);
		saveInfo.forDisk = true;
		saveInfo.cachedTime = ThreadSafeTime.TakeSnapshot();
		SaveInfo info = saveInfo;
		using (info.msg = Facepunch.Pool.Get<ProtoBuf.Entity>())
		{
			lockEntity.Save(info);
			codelockData = info.msg.codeLock?.Copy();
			keylockData = info.msg.keyLock?.Copy();
			lockOwnerId = lockEntity.OwnerID;
		}
	}

	private void SpawnLock(BaseEntity parent)
	{
		bool flag = false;
		BaseEntity baseEntity;
		if (codelockData != null)
		{
			baseEntity = GameManager.server.CreateEntity("Assets/Prefabs/Locks/Keypad/lock.code.prefab") as CodeLock;
			flag = codelockData.pv != null && !string.IsNullOrEmpty(codelockData.pv.code);
		}
		else
		{
			if (keylockData == null)
			{
				return;
			}
			baseEntity = GameManager.server.CreateEntity("Assets/Prefabs/Locks/keylock/lock.key.prefab") as KeyLock;
			flag = true;
		}
		baseEntity.SetParent(parent, parent.GetSlotAnchorName(Slot.Lock));
		baseEntity.OwnerID = lockOwnerId;
		if (baseEntity is CodeLock codeLock && codelockData != null)
		{
			codeLock.LoadCodelockPrivateData(codelockData);
		}
		else if (baseEntity is KeyLock keyLock && keylockData != null)
		{
			keyLock.LoadKeylockData(keylockData);
		}
		baseEntity.SetFlagLocal(Flags.Locked, flag);
		baseEntity.Spawn();
		parent.SetSlot(Slot.Lock, baseEntity);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (base.inventory.IsEmpty())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	private void SERVER_RequestOwnerData(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && IsOwner(player))
		{
			SendOwnerData(player);
		}
	}

	public virtual void SendOwnerData(BasePlayer player)
	{
		ClientRPC(RpcTarget.Player("CLIENT_ReceiveOwnerData", player));
	}

	public bool IsOwner(BasePlayer player)
	{
		if ((ulong)player.userID == base.OwnerID)
		{
			return true;
		}
		BuildingPrivlidge cachedTc = GetCachedTc();
		if (cachedTc != null && cachedTc.IsAuthed(player))
		{
			return true;
		}
		return false;
	}

	private BuildingPrivlidge GetCachedTc()
	{
		if (_cachedTc != null && _cachedTc.IsDestroyed)
		{
			_cachedTc = null;
		}
		if (_cachedTc == null || UnityEngine.Time.realtimeSinceStartup > _cacheTimeout)
		{
			_cachedTc = null;
			BuildingManager.Building building = GetBuilding();
			if (building != null)
			{
				_cachedTc = building.GetDominatingBuildingPrivilege();
			}
			if (_cachedTc == null)
			{
				return GetNearestBuildingPrivilege(cached: true, 3f);
			}
			_cacheTimeout = UnityEngine.Time.realtimeSinceStartup + 3f;
		}
		return _cachedTc;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (base.inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = base.inventory.Save();
			}
			ContainerCorpseData containerCorpseData = Facepunch.Pool.Get<ContainerCorpseData>();
			info.msg.containerCorpse = containerCorpseData;
			containerCorpseData.codeLock = codelockData;
			containerCorpseData.keyLock = keylockData;
			containerCorpseData.lockOwnerId = lockOwnerId;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.containerCorpse != null)
		{
			codelockData = info.msg.containerCorpse.codeLock?.Copy();
			keylockData = info.msg.containerCorpse.keyLock?.Copy();
			lockOwnerId = info.msg.containerCorpse.lockOwnerId;
		}
	}
}

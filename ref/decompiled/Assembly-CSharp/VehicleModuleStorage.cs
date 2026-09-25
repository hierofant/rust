#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleStorage : VehicleModuleSeating
{
	[Serializable]
	public class Storage
	{
		public GameObjectRef storageUnitPrefab;

		public Transform storageUnitPoint;
	}

	[SerializeField]
	private Storage storage;

	[SerializeField]
	private GameObjectRef crudeExplosionEntity;

	[SerializeField]
	private GameObjectRef crudeExplosionEffect;

	[SerializeField]
	private float explosionThreshold = 50f;

	[SerializeField]
	private float minimumTimeBetweenExplosions = 5f;

	[SerializeField]
	private int oilToConsumePerExplosion = 100;

	private EntityRef storageUnitInstance;

	private static ItemDefinition _crudeItem = null;

	public static readonly Translate.Phrase StorageCantBeMovedError = new Translate.Phrase("error.itemsinstorage", "Cannot move item: Storage contains items!");

	private TimeSince lastExplosionSpawned;

	public static ItemDefinition CrudeItem => _crudeItem ?? (_crudeItem = ItemManager.FindItemDefinition("crude.oil"));

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleStorage.OnRpcMessage"))
		{
			if (rpc == 4254195175u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Open");
				}
				using (TimeWarning.New("RPC_Open"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4254195175u, "RPC_Open", this, player, 3f))
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
							RPC_Open(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Open");
					}
				}
				return true;
			}
			if (rpc == 425471188 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_TryOpenWithKeycode");
				}
				using (TimeWarning.New("RPC_TryOpenWithKeycode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(425471188u, "RPC_TryOpenWithKeycode", this, player, 3f))
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
							RPC_TryOpenWithKeycode(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_TryOpenWithKeycode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public IItemContainerEntity GetContainer()
	{
		BaseEntity baseEntity = storageUnitInstance.Get(base.isServer);
		if (baseEntity != null && baseEntity.IsValid())
		{
			return baseEntity as IItemContainerEntity;
		}
		return null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		storageUnitInstance.uid = info.msg.simpleUID.uid;
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public BaseEntity GetStorageUnitInstance()
	{
		return storageUnitInstance.Get(base.isServer);
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave && storage.storageUnitPoint.gameObject.activeSelf)
		{
			CreateStorageEntity();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container))
		{
			InitStorageEvents(container.inventory);
		}
	}

	private void OnItemAddedRemoved(Item item, bool add)
	{
		AssociatedItemInstance?.LockUnlock(!CanBeMovedNowOnVehicle());
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			IItemContainerEntity container = GetContainer();
			if (!ObjectEx.IsUnityNull(container))
			{
				container.DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.simpleUID = Facepunch.Pool.Get<SimpleUID>();
		info.msg.simpleUID.uid = storageUnitInstance.uid;
	}

	[UnityEvent]
	public void CreateStorageEntity()
	{
		if (IsFullySpawned() && base.isServer && !storageUnitInstance.IsValid(base.isServer))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(storage.storageUnitPrefab.resourcePath, storage.storageUnitPoint.localPosition, storage.storageUnitPoint.localRotation);
			storageUnitInstance.Set(baseEntity);
			baseEntity.SetParent(this);
			baseEntity.Spawn();
			InitStorageEvents(GetContainer().inventory);
		}
	}

	protected virtual void InitStorageEvents(ItemContainer container)
	{
		container.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(container.onItemAddedRemoved, new Action<Item, bool>(OnItemAddedRemoved));
	}

	[UnityEvent]
	public void DestroyStorageEntity()
	{
		if (!IsFullySpawned() || !base.isServer)
		{
			return;
		}
		BaseEntity baseEntity = storageUnitInstance.Get(base.isServer);
		if (baseEntity.IsValid())
		{
			if (baseEntity is BaseCombatEntity baseCombatEntity)
			{
				baseCombatEntity.Die();
			}
			else
			{
				baseEntity.Kill();
			}
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_Open(RPCMessage msg)
	{
		TryOpen(msg.player);
	}

	private bool TryOpen(BasePlayer player)
	{
		if (!player.IsValid() || !CanBeLooted(player))
		{
			return false;
		}
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container))
		{
			container.PlayerOpenLoot(player);
		}
		else
		{
			Debug.LogError(GetType().Name + ": No container component found.");
		}
		return true;
	}

	protected override bool CanBeMovedNowOnVehicle()
	{
		IItemContainerEntity container = GetContainer();
		if (!ObjectEx.IsUnityNull(container) && !container.inventory.IsEmpty())
		{
			return false;
		}
		return true;
	}

	public override Translate.Phrase CannotBeMovedNowReason()
	{
		return StorageCantBeMovedError;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_TryOpenWithKeycode(RPCMessage msg)
	{
		if (!base.IsOnACar)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			string codeEntered = msg.read.String();
			if (base.Car.CarLock.TryOpenWithCode(player, codeEntered))
			{
				TryOpen(player);
			}
			else
			{
				base.Car.ClientRPC(RpcTarget.NetworkGroup("CodeEntryFailed"));
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		TrySpawnExplosion(info);
		base.Hurt(info);
	}

	private void TrySpawnExplosion(HitInfo info)
	{
		if (!((float)lastExplosionSpawned > minimumTimeBetweenExplosions) || !(info.damageTypes.Get(DamageType.Explosion) > explosionThreshold) || !(storageUnitInstance.Get(serverside: true) is LiquidContainer liquidContainer))
		{
			return;
		}
		Item liquidItem = liquidContainer.GetLiquidItem();
		if (liquidItem == null || !(liquidItem.info.shortname == "crude.oil") || liquidItem.amount < oilToConsumePerExplosion)
		{
			return;
		}
		liquidItem.UseItem(oilToConsumePerExplosion);
		lastExplosionSpawned = 0f;
		if (!crudeExplosionEntity.isValid)
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(crudeExplosionEntity.resourcePath, base.transform.position, base.transform.rotation);
		if (!(baseEntity == null))
		{
			ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
			baseEntity.Spawn();
			if (component != null && component.TryGetComponent<TimedExplosive>(out var component2))
			{
				component2.creatorEntity = creatorEntity;
				component2.Explode();
			}
			if (crudeExplosionEffect.isValid)
			{
				Effect.server.Run(crudeExplosionEffect.resourcePath, baseEntity.transform.position);
			}
		}
	}
}

#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ContainerIOEntity : IOEntity, IItemContainerEntity, IIdealSlotEntity, ILootableEntity, IInventoryProvider, LootPanel.IHasLootPanel, IContainerSounds
{
	[Header("ContainerIOEntity")]
	public ItemDefinition onlyAllowedItem;

	public ItemContainer.ContentsType allowedContents = ItemContainer.ContentsType.Generic;

	public int maxStackSize = 1;

	public int numSlots;

	public string lootPanelName = "generic";

	public Translate.Phrase panelTitle = new Translate.Phrase("loot", "Loot");

	public bool needsBuildingPrivilegeToUse;

	public bool requireAuthIfNotLocked;

	public bool isLootable = true;

	public bool isMonitorable;

	public bool dropsLoot;

	public float dropLootDestroyPercent;

	public bool dropFloats;

	public bool onlyOneUser;

	public bool supportsChildDeployables;

	public bool allowSorting;

	public bool isLockable;

	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	private ItemContainer _inventory;

	private IndustrialStorageAdaptor cachedAdaptor;

	public Translate.Phrase LootPanelTitle => panelTitle;

	public ItemContainer inventory => _inventory;

	public Transform Transform => base.transform;

	public bool DropsLoot => dropsLoot;

	public bool DropFloats => dropFloats;

	public float DestroyLootPercent
	{
		get
		{
			if (!ShouldDropDeployableCorpse(lastAttacker, lastDamage))
			{
				return dropLootDestroyPercent;
			}
			return 0f;
		}
	}

	public ulong LastLootedBy { get; set; }

	public BasePlayer LastLootedByPlayer { get; set; }

	public override bool PreserveChildrenWhenReskinning => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ContainerIOEntity.OnRpcMessage"))
		{
			if (rpc == 331989034 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenLoot");
				}
				using (TimeWarning.New("RPC_OpenLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(331989034u, "RPC_OpenLoot", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_OpenLoot(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenLoot");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		pickupErrorToFormat.arg0 = pickup.itemTarget.displayName;
		if (isLockable && GetSlot(Slot.Lock) != null)
		{
			pickupErrorToFormat.format = PickupErrors.ItemHasLock;
			return false;
		}
		if (children.Count != 0)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		if (pickup.requireEmptyInv && _inventory != null && !_inventory.IsEmpty())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemInventoryMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public override void ServerInit()
	{
		if (_inventory == null)
		{
			CreateInventory(giveUID: true);
			OnInventoryFirstCreated(_inventory);
		}
		base.ServerInit();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Facepunch.Pool.Free(ref _inventory);
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventory(giveUID: false);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (_inventory != null && !_inventory.uid.IsValid)
		{
			_inventory.GiveUID();
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Open, b: false);
	}

	public void CreateInventory(bool giveUID)
	{
		Debug.Assert(_inventory == null, "Double init of inventory!");
		_inventory = Facepunch.Pool.Get<ItemContainer>();
		_inventory.entityOwner = this;
		_inventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		_inventory.SetOnlyAllowedItem(onlyAllowedItem);
		_inventory.maxStackSize = maxStackSize;
		_inventory.ServerInitialize(null, numSlots);
		if (giveUID)
		{
			_inventory.GiveUID();
		}
		_inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		_inventory.onDirty += OnInventoryDirty;
	}

	public virtual void ReceiveInventoryFromItem(Item item)
	{
		if (item.contents != null)
		{
			MoveAllInventoryItems(item.contents, inventory);
		}
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		if (createdItem != null && createdItem.contents != null)
		{
			MoveAllInventoryItems(_inventory, createdItem.contents);
			return;
		}
		for (int i = 0; i < _inventory.capacity; i++)
		{
			Item slot = _inventory.GetSlot(i);
			if (slot != null)
			{
				slot.RemoveFromContainer();
				player.GiveItem(slot, GiveItemReason.PickedUp);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (_inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = _inventory.Save();
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public virtual void OnInventoryFirstCreated(ItemContainer container)
	{
	}

	public virtual void OnItemAddedOrRemoved(Item item, bool added)
	{
	}

	protected virtual void OnInventoryDirty()
	{
		if (HasAttachedStorageAdaptor(out var adaptor))
		{
			adaptor.RequestSort();
		}
		InvalidateNetworkCache();
	}

	public override void OnDied(HitInfo info)
	{
		DropItems();
		base.OnDied(info);
	}

	public void DropItems(BaseEntity initiator = null)
	{
		StorageContainer.DropItems(this, initiator);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (_inventory != null)
		{
			BasePlayer player = rpc.player;
			if ((bool)player && player.CanInteract() && !player.IsBlockedFromLootingByMountable())
			{
				PlayerOpenLoot(player, lootPanelName);
			}
		}
	}

	public virtual bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if (!CanBeLooted(player))
		{
			return false;
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock != null && !baseLock.OnTryToOpen(player))
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
			return false;
		}
		return true;
	}

	public virtual bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (Interface.CallHook("CanLootEntity", player, this) != null)
		{
			return false;
		}
		if ((needsBuildingPrivilegeToUse || StorageContainer.ShouldRequireAuthIfNoCodelock(this, requireAuthIfNotLocked)) && !player.CanBuild())
		{
			return false;
		}
		if (needsBuildingPrivilegeToUse && !player.CanBuild())
		{
			return false;
		}
		if ((onlyOneUser && IsOpen()) || IsTransferring())
		{
			player.ShowToast(GameTip.Styles.Red_Normal, StorageContainer.LockedMessage, false);
			return false;
		}
		if (panelToOpen == "")
		{
			panelToOpen = lootPanelName;
		}
		if (!CanOpenLootPanel(player, panelToOpen))
		{
			return false;
		}
		if (player.inventory.loot.StartLootingEntity(this, doPositionChecks))
		{
			SetFlagLocal(Flags.Open, b: true);
			player.inventory.loot.AddContainer(_inventory);
			player.inventory.loot.SendImmediate();
			player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), panelToOpen);
			SendNetworkUpdate();
			return true;
		}
		return false;
	}

	public virtual void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		SetFlagLocal(Flags.Open, b: false);
		SendNetworkUpdate();
	}

	public bool ShouldDropItemsIndividually()
	{
		return false;
	}

	public virtual int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		return -1;
	}

	public virtual ItemContainerId GetIdealContainer(BasePlayer player, Item item, ItemMoveModifier modifier)
	{
		return default(ItemContainerId);
	}

	public virtual void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
	}

	public void GetAllInventories(List<ItemContainer> list)
	{
		list.Add(inventory);
	}

	public override void OnDeployableCorpseSpawned(BaseEntity corpse)
	{
		base.OnDeployableCorpseSpawned(corpse);
		if (corpse is ContainerCorpse containerCorpse)
		{
			containerCorpse.TakeFrom(new ItemContainer[1] { inventory }, StorageContainer.corpseItemsSavedPercent);
		}
	}

	public bool MoveAllInventoryItems(ItemContainer source, ItemContainer dest)
	{
		return StorageContainer.MoveAllInventoryItems(source, dest);
	}

	public bool MoveAllInventoryItems(ItemContainer from)
	{
		return MoveAllInventoryItems(from, inventory);
	}

	public override bool HasSlot(Slot slot)
	{
		if (isLockable && slot == Slot.Lock)
		{
			return true;
		}
		if (isMonitorable && slot == Slot.StorageMonitor)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public bool HasAttachedStorageAdaptor(out IndustrialStorageAdaptor adaptor)
	{
		using (TimeWarning.New("HasAttachedStorageAdaptor"))
		{
			if (cachedAdaptor != null)
			{
				adaptor = cachedAdaptor;
				return true;
			}
			adaptor = null;
			if (children == null || children.Count == 0)
			{
				return false;
			}
			foreach (BaseEntity child in children)
			{
				if (child is IndustrialStorageAdaptor industrialStorageAdaptor)
				{
					adaptor = industrialStorageAdaptor;
					cachedAdaptor = industrialStorageAdaptor;
					return true;
				}
			}
			return false;
		}
	}

	protected bool HasAttachedStorageMonitor()
	{
		if (children == null)
		{
			return false;
		}
		foreach (BaseEntity child in children)
		{
			if (child is StorageMonitor)
			{
				return true;
			}
		}
		return false;
	}

	public bool OccupiedCheck(BasePlayer player = null)
	{
		if (player != null && player.inventory.loot.entitySource == this)
		{
			return true;
		}
		if (onlyOneUser)
		{
			return !IsOpen();
		}
		return true;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.storageBox != null)
		{
			if (_inventory != null)
			{
				_inventory.Load(info.msg.storageBox.contents);
				_inventory.capacity = numSlots;
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public override bool SupportsChildDeployables()
	{
		return supportsChildDeployables;
	}
}

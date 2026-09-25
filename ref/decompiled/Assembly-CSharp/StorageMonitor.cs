using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class StorageMonitor : AppIOEntity
{
	public readonly Action<Item, bool> _onItemAddedRemoved;

	private readonly Action<Item, int> _onItemAddedToStack;

	private readonly Action<Item, int> _onItemRemovedFromStack;

	private readonly Action _resetSwitchHandler;

	private double _lastPowerOnUpdate;

	public override AppEntityType Type => AppEntityType.StorageMonitor;

	public override bool Value
	{
		get
		{
			return IsOn();
		}
		set
		{
		}
	}

	public StorageMonitor()
	{
		_onItemAddedRemoved = OnItemAddedRemoved;
		_onItemAddedToStack = OnItemAddedToStack;
		_onItemRemovedFromStack = OnItemRemovedFromStack;
		_resetSwitchHandler = ResetSwitch;
	}

	internal override void FillEntityPayload(AppEntityPayload payload)
	{
		base.FillEntityPayload(payload);
		StorageContainer storageContainer = GetStorageContainer();
		ContainerIOEntity containerIOEntity = GetContainerIOEntity();
		if ((storageContainer == null && containerIOEntity == null) || !HasFlag(Flags.Reserved8))
		{
			return;
		}
		payload.items = Pool.Get<List<AppEntityPayload.Item>>();
		ItemContainer inventory = GetInventory();
		if (inventory == null)
		{
			return;
		}
		foreach (Item item2 in inventory.itemList)
		{
			AppEntityPayload.Item item = Pool.Get<AppEntityPayload.Item>();
			item.itemId = (item2.IsBlueprint() ? item2.blueprintTargetDef.itemid : item2.info.itemid);
			item.quantity = item2.amount;
			item.itemIsBlueprint = item2.IsBlueprint();
			payload.items.Add(item);
		}
		payload.capacity = inventory.capacity;
		if (storageContainer != null && storageContainer is BuildingPrivlidge buildingPrivlidge)
		{
			payload.hasProtection = true;
			float protectedMinutes = buildingPrivlidge.GetProtectedMinutes();
			if (protectedMinutes > 0f)
			{
				payload.protectionExpiry = (uint)DateTimeOffset.UtcNow.AddMinutes(protectedMinutes).ToUnixTimeSeconds();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		ItemContainer inventory = GetInventory();
		if (inventory != null)
		{
			inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Combine(inventory.onItemAddedRemoved, _onItemAddedRemoved);
			inventory.onItemAddedToStack = (Action<Item, int>)Delegate.Combine(inventory.onItemAddedToStack, _onItemAddedToStack);
			inventory.onItemRemovedFromStack = (Action<Item, int>)Delegate.Combine(inventory.onItemRemovedFromStack, _onItemRemovedFromStack);
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		ItemContainer inventory = GetInventory();
		if (inventory != null)
		{
			inventory.onItemAddedRemoved = (Action<Item, bool>)Delegate.Remove(inventory.onItemAddedRemoved, _onItemAddedRemoved);
			inventory.onItemAddedToStack = (Action<Item, int>)Delegate.Remove(inventory.onItemAddedToStack, _onItemAddedToStack);
			inventory.onItemRemovedFromStack = (Action<Item, int>)Delegate.Remove(inventory.onItemRemovedFromStack, _onItemRemovedFromStack);
		}
	}

	private StorageContainer GetStorageContainer()
	{
		return GetParentEntity() as StorageContainer;
	}

	private ContainerIOEntity GetContainerIOEntity()
	{
		return GetParentEntity() as ContainerIOEntity;
	}

	private ItemContainer GetInventory()
	{
		StorageContainer storageContainer = GetStorageContainer();
		ContainerIOEntity containerIOEntity = GetContainerIOEntity();
		if (storageContainer == null && containerIOEntity != null && containerIOEntity.inventory != null)
		{
			return containerIOEntity.inventory;
		}
		if (storageContainer != null && storageContainer.inventory != null && containerIOEntity == null)
		{
			return storageContainer.inventory;
		}
		return null;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		switch (outputSlot)
		{
		case 0:
			if (!IsOn())
			{
				return 0;
			}
			return Mathf.Min(1, GetCurrentEnergy());
		case 1:
		{
			int num = GetCurrentEnergy();
			if (!IsOn())
			{
				return num;
			}
			return num - 1;
		}
		default:
			return 0;
		}
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		bool flag = HasFlag(Flags.Reserved8);
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputSlot == 0)
		{
			bool num = inputAmount >= ConsumptionAmount();
			double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
			if (num && !flag && _lastPowerOnUpdate < realtimeSinceStartup - 1.0)
			{
				_lastPowerOnUpdate = realtimeSinceStartup;
				BroadcastValueChange();
			}
		}
	}

	private void OnItemAddedRemoved(Item item, bool added)
	{
		OnContainerChanged();
	}

	private void OnItemAddedToStack(Item item, int amount)
	{
		OnContainerChanged();
	}

	private void OnItemRemovedFromStack(Item item, int amount)
	{
		OnContainerChanged();
	}

	public void OnContainerChanged()
	{
		if (HasFlag(Flags.Reserved8))
		{
			Invoke(_resetSwitchHandler, 0.5f);
			if (!IsOn())
			{
				SetFlagLocal(Flags.On, b: true);
				SendNetworkUpdate_Flags();
				MarkDirty();
				BroadcastValueChange();
			}
		}
	}

	private void ResetSwitch()
	{
		SetFlagLocal(Flags.On, b: false);
		SendNetworkUpdate_Flags();
		MarkDirty();
		BroadcastValueChange();
	}
}

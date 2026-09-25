#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerLoot : EntityComponent<BasePlayer>
{
	public BaseEntity entitySource;

	public Item itemSource;

	public List<ItemContainer> containers = new List<ItemContainer>();

	public bool PositionChecks = true;

	private bool isInvokingSendUpdate;

	private Action SendUpdateAction;

	private Action _markDirtyCallback;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerLoot.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsLooting()
	{
		return containers.Count > 0;
	}

	public bool IsLooting(ItemContainer container)
	{
		return containers.Contains(container);
	}

	public float GetRadsInPlayerLoot()
	{
		float num = 0f;
		foreach (ItemContainer container in containers)
		{
			num += container.GetRadioactiveMaterialInContainer();
		}
		return num;
	}

	public void Clear()
	{
		if (!IsLooting())
		{
			return;
		}
		Interface.CallHook("OnPlayerLootEnd", this);
		base.baseEntity.HasClosedLoot();
		MarkDirty();
		if ((bool)entitySource)
		{
			entitySource.SendMessage("PlayerStoppedLooting", base.baseEntity, SendMessageOptions.DontRequireReceiver);
		}
		foreach (ItemContainer container in containers)
		{
			if (container != null)
			{
				container.onDirty -= _markDirtyCallback;
			}
		}
		ClearContainers();
		entitySource = null;
		itemSource = null;
	}

	public ItemContainer FindContainer(ItemContainerId id)
	{
		Check();
		if (!IsLooting())
		{
			return null;
		}
		foreach (ItemContainer container in containers)
		{
			ItemContainer itemContainer = container.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
		}
		return null;
	}

	public Item FindItem(ItemId id)
	{
		Check();
		if (!IsLooting())
		{
			return null;
		}
		foreach (ItemContainer container in containers)
		{
			Item item = container.FindItemByUID(id);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		return null;
	}

	public void Check()
	{
		if (!IsLooting() || !base.baseEntity.isServer)
		{
			return;
		}
		if (entitySource == null)
		{
			base.baseEntity.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.LootableDoesntExist, false);
			Clear();
		}
		else if (!entitySource.CanBeLooted(base.baseEntity) || entitySource.IsTransferring())
		{
			Clear();
		}
		else
		{
			if (!PositionChecks)
			{
				return;
			}
			float num = entitySource.Distance(base.baseEntity.eyes.position);
			if (num > 3f)
			{
				LootDistanceOverride component = entitySource.GetComponent<LootDistanceOverride>();
				if (component == null || num > component.amount)
				{
					Clear();
				}
			}
		}
	}

	public void MarkDirty()
	{
		if (!isInvokingSendUpdate)
		{
			isInvokingSendUpdate = true;
			if (SendUpdateAction == null)
			{
				SendUpdateAction = SendUpdate;
			}
			Invoke(SendUpdateAction, 0.1f);
		}
	}

	public void SendImmediate()
	{
		if (isInvokingSendUpdate)
		{
			isInvokingSendUpdate = false;
			CancelInvoke(SendUpdateAction);
		}
		SendUpdate();
	}

	private void SendUpdate()
	{
		isInvokingSendUpdate = false;
		if (!base.baseEntity.IsValid() || Interface.CallHook("OnLootNetworkUpdate", this) != null)
		{
			return;
		}
		using PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		if ((bool)entitySource && entitySource.net != null)
		{
			playerUpdateLoot.entityID = entitySource.net.ID;
		}
		if (itemSource != null)
		{
			playerUpdateLoot.itemID = itemSource.uid;
		}
		if (containers.Count > 0)
		{
			playerUpdateLoot.containers = Pool.Get<List<ProtoBuf.ItemContainer>>();
			foreach (ItemContainer container in containers)
			{
				playerUpdateLoot.containers.Add(container.Save());
			}
		}
		base.baseEntity.ClientRPC(RpcTarget.Player("UpdateLoot", base.baseEntity), playerUpdateLoot);
	}

	public bool StartLootingEntity(BaseEntity targetEntity, bool doPositionChecks = true)
	{
		Clear();
		if (!targetEntity)
		{
			return false;
		}
		if (!targetEntity.OnStartBeingLooted(base.baseEntity))
		{
			return false;
		}
		Assert.IsTrue(targetEntity.isServer, "Assure is server");
		PositionChecks = doPositionChecks;
		entitySource = targetEntity;
		itemSource = null;
		Interface.CallHook("OnLootEntity", GetComponent<BasePlayer>(), targetEntity);
		MarkDirty();
		if (targetEntity is ILootableEntity lootableEntity)
		{
			lootableEntity.LastLootedBy = base.baseEntity.userID;
			lootableEntity.LastLootedByPlayer = base.baseEntity;
		}
		return true;
	}

	public void AddContainer(ItemContainer container)
	{
		if (container != null)
		{
			containers.Add(container);
			if (_markDirtyCallback == null)
			{
				_markDirtyCallback = MarkDirty;
			}
			container.onDirty += _markDirtyCallback;
			if (container.entityOwner != null)
			{
				base.baseEntity.ProcessMissionEvent(BaseMission.MissionEventType.OPEN_STORAGE, new BaseMission.MissionEventPayload
				{
					UintIdentifier = container.entityOwner.prefabID,
					NetworkIdentifier = container.entityOwner.net.ID,
					WorldPosition = container.entityOwner.transform.position
				}, 0f);
			}
		}
	}

	public void RemoveContainer(ItemContainer container)
	{
		if (container != null)
		{
			container.onDirty -= _markDirtyCallback;
			containers.Remove(container);
		}
	}

	public bool RemoveContainerAt(int index)
	{
		if (index < 0 || index >= containers.Count)
		{
			return false;
		}
		if (containers[index] != null)
		{
			containers[index].onDirty -= _markDirtyCallback;
		}
		containers.RemoveAt(index);
		return true;
	}

	public void StartLootingItem(Item item)
	{
		Clear();
		if (item != null && item.contents != null)
		{
			PositionChecks = true;
			containers.Add(item.contents);
			if (_markDirtyCallback == null)
			{
				_markDirtyCallback = MarkDirty;
			}
			item.contents.onDirty += _markDirtyCallback;
			itemSource = item;
			entitySource = item.GetWorldEntity();
			Interface.CallHook("OnLootItem", GetComponent<BasePlayer>(), item);
			MarkDirty();
		}
	}

	private void ClearContainers()
	{
		containers.Clear();
	}
}

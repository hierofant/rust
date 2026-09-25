#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ItemBasedFlowRestrictor : IOEntity, IContainerSounds, PlayerInventory.ICanMoveFrom
{
	public const Flags HasPassthrough = Flags.Reserved1;

	public const Flags Sparks = Flags.Reserved3;

	[Header("Item Based Flow Restrictor")]
	public ItemDefinition[] validPassthroughItems;

	public ItemContainer.ContentsType allowedContents = ItemContainer.ContentsType.Generic;

	public int maxStackSize = 1;

	public int numSlots;

	public string lootPanelName = "generic";

	[Header("Item Decay/Destroying")]
	public float passthroughItemConditionLossPerSec = 1f;

	public int destroyPassthroughItemEveryXSeconds;

	[Header("Conditional Locking")]
	public bool lockInventoryWhenItemPresent;

	public bool lockOnlyWithPower = true;

	[Header("Sounds")]
	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	public ItemContainer inventory;

	private int lastDestroyedItemTimer;

	private static readonly Translate.Phrase PullLockedError = new Translate.Phrase("error.pull_locked", "Item is locked in!");

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ItemBasedFlowRestrictor.OnRpcMessage"))
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

	public override void ResetIOState()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if (inventory != null && inventory.TryGetItemAtSlot(0, out var item))
		{
			item.Drop(debugOrigin.transform.position + base.transform.forward * 0.5f, GetInheritedDropVelocity() + base.transform.forward * 2f);
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!HasFlag(Flags.Reserved1))
		{
			return 0;
		}
		return base.GetPassthroughAmount(outputSlot);
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, IsPowered());
		flagsUpdateScope.Set(Flags.Reserved1, HasPassthroughItem());
		flagsUpdateScope.Set(Flags.Reserved3, IsOn() && !HasFlag(Flags.Reserved1));
	}

	public virtual bool HasPassthroughItem()
	{
		if (inventory == null || inventory.itemList.Count <= 0)
		{
			return false;
		}
		if (!inventory.TryGetItemAtSlot(0, out var item))
		{
			return false;
		}
		if (!IsValidPassthroughItem(item))
		{
			return false;
		}
		if (passthroughItemConditionLossPerSec > 0f && item.hasCondition && item.conditionNormalized <= 0f)
		{
			return false;
		}
		return true;
	}

	public bool IsValidPassthroughItem(Item item)
	{
		return IsValidPassthroughItem(item.info);
	}

	public bool IsValidPassthroughItem(ItemDefinition itemDefinition)
	{
		ItemDefinition[] array = validPassthroughItems;
		foreach (ItemDefinition itemDefinition2 in array)
		{
			if (itemDefinition == itemDefinition2)
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool GetPassthroughItem(out Item item)
	{
		item = null;
		if (inventory == null || inventory.itemList.Count <= 0)
		{
			return false;
		}
		if (!inventory.TryGetItemAtSlot(0, out item))
		{
			return false;
		}
		if (!IsValidPassthroughItem(item))
		{
			return false;
		}
		if (passthroughItemConditionLossPerSec > 0f && item.hasCondition && item.conditionNormalized <= 0f)
		{
			return false;
		}
		return true;
	}

	public virtual void TickPassthroughItemDecay()
	{
		if (inventory != null && inventory.itemList.Count > 0 && ShouldDestroyOrDecayItem() && HasFlag(Flags.On))
		{
			DecayPassthroughItem(passthroughItemConditionLossPerSec);
		}
	}

	public void DecayPassthroughItem(float amount)
	{
		if (passthroughItemConditionLossPerSec > 0f && inventory.TryGetItemAtSlot(0, out var item) && item.hasCondition)
		{
			item.LoseCondition(amount);
		}
	}

	public void TickDestroyPassthroughItem()
	{
		if (!IsOn() || !HasFlag(Flags.Reserved1) || destroyPassthroughItemEveryXSeconds <= 0 || !ShouldDestroyOrDecayItem())
		{
			return;
		}
		lastDestroyedItemTimer++;
		if (lastDestroyedItemTimer >= destroyPassthroughItemEveryXSeconds)
		{
			lastDestroyedItemTimer = 0;
			if (inventory.TryGetItemAtSlot(0, out var item))
			{
				item.UseItem();
				OnItemAddedOrRemoved(item, added: false);
			}
		}
	}

	protected virtual bool ShouldDestroyOrDecayItem()
	{
		return true;
	}

	public override void ServerInit()
	{
		if (inventory == null)
		{
			CreateInventory(giveUID: true);
			OnInventoryFirstCreated(inventory);
		}
		InvokeRandomized(TickPassthroughItemDecay, 1f, 1f, 0.015f);
		if (destroyPassthroughItemEveryXSeconds > 0)
		{
			InvokeRepeating(TickDestroyPassthroughItem, 1f, 1f);
		}
		base.ServerInit();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Facepunch.Pool.Free(ref inventory);
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventory(giveUID: false);
	}

	public void CreateInventory(bool giveUID)
	{
		Debug.Assert(inventory == null, "Double init of inventory!");
		inventory = Facepunch.Pool.Get<ItemContainer>();
		inventory.entityOwner = this;
		inventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		inventory.SetOnlyAllowedItems(validPassthroughItems);
		inventory.maxStackSize = maxStackSize;
		inventory.ServerInitialize(null, numSlots);
		if (giveUID)
		{
			inventory.GiveUID();
		}
		inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = inventory.Save();
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.storageBox != null)
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
				inventory.capacity = numSlots;
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
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, HasPassthroughItem());
		}
		MarkDirty();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (inventory != null)
		{
			BasePlayer player = rpc.player;
			if ((bool)player && player.CanInteract() && player.inventory.loot.StartLootingEntity(this))
			{
				SetFlagLocal(Flags.Open, b: true);
				player.inventory.loot.AddContainer(inventory);
				player.inventory.loot.SendImmediate();
				player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), lootPanelName);
				SendNetworkUpdate();
			}
		}
	}

	public void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
	}

	public PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		return new PlayerInventory.CanMoveFromResponse(!lockInventoryWhenItemPresent || (lockOnlyWithPower && !IsPowered()) || !HasPassthroughItem(), PullLockedError);
	}
}

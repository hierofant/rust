#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class DroppedItemContainer : BaseCombatEntity, LootPanel.IHasLootPanel, IContainerSounds, ILootableEntity, IInventoryProvider
{
	public string lootPanelName = "generic";

	public int maxItemCount = 36;

	[NonSerialized]
	public ulong playerSteamID;

	[NonSerialized]
	public string _playerName;

	public bool ItemBasedDespawn;

	public bool onlyOwnerLoot;

	public bool buryLeftoverItems;

	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	public const Flags HasItems = Flags.Reserved1;

	public const Flags HasBeenOpened = Flags.Reserved2;

	private int _realCapacity;

	[NonSerialized]
	private bool firstLooted;

	public ItemContainer inventory;

	public Translate.Phrase LootPanelTitle => playerName;

	public string playerName
	{
		get
		{
			if (playerSteamID == 0L && _playerName == null)
			{
				return "";
			}
			return NameHelper.Get(playerSteamID, _playerName, base.isClient);
		}
		set
		{
			_playerName = value;
		}
	}

	public int RealCapacity => Mathf.Max(_realCapacity, inventory.capacity);

	public ulong LastLootedBy { get; set; }

	public BasePlayer LastLootedByPlayer { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DroppedItemContainer.OnRpcMessage"))
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

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		bool flag = Player.adminsafezonelooting && baseEntity.IsAdmin;
		if (playerSteamID != 0L && !flag && (baseEntity.InSafeZone() || InSafeZone()) && (ulong)baseEntity.userID != playerSteamID && (!baseEntity.InSafeCombatZone() || !InSafeCombatZone()))
		{
			baseEntity.ShowToast(GameTip.Styles.Error, PlayerCorpse.CantLootSafeZoneError, false);
			return false;
		}
		if (!flag && onlyOwnerLoot && (ulong)baseEntity.userID != playerSteamID)
		{
			return false;
		}
		if (!firstLooted)
		{
			if (playerSteamID != 0L && playerSteamID <= 10000000)
			{
				foreach (Item item in inventory.itemList)
				{
					item.SetItemOwnership(baseEntity, ItemOwnershipPhrases.LootedPhrase);
				}
			}
			firstLooted = true;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved2, b: true);
		}
		EvaluateBagConditions();
		return base.OnStartBeingLooted(baseEntity);
	}

	public override void ServerInit()
	{
		ResetRemovalTime();
		base.ServerInit();
	}

	public void RemoveMe()
	{
		if (IsOpen())
		{
			ResetRemovalTime();
			return;
		}
		try
		{
			BuryLeftoverItems();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		Kill();
	}

	private void BuryLeftoverItems()
	{
		using (TimeWarning.New("DroppedItemContainer.BuryLeftoverItems"))
		{
			if (!buryLeftoverItems || inventory == null)
			{
				return;
			}
			Vector3 position = base.transform.position;
			foreach (Item item in inventory.itemList)
			{
				if (item.IsValid())
				{
					BuriedItems.Instance.Register(item, position);
				}
			}
		}
	}

	private void EvaluateBagConditions()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		bool b = inventory != null && inventory.itemList.Count > 0 && inventory.itemList.Count > 3 && component != null && component.IsSleeping();
		flagsUpdateScope.Set(Flags.Reserved1, b);
	}

	public void ResetRemovalTime(float dur)
	{
		using (TimeWarning.New("ResetRemovalTime"))
		{
			Invoke(RemoveMe, dur);
		}
	}

	public void ResetRemovalTime()
	{
		ResetRemovalTime(CalculateRemovalTime());
	}

	public float CalculateRemovalTime()
	{
		if (!ItemBasedDespawn)
		{
			return ConVar.Server.itemdespawn * 16f * ConVar.Server.itemdespawn_container_scale;
		}
		float num = ConVar.Server.itemdespawn_quick;
		if (inventory != null)
		{
			foreach (Item item in inventory.itemList)
			{
				num = Mathf.Max(num, item.GetDespawnDuration());
			}
		}
		return num;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (inventory != null)
		{
			Facepunch.Pool.Free(ref inventory);
		}
	}

	private ItemContainer CreateContainer()
	{
		ItemContainer itemContainer = Facepunch.Pool.Get<ItemContainer>();
		itemContainer.ServerInitialize(null, maxItemCount);
		itemContainer.GiveUID();
		itemContainer.entityOwner = this;
		itemContainer.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		itemContainer.SetFlag(ItemContainer.Flag.DroppedItemContainer, b: true);
		return itemContainer;
	}

	public void TakeFrom(ItemContainer[] source, float destroyPercent)
	{
		Assert.IsTrue(inventory == null, "Initializing Twice");
		inventory = CreateContainer();
		destroyPercent = Mathf.Clamp01(destroyPercent);
		float movePercent = 1f - destroyPercent;
		TakeFractionOfItems(source, inventory, movePercent);
		_realCapacity = inventory.capacity;
		inventory.capacity = inventory.itemList.Count;
		ResetRemovalTime();
	}

	public static void TakeFractionOfItems(ItemContainer[] source, ItemContainer output, float movePercent)
	{
		output.containerVolume = source.Max((ItemContainer x) => x.containerVolume);
		List<Item> obj = Facepunch.Pool.Get<List<Item>>();
		ItemContainer[] array = source;
		foreach (ItemContainer obj2 in array)
		{
			obj2.RemoveFractionOfContainer(obj, movePercent);
			obj2.MergeAllStacks();
		}
		foreach (Item item in obj)
		{
			if (item.MoveToContainer(output))
			{
				continue;
			}
			bool flag = false;
			array = source;
			foreach (ItemContainer newcontainer in array)
			{
				if (item.MoveToContainer(newcontainer))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				item.DropAndTossUpwards(output.dropPosition);
			}
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (inventory != null)
		{
			BasePlayer player = rpc.player;
			if (CanLoot(player))
			{
				PlayerOpenLoot(player);
			}
		}
	}

	public void PlayerOpenLoot(BasePlayer player)
	{
		if ((bool)player && player.CanInteract() && Interface.CallHook("CanLootEntity", player, this) == null && player.inventory.loot.StartLootingEntity(this))
		{
			SetFlagLocal(Flags.Open, b: true);
			player.inventory.loot.AddContainer(inventory);
			player.inventory.loot.SendImmediate();
			player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), lootPanelName);
			SendNetworkUpdate();
		}
	}

	public void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		if (inventory == null || inventory.itemList == null || inventory.itemList.Count == 0)
		{
			Kill();
		}
		else
		{
			ResetRemovalTime();
			SetFlagLocal(Flags.Open, b: false);
			SendNetworkUpdate();
		}
		EvaluateBagConditions();
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		Debug.Assert(inventory == null, "Double init of inventory!");
		inventory = Facepunch.Pool.Get<ItemContainer>();
		inventory.entityOwner = this;
		inventory.ServerInitialize(null, 0);
		inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		inventory.SetFlag(ItemContainer.Flag.DroppedItemContainer, b: true);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lootableCorpse = Facepunch.Pool.Get<ProtoBuf.LootableCorpse>();
		info.msg.lootableCorpse.playerName = playerName;
		info.msg.lootableCorpse.playerID = playerSteamID;
		if (info.forDisk)
		{
			if (inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = inventory.Save();
			}
			else
			{
				Debug.LogWarning("Dropped item container without inventory: " + ToString());
			}
		}
	}

	public void GetAllInventories(List<ItemContainer> list)
	{
		list.Add(inventory);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lootableCorpse != null)
		{
			playerName = info.msg.lootableCorpse.playerName;
			playerSteamID = info.msg.lootableCorpse.playerID;
		}
		if (info.msg.storageBox != null)
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
			}
			else
			{
				Debug.LogWarning("Dropped item container without inventory: " + ToString());
			}
		}
	}

	public bool CanLoot(BasePlayer player)
	{
		return !player.IsBlockedFromLootingByMountable();
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}
}

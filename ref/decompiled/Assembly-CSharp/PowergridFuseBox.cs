#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class PowergridFuseBox : BaseEntity, IContainerSounds, ILootableEntity, PlayerInventory.ICanMoveFrom, PlayerInventory.ICanSwapFrom
{
	[Serializable]
	public class StageChangeEvent
	{
		public UnityEvent onStageReached;
	}

	[Header("Powergrid Fuse Box")]
	public ItemDefinition[] validPassthroughItems;

	public GameObject[] fuses = Array.Empty<GameObject>();

	public string lootPanelName = "generic";

	public float gibForwardVelocityScale = 1.5f;

	public GameObjectRef electricShockEffect;

	public StageChangeEvent[] stageChangeEvents = Array.Empty<StageChangeEvent>();

	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	[Header("Fuse Box Power Sounds")]
	public GameObject soundEmitterObject;

	public SoundDefinition boxOnlineSound;

	public SoundDefinition boxOnlineLoopSound;

	public SoundDefinition boxOfflineSound;

	public SoundDefinition fuseExpendedSound;

	public static readonly Translate.Phrase CannotRemoveFusePhrase = new Translate.Phrase("fusebox.cannotremove", "Fuses can only be removed via an item swap");

	public static readonly Translate.Phrase CannotSwapFusePhrase = new Translate.Phrase("fusebox.cannotswap", "A replacement fuse must be of higher condition");

	private ItemContainer inventory;

	public ulong LastLootedBy { get; set; }

	public BasePlayer LastLootedByPlayer { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PowergridFuseBox.OnRpcMessage"))
		{
			if (rpc == 331989034 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
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

	PlayerInventory.CanMoveFromResponse PlayerInventory.ICanMoveFrom.CanMoveFrom(BasePlayer player, Item item)
	{
		if (inventory != null && item.parent == inventory)
		{
			Server_OnFuseRemovalAttempt(player, item.position);
			return PlayerInventory.CanMoveFromResponse.Failure(CannotRemoveFusePhrase);
		}
		return PlayerInventory.CanMoveFromResponse.Success();
	}

	PlayerInventory.CanMoveFromResponse PlayerInventory.ICanSwapFrom.CanSwapFrom(BasePlayer player, Item displacedItem, Item incomingItem)
	{
		if (displacedItem.info != incomingItem.info)
		{
			return PlayerInventory.CanMoveFromResponse.Failure(PlayerInventoryErrors.InvalidItem);
		}
		if (inventory != null && displacedItem.parent == inventory)
		{
			if (incomingItem.condition > displacedItem.condition)
			{
				return PlayerInventory.CanMoveFromResponse.Success();
			}
			return PlayerInventory.CanMoveFromResponse.Failure(CannotSwapFusePhrase);
		}
		return PlayerInventory.CanMoveFromResponse.Success();
	}

	private void Server_OnFuseRemovalAttempt(BasePlayer player, int fuseSlotIndex)
	{
		player.Hurt(2f, DamageType.ElectricShock, this, useProtection: false);
		if (fuseSlotIndex < 0 || fuseSlotIndex >= fuses.Length)
		{
			Debug.LogError($"Invalid fuse slot {fuseSlotIndex} for number of fuses {fuses.Length} on {base.name}", this);
			return;
		}
		Vector3 position = fuses[fuseSlotIndex].transform.position;
		Vector3 posLocal = base.transform.InverseTransformPoint(position);
		Effect.server.Run(electricShockEffect.resourcePath, this, 0u, posLocal, Vector3.forward);
	}

	public override void ServerInit()
	{
		if (inventory == null)
		{
			CreateInventory(giveUID: true);
		}
		PowergridManager.Server_AddPowergridFuseBox(this);
		base.ServerInit();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		PowergridManager.Server_RemovePowergridFuseBox(this);
		Facepunch.Pool.Free(ref inventory);
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

	public void Server_DeteriorateFuse(Item item, float deltaTime, float decayRateScale)
	{
		int position = item.position;
		if (position < 0 || position >= fuses.Length)
		{
			Debug.LogError($"Item {item.info.name} is in invalid slot {position} for number of fuses {fuses.Length} on {base.name}", this);
			return;
		}
		float max = item.info.condition.max;
		if (max <= 0f)
		{
			return;
		}
		float fuseLifespanSeconds = Powergrid.fuseLifespanSeconds;
		if (fuseLifespanSeconds <= 0f || decayRateScale <= 0f)
		{
			return;
		}
		float num = max / fuseLifespanSeconds * decayRateScale;
		item.LoseCondition(num * deltaTime);
		if (item.isBroken)
		{
			if (item.position >= 0)
			{
				item.Drop(base.transform.position + base.transform.forward * 0.5f, GetInheritedDropVelocity() + base.transform.forward * 2f);
			}
			ClientRPC(RpcTarget.NetworkGroup("Client_FuseExpended"), position);
		}
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
		inventory.allowedContents = ItemContainer.ContentsType.Generic;
		inventory.SetOnlyAllowedItems(validPassthroughItems);
		inventory.maxStackSize = 1;
		inventory.ServerInitialize(null, GetMaxNoOfFuses());
		if (giveUID)
		{
			inventory.GiveUID();
		}
		inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		inventory.onItemPositionChanged = OnItemPositionChanged;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.isServer)
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

	public void OnItemAddedOrRemoved(Item item, bool added)
	{
		if (!IsValidPassthroughItem(item))
		{
			if (added)
			{
				Debug.LogError("Item " + item.info.name + " was added to " + base.name + " but this is not a valid passthrough item", this);
			}
			else
			{
				Debug.LogError("Item " + item.info.name + " was removed from " + base.name + " but this is not a valid passthrough item", this);
			}
			return;
		}
		if (added)
		{
			PowergridManager.Server_OnFuseInsertedIntoFuseBox(this, item, LastLootedByPlayer);
		}
		else
		{
			PowergridManager.Server_OnFuseRemovedFromFuseBox(item);
		}
		if (Rust.Application.isServerStarted)
		{
			SendNetworkUpdate();
			if (added && LastLootedByPlayer != null && LastLootedByPlayer.serverClan != null)
			{
				LastLootedByPlayer.AddClanScore(ClanScoreEventType.InsertHeavyFuseInPowerPlant);
			}
		}
	}

	public void OnItemPositionChanged(Item item, int oldPosition, int newPosition)
	{
		if (!IsValidPassthroughItem(item))
		{
			Debug.LogError($"Item {item.info.name} was moved from pos {oldPosition} to {newPosition} in {base.name} but this is not a valid passthrough item", this);
		}
		else
		{
			SendNetworkUpdate();
		}
	}

	public void Server_OnPowergridStageChanged()
	{
		ClientRPC(RpcTarget.NetworkGroup("ClientRPC_OnPowergridStageChanged"), PointEntity<PowergridManager>.ServerInstance.CurrentStage);
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

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.storageBox == null || !base.isServer)
		{
			return;
		}
		using (TimeWarning.New("PowerGridFuseBox.Load.Server"))
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
				inventory.capacity = GetMaxNoOfFuses();
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public int GetMaxNoOfFuses()
	{
		return fuses.Length;
	}
}

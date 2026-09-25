#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class Locker : StorageContainer
{
	public enum RowType
	{
		Clothing,
		Belt
	}

	public static class LockerFlags
	{
		public const Flags IsEquipping = Flags.Reserved1;
	}

	public GameObjectRef equipSound;

	public int maxGearSets = 3;

	public const int attireSize = 8;

	public const int beltSize = 6;

	public const int columnSize = 2;

	public const int backpackSlotIndex = 7;

	public const int setSize = 14;

	private static Item[] clothingBuffer = new Item[8];

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Locker.OnRpcMessage"))
		{
			if (rpc == 1799659668 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Equip");
				}
				using (TimeWarning.New("RPC_Equip"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1799659668u, "RPC_Equip", this, player, 3f))
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
							RPC_Equip(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Equip");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsEquipping()
	{
		return HasFlag(Flags.Reserved1);
	}

	public RowType GetRowType(int slot)
	{
		if (slot == -1)
		{
			return RowType.Clothing;
		}
		if (slot % 14 >= 8)
		{
			return RowType.Belt;
		}
		return RowType.Clothing;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public void ClearEquipping()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
		object obj = Interface.CallHook("CanLockerAcceptItem", this, item, targetSlot, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!base.ItemFilter(player, item, targetSlot))
		{
			return false;
		}
		bool num = item.IsBackpack();
		bool flag = IsBackpackSlot(targetSlot);
		if (num != flag)
		{
			return false;
		}
		if (isTransferringIndustrialItem && GetRowType(targetSlot) == RowType.Belt && item.info.category == ItemCategory.Attire)
		{
			return false;
		}
		if (item.info.category == ItemCategory.Attire)
		{
			return true;
		}
		return GetRowType(targetSlot) == RowType.Belt;
	}

	public bool IsBackpackSlot(int slot)
	{
		return (slot - 7) % 14 == 0;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Equip(RPCMessage msg)
	{
		int num = msg.read.Int32();
		if (num < 0 || num >= maxGearSets || Interface.CallHook("OnLockerSwap", this, num, msg.player) != null || IsEquipping())
		{
			return;
		}
		BasePlayer player = msg.player;
		if (player.IsDead() || !CanBeLooted(player))
		{
			return;
		}
		BaseLock @lock = GetLock();
		if (@lock != null && !@lock.OnTryToOpen(player))
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
			return;
		}
		int startSlot = num * 14;
		if (SwapPlayerInventoryWithContainer(player, base.inventory, startSlot, GetDropPosition(), GetDropVelocity(), doBelt: true))
		{
			Effect.server.Run(equipSound.resourcePath, player, StringPool.Get("spine3"), Vector3.zero, Vector3.zero);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ClearEquipping, 1.5f);
		}
	}

	public static bool SwapPlayerInventoryWithContainer(BasePlayer player, ItemContainer inventory, int startSlot, Vector3 dropPosition, Vector3 dropVelocity, bool doBelt, Func<Item, bool> filterItems = null)
	{
		bool result = false;
		for (int i = 0; i < clothingBuffer.Length; i++)
		{
			Item slot = player.inventory.containerWear.GetSlot(i);
			if (slot != null && (filterItems == null || filterItems(slot)))
			{
				slot.RemoveFromContainer();
				clothingBuffer[i] = slot;
			}
		}
		for (int j = 0; j < 8; j++)
		{
			int num = startSlot + j;
			Item slot2 = inventory.GetSlot(num);
			Item item = clothingBuffer[j];
			if (slot2 != null)
			{
				result = true;
				if (slot2.info.category != ItemCategory.Attire || !slot2.MoveToContainer(player.inventory.containerWear, j))
				{
					slot2.Drop(dropPosition, dropVelocity);
				}
			}
			if (item != null)
			{
				result = true;
				if (!item.MoveToContainer(inventory, num) && !item.MoveToContainer(player.inventory.containerWear, j))
				{
					item.Drop(dropPosition, dropVelocity);
				}
			}
			clothingBuffer[j] = null;
		}
		if (doBelt)
		{
			for (int k = 0; k < 6; k++)
			{
				int num2 = startSlot + k + 8;
				int iTargetPos = k;
				Item slot3 = inventory.GetSlot(num2);
				Item slot4 = player.inventory.containerBelt.GetSlot(k);
				slot4?.RemoveFromContainer();
				if (slot3 != null)
				{
					result = true;
					if (!slot3.MoveToContainer(player.inventory.containerBelt, iTargetPos))
					{
						slot3.Drop(dropPosition, dropVelocity);
					}
				}
				if (slot4 != null)
				{
					result = true;
					if (!slot4.MoveToContainer(inventory, num2))
					{
						slot4.Drop(dropPosition, dropVelocity);
					}
				}
			}
		}
		return result;
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		for (int i = 0; i < inventorySlots; i++)
		{
			RowType rowType = GetRowType(i);
			if (item.info.category == ItemCategory.Attire)
			{
				if (rowType != 0)
				{
					continue;
				}
			}
			else if (rowType != RowType.Belt)
			{
				continue;
			}
			if (!base.inventory.SlotTaken(item, i) && (rowType != 0 || !DoesWearableConflictWithRow(item, i)))
			{
				return i;
			}
		}
		return int.MinValue;
	}

	public bool DoesWearableConflictWithRow(Item item, int pos)
	{
		int num = pos / 14 * 14;
		ItemModWearable itemModWearable = item.info.ItemModWearable;
		if (itemModWearable == null)
		{
			return false;
		}
		bool num2 = item.IsBackpack();
		bool flag = IsBackpackSlot(pos);
		if (num2 != flag)
		{
			return true;
		}
		for (int i = num; i < num + 8; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null)
			{
				ItemModWearable itemModWearable2 = slot.info.ItemModWearable;
				if (!(itemModWearable2 == null) && !itemModWearable2.CanExistWith(itemModWearable))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector2i GetIndustrialSlotRange(Vector3 localPosition)
	{
		if (localPosition.x < -0.3f)
		{
			return new Vector2i(28, 41);
		}
		if (localPosition.x > 0.3f)
		{
			return new Vector2i(0, 13);
		}
		return new Vector2i(14, 27);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (HasAttachedStorageAdaptor())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemHasAttachment, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}
}

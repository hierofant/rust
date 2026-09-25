#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Mannequin : StorageContainer
{
	public static class MannequinFlags
	{
		public const Flags IsEquipping = Flags.Reserved1;
	}

	[Header("Mannequin Settings")]
	public GameObjectRef EquipSound;

	public GameObjectRef ChangePoseSound;

	public GameObject SupportRoot;

	public Transform MannequinSpawnPoint;

	public BaseCollision HitBoxCollision;

	public PhysicsMaterial OverrideHitBoxMaterial;

	[Range(0f, 1f)]
	public float LodRange0 = 0.3f;

	[Range(0f, 1f)]
	public float LodRange1 = 0.15f;

	[Range(0f, 1f)]
	public float LodRange2 = 0.08f;

	[Range(0f, 1f)]
	public float LodRange3 = 0.02f;

	private const int BACKPACK_SLOT_INDEX = 7;

	protected static string headPartPath = "assets/prefabs/clothes/skin/mannequin/head.male.mannequin.prefab";

	protected static string torsoPartPath = "assets/prefabs/clothes/skin/mannequin/torso.male.mannequin.prefab";

	protected static string handsPartPath = "assets/prefabs/clothes/skin/mannequin/hands.male.mannequin.prefab";

	protected static string legsPartPath = "assets/prefabs/clothes/skin/mannequin/legs.male.mannequin.prefab";

	public static HumanBodyBones[] ValidBoneArray = new HumanBodyBones[49]
	{
		HumanBodyBones.Hips,
		HumanBodyBones.Spine,
		HumanBodyBones.Chest,
		HumanBodyBones.LeftShoulder,
		HumanBodyBones.LeftUpperArm,
		HumanBodyBones.LeftLowerArm,
		HumanBodyBones.LeftHand,
		HumanBodyBones.RightShoulder,
		HumanBodyBones.RightUpperArm,
		HumanBodyBones.RightLowerArm,
		HumanBodyBones.RightHand,
		HumanBodyBones.LeftUpperLeg,
		HumanBodyBones.LeftLowerLeg,
		HumanBodyBones.LeftFoot,
		HumanBodyBones.RightUpperLeg,
		HumanBodyBones.RightLowerLeg,
		HumanBodyBones.RightFoot,
		HumanBodyBones.Neck,
		HumanBodyBones.Head,
		HumanBodyBones.LeftThumbProximal,
		HumanBodyBones.LeftThumbIntermediate,
		HumanBodyBones.LeftThumbDistal,
		HumanBodyBones.LeftIndexProximal,
		HumanBodyBones.LeftIndexIntermediate,
		HumanBodyBones.LeftIndexDistal,
		HumanBodyBones.LeftMiddleProximal,
		HumanBodyBones.LeftMiddleIntermediate,
		HumanBodyBones.LeftMiddleDistal,
		HumanBodyBones.LeftRingProximal,
		HumanBodyBones.LeftRingIntermediate,
		HumanBodyBones.LeftRingDistal,
		HumanBodyBones.LeftLittleProximal,
		HumanBodyBones.LeftLittleIntermediate,
		HumanBodyBones.LeftLittleDistal,
		HumanBodyBones.RightThumbProximal,
		HumanBodyBones.RightThumbIntermediate,
		HumanBodyBones.RightThumbDistal,
		HumanBodyBones.RightIndexProximal,
		HumanBodyBones.RightIndexIntermediate,
		HumanBodyBones.RightIndexDistal,
		HumanBodyBones.RightMiddleProximal,
		HumanBodyBones.RightMiddleIntermediate,
		HumanBodyBones.RightMiddleDistal,
		HumanBodyBones.RightRingProximal,
		HumanBodyBones.RightRingIntermediate,
		HumanBodyBones.RightRingDistal,
		HumanBodyBones.RightLittleProximal,
		HumanBodyBones.RightLittleIntermediate,
		HumanBodyBones.RightLittleDistal
	};

	public MannequinPose[] AvailablePoses;

	private static Item[] clothingBuffer = new Item[8];

	private static Item[] lockerBuffer = new Item[8];

	private int __sync_PoseIndex;

	[Sync(Autosave = true)]
	public int PoseIndex
	{
		[CompilerGenerated]
		get
		{
			return __sync_PoseIndex;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_PoseIndex, value))
			{
				__sync_PoseIndex = value;
				byte nameID = __GetWeaverID("PoseIndex");
				QueueSyncVar(nameID);
			}
		}
	}

	public TimeSince LastPoseChange { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Mannequin.OnRpcMessage"))
		{
			if (rpc == 1116452643 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_ChangePose");
				}
				using (TimeWarning.New("Server_ChangePose"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1116452643u, "Server_ChangePose", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1116452643u, "Server_ChangePose", this, player, 3f))
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
							Server_ChangePose(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_ChangePose");
					}
				}
				return true;
			}
			if (rpc == 1422897100 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestSwap");
				}
				using (TimeWarning.New("Server_RequestSwap"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1422897100u, "Server_RequestSwap", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1422897100u, "Server_RequestSwap", this, player, 3f))
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
							Server_RequestSwap(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_RequestSwap");
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

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.mannequin = Facepunch.Pool.Get<ProtoBuf.Mannequin>();
		info.msg.mannequin.clothingItems = Facepunch.Pool.Get<List<ProtoBuf.Mannequin.ClothingItem>>();
		foreach (Item item in base.inventory.itemList)
		{
			ProtoBuf.Mannequin.ClothingItem clothingItem = Facepunch.Pool.Get<ProtoBuf.Mannequin.ClothingItem>();
			clothingItem.itemId = item.info.itemid;
			clothingItem.skin = item.skin;
			clothingItem.uid = item.uid;
			clothingItem.reserved_int_0 = item.instanceData?.dataInt ?? 0;
			info.msg.mannequin.clothingItems.Add(clothingItem);
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		SendNetworkUpdate();
	}

	private bool IsBackpackSlot(int slot)
	{
		return (slot - 7) % 14 == 0;
	}

	public override bool ItemFilter(BasePlayer player, Item item, int targetSlot)
	{
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
		if (item.info.ItemModWearable != null && item.info.ItemModWearable.blockFromMannequin)
		{
			return false;
		}
		return CanAcceptItem(player, item, targetSlot);
	}

	private bool CanAcceptItem(BasePlayer player, Item newItem, int slot)
	{
		if (!newItem.info.TryGetComponent<ItemModWearable>(out var component))
		{
			return false;
		}
		for (int i = 0; i < base.inventory.capacity; i++)
		{
			Item slot2 = base.inventory.GetSlot(i);
			if (slot2 != null && slot2.info.TryGetComponent<ItemModWearable>(out var component2) && !component.CanExistWith(component2) && slot != i)
			{
				return false;
			}
		}
		return true;
	}

	public void ClearEquipping()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	public override void OnInventoryFirstCreated(ItemContainer container)
	{
		base.OnInventoryFirstCreated(container);
		container.flags = ItemContainer.Flag.Clothing;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void Server_ChangePose(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanBuild(cached: true) && Interface.CallHook("CanMannequinChangePose", this, msg.player) == null)
		{
			int num = PoseIndex + 1;
			if (num >= AvailablePoses.Length)
			{
				num = 0;
			}
			PoseIndex = num;
			if (ChangePoseSound.isValid)
			{
				Effect.server.Run(ChangePoseSound.resourcePath, base.transform.position);
			}
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void Server_RequestSwap(RPCMessage msg)
	{
		if (IsEquipping())
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!CanBeLooted(player) || player.IsDead() || Interface.CallHook("CanMannequinSwap", this, player) != null)
		{
			return;
		}
		BaseLock @lock = GetLock();
		if (@lock != null && !@lock.OnTryToOpen(player))
		{
			player.ShowToast(GameTip.Styles.Error, PlayerInventoryErrors.ContainerLocked, false);
		}
		else if (SwapPlayerInventoryWithContainer(msg.player, base.inventory, GetDropPosition(), GetDropVelocity(), FilterItems))
		{
			if (EquipSound != null)
			{
				Effect.server.Run(EquipSound.resourcePath, player, StringPool.Get("spine3"), Vector3.zero, Vector3.zero);
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ClearEquipping, 1.5f);
		}
	}

	private bool FilterItems(Item item)
	{
		if (item.info.ItemModWearable != null && item.info.ItemModWearable.blockFromMannequin)
		{
			return false;
		}
		return true;
	}

	public static bool SwapPlayerInventoryWithContainer(BasePlayer player, ItemContainer inventory, Vector3 dropPosition, Vector3 dropVelocity, Func<Item, bool> filterItems = null)
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
		for (int j = 0; j < lockerBuffer.Length; j++)
		{
			Item slot2 = inventory.GetSlot(j);
			if (slot2 != null && (filterItems == null || filterItems(slot2)))
			{
				slot2.RemoveFromContainer();
				lockerBuffer[j] = slot2;
			}
		}
		for (int k = 0; k < clothingBuffer.Length; k++)
		{
			Item item = lockerBuffer[k];
			Item item2 = clothingBuffer[k];
			if (item != null)
			{
				result = true;
				if (item.info.category != ItemCategory.Attire || !item.MoveToContainer(player.inventory.containerWear, k))
				{
					item.Drop(dropPosition, dropVelocity);
				}
			}
			if (item2 != null)
			{
				result = true;
				if (!item2.MoveToContainer(inventory, k) && !item2.MoveToContainer(player.inventory.containerWear, k) && !item2.MoveToContainer(player.inventory.containerMain))
				{
					item2.Drop(dropPosition, dropVelocity);
				}
			}
			clothingBuffer[k] = null;
			lockerBuffer[k] = null;
		}
		return result;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: PoseIndex for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_PoseIndex);
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
				_ = __sync_PoseIndex;
				int _sync_PoseIndex = reader.Int32();
				__sync_PoseIndex = _sync_PoseIndex;
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
		if (propertyName == "PoseIndex")
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
		__sync_PoseIndex = 0;
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

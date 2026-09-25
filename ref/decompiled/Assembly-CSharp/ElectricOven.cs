using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class ElectricOven : BaseOven
{
	public struct ElectricOvenPreserveInfo
	{
		public IOEntity.SlotPreserveInfo inputSlotInfo;

		public IOEntity.SlotPreserveInfo connectedToSlotInfo;
	}

	public GameObjectRef IoEntity;

	public Transform IoEntityAnchor;

	public EntityRef<IOEntity> spawnedIo;

	public override string TitleItemShortname => "electric.furnace";

	public override bool ShowFuelInLootPanel => false;

	public override bool CanRunWithNoFuel
	{
		get
		{
			if (spawnedIo.IsValid(serverside: true))
			{
				return spawnedIo.Get(serverside: true).IsPowered();
			}
			return false;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			SpawnIOEnt();
		}
	}

	public void SpawnIOEnt()
	{
		if (!spawnedIo.IsValid(serverside: true) && IoEntity.isValid && IoEntityAnchor != null)
		{
			IOEntity iOEntity = GameManager.server.CreateEntity(IoEntity.resourcePath, IoEntityAnchor.position, IoEntityAnchor.rotation) as IOEntity;
			iOEntity.SetParent(this, worldPositionStays: true);
			spawnedIo.Set(iOEntity);
			iOEntity.Spawn();
			iOEntity.SendChangedToRoot(forceUpdate: true);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child is IOEntity iOEntity && !spawnedIo.IsValid(serverside: true))
		{
			spawnedIo.Set(iOEntity);
			iOEntity.SendChangedToRoot(forceUpdate: true);
		}
	}

	public override void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		ref ElectricOvenPreserveInfo electricOvenPreserve = ref preserveInfo.electricOvenPreserve;
		if (spawnedIo.IsValid(serverside: true))
		{
			IOEntity iOEntity = spawnedIo.Get(serverside: true);
			IOEntity.IOSlot iOSlot = iOEntity.inputs[0];
			if (iOEntity.inputs[0].IsConnected())
			{
				iOSlot.Preserve(ref electricOvenPreserve.inputSlotInfo);
				electricOvenPreserve.inputSlotInfo.connectedTo.outputs[iOSlot.connectedToSlot].Preserve(ref electricOvenPreserve.connectedToSlotInfo);
			}
			spawnedIo.Get(serverside: true).Kill();
		}
		base.Reskin_Preserve(ref preserveInfo);
	}

	public override void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		base.Reskin_Restore(ref preserveInfo);
		ref ElectricOvenPreserveInfo electricOvenPreserve = ref preserveInfo.electricOvenPreserve;
		if (!(electricOvenPreserve.inputSlotInfo.connectedTo != null) || !spawnedIo.IsValid(serverside: true))
		{
			return;
		}
		IOEntity iOEntity = spawnedIo.Get(serverside: true);
		electricOvenPreserve.connectedToSlotInfo.connectedTo = iOEntity;
		iOEntity.inputs[0].Restore(ref electricOvenPreserve.inputSlotInfo);
		electricOvenPreserve.inputSlotInfo.connectedTo.outputs[electricOvenPreserve.inputSlotInfo.connectedToSlot].Restore(ref electricOvenPreserve.connectedToSlotInfo);
		HashSet<IOEntity> obj = Pool.Get<HashSet<IOEntity>>();
		iOEntity.SnapLinesToHandlePositions(obj);
		foreach (IOEntity item in obj)
		{
			item.MarkDirtyForceUpdateOutputs();
			item.SendNetworkUpdateImmediate();
			item.NotifyClientsLineChanged();
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.simpleUID == null)
		{
			info.msg.simpleUID = Pool.Get<SimpleUID>();
		}
		info.msg.simpleUID.uid = spawnedIo.uid;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.simpleUID != null)
		{
			spawnedIo.uid = info.msg.simpleUID.uid;
		}
	}

	public override void OvenFull()
	{
		Invoke(PauseCooking, 0f);
	}

	private void PauseCooking()
	{
		UpdateAttachmentTemperature();
		if (base.inventory != null)
		{
			base.inventory.temperature = 15f;
			foreach (Item item in base.inventory.itemList)
			{
				if (item.HasFlag(Item.Flag.OnFire))
				{
					item.SetFlag(Item.Flag.OnFire, b: false);
					item.MarkDirty();
				}
				if (item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: false);
					item.MarkDirty();
				}
			}
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved8, b: true);
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		if (item == null || bAdded || !HasFlag(Flags.Reserved8))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved8, b: false);
	}

	protected override bool CanPickupOven()
	{
		return children.Count == 1;
	}
}

using Oxide.Core;
using Rust;
using UnityEngine;

public class WaterPurifier : LiquidContainer
{
	public static class WaterPurifierFlags
	{
		public const Flags Boiling = Flags.Reserved1;
	}

	public GameObjectRef storagePrefab;

	public Transform storagePrefabAnchor;

	public ItemDefinition freshWater;

	public int waterToProcessPerMinute = 120;

	public int freshWaterRatio = 4;

	public bool stopWhenOutputFull;

	public LiquidContainer purifiedWaterStorage;

	public float dirtyWaterProcssed;

	public float pendingFreshWater;

	public bool IsBoiling()
	{
		return HasFlag(Flags.Reserved1);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		bool flag = false;
		if (!Rust.Application.isLoadingSave && !flag)
		{
			SpawnStorageEnt(load: false);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SpawnStorageEnt(load: true);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (purifiedWaterStorage != null)
		{
			purifiedWaterStorage.Kill();
		}
	}

	protected virtual void SpawnStorageEnt(bool load)
	{
		if (load)
		{
			BaseEntity baseEntity = GetParentEntity();
			if ((bool)baseEntity)
			{
				foreach (BaseEntity child in baseEntity.children)
				{
					if (child != this && child is LiquidContainer liquidContainer)
					{
						purifiedWaterStorage = liquidContainer;
						break;
					}
				}
			}
		}
		if (purifiedWaterStorage != null)
		{
			purifiedWaterStorage.SetConnectedTo(this);
			return;
		}
		purifiedWaterStorage = GameManager.server.CreateEntity(storagePrefab.resourcePath, storagePrefabAnchor.localPosition, storagePrefabAnchor.localRotation) as LiquidContainer;
		purifiedWaterStorage.SetParent(GetParentEntity());
		purifiedWaterStorage.Spawn();
		purifiedWaterStorage.SetConnectedTo(this);
	}

	internal override void OnParentRemoved()
	{
		Kill(DestroyMode.Gib);
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (!purifiedWaterStorage.IsDestroyed)
		{
			purifiedWaterStorage.Kill();
		}
	}

	public void ParentTemperatureUpdate(float temp)
	{
	}

	public void CheckCoolDown()
	{
		if (!GetParentEntity() || !GetParentEntity().IsOn() || !HasDirtyWater())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
			CancelInvoke(CheckCoolDown);
		}
	}

	public bool HasDirtyWater()
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot != null && slot.info.itemType == ItemContainer.ContentsType.Liquid)
		{
			return slot.amount > 0;
		}
		return false;
	}

	public bool HasPurifiedWater()
	{
		if (purifiedWaterStorage == null || purifiedWaterStorage.inventory == null)
		{
			return false;
		}
		Item slot = purifiedWaterStorage.inventory.GetSlot(0);
		if (slot != null && slot.info.itemType == ItemContainer.ContentsType.Liquid)
		{
			return slot.amount > 0;
		}
		return false;
	}

	public void Cook(float timeCooked)
	{
		if (purifiedWaterStorage == null)
		{
			return;
		}
		bool flag = HasDirtyWater();
		if (!IsBoiling() && flag)
		{
			InvokeRepeating(CheckCoolDown, 2f, 2f);
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		if (IsBoiling() && flag)
		{
			ConvertWater(timeCooked);
		}
	}

	protected void ConvertWater(float timeCooked)
	{
		Item slot = purifiedWaterStorage.inventory.GetSlot(0);
		if ((stopWhenOutputFull && slot != null && slot.amount >= slot.MaxStackable()) || Interface.CallHook("OnWaterPurify", this, timeCooked) != null)
		{
			return;
		}
		float num = timeCooked * ((float)waterToProcessPerMinute / 60f);
		if (slot != null)
		{
			float num2 = Mathf.Max(slot.MaxStackable() - slot.amount, 0);
			num = Mathf.Min(num, num2 * (float)freshWaterRatio);
		}
		dirtyWaterProcssed += num;
		if (dirtyWaterProcssed >= 1f)
		{
			Item slot2 = base.inventory.GetSlot(0);
			int num3 = Mathf.Min(Mathf.FloorToInt(dirtyWaterProcssed), slot2.amount);
			num = num3;
			slot2.UseItem(num3);
			dirtyWaterProcssed -= num3;
			SendNetworkUpdate();
		}
		pendingFreshWater += num / (float)freshWaterRatio;
		if (!(pendingFreshWater >= 1f))
		{
			return;
		}
		int num4 = Mathf.FloorToInt(pendingFreshWater);
		pendingFreshWater -= num4;
		Item slot3 = purifiedWaterStorage.inventory.GetSlot(0);
		if (slot3 != null && slot3.info != freshWater)
		{
			slot3.RemoveFromContainer();
			slot3.Remove();
		}
		if (slot3 == null)
		{
			Item item = ItemManager.Create(freshWater, num4, 0uL, isServerSide: true, 0uL);
			if (!item.MoveToContainer(purifiedWaterStorage.inventory))
			{
				item.Remove();
			}
		}
		else
		{
			slot3.amount += num4;
			slot3.amount = Mathf.Clamp(slot3.amount, 0, purifiedWaterStorage.maxStackSize);
			purifiedWaterStorage.inventory.MarkDirty();
		}
		Interface.CallHook("OnWaterPurified", this, timeCooked);
		purifiedWaterStorage.SendNetworkUpdate();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.On, b: false);
			}
		}
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (HasDirtyWater() || HasPurifiedWater())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemInventoryMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}
}

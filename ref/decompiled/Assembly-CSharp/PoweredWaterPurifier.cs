using UnityEngine;

public class PoweredWaterPurifier : WaterPurifier
{
	public float ConvertInterval = 5f;

	public int PowerDrain = 5;

	public Material PoweredMaterial;

	public Material UnpoweredMaterial;

	public MeshRenderer TargetRenderer;

	public override void ResetState()
	{
		base.ResetState();
	}

	protected override void SpawnStorageEnt(bool load)
	{
		if (load)
		{
			foreach (BaseEntity child in children)
			{
				if (child is LiquidContainer liquidContainer)
				{
					purifiedWaterStorage = liquidContainer;
				}
			}
		}
		if (purifiedWaterStorage != null)
		{
			purifiedWaterStorage.SetConnectedTo(this);
			return;
		}
		purifiedWaterStorage = GameManager.server.CreateEntity(storagePrefab.resourcePath, storagePrefabAnchor.position, storagePrefabAnchor.rotation) as LiquidContainer;
		purifiedWaterStorage.SetParent(this, worldPositionStays: true);
		purifiedWaterStorage.Spawn();
		purifiedWaterStorage.SetConnectedTo(this);
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (HasDirtyWater() || HasPurifiedWater())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemInventoryMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return true;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (HasLiquidItem())
		{
			if (HasFlag(Flags.Reserved8) && !IsInvoking(ConvertWater))
			{
				InvokeRandomized(ConvertWater, ConvertInterval, ConvertInterval, ConvertInterval * 0.1f);
			}
		}
		else if (IsInvoking(ConvertWater))
		{
			CancelInvoke(ConvertWater);
		}
	}

	private void ConvertWater()
	{
		if (HasDirtyWater())
		{
			ConvertWater(ConvertInterval);
			dirtyWaterProcssed = 0f;
		}
	}

	public override int ConsumptionAmount()
	{
		return PowerDrain;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!base.isServer)
		{
			return;
		}
		if ((old & Flags.Reserved8) == Flags.Reserved8 != ((next & Flags.Reserved8) == Flags.Reserved8))
		{
			if ((next & Flags.Reserved8) == Flags.Reserved8)
			{
				if (!IsInvoking(ConvertWater))
				{
					InvokeRandomized(ConvertWater, ConvertInterval, ConvertInterval, ConvertInterval * 0.1f);
				}
			}
			else if (IsInvoking(ConvertWater))
			{
				CancelInvoke(ConvertWater);
			}
		}
		if (purifiedWaterStorage != null)
		{
			using (FlagsUpdateScope flagsUpdateScope = purifiedWaterStorage.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, HasFlag(Flags.Reserved8));
			}
		}
	}
}

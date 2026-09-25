using System;
using Oxide.Core;
using UnityEngine;

public class WaterPump : LiquidContainer
{
	public Transform WaterResourceLocation;

	public float PumpInterval = 20f;

	public int AmountPerPump = 30;

	public int PowerConsumption = 5;

	private Action createWaterAction;

	public override bool IsGravitySource => true;

	public override int ConsumptionAmount()
	{
		return PowerConsumption;
	}

	public void CreateWater()
	{
		if (!IsFull())
		{
			ItemDefinition itemDefinition = WaterResource.SV_GetAtPoint(WaterResourceLocation.position);
			if (itemDefinition != null && Interface.CallHook("OnWaterCollect", this, itemDefinition) == null)
			{
				base.inventory.AddItem(itemDefinition, AmountPerPump, 0uL);
				UpdateOnFlag();
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (next & Flags.Reserved8) == Flags.Reserved8;
		if (!base.isServer || (old & Flags.Reserved8) == Flags.Reserved8 == flag)
		{
			return;
		}
		if (createWaterAction == null)
		{
			createWaterAction = CreateWater;
		}
		if (flag)
		{
			if (!IsInvoking(createWaterAction))
			{
				InvokeRandomized(createWaterAction, PumpInterval, PumpInterval, PumpInterval * 0.1f);
			}
		}
		else if (IsInvoking(createWaterAction))
		{
			CancelInvoke(createWaterAction);
		}
		if (flag)
		{
			MarkDirtyForceUpdateOutputs();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return Mathf.Clamp(GetLiquidCount(), 0, maxOutputFlow);
	}

	public bool IsFull()
	{
		if (base.inventory.itemList.Count == 0)
		{
			return false;
		}
		if (base.inventory.itemList[0].amount < base.inventory.maxStackSize)
		{
			return false;
		}
		return true;
	}

	public override void UpdateOutputs()
	{
		ensureOutputsUpdated = true;
		base.UpdateOutputs();
	}
}

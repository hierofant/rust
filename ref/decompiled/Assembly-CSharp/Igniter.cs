using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class Igniter : IOEntity
{
	[Space]
	public float IgniteRange = 5f;

	public float IgniteFrequency = 1f;

	public float IgniteStartDelay;

	public Transform LineOfSightEyes;

	public float SelfDamagePerIgnite = 0.5f;

	[Space]
	public int PowerConsumption = 2;

	public override int ConsumptionAmount()
	{
		return PowerConsumption;
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		base.UpdateFromInput(inputAmount, inputSlot);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (inputAmount >= ConsumptionAmount() && CanIgnite())
		{
			InvokeRepeating(IgniteInRange, IgniteStartDelay, IgniteFrequency);
			flagsUpdateScope.Set(Flags.On, b: true);
			return;
		}
		if (IsInvoking(IgniteInRange))
		{
			CancelInvoke(IgniteInRange);
		}
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		if (!CanIgnite())
		{
			return 0;
		}
		return base.DesiredPower(inputIndex);
	}

	public override void OnRepair()
	{
		base.OnRepair();
		if (CanIgnite())
		{
			SendChangedToRoot(forceUpdate: true);
		}
	}

	public bool CanIgnite()
	{
		return base.healthFraction >= 0.1f;
	}

	private void IgniteInRange()
	{
		List<BaseEntity> obj = Pool.Get<List<BaseEntity>>();
		Vis.Entities(LineOfSightEyes.position, IgniteRange, obj, 1237019409);
		foreach (BaseEntity item in obj)
		{
			if (!item.HasFlag(Flags.On) && item.IsVisible(LineOfSightEyes.position))
			{
				if (item.isServer && item is BaseOven)
				{
					(item as BaseOven).StartCooking();
				}
				else if (item.isServer && item is IIgniteable igniteable && igniteable.CanIgnite())
				{
					igniteable.Ignite(base.transform.position);
				}
			}
		}
		Pool.FreeUnmanaged(ref obj);
		Hurt(SelfDamagePerIgnite, DamageType.ElectricShock, this, useProtection: false);
		if (!CanIgnite())
		{
			SendChangedToRoot(forceUpdate: true);
		}
	}
}

using UnityEngine;

public class ElectricalHeater : IOEntity
{
	public float fadeDuration = 1f;

	public Light sourceLight;

	public Light secondaryLight;

	public GrowableHeatSource growableHeatSource;

	public override int ConsumptionAmount()
	{
		return 3;
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (next & Flags.Reserved8) == Flags.Reserved8;
		if ((old & Flags.Reserved8) == Flags.Reserved8 != flag && growableHeatSource != null)
		{
			growableHeatSource.ForceUpdateGrowablesInRange();
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (growableHeatSource != null)
		{
			growableHeatSource.ForceUpdateGrowablesInRange();
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		GrowableHeatSource.FarmHeatSourceGrid.OnParentChanged(growableHeatSource, this, oldParent, newParent);
	}
}

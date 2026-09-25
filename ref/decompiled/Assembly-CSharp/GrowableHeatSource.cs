using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class GrowableHeatSource : EntityComponent<BaseEntity>, IServerComponent
{
	public float heatAmount = 5f;

	public AnimationCurve HeatFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public static PartialMobileStaticGrid<GrowableHeatSource> FarmHeatSourceGrid = new PartialMobileStaticGrid<GrowableHeatSource>();

	public float ApplyHeat(Vector3 forPosition)
	{
		if (base.baseEntity == null)
		{
			return 0f;
		}
		if (base.baseEntity.IsOn() || (base.baseEntity is IOEntity iOEntity && iOEntity.IsPowered()))
		{
			float num = Vector3.Distance(forPosition, base.transform.position);
			float num2 = HeatFalloff.Evaluate(num / Server.artificialTemperatureGrowableRange);
			return heatAmount * num2;
		}
		return 0f;
	}

	public void ForceUpdateGrowablesInRange()
	{
		List<IHeatSourceListener> obj = Facepunch.Pool.Get<List<IHeatSourceListener>>();
		int layerMask = 524544;
		Vis.Entities(base.transform.position, Server.artificialTemperatureGrowableRange, obj, layerMask);
		List<PlanterBox> obj2 = Facepunch.Pool.Get<List<PlanterBox>>();
		foreach (IHeatSourceListener item in obj)
		{
			if (item is GrowableEntity growableEntity)
			{
				if (!growableEntity.isServer)
				{
					continue;
				}
				PlanterBox planter = growableEntity.GetPlanter();
				if (planter != null && !obj2.Contains(planter))
				{
					obj2.Add(planter);
					planter.ForceTemperatureUpdate();
				}
			}
			item.OnHeatSourceChanged();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public override void InitShared()
	{
		base.InitShared();
		if (base.baseEntity.isServer)
		{
			FarmHeatSourceGrid.RegisterEntity(this, base.baseEntity);
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.baseEntity.isServer)
		{
			FarmHeatSourceGrid.DeregisterEntity(this);
		}
	}
}

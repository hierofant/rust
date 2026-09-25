using UnityEngine;

public class BaseVehicleMountPoint : BaseMountable
{
	[Header("BaseVehicleMountPoint")]
	[Tooltip("Only Set this if you definitely need a VehicleFixedUpdate tick on the seat for some reason")]
	public bool RequiresVehicleFixedUpdateOnSeat;

	public override bool DirectlyMountable()
	{
		return false;
	}

	public override BaseVehicle VehicleParent()
	{
		if (ignoreVehicleParent)
		{
			return null;
		}
		BaseVehicle baseVehicle = GetParentEntity() as BaseVehicle;
		while (baseVehicle != null && !baseVehicle.IsVehicleRoot())
		{
			BaseVehicle baseVehicle2 = baseVehicle.GetParentEntity() as BaseVehicle;
			if (baseVehicle2 == null)
			{
				return baseVehicle;
			}
			baseVehicle = baseVehicle2;
		}
		return baseVehicle;
	}

	public override float WaterFactorForPlayer(BasePlayer player, out WaterLevel.WaterInfo info)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle == null)
		{
			info = default(WaterLevel.WaterInfo);
			return 0f;
		}
		return baseVehicle.WaterFactorForPlayer(player, out info);
	}

	public override float AirFactor()
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle == null)
		{
			return 0f;
		}
		return baseVehicle.AirFactor();
	}

	public override bool BlocksWaterFor(BasePlayer player)
	{
		BaseVehicle baseVehicle = VehicleParent();
		if (baseVehicle == null)
		{
			return false;
		}
		return baseVehicle.BlocksWaterFor(player);
	}
}

using Rust;
using UnityEngine;

public class VehicleLiftOccupantTrigger : TriggerBase
{
	public bool checkNonModularCarVehicles;

	public ModularCar carOccupant { get; private set; }

	public BaseVehicle vehicleOccupant { get; private set; }

	protected override void OnDisable()
	{
		if (!Rust.Application.isQuitting)
		{
			base.OnDisable();
			if (carOccupant != null)
			{
				carOccupant = null;
			}
			if (vehicleOccupant != null)
			{
				vehicleOccupant = null;
			}
		}
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if (base.InterestedInObject(obj) == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null || baseEntity.isClient)
		{
			return null;
		}
		if (checkNonModularCarVehicles)
		{
			if (!(baseEntity is BaseVehicle))
			{
				return null;
			}
		}
		else if (!(baseEntity is ModularCar))
		{
			return null;
		}
		return obj;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (checkNonModularCarVehicles)
		{
			if (vehicleOccupant == null && ent.isServer)
			{
				vehicleOccupant = (BaseVehicle)ent;
			}
			if (carOccupant == null && ent.isServer && ent is ModularCar modularCar)
			{
				carOccupant = modularCar;
			}
		}
		else if (carOccupant == null && ent.isServer)
		{
			carOccupant = (ModularCar)ent;
		}
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (!(carOccupant == ent) && (!checkNonModularCarVehicles || !(vehicleOccupant == ent)))
		{
			return;
		}
		vehicleOccupant = null;
		carOccupant = null;
		if (entityContents == null || entityContents.Count <= 0)
		{
			return;
		}
		foreach (BaseEntity entityContent in entityContents)
		{
			if (!(entityContent != null))
			{
				continue;
			}
			if (checkNonModularCarVehicles)
			{
				vehicleOccupant = (BaseVehicle)entityContent;
				if (entityContent is ModularCar modularCar)
				{
					carOccupant = modularCar;
				}
			}
			else
			{
				carOccupant = (ModularCar)entityContent;
			}
			break;
		}
	}
}

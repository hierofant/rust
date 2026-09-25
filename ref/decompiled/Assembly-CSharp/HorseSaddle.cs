using UnityEngine;

public class HorseSaddle : BaseVehicleSeat
{
	[SerializeField]
	private bool isDriver;

	[SerializeField]
	private Transform eyePosRef;

	private RidableHorse _owner;

	protected RidableHorse Owner
	{
		get
		{
			if (_owner == null)
			{
				_owner = GetComponentInParent<RidableHorse>();
			}
			return _owner;
		}
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override void VehicleFixedUpdate()
	{
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		BasePlayer mounted = GetMounted();
		if (mounted != null)
		{
			BaseVehicle baseVehicle = VehicleParent();
			if (baseVehicle != null)
			{
				baseVehicle.PlayerMounted(mounted, this);
			}
		}
	}
}

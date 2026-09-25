using UnityEngine;

public class DiveSite : JunkPile
{
	public Transform bobber;

	public override bool DespawnIfAnyLootTaken => false;

	public override float TimeoutPlayerCheckRadius()
	{
		return 80f;
	}

	public override void Spawn()
	{
		base.Spawn();
		if (Physics.CheckSphere(base.transform.position.WithY(0f), 5f, 134217728))
		{
			Kill();
		}
		else if (BoatBuildingStation.GetStationIntersectingOBB(new OBB(base.transform.position.WithY(0f), base.transform.lossyScale, base.transform.rotation, bounds), isServer: true) != null)
		{
			Kill();
		}
	}
}

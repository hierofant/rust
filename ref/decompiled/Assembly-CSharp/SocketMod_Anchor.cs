using UnityEngine;

public class SocketMod_Anchor : SocketMod
{
	public float Radius = 0.38f;

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 pos = place.position + place.rotation * worldPosition;
		return CanSeeNetting(pos);
	}

	private bool CanSeeNetting(Vector3 pos)
	{
		pos += Vector3.up;
		if (GamePhysics.Trace(new Ray(pos, Vector3.down), Radius, out var hitInfo, 10f, 144769024))
		{
			return hitInfo.collider.GetComponent<BoatBuildingNetting>() != null;
		}
		return false;
	}
}

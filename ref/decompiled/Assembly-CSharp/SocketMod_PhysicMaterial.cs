using UnityEngine;

public class SocketMod_PhysicMaterial : SocketMod
{
	public PhysicsMaterial[] ValidMaterials;

	private PhysicsMaterial foundMaterial;

	public override bool DoCheck(ref Construction.Placement place)
	{
		if (Physics.Raycast(place.position + place.rotation.eulerAngles.normalized * 0.5f, -place.rotation.eulerAngles.normalized, out var hitInfo, 1f, 161546240, QueryTriggerInteraction.Ignore))
		{
			foundMaterial = ColliderEx.GetMaterialAt(hitInfo.collider, hitInfo.point);
			PhysicsMaterial[] validMaterials = ValidMaterials;
			for (int i = 0; i < validMaterials.Length; i++)
			{
				if (validMaterials[i] == foundMaterial)
				{
					return true;
				}
			}
		}
		return false;
	}
}

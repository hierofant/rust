using UnityEngine;

public class HarborCrane : HarborProximityEntity
{
	public Transform CraneGrab;

	public Transform ArmRoot;

	public Transform ArmSupportLower;

	public Transform ArmSupportUpper;

	public TransformLineRenderer[] LineRenderers;

	protected void UpdateArmSupports(Vector3 fwd)
	{
		if (!(ArmSupportUpper == null) && !(ArmSupportLower == null))
		{
			Vector3 normalized = (ArmSupportUpper.position - ArmSupportLower.position).normalized;
			ArmSupportLower.rotation = Quaternion.LookRotation(fwd, normalized);
			ArmSupportUpper.rotation = Quaternion.LookRotation(fwd, normalized);
		}
	}
}

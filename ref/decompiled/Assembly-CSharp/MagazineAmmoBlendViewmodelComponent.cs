using System;
using UnityEngine;

public class MagazineAmmoBlendViewmodelComponent : MonoBehaviour, IViewmodelComponent, IAnimationEventReceiver
{
	[Serializable]
	public struct JointPose
	{
		public Vector3 Position;

		public Vector3 Rotation;
	}

	[Serializable]
	public class JointBlendSetup
	{
		public Transform TargetJoint;

		[Tooltip("Local offsets ordered from empty (index 0) to full (last index), blended between the two nearest entries based on the current ammo fraction, then added on top of TargetJoint's animated local pose for this frame.")]
		public JointPose[] AmmoPoses;
	}

	[Serializable]
	public class MaterialUVSetup
	{
		public Renderer TargetRenderer;

		public string MaterialProperty = "_Cutoff";

		[Tooltip("Values ordered from empty (index 0) to full (last index), blended between the two nearest entries based on the current ammo fraction — same indexing as JointBlendSetups' AmmoPoses.")]
		public float[] Values;
	}

	public JointBlendSetup[] JointBlendSetups;

	[Tooltip("Material float properties driven by the same ammo fraction as the joints, via MaterialPropertyBlock (no material instancing).")]
	public MaterialUVSetup[] MaterialUVSetups;

	[Tooltip("Safety cap on how long the forced full-ammo pose (triggered by the 'FullAmmoPose' animation event) can be held if reloading never actually stops. Normally released as soon as ClientIsReloading() goes false.")]
	[Min(0f)]
	public float ForceFullPoseDuration = 5f;
}

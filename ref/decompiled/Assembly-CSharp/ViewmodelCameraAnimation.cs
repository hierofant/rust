using UnityEngine;

public class ViewmodelCameraAnimation : FacepunchBehaviour
{
	public Transform CameraModifyBone;

	public bool ShouldSuppressLeftHandScreenShake;

	public float FadeInTime;

	public Vector3 PositionOffset;

	public Vector3 RotationOffset;

	public Animator CameraAnimator;
}

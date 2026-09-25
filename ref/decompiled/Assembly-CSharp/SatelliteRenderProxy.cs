using UnityEngine;

public class SatelliteRenderProxy : SatelliteCameraProxyBase
{
	[Tooltip("Pull the model to at most this distance from the camera so it isn't culled.")]
	public float maxCameraDistance = 2500f;

	[Tooltip("Never draw the model smaller than it would look at this distance. ")]
	public float minApparentDistance = 2000f;

	[Header("Approach Grow Ramp")]
	public float growStartApparentDistance = 1500f;

	public float growEndApparentDistance = 450f;

	[Tooltip("Shapes the ramp. Time 0 = where the approach began (first projected frame), 1 = reaching the camera; value blends start (0) to end (1) apparent distance.")]
	public AnimationCurve growEase = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
}

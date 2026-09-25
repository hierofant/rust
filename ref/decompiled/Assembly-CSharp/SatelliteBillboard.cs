using UnityEngine;

public class SatelliteBillboard : SatelliteCameraProxyBase
{
	[Tooltip("Distance (m) at which the object is shown at its authored scale. Farther away it scales up to hold a constant on-screen size. Lower = appears bigger. Set 0 to disable scaling (face-only).")]
	public float referenceDistance = 1000f;

	[Tooltip("Maximum up-scale applied when far away (safety cap).")]
	public float maxScale = 100f;

	public float maxCameraDistance;

	[HideInInspector]
	public float sizeMultiplier = 1f;
}

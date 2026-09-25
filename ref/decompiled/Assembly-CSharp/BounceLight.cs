using System.Collections.Generic;
using UnityEngine;

public class BounceLight : MonoBehaviour
{
	public Light masterLight;

	public float maxBounceLightDistance;

	public float maxBounceLightSpawnRange;

	public List<Light> bouncedLights = new List<Light>();

	public GameObject bounceLightPrefab;

	public int maxBounceLights = 64;

	public float placementBias = 0.2f;

	public float radiusFraction = 2f;

	public float minPlacementDistance;

	public float bounceIntensityOverrideScale = 1f;

	public float mergeDist = 1f;
}

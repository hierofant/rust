using UnityEngine;

public class LODGroupDeepSeaConfig : MonoBehaviour, IClientComponent
{
	public LODGroup Group;

	[Range(0f, 1f)]
	public float MainlandCullPercent = 0.02f;

	[Range(0f, 1f)]
	public float DeepSeaCullPercent = 0.005f;
}

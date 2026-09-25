using UnityEngine;

public class WorkshopRenderSettings : MonoBehaviour
{
	[ItemSelector]
	public ItemDefinition ItemDefinition;

	public int itemID;

	public bool ToggleLightingRig;

	public Transform LightingRig;
}

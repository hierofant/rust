using UnityEngine;

public class SkinViewerRenderSettings : MonoBehaviour
{
	[ItemSelector]
	public ItemDefinition ItemDefinition;

	public int itemID;

	public Transform CustomLightingRig;

	[Space]
	public bool overrideFullScreenPosition;

	public Vector3 fullScreenPositionOffset;

	[Space]
	public bool overrideFullScreenRotation;

	public Vector3 fullScreenRotation;

	[Space]
	public bool overrideZoom;

	public Vector2 minMaxZoom = new Vector2(20f, 8f);

	[Space]
	public bool limitVerticalRotation;

	public bool limitHorizontalRotation;

	public float maxRotationAngle = 15f;
}

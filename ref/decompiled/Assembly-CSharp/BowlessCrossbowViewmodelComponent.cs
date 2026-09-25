using UnityEngine;

public class BowlessCrossbowViewmodelComponent : MonoBehaviour, IViewmodelComponent
{
	[SerializeField]
	private Transform wheelTransform;

	[SerializeField]
	private Renderer crossbowRenderer;

	[SerializeField]
	private int stringMaterialIndex;

	[SerializeField]
	private float stringTextureOffsetMultiplier = 1f;
}

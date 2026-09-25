using UnityEngine;

public class DynamicDirtLayerController : MonoBehaviour, IClientComponent, ICustomMaterialReplacer
{
	[Range(0f, 1f)]
	public float Amount = 0.5f;

	public Texture DirtTexture;
}

using UnityEngine;

public class DeepSeaBuoy : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color colorOpen = Color.green;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color colorClosed = Color.red;

	[SerializeField]
	private GameObject lightClosed;

	[SerializeField]
	private GameObject lightOpen;

	private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	private static MaterialPropertyBlock propertyBlock;

	private void OnEnable()
	{
		UpdateLights();
	}

	public void UpdateLights()
	{
		DeepSeaManager deepSeaManager = DeepSeaManager.Get(server: false);
		bool flag = deepSeaManager != null && deepSeaManager.IsAccessible();
		if (lightClosed != null)
		{
			lightClosed.SetActive(!flag);
		}
		if (lightOpen != null)
		{
			lightOpen.SetActive(flag);
		}
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetColor(EmissionColor, flag ? colorOpen : colorClosed);
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			if (meshRenderer != null)
			{
				meshRenderer.SetPropertyBlock(propertyBlock);
			}
		}
	}
}

using UnityEngine;

public class EmissionToggle : MonoBehaviour, IClientComponent, ICustomMaterialReplacer
{
	private Color emissionColor;

	public Renderer[] targetRenderers;

	public int materialIndex = -1;
}

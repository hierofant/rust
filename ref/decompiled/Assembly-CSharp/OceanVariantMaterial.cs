using UnityEngine;

[CreateAssetMenu(fileName = "New Ocean Variant Material", menuName = "Water5/Ocean Variant Material")]
public class OceanVariantMaterial : ScriptableObject
{
	public Color color;

	public Color specColor;

	public float smoothness;

	public Color waterColor;

	public Color waterExtinction;

	public float scatteringCoefficient;

	public Color subSurfaceColor;

	public float subSurfaceAmount;

	public float fadeInEnd;

	public float fadeInBegin;
}

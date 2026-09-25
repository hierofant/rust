using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Deploy Guide Material")]
public class DeployGuideMaterial : ScriptableObject
{
	public Color Albedo = Color.white;

	public Color Emission = Color.white;

	public float EmissionStrength = 1f;

	public float FresnelPower = 1f;

	public float FresnelStrength = 1f;

	public float RimPower = 1f;

	public float RimStrength = 1f;

	public float Alpha = 0.5f;

	public float AlphaFresnelPower = 1f;

	public float BackfaceBrightness = 1f;

	public float BackfaceAmount = 0.1f;
}

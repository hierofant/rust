using UnityEngine;

namespace Rust.Water5;

[CreateAssetMenu(fileName = "New Spectrum Settings", menuName = "Water5/Spectrum Settings")]
public class OceanSpectrumSettings : ScriptableObject
{
	public OceanSettings oceanSettings;

	[Header("Deep Wave Settings")]
	public float g;

	public float beaufort;

	public float depth;

	public SpectrumSettings local;

	public SpectrumSettings swell;

	[Header("Material Settings")]
	public MaterialSettings mainlandMaterial;

	public MaterialSettings deepSeaMaterial;

	[Button("Update Spectrum")]
	public void UpdateSpectrum()
	{
		WaterSystem.Instance?.Refresh();
	}

	public MaterialSettings GetMaterial(bool inDeepSea)
	{
		if (!inDeepSea)
		{
			return mainlandMaterial;
		}
		return deepSeaMaterial;
	}
}

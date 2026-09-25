using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Volume Clouds/Cirrus Config")]
public class VolumeCloudsCirrusConfig : ScriptableObject
{
	public VolumeCloudsWeatherLayerConfig WeatherLayer = new VolumeCloudsWeatherLayerConfig();

	public float Opaqueness = 1f;

	public float Absorption = 0.2f;

	public float Eccentricity = 0.3f;

	public float CloudTextureGamma = 1f;

	public float CloudTextureScale = 1f;

	public void CopyWeatherGen(VolumeCloudsCirrusConfig copy)
	{
		WeatherLayer.CopyFrom(copy.WeatherLayer);
	}

	public void CopyFrom(VolumeCloudsCirrusConfig copy)
	{
		CopyWeatherGen(copy);
		Opaqueness = copy.Opaqueness;
		Absorption = copy.Absorption;
		Eccentricity = copy.Eccentricity;
		CloudTextureGamma = copy.CloudTextureGamma;
		CloudTextureScale = copy.CloudTextureScale;
	}

	public void Lerp(VolumeCloudsCirrusConfig a, VolumeCloudsCirrusConfig b, float t)
	{
		Opaqueness = Mathf.Lerp(a.Opaqueness, b.Opaqueness, t);
		Absorption = Mathf.Lerp(a.Absorption, b.Absorption, t);
		Eccentricity = Mathf.Lerp(a.Eccentricity, b.Eccentricity, t);
		CloudTextureGamma = Mathf.Lerp(a.CloudTextureGamma, b.CloudTextureGamma, t);
		CloudTextureScale = Mathf.Lerp(a.CloudTextureScale, b.CloudTextureScale, t);
	}
}

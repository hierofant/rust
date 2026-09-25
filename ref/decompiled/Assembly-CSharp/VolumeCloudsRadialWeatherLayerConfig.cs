using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Volume Clouds/Storm Layer Config")]
public class VolumeCloudsRadialWeatherLayerConfig : ScriptableObject
{
	public VolumeCloudsWeatherLayerConfig Weather = new VolumeCloudsWeatherLayerConfig();

	public bool Enabled;

	public float Radius;

	public Vector2 Offset;

	public float Falloff;

	public float Blend;

	public bool FollowCamera;

	public void CopyFrom(VolumeCloudsRadialWeatherLayerConfig copy)
	{
		Enabled = copy.Enabled;
		Radius = copy.Radius;
		Offset = copy.Offset;
		Falloff = copy.Falloff;
		Blend = copy.Blend;
		FollowCamera = copy.FollowCamera;
	}
}

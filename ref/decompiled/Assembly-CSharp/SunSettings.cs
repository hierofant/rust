using ConVar;
using UnityEngine;

[ExecuteAlways]
public class SunSettings : MonoBehaviour, IClientComponent
{
	private static readonly int mainLightDirectionId = Shader.PropertyToID("_MainLightDir");

	private static readonly int mainLightColorId = Shader.PropertyToID("_MainLightColor");

	private Light light;

	private void OnEnable()
	{
		light = GetComponent<Light>();
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			LightShadows lightShadows = (LightShadows)Mathf.Clamp(ConVar.Graphics.shadowmode, 1, 2);
			if (light.shadows != lightShadows)
			{
				light.shadows = lightShadows;
			}
		}
		if (light != null)
		{
			Shader.SetGlobalColor(mainLightColorId, light.color.linear * light.intensity);
			Shader.SetGlobalVector(mainLightDirectionId, light.transform.forward);
		}
	}
}

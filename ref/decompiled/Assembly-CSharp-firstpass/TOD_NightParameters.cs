using System;
using UnityEngine;

[Serializable]
public class TOD_NightParameters
{
	[Tooltip("Color of the moon mesh.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient MoonColor;

	[Tooltip("Color of the moon mesh when red night mode is active.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient MoonColorRed;

	[Tooltip("Color of the light that hits the ground.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient LightColor;

	[Tooltip("Color of the god rays.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient RayColor;

	[Tooltip("Color of the light that hits the atmosphere.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient SkyColor;

	[Tooltip("Color of the clouds.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient CloudColor;

	[Tooltip("Color of the atmosphere fog.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient FogColor;

	[Tooltip("Color of the ambient light.\nLeft value: Sun at horizon.\nRight value: Sun opposite to zenith.")]
	public Gradient AmbientColor;

	[HideInInspector]
	public float runtimeLightIntensity = 0.1f;

	[Tooltip("Opacity of the shadows dropped by the light source.")]
	[Range(0f, 1f)]
	public float ShadowStrength = 1f;

	[HideInInspector]
	public float runtimeAmbientMultiplier = 1f;

	[HideInInspector]
	public float runtimeReflectionMultiplier = 1f;

	[Tooltip("Max reflection multiplier")]
	[Range(0f, 1f)]
	public float ReflectionMaxClamp = 1f;

	[field: Tooltip("Intensity of the light source.")]
	[field: SerializeField]
	[field: TOD_Min(0f)]
	public float LightIntensity { get; private set; } = 0.1f;


	[field: Tooltip("Brightness multiplier of the ambient light.")]
	[field: SerializeField]
	[field: Range(0f, 8f)]
	public float AmbientMultiplier { get; private set; } = 1f;


	[field: Tooltip("Brightness multiplier of the reflection probe.")]
	[field: SerializeField]
	[field: Range(0f, 1f)]
	public float ReflectionMultiplier { get; private set; } = 1f;

}

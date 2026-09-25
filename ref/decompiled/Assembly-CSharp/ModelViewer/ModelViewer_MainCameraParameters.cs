using System;
using UnityEngine;

namespace ModelViewer;

[Serializable]
public class ModelViewer_MainCameraParameters
{
	public enum OverlayType
	{
		None,
		RuleOfThirds,
		GoldenRatio,
		CrossHair,
		AspectRatio,
		SafeFrames
	}

	public enum TonemappingMode
	{
		None,
		Neutral,
		ACES
	}

	[Header("Control")]
	public float orbitSpeed = 4f;

	public float mouseZoomSpeed = 1.2f;

	public float scrollZoomSpeed = 0.1f;

	public float panSpeed = 0.1f;

	public float moveSpeed = 0.05f;

	public float followLag;

	[Header("Lens")]
	[Header("                ")]
	[Range(0.01f, 360f)]
	public float fieldOfView = 15f;

	public float nearPlane = 0.05f;

	public float farPlane = 2500f;

	public OverlayType overlayType;

	[Range(0.01f, 100f)]
	public float Aspect = 4f;

	[Range(0.01f, 100f)]
	public float Ratio = 3f;

	[Header("Focus")]
	[Header("                ")]
	public bool depthOfField;

	public float focalLength = 3f;

	public float focalSize = 0.1f;

	public float apeture = 80f;

	public float maxBlurSize = 7f;

	public bool debugMode;

	[Header("Motion Blur")]
	[Header("                ")]
	[Range(0f, 360f)]
	public float shutterAngle;

	[Header("                ")]
	[Header("Distortion")]
	[Range(-100f, 100f)]
	public float barrelAndPincushion;

	[Range(0f, 1f)]
	public float chromaticAbberation;

	[Header("Post Effects")]
	[Header("                ")]
	public TonemappingMode ToneMapping = TonemappingMode.Neutral;

	public float Exposure = 2.34f;

	[Range(-100f, 100f)]
	public float Contrast;

	[Range(-100f, 100f)]
	public float Saturation;

	[Header("                ")]
	[Header("Sharpen")]
	public float Strength;

	public float limit;

	[Header("                ")]
	[Header("God Rays")]
	public bool GodRays = true;

	[Header("                ")]
	[Header("Bloom")]
	public float Brightness = 0.15f;

	public float Threshold = 1f;

	[Range(0f, 1f)]
	public float SoftKnee = 0.5f;

	public float Clamp;

	[Range(0f, 10f)]
	public float Diffusion = 8f;

	[Range(0f, 1f)]
	public float AnamorphicRatio = 0.55f;

	public Color Color = Color.white;

	[Header("Vignette")]
	[Header("                ")]
	public float Darkness;

	public float Sharpness;

	[Header("Grain")]
	[Header("                ")]
	public bool Coloured = true;

	[Range(0f, 1f)]
	public float Intensity;

	[Range(0.3f, 3f)]
	public float Size = 1f;

	[Range(0f, 1f)]
	public float luminanceContribution = 0.8f;
}

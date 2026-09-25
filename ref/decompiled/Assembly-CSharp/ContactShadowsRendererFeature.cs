using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/ContactShadowsRendererFeature")]
public class ContactShadowsRendererFeature : RustRendererFeature
{
	[Range(0f, 1f)]
	public float blendStrength = 1f;

	[Range(0f, 1f)]
	public float accumulation = 0.9f;

	[Range(0.1f, 5f)]
	public float lengthFade = 0.7f;

	[Range(0.01f, 5f)]
	public float range = 0.7f;

	[Range(0f, 1f)]
	public float zThickness = 0.1f;

	[Range(2f, 92f)]
	public int samples = 32;

	[Range(0.5f, 4f)]
	public float nearSampleQuality = 1.5f;

	[Range(0f, 1f)]
	public float traceBias = 0.03f;

	public bool stochasticSampling = true;

	public bool leverageTemporalAA;

	public bool bilateralBlur = true;

	[Range(1f, 2f)]
	public int blurPasses = 1;

	[Range(0.01f, 0.5f)]
	public float blurDepthTolerance = 0.1f;

	public Shader screenSpaceShadowsShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return new ContactShadowsCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return new ContactShadowsCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

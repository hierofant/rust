using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AtmosphereVolumeRendererFeature")]
public class AtmosphereVolumeRendererFeature : RustRendererFeature
{
	public FogMode Mode = FogMode.ExponentialSquared;

	public bool DistanceFog = true;

	public bool HeightFog = true;

	public Shader fogVolumeShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return new AtmosphereVolumeCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return new AtmosphereVolumeCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

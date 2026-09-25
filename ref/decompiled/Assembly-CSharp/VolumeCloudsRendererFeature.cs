using Rust.RenderPipeline.Runtime;
using Unity.Profiling;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/VolumeCloudsRendererFeature")]
public class VolumeCloudsRendererFeature : RustRendererFeature
{
	private VolumeCloudsShadowPass _shadowPass;

	private VolumeCloudsDrawPass _drawPass;

	private VolumeCloudsUpscalePass _upscalePass;

	private VolumeCloudsAtmosphericScatteringPass _scatteringPass;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return new VolumeCloudsCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return new VolumeCloudsCameraContext();
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
		renderer.FrameData.Create<VolumeCloudsFrameData>();
		renderer.EnqueuePass(_shadowPass);
		renderer.EnqueuePass(_drawPass);
		renderer.EnqueuePass(_upscalePass);
		renderer.EnqueuePass(_scatteringPass);
	}

	public override void Create()
	{
		if (_shadowPass == null)
		{
			_shadowPass = new VolumeCloudsShadowPass
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingGBuffer,
				profilerMarker = new ProfilerMarker("VolumeCloudsShadowPass")
			};
		}
		if (_drawPass == null)
		{
			_drawPass = new VolumeCloudsDrawPass
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingDeferredLights,
				profilerMarker = new ProfilerMarker("VolumeCloudsDrawPass"),
				sort = 2
			};
		}
		if (_upscalePass == null)
		{
			_upscalePass = new VolumeCloudsUpscalePass
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingDeferredLights,
				profilerMarker = new ProfilerMarker("VolumeCloudsUpscalePass"),
				sort = 3
			};
		}
		if (_scatteringPass == null)
		{
			_scatteringPass = new VolumeCloudsAtmosphericScatteringPass
			{
				renderPassEvent = RenderPassEvent.BeforeRenderingDeferredLights,
				profilerMarker = new ProfilerMarker("VolumeCloudsAtmosphericScatteringPass"),
				sort = 4
			};
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			_shadowPass = null;
			_drawPass = null;
			_upscalePass = null;
			_scatteringPass = null;
		}
	}
}

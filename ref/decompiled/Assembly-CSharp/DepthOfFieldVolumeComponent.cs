using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Depth of Field")]
public class DepthOfFieldVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public FloatParameter focalLength = new FloatParameter(10f);

	public FloatParameter focalSize = new FloatParameter(0.05f);

	public FloatParameter aperture = new FloatParameter(11.5f);

	[Range(0f, 3f)]
	public FloatParameter anamorphicSqueeze = new FloatParameter(0f);

	[Range(0f, 1f)]
	public FloatParameter anamorphicBarrel = new FloatParameter(0f);

	public FloatParameter maxBlurSize = new FloatParameter(2f);

	public BoolParameter highResolution = new BoolParameter(value: true);

	public BoolParameter enabled = new BoolParameter(value: false);

	public DOFBlurSampleCountParameter_RRP blurSampleCount = new DOFBlurSampleCountParameter_RRP
	{
		value = DOFBlurSampleCount_RRP.Low
	};

	public Transform focalTransform;

	public bool IsActive()
	{
		if (active)
		{
			return enabled.value;
		}
		return false;
	}
}

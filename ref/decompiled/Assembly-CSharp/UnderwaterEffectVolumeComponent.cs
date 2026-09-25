using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("RRP/Underwater Post Effect")]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
public class UnderwaterEffectVolumeComponent : VolumeComponent, IPostProcessComponent
{
	[Header("Wiggle")]
	public BoolParameter wiggle = new BoolParameter(value: true);

	public FloatParameter speed = new FloatParameter(1f);

	public FloatParameter scale = new FloatParameter(12f);

	[Header("Water Line")]
	public ColorParameter waterLineColor = new ColorParameter(Color.white);

	public IntParameter waterLineBlurIterations = new IntParameter(1);

	public FloatParameter waterLineBlurSize = new FloatParameter(0f);

	[Header("Blur")]
	[Range(0f, 2f)]
	public IntParameter downsample = new IntParameter(0);

	[Range(1f, 4f)]
	public IntParameter blurIterations = new IntParameter(1);

	[Range(0f, 10f)]
	public FloatParameter blurSize = new FloatParameter(0f);

	public FloatParameter fadeToBlurDistance = new FloatParameter(0f);

	[Header("General")]
	public BoolParameter effectActive = new BoolParameter(value: false);

	public bool IsActive()
	{
		if (active)
		{
			return effectActive.value;
		}
		return false;
	}
}

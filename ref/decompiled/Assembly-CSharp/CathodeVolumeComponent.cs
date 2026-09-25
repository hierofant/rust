using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Cathode")]
public class CathodeVolumeComponent : VolumeComponent, IPostProcessComponent
{
	public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedIntParameter downscale = new ClampedIntParameter(1, 1, 16);

	public ClampedIntParameter downscaleTemporal = new ClampedIntParameter(1, 1, 16);

	public ClampedFloatParameter horizontalBlur = new ClampedFloatParameter(1f, 0f, 3f);

	public ClampedFloatParameter verticalBlur = new ClampedFloatParameter(1f, 0f, 3f);

	public ClampedFloatParameter chromaSubsampling = new ClampedFloatParameter(1.7f, 0f, 5f);

	public ClampedFloatParameter sharpen = new ClampedFloatParameter(1.2f, 0f, 5f);

	public ClampedFloatParameter sharpenRadius = new ClampedFloatParameter(1.2f, 0f, 5f);

	public ClampedFloatParameter colorNoise = new ClampedFloatParameter(0.05f, 0f, 0.5f);

	public ClampedFloatParameter restlessFoot = new ClampedFloatParameter(0.2f, 0f, 5f);

	public ClampedFloatParameter footAmplitude = new ClampedFloatParameter(0.02f, 0f, 0.1f);

	public ClampedFloatParameter chromaIntensity = new ClampedFloatParameter(1f, 0f, 3f);

	public ClampedFloatParameter chromaInstability = new ClampedFloatParameter(1f, 0f, 1f);

	public ClampedFloatParameter chromaOffset = new ClampedFloatParameter(0.02f, 0f, 0.1f);

	public ClampedFloatParameter responseCurve = new ClampedFloatParameter(0f, -2f, 2f);

	public ClampedFloatParameter saturation = new ClampedFloatParameter(1f, -1f, 1f);

	public ClampedFloatParameter cometTrailing = new ClampedFloatParameter(0.3f, 0f, 1f);

	public ClampedFloatParameter burnIn = new ClampedFloatParameter(0.1f, 0f, 1f);

	public ClampedFloatParameter tapeDust = new ClampedFloatParameter(0.1f, 0f, 1f);

	public ClampedFloatParameter wobble = new ClampedFloatParameter(1f, 0f, 2f);

	public Vector2Parameter blackWhiteLevels = new Vector2Parameter(new Vector2(0f, 1f));

	public Vector2Parameter dynamicRange = new Vector2Parameter(new Vector2(0f, 1f));

	public ClampedFloatParameter whiteBalance = new ClampedFloatParameter(0f, -1f, 1f);

	public bool IsActive()
	{
		if (active)
		{
			return intensity.value > 0f;
		}
		return false;
	}
}

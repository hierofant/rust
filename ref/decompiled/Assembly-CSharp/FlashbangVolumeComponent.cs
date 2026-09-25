using System;
using Rust.RenderPipeline.Runtime;
using UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(typeof(RustRenderPipelineAsset))]
[VolumeComponentMenu("RRP/Flashbang")]
public class FlashbangVolumeComponent : VolumeComponent, IPostProcessComponent
{
	private const float ActivationThreshold = 0.001f;

	public ClampedFloatParameter burnIntensity = new ClampedFloatParameter(0f, 0f, 1f);

	public ClampedFloatParameter whiteoutIntensity = new ClampedFloatParameter(0f, 0f, 1f);

	public bool IsActive()
	{
		if (active)
		{
			if (!(burnIntensity.value > 0.001f))
			{
				return whiteoutIntensity.value > 0.001f;
			}
			return true;
		}
		return false;
	}
}

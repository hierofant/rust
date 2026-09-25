using Rust.RenderPipeline.Runtime.PostProcessing.VolumeComponents;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class BlurManager : SingletonComponent<BlurManager>
{
	public PostProcessVolume postProcessVolume;

	public Volume postProcessVolumeRRP;

	public PostProcessProfile standardBlurProfile;

	public VolumeProfile standardBlurProfileRRP;

	public PostProcessProfile heavyBlurProfile;

	public VolumeProfile heavyBlurProfileRRP;

	public VolumeProfile uiBlurProfileRRP;

	private Rust.RenderPipeline.Runtime.PostProcessing.VolumeComponents.BlurOptimized uiBlurSettings;
}

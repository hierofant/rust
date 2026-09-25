using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class VolumetricFogFrameData : ContextItem
{
	public TextureHandle shadowMap;

	public TextureHandle fogVolume;

	public TextureHandle skyFogTexture;

	public override void Reset()
	{
		shadowMap = TextureHandle.nullHandle;
		fogVolume = TextureHandle.nullHandle;
		skyFogTexture = TextureHandle.nullHandle;
	}
}

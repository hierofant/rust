using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class VolumeCloudsFrameData : ContextItem
{
	public TextureHandle shadowMap;

	public TextureHandle lowResImage;

	public TextureHandle depthBuffer;

	public TextureHandle upscaledImage;

	public TextureHandle atmosphericScattering;

	public override void Reset()
	{
		shadowMap = TextureHandle.nullHandle;
		lowResImage = TextureHandle.nullHandle;
		depthBuffer = TextureHandle.nullHandle;
		upscaledImage = TextureHandle.nullHandle;
		atmosphericScattering = TextureHandle.nullHandle;
	}
}

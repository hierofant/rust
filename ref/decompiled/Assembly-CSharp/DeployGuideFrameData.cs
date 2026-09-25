using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class DeployGuideFrameData : ContextItem
{
	public TextureHandle depthHandle;

	public TextureHandle depthBackHandle;

	public TextureHandle normalsHandle;

	public TextureHandle normalsBackHandle;

	public DeployGuideRendererFeature rendererFeature;

	public override void Reset()
	{
		depthHandle = (depthBackHandle = TextureHandle.nullHandle);
		normalsHandle = (normalsBackHandle = TextureHandle.nullHandle);
		rendererFeature = null;
	}
}

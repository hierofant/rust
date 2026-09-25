using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class PostOpaqueDepthResourceData : ContextItem
{
	public TextureHandle postOpaqueDepthHandle;

	public override void Reset()
	{
		postOpaqueDepthHandle = TextureHandle.nullHandle;
	}
}

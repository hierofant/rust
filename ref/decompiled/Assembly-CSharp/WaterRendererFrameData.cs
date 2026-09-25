using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class WaterRendererFrameData : ContextItem
{
	public TextureHandle surfaceTex;

	public TextureHandle surfaceMotionTex;

	public TextureHandle surfaceMaskTex;

	public TextureHandle preFogBackgroundTex;

	public TextureHandle causticsTex;

	public TextureHandle ssrReflectionTex;

	public TextureHandle backgroundColorTex;

	public TextureHandle combinedDynamicsTex;

	public BufferHandle oceanVFaceBuf;

	public override void Reset()
	{
		surfaceTex = TextureHandle.nullHandle;
		surfaceMotionTex = TextureHandle.nullHandle;
		surfaceMaskTex = TextureHandle.nullHandle;
		preFogBackgroundTex = TextureHandle.nullHandle;
		backgroundColorTex = TextureHandle.nullHandle;
		causticsTex = TextureHandle.nullHandle;
		ssrReflectionTex = TextureHandle.nullHandle;
		oceanVFaceBuf = BufferHandle.nullHandle;
		combinedDynamicsTex = TextureHandle.nullHandle;
	}
}

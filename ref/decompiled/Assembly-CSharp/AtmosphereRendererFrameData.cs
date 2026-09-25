using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class AtmosphereRendererFrameData : ContextItem
{
	public BufferHandle ambientBuf;

	public override void Reset()
	{
		ambientBuf = BufferHandle.nullHandle;
	}
}

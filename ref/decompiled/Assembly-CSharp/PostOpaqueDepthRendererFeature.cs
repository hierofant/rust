using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/PostOpaqueDepth")]
public class PostOpaqueDepthRendererFeature : RustRendererFeature
{
	public Shader copyCameraDepthShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

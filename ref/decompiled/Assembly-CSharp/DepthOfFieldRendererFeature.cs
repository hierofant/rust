using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/DepthOfFieldRendererFeature")]
public class DepthOfFieldRendererFeature : RustRendererFeature
{
	public Shader dofShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

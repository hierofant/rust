using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/FlashbangRendererFeature")]
public class FlashbangRendererFeature : RustRendererFeature
{
	public Shader flashbangShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

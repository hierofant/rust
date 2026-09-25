using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/PostSubsurfaceRendererFeature")]
public class PostSubsurfaceRendererFeature : RustRendererFeature
{
	public bool halfResolution = true;

	public float radiusScale = 1f;

	public float depthScale = 100f;

	public Shader postProcessSubSurfaceShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

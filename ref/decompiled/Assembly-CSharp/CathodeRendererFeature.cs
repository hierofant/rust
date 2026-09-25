using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/CathodeRendererFeature")]
public class CathodeRendererFeature : RustRendererFeature
{
	public Shader grayShader;

	public Shader primaryTransformShader;

	public Shader trailShader;

	public Shader postTVShader;

	public Shader tvShader;

	public Texture2D noiseTexture;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

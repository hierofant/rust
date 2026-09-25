using Rust.RenderPipeline.Runtime;
using UnityEngine;

namespace Rendering.RendererFeatures.NightLight;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/NightLightRendererFeature")]
public class NightLightRendererFeature : RustRendererFeature
{
	[SerializeField]
	private Shader nightLightShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

using Rust.RenderPipeline.Runtime;
using UnityEngine;

namespace Rendering.RendererFeatures.LensDirtiness;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/LensDirtinessRendererFeature")]
public class LensDirtinessRendererFeature : RustRendererFeature
{
	[SerializeField]
	private Shader lensDirtinessShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

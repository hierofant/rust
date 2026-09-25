using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/DecalsRendererFeature")]
public class DecalsRendererFeature : RustRendererFeature
{
	[SerializeField]
	private bool enableDeferredDecals = true;

	[SerializeField]
	private bool enableDeferredMeshDecals = true;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

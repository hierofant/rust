using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/FoliageDisplacement")]
public class FoliageDisplacementRendererFeature : RustRendererFeature
{
	[SerializeField]
	private Material clearDisplacementMaterial;

	[SerializeField]
	private RenderPassEvent cameraEvent = RenderPassEvent.BeforeRenderingGBuffer;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

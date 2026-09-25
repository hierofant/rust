using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/TerrainHoleRendererFeature")]
public class TerrainHoleRendererFeature : RustRendererFeature
{
	[SerializeField]
	private RenderPassEvent cameraEvent = RenderPassEvent.BeforeRenderingGBuffer;

	[SerializeField]
	private Material stencilMaterial;

	[SerializeField]
	private Material holeMapMaterial;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

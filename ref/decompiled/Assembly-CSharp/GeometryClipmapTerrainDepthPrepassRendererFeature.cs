using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/GeometryClipmapTerrainDepthPrepass")]
public class GeometryClipmapTerrainDepthPrepassRendererFeature : RustRendererFeature
{
	public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingGBuffer;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

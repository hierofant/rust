using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/BinocularEffectRendererFeature")]
public class BinocularEffectRendererFeature : RustRendererFeature
{
	public Material effectMaterial;

	[Header("Day/Night Settings")]
	public float dayFresnel;

	public float nightFresnel;

	public float dayGlare;

	public float nightGlare;

	public float dayCoating;

	public float nightCoating;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/DeployGuideRendererFeature")]
public class DeployGuideRendererFeature : RustRendererFeature
{
	private static readonly int bumpMapId = Shader.PropertyToID("_BumpMap");

	private static readonly int bumpScaleId = Shader.PropertyToID("_BumpScale");

	private static readonly int mainTexId = Shader.PropertyToID("_MainTex");

	private static readonly int cutoffId = Shader.PropertyToID("_Cutoff");

	private static readonly int modeId = Shader.PropertyToID("_Mode");

	private static readonly int baseColorMapId = Shader.PropertyToID("_BaseColorMap");

	private static readonly int opacityMaskClipId = Shader.PropertyToID("_OpacityMaskClip");

	public DeployGuideRendererResources Resources;

	private readonly DeployGuideRendererInternalResources _internResources = new DeployGuideRendererInternalResources();

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

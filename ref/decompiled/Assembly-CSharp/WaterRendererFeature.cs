using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/WaterRendererFeature")]
public class WaterRendererFeature : RustRendererFeature
{
	[SerializeField]
	private ComputeShader oceanVFaceComputeShader;

	[SerializeField]
	private Shader interiorShader;

	[SerializeField]
	private Shader oceanUnderWaterFillShader;

	[SerializeField]
	private Shader multiCopyShader;

	[SerializeField]
	private Shader reflectionShader;

	[SerializeField]
	private Shader interactionShader;

	[SerializeField]
	private Shader combineInteractionsShader;

	[SerializeField]
	private Shader underwaterShader;

	[SerializeField]
	private Shader underwaterEffectShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return new WaterRendererCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return new WaterRendererCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/ViewmodelRendererFeature")]
public class ViewmodelRendererFeature : RustRendererFeature
{
	public Shader viewModelShader;

	public override RustRendererFeatureCameraBase CreateCameraComponent()
	{
		return new ViewmodelRendererCamera();
	}

	public override RustRendererFeatureCameraContext CreateCameraContext()
	{
		return new ViewmodelRendererCameraContext();
	}

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

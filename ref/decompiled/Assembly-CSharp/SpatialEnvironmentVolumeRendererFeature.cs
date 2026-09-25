using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/SpatialEnvironmentVolume")]
public class SpatialEnvironmentVolumeRendererFeature : RustRendererFeature
{
	[SerializeField]
	private Shader shader;

	[SerializeField]
	private Shader copyToDepthBufferShader;

	[SerializeField]
	private Mesh cubeMesh;

	[SerializeField]
	private Mesh sphereMesh;

	[SerializeField]
	private Mesh capsuleMesh;

	[SerializeField]
	private EnvironmentVolumePropertiesCollection environmentVolumeProperties;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

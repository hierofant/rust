using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(DeferredIndirectLightingPass))]
[RequireComponent(typeof(CommandBufferManager))]
[ExecuteInEditMode]
public class SpatialEnvironmentVolumePass : MonoBehaviour
{
	private const CameraEvent VOLUME_RENDERING_CAMERA_EVENT = CameraEvent.BeforeLighting;

	private const int VOLUME_RENDERING_ORDER_ID = 1000;

	private static readonly int envVolumeBufferId = Shader.PropertyToID("_EnvVolumeBuffer");

	private static readonly int numberOfVolumesId = Shader.PropertyToID("_NumberOfEnvironmentVolumes");

	[SerializeField]
	private Mesh cubeMesh;

	[SerializeField]
	private Mesh sphereMesh;

	[SerializeField]
	private Mesh capsuleMesh;

	[SerializeField]
	private Shader shader;

	[SerializeField]
	private EnvironmentVolumePropertiesCollection environmentVolumeProperties;

	[SerializeField]
	private Shader copyToDepthBufferShader;

	public bool IsInitialized { get; private set; }
}

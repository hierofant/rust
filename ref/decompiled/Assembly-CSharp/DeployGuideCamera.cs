using UnityEngine;

[RequireComponent(typeof(CommandBufferManager))]
[RequireComponent(typeof(Camera))]
public class DeployGuideCamera : SingletonComponent<DeployGuideCamera>
{
	public DeployGuideRendererResources Resources;

	private DeployGuideRendererInternalResources _internResources = new DeployGuideRendererInternalResources();
}

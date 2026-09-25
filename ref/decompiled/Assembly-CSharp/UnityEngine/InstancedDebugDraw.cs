namespace UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class InstancedDebugDraw : SingletonComponent<InstancedDebugDraw>
{
	public Material overlayMaterial;

	public Material depthTestedMaterial;
}

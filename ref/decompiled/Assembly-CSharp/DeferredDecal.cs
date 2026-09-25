using UnityEngine;

[ExecuteInEditMode]
public class DeferredDecal : MonoBehaviour, IClientComponent
{
	public Mesh mesh;

	public Material material;

	public DeferredDecalQueue queue;

	public bool applyImmediately = true;
}

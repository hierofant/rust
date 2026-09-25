using Rust.Workshop;
using UnityEngine;

public class RendererBatch : MonoBehaviour, IClientComponent, ICustomMaterialReplacer, IWorkshopPreview
{
	[SerializeField]
	[HideInInspector]
	public int MaxVertexCountOverride;

	[SerializeField]
	public bool AllowSubmeshes;
}

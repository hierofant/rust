using Rust.Workshop;
using UnityEngine;

public class SprayCanSpray_Decal : SprayCanSpray, ICustomMaterialReplacer, IPropRenderNotify, INotifyLOD, IWorkshopPreview
{
	public DeferredDecal DecalComponent;

	public GameObject IconPreviewRoot;

	public Material DefaultMaterial;
}

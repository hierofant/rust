using UnityEngine;

public class PaintballGunViewmodelComponent : MonoBehaviour, IViewmodelComponent, IAnimationEventReceiver
{
	private static readonly int shaderProperty_Color = Shader.PropertyToID("_Color");

	private static readonly int animatorHash_ammoCount = Animator.StringToHash("ammoCount");

	public SkinnedMeshRenderer[] paintballPellets;

	public Transform pelletVisibilityCutoff;

	public Material paintballAmmoMaterialReference;

	[Min(0f)]
	public float animatorAmmoCountLerpSeed = 0.1f;

	public int paintballCutoffIndexOffset;
}

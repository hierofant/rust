using UnityEngine;

public abstract class ArrowWeapon : BaseProjectile
{
	[Tooltip("Object holding the arrow, shown and hidden as one by the animation events")]
	public GameObject arrowRoot;

	[Tooltip("Arrow renderers that live outside arrowRoot")]
	public Renderer[] extraArrowRenderers;

	public override bool ForceSendMagazine(SaveInfo saveInfo)
	{
		return true;
	}
}

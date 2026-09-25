using System;
using UnityEngine;

public class MagazineStateViewmodel : MonoBehaviour, IClientComponent, IViewmodelComponent, IAnimationEventReceiver
{
	public Animator TargetAnimator;

	[Tooltip("These gameobjects will be toggled active if the gun has > 1 bullet.")]
	public GameObject[] ShellRoots = Array.Empty<GameObject>();

	[Tooltip("Will set layer 1 to full weight if the gun has no ammo.")]
	public bool EmptyAmmoLayer;

	[Tooltip("Will update parameter 'hasAmmo' on the vm animator.")]
	public bool SetHasAmmoParam;

	[Tooltip("Will update parameters 'ammo_true' and 'ammo_false' on the vm animator.")]
	public bool SetAmmoTrueFalseParams;
}

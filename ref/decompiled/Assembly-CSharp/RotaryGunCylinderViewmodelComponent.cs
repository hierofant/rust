using System;
using UnityEngine;

public class RotaryGunCylinderViewmodelComponent : MonoBehaviour, IViewmodelComponent, IAnimationEventReceiver
{
	[Serializable]
	public class AmmoVisualSwapData
	{
		public ItemDefinition AmmoItem;

		public GameObject[] AmmoVisualRoots;
	}

	public enum RotationType
	{
		Reload,
		GunFired
	}

	public Transform AmmoCylinderRoot;

	[Tooltip("Root GameObject for the ammo piece which is visible in the midst of a reload.")]
	public GameObject MainReloadingAmmoRoot;

	[Tooltip("Root GameObjects for the ammo pieces visible inside the cylinder.")]
	public GameObject[] AmmoLocationRoots;

	public AmmoVisualSwapData[] AmmoVisualSwapSetups;

	[Min(0f)]
	public float AmmoEjectionVisibleDuration;

	[Min(0f)]
	public float ReloadRotationDuration;

	[Min(0f)]
	public float FiredRotationDuration;

	public LeanTweenType ReloadRotationEaseType = LeanTweenType.linear;

	public LeanTweenType FiredRotationEaseType = LeanTweenType.linear;
}

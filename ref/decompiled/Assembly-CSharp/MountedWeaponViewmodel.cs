using UnityEngine;

public class MountedWeaponViewmodel : BaseViewModel
{
	[Header("Mounted Weapon Viewmodel")]
	[SerializeField]
	private bool _invertForward;

	[SerializeField]
	private Vector3 _positionOffset;
}

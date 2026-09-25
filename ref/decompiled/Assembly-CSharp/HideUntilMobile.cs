using UnityEngine;

public class HideUntilMobile : EntityComponent<BaseEntity>
{
	public GameObject[] visuals;

	[Tooltip("If turned on, the script will use the convar \"server.projectile.camera.clipdistance\" to determine the distance at which the object will be visible. If turned off, it will use the value in \"overrideDistance\".")]
	public bool useConvarDistance = true;

	public float overrideDistance = 0.3f;

	private Vector3 startPos;
}

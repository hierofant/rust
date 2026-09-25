using UnityEngine;

public class ServerProjectileTestSpawner : MonoBehaviour
{
	public GameObjectRef projectilePrefab;

	public float velocityScale = 1f;

	public float timeout = 1f;

	[Header("Trajectory Preview")]
	public int trajectorySteps = 50;
}

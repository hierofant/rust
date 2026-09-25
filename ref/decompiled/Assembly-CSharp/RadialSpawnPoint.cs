using UnityEngine;

public class RadialSpawnPoint : BaseSpawnPoint
{
	[Tooltip("Circle to spawn within")]
	[SerializeField]
	[Header("Position Settings")]
	public float radius = 10f;

	[SerializeField]
	private bool randomYAxisOffsetEnabled;

	[SerializeField]
	private float yAxisOffsetMin;

	[SerializeField]
	private float yAxisOffsetMax;

	[Header("Random Rotation Settings")]
	[SerializeField]
	private bool xRotationEnabled;

	[Range(-180f, 180f)]
	[SerializeField]
	private float xRotationMin = -180f;

	[Range(-180f, 180f)]
	[SerializeField]
	private float xRotationMax = 180f;

	[SerializeField]
	private bool yRotationEnabled = true;

	[Range(-180f, 180f)]
	[SerializeField]
	private float yRotationMin = -180f;

	[Range(-180f, 180f)]
	[SerializeField]
	private float yRotationMax = 180f;

	[SerializeField]
	private bool zRotationEnabled;

	[Range(-180f, 180f)]
	[SerializeField]
	private float zRotationMin = -180f;

	[Range(-180f, 180f)]
	[SerializeField]
	private float zRotationMax = 180f;

	public Quaternion GetRandomRotation()
	{
		return Quaternion.Euler(xRotationEnabled ? Random.Range(xRotationMin, xRotationMax) : 0f, yRotationEnabled ? Random.Range(yRotationMin, yRotationMax) : 0f, zRotationEnabled ? Random.Range(zRotationMin, zRotationMax) : 0f);
	}

	public override void GetLocation(out Vector3 pos, out Quaternion rot)
	{
		Vector2 vector = Random.insideUnitCircle * radius;
		pos = base.transform.position + new Vector3(vector.x, 0f, vector.y);
		rot = GetRandomRotation();
		DropToGround(ref pos, ref rot);
		if (randomYAxisOffsetEnabled)
		{
			pos.y += Random.Range(yAxisOffsetMin, yAxisOffsetMax);
		}
	}

	public override bool HasPlayersIntersecting()
	{
		return BaseNetworkable.HasCloseConnections(base.transform.position, radius + playerCheckMargin);
	}

	public override void ObjectSpawned(SpawnPointInstance instance)
	{
	}

	public override void ObjectRetired(SpawnPointInstance instance)
	{
	}
}

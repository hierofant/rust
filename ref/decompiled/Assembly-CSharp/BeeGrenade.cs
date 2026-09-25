using UnityEngine;

public class BeeGrenade : TimedExplosive
{
	public GameObjectRef beeSwarmPrefab;

	[Header("Spawning Settings")]
	public int beeSwarmAmount = 1;

	public float spawnRadius = 2f;

	private const int mask = -928830719;

	public override void Explode()
	{
		if (beeSwarmPrefab.isValid && !WaterLevel.Test(base.transform.position, waves: true, volumes: true, this))
		{
			for (int i = 0; i < Mathf.Max(1, beeSwarmAmount); i++)
			{
				Vector3 vector = base.transform.position;
				if (beeSwarmAmount > 1)
				{
					Vector2 vector2 = Random.insideUnitCircle * spawnRadius;
					Vector3 vector3 = base.transform.position + new Vector3(vector2.x, 0f, vector2.y);
					if (Physics.Linecast(base.transform.position, vector3, out var hitInfo, -928830719))
					{
						Vector3 point = hitInfo.point;
						Vector3 normalized = (base.transform.position - point).normalized;
						vector = point + normalized * 1.5f;
					}
					else
					{
						Vector3 normalized2 = (base.transform.position - base.transform.position).normalized;
						vector = vector3;
						vector += normalized2 * 0.5f;
					}
				}
				if (Physics.Raycast(new Ray(vector + Vector3.up * 0.5f, Vector3.down), out var hitInfo2, 2f, -928830719))
				{
					vector.y = hitInfo2.point.y;
				}
				vector += Vector3.up * 1.5f;
				if (Physics.Linecast(base.transform.position, vector, out var hitInfo3, -928830719))
				{
					vector = hitInfo3.point;
				}
				if (creatorPlayer != null)
				{
					Vector3 normalized3 = (creatorPlayer.transform.position - base.transform.position).normalized;
					vector += normalized3;
				}
				BaseEntity baseEntity = GameManager.server.CreateEntity(beeSwarmPrefab.resourcePath, vector, Quaternion.identity);
				if (creatorPlayer != null)
				{
					baseEntity.OwnerID = creatorPlayer.userID;
					baseEntity.creatorEntity = creatorPlayer;
				}
				baseEntity.Spawn();
			}
		}
		base.Explode();
	}

	public void DelayedDestroy()
	{
		Kill(DestroyMode.Gib);
	}
}

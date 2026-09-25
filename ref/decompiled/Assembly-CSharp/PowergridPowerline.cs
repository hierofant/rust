using UnityEngine;

public class PowergridPowerline : BaseEntity
{
	[Header("Access Point Spawn")]
	public uint accessPointSpawnSeed = 275u;

	public float accessPointSpawnProbability = 0.65f;

	public BaseEntityRef accessPointPrefab;

	public Transform spawnAccessPointHere;

	public GameObject[] disableIfNoAccessPoint;

	public override void ServerInit()
	{
		base.ServerInit();
		if (ShouldSpawnAccessPoint())
		{
			if (!World.LoadedFromSave)
			{
				spawnAccessPointHere.GetPositionAndRotation(out var position, out var rotation);
				BaseEntity baseEntity = GameManager.server.CreateEntity(accessPointPrefab.resourcePath, position, rotation);
				if (baseEntity == null)
				{
					Debug.LogError("Failed to spawn entity from " + accessPointPrefab.resourcePath);
				}
				else
				{
					baseEntity.Spawn();
				}
			}
			OnAccessPointSpawn();
		}
		else
		{
			OnNoAccessPointSpawn();
		}
	}

	public bool ShouldSpawnAccessPoint()
	{
		uint seed = base.transform.position.Seed(World.Seed + accessPointSpawnSeed);
		return SeedRandom.Value(ref seed) > accessPointSpawnProbability;
	}

	private void OnAccessPointSpawn()
	{
		GameObject[] array = disableIfNoAccessPoint;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
	}

	private void OnNoAccessPointSpawn()
	{
		GameObject[] array = disableIfNoAccessPoint;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}
}

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public struct GenerateSpawnPoints : IJob
{
	public NativeList<PlaceEntitiesOffshore.SpawnPoint> spawnPoints;

	public int targetCount;

	public uint seed;

	public PlaceEntitiesOffshore.TerrainSpawnBounds terrainSpawnBounds;

	public float minDistanceFromOtherEntities;

	public void Execute()
	{
		int num = 0;
		while (spawnPoints.Length < targetCount && num < 10000)
		{
			num++;
			GetSpawnPoint(ref seed, in terrainSpawnBounds, in spawnPoints, in minDistanceFromOtherEntities, out var valid, out var position, out var rotation);
			if (valid)
			{
				ref NativeList<PlaceEntitiesOffshore.SpawnPoint> reference = ref spawnPoints;
				PlaceEntitiesOffshore.SpawnPoint value = new PlaceEntitiesOffshore.SpawnPoint
				{
					position = position,
					rotation = rotation
				};
				reference.Add(in value);
			}
		}
	}

	private static void GetSpawnPoint(ref uint seed, in PlaceEntitiesOffshore.TerrainSpawnBounds terrainSpawnBounds, in NativeList<PlaceEntitiesOffshore.SpawnPoint> existingSpawnPoints, in float minDistanceFromOtherEntities, out bool valid, out float3 position, out quaternion rotation)
	{
		float x = 0f;
		float z = 0f;
		switch (seed % 4)
		{
		case 0u:
			x = SeedRandom.Range(ref seed, terrainSpawnBounds.leftOuterX, terrainSpawnBounds.leftInnerX);
			z = SeedRandom.Range(ref seed, terrainSpawnBounds.bottomOuterZ, terrainSpawnBounds.topOuterZ);
			break;
		case 1u:
			x = SeedRandom.Range(ref seed, terrainSpawnBounds.rightInnerX, terrainSpawnBounds.rightOuterX);
			z = SeedRandom.Range(ref seed, terrainSpawnBounds.bottomOuterZ, terrainSpawnBounds.topOuterZ);
			break;
		case 2u:
			x = SeedRandom.Range(ref seed, terrainSpawnBounds.leftOuterX, terrainSpawnBounds.rightOuterX);
			z = SeedRandom.Range(ref seed, terrainSpawnBounds.bottomOuterZ, terrainSpawnBounds.bottomInnerZ);
			break;
		case 3u:
			x = SeedRandom.Range(ref seed, terrainSpawnBounds.leftOuterX, terrainSpawnBounds.rightOuterX);
			z = SeedRandom.Range(ref seed, terrainSpawnBounds.topInnerZ, terrainSpawnBounds.topOuterZ);
			break;
		}
		float x2 = SeedRandom.Range(ref seed, 0f, 360f);
		position = new float3(x, 0f, z);
		rotation = quaternion.Euler(0f, math.radians(x2), 0f);
		valid = true;
		foreach (PlaceEntitiesOffshore.SpawnPoint existingSpawnPoint in existingSpawnPoints)
		{
			if (math.distance(existingSpawnPoint.position, position) < minDistanceFromOtherEntities)
			{
				valid = false;
				break;
			}
		}
	}
}

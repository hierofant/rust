using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlaceEntitiesOffshore : ProceduralComponent
{
	public struct SpawnPoint
	{
		public float3 position;

		public quaternion rotation;
	}

	public struct TerrainSpawnBounds
	{
		public float leftOuterX;

		public float leftInnerX;

		public float rightInnerX;

		public float rightOuterX;

		public float bottomOuterZ;

		public float bottomInnerZ;

		public float topInnerZ;

		public float topOuterZ;
	}

	[SerializeField]
	private GameObjectRef prefab;

	[SerializeField]
	private float minWorldSize;

	[SerializeField]
	private int targetCount;

	[SerializeField]
	private float minDistanceFromTerrain = 100f;

	[SerializeField]
	private float maxDistanceFromTerrain = 500f;

	[SerializeField]
	private float minDistanceFromOtherEntities = 100f;

	public const int Attempts = 10000;

	public override void Process(uint seed)
	{
		if ((float)World.Size < minWorldSize)
		{
			return;
		}
		float3 terrainPosition = TerrainMeta.Position;
		float3 terrainSize = TerrainMeta.Size;
		GetTerrainSpawnBounds(in terrainPosition, in terrainSize, in minDistanceFromTerrain, in maxDistanceFromTerrain, out var bounds);
		using NativeList<SpawnPoint> spawnPoints = new NativeList<SpawnPoint>(targetCount, Allocator.TempJob);
		GenerateSpawnPoints generateSpawnPoints = default(GenerateSpawnPoints);
		generateSpawnPoints.spawnPoints = spawnPoints;
		generateSpawnPoints.targetCount = targetCount;
		generateSpawnPoints.seed = seed;
		generateSpawnPoints.terrainSpawnBounds = bounds;
		generateSpawnPoints.minDistanceFromOtherEntities = minDistanceFromOtherEntities;
		GenerateSpawnPoints jobData = generateSpawnPoints;
		jobData.Schedule().Complete();
		PlacePrefabs(in jobData.spawnPoints);
	}

	public static void GetTerrainSpawnBounds(in float3 terrainPosition, in float3 terrainSize, in float maxDistance, in float minDistance, out TerrainSpawnBounds bounds)
	{
		bounds.leftOuterX = terrainPosition.x - maxDistance;
		bounds.leftInnerX = terrainPosition.x - minDistance;
		bounds.rightInnerX = terrainPosition.x + terrainSize.x + minDistance;
		bounds.rightOuterX = terrainPosition.x + terrainSize.x + maxDistance;
		bounds.bottomOuterZ = terrainPosition.z - maxDistance;
		bounds.bottomInnerZ = terrainPosition.z - minDistance;
		bounds.topInnerZ = terrainPosition.z + terrainSize.z + minDistance;
		bounds.topOuterZ = terrainPosition.z + terrainSize.z + maxDistance;
	}

	private void PlacePrefabs(in NativeList<SpawnPoint> spawnPoints)
	{
		Span<Vector3> span = stackalloc Vector3[spawnPoints.Length];
		Span<Quaternion> span2 = stackalloc Quaternion[spawnPoints.Length];
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			span[i] = spawnPoints[i].position;
			span2[i] = spawnPoints[i].rotation;
		}
		UnityEngine.Object.InstantiateAsync(new GameObject("TestEntityOffshore"), spawnPoints.Length, span, span2);
	}
}

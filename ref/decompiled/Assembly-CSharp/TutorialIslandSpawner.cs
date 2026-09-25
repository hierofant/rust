using System.Collections.Generic;
using UnityEngine;

public static class TutorialIslandSpawner
{
	public static void GetEdgeSpawnPoints(List<Vector3> positions, Vector3 start, Vector3 bounds, Vector3 cellSize, int loopCount, out Bounds worldBoundsMinusTutorialIslands)
	{
		positions.Clear();
		for (int i = 0; i < loopCount; i++)
		{
			GetEdgeSpawnPoints(positions, start, bounds, cellSize, i);
		}
		worldBoundsMinusTutorialIslands = new Bounds(start + bounds / 2f, bounds - cellSize * 2f * loopCount);
		worldBoundsMinusTutorialIslands.size = new Vector3(worldBoundsMinusTutorialIslands.size.x, 1000f, worldBoundsMinusTutorialIslands.size.z);
	}

	private static void GetEdgeSpawnPoints(List<Vector3> points, Vector3 start, Vector3 bounds, Vector3 cellSize, int curLoop)
	{
		bounds -= cellSize * 2f * curLoop;
		start += cellSize * curLoop;
		Vector3 vector = start + bounds - cellSize / 2f;
		_ = bounds.x / cellSize.x;
		int num = (int)(bounds.z / cellSize.z) - 1;
		Vector3 vector2 = start + cellSize / 2f;
		vector2 = start + cellSize / 2f + new Vector3(0f, 0f, cellSize.z);
		for (int i = 1; i < num - 1; i++)
		{
			points.Add(vector2);
			points.Add(new Vector3(vector.x, 0f, vector2.z));
			vector2 += new Vector3(0f, 0f, cellSize.z);
		}
	}
}

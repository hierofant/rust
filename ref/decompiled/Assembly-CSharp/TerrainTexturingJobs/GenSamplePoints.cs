using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainTexturingJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct GenSamplePoints : IJob
{
	public int shoreMapSize;

	public float upscaleCoords;

	public Vector3 terrainPosition;

	[WriteOnly]
	public NativeArray<Vector3> positions;

	[WriteOnly]
	public NativeArray<int> indices;

	public void Execute()
	{
		int i = 0;
		int num = 0;
		for (; i < shoreMapSize; i++)
		{
			float z = ((float)i + 0.5f) * upscaleCoords;
			int num2 = 0;
			while (num2 < shoreMapSize)
			{
				float x = ((float)num2 + 0.5f) * upscaleCoords;
				Vector3 value = new Vector3(terrainPosition.x, 0f, terrainPosition.z) + new Vector3(x, 0f, z);
				positions[num] = value;
				indices[num] = num;
				num2++;
				num++;
			}
		}
	}
}

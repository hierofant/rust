using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainHeightMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
public struct GetNormalsJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<Vector3> Normals;

	public NativeArray<Vector3>.ReadOnly Pos;

	public NativeArray<int>.ReadOnly Indices;

	public HeightMapData HeightMapData;

	public void Execute()
	{
		Vector3 min = HeightMapData.DeepSeaBounds.min;
		Vector2 vector = new Vector2(1f / HeightMapData.DeepSeaBounds.size.x, 1f / HeightMapData.DeepSeaBounds.size.z);
		foreach (int index in Indices)
		{
			bool num = HeightMapData.DeepSeaBounds.Contains(Pos[index]);
			Vector3 vector2 = (num ? min : HeightMapData.TerrainPos);
			Vector2 vector3 = (num ? vector : HeightMapData.TerrainOneOverSize);
			float x = (Pos[index].x - vector2.x) * vector3.x;
			float y = (Pos[index].z - vector2.z) * vector3.y;
			NativeArray<short>.ReadOnly data = (num ? HeightMapData.DeepSeaData : HeightMapData.Data);
			Normals[index] = HeightMapData.GetNormal(new Vector2(x, y), HeightMapData.NormY, data, HeightMapData.Res);
		}
	}
}

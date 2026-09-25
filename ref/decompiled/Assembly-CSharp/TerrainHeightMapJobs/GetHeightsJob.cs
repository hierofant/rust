using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainHeightMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
public struct GetHeightsJob : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<float> Heights;

	public NativeArray<Vector3>.ReadOnly Pos;

	public HeightMapData HeightMapData;

	public void Execute(int index)
	{
		Vector3 min = HeightMapData.DeepSeaBounds.min;
		Vector2 vector = new Vector2(1f / HeightMapData.DeepSeaBounds.size.x, 1f / HeightMapData.DeepSeaBounds.size.z);
		bool flag = HeightMapData.DeepSeaBounds.Contains(Pos[index]);
		Vector3 vector2 = (flag ? min : HeightMapData.TerrainPos);
		Vector2 vector3 = (flag ? vector : HeightMapData.TerrainOneOverSize);
		float x = (Pos[index].x - vector2.x) * vector3.x;
		float y = (Pos[index].z - vector2.z) * vector3.y;
		float num = HeightMapData.GetHeight01(data: flag ? HeightMapData.DeepSeaData : HeightMapData.Data, uv: new Vector2(x, y), res: HeightMapData.Res);
		Heights[index] = HeightMapData.TerrainPos.y + num * HeightMapData.TerrainScale;
	}
}

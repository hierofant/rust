using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainHeightMapJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
public struct GetHeightsByUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<UnityEngine.Vector2>.ReadOnly UVs;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public HeightMapData HeightMapData;

	public NativeArray<short>.ReadOnly Data;

	public void Execute()
	{
		int res = HeightMapData.Res;
		float y = HeightMapData.TerrainPos.y;
		float terrainScale = HeightMapData.TerrainScale;
		foreach (int index in Indices)
		{
			float height = HeightMapData.GetHeight01(UVs[index], Data, res);
			Heights[index] = y + height * terrainScale;
		}
	}
}

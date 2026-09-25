using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TerrainWaterMapJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GetHeightByIndexJob : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector2i> Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<short> Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	[Unity.Collections.ReadOnly]
	public float Offset;

	[Unity.Collections.ReadOnly]
	public float Scale;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i].y * Res + Indices[i].x;
			Heights[i] = Offset + BitUtility.Short2Float(Data[index]) * Scale;
		}
	}
}

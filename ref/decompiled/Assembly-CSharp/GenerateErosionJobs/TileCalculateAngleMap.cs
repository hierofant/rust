using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct TileCalculateAngleMap : IJobParallelFor
{
	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<float> AngleMap;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly TerrainHeightMapSrcFloat;

	public float NormY;

	public int Res;

	public int NumXTiles;

	public int TileSizeX;

	public int TileSizeZ;

	public void Execute(int index)
	{
		int num = index % NumXTiles;
		int num2 = index / NumXTiles;
		int num3 = math.max(num * TileSizeX, 1);
		int num4 = math.max(num2 * TileSizeZ, 1);
		int num5 = math.min(num3 + TileSizeX, Res - 1);
		int num6 = math.min(num4 + TileSizeZ, Res - 1);
		int4 @int = new int4(Res);
		int2 int2 = new int2(1, -1);
		for (int i = num4; i < num6; i++)
		{
			int4 int3 = (new int2(i) + int2).yyxy * @int;
			for (int j = num3; j < num5; j++)
			{
				float4 @float = int3 + new int4(j + 1, j - 1, j - 1, j - 1);
				float3 float2 = math.normalize(new float3((TerrainHeightMapSrcFloat[(int)@float.x] - TerrainHeightMapSrcFloat[(int)@float.y]) * -0.5f, z: (TerrainHeightMapSrcFloat[(int)@float.z] - TerrainHeightMapSrcFloat[(int)@float.w]) * -0.5f, y: NormY));
				float num7 = math.dot(float2, float2);
				AngleMap[i * Res + j] = math.acos(math.clamp(math.dot(math.up(), math.normalize(float2)) / num7, -1f, 1f));
			}
		}
	}
}

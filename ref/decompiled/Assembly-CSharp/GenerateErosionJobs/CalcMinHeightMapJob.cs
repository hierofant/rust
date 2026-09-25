using TerrainTopologyMapJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct CalcMinHeightMapJob : IJobParallelFor
{
	public NativeArray<float>.ReadOnly TerrainHeightMap;

	public int HeightMapRes;

	[WriteOnly]
	public NativeArray<float> MinTerrainHeightMap;

	public NativeArray<int>.ReadOnly TopologyMap;

	public int TopologyMapRes;

	public float OceanHeight;

	public float TerrainOneOverSizeX;

	public void Execute(int index)
	{
		float num = TerrainHeightMap[index];
		if (!(num < OceanHeight))
		{
			int num2 = index % HeightMapRes;
			int num3 = index / HeightMapRes;
			float normX = ((float)num2 - 0.5f) / (float)HeightMapRes;
			float normZ = ((float)num3 - 0.5f) / (float)HeightMapRes;
			bool flag = (TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, 0f, normX, normZ) & 0x14080) != 0;
			float x = 8f;
			float num4 = 8f;
			while (num4 > 0f && !flag && ((uint)TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, num4, normX, normZ) & 0x3C198u) != 0)
			{
				x = num4;
				num4 -= 0.25f;
			}
			float num5 = (flag ? 0f : math.unlerp(0f, 8f, x));
			num = math.max(OceanHeight, num - 1f * num5);
		}
		MinTerrainHeightMap[index] = num;
	}
}

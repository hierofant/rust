using TerrainTopologyMapJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GenerateErosionJobs;

[BurstCompile(FloatMode = FloatMode.Deterministic)]
internal struct PaintSplatJob : IJobFor
{
	[NativeDisableParallelForRestriction]
	public NativeArray<float>.ReadOnly HeightMapDelta;

	public int HeightMapRes;

	[NativeDisableParallelForRestriction]
	public NativeArray<float>.ReadOnly AngleMapDeg;

	[NativeDisableParallelForRestriction]
	public NativeArray<int>.ReadOnly TopologyMap;

	public int TopologyMapRes;

	[NativeDisableParallelForRestriction]
	public NativeArray<byte> SplatMap;

	public int SplatMapRes;

	public int SplatNum;

	[NativeDisableParallelForRestriction]
	public NativeHashMap<int, int>.ReadOnly SplatType2Index;

	public float TerrainOneOverSizeX;

	public void Execute(int index)
	{
		int num = index % HeightMapRes;
		int num2 = index / HeightMapRes;
		float num3 = ((float)num - 0.5f) / (float)HeightMapRes;
		float num4 = ((float)num2 - 0.5f) / (float)HeightMapRes;
		if (((uint)TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, 0f, num3, num4) & 0xB4990u) != 0 || ((uint)TerrainTopologyMapJobUtil.GetTopologyRadius(TopologyMap, TopologyMapRes, TerrainOneOverSizeX, 8f, num3, num4) & 2u) != 0 || AngleMapDeg[num2 * HeightMapRes + num] < 3f)
		{
			return;
		}
		float grad;
		float num5 = ConcavityFactor(HeightMapDelta, HeightMapRes, num, num2, out grad);
		if (!(num5 < 3.5762787E-07f))
		{
			int x = Index(num3, SplatMapRes);
			int z = Index(num4, SplatMapRes);
			float splat = GetSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, num3, num4, 2);
			float splat2 = GetSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, num3, num4, 4);
			if (splat > 0.25f || splat2 > 0.25f)
			{
				num5 = math.saturate(num5 * 3f);
				grad = math.pow(grad, 2f);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 64, math.pow(num5, 0.8f) * grad);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 128, math.pow(num5, 1.5f) * grad);
			}
			else
			{
				num5 = math.saturate(num5 * 3f);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 1, math.pow(num5, 4f) * math.pow(grad, 1.5f));
				grad = math.pow(grad, 2f);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 64, math.pow(num5, 0.8f) * grad);
				AddSplat(SplatMap, SplatMapRes, SplatNum, SplatType2Index, x, z, 128, math.pow(num5, 1.4f) * grad);
			}
		}
		static int Index(float normalized, int res)
		{
			int num6 = (int)(normalized * (float)res);
			if (num6 >= 0)
			{
				if (num6 <= res - 1)
				{
					return num6;
				}
				return res - 1;
			}
			return 0;
		}
	}

	private static void AddSplat(NativeArray<byte> src, int res, int splatNum, NativeHashMap<int, int>.ReadOnly type2Index, int x, int z, int id, float d)
	{
		float splat = GetSplat(src, res, splatNum, type2Index, x, z, id);
		float num = math.saturate(splat + d);
		int num2 = type2Index[id];
		if (splat >= 1f)
		{
			return;
		}
		float num3 = (1f - num) / (1f - splat);
		for (int i = 0; i < splatNum; i++)
		{
			if (i == num2)
			{
				src[(i * res + z) * res + x] = BitUtility.Float2Byte(num);
			}
			else
			{
				src[(i * res + z) * res + x] = BitUtility.Float2Byte(num3 * BitUtility.Byte2Float(src[(i * res + z) * res + x]));
			}
		}
	}

	private static float GetSplat(NativeArray<byte> src, int res, int splatNum, NativeHashMap<int, int>.ReadOnly type2Index, float xn, float zn, int mask)
	{
		int num = res - 1;
		float num2 = xn * (float)num;
		float num3 = zn * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		float a = Mathf.Lerp(GetSplat(src, res, splatNum, type2Index, num4, num5, mask), GetSplat(src, res, splatNum, type2Index, x, num5, mask), num2 - (float)num4);
		float b = Mathf.Lerp(GetSplat(src, res, splatNum, type2Index, num4, z, mask), GetSplat(src, res, splatNum, type2Index, x, z, mask), num2 - (float)num4);
		return Mathf.Lerp(a, b, num3 - (float)num5);
	}

	private static float GetSplat(NativeArray<byte> src, int res, int splatNum, NativeHashMap<int, int>.ReadOnly type2Index, int x, int z, int mask)
	{
		if (Mathf.IsPowerOfTwo(mask))
		{
			return BitUtility.Byte2Float(src[(type2Index[mask] * res + z) * res + x]);
		}
		int num = 0;
		for (int i = 0; i < splatNum; i++)
		{
			if ((TerrainSplat.IndexToType(i) & mask) != 0)
			{
				num += src[(i * res + z) * res + x];
			}
		}
		return Mathf.Clamp01(BitUtility.Byte2Float(num));
	}

	private static float ConcavityFactor(NativeArray<float>.ReadOnly data, int res, int x, int z, out float grad)
	{
		int num = x - 1;
		int num2 = x + 1;
		int num3 = z - 1;
		int num4 = z + 1;
		float num5 = data[z * res + x];
		float num6 = data[z * res + num];
		float num7 = data[z * res + num2];
		float num8 = data[num4 * res + x];
		float num9 = data[num3 * res + x];
		float num10 = num6 + num7 + num8 + num9;
		float num11 = data[num3 * res + num] + data[num3 * res + num2] + data[num4 * res + num] + data[num4 * res + num2];
		float num12 = num7 - num6;
		float num13 = num8 - num9;
		grad = math.sqrt(num12 * num12 + num13 * num13);
		return math.max(num10 / 4f + num11 / 4f - num5, 0f);
	}
}

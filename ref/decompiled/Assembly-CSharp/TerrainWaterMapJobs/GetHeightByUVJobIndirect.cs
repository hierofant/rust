using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainWaterMapJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GetHeightByUVJobIndirect : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector2>.ReadOnly UV;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public NativeArray<short>.ReadOnly Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	[Unity.Collections.ReadOnly]
	public float Offset;

	[Unity.Collections.ReadOnly]
	public float Scale;

	public void Execute()
	{
		int num = Res - 1;
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			float num2 = UV[index].x * (float)num;
			float num3 = UV[index].y * (float)num;
			int num4 = Mathf.Clamp((int)num2, 0, num);
			int num5 = Mathf.Clamp((int)num3, 0, num);
			int num6 = Mathf.Min(num4 + 1, num);
			int num7 = Mathf.Min(num5 + 1, num);
			float a = BitUtility.Short2Float(Data[num5 * Res + num4]);
			float b = BitUtility.Short2Float(Data[num5 * Res + num6]);
			float a2 = BitUtility.Short2Float(Data[num7 * Res + num4]);
			float b2 = BitUtility.Short2Float(Data[num7 * Res + num6]);
			float a3 = Mathf.Lerp(a, b, num2 - (float)num4);
			float b3 = Mathf.Lerp(a2, b2, num2 - (float)num4);
			Heights[index] = Offset + Mathf.Lerp(a3, b3, num3 - (float)num5) * Scale;
		}
	}
}

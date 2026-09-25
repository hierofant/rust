using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TerrainWaterMapJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GetHeightByPosJob : IJob
{
	[WriteOnly]
	public NativeArray<float> Heights;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3> Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<short> Data;

	[Unity.Collections.ReadOnly]
	public int Res;

	[Unity.Collections.ReadOnly]
	public float Offset;

	[Unity.Collections.ReadOnly]
	public float Scale;

	[Unity.Collections.ReadOnly]
	public Vector2 DataOrigin;

	[Unity.Collections.ReadOnly]
	public Vector2 DataScale;

	public void Execute()
	{
		int num = Res - 1;
		for (int i = 0; i < Pos.Length; i++)
		{
			float num2 = (Pos[i].x - DataOrigin.x) * DataScale.x;
			float num3 = (Pos[i].z - DataOrigin.y) * DataScale.y;
			float num4 = num2 * (float)num;
			float num5 = num3 * (float)num;
			int num6 = Mathf.Clamp((int)num4, 0, num);
			int num7 = Mathf.Clamp((int)num5, 0, num);
			int num8 = Mathf.Min(num6 + 1, num);
			int num9 = Mathf.Min(num7 + 1, num);
			float a = BitUtility.Short2Float(Data[num7 * Res + num6]);
			float b = BitUtility.Short2Float(Data[num7 * Res + num8]);
			float a2 = BitUtility.Short2Float(Data[num9 * Res + num6]);
			float b2 = BitUtility.Short2Float(Data[num9 * Res + num8]);
			float a3 = Mathf.Lerp(a, b, num4 - (float)num6);
			float b3 = Mathf.Lerp(a2, b2, num4 - (float)num6);
			Heights[i] = Offset + Mathf.Lerp(a3, b3, num5 - (float)num7) * Scale;
		}
	}
}

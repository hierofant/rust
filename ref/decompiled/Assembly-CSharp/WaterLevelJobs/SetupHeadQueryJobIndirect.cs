using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace WaterLevelJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct SetupHeadQueryJobIndirect : IJob
{
	public NativeArray<int> Indices;

	[WriteOnly]
	public NativeReference<int> QueryIndexCount;

	[WriteOnly]
	public NativeArray<Vector3> QueryStarts;

	[WriteOnly]
	public NativeArray<float> QueryRadii;

	[Unity.Collections.ReadOnly]
	public NativeArray<bool>.ReadOnly ValidInfos;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Starts;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Ends;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radii;

	public void Execute()
	{
		int value = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			if (ValidInfos[num])
			{
				Vector3 vector = Starts[num];
				Vector3 vector2 = Ends[num];
				float num2 = Radii[num];
				float a = Mathf.Min(vector.y, vector2.y) - num2;
				Vector3 value2 = Vector3Ex.WithY(y: Mathf.Lerp(a, Mathf.Max(vector.y, vector2.y) + num2, 0.75f), v: (vector + vector2) * 0.5f);
				Indices[value++] = num;
				QueryStarts[num] = value2;
				QueryRadii[num] = 0.01f;
			}
		}
		QueryIndexCount.Value = value;
	}
}

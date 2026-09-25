using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace CoarseQueryGridJobs;

[BurstCompile]
public struct CheckBoundsJobIndirect : IJob
{
	[WriteOnly]
	public NativeList<int> OverlapIndices;

	[NativeDisableContainerSafetyRestriction]
	[Unity.Collections.ReadOnly]
	public CoarseQueryGrid Grid;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Starts;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Ends;

	[Unity.Collections.ReadOnly]
	public NativeArray<float>.ReadOnly Radii;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			Vector3 lhs = Starts[num];
			Vector3 rhs = Ends[num];
			float num2 = Radii[num];
			Vector3 vector = Vector3.one * num2;
			Vector3 vector2 = Vector3.Min(lhs, rhs) - vector;
			Vector3 vector3 = Vector3.Max(lhs, rhs) + vector;
			Bounds checkBounds = new Bounds((vector3 + vector2) * 0.5f, vector3 - vector2);
			if (Grid.Check(checkBounds))
			{
				OverlapIndices.AddNoResize(num);
			}
		}
	}
}

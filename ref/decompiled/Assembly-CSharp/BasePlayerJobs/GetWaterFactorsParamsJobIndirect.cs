using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BasePlayerJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GetWaterFactorsParamsJobIndirect : IJob
{
	public NativeArray<Vector3> Starts;

	public NativeArray<Vector3> Ends;

	[WriteOnly]
	public NativeArray<float> Radii;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly Pos;

	[Unity.Collections.ReadOnly]
	public NativeArray<Quaternion>.ReadOnly Rots;

	[Unity.Collections.ReadOnly]
	public NativeArray<int>.ReadOnly Indices;

	public void Execute()
	{
		for (int i = 0; i < Indices.Length; i++)
		{
			int index = Indices[i];
			Vector2 vector = Ends[index];
			float x = vector.x;
			float num = vector.y * 0.5f;
			Vector3 vector2 = Starts[index];
			Vector3 vector3 = Pos[index];
			Quaternion quaternion = Rots[index];
			Vector3 value = vector3 + quaternion * (vector2 - Vector3.up * (num - x));
			Vector3 value2 = vector3 + quaternion * (vector2 + Vector3.up * (num - x));
			Starts[index] = value;
			Ends[index] = value2;
			Radii[index] = x;
		}
	}
}

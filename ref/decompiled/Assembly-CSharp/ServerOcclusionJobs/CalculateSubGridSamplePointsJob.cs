using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ServerOcclusionJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct CalculateSubGridSamplePointsJob : IJobFor
{
	[NativeDisableParallelForRestriction]
	[WriteOnly]
	public NativeArray<Vector3> Posi;

	[Unity.Collections.ReadOnly]
	public NativeArray<ServerOcclusion.SubGrid>.ReadOnly SubGridCells;

	[Unity.Collections.ReadOnly]
	public NativeArray<Vector3>.ReadOnly GridOffsets;

	[Unity.Collections.ReadOnly]
	public Vector3 CellOffset;

	public void Execute(int index)
	{
		int length = GridOffsets.Length;
		ServerOcclusion.SubGrid subGrid = SubGridCells[index];
		Vector3 vector = new Vector3((float)subGrid.x - CellOffset.x, (float)subGrid.y - CellOffset.y, (float)subGrid.z - CellOffset.z) * 2f;
		for (int i = 0; i < length; i++)
		{
			Posi[index * length + i] = vector + GridOffsets[i];
		}
	}
}

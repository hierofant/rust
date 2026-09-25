using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GatherFlyingBatchesJob : IJob
{
	[WriteOnly]
	public NativeList<Vector3> From;

	[WriteOnly]
	public NativeList<Vector3> To;

	[WriteOnly]
	public NativeList<Vector3> CheckPoses;

	public NativeArray<AntiHack.FlyingBatch> Batches;

	[Unity.Collections.ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	public NativeArray<Matrix4x4>.ReadOnly Matrices;

	public NativeArray<int>.ReadOnly Indices;

	[Unity.Collections.ReadOnly]
	public int MaxSteps;

	[Unity.Collections.ReadOnly]
	public float DefaultStepSize;

	[Unity.Collections.ReadOnly]
	public int Protection;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < Indices.Length; i++)
		{
			int playerIndex = Indices[i];
			TickInterpolatorCache.PlayerTickIterator playerTickIterator = TickInterpolatorCache.GetPlayerTickIterator(TickCache, playerIndex);
			Matrix4x4 matrix4x = Matrices[i];
			bool flag = matrix4x[15] == 0f;
			Vector3 vector = (flag ? playerTickIterator.StartPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.StartPoint));
			Vector3 vector2 = (flag ? playerTickIterator.EndPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.EndPoint));
			AntiHack.FlyingBatch value = Batches[i];
			value.PlayerIndex = playerIndex;
			playerTickIterator.Reset();
			if (!playerTickIterator.HasNext())
			{
				continue;
			}
			if (Protection >= 3)
			{
				float distance = Mathf.Max(playerTickIterator.Length / (float)MaxSteps, DefaultStepSize);
				int num2 = 0;
				while (playerTickIterator.MoveNext(distance))
				{
					vector2 = (flag ? playerTickIterator.CurrentPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.CurrentPoint));
					From.AddNoResize(vector);
					To.AddNoResize(vector2);
					CheckPoses.AddNoResize((vector + vector2) * 0.5f);
					vector = vector2;
					num2++;
					num++;
				}
				value.Count = num2;
			}
			else
			{
				From.AddNoResize(vector);
				To.AddNoResize(vector2);
				CheckPoses.AddNoResize((vector + vector2) * 0.5f);
				value.Count = 1;
				num++;
			}
			Batches[i] = value;
		}
	}
}

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct GatherNoClipBatchesJob : IJob
{
	[WriteOnly]
	public NativeList<Vector3> From;

	[WriteOnly]
	public NativeList<Vector3> To;

	public NativeArray<AntiHack.Batch> Batches;

	[Unity.Collections.ReadOnly]
	public TickInterpolatorCache.ReadOnlyState TickCache;

	public NativeArray<Matrix4x4>.ReadOnly Matrices;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<float>.ReadOnly DeltaTimes;

	[Unity.Collections.ReadOnly]
	public int MaxSteps;

	[Unity.Collections.ReadOnly]
	public float DefaultStepSize;

	[Unity.Collections.ReadOnly]
	public float LagThreshold;

	[Unity.Collections.ReadOnly]
	public bool TickBufferPrevention;

	[Unity.Collections.ReadOnly]
	public float MaxTickCount;

	[Unity.Collections.ReadOnly]
	public int DefaultProtection;

	public void Execute()
	{
		Vector3 vector = BasePlayer.NoClipOffset();
		for (int i = 0; i < Indices.Length; i++)
		{
			int num = Indices[i];
			TickInterpolatorCache.PlayerTickIterator playerTickIterator = TickInterpolatorCache.GetPlayerTickIterator(TickCache, num);
			Matrix4x4 matrix4x = Matrices[i];
			bool flag = matrix4x[15] == 0f;
			Vector3 vector2 = (flag ? playerTickIterator.StartPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.StartPoint));
			Vector3 vector3 = (flag ? playerTickIterator.EndPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.EndPoint));
			AntiHack.Batch value = Batches[i];
			bool num2 = DeltaTimes[num] < LagThreshold && TickBufferPrevention;
			int count = value.Count;
			int num3 = DefaultProtection;
			if (num2 && (float)count >= MaxTickCount)
			{
				num3 = Mathf.Min(2, num3);
			}
			if (num3 >= 3)
			{
				float distance = Mathf.Max(playerTickIterator.Length / (float)MaxSteps, DefaultStepSize);
				int num4 = 0;
				while (playerTickIterator.MoveNext(distance))
				{
					num4++;
					vector3 = (flag ? playerTickIterator.CurrentPoint : matrix4x.MultiplyPoint3x4(playerTickIterator.CurrentPoint));
					From.AddNoResize(vector2 + vector);
					To.AddNoResize(vector3 + vector);
					vector2 = vector3;
				}
				value.Count = num4;
			}
			else
			{
				From.AddNoResize(vector2 + vector);
				To.AddNoResize(vector3 + vector);
				value.Count = 1;
			}
			Batches[i] = value;
		}
	}
}

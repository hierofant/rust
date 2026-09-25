using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile]
internal struct AppendRaycastHitsJob : IJob
{
	public NativeArray<RaycastHit> Dst;

	public NativeArray<RaycastHit>.ReadOnly Src;

	public int DstMaxHitsPerBatch;

	public int SrcMaxHitsPerBatch;

	public void Execute()
	{
		if (Src.Length == 0 || Dst.Length == 0)
		{
			return;
		}
		int num = Src.Length / SrcMaxHitsPerBatch;
		for (int i = 0; i < num; i++)
		{
			int endInd;
			int num2 = GamePhysicsJobs.Util.FindFreeSlot(i, in Dst, DstMaxHitsPerBatch, out endInd);
			int num3 = i * SrcMaxHitsPerBatch;
			int num4 = num3 + SrcMaxHitsPerBatch;
			while (num2 < endInd && num3 < num4)
			{
				RaycastHit value = Src[num3++];
				if (value.normal == Vector3.zero)
				{
					break;
				}
				Dst[num2++] = value;
			}
			if (num2 < endInd)
			{
				Dst[num2] = default(RaycastHit);
			}
		}
	}
}

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace GamePhysicsJobs;

[BurstCompile]
public struct RemoveLayerMaskJob : IJob
{
	public NativeArray<int> LayerMasks;

	[Unity.Collections.ReadOnly]
	public NativeArray<bool>.ReadOnly ShouldIgnore;

	[Unity.Collections.ReadOnly]
	public int MaskToRemove;

	public void Execute()
	{
		for (int i = 0; i < ShouldIgnore.Length; i++)
		{
			if (ShouldIgnore[i])
			{
				LayerMasks[i] &= ~MaskToRemove;
			}
		}
	}
}

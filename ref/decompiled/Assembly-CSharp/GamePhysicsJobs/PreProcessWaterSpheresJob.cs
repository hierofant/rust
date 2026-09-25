using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GamePhysicsJobs;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct PreProcessWaterSpheresJob : IJob
{
	public NativeArray<RaycastHit>.ReadOnly hits;

	public NativeArray<SpherecastCommand>.ReadOnly rays;

	public int maxHitsPerTrace;

	public NativeList<Vector2i> WaterIndices;

	public NativeList<Ray> WaterRays;

	public NativeArray<float> WaterMaxDists;

	public NativeList<int> DeepIndices;

	public NativeList<int> MainIndices;

	public Bounds DeepSeaBounds;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < rays.Length; i++)
		{
			SpherecastCommand spherecastCommand = rays[i];
			if ((spherecastCommand.queryParameters.layerMask & 0x10) == 0)
			{
				continue;
			}
			int endInd;
			int num2 = GamePhysicsJobs.Util.FindFreeSlot(i, in hits, maxHitsPerTrace, out endInd);
			if (num2 != endInd)
			{
				int value = num++;
				Ray value2 = new Ray(spherecastCommand.origin, spherecastCommand.direction);
				WaterRays.Add(in value2);
				if (DeepSeaBounds.Contains(value2.origin))
				{
					DeepIndices.Add(in value);
				}
				else
				{
					MainIndices.Add(in value);
				}
				WaterMaxDists[value] = spherecastCommand.distance;
				ref NativeList<Vector2i> waterIndices = ref WaterIndices;
				Vector2i value3 = new Vector2i(num2, endInd);
				waterIndices.Add(in value3);
			}
		}
	}
}

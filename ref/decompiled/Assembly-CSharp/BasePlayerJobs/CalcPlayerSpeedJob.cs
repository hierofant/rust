using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace BasePlayerJobs;

[BurstCompile]
public struct CalcPlayerSpeedJob : IJobFor
{
	[WriteOnly]
	public NativeArray<float> Speed;

	public NativeArray<BasePlayer.CachedState>.ReadOnly States;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<RDC>.ReadOnly RDCs;

	public void Execute(int jobInd)
	{
		int index = Indices[jobInd];
		BasePlayer.CachedState cachedState = States[index];
		RDC rDC = RDCs[index];
		float num = 1f;
		num -= cachedState.ClothingMoveSpeedReduction;
		if (cachedState.IsSwimming)
		{
			num += cachedState.ClothingWaterSpeedBonus;
		}
		float num2;
		if (rDC.Crawling > 0f)
		{
			num2 = Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * num * cachedState.ModifiersMovementMultiplier;
		}
		else
		{
			num2 = Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, rDC.Running), 1.7f, rDC.Ducking) * num * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
			if (!cachedState.IsSwimming)
			{
				num2 = Mathf.Lerp(num2, 0f, Mathf.Max(cachedState.MovementModify.drag, cachedState.ClothingMoveSpeedReduction));
			}
		}
		Speed[jobInd] = num2;
	}
}

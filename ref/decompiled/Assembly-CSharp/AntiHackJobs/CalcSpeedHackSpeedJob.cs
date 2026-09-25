using BasePlayerJobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AntiHackJobs;

[BurstCompile]
public struct CalcSpeedHackSpeedJob : IJobFor
{
	[WriteOnly]
	public NativeArray<float> Speed;

	public NativeArray<BasePlayer.CachedState>.ReadOnly States;

	public NativeArray<int>.ReadOnly Indices;

	public NativeArray<RDC>.ReadOnly RDCs;

	[Unity.Collections.ReadOnly]
	public float WaterThreshold;

	public void Execute(int jobInd)
	{
		int index = Indices[jobInd];
		BasePlayer.CachedState cachedState = States[index];
		RDC rDC = RDCs[index];
		float num = 1f - cachedState.ClothingMoveSpeedReduction;
		float num2 = num + cachedState.ClothingWaterSpeedBonus;
		if (cachedState.IsSwimming)
		{
			float value = ((!(rDC.Crawling > 0f)) ? (Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, rDC.Running), 1.7f, rDC.Ducking) * num2 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier) : (Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * cachedState.ModifiersMovementMultiplier * num2));
			Speed[jobInd] = value;
			return;
		}
		if (cachedState.WaterFactor < WaterThreshold)
		{
			float num3 = num;
			float value2;
			if (rDC.Crawling > 0f)
			{
				value2 = Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * num3 * cachedState.ModifiersMovementMultiplier;
			}
			else
			{
				value2 = Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, rDC.Running), 1.7f, rDC.Ducking) * num3 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
				value2 = Mathf.Lerp(value2, 0f, Mathf.Max(cachedState.MovementModify.drag, cachedState.ClothingMoveSpeedReduction));
			}
			Speed[jobInd] = value2;
			return;
		}
		float num4 = num;
		float a;
		float b;
		if (rDC.Crawling > 0f)
		{
			float num5 = Mathf.Lerp(2.8f, 0.72f, rDC.Crawling) * cachedState.ModifiersMovementMultiplier;
			a = num5 * num2;
			b = num5 * num4;
		}
		else
		{
			float a2 = Mathf.Lerp(2.8f, 5.5f, rDC.Running);
			float num6 = Mathf.Lerp(a2, 1.7f, rDC.Ducking) * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
			a = Mathf.Lerp(a2, 1.7f, 1f) * num2 * cachedState.WeaponMoveSpeedScale * cachedState.ModifiersMovementMultiplier;
			b = Mathf.Lerp(num6 * num4, 0f, Mathf.Max(cachedState.MovementModify.drag, cachedState.ClothingMoveSpeedReduction));
		}
		Speed[jobInd] = Mathf.Max(a, b);
	}
}

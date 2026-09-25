using UnityEngine;

public class WaterTreatmentWaterCatcher : WaterCatcher
{
	[ServerVar(Saved = true)]
	public static float evaporationPerMinute = 200f;

	public override void ServerInit()
	{
		base.ServerInit();
		WaterTreatmentFlowRateBroadcast.receivers++;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		WaterTreatmentFlowRateBroadcast.receivers--;
	}

	protected override void CollectWater()
	{
		if (doDrippingFlags)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
			flagsUpdateScope.Set(Flags.Reserved9, HasFlag(Flags.Reserved3) && hasResource() && (pushTargets == null || pushTargets.Count == 0) && WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate > 0f);
		}
		ToggleProducing(_: true);
		float num = ((overrideCollectInterval > 0f) ? overrideCollectInterval : 60f);
		nextCollect = num + Random.Range(0f, num * 0.1f);
		if (base.inventory != null && !IsFull())
		{
			if (WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate > 0f)
			{
				float f = WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate * ((float)nextCollect / 60f);
				AddResource(Mathf.CeilToInt(f));
			}
			else
			{
				float amount = Mathf.CeilToInt(evaporationPerMinute * ((float)nextCollect / 60f));
				RemoveResource(amount);
			}
		}
	}

	private void RemoveResource(float amount)
	{
		if (hasResource())
		{
			Item liquidItem = GetLiquidItem();
			liquidItem.amount -= Mathf.RoundToInt(amount);
			liquidItem.MarkDirty();
			if (liquidItem.amount <= 0)
			{
				liquidItem.Remove();
			}
		}
	}

	public override void ToggleProducing(bool _)
	{
		bool b = WaterTreatmentFlowRateBroadcast.WaterTreatmentBroadcastFlowRate > 0f;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved3, b);
	}
}

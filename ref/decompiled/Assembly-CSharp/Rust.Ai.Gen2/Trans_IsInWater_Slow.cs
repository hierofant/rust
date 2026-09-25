using System;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_IsInWater_Slow : FSMSlowTransitionBase
{
	protected override bool EvaluateAtInterval(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_IsInWater_Slow"))
		{
			if (base.Agent.canSwim)
			{
				return base.Agent.IsSwimming;
			}
			return WaterLevel.GetWaterDepth(Owner.transform.position, waves: false, volumes: false) >= 0.3f;
		}
	}
}

using System;

namespace Rust.Ai.Gen2;

[Serializable]
internal class Trans_IsTargetProtectedByMount : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_IsTargetProtectedByMount"))
		{
			if (!base.Senses.FindTarget(out var target) || !target.ToNonNpcPlayer(out var player))
			{
				return false;
			}
			BaseMountable castedUnityObject;
			return BaseNetworkableEx.Is<BaseMountable>(player.GetMounted(), out castedUnityObject) && castedUnityObject.ProtectsFromAnimals;
		}
	}
}

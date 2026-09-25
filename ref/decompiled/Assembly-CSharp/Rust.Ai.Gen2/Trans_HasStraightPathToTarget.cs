using System;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_HasStraightPathToTarget : FSMTransitionBase
{
	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_HasStraightPathToTarget"))
		{
			if (!base.Senses.FindTargetPosition(out var targetPosition))
			{
				return false;
			}
			NavMeshHit hitWS;
			return !base.Agent.Raycast(targetPosition, out hitWS);
		}
	}
}

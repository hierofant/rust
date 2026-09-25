using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class Trans_TargetInRange : FSMTransitionBase
{
	[SerializeField]
	public float Range = 4f;

	[SerializeField]
	public float TimeToPredict;

	protected override bool EvaluateInternal(ref FSMPayload payload)
	{
		using (TimeWarning.New("Trans_TargetInRange"))
		{
			if (!base.Senses.FindTarget(out var target))
			{
				return false;
			}
			Vector3 position = target.transform.position;
			if (TimeToPredict > 0f && target.ToNonNpcPlayer(out var player))
			{
				Vector3 inferedVelocity = player.inferedVelocity;
				inferedVelocity = Vector3.ProjectOnPlane(inferedVelocity, Owner.transform.right);
				position += inferedVelocity * TimeToPredict;
			}
			return Vector3.Distance(position, Owner.transform.position) <= Range;
		}
	}

	public override string GetName()
	{
		return string.Format("{0} {1}{2}m", base.GetName(), Inverted ? ">=" : "<", Range);
	}
}

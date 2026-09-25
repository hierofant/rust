using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_PlayAnimLoop : State_PlayAnimationBase
{
	[SerializeField]
	public AnimationClip Start;

	[SerializeField]
	public AnimationClip Loop;

	[SerializeField]
	public AnimationClip Stop;

	[SerializeField]
	public float MinDuration = 7f;

	[SerializeField]
	public float MaxDuration = 14f;

	private float duration;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		EFSMStateStatus result = base.OnStateEnter(payload);
		duration = UnityEngine.Random.Range(MinDuration, MaxDuration);
		animState = base.AnimPlayer.PlayServerAndTakeFromPool(Start);
		return result;
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (duration > 0f)
		{
			duration -= deltaTime;
			if (duration <= 0f)
			{
				base.AnimPlayer.StopServerAndReturnToPool(ref animState, interrupt: false);
				animState = base.AnimPlayer.PlayServerAndTakeFromPool(Stop);
			}
			else if (!animState.isPlaying)
			{
				base.AnimPlayer.StopServerAndReturnToPool(ref animState, interrupt: false);
				animState = base.AnimPlayer.PlayServerAndTakeFromPool(Loop);
			}
		}
		return base.OnStateUpdate(deltaTime);
	}
}

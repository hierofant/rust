using UnityEngine;
using UnityEngine.Animations;

public class CurveStateSpeed : StateMachineBehaviour
{
	public AnimationCurve SpeedCurve = AnimationCurve.Constant(0f, 1f, 1f);

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
		float speed = 1f;
		if (!animator.IsInTransition(layerIndex))
		{
			speed = SpeedCurve.Evaluate(stateInfo.normalizedTime);
		}
		animator.speed = speed;
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.speed = 1f;
	}
}

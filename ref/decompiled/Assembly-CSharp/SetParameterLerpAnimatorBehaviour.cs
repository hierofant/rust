using UnityEngine;
using UnityEngine.Animations;

public class SetParameterLerpAnimatorBehaviour : StateMachineBehaviour
{
	public string FloatParameterName;

	public float LerpSpeed;

	public float Target;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex, AnimatorControllerPlayable controller)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex, controller);
		float @float = controller.GetFloat(FloatParameterName);
		@float = Mathf.Lerp(@float, Target, Time.deltaTime * LerpSpeed);
		controller.SetFloat(FloatParameterName, @float);
	}
}

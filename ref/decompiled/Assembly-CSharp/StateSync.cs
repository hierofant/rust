using UnityEngine;

public class StateSync : StateMachineBehaviour
{
	public enum PlayerAnimatorLayer
	{
		Body,
		Hands
	}

	public string PlayerAnimatorStateName;

	public PlayerAnimatorLayer Layer = PlayerAnimatorLayer.Hands;

	public string OffsetParamName;
}

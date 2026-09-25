using UnityEngine;

public class HeldEntitySprintModifier : AnimationSubSystem
{
	[SerializeField]
	private float MaxSpeed = 5.5f;

	[SerializeField]
	private AnimationClip SprintPose;

	[SerializeField]
	private AnimationCurve WeightBySpeed = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float SpeedLerpMulti = 2f;
}

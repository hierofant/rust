using UnityEngine;
using UnityEngine.Serialization;

public class MasterKeyAnimationSubSystem : AnimationSubSystem
{
	public static readonly int BreakInSecondsLeftParam = Animator.StringToHash("breakinsecondsleft");

	public static readonly int LookingAtDoorParam = Animator.StringToHash("looking_door");

	public static readonly int BreakInActiveParam = Animator.StringToHash("breakin_active");

	private static readonly int ViewModelTurnStateHash = Animator.StringToHash("attack");

	[FormerlySerializedAs("ShakeClip")]
	public AnimationClip ShakeAnimationClip;

	[Tooltip("How long the shake animation takes to blend in and out when an attempt starts or stops")]
	public float ShakeFadeTime = 0.25f;
}

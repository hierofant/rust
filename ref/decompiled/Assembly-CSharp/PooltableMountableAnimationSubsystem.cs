using UnityEngine;

public class PooltableMountableAnimationSubsystem : ChildAnimatorSubSystem, INotifyLOD
{
	[SerializeField]
	private AnimationClip HitClip;

	[SerializeField]
	private bool hitUpperBodyOnly = true;

	[SerializeField]
	[Tooltip("Played full body when the player mounts - the 3p equivalent of the viewmodel deploy.")]
	private AnimationClip StartClip;

	[SerializeField]
	[Tooltip("Blend in/out time for the one shot clips. The blend out runs past the end of the clip, so it never unwinds the follow through.")]
	private float oneShotBlendTime = 0.1f;

	[SerializeField]
	[Tooltip("Lateral speed (m/s) at which the walk clips reach full weight")]
	private float walkSpeedNormalization = 1f;

	[SerializeField]
	private float blendSpeed = 8f;

	[SerializeField]
	private float movementDeadzone = 0.05f;
}

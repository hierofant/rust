using UnityEngine;

public class GenericMountableAnimationSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip MountPose;

	[SerializeField]
	private AvatarMask HeldEntityMask;
}

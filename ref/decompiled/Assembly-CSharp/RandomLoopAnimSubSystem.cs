using UnityEngine;

public class RandomLoopAnimSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip[] IdleClips;

	[SerializeField]
	private float TransitionTime = 0.25f;
}

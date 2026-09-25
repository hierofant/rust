using UnityEngine;

public class OneShotAnimationSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip[] OneShotClips;

	[SerializeField]
	private float ClipFadeIn;

	[SerializeField]
	private float ClipFadeOut;

	[SerializeField]
	private bool Additive;

	[SerializeField]
	private AnimationSubSystem ResetOnShotComplete;
}

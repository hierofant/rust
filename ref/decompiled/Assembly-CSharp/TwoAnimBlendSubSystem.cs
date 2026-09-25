using UnityEngine;

public class TwoAnimBlendSubSystem : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip ClipA;

	[SerializeField]
	private AnimationClip ClipB;

	[SerializeField]
	[Range(0f, 1f)]
	[SubSystemVariable]
	private float Blend;
}

using UnityEngine;

public class TwoAxisOneShotBlend : AnimationSubSystem
{
	[SerializeField]
	private AnimationClip NorthClip;

	[SerializeField]
	private AnimationClip SouthClip;

	[SerializeField]
	private AnimationClip EastClip;

	[SerializeField]
	private AnimationClip WestClip;

	[SerializeField]
	[Range(-1f, 1f)]
	private float XAxis;

	[SerializeField]
	[Range(-1f, 1f)]
	private float YAxis;

	[SerializeField]
	private float ClipFadeIn = 0.1f;

	[SerializeField]
	private float ClipFadeOut = 0.1f;
}

using UnityEngine;

public static class EaseExtensions
{
	public static AnimationCurve FadeInFadeOutCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f));
}

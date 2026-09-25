using UnityEngine;

public static class LeanTweenHelpers
{
	public static float ease(float start, float end, float progress, LeanTweenType tweenType)
	{
		switch (tweenType)
		{
		case LeanTweenType.linear:
			return LeanTween.linear(start, end, progress);
		case LeanTweenType.easeOutQuad:
			return LeanTween.easeOutQuad(start, end, progress);
		case LeanTweenType.easeInQuad:
			return LeanTween.easeInQuad(start, end, progress);
		case LeanTweenType.easeInOutQuad:
			return LeanTween.easeInOutQuad(start, end, progress);
		case LeanTweenType.easeInCubic:
			return LeanTween.easeInCubic(start, end, progress);
		case LeanTweenType.easeOutCubic:
			return LeanTween.easeOutCubic(start, end, progress);
		case LeanTweenType.easeInOutCubic:
			return LeanTween.easeInOutCubic(start, end, progress);
		case LeanTweenType.easeInQuart:
			return LeanTween.easeInQuart(start, end, progress);
		case LeanTweenType.easeOutQuart:
			return LeanTween.easeOutQuart(start, end, progress);
		case LeanTweenType.easeInOutQuart:
			return LeanTween.easeInOutQuart(start, end, progress);
		case LeanTweenType.easeInQuint:
			return LeanTween.easeInQuint(start, end, progress);
		case LeanTweenType.easeOutQuint:
			return LeanTween.easeOutQuint(start, end, progress);
		case LeanTweenType.easeInOutQuint:
			return LeanTween.easeInOutQuint(start, end, progress);
		case LeanTweenType.easeInSine:
			return LeanTween.easeInSine(start, end, progress);
		case LeanTweenType.easeOutSine:
			return LeanTween.easeOutSine(start, end, progress);
		case LeanTweenType.easeInOutSine:
			return LeanTween.easeInOutSine(start, end, progress);
		case LeanTweenType.easeInExpo:
			return LeanTween.easeInExpo(start, end, progress);
		case LeanTweenType.easeOutExpo:
			return LeanTween.easeOutExpo(start, end, progress);
		case LeanTweenType.easeInOutExpo:
			return LeanTween.easeInOutExpo(start, end, progress);
		case LeanTweenType.easeInCirc:
			return LeanTween.easeInCirc(start, end, progress);
		case LeanTweenType.easeOutCirc:
			return LeanTween.easeOutCirc(start, end, progress);
		case LeanTweenType.easeInOutCirc:
			return LeanTween.easeInOutCirc(start, end, progress);
		case LeanTweenType.easeInBounce:
			return LeanTween.easeInBounce(start, end, progress);
		case LeanTweenType.easeOutBounce:
			return LeanTween.easeOutBounce(start, end, progress);
		case LeanTweenType.easeInOutBounce:
			return LeanTween.easeInOutBounce(start, end, progress);
		case LeanTweenType.easeInBack:
			return LeanTween.easeInBack(start, end, progress);
		case LeanTweenType.easeOutBack:
			return LeanTween.easeOutBack(start, end, progress);
		case LeanTweenType.easeInOutBack:
			return LeanTween.easeInOutBack(start, end, progress);
		case LeanTweenType.easeInElastic:
			return LeanTween.easeInElastic(start, end, progress);
		case LeanTweenType.easeOutElastic:
			return LeanTween.easeOutElastic(start, end, progress);
		case LeanTweenType.easeInOutElastic:
			return LeanTween.easeInOutElastic(start, end, progress);
		default:
			Debug.LogError($"Ease type {tweenType} is unsupported for this helper method.");
			return float.NaN;
		}
	}

	public static float ease(float val, LeanTweenType tweenType)
	{
		return ease(0f, 1f, val, tweenType);
	}

	public static Vector3 ease(Vector3 start, Vector3 end, float progress, LeanTweenType tweenType)
	{
		float t = ease(progress, tweenType);
		return Vector3.Lerp(start, end, t);
	}

	public static Quaternion ease(Quaternion start, Quaternion end, float progress, LeanTweenType tweenType)
	{
		float t = ease(progress, tweenType);
		return Quaternion.Slerp(start, end, t);
	}
}

using UnityEngine.Animations;

namespace UnityEngine;

public static class AniamtorEx
{
	public static void SetFloatFixed(this Animator animator, int id, float value, float dampTime, float deltaTime)
	{
		if (value == 0f)
		{
			float @float = animator.GetFloat(id);
			if (@float == 0f)
			{
				return;
			}
			if (@float < float.Epsilon)
			{
				animator.SetFloat(id, 0f);
				return;
			}
		}
		animator.SetFloat(id, value, dampTime, deltaTime);
	}

	public static void SetFloatFixed(this AnimatorControllerPlayable playable, int id, float value, float dampTime, float deltaTime)
	{
		float @float = playable.GetFloat(id);
		if (value == 0f)
		{
			if (@float == 0f)
			{
				return;
			}
			if (@float < float.Epsilon)
			{
				playable.SetFloat(id, 0f);
				return;
			}
		}
		float value2 = Mathf.Lerp(@float, value, deltaTime / Mathf.Max(dampTime, 0.0001f));
		playable.SetFloat(id, value2);
	}

	public static void SetBoolChecked(this AnimatorControllerPlayable playable, int id, bool value)
	{
		if (playable.GetBool(id) != value)
		{
			playable.SetBool(id, value);
		}
	}

	public static void SetBoolChecked(this Animator animator, int id, bool value)
	{
		if (animator.GetBool(id) != value)
		{
			animator.SetBool(id, value);
		}
	}
}

using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
	private static readonly int Up = Animator.StringToHash("up");

	private static readonly int Right = Animator.StringToHash("right");

	public const string EventFunctionName = "Event";

	public const string ClearEventName = "clear_holdtype";

	public const string DartThrowEventName = "ThrowDart";
}

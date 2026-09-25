using UnityEngine.InputSystem;

namespace Facepunch;

public static class MouseScroll
{
	public const float WheelDelta = 10f;

	public static float ReadNotches()
	{
		Mouse current = Mouse.current;
		if (current == null)
		{
			return 0f;
		}
		return current.scroll.ReadValue().y * 10f;
	}
}

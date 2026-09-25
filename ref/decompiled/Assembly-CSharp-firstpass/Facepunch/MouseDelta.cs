using UnityEngine;
using UnityEngine.InputSystem;

namespace Facepunch;

public static class MouseDelta
{
	public const float LegacyScale = 0.05f;

	public static Vector2 Read()
	{
		Mouse current = Mouse.current;
		if (current == null)
		{
			return Vector2.zero;
		}
		return current.delta.ReadValue() * 0.05f;
	}
}

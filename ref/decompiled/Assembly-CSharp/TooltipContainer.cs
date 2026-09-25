using Rust.UI;
using UnityEngine;

public class TooltipContainer : MonoBehaviour
{
	public enum PositionMode
	{
		Auto,
		Top,
		Bottom,
		Left,
		Right,
		TopLeft
	}

	public Transform ScaleRoot;

	public RustText TooltipText;

	public RectTransform OverrideLayoutRoot;
}

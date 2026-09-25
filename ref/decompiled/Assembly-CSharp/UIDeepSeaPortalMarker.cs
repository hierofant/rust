using Rust.UI;
using UnityEngine;

public class UIDeepSeaPortalMarker : MonoBehaviour, IClientComponent
{
	public RectTransform movingParentRect;

	public RectTransform viewportRect;

	public RectTransform iconRect;

	public RectTransform textRect;

	public Vector2 missionMarkerOffset;

	public RustText timerText;
}

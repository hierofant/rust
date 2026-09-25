using Rust.UI;
using UnityEngine;

public class UIGenericOverlay : SingletonComponent<UIGenericOverlay>
{
	public enum OverlayAnchor
	{
		Top,
		Center
	}

	[SerializeField]
	private GameObject labelGo;

	[SerializeField]
	private RustText label;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private RectTransform topContentRoot;

	[SerializeField]
	private RectTransform centerContentRoot;
}

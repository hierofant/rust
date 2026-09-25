using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreLimitedTab : UI_StoreTabBase
{
	[Space]
	[SerializeField]
	private UI_StoreItemGrid itemGrid;

	[SerializeField]
	private UI_StoreCountdown countdown;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private ScrollRect scrollRect;

	public RectTransform pageOverlayParent;

	public static UI_StoreLimitedTab Instance { get; private set; }
}

using UnityEngine;

namespace Rust.UI.MainMenu.Workshop;

public class UI_WorkshopItemButton : MonoBehaviour
{
	[SerializeField]
	private HttpImage httpImage;

	[SerializeField]
	private Transform scaledParent;

	[SerializeField]
	private GameObject loadingIndicator;
}

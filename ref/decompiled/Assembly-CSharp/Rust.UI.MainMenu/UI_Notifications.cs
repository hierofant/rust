using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_Notifications : UI_Window
{
	[Header("Prefab & Container")]
	[SerializeField]
	private GameObjectRef entryPrefab;

	[SerializeField]
	private RectTransform contentRoot;

	[SerializeField]
	private GameObject noNotifications;

	[SerializeField]
	private GameObject circle;

	[SerializeField]
	private GameObject clearButton;

	[SerializeField]
	private StyleAsset regularStyle;

	[SerializeField]
	private StyleAsset seenStyle;
}

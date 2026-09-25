using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SettingsGesturePack : MonoBehaviour
{
	[SerializeField]
	private RustText headerText;

	[SerializeField]
	public RectTransform contentParent;

	[SerializeField]
	private GameObject storeButton;

	[SerializeField]
	private GameObject lockIcon;
}

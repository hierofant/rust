using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreItemDLC : MonoBehaviour
{
	public int appID;

	public UI_StoreItemOverlayPage overlayPagePrefab;

	public UI_StoreAddCartButton cartButton;

	private IPlayerItemDefinition _item;
}

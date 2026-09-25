using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SteamInventoryItem : MonoBehaviour
{
	public RustButton button;

	[SerializeField]
	private HttpImage image;

	[SerializeField]
	private GameObject takeoverGroup;

	[SerializeField]
	private CoverImage takeoverImage;

	[SerializeField]
	private RustText nameText;

	[SerializeField]
	private RustText subtitleText;

	[SerializeField]
	private GameObject twitchDropTag;

	public IPlayerItem PlayerItem;
}

using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_DropsController : FacepunchBehaviour
{
	[SerializeField]
	private GameObject _dropPrefab;

	[SerializeField]
	private Transform _dropsParent;

	[ClientVar(Saved = true, Help = "(Generated) When enabled, shows placeholder/test drop data in the drops controller UI instead of live manifest data; saved between sessions")]
	public static bool show_placeholder_drop_data;

	private const int MAX_DROPS = 3;
}

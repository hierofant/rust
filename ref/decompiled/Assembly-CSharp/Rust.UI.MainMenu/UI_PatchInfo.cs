using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_PatchInfo : FacepunchBehaviour
{
	[SerializeField]
	private RustText _title;

	[SerializeField]
	private RustText _description;

	[SerializeField]
	private RustText _date;

	[SerializeField]
	private RustButton _readMoreButton;

	[SerializeField]
	private RustButton _changelogButton;

	[SerializeField]
	private GameObject _devlogTag;
}

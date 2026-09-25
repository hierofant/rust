using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_KeyBindLabel : MonoBehaviour
{
	[SerializeField]
	private bool strict;

	[SerializeField]
	private string command;

	[SerializeField]
	private RustText keyText;
}

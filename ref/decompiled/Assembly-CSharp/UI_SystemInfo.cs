using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;

public class UI_SystemInfo : UI_Window
{
	[Space]
	[SerializeField]
	private RustText systemText;

	[SerializeField]
	private RustText cpuText;

	[SerializeField]
	private RustText gpuText;

	[SerializeField]
	private RustText processText;

	[SerializeField]
	private RustText monoText;

	[SerializeField]
	private GameObject worldGroup;

	[SerializeField]
	private RustText worldText;
}

using Rust.UI;
using UnityEngine;

public class UI_ToggleConvar : MonoBehaviour, IClientComponent
{
	[SerializeField]
	private RustButton toggleControl;

	[SerializeField]
	private string convar;
}

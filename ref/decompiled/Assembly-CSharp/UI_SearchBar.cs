using Facepunch.Flexbox;
using Rust.UI;
using TMPro;
using UnityEngine;

public class UI_SearchBar : MonoBehaviour
{
	public RustInput rustInput;

	[Space]
	public RustButton cancelButton;

	public GameObject defaultIcon;

	[Space]
	public FlexTransition transition;

	public UIEscapeCapture escapeCapture;

	public TMP_InputField InputField => rustInput.InputField;
}

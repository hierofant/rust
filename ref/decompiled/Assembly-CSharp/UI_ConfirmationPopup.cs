using Rust.UI;
using UnityEngine;

public class UI_ConfirmationPopup : MonoBehaviour
{
	[SerializeField]
	private Transform buttonsParent;

	[SerializeField]
	private RustText messageText;

	[SerializeField]
	private RustButton buttonTemplate;

	public RustButton[] buttons;
}

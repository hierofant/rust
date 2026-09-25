using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SafeModeWarning : UI_Popup, IClientComponent
{
	[Space]
	[SerializeField]
	private GameObject yesButton;

	[SerializeField]
	private GameObject timer;

	[SerializeField]
	private RustText timerText;

	[SerializeField]
	private float timeToWait = 5f;
}

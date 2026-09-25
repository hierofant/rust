using Facepunch.Flexbox;
using Rust.UI;
using UnityEngine;

public class UI_ToggleHUDLayer : MonoBehaviour, IClientComponent
{
	[SerializeField]
	private FlexElement flexElement;

	[SerializeField]
	private RustButton toggleControl;

	[SerializeField]
	private RustText hudLayerNameText;
}

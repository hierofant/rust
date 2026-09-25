using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_StoreGeneralSubTabButton : BaseMonoBehaviour
{
	[SerializeField]
	private FlexTransition transition;

	[SerializeField]
	[Space]
	private FlexElement flexElement;

	[SerializeField]
	private FlexElement textParent;

	[SerializeField]
	private RustText text;
}

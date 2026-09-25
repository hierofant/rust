using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_Header : MonoBehaviour
{
	public enum HeaderState
	{
		None,
		Ascending,
		Descending
	}

	[SerializeField]
	[Header("References")]
	private FlexTransition _ascendingTransition;

	[SerializeField]
	private FlexTransition _descendingTransition;
}

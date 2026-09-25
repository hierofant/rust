using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerAdminPlayerEntry : MonoBehaviour
{
	[SerializeField]
	private RawImage avatarImage;

	[SerializeField]
	private RustText playerNameText;

	[SerializeField]
	private RustText playerIdText;

	[SerializeField]
	private RustText pingText;

	[SerializeField]
	[Space]
	private Color evenColor;

	[SerializeField]
	private Image bgImage;

	public RustButton button;
}

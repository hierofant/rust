using Rust.UI;
using UnityEngine;

public class UI_LoadoutFileButton : MonoBehaviour
{
	[SerializeField]
	private RustText fileNameText;

	[SerializeField]
	private RustText dateText;

	[SerializeField]
	private RustText itemCountText;

	[SerializeField]
	[Space]
	private GameObject deleteButton;

	[SerializeField]
	private RectTransform virtualItemParent;
}

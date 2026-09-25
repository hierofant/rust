using Rust.UI;
using UnityEngine;

public class UI_DemoItem : RustButton
{
	[HideInInspector]
	public UI_DemosMenuWindow demos;

	public int itemId;

	public RustText nameText;

	public RustText dateText;

	public RustText lengthText;
}

using Rust.UI;
using UnityEngine;

public class UI_CommandListButton : MonoBehaviour
{
	public RustFlexText RealmTag;

	public RustFlexText CommandString;

	public RustFlexText DescriptionString;

	public RustFlexText Default;

	public RustButton Button;

	public StyleAsset StandardStyle;

	public StyleAsset DarkStyle;

	public StyleAsset ChangedStyle;

	[Header("Tooltips")]
	public Tooltip DescriptionTooltip;

	public Tooltip CommandTooltip;

	public Tooltip ValueTooltip;
}

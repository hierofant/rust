using System.Collections.Generic;
using Rust.UI;
using UnityEngine;

public class UI_SuggestionsHolder : MonoBehaviour
{
	[SerializeField]
	private List<ItemDefinition> items = new List<ItemDefinition>();

	[SerializeField]
	private List<VirtualItemIcon> icons = new List<VirtualItemIcon>();

	[SerializeField]
	private List<RustButton> buttons = new List<RustButton>();
}

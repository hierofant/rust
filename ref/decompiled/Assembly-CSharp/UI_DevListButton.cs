using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_DevListButton : MonoBehaviour
{
	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private RustText itemNameText;

	[SerializeField]
	private Image forbiddenImage;

	[SerializeField]
	private GameObject giveButtons;

	[SerializeField]
	private RustButton favouriteButton;

	[SerializeField]
	private RustButton equipButton;

	[SerializeField]
	private GameObject selectionHighlight;

	[NonSerialized]
	public ItemDefinition itemDef;

	[NonSerialized]
	public ScrollRect scroll;
}

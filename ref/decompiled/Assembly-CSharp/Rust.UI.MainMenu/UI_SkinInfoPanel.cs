using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SkinInfoPanel : MonoBehaviour
{
	[Serializable]
	private struct Tag
	{
		public string Name;

		public GameObject GameObject;
	}

	[SerializeField]
	private RustText nameText;

	[SerializeField]
	private RustText itemTypeText;

	[SerializeField]
	private RustText descText;

	[SerializeField]
	private List<Tag> tagDefinitions = new List<Tag>();

	[Header("Marketable Tag")]
	[SerializeField]
	private GameObject marketablePriceGroup;

	[SerializeField]
	private GameObject marketableLockedGroup;

	[SerializeField]
	private RustText daysLeftText;

	[SerializeField]
	private RustText priceText;
}

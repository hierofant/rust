using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_SteamInventoryCraftingModal : UI_SteamInventoryItemBaseModal
{
	[Serializable]
	public struct MaterialGroup
	{
		public GameObject GameObject;

		public RustText CountText;

		public Image BackgroundImage;
	}

	[SerializeField]
	private MaterialGroup woodGroup;

	[SerializeField]
	private MaterialGroup metalGroup;

	[SerializeField]
	private MaterialGroup clothGroup;

	[SerializeField]
	[Space]
	private GameObject craftOK;

	[SerializeField]
	private GameObject craftKO;

	[SerializeField]
	private UI_SteamInventoryNewItemModal newItemModal;
}

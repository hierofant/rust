using System.Collections.Generic;
using UnityEngine;

public class UI_FakeInventory : MonoBehaviour
{
	[SerializeField]
	private List<VirtualItemIcon> inventoryIcons = new List<VirtualItemIcon>();

	[SerializeField]
	private List<UI_DragVirtualItemIcon> dragIcons = new List<UI_DragVirtualItemIcon>();
}

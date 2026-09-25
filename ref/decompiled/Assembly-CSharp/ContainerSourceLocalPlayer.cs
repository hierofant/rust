using UnityEngine;

public class ContainerSourceLocalPlayer : ItemContainerSource
{
	private const int AboveInventorySortingOrder = 110;

	public PlayerInventory.Type type;

	public bool hideInvalidIcons;

	public Transform rootOverride;

	private ItemIcon[] allIcons;

	public override ItemContainer GetItemContainer()
	{
		return null;
	}
}

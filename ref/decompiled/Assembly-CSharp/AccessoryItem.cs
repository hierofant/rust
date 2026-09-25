using UnityEngine;

[CreateAssetMenu(menuName = "Create/Rust/Skins/Accessory Item")]
public class AccessoryItem : SteamInventoryItem
{
	[Header("Accessory")]
	public GameObjectRef AccessoryPrefab;

	public GameObjectRef AccessoryAltPrefab;

	public GameObjectRef AccessoryTertiaryPrefab;
}

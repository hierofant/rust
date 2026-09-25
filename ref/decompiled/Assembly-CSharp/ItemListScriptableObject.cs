using UnityEngine;

[CreateAssetMenu(fileName = "NewItemList", menuName = "Rust/ItemList")]
public class ItemListScriptableObject : BaseScriptableObject
{
	public ItemDefinition[] Items;
}

using Oxide.Core;
using UnityEngine;

public class LootFill : BaseMonoBehaviour, IServerComponent
{
	[Header("Loot")]
	public StorageContainer StorageContainer;

	public LootSpawn LootDefinition;

	public int MaxDefinitionsToSpawn;

	public LootContainer.LootSpawnSlot[] LootSpawnSlots;

	public void FillLoot()
	{
		Invoke(DelayFill, 5f);
	}

	private void DelayFill()
	{
		if (!(StorageContainer == null) && Interface.CallHook("OnLootSpawn", this) == null)
		{
			LootContainer.FillLoot(StorageContainer.inventory, LootDefinition, MaxDefinitionsToSpawn, LootSpawnSlots);
		}
	}
}

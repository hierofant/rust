using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public interface IItemContainerEntity : IIdealSlotEntity, ILootableEntity, IInventoryProvider
{
	public struct ContainerSet
	{
		public int ContainerIndex;

		public uint PrefabId;
	}

	public struct ContainerPreserveInfo
	{
		public Dictionary<ContainerSet, List<Item>> storageDict;
	}

	string PrefabName { get; }

	ItemContainer inventory { get; }

	Transform Transform { get; }

	bool DropsLoot { get; }

	float DestroyLootPercent { get; }

	bool DropFloats { get; }

	void DropItems(BaseEntity initiator = null);

	bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true);

	bool ShouldDropItemsIndividually();

	void DropBonusItems(BaseEntity initiator, ItemContainer container);

	Vector3 GetDropPosition();

	void Reskin_Preserve_Container(ref ContainerPreserveInfo preserve, BaseEntity entity, int index)
	{
		if (preserve.storageDict == null)
		{
			preserve.storageDict = Pool.Get<Dictionary<ContainerSet, List<Item>>>();
		}
		ContainerSet containerSet = default(ContainerSet);
		containerSet.ContainerIndex = index;
		containerSet.PrefabId = ((index != -1) ? entity.prefabID : 0u);
		ContainerSet key = containerSet;
		if (preserve.storageDict.ContainsKey(key))
		{
			Debug.LogError("Multiple containers with the same prefab id being added during reskin");
			return;
		}
		preserve.storageDict.Add(key, Pool.Get<List<Item>>());
		foreach (Item item in inventory.itemList)
		{
			preserve.storageDict[key].Add(item);
		}
		foreach (Item item2 in preserve.storageDict[key])
		{
			item2.RemoveFromContainer();
		}
	}

	void Reskin_Restore_Container(ref ContainerPreserveInfo preserve, BaseEntity entity, int index)
	{
		ContainerSet containerSet = default(ContainerSet);
		containerSet.ContainerIndex = index;
		containerSet.PrefabId = ((index != -1) ? entity.prefabID : 0u);
		ContainerSet key = containerSet;
		if (preserve.storageDict == null)
		{
			Debug.LogError("Inventory not found in preserve after reskinning to " + entity.ShortPrefabName);
			return;
		}
		if (!preserve.storageDict.ContainsKey(key))
		{
			bool flag = false;
			foreach (ContainerSet key2 in preserve.storageDict.Keys)
			{
				if (key2.PrefabId == key.PrefabId)
				{
					key = key2;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogError("Matching inventory not found in preserve after reskinning: " + entity.ShortPrefabName);
				return;
			}
		}
		foreach (Item item in preserve.storageDict[key])
		{
			item.MoveToContainer(inventory);
		}
		List<Item> obj = preserve.storageDict[key];
		Pool.FreeUnmanaged(ref obj);
		preserve.storageDict.Remove(key);
		if (CollectionEx.IsEmpty(preserve.storageDict))
		{
			Pool.FreeUnmanaged(ref preserve.storageDict);
		}
	}
}

using System;
using Rust;
using UnityEngine;

public class TrainCarUnloadableLoot : TrainCarUnloadable
{
	[Serializable]
	public class LootCrateSet
	{
		public GameObjectRef[] crates;
	}

	[SerializeField]
	private LootCrateSet[] lootLayouts;

	[SerializeField]
	private Transform[] lootPositions;

	public override void Spawn()
	{
		base.Spawn();
		bool flag = false;
		if (Rust.Application.isLoadingSave || flag)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, lootLayouts.Length);
		for (int i = 0; i < lootLayouts[num].crates.Length; i++)
		{
			GameObjectRef gameObjectRef = lootLayouts[num].crates[i];
			LootContainer lootContainer = GameManager.server.CreateEntity(gameObjectRef.resourcePath, lootPositions[i].localPosition, lootPositions[i].localRotation) as LootContainer;
			if (lootContainer != null)
			{
				lootContainer.Spawn();
				lootContainer.SetParent(this);
				lootContainer.inventory.SetLocked(!IsEmpty());
				lootContainers.Add(new EntityRef<LootContainer>(lootContainer.net.ID));
			}
		}
	}
}

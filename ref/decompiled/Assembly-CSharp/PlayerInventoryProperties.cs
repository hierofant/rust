using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Player/Player Inventory Properties")]
public class PlayerInventoryProperties : BaseScriptableObject
{
	[Serializable]
	public class ItemAmountSkinned : ItemAmount
	{
		public ulong skinOverride;

		public bool blueprint;
	}

	public enum LoadoutCategory
	{
		Tiers,
		NPCs,
		Other
	}

	public string niceName;

	public int order = 100;

	public LoadoutCategory category = LoadoutCategory.Other;

	public List<ItemAmountSkinned> belt;

	public List<ItemAmountSkinned> main;

	public List<ItemAmountSkinned> wear;

	public GameObjectRef DeathIconPrefab;

	public bool StripBelt = true;

	public PlayerInventoryProperties giveBase;

	public bool StripMain = true;

	public bool StripWear = true;

	private static PlayerInventoryProperties[] allInventories;

	public void GiveToPlayer(BasePlayer player)
	{
		if (player == null)
		{
			return;
		}
		if (player is HumanNPC humanNPC && DeathIconPrefab.isValid)
		{
			humanNPC.DeathIconOverride = DeathIconPrefab;
		}
		if (giveBase != null)
		{
			giveBase.GiveToPlayer(player);
		}
		if (belt != null && belt.Count > 0)
		{
			if (StripBelt)
			{
				player.inventory.containerBelt.Clear();
			}
			foreach (ItemAmountSkinned item2 in belt)
			{
				CreateItem(item2, player.inventory.containerBelt);
			}
		}
		if (main != null && main.Count > 0)
		{
			if (StripMain)
			{
				player.inventory.containerMain.Clear();
			}
			foreach (ItemAmountSkinned item3 in main)
			{
				CreateItem(item3, player.inventory.containerMain);
			}
		}
		if (wear == null || wear.Count <= 0)
		{
			return;
		}
		if (StripWear)
		{
			player.inventory.containerWear.Clear();
		}
		foreach (ItemAmountSkinned item4 in wear)
		{
			CreateItem(item4, player.inventory.containerWear);
		}
		void CreateItem(ItemAmountSkinned toCreate, ItemContainer destination)
		{
			Item item = null;
			if (toCreate.blueprint)
			{
				item = ItemManager.Create(ItemManager.blueprintBaseDef, 1, 0uL, isServerSide: true, 0uL);
				item.blueprintTarget = ((toCreate.itemDef.isRedirectOf != null) ? toCreate.itemDef.isRedirectOf.itemid : toCreate.itemDef.itemid);
			}
			else
			{
				item = ItemManager.Create(toCreate.itemDef, (int)toCreate.amount, toCreate.skinOverride, isServerSide: true, 0uL);
			}
			player.inventory.GiveItem(item, destination);
		}
	}

	public static PlayerInventoryProperties GetInventoryConfig(string name)
	{
		if (allInventories == null)
		{
			allInventories = FileSystem.LoadAll<PlayerInventoryProperties>("assets/content/properties/playerinventory");
		}
		if (allInventories != null)
		{
			PlayerInventoryProperties[] array = allInventories;
			foreach (PlayerInventoryProperties playerInventoryProperties in array)
			{
				if (playerInventoryProperties.niceName.ToLower() == name.ToLower())
				{
					return playerInventoryProperties;
				}
			}
		}
		return null;
	}
}

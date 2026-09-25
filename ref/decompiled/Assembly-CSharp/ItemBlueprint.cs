using System;
using System.Collections.Generic;
using ConVar;
using Rust;
using UnityEngine;

public class ItemBlueprint : MonoBehaviour
{
	[Serializable]
	public struct BlueprintOverride
	{
		public Era TargetEra;

		public List<ItemAmount> Ingredients;

		public float craftTime;

		public int workbenchLevel;

		public BlueprintOverride(ItemBlueprint bp)
		{
			TargetEra = Era.None;
			Ingredients = bp.ingredients;
			craftTime = bp.time;
			workbenchLevel = bp.workbenchLevelRequired;
		}
	}

	[Serializable]
	public class SurplusEntry
	{
		[ItemSelector]
		public ItemDefinition itemDef;

		public int amount = 1;

		[Tooltip("Chance (0-1) this surplus item is created when crafted at a workbench with the surplus upgrade.")]
		[Range(0f, 1f)]
		public float chance = 0.1f;
	}

	private ItemDefinition _targetItem;

	public List<ItemAmount> ingredients = new List<ItemAmount>();

	public List<ItemDefinition> additionalUnlocks = new List<ItemDefinition>();

	public bool defaultBlueprint;

	public bool userCraftable = true;

	public bool isResearchable = true;

	public bool forceShowInConveyorFilter;

	public Rarity rarity;

	[Header("Workbench")]
	public int workbenchLevelRequired;

	[Header("Surplus")]
	[Tooltip("Items that may be produced as surplus when crafted at a workbench with the surplus upgrade installed. Leave null/empty for no surplus.")]
	public List<SurplusEntry> surplusItems;

	[Header("Scrap")]
	public int scrapRequired;

	public int scrapFromRecycle;

	[Tooltip("This item won't show anywhere unless you have the corresponding SteamItem in your inventory - which is defined on the ItemDefinition")]
	[Header("Unlocking")]
	public bool NeedsSteamItem;

	public ItemDefinition RequireUnlockedItem;

	public int blueprintStackSize = -1;

	public float time = 1f;

	public int amountToCreate = 1;

	public bool ForceThisCraftTime;

	public string UnlockAchievment;

	public string RecycleStat;

	public List<BlueprintOverride> Overrides = new List<BlueprintOverride>();

	public ItemDefinition targetItem
	{
		get
		{
			if ((object)_targetItem == null)
			{
				_targetItem = GetComponent<ItemDefinition>();
			}
			return _targetItem;
		}
	}

	public bool NeedsSteamDLC => targetItem.steamDlc != null;

	public List<ItemAmount> GetIngredients()
	{
		BlueprintOverride recipeOverride = GetRecipeOverride();
		float multiplier = 1f;
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null)
		{
			multiplier = activeGameMode.GetCraftingCostModifier(targetItem);
		}
		if (recipeOverride.Ingredients != null && recipeOverride.Ingredients.Count > 0)
		{
			ApplyMultiplierToIngredients(recipeOverride.Ingredients, multiplier);
			return recipeOverride.Ingredients;
		}
		ApplyMultiplierToIngredients(ingredients, multiplier);
		return ingredients;
	}

	private void ApplyMultiplierToIngredients(List<ItemAmount> ingredients, float multiplier)
	{
		foreach (ItemAmount ingredient in ingredients)
		{
			ingredient.amount = Mathf.RoundToInt(ingredient.startAmount * multiplier);
		}
	}

	public float GetCraftTime()
	{
		float craftTime = time;
		float num = 5f;
		ItemDefinition itemDefinition = targetItem;
		if (itemDefinition != null && (itemDefinition.stackable == 1 || itemDefinition.isHoldable || itemDefinition.isWearable || itemDefinition.isUsable || itemDefinition.category == ItemCategory.Construction || itemDefinition.category == ItemCategory.Traps))
		{
			num += (float)Mathf.Max(workbenchLevelRequired, 1) * 10f;
		}
		BlueprintOverride recipeOverride = GetRecipeOverride();
		if (recipeOverride.craftTime > 0f)
		{
			craftTime = recipeOverride.craftTime;
		}
		if (ForceThisCraftTime)
		{
			return craftTime;
		}
		return Mathf.Min(num, craftTime);
	}

	public BlueprintOverride GetRecipeOverride()
	{
		if (Overrides.Count == 0)
		{
			return new BlueprintOverride(this);
		}
		foreach (BlueprintOverride @override in Overrides)
		{
			if (@override.TargetEra == ConVar.Server.Era)
			{
				return @override;
			}
		}
		return new BlueprintOverride(this);
	}

	public int GetWorkbenchLevel()
	{
		BlueprintOverride recipeOverride = GetRecipeOverride();
		if (recipeOverride.workbenchLevel != 0)
		{
			return Mathf.Max(0, recipeOverride.workbenchLevel);
		}
		return workbenchLevelRequired;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Rust;
using UnityEngine;

public class ItemDefinition : MonoBehaviour, IEqualityComparer<ItemDefinition>
{
	[Flags]
	public enum LootDistributionModifierType
	{
		None = 0,
		Firearm = 1,
		FirearmAmmunition = 2,
		Unused = int.MinValue
	}

	[Serializable]
	public struct Condition
	{
		[Serializable]
		public class WorldSpawnCondition
		{
			public float fractionMin = 1f;

			public float fractionMax = 1f;
		}

		public bool enabled;

		[Tooltip("The maximum condition this item type can have, new items will start with this value")]
		public float max;

		[Tooltip("If false then item will destroy when condition reaches 0")]
		public bool repairable;

		[Tooltip("If true, never lose max condition when repaired")]
		public bool maintainMaxCondition;

		public bool ovenCondition;

		public WorldSpawnCondition foundCondition;

		public bool hideConditionBar;

		public GameObjectRef breakEffect;
	}

	[Serializable]
	public struct OverrideWorldModel
	{
		public GameObjectRef worldModel;

		public int minStackSize;
	}

	public enum RedirectVendingBehaviour
	{
		NoListing,
		ListAsUniqueItem
	}

	[Flags]
	public enum Flag
	{
		NoDropping = 1,
		NotStraightToBelt = 2,
		NotAllowedInBelt = 4,
		Backpack = 8,
		PrioritizeBelt = 0x10
	}

	public enum AmountType
	{
		Count,
		Millilitre,
		Feet,
		Genetics,
		OxygenSeconds,
		Frequency,
		Generic,
		BagLimit,
		ShelterLimit,
		ContentCount,
		TurretLimit,
		NucleusGrades,
		BBSLimit
	}

	[ReadOnly]
	[Header("Item")]
	public int itemid;

	[Tooltip("The shortname should be unique. A hash will be generated from it to identify the item type. If this name changes at any point it will make all saves incompatible")]
	public string shortname;

	public Era era;

	public EraRestriction eraRestrictions;

	public LootDistributionModifierType lootDistributionType;

	[Header("Appearance")]
	public Translate.Phrase displayName;

	public Translate.Phrase displayDescription;

	public Sprite iconSprite;

	public ItemCategory category;

	public ItemSelectionPanel selectionPanel;

	[Header("Appearance - Vehicle Item")]
	public bool vehicleItem;

	public VehicleCategory vehicleCategory = VehicleCategory.Misc;

	[Header("Containment")]
	public int maxDraggable;

	public ItemContainer.ContentsType itemType = ItemContainer.ContentsType.Generic;

	public AmountType amountType;

	[InspectorFlags]
	public ItemSlot occupySlots = ItemSlot.None;

	public int stackable;

	public int volume;

	public float baseRadioactivity;

	[NonSerialized]
	public float ApartmentTaxPerStack;

	public bool quickDespawn;

	public bool blockStealingInSafeZone;

	[Tooltip("Should this item be blocked from being burried and found by other players? Off by default to allow most items.")]
	public bool allowBurying;

	public BasePlayer.TutorialItemAllowance tutorialAllowance;

	[Tooltip("If true, this item will support item ownership even if it's stacksize is >1")]
	public bool supportsStackableOwnership;

	[Header("Spawn Tables")]
	[Tooltip("How rare this item is and how much it costs to research")]
	public Rarity rarity;

	public Rarity despawnRarity;

	public bool spawnAsBlueprint;

	[Header("Sounds")]
	public SoundDefinition inventoryGrabSound;

	public SoundDefinition inventoryDropSound;

	public SoundDefinition physImpactSoundDef;

	public Condition condition;

	[Header("Misc")]
	public bool hidden;

	[InspectorFlags]
	public Flag flags;

	public bool hideSelectedPanel;

	[Tooltip("User can craft this item on any server if they have this steam item")]
	public SteamInventoryItem steamItem;

	[Tooltip("User can craft this item if they have this DLC purchased")]
	public SteamDLCItem steamDlc;

	public bool supportsAccessories;

	[Tooltip("Can only craft this item if the parent is craftable (tech tree)")]
	public ItemDefinition Parent;

	[Header("World Model")]
	public GameObjectRef worldModelPrefab;

	public OverrideWorldModel[] worldModelOverrides;

	public bool treatAsComponentForRepairs;

	public bool AlignWorldModelOnDrop;

	public Vector3 WorldModelDropOffset;

	public bool AdjustCenterOfMassOnDrop;

	public Vector3 DropCenterOfMass;

	public ItemDefinition isRedirectOf;

	public RedirectVendingBehaviour redirectVendingBehaviour;

	[NonSerialized]
	public ItemMod[] itemMods;

	public BaseEntity.TraitFlag Traits;

	private string _harvestStatKey;

	public ItemSkinDirectory.Skin[] skins;

	[NonSerialized]
	public IPlayerItemDefinition[] _skins2;

	private float _worldModelMass;

	[Tooltip("Panel to show in the inventory menu when selected")]
	public GameObject panel;

	private ItemBlueprint _blueprint;

	[NonSerialized]
	public ItemDefinition[] Children = new ItemDefinition[0];

	public string HarvestStatKey
	{
		get
		{
			if (_harvestStatKey == null)
			{
				_harvestStatKey = "harvest." + shortname;
			}
			return _harvestStatKey;
		}
	}

	public IPlayerItemDefinition[] skins2
	{
		get
		{
			if (_skins2 != null)
			{
				return _skins2;
			}
			if (PlatformService.Instance.IsValid && PlatformService.Instance.ItemDefinitions != null)
			{
				string prefabname = base.name;
				_skins2 = PlatformService.Instance.ItemDefinitions.Where((IPlayerItemDefinition x) => (x.ItemShortName == shortname || x.ItemShortName == prefabname) && x.WorkshopId != 0).ToArray();
			}
			return _skins2;
		}
	}

	public ItemBlueprint Blueprint
	{
		get
		{
			if ((object)_blueprint == null)
			{
				_blueprint = GetComponent<ItemBlueprint>();
			}
			return _blueprint;
		}
	}

	public int craftingStackable => Mathf.Max(10, stackable);

	public bool isWearable => ItemModWearable != null;

	public ItemModWearable ItemModWearable { get; set; }

	public ItemModBurnable ItemModBurnable { get; set; }

	public CookableItemInfo ItemModCookable { get; set; }

	public CookableItemInfo ItemModCompostable { get; private set; }

	public ItemModEntity ItemModEntity { get; private set; }

	public bool HasItemModEntity { get; private set; }

	public ItemModSpriteConfig ItemModSpriteConfig { get; private set; }

	public bool isHoldable { get; private set; }

	public bool isUsable { get; private set; }

	public bool HasSkins
	{
		get
		{
			if (skins2 != null && skins2.Length != 0)
			{
				return true;
			}
			if (skins != null && skins.Length != 0)
			{
				return true;
			}
			return false;
		}
	}

	public bool CraftableWithSkin { get; private set; }

	public bool Hidden()
	{
		return hidden;
	}

	public bool MatchesItemId(int itemid, bool redirectAllowed)
	{
		if (this.itemid == itemid)
		{
			return true;
		}
		if (redirectAllowed && isRedirectOf != null)
		{
			return isRedirectOf.itemid == itemid;
		}
		return false;
	}

	public void InvalidateWorkshopSkinCache()
	{
		_skins2 = null;
	}

	public bool IsAllowed(EraRestriction targetRestriction)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null && !activeGameMode.IsAllowed(this, targetRestriction))
		{
			return false;
		}
		return IsAllowedInEra(targetRestriction);
	}

	public bool IsAllowed(EraRestriction targetRestriction, Era serverEra)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode != null && !activeGameMode.IsAllowed(this, targetRestriction))
		{
			return false;
		}
		return IsAllowedInEra(targetRestriction, serverEra);
	}

	public bool IsAllowedInEra(EraRestriction targetRestriction)
	{
		if (ConVar.Server.Era == Era.None)
		{
			return true;
		}
		return IsAllowedInEra(targetRestriction, ConVar.Server.Era);
	}

	private bool IsAllowedInEra(EraRestriction targetRestriction, Era serverEra)
	{
		if (serverEra == Era.None)
		{
			return true;
		}
		if (isRedirectOf != null)
		{
			return isRedirectOf.IsAllowedInEra(targetRestriction);
		}
		switch (era)
		{
		case Era.None:
			return true;
		case Era.Any:
			return true;
		default:
			if (era <= serverEra)
			{
				if (targetRestriction != 0 && eraRestrictions != 0 && (eraRestrictions & targetRestriction) != eraRestrictions)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public static ulong FindSkin(int itemID, int skinID)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemID);
		if (itemDefinition == null)
		{
			return 0uL;
		}
		IPlayerItemDefinition itemDefinition2 = PlatformService.Instance.GetItemDefinition(skinID);
		if (itemDefinition2 != null)
		{
			ulong workshopDownload = itemDefinition2.WorkshopDownload;
			if (workshopDownload != 0L)
			{
				string itemShortName = itemDefinition2.ItemShortName;
				if (itemShortName == itemDefinition.shortname || itemShortName == itemDefinition.name)
				{
					return workshopDownload;
				}
			}
		}
		for (int i = 0; i < itemDefinition.skins.Length; i++)
		{
			if (itemDefinition.skins[i].id == skinID)
			{
				return (ulong)skinID;
			}
		}
		return 0uL;
	}

	public float GetWorldModelMass()
	{
		if (_worldModelMass != 0f)
		{
			return _worldModelMass;
		}
		if (worldModelPrefab.isValid)
		{
			WorldModel component = worldModelPrefab.Get().GetComponent<WorldModel>();
			if (component != null && component.mass != 0f)
			{
				_worldModelMass = component.mass;
				return _worldModelMass;
			}
		}
		_worldModelMass = 1f;
		return _worldModelMass;
	}

	public int GetWorldModelTriCount(int lod = 0)
	{
		if (worldModelPrefab == null || !worldModelPrefab.isValid)
		{
			return 0;
		}
		GameObject gameObject = worldModelPrefab.Get();
		if (gameObject == null)
		{
			return 0;
		}
		if (gameObject.TryGetComponent<WorldModel>(out var component))
		{
			return component.GetTriCount(lod);
		}
		return 0;
	}

	public bool HasFlag(Flag f)
	{
		return (flags & f) == f;
	}

	public void Initialize(List<ItemDefinition> itemList)
	{
		if (itemMods != null)
		{
			Debug.LogError("Item Definition Initializing twice: " + base.name);
		}
		skins = ItemSkinDirectory.ForItem(this);
		itemMods = GetComponentsInChildren<ItemMod>(includeInactive: true);
		ItemMod[] array = itemMods;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ModInit(this);
		}
		Children = itemList.Where((ItemDefinition x) => x.Parent == this).ToArray();
		ItemModWearable = GetComponent<ItemModWearable>();
		ItemModBurnable = GetComponent<ItemModBurnable>();
		ItemModCookable component = GetComponent<ItemModCookable>();
		if (component != null)
		{
			ItemModCookable = new CookableItemInfo(component);
		}
		ItemModEntity = GetComponent<ItemModEntity>();
		HasItemModEntity = ItemModEntity != null;
		ItemModSpriteConfig = GetComponent<ItemModSpriteConfig>();
		isHoldable = GetComponent<ItemModEntity>() != null;
		isUsable = GetComponent<ItemModEntity>() != null || GetComponent<ItemModConsume>() != null;
		ItemModCompostable component2 = GetComponent<ItemModCompostable>();
		if (component2 != null && component2.TotalFertilizerProduced > 0f)
		{
			ItemModCompostable = new CookableItemInfo(component2);
		}
	}

	public GameObjectRef GetWorldModel(int amount)
	{
		if (worldModelOverrides == null || worldModelOverrides.Length == 0)
		{
			return worldModelPrefab;
		}
		for (int num = worldModelOverrides.Length - 1; num >= 0; num--)
		{
			if (amount >= worldModelOverrides[num].minStackSize)
			{
				return worldModelOverrides[num].worldModel;
			}
		}
		return worldModelPrefab;
	}

	public int GetWorldModelIndex(int amount)
	{
		if (worldModelOverrides == null || worldModelOverrides.Length == 0)
		{
			return -1;
		}
		for (int num = worldModelOverrides.Length - 1; num >= 0; num--)
		{
			if (amount >= worldModelOverrides[num].minStackSize)
			{
				return num;
			}
		}
		return -1;
	}

	public bool SupportsItemOwnership()
	{
		if (stackable != 1)
		{
			if (supportsStackableOwnership)
			{
				return Inventory.stackable_item_ownership;
			}
			return false;
		}
		return true;
	}

	public bool Equals(ItemDefinition x, ItemDefinition y)
	{
		if ((object)x == null)
		{
			return false;
		}
		if ((object)y == null)
		{
			return false;
		}
		return x.itemid == y.itemid;
	}

	public int GetHashCode(ItemDefinition obj)
	{
		return obj.itemid;
	}

	public static Translate.Phrase GetCategoryLabel(ItemCategory category)
	{
		return category switch
		{
			ItemCategory.Weapon => Translate.GetPhrase("bp_weapons"), 
			ItemCategory.Attire => Translate.GetPhrase("bp_clothing"), 
			ItemCategory.Tool => Translate.GetPhrase("bp_tools"), 
			ItemCategory.Ammunition => Translate.GetPhrase("bp_ammo"), 
			ItemCategory.Misc => Translate.GetPhrase("bp_misc"), 
			_ => Translate.GetPhrase("bp_" + category.ToString().ToLower()), 
		};
	}

	public static Translate.Phrase GetVehicleCategoryLabel(VehicleCategory category)
	{
		if (category == VehicleCategory.Misc)
		{
			return Translate.GetPhrase("bp_misc");
		}
		return Translate.GetPhrase("vehicle." + category.ToString().ToLower());
	}
}

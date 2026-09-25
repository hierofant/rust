using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.UI;

public class LifeInfographic : SingletonComponent<LifeInfographic>, IPrefabPreProcess
{
	[Serializable]
	public struct DamageSetting
	{
		public DamageType ForType;

		public string Display;

		public Sprite DamageSprite;
	}

	[Serializable]
	public struct EntityNameToItemDefinition
	{
		public string Name;

		public ItemDefinition ItemDefinition;
	}

	[NonSerialized]
	public PlayerLifeStory life;

	public GameObject container;

	public RawImage AttackerAvatarImage;

	public Image DamageSourceImage;

	public LifeInfographicStat[] Stats;

	public Animator[] AllAnimators;

	public GameObject WeaponRoot;

	public GameObject DistanceRoot;

	public GameObject DistanceDivider;

	public Image WeaponImage;

	public DamageSetting[] DamageDisplays;

	public Texture2D defaultAvatarTexture;

	public bool ShowDebugData;

	[ReadOnly]
	[Tooltip("Automatically filled in by prefab preprocess")]
	public EntityNameToItemDefinition[] EntityNameToItemDefinitions;

	bool IPrefabPreProcess.CanRunDuringBundling => true;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (bundling || !FileSystem.IsBundled)
		{
			EntityNameToItemDefinitions = BuildEntityNameToItemDefinitionTable();
		}
	}

	private static EntityNameToItemDefinition[] BuildEntityNameToItemDefinitionTable()
	{
		List<ItemDefinition> list = GetItemList();
		Dictionary<string, ItemDefinition> dictionary = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
		foreach (ItemDefinition item in list)
		{
			dictionary.TryAdd(item.shortname, item);
			try
			{
				if (item.TryGetComponent<ItemModEntity>(out var component) && component.entityPrefab.isValid)
				{
					GameObject gameObject = component.entityPrefab.Get();
					dictionary.TryAdd(gameObject.name, item);
					if (gameObject.TryGetComponent<ThrownWeapon>(out var component2) && component2.prefabToThrow.isValid)
					{
						dictionary.TryAdd(component2.prefabToThrow.Get().name, item);
					}
				}
				if (item.TryGetComponent<ItemModDeployable>(out var component3) && component3.entityPrefab != null && component3.entityPrefab.isValid)
				{
					dictionary.TryAdd(component3.entityPrefab.Get().name, item);
				}
				ItemModProjectile[] components = item.GetComponents<ItemModProjectile>();
				foreach (ItemModProjectile itemModProjectile in components)
				{
					if (itemModProjectile.projectileObject != null && itemModProjectile.projectileObject.isValid)
					{
						dictionary.TryAdd(itemModProjectile.projectileObject.Get().name, item);
					}
					if (itemModProjectile.SkinOverrides == null)
					{
						continue;
					}
					ItemModProjectile.SkinOverride[] skinOverrides = itemModProjectile.SkinOverrides;
					for (int j = 0; j < skinOverrides.Length; j++)
					{
						ItemModProjectile.SkinOverride skinOverride = skinOverrides[j];
						if (skinOverride.OverrideProjectile.Get() != null)
						{
							dictionary.TryAdd(skinOverride.OverrideProjectile.Get().name, item);
						}
					}
				}
			}
			catch (Exception message)
			{
				Debug.LogError("Invalid entity found in LifeInfographic.PreProcess - likely a corrupt prefab: " + item.shortname);
				Debug.LogError(message);
			}
		}
		return dictionary.Select(delegate(KeyValuePair<string, ItemDefinition> kvp)
		{
			EntityNameToItemDefinition result = default(EntityNameToItemDefinition);
			result.Name = kvp.Key;
			result.ItemDefinition = kvp.Value;
			return result;
		}).OrderBy((EntityNameToItemDefinition x) => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
		static List<ItemDefinition> GetItemList()
		{
			return ItemManager.itemList;
		}
	}
}

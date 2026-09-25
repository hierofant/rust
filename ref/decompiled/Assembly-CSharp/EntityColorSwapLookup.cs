using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Entity Colour Swap Lookup")]
public class EntityColorSwapLookup : BaseScriptableObject
{
	[Serializable]
	public class EntityColorData
	{
		public BaseEntityRef entityPrefab;

		public string colorDatasetIdentifier;
	}

	[Serializable]
	public class ColorDataset
	{
		public string identifier;

		[InspectorName("Client ConVar")]
		public string clientConVar;

		public ColorDataEntry[] colorDataEntries = Array.Empty<ColorDataEntry>();
	}

	[Serializable]
	public class ColorDataEntry
	{
		public Translate.Phrase colorNamePhrase;

		public Translate.Phrase colorDescriptionPhrase;

		public Color materialColor;

		public Color radialSelectMenuColor;
	}

	private static EntityColorSwapLookup _instance;

	[SerializeField]
	private EntityColorData[] entityEntries = Array.Empty<EntityColorData>();

	[SerializeField]
	private ColorDataset[] colorDatasets = Array.Empty<ColorDataset>();

	private Dictionary<uint, ColorDataset> _entityColorDataLookup;

	private Dictionary<string, ColorDataset> _colorDatasetLookup;

	public static EntityColorSwapLookup instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<EntityColorSwapLookup>("assets/content/config/entitycolourswaplookup.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load EntityColorSwapLookup");
			}
			return _instance;
		}
	}

	private Dictionary<uint, ColorDataset> entityColorDataLookup
	{
		get
		{
			if (_entityColorDataLookup == null)
			{
				_entityColorDataLookup = new Dictionary<uint, ColorDataset>();
				EntityColorData[] array = entityEntries;
				foreach (EntityColorData entityColorData in array)
				{
					if (!entityColorData.entityPrefab.isValid)
					{
						continue;
					}
					BaseEntity baseEntity = entityColorData.entityPrefab.Get();
					if (!(baseEntity == null))
					{
						if (!colorDatasetLookup.TryGetValue(entityColorData.colorDatasetIdentifier, out var value))
						{
							Debug.LogError("Failed to retrieve a colour dataset for identifier: (" + entityColorData.colorDatasetIdentifier + ") for deployable prefab " + baseEntity.name + " in EntityColorSwapLookup", this);
						}
						else
						{
							_entityColorDataLookup.Add(baseEntity.prefabID, value);
						}
					}
				}
			}
			return _entityColorDataLookup;
		}
	}

	private Dictionary<string, ColorDataset> colorDatasetLookup
	{
		get
		{
			if (_colorDatasetLookup == null)
			{
				_colorDatasetLookup = new Dictionary<string, ColorDataset>();
				ColorDataset[] array = colorDatasets;
				foreach (ColorDataset colorDataset in array)
				{
					if (!_colorDatasetLookup.TryAdd(colorDataset.identifier, colorDataset))
					{
						Debug.LogError("Double color dataset identifier: (" + colorDataset.identifier + ") in EntityColorSwapLookup", this);
					}
				}
			}
			return _colorDatasetLookup;
		}
	}

	public bool EntityHasColorData(BaseEntity entity)
	{
		return entityColorDataLookup.ContainsKey(entity.prefabID);
	}

	public bool TryGetEntityColorDataset(BaseEntity decorDeployable, out ColorDataset colorDataset)
	{
		return entityColorDataLookup.TryGetValue(decorDeployable.prefabID, out colorDataset);
	}

	public bool TryGetColorDataset(string identifier, out ColorDataset colorDataset)
	{
		return colorDatasetLookup.TryGetValue(identifier, out colorDataset);
	}
}

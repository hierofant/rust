using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/RecyclerConfig")]
public class RecyclerConfig : BaseScriptableObject
{
	public enum RecyclerType
	{
		Green,
		Yellow,
		Red
	}

	[Serializable]
	public class RecyclerTypeConfig
	{
		public RecyclerType recyclerType;

		public int requiredPowergridStageToUse;

		public float efficiency;

		public float duration;

		public int efficiencyBuffPowergridStage;

		public float powergridEfficiency;

		public int durationBuffPowergridStage;

		public float powergridDuration;
	}

	private static RecyclerConfig _instance;

	public List<RecyclerTypeConfig> recyclerTypeConfigs = new List<RecyclerTypeConfig>();

	private Dictionary<RecyclerType, RecyclerTypeConfig> _entriesLookup;

	public static RecyclerConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<RecyclerConfig>("assets/prefabs/deployable/recycler/config/recyclerconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load RecyclerConfig");
			}
			return _instance;
		}
	}

	public Dictionary<RecyclerType, RecyclerTypeConfig> entriesLookup
	{
		get
		{
			if (_entriesLookup == null)
			{
				_entriesLookup = new Dictionary<RecyclerType, RecyclerTypeConfig>();
				for (int i = 0; i < recyclerTypeConfigs.Count; i++)
				{
					RecyclerTypeConfig recyclerTypeConfig = recyclerTypeConfigs[i];
					entriesLookup.Add(recyclerTypeConfig.recyclerType, recyclerTypeConfig);
				}
			}
			return _entriesLookup;
		}
	}

	public bool TryGetConfigForType(RecyclerType recyclerType, out RecyclerTypeConfig zoneConfig)
	{
		return entriesLookup.TryGetValue(recyclerType, out zoneConfig);
	}

	public RecyclerTypeConfig GetDefaultTypeConfig()
	{
		return entriesLookup[RecyclerType.Green];
	}
}

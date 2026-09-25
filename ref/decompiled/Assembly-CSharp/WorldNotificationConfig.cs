using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/WorldNotificationConfig")]
public class WorldNotificationConfig : BaseScriptableObject
{
	public enum NotificationType
	{
		Undefined,
		ExcavatorBegunMining,
		SmallOilRigReset,
		LargeOilRigReset,
		CargoShipSpawned,
		DeepSeaOpen,
		TravellingVendorSpawned,
		AirdropInbound,
		CargoHelicopterInbound,
		PatrolHelicopterInbound
	}

	[Serializable]
	public class Data
	{
		public NotificationType NotificationType;

		public SoundDefinition NotificationSound;

		public Translate.Phrase ToastMessage;
	}

	private static WorldNotificationConfig _instance;

	public List<Data> entries = new List<Data>();

	private Dictionary<NotificationType, Data> _entriesLookup;

	public static WorldNotificationConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<WorldNotificationConfig>("assets/content/world/worldnotificationconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load WorldNotificationConfig");
			}
			return _instance;
		}
	}

	public Dictionary<NotificationType, Data> entriesLookup
	{
		get
		{
			if (_entriesLookup == null)
			{
				_entriesLookup = new Dictionary<NotificationType, Data>();
				for (int i = 0; i < entries.Count; i++)
				{
					Data data = entries[i];
					entriesLookup.Add(data.NotificationType, data);
				}
			}
			return _entriesLookup;
		}
	}

	public bool TryGetDataForMonumentType(NotificationType notificationType, out Data data)
	{
		return entriesLookup.TryGetValue(notificationType, out data);
	}
}

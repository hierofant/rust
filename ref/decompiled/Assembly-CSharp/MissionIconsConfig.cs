using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/MissionIconsConfig")]
public class MissionIconsConfig : BaseScriptableObject
{
	public enum IconType
	{
		Undefined,
		Generic,
		NPC
	}

	[Serializable]
	public class Data
	{
		[FilteredEnum(0, 1)]
		public IconType iconType;

		public Sprite icon;
	}

	private static MissionIconsConfig _instance;

	public List<Data> entries = new List<Data>();

	public static MissionIconsConfig Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<MissionIconsConfig>("assets/prefabs/missions/missioniconsconfig/missioniconsconfig.asset");
			}
			if (_instance == null)
			{
				Debug.LogError("Failed to load MissionIconsConfig");
			}
			return _instance;
		}
	}

	public Sprite GetIcon(IconType iconType)
	{
		if (iconType == IconType.Undefined)
		{
			Debug.LogError("Icon type is undefined, unable to retrieve icon");
			return null;
		}
		for (int i = 0; i < entries.Count; i++)
		{
			Data data = entries[i];
			if (data != null && iconType == data.iconType)
			{
				return data.icon;
			}
		}
		Debug.LogError($"Failed to find icon for icon type {iconType}");
		return null;
	}
}

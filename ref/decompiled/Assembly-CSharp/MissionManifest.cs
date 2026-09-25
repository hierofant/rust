using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/MissionManifest")]
public class MissionManifest : ScriptableObject
{
	public ScriptableObjectRef[] missionList;

	public WorldPositionGenerator[] positionGenerators;

	public static MissionManifest instance;

	private static Dictionary<uint, BaseMission> idToMissionCache = new Dictionary<uint, BaseMission>();

	private static Dictionary<string, BaseMission> shortnameToMissionCache = new Dictionary<string, BaseMission>();

	public static MissionManifest Get()
	{
		using (TimeWarning.New("MissionManifest.Get"))
		{
			if (instance != null)
			{
				return instance;
			}
			using (TimeWarning.New("MissionManifest.Get.ResourcesLoadManifest"))
			{
				instance = Resources.Load<MissionManifest>("MissionManifest");
			}
			using (TimeWarning.New("MissionManifest.Get.PositionGeneratorLoop"))
			{
				WorldPositionGenerator[] array = instance.positionGenerators;
				foreach (WorldPositionGenerator worldPositionGenerator in array)
				{
					if (!(worldPositionGenerator == null))
					{
						WorldPositionGenerator.PrecalculatePositions(worldPositionGenerator);
					}
				}
			}
			return instance;
		}
	}

	public static BaseMission GetFromShortName(string shortname)
	{
		using (TimeWarning.New("MissionManifest.GetFromShortName"))
		{
			MissionManifest missionManifest = Get();
			if (missionManifest.missionList == null)
			{
				return null;
			}
			if (shortnameToMissionCache.TryGetValue(shortname, out var value))
			{
				return value;
			}
			ScriptableObjectRef[] array = missionManifest.missionList;
			foreach (ScriptableObjectRef scriptableObjectRef in array)
			{
				if (!(scriptableObjectRef.Get() is BaseMission baseMission))
				{
					Debug.LogError("Mission manifest is holding reference to a " + scriptableObjectRef.Get().GetType().Name + " with guid " + scriptableObjectRef.guid);
				}
				else if (baseMission.shortname == shortname)
				{
					shortnameToMissionCache[shortname] = baseMission;
					return baseMission;
				}
			}
			return null;
		}
	}

	public static BaseMission GetFromID(uint id)
	{
		using (TimeWarning.New("MissionManifest.GetFromID"))
		{
			MissionManifest missionManifest = Get();
			if (missionManifest.missionList == null)
			{
				return null;
			}
			if (idToMissionCache.TryGetValue(id, out var value))
			{
				return value;
			}
			ScriptableObjectRef[] array = missionManifest.missionList;
			foreach (ScriptableObjectRef scriptableObjectRef in array)
			{
				if (!(scriptableObjectRef.Get() is BaseMission baseMission))
				{
					Debug.LogError("Mission manifest is holding reference to a " + scriptableObjectRef.Get().GetType().Name + " with guid " + scriptableObjectRef.guid);
				}
				else if (baseMission.id == id)
				{
					idToMissionCache[id] = baseMission;
					return baseMission;
				}
			}
			return null;
		}
	}

	public static bool TryGetFromID(uint id, out BaseMission mission)
	{
		mission = GetFromID(id);
		return mission != null;
	}
}

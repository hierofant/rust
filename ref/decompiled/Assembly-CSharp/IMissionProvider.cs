using System;
using ConVar;
using UnityEngine;

public interface IMissionProvider
{
	NetworkableId ProviderID();

	Vector3 ProviderPosition();

	BaseEntity GetEntity();

	BufferList<BaseMission> GetAllMissions();

	bool TryGetMission(uint missionId, out BaseMission mission)
	{
		BufferList<BaseMission> allMissions = GetAllMissions();
		for (int i = 0; i < allMissions.Count; i++)
		{
			mission = allMissions[i];
			if (!(mission == null) && mission.id == missionId)
			{
				return true;
			}
		}
		mission = null;
		return false;
	}

	bool HasPlayerRecentlyCompletedMission(BasePlayer player)
	{
		for (int i = 0; i < player.acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = player.acceptedMissions[i];
			if (missionInstance.status == BaseMission.MissionStatus.Completed && missionInstance.providerID == ProviderID() && missionInstance.endTimeUtcSeconds != long.MinValue && (float)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - missionInstance.endTimeUtcSeconds) * ConVar.Time.missiontimerscale < 5f)
			{
				return true;
			}
		}
		return false;
	}

	bool Server_HasMissionAvailable(BasePlayer player);
}

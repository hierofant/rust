using UnityEngine;

public class MetalDetectorMissionFlag : MetalDetectorFlag
{
	public override void OnFullyDug(BasePlayer player)
	{
		if (player == null)
		{
			return;
		}
		if (player.TryGetActiveMissionInstance(out var instance))
		{
			BaseMission mission = instance.GetMission();
			int count = instance.objectiveStatuses.Count;
			int num = mission.objectives.Length;
			if (count != num)
			{
				Debug.LogError($"Mission instance for mission {mission.name} contains data for {count} objectives but mission has {num} objectives", mission);
				return;
			}
			for (int i = 0; i < instance.objectiveStatuses.Count; i++)
			{
				BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
				if (objectiveStatus.IsObjectiveActive() && !objectiveStatus.softCompleted && mission.objectives[i].objective is MissionObjective_MetalDetectorDeepSeaTreasure missionObjective_MetalDetectorDeepSeaTreasure && objectiveStatus.progressCurrent >= (float)missionObjective_MetalDetectorDeepSeaTreasure.minimumDigAttempts && (objectiveStatus.progressCurrent >= (float)missionObjective_MetalDetectorDeepSeaTreasure.maximumDigAttempts || Random.Range(0f, 1f) <= missionObjective_MetalDetectorDeepSeaTreasure.successfulDigChange))
				{
					if (Collision != null)
					{
						Collision.enabled = false;
					}
					BaseEntity baseEntity = GameManager.server.CreateEntity(missionObjective_MetalDetectorDeepSeaTreasure.treasurePrefab.resourcePath, base.transform.position, Quaternion.Euler(0f, Random.Range(0, 360), 0f));
					baseEntity.Spawn();
					if (baseEntity is SingleUseMissionStorageContainer singleUseMissionStorageContainer)
					{
						singleUseMissionStorageContainer.PermitUserId(player.userID.Get());
					}
					instance.persistentMissionEntities.Add(baseEntity);
					BaseMission.MissionEventPayload missionEventPayload = default(BaseMission.MissionEventPayload);
					missionEventPayload.NetworkIdentifier = baseEntity.net.ID;
					missionEventPayload.UintIdentifier = baseEntity.prefabID;
					missionEventPayload.WorldPosition = base.transform.position;
					BaseMission.MissionEventPayload payload = missionEventPayload;
					player.ProcessMissionEvent(BaseMission.MissionEventType.METAL_DETECTOR_FIND, payload, 1f);
					return;
				}
			}
		}
		base.OnFullyDug(player);
	}
}

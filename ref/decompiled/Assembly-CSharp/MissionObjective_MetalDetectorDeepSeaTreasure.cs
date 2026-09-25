using UnityEngine;

public class MissionObjective_MetalDetectorDeepSeaTreasure : MissionObjective
{
	public GameObjectRef treasurePrefab;

	public Translate.Phrase notePhrase;

	[Min(0f)]
	[Tooltip("Player must have dug this many metal detector sources already for the treasure to spawn.")]
	public int minimumDigAttempts;

	[Min(0f)]
	[Tooltip("After this many dug up metal detector sources treasure spawn is guaranteed.")]
	public int maximumDigAttempts;

	[Tooltip("Random chance of treasure spawning after Minimum Dig Attempts.")]
	[Range(0f, 1f)]
	public float successfulDigChange = 0.33f;

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			return false;
		}
		for (int i = 0; i < DeepSeaManager.ServerIslands.Count; i++)
		{
			if (DeepSeaManager.ServerIslands[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			Debug.LogError("Mission instance for " + instance.GetMission().name + " failed to retrieve server instance for DeepSeaManager", instance.GetMission());
		}
		else
		{
			SetObjectiveWorldLocation(index, instance, DeepSeaManager.DeepSeaBounds.center);
		}
	}

	public override void PostServerLoad(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.PostServerLoad(index, instance, forPlayer);
		if (!instance.objectiveStatuses[index].softCompleted)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < instance.persistentMissionEntities.Count; i++)
		{
			BaseEntity baseEntity = instance.persistentMissionEntities[i];
			if (baseEntity.IsValid() && baseEntity.prefabID == treasurePrefab.resourceID && baseEntity is SingleUseMissionStorageContainer singleUseMissionStorageContainer && singleUseMissionStorageContainer.PermittedUserId == forPlayer.userID.Get())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			ClearSoftCompletedStatus(index, instance, forPlayer);
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (!IsObjectiveActive(index, instance))
		{
			return;
		}
		switch (type)
		{
		case BaseMission.MissionEventType.KILL_ENTITY:
			if (payload.UintIdentifier == treasurePrefab.resourceID)
			{
				ClearSoftCompletedStatus(index, instance, playerFor);
			}
			break;
		case BaseMission.MissionEventType.METAL_DETECTOR_FIND:
			if (payload.UintIdentifier == treasurePrefab.resourceID)
			{
				instance.objectiveStatuses[index].progressCurrent = float.PositiveInfinity;
				SoftCompleteObjective(index, instance, playerFor);
			}
			else if (DeepSeaManager.IsInsideDeepSea(payload.WorldPosition) && !instance.objectiveStatuses[index].progressCurrent.IsInfinity())
			{
				instance.objectiveStatuses[index].progressCurrent += 1f;
			}
			break;
		case BaseMission.MissionEventType.OPEN_STORAGE:
			if (payload.UintIdentifier == treasurePrefab.resourceID)
			{
				CompleteObjective(index, instance, playerFor);
			}
			break;
		}
	}
}

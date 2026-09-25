using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/MountMissionEntity")]
public class MissionObjective_MountMissionEntity : MissionObjective
{
	public string targetIdentifier;

	public bool shouldUpdateMissionLocation = true;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.GetSpawnedMissionEntity(targetIdentifier, playerFor);
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.MOUNT_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		MissionEntity spawnedMissionEntity = instance.GetSpawnedMissionEntity(targetIdentifier, playerFor);
		if (spawnedMissionEntity == null)
		{
			FailObjective(index, instance, playerFor);
			return;
		}
		BaseEntity entity = spawnedMissionEntity.GetEntity();
		if (!entity.IsValid())
		{
			FailObjective(index, instance, playerFor);
			return;
		}
		EntityRef<BaseMountable> entityRef = default(EntityRef<BaseMountable>);
		entityRef.uid = payload.NetworkIdentifier;
		EntityRef<BaseMountable> entityRef2 = entityRef;
		BaseMountable baseMountable = entityRef2.Get(serverside: true);
		if (baseMountable.IsValid())
		{
			BaseVehicle baseVehicle = baseMountable.VehicleParent();
			if (baseMountable.EqualNetID(entity) || (baseVehicle != null && baseVehicle.EqualNetID(entity)))
			{
				CompleteObjective(index, instance, playerFor);
			}
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (!IsStarted(index, instance) || IsCompleted(index, instance))
		{
			return;
		}
		ref RealTimeSince sinceLastThink = ref instance.objectiveStatuses[index].sinceLastThink;
		if ((float)sinceLastThink < 1f)
		{
			return;
		}
		sinceLastThink = 0f;
		MissionEntity spawnedMissionEntity = instance.GetSpawnedMissionEntity(targetIdentifier, assignee);
		if (spawnedMissionEntity == null)
		{
			FailObjective(index, instance, assignee);
		}
		else if (shouldUpdateMissionLocation)
		{
			Vector3 position = spawnedMissionEntity.transform.position;
			if (position != GetObjectiveWorldLocation(index, instance))
			{
				SetObjectiveWorldLocation(index, instance, position);
				assignee.MissionsDirty();
			}
		}
	}
}

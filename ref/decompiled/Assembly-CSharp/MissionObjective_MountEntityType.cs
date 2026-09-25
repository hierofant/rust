using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/MountEntityType")]
public class MissionObjective_MountEntityType : MissionObjective
{
	public BaseEntityRef[] targetEntities;

	public LayerMask targetLayerMask = -1;

	public int numToMount = 1;

	public bool shouldUpdateMissionLocation = true;

	private bool isInitialized;

	private readonly ListHashSet<uint> targetPrefabIDs = new ListHashSet<uint>();

	private void EnsureInitialized()
	{
		if (isInitialized)
		{
			return;
		}
		BaseEntityRef[] array = targetEntities;
		foreach (BaseEntityRef baseEntityRef in array)
		{
			if (baseEntityRef.isValid)
			{
				targetPrefabIDs.TryAdd(baseEntityRef.Get().prefabID);
			}
		}
		isInitialized = true;
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		return targetPrefabIDs.Contains(entity.prefabID);
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		if (numToMount > 1)
		{
			instance.objectiveStatuses[index].progressTarget = numToMount;
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.MOUNT_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		EnsureInitialized();
		EntityRef<BaseMountable> entityRef = default(EntityRef<BaseMountable>);
		entityRef.uid = payload.NetworkIdentifier;
		EntityRef<BaseMountable> entityRef2 = entityRef;
		BaseMountable baseMountable = entityRef2.Get(serverside: true);
		if (!baseMountable.IsValid())
		{
			return;
		}
		for (int i = 0; i < targetPrefabIDs.Count; i++)
		{
			uint num = targetPrefabIDs[i];
			BaseVehicle baseVehicle = baseMountable.VehicleParent();
			if (num == baseMountable.prefabID || (!(baseVehicle == null) && num == baseVehicle.prefabID))
			{
				instance.objectiveStatuses[index].progressCurrent += (int)amount;
				if (instance.objectiveStatuses[index].progressCurrent >= (float)numToMount)
				{
					CompleteObjective(index, instance, playerFor);
				}
				playerFor.MissionsDirty(saveImmediately: true);
				break;
			}
		}
	}
}

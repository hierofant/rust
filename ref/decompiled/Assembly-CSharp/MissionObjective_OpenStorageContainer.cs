using UnityEngine;

public class MissionObjective_OpenStorageContainer : MissionObjective
{
	public BaseEntity TargetEntity;

	[Tooltip("If set, the objective world location will be set to this position.")]
	[BaseMission.PositionGenerator.PositionPoint]
	public string SetObjectiveLocation;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("The opened container must be nearby this mission point for the objective to complete.")]
	public string RequireProximityToPosition;

	[Tooltip("If RequireProximityToPosition is set, this defines the minimum proximity between the opened storage container and the mission point.")]
	[Min(0f)]
	public float MinimumDistanceToMissionPoint;

	private float sqrDistanceToMissionPoint;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceToMissionPoint = MinimumDistanceToMissionPoint * MinimumDistanceToMissionPoint;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		if (!string.IsNullOrWhiteSpace(SetObjectiveLocation))
		{
			if (instance.TryGetMissionPoint(SetObjectiveLocation, out var point))
			{
				SetObjectiveWorldLocation(index, instance, point);
				playerFor.MissionsDirty();
				return;
			}
			Debug.LogError("Objective " + base.name + " on mission " + instance.GetMission().name + " failed to find an objective location for identifier " + SetObjectiveLocation, instance.GetMission());
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.OPEN_STORAGE || IsCompleted(index, instance) || !CanProgress(index, instance) || TargetEntity.prefabID != payload.UintIdentifier)
		{
			return;
		}
		if (!BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			Debug.LogError($"Failed to find {payload.NetworkIdentifier} in server entities", this);
			return;
		}
		if (!string.IsNullOrWhiteSpace(RequireProximityToPosition))
		{
			instance.TryGetMissionPoint(RequireProximityToPosition, out var point);
			if (Vector3.SqrMagnitude(point - entity.transform.position) > sqrDistanceToMissionPoint)
			{
				return;
			}
		}
		CompleteObjective(index, instance, playerFor);
	}
}

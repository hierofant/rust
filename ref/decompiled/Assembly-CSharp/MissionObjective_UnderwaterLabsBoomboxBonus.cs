using UnityEngine;

public class MissionObjective_UnderwaterLabsBoomboxBonus : MissionObjective
{
	public string targetRadioIp;

	[BaseMission.PositionGenerator.PositionPoint]
	[Tooltip("The boombox must be nearby this mission point for the objective to complete.")]
	public string requireProximityToPosition;

	[Min(0f)]
	[Tooltip("If RequireProximityToPosition is set, this defines the minimum proximity between the boombox and the mission point.")]
	public float minimumDistanceToMissionPoint;

	public bool shouldHideCompassMarkerWhenClose;

	[Min(0f)]
	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	public float hideCompassMarkerDistance = 50f;

	private float sqrDistanceToHideCompassMarker;

	public float sqrMinimumDistanceToMissionPoint { get; private set; }

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrMinimumDistanceToMissionPoint = minimumDistanceToMissionPoint * minimumDistanceToMissionPoint;
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(requireProximityToPosition, out var point);
		SetObjectiveWorldLocation(index, instance, point);
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		int num = 0;
		for (int i = 0; i < DeployableBoomBox.ServerStaticInstances.Count; i++)
		{
			DeployableBoomBox deployableBoomBox = DeployableBoomBox.ServerStaticInstances[i];
			if (deployableBoomBox == null)
			{
				Debug.LogWarning(string.Format("Null boombox at index {0} in {1}", i, "ServerStaticInstances"));
			}
			else if (IsBoomboxPositionValid(deployableBoomBox.transform.position, forPlayer, instance))
			{
				num++;
			}
		}
		int num2 = num - 1;
		if (num2 <= 0)
		{
			CompleteObjective(index, instance, forPlayer);
			return;
		}
		instance.objectiveStatuses[index].progressTarget = 0f;
		instance.objectiveStatuses[index].progressTarget = num2;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.PLAY_BOOMBOX || !Mathf.Approximately(amount, 1f) || IsCompleted(index, instance) || !CanProgress(index, instance) || string.IsNullOrWhiteSpace(payload.StringIdentifier) || payload.StringIdentifier != targetRadioIp || !IsBoomboxPositionValid(payload.WorldPosition, playerFor, instance))
		{
			return;
		}
		if (!BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			Debug.LogError("Failed to find a server entity with network ID " + payload.NetworkIdentifier.ToString());
			return;
		}
		for (int i = 0; i < instance.persistentMissionEntities.Count; i++)
		{
			if (instance.persistentMissionEntities[i] == entity)
			{
				return;
			}
		}
		instance.persistentMissionEntities.Add(entity);
		BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[index];
		objectiveStatus.progressCurrent += 1f;
		if (objectiveStatus.progressCurrent >= objectiveStatus.progressTarget)
		{
			CompleteObjective(index, instance, playerFor);
		}
		playerFor.MissionsDirty(saveImmediately: true);
	}

	private bool IsBoomboxPositionValid(Vector3 boomboxWorldPosition, BasePlayer playerFor, BaseMission.MissionInstance instance)
	{
		if (!string.IsNullOrWhiteSpace(requireProximityToPosition) && (!instance.TryGetMissionPoint(requireProximityToPosition, out var point) || Vector3.SqrMagnitude(point - boomboxWorldPosition) > sqrMinimumDistanceToMissionPoint))
		{
			return false;
		}
		if (!EnvironmentManager.Check(boomboxWorldPosition, EnvironmentType.UnderwaterLab))
		{
			return false;
		}
		return true;
	}
}

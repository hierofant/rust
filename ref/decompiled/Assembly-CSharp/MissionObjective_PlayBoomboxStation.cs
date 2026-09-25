using UnityEngine;

public class MissionObjective_PlayBoomboxStation : MissionObjective
{
	public string targetRadioIp;

	[Tooltip("If true, the boombox must be a static environment boombox for this objective to complete.")]
	public bool boomboxMustBeStatic;

	[Tooltip("The boombox must be nearby this mission point for the objective to complete.")]
	[BaseMission.PositionGenerator.PositionPoint]
	public string requireProximityToPosition;

	[Min(0f)]
	[Tooltip("If RequireProximityToPosition is set, this defines the minimum proximity between the boombox and the mission point.")]
	public float minimumDistanceToMissionPoint;

	public bool shouldHideCompassMarkerWhenClose;

	[Min(0f)]
	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	public float hideCompassMarkerDistance = 50f;

	private float sqrDistanceToMissionPoint;

	private float sqrDistanceToHideCompassMarker;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceToMissionPoint = minimumDistanceToMissionPoint * minimumDistanceToMissionPoint;
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(requireProximityToPosition, out var point);
		SetObjectiveWorldLocation(index, instance, point);
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.PLAY_BOOMBOX || IsCompleted(index, instance) || !CanProgress(index, instance) || (boomboxMustBeStatic && !Mathf.Approximately(1f, amount)) || string.IsNullOrWhiteSpace(payload.StringIdentifier) || payload.StringIdentifier != targetRadioIp)
		{
			return;
		}
		if (!string.IsNullOrWhiteSpace(requireProximityToPosition))
		{
			instance.TryGetMissionPoint(requireProximityToPosition, out var point);
			if (Vector3.SqrMagnitude(point - payload.WorldPosition) > sqrDistanceToMissionPoint)
			{
				return;
			}
		}
		if (BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			instance.persistentMissionEntities.Add(entity);
		}
		else
		{
			Debug.LogError("Failed to find a server entity with network ID " + payload.NetworkIdentifier.ToString());
		}
		CompleteObjective(index, instance, playerFor);
	}
}

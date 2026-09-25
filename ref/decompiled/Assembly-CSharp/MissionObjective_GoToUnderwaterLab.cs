using UnityEngine;

public class MissionObjective_GoToUnderwaterLab : MissionObjective
{
	[BaseMission.PositionGenerator.PositionPoint]
	public string position;

	[Tooltip("Player must be within underwater labs environment volume and within this distance of the mission point for the objective to complete.")]
	[Min(0f)]
	public float minimumDistanceToPosition = 100f;

	public bool shouldHideCompassMarkerWhenClose;

	[Min(0f)]
	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	public float hideCompassMarkerDistance = 50f;

	private float sqrMinimumDistanceToPosition;

	private float sqrDistanceToHideCompassMarker;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrMinimumDistanceToPosition = minimumDistanceToPosition * minimumDistanceToPosition;
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		instance.TryGetMissionPoint(position, out var point);
		SetObjectiveWorldLocation(index, instance, point);
		playerFor.MissionsDirty();
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		float num = Vector3.SqrMagnitude(GetObjectiveWorldLocation(index, instance) - assignee.transform.position);
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = num < sqrMinimumDistanceToPosition && EnvironmentManager.Check(assignee.transform.position, EnvironmentType.UnderwaterLab);
		if (completed != flag)
		{
			if (flag)
			{
				CompleteObjective(index, instance, assignee);
			}
			else
			{
				ResetObjective(index, instance, assignee);
			}
		}
	}
}

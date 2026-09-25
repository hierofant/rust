using UnityEngine;

public class MissionObjective_GoToDeepSea : MissionObjective
{
	[InspectorName("Maximum Time To Wipe (s)")]
	[Tooltip("Time in seconds from deep sea wipe in which this objective will be considered as valid. If current time to wipe is less than this, then the mission cannot be started. If value <= 0, then this value is ignored.")]
	public int maximumTimeToDeepSeaWipe;

	[Tooltip("Should fail mission when deep sea closes.")]
	public bool shouldFailMissionWhenDeepSeaCloses;

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance != null && PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			if (maximumTimeToDeepSeaWipe > 0)
			{
				return PointEntity<DeepSeaManager>.ServerInstance.GetTimeToWipe() > (float)maximumTimeToDeepSeaWipe;
			}
			return true;
		}
		return false;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			Debug.LogError("Mission instance for " + instance.GetMission().name + " failed to retrieve server instance for DeepSeaManager", instance.GetMission());
			return;
		}
		SetObjectiveWorldLocation(index, instance, DeepSeaManager.DeepSeaBounds.center);
		playerFor.MissionsDirty();
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			return;
		}
		if (shouldFailMissionWhenDeepSeaCloses && !PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			FailMission(instance, assignee, BaseMission.MissionFailReason.DeepSeaClosed);
		}
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = DeepSeaManager.IsInsideDeepSea(assignee);
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

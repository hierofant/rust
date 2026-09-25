using UnityEngine;

public class MissionObjective_GoToDeepSeaIsland : MissionObjective
{
	private const float ISLAND_PLAYER_DISTANCE_THRESHOLD = 150f;

	private const float ISLAND_PLAYER_DISTANCE_THRESHOLD_SQR = 22500f;

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if (PointEntity<DeepSeaManager>.ServerInstance == null)
		{
			return false;
		}
		if (!PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
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

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (DeepSeaManager.IsInsideDeepSea(assignee))
		{
			for (int i = 0; i < DeepSeaManager.ServerIslands.Count; i++)
			{
				DeepSeaIsland deepSeaIsland = DeepSeaManager.ServerIslands[i];
				if (!(deepSeaIsland == null) && !(Vector3.SqrMagnitude(deepSeaIsland.transform.position - assignee.transform.position) > 22500f))
				{
					flag2 = true;
					if (assignee.IsStandingOnEntity(deepSeaIsland, 8388608))
					{
						flag = true;
						break;
					}
				}
			}
		}
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag3 = flag;
		if (completed != flag3)
		{
			if (flag3)
			{
				CompleteObjective(index, instance, assignee);
			}
			else if (!flag2)
			{
				ResetObjective(index, instance, assignee);
			}
		}
	}
}

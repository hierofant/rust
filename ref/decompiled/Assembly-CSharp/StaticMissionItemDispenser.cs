using UnityEngine;

public class StaticMissionItemDispenser : StorageContainer
{
	private const float MINIMUM_PROXIMITY_TO_OBJECTIVE = 3f;

	private const float MINIMUM_PROXIMITY_TO_OBJECTIVE_SQR = 9f;

	public override void ServerInit()
	{
		base.ServerInit();
		if (base.inventory != null)
		{
			base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (base.CanBeLooted(player))
		{
			return HasValidMission(player);
		}
		return false;
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (player.TryGetActiveMissionInstance(out var instance))
		{
			BaseMission mission = instance.GetMission();
			int count = instance.objectiveStatuses.Count;
			int num = mission.objectives.Length;
			if (count != num)
			{
				Debug.LogError($"Mission instance for mission {mission.name} contains data for {count} objectives but mission has {num} objectives", mission);
				return false;
			}
			for (int i = 0; i < instance.objectiveStatuses.Count; i++)
			{
				if (instance.objectiveStatuses[i].IsObjectiveActive() && mission.objectives[i].objective is MissionObjective_AcquireItem missionObjective_AcquireItem)
				{
					base.inventory.AddItem(missionObjective_AcquireItem.targetItem, missionObjective_AcquireItem.targetItemAmount, 0uL);
				}
			}
		}
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.inventory.Clear();
		base.PlayerStoppedLooting(player);
	}

	private bool HasValidMission(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (!player.TryGetActiveMissionInstance(out var instance))
		{
			return false;
		}
		BaseMission mission = instance.GetMission();
		int count = instance.objectiveStatuses.Count;
		int num = mission.objectives.Length;
		if (count != num)
		{
			Debug.LogError($"Mission instance for mission {mission.name} contains data for {count} objectives but mission has {num} objectives", mission);
			return false;
		}
		for (int i = 0; i < instance.objectiveStatuses.Count; i++)
		{
			BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[i];
			if (objectiveStatus.IsObjectiveActive() && !objectiveStatus.softCompleted && mission.objectives[i].Get() is MissionObjective_AcquireItem missionObjective_AcquireItem)
			{
				if (!string.IsNullOrEmpty(missionObjective_AcquireItem.position) && base.isServer && (!instance.missionPoints.TryGetValue(missionObjective_AcquireItem.position, out var value) || Vector3.SqrMagnitude(value - base.transform.position) > 9f))
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}
}

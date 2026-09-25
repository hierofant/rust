using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/HaveItem")]
public class MissionObjective_HaveItem : MissionObjective
{
	[ItemSelector]
	public ItemDefinition targetItem;

	public int targetItemAmount;

	[Tooltip("If true, this objective will no longer be marked as completed if the objective criteria are no longer met.")]
	public bool canBeReset = true;

	[Tooltip("If true and canBeReset is enabled, then all objectives after this one will also be reset if this objective criteria is no longer met.")]
	public bool resetFollowupObjectives = true;

	[Header("Tutorial")]
	public BaseEntityRef[] pingEntitiesOnTutorialIsland;

	public BasePlayer.PingType pingType = BasePlayer.PingType.GoTo;

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		instance.objectiveStatuses[index].progressTarget = targetItemAmount;
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (!CanProgress(index, instance) || targetItem == null || (IsCompleted(index, instance) && canBeReset && instance.objectiveStatuses[index].blockReset) || (IsCompleted(index, instance) && !canBeReset))
		{
			return;
		}
		int amount = assignee.inventory.GetAmount(targetItem.itemid);
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = amount >= targetItemAmount;
		if (completed != flag)
		{
			if (flag)
			{
				CompleteObjective(index, instance, assignee);
			}
			else
			{
				ResetObjective(index, instance, assignee);
				if (resetFollowupObjectives)
				{
					ResetFollowupObjectives(index, instance, assignee, resetStartedStatus: true);
				}
			}
		}
		if (amount != (int)instance.objectiveStatuses[index].progressCurrent)
		{
			instance.objectiveStatuses[index].progressCurrent = amount;
			assignee.MissionsDirty();
		}
	}
}

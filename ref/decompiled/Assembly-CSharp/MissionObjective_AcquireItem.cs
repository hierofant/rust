using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/AcquireItem")]
public class MissionObjective_AcquireItem : MissionObjective
{
	[ItemSelector]
	public ItemDefinition targetItem;

	public int targetItemAmount;

	public bool allowStackEvents;

	public bool showResourcePings;

	public bool acceptExistingItems;

	[BaseMission.PositionGenerator.PositionPoint]
	[FormerlySerializedAs("requireProximityToPosition")]
	public string position;

	[Tooltip("If true, objective progress will reset upon no longer having the required item.")]
	public bool canBeReset;

	public bool shouldHideCompassMarkerWhenClose;

	[Min(0f)]
	[Tooltip("If \"Should Hide Compass Marker When Close\" is enabled and \"Require Proximity To Position\" is set and player is within this distance of the mission point then hide the compass marker, else the compass marker is visible.")]
	public float hideCompassMarkerDistance = 20f;

	private float sqrDistanceToHideCompassMarker;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceToHideCompassMarker = hideCompassMarkerDistance * hideCompassMarkerDistance;
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		instance.objectiveStatuses[index].progressTarget = targetItemAmount;
		if (showResourcePings && forPlayer.IsInTutorial)
		{
			forPlayer.EnableResourcePings(targetItem, BasePlayer.PingType.GoTo);
		}
		if (!string.IsNullOrWhiteSpace(position) && instance.TryGetMissionPoint(position, out var point))
		{
			SetObjectiveWorldLocation(index, instance, point);
		}
		if (acceptExistingItems)
		{
			int amount = forPlayer.inventory.GetAmount(targetItem);
			if (amount > 0)
			{
				ProcessMissionEvent(forPlayer, instance, index, BaseMission.MissionEventType.ACQUIRE_ITEM, new BaseMission.MissionEventPayload
				{
					IntIdentifier = targetItem.itemid
				}, amount);
			}
		}
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (((type != BaseMission.MissionEventType.ACQUITE_ITEM_STACK || !allowStackEvents) && type != BaseMission.MissionEventType.ACQUIRE_ITEM) || IsCompleted(index, instance) || !CanProgress(index, instance) || targetItem.itemid != payload.IntIdentifier)
		{
			return;
		}
		instance.objectiveStatuses[index].progressCurrent += (int)amount;
		if (instance.objectiveStatuses[index].progressCurrent >= (float)targetItemAmount)
		{
			CompleteObjective(index, instance, playerFor);
			if (canBeReset)
			{
				instance.objectiveStatuses[index].softCompleted = true;
			}
		}
		if (showResourcePings)
		{
			playerFor.DisableResourcePings(targetItem, BasePlayer.PingType.GoTo);
		}
		playerFor.MissionsDirty(saveImmediately: true);
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float timeSinceLastThink)
	{
		base.DoServerThink(index, instance, assignee, timeSinceLastThink);
		if (canBeReset && CanProgress(index, instance) && !(targetItem == null) && (!IsCompleted(index, instance) || !instance.objectiveStatuses[index].blockReset))
		{
			int amount = assignee.inventory.GetAmount(targetItem.itemid);
			bool completed = instance.objectiveStatuses[index].completed;
			bool flag = amount >= targetItemAmount;
			if (completed && !flag)
			{
				ResetObjective(index, instance, assignee, resetStartedStatus: false, resetSoftCompletedStatus: false);
			}
		}
	}
}

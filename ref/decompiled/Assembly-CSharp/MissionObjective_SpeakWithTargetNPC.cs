using System;
using ConVar;
using UnityEngine;

public class MissionObjective_SpeakWithTargetNPC : MissionObjective
{
	public BaseEntityRef TargetNPC;

	public LayerMask targetLayerMask = -1;

	public ItemAmount[] requiredReturnItems = Array.Empty<ItemAmount>();

	[Tooltip("The target NPC must be nearby this mission point for the objective to complete.")]
	[BaseMission.PositionGenerator.PositionPoint]
	public string RequireProximityToPosition;

	[Min(0f)]
	[Tooltip("This defines the minimum proximity between the target NPC and the mission point.")]
	public float MinimumDistanceToMissionPoint;

	public bool destroyReturnItems;

	public bool showPing;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		Vector3 value;
		NPCPlayer entity;
		if (string.IsNullOrWhiteSpace(RequireProximityToPosition))
		{
			Debug.LogError("RequireProximityToPosition is not set on objective " + base.name, this);
		}
		else if (!instance.missionPoints.TryGetValue(RequireProximityToPosition, out value))
		{
			Debug.LogError("No mission point found for " + RequireProximityToPosition + " on objective " + base.name, this);
		}
		else if (TryFindNearby<NPCPlayer>(value, targetLayerMask, out entity, 100f))
		{
			SetObjectiveWorldLocation(index, instance, entity.transform.position);
			if (showPing && playerFor.IsInTutorial)
			{
				playerFor.RegisterPingedEntity(entity, PingType);
			}
		}
		else
		{
			Debug.LogError("Failed to find an entity for " + TargetNPC.resourcePath + " on objective " + base.name, this);
		}
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if (!TargetNPC.isValid)
		{
			return false;
		}
		BaseEntity baseEntity = TargetNPC.Get();
		if (baseEntity == null)
		{
			return false;
		}
		return baseEntity.prefabID == entity.prefabID;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.CONVERSATION)
		{
			return;
		}
		BaseMission mission = instance.GetMission();
		if (mission == null)
		{
			Debug.LogError($"Failed to retrieve mission from mission instance ID {instance.missionID}");
			return;
		}
		if (Debugging.printMissionSpeakInfo)
		{
			Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} instance IsCompleted:{IsCompleted(index, instance)} CanProgress:{CanProgress(index, instance)}, amount: {amount}");
		}
		if (IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		if (!BaseNetworkable.serverEntities.TryGetEntity(payload.NetworkIdentifier, out var entity))
		{
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} failed to find a entity from payload NetworkIdentifier: {payload.NetworkIdentifier}");
			}
			return;
		}
		if (entity.prefabID != TargetNPC.Get().prefabID)
		{
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} entity {entity.name} provided from payload NetworkIdentifier: {payload.NetworkIdentifier} has prefabID: {entity.prefabID} which does not match target prefabID: {TargetNPC.Get().prefabID}", entity);
			}
			return;
		}
		if (!string.IsNullOrWhiteSpace(RequireProximityToPosition))
		{
			if (!instance.missionPoints.TryGetValue(RequireProximityToPosition, out var value))
			{
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log("[MissionSpeakInfo] objective " + base.name + " on " + mission.name + " failed to find mission point for " + RequireProximityToPosition);
				}
				return;
			}
			float num = Vector3.SqrMagnitude(value - entity.transform.position);
			float num2 = MinimumDistanceToMissionPoint * MinimumDistanceToMissionPoint;
			if (num > num2)
			{
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} entity {entity.name} provided from payload NetworkIdentifier: {payload.NetworkIdentifier} is {num} square distance away from point {value}, minimum square distance is {num2}", entity);
				}
				return;
			}
		}
		if (Mathf.Approximately(amount, 1f))
		{
			bool flag = true;
			ItemAmount[] array = requiredReturnItems;
			foreach (ItemAmount itemAmount in array)
			{
				if ((float)playerFor.inventory.GetAmount(itemAmount.itemDef.itemid) < itemAmount.amount)
				{
					flag = false;
					break;
				}
			}
			if (mission.HasRewards())
			{
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} CheckRewardsSpace: {playerFor.HasSpaceForMissionRewards(instance)}");
				}
				if (flag && destroyReturnItems)
				{
					if (!playerFor.HasSpaceForMissionRewards(instance, showToastOnFailure: true))
					{
						return;
					}
					array = requiredReturnItems;
					foreach (ItemAmount itemAmount2 in array)
					{
						playerFor.inventory.Take(null, itemAmount2.itemDef.itemid, (int)itemAmount2.amount);
					}
				}
				if (Debugging.printMissionSpeakInfo)
				{
					Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} CheckRewardsSpace: {playerFor.HasSpaceForMissionRewards(instance)}");
				}
				if (!playerFor.HasSpaceForMissionRewards(instance, showToastOnFailure: true))
				{
					return;
				}
			}
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} requiredReturnItems == null: {requiredReturnItems == null}, requiredReturnItems.Length: {requiredReturnItems.Length}, hasAllReturnItems: {flag}");
			}
			if (requiredReturnItems == null || requiredReturnItems.Length == 0 || flag)
			{
				CompleteObjective(index, instance, playerFor);
			}
		}
		else if (Debugging.printMissionSpeakInfo)
		{
			Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} supplied amount {amount} is not approximately 1f");
		}
	}

	public override void ObjectiveCompleted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ObjectiveCompleted(playerFor, index, instance);
		if (showPing)
		{
			DeregisterPing(playerFor, instance);
		}
	}

	private static void DeregisterPing(BasePlayer playerFor, BaseMission.MissionInstance instance)
	{
		IMissionProvider missionProvider = instance.GetMissionProvider();
		if (missionProvider != null)
		{
			playerFor.DeregisterPingedEntity(missionProvider.ProviderID(), BasePlayer.PingType.GoTo);
		}
	}

	public override void ObjectiveFailed(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ObjectiveFailed(playerFor, index, instance);
		if (showPing)
		{
			DeregisterPing(playerFor, instance);
		}
	}
}

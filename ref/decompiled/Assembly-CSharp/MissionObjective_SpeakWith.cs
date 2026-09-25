using System;
using ConVar;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/SpeakWith")]
public class MissionObjective_SpeakWith : MissionObjective
{
	public ItemAmount[] requiredReturnItems = Array.Empty<ItemAmount>();

	public bool destroyReturnItems;

	public bool showPing;

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		IMissionProvider missionProvider = instance.GetMissionProvider();
		if (missionProvider != null)
		{
			SetObjectiveWorldLocation(index, instance, missionProvider.ProviderPosition());
			if (showPing && playerFor.IsInTutorial)
			{
				playerFor.RegisterPingedEntity(missionProvider.GetEntity(), BasePlayer.PingType.GoTo);
			}
			playerFor.MissionsDirty();
		}
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
		IMissionProvider missionProvider = instance.GetMissionProvider();
		if (missionProvider == null)
		{
			if (Debugging.printMissionSpeakInfo)
			{
				Debug.Log("[MissionSpeakInfo] objective " + base.name + " on " + mission.name + " failed to find a provider entity attached to this mission instance");
			}
			return;
		}
		if (Debugging.printMissionSpeakInfo)
		{
			Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} looking for provider: {instance.providerID.Value}/{missionProvider.GetEntity().name} Supplied NPC:{payload.NetworkIdentifier}");
		}
		if (missionProvider.ProviderID() == payload.NetworkIdentifier && Mathf.Approximately(amount, 1f))
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
			if (missionProvider.ProviderID() != payload.NetworkIdentifier)
			{
				Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} failed to match supplied network ID {payload.NetworkIdentifier} with instance missionProvider.ProviderID(): {missionProvider.ProviderID()}. ProviderID() should match instance.providerID: {instance.providerID}");
			}
			if (!Mathf.Approximately(amount, 1f))
			{
				Debug.Log($"[MissionSpeakInfo] objective {base.name} on {mission.name} supplied amount {amount} is not approximately 1f");
			}
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

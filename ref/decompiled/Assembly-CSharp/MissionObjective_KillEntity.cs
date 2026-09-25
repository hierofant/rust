using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/OBJECTIVES/Kill")]
public class MissionObjective_KillEntity : MissionObjective
{
	public BaseEntityRef[] targetEntities;

	public LayerMask targetLayerMask = -1;

	public int numToKill;

	public bool shouldUpdateMissionLocation;

	public bool pingTargets;

	public bool doKillsFromTeamMembersCount;

	[Tooltip("If enabled, the player must be within the defined distance threshold of the team member which initiated the kill for the objective to progress.")]
	public bool enableDistanceThresholdForTeamkills;

	public float teamkillDistanceThreshold = 50f;

	public TerrainBiome.Enum mustBeInBiome = (TerrainBiome.Enum)(-1);

	private readonly HashSet<uint> targetPrefabIDs = new HashSet<uint>();

	private bool isInitialized;

	private float teamkillDistanceThresholdSqr;

	public override BasePlayer.PingType PingType => BasePlayer.PingType.Hostile;

	private void EnsureInitialized()
	{
		if (isInitialized)
		{
			return;
		}
		CacheSqrDistanceForCompletion();
		BaseEntityRef[] array = targetEntities;
		foreach (BaseEntityRef baseEntityRef in array)
		{
			if (!baseEntityRef.isValid)
			{
				break;
			}
			targetPrefabIDs.Add(baseEntityRef.Get().prefabID);
		}
		isInitialized = true;
	}

	public override bool IsEntityValidForObjective<T>(T entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (!(entity is BaseCombatEntity baseCombatEntity))
		{
			return false;
		}
		if (!targetPrefabIDs.Contains(entity.prefabID))
		{
			return false;
		}
		if (!baseCombatEntity.IsAlive())
		{
			return false;
		}
		return true;
	}

	private void CacheSqrDistanceForCompletion()
	{
		teamkillDistanceThresholdSqr = teamkillDistanceThreshold * teamkillDistanceThreshold;
	}

	public override void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
		base.MissionStarted(index, instance, forPlayer);
		instance.objectiveStatuses[index].progressCurrent = 0f;
		instance.objectiveStatuses[index].progressTarget = numToKill;
	}

	public override void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		base.ProcessMissionEvent(playerFor, instance, index, type, payload, amount);
		if (type != BaseMission.MissionEventType.KILL_ENTITY || IsCompleted(index, instance) || !CanProgress(index, instance))
		{
			return;
		}
		EnsureInitialized();
		NetworkableId networkIdentifier = payload.NetworkIdentifier;
		uint uintIdentifier = payload.UintIdentifier;
		int intIdentifier = payload.IntIdentifier;
		bool flag = playerFor.net.ID == networkIdentifier;
		if (doKillsFromTeamMembersCount)
		{
			if (!flag)
			{
				if (!(BaseNetworkable.serverEntities.Find(networkIdentifier) is BasePlayer { Team: not null } basePlayer))
				{
					return;
				}
				bool flag2 = false;
				foreach (ulong member in basePlayer.Team.members)
				{
					if (member == (ulong)playerFor.userID)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2 || (enableDistanceThresholdForTeamkills && Vector3.SqrMagnitude(basePlayer.transform.position - playerFor.transform.position) > teamkillDistanceThresholdSqr))
				{
					return;
				}
			}
		}
		else if (!flag)
		{
			return;
		}
		if ((mustBeInBiome == (TerrainBiome.Enum)(-1) || TerrainMeta.IsInBiome(payload.WorldPosition, mustBeInBiome)) && targetPrefabIDs.Contains(uintIdentifier))
		{
			instance.objectiveStatuses[index].progressCurrent += intIdentifier;
			if (instance.objectiveStatuses[index].progressCurrent >= (float)numToKill)
			{
				CompleteObjective(index, instance, playerFor);
			}
			playerFor.DeregisterPingedEntitiesOfType(BasePlayer.PingType.Hostile);
			playerFor.MissionsDirty(saveImmediately: true);
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float timeSinceLastThink)
	{
		if (!shouldUpdateMissionLocation || !IsStarted(index, instance) || IsCompleted(index, instance))
		{
			return;
		}
		ref RealTimeSince sinceLastThink = ref instance.objectiveStatuses[index].sinceLastThink;
		if (!((float)sinceLastThink < 1f))
		{
			sinceLastThink = 0f;
			EnsureInitialized();
			assignee.DeregisterPingedEntitiesOfType(BasePlayer.PingType.Hostile);
			if (pingTargets && TryFindNearby<BaseCombatEntity>(assignee.transform.position, targetLayerMask.value, out var entity, 200f))
			{
				SetObjectiveWorldLocation(index, instance, entity.transform.position);
				assignee.MissionsDirty();
				assignee.RegisterPingedEntity(entity, BasePlayer.PingType.Hostile);
			}
		}
	}
}

using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public abstract class MissionObjective : ScriptableObject
{
	public virtual BasePlayer.PingType PingType => BasePlayer.PingType.GoTo;

	public virtual void MissionStarted(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
	}

	public virtual void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		BaseMission mission = instance.GetMission();
		if (mission == null)
		{
			return;
		}
		int count = instance.objectiveStatuses.Count;
		int num = mission.objectives.Length;
		if (count != num)
		{
			Debug.LogError($"Mission instance for mission {mission.name} contains data for {count} objectives but mission has {num} objectives", mission);
			return;
		}
		instance.objectiveStatuses[index].started = true;
		if (mission.objectives[index].requiredEntities != null)
		{
			string[] requiredEntities = mission.objectives[index].requiredEntities;
			foreach (string identifier in requiredEntities)
			{
				instance.GetSpawnedMissionEntity(identifier, playerFor);
			}
		}
		playerFor.MissionsDirty();
	}

	public virtual void ObjectiveCompleted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
	}

	public virtual void ObjectiveFailed(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
	}

	public void ResetObjective(int index, BaseMission.MissionInstance instance, BasePlayer playerFor, bool resetStartedStatus = false, bool resetSoftCompletedStatus = true, float newProgressCurrent = 0f)
	{
		BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = instance.objectiveStatuses[index];
		if (resetStartedStatus)
		{
			objectiveStatus.started = false;
		}
		if (resetSoftCompletedStatus)
		{
			objectiveStatus.softCompleted = false;
		}
		objectiveStatus.completed = false;
		objectiveStatus.failed = false;
		objectiveStatus.progressCurrent = newProgressCurrent;
		playerFor.MissionsDirty(saveImmediately: true);
	}

	public void ResetFollowupObjectives(int fromIndex, BaseMission.MissionInstance instance, BasePlayer playerFor, bool resetStartedStatus)
	{
		for (int i = fromIndex + 1; i < instance.objectiveStatuses.Count; i++)
		{
			ResetObjective(i, instance, playerFor, resetStartedStatus);
		}
	}

	public void ClearSoftCompletedStatus(int index, BaseMission.MissionInstance instance, BasePlayer playerFor)
	{
		instance.objectiveStatuses[index].softCompleted = false;
		playerFor.MissionsDirty(saveImmediately: true);
	}

	public void SoftCompleteObjective(int index, BaseMission.MissionInstance instance, BasePlayer playerFor)
	{
		instance.objectiveStatuses[index].softCompleted = true;
		playerFor.MissionsDirty(saveImmediately: true);
	}

	public void CompleteObjective(int index, BaseMission.MissionInstance instance, BasePlayer playerFor)
	{
		if (!instance.objectiveStatuses[index].completed && !instance.objectiveStatuses[index].failed)
		{
			instance.objectiveStatuses[index].completed = true;
			instance.GetMission().OnObjectiveCompleted(index, instance, playerFor);
			ObjectiveCompleted(playerFor, index, instance);
			playerFor.MissionsDirty(saveImmediately: true);
		}
	}

	public void FailObjective(int index, BaseMission.MissionInstance instance, BasePlayer playerFor)
	{
		if (!instance.objectiveStatuses[index].completed && !instance.objectiveStatuses[index].failed)
		{
			instance.objectiveStatuses[index].failed = true;
			instance.GetMission().OnObjectiveFailed(index, instance, playerFor);
			ObjectiveFailed(playerFor, index, instance);
			playerFor.MissionsDirty(saveImmediately: true);
		}
	}

	public void FailMission(BaseMission.MissionInstance instance, BasePlayer assignee, BaseMission.MissionFailReason failReason)
	{
		instance.GetMission().MissionFailed(instance, assignee, failReason);
	}

	public virtual void ProcessMissionEvent(BasePlayer playerFor, BaseMission.MissionInstance instance, int index, BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
	}

	public void ServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float timeSinceLastThink)
	{
		if (ArePrerequisiteObjectivesMet(index, instance) && !IsStarted(index, instance))
		{
			ServerObjectiveStarted(assignee, index, instance);
		}
		DoServerThink(index, instance, assignee, timeSinceLastThink);
	}

	protected virtual void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float timeSinceLastThink)
	{
	}

	public virtual void PostServerLoad(int index, BaseMission.MissionInstance instance, BasePlayer forPlayer)
	{
	}

	public void SetObjectiveWorldLocation(int index, BaseMission.MissionInstance instance, Vector3 worldLocation)
	{
		instance.objectiveStatuses[index].worldLocation = worldLocation;
	}

	public bool ArePrerequisiteObjectivesMet(int index, BaseMission.MissionInstance instance)
	{
		BaseMission mission = instance.GetMission();
		if (index < 0 || index >= mission.objectives.Length)
		{
			Debug.LogError($"Objective index {index} is invalid, mission {mission.name} has {mission.objectives.Length} objectives");
			return false;
		}
		BaseMission.MissionObjectiveEntry missionObjectiveEntry = mission.objectives[index];
		if (missionObjectiveEntry.startAfterPriorObjectives)
		{
			for (int i = 0; i < index; i++)
			{
				if (!IsCompleted(i, instance) && mission.objectives[i].isRequired)
				{
					return false;
				}
			}
			return true;
		}
		for (int j = 0; j < missionObjectiveEntry.startAfterCompletedObjectives.Length; j++)
		{
			int num = missionObjectiveEntry.startAfterCompletedObjectives[j];
			if (!IsCompleted(num, instance) && mission.objectives[num].isRequired)
			{
				return false;
			}
		}
		return true;
	}

	public bool CanProgress(int index, BaseMission.MissionInstance instance)
	{
		if (instance.GetMission().objectives[index].onlyProgressIfStarted)
		{
			return IsStarted(index, instance);
		}
		return true;
	}

	public bool IsStarted(int index, BaseMission.MissionInstance instance)
	{
		if (instance == null || instance.objectiveStatuses == null)
		{
			return false;
		}
		if (index < 0 || index >= instance.objectiveStatuses.Count)
		{
			return false;
		}
		if (instance.objectiveStatuses[index] == null)
		{
			return false;
		}
		return instance.objectiveStatuses[index].started;
	}

	public bool IsCompleted(int index, BaseMission.MissionInstance instance)
	{
		if (!instance.objectiveStatuses[index].completed)
		{
			return instance.objectiveStatuses[index].failed;
		}
		return true;
	}

	public bool IsObjectiveActive(int index, BaseMission.MissionInstance instance)
	{
		if (instance.objectiveStatuses[index].IsObjectiveActive())
		{
			return instance.IsActive();
		}
		return false;
	}

	public Vector3 GetObjectiveWorldLocation(int index, BaseMission.MissionInstance instance)
	{
		return GetAuthoritativeObjectiveLocation(index, instance);
	}

	protected Vector3 GetAuthoritativeObjectiveLocation(int index, BaseMission.MissionInstance instance)
	{
		if (index < 0 || index >= instance.objectiveStatuses.Count)
		{
			Debug.LogError($"Failed to retrieve a world location for objective {base.name} index {index} on instance for mission {instance.GetMission().name} as it is out of range of objectiveStatuses count {instance.objectiveStatuses.Count}");
			return Vector3.zero;
		}
		return instance.objectiveStatuses[index].worldLocation;
	}

	public virtual bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		return true;
	}

	protected bool TryFindNearby<T>(Vector3 origin, int layerMask, out T entity, float radius = 20f) where T : BaseEntity
	{
		List<T> obj = Pool.Get<List<T>>();
		Vis.Entities(origin, radius, obj, layerMask);
		bool result = TryGetNearestValidEntity(origin, obj, out entity);
		Pool.FreeUnmanaged(ref obj);
		return result;
	}

	protected bool TryFindNearby<T>(Vector3 origin, int layerMask, List<T> buffer, float radius = 20f) where T : BaseEntity
	{
		using (TimeWarning.New("MissionObjective.TryFindNearby"))
		{
			bool result = false;
			List<T> obj = Pool.Get<List<T>>();
			Vis.Entities(origin, radius, obj, layerMask);
			int i = 0;
			for (int count = obj.Count; i < count; i++)
			{
				T val = obj[i];
				if (IsEntityValidForObjective(val))
				{
					buffer.Add(val);
					result = true;
				}
			}
			Pool.FreeUnmanaged(ref obj);
			return result;
		}
	}

	public virtual bool IsEntityValidForObjective<T>(T entity) where T : BaseEntity
	{
		return true;
	}

	public bool TryGetNearestValidEntity<T>(Vector3 origin, List<T> entities, out T nearestEntity) where T : BaseEntity
	{
		int num = -1;
		float num2 = float.PositiveInfinity;
		int count = entities.Count;
		if (count == 1)
		{
			if (IsEntityValidForObjective(entities[0]))
			{
				num = 0;
			}
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				T val = entities[i];
				if (IsEntityValidForObjective(val))
				{
					float num3 = Vector3.SqrMagnitude(val.transform.position - origin);
					if (num3 < num2)
					{
						num = i;
						num2 = num3;
					}
				}
			}
		}
		if (num >= 0)
		{
			nearestEntity = entities[num];
			return true;
		}
		nearestEntity = null;
		return false;
	}
}

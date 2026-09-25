using Facepunch;
using UnityEngine;

public class MissionObjective_GoToFloatingCity : MissionObjective
{
	[Tooltip("Distance threshold to player for objective to complete (if distance is less that this value).")]
	[InspectorName("Distance For Completion (m)")]
	public float distanceForCompletion = 50f;

	[Tooltip("Distance threshold to player for objective to reset (if distance is greater than this value).")]
	[InspectorName("Distance For Reset (m)")]
	public float distanceForReset = 50f;

	[Tooltip("If true, disregards distance on the y-plane.")]
	public bool use2DDistance = true;

	private float sqrDistanceForCompletion;

	private float sqrDistanceForReset;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceForCompletion = distanceForCompletion * distanceForCompletion;
		sqrDistanceForReset = distanceForReset * distanceForReset;
	}

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
		for (int i = 0; i < DeepSeaManager.ServerFloatingCities.Count; i++)
		{
			if (DeepSeaManager.ServerFloatingCities[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		base.ServerObjectiveStarted(playerFor, index, instance);
		DeepSeaFloatingCity deepSeaFloatingCity = null;
		int count = DeepSeaManager.ServerFloatingCities.Count;
		using PooledList<int> pooledList = Pool.Get<PooledList<int>>();
		pooledList.Capacity = count;
		for (int i = 0; i < count; i++)
		{
			pooledList.Add(i);
		}
		for (int j = 0; j < count; j++)
		{
			int num = Random.Range(j, count);
			int index2 = j;
			PooledList<int> pooledList2 = pooledList;
			int index3 = num;
			int num2 = pooledList[num];
			int num3 = pooledList[j];
			int num5 = (pooledList[index2] = num2);
			num5 = (pooledList2[index3] = num3);
			int index4 = pooledList[num];
			DeepSeaFloatingCity deepSeaFloatingCity2 = DeepSeaManager.ServerFloatingCities[index4];
			if (deepSeaFloatingCity2 != null)
			{
				deepSeaFloatingCity = deepSeaFloatingCity2;
				break;
			}
		}
		if (deepSeaFloatingCity == null)
		{
			Debug.LogError("Mission " + instance.GetMission().name + " failed to find a floating city", instance.GetMission());
			return;
		}
		SetObjectiveWorldLocation(index, instance, deepSeaFloatingCity.transform.position);
		playerFor.MissionsDirty();
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		Vector3 objectiveWorldLocation = GetObjectiveWorldLocation(index, instance);
		float num = (use2DDistance ? (objectiveWorldLocation - assignee.transform.position).SqrMagnitude2D() : Vector3.SqrMagnitude(objectiveWorldLocation - assignee.transform.position));
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = num <= sqrDistanceForCompletion;
		if (completed != flag)
		{
			if (flag)
			{
				CompleteObjective(index, instance, assignee);
			}
			else if (num >= sqrDistanceForReset)
			{
				ResetObjective(index, instance, assignee);
			}
		}
	}
}

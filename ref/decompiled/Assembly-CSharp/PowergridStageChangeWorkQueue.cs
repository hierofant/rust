using System;
using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;
using UnityEngine;

public class PowergridStageChangeWorkQueue : PersistentObjectWorkQueue
{
	private int currentIndex;

	private bool isRunning;

	private double lastUpdateTime;

	private double cooldownTimer;

	private readonly HashSet<IPowergridEntity> skipEntities = new HashSet<IPowergridEntity>();

	private Stopwatch stopwatch = new Stopwatch();

	private List<PowergridManager.PowergridEntityEntry> powergridEntities { get; }

	public override int ListLength => powergridEntities?.Count ?? 0;

	public bool IsRunning => isRunning;

	public PowergridStageChangeWorkQueue(List<PowergridManager.PowergridEntityEntry> entitiesList)
	{
		Name = GetType().FullName;
		PersistentObjectWorkQueue.All.Add(this);
		powergridEntities = entitiesList;
	}

	public void StopWorkQueue()
	{
		currentIndex = 0;
		isRunning = false;
		skipEntities.Clear();
	}

	public void StartWorkQueue()
	{
		isRunning = true;
		lastUpdateTime = Time.timeAsDouble;
	}

	public void RestartWorkQueue()
	{
		StopWorkQueue();
		StartWorkQueue();
	}

	public void OnEntityInserted(int index, IPowergridEntity entity)
	{
		if (isRunning)
		{
			skipEntities.Add(entity);
			if (index < currentIndex)
			{
				currentIndex++;
			}
		}
	}

	public void OnEntityRemoved(int index, IPowergridEntity entity)
	{
		if (isRunning)
		{
			skipEntities.Remove(entity);
			if (index < currentIndex)
			{
				currentIndex--;
			}
		}
	}

	public void RunList(double maximumMilliseconds)
	{
		double timeAsDouble = Time.timeAsDouble;
		double num = timeAsDouble - lastUpdateTime;
		lastUpdateTime = timeAsDouble;
		Stats.Clear();
		Stats.BudgetTime = ((maximumMilliseconds >= 1000.0) ? default(TimeSpan) : TimeSpanExt.FromMicroseconds(maximumMilliseconds));
		if (!isRunning)
		{
			return;
		}
		if (Powergrid.stageChangeWorkQueueDelayBetweenJobs > 0f)
		{
			cooldownTimer += num;
			if (cooldownTimer < (double)Powergrid.stageChangeWorkQueueDelayBetweenJobs)
			{
				return;
			}
		}
		cooldownTimer = 0.0;
		int listLength = ListLength;
		if (currentIndex >= listLength || listLength == 0)
		{
			StopWorkQueue();
			return;
		}
		int num2 = listLength;
		using (TimeWarning.New(Name, (int)WarningThreshold.TotalMilliseconds))
		{
			stopwatch.Restart();
			Vector3? vector = null;
			while (currentIndex < num2)
			{
				IPowergridEntity entity = powergridEntities[currentIndex].Entity;
				_ = powergridEntities[currentIndex];
				if (entity != null && !skipEntities.Contains(entity))
				{
					PowergridManager serverInstance = PointEntity<PowergridManager>.ServerInstance;
					entity.Server_OnPowergridStageChanged(serverInstance.CurrentStage);
					Vector3 valueOrDefault = vector.GetValueOrDefault();
					if (!vector.HasValue)
					{
						valueOrDefault = entity.GetEntity().transform.position;
						vector = valueOrDefault;
					}
					Stats.ProcessedCount++;
				}
				currentIndex++;
				if (currentIndex >= num2)
				{
					StopWorkQueue();
					break;
				}
				if (Stats.ProcessedCount > 0 && Powergrid.stageChangeWorkQueueDelayBetweenJobs > 0f)
				{
					bool flag = true;
					if (Powergrid.stageChangeWorkQueueGroupJobsDistance > 0f)
					{
						IPowergridEntity entity2 = powergridEntities[currentIndex].Entity;
						if (Vector3.SqrMagnitude(vector.Value - entity2.GetEntity().transform.position) <= Powergrid.stageChangeWorkQueueGroupJobsSqrDistance)
						{
							flag = false;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (stopwatch.Elapsed.TotalMilliseconds >= maximumMilliseconds)
				{
					break;
				}
			}
		}
		Stats.QueueCount = num2;
		Stats.ExecutionTime = stopwatch.Elapsed;
		TotalExecutionTime += Stats.ExecutionTime;
	}

	public string Info()
	{
		return $"{ListLength:n0}, lastCount: {Stats.ProcessedCount:n0}, lastMS: {Stats.ExecutionTime.TotalMilliseconds:R}, totMS: {TotalExecutionTime.TotalMilliseconds:n0}";
	}
}

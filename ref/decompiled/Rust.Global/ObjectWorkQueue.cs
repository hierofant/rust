using System;
using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;
using UnityEngine;

public abstract class ObjectWorkQueue
{
	public static readonly List<ObjectWorkQueue> All = new List<ObjectWorkQueue>();

	public string Name = "Untitled ObjectWorkQueue";

	public TimeSpan WarningThreshold = TimeSpan.FromMilliseconds(200.0);

	public long TotalProcessedCount;

	public TimeSpan TotalExecutionTime;

	public int HashSetMaxLength;

	public WorkQueueTelemStats Stats;

	public abstract int QueueLength { get; }

	public abstract int Capacity { get; }
}
public abstract class ObjectWorkQueue<T> : ObjectWorkQueue
{
	protected Queue<T> queue = new Queue<T>(256);

	protected HashSet<T> containerTest = new HashSet<T>(256);

	private const int SecondsToTrack = 60;

	private const int OverflowCountThreshold = 60;

	private const int InitialCapacity = 256;

	private const float LinearGrowthFactor = 1.1f;

	private int minCapacity = 256;

	private int capacityExceededCounter;

	private DateTime nextCapacityEvalTime;

	private readonly Stopwatch stopwatch = new Stopwatch();

	public override int QueueLength => queue.Count;

	public override int Capacity => minCapacity;

	public ObjectWorkQueue()
	{
		Name = GetType().Name;
		ObjectWorkQueue.All.Add(this);
	}

	public void Clear()
	{
		queue.Clear();
		containerTest.Clear();
		HashSetMaxLength = 0;
		minCapacity = 256;
		nextCapacityEvalTime = default(DateTime);
		capacityExceededCounter = 0;
	}

	public void TryShrink()
	{
		if (DateTime.Now > nextCapacityEvalTime)
		{
			nextCapacityEvalTime = DateTime.Now + TimeSpan.FromSeconds(60.0);
			capacityExceededCounter = 0;
		}
		if (HashSetMaxLength > minCapacity)
		{
			containerTest = new HashSet<T>(minCapacity);
			if (++capacityExceededCounter > 60)
			{
				minCapacity = (int)((float)minCapacity * 1.1f);
				nextCapacityEvalTime = DateTime.Now + TimeSpan.FromSeconds(60.0);
				capacityExceededCounter = 0;
			}
			HashSetMaxLength = 0;
		}
	}

	public void RunQueue(double maximumMilliseconds)
	{
		Stats.Clear();
		Stats.BudgetTime = ((maximumMilliseconds >= 1000.0) ? default(TimeSpan) : TimeSpanExt.FromMicroseconds(maximumMilliseconds));
		if (queue.Count == 0)
		{
			return;
		}
		stopwatch.Restart();
		SortQueue();
		BeforeRunJobs();
		HashSetMaxLength = Mathf.Max(containerTest.Count, HashSetMaxLength);
		using (TimeWarning.New(Name, (int)WarningThreshold.TotalMilliseconds))
		{
			while (queue.Count > 0)
			{
				Stats.ProcessedCount++;
				TotalProcessedCount++;
				T val = queue.Dequeue();
				containerTest.Remove(val);
				if (IsValidToRun(val))
				{
					RunJob(val);
				}
				if (stopwatch.Elapsed.TotalMilliseconds >= maximumMilliseconds)
				{
					break;
				}
			}
		}
		if (queue.Count == 0)
		{
			TryShrink();
		}
		Stats.QueueCount = Stats.ProcessedCount;
		Stats.QueueCount += QueueLength;
		Stats.ExecutionTime = stopwatch.Elapsed;
		TotalExecutionTime += Stats.ExecutionTime;
	}

	public void Add(T entity)
	{
		if (!Contains(entity) && ShouldAdd(entity))
		{
			queue.Enqueue(entity);
			containerTest.Add(entity);
		}
	}

	public void AddNoContainsCheck(T entity)
	{
		if (ShouldAdd(entity))
		{
			queue.Enqueue(entity);
		}
	}

	public bool Contains(T entity)
	{
		return containerTest.Contains(entity);
	}

	protected virtual void SortQueue()
	{
	}

	protected virtual bool ShouldAdd(T entity)
	{
		return true;
	}

	protected virtual void BeforeRunJobs()
	{
	}

	protected virtual bool IsValidToRun(T entity)
	{
		return entity != null;
	}

	protected abstract void RunJob(T entity);

	public string Info()
	{
		return $"{QueueLength:n0}, lastCount: {Stats.ProcessedCount:n0}, totCount: {TotalProcessedCount:n0}, totMS: {TotalExecutionTime:n0} ";
	}
}

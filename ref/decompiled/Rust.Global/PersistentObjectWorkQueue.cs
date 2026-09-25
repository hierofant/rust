using System;
using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;

public abstract class PersistentObjectWorkQueue
{
	public static readonly List<PersistentObjectWorkQueue> All = new List<PersistentObjectWorkQueue>();

	public string Name = "Untitled PersistentObjectWorkQueue";

	public TimeSpan WarningThreshold = TimeSpan.FromMilliseconds(200.0);

	public WorkQueueTelemStats Stats;

	public TimeSpan TotalExecutionTime;

	public abstract int ListLength { get; }
}
public abstract class PersistentObjectWorkQueue<T> : PersistentObjectWorkQueue
{
	protected ListHashSet<T> workList = new ListHashSet<T>();

	private int currentIndex;

	private Stopwatch stopwatch = new Stopwatch();

	public override int ListLength => workList.Count;

	public PersistentObjectWorkQueue()
	{
		Name = GetType().FullName;
		PersistentObjectWorkQueue.All.Add(this);
	}

	public void Clear()
	{
		workList.Clear();
	}

	public void RunList(double maximumMilliseconds)
	{
		Stats.Clear();
		Stats.BudgetTime = ((maximumMilliseconds >= 1000.0) ? default(TimeSpan) : TimeSpanExt.FromMicroseconds(maximumMilliseconds));
		if (workList.Count == 0)
		{
			return;
		}
		Stats.QueueCount = workList.Count;
		using (TimeWarning.New(Name, (int)WarningThreshold.TotalMilliseconds))
		{
			stopwatch.Restart();
			int count = workList.Count;
			T[] buffer = workList.Values.Buffer;
			if (currentIndex >= workList.Count)
			{
				currentIndex = 0;
			}
			int num = currentIndex;
			while (Stats.ProcessedCount < count)
			{
				Stats.ProcessedCount++;
				T val = buffer[currentIndex];
				if (val != null)
				{
					RunJob(val);
				}
				currentIndex++;
				if (currentIndex >= count)
				{
					currentIndex = 0;
				}
				if (currentIndex == num || stopwatch.Elapsed.TotalMilliseconds >= maximumMilliseconds)
				{
					break;
				}
			}
		}
		Stats.ExecutionTime = stopwatch.Elapsed;
		TotalExecutionTime += Stats.ExecutionTime;
	}

	public void Add(T entity)
	{
		if (!Contains(entity) && ShouldAdd(entity))
		{
			workList.Add(entity);
			OnAdded(entity);
		}
	}

	protected virtual void OnAdded(T entity)
	{
	}

	public void Remove(T entity)
	{
		workList.Remove(entity);
		OnRemoved(entity);
	}

	protected virtual void OnRemoved(T entity)
	{
	}

	public bool Contains(T entity)
	{
		return workList.Contains(entity);
	}

	protected virtual bool ShouldAdd(T entity)
	{
		return true;
	}

	protected abstract void RunJob(T entity);

	public string Info()
	{
		return $"{ListLength:n0}, lastCount: {Stats.ProcessedCount:n0}, lastMS: {Stats.ExecutionTime.TotalMilliseconds:R}, totMS: {TotalExecutionTime.TotalMilliseconds:n0}";
	}

	public void RunOnAll(Action<T> target)
	{
		foreach (T work in workList)
		{
			target(work);
		}
	}
}

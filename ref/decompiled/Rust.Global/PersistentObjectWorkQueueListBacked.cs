using System;
using System.Diagnostics;
using Facepunch;

public abstract class PersistentObjectWorkQueueListBacked<T> : PersistentObjectWorkQueue
{
	private int currentIndex;

	private Stopwatch stopwatch = new Stopwatch();

	public abstract BufferList<T> AssignedList { get; }

	public override int ListLength => AssignedList?.Count ?? 0;

	public PersistentObjectWorkQueueListBacked()
	{
		Name = GetType().FullName;
		PersistentObjectWorkQueue.All.Add(this);
	}

	public void RunList(double maximumMilliseconds)
	{
		Stats.Clear();
		Stats.BudgetTime = ((maximumMilliseconds >= 1000.0) ? default(TimeSpan) : TimeSpanExt.FromMicroseconds(maximumMilliseconds));
		if (ListLength == 0)
		{
			return;
		}
		int listLength = ListLength;
		using (TimeWarning.New(Name, (int)WarningThreshold.TotalMilliseconds))
		{
			stopwatch.Restart();
			BufferList<T> assignedList = AssignedList;
			if (currentIndex >= listLength)
			{
				currentIndex = 0;
			}
			int num = currentIndex;
			Stats.ProcessedCount = 0;
			while (Stats.ProcessedCount < listLength)
			{
				Stats.ProcessedCount++;
				T val = assignedList[currentIndex];
				if (val != null)
				{
					RunJob(val);
				}
				currentIndex++;
				if (currentIndex >= listLength)
				{
					currentIndex = 0;
				}
				if (currentIndex == num || stopwatch.Elapsed.TotalMilliseconds >= maximumMilliseconds)
				{
					break;
				}
			}
		}
		Stats.QueueCount = listLength;
		Stats.ExecutionTime = stopwatch.Elapsed;
		TotalExecutionTime += Stats.ExecutionTime;
	}

	protected abstract void RunJob(T entity);

	public string Info()
	{
		return $"{ListLength:n0}, lastCount: {Stats.ProcessedCount:n0}, lastMS: {Stats.ExecutionTime.TotalMilliseconds:R}, totMS: {TotalExecutionTime.TotalMilliseconds:n0}";
	}

	public void RunOnAll(Action<T> target)
	{
		if (AssignedList == null)
		{
			return;
		}
		foreach (T assigned in AssignedList)
		{
			target(assigned);
		}
	}
}

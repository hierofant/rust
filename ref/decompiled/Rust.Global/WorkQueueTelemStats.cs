using System;

public struct WorkQueueTelemStats
{
	public TimeSpan ExecutionTime;

	public TimeSpan BudgetTime;

	public int ProcessedCount;

	public int QueueCount;

	public void Clear()
	{
		ExecutionTime = default(TimeSpan);
		BudgetTime = default(TimeSpan);
		ProcessedCount = 0;
		QueueCount = 0;
	}

	public void Append(WorkQueueTelemStats otherStats)
	{
		ExecutionTime += otherStats.ExecutionTime;
		BudgetTime += otherStats.BudgetTime;
		ProcessedCount += otherStats.ProcessedCount;
		QueueCount += otherStats.QueueCount;
	}
}

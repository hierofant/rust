using System;
using ConVar;
using UnityEngine;

namespace Facepunch.Rust.Profiling;

public static class WorkQueueProfiler
{
	public static bool enabled;

	public static void Serialize(AnalyticsTable table, int frameIndex, DateTime timestamp)
	{
		if (!enabled)
		{
			return;
		}
		try
		{
			WorkQueueTelemStats workQueueTelemStats = default(WorkQueueTelemStats);
			foreach (ObjectWorkQueue item in ObjectWorkQueue.All)
			{
				workQueueTelemStats.Append(item.Stats);
				if (item.Stats.ProcessedCount != 0)
				{
					EventRecord eventRecord = EventRecord.CSV();
					eventRecord.AddField("frame_index", frameIndex).AddField("Timestamp", timestamp).AddField("QueueName", item.Name)
						.AddField("StartCount", item.Stats.QueueCount)
						.AddField("TimeTaken", item.Stats.ExecutionTime)
						.AddField("ProcessedCount", item.Stats.ProcessedCount)
						.AddField("server_id", Server.server_id)
						.AddField("BudgetTime", item.Stats.BudgetTime);
					table.Append(eventRecord);
				}
			}
			foreach (PersistentObjectWorkQueue item2 in PersistentObjectWorkQueue.All)
			{
				workQueueTelemStats.Append(item2.Stats);
				if (item2.Stats.ProcessedCount != 0)
				{
					EventRecord eventRecord2 = EventRecord.CSV();
					eventRecord2.AddField("frame_index", frameIndex).AddField("Timestamp", timestamp).AddField("QueueName", item2.Name)
						.AddField("StartCount", item2.Stats.QueueCount)
						.AddField("TimeTaken", item2.Stats.ExecutionTime)
						.AddField("ProcessedCount", item2.Stats.ProcessedCount)
						.AddField("server_id", Server.server_id)
						.AddField("BudgetTime", item2.Stats.BudgetTime);
					table.Append(eventRecord2);
				}
			}
			EventRecord eventRecord3 = EventRecord.CSV();
			eventRecord3.AddField("frame_index", frameIndex).AddField("Timestamp", timestamp).AddField("QueueName", "Aggregate")
				.AddField("StartCount", workQueueTelemStats.QueueCount)
				.AddField("TimeTaken", workQueueTelemStats.ExecutionTime)
				.AddField("ProcessedCount", workQueueTelemStats.ProcessedCount)
				.AddField("server_id", Server.server_id)
				.AddField("BudgetTime", workQueueTelemStats.BudgetTime);
			table.Append(eventRecord3);
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to serialize work queues: " + ex.Message);
		}
	}

	public static void Reset()
	{
	}
}

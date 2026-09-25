using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Facepunch.Rust;

public class AnalyticsTable
{
	private string name;

	private TimeSpan uploadInterval;

	private DateTime nextUploadStamp;

	private AnalyticsDocumentMode mode;

	private bool useJsonDataObject;

	private int queueCount;

	private int maxQueueCount;

	private ConcurrentBag<EventRecord> records = new ConcurrentBag<EventRecord>();

	public int QueueCount => queueCount;

	public int MaxQueueCount => maxQueueCount;

	public bool WantsUpload => DateTime.Now > nextUploadStamp;

	public string Name => name;

	public bool UseJsonDataObject => useJsonDataObject;

	public AnalyticsDocumentMode Mode => mode;

	public TimeSpan UploadInterval
	{
		get
		{
			return uploadInterval;
		}
		set
		{
			nextUploadStamp -= uploadInterval;
			uploadInterval = value;
			nextUploadStamp += uploadInterval;
		}
	}

	public AnalyticsTable(string tableName, TimeSpan uploadInterval, AnalyticsDocumentMode mode = AnalyticsDocumentMode.JSON, bool useJsonDataObject = false)
	{
		name = tableName;
		this.uploadInterval = uploadInterval;
		nextUploadStamp = DateTime.Now + uploadInterval;
		this.mode = mode;
		this.useJsonDataObject = useJsonDataObject;
	}

	public void Append(EventRecord record)
	{
		records.Add(record);
		int num = Interlocked.Increment(ref queueCount);
		int num2;
		do
		{
			num2 = maxQueueCount;
		}
		while (num > num2 && Interlocked.CompareExchange(ref maxQueueCount, num, num2) != num2);
	}

	public void Clear()
	{
		EventRecord result;
		while (records.TryTake(out result))
		{
			result.MarkSubmitted();
			Pool.Free(ref result);
		}
		queueCount = 0;
	}

	public MemoryStream Accumulate(AnalyticsManager.IAccumulator accumulator, bool forceConsume)
	{
		using (TimeWarning.New("AnalyticsTable.Accumulate"))
		{
			int num = 0;
			EventRecord result;
			while (records.TryTake(out result))
			{
				accumulator.Accumulate(this, result);
				result.MarkSubmitted();
				Pool.Free(ref result);
				num++;
			}
			Interlocked.Add(ref queueCount, -num);
			MemoryStream result2 = null;
			if (accumulator.HasPending)
			{
				if (WantsUpload || forceConsume)
				{
					UpdateNextUploadTime();
					result2 = accumulator.Consume(this);
				}
				if (Analytics.Log)
				{
					string text = accumulator.ConsumeLogs();
					if (!string.IsNullOrEmpty(text))
					{
						Debug.Log($"{Name} appending {text.Length} bytes:\n{text}");
					}
				}
			}
			return result2;
		}
	}

	public void UpdateNextUploadTime()
	{
		nextUploadStamp = DateTime.Now + uploadInterval;
	}

	public void Flush()
	{
		nextUploadStamp = DateTime.Now;
	}

	public void ResetStats()
	{
		queueCount = 0;
		maxQueueCount = 0;
	}
}

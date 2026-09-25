using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Facepunch;
using UnityEngine;

public static class ThreadUtil
{
	public enum ErrorLogging
	{
		None,
		LogExceptionAndBreak
	}

	public static async Task IterateBudgetUnbuffered<T>(IReadOnlyList<T> collection, TimeSpan budget, ErrorLogging errorLogging, Action<T> callback)
	{
		Stopwatch timer = Stopwatch.StartNew();
		for (int i = 0; i < collection.Count; i++)
		{
			try
			{
				callback(collection[i]);
			}
			catch (Exception exception)
			{
				if (errorLogging == ErrorLogging.LogExceptionAndBreak)
				{
					UnityEngine.Debug.LogException(exception);
					break;
				}
			}
			if (timer.Elapsed > budget)
			{
				await Task.Delay(1);
				timer.Restart();
			}
		}
	}

	public static async Task IterateBudget<T>(IReadOnlyList<T> collection, TimeSpan budget, ErrorLogging errorLogging, Action<T> callback)
	{
		List<T> list = Pool.Get<List<T>>();
		list.AddRange(collection);
		await IterateBudgetUnbuffered(list, budget, errorLogging, callback);
		Pool.FreeUnmanaged(ref list);
		await Task.CompletedTask;
	}
}

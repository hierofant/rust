using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Jobs.LowLevel.Unsafe;

namespace Facepunch;

public static class ThreadUtils
{
	public static int GetBatchSize(int count, int subdivideFactor = 4, int minBatchSize = 64)
	{
		return Math.Max(count / JobsUtility.JobWorkerCount / subdivideFactor, minBatchSize);
	}

	public static void WaitForTasks(List<UniTask> tasks)
	{
		if (tasks.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("WaitForTasks"))
		{
			bool flag;
			do
			{
				flag = false;
				foreach (UniTask task in tasks)
				{
					flag |= !task.Status.IsCompleted();
				}
			}
			while (flag);
			foreach (UniTask task2 in tasks)
			{
				task2.GetAwaiter().GetResult();
			}
		}
	}
}

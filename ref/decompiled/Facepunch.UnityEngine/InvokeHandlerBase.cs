using System;
using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;
using UnityEngine;

public abstract class InvokeHandlerBase<T> : SingletonComponent<T> where T : MonoBehaviour
{
	protected ListDictionary<InvokeAction, float> curList = new ListDictionary<InvokeAction, float>(2048);

	protected ListHashSet<InvokeAction> addList = new ListHashSet<InvokeAction>(1024);

	protected ListHashSet<InvokeAction> delList = new ListHashSet<InvokeAction>(1024);

	public InvokeProfiler profiler;

	protected int nullIndex;

	protected const int nullChecks = 50;

	private DebugMTLock mtGuard = new DebugMTLock(typeof(T).Name);

	private Stopwatch doTickTimer = new Stopwatch();

	private Stopwatch invokeTimer = new Stopwatch();

	protected void LateUpdate()
	{
		if (profiler == null)
		{
			profiler = InvokeProfiler.update;
		}
		using (mtGuard.Lock())
		{
			ApplyRemoves();
			ApplyAdds();
			DoTick();
			RemoveExpired();
			ApplyRemoves();
			ApplyAdds();
		}
	}

	protected abstract float GetTime();

	protected void DoTick()
	{
		float[] buffer = curList.Values.Buffer;
		InvokeAction[] buffer2 = curList.Keys.Buffer;
		int count = curList.Count;
		float time = GetTime();
		bool flag = profiler.mode > 0;
		bool flag2 = profiler.mode > 1;
		if (flag)
		{
			doTickTimer.Restart();
		}
		int num = 0;
		TimeSpan executedTime = default(TimeSpan);
		for (int i = 0; i < count; i++)
		{
			if (time < buffer[i])
			{
				continue;
			}
			InvokeAction invokeAction = buffer2[i];
			if ((bool)invokeAction.sender && !delList.Contains(invokeAction))
			{
				if (invokeAction.repeat >= 0f)
				{
					float num2 = time - buffer[i];
					float num3 = time + invokeAction.repeat - num2 % invokeAction.repeat;
					if (invokeAction.random > 0f)
					{
						num3 += UnityEngine.Random.Range(0f - invokeAction.random, invokeAction.random);
					}
					buffer[i] = num3;
				}
				else
				{
					QueueRemove(invokeAction);
				}
				if (flag2)
				{
					invokeTimer.Restart();
				}
				invokeAction.action();
				if (flag2)
				{
					TimeSpan elapsed = invokeTimer.Elapsed;
					invokeAction.TrackingData.ExecutionTime += elapsed;
					invokeAction.TrackingData.Calls++;
					num++;
					executedTime += elapsed;
				}
			}
			else
			{
				QueueRemove(invokeAction);
			}
		}
		if (flag)
		{
			profiler.tickCount = count;
			profiler.executedCount = num;
			profiler.elapsedTime = doTickTimer.Elapsed;
			profiler.executedTime = executedTime;
		}
	}

	protected void RemoveExpired()
	{
		InvokeAction[] buffer = curList.Keys.Buffer;
		int count = curList.Count;
		if (nullIndex >= count)
		{
			nullIndex = 0;
		}
		int num = Mathf.Min(nullIndex + 50, count);
		while (nullIndex < num)
		{
			InvokeAction invoke = buffer[nullIndex];
			if (!invoke.sender)
			{
				QueueRemove(invoke);
			}
			nullIndex++;
		}
	}

	protected void QueueAdd(InvokeAction invoke)
	{
		if (invoke.action == null)
		{
			UnityEngine.Debug.LogError($"Trying to add an invoke with a null action: {new StackTrace()}");
			return;
		}
		using (mtGuard.Lock())
		{
			delList.Remove(invoke);
			addList.Remove(invoke);
			addList.Add(invoke);
		}
	}

	protected void QueueRemove(InvokeAction invoke)
	{
		using (mtGuard.Lock())
		{
			delList.Remove(invoke);
			addList.Remove(invoke);
			delList.Add(invoke);
		}
	}

	protected bool Contains(InvokeAction invoke)
	{
		if (!delList.Contains(invoke))
		{
			if (!curList.Contains(invoke))
			{
				return addList.Contains(invoke);
			}
			return true;
		}
		return false;
	}

	protected void ApplyAdds()
	{
		InvokeAction[] buffer = addList.Values.Buffer;
		int count = addList.Count;
		float time = GetTime();
		profiler.addCount += count;
		for (int i = 0; i < count; i++)
		{
			InvokeAction key = buffer[i];
			curList.Remove(key);
			curList.Add(key, time + key.initial);
			key.TrackingData.InvokeCount++;
		}
		addList.Clear();
	}

	protected void ApplyRemoves()
	{
		InvokeAction[] buffer = delList.Values.Buffer;
		int count = delList.Count;
		profiler.deletedCount += count;
		for (int i = 0; i < count; i++)
		{
			InvokeAction key = buffer[i];
			curList.Remove(key);
			key.TrackingData.InvokeCount--;
		}
		delList.Clear();
	}

	public void ForEach(Action<InvokeAction> callback)
	{
		foreach (var (obj, _) in curList)
		{
			callback(obj);
		}
	}

	public void CancelInvokes(HashSet<Behaviour> senders)
	{
		if (senders.Count == 0)
		{
			return;
		}
		InvokeAction[] buffer = curList.Keys.Buffer;
		int count = curList.Count;
		for (int i = 0; i < count; i++)
		{
			InvokeAction invoke = buffer[i];
			if (senders.Contains(invoke.sender))
			{
				QueueRemove(invoke);
			}
		}
		InvokeAction[] buffer2 = addList.Values.Buffer;
		for (int num = addList.Count - 1; num >= 0; num--)
		{
			InvokeAction invoke2 = buffer2[num];
			if (senders.Contains(invoke2.sender))
			{
				QueueRemove(invoke2);
			}
		}
	}
}

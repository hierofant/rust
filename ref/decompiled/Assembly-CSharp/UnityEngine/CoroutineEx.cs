using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Facepunch;
using Rust;

namespace UnityEngine;

public static class CoroutineEx
{
	public static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

	public static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

	private static Dictionary<float, WaitForSeconds> waitForSecondsBuffer = new Dictionary<float, WaitForSeconds>();

	public static WaitForSeconds waitForSeconds(float seconds)
	{
		if (!waitForSecondsBuffer.TryGetValue(seconds, out var value))
		{
			value = new WaitForSeconds(seconds);
			waitForSecondsBuffer.Add(seconds, value);
		}
		return value;
	}

	public static WaitForSecondsRealtimeEx waitForSecondsRealtime(float seconds)
	{
		WaitForSecondsRealtimeEx waitForSecondsRealtimeEx = Facepunch.Pool.Get<WaitForSecondsRealtimeEx>();
		waitForSecondsRealtimeEx.WaitTime = seconds;
		return waitForSecondsRealtimeEx;
	}

	public static IEnumerator Combine(params IEnumerator[] coroutines)
	{
		while (true)
		{
			bool flag = true;
			foreach (IEnumerator enumerator in coroutines)
			{
				if (enumerator != null && enumerator.MoveNext())
				{
					flag = false;
				}
			}
			if (flag)
			{
				break;
			}
			yield return waitForEndOfFrame;
		}
	}

	public static Task AsTask(this IEnumerator coroutine)
	{
		if (coroutine == null)
		{
			return Task.CompletedTask;
		}
		TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
		Global.Runner.StartCoroutine(RunImpl());
		return tcs.Task;
		IEnumerator RunImpl()
		{
			yield return coroutine;
			tcs.SetResult(null);
		}
	}
}

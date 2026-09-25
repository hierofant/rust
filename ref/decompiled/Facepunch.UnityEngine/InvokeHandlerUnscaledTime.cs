using System;
using UnityEngine;

public class InvokeHandlerUnscaledTime : InvokeHandlerBase<InvokeHandlerUnscaledTime>
{
	protected override float GetTime()
	{
		return Time.unscaledTime;
	}

	public static bool IsInvoking(Behaviour sender, Action action)
	{
		if (!SingletonComponent<InvokeHandlerUnscaledTime>.Instance)
		{
			return false;
		}
		return SingletonComponent<InvokeHandlerUnscaledTime>.Instance.Contains(new InvokeAction(sender, action, null));
	}

	public static void Invoke(Behaviour sender, Action action, float time)
	{
		if (!SingletonComponent<InvokeHandlerUnscaledTime>.Instance)
		{
			CreateInstance();
		}
		InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
		SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueAdd(new InvokeAction(sender, action, trackingData, time));
	}

	public static void InvokeRepeating(Behaviour sender, Action action, float time, float repeat)
	{
		if (!SingletonComponent<InvokeHandlerUnscaledTime>.Instance)
		{
			CreateInstance();
		}
		InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
		SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueAdd(new InvokeAction(sender, action, trackingData, time, repeat));
	}

	public static void CancelInvoke(Behaviour sender, Action action)
	{
		if (!(SingletonComponent<InvokeHandlerUnscaledTime>.Instance == null))
		{
			InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
			SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueRemove(new InvokeAction(sender, action, trackingData));
		}
	}

	public static void InvokeRandomized(Behaviour sender, Action action, float time, float repeat, float random)
	{
		if (!SingletonComponent<InvokeHandlerUnscaledTime>.Instance)
		{
			CreateInstance();
		}
		InvokeTrackingData trackingData = SingletonComponent<InvokeHandlerUnscaledTime>.Instance.profiler.GetTrackingData(new InvokeTrackingKey(action));
		SingletonComponent<InvokeHandlerUnscaledTime>.Instance.QueueAdd(new InvokeAction(sender, action, trackingData, time, repeat, random));
	}

	private static void CreateInstance()
	{
		GameObject obj = new GameObject();
		obj.name = "InvokeHandlerDemo";
		obj.AddComponent<InvokeHandlerUnscaledTime>().profiler = InvokeProfiler.demo;
		UnityEngine.Object.DontDestroyOnLoad(obj);
	}
}

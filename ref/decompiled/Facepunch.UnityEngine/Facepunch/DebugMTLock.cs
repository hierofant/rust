using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Facepunch;

public class DebugMTLock
{
	public struct LockScope : IDisposable
	{
		internal DebugMTLock _owner;

		internal bool _alreadyPrinted;

		public void Dispose()
		{
			_owner.ReleaseLock(_alreadyPrinted);
		}
	}

	public static bool Enabled;

	public static int MaxFrameCountToEmit = 5;

	private string _name;

	private int _threadIds;

	private int _reentrantThreadId;

	private int _reentryCount;

	public DebugMTLock(string name)
	{
		_name = name;
	}

	public LockScope Lock()
	{
		bool alreadyPrinted = TakeLock();
		LockScope result = default(LockScope);
		result._owner = this;
		result._alreadyPrinted = alreadyPrinted;
		return result;
	}

	private bool TakeLock()
	{
		if (!Enabled)
		{
			return false;
		}
		int currentManagedThreadId = Environment.CurrentManagedThreadId;
		int num = Interlocked.Add(ref _threadIds, currentManagedThreadId);
		bool flag = num != currentManagedThreadId;
		int num2 = Interlocked.CompareExchange(ref _reentrantThreadId, currentManagedThreadId, 0);
		if (num2 == 0 || num2 == currentManagedThreadId)
		{
			_reentryCount++;
		}
		if (flag && num2 != currentManagedThreadId)
		{
			string text = "DebugMTLock \"" + _name + "\" is already locked when entering! Callstack:\n";
			StackTrace stackTrace = new StackTrace(2);
			int num3 = Math.Min(stackTrace.FrameCount, MaxFrameCountToEmit);
			for (int i = 0; i < num3; i++)
			{
				MethodBase method = stackTrace.GetFrame(i).GetMethod();
				text = text + method.DeclaringType.Name + "." + method.Name + "\n";
			}
			UnityEngine.Debug.LogError(text);
			while (Volatile.Read(ref _threadIds) >= num)
			{
				Thread.Yield();
			}
			flag = true;
		}
		return flag;
	}

	private void ReleaseLock(bool skipErrorLog)
	{
		if (!Enabled)
		{
			return;
		}
		int currentManagedThreadId = Environment.CurrentManagedThreadId;
		if (Interlocked.Add(ref _threadIds, -currentManagedThreadId) != 0 && !skipErrorLog)
		{
			string text = "DebugMTLock \"" + _name + "\" is still locked when leaving! Callstack:\n";
			StackTrace stackTrace = new StackTrace(2);
			int num = Math.Min(stackTrace.FrameCount, MaxFrameCountToEmit);
			for (int i = 0; i < num; i++)
			{
				MethodBase method = stackTrace.GetFrame(i).GetMethod();
				text = text + method.DeclaringType.Name + "." + method.Name + "\n";
			}
			UnityEngine.Debug.LogError(text);
		}
		if (Volatile.Read(ref _reentrantThreadId) == currentManagedThreadId)
		{
			_reentryCount--;
			if (_reentryCount == 0)
			{
				Volatile.Write(ref _reentrantThreadId, 0);
			}
		}
	}
}

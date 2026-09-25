using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Facepunch;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct UnsafeScriptingAccess : IDisposable
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MaybeSwitchToThreadPool
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct Awaiter : ICriticalNotifyCompletion, INotifyCompletion
		{
			public bool IsCompleted
			{
				get
				{
					if (Enabled)
					{
						return !((Thread.CurrentThread.ManagedThreadId == 1) & _hasInitialized);
					}
					return true;
				}
			}

			public void GetResult()
			{
			}

			public void OnCompleted(Action continuation)
			{
				default(SwitchToThreadPoolAwaitable.Awaiter).OnCompleted(continuation);
			}

			public void UnsafeOnCompleted(Action continuation)
			{
				default(SwitchToThreadPoolAwaitable.Awaiter).UnsafeOnCompleted(continuation);
			}
		}

		public Awaiter GetAwaiter()
		{
			return default(Awaiter);
		}
	}

	public static bool Enabled = true;

	private static bool _hasInitialized;

	private static uint _threadSafeCheckBitField_TLS_Slot = uint.MaxValue;

	[DllImport("RustNative")]
	private unsafe static extern void SetTLSValue(uint index, void* val);

	[DllImport("RustNative")]
	private unsafe static extern void* GetTLSValue(uint index);

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void PostLoadInit()
	{
		Initialize();
	}

	private unsafe static void Initialize()
	{
		_hasInitialized = false;
		if (!Enabled)
		{
			return;
		}
		IntPtr intPtr = new IntPtr(1);
		for (uint num = 0u; num < 512; num++)
		{
			IntPtr intPtr2 = new IntPtr(GetTLSValue(num));
			if (intPtr2 == intPtr)
			{
				SetTLSValue(num, new IntPtr(0).ToPointer());
				bool flag = false;
				try
				{
					_ = Time.time;
				}
				catch (Exception)
				{
					flag = true;
				}
				SetTLSValue(num, intPtr2.ToPointer());
				if (flag)
				{
					_threadSafeCheckBitField_TLS_Slot = num;
					break;
				}
			}
		}
		if (_threadSafeCheckBitField_TLS_Slot == uint.MaxValue)
		{
			Enabled = false;
		}
		_hasInitialized = Enabled;
	}

	private unsafe static void SetThreadScriptExecutionEnabled(bool isEnabled)
	{
		IntPtr intPtr = new IntPtr(isEnabled ? 1 : 0);
		SetTLSValue(_threadSafeCheckBitField_TLS_Slot, intPtr.ToPointer());
	}

	public static UnsafeScriptingAccess Start()
	{
		if (Enabled && _hasInitialized)
		{
			SetThreadScriptExecutionEnabled(isEnabled: true);
		}
		return default(UnsafeScriptingAccess);
	}

	void IDisposable.Dispose()
	{
		if (Enabled && _hasInitialized)
		{
			SetThreadScriptExecutionEnabled(isEnabled: false);
		}
	}

	public static MaybeSwitchToThreadPool SwitchToMultithreading()
	{
		return default(MaybeSwitchToThreadPool);
	}
}

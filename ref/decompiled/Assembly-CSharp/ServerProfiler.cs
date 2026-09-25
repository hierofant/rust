using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[SuppressUnmanagedCodeSecurity]
public static class ServerProfiler
{
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 9)]
	public struct Mark
	{
		public enum Type : byte
		{
			Sync,
			Enter,
			Exit,
			Exception,
			Alloc,
			GCBegin,
			GCEnd,
			AllocWithStack
		}

		public long Timestamp;

		public Type Event;
	}

	public struct Alloc
	{
		public unsafe Native.MonoClass* Class;

		public unsafe Native.MonoMethod* LastMethod;

		public uint AlignedSize;

		public uint FlatArraySize;
	}

	[StructLayout(LayoutKind.Explicit, Size = 32)]
	public struct Profile
	{
		[FieldOffset(0)]
		public unsafe byte* Data;

		[FieldOffset(16)]
		public uint WriteEnd;

		[FieldOffset(20)]
		public int ThreadId;

		[FieldOffset(24)]
		public long Timestamp;
	}

	[StructLayout(LayoutKind.Sequential, Size = 64)]
	public struct MemoryReading
	{
		public long Timestamp;

		public ulong WorkingSet;

		public ulong VirtualSet;
	}

	[StructLayout(LayoutKind.Sequential, Size = 16)]
	public struct MemoryState
	{
		public unsafe MemoryReading* Readings;

		public uint Created;
	}

	public enum NotifyMetric : byte
	{
		TotalAllocCount,
		TotalMem,
		MainAllocCount,
		MainMem,
		WorkerAllocCount,
		WorkerMem,
		Count
	}

	public static class Native
	{
		[StructLayout(LayoutKind.Explicit)]
		public struct MonoImage
		{
			[FieldOffset(48)]
			public unsafe byte* AssemblyName;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct MonoClass
		{
			private const int ImageOffset = 64;

			[FieldOffset(64)]
			public unsafe MonoImage* Image;

			[FieldOffset(72)]
			public unsafe byte* Name;

			[FieldOffset(80)]
			public unsafe byte* Namespace;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct MonoMethod
		{
			[FieldOffset(8)]
			public unsafe MonoClass* Class;

			[FieldOffset(24)]
			public unsafe byte* Name;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct MonoVTable
		{
			[FieldOffset(0)]
			public unsafe MonoClass* Class;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct MonoObject
		{
			[FieldOffset(0)]
			public unsafe MonoVTable* VTable;
		}

		public enum StorageType : byte
		{
			FrameLimited,
			FixedBuffer
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct ThreadData
		{
			[FieldOffset(0)]
			public unsafe byte* Storage;

			[FieldOffset(8)]
			public uint StorageEnd;

			[FieldOffset(12)]
			public int ThreadId;
		}

		[StructLayout(LayoutKind.Explicit, Size = 16)]
		public struct MemoryData
		{
			[FieldOffset(0)]
			public unsafe MemoryReading* Storage;

			[FieldOffset(8)]
			public ulong StorageEnd;
		}

		public enum RecordingType : byte
		{
			Forward,
			Continuous
		}

		[DllImport("ServerProfiler.Core")]
		public static extern void Install();

		[DllImport("ServerProfiler.Core")]
		public static extern void SetStorageType(byte aStorageType);

		[DllImport("ServerProfiler.Core")]
		public static extern bool SetFramesToRecord(byte aFrameCount);

		[DllImport("ServerProfiler.Core")]
		public static extern bool SetFixedBufferCap(uint aMainThreadCap, uint aWorkerThreadCap);

		[DllImport("ServerProfiler.Core")]
		public static extern void TakeSnapshot();

		[DllImport("ServerProfiler.Core")]
		public static extern void StartContinuousProfiling(byte aMaxStackDepth);

		[DllImport("ServerProfiler.Core")]
		public static extern void StopContinuousProfiling();

		[DllImport("ServerProfiler.Core")]
		public static extern void ResumeContinuousProfiling();

		[DllImport("ServerProfiler.Core")]
		public static extern void SetContinuousProfilerNotifySettings(NotifyMetric aSetting, uint aValue);

		[DllImport("ServerProfiler.Core")]
		public static extern bool OnFrameEnd();

		[DllImport("ServerProfiler.Core")]
		public unsafe static extern void GetData(out Profile** profiles, out byte count);

		[DllImport("ServerProfiler.Core")]
		public unsafe static extern void GetMemoryUsage(out MemoryState* state);

		[DllImport("ServerProfiler.Core")]
		public static extern bool ReleaseResources();

		[DllImport("ServerProfiler.Core")]
		public unsafe static extern bool AllocateRecorder(byte* handle);

		[DllImport("ServerProfiler.Core")]
		public static extern void FreeRecorder(byte handle);

		[DllImport("ServerProfiler.Core")]
		public static extern void ConfigureRecorderStorage(byte handle, uint mainThreadCapacity, uint workerThreadCapacity);

		[DllImport("ServerProfiler.Core")]
		public static extern void ConfigureRecorderType(byte handle, RecordingType type);

		[DllImport("ServerProfiler.Core")]
		public static extern void ConfigureRecorderFramesToRecord(byte handle, ushort aFrameCount);

		[DllImport("ServerProfiler.Core")]
		public static extern void StartRecording(byte handle);

		[DllImport("ServerProfiler.Core")]
		public static extern void StopRecording(byte handle);

		[DllImport("ServerProfiler.Core")]
		public static extern void PauseRecording(byte handle);

		[DllImport("ServerProfiler.Core")]
		public static extern void ResumeRecording(byte handle);

		[DllImport("ServerProfiler.Core")]
		public unsafe static extern bool GetRecordedData(byte handle, ThreadData** threadDatas, byte* count, MemoryData* aMemState);

		[DllImport("ServerProfiler.Core")]
		public unsafe static extern bool GetRecordingTimestamp(byte handle, long* timestamp);
	}

	public struct RecorderHandle
	{
		private byte id;

		public byte Id => (byte)(id - 1);

		public bool IsValid => id != 0;

		public RecorderHandle(byte id)
		{
			this.id = id;
		}
	}

	public struct RecorderState
	{
		public readonly List<Profile> ThreadProfiles;

		public readonly MemoryState MemoryState;

		public bool IsValid => ThreadProfiles != null;

		public RecorderState(List<Profile> threadProfiles, MemoryState memState)
		{
			ThreadProfiles = threadProfiles;
			MemoryState = memState;
		}
	}

	public struct ScopeRecorder : IDisposable
	{
		private RecorderHandle handle;

		private string name;

		public ScopeRecorder(RecorderHandle handle, string name)
		{
			this.handle = handle;
			this.name = name;
		}

		public void Dispose()
		{
			EndRecording(handle);
			if (handle.IsValid)
			{
				ExportRecording(handle, name);
			}
			handle = default(RecorderHandle);
		}

		public void Pause()
		{
			PauseRecording(handle);
		}

		public void Resume()
		{
			ResumeRecording(handle);
		}
	}

	public struct SlowScopeRecorder : IDisposable
	{
		private RecorderHandle handle;

		private DateTime timeLimit;

		private string name;

		public SlowScopeRecorder(RecorderHandle handle, string name, TimeSpan slowLimit)
		{
			this.handle = handle;
			timeLimit = DateTime.Now + slowLimit;
			this.name = name;
		}

		void IDisposable.Dispose()
		{
			EndRecording(handle);
			if (handle.IsValid && DateTime.Now > timeLimit)
			{
				ExportRecording(handle, name);
			}
			else
			{
				ReleaseState(handle);
			}
			handle = default(RecorderHandle);
		}

		public void Pause()
		{
			PauseRecording(handle);
		}

		public void Resume()
		{
			ResumeRecording(handle);
		}
	}

	private struct RecordTaskState
	{
		public RecorderHandle handle;

		public string name;
	}

	public const byte MaxFrames = 10;

	private static bool canBeActivated = false;

	private static Action<IList<Profile>, MemoryState> onDoneCallback;

	private static bool isContinuous = false;

	private static int mainThreadId;

	public static bool ImmediateModeEnabled = false;

	public static bool ExportAsync = true;

	public static int ExportIntervalS = 1800;

	private static bool canExportThisFrame = true;

	private static RealTimeUntil nextExportUnlock = 0f;

	private static bool alreadyRecording;

	public static bool IsRunning
	{
		get
		{
			if (onDoneCallback == null)
			{
				return alreadyRecording;
			}
			return true;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		if (!Environment.CommandLine.Contains("-enableProfiler"))
		{
			Debug.Log("Profiler Disabled!");
			return;
		}
		mainThreadId = Environment.CurrentManagedThreadId;
		Native.Install();
		canBeActivated = true;
		ImmediateModeEnabled = true;
		Debug.Log("Profiler Initialized!");
		PostUpdateHook.EndOfFrame = (Action)Delegate.Combine(PostUpdateHook.EndOfFrame, new Action(OnFrameEnd));
		ResetExportInterval();
		onDoneCallback = null;
		alreadyRecording = false;
		canExportThisFrame = true;
		isContinuous = false;
	}

	public static void RecordNextFrames(int frames, Action<IList<Profile>, MemoryState> onDone)
	{
		if (onDone != null && IsEnabled() && !IsRunning)
		{
			onDoneCallback = onDone;
			Native.SetStorageType(0);
			Native.SetFramesToRecord((byte)Math.Clamp(frames, 1, 10));
			Native.TakeSnapshot();
			isContinuous = false;
		}
	}

	public static void RecordIntoBuffer(uint mainThreadCap, uint workerThreadCap, Action<IList<Profile>, MemoryState> onDone)
	{
		if (onDone != null && IsEnabled() && !IsRunning)
		{
			onDoneCallback = onDone;
			Native.SetStorageType(1);
			Native.SetFixedBufferCap(mainThreadCap, workerThreadCap);
			Native.TakeSnapshot();
			isContinuous = false;
		}
	}

	public static void StartContinuousRecording(byte maxStackDepth, Action<IList<Profile>, MemoryState> onDone)
	{
		if (onDone != null && IsEnabled() && !IsRunning)
		{
			onDoneCallback = onDone;
			Native.SetStorageType(1);
			Native.SetFixedBufferCap(33554432u, 8388608u);
			Native.StartContinuousProfiling(maxStackDepth);
			isContinuous = true;
		}
	}

	public static void StopContinuousRecording()
	{
		Native.StopContinuousProfiling();
		onDoneCallback = null;
	}

	public static void ResumeContinuousRecording()
	{
		Native.ResumeContinuousProfiling();
	}

	public static void ReleaseResources()
	{
		Native.ReleaseResources();
	}

	public static bool IsEnabled()
	{
		return canBeActivated;
	}

	public unsafe static void OnFrameEnd()
	{
		if (Native.OnFrameEnd() && onDoneCallback != null)
		{
			List<Profile> list = null;
			Native.GetData(out var profiles, out var count);
			list = new List<Profile>(count);
			for (byte b = 0; b < count; b++)
			{
				if (profiles[(int)b]->WriteEnd != 0)
				{
					list.Add(*profiles[(int)b]);
				}
			}
			Native.GetMemoryUsage(out var state);
			MemoryState arg = *state;
			onDoneCallback(list, arg);
			if (!isContinuous)
			{
				onDoneCallback = null;
			}
		}
		canExportThisFrame = (float)nextExportUnlock <= 0f;
	}

	public static TimeSpan TimestampToTimespan(long stamp)
	{
		return TimeSpan.FromMilliseconds((double)stamp / 1000000.0);
	}

	public static long TimestampToMicros(long stamp)
	{
		return stamp / 1000;
	}

	public unsafe static void AppendNameTo(Native.MonoMethod* method, StringBuilder builder)
	{
		for (byte* ptr = method->Class->Image->AssemblyName; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
		builder.Append('!');
		for (byte* ptr = method->Class->Name; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
		builder.Append("::");
		for (byte* ptr = method->Name; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
	}

	public unsafe static void AppendNameTo(Native.MonoMethod* method, ProfileExporter.JSON.StringStream builder)
	{
		for (byte* ptr = method->Class->Image->AssemblyName; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
		builder.Append('!');
		for (byte* ptr = method->Class->Name; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
		builder.Append("::");
		for (byte* ptr = method->Name; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
	}

	public unsafe static void SerializeNameTo(Native.MonoMethod* method, MemoryStream stream)
	{
		long position = stream.Position;
		ushort num = 0;
		stream.WriteByte(0);
		stream.WriteByte(0);
		byte* ptr = method->Class->Image->AssemblyName;
		while (*ptr != 0)
		{
			stream.WriteByte(*ptr);
			ptr++;
			num++;
		}
		stream.WriteByte(33);
		num++;
		ptr = method->Class->Name;
		while (*ptr != 0)
		{
			stream.WriteByte(*ptr);
			ptr++;
			num++;
		}
		stream.WriteByte(58);
		stream.WriteByte(58);
		num += 2;
		ptr = method->Name;
		while (*ptr != 0)
		{
			stream.WriteByte(*ptr);
			ptr++;
			num++;
		}
		byte[] buffer = stream.GetBuffer();
		buffer[position] = (byte)(num >> 8);
		buffer[position + 1] = (byte)num;
	}

	public unsafe static void AppendNameTo(Alloc alloc, StringBuilder builder)
	{
		for (byte* ptr = alloc.Class->Image->AssemblyName; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
		builder.Append('!');
		for (byte* ptr = alloc.Class->Name; *ptr != 0; ptr++)
		{
			char c = (char)(*ptr);
			builder.Append(c);
			if (alloc.FlatArraySize != 0 && c == '[')
			{
				builder.Append(alloc.FlatArraySize);
			}
		}
	}

	public unsafe static void AppendNameTo(Alloc alloc, ProfileExporter.JSON.StringStream builder)
	{
		for (byte* ptr = alloc.Class->Image->AssemblyName; *ptr != 0; ptr++)
		{
			builder.Append((char)(*ptr));
		}
		builder.Append('!');
		for (byte* ptr = alloc.Class->Name; *ptr != 0; ptr++)
		{
			char c = (char)(*ptr);
			builder.Append(c);
			if (alloc.FlatArraySize != 0 && c == '[')
			{
				builder.Append(alloc.FlatArraySize);
			}
		}
	}

	public unsafe static void SerializeNameTo(Alloc alloc, MemoryStream stream)
	{
		long position = stream.Position;
		ushort num = 0;
		stream.WriteByte(0);
		stream.WriteByte(0);
		byte* ptr = alloc.Class->Image->AssemblyName;
		while (*ptr != 0)
		{
			stream.WriteByte(*ptr);
			ptr++;
			num++;
		}
		stream.WriteByte(33);
		num++;
		ptr = alloc.Class->Name;
		while (*ptr != 0)
		{
			stream.WriteByte(*ptr);
			ptr++;
			num++;
		}
		byte[] buffer = stream.GetBuffer();
		buffer[position] = (byte)(num >> 8);
		buffer[position + 1] = (byte)num;
	}

	public static int GetMainThreadId()
	{
		return mainThreadId;
	}

	private unsafe static RecorderHandle CreateRecorder()
	{
		ValidateIsOnMainThread();
		byte b = default(byte);
		if (Native.AllocateRecorder(&b))
		{
			return new RecorderHandle((byte)(b + 1));
		}
		return default(RecorderHandle);
	}

	public static void StartRecording(out RecorderHandle handle, bool shouldRecord = true)
	{
		handle = default(RecorderHandle);
		if (ImmediateModeEnabled && shouldRecord && !alreadyRecording && canExportThisFrame)
		{
			handle = CreateRecorder();
			if (handle.IsValid)
			{
				alreadyRecording = true;
				Native.ConfigureRecorderStorage(handle.Id, 12582912u, 1048576u);
				Native.ConfigureRecorderType(handle.Id, Native.RecordingType.Forward);
				Native.StartRecording(handle.Id);
			}
		}
	}

	public static void EndRecording(RecorderHandle handle)
	{
		if (handle.IsValid)
		{
			ValidateIsOnMainThread();
			Native.StopRecording(handle.Id);
			alreadyRecording = false;
		}
	}

	public static void PauseRecording(RecorderHandle handle)
	{
		if (handle.IsValid)
		{
			Native.PauseRecording(handle.Id);
		}
	}

	public static void ResumeRecording(RecorderHandle handle)
	{
		if (handle.IsValid)
		{
			Native.ResumeRecording(handle.Id);
		}
	}

	public unsafe static RecorderState GetRecorderState(RecorderHandle handle)
	{
		if (!handle.IsValid)
		{
			return default(RecorderState);
		}
		long timestamp = default(long);
		if (!Native.GetRecordingTimestamp(handle.Id, &timestamp))
		{
			return default(RecorderState);
		}
		Native.ThreadData* ptr = default(Native.ThreadData*);
		byte b = default(byte);
		Native.MemoryData memoryData = default(Native.MemoryData);
		Native.GetRecordedData(handle.Id, &ptr, &b, &memoryData);
		List<Profile> list = new List<Profile>(b);
		for (int i = 0; i < b; i++)
		{
			Native.ThreadData threadData = ptr[i];
			Profile profile = default(Profile);
			profile.Data = threadData.Storage;
			profile.WriteEnd = threadData.StorageEnd;
			profile.ThreadId = threadData.ThreadId;
			profile.Timestamp = timestamp;
			Profile item = profile;
			list.Add(item);
		}
		MemoryState memoryState = default(MemoryState);
		memoryState.Readings = memoryData.Storage;
		memoryState.Created = (uint)(memoryData.StorageEnd / (uint)sizeof(Native.MemoryData));
		MemoryState memState = memoryState;
		return new RecorderState(list, memState);
	}

	public static void ReleaseState(RecorderHandle handle)
	{
		if (handle.IsValid)
		{
			Native.FreeRecorder(handle.Id);
		}
	}

	public static ScopeRecorder RecordScope(string name, bool shouldRecord = true)
	{
		RecorderHandle handle;
		return RecordScope(name, shouldRecord, out handle);
	}

	public static ScopeRecorder RecordScope(string name, bool shouldRecord, out RecorderHandle handle)
	{
		StartRecording(out handle, shouldRecord);
		return new ScopeRecorder(handle, name);
	}

	public static SlowScopeRecorder RecordScopeIfSlow(string name, TimeSpan slowLimit, bool shouldRecord = true)
	{
		RecorderHandle handle;
		return RecordScopeIfSlow(name, slowLimit, shouldRecord, out handle);
	}

	public static SlowScopeRecorder RecordScopeIfSlow(string name, TimeSpan slowLimit, bool shouldRecord, out RecorderHandle handle)
	{
		StartRecording(out handle, shouldRecord);
		return new SlowScopeRecorder(handle, name, slowLimit);
	}

	public static void ResetExportInterval()
	{
		nextExportUnlock = 0f;
		canExportThisFrame = true;
	}

	private static void ExportRecording(RecorderHandle handle, string name)
	{
		if (!handle.IsValid)
		{
			return;
		}
		nextExportUnlock = ExportIntervalS;
		if (ExportAsync)
		{
			RecordTaskState recordTaskState = default(RecordTaskState);
			recordTaskState.handle = handle;
			recordTaskState.name = name;
			RecordTaskState recordTaskState2 = recordTaskState;
			Task.Factory.StartNew(delegate(object stateBox)
			{
				RecordTaskState obj = (RecordTaskState)stateBox;
				RecorderState recorderState2 = GetRecorderState(obj.handle);
				ProfileExporter.JSON.Export(obj.name, recorderState2.ThreadProfiles, recorderState2.MemoryState, skipToStackStart: false);
				ReleaseState(obj.handle);
			}, recordTaskState2);
		}
		else
		{
			RecorderState recorderState = GetRecorderState(handle);
			ProfileExporter.JSON.Export(name, recorderState.ThreadProfiles, recorderState.MemoryState);
			ReleaseState(handle);
		}
	}

	private static void ValidateIsOnMainThread()
	{
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Facepunch.Math;
using UnityEngine;

namespace Facepunch;

public static class Output
{
	public struct Entry
	{
		public string Message;

		public string Stacktrace;

		public string Type;

		public int Time;
	}

	private struct ThreadedEntry
	{
		public string Message;

		public string Stacktrace;

		public LogType Type;
	}

	private static readonly ConcurrentQueue<ThreadedEntry> threadedLogs = new ConcurrentQueue<ThreadedEntry>();

	private const int MaxQueuedThreadedLogs = 10000;

	private static int MainThreadId;

	public static bool installed = false;

	public static Queue<Entry> HistoryOutput = new Queue<Entry>();

	private static readonly Memoized<string, LogType> LogTypeToString = new Memoized<string, LogType>((LogType type) => type.ToString());

	public static event Action<string, string, LogType> OnMessage;

	public static event Action<string, string, LogType> OnPostMessage;

	public static void Install()
	{
		if (!installed)
		{
			MainThreadId = Thread.CurrentThread.ManagedThreadId;
			UnityEngine.Application.logMessageReceived += LogHandler;
			UnityEngine.Application.logMessageReceivedThreaded += ThreadedLogHandler;
			PreUpdateHook.OnUpdate = (Action)Delegate.Combine(PreUpdateHook.OnUpdate, new Action(ProcessThreadedLogs));
			TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler;
			installed = true;
		}
	}

	private static void UnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
	{
		foreach (Exception innerException in e.Exception.InnerExceptions)
		{
			Debug.LogException(innerException);
		}
		e.SetObserved();
	}

	private static void ThreadedLogHandler(string log, string stacktrace, LogType type)
	{
		if (Thread.CurrentThread.ManagedThreadId != MainThreadId && threadedLogs.Count < 10000)
		{
			threadedLogs.Enqueue(new ThreadedEntry
			{
				Message = log,
				Stacktrace = stacktrace,
				Type = type
			});
		}
	}

	public static void ProcessThreadedLogs()
	{
		ThreadedEntry result;
		while (threadedLogs.TryDequeue(out result))
		{
			LogHandler(result.Message, result.Stacktrace, result.Type);
		}
	}

	public static void LogHandler(string log, string stacktrace, LogType type)
	{
		if ((Output.OnMessage == null && Output.OnPostMessage == null) || log.StartsWith("Kinematic body only supports Speculative Continuous collision detection") || log.StartsWith("Non-convex MeshCollider with non-kinematic Rigidbody is no longer supported") || log.StartsWith("Too many layers used to exclude objects from lighting") || log.StartsWith("Skipped frame because GfxDevice") || log.StartsWith("Your current multi-scene setup has inconsistent Lighting") || log.Contains("HandleD3DDeviceLost") || log.Contains("ResetD3DDevice") || log.Contains("dev->Reset") || log.Contains("D3Dwindow device not lost anymore") || log.Contains("D3D device reset") || log.Contains("group < 0xfff") || log.Contains("Mesh can not have more than 65000 vert") || log.Contains("Trying to add (Layout Rebuilder for)") || log.Contains("Coroutine continue failure") || log.Contains("No texture data available to upload") || log.Contains("Trying to reload asset from disk that is not") || log.Contains("Unable to find shaders used for the terrain engine.") || log.Contains("Canvas element contains more than 65535 vertices") || log.Contains("RectTransform.set_anchorMin") || log.Contains("FMOD failed to initialize the output device") || log.Contains("Cannot create FMOD::Sound") || log.Contains("invalid utf-16 sequence") || log.Contains("missing surrogate tail") || log.Contains("Failed to create agent because it is not close enough to the Nav") || log.Contains("user-provided triangle mesh descriptor is invalid") || log.Contains("Releasing render texture that is set as") || log.Contains("AsyncResourceUpload failed.") || log.StartsWith("Missing shader") || log.StartsWith("ERROR: Shader") || log.StartsWith("Warning: Shader") || log.StartsWith("Shader '") || log.StartsWith("The shader Hidden") || log.StartsWith("HDR Render Texture not supported"))
		{
			return;
		}
		using (TimeWarning.New("Facepunch.Output.LogHandler"))
		{
			try
			{
				Output.OnMessage?.Invoke(log, stacktrace, type);
			}
			catch (Exception)
			{
			}
			try
			{
				Output.OnPostMessage?.Invoke(log, stacktrace, type);
			}
			catch (Exception)
			{
			}
		}
		Entry entry = default(Entry);
		entry.Message = log;
		entry.Stacktrace = stacktrace;
		entry.Type = LogTypeToString.Get(type);
		entry.Time = Epoch.Current;
		Entry item = entry;
		HistoryOutput.Enqueue(item);
		while (HistoryOutput.Count > 1024)
		{
			HistoryOutput.Dequeue();
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Facepunch;
using Facepunch.Extend;
using UnityEngine;

namespace ConVar;

[Factory("profile")]
public class Profile : ConsoleSystem
{
	private static Action delayedTakeSnapshot;

	private static bool exportDone = true;

	[ServerVar(Saved = true, Help = "Controls whether perfsnapshot commands emit chat messages")]
	public static bool Quiet = false;

	private const string PerfSnapshotHelp = "profile.perfsnapshot [delay=15, int] [name='Profile', str, no extension, max 32chars] [frames=10, int, max 10] [debug=false, dumps a binary snapshot as well]\nWill produce a JSON perf snapshot of <frames> that can be viewed in Perfetto or similar tools";

	private const string PerfSnapshot_StreamHelp = "profile.perfsnapshot_stream [name='Profile', str, no extension, max 32chars] [MainCap=32, int, max 256, buffer size for Main thread in Megabytes] [WorkerCap=8, int, max 256, buffer size for each Worker thread in Megabytes] [debug=false, dumps a binary snapshot as well]\nWill stream <mainCap>MB worth of data and generate a JSON snapshot that can be viewed in Perfetto or similar tools";

	private const string WatchAllocsHelp = "Params: [Name = 'Allocs'] [maxStackDepth = 16].\nStarts tracking of allocs, dumping a [Name].json.gz record once conditions are met";

	private static uint notifyOnTotalAllocCount = 16000u;

	private static uint notifyOnTotalMemKB = 12288u;

	private static uint notifyOnMainAllocCount = 0u;

	private static uint notifyOnMainMemKB = 0u;

	private static uint notifyOnWorkerAllocCount = 0u;

	private static uint notifyOnWorkerMemKB = 0u;

	[ClientVar(ClientAdmin = true, Help = "Log how long entities take to spawn on the client, use 'minloggedspawntime' to filter low spawn times")]
	public static bool LogEntitySpawnTime { get; set; }

	[ClientVar(ClientAdmin = true, Help = "Minimum spawn time before logging (in ms)")]
	public static int LogEntitySpawnTime_Min { get; set; }

	[ServerVar(Help = "(Generated) Allocation count threshold (per profiler tick) above which the native continuous profiler fires a notification; useful for detecting unexpected allocation spikes")]
	public static int NotifyOnTotalAllocCount
	{
		get
		{
			return (int)notifyOnTotalAllocCount;
		}
		set
		{
			if (notifyOnTotalAllocCount != value)
			{
				notifyOnTotalAllocCount = (uint)value;
				ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.TotalAllocCount, notifyOnTotalAllocCount);
			}
		}
	}

	[ServerVar(Help = "(Generated) Total managed memory threshold in kilobytes above which the native profiler fires a notification; helps detect memory leaks during long server runs")]
	public static int NotifyOnTotalMemKB
	{
		get
		{
			return (int)notifyOnTotalMemKB;
		}
		set
		{
			if (notifyOnTotalMemKB != value)
			{
				notifyOnTotalMemKB = (uint)value;
				ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.TotalMem, notifyOnTotalMemKB * 1024);
			}
		}
	}

	[ServerVar(Help = "(Generated) Main-thread allocation count threshold per tick for profiler notifications; 0 = disabled; helps isolate main-thread GC pressure")]
	public static int NotifyOnMainAllocCount
	{
		get
		{
			return (int)notifyOnMainAllocCount;
		}
		set
		{
			if (notifyOnMainAllocCount != value)
			{
				notifyOnMainAllocCount = (uint)value;
				ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.MainAllocCount, notifyOnMainAllocCount);
			}
		}
	}

	[ServerVar(Help = "(Generated) Main-thread memory usage threshold in kilobytes for profiler notifications; 0 = disabled")]
	public static int NotifyOnMainMemKB
	{
		get
		{
			return (int)notifyOnMainMemKB;
		}
		set
		{
			if (notifyOnMainMemKB != value)
			{
				notifyOnMainMemKB = (uint)value;
				ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.MainMem, notifyOnMainMemKB * 1024);
			}
		}
	}

	[ServerVar(Help = "(Generated) Worker-thread allocation count threshold per tick for profiler notifications; 0 = disabled; helps detect background task GC pressure")]
	public static int NotifyOnWorkerAllocCount
	{
		get
		{
			return (int)notifyOnWorkerAllocCount;
		}
		set
		{
			if (notifyOnWorkerAllocCount != value)
			{
				notifyOnWorkerAllocCount = (uint)value;
				ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.WorkerAllocCount, notifyOnWorkerAllocCount);
			}
		}
	}

	[ServerVar(Help = "(Generated) Worker-thread memory usage threshold in kilobytes for profiler notifications; 0 = disabled")]
	public static int NotifyOnWorkerMemKB
	{
		get
		{
			return (int)notifyOnWorkerMemKB;
		}
		set
		{
			if (notifyOnWorkerMemKB != value)
			{
				notifyOnWorkerMemKB = (uint)value;
				ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.WorkerMem, notifyOnWorkerMemKB * 1024);
			}
		}
	}

	[ServerVar(Help = "(Generated) When enabled, the server native profiler runs in immediate mode, capturing every frame rather than sampling; more accurate but higher overhead")]
	public static bool ImmediateModeEnabled
	{
		get
		{
			return ServerProfiler.ImmediateModeEnabled;
		}
		set
		{
			ServerProfiler.ImmediateModeEnabled = value;
		}
	}

	[ServerVar(Help = "(Generated) Interval in seconds between automatic exports of server profiler data to disk; lower values provide more frequent snapshots at a higher I/O cost")]
	public static int ExportIntervalS
	{
		get
		{
			return ServerProfiler.ExportIntervalS;
		}
		set
		{
			ServerProfiler.ExportIntervalS = value;
		}
	}

	private static void NeedProfileFolder()
	{
		if (!Directory.Exists("profile"))
		{
			Directory.CreateDirectory("profile");
		}
	}

	[ClientVar(Help = "(Generated) Starts recording a Unity Profiler binary log to a timestamped file in the profile/ folder; requires ENABLE_PROFILER build flag")]
	[ServerVar(Help = "(Generated) Starts recording a Unity Profiler binary log to a timestamped file in the profile/ folder; requires ENABLE_PROFILER build flag")]
	public static void start(Arg arg)
	{
	}

	[ClientVar(Help = "(Generated) Stops the active Unity Profiler binary log recording and finalises the file")]
	[ServerVar(Help = "(Generated) Stops the active Unity Profiler binary log recording and finalises the file")]
	public static void stop(Arg arg)
	{
	}

	[ServerVar(Help = "(Generated) Flushes any buffered analytics events immediately to the analytics backend rather than waiting for the next scheduled flush")]
	[ClientVar(Help = "(Generated) Flushes any buffered analytics events immediately to the analytics backend rather than waiting for the next scheduled flush")]
	public static void flush_analytics(Arg arg)
	{
	}

	[ServerVar(Help = "profile.perfsnapshot [delay=15, int] [name='Profile', str, no extension, max 32chars] [frames=10, int, max 10] [debug=false, dumps a binary snapshot as well]\nWill produce a JSON perf snapshot of <frames> that can be viewed in Perfetto or similar tools")]
	public static void PerfSnapshot(Arg arg)
	{
		if (!ServerProfiler.IsEnabled())
		{
			arg.ReplyWith("ServerProfiler is disabled");
			return;
		}
		if (!exportDone)
		{
			arg.ReplyWith("Already taking snapshot!");
			return;
		}
		exportDone = false;
		int delay = arg.GetInt(0, 15);
		string name = arg.GetString(1, "Profile").Truncate(32);
		int frames = arg.GetInt(2, 4);
		bool generateBinary = arg.GetBool(3);
		if (delay == 0 || Quiet)
		{
			if (!Quiet)
			{
				Chat.Broadcast("Server taking a perf snapshot", "SERVER", "#eee", 0uL);
			}
			ServerProfiler.RecordNextFrames(frames, delegate(IList<ServerProfiler.Profile> profiles, ServerProfiler.MemoryState memState)
			{
				if (!Quiet)
				{
					Chat.Broadcast("Snapshot taken", "SERVER", "#eee", 0uL);
				}
				Task.Run(delegate
				{
					try
					{
						if (generateBinary)
						{
							ProfileExporter.Binary.Export(name, profiles);
						}
						ProfileExporter.JSON.Export(name, profiles, memState);
					}
					finally
					{
						ServerProfiler.ReleaseResources();
						exportDone = true;
					}
				});
			});
			arg.ReplyWith("ServerProfiler is recording a perf snapshot");
			return;
		}
		Chat.Broadcast($"Server will be taking a perf snapshot, expect stutters in {delay} seconds", "SERVER", "#eee", 0uL);
		delayedTakeSnapshot = delegate
		{
			delay--;
			if (delay > 10 && delay % 5 == 0)
			{
				Chat.Broadcast($"Server will be taking a perf snapshot, expect stutters in {delay} seconds", "SERVER", "#eee", 0uL);
			}
			else if (delay > 0 && delay <= 10)
			{
				Chat.Broadcast($"{delay}...", "SERVER", "#eee", 0uL);
			}
			if (delay == 0)
			{
				ServerProfiler.RecordNextFrames(frames, delegate(IList<ServerProfiler.Profile> profiles, ServerProfiler.MemoryState memState)
				{
					Chat.Broadcast("Snapshot taken", "SERVER", "#eee", 0uL);
					Task.Run(delegate
					{
						try
						{
							if (generateBinary)
							{
								ProfileExporter.Binary.Export(name, profiles);
							}
							ProfileExporter.JSON.Export(name, profiles, memState);
						}
						finally
						{
							ServerProfiler.ReleaseResources();
							exportDone = true;
						}
					});
				});
				InvokeHandler.CancelInvoke(SingletonComponent<InvokeHandler>.Instance, delayedTakeSnapshot);
				delayedTakeSnapshot = null;
			}
		};
		InvokeHandler.InvokeRepeating(SingletonComponent<InvokeHandler>.Instance, delayedTakeSnapshot, 0f, 1f);
		arg.ReplyWith("ServerProfiler will record a perf snapshot after a delay");
	}

	[ServerVar(Help = "profile.perfsnapshot_stream [name='Profile', str, no extension, max 32chars] [MainCap=32, int, max 256, buffer size for Main thread in Megabytes] [WorkerCap=8, int, max 256, buffer size for each Worker thread in Megabytes] [debug=false, dumps a binary snapshot as well]\nWill stream <mainCap>MB worth of data and generate a JSON snapshot that can be viewed in Perfetto or similar tools")]
	public static void PerfSnapshot_Stream(Arg arg)
	{
		if (!ServerProfiler.IsEnabled())
		{
			arg.ReplyWith("ServerProfiler is disabled");
			return;
		}
		if (!exportDone)
		{
			arg.ReplyWith("Already taking snapshot!");
			return;
		}
		exportDone = false;
		string name = arg.GetString(0, "Profile").Truncate(32);
		uint mainThreadCap = Math.Min(arg.GetUInt(1, 32u), 256u) * 1048576;
		uint workerThreadCap = Math.Min(arg.GetUInt(2, 8u), 256u) * 1048576;
		bool generateBinary = arg.GetBool(3);
		if (!Quiet)
		{
			Chat.Broadcast("Server taking a perf snapshot, there might be stutters", "SERVER", "#eee", 0uL);
		}
		ServerProfiler.RecordIntoBuffer(mainThreadCap, workerThreadCap, delegate(IList<ServerProfiler.Profile> profiles, ServerProfiler.MemoryState memState)
		{
			if (!Quiet)
			{
				Chat.Broadcast("Snapshot taken", "SERVER", "#eee", 0uL);
			}
			Task.Run(delegate
			{
				try
				{
					if (generateBinary)
					{
						ProfileExporter.Binary.Export(name, profiles);
					}
					ProfileExporter.JSON.Export(name, profiles, memState);
				}
				finally
				{
					ServerProfiler.ReleaseResources();
					exportDone = true;
				}
			});
		});
		arg.ReplyWith("ServerProfiler started recording a perf stream snapshot");
	}

	[ServerVar(Help = "Params: [Name = 'Allocs'] [maxStackDepth = 16].\nStarts tracking of allocs, dumping a [Name].json.gz record once conditions are met")]
	public static void WatchAllocs(Arg arg)
	{
		if (!ServerProfiler.IsEnabled())
		{
			arg.ReplyWith("ServerProfiler is disabled");
			return;
		}
		if (ServerProfiler.IsRunning)
		{
			arg.ReplyWith("ServerProfiler is busy with a previous task");
			return;
		}
		ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.TotalAllocCount, notifyOnTotalAllocCount);
		ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.TotalMem, notifyOnTotalMemKB * 1024);
		ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.MainAllocCount, notifyOnMainAllocCount);
		ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.MainMem, notifyOnMainMemKB * 1024);
		ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.WorkerAllocCount, notifyOnWorkerAllocCount);
		ServerProfiler.Native.SetContinuousProfilerNotifySettings(ServerProfiler.NotifyMetric.WorkerMem, notifyOnWorkerMemKB * 1024);
		string name = arg.GetString(0, "Allocs");
		ServerProfiler.StartContinuousRecording((byte)arg.GetInt(1, 16), delegate(IList<ServerProfiler.Profile> profiles, ServerProfiler.MemoryState memState)
		{
			Task.Run(delegate
			{
				if (ProfileExporter.JSON.Export(name, profiles, memState))
				{
					ServerProfiler.ResumeContinuousRecording();
				}
				else
				{
					Debug.Log("Stopping watching allocations due to export error");
					ServerProfiler.StopContinuousRecording();
				}
			});
		});
		arg.ReplyWith("ServerProfiler started tracking allocations");
	}

	[ServerVar(Help = "Stops tracking of allocations")]
	public static void StopWatchingAllocs(Arg arg)
	{
		if (!ServerProfiler.IsEnabled())
		{
			arg.ReplyWith("ServerProfiler is disabled");
			return;
		}
		ServerProfiler.StopContinuousRecording();
		arg.ReplyWith("ServerProfiler stopped tracking allocations");
	}

	[ServerVar(Help = "(Generated) Resets the profiler export interval timer, causing the next export to happen after a full ExportIntervalS from now")]
	public static void ResetExportInterval(Arg arg)
	{
		ServerProfiler.ResetExportInterval();
		arg.ReplyWith("Done");
	}

	[ServerVar(Help = "(Generated) Counts and prints all entities that use synchronous movement updates grouped by prefab, helping identify expensive per-frame entity movers")]
	public static void CountSyncMoveEntities(Arg arg)
	{
		StringBuilder obj = Facepunch.Pool.Get<StringBuilder>();
		Dictionary<uint, uint> countPerPrefab = Facepunch.Pool.Get<Dictionary<uint, uint>>();
		if (SingletonComponent<InvokeHandler>.Instance != null)
		{
			SingletonComponent<InvokeHandler>.Instance.ForEach(Aggregate);
			obj.AppendLine("InvokeHandler");
			Print(countPerPrefab, obj);
			countPerPrefab.Clear();
		}
		if (SingletonComponent<InvokeHandlerFixedTime>.Instance != null)
		{
			SingletonComponent<InvokeHandlerFixedTime>.Instance.ForEach(Aggregate);
			obj.AppendLine("\nInvokeHandlerFixedTime");
			Print(countPerPrefab, obj);
		}
		Facepunch.Pool.FreeUnmanaged(ref countPerPrefab);
		arg.ReplyWith(obj.ToString());
		Facepunch.Pool.FreeUnmanaged(ref obj);
		void Aggregate(InvokeAction action)
		{
			if (action.sender != null && action.sender is BaseEntity baseEntity && action.action == baseEntity.NetworkPosTickCallback)
			{
				if (countPerPrefab.TryGetValue(baseEntity.prefabID, out var value))
				{
					countPerPrefab[baseEntity.prefabID] = value + 1;
				}
				else
				{
					countPerPrefab.Add(baseEntity.prefabID, 1u);
				}
			}
		}
		static void Print(Dictionary<uint, uint> counts, StringBuilder builder)
		{
			TextTable obj2 = Facepunch.Pool.Get<TextTable>();
			obj2.ResizeColumns(2);
			obj2.AddColumn("Count");
			obj2.AddColumn("Prefab");
			obj2.ResizeRows(counts.Count);
			List<(uint, uint)> obj3 = Facepunch.Pool.Get<List<(uint, uint)>>();
			foreach (KeyValuePair<uint, uint> count in counts)
			{
				obj3.Add((count.Key, count.Value));
			}
			obj3.Sort(((uint, uint) left, (uint, uint) right) => right.Item2.CompareTo(left.Item2));
			uint num = 0u;
			foreach (var item3 in obj3)
			{
				uint item = item3.Item1;
				uint item2 = item3.Item2;
				num += item2;
				obj2.AddValue(item2);
				obj2.AddValue(StringPool.Get(item));
			}
			Facepunch.Pool.FreeUnmanaged(ref obj3);
			builder.Append(obj2.ToString());
			builder.AppendLine($"Total: {num}");
			Facepunch.Pool.Free(ref obj2);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ConVar;
using Facepunch.Ping;
using Network;
using UnityEngine;

namespace Facepunch.Rust;

public class PerformanceLogging
{
	private enum FrameType : byte
	{
		InGame
	}

	private struct LagSpike
	{
		public int FrameIndex;

		public TimeSpan Time;

		public bool WasGC;
	}

	private struct GarbageCollect
	{
		public int FrameIndex;

		public TimeSpan Time;
	}

	private class PerformancePool
	{
		public List<TimeSpan> Frametimes;

		public List<int> Ping;
	}

	[JsonModel]
	private struct PluginInfo
	{
		public string Name;

		public string Author;

		public string Version;
	}

	[JsonModel]
	private struct ProcessInfo
	{
		public string Name;

		public long WorkingSet;
	}

	public static PerformanceLogging server = new PerformanceLogging(client: false);

	public static PerformanceLogging client = new PerformanceLogging(client: true);

	private readonly TimeSpan ClientInterval = TimeSpan.FromMinutes(10.0);

	private readonly TimeSpan ServerInterval = TimeSpan.FromMinutes(1.0);

	private readonly TimeSpan PublicServerInterval = TimeSpan.FromHours(1.0);

	private readonly TimeSpan PingInterval = TimeSpan.FromSeconds(5.0);

	private List<TimeSpan> Frametimes = new List<TimeSpan>();

	private List<int> PingHistory = new List<int>();

	private List<LagSpike> lagSpikes = new List<LagSpike>();

	private List<GarbageCollect> garbageCollections = new List<GarbageCollect>();

	private Dictionary<string, int> pendingTimings = new Dictionary<string, int>();

	private bool isClient;

	private Stopwatch frameWatch = new Stopwatch();

	private DateTime nextPingTime;

	private DateTime nextFlushTime;

	private DateTime connectedTime;

	private int serverIndex;

	private Guid serverSessionId;

	private int lastFrameGC;

	private int nonGCLagSpikes;

	private Type oxideType;

	private bool hasOxideType;

	public PerformanceLogging(bool client)
	{
		isClient = client;
	}

	private TimeSpan GetLagSpikeThreshold()
	{
		if (!isClient)
		{
			return TimeSpan.FromMilliseconds(200.0);
		}
		return TimeSpan.FromMilliseconds(100.0);
	}

	public void OnFrame()
	{
		if (!isClient && !Analytics.ServerPerformanceConVar)
		{
			ResetMeasurements();
			return;
		}
		TimeSpan elapsed = frameWatch.Elapsed;
		FrameType frameType = FrameType.InGame;
		if (!isClient)
		{
			frameType = GetServerFrameType();
		}
		if (frameType == FrameType.InGame)
		{
			Frametimes.Add(elapsed);
		}
		frameWatch.Restart();
		DateTime utcNow = DateTime.UtcNow;
		int num = System.GC.CollectionCount(0);
		bool flag = lastFrameGC != num;
		lastFrameGC = num;
		if (flag)
		{
			GarbageCollect item = new GarbageCollect
			{
				FrameIndex = Frametimes.Count - 1,
				Time = elapsed
			};
			if (frameType == FrameType.InGame)
			{
				garbageCollections.Add(item);
			}
		}
		if (elapsed > GetLagSpikeThreshold())
		{
			LagSpike item2 = new LagSpike
			{
				FrameIndex = Frametimes.Count - 1,
				Time = elapsed,
				WasGC = flag
			};
			if (frameType == FrameType.InGame)
			{
				lagSpikes.Add(item2);
			}
			if (!flag)
			{
				nonGCLagSpikes++;
			}
		}
		if (utcNow > nextFlushTime)
		{
			try
			{
				FlushMainThread();
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Failed to flush analytics: " + ex);
			}
		}
		static FrameType GetServerFrameType()
		{
			return FrameType.InGame;
		}
	}

	private Dictionary<string, string> FindModifiedConvars()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		ConsoleSystem.Command[] all = ConsoleSystem.Index.All;
		foreach (ConsoleSystem.Command command in all)
		{
			if (command.DefaultValue != null && command.GetOveride != null)
			{
				string text = command.GetOveride();
				if (text != command.DefaultValue)
				{
					dictionary[command.FullName] = text;
				}
			}
		}
		return dictionary;
	}

	public void FlushMainThread()
	{
		nextFlushTime = DateTime.UtcNow.Add(GetFlushInterval());
		if (!isClient && (BasePlayer.activePlayerList.Count == 0 || !Analytics.ServerPerformanceConVar))
		{
			ResetMeasurements();
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		EventRecord record = EventRecord.New(isClient ? "client_performance" : "server_performance", !isClient);
		record.AddObject("modified_convars", FindModifiedConvars());
		record.AddField("command_line", CommandLine.FullSafe);
		bool flag = false;
		if (!CollectionEx.IsEmpty(lagSpikes))
		{
			record.AddField("lag_spike_count", lagSpikes.Count);
			flag = true;
		}
		if (nonGCLagSpikes > 0)
		{
			record.AddField("lag_spike_no_gc_count", nonGCLagSpikes);
			flag = true;
		}
		if (flag)
		{
			record.AddLegacyTimespan("lag_spike_threshold", GetLagSpikeThreshold());
		}
		if (!CollectionEx.IsEmpty(garbageCollections))
		{
			record.AddField("gc_count", garbageCollections.Count);
		}
		record.AddField("ram_managed", System.GC.GetTotalMemory(forceFullCollection: false)).AddField("ram_total", SystemInfoEx.systemMemoryUsed).AddField("uptime", (int)UnityEngine.Time.realtimeSinceStartup)
			.AddField("map_url", World.Url)
			.AddField("world_size", World.Size)
			.AddField("world_seed", World.Seed)
			.AddField("world_ts", World.Timestamp)
			.AddField("active_scene", LevelManager.CurrentLevelName);
		if (pendingTimings.Count > 0)
		{
			record.AddObject("load_times", pendingTimings);
			pendingTimings.Clear();
		}
		IPingEstimateResults estimateToAllRegions = PingEstimater.GetEstimateToAllRegions();
		if (estimateToAllRegions != null)
		{
			record.AddObject("ping_regions", estimateToAllRegions.GetAllRegions());
		}
		if (!isClient && !isClient)
		{
			int value = (int)((global::Network.Net.sv != null) ? global::Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond) : 0);
			int value2 = (int)((global::Network.Net.sv != null) ? global::Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond) : 0);
			record.AddField("is_official", ConVar.Server.official && ConVar.Server.stats).AddField("bot_count", BasePlayer.bots.Count).AddField("player_count", BasePlayer.activePlayerList.Count)
				.AddField("max_players", ConVar.Server.maxplayers)
				.AddField("ent_count", BaseNetworkable.serverEntities.Count)
				.AddField("hostname", ConVar.Server.hostname)
				.AddField("net_in", value)
				.AddField("net_out", value2);
		}
		if (!isClient)
		{
			try
			{
				if (!hasOxideType)
				{
					oxideType = Type.GetType("Oxide.Core.Interface,Oxide.Core");
					hasOxideType = true;
				}
				if (oxideType != null)
				{
					record.AddField("is_oxide", value: true);
					object obj = oxideType.GetProperty("Oxide", BindingFlags.Static | BindingFlags.Public)?.GetValue(null);
					if (obj != null)
					{
						object obj2 = obj.GetType().GetProperty("RootPluginManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
						if (obj2 != null)
						{
							List<PluginInfo> list = new List<PluginInfo>();
							foreach (object item in obj2.GetType().GetMethod("GetPlugins")?.Invoke(obj2, null) as IEnumerable)
							{
								if (item != null)
								{
									string name = item.GetType().GetProperty("Name")?.GetValue(item) as string;
									string author = item.GetType().GetProperty("Author")?.GetValue(item) as string;
									string version = item.GetType().GetProperty("Version")?.GetValue(item)?.ToString();
									list.Add(new PluginInfo
									{
										Name = name,
										Author = author,
										Version = version
									});
								}
							}
							record.AddObject("oxide_plugins", list);
							record.AddField("oxide_plugin_count", list.Count);
						}
					}
				}
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.LogError($"Failed to get oxide when flushing server performance: {arg}");
			}
			try
			{
				List<ProcessInfo> list2 = new List<ProcessInfo>();
				Process[] processes = Process.GetProcesses();
				Process currentProcess = Process.GetCurrentProcess();
				Process[] array = processes;
				foreach (Process process in array)
				{
					try
					{
						if (currentProcess.Id != process.Id && process.ProcessName.Contains("RustDedicated"))
						{
							list2.Add(new ProcessInfo
							{
								Name = process.ProcessName,
								WorkingSet = process.WorkingSet64
							});
						}
					}
					catch (Exception ex)
					{
						if (!(ex is InvalidOperationException))
						{
							UnityEngine.Debug.LogWarning($"Failed to get memory from process when flushing performance info: {ex}");
							list2.Add(new ProcessInfo
							{
								Name = process.ProcessName,
								WorkingSet = -1L
							});
						}
					}
				}
				record.AddObject("other_servers", list2);
				record.AddField("other_server_count", list2.Count);
			}
			catch (Exception arg2)
			{
				UnityEngine.Debug.LogError($"Failed to log processes when flushing performance info: {arg2}");
			}
		}
		if (!isClient)
		{
			IEnumerable<HarmonyModInfo> harmonyMods = HarmonyLoader.GetHarmonyMods();
			record.AddObject("harmony_mods", harmonyMods);
			record.AddField("harmony_mod_count", harmonyMods.Count());
		}
		if (!isClient && NexusServer.Started)
		{
			record.AddField("nexus_endpoint", ConVar.Nexus.endpoint);
			record.AddField("nexus_zone_id", NexusServer.ZoneClient?.Zone?.ZoneId ?? (-1));
			record.AddField("nexus_zone_key", NexusServer.ZoneKey ?? "");
			record.AddField("nexus_controller", ConVar.Nexus.zoneController);
		}
		record.AddObject("hardware", Analytics.Azure.GetHardwareData()).AddObject("application", Analytics.Azure.GetApplicationData());
		stopwatch.Stop();
		record.AddField("flush_ms", stopwatch.ElapsedMilliseconds);
		List<TimeSpan> frametimes = Frametimes;
		List<int> ping = PingHistory;
		List<long> gpuFrameTimes = null;
		List<TimeSpan> mainMenuFrametimes = null;
		List<TimeSpan> inventoryFrametimes = null;
		List<TimeSpan> craftingFrametimes = null;
		List<TimeSpan> contactsFrametimes = null;
		List<TimeSpan> mapFrametimes = null;
		Task.Run(async delegate
		{
			try
			{
				await ProcessPerformanceData(record, frametimes, mainMenuFrametimes, inventoryFrametimes, craftingFrametimes, contactsFrametimes, mapFrametimes, gpuFrameTimes, ping);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		});
		ResetMeasurements();
	}

	private TimeSpan GetFlushInterval()
	{
		if (!isClient)
		{
			if (Analytics.Azure.GameplayAnalytics)
			{
				return ServerInterval;
			}
			return PublicServerInterval;
		}
		return TimeSpan.FromHours(1.0);
	}

	private void ResetMeasurements()
	{
		nextFlushTime = DateTime.UtcNow.Add(GetFlushInterval());
		if (Frametimes.Count != 0)
		{
			Frametimes = Pool.Get<List<TimeSpan>>();
			PingHistory = Pool.Get<List<int>>();
			lagSpikes.Clear();
			garbageCollections.Clear();
			nonGCLagSpikes = 0;
		}
	}

	private Task ProcessPerformanceData(EventRecord record, List<TimeSpan> frametimes, List<TimeSpan> mainMenuFrametimes, List<TimeSpan> inventoryFrametimes, List<TimeSpan> craftingFrametimes, List<TimeSpan> contactsFrametimes, List<TimeSpan> mapFrametimes, List<long> gpuFrameTimes, List<int> ping)
	{
		int num = frametimes.Count;
		if (isClient)
		{
			num += mainMenuFrametimes.Count;
			num += inventoryFrametimes.Count;
			num += craftingFrametimes.Count;
			num += contactsFrametimes.Count;
			num += mapFrametimes.Count;
		}
		if (num == 0)
		{
			return Task.CompletedTask;
		}
		ApendFrameTimes(record, "", frametimes);
		if (isClient)
		{
			ApendFrameTimes(record, "mainmenu_", mainMenuFrametimes);
			ApendFrameTimes(record, "inventory_", inventoryFrametimes);
			ApendFrameTimes(record, "crafting_", craftingFrametimes);
			ApendFrameTimes(record, "contacts_", contactsFrametimes);
			ApendFrameTimes(record, "map_", mapFrametimes);
		}
		record.AddField("gc_generations", System.GC.MaxGeneration).AddField("gc_total", System.GC.CollectionCount(System.GC.MaxGeneration));
		if (isClient)
		{
			record.AddField("ping_average", (ping.Count != 0) ? ((int)ping.Average()) : 0).AddField("ping_count", ping.Count);
		}
		record.Submit();
		Pool.FreeUnmanaged(ref frametimes);
		if (isClient)
		{
			Pool.FreeUnmanaged(ref mainMenuFrametimes);
			Pool.FreeUnmanaged(ref inventoryFrametimes);
			Pool.FreeUnmanaged(ref craftingFrametimes);
			Pool.FreeUnmanaged(ref contactsFrametimes);
			Pool.FreeUnmanaged(ref mapFrametimes);
		}
		Pool.FreeUnmanaged(ref ping);
		if (gpuFrameTimes != null)
		{
			Pool.FreeUnmanaged(ref gpuFrameTimes);
		}
		return Task.CompletedTask;
		static void ApendFrameTimes(EventRecord record, string prefix, List<TimeSpan> times)
		{
			if (times.Count <= 1)
			{
				return;
			}
			using PooledList<TimeSpan> pooledList = Pool.Get<PooledList<TimeSpan>>();
			pooledList.AddRange(times);
			pooledList.Sort();
			int count = times.Count;
			Mathf.Max(1, times.Count / 100);
			Mathf.Max(1, times.Count / 1000);
			TimeSpan value = default(TimeSpan);
			for (int i = 0; i < count; i++)
			{
				TimeSpan timeSpan = pooledList[i];
				value += timeSpan;
			}
			double frametime_average = value.TotalMilliseconds / (double)count;
			double value2 = System.Math.Sqrt(pooledList.Sum((TimeSpan x) => System.Math.Pow(x.TotalMilliseconds - frametime_average, 2.0)) / (double)(pooledList.Count - 1));
			record.AddLegacyTimespan(prefix + "total_time", value).AddField(prefix + "frames", count).AddField(prefix + "frametime_average", value.TotalSeconds / (double)count)
				.AddLegacyTimespan(prefix + "frametime_99_9", pooledList[Mathf.Clamp(count - count / 1000, 0, count - 1)])
				.AddLegacyTimespan(prefix + "frametime_99", pooledList[Mathf.Clamp(count - count / 100, 0, count - 1)])
				.AddLegacyTimespan(prefix + "frametime_90", pooledList[Mathf.Clamp(count - count / 10, 0, count - 1)])
				.AddLegacyTimespan(prefix + "frametime_75", pooledList[Mathf.Clamp(count - count / 4, 0, count - 1)])
				.AddLegacyTimespan(prefix + "frametime_50", pooledList[count / 2])
				.AddLegacyTimespan(prefix + "frametime_25", pooledList[count / 4])
				.AddLegacyTimespan(prefix + "frametime_10", pooledList[count / 10])
				.AddLegacyTimespan(prefix + "frametime_1", pooledList[count / 100])
				.AddLegacyTimespan(prefix + "frametime_0_1", pooledList[count / 1000])
				.AddField(prefix + "frametime_std_dev", value2);
		}
	}

	public void SetTiming(string category, TimeSpan elapsed)
	{
		pendingTimings[category] = (int)elapsed.TotalMilliseconds;
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Rust.Ai.Gen2;

public static class NavStressStats
{
	public static bool enabled;

	public static int setDestinationCalls;

	public static int setDestinationFails;

	public static int corridorInvalidations;

	public static int replans;

	public static int cornerRefreshes;

	public static int pathResets;

	public static int destinationPatches;

	private static readonly Stopwatch stopwatch = new Stopwatch();

	private static readonly Stopwatch driverStopwatch = new Stopwatch();

	private static readonly List<double> frameMs = new List<double>(32768);

	private static readonly List<double> driverMs = new List<double>(32768);

	private static long agentTicks;

	private static long allocBytes;

	private static long allocAtFrameStart;

	private static bool allocSupported = true;

	[Conditional("DEBUG")]
	public static void CountCornerRefresh()
	{
		cornerRefreshes++;
	}

	[Conditional("DEBUG")]
	public static void CountCorridorInvalidation()
	{
		corridorInvalidations++;
	}

	[Conditional("DEBUG")]
	public static void CountReplan()
	{
		replans++;
	}

	[Conditional("DEBUG")]
	public static void CountPathReset()
	{
		pathResets++;
	}

	[Conditional("DEBUG")]
	public static void CountDestinationPatch()
	{
		destinationPatches++;
	}

	[Conditional("DEBUG")]
	public static void FrameBegin()
	{
		if (!enabled)
		{
			return;
		}
		if (allocSupported)
		{
			try
			{
				allocAtFrameStart = System.GC.GetAllocatedBytesForCurrentThread();
			}
			catch (Exception)
			{
				allocSupported = false;
			}
		}
		stopwatch.Restart();
	}

	[Conditional("DEBUG")]
	public static void FrameEnd(int agentCount)
	{
		if (enabled)
		{
			stopwatch.Stop();
			if (frameMs.Count < 200000)
			{
				frameMs.Add(stopwatch.Elapsed.TotalMilliseconds);
				driverMs.Add(driverStopwatch.Elapsed.TotalMilliseconds);
			}
			driverStopwatch.Reset();
			agentTicks += agentCount;
			if (allocSupported)
			{
				allocBytes += System.GC.GetAllocatedBytesForCurrentThread() - allocAtFrameStart;
			}
		}
	}

	[Conditional("DEBUG")]
	public static void DriverCallBegin()
	{
		if (enabled)
		{
			driverStopwatch.Start();
		}
	}

	[Conditional("DEBUG")]
	public static void DriverCallEnd()
	{
		if (enabled)
		{
			driverStopwatch.Stop();
		}
	}

	public static void ResetSamples()
	{
		frameMs.Clear();
		driverMs.Clear();
		agentTicks = 0L;
		allocBytes = 0L;
		setDestinationCalls = 0;
		setDestinationFails = 0;
		corridorInvalidations = 0;
		replans = 0;
		cornerRefreshes = 0;
		pathResets = 0;
		destinationPatches = 0;
	}

	public static double MeanMs()
	{
		if (frameMs.Count == 0)
		{
			return 0.0;
		}
		double num = 0.0;
		for (int i = 0; i < frameMs.Count; i++)
		{
			num += frameMs[i];
		}
		return num / (double)frameMs.Count;
	}

	public static double MeanDriverMs()
	{
		if (driverMs.Count == 0)
		{
			return 0.0;
		}
		double num = 0.0;
		for (int i = 0; i < driverMs.Count; i++)
		{
			num += driverMs[i];
		}
		return num / (double)driverMs.Count;
	}

	public static string Report(int agentCount, string label)
	{
		int count = frameMs.Count;
		if (count == 0)
		{
			return "[" + label + "] no samples";
		}
		List<double> list = new List<double>(frameMs);
		list.Sort();
		double num = MeanMs();
		double num2 = MeanDriverMs();
		double num3 = list[count / 2];
		double num4 = list[Mathf.Min(count - 1, Mathf.FloorToInt((float)count * 0.95f))];
		double num5 = list[count - 1];
		double num6 = ((agentTicks > 0) ? (num * 1000.0 * (double)count / (double)agentTicks) : 0.0);
		double num7 = (double)allocBytes / (double)count;
		return $"[{label}] {agentCount} agents, {count} frames: tick {num * 1000.0:F1}us + driver {num2 * 1000.0:F1}us p50 {num3 * 1000.0:F1}us p95 {num4 * 1000.0:F1}us max {num5 * 1000.0:F1}us, {num6:F2}us/agent, alloc {num7:F0}B/frame, setDest {setDestinationCalls} ({setDestinationFails} fail, {destinationPatches} patched), corridorInvalid {corridorInvalidations}, replans {replans}, cornerPulls {cornerRefreshes}, pathResets {pathResets}";
	}
}

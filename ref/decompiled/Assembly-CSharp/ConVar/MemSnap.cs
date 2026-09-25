using System;
using System.IO;
using Unity.Profiling.Memory;

namespace ConVar;

[Factory("memsnap")]
public class MemSnap : ConsoleSystem
{
	private static string NeedProfileFolder()
	{
		string path = "profile/memory";
		if (!Directory.Exists(path))
		{
			return Directory.CreateDirectory(path).FullName;
		}
		return new DirectoryInfo(path).FullName;
	}

	[ClientVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing managed (C#) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	[ServerVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing managed (C#) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	public static void managed(Arg arg)
	{
		MemoryProfiler.TakeSnapshot(NeedProfileFolder() + "/memdump-" + DateTime.Now.ToString("MM-dd-yyyy-h-mm-ss") + ".snap", null, CaptureFlags.ManagedObjects);
	}

	[ServerVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing native (C++) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	[ClientVar(Help = "(Generated) Takes a Unity Memory Profiler snapshot capturing native (C++) heap allocations and saves it as a timestamped .snap file in the profile/memory/ folder")]
	public static void native(Arg arg)
	{
		MemoryProfiler.TakeSnapshot(NeedProfileFolder() + "/memdump-" + DateTime.Now.ToString("MM-dd-yyyy-h-mm-ss") + ".snap", null, CaptureFlags.NativeObjects);
	}

	[ServerVar(Help = "(Generated) Takes a full Unity Memory Profiler snapshot capturing all managed, native, and graphics memory and saves it as a timestamped .snap file in profile/memory/")]
	[ClientVar(Help = "(Generated) Takes a full Unity Memory Profiler snapshot capturing all managed, native, and graphics memory and saves it as a timestamped .snap file in profile/memory/")]
	public static void full(Arg arg)
	{
		MemoryProfiler.TakeSnapshot(NeedProfileFolder() + "/memdump-" + DateTime.Now.ToString("MM-dd-yyyy-h-mm-ss") + ".snap", null, CaptureFlags.ManagedObjects | CaptureFlags.NativeObjects | CaptureFlags.NativeAllocations | CaptureFlags.NativeAllocationSites | CaptureFlags.NativeStackTraces);
	}
}

using Rust;
using UnityEngine;
using UnityEngine.Scripting;

namespace ConVar;

[Factory("gc")]
public class GC : ConsoleSystem
{
	[ClientVar(Help = "(Generated) Enables the managed memory GC buffer, which pre-allocates a buffer to delay GC collections and reduce GC-induced frame hitches")]
	public static bool buffer_enabled = true;

	[ClientVar(Help = "(Generated) Controls the verbosity of GC debug output; higher values print more details about collection frequency and memory pressure")]
	public static int debuglevel = 1;

	[ClientVar(Saved = true, Help = "(Generated) Raw size in bytes of the GC buffer allocation; do not set directly — use gc.buffer_enabled and let the system manage allocation")]
	public static int buffer = Rust.GC.gcDefaultValue;

	public static int safeBuffer
	{
		get
		{
			return Rust.GC.GetSafeGCValue(buffer);
		}
		set
		{
			buffer = value;
		}
	}

	[ClientVar(Help = "(Generated) Read-only: reports whether Unity incremental garbage collection is enabled for this runtime; cannot be changed at runtime")]
	[ServerVar(Help = "(Generated) Read-only: reports whether Unity incremental garbage collection is enabled for this runtime; cannot be changed at runtime")]
	public static bool incremental_enabled
	{
		get
		{
			return GarbageCollector.isIncremental;
		}
		set
		{
			Debug.LogWarning("Cannot set gc.incremental as it is read only");
		}
	}

	[ServerVar(Help = "(Generated) Time slice in milliseconds allocated to incremental GC per frame; lower values reduce GC stutter but spread collection over more frames")]
	[ClientVar(Help = "(Generated) Time slice in milliseconds allocated to incremental GC per frame; lower values reduce GC stutter but spread collection over more frames")]
	public static int incremental_milliseconds
	{
		get
		{
			return (int)(GarbageCollector.incrementalTimeSliceNanoseconds / 1000000);
		}
		set
		{
			GarbageCollector.incrementalTimeSliceNanoseconds = 1000000uL * (ulong)Mathf.Max(value, 0);
		}
	}

	[ClientVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	[ServerVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	public static bool enabled
	{
		get
		{
			return Rust.GC.Enabled;
		}
		set
		{
			Debug.LogWarning("Cannot set gc.enabled as it is read only");
		}
	}

	[ClientVar(Help = "(Generated) Triggers an immediate full managed garbage collection pass; useful after large allocations for memory profiling")]
	[ServerVar(Help = "(Generated) Triggers an immediate full managed garbage collection pass; useful after large allocations for memory profiling")]
	public static void collect()
	{
		Rust.GC.Collect();
	}

	[ServerVar(Help = "(Generated) Calls Resources.UnloadUnusedAssets() to unload assets no longer referenced by any object, freeing RAM and VRAM")]
	[ClientVar(Help = "(Generated) Calls Resources.UnloadUnusedAssets() to unload assets no longer referenced by any object, freeing RAM and VRAM")]
	public static void unload()
	{
		Resources.UnloadUnusedAssets();
	}

	[ClientVar(Help = "(Generated) Allocates a byte array of the given size (default 1 MB) as a GC pressure test; useful for profiling memory allocation throughput")]
	[ServerVar(Help = "(Generated) Allocates a byte array of the given size (default 1 MB) as a GC pressure test; useful for profiling memory allocation throughput")]
	public static void alloc(Arg args)
	{
		byte[] array = new byte[args.GetInt(0, 1048576)];
		args.ReplyWith("Allocated " + array.Length + " bytes");
	}
}

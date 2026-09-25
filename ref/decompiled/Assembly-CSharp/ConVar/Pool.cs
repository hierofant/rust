using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using Network;
using Network.Relay;
using UnityEngine;

namespace ConVar;

[Factory("pool")]
public class Pool : ConsoleSystem
{
	[ClientVar(ClientAdmin = true)]
	[ServerVar(Help = "(Generated) When enabled, object pools are pre-allocated at startup to avoid first-use latency; increases startup time but reduces runtime GC stutter")]
	public static int mode = 2;

	[ClientVar(Help = "(Generated) When enabled, object pools are pre-allocated at startup to avoid first-use latency; increases startup time but reduces runtime GC stutter")]
	[ServerVar(Help = "(Generated) When enabled, object pools are pre-allocated at startup to avoid first-use latency; increases startup time but reduces runtime GC stutter")]
	public static bool prewarm = true;

	[ClientVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	[ServerVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	public static bool enabled = true;

	[ServerVar(Help = "(Generated) When enabled, logs additional diagnostic information about pool hits, misses, and spills to the console")]
	[ClientVar(Help = "(Generated) When enabled, logs additional diagnostic information about pool hits, misses, and spills to the console")]
	public static bool debug = false;

	[ServerVar(Help = "Whether to use original pool implementation (slower, but tested). Default is false")]
	[ClientVar(Help = "Whether to use original pool implementation (slower, but tested). Default is false")]
	public static bool UseMutexPool
	{
		get
		{
			return Facepunch.Pool.UseMutexPool;
		}
		set
		{
			Facepunch.Pool.UseMutexPool = value;
		}
	}

	[ClientVar(Help = "(Generated) Prints a table of all object pool entries showing type, capacity, active count, peak usage, hit/miss counts, and spill counts; supports --json")]
	[ServerVar(Help = "(Generated) Prints a table of all object pool entries showing type, capacity, active count, peak usage, hit/miss counts, and spill counts; supports --json")]
	public static void print_memory(Arg arg)
	{
		if (Facepunch.Pool.Directory.Count == 0)
		{
			arg.ReplyWith("Memory pool is empty.");
			return;
		}
		bool flag = arg.HasArg("--raw", remove: true);
		bool flag2 = arg.HasArg("--json", remove: true);
		string @string = arg.GetString(0, null);
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag2;
		textTable.AddColumn("type");
		textTable.AddColumn("capacity");
		textTable.AddColumn("pooled");
		textTable.AddColumn("active");
		textTable.AddColumn("max");
		textTable.AddColumn("hits");
		textTable.AddColumn("misses");
		textTable.AddColumn("spills");
		foreach (KeyValuePair<Type, Facepunch.Pool.IPoolCollection> item in Facepunch.Pool.Directory.OrderByDescending((KeyValuePair<Type, Facepunch.Pool.IPoolCollection> x) => x.Value.ItemsCreated))
		{
			Type key = item.Key;
			Facepunch.Pool.IPoolCollection value = item.Value;
			if (@string == null || key.ToString().Contains(@string))
			{
				textTable.AddRow(key.ToString().Replace("System.Collections.Generic.", ""), flag ? value.ItemsCapacity.ToString() : value.ItemsCapacity.FormatNumberShort(), flag ? value.ItemsInStack.ToString() : value.ItemsInStack.FormatNumberShort(), flag ? value.ItemsInUse.ToString() : value.ItemsInUse.FormatNumberShort(), flag ? value.MaxItemsInUse.ToString() : value.MaxItemsInUse.FormatNumberShort(), flag ? value.ItemsTaken.ToString() : value.ItemsTaken.FormatNumberShort(), flag ? value.ItemsCreated.ToString() : value.ItemsCreated.FormatNumberShort(), flag ? value.ItemsSpilled.ToString() : value.ItemsSpilled.FormatNumberShort());
			}
		}
		arg.ReplyWith(flag2 ? textTable.ToJson() : textTable.ToString());
	}

	[ClientVar(Help = "(Generated) Resets the peak-usage high-water-mark counter for all pools, allowing fresh measurement of maximum pool demand")]
	[ServerVar(Help = "(Generated) Resets the peak-usage high-water-mark counter for all pools, allowing fresh measurement of maximum pool demand")]
	public static void reset_max_pool_counter(Arg arg)
	{
		if (Facepunch.Pool.Directory.Count == 0)
		{
			arg.ReplyWith("Memory pool is empty.");
			return;
		}
		foreach (Facepunch.Pool.IPoolCollection value in Facepunch.Pool.Directory.Values)
		{
			value.ResetMaxUsageCounter();
		}
		arg.ReplyWith("Reset max item counter of pool");
	}

	[ClientVar(Help = "(Generated) Prints a usage report for the BaseNetwork and ProtocolParser array pools, showing bucket sizes, capacities, and hit/miss stats")]
	[ServerVar(Help = "(Generated) Prints a usage report for the BaseNetwork and ProtocolParser array pools, showing bucket sizes, capacities, and hit/miss stats")]
	public static void print_arraypool(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		string text = (flag ? "[" : string.Empty);
		string table2 = PrintArrayPool<byte>(BaseNetwork.ArrayPool, flag);
		text += FormatTable("BaseNetwork.ArrayPool", table2, flag);
		text += (flag ? "," : "\n");
		string table3 = PrintArrayPool<byte>(BufferStream.Shared.ArrayPool, flag);
		text += FormatTable("ProtocolParser.ArrayPool", table3, flag);
		text += (flag ? "," : "\n");
		string table4 = PrintArrayPool<byte>(RustRelay.PacketArrayPool, flag);
		text += FormatTable("RustRelay.PacketArrayPool", table4, flag);
		if (flag)
		{
			text += "]";
		}
		arg.ReplyWith(text);
		static string FormatTable(string name, string table, bool toJson)
		{
			if (!toJson)
			{
				return name + "\n" + table;
			}
			return "{\"name\":\"" + name + "\",\"content\":" + table + "}";
		}
		unsafe static string PrintArrayPool<T>(ArrayPool<T> pool, bool toJson) where T : unmanaged
		{
			ConcurrentQueue<T[]>[] buffer = pool.GetBuffer();
			using TextTable textTable = Facepunch.Pool.Get<TextTable>();
			textTable.ShouldPadColumns = !toJson;
			textTable.ResizeColumns(5);
			textTable.AddColumn("index");
			textTable.AddColumn("size");
			textTable.AddColumn("bytes");
			textTable.AddColumn("count");
			textTable.AddColumn("memory");
			textTable.ResizeRows(buffer.Length);
			int num = 1;
			num = sizeof(T);
			for (int i = 0; i < buffer.Length; i++)
			{
				int num2 = pool.IndexToSize(i);
				int num3 = num2 * num;
				int count = buffer[i].Count;
				int input = num3 * count;
				textTable.AddValue(i);
				textTable.AddValue(num2);
				textTable.AddValue(num2.FormatBytes());
				textTable.AddValue(count);
				textTable.AddValue(input.FormatBytes());
			}
			return toJson ? textTable.ToJson(stringify: false) : textTable.ToString();
		}
	}

	[ServerVar(Help = "(Generated) Prints a table of all prefab pool entries showing prefab name, miss count, current count, target capacity, and push/pop counts; supports --json")]
	[ClientVar(Help = "(Generated) Prints a table of all prefab pool entries showing prefab name, miss count, current count, target capacity, and push/pop counts; supports --json")]
	public static void print_prefabs(Arg arg)
	{
		PrefabPoolCollection pool = GameManager.server.pool;
		if (pool.storage.Count == 0)
		{
			arg.ReplyWith("Prefab pool is empty.");
			return;
		}
		string @string = arg.GetString(0, string.Empty);
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.AddColumn("id");
		textTable.AddColumn("name");
		textTable.AddColumn("missed");
		textTable.AddColumn("count");
		textTable.AddColumn("target");
		textTable.AddColumn("added");
		textTable.AddColumn("removed");
		foreach (PrefabPool item in pool.storage.Values.OrderByDescending((PrefabPool x) => x.Missed))
		{
			string text = StringPool.Get(item.PrefabName).ToString();
			string prefabName = item.PrefabName;
			string text2 = item.Count.ToString();
			if (string.IsNullOrEmpty(@string) || prefabName.Contains(@string, CompareOptions.IgnoreCase))
			{
				textTable.AddRow(text, Path.GetFileNameWithoutExtension(prefabName), text2, item.TargetCapacity.ToString(), item.Missed.ToString(), item.Pushed.ToString(), item.Popped.ToString());
			}
		}
		arg.ReplyWith(flag ? textTable.ToJson() : textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Prints a table of all asset pool entries showing asset type, current pooled count, and pool capacity")]
	[ClientVar(Help = "(Generated) Prints a table of all asset pool entries showing asset type, current pooled count, and pool capacity")]
	public static void print_assets(Arg arg)
	{
		if (AssetPool.storage.Count == 0)
		{
			arg.ReplyWith("Asset pool is empty.");
			return;
		}
		string @string = arg.GetString(0, string.Empty);
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.AddColumn("type");
		textTable.AddColumn("allocated");
		textTable.AddColumn("available");
		foreach (KeyValuePair<Type, AssetPool.Pool> item in AssetPool.storage)
		{
			string text = item.Key.ToString();
			string text2 = item.Value.allocated.ToString();
			string text3 = item.Value.available.ToString();
			if (string.IsNullOrEmpty(@string) || text.Contains(@string, CompareOptions.IgnoreCase))
			{
				textTable.AddRow(text, text2, text3);
			}
		}
		arg.ReplyWith(flag ? textTable.ToJson() : textTable.ToString());
	}

	[ServerVar(Help = "(Generated) Clears all entries from the object memory pool matching the optional name filter; freed pooled objects are garbage collected")]
	[ClientVar(Help = "(Generated) Clears all entries from the object memory pool matching the optional name filter; freed pooled objects are garbage collected")]
	public static void clear_memory(Arg arg)
	{
		Facepunch.Pool.Clear(arg.GetString(0, string.Empty));
	}

	[ServerVar(Help = "(Generated) Clears all cached prefab instances from the prefab pool matching the optional filter, across client, server, and generic pools")]
	[ClientVar(Help = "(Generated) Clears all cached prefab instances from the prefab pool matching the optional filter, across client, server, and generic pools")]
	public static void clear_prefabs(Arg arg)
	{
		string @string = arg.GetString(0, string.Empty);
		GameManager.server.pool.Clear(@string);
	}

	[ClientVar(Help = "(Generated) Clears all cached entries from the asset pool matching the optional name filter")]
	[ServerVar(Help = "(Generated) Clears all cached entries from the asset pool matching the optional name filter")]
	public static void clear_assets(Arg arg)
	{
		AssetPool.Clear(arg.GetString(0, string.Empty));
	}

	[ClientVar(Help = "(Generated) Exports the current prefab pool contents to a prefabs.csv file listing pool ID, prefab short name, and instance count")]
	[ServerVar(Help = "(Generated) Exports the current prefab pool contents to a prefabs.csv file listing pool ID, prefab short name, and instance count")]
	public static void export_prefabs(Arg arg)
	{
		PrefabPoolCollection pool = GameManager.server.pool;
		if (pool.storage.Count == 0)
		{
			arg.ReplyWith("Prefab pool is empty.");
			return;
		}
		string @string = arg.GetString(0, string.Empty);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<uint, PrefabPool> item in pool.storage)
		{
			string arg2 = item.Key.ToString();
			string text = StringPool.Get(item.Key);
			string arg3 = item.Value.Count.ToString();
			if (string.IsNullOrEmpty(@string) || text.Contains(@string, CompareOptions.IgnoreCase))
			{
				stringBuilder.AppendLine($"{arg2},{Path.GetFileNameWithoutExtension(text)},{arg3}");
			}
		}
		File.WriteAllText("prefabs.csv", stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Pre-warms the prefab pool by instantiating and pooling prefabs matching the optional filter up to the given count override")]
	[ClientVar(Help = "(Generated) Pre-warms the prefab pool by instantiating and pooling prefabs matching the optional filter up to the given count override")]
	public static void fill_prefabs(Arg arg)
	{
		string @string = arg.GetString(0, string.Empty);
		int @int = arg.GetInt(1);
		PrefabPoolWarmup.Run(@string, @int);
	}
}

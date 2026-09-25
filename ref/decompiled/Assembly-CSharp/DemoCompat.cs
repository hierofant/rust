using System.Collections.Generic;
using ConVar;
using ProtoBuf;
using UnityEngine;

public static class DemoCompat
{
	public const string COMPAT_MANIFEST_PATH = "Assets/demo_compat_manifest.asset";

	public const int DEFAULT_RETENTION_DAYS = 120;

	private static DemoCompatManifest _compatManifest;

	private static Dictionary<uint, string> _deprecatedHashToPath;

	private static Dictionary<uint, string> _remapHashToNewPath;

	private static bool _lookupBuilt = false;

	private static bool _active;

	private static bool? _compatOverride;

	private static readonly Dictionary<uint, string> _resolvedPrefabs = new Dictionary<uint, string>();

	private static readonly Dictionary<uint, uint> _resolvedRPCs = new Dictionary<uint, uint>();

	public static bool IsActive => _active;

	private static void BuildLookups()
	{
		if (_lookupBuilt)
		{
			return;
		}
		_compatManifest = DemoCompatManifest.Load();
		_deprecatedHashToPath = new Dictionary<uint, string>();
		if (_compatManifest?.entries != null)
		{
			DemoCompatManifest.DeprecatedEntry[] entries = _compatManifest.entries;
			for (int i = 0; i < entries.Length; i++)
			{
				DemoCompatManifest.DeprecatedEntry deprecatedEntry = entries[i];
				_deprecatedHashToPath.TryAdd(deprecatedEntry.hash, deprecatedEntry.str);
			}
		}
		_remapHashToNewPath = new Dictionary<uint, string>();
		if (_compatManifest?.remaps != null)
		{
			DemoCompatManifest.RemapEntry[] remaps = _compatManifest.remaps;
			for (int i = 0; i < remaps.Length; i++)
			{
				DemoCompatManifest.RemapEntry remapEntry = remaps[i];
				_remapHashToNewPath.TryAdd(remapEntry.oldHash, remapEntry.newPath);
			}
		}
		_lookupBuilt = true;
	}

	public static void OverrideNextPlayback(bool enabled)
	{
		_compatOverride = enabled;
	}

	public static void BeginPlayback(DemoHeader header)
	{
		BeginPlayback(RequiresCompat(header), header);
	}

	public static void BeginPlayback()
	{
		BeginPlayback(requiresCompat: true, null);
	}

	private static void BeginPlayback(bool requiresCompat, DemoHeader header)
	{
		_resolvedPrefabs.Clear();
		_resolvedRPCs.Clear();
		bool flag = _compatOverride ?? Demo.compatibilitylayer;
		_compatOverride = null;
		_active = flag && requiresCompat;
		LogCompatibilityInfo(header, flag);
	}

	public static void EndPlayback()
	{
		_active = false;
		_resolvedPrefabs.Clear();
		_resolvedRPCs.Clear();
	}

	public static string ResolvePrefab(uint prefabID)
	{
		if (!_active)
		{
			return StringPool.Get(prefabID);
		}
		if (_resolvedPrefabs.TryGetValue(prefabID, out var value))
		{
			return value;
		}
		string text = ResolvePrefabInternal(prefabID);
		_resolvedPrefabs.Add(prefabID, text);
		return text;
	}

	private static string ResolvePrefabInternal(uint prefabID)
	{
		if (StringPool.TryGet(prefabID, out var str) && FileSystem.HasAsset(str))
		{
			return str;
		}
		BuildLookups();
		if (_remapHashToNewPath.TryGetValue(prefabID, out var value))
		{
			if (FileSystem.HasAsset(value))
			{
				Debug.Log($"[DemoCompat] Remapped hash {prefabID} → '{value}' (GUID match)");
				return value;
			}
			Debug.LogWarning($"[DemoCompat] Remap target '{value}' for hash {prefabID} no longer exists — skipping");
			return string.Empty;
		}
		if (_deprecatedHashToPath.TryGetValue(prefabID, out var value2))
		{
			Debug.LogWarning($"[DemoCompat] Could not resolve deprecated prefab '{value2}' (hash: {prefabID}) — no GUID remap found");
		}
		else
		{
			Debug.LogWarning($"[DemoCompat] Could not resolve prefab hash {prefabID} — unknown to StringPool and the compat manifest");
		}
		return string.Empty;
	}

	public static uint RemapRPC(uint rpcHash)
	{
		if (!_active)
		{
			return rpcHash;
		}
		if (_resolvedRPCs.TryGetValue(rpcHash, out var value))
		{
			return value;
		}
		uint num = RemapRPCInternal(rpcHash);
		_resolvedRPCs.Add(rpcHash, num);
		return num;
	}

	private static uint RemapRPCInternal(uint rpcHash)
	{
		if (StringPool.TryGet(rpcHash, out var _))
		{
			return rpcHash;
		}
		BuildLookups();
		if (_deprecatedHashToPath.TryGetValue(rpcHash, out var value))
		{
			uint num = StringPool.Get(value);
			if (num != 0)
			{
				return num;
			}
			Debug.LogWarning($"[DemoCompat] RPC '{value}' (hash: {rpcHash}) no longer exists — skipping");
		}
		else
		{
			Debug.LogWarning($"[DemoCompat] Could not remap RPC hash {rpcHash} — skipping");
		}
		return 0u;
	}

	private static void LogCompatibilityInfo(DemoHeader header, bool enabled)
	{
		if (!enabled)
		{
			Debug.Log("[DemoCompat] Compatibility layer is DISABLED for this playback.");
			return;
		}
		if (!_active)
		{
			Debug.Log("[DemoCompat] Demo manifest matches current build — no remapping needed.");
			return;
		}
		BuildLookups();
		int count = _deprecatedHashToPath.Count;
		if (header == null || header.manifestCRC == 0)
		{
			Debug.Log("[DemoCompat] Demo has no manifest CRC — compatibility layer active as precaution.");
		}
		else
		{
			Debug.Log($"[DemoCompat] Demo manifest CRC {header.manifestCRC} differs from current {GameManifest.CRC} — compatibility layer active.");
		}
		if (count > 0)
		{
			Debug.Log($"[DemoCompat] Compat manifest has {count} deprecated entries available.");
		}
	}

	public static bool RequiresCompat(DemoHeader header)
	{
		if (header == null || header.manifestCRC == 0)
		{
			return true;
		}
		return header.manifestCRC != GameManifest.CRC;
	}

	public static void Reset()
	{
		_compatManifest = null;
		_deprecatedHashToPath = null;
		_remapHashToNewPath = null;
		_lookupBuilt = false;
		_resolvedPrefabs.Clear();
		_resolvedRPCs.Clear();
	}
}

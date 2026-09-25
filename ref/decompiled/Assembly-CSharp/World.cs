using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

[ResetStaticFields]
public static class World
{
	public struct SpawnTiming
	{
		public string category;

		public Prefab prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public TimeSpan time;
	}

	private static uint _size;

	public static readonly Dictionary<string, HashSet<GameObject>> SpawnedPrefabs = new Dictionary<string, HashSet<GameObject>>(StringComparer.OrdinalIgnoreCase);

	private static Stopwatch spawnTimer = new Stopwatch();

	private static List<SpawnTiming> spawnTimings = new List<SpawnTiming>();

	public static uint Seed { get; set; }

	public static uint Salt { get; set; }

	public static uint Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	public static string Checksum { get; set; }

	public static long Timestamp { get; set; }

	public static string Url { get; set; }

	public static bool Procedural { get; set; }

	public static bool Cached { get; set; }

	public static bool Networked { get; set; }

	public static bool Receiving { get; set; }

	public static bool Transfer { get; set; }

	public static bool Nexus => NexusServer.Started;

	public static bool LoadedFromSave { get; set; }

	public static int SpawnIndex { get; set; }

	public static WorldSerialization Serialization { get; set; }

	public static WorldConfig Config { get; set; }

	public static string Name
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (MapUploader.IsUploaded)
			{
				return MapUploader.OriginalName;
			}
			if (CanLoadFromUrl())
			{
				return Path.GetFileNameWithoutExtension(WWW.UnEscapeURL(Url));
			}
			return UnityEngine.Application.loadedLevelName;
		}
	}

	public static string MapFileName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (MapUploader.IsUploaded)
			{
				return MapUploader.OriginalMapFileName;
			}
			if (CanLoadFromUrl())
			{
				return $"{Name}_{Url.MurmurHashUnsigned()}.map";
			}
			return Name.Replace(" ", "").ToLower() + "." + Size + "." + Seed + "." + 288 + ".map";
		}
	}

	public static string MapFolderName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return Server.rootFolder;
		}
	}

	public static string SaveFileName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			if (MapUploader.IsUploaded)
			{
				return MapUploader.OriginalSaveFileName;
			}
			if (CanLoadFromUrl())
			{
				return Name + "." + 288 + ".sav";
			}
			return Name.Replace(" ", "").ToLower() + "." + Size + "." + Seed + "." + 288 + ".sav";
		}
	}

	public static string SaveFolderName
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return Server.rootFolder;
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static string GetServerBrowserMapName()
	{
		if (MapUploader.IsUploaded)
		{
			return Name;
		}
		if (!CanLoadFromUrl())
		{
			return Name;
		}
		if (Name.StartsWith("proceduralmap."))
		{
			return "Procedural Map";
		}
		return "Custom Map";
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool CanLoadFromUrl()
	{
		return !string.IsNullOrEmpty(Url);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public static bool CanLoadFromDisk()
	{
		return File.Exists(MapFolderName + "/" + MapFileName);
	}

	public static void CleanupOldFiles()
	{
		if (!Directory.Exists(MapFolderName))
		{
			return;
		}
		Regex regex1 = new Regex("proceduralmap\\.[0-9]+\\.[0-9]+\\.[0-9]+(_occlusion)*\\.(map|dat)");
		Regex regex2 = new Regex("\\.[0-9]+\\.[0-9]+\\." + 288 + "+(_occlusion)*\\.(map|dat)");
		foreach (string item in new string[2] { "*.map", "*.dat" }.SelectMany((string ext) => from path in Directory.GetFiles(MapFolderName, ext)
			where regex1.IsMatch(path) && !regex2.IsMatch(path)
			select path))
		{
			try
			{
				File.Delete(item);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError(ex.Message);
			}
		}
	}

	public static void InitSeed(int seed)
	{
		InitSeed((uint)seed);
	}

	public static void InitSeed(uint seed)
	{
		if (seed == 0)
		{
			seed = SeedIdentifier().MurmurHashUnsigned() % int.MaxValue;
		}
		if (seed == 0)
		{
			seed = 123456u;
		}
		Seed = seed;
		Server.seed = (int)seed;
	}

	private static string SeedIdentifier()
	{
		return SystemInfo.deviceUniqueIdentifier + "_" + 288 + "_" + Server.identity;
	}

	public static void InitSalt(int salt)
	{
		InitSalt((uint)salt);
	}

	public static void InitSalt(uint salt)
	{
		if (salt == 0)
		{
			salt = SaltIdentifier().MurmurHashUnsigned() % int.MaxValue;
		}
		if (salt == 0)
		{
			salt = 654321u;
		}
		Salt = salt;
		Server.salt = (int)salt;
	}

	private static string SaltIdentifier()
	{
		return SystemInfo.deviceUniqueIdentifier + "_salt";
	}

	public static void InitSize(int size)
	{
		InitSize((uint)size);
	}

	public static void InitSize(uint size)
	{
		if (size == 0)
		{
			size = 4500u;
		}
		if (size < 1000)
		{
			size = 1000u;
		}
		if (size > 6000)
		{
			size = 6000u;
		}
		Size = size;
		Server.worldsize = (int)size;
	}

	public static byte[] GetMap(string name)
	{
		return Serialization.GetMap(name)?.data;
	}

	public static int GetCachedHeightMapResolution()
	{
		return Mathf.RoundToInt(Mathf.Sqrt(GetMap("height").Length / 2));
	}

	public static int GetCachedSplatMapResolution()
	{
		return Mathf.RoundToInt(Mathf.Sqrt(GetMap("splat").Length / 8));
	}

	public static void AddMap(string name, byte[] data)
	{
		Serialization.AddMap(name, data);
	}

	public static void AddPrefab(string category, Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		Serialization.AddPrefab(category, prefab.ID, position, rotation, scale);
		if (!Cached)
		{
			rotation = Quaternion.Euler(rotation.eulerAngles);
			SpawnPrefab(category, prefab, position, rotation, scale);
		}
	}

	public static PathData PathListToPathData(PathList src)
	{
		return new PathData
		{
			name = src.Name,
			spline = src.Spline,
			start = src.Start,
			end = src.End,
			width = src.Width,
			innerPadding = src.InnerPadding,
			outerPadding = src.OuterPadding,
			innerFade = src.InnerFade,
			outerFade = src.OuterFade,
			randomScale = src.RandomScale,
			meshOffset = src.MeshOffset,
			terrainOffset = src.TerrainOffset,
			splat = src.Splat,
			topology = src.Topology,
			hierarchy = src.Hierarchy,
			nodes = VectorArrayToList(src.Path.Points)
		};
	}

	public static PathList PathDataToPathList(PathData src)
	{
		PathList pathList = new PathList(src.name, VectorListToArray(src.nodes));
		pathList.Spline = src.spline;
		pathList.Start = src.start;
		pathList.End = src.end;
		pathList.Width = src.width;
		pathList.InnerPadding = src.innerPadding;
		pathList.OuterPadding = src.outerPadding;
		pathList.InnerFade = src.innerFade;
		pathList.OuterFade = src.outerFade;
		pathList.RandomScale = src.randomScale;
		pathList.MeshOffset = src.meshOffset;
		pathList.TerrainOffset = src.terrainOffset;
		pathList.Splat = src.splat;
		pathList.Topology = src.topology;
		pathList.Hierarchy = src.hierarchy;
		pathList.Path.RecalculateTangents();
		return pathList;
	}

	public static Vector3[] VectorListToArray(List<VectorData> src)
	{
		Vector3[] array = new Vector3[src.Count];
		for (int i = 0; i < array.Length; i++)
		{
			VectorData vectorData = src[i];
			Vector3 vector = default(Vector3);
			vector.x = vectorData.x;
			vector.y = vectorData.y;
			vector.z = vectorData.z;
			array[i] = vector;
		}
		return array;
	}

	public static List<VectorData> VectorArrayToList(Vector3[] src)
	{
		List<VectorData> list = new List<VectorData>(src.Length);
		for (int i = 0; i < src.Length; i++)
		{
			Vector3 vector = src[i];
			VectorData item = default(VectorData);
			item.x = vector.x;
			item.y = vector.y;
			item.z = vector.z;
			list.Add(item);
		}
		return list;
	}

	public static IEnumerable<PathList> GetPaths(string name)
	{
		return from p in Serialization.GetPaths(name)
			select PathDataToPathList(p);
	}

	public static void AddPaths(IEnumerable<PathList> paths)
	{
		foreach (PathList path in paths)
		{
			AddPath(path);
		}
	}

	public static void AddPath(PathList path)
	{
		Serialization.AddPath(PathListToPathData(path));
	}

	public static IEnumerator Spawn(float deltaTime, Action<string> statusFunction = null, CancellationToken ct = default(CancellationToken))
	{
		FileSystemBackend backend = FileSystem.Backend;
		if (backend is AssetBundleBackend assetBundleBackend)
		{
			HashSet<string> prefabPaths = Serialization.world.prefabs.Select((PrefabData p) => StringPool.Get(p.id)).ToHashSet(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, HashSet<string>> requiredAssetScenesForPrefabs = assetBundleBackend.GetRequiredAssetScenesForPrefabs(prefabPaths);
			List<string> requiredAssetScenes = requiredAssetScenesForPrefabs.Keys.ToList();
			IEnumerator loading = assetBundleBackend.LoadAssetScenes(requiredAssetScenes);
			List<string> list = new List<string>();
			if (requiredAssetScenesForPrefabs.TryGetValue("AssetScene-props.common", out var value))
			{
				list.AddRange(value);
			}
			if (requiredAssetScenesForPrefabs.TryGetValue("AssetScene-props.other", out var value2))
			{
				list.AddRange(value2);
			}
			EventRecord.New("world_prop_usage", isServer: false).AddField("server", Facepunch.Application.Integration.ServerAddress).AddField("map_filename", MapFileName)
				.AddObject("required_props", from p in list
					select p.ToLowerInvariant() into p
					orderby p
					select p)
				.Submit();
			bool wantsCancel = false;
			float lastProgress = 0f;
			while (loading.MoveNext())
			{
				if (!wantsCancel && ct.IsCancellationRequested)
				{
					wantsCancel = true;
					UnityEngine.Debug.LogWarning("Cancel was requested but must wait for asset scenes to finish loading");
				}
				float assetSceneProgress = assetBundleBackend.GetAssetSceneProgress(requiredAssetScenes);
				if (!Mathf.Approximately(assetSceneProgress, lastProgress))
				{
					lastProgress = assetSceneProgress;
					Status(statusFunction, $"Loading World Prefabs {assetSceneProgress * 100f:0.0}%");
				}
				yield return loading.Current;
			}
		}
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < Serialization.world.prefabs.Count; i++)
		{
			if (ct.IsCancellationRequested)
			{
				break;
			}
			if (sw.Elapsed.TotalSeconds > (double)deltaTime || i == 0 || i == Serialization.world.prefabs.Count - 1)
			{
				Status(statusFunction, "Spawning World ({0}/{1})", i + 1, Serialization.world.prefabs.Count);
				yield return CoroutineEx.waitForEndOfFrame;
				sw.Reset();
				sw.Start();
			}
			SpawnPrefabData(Serialization.world.prefabs[i]);
		}
	}

	public static void Spawn(Bounds? prefabBounds)
	{
		for (int i = 0; i < Serialization.world.prefabs.Count; i++)
		{
			PrefabData prefabData = Serialization.world.prefabs[i];
			if (!prefabBounds.HasValue || prefabBounds.Value.Contains((Vector3)prefabData.position))
			{
				SpawnPrefabData(prefabData);
			}
		}
	}

	public static void Spawn(string category, string folder = null)
	{
		for (int i = SpawnIndex; i < Serialization.world.prefabs.Count; i++)
		{
			PrefabData prefabData = Serialization.world.prefabs[i];
			if (!(prefabData.category != category))
			{
				string text = StringPool.Get(prefabData.id);
				if (string.IsNullOrEmpty(folder) || text.StartsWith(folder))
				{
					SpawnPrefabData(prefabData);
					SpawnIndex++;
					continue;
				}
				break;
			}
			break;
		}
	}

	public static void Spawn(string category, string[] folders, GameObjectRef[] gameObjectRefs = null)
	{
		int i = SpawnIndex;
		for (int count = Serialization.world.prefabs.Count; i < count; i++)
		{
			PrefabData prefabData = Serialization.world.prefabs[i];
			if (prefabData.category != category)
			{
				break;
			}
			bool flag = false;
			if (folders.Length != 0)
			{
				flag = StringPool.Get(prefabData.id).StartsWithAny(folders);
			}
			if (gameObjectRefs != null && !flag)
			{
				foreach (GameObjectRef gameObjectRef in gameObjectRefs)
				{
					if (gameObjectRef != null && gameObjectRef.isValid && prefabData.id == gameObjectRef.resourceID)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				SpawnPrefabData(prefabData);
				SpawnIndex++;
				continue;
			}
			break;
		}
	}

	private static void SpawnPrefabData(PrefabData prefab)
	{
		StringPool.Get(prefab.id);
		uint id = prefab.id;
		SpawnPrefab(prefab.category, Prefab.Load(id), prefab.position, prefab.rotation, prefab.scale);
	}

	private static void SpawnPrefab(string category, Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (prefab != null && (bool)prefab.Object)
		{
			spawnTimer.Restart();
			if (!Cached)
			{
				prefab.ApplyTerrainPlacements(position, rotation, scale);
				prefab.ApplyTerrainModifiers(position, rotation, scale);
			}
			GameObject gameObject = prefab.Spawn(position, rotation, scale);
			TrackSpawnedPrefab(category, gameObject);
			Interface.CallHook("OnWorldPrefabSpawned", gameObject, category);
			spawnTimer.Stop();
			spawnTimings.Add(new SpawnTiming
			{
				category = category,
				prefab = prefab,
				position = position,
				rotation = rotation,
				scale = scale,
				time = spawnTimer.Elapsed
			});
		}
	}

	public static void TrackSpawnedPrefab(string category, GameObject instance)
	{
		if (!string.IsNullOrEmpty(category) && !(instance == null))
		{
			if (!SpawnedPrefabs.TryGetValue(category, out var value))
			{
				value = new HashSet<GameObject>();
				SpawnedPrefabs[category] = value;
			}
			value.Add(instance);
		}
	}

	private static void Status(Action<string> statusFunction, string status, object obj1)
	{
		statusFunction?.Invoke(string.Format(status, obj1));
	}

	private static void Status(Action<string> statusFunction, string status, object obj1, object obj2)
	{
		statusFunction?.Invoke(string.Format(status, obj1, obj2));
	}

	private static void Status(Action<string> statusFunction, string status, object obj1, object obj2, object obj3)
	{
		statusFunction?.Invoke(string.Format(status, obj1, obj2, obj3));
	}

	private static void Status(Action<string> statusFunction, string status, params object[] objs)
	{
		statusFunction?.Invoke(string.Format(status, objs));
	}

	public static IEnumerable<SpawnTiming> GetSpawnTimings()
	{
		return spawnTimings;
	}

	public static void ResetTiming()
	{
		spawnTimings.Clear();
	}
}

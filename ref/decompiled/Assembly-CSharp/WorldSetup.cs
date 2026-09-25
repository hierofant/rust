using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using ConVar;
using Rust;
using UnityEngine;
using UnityEngine.Networking;

public class WorldSetup : SingletonComponent<WorldSetup>
{
	public bool AutomaticallySetup;

	public bool BypassProceduralSpawn;

	public bool ForceGenerateOceanPatrols;

	public GameObject terrain;

	public GameObject decorPrefab;

	public GameObject grassPrefab;

	public GameObject spawnPrefab;

	private TerrainMeta terrainMeta;

	public uint EditorSeed;

	public uint EditorSalt;

	public uint EditorSize;

	public string EditorUrl = string.Empty;

	public string EditorConfigFile = string.Empty;

	[TextArea]
	public string EditorConfigString = string.Empty;

	public List<ProceduralObject> ProceduralObjects = new List<ProceduralObject>();

	internal List<MonumentNode> MonumentNodes = new List<MonumentNode>();

	private static readonly Regex RustCachedMapPattern = new Regex("^https:\\/\\/files.facepunch.com\\/rust\\/maps\\/([0-9a-f]{64})\\/[^\\/]+\\.map$");

	public void OnValidate()
	{
		if (this.terrain == null)
		{
			UnityEngine.Terrain terrain = UnityEngine.Object.FindObjectOfType<UnityEngine.Terrain>();
			if (terrain != null)
			{
				this.terrain = terrain.gameObject;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/world", null, null, useProbabilities: false, useWorldConfig: false);
		foreach (Prefab prefab in array)
		{
			if (prefab.Object.GetComponent<BaseEntity>() != null)
			{
				prefab.SpawnEntity(Vector3.zero, Quaternion.identity).Spawn();
			}
			else
			{
				prefab.Spawn(Vector3.zero, Quaternion.identity);
			}
		}
		SingletonComponent[] array2 = UnityEngine.Object.FindObjectsByType<SingletonComponent>(FindObjectsSortMode.None);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SingletonSetup();
		}
		if ((bool)terrain)
		{
			if ((bool)terrain.GetComponent<TerrainGenerator>())
			{
				World.Procedural = true;
			}
			else
			{
				World.Procedural = false;
				terrainMeta = terrain.GetComponent<TerrainMeta>();
				if (!terrainMeta.terrainRenderer.HasTerrain && terrain.TryGetComponent<UnityEngine.Terrain>(out var component))
				{
					terrainMeta.terrainRenderer.SetTerrain(component);
				}
				terrainMeta.terrainData = terrain.GetComponent<TerrainCollider>().terrainData;
				terrainMeta.Init();
				terrainMeta.SetupComponents();
				terrainMeta.BindShaderProperties();
				terrainMeta.PostSetupComponents();
				World.InitSize(Mathf.RoundToInt(TerrainMeta.Size.x));
				CreateObject(decorPrefab);
				CreateObject(grassPrefab);
				CreateObject(spawnPrefab);
			}
		}
		World.Serialization = new WorldSerialization();
		World.Cached = false;
		World.CleanupOldFiles();
		World.SpawnedPrefabs.Clear();
		if (!string.IsNullOrEmpty(EditorConfigString))
		{
			ConVar.World.configString = EditorConfigString;
		}
		if (!string.IsNullOrEmpty(EditorConfigFile))
		{
			ConVar.World.configFile = EditorConfigFile;
		}
		if (AutomaticallySetup)
		{
			StartCoroutine(InitCoroutine());
		}
	}

	public void CreateObject(GameObject prefab)
	{
		if (!(prefab == null))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			if (gameObject != null)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	public IEnumerator InitCoroutine(CancellationToken ct = default(CancellationToken))
	{
		if (World.CanLoadFromUrl())
		{
			Debug.Log("Loading custom map from " + World.Url);
		}
		else
		{
			Debug.Log("Generating procedural map of size " + World.Size + " with seed " + World.Seed);
		}
		World.Config = new WorldConfig();
		World.Config.LoadScriptableConfigs();
		if (!string.IsNullOrEmpty(ConVar.World.configString))
		{
			Debug.Log("Loading custom world config from world.configstring convar");
			World.Config.LoadFromJsonString(ConVar.World.configString);
		}
		else if (!string.IsNullOrEmpty(ConVar.World.configFile))
		{
			string text = ConVar.Server.rootFolder + "/" + ConVar.World.configFile;
			Debug.Log("Loading custom world config from world.configfile convar: " + text);
			World.Config.LoadFromJsonFile(text);
		}
		World.ResetTiming();
		ProceduralComponent[] components = GetComponentsInChildren<ProceduralComponent>(includeInactive: true);
		int retryCount = 0;
		bool downloadedWorld = false;
		bool shouldRetry;
		do
		{
			shouldRetry = false;
			string mapFileName = World.MapFolderName + "/" + World.MapFileName;
			if (World.Procedural && World.CanLoadFromUrl())
			{
				if (!World.CanLoadFromDisk())
				{
					yield return DownloadWorld(ct);
					downloadedWorld = true;
				}
				Match match = RustCachedMapPattern.Match(World.Url);
				if (match.Success && World.CanLoadFromDisk())
				{
					string value = match.Groups[1].Value;
					string text2 = GetFileHash(mapFileName);
					if (text2 != value)
					{
						if (retryCount != 0)
						{
							goto IL_02ac;
						}
						try
						{
							Debug.LogWarning("Cached map hash mismatch: " + text2 + " != " + value);
							File.Delete(mapFileName);
							retryCount++;
							shouldRetry = true;
							downloadedWorld = false;
						}
						catch (Exception arg)
						{
							Debug.LogError($"Failed to delete cached map: {mapFileName}\n{arg}");
							goto IL_02ac;
						}
						continue;
					}
				}
			}
			Timing loadTimer = Timing.Start("Loading World");
			if (World.Procedural && !World.Cached && World.CanLoadFromDisk())
			{
				UI_LoadingScreen.Update("LOADING WORLD");
				yield return CoroutineEx.waitForEndOfFrame;
				yield return CoroutineEx.waitForEndOfFrame;
				yield return CoroutineEx.waitForEndOfFrame;
				World.Serialization.Load(mapFileName);
				World.Cached = true;
			}
			loadTimer.End();
			if (World.Cached && 10 != World.Serialization.Version)
			{
				Debug.LogWarning("World cache version mismatch: " + 10u + " != " + World.Serialization.Version);
				World.Serialization.Clear();
				World.Cached = false;
				if (World.CanLoadFromUrl())
				{
					if (retryCount != 0 || downloadedWorld || !World.Procedural || !World.CanLoadFromDisk())
					{
						goto IL_0432;
					}
					try
					{
						Debug.LogWarning("Cached map had incorrect version, redownloading");
						File.Delete(mapFileName);
						retryCount++;
						shouldRetry = true;
					}
					catch (Exception arg2)
					{
						Debug.LogError($"Failed to delete cached map: {mapFileName}\n{arg2}");
						goto IL_0432;
					}
					continue;
				}
			}
			if (World.Cached && string.IsNullOrEmpty(World.Checksum))
			{
				World.Checksum = World.Serialization.Checksum;
			}
			World.Timestamp = World.Serialization.Timestamp;
			continue;
			IL_02ac:
			CancelSetup("World File Mismatch: " + World.Name);
			yield break;
			IL_0432:
			CancelSetup("World File Outdated: " + World.Name);
			yield break;
		}
		while (retryCount <= 1 && shouldRetry);
		if (World.Cached)
		{
			World.InitSize(World.Serialization.world.size);
		}
		if (WaterSystem.Collision != null)
		{
			WaterSystem.Collision.Setup();
		}
		if ((bool)terrain)
		{
			TerrainGenerator component2 = terrain.GetComponent<TerrainGenerator>();
			if ((bool)component2)
			{
				if (World.Cached)
				{
					int cachedHeightMapResolution = World.GetCachedHeightMapResolution();
					int cachedSplatMapResolution = World.GetCachedSplatMapResolution();
					terrain = component2.CreateTerrain(cachedHeightMapResolution, cachedSplatMapResolution);
				}
				else
				{
					terrain = component2.CreateTerrain();
				}
				terrainMeta = terrain.GetComponent<TerrainMeta>();
				terrainMeta.Init();
				terrainMeta.SetupComponents();
				CreateObject(decorPrefab);
				CreateObject(grassPrefab);
				CreateObject(spawnPrefab);
			}
		}
		Timing spawnTimer = Timing.Start("Spawning World");
		if (World.Cached)
		{
			UI_LoadingScreen.Update("SPAWNING WORLD");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			if (ct.IsCancellationRequested || TerrainMeta.HeightMap == null)
			{
				yield break;
			}
			TerrainMeta.HeightMap.FromByteArray(World.GetMap("terrain"));
			TerrainMeta.SplatMap.FromByteArray(World.GetMap("splat"));
			TerrainMeta.BiomeMap.FromByteArray(World.GetMap("biome"));
			TerrainMeta.TopologyMap.FromByteArray(World.GetMap("topology"));
			TerrainMeta.AlphaMap.FromByteArray(World.GetMap("alpha"));
			TerrainMeta.WaterMap.FromByteArray(World.GetMap("water"));
			IEnumerator worldSpawn2 = World.Spawn(0.2f, UI_LoadingScreen.Update, ct);
			while (worldSpawn2.MoveNext())
			{
				if (ct.IsCancellationRequested)
				{
					yield break;
				}
				yield return worldSpawn2.Current;
			}
			TerrainMeta.Path.Clear();
			TerrainMeta.Path.Roads.AddRange(World.GetPaths("Road"));
			TerrainMeta.Path.AddRoad(TerrainMeta.Path.Roads, addToMaster: false);
			TerrainMeta.Path.Rivers.AddRange(World.GetPaths("River"));
			TerrainMeta.Path.Powerlines.AddRange(World.GetPaths("Powerline"));
			TerrainMeta.Path.Rails.AddRange(World.GetPaths("Rail"));
		}
		if (TerrainMeta.Path != null)
		{
			foreach (DungeonBaseLink dungeonBaseLink in TerrainMeta.Path.DungeonBaseLinks)
			{
				if (dungeonBaseLink != null)
				{
					dungeonBaseLink.Initialize();
				}
			}
		}
		spawnTimer.End();
		Timing loadPrefabsTimer = Timing.Start("Loading Monument Prefabs");
		if (!World.Cached && World.Procedural)
		{
			FileSystemBackend backend = FileSystem.Backend;
			if (backend is AssetBundleBackend assetBundleBackend)
			{
				List<string> requiredAssetScenes = AssetSceneManifest.Current.MonumentScenes;
				IEnumerator worldSpawn2 = assetBundleBackend.LoadAssetScenes(requiredAssetScenes);
				bool wantsCancel = false;
				float lastProgress = 0f;
				while (worldSpawn2.MoveNext())
				{
					if (!wantsCancel && ct.IsCancellationRequested)
					{
						wantsCancel = true;
						Debug.LogWarning("Cancel was requested but must wait for asset scenes to finish loading");
					}
					float assetSceneProgress = assetBundleBackend.GetAssetSceneProgress(requiredAssetScenes);
					if (!Mathf.Approximately(assetSceneProgress, lastProgress))
					{
						lastProgress = assetSceneProgress;
						UI_LoadingScreen.Update($"Loading Monument Prefabs {assetSceneProgress * 100f:0.0}%");
					}
					yield return worldSpawn2.Current;
				}
			}
		}
		loadPrefabsTimer.End();
		Timing procgenTimer = Timing.Start("Processing World");
		if (components.Length != 0)
		{
			for (int i = 0; i < components.Length; i++)
			{
				ProceduralComponent component = components[i];
				if ((bool)component && component.ShouldRun())
				{
					if (ct.IsCancellationRequested)
					{
						yield break;
					}
					uint seed = (uint)(World.Seed + i);
					UI_LoadingScreen.Update(component.Description.ToUpper());
					yield return CoroutineEx.waitForEndOfFrame;
					yield return CoroutineEx.waitForEndOfFrame;
					yield return CoroutineEx.waitForEndOfFrame;
					Timing timing = Timing.Start(component.Description);
					if ((bool)component)
					{
						component.Process(seed);
					}
					timing.End();
				}
			}
		}
		procgenTimer.End();
		Timing saveTimer = Timing.Start("Saving World");
		if (ConVar.World.cache && World.Procedural && !World.Cached)
		{
			UI_LoadingScreen.Update("SAVING WORLD");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			World.Serialization.world.size = World.Size;
			World.AddPaths(TerrainMeta.Path.Roads);
			World.AddPaths(TerrainMeta.Path.Rivers);
			World.AddPaths(TerrainMeta.Path.Powerlines);
			World.AddPaths(TerrainMeta.Path.Rails);
			World.Serialization.Save(World.MapFolderName + "/" + World.MapFileName);
		}
		saveTimer.End();
		Timing checksumTimer = Timing.Start("Calculating Checksum");
		if (string.IsNullOrEmpty(World.Serialization.Checksum))
		{
			UI_LoadingScreen.Update("CALCULATING CHECKSUM");
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			yield return CoroutineEx.waitForEndOfFrame;
			World.Serialization.CalculateChecksum();
		}
		checksumTimer.End();
		if (string.IsNullOrEmpty(World.Checksum))
		{
			World.Checksum = World.Serialization.Checksum;
		}
		Timing oceanTimer = Timing.Start("Ocean Patrol Paths");
		UI_LoadingScreen.Update("OCEAN PATROL PATHS");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		if ((BaseBoat.generate_paths && TerrainMeta.Path != null) || ForceGenerateOceanPatrols)
		{
			TerrainMeta.Path.OceanPatrolFar = BaseBoat.GenerateOceanPatrolPath(200f);
		}
		else
		{
			Debug.Log("Skipping ocean patrol paths, baseboat.generate_paths == false");
		}
		oceanTimer.End();
		Timing finalizeTimer = Timing.Start("Finalizing World");
		UI_LoadingScreen.Update("FINALIZING WORLD");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		if ((bool)terrainMeta)
		{
			if (World.Procedural)
			{
				terrainMeta.BindShaderProperties();
				terrainMeta.PostSetupComponents();
			}
			TerrainMargin.Create();
		}
		finalizeTimer.End();
		Timing cleaningTimer = Timing.Start("Cleaning Up");
		UI_LoadingScreen.Update("CLEANING UP");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		List<string> invalidAssets = FileSystem.Backend.UnloadBundles("monuments");
		if (FileSystem.Backend is AssetBundleBackend assetBundleBackend2)
		{
			List<string> unloadableScenes = AssetSceneManifest.Current.UnloadableScenes;
			yield return assetBundleBackend2.UnloadAssetScenes(unloadableScenes, delegate(string sceneName, Dictionary<string, GameObject> prefabs)
			{
				foreach (var (item, _) in prefabs)
				{
					invalidAssets.Add(item);
				}
			});
		}
		foreach (string item2 in invalidAssets)
		{
			GameManager.server.preProcessed.Invalidate(item2);
			GameManifest.Invalidate(item2);
			PrefabAttribute.server.Invalidate(StringPool.Get(item2));
		}
		ConVar.GC.unload();
		cleaningTimer.End();
		UI_LoadingScreen.Update("DONE");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		if ((bool)this)
		{
			GameManager.Destroy(base.gameObject);
		}
		static string GetFileHash(string fileName)
		{
			using SHA256 sHA = SHA256.Create();
			using FileStream inputStream = File.OpenRead(fileName);
			return BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", "").ToLowerInvariant();
		}
	}

	private IEnumerator DownloadWorld(CancellationToken ct)
	{
		if (!World.Procedural || !World.CanLoadFromUrl())
		{
			Debug.LogError("Cannot download world - not procedural or no url set");
			yield break;
		}
		Timing downloadTimer = Timing.Start("Downloading World");
		UI_LoadingScreen.Update("DOWNLOADING WORLD");
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		yield return CoroutineEx.waitForEndOfFrame;
		string mapFilename = World.MapFolderName + "/" + World.MapFileName;
		string fullMapPath = Path.Combine(Rust.Application.installPath, mapFilename);
		string path = World.MapFolderName + "/" + Guid.NewGuid().ToString("N") + ".part";
		string fullTempPath = Path.Combine(Rust.Application.installPath, path);
		UnityWebRequest request = UnityWebRequest.Get(World.Url);
		request.downloadHandler = new DownloadHandlerFile(fullTempPath);
		request.Send();
		float lastProgress = 0f;
		while (!request.isDone)
		{
			if (ct.IsCancellationRequested)
			{
				request.Abort();
				request.Dispose();
				TryRemoveMapFile(fullTempPath);
				yield break;
			}
			float downloadProgress = request.downloadProgress;
			if (!Mathf.Approximately(downloadProgress, lastProgress))
			{
				lastProgress = downloadProgress;
				UI_LoadingScreen.Update($"DOWNLOADING WORLD {downloadProgress * 100f:0.0}%");
			}
			yield return CoroutineEx.waitForEndOfFrame;
		}
		if (!request.isHttpError && !request.isNetworkError)
		{
			TryRemoveMapFile(fullMapPath);
			File.Move(fullTempPath, fullMapPath);
			World.Serialization.Load(mapFilename);
			World.Cached = true;
		}
		else
		{
			TryRemoveMapFile(fullTempPath);
			CancelSetup("Couldn't Download Level: " + World.Name + " (" + request.error + ")");
		}
		downloadTimer.End();
	}

	private void TryRemoveMapFile(string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Failed to delete temp file: {filePath}\n{arg}");
		}
	}

	private void CancelSetup(string msg)
	{
		Debug.LogError(msg);
		Rust.Application.Quit();
	}
}

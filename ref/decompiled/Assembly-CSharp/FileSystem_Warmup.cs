using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;

public class FileSystem_Warmup : MonoBehaviour
{
	private static List<(string, TimeSpan)> warmupTimes = new List<(string, TimeSpan)>();

	private static bool run = true;

	private static string[] ExcludeFilter = new string[13]
	{
		"/bundled/prefabs/autospawn/monument", "/bundled/prefabs/autospawn/mountain", "/bundled/prefabs/autospawn/canyon", "/bundled/prefabs/autospawn/decor", "/bundled/prefabs/autospawn/tunnel", "/bundled/prefabs/remapped", "/bundled/prefabs/navmesh", "/content/ui/", "/prefabs/ui/", "/prefabs/world/",
		"/prefabs/system/", "/standard assets/", "/third party/"
	};

	private static Stopwatch warmupTimer = new Stopwatch();

	public static IEnumerable<(string, TimeSpan)> GetWarmupTimes()
	{
		return warmupTimes;
	}

	public static void Run()
	{
		if (!run)
		{
			return;
		}
		if (FileSystem.Backend is AssetBundleBackend assetBundleBackend && assetBundleBackend.GetAssetSceneProgress() < 1f)
		{
			UnityEngine.Debug.LogError("Cannot run synchronous asset warmup until all asset scenes are finished loading");
			return;
		}
		List<string> assetList = GetAssetList();
		for (int i = 0; i < assetList.Count; i++)
		{
			PrefabWarmup(assetList[i]);
		}
		run = false;
	}

	public static IEnumerator Run(Action<string> statusFunction, string format = null, CancellationToken ct = default(CancellationToken))
	{
		if (!run)
		{
			yield break;
		}
		FileSystemBackend backend = FileSystem.Backend;
		if (backend is AssetBundleBackend assetBundleBackend)
		{
			float lastProgress = 0f;
			while (!ct.IsCancellationRequested)
			{
				float assetSceneProgress = assetBundleBackend.GetAssetSceneProgress();
				if (assetSceneProgress >= 1f)
				{
					break;
				}
				if (!Mathf.Approximately(assetSceneProgress, lastProgress))
				{
					lastProgress = assetSceneProgress;
					statusFunction?.Invoke($"Loading Game Prefabs {assetSceneProgress * 100f:0.0}%");
				}
				yield return null;
			}
		}
		Timing timer = new Timing("asset_warmup");
		List<string> assetList = GetAssetList();
		yield return RunForAssets(assetList, statusFunction, format, ct);
		timer.End();
		run = false;
	}

	public static IEnumerator RunForAssets(List<string> assetList, Action<string> statusFunction, string format = null, CancellationToken ct = default(CancellationToken))
	{
		if (assetList.Count == 0)
		{
			yield break;
		}
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < assetList.Count; i++)
		{
			if (ct.IsCancellationRequested)
			{
				break;
			}
			if (sw.Elapsed.TotalSeconds > (double)CalculateFrameBudget() || i == 0 || i == assetList.Count - 1)
			{
				statusFunction?.Invoke(string.Format((format != null) ? format : "{0}/{1}", i + 1, assetList.Count));
				yield return CoroutineEx.waitForEndOfFrame;
				sw.Restart();
			}
			PrefabWarmup(assetList[i]);
		}
	}

	private static float CalculateFrameBudget()
	{
		return 2f;
	}

	private static bool ShouldIgnore(string path)
	{
		for (int i = 0; i < ExcludeFilter.Length; i++)
		{
			if (path.Contains(ExcludeFilter[i], CompareOptions.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static List<string> GetAssetList()
	{
		if (!(FileSystem.Backend is AssetBundleBackend assetBundleBackend))
		{
			return new List<string>();
		}
		return (from t in assetBundleBackend.GetAssetScenePrefabs(AssetSceneManifest.Current.AutoLoadScenes)
			where !ShouldIgnore(t.Path)
			select t.Path).ToList();
	}

	private static void PrefabWarmup(string path)
	{
		warmupTimer.Restart();
		GameManager.server.FindPrefab(path);
		warmupTimer.Stop();
		warmupTimes.Add(ValueTuple.Create(path, warmupTimer.Elapsed));
	}
}

using System;
using Rust.Components;
using UnityEngine;

namespace Rust;

public static class Global
{
	public static Func<string, GameObject> LoadPrefab;

	public static Func<string, GameObject> FindPrefab;

	public static Func<string, GameObject> CreatePrefab;

	public static Action OpenMainMenu;

	public static Func<bool> IsLoadingScreenActive;

	public static Func<bool> IsAsyncBundleQueueBlocked;

	private static FacepunchBehaviour _runner;

	public static FacepunchBehaviour Runner
	{
		get
		{
			if (_runner == null)
			{
				GameObject gameObject = new GameObject("Coroutine Runner");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				_runner = gameObject.AddComponent<NullMonoBehaviour>();
			}
			return _runner;
		}
	}

	public static int LaunchCountThisVersion { get; private set; }

	public static void Init()
	{
		LaunchCountThisVersion = PlayerPrefs.GetInt($"launch{2633}", 0) + 1;
		PlayerPrefs.SetInt($"launch{2633}", LaunchCountThisVersion);
	}
}

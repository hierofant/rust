using UnityEngine;

namespace Rust;

public static class Application
{
	public static bool isPlaying;

	public static bool isQuitting;

	public static bool isLoading;

	public static bool isReceiving;

	public static bool isLoadingSave;

	public static bool isLoadingPrefabs;

	public static bool isReturningToMenu;

	public static bool isServerStarted;

	public static bool isUnloadingWorld
	{
		get
		{
			if (!isQuitting)
			{
				return isReturningToMenu;
			}
			return true;
		}
	}

	public static string installPath
	{
		get
		{
			if (UnityEngine.Application.platform == RuntimePlatform.OSXPlayer)
			{
				return UnityEngine.Application.dataPath + "/../..";
			}
			return UnityEngine.Application.dataPath + "/..";
		}
	}

	public static string dataPath => UnityEngine.Application.dataPath;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Startup()
	{
		isPlaying = true;
	}

	public static void Quit()
	{
		isQuitting = true;
		UnityEngine.Application.Quit();
	}
}

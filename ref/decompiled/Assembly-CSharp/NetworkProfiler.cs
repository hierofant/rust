using ConVar;
using Network;
using UnityEngine;

public static class NetworkProfiler
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void InstallResolvers()
	{
		NetProfileCapture.NameResolver = StringPool.Get;
		NetProfileCapture.PrefabResolver = ResolvePrefabId;
	}

	private static uint ResolvePrefabId(ulong entityId, bool serverRealm)
	{
		NetworkableId uid = new NetworkableId(entityId);
		if (serverRealm)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(uid);
			if (!(baseNetworkable != null))
			{
				return 0u;
			}
			return baseNetworkable.prefabID;
		}
		return 0u;
	}

	private static void ExportProfile()
	{
		NetProfileSnapshot netProfileSnapshot = NetProfileCapture.CreateSnapshot();
		NetProfileCapture.Stop();
		if (netProfileSnapshot != null)
		{
			string text = NetProfileSnapshot.DefaultPath();
			netProfileSnapshot.Save(text);
			Debug.Log("[NetworkProfiler] Exported profile to: " + text);
		}
	}

	[ServerVar(Help = "networkprofiler.serverprofile [time to profile(in seconds), min(0.1), max(1000), float]", ServerAdmin = true)]
	public static void ServerProfile(ConsoleSystem.Arg arg)
	{
		float @float = arg.GetFloat(0);
		@float = Mathf.Clamp(@float, 0.1f, 1000f);
		NetProfileCapture.Start(@float);
		Chat.Broadcast($"Server is taking a network snapshot for {@float} seconds...", "SERVER", "#eee", 0uL);
		InvokeHandler.Invoke(SingletonComponent<InvokeHandler>.Instance, delegate
		{
			Chat.Broadcast("Done!", "SERVER", "#eee", 0uL);
			ExportProfile();
		}, @float);
	}
}

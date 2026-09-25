using UnityEngine;

namespace ConVar;

[Factory("bradley")]
public class Bradley : ConsoleSystem
{
	[ServerVar(Help = "(Generated) Number of minutes after the Bradley APC is destroyed before it respawns at its patrol path; default is 60 minutes")]
	public static float respawnDelayMinutes = 60f;

	[ServerVar(Help = "(Generated) Random variance multiplier applied to respawnDelayMinutes; a value of 1.0 means the actual delay is randomly chosen between 0 and respawnDelayMinutes")]
	public static float respawnDelayVariance = 1f;

	[ServerVar(Help = "(Generated) When false, prevents the Bradley APC from spawning or respawning on the server")]
	public static bool enabled = true;

	[ServerVar(Help = "(Generated) Forces an immediate Bradley APC respawn, bypassing the normal respawn delay; admin only")]
	public static void quickrespawn(Arg arg)
	{
		if (!ArgEx.Player(arg))
		{
			return;
		}
		BradleySpawner singleton = BradleySpawner.singleton;
		if (singleton == null)
		{
			Debug.LogWarning("No Spawner");
			return;
		}
		if ((bool)singleton.spawned)
		{
			singleton.spawned.Kill();
		}
		singleton.spawned = null;
		singleton.DoRespawn();
	}
}

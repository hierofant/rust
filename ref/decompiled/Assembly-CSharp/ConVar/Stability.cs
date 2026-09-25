using System.Linq;
using UnityEngine;

namespace ConVar;

[Factory("stability")]
public class Stability : ConsoleSystem
{
	[ServerVar(Help = "(Generated) Verbosity level for stability system logging; 0 = silent, higher values print more detail about stability calculations and propagation")]
	public static int verbose = 0;

	[ServerVar(Help = "(Generated) Number of stability propagation iterations performed per tick; higher values resolve complex multi-block stability chains faster at the cost of CPU time")]
	public static int strikes = 10;

	[ServerVar(Help = "(Generated) Stability value below which a building block is considered unsupported and will collapse; default 0.05 means blocks below 5% support fall")]
	public static float collapse = 0.05f;

	[ServerVar(Help = "(Generated) Floating-point tolerance used when comparing stability values during propagation; smaller values are more precise but can cause more recalculations")]
	public static float accuracy = 0.001f;

	[ServerVar(Help = "(Generated) Maximum time in seconds that stability update jobs can run per server tick before being deferred to the next tick")]
	public static float stabilityqueue = 9f;

	[ServerVar(Help = "(Generated) Maximum time in seconds that surrounding-support check jobs can run per server tick before deferral")]
	public static float surroundingsqueue = 3f;

	public static bool support_highest_stability = false;

	[ServerVar(Help = "(Generated) When enabled, logs each entity death caused by a stability collapse to the server console with position and prefab name")]
	public static bool log_stability_death = false;

	[ServerVar(Help = "(Generated) When enabled, logs when entities die because their ground support entity was destroyed or is missing")]
	public static bool log_ground_missing_death = false;

	[ServerVar(Help = "(Generated) When enabled, logs every stability value change during propagation; very verbose, use only for targeted debugging")]
	public static bool log_stability_updates = false;

	[ServerVar(Help = "(Generated) Forces an immediate recalculation of stability for all building blocks in the world; expensive on large bases")]
	public static void refresh_stability(Arg args)
	{
		StabilityEntity[] array = BaseNetworkable.serverEntities.OfType<StabilityEntity>().ToArray();
		Debug.Log("Refreshing stability on " + array.Length + " entities...");
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateStability();
		}
	}
}

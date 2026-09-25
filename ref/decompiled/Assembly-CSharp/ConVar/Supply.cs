using UnityEngine;

namespace ConVar;

[Factory("supply")]
public class Supply : ConsoleSystem
{
	private const string path = "assets/prefabs/npc/cargo plane/cargo_plane.prefab";

	[ServerVar(Help = "(Generated) Spawns a supply drop at the calling admin or player position; the drop falls from the sky with a parachute like a naturally occurring airdrop")]
	public static void drop(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			Debug.Log("Supply Drop Inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.GetComponent<CargoPlane>().InitDropPosition(basePlayer.transform.position + new Vector3(0f, 10f, 0f));
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Calls in a supply drop to a specific grid coordinate or position; useful for testing supply crate loot tables and airdrop pathing")]
	public static void call(Arg arg)
	{
		if ((bool)ArgEx.Player(arg))
		{
			Debug.Log("Supply Drop Inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/cargo plane/cargo_plane.prefab");
			if ((bool)baseEntity)
			{
				baseEntity.Spawn();
			}
		}
	}
}

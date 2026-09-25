using System.Collections.Generic;
using UnityEngine;

public class ConvarTimedPrefabSpawner : BaseMonoBehaviour, IServerComponent
{
	[Tooltip("A random prefab from this list will be spawned each interval")]
	public List<GameObjectRef> prefabsToSpawn = new List<GameObjectRef>();

	[ServerVar(Help = "Time in seconds between prefab spawns. Set to 0 to disable spawning.")]
	[HideInInspector]
	public static float prefab_spawn_interval = 0f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Variance in seconds to add/subtract from the spawn interval")]
	public static float prefab_spawn_interval_variance = 3f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "If true, spawned prefabs will have a random rotation")]
	public static bool prefab_spawn_random_rotation = true;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Maximum random offset (sphere radius) from the spawner's position when spawning prefabs")]
	public static float prefab_spawn_random_position_offset = 0f;

	[HideInInspector]
	[ServerVar(Saved = true, Help = "Time in seconds before spawned prefabs are despawned. Set to 0 to disable despawning.")]
	public static float prefab_despawn_time = 20f;

	private TimeSince timeSinceLastSpawn;

	private float randomIntervalOffset;

	public void FixedUpdate()
	{
		if (prefab_spawn_interval == 0f || prefabsToSpawn.Count == 0 || !((float)timeSinceLastSpawn >= prefab_spawn_interval + randomIntervalOffset))
		{
			return;
		}
		timeSinceLastSpawn = 0f;
		randomIntervalOffset = Random.Range(0f - prefab_spawn_interval_variance, prefab_spawn_interval_variance);
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)].resourcePath, base.transform.position + Random.insideUnitSphere * prefab_spawn_random_position_offset, prefab_spawn_random_rotation ? Random.rotation : base.transform.rotation);
		if (!(baseEntity != null))
		{
			return;
		}
		baseEntity.Spawn();
		if (prefab_despawn_time > 0f)
		{
			EntityTimedDestroy entityTimedDestroy = baseEntity.gameObject.AddComponent<EntityTimedDestroy>();
			if (entityTimedDestroy != null)
			{
				entityTimedDestroy.SetTime(prefab_despawn_time);
			}
		}
	}
}

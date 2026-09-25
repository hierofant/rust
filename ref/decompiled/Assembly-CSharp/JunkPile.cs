using System.Collections;
using System.Collections.Generic;
using Facepunch;
using Network;
using UnityEngine;

public class JunkPile : BaseEntity
{
	public GameObjectRef sinkEffect;

	public SpawnGroup[] spawngroups;

	public NPCSpawner NPCSpawn;

	private const float lifetimeMinutes = 30f;

	private const float lifetimeJitterSeconds = 30f;

	[ServerVar]
	public static bool DestroyIfSpawnOnSleepingBag = true;

	[ServerVar]
	public static float DestroyIfSpawnOnSleepingBagTime = 4f;

	[ServerVar]
	public static float DestroyIfSpawnOnSleepingBagDistance = 3f;

	protected bool isSinking;

	private float timeWantingDespawn;

	private float timeBeforeDespawn = 90f;

	private const float CheckEmptyDelay = 30f;

	private const float DelayRandomness = 5f;

	public virtual bool DespawnIfAnyLootTaken => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("JunkPile.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		StartTimeout();
		StartCoroutine(SpawnInitialCoroutine());
		isSinking = false;
	}

	public override void Spawn()
	{
		base.Spawn();
		if (DestroyIfSpawnOnSleepingBag)
		{
			Invoke(KillIfOnSleepingBag, DestroyIfSpawnOnSleepingBagTime, DestroyIfSpawnOnSleepingBagTime * 0.5f);
		}
	}

	private void KillIfOnSleepingBag()
	{
		List<SleepingBag> obj = Pool.Get<List<SleepingBag>>();
		Vis.Entities(base.transform.position, DestroyIfSpawnOnSleepingBagDistance, obj, 153092352);
		if (obj.Count > 0)
		{
			SpawnGroup[] array = spawngroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
			Kill();
		}
		Pool.FreeUnmanaged(ref obj);
	}

	protected virtual void StartTimeout()
	{
		Invoke(TimeOut, 1800f + Random.Range(-30f, 30f));
		InvokeRandomized(CheckEmpty, 10f, 30f, 5f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		StabilityEntity.updateSurroundingsQueue.Add(WorldSpaceBounds().ToBounds());
	}

	private IEnumerator SpawnInitialCoroutine()
	{
		yield return CoroutineEx.waitForSeconds(1f);
		SpawnGroup[] array = spawngroups;
		foreach (SpawnGroup s in array)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			s.SpawnInitial();
		}
	}

	public bool SpawnGroupsEmpty()
	{
		SpawnGroup[] array = spawngroups;
		foreach (SpawnGroup spawnGroup in array)
		{
			if (spawnGroup.resetBehavior == SpawnGroupResetBehavior.Exclude || (spawnGroup.DoesGroupContainNPCs() && spawnGroup.resetBehavior != SpawnGroupResetBehavior.Include))
			{
				continue;
			}
			if (DespawnIfAnyLootTaken)
			{
				if (spawnGroup.ObjectsRemoved > 0)
				{
					return true;
				}
				foreach (SpawnPointInstance spawnInstance in spawnGroup.SpawnInstances)
				{
					if (spawnInstance.Entity is LootContainer { HasBeenLooted: not false })
					{
						return true;
					}
				}
			}
			else if (spawnGroup.currentPopulation > 0)
			{
				return false;
			}
		}
		if (NPCSpawn != null && NPCSpawn.currentPopulation > 0)
		{
			return false;
		}
		if (DespawnIfAnyLootTaken)
		{
			return false;
		}
		return true;
	}

	public virtual void CheckEmpty()
	{
		if (SpawnGroupsEmpty() && !BaseNetworkable.HasCloseConnections(base.transform.position, TimeoutPlayerCheckRadius()))
		{
			timeWantingDespawn += 30f;
			if (timeWantingDespawn >= timeBeforeDespawn)
			{
				CancelInvoke(CheckEmpty);
				SinkAndDestroy();
			}
		}
		else
		{
			timeWantingDespawn = 0f;
		}
	}

	public virtual float TimeoutPlayerCheckRadius()
	{
		return 15f;
	}

	public void TimeOut()
	{
		if (BaseNetworkable.HasCloseConnections(base.transform.position, TimeoutPlayerCheckRadius()))
		{
			Invoke(TimeOut, 30f);
			return;
		}
		SpawnGroupsEmpty();
		SinkAndDestroy();
	}

	public void SinkAndDestroy()
	{
		if (base.isServer)
		{
			CancelInvoke(SinkAndDestroy);
			SpawnGroup[] array = spawngroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, b: true, recursive: true);
			}
			if (NPCSpawn != null)
			{
				NPCSpawn.Clear();
			}
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_StartSink"));
			base.transform.position -= new Vector3(0f, 5f, 0f);
			isSinking = true;
			Invoke(KillMe, 22f);
		}
	}

	public void KillMe()
	{
		Kill();
	}

	public static void NotifyLootContainerLooted(BaseEntity entity)
	{
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		return true;
	}
}

using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class BeeSwarmMaster : BaseCombatEntity, ISplashable
{
	[Header("References")]
	public ParticleSystem PSystem;

	public Light OnFireLight;

	public ParticleSystemForceField AngerForceField;

	public GameObject sound;

	public GameObjectRef beeSwarmPrefab;

	public const Flags IsDying = Flags.Reserved13;

	[ServerVar(Help = "How long a master swarm will stick around without a target")]
	public static float killWithoutATargetTime = 150f;

	[ServerVar(Help = "How many child swarms a master swarm will create")]
	public static int amountToSpawn = 3;

	[ServerVar(Help = "How long before a master swarm will create a child")]
	public static float secondsBetweenSpawns = 60f;

	private TimeSince timeSinceLastSpawnedSwarm;

	private int hasSpawnedCount;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BeeSwarmMaster.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && base.isServer)
		{
			StartDie();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!base.isClient)
		{
			timeSinceLastSpawnedSwarm = secondsBetweenSpawns;
			InvokeRepeating(ThinkAI, 0f, 0.25f);
		}
	}

	private void ThinkAI()
	{
		using (TimeWarning.New("BeeSwarmMaster.ThinkAI"))
		{
			if (!AI.effectaiweapons || (!AI.ignoreplayers && AI.think))
			{
				if (IsSmoke())
				{
					StartDie();
				}
				if (IsFire())
				{
					SetOnFire();
				}
				if ((float)timeSinceLastSpawnedSwarm > killWithoutATargetTime)
				{
					StartDie();
				}
				if ((float)timeSinceLastSpawnedSwarm >= secondsBetweenSpawns && BeeSwarmAI.FindTarget(base.transform) != null)
				{
					SpawnSwarm();
				}
			}
		}
	}

	private void StartDie()
	{
		if (!HasFlag(Flags.Reserved13))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
			Invoke(ActuallyDie, 3f, 0f);
		}
	}

	private void ActuallyDie()
	{
		Die();
	}

	private void SpawnSwarm()
	{
		timeSinceLastSpawnedSwarm = 0f;
		hasSpawnedCount++;
		if (hasSpawnedCount >= amountToSpawn)
		{
			StartDie();
		}
		else
		{
			float arg = (float)hasSpawnedCount / (float)amountToSpawn;
			if (net.group != null)
			{
				ClientRPC(RpcTarget.NetworkGroup("RPC_PopulationChange"), arg);
			}
		}
		Vector3 position = base.transform.position;
		BaseEntity baseEntity = GameManager.server.CreateEntity(beeSwarmPrefab.resourcePath, position, Quaternion.identity);
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.creatorEntity = creatorEntity;
		baseEntity.Spawn();
	}

	private void SetOnFire()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.OnFire, b: true);
		}
		StartDie();
	}

	private bool IsSmoke()
	{
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		SingletonComponent<SmokeGrenadeManager>.Instance.GetSmokeAround(base.transform.position, 5f, pooledList);
		return pooledList != null && pooledList.Count > 0;
	}

	private bool IsFire()
	{
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		SingletonComponent<NpcFireManager>.Instance.GetFiresAround(base.transform.position, 10f, pooledList);
		foreach (BaseEntity item in pooledList)
		{
			if (!(item == null) && !item.IsDestroyed && item is FlameThrower && Vector3.Distance(base.transform.position, item.transform.position) <= BeeSwarmAI.flameSettingDistance)
			{
				return true;
			}
		}
		return false;
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (splashType == null || splashType.shortname == null)
		{
			return false;
		}
		if (HasFlag(Flags.Reserved13))
		{
			return false;
		}
		if (amount > 0)
		{
			return true;
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		float num = base.health - 10f;
		if (num > 0f)
		{
			Hurt(num);
		}
		if (base.health <= 10f)
		{
			StartDie();
		}
		return amount;
	}
}

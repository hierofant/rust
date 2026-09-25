using Facepunch;
using Rust;
using UnityEngine;

public class DeployableSiegeExplosive : BaseCombatEntity, IIgniteable, ISplashable
{
	public GameObjectRef ExplosionEffect;

	public GameObjectRef ExplosionImpact;

	public Vector3 EffectOffset;

	public Transform ExplosionSpawnPoint;

	public const Flags Lit = Flags.Reserved1;

	public float MinimumFuseTime = 3f;

	public float MaximumFuseTime = 10f;

	public float NeighbourExplodeRadius = 2f;

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		creatorEntity = deployedBy;
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void Hurt(HitInfo info)
	{
		if (!base.isClient && !HasFlag(Flags.Reserved1))
		{
			info.damageTypes.ScaleAll(0f);
			base.Hurt(info);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ActuallyExplode, Random.Range(MinimumFuseTime, MaximumFuseTime));
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasFlag(Flags.Reserved1))
		{
			Invoke(ActuallyExplode, Random.Range(MinimumFuseTime, MaximumFuseTime));
		}
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	private void ActuallyExplode()
	{
		if (ExplosionEffect.isValid)
		{
			ExplosionSpawnPoint.GetPositionAndRotation(out var position, out var rotation);
			BaseEntity baseEntity = GameManager.server.CreateEntity(ExplosionEffect.resourcePath, position, rotation);
			ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
			baseEntity.Spawn();
			if (component.TryGetComponent<TimedExplosive>(out var component2))
			{
				component2.creatorEntity = creatorEntity;
				component2.Explode();
			}
			if (ExplosionImpact.isValid)
			{
				Effect.server.Run(ExplosionImpact.resourcePath, position + EffectOffset);
			}
			using PooledList<DeployableSiegeExplosive> pooledList = Pool.Get<PooledList<DeployableSiegeExplosive>>();
			Vis.Entities(CenterPoint(), NeighbourExplodeRadius, pooledList, 256);
			foreach (DeployableSiegeExplosive item in pooledList)
			{
				if (item.isServer && !item.HasFlag(Flags.Reserved1) && CanSee(position, item.ExplosionSpawnPoint.position))
				{
					item.Hurt(3f, DamageType.Heat, this);
				}
			}
		}
		Kill();
	}

	public void Ignite(Vector3 fromPos)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		Invoke(ActuallyExplode, Random.Range(MinimumFuseTime, MaximumFuseTime));
	}

	public bool CanIgnite()
	{
		return !HasFlag(Flags.Reserved1);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		return HasFlag(Flags.Reserved1);
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
		}
		CancelInvoke(ActuallyExplode);
		return 0;
	}

	private void OnGroundMissing()
	{
		ActuallyExplode();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!HasFlag(Flags.Reserved1))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}
}

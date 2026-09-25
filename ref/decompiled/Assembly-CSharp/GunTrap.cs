using System.Collections.Generic;
using Facepunch;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;

public class GunTrap : StorageContainer
{
	public class GunTrapScanWorkQueue : PersistentObjectWorkQueue<GunTrap>
	{
		protected override void RunJob(GunTrap entity)
		{
			if (ShouldAdd(entity))
			{
				entity.ServerThink();
			}
		}

		protected override bool ShouldAdd(GunTrap entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	[ServerVar(Help = "How many milliseconds to spend on target scanning per frame")]
	public static float gun_trap_budget_ms = 0.5f;

	public static GunTrapScanWorkQueue updateGunTrapWorkQueue = new GunTrapScanWorkQueue();

	public GameObjectRef gun_fire_effect;

	public GameObjectRef bulletEffect;

	public GameObjectRef triggeredEffect;

	public Transform muzzlePos;

	public Transform eyeTransform;

	public int numPellets = 15;

	public int aimCone = 30;

	public float sensorRadius = 1.25f;

	public ItemDefinition ammoType;

	public TargetTrigger trigger;

	public const Flags Flag_Triggered = Flags.Reserved4;

	private float triggeredTime;

	private readonly float triggerCooldownDuration = 0.5f;

	private float triggerCooldown;

	private float _cacheTimeout;

	private IPrivilege _cachedPriv;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("GunTrap.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override string Categorize()
	{
		return "GunTrap";
	}

	public bool IsTriggered()
	{
		return HasFlag(Flags.Reserved4);
	}

	public Vector3 GetEyePosition()
	{
		return eyeTransform.position;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		updateGunTrapWorkQueue.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		updateGunTrapWorkQueue.Remove(this);
	}

	public void ServerThink()
	{
		if (IsTriggered() && Time.realtimeSinceStartup - triggeredTime > triggerCooldownDuration)
		{
			SetTriggered(triggered: false);
		}
		if (!(triggerCooldown > Time.realtimeSinceStartup) && CanFire() && CheckTrigger())
		{
			SetTriggered(triggered: true);
			FireWeapon();
			triggerCooldown = Time.realtimeSinceStartup + triggerCooldownDuration;
		}
	}

	public bool CheckTrigger()
	{
		List<RaycastHit> obj = null;
		try
		{
			HashSet<BaseEntity> entityContents = trigger.entityContents;
			if (entityContents == null || entityContents.Count == 0)
			{
				return false;
			}
			if (!CanFire())
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			IPrivilege privilege = null;
			foreach (BaseEntity item in entityContents)
			{
				BasePlayer basePlayer = item as BasePlayer;
				if (basePlayer == null || basePlayer.IsSleeping() || !basePlayer.IsAlive())
				{
					continue;
				}
				object obj2 = Interface.CallHook("CanBeTargeted", basePlayer, this);
				if (obj2 is bool)
				{
					flag = (bool)obj2;
					break;
				}
				if (!flag2)
				{
					flag2 = true;
					privilege = GetCachedPrivilege();
				}
				if (privilege != null && privilege.IsAuthed(basePlayer))
				{
					continue;
				}
				if (obj == null)
				{
					obj = Pool.Get<List<RaycastHit>>();
				}
				else
				{
					obj.Clear();
				}
				GamePhysics.TraceAll(new Ray(basePlayer.eyes.position, (GetEyePosition() - basePlayer.eyes.position).normalized), 0f, obj, 9f, 1218519297);
				for (int i = 0; i < obj.Count; i++)
				{
					BaseEntity entity = RaycastHitEx.GetEntity(obj[i]);
					if (entity != null && (entity == this || entity.EqualNetID(this)))
					{
						flag = true;
						break;
					}
					if (!(entity != null) || entity.ShouldBlockProjectiles())
					{
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			return flag;
		}
		finally
		{
			if (obj != null)
			{
				Pool.FreeUnmanaged(ref obj);
			}
		}
	}

	public void FireWeapon()
	{
		if (UseAmmo())
		{
			Effect.server.Run(gun_fire_effect.resourcePath, this, StringPool.Get(muzzlePos.gameObject.name), Vector3.zero, Vector3.zero);
			for (int i = 0; i < numPellets; i++)
			{
				FireBullet();
			}
		}
	}

	public void FireBullet()
	{
		float damageAmount = 10f;
		Vector3 vector = muzzlePos.transform.position - muzzlePos.forward * 0.25f;
		Vector3 forward = muzzlePos.transform.forward;
		Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(aimCone, forward);
		Vector3 arg = vector + modifiedAimConeDirection * 300f;
		ClientRPC(RpcTarget.NetworkGroup("CLIENT_FireGun"), arg);
		using PooledList<RaycastHit> pooledList = Pool.Get<PooledList<RaycastHit>>();
		int layerMask = 1220225793;
		GamePhysics.TraceAll(new Ray(vector, modifiedAimConeDirection), 0.1f, pooledList, 300f, layerMask);
		for (int i = 0; i < pooledList.Count; i++)
		{
			RaycastHit hit = pooledList[i];
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (entity != null && (entity == this || entity.EqualNetID(this)))
			{
				continue;
			}
			if (entity as BaseCombatEntity != null)
			{
				HitInfo info = new HitInfo(this, entity, DamageType.Bullet, damageAmount, hit.point);
				entity.OnAttacked(info);
				if (entity is BasePlayer || entity is BaseNpc)
				{
					Effect.server.ImpactEffect(new HitInfo
					{
						HitPositionWorld = hit.point,
						HitNormalWorld = -hit.normal,
						HitMaterial = StringPool.Get("Flesh")
					});
				}
			}
			if (!(entity != null) || entity.ShouldBlockProjectiles())
			{
				arg = hit.point;
				break;
			}
		}
	}

	public bool CanFire()
	{
		foreach (Item item in base.inventory.itemList)
		{
			if (item.info == ammoType && item.amount > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool UseAmmo()
	{
		foreach (Item item in base.inventory.itemList)
		{
			if (item.info == ammoType && item.amount > 0)
			{
				item.UseItem();
				return true;
			}
		}
		return false;
	}

	private IPrivilege GetCachedPrivilege()
	{
		if (_cachedPriv != null && ((BaseEntity)_cachedPriv).IsDestroyed)
		{
			_cachedPriv = null;
		}
		if (_cachedPriv == null || Time.realtimeSinceStartup > _cacheTimeout)
		{
			_cachedPriv = null;
			_cachedPriv = GetPrivilege();
			_cacheTimeout = Time.realtimeSinceStartup + 3f;
		}
		return _cachedPriv;
	}

	public void SetTriggered(bool triggered)
	{
		if (triggered && CanFire())
		{
			triggeredTime = Time.realtimeSinceStartup;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, triggered && CanFire());
	}
}

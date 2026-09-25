using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using Rust;
using UnityEngine;

public class FlameTurret : StorageContainer
{
	public class UpdateFlameTurretWorkQueue : ObjectWorkQueue<FlameTurret>
	{
		protected override void RunJob(FlameTurret entity)
		{
			if (ShouldAdd(entity))
			{
				entity.ServerThink();
			}
		}

		protected override bool ShouldAdd(FlameTurret entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public static UpdateFlameTurretWorkQueue updateFlameTurretQueueServer = new UpdateFlameTurretWorkQueue();

	public Transform upper;

	public float arc = 45f;

	public float triggeredDuration = 5f;

	public float flameRange = 7f;

	public float flameRadius = 4f;

	public float fuelPerSec = 1f;

	public Transform eyeTransform;

	public List<DamageTypeEntry> damagePerSec;

	public GameObjectRef triggeredEffect;

	public GameObjectRef fireballPrefab;

	public GameObjectRef explosionEffect;

	public TargetTrigger trigger;

	public const Flags Flag_Triggered = Flags.Reserved4;

	private int turnDir = 1;

	private Vector3 aimDir;

	private float lastMovementUpdate;

	private float nextFireballTime;

	private float triggeredTime;

	private float lastServerThink;

	private float triggerCheckRate = 2f;

	private float nextTriggerCheckTime;

	private float _cacheTimeout;

	private IPrivilege _cachedPriv;

	private float pendingFuel;

	public void MovementUpdate()
	{
		float num = Time.realtimeSinceStartup - lastMovementUpdate;
		lastMovementUpdate = Time.realtimeSinceStartup;
		aimDir += new Vector3(0f, num * GetSpinSpeed(), 0f) * turnDir;
		if (aimDir.y >= arc || aimDir.y <= 0f - arc)
		{
			turnDir *= -1;
			aimDir.y = Mathf.Clamp(aimDir.y, 0f - arc, arc);
		}
		if (base.isServer)
		{
			updateFlameTurretQueueServer.Add(this);
		}
	}

	public float GetSpinSpeed()
	{
		return IsTriggered() ? 180 : 45;
	}

	public bool IsTriggered()
	{
		return HasFlag(Flags.Reserved4);
	}

	public Vector3 GetEyePosition()
	{
		return eyeTransform.position;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!IsTriggered())
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRepeating(MovementUpdate, 0f, 0.1f);
	}

	public void ServerThink()
	{
		if (!base.isClient)
		{
			bool num = IsTriggered();
			float delta = Time.realtimeSinceStartup - lastServerThink;
			lastServerThink = Time.realtimeSinceStartup;
			if (IsTriggered() && (Time.realtimeSinceStartup - triggeredTime > triggeredDuration || !HasFuel()))
			{
				SetTriggered(triggered: false);
			}
			if (!IsTriggered() && HasFuel() && CheckTrigger())
			{
				SetTriggered(triggered: true);
				Effect.server.Run(triggeredEffect.resourcePath, base.transform.position, Vector3.up);
			}
			if (num != IsTriggered())
			{
				SendNetworkUpdateImmediate();
			}
			if (IsTriggered())
			{
				DoFlame(delta);
			}
		}
	}

	public void SetTriggered(bool triggered)
	{
		if (triggered && HasFuel())
		{
			triggeredTime = Time.realtimeSinceStartup;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, triggered && HasFuel());
	}

	public override void OnAttacked(HitInfo info)
	{
		if (!base.isClient)
		{
			if (info.damageTypes.IsMeleeType())
			{
				SetTriggered(triggered: true);
			}
			base.OnAttacked(info);
		}
	}

	public bool CheckTrigger()
	{
		if (Time.realtimeSinceStartup < nextTriggerCheckTime)
		{
			return false;
		}
		nextTriggerCheckTime = Time.realtimeSinceStartup + 1f / triggerCheckRate;
		List<RaycastHit> obj = null;
		try
		{
			HashSet<BaseEntity> entityContents = trigger.entityContents;
			if (entityContents == null || entityContents.Count == 0)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			IPrivilege privilege = null;
			foreach (BaseEntity item in entityContents)
			{
				BasePlayer basePlayer = item as BasePlayer;
				if (basePlayer == null || basePlayer.IsSleeping() || !basePlayer.IsAlive() || basePlayer.transform.position.y > GetEyePosition().y + 0.5f)
				{
					continue;
				}
				object obj2 = Interface.CallHook("CanBeTargeted", basePlayer, this);
				if (obj2 is bool)
				{
					if (obj != null)
					{
						Pool.FreeUnmanaged(ref obj);
					}
					return (bool)obj2;
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

	public override void OnDied(HitInfo info)
	{
		float num = (float)GetFuelAmount() / 500f;
		DamageUtil.RadiusDamage(this, LookupPrefab(), GetEyePosition(), 2f, 6f, damagePerSec, 133120, useLineOfSight: true);
		SeismicSensor.Notify(GetEyePosition(), 1);
		Effect.server.Run(explosionEffect.resourcePath, base.transform.position, Vector3.up);
		int num2 = Mathf.CeilToInt(Mathf.Clamp(num * 8f, 1f, 8f));
		for (int i = 0; i < num2; i++)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(fireballPrefab.resourcePath, base.transform.position, base.transform.rotation);
			if ((bool)baseEntity)
			{
				Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
				baseEntity.transform.position = base.transform.position + new Vector3(0f, 1.5f, 0f) + onUnitSphere * UnityEngine.Random.Range(-1f, 1f);
				baseEntity.Spawn();
				baseEntity.SetVelocity(onUnitSphere * UnityEngine.Random.Range(3, 10));
			}
		}
		base.OnDied(info);
	}

	public int GetFuelAmount()
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return 0;
		}
		return slot.amount;
	}

	public bool HasFuel()
	{
		return GetFuelAmount() > 0;
	}

	public bool UseFuel(float seconds)
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return false;
		}
		pendingFuel += seconds * fuelPerSec;
		if (pendingFuel >= 1f)
		{
			int num = Mathf.FloorToInt(pendingFuel);
			slot.UseItem(num);
			Facepunch.Rust.Analytics.Azure.AddPendingItems(this, slot.info.shortname, num, "flame_turret");
			pendingFuel -= num;
		}
		return true;
	}

	public void DoFlame(float delta)
	{
		if (!UseFuel(delta))
		{
			return;
		}
		Ray ray = new Ray(GetEyePosition(), base.transform.TransformDirection(Quaternion.Euler(aimDir) * Vector3.forward));
		Vector3 origin = ray.origin;
		RaycastHit hitInfo;
		bool flag = Physics.SphereCast(ray, 0.4f, out hitInfo, flameRange, 1218652417);
		if (!flag)
		{
			hitInfo.point = origin + ray.direction * flameRange;
		}
		float amount = damagePerSec[0].amount;
		damagePerSec[0].amount = amount * delta;
		DamageUtil.RadiusDamage(this, LookupPrefab(), hitInfo.point - ray.direction * 0.1f, flameRadius * 0.5f, flameRadius, damagePerSec, 2230272, useLineOfSight: true);
		DamageUtil.RadiusDamage(this, LookupPrefab(), base.transform.position + new Vector3(0f, 1.25f, 0f), 0.25f, 0.25f, damagePerSec, 133120, useLineOfSight: false);
		damagePerSec[0].amount = amount;
		if (Time.realtimeSinceStartup >= nextFireballTime)
		{
			nextFireballTime = Time.realtimeSinceStartup + UnityEngine.Random.Range(1f, 2f);
			Vector3 vector = ((UnityEngine.Random.Range(0, 10) <= 7 && flag) ? hitInfo.point : (ray.origin + ray.direction * (flag ? hitInfo.distance : flameRange) * UnityEngine.Random.Range(0.4f, 1f)));
			BaseEntity baseEntity = GameManager.server.CreateEntity(fireballPrefab.resourcePath, vector - ray.direction * 0.25f);
			if ((bool)baseEntity)
			{
				baseEntity.creatorEntity = this;
				baseEntity.Spawn();
			}
		}
	}
}

using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust;
using UnityEngine;

public class ThrownBoomerangServerProjectile : ServerProjectile
{
	public DamageProperties damageProperties;

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	public float worldAttackRadius;

	private Vector3 startPosition;

	private bool willKill;

	public void ProjectileHandleMovement(bool state)
	{
		shouldMoveProjectile = state;
	}

	public void SetStartPosition(Vector3 position)
	{
		startPosition = position;
	}

	public void CalculateDamage(HitInfo info, float scale)
	{
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			info.damageTypes.Add(damageType.type, damageType.amount * scale);
		}
		if (ConVar.Global.developer > 0)
		{
			Debug.Log(" Projectile damage: " + info.damageTypes.Total() + " (scalar=" + scale + ")");
		}
	}

	protected override bool AutomaticallyRotate()
	{
		return false;
	}

	protected override void OnHit(RaycastHit rayHit, BaseEntity hitEntity)
	{
		base.OnHit(rayHit, hitEntity);
		willKill = true;
		HitInfo hitInfo = new HitInfo();
		hitInfo.Initiator = base.baseEntity.creatorEntity;
		hitInfo.WeaponPrefab = base.baseEntity;
		hitInfo.IsPredicting = false;
		hitInfo.DoDecals = true;
		hitInfo.DoHitEffects = true;
		hitInfo.DidHit = true;
		hitInfo.HitPositionWorld = rayHit.point;
		hitInfo.HitNormalWorld = rayHit.normal;
		hitInfo.ProjectileVelocity = base.CurrentVelocity;
		hitInfo.PointStart = startPosition;
		hitInfo.PointEnd = rayHit.point;
		hitInfo.damageProperties = damageProperties;
		CalculateDamage(hitInfo, 1f);
		hitInfo.HitMaterial = StringPool.Get(GetMaterialName(rayHit));
		ThrownBoomerang obj = base.baseEntity as ThrownBoomerang;
		obj.OnHit();
		if (hitEntity.IsValid())
		{
			hitInfo.HitEntity = hitEntity;
			hitInfo.HitPositionLocal = hitInfo.HitEntity.transform.InverseTransformPoint(hitInfo.HitPositionWorld);
			hitInfo.HitNormalLocal = hitInfo.HitEntity.transform.InverseTransformDirection(hitInfo.HitNormalWorld);
			Shield shield = hitInfo.HitEntity as Shield;
			if (hitInfo.HitEntity is BasePlayer || hitInfo.HitEntity is BaseNpc || shield != null)
			{
				hitInfo.HitMaterial = StringPool.Get((shield != null) ? shield.GetHitMaterialString() : "Flesh");
			}
			if (!(hitInfo.HitEntity is BasePlayer) && !(hitInfo.HitEntity is BaseNpc))
			{
				hitInfo.damageTypes.ScaleAll(0.03f);
			}
			hitInfo.HitEntity.OnAttacked(hitInfo);
		}
		obj.CreateWorldModel(hitInfo, base.CurrentVelocity.normalized);
		Effect.server.ImpactEffect(hitInfo);
	}

	protected override bool DoHitDetection(Vector3 velocityToUse, float distance)
	{
		List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
		List<RaycastHit> obj2 = Facepunch.Pool.Get<List<RaycastHit>>();
		Vector3 position = base.transform.position;
		GamePhysics.TraceAll(new Ray(position, velocityToUse.normalized), radius, obj, distance + scanRange, mask, QueryTriggerInteraction.Ignore);
		GamePhysics.TraceAll(new Ray(position, velocityToUse.normalized), worldAttackRadius, obj2, distance + scanRange, mask, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!(entity != null) || !entity.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity)) && (entity is BasePlayer || entity is BaseNpc) && IsAValidHit(entity) && GamePhysics.LineOfSight(base.transform.position, item.point, mask, 0f))
			{
				ProcessHit(item, entity, position);
				Facepunch.Pool.FreeUnmanaged(ref obj);
				Facepunch.Pool.FreeUnmanaged(ref obj2);
				return true;
			}
		}
		foreach (RaycastHit item2 in obj2)
		{
			BaseEntity entity2 = RaycastHitEx.GetEntity(item2);
			if ((!(entity2 != null) || !entity2.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity2)) && IsAValidHit(entity2) && IsShootable(item2))
			{
				ProcessHit(item2, entity2, position);
				Facepunch.Pool.FreeUnmanaged(ref obj);
				Facepunch.Pool.FreeUnmanaged(ref obj2);
				return true;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		return false;
	}

	protected override void PostDoMove()
	{
		if (willKill)
		{
			base.baseEntity.Kill();
		}
	}

	protected override bool IsAValidHit(BaseEntity hitEnt)
	{
		if (hitEnt != null)
		{
			if (base.baseEntity.creatorEntity.IsValid() && hitEnt.net.ID == base.baseEntity.creatorEntity.net.ID)
			{
				return false;
			}
			if (ignoreEntity.IsValid() && hitEnt.net.ID == ignoreEntity.net.ID)
			{
				return false;
			}
		}
		return true;
	}

	private string GetMaterialName(RaycastHit rayHit)
	{
		string result = "generic";
		if (RaycastHitEx.GetCollider(rayHit) != null && RaycastHitEx.GetCollider(rayHit).sharedMaterial != null)
		{
			result = RaycastHitEx.GetCollider(rayHit).sharedMaterial.name;
		}
		if (RaycastHitEx.IsWaterHit(rayHit))
		{
			result = "Water";
		}
		return result;
	}
}

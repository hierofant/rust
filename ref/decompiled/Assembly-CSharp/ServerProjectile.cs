using System;
using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2;
using UnityEngine;

public class ServerProjectile : EntityComponent<BaseEntity>
{
	public interface IProjectileImpact
	{
		void ProjectileImpact(RaycastHit hitInfo, Vector3 rayOrigin);
	}

	public Vector3 initialVelocity;

	public float drag;

	public float gravityModifier = 1f;

	public float speed = 15f;

	public float scanRange;

	public Vector3 swimScale;

	public Vector3 swimSpeed;

	public float radius;

	public bool IgnoreAI;

	public bool runSpawnHitChecks;

	[HideInInspector]
	public BaseEntity ignoreEntity;

	protected bool shouldMoveProjectile = true;

	public bool impacted;

	public float swimRandom;

	public virtual bool HasRangeLimit => true;

	protected virtual int mask => 1237003025;

	public Vector3 CurrentVelocity { get; set; }

	public bool Impacted => impacted;

	public float GetMaxRange(float maxFuseTime)
	{
		if (gravityModifier == 0f)
		{
			return float.PositiveInfinity;
		}
		float a = Mathf.Sin(MathF.PI / 2f) * speed * speed / (0f - Physics.gravity.y * gravityModifier);
		float b = speed * maxFuseTime;
		return Mathf.Min(a, b);
	}

	protected void FixedUpdate()
	{
		if (base.baseEntity != null && base.baseEntity.isServer && base.baseEntity.IsFullySpawned())
		{
			DoMovement();
		}
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		base.ServerInitPostNetworkGroupAssign();
		if (runSpawnHitChecks)
		{
			CheckSpawnedInsideCollider(CurrentVelocity.normalized);
		}
	}

	private void CheckSpawnedInsideCollider(Vector3 direction)
	{
		List<Collider> obj = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(base.transform.position, Mathf.Max(radius, 0.01f), obj, mask);
		bool num = obj.Count > 0;
		Pool.FreeUnmanaged(ref obj);
		if (!num)
		{
			return;
		}
		List<RaycastHit> obj2 = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(base.transform.position, -direction), radius, obj2, 1f, mask, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit item in obj2)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!(entity != null) || !entity.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity)) && IsAValidHit(entity) && IsShootable(item))
			{
				Pool.FreeUnmanaged(ref obj2);
				ProcessHit(item, entity, base.transform.position);
				return;
			}
		}
		Pool.FreeUnmanaged(ref obj2);
	}

	public void AdjustVelocity(Vector3 adjustment)
	{
		CurrentVelocity += adjustment;
	}

	public virtual Vector3 GetVelocityStep()
	{
		return Physics.gravity * gravityModifier * Time.fixedDeltaTime * Time.timeScale;
	}

	public virtual void InitializeVelocity(Vector3 overrideVel)
	{
		if (AutomaticallyRotate())
		{
			base.transform.rotation = Quaternion.LookRotation(overrideVel.normalized);
		}
		initialVelocity = overrideVel;
		CurrentVelocity = overrideVel;
	}

	public void SetVelocity(Vector3 overrideVel)
	{
		if (AutomaticallyRotate() && overrideVel != Vector3.zero)
		{
			base.transform.rotation = Quaternion.LookRotation(overrideVel.normalized);
		}
		CurrentVelocity = overrideVel;
	}

	public virtual bool DoMovement()
	{
		if (base.baseEntity.isClient)
		{
			return false;
		}
		if (impacted)
		{
			return false;
		}
		CurrentVelocity += GetVelocityStep();
		Vector3 vector = AddSwim(CurrentVelocity);
		float num = vector.magnitude * Time.fixedDeltaTime;
		if (DoHitDetection(vector, num))
		{
			return false;
		}
		if (shouldMoveProjectile)
		{
			base.transform.position += base.transform.forward * num;
		}
		if (AutomaticallyRotate() && vector != Vector3.zero)
		{
			base.transform.rotation = Quaternion.LookRotation(vector.normalized);
		}
		PostDoMove();
		return true;
	}

	protected virtual bool DoHitDetection(Vector3 velocityToUse, float distance)
	{
		List<RaycastHit> obj = Pool.Get<List<RaycastHit>>();
		Vector3 position = base.transform.position;
		GamePhysics.TraceAll(new Ray(position, velocityToUse.normalized), radius, obj, distance + scanRange, mask, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!(entity != null) || !entity.isClient) && (!IgnoreAI || !IsAnIgnoredAI(entity)) && IsAValidHit(entity) && IsShootable(item))
			{
				ProcessHit(item, entity, position);
				Pool.FreeUnmanaged(ref obj);
				return true;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return false;
	}

	private Vector3 AddSwim(Vector3 currentVelocity)
	{
		Vector3 result = currentVelocity;
		if (swimScale != Vector3.zero)
		{
			if (swimRandom == 0f)
			{
				swimRandom = UnityEngine.Random.Range(0f, 20f);
			}
			float num = Time.time + swimRandom;
			Vector3 direction = new Vector3(Mathf.Sin(num * swimSpeed.x) * swimScale.x, Mathf.Cos(num * swimSpeed.y) * swimScale.y, Mathf.Sin(num * swimSpeed.z) * swimScale.z);
			direction = base.transform.InverseTransformDirection(direction);
			result += direction;
		}
		return result;
	}

	protected void ProcessHit(RaycastHit hitInfo, BaseEntity hitEnt, Vector3 rayOrigin)
	{
		base.transform.position += base.transform.forward * Mathf.Max(0f, hitInfo.distance - 0.1f);
		GetComponent<IProjectileImpact>()?.ProjectileImpact(hitInfo, rayOrigin);
		SingletonComponent<NpcNoiseManager>.Instance.OnServerProjectileHit(base.baseEntity, this, hitInfo);
		impacted = true;
		OnHit(hitInfo, hitEnt);
		PostDoMove();
	}

	protected bool IsShootable(RaycastHit hitInfo)
	{
		ColliderInfo colliderInfo = ((hitInfo.collider != null) ? hitInfo.collider.GetComponent<ColliderInfo>() : null);
		if (!(colliderInfo == null))
		{
			return colliderInfo.HasFlag(ColliderInfo.Flags.Shootable);
		}
		return true;
	}

	protected virtual void OnHit(RaycastHit rayHit, BaseEntity hitEntity)
	{
	}

	protected virtual void PostDoMove()
	{
	}

	protected virtual bool IsAValidHit(BaseEntity hitEnt)
	{
		if (!hitEnt.IsValid())
		{
			return true;
		}
		if (base.baseEntity.creatorEntity.IsValid() && hitEnt.net.ID == base.baseEntity.creatorEntity.net.ID)
		{
			return false;
		}
		if (ignoreEntity.IsValid() && hitEnt.net.ID == ignoreEntity.net.ID)
		{
			return false;
		}
		return true;
	}

	protected virtual bool IsAnIgnoredAI(BaseEntity hitEnt)
	{
		return hitEnt is ScientistNPC;
	}

	protected virtual bool AutomaticallyRotate()
	{
		return true;
	}
}

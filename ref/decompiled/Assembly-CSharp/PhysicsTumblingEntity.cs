using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PhysicsTumblingEntity : BaseEntity
{
	[ServerVar(Saved = true, Help = "Minimum force applied on collision to cause tumbling")]
	public static float min_tumbling_force = 600f;

	[ServerVar(Saved = true, Help = "Maximum force applied on collision to cause tumbling")]
	public static float max_tumbling_force = 1000f;

	[ServerVar(Saved = true, Help = "Cone angle in degrees for randomizing tumbling force direction")]
	public static float tumbling_force_cone_angle = 30f;

	[ServerVar(Saved = true, Help = "Override for rigidbody drag. Set to -1 to disable.")]
	public static float drag_override = -1f;

	[ServerVar(Saved = true, Help = "Override for rigidbody angular drag. Set to -1 to disable.")]
	public static float angular_drag_override = -1f;

	[ServerVar(Saved = true, Help = "Multiplier for impulse applied to players when ragdolled by this entity")]
	public static float player_impulse_multiplier = 10f;

	[ServerVar(Saved = true, Help = "Minimum velocity required for an object to get tumbling force applied on collision")]
	public static float velocity_threshold_for_tumbling_force = 1.5f;

	private Rigidbody rb;

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		if (hitEntity == null)
		{
			hitEntity = GameObjectEx.ToBaseEntity(collision.collider);
		}
		if (hitEntity == this || (hitEntity != null && hitEntity.isServer != base.isServer) || hitEntity is TimedExplosive)
		{
			return;
		}
		float num = Random.Range(min_tumbling_force, max_tumbling_force);
		if (!(rb.linearVelocity.magnitude < velocity_threshold_for_tumbling_force))
		{
			if (hitEntity == null)
			{
				RagdollPlayers(collision.contacts[0].point, -collision.impulse.normalized * player_impulse_multiplier);
				return;
			}
			Vector3 vector = RandomVectorInCone(collision.impulse.normalized, tumbling_force_cone_angle);
			vector *= num;
			rb.AddForce(vector, ForceMode.Impulse);
			Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].point + vector, Color.gray, 10f);
		}
	}

	public void FixedUpdate()
	{
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		if (drag_override != -1f && rb.linearDamping != drag_override)
		{
			rb.linearDamping = drag_override;
		}
		if (angular_drag_override != -1f && rb.angularDamping != angular_drag_override)
		{
			rb.angularDamping = angular_drag_override;
		}
	}

	private void RagdollPlayers(Vector3 worldPosition, Vector3 impulse)
	{
		List<BasePlayer> obj = Pool.Get<List<BasePlayer>>();
		Vis.Entities(worldPosition, 0.4f, obj, 131072);
		foreach (BasePlayer item in obj)
		{
			if (item.isServer && !item.IsDead() && !item.InSafeZone())
			{
				item.Ragdoll(item.GetWorldVelocity() + impulse);
			}
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public static Vector3 RandomVectorInCone(Vector3 normal, float coneAngle)
	{
		normal = normal.normalized;
		float angle = Random.Range(0f, coneAngle);
		float angle2 = Random.Range(0f, 360f);
		Vector3 vector = Vector3.Cross(normal, Vector3.up);
		if (vector.sqrMagnitude < 0.001f)
		{
			vector = Vector3.Cross(normal, Vector3.right);
		}
		vector.Normalize();
		vector = Quaternion.AngleAxis(angle2, normal) * vector;
		return (Quaternion.AngleAxis(angle, vector) * normal).normalized;
	}
}

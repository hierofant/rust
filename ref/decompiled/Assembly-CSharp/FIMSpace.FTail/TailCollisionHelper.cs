using UnityEngine;
using UnityEngine.Tilemaps;

namespace FIMSpace.FTail;

[AddComponentMenu("FImpossible Creations/Hidden/Tail Collision Helper")]
public class TailCollisionHelper : MonoBehaviour
{
	public TailAnimator2 ParentTail;

	public Collider TailCollider;

	public Collider2D TailCollider2D;

	public int Index;

	private Transform previousCollision;

	internal Rigidbody RigBody { get; private set; }

	internal Rigidbody2D RigBody2D { get; private set; }

	internal TailCollisionHelper Init(bool addRigidbody = true, float mass = 1f, bool kinematic = false)
	{
		if (TailCollider2D == null)
		{
			if (addRigidbody)
			{
				Rigidbody rigidbody = GetComponent<Rigidbody>();
				if (!rigidbody)
				{
					rigidbody = base.gameObject.AddComponent<Rigidbody>();
				}
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				rigidbody.useGravity = false;
				rigidbody.isKinematic = kinematic;
				rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				rigidbody.mass = mass;
				RigBody = rigidbody;
			}
			else
			{
				RigBody = GetComponent<Rigidbody>();
				if ((bool)RigBody)
				{
					RigBody.mass = mass;
				}
			}
		}
		else if (addRigidbody)
		{
			Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
			if (!rigidbody2D)
			{
				rigidbody2D = base.gameObject.AddComponent<Rigidbody2D>();
			}
			rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
			rigidbody2D.gravityScale = 0f;
			rigidbody2D.isKinematic = kinematic;
			rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
			rigidbody2D.mass = mass;
			RigBody2D = rigidbody2D;
		}
		else
		{
			RigBody2D = GetComponent<Rigidbody2D>();
			if ((bool)RigBody2D)
			{
				RigBody2D.mass = mass;
			}
		}
		return this;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (ParentTail == null)
		{
			Object.Destroy(this);
			return;
		}
		TailCollisionHelper component = collision.transform.GetComponent<TailCollisionHelper>();
		if ((!component || (ParentTail.CollideWithOtherTails && !(component.ParentTail == ParentTail))) && !ParentTail._TransformsGhostChain.Contains(collision.transform) && !ParentTail.IgnoredColliders.Contains(collision.collider))
		{
			ParentTail.CollisionDetection(Index, collision);
			previousCollision = collision.transform;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.transform == previousCollision)
		{
			ParentTail.ExitCollision(Index);
			previousCollision = null;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger && (!ParentTail.IgnoreMeshColliders || !(other is MeshCollider)) && !(other is CharacterController) && !ParentTail._TransformsGhostChain.Contains(other.transform) && !ParentTail.IgnoredColliders.Contains(other) && (ParentTail.CollideWithOtherTails || !other.transform.GetComponent<TailCollisionHelper>()))
		{
			ParentTail.AddCollider(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (ParentTail.IncludedColliders.Contains(other) && !ParentTail.DynamicAlwaysInclude.Contains(other))
		{
			ParentTail.IncludedColliders.Remove(other);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.isTrigger && !(other is CompositeCollider2D) && !(other is TilemapCollider2D) && !(other is EdgeCollider2D) && !ParentTail._TransformsGhostChain.Contains(other.transform) && !ParentTail.IgnoredColliders2D.Contains(other) && (ParentTail.CollideWithOtherTails || !other.transform.GetComponent<TailCollisionHelper>()))
		{
			ParentTail.AddCollider(other);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (ParentTail.IncludedColliders2D.Contains(other) && !ParentTail.DynamicAlwaysInclude.Contains(other))
		{
			ParentTail.IncludedColliders2D.Remove(other);
		}
	}
}

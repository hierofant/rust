using UnityEngine;

namespace FIMSpace;

public class FImp_ColliderData_CharacterCapsule : FImp_ColliderData_Base
{
	private Vector3 Top;

	private Vector3 Bottom;

	private Vector3 Direction;

	private float radius;

	private float scaleFactor;

	private float preRadius;

	public CharacterController Capsule { get; private set; }

	public FImp_ColliderData_CharacterCapsule(CharacterController collider)
	{
		Is2D = false;
		base.Transform = collider.transform;
		base.Collider = collider;
		base.Transform = collider.transform;
		Capsule = collider;
		base.ColliderType = EFColliderType.Capsule;
		CalculateCapsuleParameters(Capsule, ref Direction, ref radius, ref scaleFactor);
		RefreshColliderData();
	}

	public override void RefreshColliderData()
	{
		if (!base.IsStatic)
		{
			bool flag = false;
			if (!FEngineering.VIsSame(previousPosition, base.Transform.position))
			{
				flag = true;
			}
			else if (!FEngineering.QIsSame(base.Transform.rotation, previousRotation))
			{
				flag = true;
			}
			else if (preRadius != Capsule.radius || !FEngineering.VIsSame(previousScale, base.Transform.lossyScale))
			{
				CalculateCapsuleParameters(Capsule, ref Direction, ref radius, ref scaleFactor);
			}
			if (flag)
			{
				GetCapsuleHeadsPositions(Capsule, ref Top, ref Bottom, Direction, radius, scaleFactor);
			}
			base.RefreshColliderData();
			previousPosition = base.Transform.position;
			previousRotation = base.Transform.rotation;
			previousScale = base.Transform.lossyScale;
			preRadius = Capsule.radius;
		}
	}

	public override bool PushIfInside(ref Vector3 point, float pointRadius, Vector3 pointOffset)
	{
		return PushOutFromCapsuleCollider(pointRadius, ref point, Top, Bottom, radius, pointOffset);
	}

	public static bool PushOutFromCapsuleCollider(float segmentColliderRadius, ref Vector3 segmentPos, Vector3 capSphereCenter1, Vector3 capSphereCenter2, float capsuleRadius, Vector3 segmentOffset, bool is2D = false)
	{
		float num = capsuleRadius + segmentColliderRadius;
		Vector3 vector = capSphereCenter2 - capSphereCenter1;
		Vector3 vector2 = segmentPos + segmentOffset - capSphereCenter1;
		float num2 = Vector3.Dot(vector2, vector);
		if (num2 <= 0f)
		{
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude > 0f && sqrMagnitude < num * num)
			{
				segmentPos = capSphereCenter1 - segmentOffset + vector2 * (num / Mathf.Sqrt(sqrMagnitude));
				return true;
			}
		}
		else
		{
			float sqrMagnitude2 = vector.sqrMagnitude;
			if (num2 >= sqrMagnitude2)
			{
				vector2 = segmentPos + segmentOffset - capSphereCenter2;
				float sqrMagnitude3 = vector2.sqrMagnitude;
				if (sqrMagnitude3 > 0f && sqrMagnitude3 < num * num)
				{
					segmentPos = capSphereCenter2 - segmentOffset + vector2 * (num / Mathf.Sqrt(sqrMagnitude3));
					return true;
				}
			}
			else if (sqrMagnitude2 > 0f)
			{
				vector2 -= vector * (num2 / sqrMagnitude2);
				float sqrMagnitude4 = vector2.sqrMagnitude;
				if (sqrMagnitude4 > 0f && sqrMagnitude4 < num * num)
				{
					float num3 = Mathf.Sqrt(sqrMagnitude4);
					segmentPos += vector2 * ((num - num3) / num3);
					return true;
				}
			}
		}
		return false;
	}

	protected static void CalculateCapsuleParameters(CharacterController capsule, ref Vector3 direction, ref float trueRadius, ref float scalerFactor)
	{
		Transform transform = capsule.transform;
		direction = Vector3.up;
		scalerFactor = transform.lossyScale.y;
		float num = ((transform.lossyScale.x > transform.lossyScale.z) ? transform.lossyScale.x : transform.lossyScale.z);
		trueRadius = capsule.radius * num;
	}

	protected static void GetCapsuleHeadsPositions(CharacterController capsule, ref Vector3 upper, ref Vector3 bottom, Vector3 direction, float radius, float scalerFactor)
	{
		Vector3 direction2 = direction * (capsule.height / 2f * scalerFactor - radius);
		upper = capsule.transform.position + capsule.transform.TransformDirection(direction2) + capsule.transform.TransformVector(capsule.center);
		Vector3 direction3 = -direction * (capsule.height / 2f * scalerFactor - radius);
		bottom = capsule.transform.position + capsule.transform.TransformDirection(direction3) + capsule.transform.TransformVector(capsule.center);
	}
}

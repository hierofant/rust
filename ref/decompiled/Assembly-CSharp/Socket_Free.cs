using System.Collections.Generic;
using UnityEngine;

public class Socket_Free : Socket_Base
{
	public Vector3 idealPlacementNormal = Vector3.up;

	public bool useTargetNormal = true;

	public bool snapToTargetProvidedRotations;

	[Tooltip("Allows for rolling the rotation of the deployable depending on distance. If you don't want the deployable to be able to rotate on it's Z axis, disable this.")]
	public bool blendAimAngle = true;

	public override bool TestTarget(Construction.Target target)
	{
		return target.onTerrain;
	}

	public override Construction.Placement DoPlacement(Construction.Target target)
	{
		Quaternion identity = Quaternion.identity;
		if (snapToTargetProvidedRotations && !target.isHoldingShift && target.entity != null && target.player != null && target.entity is IPlacementDirectionProvider placementDirectionProvider)
		{
			List<Vector3> snapForwardDirections = placementDirectionProvider.GetSnapForwardDirections();
			identity = GetTargetSnappedRotation(target.entity.transform, target.player.transform.position, target.position, target.rotation.y, snapForwardDirections);
		}
		else if (useTargetNormal)
		{
			Vector3 normal = target.normal;
			Vector3 upwards = idealPlacementNormal;
			if (blendAimAngle || Mathf.Abs(target.normal.y) > 0.98f)
			{
				Vector3 normalized = (target.position - target.ray.origin).normalized;
				upwards = Vector3.Lerp(t: Mathf.Abs(Vector3.Dot(normalized, normal)), a: normalized, b: idealPlacementNormal);
			}
			identity = Quaternion.LookRotation(normal, upwards) * Quaternion.Inverse(rotation) * Quaternion.Euler(target.rotation);
		}
		else
		{
			Vector3 normalized2 = (target.position - target.ray.origin).normalized;
			normalized2.y = 0f;
			identity = Quaternion.LookRotation(normalized2, idealPlacementNormal) * Quaternion.Euler(target.rotation);
		}
		Vector3 vector = target.position;
		vector -= identity * position;
		Construction.Placement result = new Construction.Placement(target);
		result.rotation = identity;
		result.position = vector;
		return result;
	}

	private Quaternion GetTargetSnappedRotation(Transform targetTransform, Vector3 playerWorldPos, Vector3 placementWorldPos, float yOffsetDegrees, List<Vector3> cachedLocalDirs)
	{
		Vector3 vector = placementWorldPos - playerWorldPos;
		vector = Vector3.ProjectOnPlane(vector, targetTransform.up);
		vector.Normalize();
		Quaternion quaternion = Quaternion.LookRotation(SnapToBestTargetLocalCardinal(vector, targetTransform, cachedLocalDirs), targetTransform.up);
		return Quaternion.AngleAxis(yOffsetDegrees, targetTransform.up) * quaternion;
	}

	private Vector3 SnapToBestTargetLocalCardinal(Vector3 desiredWorldForward, Transform target, List<Vector3> cachedLocalDirs)
	{
		float num = -1f;
		float num2 = 0f;
		Vector3 vector = target.forward;
		for (int i = 0; i < cachedLocalDirs.Count; i++)
		{
			Vector3 vector2 = Vector3.ProjectOnPlane(target.TransformDirection(cachedLocalDirs[i]), target.up);
			if (!(vector2.sqrMagnitude < 0.0001f))
			{
				vector2.Normalize();
				float num3 = Vector3.Dot(desiredWorldForward, vector2);
				float num4 = Mathf.Abs(num3);
				if (num4 > num)
				{
					num = num4;
					num2 = num3;
					vector = vector2;
				}
			}
		}
		if (!(num2 >= 0f))
		{
			return -vector;
		}
		return vector;
	}
}

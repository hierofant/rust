using System;
using UnityEngine;

public class SocketMod_UseTargetOrientation : SocketMod
{
	[Flags]
	public enum OrientationAxes
	{
		X = 1,
		Y = 2,
		Z = 4
	}

	public OrientationAxes inheritAxes;

	public GameObjectRef[] onlyOrientateToThese = Array.Empty<GameObjectRef>();

	public bool ignoreIfHoldingShift;

	private const float alignAxisThreshold = 0.0001f;

	public override bool DoCheck(ref Construction.Placement place)
	{
		return true;
	}

	public override void ModifyPlacement(ref Construction.Placement place)
	{
		if (inheritAxes == (OrientationAxes)0 || (ignoreIfHoldingShift && place.isHoldingShift) || place.transform == null)
		{
			return;
		}
		if (onlyOrientateToThese.Length != 0)
		{
			bool flag = false;
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(place.transform);
			if (baseEntity != null)
			{
				GameObjectRef[] array = onlyOrientateToThese;
				foreach (GameObjectRef gameObjectRef in array)
				{
					if (baseEntity.prefabID == gameObjectRef.resourceID)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return;
			}
		}
		Quaternion rotation = place.rotation;
		Quaternion rotation2 = place.transform.rotation;
		bool flag2 = (inheritAxes & OrientationAxes.X) != 0;
		bool flag3 = (inheritAxes & OrientationAxes.Y) != 0;
		bool flag4 = (inheritAxes & OrientationAxes.Z) != 0;
		switch ((flag2 ? 1 : 0) + (flag3 ? 1 : 0) + (flag4 ? 1 : 0))
		{
		case 1:
		{
			Vector3 fromDirection = (flag2 ? (rotation * Vector3.right) : (flag3 ? (rotation * Vector3.up) : (rotation * Vector3.forward)));
			Vector3 toDirection = (flag2 ? (rotation2 * Vector3.right) : (flag3 ? (rotation2 * Vector3.up) : (rotation2 * Vector3.forward)));
			if (fromDirection.sqrMagnitude > 0.0001f && toDirection.sqrMagnitude > 0.0001f)
			{
				Quaternion quaternion = Quaternion.FromToRotation(fromDirection, toDirection);
				place.rotation = quaternion * rotation;
			}
			break;
		}
		case 2:
			if (flag2 && flag3)
			{
				Vector3 lhs = rotation2 * Vector3.right;
				Vector3 vector = rotation2 * Vector3.up;
				Vector3 normalized = Vector3.Cross(lhs, vector).normalized;
				if (normalized.sqrMagnitude > 0.0001f)
				{
					place.rotation = Quaternion.LookRotation(normalized, vector);
				}
			}
			else if (flag3 && flag4)
			{
				Vector3 vector2 = rotation2 * Vector3.up;
				Vector3 vector3 = rotation2 * Vector3.forward;
				if (Vector3.Cross(vector2, vector3).normalized.sqrMagnitude > 0.0001f)
				{
					place.rotation = Quaternion.LookRotation(vector3, vector2);
				}
			}
			else if (flag2 && flag4)
			{
				Vector3 rhs = rotation2 * Vector3.right;
				Vector3 vector4 = rotation2 * Vector3.forward;
				Vector3 normalized2 = Vector3.Cross(vector4, rhs).normalized;
				if (normalized2.sqrMagnitude > 0.0001f)
				{
					place.rotation = Quaternion.LookRotation(vector4, normalized2);
				}
			}
			break;
		case 3:
			place.rotation = rotation2;
			break;
		}
	}
}

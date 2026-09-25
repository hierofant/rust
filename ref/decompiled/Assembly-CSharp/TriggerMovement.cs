using System;
using UnityEngine;

public class TriggerMovement : TriggerBase
{
	[Tooltip("If set, the entering object must have line of sight to this transform to be added, note this is only checked on entry")]
	public Transform losEyes;

	public BaseEntity.MovementModify movementModify;

	[NonSerialized]
	private float scale = 1f;

	public void SetMovementScale(float newScale)
	{
		scale = newScale;
	}

	public float GetMovementScale()
	{
		return scale;
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if (obj == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null)
		{
			return null;
		}
		if (losEyes != null)
		{
			if (entityContents != null && entityContents.Contains(baseEntity))
			{
				return baseEntity.gameObject;
			}
			if (!baseEntity.IsVisible(losEyes.transform.position, baseEntity.CenterPoint()))
			{
				return null;
			}
		}
		return baseEntity.gameObject;
	}
}

using UnityEngine;

public class TriggerPlayer : TriggerBase
{
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
		if (!(baseEntity is BasePlayer) || baseEntity.IsNpc)
		{
			return null;
		}
		return baseEntity.gameObject;
	}
}

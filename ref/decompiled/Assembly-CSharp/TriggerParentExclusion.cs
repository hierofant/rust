using UnityEngine;

public class TriggerParentExclusion : TriggerBase, IServerComponent
{
	public bool IgnoreIfOnLadder;

	public override GameObject InterestedInObject(GameObject obj)
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
		if (baseEntity.isClient)
		{
			return null;
		}
		if (IgnoreIfOnLadder && baseEntity is BasePlayer basePlayer && basePlayer.OnLadder())
		{
			return null;
		}
		return baseEntity.gameObject;
	}
}

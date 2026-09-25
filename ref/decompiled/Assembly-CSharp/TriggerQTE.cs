using ConVar;
using Rust.Ai;
using UnityEngine;

public class TriggerQTE : TriggerBase, IServerComponent
{
	public WildlifeHazard Entity;

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
		if (baseEntity.isClient)
		{
			return null;
		}
		if (baseEntity as BasePlayer == null)
		{
			return null;
		}
		if (baseEntity.IsNpc)
		{
			return null;
		}
		if (AI.ignoreplayers)
		{
			return null;
		}
		if (SimpleAIMemory.PlayerIgnoreList.Contains(baseEntity as BasePlayer))
		{
			return null;
		}
		return baseEntity.gameObject;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (Entity == null)
		{
			Debug.LogWarning("TriggerQTE with no Entity linked", base.gameObject);
		}
		else
		{
			Entity.TriggeredByPlayer(ent as BasePlayer);
		}
	}
}

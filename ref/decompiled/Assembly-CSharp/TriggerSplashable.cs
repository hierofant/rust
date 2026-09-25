using UnityEngine;

public class TriggerSplashable : TriggerBase
{
	public CapsuleCollider Capsule;

	private ListDictionary<BaseEntity, (bool visible, Vector3 lastCheckPos)> visibleState = new ListDictionary<BaseEntity, (bool, Vector3)>();

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if (obj.GetComponent<ISplashable>() == null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (baseEntity == null || baseEntity.isClient)
		{
			return null;
		}
		return base.InterestedInObject(obj);
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		if (visibleState.ContainsKey(ent))
		{
			visibleState.Remove(ent);
		}
	}

	public bool ShouldCheckLineOfSight(BaseEntity ent)
	{
		Vector3 position = ent.transform.position;
		if (visibleState.ContainsKey(ent))
		{
			return (visibleState[ent].lastCheckPos - position).sqrMagnitude > 1f;
		}
		return true;
	}

	public bool HasLineOfSight(BaseEntity ent)
	{
		if (visibleState.ContainsKey(ent))
		{
			return visibleState[ent].visible;
		}
		return false;
	}

	public void RecordLineOfSight(BaseEntity ent, bool state)
	{
		if (visibleState.ContainsKey(ent))
		{
			visibleState[ent] = (state, ent.transform.position);
		}
		else
		{
			visibleState.Add(ent, (state, ent.transform.position));
		}
	}
}

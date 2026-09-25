using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

public class TriggerComfort : TriggerBase
{
	public float triggerSize;

	public float baseComfort = 0.5f;

	public float minComfortRange = 2.5f;

	public bool applyToHorses;

	private const float perPlayerComfortBonus = 0.25f;

	private const float horseComfortBonus = 0.5f;

	private const float bonusComfort = 0f;

	private List<BaseEntity> _entities = new List<BaseEntity>();

	private void OnValidate()
	{
		triggerSize = GetComponent<SphereCollider>().radius * base.transform.localScale.y;
	}

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
		return baseEntity.gameObject;
	}

	public float CalculateComfort(Vector3 position, BasePlayer forPlayer = null)
	{
		float num = Vector3.Distance(base.gameObject.transform.position, position);
		float num2 = Mathf.Clamp(minComfortRange, 0f, triggerSize);
		float num3 = triggerSize - num2;
		float max = ((num3 > 0f) ? (num / num3) : 0f);
		float num4 = 1f - Mathf.Clamp(num - num2, 0f, max);
		bool flag = false;
		float num5 = 0f;
		foreach (BaseEntity entity in _entities)
		{
			if (entity == forPlayer)
			{
				continue;
			}
			if (entity is BasePlayer { IsNpc: false } basePlayer)
			{
				float num6 = 1f;
				if (basePlayer.IsSleeping())
				{
					num6 = 0.5f;
				}
				else if (!basePlayer.IsAlive())
				{
					num6 = 0f;
				}
				num5 += 0.25f * num6;
			}
			if (applyToHorses && (entity is RidableHorse || entity is RidableHorse) && !flag)
			{
				num5 += 0.5f;
				flag = true;
			}
		}
		float num7 = 0f + num5;
		return (baseComfort + num7) * num4;
	}

	public override void OnEntityEnter(BaseEntity ent)
	{
		if ((ent is BasePlayer || ent is RidableHorse || ent is RidableHorse) && Interface.CallHook("OnEntityEnter", this, ent) == null)
		{
			_entities.Add(ent);
		}
	}

	public override void OnEntityLeave(BaseEntity ent)
	{
		if ((ent is BasePlayer || ent is RidableHorse || ent is RidableHorse) && Interface.CallHook("OnEntityLeave", this, ent) == null)
		{
			_entities.Remove(ent);
		}
	}
}

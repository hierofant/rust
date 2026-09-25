using System;
using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Hurt : State_PlayAnimationRM
{
	[SerializeField]
	private RootMotionData StrongHitL;

	[SerializeField]
	private RootMotionData StrongHitR;

	[SerializeField]
	private RootMotionData WeakHit;

	[SerializeField]
	private float StaggerRatio = 0.5f;

	public bool ShouldStagger(BaseEntity owner, HitInfo hitInfo)
	{
		float num = owner.Health() + hitInfo.damageTypes.Total();
		float num2 = owner.MaxHealth() * StaggerRatio;
		if (num > num2)
		{
			return owner.Health() < num2;
		}
		return false;
	}

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		if (payload.hitInfo == null && AI.logIssues && AI.logIssues)
		{
			Debug.LogError($"Entering {base.Name} without HitInfo payload, this should not happen and may cause issues with stats tracking. Owner: {Owner}", Owner);
		}
		if (payload.hitInfo.damageTypes.Has(DamageType.Heat))
		{
			base.Blackboard.Add("HitByFire");
		}
		if (WeakHit == null || ShouldStagger(Owner, payload.hitInfo))
		{
			bool flag = Vector3.Dot(payload.hitInfo.attackNormal, Owner.transform.right) > 0f;
			Animation = (flag ? StrongHitL : StrongHitR);
		}
		else
		{
			Animation = WeakHit;
		}
		if (payload.hitInfo.Initiator is BaseCombatEntity baseCombatEntity)
		{
			bool flag2 = true;
			if (base.Senses.FindTarget(out var target))
			{
				bool num = Owner.Distance(baseCombatEntity) < 16f;
				bool flag3 = !target.IsNonNpcPlayer() && baseCombatEntity.IsNonNpcPlayer();
				flag2 = num || flag3;
			}
			if (flag2)
			{
				base.Senses.TrySetTarget(baseCombatEntity);
			}
		}
		return base.OnStateEnter(payload);
	}
}

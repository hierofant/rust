using System;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public class State_Flee : FSMStateBase
{
	[SerializeField]
	public float desiredDistance = 50f;

	[SerializeField]
	public float distance = 20f;

	[SerializeField]
	protected RustNavMeshAgent.Speeds speed = RustNavMeshAgent.Speeds.Sprint;

	[SerializeField]
	private int maxAttempts = 3;

	private int attempts;

	protected float startDistance;

	public override EFSMStateStatus OnStateEnter(FSMPayload payload)
	{
		base.Blackboard.Remove("HitByFire");
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		attempts = 0;
		startDistance = Vector3.Distance(Owner.transform.position, targetPosition);
		return MoveAwayFromTarget();
	}

	public override void OnStateExit()
	{
		base.Agent.ResetPath();
		base.OnStateExit();
	}

	public override EFSMStateStatus OnStateUpdate(float deltaTime)
	{
		if (base.Agent.hasPath)
		{
			return base.OnStateUpdate(deltaTime);
		}
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		if (Vector3.Distance(targetPosition, Owner.transform.position) > desiredDistance + startDistance)
		{
			return EFSMStateStatus.Success;
		}
		attempts++;
		if (attempts >= maxAttempts)
		{
			return EFSMStateStatus.Success;
		}
		return MoveAwayFromTarget();
	}

	protected virtual EFSMStateStatus MoveAwayFromTarget()
	{
		if (!base.Senses.FindTargetPosition(out var targetPosition))
		{
			return EFSMStateStatus.Success;
		}
		NavVector3 nextPosition = base.Agent.nextPosition;
		using PooledList<NavVector3> pooledList = Pool.Get<PooledList<NavVector3>>();
		bool flag = Eqs.SampleNavigablePositions(base.Agent, nextPosition, pooledList, distance, distance, 8);
		using Eqs.PooledScoreList pooledScoreList = Pool.Get<Eqs.PooledScoreList>();
		NavVector3 aNS = (nextPosition - base.Agent.WorldToNavSpace(targetPosition)).NormalizeXZ();
		foreach (NavVector3 item3 in pooledList)
		{
			float item = NavVector3.Dot(aNS, (item3 - nextPosition).NormalizeXZ());
			pooledScoreList.Add((item3, item));
		}
		pooledScoreList.SortByScoreDesc(Owner);
		foreach (var item4 in pooledScoreList)
		{
			NavVector3 item2 = item4.pos;
			NavVector3 navVector = item2;
			if (!flag)
			{
				if (!base.Agent.SamplePosition(item2, out var hitNS, 10f))
				{
					continue;
				}
				navVector = hitNS.position;
			}
			if ((base.Agent.canSwim || !base.Agent.IsInWater(navVector)) && base.Agent.SetDestinationWithParams(navVector, autoBraking: false, speed))
			{
				return EFSMStateStatus.None;
			}
		}
		return EFSMStateStatus.Failure;
	}
}

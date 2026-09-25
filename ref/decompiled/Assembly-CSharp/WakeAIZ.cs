using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class WakeAIZ : EntityComponent<BaseEntity>, IServerComponent
{
	[Header("Base")]
	public float sleepDelaySeconds = 30f;

	public bool isBox;

	public Vector3 size = Vector3.one * 30f;

	public List<AIInformationZone> zones;

	private AIInformationZone aiz;

	private Action sleepAI;

	private bool hadContents;

	private float radius;

	private OBB obb;

	private Vector3 spherePos;

	private float r2;

	private Action tickWakeAIZ;

	private Func<BasePlayer, bool> gridIgnoreFilter;

	private Func<BasePlayer, bool> gridQueryFilter;

	private bool foundEmptyCoarseGrid;

	private float TimeBetweenTicksActiveZone => sleepDelaySeconds * 0.1f;

	private float TimeBetweenTicksInactiveZone => 0.15f;

	public override void InitShared()
	{
		Init();
	}

	public void Init(AIInformationZone zone = null)
	{
		if (zone != null)
		{
			aiz = zone;
		}
		else if (zones == null || zones.Count == 0)
		{
			Transform parent = base.transform.parent;
			if (parent == null)
			{
				parent = base.transform;
			}
			aiz = parent.GetComponentInChildren<AIInformationZone>();
		}
		if (aiz != null && !aiz.wakeZones.Contains(this))
		{
			aiz.wakeZones.Add(this);
		}
		SetZonesSleeping(flag: true);
		base.transform.GetPositionAndRotation(out var position, out var rotation);
		spherePos = position;
		radius = (isBox ? (size.magnitude * 0.5f) : size.x);
		obb = new OBB(position, rotation, new Bounds(Vector3.zero, size));
		r2 = radius * radius;
		BaseEntity.Query.Server.SubscribePlayerChanges(spherePos, radius, Dirty);
		if (tickWakeAIZ == null)
		{
			tickWakeAIZ = TickWakeAIZ;
		}
		if (gridIgnoreFilter == null)
		{
			gridIgnoreFilter = FilterIgnorenNPC;
		}
		if (gridQueryFilter == null)
		{
			gridQueryFilter = FilterNonNPCInTrigger;
		}
		SetTickRate(isFast: true);
	}

	private void Dirty()
	{
		if (!IsInvoking(tickWakeAIZ))
		{
			SetTickRate(isFast: true);
		}
	}

	public PooledList<BasePlayer> GetPooledListOfPlayers()
	{
		PooledList<BasePlayer> pooledList = Pool.Get<PooledList<BasePlayer>>();
		BaseEntity.Query.Server.GetPlayersInSphere(base.transform.position, radius, pooledList);
		base.transform.GetPositionAndRotation(out var position, out var rotation);
		OBB oBB = new OBB(position, rotation, new Bounds(Vector3.zero, size));
		foreach (BasePlayer item in pooledList)
		{
			if ((bool)item && (!isBox || oBB.Contains(item.TriggerPoint())))
			{
				pooledList.Add(item);
			}
		}
		return pooledList;
	}

	private bool BoxCheck(BasePlayer p)
	{
		if (!obb.Contains(p.TriggerPoint()))
		{
			return false;
		}
		return true;
	}

	private bool SphereCheck(BasePlayer p)
	{
		if (Vector3.SqrMagnitude(p.TriggerPoint() - spherePos) > r2)
		{
			return false;
		}
		return true;
	}

	private bool FilterIgnorenNPC(BasePlayer p)
	{
		return p.IsNpc;
	}

	private bool FilterNonNPCInTrigger(BasePlayer p)
	{
		if (!isBox)
		{
			return SphereCheck(p);
		}
		return BoxCheck(p);
	}

	private void TickWakeAIZ()
	{
		bool flag = BaseEntity.Query.Server.AnyPlayersInSphereFast(spherePos, radius, out foundEmptyCoarseGrid, gridIgnoreFilter, gridQueryFilter);
		if (!hadContents && flag)
		{
			if (sleepAI == null)
			{
				sleepAI = SleepAI;
			}
			CancelInvoke(sleepAI);
			SetZonesSleeping(flag: false);
			SetTickRate(isFast: false);
		}
		if (hadContents && !flag)
		{
			DelayedSleepAI();
			SetTickRate(isFast: true);
		}
		hadContents = flag;
		if (foundEmptyCoarseGrid && IsInvoking(tickWakeAIZ))
		{
			CancelInvoke(tickWakeAIZ);
		}
	}

	private void SetTickRate(bool isFast)
	{
		if (isFast)
		{
			if (IsInvoking(tickWakeAIZ))
			{
				CancelInvoke(tickWakeAIZ);
			}
			InvokeRandomized(tickWakeAIZ, TimeBetweenTicksInactiveZone, TimeBetweenTicksInactiveZone, TimeBetweenTicksInactiveZone * 0.25f);
		}
		else
		{
			if (IsInvoking(tickWakeAIZ))
			{
				CancelInvoke(tickWakeAIZ);
			}
			InvokeRepeating(tickWakeAIZ, TimeBetweenTicksActiveZone, TimeBetweenTicksActiveZone);
		}
	}

	private void SetZonesSleeping(bool flag)
	{
		if (aiz != null)
		{
			if (flag)
			{
				aiz.SleepAI();
			}
			else
			{
				aiz.WakeAI();
			}
		}
		if (zones == null || zones.Count <= 0)
		{
			return;
		}
		foreach (AIInformationZone zone in zones)
		{
			if (zone != null)
			{
				if (flag)
				{
					zone.SleepAI();
				}
				else
				{
					zone.WakeAI();
				}
			}
		}
	}

	private void DelayedSleepAI()
	{
		if (sleepAI == null)
		{
			sleepAI = SleepAI;
		}
		CancelInvoke(sleepAI);
		Invoke(sleepAI, sleepDelaySeconds);
	}

	private void SleepAI()
	{
		SetZonesSleeping(flag: true);
	}
}

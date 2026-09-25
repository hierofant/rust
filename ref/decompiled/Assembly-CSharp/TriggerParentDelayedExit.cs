using System;

public class TriggerParentDelayedExit : TriggerParent
{
	private struct TrackedEntityData
	{
		public TimeSince SinceTriggerPhysicsExit;
	}

	[ServerVar(Help = "Makes TriggerParentDelayedExit act as a TriggerParent again")]
	public static bool disable_delayed_exit;

	private ListDictionary<BaseEntity, TrackedEntityData> entityContentsWaitingForLeave;

	private Action UpdateTick;

	internal override void OnEntityEnter(BaseEntity ent)
	{
		ListDictionary<BaseEntity, TrackedEntityData> listDictionary = entityContentsWaitingForLeave;
		if (listDictionary != null && listDictionary.Contains(ent))
		{
			entityContentsWaitingForLeave.Remove(ent);
		}
		base.OnEntityEnter(ent);
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		if (disable_delayed_exit)
		{
			base.OnEntityLeave(ent);
			return;
		}
		if (ent.IsForceUpdatingTriggers)
		{
			base.OnEntityLeave(ent);
			return;
		}
		if (ent is BasePlayer basePlayer && (basePlayer.IsDead() || basePlayer.IsSleeping() || PlayerCanReparent(basePlayer)))
		{
			base.OnEntityLeave(ent);
			return;
		}
		if (entityContentsWaitingForLeave == null)
		{
			entityContentsWaitingForLeave = new ListDictionary<BaseEntity, TrackedEntityData>();
		}
		if (!entityContentsWaitingForLeave.Contains(ent))
		{
			entityContentsWaitingForLeave.Add(ent, new TrackedEntityData
			{
				SinceTriggerPhysicsExit = 0f
			});
		}
		else
		{
			entityContentsWaitingForLeave[ent] = new TrackedEntityData
			{
				SinceTriggerPhysicsExit = 0f
			};
		}
		if (UpdateTick == null)
		{
			UpdateTick = TickLeaveContents;
		}
		if (!IsInvoking(UpdateTick))
		{
			InvokeRepeating(UpdateTick, 0f, 0f);
		}
	}

	internal override void OnEmpty()
	{
		base.OnEmpty();
		if (!base.IsBeingDisabled || entityContentsWaitingForLeave == null)
		{
			return;
		}
		foreach (var (ent, _) in entityContentsWaitingForLeave)
		{
			base.OnEntityLeave(ent);
		}
		entityContentsWaitingForLeave = null;
		CancelInvoke(UpdateTick);
	}

	private bool PlayerCanReparent(BasePlayer p)
	{
		TriggerParent triggerParent = p.FindSuitableParent();
		if ((bool)triggerParent)
		{
			return triggerParent != this;
		}
		return false;
	}

	private void TickLeaveContents()
	{
		BufferList<BaseEntity> keys = entityContentsWaitingForLeave.Keys;
		BufferList<TrackedEntityData> values = entityContentsWaitingForLeave.Values;
		for (int j = 0; j < entityContentsWaitingForLeave.Count; j++)
		{
			BaseEntity baseEntity = keys[j];
			if (baseEntity == null)
			{
				entityContentsWaitingForLeave.RemoveAt(j);
				j--;
				continue;
			}
			if (disable_delayed_exit)
			{
				RemoveTickedEnt(baseEntity, ref j);
				continue;
			}
			TrackedEntityData value = values[j];
			if (baseEntity is BasePlayer basePlayer && (basePlayer.IsSleeping() || PlayerCanReparent(basePlayer)))
			{
				RemoveTickedEnt(baseEntity, ref j);
				if (basePlayer.IsSleeping())
				{
					Unparent(basePlayer);
				}
			}
			else if (!ShouldParent(baseEntity))
			{
				RemoveTickedEnt(baseEntity, ref j);
			}
			else if (!doClippingCheck && IsClipping(baseEntity) && !(baseEntity is BaseCorpse))
			{
				RemoveTickedEnt(baseEntity, ref j);
			}
			else if ((float)value.SinceTriggerPhysicsExit > 1f)
			{
				RemoveTickedEnt(baseEntity, ref j);
			}
			else
			{
				values[j] = value;
			}
		}
		if (entityContentsWaitingForLeave.Count == 0)
		{
			entityContentsWaitingForLeave = null;
			CancelInvoke(UpdateTick);
		}
		void RemoveTickedEnt(BaseEntity ent, ref int i)
		{
			base.OnEntityLeave(ent);
			entityContentsWaitingForLeave.RemoveAt(i);
			i--;
		}
	}
}

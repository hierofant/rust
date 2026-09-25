using Facepunch;
using UnityEngine;

public class LaserDetector : BaseDetector
{
	public const Flags Flag_Triggered = Flags.Reserved12;

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!HasFlag(Flags.Reserved12))
		{
			return 0;
		}
		return currentEnergy;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (IsPowered() && (next & Flags.Reserved1) == Flags.Reserved1)
		{
			InvokeRepeating(VisibilityCheck, 0f, 1f);
		}
		else
		{
			CancelInvoke(VisibilityCheck);
		}
	}

	private void VisibilityCheck()
	{
		if (myTrigger.entityContents == null)
		{
			return;
		}
		bool b = false;
		foreach (BaseEntity entityContent in myTrigger.entityContents)
		{
			if (!entityContent.isClient && CanSee(entityContent))
			{
				b = true;
				break;
			}
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b);
		}
		MarkDirty();
	}

	public override void OnEmpty()
	{
		base.OnEmpty();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
		}
		MarkDirty();
	}

	public bool CanSee(BaseEntity ent)
	{
		using PooledList<RaycastHit> pooledList = Pool.Get<PooledList<RaycastHit>>();
		Vector3 vector = base.transform.position + base.transform.forward * 0.25f;
		GamePhysics.TraceAll(new Ray(vector, base.transform.forward), 0.12f, pooledList, 12f, 0x48A12101 | (int)myTrigger.InterestLayers, QueryTriggerInteraction.Ignore, this);
		foreach (RaycastHit item in pooledList)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if (!(entity == null) && !entity.isClient)
			{
				return entity == ent;
			}
		}
		if (!(ent is BaseVehicle))
		{
			return false;
		}
		Vector3 worldVelocity = ent.GetWorldVelocity();
		if (worldVelocity.magnitude > 5f)
		{
			Vector3 normalized = (ent.transform.position + worldVelocity * Time.fixedDeltaTime - vector).normalized;
			pooledList.Clear();
			GamePhysics.TraceAll(new Ray(vector, normalized), 0.25f, pooledList, 20f, 0x48A12101 | (int)myTrigger.InterestLayers, QueryTriggerInteraction.Ignore, this);
			foreach (RaycastHit item2 in pooledList)
			{
				BaseEntity entity2 = RaycastHitEx.GetEntity(item2);
				if (!(entity2 == null) && !entity2.isClient && entity2 == ent)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		base.UpdateHasPower(inputAmount, inputSlot);
		if (inputAmount == 0)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved12, b: false);
			}
		}
	}
}

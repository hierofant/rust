using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Rust;
using Spatial;
using UnityEngine;

public class CeilingLight : IOEntity
{
	public float pushScale = 2f;

	public Rigidbody MovingJoint;

	public bool UseLowerSleepLimits;

	public TransformLineRenderer[] lines;

	[Space]
	public bool shouldAffectGrowables = true;

	public int consumptionAmount = 2;

	public const Flags RecentlyHit = Flags.Reserved3;

	private Action resetHitAction;

	public static Grid<CeilingLight> FarmLightGrid = new Grid<CeilingLight>();

	public override bool ValidateMeleeColliderAntihack => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CeilingLight.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return consumptionAmount;
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer)
		{
			float num = 3f * (info.damageTypes.Total() / 50f);
			NetworkableId arg = ((info.Initiator != null && info.Initiator is BasePlayer && !info.IsPredicting) ? info.Initiator.net.ID : default(NetworkableId));
			ClientRPC(RpcTarget.NetworkGroup("ClientPhysPush"), arg, info.attackNormal * num, info.HitPositionWorld);
			MarkRecentlyHit();
		}
		base.OnAttacked(info);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		MarkRecentlyHit();
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		bool num = IsOn();
		bool flag = IsPowered();
		if (num != flag)
		{
			SetFlagLocal(Flags.On, flag);
			SendNetworkUpdate_Flags();
			if (flag)
			{
				LightsOn();
			}
			else
			{
				LightsOff();
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (base.isServer)
		{
			if (info.damageTypes.Has(DamageType.Explosion))
			{
				ClientRPC(RpcTarget.NetworkGroup("ClientPhysPush"), default(NetworkableId), info.attackNormal * 3f * (info.damageTypes.Total() / 50f), info.HitPositionWorld);
				MarkRecentlyHit();
			}
			base.Hurt(info);
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		RefreshGrowables();
	}

	private void RefreshGrowables()
	{
		List<GrowableEntity> obj = Facepunch.Pool.Get<List<GrowableEntity>>();
		Vis.Entities(base.transform.position + new Vector3(0f, 0f - ConVar.Server.ceilingLightHeightOffset, 0f), ConVar.Server.ceilingLightGrowableRange, obj, 524288);
		List<PlanterBox> obj2 = Facepunch.Pool.Get<List<PlanterBox>>();
		foreach (GrowableEntity item in obj)
		{
			if (item.isServer)
			{
				PlanterBox planter = item.GetPlanter();
				if (planter != null && !obj2.Contains(planter))
				{
					obj2.Add(planter);
					planter.ForceLightUpdate();
				}
				item.CalculateQualities(firstTime: false, forceArtificialLightUpdates: true);
				item.SendNetworkUpdate();
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void LightsOn()
	{
		if (shouldAffectGrowables)
		{
			RefreshGrowables();
		}
	}

	private void LightsOff()
	{
		if (shouldAffectGrowables)
		{
			RefreshGrowables();
		}
	}

	private void MarkRecentlyHit()
	{
		if (resetHitAction == null)
		{
			resetHitAction = ResetRecentlyHit;
		}
		CancelInvoke(resetHitAction);
		Invoke(resetHitAction, 15f);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: true);
	}

	private void ResetRecentlyHit()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Vector3 position = base.transform.position;
		FarmLightGrid.Add(this, position.x, position.z);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		FarmLightGrid.Remove(this);
	}
}

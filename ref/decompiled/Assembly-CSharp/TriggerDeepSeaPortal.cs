using System;
using System.Collections.Generic;
using Facepunch;
using LeTai;
using Oxide.Core;
using UnityEngine;

public class TriggerDeepSeaPortal : TriggerBase
{
	public DeepSeaPortal Portal;

	[Tooltip("Set to false if you only want this trigger to show toasts warning players why they can't enter the deep sea")]
	public bool WillTeleport = true;

	public bool ShowToasts = true;

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (ent.isClient || Portal.isClient)
		{
			return;
		}
		DeepSeaManager serverInstance = PointEntity<DeepSeaManager>.ServerInstance;
		if (serverInstance == null || !serverInstance.IsOpen() || (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance && !Portal.HasFlag(BaseEntity.Flags.Open)))
		{
			return;
		}
		if (ent is BoatBuildingBlock boatBuildingBlock && boatBuildingBlock.GetParentEntity() != null)
		{
			ent = boatBuildingBlock.GetParentEntity();
		}
		var (flag, phrase) = CanEntityTeleport(ent);
		if (!flag)
		{
			if (!ShowToasts || phrase == null)
			{
				return;
			}
			if (ent is BasePlayer basePlayer)
			{
				basePlayer.ShowToast(GameTip.Styles.Blue_Long, phrase, false);
			}
			else
			{
				if (!(ent is BaseVehicle entity))
				{
					return;
				}
				List<BasePlayer> obj = Pool.Get<List<BasePlayer>>();
				BaseVehicle.GetPassengersForVehicle(entity, obj);
				foreach (BasePlayer item in obj)
				{
					item.ShowToast(GameTip.Styles.Blue_Long, phrase, false);
				}
				Pool.FreeUnmanaged(ref obj);
			}
		}
		else
		{
			if (!WillTeleport || Interface.CallHook("OnDeepSeaTeleport", this, ent) != null)
			{
				return;
			}
			if (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance)
			{
				Portal.NextFrames(delegate
				{
					PointEntity<DeepSeaManager>.ServerInstance.MoveToDeepSea(ent);
				});
			}
			else
			{
				Portal.NextFrames(delegate
				{
					PointEntity<DeepSeaManager>.ServerInstance.MoveToMainIsland(ent);
				});
			}
		}
	}

	private (bool, Translate.Phrase) CanEntityTeleport(BaseEntity entity)
	{
		object obj = Interface.CallHook("CanTeleportDeepSea", entity, Portal);
		if (obj is ValueTuple<bool, Translate.Phrase>)
		{
			return ((bool, Translate.Phrase))obj;
		}
		if (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance)
		{
			return DeepSeaManager.CanTeleportToDeepSea(entity);
		}
		if (Portal.PortalMode == DeepSeaPortal.PortalModeEnum.Exit)
		{
			return DeepSeaManager.CanTeleportToMainIsland(entity);
		}
		return (false, null);
	}
}

#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SimpleBuildingBlock : StabilityEntity, ISimpleUpgradable, IReskinCallback
{
	public List<ItemDefinition> UpgradeItems;

	public Menu.Option UpgradeMenu;

	private GameObject currentModel;

	private SimpleBuildingBlockModelVariant[] variants;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SimpleBuildingBlock.OnRpcMessage"))
		{
			if (rpc == 2824056853u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - DoSimpleUpgrade");
				}
				using (TimeWarning.New("DoSimpleUpgrade"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2824056853u, "DoSimpleUpgrade", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2824056853u, "DoSimpleUpgrade", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							DoSimpleUpgrade(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DoSimpleUpgrade");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void InitShared()
	{
		base.InitShared();
		variants = PrefabAttribute.server.FindAll<SimpleBuildingBlockModelVariant>(prefabID);
	}

	public List<ItemDefinition> GetUpgradeItems()
	{
		return UpgradeItems;
	}

	public bool CanUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		return global::SimpleUpgrade.CanUpgrade(this, upgradeItem, player);
	}

	public void DoUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		global::SimpleUpgrade.DoUpgrade(this, player, upgradeItem);
	}

	public Menu.Option GetUpgradeMenuOption()
	{
		return UpgradeMenu;
	}

	public bool UpgradingEnabled()
	{
		if (UpgradeItems != null)
		{
			return UpgradeItems.Count > 0;
		}
		return false;
	}

	public bool CostIsItem()
	{
		return true;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public void DoSimpleUpgrade(RPCMessage msg)
	{
		if (base.SecondsSinceAttacked < 30f)
		{
			msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.CantUpgradeRecentlyDamaged, false, (30f - base.SecondsSinceAttacked).ToString("N0"));
			return;
		}
		int num = msg.read.Int32();
		if (num >= 0 && num < UpgradeItems.Count)
		{
			ItemDefinition upgradeItem = UpgradeItems[num];
			if (CanUpgrade(msg.player, upgradeItem))
			{
				DoUpgrade(msg.player, upgradeItem);
			}
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		PopulateVariants();
	}

	private void PopulateVariants()
	{
		if (base.isServer && variants.Any())
		{
			ulong x = net.ID.Value;
			SeedRandom.Wanghash(ref x);
			SeedRandom.Wanghash(ref x);
			SeedRandom.Wanghash(ref x);
			int num = (int)(x % (ulong)variants.Length);
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(variants[num].Flag, b: true);
		}
	}

	public void OnReskinned(BasePlayer byPlayer)
	{
		PopulateVariants();
	}

	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		Vis.Entities(WorldSpaceBounds(), pooledList, -2145386240);
		foreach (BaseEntity item in pooledList)
		{
			if (!(item == null) && !item.isClient && !(item == this) && !(item is BuildingBlock) && !(item is SimpleBuildingBlock) && !(item is Door) && !(item is BaseOven) && !(item is Barricade))
			{
				if (!string.IsNullOrEmpty(ConstructionErrors.GetTranslatedNameFromEntity(item)))
				{
					SprayCan.LastReskinError = ConstructionErrors.BlockedBy;
					SprayCan.LastReskinErrorEntity = item;
				}
				else
				{
					SprayCan.LastReskinError = SprayCan.BlockedBySomething;
				}
				return false;
			}
		}
		return base.CanBeRedirectSwapped(player);
	}

	public void SetVariant(int index)
	{
		int num = index % variants.Length;
		SimpleBuildingBlockModelVariant[] array = variants;
		foreach (SimpleBuildingBlockModelVariant simpleBuildingBlockModelVariant in array)
		{
			SetFlagLocal(simpleBuildingBlockModelVariant.Flag, b: false);
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(variants[num].Flag, b: true);
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if (!base.isServer)
		{
			return;
		}
		SimpleBuildingBlockModelVariant[] array = variants;
		foreach (SimpleBuildingBlockModelVariant simpleBuildingBlockModelVariant in array)
		{
			if (HasFlag(simpleBuildingBlockModelVariant.Flag))
			{
				using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(simpleBuildingBlockModelVariant.Flag, b: false);
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (variants != null)
		{
			RefreshVariant();
		}
	}

	private void RefreshVariant()
	{
		if (variants == null)
		{
			return;
		}
		base.gameManager.Retire(currentModel);
		SimpleBuildingBlockModelVariant[] array = variants;
		foreach (SimpleBuildingBlockModelVariant simpleBuildingBlockModelVariant in array)
		{
			if (HasFlag(simpleBuildingBlockModelVariant.Flag))
			{
				GameObject gameObject = base.gameManager.CreatePrefab(simpleBuildingBlockModelVariant.prefab.resourcePath, base.transform);
				if ((bool)gameObject)
				{
					gameObject.transform.localPosition = simpleBuildingBlockModelVariant.localPosition;
					gameObject.transform.localRotation = simpleBuildingBlockModelVariant.localRotation;
				}
				currentModel = gameObject;
			}
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		base.gameManager.Retire(currentModel);
		currentModel = null;
	}
}

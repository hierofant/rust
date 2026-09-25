#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class WallpaperPlanner : Planner
{
	[Serializable]
	public class SurfaceTypeSetting
	{
		public GameObject[] thirdPersonModels;

		public AnimatorOverrideController holdAnimationOverride;
	}

	private static int AnimatorHash_mode = Animator.StringToHash("mode");

	private static int AnimatorHash_switch = Animator.StringToHash("switch");

	private ulong wallSkinID;

	private ulong flooringSkinID;

	private ulong ceilingSkinID;

	private int currentMode = -1;

	private static WallpaperSettings _settings;

	public GameObjectRef SkinSelectPanel;

	public SurfaceTypeSetting[] surfaceTypeSettings;

	private WallpaperSettings.Category currentModeCategory => (WallpaperSettings.Category)currentMode;

	public static WallpaperSettings Settings
	{
		get
		{
			if (_settings == null)
			{
				_settings = FileSystem.Load<WallpaperSettings>("Assets/Prefabs/Wallpaper/Wallpaper Settings.asset");
			}
			return _settings;
		}
	}

	public ItemAmount placementPrice => Settings.PlacementPrice;

	public override bool isTypeDeployable => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WallpaperPlanner.OnRpcMessage"))
		{
			if (rpc == 4026651916u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_ChangeWallpaperToolSkin");
				}
				using (TimeWarning.New("SERVER_ChangeWallpaperToolSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4026651916u, "SERVER_ChangeWallpaperToolSkin", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(4026651916u, "SERVER_ChangeWallpaperToolSkin", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(4026651916u, "SERVER_ChangeWallpaperToolSkin", this, player))
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
							SERVER_ChangeWallpaperToolSkin(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_ChangeWallpaperToolSkin");
					}
				}
				return true;
			}
			if (rpc == 236604960 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_SwitchMode");
				}
				using (TimeWarning.New("SERVER_SwitchMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(236604960u, "SERVER_SwitchMode", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(236604960u, "SERVER_SwitchMode", this, player))
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
							RPCMessage msg3 = rPCMessage;
							SERVER_SwitchMode(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SERVER_SwitchMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override Deployable GetDeployable(NetworkableId entityId)
	{
		if (entityId.IsValid)
		{
			BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(entityId) as BaseEntity;
			return Settings.GetDeployable(baseEntity as BuildingBlock);
		}
		return null;
	}

	public ulong GetSkinIDForCategory(WallpaperSettings.Category category)
	{
		return category switch
		{
			WallpaperSettings.Category.Wall => wallSkinID, 
			WallpaperSettings.Category.Floor => flooringSkinID, 
			WallpaperSettings.Category.Ceiling => ceilingSkinID, 
			_ => 0uL, 
		};
	}

	public override BaseEntity DoBuild(Construction.Target target, Construction component)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return null;
		}
		if (target.entity is BuildingBlock buildingBlock)
		{
			int side = ((!target.socket.socketName.EndsWith("1")) ? 1 : 0);
			if (!buildingBlock.CanSeeWallpaperSocket(GetOwnerPlayer(), side))
			{
				return null;
			}
			bool flag = buildingBlock.HasWallpaper(side);
			if (flag && buildingBlock.GetWallpaperSkin(side) == skinID)
			{
				float y = component.rotationAmount.y;
				int num = Mathf.RoundToInt(target.rotation.y / y) * (int)y % 360;
				int num2 = Mathf.RoundToInt(buildingBlock.GetWallpaperRotation(side) / y) * (int)y % 360;
				if (num == num2 || (num + 180) % 360 == num2)
				{
					return null;
				}
			}
			if (!flag)
			{
				if (!HasClothForPlacement(ownerPlayer))
				{
					using (ItemAmountList itemAmountList = Facepunch.Pool.Get<ItemAmountList>())
					{
						itemAmountList.amount = Facepunch.Pool.Get<List<float>>();
						itemAmountList.itemID = Facepunch.Pool.Get<List<int>>();
						GetConstructionCost(itemAmountList, component);
						ownerPlayer.ClientRPC(RpcTarget.Player("Client_OnRepairFailedResources", ownerPlayer), itemAmountList);
					}
					return null;
				}
				PayForPlacement(ownerPlayer, component);
			}
			WallpaperSettings.Category category = Settings.GetCategory(buildingBlock, side);
			ulong skinIDForCategory = GetSkinIDForCategory(category);
			buildingBlock.SetWallpaper(skinIDForCategory, side, target.rotation.y);
			if (component.deployable.placeEffect.isValid)
			{
				Effect.server.Run(component.deployable.placeEffect.resourcePath, buildingBlock.transform.TransformPoint(target.socket.worldPosition), buildingBlock.transform.up);
			}
			Facepunch.Rust.Analytics.Azure.OnWallpaperPlaced(ownerPlayer, buildingBlock, skinID, side, reskin: false);
		}
		return null;
	}

	public override void PayForPlacement(BasePlayer player, Construction component)
	{
		if (Interface.CallHook("OnPayForPlacement", player, this, component) == null && (!player.IsInCreativeMode || !Creative.freeBuild))
		{
			player.inventory.Take(null, placementPrice.itemid, (int)placementPrice.amount);
			player.Command("note.inv", placementPrice.itemid, (int)placementPrice.amount * -1);
		}
	}

	public override bool CanAffordToPlace(Construction component)
	{
		return true;
	}

	private bool HasClothForPlacement(BasePlayer player)
	{
		if (player.IsInCreativeMode && Creative.freeBuild)
		{
			return true;
		}
		return (float)player.inventory.GetAmount(placementPrice.itemid) >= placementPrice.amount;
	}

	protected override void GetConstructionCost(ItemAmountList list, Construction component)
	{
		list.amount.Clear();
		list.itemID.Clear();
		list.itemID.Add(placementPrice.itemid);
		list.amount.Add((int)placementPrice.amount);
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	[RPC_Server]
	private void SERVER_ChangeWallpaperToolSkin(RPCMessage msg)
	{
		int num = msg.read.Int32();
		int num2 = msg.read.Int32();
		BasePlayer player = msg.player;
		if (!(player == null) && !(player != GetOwnerPlayer()) && num2 >= 0 && num2 <= 3 && (num == 0 || player.blueprints.CheckSkinOwnership(num, player)))
		{
			ulong num3 = ItemDefinition.FindSkin(WallpaperSettings.GetItemDefForCategory((WallpaperSettings.Category)num2).itemid, num);
			switch ((WallpaperSettings.Category)num2)
			{
			case WallpaperSettings.Category.Wall:
				wallSkinID = num3;
				break;
			case WallpaperSettings.Category.Floor:
				flooringSkinID = num3;
				break;
			case WallpaperSettings.Category.Ceiling:
				ceilingSkinID = num3;
				break;
			}
			skinID = num3;
			SendNetworkUpdate();
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_ChangeSkin"), skinID, currentMode);
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	public void SERVER_SwitchMode(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || player != GetOwnerPlayer())
		{
			return;
		}
		int num = msg.read.Int32();
		if (currentMode != num && num >= 1 && num <= 3)
		{
			currentMode = num;
			switch (currentModeCategory)
			{
			case WallpaperSettings.Category.Wall:
				skinID = wallSkinID;
				break;
			case WallpaperSettings.Category.Floor:
				skinID = flooringSkinID;
				break;
			case WallpaperSettings.Category.Ceiling:
				skinID = ceilingSkinID;
				break;
			}
			ClientRPC(RpcTarget.NetworkGroup("CLIENT_SwitchMode"), skinID, currentMode);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.wallpaperTool == null)
		{
			info.msg.wallpaperTool = Facepunch.Pool.Get<WallpaperTool>();
			info.msg.wallpaperTool.wallSkinID = wallSkinID;
			info.msg.wallpaperTool.flooringSkinID = flooringSkinID;
			info.msg.wallpaperTool.ceilingSkinID = ceilingSkinID;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.wallpaperTool != null)
		{
			wallSkinID = info.msg.wallpaperTool.wallSkinID;
			flooringSkinID = info.msg.wallpaperTool.flooringSkinID;
			ceilingSkinID = info.msg.wallpaperTool.ceilingSkinID;
		}
	}
}

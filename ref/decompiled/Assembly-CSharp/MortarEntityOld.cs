#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class MortarEntityOld : StorageContainer
{
	[Header("Mortar")]
	public float MinAngle = 5f;

	public float MaxAngle = 90f;

	public float CurrentAngle = 45f;

	public float AnglePerAdjustment = 5f;

	public float CooldownDuration = 4f;

	public Transform ShellSpawnPoint;

	public Transform BarrelTransform;

	public float VelocityOverride = 20f;

	public float GravityOverride = 1f;

	public float DragOverride;

	public static readonly Flags IsIncreasingAngleFlag = Flags.Reserved10;

	public static readonly Flags CooldownFlag = Flags.Busy;

	public static readonly Flags AdjustmentModeFlag = Flags.Reserved2;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MortarEntityOld.OnRpcMessage"))
		{
			if (rpc == 3116244173u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - AdjustAngle");
				}
				using (TimeWarning.New("AdjustAngle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3116244173u, "AdjustAngle", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							AdjustAngle(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AdjustAngle");
					}
				}
				return true;
			}
			if (rpc == 3857190246u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - FireGun");
				}
				using (TimeWarning.New("FireGun"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3857190246u, "FireGun", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							FireGun(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in FireGun");
					}
				}
				return true;
			}
			if (rpc == 1766714930 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SetAdjustmentMode");
				}
				using (TimeWarning.New("SetAdjustmentMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1766714930u, "SetAdjustmentMode", this, player, 3f))
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
							RPCMessage adjustmentMode = rPCMessage;
							SetAdjustmentMode(adjustmentMode);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in SetAdjustmentMode");
					}
				}
				return true;
			}
			if (rpc == 871919740 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SwitchAdjustmentAngle");
				}
				using (TimeWarning.New("SwitchAdjustmentAngle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(871919740u, "SwitchAdjustmentAngle", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							SwitchAdjustmentAngle(rpc4);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in SwitchAdjustmentAngle");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = CanAcceptItem;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, b: false);
		}
		base.inventory.capacity = 1;
	}

	public bool CanAcceptItem(BasePlayer player, Item item, int slot)
	{
		return IsMortarAmmo(item);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void FireGun(RPCMessage rpc)
	{
		if (!CanFireGun())
		{
			return;
		}
		if (rpc.player.HasMortarCooldown())
		{
			Debug.LogWarning($"Player {rpc.player} called FireGun() but still has an active personal mortar cooldown");
			return;
		}
		Item activeItem = rpc.player.GetActiveItem();
		if (activeItem == null)
		{
			Debug.LogWarning($"Player {rpc.player} called FireGun() while their hands were empty!");
			return;
		}
		if (!IsMortarAmmo(activeItem))
		{
			Debug.LogWarning($"Player {rpc.player} called FireGun() while holding non-mortar ammo item {activeItem.info.shortname}!");
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(CooldownFlag, b: true);
		}
		rpc.player.SetMortarCooldown(CooldownDuration);
		Invoke(EndCooldown, CooldownDuration);
		ItemDefinition info = activeItem.info;
		activeItem.UseItem();
		DoShoot(info);
	}

	private void EndCooldown()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(CooldownFlag, b: false);
	}

	private void DoShoot(ItemDefinition ammoDef)
	{
		string resourcePath = ammoDef.GetComponent<ItemModProjectile>().GetOverrideProjectile(this).resourcePath;
		BaseEntity baseEntity = GameManager.server.CreateEntity(resourcePath, ShellSpawnPoint.position, ShellSpawnPoint.rotation);
		ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
		if (component == null)
		{
			Debug.LogError("MortarEntity.DoShoot() Spawned projectile '" + resourcePath + "' has no ServerProjectile component");
			return;
		}
		Vector3 overrideVel = ShellSpawnPoint.forward * VelocityOverride;
		component.gravityModifier = GravityOverride;
		component.drag = DragOverride;
		component.InitializeVelocity(overrideVel);
		baseEntity.Spawn();
		Debug.Log($"Launching mortar with velocity of {Math.Round(overrideVel.magnitude, 1)}m/s with drag of {component.drag} and gravity of {component.gravityModifier}");
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SwitchAdjustmentAngle(RPCMessage rpc)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(IsIncreasingAngleFlag, !HasFlag(IsIncreasingAngleFlag));
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AdjustAngle(RPCMessage rpc)
	{
		bool flag = rpc.read.Bool();
		CurrentAngle = Mathf.Clamp(CurrentAngle + AnglePerAdjustment * (float)(flag ? 1 : (-1)), MinAngle, MaxAngle);
		Debug.Log($"Angle: {CurrentAngle}");
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetAdjustmentMode(RPCMessage rpc)
	{
		bool b = rpc.read.Bool();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(AdjustmentModeFlag, b);
	}

	private void Update()
	{
		if (BarrelTransform != null)
		{
			BarrelTransform.localRotation = Quaternion.Euler(90f - CurrentAngle, 0f, 0f);
		}
	}

	public bool IsHoldingMortarAmmo(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		Item activeItem = player.GetActiveItem();
		if (activeItem != null)
		{
			return IsMortarAmmo(activeItem);
		}
		return false;
	}

	public static bool IsMortarAmmo(Item item)
	{
		if (item == null)
		{
			return false;
		}
		ItemModProjectile component = item.info.GetComponent<ItemModProjectile>();
		if (component != null)
		{
			return (component.ammoType & AmmoTypes.MORTAR) == AmmoTypes.MORTAR;
		}
		return false;
	}

	public bool CanFireGun()
	{
		return !HasFlag(CooldownFlag);
	}

	public bool InAdjustmentMode()
	{
		return HasFlag(AdjustmentModeFlag);
	}

	public bool IsHoldingAdjustmentTool(BasePlayer player)
	{
		Item activeItem = player.GetActiveItem();
		if (activeItem != null)
		{
			return activeItem.info.shortname == "pipetool";
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.mortar = Facepunch.Pool.Get<MortarData>();
		info.msg.mortar.angle = CurrentAngle;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.mortar != null)
		{
			CurrentAngle = info.msg.mortar.angle;
		}
	}

	public bool IsMortarAdjustable(BasePlayer player)
	{
		if (IsHoldingAdjustmentTool(player))
		{
			return InAdjustmentMode();
		}
		return false;
	}
}

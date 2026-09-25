#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SmallEngine : DecayEntity, global::IBoatBuildingPiece, IBoatPropulsion, IEngineControllerUser, IEntity
{
	[ReplicatedVar]
	public static float MaxThrustMultiplier = 1f;

	[Header("Small Engine")]
	[SerializeField]
	private float maxThrust = 1000f;

	[SerializeField]
	private float ReverseSpeedRatio = 0.5f;

	[Header("Fuel")]
	public GameObjectRef fuelStoragePrefab;

	public float fuelPerSec;

	[Header("Visuals")]
	public Animator Animator;

	private IFuelSystem fuelSystem;

	private const Flags Flag_HasFuel = Flags.Reserved4;

	private const Flags Flag_InReverse = Flags.Reserved3;

	public float MaxThrust => maxThrust * MaxThrustMultiplier;

	public bool InReverse => HasFlag(Flags.Reserved3);

	public float ReverseMod => 0f - ReverseSpeedRatio;

	public float CurrentThrust
	{
		get
		{
			if (!IsOn())
			{
				return 0f;
			}
			return MaxThrust;
		}
	}

	float IBoatPropulsion.MaxThrust => MaxThrust;

	public Vector3 ThrustPosition => base.transform.position + base.transform.up * 1f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SmallEngine.OnRpcMessage"))
		{
			if (rpc == 1851540757 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenFuel");
				}
				using (TimeWarning.New("RPC_OpenFuel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1851540757u, "RPC_OpenFuel", this, player, 3f))
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
							RPC_OpenFuel(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenFuel");
					}
				}
				return true;
			}
			if (rpc == 4093422150u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_ToggleReverse");
				}
				using (TimeWarning.New("SV_ToggleReverse"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4093422150u, "SV_ToggleReverse", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4093422150u, "SV_ToggleReverse", this, player, 3f))
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
							SV_ToggleReverse(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SV_ToggleReverse");
					}
				}
				return true;
			}
			if (rpc == 2179891358u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - TurnOff");
				}
				using (TimeWarning.New("TurnOff"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2179891358u, "TurnOff", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2179891358u, "TurnOff", this, player, 3f))
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
							RPCMessage msg4 = rPCMessage;
							TurnOff(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in TurnOff");
					}
				}
				return true;
			}
			if (rpc == 6309714 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - TurnOn");
				}
				using (TimeWarning.New("TurnOn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(6309714u, "TurnOn", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(6309714u, "TurnOn", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							TurnOn(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in TurnOn");
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
		fuelSystem = new EntityFuelSystem(base.isServer, fuelStoragePrefab, children, editorGiveFreeFuel: true, FuelAddedRemovedCallback);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.smallEngine != null)
		{
			fuelSystem.SetInstanceID(info.msg.smallEngine.fuelStorageID);
		}
	}

	public bool AdminFixUp()
	{
		if (IsDead() || fuelSystem == null)
		{
			return false;
		}
		fuelSystem.FillFuel();
		return true;
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		if (fuelSystem.GetFuelAmount() > 0)
		{
			EntityFuelSystem entityFuelSystem = fuelSystem as EntityFuelSystem;
			player.GiveItem(entityFuelSystem.GetFuelItem(), GiveItemReason.PickedUp);
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && isSpawned)
		{
			fuelSystem.CheckNewChild(child);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.smallEngine = Facepunch.Pool.Get<ProtoBuf.SmallEngine>();
		info.msg.smallEngine.fuelStorageID = fuelSystem.GetInstanceID();
	}

	private void FuelAddedRemovedCallback(bool added)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, fuelSystem.HasFuel());
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	public void TurnOn(RPCMessage msg)
	{
		if (Interface.CallHook("OnEngineStart", this, msg.player) == null && fuelSystem.HasFuel() && PlayerBoat.IsPlayerAuthedOnChildEntity(this, msg.player, authedIfNoPrivOrLock: true))
		{
			TurnOn();
		}
	}

	public void TurnOn()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: true);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void TurnOff(RPCMessage msg)
	{
		if (Interface.CallHook("OnEngineStop", this, msg.player) == null && PlayerBoat.IsPlayerAuthedOnChildEntity(this, msg.player, authedIfNoPrivOrLock: true))
		{
			TurnOff();
		}
	}

	public void TurnOff()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void SV_ToggleReverse(RPCMessage msg)
	{
		if (Interface.CallHook("OnEngineReverse", this, msg.player) != null || !PlayerBoat.IsPlayerAuthedOnChildEntity(this, msg.player, authedIfNoPrivOrLock: true) || !PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, !InReverse);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && PlayerBoat.IsPlayerAuthedOnChildEntity(this, msg.player, authedIfNoPrivOrLock: true))
		{
			fuelSystem.LootFuel(player);
		}
	}

	public override void Hurt(HitInfo info)
	{
		PlayerBoat parentPlayerBoat = PlayerBoat.GetParentPlayerBoat(this);
		if (parentPlayerBoat != null && !parentPlayerBoat.IsDestructibleWreck)
		{
			parentPlayerBoat.OnBoatDeployableHurt(this, info);
		}
		else
		{
			base.Hurt(info);
		}
	}

	public bool TryUseFuel()
	{
		if (!IsOn())
		{
			return false;
		}
		if (!fuelSystem.HasFuel())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved4, b: true);
			}
			if (IsOn())
			{
				TurnOff();
			}
		}
		fuelSystem.TryUseFuel(UnityEngine.Time.fixedDeltaTime, fuelPerSec);
		return true;
	}

	public void OnEngineStartFailed()
	{
	}

	public bool MeetsEngineRequirements()
	{
		return true;
	}

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		TurnOff();
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfInteractablePlayerBoat(this);
		}
		return false;
	}

	void IEngineControllerUser.Invoke(Action action, float time)
	{
		Invoke(action, time);
	}

	void IEngineControllerUser.CancelInvoke(Action action)
	{
		CancelInvoke(action);
	}
}

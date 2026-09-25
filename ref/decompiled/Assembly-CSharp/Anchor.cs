#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Anchor : DecayEntity, global::IBoatBuildingPiece
{
	public interface IAnchorable
	{
		void OnAnchoredChanged();
	}

	[Header("Anchor")]
	public float RaiseDuration = 3f;

	public float LowerDuration = 3f;

	public float RopeLength = 1.5f;

	public float AnchorRadius = 0.5f;

	public Transform AnchorTestPoint;

	[Header("Visuals")]
	public Animator Animator;

	public const Flags Flag_Lowered = Flags.Reserved3;

	public const Flags Flag_Lowering = Flags.Reserved12;

	public const Flags Flag_Raising = Flags.Reserved13;

	public const Flags Flag_Anchoring = Flags.On;

	public bool Lowering => HasFlag(Flags.Reserved12);

	public bool Raising => HasFlag(Flags.Reserved13);

	public bool Lowered => HasFlag(Flags.Reserved3);

	public bool Anchoring
	{
		get
		{
			if (Lowered)
			{
				return HasFlag(Flags.On);
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Anchor.OnRpcMessage"))
		{
			if (rpc == 3949891756u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - LowerAnchor");
				}
				using (TimeWarning.New("LowerAnchor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3949891756u, "LowerAnchor", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3949891756u, "LowerAnchor", this, player, 3f))
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
							LowerAnchor(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in LowerAnchor");
					}
				}
				return true;
			}
			if (rpc == 1507568190 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RaiseAnchor");
				}
				using (TimeWarning.New("RaiseAnchor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1507568190u, "RaiseAnchor", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1507568190u, "RaiseAnchor", this, player, 3f))
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
							RaiseAnchor(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RaiseAnchor");
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
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
		flagsUpdateScope.Set(Flags.Reserved13, b: false);
		flagsUpdateScope.Set(Flags.Reserved12, b: false);
		flagsUpdateScope.Set(Flags.Busy, b: false);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk)
		{
			LowerAnchor(null, instant: true);
		}
	}

	public bool CanBeRaised(BasePlayer player)
	{
		if (player != null && !PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		if (!PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (!Lowered)
		{
			return false;
		}
		return true;
	}

	public bool CanBeLowered(BasePlayer player)
	{
		if (player != null && !PlayerBoat.IsPlayerAuthedOnChildEntity(this, player, authedIfNoPrivOrLock: true))
		{
			return false;
		}
		if (!PlayerBoat.IsChildOfInteractablePlayerBoat(this))
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (Lowered)
		{
			return false;
		}
		if (!CanReachWater())
		{
			return false;
		}
		return true;
	}

	public bool CanReachWater()
	{
		if (!GamePhysics.LineOfSightRadius(AnchorTestPoint.position, AnchorTestPoint.position + -base.transform.up * RopeLength, 1218519041, AnchorRadius))
		{
			return false;
		}
		return true;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void LowerAnchor(RPCMessage msg)
	{
		LowerAnchor(msg.player);
	}

	public void LowerAnchor(BasePlayer player, bool instant = false)
	{
		if (CanBeLowered(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved12, b: true);
			}
			WaitForLower(instant);
		}
	}

	private void WaitForLower(bool instant)
	{
		if (instant)
		{
			OnFullyLowered();
			return;
		}
		CancelInvoke(OnFullyLowered);
		Invoke(OnFullyLowered, LowerDuration);
	}

	private void OnFullyLowered()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved12, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		OnRaisedOrLowered();
	}

	private void RefreshAnchoring()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (!Lowered || Lowering || Raising)
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		else
		{
			flagsUpdateScope.Set(Flags.On, b: true);
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RaiseAnchor(RPCMessage msg)
	{
		RaiseAnchor(msg.player);
	}

	public void RaiseAnchor(BasePlayer player)
	{
		if (CanBeRaised(player))
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
				flagsUpdateScope.Set(Flags.Reserved13, b: true);
			}
			WaitForRaise();
		}
	}

	private void WaitForRaise()
	{
		CancelInvoke(OnFullyRaised);
		Invoke(OnFullyRaised, RaiseDuration);
	}

	private void OnFullyRaised()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
			flagsUpdateScope.Set(Flags.Reserved13, b: false);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		OnRaisedOrLowered();
	}

	private void OnRaisedOrLowered()
	{
		RefreshAnchoring();
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (baseEntity.IsValid() && baseEntity is IAnchorable anchorable)
		{
			anchorable.OnAnchoredChanged();
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

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		if (!Rust.Application.isLoadingSave)
		{
			RaiseAnchor(null);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfFinishedPlayerBoat(this);
		}
		return false;
	}
}

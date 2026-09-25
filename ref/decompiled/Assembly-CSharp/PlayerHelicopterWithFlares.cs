#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerHelicopterWithFlares : PlayerHelicopter, ICanFireHelicopterFlares
{
	[Header("Helicopter Flares")]
	[SerializeField]
	private GameObjectRef flareStoragePrefab;

	[SerializeField]
	private Renderer flareLightOff;

	[SerializeField]
	private Renderer flareLightRed;

	[SerializeField]
	private Renderer flareLightGreen;

	public EntityRef<HelicopterFlares> flaresInstance;

	private TimeSince timeSinceFailedFlareRPC;

	public BaseEntity flareEntity => this;

	public HelicopterFlares FlaresInstance => flaresInstance.Get(base.isServer);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerHelicopterWithFlares.OnRpcMessage"))
		{
			if (rpc == 4185921214u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenStorage");
				}
				using (TimeWarning.New("RPC_OpenStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4185921214u, "RPC_OpenStorage", this, player, 6f))
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
							RPC_OpenStorage(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenStorage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child.prefabID == flareStoragePrefab.GetEntity().prefabID)
		{
			HelicopterFlares helicopterFlares = (HelicopterFlares)child;
			flaresInstance.Set(helicopterFlares);
			helicopterFlares.owner = this;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.helicopterFlares != null)
		{
			flaresInstance.uid = info.msg.helicopterFlares.flareStorageID;
		}
	}

	public HelicopterFlares GetFlares()
	{
		HelicopterFlares helicopterFlares = flaresInstance.Get(base.isServer);
		if (helicopterFlares.IsValid())
		{
			return helicopterFlares;
		}
		return null;
	}

	public void FireFlares()
	{
		throw new NotImplementedException();
	}

	public void FlareFireFailed(BasePlayer player)
	{
		if (!((float)timeSinceFailedFlareRPC <= 1f))
		{
			ClientRPC(RpcTarget.Player("ClientFlareFireFailed", player));
			timeSinceFailedFlareRPC = 0f;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.helicopterFlares = Facepunch.Pool.Get<ProtoBuf.HelicopterFlares>();
		info.msg.helicopterFlares.flareStorageID = flaresInstance.uid;
	}

	public override void PilotInput(InputState inputState, BasePlayer player)
	{
		base.PilotInput(inputState, player);
		if (IsOn() && inputState.WasJustPressed(BUTTON.FIRE_SECONDARY) && !GetFlares().TryFireFlare())
		{
			FlareFireFailed(player);
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot && flaresInstance.IsValid(base.isServer))
		{
			flaresInstance.Get(base.isServer).DropItems();
		}
		base.DoServerDestroy();
	}

	[RPC_Server.IsVisible(6f)]
	[RPC_Server]
	public void RPC_OpenStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsOn() || !CanBeLooted(player) || player.isMounted || (IsSafe() && player != creatorEntity))
		{
			return;
		}
		StorageContainer flares = GetFlares();
		if (!(flares == null))
		{
			BasePlayer driver = GetDriver();
			if (!(driver != null) || !(driver != player))
			{
				flares.PlayerOpenLoot(player);
			}
		}
	}

	public override bool AdminFixUp(int tier)
	{
		if (!base.AdminFixUp(tier))
		{
			return false;
		}
		GetFlares()?.RefillFlares();
		return true;
	}
}

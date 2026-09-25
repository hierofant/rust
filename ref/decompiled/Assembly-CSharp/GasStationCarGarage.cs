#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class GasStationCarGarage : ModularCarGarage
{
	[Header("GasStationCarGarage")]
	public Transform liftTransform;

	public Transform loweredPosition;

	public Transform raisedPosition;

	public AnimationCurve movementAnimationCurve;

	public float movementDuration = 5f;

	public VehicleLiftOccupantTrigger bottomCrusherOccupantTrigger;

	public VehicleLiftOccupantTrigger topCrusherOccupantTrigger;

	public SoundDefinition liftStartMovingSound;

	public SoundDefinition liftMovingLoopSound;

	public SoundDefinition liftStopMovingSound;

	private VehicleLiftState LiftHeightState = VehicleLiftState.Up;

	private bool isMoving;

	private TimeSince timeSinceStartMove;

	private bool __sync_isLiftUp;

	protected override bool LiftIsUp => LiftHeightState == VehicleLiftState.Up;

	protected override bool LiftIsMoving => isMoving;

	protected override bool LiftIsDown => LiftHeightState == VehicleLiftState.Down;

	[Sync(Autosave = true)]
	public bool isLiftUp
	{
		[CompilerGenerated]
		get
		{
			return __sync_isLiftUp;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_isLiftUp, value))
			{
				__sync_isLiftUp = value;
				byte nameID = __GetWeaverID("isLiftUp");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("GasStationCarGarage.OnRpcMessage"))
		{
			if (rpc == 401754885 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ToggleLiftHeight");
				}
				using (TimeWarning.New("RPC_ToggleLiftHeight"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(401754885u, "RPC_ToggleLiftHeight", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(401754885u, "RPC_ToggleLiftHeight", this, player, 3f))
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
							RPC_ToggleLiftHeight(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_ToggleLiftHeight");
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
		isLiftUp = true;
		LiftHeightState = VehicleLiftState.Up;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_ToggleLiftHeight(RPCMessage msg)
	{
		bool flag = msg.read.Bool();
		isLiftUp = flag;
		LiftHeightState = (isLiftUp ? VehicleLiftState.Up : VehicleLiftState.Down);
		StartLiftMovement();
	}

	public void StartLiftMovement()
	{
		if (!isMoving)
		{
			timeSinceStartMove = 0f;
			isMoving = true;
			if (base.isServer)
			{
				InvokeRepeatingFixedTime(ProcessLiftMovement);
			}
			else if (base.isClient)
			{
				InvokeRepeating(ProcessLiftMovement, 0f, 0f);
			}
		}
	}

	public void ProcessLiftMovement()
	{
		float num = ((movementDuration > 0f) ? Mathf.Clamp01((float)timeSinceStartMove / movementDuration) : 1f);
		float t = movementAnimationCurve.Evaluate(num);
		Vector3 a = ((LiftHeightState == VehicleLiftState.Up) ? loweredPosition.position : raisedPosition.position);
		Vector3 vector = ((LiftHeightState == VehicleLiftState.Up) ? raisedPosition.position : loweredPosition.position);
		liftTransform.position = Vector3.LerpUnclamped(a, vector, t);
		if (num >= 0.4f)
		{
			if (num <= 0.6f)
			{
				if (bottomCrusherOccupantTrigger.carOccupant != null)
				{
					bottomCrusherOccupantTrigger.carOccupant.Kill(DestroyMode.Gib);
				}
				else if (bottomCrusherOccupantTrigger.vehicleOccupant != null)
				{
					bottomCrusherOccupantTrigger.vehicleOccupant.Die();
				}
			}
			if (topCrusherOccupantTrigger.carOccupant != null)
			{
				topCrusherOccupantTrigger.carOccupant.Kill(DestroyMode.Gib);
			}
			else if (topCrusherOccupantTrigger.vehicleOccupant != null)
			{
				topCrusherOccupantTrigger.vehicleOccupant.Die();
			}
		}
		if (num >= 1f)
		{
			liftTransform.position = vector;
			isMoving = false;
			base.GetVehicleLiftPos.hasChanged = true;
			if (base.isServer)
			{
				CancelInvokeFixedTime(ProcessLiftMovement);
			}
			if (base.isClient)
			{
				CancelInvoke(ProcessLiftMovement);
			}
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: isLiftUp for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_isLiftUp);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_isLiftUp;
				bool _sync_isLiftUp = reader.Bool();
				__sync_isLiftUp = _sync_isLiftUp;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "isLiftUp")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite obj = Network.Net.sv.StartWrite();
		WriteAutoSaveSyncVars(obj);
		var (src, num) = obj.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Facepunch.Pool.Free(ref obj);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead obj = Facepunch.Pool.Get<NetRead>();
			obj.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(obj);
			Facepunch.Pool.Free(ref obj);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_isLiftUp = false;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}

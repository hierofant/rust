#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch.Rust;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class ItemRestrictedWheelSwitch : ItemBasedFlowRestrictor
{
	[Header("Item Restricted Wheel Switch (IRWS)")]
	public float rotateSpeed = 0.1f;

	public float powerOutput = 1f;

	[Header("Max Power output. Set to -1 to Disable")]
	public float maxPowerOutputThreshold = -1f;

	public float maxPowerDecayPerSecond = 0.5f;

	[Header("IRWS Animation Options")]
	public Animator animator;

	public List<Transform> rotateObjects;

	public ClientPressureGauge pressureGauge;

	[Header("Rotation Sounds")]
	public GameObject soundEmitterObject;

	public SoundDefinition startRotateSound;

	public SoundDefinition rotateLoopSound;

	public SoundDefinition stopRotateSound;

	public const Flags Flag_BeingRotated = Flags.Reserved10;

	private float progressTickRate = 0.1f;

	private BasePlayer rotatorPlayer;

	private float __sync_rotateProgress;

	private float __sync_outputtedPower;

	public bool IsBeingRotated => HasFlag(Flags.Reserved10);

	[Sync]
	public float rotateProgress
	{
		[CompilerGenerated]
		get
		{
			return __sync_rotateProgress;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_rotateProgress, value))
			{
				__sync_rotateProgress = value;
				byte nameID = __GetWeaverID("rotateProgress");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync]
	public float outputtedPower
	{
		[CompilerGenerated]
		get
		{
			return __sync_outputtedPower;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_outputtedPower, value))
			{
				__sync_outputtedPower = value;
				byte nameID = __GetWeaverID("outputtedPower");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ItemRestrictedWheelSwitch.OnRpcMessage"))
		{
			if (rpc == 2223603322u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - BeginRotate");
				}
				using (TimeWarning.New("BeginRotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2223603322u, "BeginRotate", this, player, 3f))
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
							BeginRotate(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in BeginRotate");
					}
				}
				return true;
			}
			if (rpc == 434251040 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - CancelRotate");
				}
				using (TimeWarning.New("CancelRotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(434251040u, "CancelRotate", this, player, 3f))
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
							CancelRotate(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in CancelRotate");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		if (!IsBeingRotated)
		{
			return 0;
		}
		return Mathf.CeilToInt(powerOutput);
	}

	protected override bool ShouldDestroyOrDecayItem()
	{
		return IsBeingRotated;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		CancelPlayerRotation();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void BeginRotate(RPCMessage msg)
	{
		if (!IsBeingRotated && IsPowered())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved10, b: true);
			}
			rotatorPlayer = msg.player;
			InvokeRepeating(RotateProgress, 0f, progressTickRate);
			CancelInvoke(DecayMaxPowerOutput);
			SendNetworkUpdate();
		}
	}

	public void CancelPlayerRotation()
	{
		CancelInvoke(RotateProgress);
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			if (iOSlot.connectedTo.Get() != null)
			{
				iOSlot.connectedTo.Get().IOInput(this, ioType, 0f, iOSlot.connectedToSlot);
			}
		}
		rotatorPlayer = null;
		if (maxPowerOutputThreshold > 0f && outputtedPower > 0f)
		{
			InvokeRepeating(DecayMaxPowerOutput, 10f, 1f);
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope.Set(Flags.Reserved10, b: false);
		}
		SendNetworkUpdate();
	}

	public void RotateProgress()
	{
		if (rotatorPlayer == null || !IsPowered() || rotatorPlayer.IsDead() || rotatorPlayer.IsSleeping() || Vector3Ex.Distance2D(rotatorPlayer.transform.position, base.transform.position) > 2f || !HasFlag(Flags.Reserved1))
		{
			CancelPlayerRotation();
			return;
		}
		float num = powerOutput * progressTickRate;
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			if (iOSlot.connectedTo.Get() != null)
			{
				float num2 = num;
				num = iOSlot.connectedTo.Get().IOInput(this, ioType, num, iOSlot.connectedToSlot);
				if (maxPowerOutputThreshold > 0f)
				{
					outputtedPower += num2 - num;
				}
			}
		}
		if (num == 0f)
		{
			SetRotateProgress(rotateProgress + rotateSpeed);
		}
		if (maxPowerOutputThreshold > 0f && outputtedPower >= maxPowerOutputThreshold)
		{
			CancelPlayerRotation();
			outputtedPower = maxPowerOutputThreshold;
		}
	}

	public void SetRotateProgress(float newValue)
	{
		rotateProgress = newValue;
		SendNetworkUpdate();
	}

	private void DecayMaxPowerOutput()
	{
		outputtedPower -= maxPowerDecayPerSecond;
		if (outputtedPower <= 0f)
		{
			outputtedPower = 0f;
			CancelInvoke(DecayMaxPowerOutput);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void CancelRotate(RPCMessage msg)
	{
		CancelPlayerRotation();
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (!added && rotatorPlayer != null)
		{
			rotatorPlayer.AddClanScore(ClanScoreEventType.EnableWaterTreatmentPlant);
			Analytics.Azure.OnWaterTreatmentPlantEnabled(rotatorPlayer, item);
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: rotateProgress for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_rotateProgress);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: outputtedPower for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_outputtedPower);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_rotateProgress;
				float _sync_rotateProgress = reader.Float();
				__sync_rotateProgress = _sync_rotateProgress;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_outputtedPower;
				float _sync_outputtedPower = reader.Float();
				__sync_outputtedPower = _sync_outputtedPower;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (!(propertyName == "rotateProgress"))
		{
			if (propertyName == "outputtedPower")
			{
				return 1;
			}
			return byte.MaxValue;
		}
		return 0;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_rotateProgress = 0f;
		__sync_outputtedPower = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}

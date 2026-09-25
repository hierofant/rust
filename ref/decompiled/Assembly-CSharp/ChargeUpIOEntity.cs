using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class ChargeUpIOEntity : IOEntity
{
	[Tooltip("This amount will be added to the charge meter every second.")]
	[Header("ChargeUp IO")]
	public int powerConsumptionWhileCharging = 1;

	public float chargeRequiredToBecomeActive = 10f;

	[Tooltip("If unpowered and inactive, how much charge is lost per second")]
	public float chargeDecayWhileInactive = 1f;

	[Tooltip("How long does the entity stay active after reaching the required charge, even if it becomes unpowered.")]
	public float activeTimerDuration = 10f;

	[Tooltip("How fast does the charge tick rate run, default is 1s")]
	public float chargingTickDuration = 1f;

	[Tooltip("How long to prevent recharging after activation.")]
	public float cooldownAfterActivation = 10f;

	public bool RequireInputToTrigger;

	[Tooltip("If this entity has no sound, disable this to save on resources")]
	[Header("Charging Sounds")]
	public bool doSounds = true;

	public SoundDefinition startChargingSound;

	[Tooltip("This sound will play when charging stops WITHOUT going active. When charging has been halted prematurely.")]
	public SoundDefinition interruptChargingSound;

	public SoundDefinition quarterChargeSound;

	public SoundDefinition halfChargeSound;

	public SoundDefinition threeQuarterChargeSound;

	public SoundDefinition activatedSound;

	public SoundDefinition activeLoopSound;

	public SoundDefinition deactivatedSound;

	[Header("Power Sounds")]
	public SoundDefinition powerOnSound;

	public SoundDefinition powerOnLoopSound;

	public SoundDefinition powerOffSound;

	public float chargingSoundDelay = 0.5f;

	public const Flags Flag_Charging = Flags.Reserved10;

	public const Flags Flag_QuarterCharge = Flags.Reserved16;

	public const Flags Flag_HalfCharge = Flags.Reserved17;

	public const Flags Flag_ThreeQuarterCharge = Flags.Reserved18;

	public const Flags Flag_OnCooldown = Flags.Reserved19;

	[NonSerialized]
	protected bool isActivated;

	protected TimeSince activeTimer;

	protected TimeSince cooldownTimer;

	private float __sync_Charge;

	[Sync(Autosave = true)]
	public float Charge
	{
		[CompilerGenerated]
		get
		{
			return __sync_Charge;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Charge, value))
			{
				__sync_Charge = value;
				byte nameID = __GetWeaverID("Charge");
				QueueSyncVar(nameID);
			}
		}
	}

	public override int ConsumptionAmount()
	{
		if (!IsOn())
		{
			return powerConsumptionWhileCharging;
		}
		return 0;
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		isActivated = false;
		Charge = 0f;
		cooldownTimer = 9999f;
		UpdateChargeFlags();
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (inputSlot == 1)
		{
			if (RequireInputToTrigger && inputAmount > 0 && Charge >= chargeRequiredToBecomeActive)
			{
				Activate();
				UpdateChargeFlags();
			}
			return;
		}
		bool flag = IsOn();
		bool flag2 = IsPowered();
		if (!flag && flag2)
		{
			CancelInvoke(InactiveDecayCharge);
			InvokeRepeating(InactiveChargeUp, 0f, chargingTickDuration);
		}
		else if (!flag && !flag2)
		{
			CancelInvoke(InactiveChargeUp);
			if (chargeDecayWhileInactive > 0f)
			{
				InvokeRepeating(InactiveDecayCharge, 0f, chargingTickDuration);
			}
		}
		UpdateChargeFlags();
	}

	public virtual void InactiveChargeUp()
	{
		if (!((float)cooldownTimer < cooldownAfterActivation))
		{
			AddCharge(1f);
		}
	}

	public virtual void AddCharge(float amount)
	{
		Charge += amount;
		Charge = Mathf.Clamp(Charge, 0f, chargeRequiredToBecomeActive);
		if (Charge >= chargeRequiredToBecomeActive && !RequireInputToTrigger)
		{
			Activate();
		}
		UpdateChargeFlags();
	}

	public virtual void InactiveDecayCharge()
	{
		if (Charge > 0f)
		{
			Charge = Mathf.Max(0f, Charge - chargeDecayWhileInactive);
		}
		UpdateChargeFlags();
	}

	public virtual void Activate()
	{
		isActivated = true;
		Charge = chargeRequiredToBecomeActive;
		activeTimer = 0f;
		CancelInvoke(InactiveChargeUp);
		InvokeRepeating(ActivateDecayCharge, 0f, chargingTickDuration);
	}

	public virtual void Deactivate()
	{
		isActivated = false;
		Charge = 0f;
		cooldownTimer = 0f;
		CancelInvoke(ActivateDecayCharge);
		if (IsPowered())
		{
			InvokeRepeating(InactiveChargeUp, 0f, chargingTickDuration);
		}
	}

	public virtual void ActivateDecayCharge()
	{
		float num = (float)activeTimer / activeTimerDuration;
		Charge = Mathf.Lerp(chargeRequiredToBecomeActive, 0f, num);
		if (num >= 1f || Charge <= 0f)
		{
			Deactivate();
		}
		UpdateChargeFlags();
	}

	public virtual void UpdateChargeFlags()
	{
		SetFlagLocal(Flags.On, isActivated);
		SetFlagLocal(Flags.Reserved10, IsPowered() && !IsOn());
		SetFlagLocal(Flags.Reserved19, (float)cooldownTimer < cooldownAfterActivation);
		float num = Charge / chargeRequiredToBecomeActive;
		SetFlagLocal(Flags.Reserved16, (double)num >= 0.25);
		SetFlagLocal(Flags.Reserved17, (double)num >= 0.5);
		SetFlagLocal(Flags.Reserved18, (double)num >= 0.75);
		SendNetworkUpdate_Flags();
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: Charge for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_Charge);
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
				_ = __sync_Charge;
				float _sync_Charge = reader.Float();
				__sync_Charge = _sync_Charge;
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
		if (propertyName == "Charge")
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
		__sync_Charge = 0f;
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

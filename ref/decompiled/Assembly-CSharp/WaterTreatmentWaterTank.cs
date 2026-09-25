using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class WaterTreatmentWaterTank : IOEntity
{
	[Header("Water Treatment Water Tank")]
	public WaterTreatmentFlowRateBroadcast broadcaster;

	public Transform BladesTransform;

	public WaterBody Water;

	[Header("Blade Movement Settings")]
	public AnimationCurve BladeSpinUpCurve;

	public AnimationCurve BladeWindDownCurve;

	public AnimationCurve WaterBlendOverTimeCurve;

	[Header("Pressure Gauges")]
	public List<ClientPressureGauge> pressureGauges;

	[Header("Blade Sounds")]
	public GameObject soundEmitterObject;

	public SoundDefinition startBladeSound;

	public SoundDefinition bladeLoopSound;

	public SoundDefinition stopBladeSound;

	public SoundDefinition waterMovementLoopSound;

	public SoundDefinition spinnerFullyStoppedSound;

	[ReplicatedVar(Saved = true)]
	public static float maximumPressure = 360f;

	[ServerVar(Saved = true)]
	public static float maxFlowRatePerMinute = 1020f;

	[ServerVar(Saved = true)]
	public static float pressureDecayPerMinute = 1f;

	[NonSerialized]
	public float FlowRatePerMinute;

	public TimeSince timeSinceLastPressureIncrease;

	private int latestFlowRate;

	private float __sync_Pressure;

	[Sync(Autosave = true)]
	public float Pressure
	{
		[CompilerGenerated]
		get
		{
			return __sync_Pressure;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_Pressure, value))
			{
				__sync_Pressure = value;
				byte nameID = __GetWeaverID("Pressure");
				QueueSyncVar(nameID);
			}
		}
	}

	public float PressureAsPercentage => Mathf.Clamp01(Pressure / maximumPressure);

	[ServerVar]
	public static void debug_wtp_pressure(ConsoleSystem.Arg arg)
	{
		WaterTreatmentWaterTank[] array = UnityEngine.Object.FindObjectsByType<WaterTreatmentWaterTank>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		string text = "WaterTreatmentPlant WaterTank Pressures:\n";
		WaterTreatmentWaterTank[] array2 = array;
		foreach (WaterTreatmentWaterTank waterTreatmentWaterTank in array2)
		{
			if (waterTreatmentWaterTank != null)
			{
				text += $"Water Tank id:{waterTreatmentWaterTank.GetEntity().net.ID} Current Pressure: {waterTreatmentWaterTank.Pressure}, Is On: {waterTreatmentWaterTank.IsOn()}, Flow Rate per Minute: {waterTreatmentWaterTank.FlowRatePerMinute}\n";
			}
		}
		text += $"ConVar settings: maxPressure: {maximumPressure}, maxFlowRatePerMinute: {maxFlowRatePerMinute}, pressureDecayPerMinute: {pressureDecayPerMinute}\n";
		arg.ReplyWith(text);
	}

	[ServerVar]
	public static void force_pressure(ConsoleSystem.Arg arg)
	{
		int @int = arg.GetInt(0, 200);
		WaterTreatmentWaterTank[] array = UnityEngine.Object.FindObjectsByType<WaterTreatmentWaterTank>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		WaterTreatmentWaterTank[] array2 = array;
		foreach (WaterTreatmentWaterTank waterTreatmentWaterTank in array2)
		{
			if (waterTreatmentWaterTank != null)
			{
				waterTreatmentWaterTank.Pressure = @int;
				waterTreatmentWaterTank.timeSinceLastPressureIncrease = 0f;
			}
		}
		arg.ReplyWith($"Forced pressure to ({@int}) for ({array.Length}) water tanks.");
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Pressure = 0f;
		FlowRatePerMinute = 0f;
		if (broadcaster != null)
		{
			broadcaster.SetWaterTank(this);
		}
		InvokeRepeating(TickWaterPressure, 5f, 5f);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		CancelInvoke(TickWaterPressure);
		if (broadcaster != null)
		{
			broadcaster.CleanUp();
		}
	}

	public override void ResetIOState()
	{
		Pressure = 0f;
		FlowRatePerMinute = 0f;
		SetFlagLocal(Flags.On, b: false);
		SendNetworkUpdate_Flags();
		base.ResetIOState();
	}

	public override float IOInput(IOEntity from, IOType inputType, float inputAmount, int slot = 0)
	{
		if (inputAmount > 0f)
		{
			if (Pressure <= 0f)
			{
				Pressure = 0f;
			}
			if (Pressure >= maximumPressure)
			{
				return inputAmount;
			}
			Pressure = Mathf.Min(Pressure + inputAmount, maximumPressure);
			timeSinceLastPressureIncrease = 0f;
			return 0f;
		}
		return inputAmount;
	}

	private void TickWaterPressure()
	{
		if ((float)timeSinceLastPressureIncrease >= 10f)
		{
			Pressure -= pressureDecayPerMinute / 12f;
			Pressure = Mathf.Max(Pressure, 0f);
		}
		FlowRatePerMinute = Mathf.Lerp(0f, maxFlowRatePerMinute, PressureAsPercentage);
		int num = Mathf.CeilToInt(FlowRatePerMinute);
		if (num != latestFlowRate)
		{
			WaterTreatmentFlowRateBroadcast.Refresh();
		}
		latestFlowRate = num;
		SetFlagLocal(Flags.On, Pressure > 0f);
		SendNetworkUpdate_Flags();
		MarkDirty();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return 1;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: Pressure for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_Pressure);
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
				_ = __sync_Pressure;
				float _sync_Pressure = reader.Float();
				__sync_Pressure = _sync_Pressure;
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
		if (propertyName == "Pressure")
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
		__sync_Pressure = 0f;
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

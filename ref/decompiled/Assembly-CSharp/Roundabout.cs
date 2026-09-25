using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class Roundabout : BaseVehicle
{
	public Transform spinner;

	public float spinAcceleration;

	public float maxAngularSpeed;

	public float coastDamping;

	public SoundDefinition rotationLoopDef;

	public AnimationCurve rotationGainCurve;

	private Quaternion spinnerBaseRot;

	private bool cachedBaseRot;

	private float spinVelocity;

	private Action spinTick;

	private float __sync_SpinAngle;

	[Sync(Autosave = true)]
	public float SpinAngle
	{
		[CompilerGenerated]
		get
		{
			return __sync_SpinAngle;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_SpinAngle, value))
			{
				__sync_SpinAngle = value;
				byte nameID = __GetWeaverID("SpinAngle");
				QueueSyncVar(nameID);
			}
		}
	}

	private void ApplySpin(float angle)
	{
		if (!(spinner == null))
		{
			if (!cachedBaseRot)
			{
				spinnerBaseRot = spinner.localRotation;
				cachedBaseRot = true;
			}
			spinner.localRotation = spinnerBaseRot * Quaternion.AngleAxis(angle, Vector3.up);
		}
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		if (spinTick == null)
		{
			spinTick = SpinTick;
		}
		if (!IsInvokingFixedTime(spinTick))
		{
			InvokeRepeatingFixedTime(spinTick);
		}
	}

	private void SpinTick()
	{
		float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
		int num = CountPushers();
		spinVelocity += spinAcceleration * (float)num * fixedDeltaTime;
		spinVelocity -= spinVelocity * coastDamping * fixedDeltaTime;
		spinVelocity = Mathf.Clamp(spinVelocity, 0f - maxAngularSpeed, maxAngularSpeed);
		SpinAngle = Mathf.Repeat(SpinAngle + spinVelocity * fixedDeltaTime, 360f);
		ApplySpin(SpinAngle);
		if (!AnyMounted() && Mathf.Abs(spinVelocity) < 1f)
		{
			spinVelocity = 0f;
			CancelInvokeFixedTime(spinTick);
		}
	}

	private int CountPushers()
	{
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		GetMountedPlayers(obj);
		int num = 0;
		foreach (BasePlayer item in obj)
		{
			if (item != null && item.serverInput.IsDown(BUTTON.FORWARD))
			{
				num++;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return num;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: SpinAngle for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_SpinAngle);
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
				_ = __sync_SpinAngle;
				float _sync_SpinAngle = reader.Float();
				__sync_SpinAngle = _sync_SpinAngle;
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
		if (propertyName == "SpinAngle")
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
		__sync_SpinAngle = 0f;
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

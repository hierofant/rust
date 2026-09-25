using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class SwingSeat : BaseVehicleSeat
{
	public Transform pivot;

	public Vector3 swingAxis = Vector3.right;

	public float minAngle;

	public float maxAngle;

	public float pumpAcceleration;

	public float gravityRestore;

	public float damping;

	public float launchScale = 1f;

	public float launchMinSpeed = 20f;

	[Range(0f, 1f)]
	public float launchLift = 0.25f;

	public bool heavyLanding;

	public bool flailOnLaunch = true;

	public ChildAnimatorSubSystem swingAnimator;

	public AnimationCurve swingBlendCurve;

	public SoundDefinition swingMovementLoopDef;

	public SoundDefinition swingMovementAccentDef;

	public AnimationCurve swingMovementGainCurve;

	public float swingMovementSmoothing = 4f;

	public AnimationCurve swingMovementAccentGainCurve;

	public const Flags Flag_Swinging = Flags.Reserved1;

	private const float SwingingVelocityThreshold = 5f;

	private const float SwingingAngleThreshold = 5f;

	private Quaternion pivotBaseRot;

	private bool cachedBaseRot;

	private float velocity;

	private int inputSign;

	private Action swingTick;

	private float __sync_SwingAngle;

	[Sync(Autosave = true)]
	public float SwingAngle
	{
		[CompilerGenerated]
		get
		{
			return __sync_SwingAngle;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_SwingAngle, value))
			{
				__sync_SwingAngle = value;
				byte nameID = __GetWeaverID("SwingAngle");
				QueueSyncVar(nameID);
			}
		}
	}

	public bool IsSwinging()
	{
		return HasFlag(Flags.Reserved1);
	}

	private void ApplySwing(float angle)
	{
		if (!(pivot == null))
		{
			if (!cachedBaseRot)
			{
				pivotBaseRot = pivot.localRotation;
				cachedBaseRot = true;
			}
			pivot.localRotation = pivotBaseRot * Quaternion.AngleAxis(angle, swingAxis);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateSwingingFlag();
		if (IsSwinging())
		{
			StartSwingTick();
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (!IsSwinging())
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		StartSwingTick();
	}

	private void StartSwingTick()
	{
		if (swingTick == null)
		{
			swingTick = SwingTick;
		}
		if (!IsInvokingFixedTime(swingTick))
		{
			InvokeRepeatingFixedTime(swingTick);
		}
	}

	private void UpdateSwingingFlag()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, Mathf.Abs(velocity) > 5f || Mathf.Abs(SwingAngle) > 5f);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if (pivot != null && mountAnchor != null && player != null && Mathf.Abs(velocity) > launchMinSpeed)
		{
			Vector3 lhs = pivot.TransformDirection(swingAxis).normalized * (velocity * (MathF.PI / 180f));
			Vector3 rhs = mountAnchor.position - pivot.position;
			Vector3 velocityOverride = Vector3.Cross(lhs, rhs) * launchScale;
			velocityOverride += Vector3.up * (velocityOverride.magnitude * launchLift);
			player.Ragdoll(velocityOverride, heavyLanding, flailOnLaunch);
			BaseMountable mounted = player.GetMounted();
			if (mounted != null)
			{
				mounted.gameObject.SetIgnoreCollisions(base.gameObject, ignore: true);
			}
		}
		inputSign = 0;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		bool flag = inputState.IsDown(BUTTON.BACKWARD);
		bool flag2 = inputState.IsDown(BUTTON.FORWARD);
		inputSign = ((flag != flag2) ? (flag ? 1 : (-1)) : 0);
	}

	private void SwingTick()
	{
		float fixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
		velocity += (float)inputSign * pumpAcceleration * fixedDeltaTime;
		velocity -= gravityRestore * Mathf.Sin(SwingAngle * (MathF.PI / 180f)) * fixedDeltaTime;
		velocity -= velocity * damping * fixedDeltaTime;
		float num = SwingAngle + velocity * fixedDeltaTime;
		if (num > maxAngle)
		{
			num = maxAngle;
			velocity = 0f;
		}
		else if (num < minAngle)
		{
			num = minAngle;
			velocity = 0f;
		}
		SwingAngle = num;
		ApplySwing(num);
		if (!AnyMounted() && Mathf.Abs(velocity) < 0.5f && Mathf.Abs(num) < 0.5f)
		{
			velocity = 0f;
			SwingAngle = 0f;
			ApplySwing(0f);
			CancelInvokeFixedTime(swingTick);
		}
		UpdateSwingingFlag();
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: SwingAngle for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_SwingAngle);
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
				_ = __sync_SwingAngle;
				float _sync_SwingAngle = reader.Float();
				__sync_SwingAngle = _sync_SwingAngle;
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
		if (propertyName == "SwingAngle")
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
		__sync_SwingAngle = 0f;
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

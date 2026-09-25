#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Chandelier : IOEntity
{
	public float DefaultLength = 0.25f;

	public float MaxLength = 3f;

	public float MinLength = 0.25f;

	public float MoveIncrement = 0.25f;

	public Transform ChandelierBodyRoot;

	public Transform ChandelierCableRoot;

	public MeshRenderer CableRenderer;

	public MeshRenderer PedalCableRenderer;

	public GameObjectRef adjustHeightEffect;

	[SerializeField]
	private float pedalAnglePerIncrement;

	public Transform PedalTransform;

	public BoxCollider ReferenceBoxTrace;

	private float lastLength = -1f;

	private const float baseBoundsSizeY = 0.35f;

	public static Translate.Phrase BlockedByObjectPhrase = new Translate.Phrase("chandelier.blocked", "Cannot extend through solid objects");

	public static Translate.Phrase MaxLengthPhrase = new Translate.Phrase("chandelier.maxlengthreached", "Maximum length reached");

	private float __sync_ChandelierLength;

	[Sync(Autosave = true)]
	public float ChandelierLength
	{
		[CompilerGenerated]
		get
		{
			return __sync_ChandelierLength;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ChandelierLength, value))
			{
				__sync_ChandelierLength = value;
				byte nameID = __GetWeaverID("ChandelierLength");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Chandelier.OnRpcMessage"))
		{
			if (rpc == 3461669953u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SERVER_AdjustChandelierLength");
				}
				using (TimeWarning.New("SERVER_AdjustChandelierLength"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3461669953u, "SERVER_AdjustChandelierLength", this, player, 5uL))
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
							SERVER_AdjustChandelierLength(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_AdjustChandelierLength");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 4;
	}

	public override float AntiHackPadding()
	{
		return ChandelierLength + 0.25f;
	}

	private void UpdateChandelierLength(float deltaTime)
	{
		float chandelierLength = ChandelierLength;
		if (base.isServer)
		{
			if (ChandelierBodyRoot != null)
			{
				float y = Mathx.Lerp(ChandelierBodyRoot.localPosition.y, 0f - ChandelierLength, 15f, deltaTime);
				ChandelierBodyRoot.localPosition = ChandelierBodyRoot.localPosition.WithY(y);
			}
		}
		else
		{
			SetBoundsYSize(0.35f + chandelierLength * 0.5f);
		}
	}

	private void ResetLength()
	{
		lastLength = -1f;
	}

	private void SetBoundsYSize(float y)
	{
		Bounds bounds = base.bounds;
		bounds.center = new Vector3(base.bounds.center.x, 0f - y, base.bounds.center.z);
		bounds.extents = new Vector3(base.bounds.extents.x, y, base.bounds.extents.z);
		base.bounds = bounds;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		UpdateChandelierLength(1000f);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		SetChandelierLength(0f);
	}

	private void StartServerTick()
	{
		CancelInvoke(ServerTick);
		InvokeRepeating(ServerTick, 0f, 0f);
		Invoke(delegate
		{
			CancelInvoke(ServerTick);
		}, 2.5f);
	}

	public void ServerTick()
	{
		if (!base.IsDestroyed)
		{
			UpdateChandelierLength(UnityEngine.Time.deltaTime);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	private void SERVER_AdjustChandelierLength(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || !player.CanBuild(cached: true) || player.Distance(ChandelierBodyRoot.position) > 3f)
		{
			return;
		}
		bool flag = msg.read.Bool();
		float num = MoveIncrement;
		if (flag)
		{
			num = 0f - num;
		}
		if (!flag && ChandelierLength >= MaxLength)
		{
			player.ShowToast(GameTip.Styles.Error, BlockedByObjectPhrase, false);
		}
		else
		{
			if (flag && ChandelierLength == MinLength)
			{
				return;
			}
			if (ReferenceBoxTrace != null)
			{
				using PooledList<RaycastHit> pooledList = Facepunch.Pool.Get<PooledList<RaycastHit>>();
				Vector3 direction = (flag ? Vector3.up : (-Vector3.up));
				GamePhysics.OBBSweep(new OBB(ReferenceBoxTrace.transform, new Bounds(ReferenceBoxTrace.center, ReferenceBoxTrace.size)), direction, MoveIncrement, pooledList, 1755513089);
				foreach (RaycastHit item in pooledList)
				{
					BaseEntity entity = RaycastHitEx.GetEntity(item);
					if (!(entity == this))
					{
						if (entity != null && entity.isServer)
						{
							player.ShowBlockedByEntityToast(entity, BlockedByObjectPhrase);
							return;
						}
						if (entity == null)
						{
							player.ShowToast(GameTip.Styles.Error, BlockedByObjectPhrase, false);
							return;
						}
					}
				}
			}
			float chandelierLength = Mathf.Clamp(ChandelierLength + num, MinLength, MaxLength);
			SetChandelierLength(chandelierLength);
		}
	}

	private void SetChandelierLength(float length)
	{
		if (length != ChandelierLength)
		{
			ChandelierLength = length;
			lastLength = length;
			StartServerTick();
			if (adjustHeightEffect.isValid)
			{
				Effect.server.Run(adjustHeightEffect.resourcePath, base.transform.position);
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && lastLength != ChandelierLength)
		{
			StartServerTick();
			lastLength = ChandelierLength;
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: ChandelierLength for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_ChandelierLength);
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
				_ = __sync_ChandelierLength;
				float _sync_ChandelierLength = reader.Float();
				__sync_ChandelierLength = _sync_ChandelierLength;
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
		if (propertyName == "ChandelierLength")
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
		__sync_ChandelierLength = 0f;
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

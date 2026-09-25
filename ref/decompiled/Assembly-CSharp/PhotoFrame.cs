#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PhotoFrame : StorageContainer, ILOD, IImageReceiver, ISignage, IUGCBrowserEntity, IEaselPaintable
{
	public GameObjectRef SignEditorDialog;

	public OverlayMeshPaintableSource PaintableSource;

	private const float TextureRequestDistance = 100f;

	public EntityRef _photoEntity;

	public uint _overlayTextureCrc;

	private EaselDeployable _cachedParentEasel;

	private List<ulong> editHistory = new List<ulong>();

	private NetworkableId __sync_EaselId;

	public Vector2i TextureSize => new Vector2i(PaintableSource.texWidth, PaintableSource.texHeight);

	public int TextureCount => 1;

	public GameObject GameObject => base.gameObject;

	[Sync(Autosave = true)]
	public NetworkableId EaselId
	{
		[CompilerGenerated]
		get
		{
			return __sync_EaselId;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_EaselId, value))
			{
				__sync_EaselId = value;
				byte nameID = __GetWeaverID("EaselId");
				QueueSyncVar(nameID);
			}
		}
	}

	public EaselDeployable parentEasel
	{
		get
		{
			if (!EaselId.IsValid)
			{
				return null;
			}
			if (_cachedParentEasel == null)
			{
				_cachedParentEasel = new EntityRef<EaselDeployable>(EaselId).Get(base.isServer);
			}
			return _cachedParentEasel;
		}
	}

	public NetworkableId NetworkID => net.ID;

	public FileStorage.Type FileType => FileStorage.Type.png;

	public UGCType ContentType => UGCType.ImagePng;

	public List<ulong> EditingHistory => editHistory;

	public uint[] GetContentCRCs => new uint[1] { _overlayTextureCrc };

	public override bool ShouldTransferAssociatedFiles => true;

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PhotoFrame.OnRpcMessage"))
		{
			if (rpc == 1455609404 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - LockSign");
				}
				using (TimeWarning.New("LockSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1455609404u, "LockSign", this, player, 3f))
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
							LockSign(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in LockSign");
					}
				}
				return true;
			}
			if (rpc == 4149904254u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UnLockSign");
				}
				using (TimeWarning.New("UnLockSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4149904254u, "UnLockSign", this, player, 3f))
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
							UnLockSign(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in UnLockSign");
					}
				}
				return true;
			}
			if (rpc == 1255380462 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UpdateSign");
				}
				using (TimeWarning.New("UpdateSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1255380462u, "UpdateSign", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1255380462u, "UpdateSign", this, player, 5f))
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
							UpdateSign(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in UpdateSign");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool CanUpdateSign(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUpdateSign", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (IsLocked())
		{
			return (ulong)player.userID == base.OwnerID;
		}
		return true;
	}

	public bool CanUnlockSign(BasePlayer player)
	{
		if (!IsLocked())
		{
			return false;
		}
		return CanUpdateSign(player);
	}

	public bool CanLockSign(BasePlayer player)
	{
		if (IsLocked())
		{
			return false;
		}
		return CanUpdateSign(player);
	}

	[RPC_Server.MaxDistance(5f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public void UpdateSign(RPCMessage msg)
	{
		if (!(msg.player == null) && CanUpdateSign(msg.player))
		{
			byte[] array = msg.read.BytesWithSize();
			if (array != null && ImageProcessing.IsValidPNG(array, 1024, 1024))
			{
				FileStorage.server.RemoveAllByEntity(net.ID);
				_overlayTextureCrc = FileStorage.server.Store(array, FileStorage.Type.png, net.ID);
				LogEdit(msg.player);
				SendNetworkUpdate();
				Interface.CallHook("OnSignUpdated", this, msg.player);
			}
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void LockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUpdateSign(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: true);
			SendNetworkUpdate();
			base.OwnerID = msg.player.userID;
			Interface.CallHook("OnSignLocked", this, msg.player);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void UnLockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUnlockSign(msg.player))
		{
			SetFlagLocal(Flags.Locked, b: false);
			SendNetworkUpdate();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		RemoveFromEasel();
		if (net != null)
		{
			FileStorage.server.RemoveAllByEntity(net.ID);
		}
		_overlayTextureCrc = 0u;
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override string Categorize()
	{
		return "sign";
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.photoFrame != null)
		{
			_photoEntity.uid = info.msg.photoFrame.photoEntityId;
			_overlayTextureCrc = info.msg.photoFrame.overlayImageCrc;
		}
		if (!base.isServer || info.msg.photoFrame == null)
		{
			return;
		}
		if (info.msg.photoFrame.editHistory != null)
		{
			if (editHistory == null)
			{
				editHistory = Facepunch.Pool.Get<List<ulong>>();
			}
			editHistory.Clear();
			{
				foreach (ulong item in info.msg.photoFrame.editHistory)
				{
					editHistory.Add(item);
				}
				return;
			}
		}
		if (editHistory != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref editHistory);
		}
	}

	public uint[] GetTextureCRCs()
	{
		return new uint[1] { _overlayTextureCrc };
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.photoFrame = Facepunch.Pool.Get<ProtoBuf.PhotoFrame>();
		info.msg.photoFrame.photoEntityId = _photoEntity.uid;
		info.msg.photoFrame.overlayImageCrc = _overlayTextureCrc;
		if (editHistory.Count <= 0)
		{
			return;
		}
		info.msg.photoFrame.editHistory = Facepunch.Pool.Get<List<ulong>>();
		foreach (ulong item in editHistory)
		{
			info.msg.photoFrame.editHistory.Add(item);
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		Item item2 = ((base.inventory.itemList.Count > 0) ? base.inventory.itemList[0] : null);
		NetworkableId networkableId = ((item2 != null && item2.IsValid()) ? item2.instanceData.subEntity : default(NetworkableId));
		if (networkableId != _photoEntity.uid)
		{
			_photoEntity.uid = networkableId;
			SendNetworkUpdate();
		}
	}

	public override void OnPickedUpPreItemMove(Item createdItem, BasePlayer player)
	{
		base.OnPickedUpPreItemMove(createdItem, player);
		SaveSignageToItem(createdItem);
	}

	public void SaveSignageToItem(Item createdItem)
	{
		if (_overlayTextureCrc != 0 && createdItem.info.TryGetComponent<ItemModSign>(out var component))
		{
			component.OnSignPickedUp(this, this, createdItem);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		AddToEasel(parent);
		if (fromItem.info.TryGetComponent<ItemModSign>(out var _))
		{
			SignContent associatedEntity = ItemModAssociatedEntity<SignContent>.GetAssociatedEntity(fromItem);
			if (associatedEntity != null)
			{
				associatedEntity.CopyInfoToSign(this, this);
			}
		}
	}

	public void SetTextureCRCs(uint[] crcs)
	{
		if (crcs.Length != 0)
		{
			_overlayTextureCrc = crcs[0];
			SendNetworkUpdate();
		}
	}

	private void LogEdit(BasePlayer byPlayer)
	{
		if (!editHistory.Contains(byPlayer.userID))
		{
			editHistory.Insert(0, byPlayer.userID);
			int num = 0;
			while (editHistory.Count > 5 && num < 10)
			{
				editHistory.RemoveAt(5);
				num++;
			}
		}
	}

	public void ClearContent()
	{
		_overlayTextureCrc = 0u;
		SendNetworkUpdate();
	}

	public void RemoveFromEasel()
	{
		if (parentEasel != null)
		{
			parentEasel.RemovePainting(this);
		}
		EaselId = default(NetworkableId);
	}

	public void AddToEasel(BaseEntity parent)
	{
		if (parent != null && parent is EaselDeployable easelDeployable)
		{
			EaselId = easelDeployable.net.ID;
			easelDeployable.SetPainting(this);
		}
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		if (_photoEntity.uid.IsValid)
		{
			pickupErrorToFormat = (format: PickupErrors.ItemMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		if (base.CanCompletePickup(player))
		{
			return !_photoEntity.uid.IsValid;
		}
		return false;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: EaselId for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_EaselId);
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
				_ = __sync_EaselId;
				NetworkableId _sync_EaselId = reader.EntityID();
				__sync_EaselId = _sync_EaselId;
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
		if (propertyName == "EaselId")
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
		__sync_EaselId = default(NetworkableId);
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

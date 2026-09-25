#define UNITY_ASSERTIONS
using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SteeringWheel : BaseMountable, global::IBoatBuildingPiece
{
	public static Translate.Phrase TipPhrase = new Translate.Phrase("boat_steeringwheel_tip", "Interact with the steering wheel to code lock your boat.");

	public static Translate.Phrase MountedTipPhrase = new Translate.Phrase("boat_steeringwheel_tip_mounted", "Look at the center of the wheel for options when mounted.");

	[Header("Steering Wheel")]
	public GameObjectRef PrivPrefab;

	public GameObjectRef KeyEnterDialog;

	public float TurnLerpSpeed = 1f;

	public Transform Wheel;

	public PlayerBoatPrivilege Privilege;

	public SoundDefinition wheelTurnLoopDef;

	public SoundDefinition wheelTurnStartDef;

	public SoundDefinition wheelTurnStopDef;

	public float wheelTurnLoopFadeTime = 0.1f;

	public float stopDelay;

	public GameObjectRef finishBuildingEffect;

	public SoundDefinition wheelCenterDef;

	[Header("Effects")]
	public Transform EffectLocation;

	public GameObjectRef effectUnlocked;

	public GameObjectRef effectLocked;

	public GameObjectRef effectDenied;

	public GameObjectRef effectCodeChanged;

	public GameObjectRef effectShock;

	[NonSerialized]
	public PlayerBoat ParentBoat;

	private Action _mountedPlayerClipCheck;

	private float __sync_ServerSteeringRotation;

	public PlayerBoatLock BoatLock { get; private set; }

	[Sync(Pack = false, Autosave = true)]
	public float ServerSteeringRotation
	{
		[CompilerGenerated]
		get
		{
			return __sync_ServerSteeringRotation;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ServerSteeringRotation, value))
			{
				__sync_ServerSteeringRotation = value;
				byte nameID = __GetWeaverID("ServerSteeringRotation");
				SV_SyncVarSend(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SteeringWheel.OnRpcMessage"))
		{
			if (rpc == 3277541392u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ReceiveClientRotation");
				}
				using (TimeWarning.New("ReceiveClientRotation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3277541392u, "ReceiveClientRotation", this, player, 15uL))
						{
							return true;
						}
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<float>()))
						{
							return true;
						}
						msg.read.Position = position;
						if (!RPC_Server.IsVisible.Test(3277541392u, "ReceiveClientRotation", this, player, 3f))
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
							ReceiveClientRotation(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ReceiveClientRotation");
					}
				}
				return true;
			}
			if (rpc == 1618039250 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestDeployAndEditBoat");
				}
				using (TimeWarning.New("RequestDeployAndEditBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1618039250u, "RequestDeployAndEditBoat", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1618039250u, "RequestDeployAndEditBoat", this, player, 3f))
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
							RequestDeployAndEditBoat(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RequestDeployAndEditBoat");
					}
				}
				return true;
			}
			if (rpc == 3915953376u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestEditBoat");
				}
				using (TimeWarning.New("RequestEditBoat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3915953376u, "RequestEditBoat", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3915953376u, "RequestEditBoat", this, player, 3f))
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
							RequestEditBoat(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RequestEditBoat");
					}
				}
				return true;
			}
			if (rpc == 3194773350u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestFinishBuilding");
				}
				using (TimeWarning.New("RequestFinishBuilding"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3194773350u, "RequestFinishBuilding", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3194773350u, "RequestFinishBuilding", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							RequestFinishBuilding(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RequestFinishBuilding");
					}
				}
				return true;
			}
			if (rpc == 3963850389u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestFinishBuildingFromWheel");
				}
				using (TimeWarning.New("RequestFinishBuildingFromWheel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3963850389u, "RequestFinishBuildingFromWheel", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3963850389u, "RequestFinishBuildingFromWheel", this, player, 3f))
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
							RPCMessage msg6 = rPCMessage;
							RequestFinishBuildingFromWheel(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RequestFinishBuildingFromWheel");
					}
				}
				return true;
			}
			if (rpc == 3710764312u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_RequestAddLock");
				}
				using (TimeWarning.New("RPC_RequestAddLock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3710764312u, "RPC_RequestAddLock", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3710764312u, "RPC_RequestAddLock", this, player, 3f))
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
							RPCMessage msg7 = rPCMessage;
							RPC_RequestAddLock(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in RPC_RequestAddLock");
					}
				}
				return true;
			}
			if (rpc == 2818660542u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_TryMountWithKeycode");
				}
				using (TimeWarning.New("RPC_TryMountWithKeycode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2818660542u, "RPC_TryMountWithKeycode", this, player, 3f))
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
							RPCMessage msg8 = rPCMessage;
							RPC_TryMountWithKeycode(msg8);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in RPC_TryMountWithKeycode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public PlayerBoat GetParentBoat()
	{
		return PlayerBoat.GetParentPlayerBoat(this);
	}

	public override bool DirectlyMountable()
	{
		return true;
	}

	private BoatBuildingStation GetCurrentBoatBuildingStation(BasePlayer player)
	{
		TriggerBoatBuildingArea triggerBoatBuildingArea = player.FindTrigger<TriggerBoatBuildingArea>();
		if (triggerBoatBuildingArea == null)
		{
			return null;
		}
		return triggerBoatBuildingArea.GetComponentInParent<BoatBuildingStation>();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		BoatLock.Load(info);
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (child is PlayerBoatPrivilege privilege)
		{
			Privilege = privilege;
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		if (BoatLock == null)
		{
			BoatLock = new PlayerBoatLock(this, base.isServer);
		}
	}

	public bool IsFlipped()
	{
		return Vector3.Dot(Vector3.up, base.transform.up) <= 0.175f;
	}

	public bool IsAuthed(BasePlayer player)
	{
		if (Privilege == null)
		{
			return false;
		}
		return Privilege.IsAuthed(player);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Locked, BoatLock != null && BoatLock.HasALock);
		}
		ServerSteeringRotation = 0f;
		CreatePrivilege();
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		AuthPlayer(deployedBy);
		BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(base.transform.position, isServer: true);
		if (stationOverlappingPosition != null)
		{
			stationOverlappingPosition.OnSteeringWheelPlaced(this);
		}
		ClientRPC(RpcTarget.Player("CLIENT_OnDeployed", deployedBy));
	}

	internal override void DoServerDestroy()
	{
		if (ParentBoat == null)
		{
			BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(base.transform.position, isServer: true);
			if (stationOverlappingPosition != null)
			{
				stationOverlappingPosition.OnSteeringWheelRemoved(this);
			}
		}
		base.DoServerDestroy();
	}

	private void CreatePrivilege()
	{
		if (!Rust.Application.isLoadingSave)
		{
			Privilege = null;
			BaseEntity baseEntity = GameManager.server.CreateEntity(PrivPrefab.resourcePath, base.transform.position, base.transform.rotation);
			if (baseEntity != null)
			{
				baseEntity.SetParent(this, worldPositionStays: true);
				baseEntity.Spawn();
			}
			Privilege = baseEntity as PlayerBoatPrivilege;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.steeringWheel = Facepunch.Pool.Get<ProtoBuf.SteeringWheel>();
		BoatLock.Save(info);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (!base.isServer)
		{
			return;
		}
		BoatLock.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, BoatLock.HasALock);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	public void RPC_RequestAddLock(RPCMessage msg)
	{
		if (BoatLock.HasALock)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (player == null)
		{
			return;
		}
		string code = msg.read.String();
		BoatLock.TryAddALock(code, player);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, BoatLock.HasALock);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_TryMountWithKeycode(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			string codeEntered = msg.read.String();
			if (BoatLock.TryOpenWithCode(player, codeEntered))
			{
				AuthPlayer(player);
				WantsMount(player);
			}
		}
	}

	private void AuthPlayer(BasePlayer player)
	{
		if (!(Privilege == null) && !Privilege.IsAuthed(player))
		{
			Privilege.AddPlayer(player);
			Privilege.SendNetworkUpdate();
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if ((!BoatLock.HasALock || BoatLock.HasLockPermission(player)) && !IsFlipped() && (!(ParentBoat != null) || !ParentBoat.IsDying))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		if ((bool)ParentBoat)
		{
			using FlagsUpdateScope flagsUpdateScope = ParentBoat.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved17, b: true);
		}
		if (_mountedPlayerClipCheck == null)
		{
			_mountedPlayerClipCheck = CheckSeatClipping;
		}
		InvokeRepeatingFixedTime(_mountedPlayerClipCheck);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if ((bool)ParentBoat)
		{
			using FlagsUpdateScope flagsUpdateScope = ParentBoat.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved17, b: false);
		}
		if (_mountedPlayerClipCheck == null)
		{
			_mountedPlayerClipCheck = CheckSeatClipping;
		}
		CancelInvokeFixedTime(_mountedPlayerClipCheck);
	}

	private void CheckSeatClipping()
	{
		if (_mountedPlayerClipCheck == null)
		{
			_mountedPlayerClipCheck = CheckSeatClipping;
		}
		if (!AnyMounted())
		{
			CancelInvokeFixedTime(_mountedPlayerClipCheck);
		}
		else if (IsSeatClipping(this))
		{
			DismountAllPlayers();
			CancelInvokeFixedTime(_mountedPlayerClipCheck);
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RequestFinishBuilding(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			BoatBuildingStation currentBoatBuildingStation = GetCurrentBoatBuildingStation(player);
			if (!(currentBoatBuildingStation == null))
			{
				BoatBuildingStation.LogBuildingEvent(base.transform.position, player, null, "SteeringWheel rquesting FinishBuilding");
				currentBoatBuildingStation.FinishBuilding(msg.player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void RequestFinishBuildingFromWheel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null)
		{
			return;
		}
		BoatBuildingStation.LogBuildingEvent(base.transform.position, msg.player, null, "Finish boat requested via steering wheel.");
		BoatBuildingStation currentBoatBuildingStation = GetCurrentBoatBuildingStation(player);
		if (!(currentBoatBuildingStation != null) || !currentBoatBuildingStation.FinishBuilding(msg.player) || currentBoatBuildingStation.IsStatic)
		{
			return;
		}
		currentBoatBuildingStation.KilledDuringWheelFinish = true;
		currentBoatBuildingStation.Kill();
		if (!(ParentBoat == null))
		{
			Item item = ItemManager.Create(ParentBoat.BoatBuildingStationItem, 1, 0uL, isServerSide: true, 0uL);
			item.SetItemOwnership(player, ItemOwnershipPhrases.PickedUp);
			player.GiveItem(item, GiveItemReason.PickedUp);
			if (finishBuildingEffect.isValid)
			{
				Effect.server.Run(finishBuildingEffect.resourcePath, this);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void RequestEditBoat(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null)
		{
			return;
		}
		BoatBuildingStation currentBoatBuildingStation = GetCurrentBoatBuildingStation(player);
		if (!(currentBoatBuildingStation == null))
		{
			BoatBuildingStation.LogBuildingEvent(base.transform.position, player, null, "Edit boat requested from steering wheel.");
			if (currentBoatBuildingStation.CanEnterEditMode(player, sendErrorToasts: true))
			{
				currentBoatBuildingStation.EnterEditMode();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void RequestDeployAndEditBoat(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			PlayerBoat parentBoat = GetParentBoat();
			if (!(parentBoat == null))
			{
				BoatBuildingStation.LogBuildingEvent(base.transform.position, player, parentBoat, "Deploy and Edit requested via steering wheel.");
				parentBoat.DeployAndEdit(msg.player);
			}
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

	[RPC_Server.InputValidation(new Type[] { typeof(float) })]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(15uL)]
	public void ReceiveClientRotation(RPCMessage msg)
	{
		if (!(msg.player == null) && !(GetMounted() != msg.player) && !(ParentBoat == null))
		{
			float val = (ServerSteeringRotation = msg.read.Float());
			ParentBoat.steering = Mathx.RemapValClamped(val, -170f, 170f, 1f, -1f);
		}
	}

	public void ResetSteering()
	{
		ServerSteeringRotation = 0f;
		ParentBoat.steering = 0f;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if (ParentBoat != null)
		{
			ParentBoat.ResetTimeSinceUsed();
		}
	}

	void global::IBoatBuildingPiece.OnAddedToBoat(PlayerBoat boat)
	{
		ParentBoat = boat;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return !PlayerBoat.IsChildOfInteractablePlayerBoat(this);
		}
		return false;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		if (id == 0)
		{
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: ServerSteeringRotation for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_ServerSteeringRotation);
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
				_ = __sync_ServerSteeringRotation;
				float _sync_ServerSteeringRotation = reader.Float();
				__sync_ServerSteeringRotation = _sync_ServerSteeringRotation;
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
		if (propertyName == "ServerSteeringRotation")
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
		__sync_ServerSteeringRotation = 0f;
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

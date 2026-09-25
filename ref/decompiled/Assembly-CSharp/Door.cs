#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Door : AnimatedBuildingBlock, INotifyTrigger, ISimpleUpgradable
{
	public static readonly Translate.Phrase UpgradeBlockedLock = new Translate.Phrase("simple.upgrade.blocked_lock", "Remove lock to upgrade.");

	public GameObjectRef knockEffect;

	public bool canTakeLock = true;

	public bool hasHatch;

	public bool canTakeCloser;

	public bool canTakeKnocker;

	public bool canNpcOpen = true;

	public bool canHandOpen = true;

	public bool isSecurityDoor;

	public bool canReverseOpen;

	public TriggerNotify[] vehiclePhysBoxes;

	public bool checkPhysBoxesOnOpen;

	public SoundDefinition vehicleCollisionSfx;

	public GameObject[] BusyColliderRoots;

	public GameObject[] ClosedColliderRoots;

	public bool allowOnCargoShip;

	public bool useCastNoClipChecks;

	public List<ItemDefinition> UpgradeItems;

	public Menu.Option UpgradeMenu;

	[ReadOnly]
	[SerializeField]
	private float openAnimLength = 4f;

	[ReadOnly]
	[SerializeField]
	private float closeAnimLength = 4f;

	public const Flags ReverseOpen = Flags.Reserved1;

	public NavMeshModifierVolume NavMeshVolumeAnimals;

	public NavMeshModifierVolume NavMeshVolumeHumanoids;

	public NPCDoorTriggerBox NpcTriggerBox;

	public NavMeshLink NavMeshLink;

	private static int nonWalkableArea = -1;

	private static int animalAgentTypeId = -1;

	private static int humanoidAgentTypeId = -1;

	private float decayResetTimeLast = float.NegativeInfinity;

	private Dictionary<BasePlayer, TimeSince> woundedOpens = new Dictionary<BasePlayer, TimeSince>();

	private Dictionary<BasePlayer, TimeSince> woundedCloses = new Dictionary<BasePlayer, TimeSince>();

	private Action enableVehiclePhysBoxesAction;

	private Action disableVehiclePhysBoxAction;

	private float nextKnockTime = float.NegativeInfinity;

	private static int openHash = Animator.StringToHash("open");

	private static int closeHash = Animator.StringToHash("close");

	private static int reverseOpenHash = Animator.StringToHash("reverseOpen");

	private static int reverseCloseAnimHash = Animator.StringToHash("CloseReverse");

	private static int reverseOpenAnimHash = Animator.StringToHash("OpenReverse");

	public bool IsNpcOpenable
	{
		get
		{
			if (canNpcOpen)
			{
				return !isSecurityDoor;
			}
			return false;
		}
	}

	public override bool AllowOnCargoShip => allowOnCargoShip;

	private bool HasVehiclePushBoxes
	{
		get
		{
			if (vehiclePhysBoxes != null)
			{
				return vehiclePhysBoxes.Length != 0;
			}
			return false;
		}
	}

	public override bool PreserveChildrenWhenReskinning => true;

	protected virtual bool IgnoreBlockageDotCheck => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Door.OnRpcMessage"))
		{
			if (rpc == 2824056853u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - DoSimpleUpgrade");
				}
				using (TimeWarning.New("DoSimpleUpgrade"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2824056853u, "DoSimpleUpgrade", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2824056853u, "DoSimpleUpgrade", this, player, 3f))
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
							DoSimpleUpgrade(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in DoSimpleUpgrade");
					}
				}
				return true;
			}
			if (rpc == 3999508679u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_CloseDoor");
				}
				using (TimeWarning.New("RPC_CloseDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3999508679u, "RPC_CloseDoor", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_CloseDoor(rpc2);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_CloseDoor");
					}
				}
				return true;
			}
			if (rpc == 1487779344 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_KnockDoor");
				}
				using (TimeWarning.New("RPC_KnockDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1487779344u, "RPC_KnockDoor", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							RPC_KnockDoor(rpc3);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_KnockDoor");
					}
				}
				return true;
			}
			if (rpc == 3314360565u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenDoor");
				}
				using (TimeWarning.New("RPC_OpenDoor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3314360565u, "RPC_OpenDoor", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							RPC_OpenDoor(rpc4);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_OpenDoor");
					}
				}
				return true;
			}
			if (rpc == 3000490601u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ToggleHatch");
				}
				using (TimeWarning.New("RPC_ToggleHatch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3000490601u, "RPC_ToggleHatch", this, player, 3f))
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
							RPCMessage rpc5 = rPCMessage;
							RPC_ToggleHatch(rpc5);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in RPC_ToggleHatch");
					}
				}
				return true;
			}
			if (rpc == 3672787865u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_NotifyWoundedClose");
				}
				using (TimeWarning.New("Server_NotifyWoundedClose"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3672787865u, "Server_NotifyWoundedClose", this, player, 3f))
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
							Server_NotifyWoundedClose(msg3);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in Server_NotifyWoundedClose");
					}
				}
				return true;
			}
			if (rpc == 3730851545u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_NotifyWoundedOpen");
				}
				using (TimeWarning.New("Server_NotifyWoundedOpen"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3730851545u, "Server_NotifyWoundedOpen", this, player, 3f))
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
							Server_NotifyWoundedOpen(msg4);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in Server_NotifyWoundedOpen");
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
		if (base.isServer)
		{
			decayResetTimeLast = float.NegativeInfinity;
			if (isSecurityDoor && NavMeshLink != null)
			{
				SetNavMeshLinkEnabled(wantsOn: false);
			}
			woundedCloses.Clear();
			woundedOpens.Clear();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.door = Facepunch.Pool.Get<ProtoBuf.Door>();
		info.msg.door.canNpcOpen = canNpcOpen;
		info.msg.door.canHandOpen = canHandOpen;
		info.msg.door.isSecurityDoor = isSecurityDoor;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (nonWalkableArea < 0)
		{
			nonWalkableArea = NavMesh.GetAreaFromName("Not Walkable");
		}
		if (animalAgentTypeId < 0)
		{
			animalAgentTypeId = NavMesh.GetSettingsByIndex(1).agentTypeID;
		}
		if (NavMeshVolumeAnimals == null)
		{
			NavMeshVolumeAnimals = base.gameObject.AddComponent<NavMeshModifierVolume>();
			NavMeshVolumeAnimals.area = nonWalkableArea;
			NavMeshVolumeAnimals.AddAgentType(animalAgentTypeId);
			NavMeshVolumeAnimals.center = Vector3.zero;
			NavMeshVolumeAnimals.size = Vector3.one;
		}
		if (!Rust.Application.isLoadingSave)
		{
			InitializeNpcInteraction();
		}
		AIInformationZone forPoint = AIInformationZone.GetForPoint(base.transform.position);
		if (forPoint != null && NavMeshLink == null)
		{
			NavMeshLink = forPoint.GetClosestNavMeshLink(base.transform.position);
		}
		DisableVehiclePhysBox();
		UpdateColliderStates();
	}

	private void InitializeNpcInteraction()
	{
		if (HasSlot(Slot.Lock))
		{
			canNpcOpen = false;
		}
		if (!canNpcOpen)
		{
			if (humanoidAgentTypeId < 0)
			{
				humanoidAgentTypeId = NavMesh.GetSettingsByIndex(0).agentTypeID;
			}
			if (NavMeshVolumeHumanoids == null)
			{
				NavMeshVolumeHumanoids = base.gameObject.AddComponent<NavMeshModifierVolume>();
				NavMeshVolumeHumanoids.area = nonWalkableArea;
				NavMeshVolumeHumanoids.AddAgentType(humanoidAgentTypeId);
				NavMeshVolumeHumanoids.center = Vector3.zero;
				NavMeshVolumeHumanoids.size = Vector3.one + Vector3.up + Vector3.forward;
			}
		}
		else if (NpcTriggerBox == null)
		{
			if (isSecurityDoor)
			{
				NavMeshObstacle navMeshObstacle = base.gameObject.AddComponent<NavMeshObstacle>();
				navMeshObstacle.carving = true;
				navMeshObstacle.center = Vector3.zero;
				navMeshObstacle.size = Vector3.one;
				navMeshObstacle.shape = NavMeshObstacleShape.Box;
			}
			NpcTriggerBox = new GameObject("NpcTriggerBox").AddComponent<NPCDoorTriggerBox>();
			NpcTriggerBox.Setup(this);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		InitializeNpcInteraction();
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		base.OnEntityMessage(from, msg);
		if (msg == "ForceOpenNPCDoor")
		{
			ForceOpenNPCDoor();
		}
	}

	public void ForceOpenNPCDoor()
	{
		SetOpen(open: true);
		Invoke(CloseRequest, 5f);
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock && canTakeLock)
		{
			return true;
		}
		switch (slot)
		{
		case Slot.UpperModifier:
			return true;
		case Slot.CenterDecoration:
			if (canTakeCloser)
			{
				return true;
			}
			break;
		}
		if (slot == Slot.LowerCenterDecoration && canTakeKnocker)
		{
			return true;
		}
		return base.HasSlot(slot);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.door != null)
		{
			canNpcOpen = info.msg.door.canNpcOpen;
			canHandOpen = info.msg.door.canHandOpen;
			isSecurityDoor = info.msg.door.isSecurityDoor;
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (IsOpen() && !GetSlot(Slot.Lock))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		pickupErrorToFormat.arg0 = pickup.itemTarget.displayName;
		if ((bool)GetSlot(Slot.UpperModifier))
		{
			pickupErrorToFormat.format = PickupErrors.ItemHasCloser;
			return false;
		}
		if ((bool)GetSlot(Slot.CenterDecoration) || (bool)GetSlot(Slot.LowerCenterDecoration))
		{
			pickupErrorToFormat.format = PickupErrors.ItemHasDecoration;
			return false;
		}
		return base.CanCompletePickup(player);
	}

	public void CloseRequest()
	{
		SetOpen(open: false);
	}

	[UnityEvent]
	public void SetOpen(bool open, bool suppressBlockageChecks = false)
	{
		SetFlagLocal(Flags.Open, open);
		SendNetworkUpdateImmediate();
		if (isSecurityDoor && NavMeshLink != null)
		{
			SetNavMeshLinkEnabled(open);
		}
		if (!suppressBlockageChecks && (!open || checkPhysBoxesOnOpen))
		{
			StartCheckingForBlockages(open);
		}
	}

	[UnityEvent]
	public void SetLocked(bool locked)
	{
		SetFlagLocal(Flags.Locked, b: false);
		SendNetworkUpdateImmediate();
	}

	public bool GetPlayerLockPermission(BasePlayer player)
	{
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock == null)
		{
			return true;
		}
		return baseLock.GetPlayerLockPermission(player);
	}

	public void SetNavMeshLinkEnabled(bool wantsOn)
	{
		if (NavMeshLink != null)
		{
			if (wantsOn)
			{
				NavMeshLink.gameObject.SetActive(value: true);
				NavMeshLink.enabled = true;
			}
			else
			{
				NavMeshLink.enabled = false;
				NavMeshLink.gameObject.SetActive(value: false);
			}
		}
	}

	protected virtual bool CanDoorBeOpened()
	{
		return true;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	protected void RPC_OpenDoor(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !canHandOpen || IsOpen() || IsBusy() || IsLocked() || IsInvoking(DelayedDoorOpening) || !CanDoorBeOpened())
		{
			return;
		}
		if (rpc.player.IsWounded())
		{
			if (!woundedOpens.ContainsKey(rpc.player) || !((float)woundedOpens[rpc.player] > 2.5f))
			{
				return;
			}
			woundedOpens.Remove(rpc.player);
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock != null)
		{
			if (!baseLock.OnTryToOpen(rpc.player))
			{
				return;
			}
			if (baseLock.IsLocked() && UnityEngine.Time.realtimeSinceStartup - decayResetTimeLast > 60f)
			{
				BuildingBlock buildingBlock = FindLinkedEntity<BuildingBlock>();
				if ((bool)buildingBlock)
				{
					Decay.BuildingDecayTouch(buildingBlock);
				}
				else
				{
					Decay.RadialDecayTouch(base.transform.position, 40f, 2097408);
				}
				decayResetTimeLast = UnityEngine.Time.realtimeSinceStartup;
			}
		}
		if (canReverseOpen)
		{
			SetFlagLocal(Flags.Reserved1, base.transform.InverseTransformPoint(rpc.player.transform.position).x > 0f);
		}
		if (ShouldDelayOpen(rpc.player, out var delay))
		{
			Invoke(DelayedDoorOpening, delay);
		}
		else
		{
			SetFlagLocal(Flags.Open, b: true);
			SendNetworkUpdateImmediate();
		}
		if (isSecurityDoor && NavMeshLink != null)
		{
			SetNavMeshLinkEnabled(wantsOn: true);
		}
		if (checkPhysBoxesOnOpen)
		{
			StartCheckingForBlockages(isOpening: true);
		}
		Facepunch.Rust.Analytics.Azure.OnBaseInteract(rpc.player, this);
		OnPlayerOpenedDoor(rpc.player);
		Interface.CallHook("OnDoorOpened", this, rpc.player);
	}

	private void DelayedDoorOpening()
	{
		SetFlagLocal(Flags.Open, b: true);
		SendNetworkUpdateImmediate();
	}

	protected virtual void OnPlayerOpenedDoor(BasePlayer p)
	{
	}

	protected virtual bool ShouldDelayOpen(BasePlayer forPlayer, out float delay)
	{
		delay = 0f;
		return false;
	}

	private void StartCheckingForBlockages(bool isOpening)
	{
		using (TimeWarning.New("StartCheckingForBlockages"))
		{
			if (HasVehiclePushBoxes)
			{
				if (enableVehiclePhysBoxesAction == null)
				{
					enableVehiclePhysBoxesAction = EnableVehiclePhysBoxes;
				}
				if (disableVehiclePhysBoxAction == null)
				{
					disableVehiclePhysBoxAction = DisableVehiclePhysBox;
				}
				float num = (isOpening ? openAnimLength : closeAnimLength);
				Invoke(enableVehiclePhysBoxesAction, num * 0.1f);
				Invoke(disableVehiclePhysBoxAction, num * 0.8f);
			}
		}
	}

	private void StopCheckingForBlockages()
	{
		using (TimeWarning.New("StopCheckingForBlockages"))
		{
			if (HasVehiclePushBoxes)
			{
				if (disableVehiclePhysBoxAction == null)
				{
					disableVehiclePhysBoxAction = DisableVehiclePhysBox;
				}
				ToggleVehiclePushBoxes(state: false);
				CancelInvoke(disableVehiclePhysBoxAction);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_CloseDoor(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !canHandOpen || !IsOpen() || IsBusy() || IsLocked())
		{
			return;
		}
		if (rpc.player.IsWounded())
		{
			if (!woundedCloses.ContainsKey(rpc.player) || !((float)woundedCloses[rpc.player] > 2.5f))
			{
				return;
			}
			woundedCloses.Remove(rpc.player);
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (!(baseLock != null) || baseLock.OnTryToClose(rpc.player))
		{
			SetFlagLocal(Flags.Open, b: false);
			SendNetworkUpdateImmediate();
			if (isSecurityDoor && NavMeshLink != null)
			{
				SetNavMeshLinkEnabled(wantsOn: false);
			}
			Facepunch.Rust.Analytics.Azure.OnBaseInteract(rpc.player, this);
			StartCheckingForBlockages(isOpening: false);
			OnPlayerClosedDoor(rpc.player);
			Interface.CallHook("OnDoorClosed", this, rpc.player);
		}
	}

	protected virtual void OnPlayerClosedDoor(BasePlayer p)
	{
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_KnockDoor(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !knockEffect.isValid || UnityEngine.Time.realtimeSinceStartup < nextKnockTime)
		{
			return;
		}
		nextKnockTime = UnityEngine.Time.realtimeSinceStartup + 0.5f;
		BaseEntity slot = GetSlot(Slot.LowerCenterDecoration);
		if (slot != null)
		{
			DoorKnocker component = slot.GetComponent<DoorKnocker>();
			if ((bool)component)
			{
				component.Knock(rpc.player);
				return;
			}
		}
		Effect.server.Run(knockEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		Interface.CallHook("OnDoorKnocked", this, rpc.player);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_ToggleHatch(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract(usableWhileCrawling: true) || !hasHatch)
		{
			return;
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if ((bool)baseLock && !baseLock.OnTryToOpen(rpc.player))
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, !HasFlag(Flags.Reserved3));
	}

	private void EnableVehiclePhysBoxes()
	{
		ToggleVehiclePushBoxes(state: true);
	}

	private void DisableVehiclePhysBox()
	{
		ToggleVehiclePushBoxes(state: false);
	}

	private void ToggleVehiclePushBoxes(bool state)
	{
		if (vehiclePhysBoxes == null)
		{
			return;
		}
		TriggerNotify[] array = vehiclePhysBoxes;
		foreach (TriggerNotify triggerNotify in array)
		{
			if (triggerNotify != null)
			{
				triggerNotify.enabled = state;
				triggerNotify.gameObject.SetActive(state);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_NotifyWoundedOpen(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.IsWounded())
		{
			if (!woundedOpens.ContainsKey(player))
			{
				woundedOpens.Add(player, default(TimeSince));
			}
			else
			{
				woundedOpens[player] = 0f;
			}
			Invoke(delegate
			{
				CheckTimedOutPlayers(woundedOpens);
			}, 5f);
		}
	}

	private void CheckTimedOutPlayers(Dictionary<BasePlayer, TimeSince> dictionary)
	{
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		foreach (KeyValuePair<BasePlayer, TimeSince> item in dictionary)
		{
			if ((float)item.Value > 5f)
			{
				obj.Add(item.Key);
			}
		}
		foreach (BasePlayer item2 in obj)
		{
			if (dictionary.ContainsKey(item2))
			{
				dictionary.Remove(item2);
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void Server_NotifyWoundedClose(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.IsWounded())
		{
			if (!woundedCloses.ContainsKey(player))
			{
				woundedCloses.Add(player, default(TimeSince));
			}
			else
			{
				woundedCloses[player] = 0f;
			}
			Invoke(delegate
			{
				CheckTimedOutPlayers(woundedCloses);
			}, 5f);
		}
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		foreach (BaseEntity child in children)
		{
			if (child is CustomDoorManipulator customDoorManipulator)
			{
				player.GiveItem(ItemManager.CreateByItemID(customDoorManipulator.sourceItem.itemid, 1, 0uL, 0uL), GiveItemReason.PickedUp);
			}
		}
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		if (!useCastNoClipChecks)
		{
			return base.ShouldUseCastNoClipChecks();
		}
		return true;
	}

	private void ReparentDisabledExplosives()
	{
		if (children.Count == 0)
		{
			return;
		}
		using PooledList<TimedExplosive> pooledList = Facepunch.Pool.Get<PooledList<TimedExplosive>>();
		foreach (BaseEntity child in children)
		{
			if (!child.gameObject.activeInHierarchy && child is TimedExplosive item)
			{
				pooledList.Add(item);
			}
		}
		if (pooledList.Count == 0)
		{
			return;
		}
		using PooledList<Collider> pooledList2 = Facepunch.Pool.Get<PooledList<Collider>>();
		base.gameObject.GetComponentsInChildren(includeInactive: false, pooledList2);
		foreach (TimedExplosive item2 in pooledList)
		{
			Collider collider = null;
			float num = float.MaxValue;
			foreach (Collider item3 in pooledList2)
			{
				if (!item3.isTrigger && !(GameObjectEx.ToBaseEntity(item3) != this))
				{
					float num2 = item3.bounds.SqrDistance(item2.WorldSpaceBounds().position);
					if (num2 < num)
					{
						num = num2;
						collider = item3;
					}
				}
			}
			if (collider != null)
			{
				item2.SetParent(this, FindBoneID(collider.transform), worldPositionStays: true, sendImmediate: true);
			}
			else
			{
				item2.SetParent(this, worldPositionStays: true, sendImmediate: true);
			}
		}
	}

	protected virtual void ReverseDoorAnimation(bool wasOpening, bool reverse)
	{
		if (!(model == null) && !(model.animator == null))
		{
			AnimatorStateInfo currentAnimatorStateInfo = model.animator.GetCurrentAnimatorStateInfo(0);
			if (reverse)
			{
				model.animator.Play(wasOpening ? reverseCloseAnimHash : reverseOpenAnimHash, 0, 1f - currentAnimatorStateInfo.normalizedTime);
			}
			else
			{
				model.animator.Play(wasOpening ? closeHash : openHash, 0, 1f - currentAnimatorStateInfo.normalizedTime);
			}
		}
	}

	public override float AntiHackPadding()
	{
		return 2f;
	}

	protected virtual bool OnlyCheckForVehicles()
	{
		return true;
	}

	protected virtual bool InverseDotCheck()
	{
		return false;
	}

	protected virtual bool CheckOnClose()
	{
		return true;
	}

	public void OnObjects(TriggerNotify trigger)
	{
		if (!base.isServer || !trigger.isActiveAndEnabled || (!HasFlag(Flags.Open) && !CheckOnClose()))
		{
			return;
		}
		bool flag = false;
		BaseEntity baseEntity = null;
		if (!flag && (trigger.entityContents != null || trigger.entityContents.Count != 0))
		{
			foreach (BaseEntity entityContent in trigger.entityContents)
			{
				if (OnlyCheckForVehicles())
				{
					if (entityContent is BaseMountable { BlocksDoors: not false } baseMountable)
					{
						flag = true;
						baseEntity = baseMountable;
						break;
					}
					if (entityContent is BaseVehicleModule baseVehicleModule && baseVehicleModule.Vehicle != null && baseVehicleModule.Vehicle.BlocksDoors)
					{
						flag = true;
						baseEntity = baseVehicleModule.VehicleParent();
						break;
					}
					if (entityContent is BoatBuildingBlock)
					{
						flag = true;
						baseEntity = entityContent;
						break;
					}
				}
				else if (!(entityContent == null) && entityContent.IsValid() && !(entityContent == this) && !(parentEntity.Get(serverside: true) == entityContent))
				{
					flag = true;
					baseEntity = entityContent;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		bool flag2 = HasFlag(Flags.Open);
		bool flag3 = HasFlag(Flags.Reserved1);
		if (checkPhysBoxesOnOpen && !canReverseOpen && !IgnoreBlockageDotCheck)
		{
			bool flag4 = true;
			TriggerNotify[] array = vehiclePhysBoxes;
			foreach (TriggerNotify triggerNotify in array)
			{
				float num = Vector3.Dot(triggerNotify.transform.forward, (baseEntity.transform.position - triggerNotify.transform.position).normalized);
				if (InverseDotCheck() ? (num < 0f) : (num > 0f))
				{
					flag4 = false;
					break;
				}
			}
			if (flag4 == flag2 || flag4 == flag3)
			{
				return;
			}
		}
		ReverseDoorAnimation(flag2, flag3);
		SetOpen(!flag2, suppressBlockageChecks: true);
		StopCheckingForBlockages();
		ClientRPC(RpcTarget.NetworkGroup("OnDoorInterrupted"), flag2, flag3);
	}

	public void OnEmpty()
	{
	}

	protected override void ApplySubAnimationParameters(bool init, Animator toAnimator)
	{
		base.ApplySubAnimationParameters(init, toAnimator);
		if (canReverseOpen)
		{
			toAnimator.SetBool(reverseOpenHash, HasFlag(Flags.Reserved1));
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer)
		{
			BaseEntity slot = GetSlot(Slot.UpperModifier);
			if ((bool)slot)
			{
				slot.SendMessage("Think");
			}
		}
		UpdateColliderStates();
		if (base.isServer && ((old ^ next) & (Flags.Open | Flags.Busy)) != 0)
		{
			ReparentDisabledExplosives();
		}
	}

	private void UpdateColliderStates()
	{
		UpdateBusyColliderState();
		UpdateClosedColliderState();
	}

	private void UpdateBusyColliderState()
	{
		if (BusyColliderRoots == null)
		{
			return;
		}
		bool flag = HasFlag(Flags.Busy);
		GameObject[] busyColliderRoots = BusyColliderRoots;
		foreach (GameObject gameObject in busyColliderRoots)
		{
			if (gameObject != null && gameObject.gameObject.activeSelf != flag)
			{
				gameObject.gameObject.SetActive(flag);
			}
		}
	}

	private void UpdateClosedColliderState()
	{
		if (ClosedColliderRoots == null)
		{
			return;
		}
		bool flag = !HasFlag(Flags.Open) && !HasFlag(Flags.Busy);
		GameObject[] closedColliderRoots = ClosedColliderRoots;
		foreach (GameObject gameObject in closedColliderRoots)
		{
			if (gameObject != null && gameObject.gameObject.activeSelf != flag)
			{
				gameObject.gameObject.SetActive(flag);
			}
		}
	}

	public List<ItemDefinition> GetUpgradeItems()
	{
		return UpgradeItems;
	}

	public bool CanUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		if (IsOpen())
		{
			return false;
		}
		return global::SimpleUpgrade.CanUpgrade(this, upgradeItem, player);
	}

	public bool HasLock()
	{
		return GetSlot(Slot.Lock) != null;
	}

	public void DoUpgrade(BasePlayer player, ItemDefinition upgradeItem)
	{
		global::SimpleUpgrade.DoUpgrade(this, player, upgradeItem);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public void DoSimpleUpgrade(RPCMessage msg)
	{
		if (base.SecondsSinceAttacked < 30f)
		{
			msg.player.ShowToast(GameTip.Styles.Error, ConstructionErrors.CantUpgradeRecentlyDamaged, false, (30f - base.SecondsSinceAttacked).ToString("N0"));
			return;
		}
		int num = msg.read.Int32();
		if (num >= 0 && num < UpgradeItems.Count && CanUpgrade(msg.player, UpgradeItems[num]))
		{
			DoUpgrade(msg.player, UpgradeItems[num]);
		}
	}

	public bool UpgradingEnabled()
	{
		if (UpgradeItems != null)
		{
			return UpgradeItems.Count > 0;
		}
		return false;
	}

	public bool CostIsItem()
	{
		return true;
	}

	public override bool CanBeReskinned(BasePlayer player)
	{
		if (!GetPlayerLockPermission(player))
		{
			SprayCan.LastReskinError = SprayCan.NeedDoorAccess;
			return false;
		}
		if (IsOpen() || IsBusy())
		{
			SprayCan.LastReskinError = SprayCan.DoorMustBeClosed;
			return false;
		}
		if (GetParentEntity() != null && GetParentEntity() is HotAirBalloonArmor)
		{
			SprayCan.LastReskinError = SprayCan.CannotReskinThatDoor;
			return false;
		}
		return base.CanBeReskinned(player);
	}
}

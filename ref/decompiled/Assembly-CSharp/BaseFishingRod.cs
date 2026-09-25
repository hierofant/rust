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
using UnityEngine;
using UnityEngine.Assertions;

public class BaseFishingRod : HeldEntity
{
	public class UpdateFishingRod : ObjectWorkQueue<BaseFishingRod>
	{
		protected override void RunJob(BaseFishingRod entity)
		{
			if (ShouldAdd(entity))
			{
				entity.CatchProcessBudgeted();
			}
		}

		protected override bool ShouldAdd(BaseFishingRod entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public enum CatchState
	{
		None,
		Aiming,
		Waiting,
		Catching,
		Caught
	}

	[Flags]
	public enum FishState
	{
		PullingLeft = 1,
		PullingRight = 2,
		PullingBack = 4
	}

	public enum FailReason
	{
		UserRequested,
		BadAngle,
		TensionBreak,
		Unequipped,
		TimeOut,
		Success,
		NoWaterFound,
		Obstructed,
		NoLure,
		TooShallow,
		TooClose,
		TooFarAway,
		PlayerMoved
	}

	public static UpdateFishingRod updateFishingRodQueue = new UpdateFishingRod();

	public static Translate.Phrase overfishedWarningPhrase = new Translate.Phrase("overfished_warning", "This area is overfished, only junk remains.");

	private TimeUntil nextFishStateChange;

	private TimeSince fishCatchDuration;

	private float strainTimer;

	private const float strainMax = 6f;

	private TimeSince lastStrainUpdate;

	private TimeUntil catchTime;

	private TimeSince lastSightCheck;

	private Vector3 playerStartPosition;

	private WaterBody surfaceBody;

	private ItemDefinition lureUsed;

	private ItemDefinition currentFishTarget;

	private ItemModFishable fishableModifier;

	private ItemModFishable lastFish;

	public static Dictionary<Vector2, (int, TimeSince)> FishedSpots = new Dictionary<Vector2, (int, TimeSince)>();

	private bool inQueue;

	[ServerVar(Help = "(Generated) When enabled, all fishing attempts succeed immediately regardless of bite probability; cheat for testing fishing catch logic")]
	public static bool ForceSuccess = false;

	[ServerVar(Help = "(Generated) When enabled, all fishing attempts fail immediately; cheat for testing failed-catch animations and UI feedback")]
	public static bool ForceFail = false;

	[ServerVar(Help = "(Generated) When enabled, fish bite the hook immediately after casting without any wait time; cheat for testing bite response")]
	public static bool ImmediateHook = false;

	public GameObjectRef FishingBobberRef;

	public float FishCatchDistance = 0.5f;

	public LineRenderer ReelLineRenderer;

	public Transform LineRendererWorldStartPos;

	private FishState currentFishState;

	private EntityRef<FishingBobber> currentBobber;

	public float ConditionLossOnSuccess = 0.02f;

	public float ConditionLossOnFail = 0.04f;

	public float GlobalStrainSpeedMultiplier = 1f;

	public float MaxCastDistance = 10f;

	public const Flags Straining = Flags.Reserved1;

	public ItemModFishable ForceFish;

	public static Flags PullingLeftFlag = Flags.Reserved6;

	public static Flags PullingRightFlag = Flags.Reserved7;

	public static Flags ReelingInFlag = Flags.Reserved8;

	public GameObjectRef BobberPreview;

	public SoundDefinition onLineSoundDef;

	public SoundDefinition strainSoundDef;

	public AnimationCurve strainGainCurve;

	public SoundDefinition tensionBreakSoundDef;

	public GameObjectRef OverfishedAreaPrefab;

	public CatchState CurrentState { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseFishingRod.OnRpcMessage"))
		{
			if (rpc == 4237324865u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_Cancel");
				}
				using (TimeWarning.New("Server_Cancel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4237324865u, "Server_Cancel", this, player))
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
							Server_Cancel(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_Cancel");
					}
				}
				return true;
			}
			if (rpc == 2268129697u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_OverfishingCheck");
				}
				using (TimeWarning.New("Server_OverfishingCheck"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2268129697u, "Server_OverfishingCheck", this, player, 1uL))
						{
							return true;
						}
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position;
						if (!RPC_Server.IsActiveItem.Test(2268129697u, "Server_OverfishingCheck", this, player))
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
							Server_OverfishingCheck(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_OverfishingCheck");
					}
				}
				return true;
			}
			if (rpc == 4238539495u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestCast");
				}
				using (TimeWarning.New("Server_RequestCast"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position2 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position2;
						if (!RPC_Server.IsActiveItem.Test(4238539495u, "Server_RequestCast", this, player))
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
							Server_RequestCast(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in Server_RequestCast");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_RequestCast(RPCMessage msg)
	{
		Vector3 pos = msg.read.Vector3();
		BasePlayer ownerPlayer = GetOwnerPlayer();
		Item currentLure = GetCurrentLure();
		if (currentLure == null)
		{
			FailedCast(FailReason.NoLure);
			return;
		}
		if (!EvaluateFishingPosition(ref pos, ownerPlayer, out var reason, out surfaceBody))
		{
			FailedCast(reason);
			return;
		}
		object obj = Interface.CallHook("CanCastFishingRod", ownerPlayer, this, currentLure, pos);
		if (!(obj is bool) || (bool)obj)
		{
			FishingBobber component = base.gameManager.CreateEntity(FishingBobberRef.resourcePath, base.transform.position + Vector3.up * 2.8f + ownerPlayer.eyes.BodyForward() * 1.8f, GetOwnerPlayer().ServerRotation).GetComponent<FishingBobber>();
			component.transform.forward = GetOwnerPlayer().eyes.BodyForward();
			component.Spawn();
			component.InitialiseBobber(ownerPlayer, surfaceBody, pos, 150f);
			int usedLureAmount = 0;
			bool isOverfished = false;
			if (FishLookup.Instance != null)
			{
				currentFishTarget = FishLookup.Instance.GetFish(component.transform.position, surfaceBody, currentLure, out fishableModifier, lastFish, out usedLureAmount, out isOverfished);
			}
			if (isOverfished)
			{
				ownerPlayer.ShowToast(GameTip.Styles.Red_Normal, overfishedWarningPhrase, false);
			}
			lureUsed = currentLure.info;
			currentLure.UseItem(usedLureAmount);
			lastFish = fishableModifier;
			currentBobber.Set(component);
			ClientRPC(RpcTarget.NetworkGroup("Client_ReceiveCastPoint"), component.net.ID);
			ownerPlayer.SignalBroadcast(Signal.Attack);
			catchTime = (ImmediateHook ? 0f : UnityEngine.Random.Range(10f, 20f));
			catchTime = (float)catchTime * fishableModifier.CatchWaitTimeMultiplier;
			float val = (lureUsed.TryGetComponent<ItemModCompostable>(out var component2) ? component2.BaitValue : 0f);
			val = Mathx.RemapValClamped(val, 0f, 20f, 1f, 10f);
			catchTime = Mathf.Clamp((float)catchTime - val, 3f, 20f);
			playerStartPosition = ownerPlayer.transform.position;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Busy, b: true);
			}
			CurrentState = CatchState.Waiting;
			InvokeRepeating(CatchProcess, 0f, 0f);
			inQueue = false;
			BasePlayer ownerPlayer2 = GetOwnerPlayer();
			if (ownerPlayer2 != null)
			{
				Facepunch.Rust.Analytics.Azure.OnStartFish(ownerPlayer2, currentLure, pos);
			}
			Interface.CallHook("OnFishingRodCast", this, ownerPlayer, currentLure);
		}
	}

	private void FailedCast(FailReason reason)
	{
		CurrentState = CatchState.None;
		ClientRPC(RpcTarget.NetworkGroup("Client_ResetLine"), (int)reason);
	}

	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	private void Server_OverfishingCheck(RPCMessage msg)
	{
		Vector3 position = msg.read.Vector3();
		if (!Fishing.disableOverfishing && OverfishedArea.GetOverfishedAreaAtPosition(position) != null)
		{
			msg.player.ShowToast(GameTip.Styles.Red_Normal, overfishedWarningPhrase, false);
		}
	}

	private void CatchProcess()
	{
		if (!inQueue)
		{
			inQueue = true;
			updateFishingRodQueue.Add(this);
		}
	}

	private void CatchProcessBudgeted()
	{
		using FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		inQueue = false;
		FishingBobber fishingBobber = currentBobber.Get(serverside: true);
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null || ownerPlayer.IsSleeping() || ownerPlayer.IsWounded() || ownerPlayer.IsDead() || fishingBobber == null)
		{
			Server_Cancel(FailReason.UserRequested);
			return;
		}
		Vector3 position = ownerPlayer.transform.position;
		float num = Vector3.Angle((fishingBobber.transform.position.WithY(0f) - position.WithY(0f)).normalized, ownerPlayer.eyes.HeadForward().WithY(0f));
		float num2 = Vector3.Distance(position, fishingBobber.transform.position.WithY(position.y));
		if (num > ((num2 > 1.2f) ? 60f : 180f))
		{
			Server_Cancel(FailReason.BadAngle);
			return;
		}
		if (num2 > 1.2f && (float)lastSightCheck > 0.4f)
		{
			if (!GamePhysics.LineOfSight(ownerPlayer.eyes.position, fishingBobber.transform.position, 1084293377))
			{
				Server_Cancel(FailReason.Obstructed);
				return;
			}
			lastSightCheck = 0f;
		}
		if (Vector3.Distance(position, fishingBobber.transform.position) > MaxCastDistance * 2f)
		{
			Server_Cancel(FailReason.TooFarAway);
			return;
		}
		if (Vector3.Distance(playerStartPosition, position) > 1f)
		{
			Server_Cancel(FailReason.PlayerMoved);
			return;
		}
		if (CurrentState == CatchState.Waiting)
		{
			if ((float)catchTime < 0f)
			{
				ClientRPC(RpcTarget.NetworkGroup("Client_HookedSomething"));
				CurrentState = CatchState.Catching;
				using (FlagsUpdateScope flagsUpdateScope = fishingBobber.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved1, b: true);
				}
				nextFishStateChange = 0f;
				fishCatchDuration = 0f;
				strainTimer = 0f;
			}
			return;
		}
		FishState fishState = currentFishState;
		if ((float)nextFishStateChange < 0f)
		{
			float num3 = Mathx.RemapValClamped(fishingBobber.TireAmount, 0f, 20f, 0f, 1f);
			if (currentFishState != 0)
			{
				currentFishState = (FishState)0;
				nextFishStateChange = UnityEngine.Random.Range(2f, 4f) * (num3 + 1f);
			}
			else
			{
				nextFishStateChange = UnityEngine.Random.Range(3f, 7f) * (1f - num3);
				if (UnityEngine.Random.Range(0, 100) < 50)
				{
					currentFishState = FishState.PullingLeft;
				}
				else
				{
					currentFishState = FishState.PullingRight;
				}
				if (UnityEngine.Random.Range(0, 100) > 60 && Vector3.Distance(fishingBobber.transform.position, ownerPlayer.transform.position) < MaxCastDistance - 2f)
				{
					currentFishState |= FishState.PullingBack;
				}
			}
		}
		if ((float)fishCatchDuration > 120f)
		{
			Server_Cancel(FailReason.TimeOut);
			return;
		}
		bool flag = ownerPlayer.serverInput.IsDown(BUTTON.RIGHT);
		bool flag2 = ownerPlayer.serverInput.IsDown(BUTTON.LEFT);
		bool flag3 = HasReelInInput(ownerPlayer.serverInput);
		if (flag2 && flag)
		{
			flag2 = (flag = false);
		}
		UpdateFlags(flag2, flag, flag3);
		if (CurrentState == CatchState.Waiting)
		{
			flag = (flag2 = (flag3 = false));
		}
		if (flag2 && !AllowPullInDirection(-ownerPlayer.eyes.HeadRight(), fishingBobber.transform.position))
		{
			flag2 = false;
		}
		if (flag && !AllowPullInDirection(ownerPlayer.eyes.HeadRight(), fishingBobber.transform.position))
		{
			flag = false;
		}
		float value = ownerPlayer.modifiers.GetValue(Modifier.ModifierType.FishingBoost, 1f);
		fishingBobber.ServerMovementUpdate(flag2, flag, flag3, ref currentFishState, position, fishableModifier, value);
		bool flag4 = false;
		float num4 = 0f;
		if (flag3 || flag2 || flag)
		{
			flag4 = true;
			num4 = 0.5f;
		}
		if (currentFishState != 0 && flag4)
		{
			if (currentFishState.Contains(FishState.PullingBack) && flag3)
			{
				num4 = 1.5f;
			}
			else if ((currentFishState.Contains(FishState.PullingLeft) || currentFishState.Contains(FishState.PullingRight)) && flag3)
			{
				num4 = 1.2f;
			}
			else if (currentFishState.Contains(FishState.PullingLeft) && flag)
			{
				num4 = 0.8f;
			}
			else if (currentFishState.Contains(FishState.PullingRight) && flag2)
			{
				num4 = 0.8f;
			}
		}
		if (flag3 && currentFishState != 0)
		{
			num4 += 1f;
		}
		num4 *= fishableModifier.StrainModifier * GlobalStrainSpeedMultiplier;
		num4 -= ownerPlayer.modifiers.GetValue(Modifier.ModifierType.FishingBoost, 1f) - 1f;
		if (flag4)
		{
			strainTimer += UnityEngine.Time.deltaTime * num4;
		}
		else
		{
			strainTimer = Mathf.MoveTowards(strainTimer, 0f, UnityEngine.Time.deltaTime * 1.5f);
		}
		float num5 = strainTimer / 6f;
		flagsUpdateScope2.Set(Flags.Reserved1, flag4 && num5 > 0.25f);
		if ((float)lastStrainUpdate > 0.4f || fishState != currentFishState)
		{
			ClientRPC(RpcTarget.NetworkGroup("Client_UpdateFishState"), (int)currentFishState, num5);
			lastStrainUpdate = 0f;
		}
		if (strainTimer > 7f || ForceFail)
		{
			Server_Cancel(FailReason.TensionBreak);
		}
		else
		{
			if (!(num2 <= FishCatchDistance) && !ForceSuccess)
			{
				return;
			}
			CurrentState = CatchState.Caught;
			if (currentFishTarget != null)
			{
				Item item = ItemManager.Create(currentFishTarget, 1, 0uL, isServerSide: true, 0uL);
				item.SetItemOwnership(ownerPlayer, ItemOwnershipPhrases.Fishing);
				object obj = Interface.CallHook("CanCatchFish", ownerPlayer, this, item);
				if (obj is bool && !(bool)obj)
				{
					return;
				}
				object obj2 = Interface.CallHook("OnFishCatch", item, this, ownerPlayer);
				if (obj2 is Item && obj2 as Item != item)
				{
					item.Remove();
					item = (Item)obj2;
				}
				ownerPlayer.GiveItem(item, GiveItemReason.Crafted);
				if (currentFishTarget.shortname == "skull.human")
				{
					item.name = RandomUsernames.Get(UnityEngine.Random.Range(0, 1000));
				}
				if (Rust.GameInfo.HasAchievements && !string.IsNullOrEmpty(fishableModifier.SteamStatName))
				{
					ownerPlayer.stats.Add(fishableModifier.SteamStatName, 1);
					ownerPlayer.stats.Save(forceSteamSave: true);
					FishLookup.Instance.CheckCatchAllAchievement(ownerPlayer);
				}
				Facepunch.Rust.Analytics.Azure.OnCaughtFish(ownerPlayer, item);
				if (!Fishing.disableOverfishing && currentFishTarget.TryGetComponent<ItemModFishable>(out var component) && !component.IsJunk)
				{
					FishArea(fishingBobber.transform.position);
				}
				ClientRPC(RpcTarget.NetworkGroup("Client_OnCaughtFish"), currentFishTarget.itemid);
			}
			ownerPlayer.SignalBroadcast(Signal.Alt_Attack);
			Invoke(ResetLine, 6f);
			fishingBobber.Kill();
			currentBobber.Set(null);
			Interface.CallHook("OnFishCaught", currentFishTarget, this, ownerPlayer);
			CancelInvoke(CatchProcess);
		}
	}

	private void ResetLine()
	{
		Server_Cancel(FailReason.Success);
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void Server_Cancel(RPCMessage msg)
	{
		if (CurrentState != CatchState.Caught)
		{
			Server_Cancel(FailReason.UserRequested);
		}
	}

	private void Server_Cancel(FailReason reason)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			if (GetItem() != null)
			{
				GetItem().LoseCondition((reason == FailReason.Success) ? ConditionLossOnSuccess : ConditionLossOnFail);
			}
			flagsUpdateScope.Set(Flags.Busy, b: false);
			UpdateFlags();
			CancelInvoke(CatchProcess);
			CurrentState = CatchState.None;
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
			FishingBobber fishingBobber = currentBobber.Get(serverside: true);
			if (fishingBobber != null)
			{
				fishingBobber.Kill();
				currentBobber.Set(null);
			}
			ClientRPC(RpcTarget.NetworkGroup("Client_ResetLine"), (int)reason);
			BasePlayer ownerPlayer = GetOwnerPlayer();
			if (ownerPlayer != null)
			{
				Facepunch.Rust.Analytics.Azure.OnFailedFish(ownerPlayer, reason);
			}
		}
		Interface.CallHook("OnFishingStopped", this, reason);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (CurrentState != 0)
		{
			Server_Cancel(FailReason.Unequipped);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (currentBobber.IsSet && info.msg.simpleUID == null)
		{
			info.msg.simpleUID = Facepunch.Pool.Get<SimpleUID>();
			info.msg.simpleUID.uid = currentBobber.uid;
		}
	}

	private void UpdateFlags(bool inputLeft = false, bool inputRight = false, bool back = false)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local);
		flagsUpdateScope.Set(PullingLeftFlag, CurrentState == CatchState.Catching && inputLeft);
		flagsUpdateScope.Set(PullingRightFlag, CurrentState == CatchState.Catching && inputRight);
		flagsUpdateScope.Set(ReelingInFlag, CurrentState == CatchState.Catching && back);
	}

	public void FishArea(Vector3 position)
	{
		if (Fishing.disableOverfishing)
		{
			return;
		}
		if (OverfishedArea.GetOverfishedAreaAtPosition(position) != null)
		{
			DebugOverfishing($"PROCESS FISH AREA | Position: ({position}) is already overfished.", this);
			return;
		}
		Vector2 vector = new Vector2(Mathf.FloorToInt(position.x / Fishing.overfishedAreaRadius), Mathf.FloorToInt(position.z / Fishing.overfishedAreaRadius));
		if (FishedSpots.ContainsKey(vector))
		{
			float num = Fishing.overfishedAreaDurationMinutes * 60f;
			TimeSince timeSince;
			int num2;
			(num2, timeSince) = FishedSpots[vector];
			if ((float)timeSince >= num)
			{
				num2 = 1;
				DebugOverfishing("PROCESS FISH AREA | Area's timer has expired, resetting back to 1", this);
			}
			else if ((float)timeSince >= num / 2f)
			{
				num2 = Mathf.FloorToInt((float)num2 / 2f) + 1;
				DebugOverfishing("PROCESS FISH AREA | Area's timer has halfway expired, halving the counter and adding 1", this);
			}
			else
			{
				num2++;
			}
			FishedSpots[vector] = (num2, 0f);
			DebugOverfishing($"PROCESS FISH AREA | SubdividedPosition: ({vector}) exists, incrementing counter to: {FishedSpots[vector]}", this);
		}
		else
		{
			FishedSpots.Add(vector, (1, 0f));
			DebugOverfishing($"PROCESS FISH AREA | SubdividedPosition: ({vector}) does not exist yet, adding counter, set to: 1", this);
		}
		if (FishedSpots[vector].Item1 >= Fishing.successesUntilOverfished)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(OverfishedAreaPrefab.resourcePath, position, Quaternion.identity);
			baseEntity.Spawn();
			FishedSpots.Remove(vector);
			DebugOverfishing($"PROCESS FISH AREA | SubdividedPosition: ({vector}) is now overfished. Creating Overfished Area at position: {position}", baseEntity);
		}
	}

	private void DebugOverfishing(string message, UnityEngine.Object context = null)
	{
		if (Fishing.debugOverfishing)
		{
			Debug.Log(message, context);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if ((!base.isServer || !info.fromDisk) && info.msg.simpleUID != null)
		{
			currentBobber.uid = info.msg.simpleUID.uid;
		}
	}

	public override bool BlocksGestures()
	{
		return CurrentState != CatchState.None;
	}

	private bool AllowPullInDirection(Vector3 worldDirection, Vector3 bobberPosition)
	{
		Vector3 position = base.transform.position;
		Vector3 vector = bobberPosition.WithY(position.y);
		return Vector3.Dot(worldDirection, (vector - position).normalized) < 0f;
	}

	private bool EvaluateFishingPosition(ref Vector3 pos, BasePlayer ply, out FailReason reason, out WaterBody waterBody)
	{
		RaycastHit hitInfo;
		bool num = GamePhysics.Trace(new Ray(pos + Vector3.up, Vector3.down), 0f, out hitInfo, 1.5f, 16);
		if (num)
		{
			waterBody = RaycastHitEx.GetWaterBody(hitInfo);
			pos.y = hitInfo.point.y;
		}
		else
		{
			waterBody = null;
		}
		if (!num)
		{
			reason = FailReason.NoWaterFound;
			return false;
		}
		if (Vector3.Distance(ply.transform.position.WithY(pos.y), pos) < 5f)
		{
			reason = FailReason.TooClose;
			return false;
		}
		if (!GamePhysics.LineOfSight(ply.eyes.position, pos, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		Vector3 p = pos + Vector3.up * 2f;
		if (!GamePhysics.LineOfSight(ply.eyes.position, p, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		Vector3 position = ply.transform.position;
		position.y = pos.y;
		float num2 = Vector3.Distance(pos, position);
		Vector3 p2 = pos + (position - pos).normalized * (num2 - FishCatchDistance);
		if (!GamePhysics.LineOfSight(pos, p2, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		if (WaterLevel.GetOverallWaterDepth(Vector3.Lerp(pos, ply.transform.position.WithY(pos.y), 0.95f), waves: true, volumes: true) < 0.1f && ply.eyes.position.y > 0f)
		{
			reason = FailReason.TooShallow;
			return false;
		}
		if (WaterLevel.GetOverallWaterDepth(pos, waves: true, volumes: true) < 0.3f && ply.eyes.position.y > 0f)
		{
			reason = FailReason.TooShallow;
			return false;
		}
		Vector3 p3 = Vector3.MoveTowards(ply.transform.position.WithY(pos.y), pos, 1f);
		if (!GamePhysics.LineOfSight(ply.eyes.position, p3, 1218652417))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		if (GamePhysics.CheckSphere(pos, 0.25f, 153092352))
		{
			reason = FailReason.Obstructed;
			return false;
		}
		reason = FailReason.Success;
		return true;
	}

	private Item GetCurrentLure()
	{
		if (GetItem() == null)
		{
			return null;
		}
		if (GetItem().contents == null)
		{
			return null;
		}
		return GetItem().contents.GetSlot(0);
	}

	private bool HasReelInInput(InputState state)
	{
		if (!state.IsDown(BUTTON.BACKWARD))
		{
			return state.IsDown(BUTTON.FIRE_PRIMARY);
		}
		return true;
	}
}

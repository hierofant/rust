#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class HackableLockedCrate : LootContainer
{
	public const Flags Flag_Hacking = Flags.Reserved1;

	public const Flags Flag_FullyHacked = Flags.Reserved3;

	public const Flags Flag_HasBeenLooted = Flags.Reserved4;

	public Text timerText;

	[ServerVar(Help = "How many seconds for the crate to unlock")]
	public static float requiredHackSeconds = 900f;

	[ServerVar(Help = "How many seconds until the crate is destroyed without any hack attempts")]
	public static float decaySeconds = 7200f;

	public SoundPlayer hackProgressBeep;

	public float hackSeconds;

	public GameObjectRef shockEffect;

	public GameObjectRef mapMarkerEntityPrefab;

	public GameObjectRef landEffect;

	public bool shouldDecay = true;

	public bool shouldParent = true;

	public string achievementStartHacking;

	public ulong originalHackerPlayerId;

	public BaseEntity mapMarkerInstance;

	public bool hasLanded;

	public bool wasDropped;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("HackableLockedCrate.OnRpcMessage"))
		{
			if (rpc == 888500940 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Hack");
				}
				using (TimeWarning.New("RPC_Hack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(888500940u, "RPC_Hack", this, player, 3f))
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
							RPC_Hack(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Hack");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsBeingHacked()
	{
		return HasFlag(Flags.Reserved1);
	}

	public bool IsFullyHacked()
	{
		return HasFlag(Flags.Reserved3);
	}

	public new bool HasBeenLooted()
	{
		return HasFlag(Flags.Reserved4);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (base.isServer && info.forDisk)
		{
			info.msg.hackableLockedCrate = Facepunch.Pool.Get<ProtoBuf.HackableLockedCrate>();
			info.msg.hackableLockedCrate.hackSeconds = hackSeconds;
			info.msg.hackableLockedCrate.originalHackerPlayerId = originalHackerPlayerId;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (base.isServer && info.fromDisk && info.msg.hackableLockedCrate != null)
		{
			hackSeconds = info.msg.hackableLockedCrate.hackSeconds;
			originalHackerPlayerId = info.msg.hackableLockedCrate.originalHackerPlayerId;
		}
	}

	public override void DestroyShared()
	{
		if (base.isServer && (bool)mapMarkerInstance)
		{
			mapMarkerInstance.Kill();
		}
		base.DestroyShared();
	}

	public void CreateMapMarker(float durationMinutes)
	{
		if (mapMarkerEntityPrefab.isValid)
		{
			if ((bool)mapMarkerInstance)
			{
				mapMarkerInstance.Kill();
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(mapMarkerEntityPrefab.resourcePath, base.transform.position, Quaternion.identity);
			if (shouldParent)
			{
				baseEntity.SetParent(this);
				baseEntity.transform.localPosition = Vector3.zero;
			}
			baseEntity.Spawn();
			baseEntity.SendNetworkUpdate();
			mapMarkerInstance = baseEntity;
		}
	}

	public void RefreshDecay()
	{
		CancelInvoke(DelayedDestroy);
		if (shouldDecay)
		{
			Invoke(DelayedDestroy, decaySeconds);
		}
	}

	public void DelayedDestroy()
	{
		Kill();
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer)
		{
			if (StringPool.Get(info.HitBone) == "laptopcollision")
			{
				if (Interface.CallHook("OnCrateLaptopAttack", this, info) != null)
				{
					return;
				}
				Effect.server.Run(shockEffect.resourcePath, this, info.HitBone, info.HitPositionLocal, Vector3.up);
				hackSeconds -= 8f * (info.damageTypes.Total() / 50f);
				if (hackSeconds < 0f)
				{
					hackSeconds = 0f;
				}
			}
			RefreshDecay();
		}
		base.OnAttacked(info);
	}

	public void SetWasDropped()
	{
		wasDropped = true;
		Interface.CallHook("OnCrateDropped", this);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (base.isServer && !Rust.Application.isLoadingSave)
		{
			Init();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Init();
	}

	public void Init()
	{
		if (wasDropped)
		{
			InvokeRepeating(LandCheck, 0f, 0.015f);
		}
		if (IsBeingHacked())
		{
			InvokeRepeating(HackProgress, 1f, 1f);
		}
		if (!HasBeenLooted())
		{
			CreateMapMarker(120f);
		}
		RefreshDecay();
		isLootable = IsFullyHacked();
		base.inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		if (!added && mapMarkerInstance != null)
		{
			mapMarkerInstance.Kill();
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.Local);
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
		}
		base.OnItemAddedOrRemoved(item, added);
	}

	public void LandCheck()
	{
		if (!hasLanded && UnityEngine.Physics.Raycast(new Ray(base.transform.position + Vector3.up * 0.5f, Vector3.down), out var hitInfo, 1f, 1084293377))
		{
			Effect.server.Run(landEffect.resourcePath, hitInfo.point, Vector3.up);
			hasLanded = true;
			Interface.CallHook("OnCrateLanded", this);
			CancelInvoke(LandCheck);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Hack(RPCMessage msg)
	{
		if (!IsBeingHacked() && Interface.CallHook("CanHackCrate", msg.player, this) == null)
		{
			Facepunch.Rust.Analytics.Azure.OnLockedCrateStarted(msg.player, this);
			originalHackerPlayerId = msg.player.userID;
			if (!string.IsNullOrEmpty(achievementStartHacking))
			{
				msg.player?.GiveAchievement(achievementStartHacking);
			}
			StartHacking();
		}
	}

	public void StartHacking()
	{
		Interface.CallHook("OnCrateHack", this);
		BroadcastEntityMessage("HackingStarted", 20f, 257);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		InvokeRepeating(HackProgress, 1f, 1f);
		ClientRPC(RpcTarget.NetworkGroup("UpdateHackProgress"), 0, (int)requiredHackSeconds);
		RefreshDecay();
	}

	public void HackProgress()
	{
		hackSeconds += 1f;
		if (hackSeconds > requiredHackSeconds)
		{
			Interface.CallHook("OnCrateHackEnd", this);
			Facepunch.Rust.Analytics.Azure.OnLockedCrateFinished(originalHackerPlayerId, this);
			BasePlayer basePlayer = BasePlayer.FindByID(originalHackerPlayerId);
			if (basePlayer != null && basePlayer.serverClan != null)
			{
				basePlayer.AddClanScore(ClanScoreEventType.HackedCrate);
			}
			RefreshDecay();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved3, b: true);
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
			}
			isLootable = true;
			CancelInvoke(HackProgress);
		}
		ClientRPC(RpcTarget.NetworkGroup("UpdateHackProgress"), (int)hackSeconds, (int)requiredHackSeconds);
	}

	public override bool OnStartBeingLooted(BasePlayer player)
	{
		bool num = base.OnStartBeingLooted(player);
		if (num && !HasBeenLooted())
		{
			player.AddClanScore(ClanScoreEventType.OpenedHackedCrate);
		}
		return num;
	}
}

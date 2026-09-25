#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BasePlayerJobs;
using CompanionServer;
using ConVar;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices.AntiCheatCommon;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Facepunch.Models;
using Facepunch.Rust;
using JetBrains.Annotations;
using Network;
using Network.Relay;
using Network.Visibility;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using ProtoBuf;
using ProtoBuf.Nexus;
using Rust;
using Rust.Ai.Gen2;
using Rust.Ai.Gen2.Nav;
using Rust.Safety;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;
using UtilityJobs;

public class BasePlayer : BaseCombatEntity, LootPanel.IHasLootPanel, IIdealSlotEntity, IInventoryProvider, PlayerInventory.ICanMoveFrom, IReceiveDeepSeaNotifications, ISplashable, IMedicalToolTarget
{
	private struct NavDrawTile
	{
		public int navId;

		public Vector2Int coord;

		public RustNavmesh navmesh;

		public Matrix4x4? transform;

		public Vector3 worldCenter;

		public bool alwaysResend;
	}

	public enum CameraMode
	{
		FirstPerson = 0,
		ThirdPerson = 1,
		Eyes = 2,
		FirstPersonWithArms = 3,
		DeathCamClassic = 4,
		LastCyclableMode = 3
	}

	public enum NetworkQueue
	{
		Update,
		UpdateDistance,
		Count
	}

	private class NetworkQueueList
	{
		public HashSet<BaseNetworkable> queueInternal = new HashSet<BaseNetworkable>();

		public int MaxLength;

		public int Length => queueInternal.Count;

		public bool Contains(BaseNetworkable ent)
		{
			return queueInternal.Contains(ent);
		}

		public void Add(BaseNetworkable ent)
		{
			if (!Contains(ent))
			{
				queueInternal.Add(ent);
			}
			MaxLength = Mathf.Max(MaxLength, queueInternal.Count);
		}

		public void Add(BaseNetworkable[] ent)
		{
			foreach (BaseNetworkable ent2 in ent)
			{
				Add(ent2);
			}
		}

		public void Clear(Group group)
		{
			using (TimeWarning.New("NetworkQueueList.Clear"))
			{
				if (group != null)
				{
					if (group.isGlobal)
					{
						return;
					}
					List<BaseNetworkable> obj = Facepunch.Pool.Get<List<BaseNetworkable>>();
					foreach (BaseNetworkable item in queueInternal)
					{
						if (item == null || item.net?.group == null || item.net.group == group)
						{
							obj.Add(item);
						}
					}
					foreach (BaseNetworkable item2 in obj)
					{
						queueInternal.Remove(item2);
					}
					Facepunch.Pool.FreeUnmanaged(ref obj);
				}
				else
				{
					queueInternal.RemoveWhere((BaseNetworkable x) => x == null || x.net?.group == null || !x.net.group.isGlobal);
				}
			}
		}
	}

	private class SendEntitySnapshots_AsyncState : Facepunch.Pool.IPooled
	{
		public BufferList<(BaseEntity from, BasePlayer to)> Pairs;

		public BufferList<NetWrite> NetWrites;

		public BufferList<int> Chains;

		public BufferList<int> ChainIndices;

		public int ChainCounter;

		void Facepunch.Pool.IPooled.EnterPool()
		{
			Debug.Assert(ChainCounter == 0 || ChainCounter == ChainIndices.Count, "Releasing before all work started!");
			Facepunch.Pool.FreeUnmanaged(ref Chains);
			Facepunch.Pool.FreeUnmanaged(ref ChainIndices);
			Facepunch.Pool.FreeUnmanaged(ref NetWrites);
		}

		void Facepunch.Pool.IPooled.LeavePool()
		{
			NetWrites = Facepunch.Pool.Get<BufferList<NetWrite>>();
			Chains = Facepunch.Pool.Get<BufferList<int>>();
			ChainIndices = Facepunch.Pool.Get<BufferList<int>>();
			ChainCounter = 0;
		}
	}

	[Flags]
	public enum PlayerFlags
	{
		Unused1 = 1,
		CombatZone = 2,
		IsAdmin = 4,
		ReceivingSnapshot = 8,
		Sleeping = 0x10,
		Spectating = 0x20,
		Wounded = 0x40,
		IsDeveloper = 0x80,
		Connected = 0x100,
		ThirdPersonViewmode = 0x400,
		EyesViewmode = 0x800,
		ChatMute = 0x1000,
		NoSprint = 0x2000,
		Aiming = 0x4000,
		DisplaySash = 0x8000,
		Relaxed = 0x10000,
		SafeZone = 0x20000,
		ServerFall = 0x40000,
		Incapacitated = 0x80000,
		Workbench1 = 0x100000,
		Workbench2 = 0x200000,
		Workbench3 = 0x400000,
		VoiceRangeBoost = 0x800000,
		ModifyClan = 0x1000000,
		LoadingAfterTransfer = 0x2000000,
		NoRespawnZone = 0x4000000,
		IsInTutorial = 0x8000000,
		IsRestrained = 0x10000000,
		CreativeMode = 0x20000000,
		WaitingForGestureInteraction = 0x40000000,
		Ragdolling = int.MinValue
	}

	public enum FogMode
	{
		Mainland = 1,
		DeepSea = 0x20
	}

	private enum RPSWinState
	{
		Win,
		Loss,
		Draw
	}

	public static class GestureIds
	{
		public const uint FlashBlindId = 235662700u;
	}

	public enum GestureStartSource
	{
		ServerAction,
		Player
	}

	public enum MapNoteType
	{
		Death,
		PointOfInterest
	}

	public enum PingType
	{
		Hostile = 0,
		GoTo = 1,
		Dollar = 2,
		Loot = 3,
		Node = 4,
		Gun = 5,
		Build = 6,
		LAST = 6
	}

	public struct PingStyle
	{
		public int IconIndex;

		public int ColourIndex;

		public Translate.Phrase PingTitle;

		public Translate.Phrase PingDescription;

		public PingType Type;

		public PingStyle(int icon, int colour, Translate.Phrase title, Translate.Phrase desc, PingType pType)
		{
			IconIndex = icon;
			ColourIndex = colour;
			PingTitle = title;
			PingDescription = desc;
			Type = pType;
		}
	}

	[JsonModel]
	public struct FiredProjectileUpdate
	{
		public Vector3 OldPosition;

		public Vector3 NewPosition;

		public Vector3 OldVelocity;

		public Vector3 NewVelocity;

		public float Mismatch;

		public float PartialTime;
	}

	public class FiredProjectile : Facepunch.Pool.IPooled
	{
		public ItemDefinition itemDef;

		public ItemModProjectile itemMod;

		public Projectile projectilePrefab;

		public float firedTime;

		public float travelTime;

		public float partialTime;

		public AttackEntity weaponSource;

		public AttackEntity weaponPrefab;

		public Projectile.Modifier projectileModifier;

		public Item pickupItem;

		public float integrity;

		public float trajectoryMismatch;

		public float startPointMismatch;

		public float endPointMismatch;

		public float entityDistance;

		public Vector3 position;

		public Vector3 initialPositionOffset;

		public Vector3 positionOffset;

		public Vector3 velocity;

		public Vector3 initialPosition;

		public Vector3 initialVelocity;

		public Vector3 inheritedVelocity;

		public int protection;

		public int ricochets;

		public int hits;

		public BaseEntity lastEntityHit;

		public float desyncLifeTime;

		public int id;

		public BasePlayer attacker;

		public bool invalid;

		public List<FiredProjectileUpdate> updates = new List<FiredProjectileUpdate>();

		public List<Vector3> simulatedPositions = new List<Vector3>();

		public void EnterPool()
		{
			itemDef = null;
			itemMod = null;
			projectilePrefab = null;
			firedTime = 0f;
			travelTime = 0f;
			partialTime = 0f;
			weaponSource = null;
			weaponPrefab = null;
			projectileModifier = default(Projectile.Modifier);
			pickupItem = null;
			integrity = 0f;
			trajectoryMismatch = 0f;
			startPointMismatch = 0f;
			endPointMismatch = 0f;
			entityDistance = 0f;
			position = default(Vector3);
			velocity = default(Vector3);
			initialPosition = default(Vector3);
			initialVelocity = default(Vector3);
			inheritedVelocity = default(Vector3);
			protection = 0;
			ricochets = 0;
			hits = 0;
			lastEntityHit = null;
			desyncLifeTime = 0f;
			id = 0;
			attacker = null;
			invalid = false;
			updates.Clear();
			simulatedPositions.Clear();
		}

		public void LeavePool()
		{
		}
	}

	public enum TimeCategory
	{
		Wilderness = 1,
		Monument = 2,
		Base = 4,
		Flying = 8,
		Boating = 0x10,
		Swimming = 0x20,
		Driving = 0x40
	}

	public class LifeStoryWorkQueue : ObjectWorkQueue<BasePlayer>
	{
		protected override void RunJob(BasePlayer entity)
		{
			entity.UpdateTimeCategory();
		}

		protected override bool ShouldAdd(BasePlayer entity)
		{
			if (base.ShouldAdd(entity))
			{
				return entity.IsValid();
			}
			return false;
		}
	}

	public class SpawnPoint
	{
		public Vector3 pos;

		public Quaternion rot;

		public bool isProcedualSpawn;
	}

	internal struct DeathBlow
	{
		public BaseEntity Initiator;

		public BaseEntity WeaponPrefab;

		public uint HitBone;

		public bool IsValid;

		public static void From(HitInfo hitInfo, out DeathBlow deathBlow)
		{
			deathBlow = default(DeathBlow);
			deathBlow.IsValid = hitInfo != null;
			if (deathBlow.IsValid)
			{
				deathBlow.Initiator = hitInfo.Initiator;
				deathBlow.WeaponPrefab = hitInfo.WeaponPrefab;
				deathBlow.HitBone = hitInfo.HitBone;
			}
			else
			{
				deathBlow.IsValid = false;
				deathBlow.Initiator = null;
				deathBlow.WeaponPrefab = null;
			}
		}

		public static void Reset(ref DeathBlow deathBlow)
		{
			deathBlow.IsValid = false;
			deathBlow.Initiator = null;
			deathBlow.WeaponPrefab = null;
			deathBlow.HitBone = 0u;
		}
	}

	public class BotColliderWorkQueue : PersistentObjectWorkQueue<BasePlayer>
	{
		protected override void RunJob(BasePlayer entity)
		{
			entity.ServerUpdateBots(UnityEngine.Time.deltaTime);
		}

		protected override void OnRemoved(BasePlayer entity)
		{
			base.OnRemoved(entity);
			if (entity.IsSleeping())
			{
				entity.RefreshColliderSize(forced: true);
			}
		}
	}

	public class RelationshipUpdateQueue : PersistentObjectWorkQueueListBacked<BasePlayer>
	{
		public override BufferList<BasePlayer> AssignedList => activePlayerList.Values;

		protected override void RunJob(BasePlayer entity)
		{
			if (!((float)entity.lastAcquaintanceUpdate < 1f))
			{
				RelationshipManager.ServerInstance.UpdateAcquaintancesFor(entity, entity.lastAcquaintanceUpdate);
				entity.lastAcquaintanceUpdate = 0f;
			}
		}
	}

	private class OcclusionPairWorkerBuffers : Facepunch.Pool.IPooled
	{
		public BufferList<OcclusionPlayerPair> ToCheck;

		public BufferList<OcclusionPlayerPair> Found;

		public BufferList<(BasePlayer target, BasePlayer observer)> SubAdds;

		public BufferList<(ulong fromId, ulong toId)> CacheAdds;

		public void EnterPool()
		{
			Facepunch.Pool.FreeUnmanaged(ref ToCheck);
			Facepunch.Pool.FreeUnmanaged(ref Found);
			Facepunch.Pool.FreeUnmanaged(ref SubAdds);
			Facepunch.Pool.FreeUnmanaged(ref CacheAdds);
		}

		public void LeavePool()
		{
			ToCheck = Facepunch.Pool.Get<BufferList<OcclusionPlayerPair>>();
			Found = Facepunch.Pool.Get<BufferList<OcclusionPlayerPair>>();
			SubAdds = Facepunch.Pool.Get<BufferList<(BasePlayer, BasePlayer)>>();
			CacheAdds = Facepunch.Pool.Get<BufferList<(ulong, ulong)>>();
		}
	}

	public struct OcclusionPlayerPair
	{
		public BasePlayer from;

		public BasePlayer to;

		public OcclusionLastSeenStatus lastSeenStatus;
	}

	public enum OcclusionLastSeenStatus : byte
	{
		None,
		Expired,
		Valid
	}

	public class SpectatorSubStrategy : ISubscriberStrategy, Facepunch.Pool.IPooled
	{
		public BasePlayer SpectatedPlayer { get; set; }

		public Group LastGroup { get; set; }

		public void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			if (SpectatedPlayer != null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(SpectatedPlayer.net, visible);
			}
			else if (LastGroup != null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(LastGroup, null, Network.Net.sv.visibility, visible);
			}
			else
			{
				Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(net, visible);
			}
		}

		public void GatherSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			if (SpectatedPlayer != null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(SpectatedPlayer.net, visible);
			}
			else if (LastGroup != null)
			{
				Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(LastGroup, null, Network.Net.sv.visibility, visible);
			}
			else
			{
				Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(net, visible);
			}
		}

		void Facepunch.Pool.IPooled.EnterPool()
		{
			SpectatedPlayer = null;
			LastGroup = null;
		}

		void Facepunch.Pool.IPooled.LeavePool()
		{
		}
	}

	public class SpectatedSubStrategy : ISubscriberStrategy, Facepunch.Pool.IPooled
	{
		private List<BasePlayer> spectators;

		public bool IsEmpty => spectators == null;

		public void AddSpectator(BasePlayer spectator)
		{
			spectators.Add(spectator);
		}

		public bool RemoveSpectator(BasePlayer spectator)
		{
			spectators.Remove(spectator);
			return CollectionEx.IsEmpty(spectators);
		}

		public ReadOnlySpan<BasePlayer> GetSpectators()
		{
			return spectators.ListAsReadOnlySpan();
		}

		public void GatherHighPrioSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			Network.Server.DefaultSubscriberStrategy.GatherHighPrioSubscriptions(net, visible);
			foreach (BasePlayer spectator in spectators)
			{
				spectator.net.OnSubscriptionChange();
			}
		}

		public void GatherSubscriptions(Networkable net, ListHashSet<Group> visible)
		{
			Network.Server.DefaultSubscriberStrategy.GatherSubscriptions(net, visible);
		}

		void Facepunch.Pool.IPooled.EnterPool()
		{
			Facepunch.Pool.FreeUnmanaged(ref spectators);
		}

		void Facepunch.Pool.IPooled.LeavePool()
		{
			spectators = Facepunch.Pool.Get<List<BasePlayer>>();
		}
	}

	private class NearbyStash
	{
		public StashContainer Entity;

		public float LookingAtTime;

		public NearbyStash(StashContainer stash)
		{
			Entity = stash;
			LookingAtTime = 0f;
		}
	}

	public struct CachedState
	{
		public WaterLevel.WaterInfo WaterInfo;

		public float WaterFactor;

		public bool IsSwimming;

		public Quaternion EyeRot;

		public Vector3 EyePos;

		public Vector3 Center;

		public MovementModify MovementModify;

		public PlayerFlags PlayerFlags;

		public float Health;

		public float ModifiersMovementMultiplier;

		public float ClothingMoveSpeedReduction;

		public float ClothingWaterSpeedBonus;

		public float WeaponMoveSpeedScale;

		public bool IsOnLadder;

		public static CachedState Default
		{
			get
			{
				CachedState result = default(CachedState);
				result.ModifiersMovementMultiplier = 1f;
				result.WeaponMoveSpeedScale = 1f;
				return result;
			}
		}
	}

	public struct EACTickState
	{
		public LogPlayerTickOptions TickOptions;

		public long Timestamp;
	}

	public enum PositionChange
	{
		Same,
		Valid,
		Invalid
	}

	public struct PlayerServerStates
	{
		public struct ReadOnly
		{
			public StableObjectArray<BasePlayer> PlayerCache;

			public NativeArray<Vector3>.ReadOnly PlayerLocalPos;

			public NativeArray<Vector3>.ReadOnly PlayerPos;

			public NativeArray<Vector3>.ReadOnly LastFramePlayerPos;

			public NativeArray<Quaternion>.ReadOnly PlayerLocalRots;

			public NativeArray<Quaternion>.ReadOnly PlayerRots;

			public NativeArray<WaterLevel.WaterInfo>.ReadOnly WaterInfos;

			public NativeArray<float>.ReadOnly WaterFactors;

			public NativeArray<CachedState>.ReadOnly CachedStates;

			public TickInterpolatorCache.ReadOnlyState TickCache;

			public NativeArray<ModelState.Flag>.ReadOnly PlayerModelStateFlags;

			public NativeArray<float>.ReadOnly PlayerModelStateDucking;

			public TransformAccessArray PlayerTransformsAccess;

			public NativeArray<bool>.ReadOnly IsMounted;

			public BufferList<BaseMountable> Mountables;

			public NativeArray<float>.ReadOnly TickDeltaTime;

			public NativeArray<bool>.ReadOnly TickNeedsFinalizing;
		}

		public StableObjectArray<BasePlayer> PlayerCache;

		public NativeArray<Vector3> PlayerLocalPos;

		public NativeArray<Vector3> PlayerPos;

		public NativeArray<Vector3> LastFramePlayerPos;

		public NativeArray<Quaternion> PlayerLocalRots;

		public NativeArray<Quaternion> PlayerRots;

		public NativeArray<WaterLevel.WaterInfo> WaterInfos;

		public NativeArray<float> WaterFactors;

		public NativeArray<CachedState> CachedStates;

		public TickInterpolatorCache TickCache;

		public NativeArray<ModelState.Flag> PlayerModelStateFlags;

		public NativeArray<float> PlayerModelStateDucking;

		public TransformAccessArray PlayerTransformsAccess;

		public NativeArray<bool> IsMounted;

		public BufferList<BaseMountable> Mountables;

		public NativeArray<float> TickDeltaTime;

		public NativeArray<bool> TickNeedsFinalizing;

		public ReadOnly AsReadOnly()
		{
			ReadOnly result = default(ReadOnly);
			result.PlayerCache = PlayerCache;
			result.PlayerLocalPos = PlayerLocalPos.AsReadOnly();
			result.PlayerPos = PlayerPos.AsReadOnly();
			result.LastFramePlayerPos = LastFramePlayerPos.AsReadOnly();
			result.PlayerLocalRots = PlayerLocalRots.AsReadOnly();
			result.PlayerRots = PlayerRots.AsReadOnly();
			result.WaterInfos = WaterInfos.AsReadOnly();
			result.WaterFactors = WaterFactors.AsReadOnly();
			result.CachedStates = CachedStates.AsReadOnly();
			result.TickCache = TickCache.ReadOnly;
			result.PlayerModelStateFlags = PlayerModelStateFlags.AsReadOnly();
			result.PlayerModelStateDucking = PlayerModelStateDucking.AsReadOnly();
			result.PlayerTransformsAccess = PlayerTransformsAccess;
			result.IsMounted = IsMounted.AsReadOnly();
			result.Mountables = Mountables;
			result.TickDeltaTime = TickDeltaTime.AsReadOnly();
			result.TickNeedsFinalizing = TickNeedsFinalizing.AsReadOnly();
			return result;
		}

		public void Init(int initCap = 32)
		{
			PlayerCache = new StableObjectArray<BasePlayer>(initCap);
			PlayerLocalPos = new NativeArray<Vector3>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			PlayerPos = new NativeArray<Vector3>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			LastFramePlayerPos = new NativeArray<Vector3>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			PlayerLocalRots = new NativeArray<Quaternion>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			PlayerRots = new NativeArray<Quaternion>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			WaterInfos = new NativeArray<WaterLevel.WaterInfo>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			WaterFactors = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			CachedStates = new NativeArray<CachedState>(initCap, Allocator.Persistent);
			TickCache = new TickInterpolatorCache(initCap);
			PlayerModelStateFlags = new NativeArray<ModelState.Flag>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			PlayerModelStateDucking = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			PlayerTransformsAccess = new TransformAccessArray(initCap);
			IsMounted = new NativeArray<bool>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			Mountables = new BufferList<BaseMountable>(initCap);
			TickDeltaTime = new NativeArray<float>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			TickNeedsFinalizing = new NativeArray<bool>(initCap, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}

		public void SafeDispose()
		{
			PlayerCache?.Dispose();
			PlayerCache = null;
			NativeArrayEx.SafeDispose(ref PlayerLocalPos);
			NativeArrayEx.SafeDispose(ref PlayerPos);
			NativeArrayEx.SafeDispose(ref LastFramePlayerPos);
			NativeArrayEx.SafeDispose(ref PlayerLocalRots);
			NativeArrayEx.SafeDispose(ref PlayerRots);
			NativeArrayEx.SafeDispose(ref WaterInfos);
			NativeArrayEx.SafeDispose(ref WaterFactors);
			NativeArrayEx.SafeDispose(ref CachedStates);
			TickCache?.Dispose();
			NativeArrayEx.SafeDispose(ref PlayerModelStateFlags);
			NativeArrayEx.SafeDispose(ref PlayerModelStateDucking);
			if (PlayerTransformsAccess.isCreated)
			{
				PlayerTransformsAccess.Dispose();
			}
			NativeArrayEx.SafeDispose(ref IsMounted);
			Mountables = null;
			NativeArrayEx.SafeDispose(ref TickDeltaTime);
			NativeArrayEx.SafeDispose(ref TickNeedsFinalizing);
		}
	}

	public enum TutorialItemAllowance
	{
		AlwaysAllowed = -1,
		None = 0,
		Level1_HatchetPickaxe = 10,
		Level2_Planner = 20,
		Level3_Bag_TC_Door = 30,
		Level3_Hammer = 35,
		Level4_Spear_Fire = 40,
		Level5_PrepareForCombat = 50,
		Level6_Furnace = 60,
		Level7_WorkBench = 70,
		Level8_Kayak = 80
	}

	public enum InjureState
	{
		Normal,
		Crawling,
		Incapacitated,
		Dead
	}

	[Serializable]
	public struct CapsuleColliderInfo
	{
		public float height;

		public float radius;

		public Vector3 center;

		public CapsuleColliderInfo(float height, float radius, Vector3 center)
		{
			this.height = height;
			this.radius = radius;
			this.center = center;
		}
	}

	private HashSet<(int navId, Vector2Int coord)> navmeshSentTiles;

	private HashSet<(int navId, Vector2Int coord)> navmeshDirtyTiles;

	private int navmeshDrawTickCounter;

	[NonSerialized]
	public TimeAverageValueLookup<uint> rpcHistory = new TimeAverageValueLookup<uint>();

	public static readonly Translate.Phrase ClanInviteSuccess = new Translate.Phrase("clan.action.invite.success", "Invited {name} to your clan.");

	public static readonly Translate.Phrase ClanInviteFailure = new Translate.Phrase("clan.action.invite.failure", "Failed to invite {name} to your clan. Please wait a minute and try again.");

	public static readonly Translate.Phrase ClanInviteFull = new Translate.Phrase("clan.action.invite.full", "Cannot invite {name} to your clan because your clan is full.");

	[NonSerialized]
	public long clanId;

	[NonSerialized]
	public IClan serverClan;

	private const string CLAN_JOIN_ACHIEVEMENT = "CLAN_JOIN";

	private const string CLAN_SIZE_ACHIEVEMENT = "CLAN_SIZE";

	private const string CLAN_SCORE_ACHIEVEMENT = "CLAN_SCORE";

	private const int CLAN_MEMBER_COUNT_ACHIEVEMENT = 10;

	private bool clanSizeAchievementCheckDisabled;

	private bool clanScoreAchievementCheckDisabled;

	public ViewModel GestureViewModel;

	[NonSerialized]
	public NPCTalking activeTalkingToNpc;

	public const int MaxLootCountdownDebugRequests = 32;

	public const float drinkRange = 1.5f;

	public const float drinkMovementSpeed = 0.1f;

	[NonSerialized]
	private NetworkQueueList[] networkQueue = new NetworkQueueList[2]
	{
		new NetworkQueueList(),
		new NetworkQueueList()
	};

	[NonSerialized]
	private NetworkQueueList SnapshotQueue = new NetworkQueueList();

	private const int FogImagesCount = 16;

	private bool hasSentFogOfWar;

	public const string GestureCancelString = "cancel";

	public TimeUntil gestureFinishedTime;

	public TimeSince blockHeldInputTimer;

	public GestureConfig currentGesture;

	public static Translate.Phrase WinRPSPhrase = new Translate.Phrase("rps_win", "You win the game!");

	public static Translate.Phrase LoseRPSPhrase = new Translate.Phrase("rps_lose", "You lose the game!");

	public static Translate.Phrase DrawRPSPhrase = new Translate.Phrase("rps_draw", "The game was a draw!");

	private HashSet<NetworkableId> recentWaveTargets = new HashSet<NetworkableId>();

	public const string WAVED_PLAYERS_STAT = "waved_at_players";

	private NetworkableId rpsTarget;

	private int selectedRpsOption = -1;

	private Action _actionTimeoutGestureServer;

	private Action _actionMonitorLoopingGesture;

	private Action _actionBotRPSRandomise;

	private Action _actionMonitorRPSGame;

	private Action _actionServer_CancelGesture;

	public const float RPSWaitTime = 10f;

	private TimeSince interactiveGestureStartTime;

	public ulong currentTeam;

	public static readonly Translate.Phrase MaxTeamSizeToast = new Translate.Phrase("maxteamsizetip", "Your team is full. Remove a member to invite another player.");

	private bool sentInstrumentTeamAchievement;

	private bool sentSummerTeamAchievement;

	private const int TEAMMATE_INSTRUMENT_COUNT_ACHIEVEMENT = 4;

	private const int TEAMMATE_SUMMER_FLOATING_COUNT_ACHIEVEMENT = 4;

	private const string TEAMMATE_INSTRUMENT_ACHIEVEMENT = "TEAM_INSTRUMENTS";

	private const string TEAMMATE_SUMMER_ACHIEVEMENT = "SUMMER_INFLATABLE";

	public static readonly Translate.Phrase ToggleOn = new Translate.Phrase("itemmodmenu.toggleon", "Toggle On");

	public static readonly Translate.Phrase ToggleOff = new Translate.Phrase("itemmodmenu.toggleoff", "Toggle Off");

	public static Translate.Phrase MarkerLimitPhrase = new Translate.Phrase("map.marker.limited", "Cannot place more than {0} markers.");

	public const int MaxMapNoteLabelLength = 10;

	public static readonly Translate.Phrase NoSpaceInInventoryPhrase = new Translate.Phrase("no_space_mission_reward", "No space for rewards in inventory, please clear some space");

	public static readonly Translate.Phrase FailedToCheckRewardsSpace = new Translate.Phrase("failed_check_rewards_space", "Failed to get required rewards space");

	private bool _missionsDirty;

	private Action _actionAssignFollowUpMission;

	[NonSerialized]
	public BufferList<BaseMission.MissionInstance> acceptedMissions = new BufferList<BaseMission.MissionInstance>();

	private int _activeMissionIndex = -1;

	private float timeSinceServerMissionThink;

	private BaseMission followupMission;

	private IMissionProvider followupMissionProvider;

	[NonSerialized]
	public ModelState modelState = new ModelState();

	private ModelState lastModelState;

	[NonSerialized]
	public EntityRef mounted;

	public float nextSeatSwapTime;

	public BaseEntity PetEntity;

	[NonSerialized]
	public IPet Pet;

	private float lastPetCommandIssuedTime;

	private static readonly Translate.Phrase HostileTitle = new Translate.Phrase("ping_hostile", "Hostile");

	private static readonly Translate.Phrase HostileDesc = new Translate.Phrase("ping_hostile_desc", "Danger in area");

	private static readonly PingStyle HostileMarker = new PingStyle(4, 3, HostileTitle, HostileDesc, PingType.Hostile);

	private static readonly Translate.Phrase GoToTitle = new Translate.Phrase("ping_goto", "Go To");

	private static readonly Translate.Phrase GoToDesc = new Translate.Phrase("ping_goto_desc", "Look at this");

	private static readonly PingStyle GoToMarker = new PingStyle(0, 2, GoToTitle, GoToDesc, PingType.GoTo);

	private static readonly Translate.Phrase DollarTitle = new Translate.Phrase("ping_dollar", "Value");

	private static readonly Translate.Phrase DollarDesc = new Translate.Phrase("ping_dollar_desc", "Something valuable is here");

	private static readonly PingStyle DollarMarker = new PingStyle(1, 1, DollarTitle, DollarDesc, PingType.Dollar);

	private static readonly Translate.Phrase LootTitle = new Translate.Phrase("ping_loot", "Loot");

	private static readonly Translate.Phrase LootDesc = new Translate.Phrase("ping_loot_desc", "Loot is here");

	private static readonly PingStyle LootMarker = new PingStyle(11, 0, LootTitle, LootDesc, PingType.Loot);

	private static readonly Translate.Phrase NodeTitle = new Translate.Phrase("ping_node", "Node");

	private static readonly Translate.Phrase NodeDesc = new Translate.Phrase("ping_node_desc", "An ore node is here");

	private static readonly PingStyle NodeMarker = new PingStyle(10, 4, NodeTitle, NodeDesc, PingType.Node);

	private static readonly Translate.Phrase GunTitle = new Translate.Phrase("ping_gun", "Weapon");

	private static readonly Translate.Phrase GunDesc = new Translate.Phrase("ping_weapon_desc", "A dropped weapon is here");

	private static readonly PingStyle GunMarker = new PingStyle(9, 5, GunTitle, GunDesc, PingType.Gun);

	private static readonly PingStyle BuildMarker = new PingStyle(12, 5, new Translate.Phrase(), new Translate.Phrase(), PingType.Build);

	private TimeSince lastTick;

	private List<(ItemDefinition item, PingType pingType)> tutorialDesiredResource = new List<(ItemDefinition, PingType)>();

	private List<(NetworkableId id, PingType pingType)> pingedEntities = new List<(NetworkableId, PingType)>();

	private TimeSince lastResourcePingUpdate;

	private bool _playerStateDirty;

	private string _wipeId;

	private float cachedVehicleBuildingPrivilegeTime;

	private BaseEntity cachedVehicleBuildingPrivilege;

	private bool cachedVehicleBuildingPrivilegeBlocked;

	private Vector3 cachedVehicleBuildingPrivilegePosition;

	private float cachedEntityBuildingPrivilegeTime;

	private BaseEntity cachedEntityBuildingPrivilege;

	private bool cachedEntityBuildingPrivilegeBlocked;

	private Vector3 cachedEntityBuildingPrivilegePosition;

	[NonSerialized]
	private TimeUntil mortarCooldown;

	[NonSerialized]
	public Dictionary<int, FiredProjectile> firedProjectiles = new Dictionary<int, FiredProjectile>();

	private const float radiationDamageTime = 1f;

	private const float radiationDamageThreshold = 2500f;

	private const float radiationRatioAdjustment = 0.05f;

	private const float containerCheckRadTime = 2500f;

	private const float containerRadRatioAdjustment = 0.05f;

	private Action inflictInventoryRadsAction;

	private float inventoryRads;

	private bool hasOpenedLoot;

	private List<ItemContainer> radiationCheckContainers = new List<ItemContainer>();

	private float containerRads;

	private Action inflictRadsAction;

	private Action checkRadsAction;

	private const string RagdollPath = "assets/prefabs/player/player_temp_ragdoll.prefab";

	private const int WILDERNESS = 1;

	private const int MONUMENT = 2;

	private const int BASE = 4;

	private const int FLYING = 8;

	private const int BOATING = 16;

	private const int SWIMMING = 32;

	private const int DRIVING = 64;

	[Help("How many milliseconds to budget for processing life story updates per frame")]
	[ServerVar]
	public static float lifeStoryFramebudgetms = 0.25f;

	[NonSerialized]
	public PlayerLifeStory lifeStory;

	[NonSerialized]
	public PlayerLifeStory previousLifeStory;

	public const float TimeCategoryUpdateFrequency = 7f;

	public float nextTimeCategoryUpdate;

	private bool hasSentPresenceState;

	private bool LifeStoryInWilderness;

	private bool LifeStoryInMonument;

	private bool LifeStoryInBase;

	private bool LifeStoryFlying;

	private bool LifeStoryBoating;

	private bool LifeStorySwimming;

	private bool LifeStoryDriving;

	private bool waitingForLifeStoryUpdate;

	public static LifeStoryWorkQueue lifeStoryQueue = new LifeStoryWorkQueue();

	[CanBeNull]
	private PlayerLifeStory.DeathInfo cachedOverrideDeathInfo;

	[NonSerialized]
	public PlayerStatistics stats;

	[NonSerialized]
	public GameObjectRef DeathIconOverride;

	[NonSerialized]
	public ItemId svActiveItemID;

	[NonSerialized]
	public float NextChatTime;

	[NonSerialized]
	public float nextSuicideTime;

	[NonSerialized]
	public float nextRespawnTime;

	[NonSerialized]
	public string respawnId;

	[NonSerialized]
	public float nextMuteCheckTime;

	[NonSerialized]
	public int server_paintballColor;

	[NonSerialized]
	public bool isInvisible;

	public static ListHashSet<BasePlayer> invisPlayers = new ListHashSet<BasePlayer>();

	public static ListHashSet<BasePlayer> playersRecordingClientDemos = new ListHashSet<BasePlayer>();

	private RealTimeUntil timeUntilLoadingExpires;

	public Dictionary<ulong, float> lastPlayerVisibility = new Dictionary<ulong, float>();

	public static NativeArray<IntPtr> ClientHandles;

	private byte onLadderCount;

	public Vector3 viewAngles;

	private static ulong botIdCounter = 1uL;

	private static List<ulong> freeBotIds = new List<ulong>();

	public float lastSubscriptionTick;

	public double lastPlayerTick;

	public float sleepStartTime = -1f;

	public float fallTickRate = 0.1f;

	public float lastFallTime;

	public float fallVelocity;

	private DeathBlow cachedNonSuicideHit;

	private float timeSinceLastStung;

	private float timeSinceLastStungRPC;

	public static ListHashSet<BasePlayer> activePlayerList = new ListHashSet<BasePlayer>();

	public static ListHashSet<BasePlayer> sleepingPlayerList = new ListHashSet<BasePlayer>();

	public static Dictionary<ulong, BasePlayer> activePlayerLookup = new Dictionary<ulong, BasePlayer>();

	public static Dictionary<ulong, BasePlayer> sleepingPlayerLookup = new Dictionary<ulong, BasePlayer>();

	public static ListHashSet<BasePlayer> bots = new ListHashSet<BasePlayer>();

	private readonly object[] noParameterCommandArgs = new object[0];

	private readonly object[] singleParameterCommandArgs = new object[1];

	private readonly object[] doubleParameterCommandArgs = new object[2];

	private readonly object[] tripleParameterCommandArgs = new object[3];

	private readonly object[] quadParameterCommandArgs = new object[4];

	public float cachedCraftLevel;

	public float nextCheckTime;

	private NetworkableId lastSentActiveWorkbenchId;

	private Workbench _cachedWorkbench;

	private Action _actionMonitorServerDemoRecording;

	public PersistantPlayer cachedPersistantPlayer;

	private static OceanPaths cachedOceanPaths = null;

	private static readonly Translate.Phrase TakingRestraintItemError = new Translate.Phrase("error.takingrestraintitem", "Cannot take the item keeping the player restrained!");

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds for the bot collider work queue that updates NPC physics colliders")]
	public static float botColliderFrameBudgetMs = 0.05f;

	public static BotColliderWorkQueue botColliderWorkQueue = new BotColliderWorkQueue();

	[ServerVar(Help = "(Generated) Per-frame CPU budget in milliseconds for processing the player relationship (contacts/team) update queue")]
	public static float relationshipUpdateQueueFrameBudgetMs = 0.05f;

	[ServerVar(Saved = true, Help = "(Generated) When enabled, server occlusion is taken into account when updating player relationship visibility data; saved between restarts")]
	public static bool allowRelationshipServerOcclusion = true;

	public static RelationshipUpdateQueue relationshipUpdateQueue = new RelationshipUpdateQueue();

	private TimeSince lastAcquaintanceUpdate;

	public static HashSet<(ulong fromId, ulong toId)> OcclusionFrameCache = new HashSet<(ulong, ulong)>();

	private List<Network.Connection> unoccludedSubscribers;

	private bool IsSpectatingTeamInfo;

	private TimeSince lastSpectateTeamInfoUpdate;

	public int SpectateOffset = 1000000;

	public string spectateFilter = "";

	private BasePlayer spectatingTarget;

	private TimeSince timeSinceLastWaterSplash;

	private List<NearbyStash> nearbyStashes = new List<NearbyStash>();

	public float lastUpdateTime = float.NegativeInfinity;

	public float cachedThreatLevel;

	private float hostilePauseTime = float.NegativeInfinity;

	[NonSerialized]
	public float weaponDrawnDuration;

	private TimeSince timeLastInCombatZone;

	[NonSerialized]
	public float lastTickTime;

	[NonSerialized]
	private int lastEACTickIndex;

	[NonSerialized]
	public float lastStallTime;

	[NonSerialized]
	private float stallProtectionTime;

	[NonSerialized]
	public float lastInputTime;

	[NonSerialized]
	private float tutorialKickTime;

	[NonSerialized]
	public ItemId? restraintItemId;

	[NonSerialized]
	public int ActivePlayerInd = -1;

	public PlayerTick lastReceivedTick = new PlayerTick();

	private List<IReceivePlayerTickListener> receiveTickListeners = new List<IReceivePlayerTickListener>();

	private readonly TimeAverageValue ticksPerSecond = new TimeAverageValue();

	private readonly TimeAverageValue rawTicksPerSecond = new TimeAverageValue();

	public Deque<Vector3> eyeHistory = new Deque<Vector3>(16);

	public TickHistory tickHistory = new TickHistory(16);

	public static NativeArray<EACTickState> EACTickStates;

	public static PlayerServerStates PlayerStates;

	private float startTutorialCooldown;

	public float nextUnderwearValidationTime;

	public uint lastValidUnderwearSkin;

	private bool isFemale;

	private static Comparison<BasePlayer> _displayNameComparison;

	private InjureState playerInjureState;

	public float woundedDuration;

	public float lastWoundedStartTime = float.NegativeInfinity;

	public float healingWhileCrawling;

	public bool woundedByFallDamage;

	private const float INCAPACITATED_HEALTH_MIN = 2f;

	private const float INCAPACITATED_HEALTH_MAX = 6f;

	public const int MaxBotIdRange = 10000000;

	[Header("BasePlayer")]
	public GameObjectRef fallDamageEffect;

	public GameObjectRef drownEffect;

	[InspectorFlags]
	public PlayerFlags playerFlags;

	private HiddenValue<PlayerEyes> eyesValue = Facepunch.Pool.Get<HiddenValue<PlayerEyes>>();

	private HiddenValue<PlayerInventory> inventoryValue = Facepunch.Pool.Get<HiddenValue<PlayerInventory>>();

	[NonSerialized]
	public PlayerBlueprints blueprints;

	[NonSerialized]
	public PlayerMetabolism metabolism;

	[NonSerialized]
	public PlayerModifiers modifiers;

	private HiddenValue<CapsuleCollider> colliderValue = Facepunch.Pool.Get<HiddenValue<CapsuleCollider>>();

	public PlayerBelt Belt;

	public Rigidbody playerRigidbody;

	[NonSerialized]
	public EncryptedValue<ulong> userID = 0uL;

	[NonSerialized]
	public string UserIDString;

	[NonSerialized]
	public int gamemodeteam = -1;

	[NonSerialized]
	public int reputation;

	protected string _displayName;

	public string _lastSetName;

	public const float crouchSpeed = 1.7f;

	public const float walkSpeed = 2.8f;

	public const float runSpeed = 5.5f;

	public const float crawlSpeed = 0.72f;

	public CapsuleColliderInfo playerColliderStanding;

	public CapsuleColliderInfo playerColliderDucked;

	public CapsuleColliderInfo playerColliderCrawling;

	public CapsuleColliderInfo playerColliderLyingDown;

	public ProtectionProperties cachedProtection;

	private ProtectionProperties protectionAgainstNPCs;

	public const float DuckedHeight = 1.1f;

	public const float Height = 1.8f;

	public const float Radius = 0.5f;

	public const float JumpHeight = 1.5f;

	public float nextColliderRefreshTime = -1f;

	public float weaponMoveSpeedScale = 1f;

	public bool clothingBlocksAiming;

	public float clothingMoveSpeedReduction;

	public float clothingWaterSpeedBonus;

	public float clothingAccuracyBonus;

	public bool equippingBlocked;

	public float eggVision;

	public PhoneController activeTelephone;

	public BaseEntity designingAIEntity;

	[NonSerialized]
	public IPlayer IPlayer;

	public float ViolationLevel
	{
		get
		{
			if (ActivePlayerInd != -1)
			{
				return AntiHack.PlayerStates[ActivePlayerInd].ViolationLevel;
			}
			return 0f;
		}
	}

	public Translate.Phrase LootPanelTitle => displayName;

	public bool IsReceivingSnapshot => HasPlayerFlag(PlayerFlags.ReceivingSnapshot);

	public bool IsAdmin => HasPlayerFlag(PlayerFlags.IsAdmin);

	public bool IsDeveloper => HasPlayerFlag(PlayerFlags.IsDeveloper);

	public bool IsInCreativeMode
	{
		get
		{
			if (!Creative.allUsers)
			{
				return HasPlayerFlag(PlayerFlags.CreativeMode);
			}
			return true;
		}
	}

	public bool AllSkinsLocked => GetSkinsAccessLevel() == -1;

	public bool AllSkinsUnlocked => GetSkinsAccessLevel() == 1;

	public bool DefaultSkinAccess
	{
		get
		{
			int skinsAccessLevel = GetSkinsAccessLevel();
			if (skinsAccessLevel != -1)
			{
				return skinsAccessLevel != 1;
			}
			return false;
		}
	}

	public bool IsAiming => HasPlayerFlag(PlayerFlags.Aiming);

	public bool IsFlying
	{
		get
		{
			if (modelState == null)
			{
				return false;
			}
			return modelState.flying;
		}
	}

	public bool IsConnected
	{
		get
		{
			if (base.isServer)
			{
				if (Network.Net.sv == null)
				{
					return false;
				}
				if (net == null)
				{
					return false;
				}
				if (net.connection == null)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsInTutorial => HasPlayerFlag(PlayerFlags.IsInTutorial);

	public bool IsRestrained
	{
		get
		{
			if (IsAlive())
			{
				return HasPlayerFlag(PlayerFlags.IsRestrained);
			}
			return false;
		}
	}

	public bool IsRestrainedOrSurrendering
	{
		get
		{
			if (!IsRestrained)
			{
				return CurrentGestureIsSurrendering;
			}
			return true;
		}
	}

	public bool ShouldRunFogOfWar
	{
		get
		{
			if (!ConVar.Server.fogofwar || CurrentFogMode != FogMode.Mainland)
			{
				if (ConVar.Server.deepSeaFogofwar)
				{
					return CurrentFogMode == FogMode.DeepSea;
				}
				return false;
			}
			return true;
		}
	}

	public FogMode CurrentFogMode
	{
		get
		{
			if (DeepSeaManager.IsInsideDeepSea(this))
			{
				return FogMode.DeepSea;
			}
			return FogMode.Mainland;
		}
	}

	public bool InGesture
	{
		get
		{
			if (currentGesture != null)
			{
				if (!((float)gestureFinishedTime > 0f))
				{
					return currentGesture.animationType == GestureConfig.AnimationType.Loop;
				}
				return true;
			}
			return false;
		}
	}

	private bool CurrentGestureBlocksMovement
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.movementMode == GestureConfig.MovementCapabilities.NoMovement;
			}
			return false;
		}
	}

	public bool CurrentGestureIsDance
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.actionType == GestureConfig.GestureActionType.DanceAchievement;
			}
			return false;
		}
	}

	public bool CurrentGestureIsFullBody
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.playerModelLayer == GestureConfig.PlayerModelLayer.FullBody;
			}
			return false;
		}
	}

	public bool CurrentGestureIsUpperBody
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.playerModelLayer == GestureConfig.PlayerModelLayer.UpperBody;
			}
			return false;
		}
	}

	public bool CurrentGestureIsSurrendering
	{
		get
		{
			if (InGesture)
			{
				return currentGesture.actionType == GestureConfig.GestureActionType.Surrender;
			}
			return false;
		}
	}

	private bool InGestureCancelCooldown => (float)blockHeldInputTimer < 0.5f;

	private Action actionTimeoutGestureServer => TimeoutGestureServer;

	private Action actionMonitorLoopingGesture => MonitorLoopingGesture;

	private Action actionBotRPSRandomise => BotRPSRandomise;

	private Action actionMonitorRPSGame => MonitorRPSGame;

	public Action actionServer_CancelGesture => Server_CancelGesture;

	public RelationshipManager.PlayerTeam Team
	{
		get
		{
			if (RelationshipManager.ServerInstance == null)
			{
				return null;
			}
			return RelationshipManager.ServerInstance.FindTeam(currentTeam);
		}
	}

	private bool CanUseMapMarkers
	{
		get
		{
			BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
			if (activeGameMode != null)
			{
				return activeGameMode.mapMarkers;
			}
			return true;
		}
	}

	public MapNote ServerCurrentDeathNote
	{
		get
		{
			return State.deathMarker;
		}
		set
		{
			State.deathMarker = value;
		}
	}

	private Action actionAssignFollowUpMission
	{
		get
		{
			if (_actionAssignFollowUpMission == null)
			{
				_actionAssignFollowUpMission = AssignFollowUpMission;
			}
			return _actionAssignFollowUpMission;
		}
	}

	public bool HasPendingFollowupMission => IsInvoking(actionAssignFollowUpMission);

	public ModelState modelStateTick { get; private set; }

	public bool isMounted => mounted.IsValid(base.isServer);

	public bool isMountingHidingWeapon
	{
		get
		{
			if (isMounted)
			{
				return !GetMounted().CanHoldItems();
			}
			return false;
		}
	}

	private int TotalPingCount
	{
		get
		{
			if (State.pings == null)
			{
				return 0;
			}
			return State.pings.Count;
		}
	}

	public PlayerState State
	{
		get
		{
			if ((ulong)userID == 0L)
			{
				throw new InvalidOperationException("Cannot get player state without a SteamID");
			}
			return SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(userID);
		}
	}

	public string WipeId
	{
		get
		{
			if (_wipeId == null)
			{
				_wipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(userID);
			}
			return _wipeId;
		}
	}

	public bool hasPreviousLife => previousLifeStory != null;

	public int currentTimeCategory { get; private set; }

	public virtual BaseNpc.AiStatistics.FamilyEnum Family => BaseNpc.AiStatistics.FamilyEnum.Player;

	public override float PositionTickRate
	{
		protected get
		{
			return -1f;
		}
	}

	public int DebugMapMarkerIndex { get; set; }

	public bool PlayHeavyLandingAnimation { get; set; }

	public bool requestingReputationUpdate { get; set; }

	public ServerOcclusion.Grid Chunk { get; set; }

	public ServerOcclusion.SubGrid SubGrid { get; set; }

	public Vector3 estimatedVelocity { get; private set; }

	public Vector3 estimatedVelocityClamped => Vector3.ClampMagnitude(estimatedVelocity, GetMaxSpeed());

	public float inferedSpeed
	{
		get
		{
			if (estimatedSpeed < 0.01f)
			{
				return 0f;
			}
			if (modelState.sprinting)
			{
				return 5.5f;
			}
			if (modelState.ducked)
			{
				return 1.7f;
			}
			return 2.8f;
		}
	}

	public Vector3 inferedVelocity => inferedSpeed * estimatedVelocity.normalized;

	public float estimatedSpeed { get; private set; }

	public float estimatedSpeed2D { get; private set; }

	public int secondsConnected { get; private set; }

	public float desyncTimeRaw { get; set; }

	public float desyncTimeClamped { get; set; }

	public float secondsSleeping
	{
		get
		{
			if (sleepStartTime == -1f || !IsSleeping())
			{
				return 0f;
			}
			return UnityEngine.Time.time - sleepStartTime;
		}
	}

	public static IEnumerable<BasePlayer> allPlayerList
	{
		get
		{
			HashSet<BasePlayer> set = Facepunch.Pool.Get<HashSet<BasePlayer>>();
			foreach (BasePlayer sleepingPlayer in sleepingPlayerList)
			{
				if (set.Add(sleepingPlayer))
				{
					yield return sleepingPlayer;
				}
			}
			foreach (BasePlayer activePlayer in activePlayerList)
			{
				if (set.Add(activePlayer))
				{
					yield return activePlayer;
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref set);
		}
	}

	public float currentCraftLevel
	{
		get
		{
			if (triggers == null)
			{
				_cachedWorkbench = null;
				return 0f;
			}
			if (nextCheckTime > UnityEngine.Time.realtimeSinceStartup)
			{
				return cachedCraftLevel;
			}
			_cachedWorkbench = null;
			nextCheckTime = UnityEngine.Time.realtimeSinceStartup + UnityEngine.Random.Range(0.4f, 0.5f);
			float num = 0f;
			int num2 = -1;
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerWorkbench triggerWorkbench = triggers[i] as TriggerWorkbench;
				if (triggerWorkbench == null || triggerWorkbench.parentBench == null || triggerWorkbench.parentBench.isClient || !triggerWorkbench.parentBench.IsVisible(eyes.position))
				{
					continue;
				}
				float num3 = triggerWorkbench.WorkbenchLevel();
				if (num3 > num)
				{
					num = num3;
					_cachedWorkbench = triggerWorkbench.parentBench;
					num2 = triggerWorkbench.parentBench.InstalledUpgradeCount;
				}
				else if (num3 == num)
				{
					int installedUpgradeCount = triggerWorkbench.parentBench.InstalledUpgradeCount;
					if (installedUpgradeCount > num2)
					{
						_cachedWorkbench = triggerWorkbench.parentBench;
						num2 = installedUpgradeCount;
					}
				}
			}
			cachedCraftLevel = num;
			return num;
		}
	}

	public float currentComfort
	{
		get
		{
			float num = 0f;
			if (isMounted)
			{
				num = GetMounted().GetComfort();
			}
			if (triggers != null)
			{
				for (int i = 0; i < triggers.Count; i++)
				{
					TriggerComfort triggerComfort = triggers[i] as TriggerComfort;
					if (!(triggerComfort == null))
					{
						float num2 = triggerComfort.CalculateComfort(CenterPoint(), this);
						if (num2 > num)
						{
							num = num2;
						}
					}
				}
			}
			float num3 = ((modifiers != null) ? modifiers.GetValue(Modifier.ModifierType.Comfort) : 0f);
			return num + num3;
		}
	}

	private Action actionMonitorServerDemoRecording => MonitorServerDemoRecording;

	public PersistantPlayer PersistantPlayerInfo
	{
		get
		{
			if (cachedPersistantPlayer == null)
			{
				cachedPersistantPlayer = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerInfo(userID);
			}
			return cachedPersistantPlayer;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			cachedPersistantPlayer = value;
			SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerInfo(userID, value);
		}
	}

	public bool wantsSpectate { get; set; }

	public bool IsBeingSpectated
	{
		get
		{
			if (net != null)
			{
				return net.SubStrategy is SpectatedSubStrategy;
			}
			return false;
		}
	}

	public BasePlayer SpectatingTarget => spectatingTarget;

	public TimeSince TimeSinceLastWaterSplash => timeSinceLastWaterSplash;

	public InputState serverInput { get; private set; } = new InputState();


	public float timeSinceLastTick
	{
		get
		{
			if (lastTickTime == 0f)
			{
				return 0f;
			}
			return UnityEngine.Time.time - lastTickTime;
		}
	}

	public float timeSinceLastStall
	{
		get
		{
			if (lastStallTime == 0f)
			{
				return 60f;
			}
			return UnityEngine.Time.time - lastStallTime;
		}
	}

	public float IdleTime
	{
		get
		{
			if (lastInputTime == 0f)
			{
				return 0f;
			}
			return UnityEngine.Time.time - lastInputTime;
		}
	}

	public bool isStalled
	{
		get
		{
			if (IsDead() || IsSleeping())
			{
				lastStallTime = 0f;
				return false;
			}
			if (stallProtectionTime <= 0f && timeSinceLastTick != 0f && timeSinceLastTick > ConVar.AntiHack.rpcstallthreshold)
			{
				lastStallTime = UnityEngine.Time.time;
				return true;
			}
			return false;
		}
	}

	public bool wasStalled
	{
		get
		{
			if (stallProtectionTime <= 0f)
			{
				if (!isStalled)
				{
					return timeSinceLastStall < ConVar.AntiHack.rpcstallfade;
				}
				return true;
			}
			return false;
		}
	}

	public Vector3 tickViewAngles { get; private set; }

	public Vector3 tickMouseDelta { get; private set; }

	public int tickHistoryCapacity => Mathf.Max(1, Mathf.CeilToInt((float)ticksPerSecond.Calculate() * ConVar.AntiHack.tickhistorytime));

	public Matrix4x4 tickHistoryMatrix
	{
		get
		{
			if (!base.transform.parent)
			{
				return Matrix4x4.identity;
			}
			return base.transform.parent.localToWorldMatrix;
		}
	}

	public ulong rawTickCount { get; set; }

	public static PlayerServerStates.ReadOnly PlayerReadOnlyStates => PlayerStates.AsReadOnly();

	public TutorialItemAllowance CurrentTutorialAllowance { get; private set; }

	public bool IsFemale => isFemale;

	public static Comparison<BasePlayer> DisplayNameComparison
	{
		get
		{
			if (_displayNameComparison == null)
			{
				_displayNameComparison = CompareByDisplayName;
			}
			return _displayNameComparison;
		}
	}

	public InjureState PlayerInjureState
	{
		get
		{
			return playerInjureState;
		}
		set
		{
			if (playerInjureState != value)
			{
				Facepunch.Rust.Analytics.Azure.OnPlayerChangeInjureState(this, PlayerInjureState, value);
				playerInjureState = value;
			}
		}
	}

	public float TimeSinceWoundedStarted => UnityEngine.Time.realtimeSinceStartup - lastWoundedStartTime;

	public Network.Connection Connection
	{
		get
		{
			if (net != null)
			{
				return net.connection;
			}
			return null;
		}
	}

	public bool IsBot => (ulong)userID < 10000000;

	public PlayerEyes eyes
	{
		get
		{
			if (eyesValue == null)
			{
				return null;
			}
			return eyesValue.Get();
		}
		set
		{
			eyesValue.Set(value);
		}
	}

	public PlayerInventory inventory
	{
		get
		{
			if (inventoryValue == null)
			{
				return null;
			}
			return inventoryValue.Get();
		}
	}

	public CapsuleCollider playerCollider
	{
		get
		{
			if (colliderValue == null)
			{
				return null;
			}
			return colliderValue.Get();
		}
	}

	public virtual string displayName
	{
		get
		{
			return NameHelper.Get(userID, _displayName, base.isClient);
		}
		set
		{
			if (!(_lastSetName == value))
			{
				_lastSetName = value;
				_displayName = SanitizePlayerNameString(value, userID);
			}
		}
	}

	public override TraitFlag Traits => base.Traits | TraitFlag.Human | TraitFlag.Food | TraitFlag.Meat | TraitFlag.Alive;

	public bool HasActiveTelephone => activeTelephone != null;

	public bool IsDesigningAI => designingAIEntity != null;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BasePlayer.OnRpcMessage"))
		{
			if (rpc == 935768323 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClientKeepConnectionAlive");
				}
				using (TimeWarning.New("ClientKeepConnectionAlive"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(935768323u, "ClientKeepConnectionAlive", this, player))
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
							ClientKeepConnectionAlive(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ClientKeepConnectionAlive");
					}
				}
				return true;
			}
			if (rpc == 3782818894u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClientLoadingComplete");
				}
				using (TimeWarning.New("ClientLoadingComplete"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3782818894u, "ClientLoadingComplete", this, player))
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
							ClientLoadingComplete(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ClientLoadingComplete");
					}
				}
				return true;
			}
			if (rpc == 1217424607 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - FogImageUpdate");
				}
				using (TimeWarning.New("FogImageUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1217424607u, "FogImageUpdate", this, player, 16uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1217424607u, "FogImageUpdate", this, player))
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
							FogImageUpdate(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in FogImageUpdate");
					}
				}
				return true;
			}
			if (rpc == 1497207530 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - IssuePetCommand");
				}
				using (TimeWarning.New("IssuePetCommand"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg5 = rPCMessage;
							IssuePetCommand(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in IssuePetCommand");
					}
				}
				return true;
			}
			if (rpc == 2041023702 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - IssuePetCommandRaycast");
				}
				using (TimeWarning.New("IssuePetCommandRaycast"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg6 = rPCMessage;
							IssuePetCommandRaycast(msg6);
						}
					}
					catch (Exception exception5)
					{
						Debug.LogException(exception5);
						player.Kick("RPC Error in IssuePetCommandRaycast");
					}
				}
				return true;
			}
			if (rpc == 495414158 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - NotifyDebugCameraEnded");
				}
				using (TimeWarning.New("NotifyDebugCameraEnded"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg7 = rPCMessage;
							NotifyDebugCameraEnded(msg7);
						}
					}
					catch (Exception exception6)
					{
						Debug.LogException(exception6);
						player.Kick("RPC Error in NotifyDebugCameraEnded");
					}
				}
				return true;
			}
			if (rpc == 3441821928u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnFeedbackReport");
				}
				using (TimeWarning.New("OnFeedbackReport"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3441821928u, "OnFeedbackReport", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3441821928u, "OnFeedbackReport", this, player))
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
							OnFeedbackReport(msg8);
						}
					}
					catch (Exception exception7)
					{
						Debug.LogException(exception7);
						player.Kick("RPC Error in OnFeedbackReport");
					}
				}
				return true;
			}
			if (rpc == 1998170713 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnPlayerLanded");
				}
				using (TimeWarning.New("OnPlayerLanded"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1998170713u, "OnPlayerLanded", this, player))
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
							RPCMessage msg9 = rPCMessage;
							OnPlayerLanded(msg9);
						}
					}
					catch (Exception exception8)
					{
						Debug.LogException(exception8);
						player.Kick("RPC Error in OnPlayerLanded");
					}
				}
				return true;
			}
			if (rpc == 2147041557 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnPlayerReported");
				}
				using (TimeWarning.New("OnPlayerReported"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2147041557u, "OnPlayerReported", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2147041557u, "OnPlayerReported", this, player))
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
							RPCMessage msg10 = rPCMessage;
							OnPlayerReported(msg10);
						}
					}
					catch (Exception exception9)
					{
						Debug.LogException(exception9);
						player.Kick("RPC Error in OnPlayerReported");
					}
				}
				return true;
			}
			if (rpc == 363681694 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnProjectileAttack");
				}
				using (TimeWarning.New("OnProjectileAttack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(363681694u, "OnProjectileAttack", this, player))
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
							RPCMessage msg11 = rPCMessage;
							OnProjectileAttack(msg11);
						}
					}
					catch (Exception exception10)
					{
						Debug.LogException(exception10);
						player.Kick("RPC Error in OnProjectileAttack");
					}
				}
				return true;
			}
			if (rpc == 1500391289 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnProjectileRicochet");
				}
				using (TimeWarning.New("OnProjectileRicochet"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1500391289u, "OnProjectileRicochet", this, player))
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
							RPCMessage msg12 = rPCMessage;
							OnProjectileRicochet(msg12);
						}
					}
					catch (Exception exception11)
					{
						Debug.LogException(exception11);
						player.Kick("RPC Error in OnProjectileRicochet");
					}
				}
				return true;
			}
			if (rpc == 2324190493u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - OnProjectileUpdate");
				}
				using (TimeWarning.New("OnProjectileUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2324190493u, "OnProjectileUpdate", this, player))
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
							RPCMessage msg13 = rPCMessage;
							OnProjectileUpdate(msg13);
						}
					}
					catch (Exception exception12)
					{
						Debug.LogException(exception12);
						player.Kick("RPC Error in OnProjectileUpdate");
					}
				}
				return true;
			}
			if (rpc == 3167788018u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - PerformanceReport");
				}
				using (TimeWarning.New("PerformanceReport"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3167788018u, "PerformanceReport", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3167788018u, "PerformanceReport", this, player))
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
							RPCMessage msg14 = rPCMessage;
							PerformanceReport(msg14);
						}
					}
					catch (Exception exception13)
					{
						Debug.LogException(exception13);
						player.Kick("RPC Error in PerformanceReport");
					}
				}
				return true;
			}
			if (rpc == 4081064578u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - PlayerRequestedTutorialStart");
				}
				using (TimeWarning.New("PlayerRequestedTutorialStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4081064578u, "PlayerRequestedTutorialStart", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(4081064578u, "PlayerRequestedTutorialStart", this, player))
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
							RPCMessage msg15 = rPCMessage;
							PlayerRequestedTutorialStart(msg15);
						}
					}
					catch (Exception exception14)
					{
						Debug.LogException(exception14);
						player.Kick("RPC Error in PlayerRequestedTutorialStart");
					}
				}
				return true;
			}
			if (rpc == 3227458058u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ReqLightToggle");
				}
				using (TimeWarning.New("ReqLightToggle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3227458058u, "ReqLightToggle", this, player))
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
							RPCMessage msg16 = rPCMessage;
							ReqLightToggle(msg16);
						}
					}
					catch (Exception exception15)
					{
						Debug.LogException(exception15);
						player.Kick("RPC Error in ReqLightToggle");
					}
				}
				return true;
			}
			if (rpc == 1280830738 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ReqLightToggleEntity");
				}
				using (TimeWarning.New("ReqLightToggleEntity"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1280830738u, "ReqLightToggleEntity", this, player))
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
							RPCMessage msg17 = rPCMessage;
							ReqLightToggleEntity(msg17);
						}
					}
					catch (Exception exception16)
					{
						Debug.LogException(exception16);
						player.Kick("RPC Error in ReqLightToggleEntity");
					}
				}
				return true;
			}
			if (rpc == 56793194 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestJoinGesture");
				}
				using (TimeWarning.New("RequestJoinGesture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(56793194u, "RequestJoinGesture", this, player, 3f))
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
							RPCMessage msg18 = rPCMessage;
							RequestJoinGesture(msg18);
						}
					}
					catch (Exception exception17)
					{
						Debug.LogException(exception17);
						player.Kick("RPC Error in RequestJoinGesture");
					}
				}
				return true;
			}
			if (rpc == 1024003327 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestParachuteDeploy");
				}
				using (TimeWarning.New("RequestParachuteDeploy"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1024003327u, "RequestParachuteDeploy", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1024003327u, "RequestParachuteDeploy", this, player))
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
							RPCMessage msg19 = rPCMessage;
							RequestParachuteDeploy(msg19);
						}
					}
					catch (Exception exception18)
					{
						Debug.LogException(exception18);
						player.Kick("RPC Error in RequestParachuteDeploy");
					}
				}
				return true;
			}
			if (rpc == 52352806 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestRespawnInformation");
				}
				using (TimeWarning.New("RequestRespawnInformation"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(52352806u, "RequestRespawnInformation", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(52352806u, "RequestRespawnInformation", this, player))
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
							RPCMessage msg20 = rPCMessage;
							RequestRespawnInformation(msg20);
						}
					}
					catch (Exception exception19)
					{
						Debug.LogException(exception19);
						player.Kick("RPC Error in RequestRespawnInformation");
					}
				}
				return true;
			}
			if (rpc == 1774681338 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RequestServerEmoji");
				}
				using (TimeWarning.New("RequestServerEmoji"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1774681338u, "RequestServerEmoji", this, player, 1uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RequestServerEmoji();
						}
					}
					catch (Exception exception20)
					{
						Debug.LogException(exception20);
						player.Kick("RPC Error in RequestServerEmoji");
					}
				}
				return true;
			}
			if (rpc == 970468557 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Assist");
				}
				using (TimeWarning.New("RPC_Assist"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(970468557u, "RPC_Assist", this, player, 3f))
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
							RPCMessage msg21 = rPCMessage;
							RPC_Assist(msg21);
						}
					}
					catch (Exception exception21)
					{
						Debug.LogException(exception21);
						player.Kick("RPC Error in RPC_Assist");
					}
				}
				return true;
			}
			if (rpc == 3263238541u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_KeepAlive");
				}
				using (TimeWarning.New("RPC_KeepAlive"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3263238541u, "RPC_KeepAlive", this, player, 3f))
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
							RPCMessage msg22 = rPCMessage;
							RPC_KeepAlive(msg22);
						}
					}
					catch (Exception exception22)
					{
						Debug.LogException(exception22);
						player.Kick("RPC Error in RPC_KeepAlive");
					}
				}
				return true;
			}
			if (rpc == 3692395068u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_LootPlayer");
				}
				using (TimeWarning.New("RPC_LootPlayer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3692395068u, "RPC_LootPlayer", this, player, 3f))
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
							RPCMessage msg23 = rPCMessage;
							RPC_LootPlayer(msg23);
						}
					}
					catch (Exception exception23)
					{
						Debug.LogException(exception23);
						player.Kick("RPC Error in RPC_LootPlayer");
					}
				}
				return true;
			}
			if (rpc == 2659547586u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReqDoRestrainedPush");
				}
				using (TimeWarning.New("RPC_ReqDoRestrainedPush"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2659547586u, "RPC_ReqDoRestrainedPush", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2659547586u, "RPC_ReqDoRestrainedPush", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2659547586u, "RPC_ReqDoRestrainedPush", this, player, 3f))
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
							RPC_ReqDoRestrainedPush(rpc2);
						}
					}
					catch (Exception exception24)
					{
						Debug.LogException(exception24);
						player.Kick("RPC Error in RPC_ReqDoRestrainedPush");
					}
				}
				return true;
			}
			if (rpc == 3974264977u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReqEquipHood");
				}
				using (TimeWarning.New("RPC_ReqEquipHood"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3974264977u, "RPC_ReqEquipHood", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3974264977u, "RPC_ReqEquipHood", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3974264977u, "RPC_ReqEquipHood", this, player, 3f))
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
							RPC_ReqEquipHood(rpc3);
						}
					}
					catch (Exception exception25)
					{
						Debug.LogException(exception25);
						player.Kick("RPC Error in RPC_ReqEquipHood");
					}
				}
				return true;
			}
			if (rpc == 4144905368u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReqForceMountNearest");
				}
				using (TimeWarning.New("RPC_ReqForceMountNearest"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4144905368u, "RPC_ReqForceMountNearest", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4144905368u, "RPC_ReqForceMountNearest", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(4144905368u, "RPC_ReqForceMountNearest", this, player, 3f))
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
							RPC_ReqForceMountNearest(rpc4);
						}
					}
					catch (Exception exception26)
					{
						Debug.LogException(exception26);
						player.Kick("RPC Error in RPC_ReqForceMountNearest");
					}
				}
				return true;
			}
			if (rpc == 3816898909u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReqForceSwapSeat");
				}
				using (TimeWarning.New("RPC_ReqForceSwapSeat"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3816898909u, "RPC_ReqForceSwapSeat", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3816898909u, "RPC_ReqForceSwapSeat", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(3816898909u, "RPC_ReqForceSwapSeat", this, player, 3f))
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
							RPC_ReqForceSwapSeat(rpc5);
						}
					}
					catch (Exception exception27)
					{
						Debug.LogException(exception27);
						player.Kick("RPC Error in RPC_ReqForceSwapSeat");
					}
				}
				return true;
			}
			if (rpc == 626234931 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReqRemoveCuffs");
				}
				using (TimeWarning.New("RPC_ReqRemoveCuffs"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(626234931u, "RPC_ReqRemoveCuffs", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(626234931u, "RPC_ReqRemoveCuffs", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(626234931u, "RPC_ReqRemoveCuffs", this, player, 3f))
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
							RPCMessage rpc6 = rPCMessage;
							RPC_ReqRemoveCuffs(rpc6);
						}
					}
					catch (Exception exception28)
					{
						Debug.LogException(exception28);
						player.Kick("RPC Error in RPC_ReqRemoveCuffs");
					}
				}
				return true;
			}
			if (rpc == 2289764809u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_ReqRemoveHood");
				}
				using (TimeWarning.New("RPC_ReqRemoveHood"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2289764809u, "RPC_ReqRemoveHood", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2289764809u, "RPC_ReqRemoveHood", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2289764809u, "RPC_ReqRemoveHood", this, player, 3f))
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
							RPCMessage rpc7 = rPCMessage;
							RPC_ReqRemoveHood(rpc7);
						}
					}
					catch (Exception exception29)
					{
						Debug.LogException(exception29);
						player.Kick("RPC Error in RPC_ReqRemoveHood");
					}
				}
				return true;
			}
			if (rpc == 1539133504 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_StartClimb");
				}
				using (TimeWarning.New("RPC_StartClimb"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position = msg.read.Position;
						msg.read.Read<bool>();
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<NetworkableId>();
						msg.read.Position = position;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg24 = rPCMessage;
							RPC_StartClimb(msg24);
						}
					}
					catch (Exception exception30)
					{
						Debug.LogException(exception30);
						player.Kick("RPC Error in RPC_StartClimb");
					}
				}
				return true;
			}
			if (rpc == 1777651896 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SelectedRPSOption");
				}
				using (TimeWarning.New("SelectedRPSOption"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(1777651896u, "SelectedRPSOption", this, player))
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
							RPCMessage msg25 = rPCMessage;
							SelectedRPSOption(msg25);
						}
					}
					catch (Exception exception31)
					{
						Debug.LogException(exception31);
						player.Kick("RPC Error in SelectedRPSOption");
					}
				}
				return true;
			}
			if (rpc == 3047177092u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_AddMarker");
				}
				using (TimeWarning.New("Server_AddMarker"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3047177092u, "Server_AddMarker", this, player, 8uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3047177092u, "Server_AddMarker", this, player))
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
							RPCMessage msg26 = rPCMessage;
							Server_AddMarker(msg26);
						}
					}
					catch (Exception exception32)
					{
						Debug.LogException(exception32);
						player.Kick("RPC Error in Server_AddMarker");
					}
				}
				return true;
			}
			if (rpc == 3618659425u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_AddPing");
				}
				using (TimeWarning.New("Server_AddPing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3618659425u, "Server_AddPing", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3618659425u, "Server_AddPing", this, player))
						{
							return true;
						}
						long position2 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Read<int>();
						msg.read.Read<bool>();
						msg.read.Position = position2;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg27 = rPCMessage;
							Server_AddPing(msg27);
						}
					}
					catch (Exception exception33)
					{
						Debug.LogException(exception33);
						player.Kick("RPC Error in Server_AddPing");
					}
				}
				return true;
			}
			if (rpc == 1005040107 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_CancelGesture");
				}
				using (TimeWarning.New("Server_CancelGesture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1005040107u, "Server_CancelGesture", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1005040107u, "Server_CancelGesture", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							Server_CancelGesture();
						}
					}
					catch (Exception exception34)
					{
						Debug.LogException(exception34);
						player.Kick("RPC Error in Server_CancelGesture");
					}
				}
				return true;
			}
			if (rpc == 706157120 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_ClearMapMarkers");
				}
				using (TimeWarning.New("Server_ClearMapMarkers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(706157120u, "Server_ClearMapMarkers", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(706157120u, "Server_ClearMapMarkers", this, player))
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
							RPCMessage msg28 = rPCMessage;
							Server_ClearMapMarkers(msg28);
						}
					}
					catch (Exception exception35)
					{
						Debug.LogException(exception35);
						player.Kick("RPC Error in Server_ClearMapMarkers");
					}
				}
				return true;
			}
			if (rpc == 310453544 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_ClearPointsOfInterest");
				}
				using (TimeWarning.New("Server_ClearPointsOfInterest"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(310453544u, "Server_ClearPointsOfInterest", this, player, 8uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(310453544u, "Server_ClearPointsOfInterest", this, player))
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
							RPCMessage msg29 = rPCMessage;
							Server_ClearPointsOfInterest(msg29);
						}
					}
					catch (Exception exception36)
					{
						Debug.LogException(exception36);
						player.Kick("RPC Error in Server_ClearPointsOfInterest");
					}
				}
				return true;
			}
			if (rpc == 2895394689u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_OnClientDemoRecordingStateChanged");
				}
				using (TimeWarning.New("Server_OnClientDemoRecordingStateChanged"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(2895394689u, "Server_OnClientDemoRecordingStateChanged", this, player))
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
							RPCMessage msg30 = rPCMessage;
							Server_OnClientDemoRecordingStateChanged(msg30);
						}
					}
					catch (Exception exception37)
					{
						Debug.LogException(exception37);
						player.Kick("RPC Error in Server_OnClientDemoRecordingStateChanged");
					}
				}
				return true;
			}
			if (rpc == 1032755717 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RemovePing");
				}
				using (TimeWarning.New("Server_RemovePing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1032755717u, "Server_RemovePing", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1032755717u, "Server_RemovePing", this, player))
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
							RPCMessage msg31 = rPCMessage;
							Server_RemovePing(msg31);
						}
					}
					catch (Exception exception38)
					{
						Debug.LogException(exception38);
						player.Kick("RPC Error in Server_RemovePing");
					}
				}
				return true;
			}
			if (rpc == 31713840 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RemovePointOfInterest");
				}
				using (TimeWarning.New("Server_RemovePointOfInterest"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(31713840u, "Server_RemovePointOfInterest", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(31713840u, "Server_RemovePointOfInterest", this, player))
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
							RPCMessage msg32 = rPCMessage;
							Server_RemovePointOfInterest(msg32);
						}
					}
					catch (Exception exception39)
					{
						Debug.LogException(exception39);
						player.Kick("RPC Error in Server_RemovePointOfInterest");
					}
				}
				return true;
			}
			if (rpc == 2844621823u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestLootCountdowns");
				}
				using (TimeWarning.New("Server_RequestLootCountdowns"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2844621823u, "Server_RequestLootCountdowns", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2844621823u, "Server_RequestLootCountdowns", this, player))
						{
							return true;
						}
						long position3 = msg.read.Position;
						msg.read.Position = position3;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg33 = rPCMessage;
							Server_RequestLootCountdowns(msg33);
						}
					}
					catch (Exception exception40)
					{
						Debug.LogException(exception40);
						player.Kick("RPC Error in Server_RequestLootCountdowns");
					}
				}
				return true;
			}
			if (rpc == 2567683804u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestMarkers");
				}
				using (TimeWarning.New("Server_RequestMarkers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2567683804u, "Server_RequestMarkers", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2567683804u, "Server_RequestMarkers", this, player))
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
							RPCMessage msg34 = rPCMessage;
							Server_RequestMarkers(msg34);
						}
					}
					catch (Exception exception41)
					{
						Debug.LogException(exception41);
						player.Kick("RPC Error in Server_RequestMarkers");
					}
				}
				return true;
			}
			if (rpc == 3637080058u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_RequestValidMissionsUpdate");
				}
				using (TimeWarning.New("Server_RequestValidMissionsUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3637080058u, "Server_RequestValidMissionsUpdate", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3637080058u, "Server_RequestValidMissionsUpdate", this, player))
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
							RPCMessage _ = rPCMessage;
							Server_RequestValidMissionsUpdate(_);
						}
					}
					catch (Exception exception42)
					{
						Debug.LogException(exception42);
						player.Kick("RPC Error in Server_RequestValidMissionsUpdate");
					}
				}
				return true;
			}
			if (rpc == 1572722245 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_StartGesture");
				}
				using (TimeWarning.New("Server_StartGesture"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1572722245u, "Server_StartGesture", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1572722245u, "Server_StartGesture", this, player))
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
							RPCMessage msg35 = rPCMessage;
							Server_StartGesture(msg35);
						}
					}
					catch (Exception exception43)
					{
						Debug.LogException(exception43);
						player.Kick("RPC Error in Server_StartGesture");
					}
				}
				return true;
			}
			if (rpc == 1180369886 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - Server_UpdateMarker");
				}
				using (TimeWarning.New("Server_UpdateMarker"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1180369886u, "Server_UpdateMarker", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1180369886u, "Server_UpdateMarker", this, player))
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
							RPCMessage msg36 = rPCMessage;
							Server_UpdateMarker(msg36);
						}
					}
					catch (Exception exception44)
					{
						Debug.LogException(exception44);
						player.Kick("RPC Error in Server_UpdateMarker");
					}
				}
				return true;
			}
			if (rpc == 2192544725u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerRequestEmojiData");
				}
				using (TimeWarning.New("ServerRequestEmojiData"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2192544725u, "ServerRequestEmojiData", this, player, 3uL))
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
							RPCMessage msg37 = rPCMessage;
							ServerRequestEmojiData(msg37);
						}
					}
					catch (Exception exception45)
					{
						Debug.LogException(exception45);
						player.Kick("RPC Error in ServerRequestEmojiData");
					}
				}
				return true;
			}
			if (rpc == 3635568749u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ServerRPC_UnderwearChange");
				}
				using (TimeWarning.New("ServerRPC_UnderwearChange"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg38 = rPCMessage;
							ServerRPC_UnderwearChange(msg38);
						}
					}
					catch (Exception exception46)
					{
						Debug.LogException(exception46);
						player.Kick("RPC Error in ServerRPC_UnderwearChange");
					}
				}
				return true;
			}
			if (rpc == 3222472445u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - StartTutorial");
				}
				using (TimeWarning.New("StartTutorial"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg39 = rPCMessage;
							StartTutorial(msg39);
						}
					}
					catch (Exception exception47)
					{
						Debug.LogException(exception47);
						player.Kick("RPC Error in StartTutorial");
					}
				}
				return true;
			}
			if (rpc == 970114602 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - SV_Drink");
				}
				using (TimeWarning.New("SV_Drink"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg40 = rPCMessage;
							SV_Drink(msg40);
						}
					}
					catch (Exception exception48)
					{
						Debug.LogException(exception48);
						player.Kick("RPC Error in SV_Drink");
					}
				}
				return true;
			}
			if (rpc == 1361044246 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UpdateSpectatePositionFromDebugCamera");
				}
				using (TimeWarning.New("UpdateSpectatePositionFromDebugCamera"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1361044246u, "UpdateSpectatePositionFromDebugCamera", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1361044246u, "UpdateSpectatePositionFromDebugCamera", this, player))
						{
							return true;
						}
						long position4 = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position4;
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg41 = rPCMessage;
							UpdateSpectatePositionFromDebugCamera(msg41);
						}
					}
					catch (Exception exception49)
					{
						Debug.LogException(exception49);
						player.Kick("RPC Error in UpdateSpectatePositionFromDebugCamera");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void ToggleShowFSMStateDebugInfo()
	{
		if (!IsInvoking(ShowStateDebugInfo))
		{
			InvokeRepeating(ShowStateDebugInfo, 0f, 0.1f);
		}
		else
		{
			CancelInvoke(ShowStateDebugInfo);
		}
	}

	private void ShowStateDebugInfo()
	{
		FSMComponent.ShowDebugInfoAroundLocation(this);
	}

	public void MarkNavmeshTileDirty(int tx, int ty)
	{
		if (navmeshDirtyTiles == null)
		{
			navmeshDirtyTiles = new HashSet<(int, Vector2Int)>();
		}
		navmeshDirtyTiles.Add((0, new Vector2Int(tx, ty)));
	}

	public void ResetNavmeshDrawState()
	{
		navmeshSentTiles?.Clear();
		navmeshDirtyTiles?.Clear();
		navmeshDrawTickCounter = 0;
	}

	public void DrawNavmesh()
	{
		using (TimeWarning.New("BasePlayer.DrawNavmesh"))
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return;
			}
			RustNavigation instance = RustNavigation.Instance;
			if (instance == null)
			{
				return;
			}
			RustNavmesh defaultNavmesh = instance.DefaultNavmesh;
			if (defaultNavmesh == null || !defaultNavmesh.IsValid())
			{
				return;
			}
			if (navmeshSentTiles == null)
			{
				navmeshSentTiles = new HashSet<(int, Vector2Int)>();
			}
			if (navmeshDirtyTiles == null)
			{
				navmeshDirtyTiles = new HashSet<(int, Vector2Int)>();
			}
			float drawRadius = RustNav.drawRadius;
			Bounds worldBounds = new Bounds(base.transform.position, new Vector3(drawRadius * 2f, drawRadius * 2f, drawRadius * 2f));
			Vector3 selfPos = base.transform.position;
			using PooledList<NavDrawTile> pooledList = Facepunch.Pool.Get<PooledList<NavDrawTile>>();
			GatherNavmeshTiles(defaultNavmesh, 0, null, alwaysResend: false, worldBounds, pooledList);
			using PooledList<IndependantNavmesh> pooledList2 = Facepunch.Pool.Get<PooledList<IndependantNavmesh>>();
			IndependantNavmesh.FindNavmeshesInBounds(worldBounds, pooledList2);
			foreach (IndependantNavmesh item2 in pooledList2)
			{
				RustNavmesh navmesh = item2.Navmesh;
				if (navmesh != null && navmesh.IsValid())
				{
					int drawNavId = RustNavigation.GetDrawNavId(item2);
					Matrix4x4? navToWorld = (item2.canMove ? new Matrix4x4?(item2.NavToWorldMatrix) : null);
					GatherNavmeshTiles(navmesh, drawNavId, navToWorld, item2.canMove, worldBounds, pooledList);
				}
			}
			using PooledHashSet<(int, Vector2Int)> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<(int, Vector2Int)>>();
			foreach (NavDrawTile item3 in pooledList)
			{
				pooledHashSet.Add((item3.navId, item3.coord));
			}
			navmeshDrawTickCounter++;
			if (RustNav.drawManifestInterval > 0 && navmeshDrawTickCounter % RustNav.drawManifestInterval == 0)
			{
				SendNavmeshManifest(pooledList);
			}
			using PooledList<(int, Vector2Int)> pooledList3 = Facepunch.Pool.Get<PooledList<(int, Vector2Int)>>();
			foreach (var navmeshSentTile in navmeshSentTiles)
			{
				if (!pooledHashSet.Contains(navmeshSentTile))
				{
					pooledList3.Add(navmeshSentTile);
				}
			}
			foreach (var item4 in pooledList3)
			{
				navmeshSentTiles.Remove(item4);
				navmeshDirtyTiles.Remove(item4);
			}
			using PooledList<NavDrawTile> pooledList4 = Facepunch.Pool.Get<PooledList<NavDrawTile>>();
			foreach (NavDrawTile item5 in pooledList)
			{
				(int, Vector2Int) item = (item5.navId, item5.coord);
				if (item5.alwaysResend || navmeshDirtyTiles.Contains(item) || !navmeshSentTiles.Contains(item))
				{
					pooledList4.Add(item5);
				}
			}
			if (pooledList4.Count == 0)
			{
				return;
			}
			pooledList4.Sort(delegate(NavDrawTile a, NavDrawTile b)
			{
				float sqrMagnitude = (a.worldCenter - selfPos).sqrMagnitude;
				float sqrMagnitude2 = (b.worldCenter - selfPos).sqrMagnitude;
				return sqrMagnitude.CompareTo(sqrMagnitude2);
			});
			int num = Mathf.Max(1, (int)(RustNav.drawKBps * 1024f * RustNav.drawRefreshRate));
			bool flag = false;
			foreach (NavDrawTile item6 in pooledList4)
			{
				if (!(!item6.alwaysResend && flag) || num > 0)
				{
					int num2 = SendNavmeshTile(item6);
					navmeshSentTiles.Add((item6.navId, item6.coord));
					navmeshDirtyTiles.Remove((item6.navId, item6.coord));
					if (!item6.alwaysResend)
					{
						num -= num2;
						flag = true;
					}
				}
			}
		}
	}

	private void GatherNavmeshTiles(RustNavmesh navmesh, int navId, Matrix4x4? navToWorld, bool alwaysResend, Bounds worldBounds, List<NavDrawTile> candidates)
	{
		Bounds bounds = worldBounds;
		if (navToWorld.HasValue)
		{
			Matrix4x4 inverse = navToWorld.Value.inverse;
			OBB oBB = new OBB(worldBounds);
			oBB.Transform(inverse.GetPosition(), inverse.lossyScale, inverse.rotation);
			bounds = oBB.ToBounds();
		}
		using PooledList<Vector2Int> pooledList = Facepunch.Pool.Get<PooledList<Vector2Int>>();
		navmesh.GetTilesInBounds(bounds, pooledList);
		foreach (Vector2Int item in pooledList)
		{
			Vector3 vector = navmesh.rcCalcTileBounds(item).center;
			if (navToWorld.HasValue)
			{
				vector = navToWorld.Value.MultiplyPoint3x4(vector);
			}
			candidates.Add(new NavDrawTile
			{
				navId = navId,
				coord = item,
				navmesh = navmesh,
				transform = navToWorld,
				worldCenter = vector,
				alwaysResend = alwaysResend
			});
		}
	}

	private int SendNavmeshTile(NavDrawTile tile)
	{
		using (TimeWarning.New("BasePlayer.SendNavmeshTile"))
		{
			using NavMeshData navMeshData = Facepunch.Pool.Get<NavMeshData>();
			navMeshData.polygons = Facepunch.Pool.Get<List<VectorList>>();
			tile.navmesh.FillDebugDrawProtoForTile(navMeshData, tile.coord.x, tile.coord.y, tile.transform);
			byte[] array = navMeshData.ToProtoBytes();
			ClientRPC(RpcTarget.Player("CL_DrawNavmeshTile", this), tile.navId, tile.coord.x, tile.coord.y, tile.worldCenter, array);
			return array.Length;
		}
	}

	private void SendNavmeshManifest(List<NavDrawTile> inRange)
	{
		using (TimeWarning.New("BasePlayer.SendNavmeshManifest"))
		{
			using NavMeshData navMeshData = Facepunch.Pool.Get<NavMeshData>();
			navMeshData.polygons = Facepunch.Pool.Get<List<VectorList>>();
			for (int i = 0; i < inRange.Count; i++)
			{
				VectorList vectorList = Facepunch.Pool.Get<VectorList>();
				vectorList.vectorPoints = Facepunch.Pool.Get<List<Vector3>>();
				vectorList.vectorPoints.Add(new Vector3(inRange[i].coord.x, inRange[i].coord.y, inRange[i].navId));
				navMeshData.polygons.Add(vectorList);
			}
			ClientRPC(RpcTarget.Player("CL_DrawNavmeshManifest", this), navMeshData.ToProtoBytes());
		}
	}

	public bool TriggeredAntiHack(float seconds = 1f, float score = float.PositiveInfinity)
	{
		return TriggeredAntiHack(AntiHack.PlayerStates.AsReadOnly(), seconds, score);
	}

	public bool TriggeredAntiHack(NativeArray<AntiHack.PlayerState>.ReadOnly ahStates, float seconds = 1f, float score = float.PositiveInfinity)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = ahStates[ActivePlayerInd];
			if (!(UnityEngine.Time.realtimeSinceStartup - playerState.LastViolationTime < seconds))
			{
				return playerState.ViolationLevel > score;
			}
			return true;
		}
		return false;
	}

	public bool TriggeredMovementAntiHack(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ActivePlayerInd];
			return UnityEngine.Time.realtimeSinceStartup - playerState.LastMovementViolationTime < seconds;
		}
		return false;
	}

	public bool UsedAdminCheat(float seconds = 2f)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ActivePlayerInd];
			return UnityEngine.Time.realtimeSinceStartup - playerState.LastAdminCheatTime < seconds;
		}
		return false;
	}

	public bool TriggeredNoclip(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			AntiHack.PlayerState playerState = AntiHack.PlayerStates[ActivePlayerInd];
			if (playerState.LastViolationType == AntiHackType.NoClip)
			{
				return UnityEngine.Time.realtimeSinceStartup - playerState.LastViolationTime < seconds;
			}
			return false;
		}
		return false;
	}

	public void PauseVehicleNoClipDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerNoclipState reference = ref ((Span<AntiHack.PlayerNoclipState>)AntiHack.PlayerNoclipStates)[ActivePlayerInd];
			reference.VehiclePauseTime = Mathf.Max(reference.VehiclePauseTime, seconds);
		}
	}

	public void PauseFlyHackDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerFlyhackState reference = ref ((Span<AntiHack.PlayerFlyhackState>)AntiHack.PlayerFlyhackStates)[ActivePlayerInd];
			reference.PauseTime = Mathf.Max(reference.PauseTime, seconds);
		}
	}

	public void AddTempSpeedHackBudget(float totalDistanceExpected = 1f, float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerSpeedhackState reference = ref ((Span<AntiHack.PlayerSpeedhackState>)AntiHack.PlayerSpeedhackStates)[ActivePlayerInd];
			reference.ExtraSpeed = totalDistanceExpected / seconds;
			reference.ExtraSpeedTime = seconds;
		}
	}

	public void PauseSpeedHackDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerSpeedhackState reference = ref ((Span<AntiHack.PlayerSpeedhackState>)AntiHack.PlayerSpeedhackStates)[ActivePlayerInd];
			reference.PauseTime = Mathf.Max(reference.PauseTime, seconds);
		}
	}

	public void PauseTickDistanceDetection(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerState reference = ref ((Span<AntiHack.PlayerState>)AntiHack.PlayerStates)[ActivePlayerInd];
			reference.TickDistancePausetime = Mathf.Max(reference.TickDistancePausetime, seconds);
		}
	}

	public void ForceCastNoClip(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			ref AntiHack.PlayerNoclipState reference = ref ((Span<AntiHack.PlayerNoclipState>)AntiHack.PlayerNoclipStates)[ActivePlayerInd];
			reference.ForceCastTime = Mathf.Max(reference.ForceCastTime, seconds);
		}
	}

	public void UpdateUnparentTime()
	{
		if (ActivePlayerInd != -1)
		{
			((Span<AntiHack.PlayerState>)AntiHack.PlayerStates)[ActivePlayerInd].UnparentTime = UnityEngine.Time.time;
		}
	}

	public bool RecentlyUnparented(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			return UnityEngine.Time.time - AntiHack.PlayerStates[ActivePlayerInd].UnparentTime <= seconds;
		}
		return false;
	}

	public bool RecentlyInAir(float seconds = 1f)
	{
		if (ActivePlayerInd != -1)
		{
			float lastInAirTime = AntiHack.PlayerFlyhackStates[ActivePlayerInd].LastInAirTime;
			return UnityEngine.Time.realtimeSinceStartup - lastInAirTime < seconds;
		}
		return false;
	}

	public int GetAntiHackKicks()
	{
		return AntiHack.GetKickRecord(this);
	}

	public static void ResetAntiHack(BasePlayer player, NativeArray<AntiHack.PlayerState> playerStates, NativeArray<AntiHack.PlayerNoclipState> noclipStates, NativeArray<AntiHack.PlayerSpeedhackState> speedhackStates, NativeArray<AntiHack.PlayerFlyhackState> flyhackStates)
	{
		if (player.ActivePlayerInd != -1)
		{
			if (playerStates.IsCreated)
			{
				playerStates[player.ActivePlayerInd] = default(AntiHack.PlayerState);
			}
			if (noclipStates.IsCreated)
			{
				noclipStates[player.ActivePlayerInd] = default(AntiHack.PlayerNoclipState);
			}
			if (speedhackStates.IsCreated)
			{
				speedhackStates[player.ActivePlayerInd] = default(AntiHack.PlayerSpeedhackState);
			}
			if (flyhackStates.IsCreated)
			{
				flyhackStates[player.ActivePlayerInd] = default(AntiHack.PlayerFlyhackState);
			}
		}
		player.rpcHistory.Clear();
	}

	public bool CanModifyClan()
	{
		if (!Clan.editsRequireClanTable)
		{
			return true;
		}
		if (base.isServer)
		{
			if (triggers == null || ClanManager.ServerInstance == null)
			{
				return false;
			}
			foreach (TriggerBase trigger in triggers)
			{
				if (trigger is TriggerClanModify)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public void LoadClanInfo()
	{
		ClanManager clanManager = ClanManager.ServerInstance;
		if (Clan.enabled && !(clanManager == null))
		{
			LoadClanInfoImpl();
		}
		async void LoadClanInfoImpl()
		{
			try
			{
				ClanValueResult<IClan> clanValueResult = await clanManager.Backend.GetByMember(userID);
				if (!clanValueResult.IsSuccess)
				{
					if (clanValueResult.Result != ClanResult.NoClan)
					{
						Debug.LogError($"Failed to find clan for {userID.Get()}: {clanValueResult.Result}");
						Invoke(LoadClanInfo, 45 + UnityEngine.Random.Range(0, 30));
						return;
					}
					serverClan = null;
					clanId = 0L;
				}
				else
				{
					serverClan = clanValueResult.Value;
					clanId = serverClan.ClanId;
				}
				SendNetworkUpdate();
				CheckClanProgressiveAchievements(serverClan);
				if (net?.connection != null)
				{
					UpdateClanLastSeen();
					if (clanId != 0L)
					{
						clanManager.ClanMemberConnectionsChanged(clanId);
					}
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void UpdateClanLastSeen()
	{
		ClanManager clanManager = ClanManager.ServerInstance;
		if (!(clanManager == null) && clanId != 0L)
		{
			UpdateClanLastSeenImpl();
		}
		async void UpdateClanLastSeenImpl()
		{
			_ = 1;
			try
			{
				ClanValueResult<IClan> clanValueResult = await clanManager.Backend.Get(clanId);
				if (!clanValueResult.IsSuccess)
				{
					LoadClanInfo();
				}
				else
				{
					ClanResult clanResult = await clanValueResult.Value.UpdateLastSeen(userID);
					if (clanResult != ClanResult.Success)
					{
						Debug.LogWarning($"Couldn't update clan last seen for {userID.Get()}: {clanResult}");
					}
				}
			}
			catch (Exception arg)
			{
				Debug.LogError($"Failed to update clan last seen for {userID.Get()}: {arg}");
			}
		}
	}

	public void GiveClanJoinedAchievement()
	{
		if (!IsNpc && !IsBot && IsConnected)
		{
			GiveAchievement("CLAN_JOIN");
		}
	}

	public void CheckClanProgressiveAchievements(IClan clan)
	{
		if (clan != null && !clanSizeAchievementCheckDisabled && !IsNpc && !IsBot && IsConnected && !IsInTutorial && !clanSizeAchievementCheckDisabled && clan.Members != null && clan.Members.Count >= 10 && GiveAchievement("CLAN_SIZE"))
		{
			clanSizeAchievementCheckDisabled = true;
		}
	}

	public void AddClanScore(ClanScoreEventType type, int multiplier = 1, BasePlayer otherPlayer = null, IClan otherClan = null, string arg1 = null, string arg2 = null)
	{
		ClanManager serverInstance = ClanManager.ServerInstance;
		if (serverInstance == null || serverClan == null || IsBot || IsNpc || multiplier == 0)
		{
			return;
		}
		int scoreForEvent = Clan.GetScoreForEvent(type);
		if (scoreForEvent != 0)
		{
			bool flag = otherPlayer != null && !otherPlayer.IsBot && !otherPlayer.IsNpc;
			serverInstance.AddScore(serverClan, new ClanScoreEvent
			{
				Type = type,
				SteamId = userID,
				Score = scoreForEvent,
				Multiplier = multiplier,
				OtherSteamId = (flag ? new ulong?(otherPlayer.userID) : null),
				OtherClanId = ((otherClan != null && otherClan != serverClan) ? new long?(otherClan.ClanId) : ((flag && otherPlayer.clanId != 0L) ? new long?(otherPlayer.clanId) : null)),
				Arg1 = arg1,
				Arg2 = arg2
			});
			if (!clanScoreAchievementCheckDisabled && IsConnected && GiveAchievement("CLAN_SCORE"))
			{
				clanScoreAchievementCheckDisabled = true;
			}
		}
	}

	private void HandleClanPlayerKilled(BasePlayer killedByPlayer)
	{
		if (!(killedByPlayer == null) && !(killedByPlayer == this))
		{
			if (serverClan != null && killedByPlayer.serverClan != null && serverClan != killedByPlayer.serverClan)
			{
				AddClanScore(ClanScoreEventType.ClanPlayerDied, 1, killedByPlayer);
				killedByPlayer.AddClanScore(ClanScoreEventType.ClanPlayerKilled, 1, this);
			}
			if (!HasPlayerFlag(PlayerFlags.DisplaySash) && killedByPlayer.serverClan != null)
			{
				killedByPlayer.AddClanScore(ClanScoreEventType.UnarmedPlayerKilled, 1, this);
			}
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		object obj = Interface.CallHook("CanLootPlayer", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player == this)
		{
			return false;
		}
		if (player.IsBlockedFromLootingByMountable())
		{
			return false;
		}
		if ((IsWounded() || IsSleeping() || CurrentGestureIsSurrendering || IsRestrainedOrSurrendering) && !IsLoadingAfterTransfer())
		{
			return !IsTransferring();
		}
		return false;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_LootPlayer(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((bool)player && player.CanInteract() && CanBeLooted(player) && player.inventory.loot.StartLootingEntity(this))
		{
			player.inventory.loot.AddContainer(inventory.containerMain);
			player.inventory.loot.AddContainer(inventory.containerWear);
			player.inventory.loot.AddContainer(inventory.containerBelt);
			Interface.CallHook("OnLootPlayer", this, player);
			player.inventory.loot.SendImmediate();
			player.RadioactiveLootCheck(player.inventory.loot.containers);
			player.ClientRPC(RpcTarget.Player("RPC_OpenLootPanel", player), "player_corpse");
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Assist(RPCMessage msg)
	{
		if (msg.player.CanInteract() && !(msg.player == this) && IsWounded() && Interface.CallHook("OnPlayerAssist", this, msg.player) == null)
		{
			StopWounded(msg.player);
			msg.player.stats.Add("wounded_assisted", 1, (Stats)5);
			stats.Add("wounded_healed", 1);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_KeepAlive(RPCMessage msg)
	{
		if (msg.player.CanInteract() && !(msg.player == this) && IsWounded() && Interface.CallHook("OnPlayerKeepAlive", this, msg.player) == null)
		{
			ProlongWounding(10f);
		}
	}

	public void SetActiveTalkingToNpc(NPCTalking npc)
	{
		if (!(activeTalkingToNpc == npc))
		{
			EndActiveConversation();
			activeTalkingToNpc = npc;
		}
	}

	public void ClearActiveTalkingToNpc(NPCTalking npc)
	{
		if (npc != activeTalkingToNpc)
		{
			string text = ((activeTalkingToNpc == null) ? "null" : activeTalkingToNpc.name);
			Debug.LogWarning(npc.name + " tried to clear active talking to NPC on " + base.name + " but their NPC is " + text, this);
		}
		activeTalkingToNpc = null;
	}

	public void EndActiveConversation()
	{
		if (!(activeTalkingToNpc == null))
		{
			activeTalkingToNpc.Server_OnConversationEnded(this);
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server]
	[RPC_Server.InputValidation(new Type[] { })]
	private void Server_RequestLootCountdowns(RPCMessage msg)
	{
		if ((!msg.player.IsAdmin && !msg.player.IsDeveloper) || !Network.Net.sv.IsConnected() || net == null)
		{
			return;
		}
		int num = Mathf.Min(msg.read.UInt16(), 32);
		using PooledList<ILootContainer> pooledList = Facepunch.Pool.Get<PooledList<ILootContainer>>();
		for (int i = 0; i < num; i++)
		{
			if (BaseNetworkable.serverEntities.Find(msg.read.EntityID()) is ILootContainer item)
			{
				pooledList.Add(item);
			}
		}
		NetWrite netWrite = ClientRPCStart("Client_ReceiveLootCountdowns");
		netWrite.UInt16((ushort)pooledList.Count);
		foreach (ILootContainer item2 in pooledList)
		{
			netWrite.EntityID(item2.GetEntity().net.ID);
			netWrite.Int32((int)item2.GetLootCountdownTimeRemaining());
		}
		ClientRPCSend(netWrite, new SendInfo(net.connection));
	}

	[RPC_Server]
	private void SV_Drink(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		Vector3 vector = msg.read.Vector3();
		if (!vector.IsNaNOrInfinity() && (bool)player && player.metabolism.CanConsume() && !(Vector3.Distance(player.transform.position, vector) > 5f) && WaterLevel.Test(vector, waves: true, volumes: true, this) && (!isMounted || GetMounted().canDrinkWhileMounted))
		{
			ItemDefinition itemDefinition = WaterResource.SV_GetAtPoint(vector);
			ItemModConsumable component = itemDefinition.GetComponent<ItemModConsumable>();
			Item item = ItemManager.Create(itemDefinition, component.amountToConsume, 0uL, isServerSide: true, 0uL);
			ItemModConsume component2 = item.info.GetComponent<ItemModConsume>();
			if (component2.CanDoAction(item, player))
			{
				component2.DoAction(item, player);
			}
			item?.Remove();
			player.metabolism.MarkConsumption();
		}
	}

	[RPC_Server.InputValidation(new Type[]
	{
		typeof(bool),
		typeof(Vector3),
		typeof(NetworkableId)
	})]
	[RPC_Server]
	public void RPC_StartClimb(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		bool flag = msg.read.Bit();
		Vector3 vector = msg.read.Vector3();
		NetworkableId networkableId = msg.read.EntityID();
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(networkableId);
		Vector3 vector2 = (flag ? baseNetworkable.transform.TransformPoint(vector) : vector);
		if (player.IsRestrained || !player.isMounted || player.Distance(vector2) > 5f || !GamePhysics.LineOfSight(player.eyes.position, vector2, 1218519041) || !GamePhysics.LineOfSight(vector2, vector2 + player.eyes.offset, 1218519041))
		{
			return;
		}
		Vector3 end = vector2 - (vector2 - player.eyes.position).normalized * 0.25f;
		if (!GamePhysics.CheckCapsule(player.eyes.position, end, 0.25f, 1218519041) && !AntiHack.TestNoClipping(player, vector2 + NoClipOffset(), vector2 + NoClipOffset(), NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _))
		{
			player.EnsureDismounted();
			player.MovePosition(vector2);
			Collider component = player.GetComponent<Collider>();
			component.enabled = false;
			component.enabled = true;
			if (flag)
			{
				player.ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", player), vector, networkableId);
			}
			else
			{
				player.ClientRPC(RpcTarget.Player("ForcePositionTo", player), vector2);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	private void RequestServerEmoji()
	{
		RustEmojiLibrary.FindAllServerEmoji();
		if (RustEmojiLibrary.allServerEmoji.Count > 0)
		{
			ClientRPCList(RpcTarget.Player("ClientReceiveEmojiList", this), RustEmojiLibrary.cachedServerList);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	private void ServerRequestEmojiData(RPCMessage msg)
	{
		string text = msg.read.String();
		if (RustEmojiLibrary.allServerEmoji.TryGetValue(text, out var value))
		{
			byte[] array = FileStorage.server.Get(value.CRC, value.FileType, RustEmojiLibrary.EmojiStorageNetworkId);
			ClientRPC(RpcTarget.Player("ClientReceiveEmojiData", msg.player), (uint)array.Length, array, text, value.CRC, (int)value.FileType);
		}
	}

	public int GetQueuedUpdateCount(NetworkQueue queue)
	{
		return networkQueue[(int)queue].Length;
	}

	public void SendSnapshots(ListHashSet<Networkable> ents)
	{
		if (ents == null)
		{
			return;
		}
		using (TimeWarning.New("SendSnapshots"))
		{
			int count = ents.Values.Count;
			Networkable[] buffer = ents.Values.Buffer;
			for (int i = 0; i < count; i++)
			{
				SnapshotQueue.Add(buffer[i].handler as BaseNetworkable);
			}
		}
	}

	public void QueueUpdate(NetworkQueue queue, BaseNetworkable ent)
	{
		if (!IsConnected)
		{
			return;
		}
		switch (queue)
		{
		case NetworkQueue.Update:
			networkQueue[0].Add(ent);
			break;
		case NetworkQueue.UpdateDistance:
			if (!IsReceivingSnapshot && !networkQueue[1].Contains(ent) && !networkQueue[0].Contains(ent))
			{
				NetworkQueueList networkQueueList = networkQueue[1];
				if (Distance(ent as BaseEntity) < 20f)
				{
					QueueUpdate(NetworkQueue.Update, ent);
				}
				else
				{
					networkQueueList.Add(ent);
				}
			}
			break;
		}
	}

	public void SendEntityUpdate()
	{
		using (TimeWarning.New("SendEntityUpdate"))
		{
			SendEntityUpdates(SnapshotQueue);
			SendEntityUpdates(networkQueue[0]);
			SendEntityUpdates(networkQueue[1]);
		}
	}

	public static void SendEntityUpdates(BasePlayer[] players, ReadOnlySpan<int> indices)
	{
		using (TimeWarning.New("SendEntityUpdates"))
		{
			ThreadSafeTime time2 = ThreadSafeTime.TakeSnapshot();
			ReadOnlySpan<int> readOnlySpan;
			if (BaseNetworkable.UseParallelSaves)
			{
				int num = 0;
				List<UniTask> obj = Facepunch.Pool.Get<List<UniTask>>();
				BufferList<int> obj2 = Facepunch.Pool.Get<BufferList<int>>();
				readOnlySpan = indices;
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					int num2 = readOnlySpan[i];
					BasePlayer basePlayer = players[num2];
					int val = (basePlayer.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
					int num3 = Math.Min(basePlayer.SnapshotQueue.Length, val);
					for (int j = 0; j < 2; j++)
					{
						num3 += Math.Min(basePlayer.networkQueue[j].Length, val);
					}
					if (num3 != 0)
					{
						obj2.Add(num2);
						num += num3;
						if (num >= ConVar.Server.ParallelNetworkQueueBatchSize)
						{
							obj.Add(ProcessPlayerBatchAsync(players, obj2, time2));
							obj2 = Facepunch.Pool.Get<BufferList<int>>();
							num = 0;
						}
					}
				}
				if (obj2.Count > 0)
				{
					ProcessPlayerBatch(players, obj2, in time2);
				}
				else
				{
					Facepunch.Pool.FreeUnmanaged(ref obj2);
				}
				ThreadUtils.WaitForTasks(obj);
				Facepunch.Pool.FreeUnmanaged(ref obj);
				return;
			}
			BufferList<(BaseEntity, BasePlayer)> obj3 = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			HashSet<BaseEntity> obj4 = Facepunch.Pool.Get<HashSet<BaseEntity>>();
			BufferList<(BaseEntity, BasePlayer)> obj5 = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			BufferList<int> obj6 = Facepunch.Pool.Get<BufferList<int>>();
			readOnlySpan = indices;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				int num4 = readOnlySpan[i];
				BasePlayer basePlayer2 = players[num4];
				int batchSize2 = (basePlayer2.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
				NetworkQueueList snapshotQueue = basePlayer2.SnapshotQueue;
				GatherFromQueue(basePlayer2, snapshotQueue, batchSize2, obj4, obj3);
				int count = obj5.Count;
				foreach (var item2 in obj3)
				{
					if (item2.Item1.ShouldNetworkTo(item2.Item2))
					{
						obj5.Add(item2);
					}
				}
				obj3.Clear();
				if (obj5.Count > count)
				{
					BuildSnapshotDependencyChains(obj5.ContentReadOnlySpan().Slice(count), count, obj6);
				}
				for (int k = 0; k < 2; k++)
				{
					snapshotQueue = basePlayer2.networkQueue[k];
					GatherFromQueue(basePlayer2, snapshotQueue, batchSize2, obj4, obj3);
					count = obj5.Count;
					foreach (var item3 in obj3)
					{
						if (item3.Item1.ShouldNetworkTo(item3.Item2))
						{
							obj5.Add(item3);
						}
					}
					obj3.Clear();
					if (obj5.Count > count)
					{
						BuildSnapshotDependencyChains(obj5.ContentReadOnlySpan().Slice(count), count, obj6);
					}
				}
				obj4.Clear();
			}
			Facepunch.Pool.FreeUnmanaged(ref obj4);
			Facepunch.Pool.FreeUnmanaged(ref obj3);
			if (obj5.Count == 0)
			{
				Facepunch.Pool.FreeUnmanaged(ref obj5);
				Facepunch.Pool.FreeUnmanaged(ref obj6);
			}
			else
			{
				SendEntitySnapshots(obj5, obj6.ContentReadOnlySpan(), in time2);
				Facepunch.Pool.FreeUnmanaged(ref obj5);
				Facepunch.Pool.FreeUnmanaged(ref obj6);
			}
		}
		static void GatherFromQueue(BasePlayer player, NetworkQueueList queue, int batchSize, HashSet<BaseEntity> alreadyScheduledPairs, BufferList<(BaseEntity from, BasePlayer to)> shouldNetworkToPairs)
		{
			if (CollectionEx.IsEmpty(queue.queueInternal))
			{
				return;
			}
			using (TimeWarning.New("GatherFromQueue"))
			{
				using PooledList<BaseNetworkable> pooledList = Facepunch.Pool.Get<PooledList<BaseNetworkable>>();
				int num5 = 0;
				foreach (BaseNetworkable item4 in queue.queueInternal)
				{
					pooledList.Add(item4);
					if (!(item4 == null) && item4.net != null)
					{
						BaseEntity baseEntity = item4 as BaseEntity;
						if (!alreadyScheduledPairs.Contains(baseEntity))
						{
							alreadyScheduledPairs.Add(baseEntity);
							shouldNetworkToPairs.Add((baseEntity, player));
							if (++num5 > batchSize)
							{
								break;
							}
						}
					}
				}
				if (pooledList.Count == queue.queueInternal.Count)
				{
					queue.queueInternal.Clear();
					if (queue.MaxLength > 2048)
					{
						queue.queueInternal = new HashSet<BaseNetworkable>();
						queue.MaxLength = 0;
					}
					return;
				}
				foreach (BaseNetworkable item5 in pooledList)
				{
					queue.queueInternal.Remove(item5);
				}
			}
		}
		static void ProcessPlayerBatch(ReadOnlySpan<BasePlayer> players, BufferList<int> indices, in ThreadSafeTime time)
		{
			using (TimeWarning.New("ProcessPlayerBatch"))
			{
				HashSet<BaseEntity> obj7 = Facepunch.Pool.Get<HashSet<BaseEntity>>();
				BufferList<(BaseEntity, BasePlayer)> obj8 = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
				bool errorLogged2 = false;
				foreach (int index in indices)
				{
					BasePlayer basePlayer3 = players[index];
					int batchSize3 = (basePlayer3.IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
					Network.Connection connection = basePlayer3.net.connection;
					NetworkQueueList snapshotQueue2 = basePlayer3.SnapshotQueue;
					GatherFromQueue(basePlayer3, snapshotQueue2, batchSize3, obj7, obj8);
					SendQueue(basePlayer3, connection, obj8, in time, ref errorLogged2);
					obj8.Clear();
					for (int l = 0; l < 2; l++)
					{
						snapshotQueue2 = basePlayer3.networkQueue[l];
						GatherFromQueue(basePlayer3, snapshotQueue2, batchSize3, obj7, obj8);
						SendQueue(basePlayer3, connection, obj8, in time, ref errorLogged2);
						obj8.Clear();
					}
					obj7.Clear();
				}
				Facepunch.Pool.FreeUnmanaged(ref obj8);
				Facepunch.Pool.FreeUnmanaged(ref obj7);
				Facepunch.Pool.FreeUnmanaged(ref indices);
			}
		}
		static async UniTask ProcessPlayerBatchAsync(BasePlayer[] players, BufferList<int> indices, ThreadSafeTime time)
		{
			await UniTask.SwitchToThreadPool();
			ProcessPlayerBatch(players, indices, in time);
		}
		static void SendQueue(BasePlayer player, Network.Connection conn, BufferList<(BaseEntity from, BasePlayer to)> pairs, in ThreadSafeTime time, ref bool errorLogged)
		{
			foreach (var pair in pairs)
			{
				BaseEntity item = pair.from;
				try
				{
					if (item.ShouldNetworkTo(player))
					{
						NetWrite write = Network.Net.sv.StartWrite();
						item.SendAsSnapshot(conn, write, in time, ordered: false);
					}
				}
				catch (Exception arg)
				{
					if (!errorLogged)
					{
						Debug.LogError($"ProcessPlayerBatch: {arg}");
						errorLogged = true;
					}
				}
			}
		}
	}

	public static void BuildSnapshotDependencyChains(ReadOnlySpan<(BaseEntity from, BasePlayer to)> queuePairs, int indexOffset, BufferList<int> depChains)
	{
		if (queuePairs.Length == 1)
		{
			depChains.Add(1);
			depChains.Add(indexOffset);
			return;
		}
		using (TimeWarning.New("BuildSnapshotDependencyChains"))
		{
			Dictionary<ulong, List<int>> dict = Facepunch.Pool.Get<Dictionary<ulong, List<int>>>();
			List<int> obj = Facepunch.Pool.Get<List<int>>();
			HashSet<ulong> obj2 = Facepunch.Pool.Get<HashSet<ulong>>();
			for (int i = 0; i < queuePairs.Length; i++)
			{
				BaseEntity item = queuePairs[i].from;
				if (item.ChildCount == 0)
				{
					BaseEntity baseEntity = GetRootOfChain(item, obj2);
					if ((object)item == baseEntity)
					{
						obj.Add(i);
					}
					else
					{
						ulong value = baseEntity.net.ID.Value;
						if (!dict.TryGetValue(value, out var value2))
						{
							value2 = (dict[value] = Facepunch.Pool.Get<List<int>>());
						}
						value2.Add(i);
					}
				}
				else
				{
					ulong value3 = GetRootOfChain(item, obj2).net.ID.Value;
					if (!dict.TryGetValue(value3, out var value4))
					{
						value4 = (dict[value3] = Facepunch.Pool.Get<List<int>>());
					}
					value4.Add(i);
				}
				obj2.Add(item.net.ID.Value);
			}
			foreach (var (_, list4) in dict)
			{
				depChains.Add(list4.Count);
				foreach (int item2 in list4)
				{
					int element = item2 + indexOffset;
					depChains.Add(element);
				}
				List<int> obj3 = list4;
				Facepunch.Pool.FreeUnmanaged(ref obj3);
			}
			foreach (int item3 in obj)
			{
				depChains.Add(1);
				int element2 = item3 + indexOffset;
				depChains.Add(element2);
			}
			Facepunch.Pool.FreeUnmanaged(ref dict);
			Facepunch.Pool.FreeUnmanaged(ref obj2);
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		static BaseEntity GetRootOfChain(BaseEntity entity, HashSet<ulong> processedEntitySet)
		{
			BaseEntity baseEntity2 = entity.GetParentEntity();
			while (!baseEntity2.IsRealNull() && processedEntitySet.Contains(baseEntity2.net.ID.Value))
			{
				entity = baseEntity2;
				baseEntity2 = entity.GetParentEntity();
			}
			return entity;
		}
	}

	private static void SendEntitySnapshots(BufferList<(BaseEntity, BasePlayer)> allPairs, ReadOnlySpan<int> chains, in ThreadSafeTime time)
	{
		using (TimeWarning.New("SendEntitySnapshots"))
		{
			BufferList<int> obj = Facepunch.Pool.Get<BufferList<int>>();
			BufferList<int> obj2 = Facepunch.Pool.Get<BufferList<int>>();
			FilterPairsForThreads(allPairs.ContentReadOnlySpan(), chains, obj, obj2);
			SendEntitySnapshots_AsyncState obj3 = Facepunch.Pool.Get<SendEntitySnapshots_AsyncState>();
			MergeDepsChains(chains, obj2.ContentReadOnlySpan(), ConVar.Server.SnapshotTaskBatchCount, obj3.Chains, obj3.ChainIndices);
			Facepunch.Pool.FreeUnmanaged(ref obj2);
			obj3.Pairs = allPairs;
			using PooledList<UniTask> pooledList = Facepunch.Pool.Get<PooledList<UniTask>>();
			if (obj3.ChainIndices.Count > 0)
			{
				for (int i = 0; i < obj3.ChainIndices.Count; i++)
				{
					pooledList.Add(SendSnapshotsAsync(obj3.Pairs, obj3.Chains, obj3.ChainIndices, i, time));
				}
			}
			SendSnapshotsMain(allPairs.ContentReadOnlySpan(), chains, obj.ContentReadOnlySpan(), in time);
			ThreadUtils.WaitForTasks(pooledList);
			Facepunch.Pool.FreeUnmanaged(ref obj);
			Facepunch.Pool.Free(ref obj3);
		}
		static void FilterPairsForThreads(ReadOnlySpan<(BaseEntity from, BasePlayer to)> allPairs, ReadOnlySpan<int> chains, BufferList<int> toSerializeAndSend, BufferList<int> toSend)
		{
			using (TimeWarning.New("FilterPairsForThreads"))
			{
				int num14;
				for (num14 = 0; num14 < chains.Length; num14++)
				{
					int num15 = chains[num14];
					int num16 = num14 + 1;
					int num17 = num16 + num15;
					bool flag = false;
					for (int num18 = num16; num18 < num17; num18++)
					{
						int index4 = chains[num18];
						var (from2, basePlayer) = allPairs[index4];
						if (NeedsSerialization(from2, basePlayer.net.connection))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						toSerializeAndSend.Add(num14);
					}
					else
					{
						toSend.Add(num14);
					}
					num14 += num15;
				}
			}
		}
		static void MergeDepsChains(ReadOnlySpan<int> chains, ReadOnlySpan<int> chainIndices, int mergeLimit, BufferList<int> newChains, BufferList<int> newChainIndices)
		{
			int num8 = 0;
			int num9 = 0;
			for (int m = 0; m < chainIndices.Length; m++)
			{
				int index3 = chainIndices[m];
				int num10 = chains[index3];
				num9 += num10;
				if (num9 > mergeLimit)
				{
					int count = newChains.Count;
					newChains.Add(num9);
					for (int n = num8; n <= m; n++)
					{
						int num11 = chainIndices[n];
						int length = chains[num11];
						int start = num11 + 1;
						newChains.AddSpan(chains.Slice(start, length));
					}
					newChainIndices.Add(count);
					num8 = m + 1;
					num9 = 0;
				}
			}
			if (num9 > 0)
			{
				int count2 = newChains.Count;
				newChains.Add(num9);
				for (int num12 = num8; num12 < chainIndices.Length; num12++)
				{
					int num13 = chainIndices[num12];
					int length2 = chains[num13];
					int start2 = num13 + 1;
					newChains.AddSpan(chains.Slice(start2, length2));
				}
				newChainIndices.Add(count2);
			}
		}
		static bool NeedsSerialization(BaseEntity from, Network.Connection to)
		{
			if (from.HasNetworkCache)
			{
				return !from.CanUseNetworkCache(to);
			}
			return true;
		}
		static async UniTask SendSnapshotsAsync(BufferList<(BaseEntity from, BasePlayer to)> pairs, BufferList<int> chains, BufferList<int> chainIndices, int batchIndex, ThreadSafeTime time)
		{
			await UniTask.SwitchToThreadPool();
			using (TimeWarning.New("SendEntitySnapshots - Process Batch"))
			{
				int num5 = chainIndices[batchIndex];
				Debug.Assert(num5 < chains.Count, "Went out of bounds of snapshot chain!");
				Network.Server sv2 = Network.Net.sv;
				int num6 = chains[num5];
				num5++;
				int num7 = num5 + num6;
				for (int l = num5; l < num7; l++)
				{
					int index2 = chains[l];
					(BaseEntity from, BasePlayer to) tuple2 = pairs[index2];
					BaseEntity item3 = tuple2.from;
					BasePlayer item4 = tuple2.to;
					NetWrite write2 = sv2.StartWrite();
					item3.SendAsSnapshot(item4.net.connection, write2, in time, ordered: false);
				}
			}
		}
		static void SendSnapshotsMain(ReadOnlySpan<(BaseEntity from, BasePlayer to)> allPairs, ReadOnlySpan<int> chains, ReadOnlySpan<int> chainIndices, in ThreadSafeTime time)
		{
			using (TimeWarning.New("SendSnapshotsMain"))
			{
				Network.Server sv = Network.Net.sv;
				ReadOnlySpan<int> readOnlySpan = chainIndices;
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					int num = readOnlySpan[j];
					int num2 = chains[num];
					int num3 = num + 1;
					int num4 = num3 + num2;
					for (int k = num3; k < num4; k++)
					{
						int index = chains[k];
						(BaseEntity from, BasePlayer to) tuple = allPairs[index];
						BaseEntity item = tuple.from;
						BasePlayer item2 = tuple.to;
						NetWrite write = sv.StartWrite();
						item.SendAsSnapshot(item2.net.connection, write, in time);
					}
				}
			}
		}
	}

	private static void SendEntitySnapshotsWithChildren(ReadOnlySpan<(BaseEntity, BasePlayer)> allPairs, List<UniTask> tasks)
	{
		using (TimeWarning.New("SendEntitySnapshotsWithChildren"))
		{
			BufferList<(BaseEntity, BasePlayer)> obj = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			BufferList<(BaseEntity, BasePlayer)> obj2 = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
			BufferList<(int, int)> obj3 = Facepunch.Pool.Get<BufferList<(int, int)>>();
			FilterPairsForThreads(allPairs, obj, obj2, obj3, ConVar.Server.SnapshotTaskBatchCount);
			ThreadSafeTime time2 = ThreadSafeTime.TakeSnapshot();
			if (obj3.Count > 0)
			{
				using PooledList<UniTask> pooledList = Facepunch.Pool.Get<PooledList<UniTask>>();
				for (int i = 0; i < obj3.Count; i++)
				{
					var (start2, count2) = obj3[i];
					pooledList.Add(ProcessBatch(obj2, start2, count2, time2));
				}
				UniTask workTask2 = UniTask.WhenAll(pooledList);
				UniTask item = Cleanup(obj2, workTask2);
				tasks.Add(item);
			}
			else
			{
				Facepunch.Pool.FreeUnmanaged(ref obj2);
			}
			Facepunch.Pool.FreeUnmanaged(ref obj3);
			SendSnapshotsMain(obj.ContentReadOnlySpan(), in time2);
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		static async UniTask Cleanup(BufferList<(BaseEntity from, BasePlayer to)> pairs, UniTask workTask)
		{
			await UniTask.SwitchToThreadPool();
			await workTask;
			Facepunch.Pool.FreeUnmanaged(ref pairs);
		}
		static async UniTask ProcessBatch(BufferList<(BaseEntity from, BasePlayer to)> pairs, int start, int count, ThreadSafeTime time)
		{
			await UniTask.SwitchToThreadPool();
			for (int j = start; j < start + count; j++)
			{
				var (baseEntity, basePlayer) = pairs[j];
				baseEntity.SendAsSnapshot(basePlayer.net.connection, in time, ordered: false);
			}
		}
	}

	private static void FilterPairsForThreads(ReadOnlySpan<(BaseEntity from, BasePlayer to)> allPairs, BufferList<(BaseEntity, BasePlayer)> toSerializeAndSend, BufferList<(BaseEntity, BasePlayer)> toSend, BufferList<(int start, int count)> toSendBatches, int snapshotsPerBatch)
	{
		using (TimeWarning.New("FilterPairsForThreads"))
		{
			using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
			int num = 0;
			ReadOnlySpan<(BaseEntity, BasePlayer)> readOnlySpan = allPairs;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				(BaseEntity, BasePlayer) tuple = readOnlySpan[i];
				if (NeedsSerializationWithChildren(tuple.Item1, tuple.Item2, pooledList))
				{
					foreach (BaseEntity item in pooledList)
					{
						toSerializeAndSend.Add((item, tuple.Item2));
					}
				}
				else
				{
					foreach (BaseEntity item2 in pooledList)
					{
						toSend.Add((item2, tuple.Item2));
					}
					int num2 = toSend.Count - num;
					if (num2 >= snapshotsPerBatch)
					{
						toSendBatches.Add((num, num2));
						num = toSend.Count;
					}
				}
				pooledList.Clear();
			}
			if (num != toSend.Count)
			{
				toSendBatches.Add((num, toSend.Count - num));
			}
		}
	}

	private static bool NeedsSerializationWithChildren(BaseEntity from, BasePlayer to, List<BaseEntity> visited)
	{
		bool flag = NeedsSerialization(from, to.net.connection);
		visited.Add(from);
		foreach (BaseEntity child in from.children)
		{
			if (child.ShouldNetworkTo(to))
			{
				flag |= NeedsSerializationWithChildren(child, to, visited);
			}
		}
		return flag;
	}

	private static bool NeedsSerialization(BaseEntity from, Network.Connection to)
	{
		if (from.HasNetworkCache)
		{
			return !from.CanUseNetworkCache(to);
		}
		return true;
	}

	private static void SendSnapshotsMain(ReadOnlySpan<(BaseEntity from, BasePlayer to)> pairs, in ThreadSafeTime time)
	{
		using (TimeWarning.New("SendSnapshotsMain"))
		{
			ReadOnlySpan<(BaseEntity, BasePlayer)> readOnlySpan = pairs;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				var (baseEntity, basePlayer) = readOnlySpan[i];
				baseEntity.SendAsSnapshot(basePlayer.net.connection, in time);
			}
		}
	}

	private static void SendEntityDestroyMessages(BufferList<(BaseEntity from, BasePlayer to)> pairs, List<UniTask> tasks)
	{
		using (TimeWarning.New("SendEntityDestroyMessages"))
		{
			int destroyTaskBatchCount = ConVar.Server.DestroyTaskBatchCount;
			int num = (pairs.Count + destroyTaskBatchCount - 1) / destroyTaskBatchCount;
			for (int i = 0; i < num; i++)
			{
				tasks.Add(ProcessBatch(pairs, i, destroyTaskBatchCount));
			}
		}
		static async UniTask ProcessBatch(BufferList<(BaseEntity from, BasePlayer to)> pairs, int index, int batchSize)
		{
			await UniTask.SwitchToThreadPool();
			int num2 = index * batchSize;
			int num3 = num2 + Math.Min(batchSize, pairs.Count - num2);
			for (int j = num2; j < num3; j++)
			{
				var (baseEntity, basePlayer) = pairs[j];
				baseEntity.DestroyOnClient(basePlayer.net.connection);
			}
		}
	}

	public void ClearEntityQueue(Group group = null)
	{
		SnapshotQueue.Clear(group);
		networkQueue[0].Clear(group);
		networkQueue[1].Clear(group);
	}

	private void SendEntityUpdates(NetworkQueueList queue)
	{
		if (queue.queueInternal.Count == 0)
		{
			return;
		}
		int num = (IsReceivingSnapshot ? ConVar.Server.updatebatchspawn : ConVar.Server.updatebatch);
		List<BaseNetworkable> obj = Facepunch.Pool.Get<List<BaseNetworkable>>();
		using (TimeWarning.New("SendEntityUpdates.SendEntityUpdates"))
		{
			int num2 = 0;
			foreach (BaseNetworkable item in queue.queueInternal)
			{
				SendEntitySnapshot(item);
				obj.Add(item);
				num2++;
				if (num2 > num)
				{
					break;
				}
			}
		}
		if (num > queue.queueInternal.Count)
		{
			queue.queueInternal.Clear();
		}
		else
		{
			using (TimeWarning.New("SendEntityUpdates.Remove"))
			{
				for (int i = 0; i < obj.Count; i++)
				{
					queue.queueInternal.Remove(obj[i]);
				}
			}
		}
		if (queue.queueInternal.Count == 0 && queue.MaxLength > 2048)
		{
			queue.queueInternal.Clear();
			queue.queueInternal = new HashSet<BaseNetworkable>();
			queue.MaxLength = 0;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void SendEntitySnapshot(BaseNetworkable ent)
	{
		if (Interface.CallHook("OnEntitySnapshot", ent, net.connection) != null)
		{
			return;
		}
		using (TimeWarning.New("SendEntitySnapshot"))
		{
			if (!(ent == null) && ent.net != null && ent.ShouldNetworkTo(this))
			{
				NetWrite netWrite = Network.Net.sv.StartWrite();
				net.connection.validate.entityUpdates++;
				SaveInfo saveInfo = default(SaveInfo);
				saveInfo.forConnection = net.connection;
				saveInfo.forDisk = false;
				saveInfo.cachedTime = ThreadSafeTime.TakeSnapshot();
				SaveInfo saveInfo2 = saveInfo;
				netWrite.PacketID(Message.Type.Entities);
				netWrite.UInt32(net.connection.validate.entityUpdates);
				ent.ToStreamForNetwork(netWrite, saveInfo2);
				NetProfileCapture.Annotate(netWrite, ent.net.ID.Value, ent.prefabID);
				netWrite.Send(new SendInfo(net.connection));
			}
		}
	}

	public bool HasPlayerFlag(PlayerFlags f)
	{
		return HasPlayerFlag(playerFlags, f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool HasPlayerFlag(PlayerFlags flags, PlayerFlags f)
	{
		return (flags & f) == f;
	}

	public int GetSkinsAccessLevel()
	{
		if (!IsDeveloper)
		{
			return 0;
		}
		if (base.isServer && IsConnected)
		{
			return net.connection.info.GetInt("client.skins_access");
		}
		return 0;
	}

	public void SetPlayerFlag(PlayerFlags f, bool b)
	{
		if (b)
		{
			if (HasPlayerFlag(f))
			{
				return;
			}
			playerFlags |= f;
		}
		else
		{
			if (!HasPlayerFlag(f))
			{
				return;
			}
			playerFlags &= ~f;
		}
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(16uL)]
	public void FogImageUpdate(RPCMessage msg)
	{
		byte b = msg.read.UInt8();
		byte b2 = msg.read.UInt8();
		uint num = msg.read.UInt32();
		uint num2 = msg.read.UInt32();
		if (num2 > 32)
		{
			return;
		}
		List<uint> fogImageList = GetFogImageList();
		if (fogImageList.Count != 16)
		{
			fogImageList.Clear();
			for (int i = 0; i < 16; i++)
			{
				fogImageList.Add(0u);
			}
		}
		if (b != 0 || fogImageList[b2] != num)
		{
			byte[] array = msg.read.BytesWithSize(5000u);
			if (array != null && Interface.CallHook("OnFogOfWarImageUpdate", this, b, b2, num, num2, array) == null)
			{
				FileStorage.server.RemoveEntityNum(net.ID, num2);
				uint value = FileStorage.server.Store(array, FileStorage.Type.png, net.ID, num2);
				fogImageList[b2] = value;
				DirtyPlayerState();
			}
		}
	}

	private List<uint> GetFogImageList()
	{
		return GetFogImageList(CurrentFogMode);
	}

	private List<uint> GetFogImageList(FogMode mode)
	{
		if (mode == FogMode.Mainland)
		{
			if (State.fogImagesMainland == null)
			{
				State.fogImagesMainland = Facepunch.Pool.Get<PooledList<uint>>();
			}
			return State.fogImagesMainland;
		}
		if (State.fogImagesDeepSea == null)
		{
			State.fogImagesDeepSea = Facepunch.Pool.Get<PooledList<uint>>();
		}
		return State.fogImagesDeepSea;
	}

	public void ServerClearFog(bool mainland, bool deepSea)
	{
		if (mainland)
		{
			ClearFogList(ref State.fogImagesMainland);
		}
		if (deepSea)
		{
			ClearFogList(ref State.fogImagesDeepSea);
		}
		Interface.CallHook("OnFogOfWarCleared", this, mainland, deepSea);
		DirtyPlayerState();
		SendFogImagesToClient();
		void ClearFogList(ref List<uint> fog)
		{
			if (fog == null || fog.Count != 16)
			{
				fog = Facepunch.Pool.Get<PooledList<uint>>();
				for (int i = 0; i < 16; i++)
				{
					fog.Add(0u);
				}
			}
			else
			{
				for (int j = 0; j < fog.Count; j++)
				{
					if (fog[j] != 0)
					{
						FileStorage.server.Remove(fog[j], FileStorage.Type.png, net.ID);
						fog[j] = 0u;
					}
				}
			}
		}
	}

	private void SendFogImagesToClient()
	{
		using PooledList<uint> pooledList = Facepunch.Pool.Get<PooledList<uint>>();
		pooledList.AddRange(GetFogImageList(FogMode.Mainland));
		pooledList.AddRange(GetFogImageList(FogMode.DeepSea));
		ClientRPCList(RpcTarget.Player("ReceiveFogOfWarImages", this), pooledList);
	}

	private void OnFogOfWarStale()
	{
		ClearFogList(GetFogImageList(FogMode.Mainland));
		ClearFogList(GetFogImageList(FogMode.DeepSea));
		Interface.CallHook("OnFogOfWarStale", this);
		static void ClearFogList(List<uint> l)
		{
			l.Clear();
			for (int i = 0; i < 16; i++)
			{
				l.Add(0u);
			}
		}
	}

	private RPSWinState Opposite(RPSWinState state)
	{
		return state switch
		{
			RPSWinState.Win => RPSWinState.Loss, 
			RPSWinState.Loss => RPSWinState.Win, 
			_ => state, 
		};
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void Server_StartGesture(RPCMessage msg)
	{
		if (!IsGestureBlocked())
		{
			uint id = msg.read.UInt32();
			GestureConfig toPlay = GestureCollection.Instance.IdToGesture(id);
			Server_StartGesture(toPlay);
		}
	}

	public void Server_StartGesture(uint gestureId, GestureStartSource startSource = GestureStartSource.Player, bool bypassOwnershipCheck = false)
	{
		GestureConfig toPlay = GestureCollection.Instance.IdToGesture(gestureId);
		Server_StartGesture(toPlay, startSource, bypassOwnershipCheck);
	}

	public void Server_StartGesture(string gestureConvarName, GestureStartSource startSource = GestureStartSource.Player, bool bypassOwnershipCheck = false)
	{
		GestureConfig toPlay = GestureCollection.Instance.GestureConvarNameToGesture(gestureConvarName);
		Server_StartGesture(toPlay, startSource, bypassOwnershipCheck);
	}

	public void Server_StartGesture(GestureConfig toPlay, GestureStartSource startSource = GestureStartSource.Player, bool bypassOwnershipCheck = false)
	{
		if (toPlay == null || (toPlay.hideInWheel && startSource == GestureStartSource.Player && !ConVar.Server.cinematic) || (!bypassOwnershipCheck && startSource != 0 && !toPlay.IsOwnedBy(this)) || !toPlay.CanBeUsedBy(this))
		{
			return;
		}
		if (toPlay.animationType == GestureConfig.AnimationType.OneShot)
		{
			Invoke(actionTimeoutGestureServer, toPlay.duration);
		}
		else if (toPlay.animationType == GestureConfig.AnimationType.Loop)
		{
			InvokeRepeating(actionMonitorLoopingGesture, 0f, 0f);
		}
		ClientRPC(RpcTarget.NetworkGroup("Client_StartGesture"), toPlay.gestureId);
		gestureFinishedTime = toPlay.duration;
		currentGesture = toPlay;
		if (!IsNpc && !IsBot)
		{
			switch (toPlay.actionType)
			{
			case GestureConfig.GestureActionType.Surrender:
				inventory.SetLockedByRestraint(flag: true);
				break;
			case GestureConfig.GestureActionType.ShowNameTag:
				if (Rust.GameInfo.HasAchievements)
				{
					int val = CountWaveTargets(base.transform.position, 4f, 0.6f, eyes.HeadForward(), recentWaveTargets, 5);
					stats.Add("waved_at_players", val);
					stats.Save(forceSteamSave: true);
				}
				break;
			case GestureConfig.GestureActionType.DanceAchievement:
			{
				TriggerDanceAchievement triggerDanceAchievement = FindTrigger<TriggerDanceAchievement>();
				if (triggerDanceAchievement != null)
				{
					triggerDanceAchievement.NotifyDanceStarted();
				}
				break;
			}
			}
		}
		if (startSource == GestureStartSource.Player && toPlay.hasMultiplayerInteraction)
		{
			SetPlayerFlag(PlayerFlags.WaitingForGestureInteraction, b: true);
		}
		if (toPlay.animationType == GestureConfig.AnimationType.Loop)
		{
			SendNetworkUpdate();
		}
	}

	private void TimeoutGestureServer()
	{
		currentGesture = null;
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	public void Server_CancelGesture()
	{
		if (currentGesture != null && currentGesture.actionType == GestureConfig.GestureActionType.Surrender)
		{
			Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
			if (handcuffs == null || !handcuffs.Locked)
			{
				inventory.SetLockedByRestraint(flag: false);
			}
		}
		currentGesture = null;
		blockHeldInputTimer = 0f;
		SetPlayerFlag(PlayerFlags.WaitingForGestureInteraction, b: false);
		ClientRPC(RpcTarget.NetworkGroup("Client_RemoteCancelledGesture"));
		CancelInvoke(actionMonitorLoopingGesture);
		CancelInvoke(actionTimeoutGestureServer);
	}

	private void MonitorLoopingGesture()
	{
		bool flag = currentGesture != null && currentGesture.canDuckDuringGesture;
		if (modelState == null || (!flag && modelState.ducked) || modelState.sleeping || IsWounded() || IsSwimming() || IsDead() || (isMounted && GetMounted().allowedGestures == BaseMountable.MountGestureType.UpperBody && !CurrentGestureIsUpperBody) || (isMounted && GetMounted().allowedGestures == BaseMountable.MountGestureType.None))
		{
			Server_CancelGesture();
		}
	}

	private void NotifyGesturesNewItemEquipped()
	{
		if (InGesture)
		{
			Server_CancelGesture();
		}
	}

	public int CountWaveTargets(Vector3 position, float distance, float minimumDot, Vector3 forward, HashSet<NetworkableId> workingList, int maxCount)
	{
		float sqrDistance = distance * distance;
		Group group = net.group;
		if (group == null)
		{
			return 0;
		}
		List<Network.Connection> subscribers = group.subscribers;
		int num = 0;
		for (int i = 0; i < subscribers.Count; i++)
		{
			Network.Connection connection = subscribers[i];
			if (!connection.active)
			{
				continue;
			}
			BasePlayer basePlayer = connection.player as BasePlayer;
			if (CheckPlayer(basePlayer))
			{
				workingList.Add(basePlayer.net.ID);
				num++;
				if (num >= maxCount)
				{
					break;
				}
			}
		}
		return num;
		bool CheckPlayer(BasePlayer player)
		{
			if (player == null)
			{
				return false;
			}
			if (player == this)
			{
				return false;
			}
			if (player.SqrDistance(position) > sqrDistance)
			{
				return false;
			}
			if (Vector3.Dot((player.transform.position - position).normalized, forward) < minimumDot)
			{
				return false;
			}
			if (workingList.Contains(player.net.ID))
			{
				return false;
			}
			return true;
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RequestJoinGesture(RPCMessage msg)
	{
		NetworkableId uid = msg.read.EntityID();
		BasePlayer basePlayer = BaseNetworkable.serverEntities.Find(uid) as BasePlayer;
		if (!HasPlayerFlag(PlayerFlags.WaitingForGestureInteraction) || !InGesture || currentGesture == null)
		{
			return;
		}
		interactiveGestureStartTime = 0f;
		if (msg.player != basePlayer || !(basePlayer != null))
		{
			return;
		}
		SetPlayerFlag(PlayerFlags.WaitingForGestureInteraction, b: false);
		if (currentGesture.actionType == GestureConfig.GestureActionType.RockPaperScissors)
		{
			rpsTarget = uid;
			basePlayer.rpsTarget = net.ID;
			basePlayer.Server_StartGesture(GestureCollection.Instance.GestureConvarNameToGesture("rps"), GestureStartSource.ServerAction);
			ClientRPC(RpcTarget.Player("PromptToPickRPSHand", basePlayer), 10f);
			ClientRPC(RpcTarget.Player("PromptToPickRPSHand", this), 10f);
			if (basePlayer.IsBot)
			{
				basePlayer.Invoke(actionBotRPSRandomise, 2f);
			}
			if (IsBot)
			{
				Invoke(actionBotRPSRandomise, 2f);
			}
			InvokeRepeating(MonitorRPSGame, 0f, 0f);
		}
	}

	private void BotRPSRandomise()
	{
		selectedRpsOption = UnityEngine.Random.Range(0, 3);
		Debug.Log($"Bot randomly selected {selectedRpsOption}");
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	private void SelectedRPSOption(RPCMessage msg)
	{
		selectedRpsOption = msg.read.Int32();
	}

	private void MonitorRPSGame()
	{
		bool flag = false;
		BasePlayer basePlayer = BaseNetworkable.serverEntities.Find(rpsTarget) as BasePlayer;
		if (basePlayer == null || Distance(basePlayer) > 5f || IsWounded() || basePlayer.IsWounded() || IsDead() || basePlayer.IsDead())
		{
			flag = true;
		}
		if (!flag && (float)interactiveGestureStartTime > 10f)
		{
			flag = true;
		}
		if (flag)
		{
			ClientRPC(RpcTarget.Player("CancelRPSGame", this));
			Server_CancelGesture();
			if (basePlayer != null)
			{
				ClientRPC(RpcTarget.Player("CancelRPSGame", basePlayer));
				basePlayer.Server_CancelGesture();
			}
			CancelInvoke(actionMonitorRPSGame);
		}
		if (basePlayer != null && basePlayer.selectedRpsOption != -1 && selectedRpsOption != -1)
		{
			RPSWinState rPSWinState = (((selectedRpsOption != 0 || basePlayer.selectedRpsOption != 2) && (selectedRpsOption != 1 || basePlayer.selectedRpsOption != 0) && (selectedRpsOption != 2 || basePlayer.selectedRpsOption != 1)) ? RPSWinState.Loss : RPSWinState.Win);
			if (selectedRpsOption == basePlayer.selectedRpsOption)
			{
				rPSWinState = RPSWinState.Draw;
			}
			ClientRPC(RpcTarget.NetworkGroup("OnRPSResult"), (int)rPSWinState, selectedRpsOption);
			basePlayer.ClientRPC(RpcTarget.NetworkGroup("OnRPSResult"), (int)Opposite(rPSWinState), basePlayer.selectedRpsOption);
			basePlayer.selectedRpsOption = -1;
			basePlayer.rpsTarget = default(NetworkableId);
			selectedRpsOption = -1;
			rpsTarget = default(NetworkableId);
			CancelInvoke(actionMonitorRPSGame);
			float time = ((rPSWinState == RPSWinState.Draw) ? 2.5f : 5f);
			Invoke(actionServer_CancelGesture, time);
			basePlayer.Invoke(basePlayer.actionServer_CancelGesture, time);
		}
	}

	private bool IsGestureBlocked()
	{
		if (isMounted && GetMounted().allowedGestures == BaseMountable.MountGestureType.None)
		{
			return true;
		}
		if ((bool)GetHeldEntity() && GetHeldEntity().BlocksGestures())
		{
			return true;
		}
		bool flag = currentGesture != null;
		if (flag && currentGesture.gestureType == GestureConfig.GestureType.Cinematic)
		{
			flag = false;
		}
		if (!(IsWounded() || flag) && !IsDead() && !IsSleeping())
		{
			return IsRestrained;
		}
		return true;
	}

	public bool InATeam()
	{
		if (currentTeam == 0L)
		{
			return false;
		}
		if (Team == null)
		{
			Debug.LogWarning($"currentTeam on ({base.name}), userID ({userID.Get()}) is {currentTeam} but Team is null", this);
			return false;
		}
		return true;
	}

	public void DelayedTeamUpdate()
	{
		UpdateTeam(currentTeam);
	}

	public void TeamUpdate()
	{
		TeamUpdate(fullTeamUpdate: false);
	}

	public void TeamUpdate(bool fullTeamUpdate)
	{
		if (!RelationshipManager.TeamsEnabled() || !IsConnected || currentTeam == 0L)
		{
			return;
		}
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(currentTeam);
		if (playerTeam == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		using PlayerTeam playerTeam2 = Facepunch.Pool.Get<PlayerTeam>();
		playerTeam2.teamLeader = playerTeam.teamLeader;
		playerTeam2.teamID = playerTeam.teamID;
		playerTeam2.teamName = playerTeam.teamName;
		playerTeam2.members = Facepunch.Pool.Get<List<PlayerTeam.TeamMember>>();
		playerTeam2.teamLifetime = playerTeam.teamLifetime;
		playerTeam2.teamPings = Facepunch.Pool.Get<List<MapNote>>();
		foreach (ulong member in playerTeam.members)
		{
			BasePlayer basePlayer = RelationshipManager.FindByID(member);
			if ((bool)basePlayer && basePlayer.IsInTutorial)
			{
				continue;
			}
			PlayerTeam.TeamMember teamMember = Facepunch.Pool.Get<PlayerTeam.TeamMember>();
			teamMember.displayName = ((basePlayer != null) ? basePlayer.displayName : (SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member) ?? "DEAD"));
			teamMember.healthFraction = ((basePlayer != null && basePlayer.IsAlive()) ? basePlayer.healthFraction : 0f);
			teamMember.position = ((basePlayer != null) ? basePlayer.transform.position : Vector3.zero);
			teamMember.online = basePlayer != null && !basePlayer.IsSleeping();
			teamMember.wounded = basePlayer != null && basePlayer.IsWounded();
			if ((!sentInstrumentTeamAchievement || !sentSummerTeamAchievement) && basePlayer != null)
			{
				if ((bool)basePlayer.GetHeldEntity() && basePlayer.GetHeldEntity().IsInstrument())
				{
					num++;
				}
				if (basePlayer.isMounted)
				{
					if (basePlayer.GetMounted().IsInstrument())
					{
						num++;
					}
					if (basePlayer.GetMounted().IsSummerDlcVehicle)
					{
						num2++;
					}
				}
				if (num >= 4 && !sentInstrumentTeamAchievement)
				{
					GiveAchievement("TEAM_INSTRUMENTS");
					sentInstrumentTeamAchievement = true;
				}
				if (num2 >= 4)
				{
					GiveAchievement("SUMMER_INFLATABLE");
					sentSummerTeamAchievement = true;
				}
			}
			teamMember.userID = member;
			playerTeam2.members.Add(teamMember);
			if (basePlayer != null)
			{
				if (basePlayer.State.pings != null && basePlayer.State.pings.Count > 0 && basePlayer != this)
				{
					playerTeam2.teamPings.AddRange(basePlayer.State.pings);
				}
				if (fullTeamUpdate && basePlayer != this)
				{
					basePlayer.TeamUpdate(fullTeamUpdate: false);
				}
			}
		}
		playerTeam2.leaderMapNotes = Facepunch.Pool.Get<List<MapNote>>();
		PlayerState playerState = SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(playerTeam.teamLeader);
		if (playerState?.pointsOfInterest != null)
		{
			foreach (MapNote item in playerState.pointsOfInterest)
			{
				playerTeam2.leaderMapNotes.Add(item);
			}
		}
		if (Interface.CallHook("OnTeamUpdated", currentTeam, playerTeam2, this) == null)
		{
			ClientRPC(RpcTarget.PlayerAndSpectators("CLIENT_ReceiveTeamInfo", this), playerTeam2);
			if (playerTeam2.leaderMapNotes != null)
			{
				playerTeam2.leaderMapNotes.Clear();
			}
			if (playerTeam2.teamPings != null)
			{
				playerTeam2.teamPings.Clear();
			}
			BasePlayer basePlayer2 = FindByID(playerTeam.teamLeader);
			if (fullTeamUpdate && basePlayer2 != null && basePlayer2 != this)
			{
				basePlayer2.TeamUpdate(fullTeamUpdate: false);
			}
		}
	}

	public void UpdateTeam(ulong newTeam)
	{
		if (Interface.CallHook("OnTeamUpdate", currentTeam, newTeam, this) == null)
		{
			currentTeam = newTeam;
			SendNetworkUpdate();
			if (RelationshipManager.ServerInstance.FindTeam(newTeam) == null)
			{
				ClearTeam();
			}
			else
			{
				TeamUpdate();
			}
		}
	}

	public void ClearTeam()
	{
		currentTeam = 0uL;
		ClientRPC(RpcTarget.PlayerAndSpectators("CLIENT_ClearTeam", this));
		SendNetworkUpdate();
	}

	public void ClearPendingInvite()
	{
		ClientRPC(RpcTarget.Player("CLIENT_PendingInvite", this), "", 0uL, 0uL);
	}

	public HeldEntity GetHeldEntity()
	{
		if (base.isServer)
		{
			Item activeItem = GetActiveItem();
			if (activeItem == null)
			{
				return null;
			}
			return activeItem.GetHeldEntity() as HeldEntity;
		}
		return null;
	}

	public bool TryGetHeldEntity<T>(out T heldEntity) where T : HeldEntity
	{
		heldEntity = null;
		HeldEntity heldEntity2 = GetHeldEntity();
		if (heldEntity2 == null)
		{
			return false;
		}
		if (heldEntity2 is T val)
		{
			heldEntity = val;
			return true;
		}
		return false;
	}

	public bool TryGetHeldEntity(out HeldEntity heldEntity)
	{
		return this.TryGetHeldEntity<HeldEntity>(out heldEntity);
	}

	public bool IsHoldingEntity<T>()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if (heldEntity == null)
		{
			return false;
		}
		return heldEntity is T;
	}

	public Item GetActiveItem()
	{
		if (base.isServer)
		{
			if (!svActiveItemID.IsValid)
			{
				return null;
			}
			if (IsDead())
			{
				return null;
			}
			if (inventory == null || inventory.containerBelt == null)
			{
				return null;
			}
			return inventory.containerBelt.FindItemByUID(svActiveItemID);
		}
		return null;
	}

	public bool TryGetActiveItem(out Item item)
	{
		item = GetActiveItem();
		return item != null;
	}

	public Shield GetActiveShield()
	{
		if (!GetHeldEntity())
		{
			return null;
		}
		if (!GetHeldEntity().canBeUsedWithShield)
		{
			return null;
		}
		Item anyBackpack = inventory.GetAnyBackpack();
		if (anyBackpack != null && anyBackpack.info.TryGetComponent<ItemModShield>(out var _))
		{
			return anyBackpack.GetHeldEntity() as Shield;
		}
		return null;
	}

	public bool TryGetActiveShield(out Shield foundShield)
	{
		foundShield = GetActiveShield();
		return foundShield != null;
	}

	public bool WantsShieldOnBack()
	{
		if (base.isServer)
		{
			return GetInfoBool("client.shieldonback", defaultVal: false);
		}
		return false;
	}

	public bool IsHostileItem(Item item)
	{
		if (!item.info.isHoldable)
		{
			return false;
		}
		ItemModEntity component = item.info.GetComponent<ItemModEntity>();
		if (component == null)
		{
			return false;
		}
		GameObject gameObject = component.entityPrefab.Get();
		if (gameObject == null)
		{
			return false;
		}
		AttackEntity component2 = gameObject.GetComponent<AttackEntity>();
		if (component2 == null)
		{
			return false;
		}
		return component2.hostile;
	}

	public bool IsItemHoldRestricted(Item item)
	{
		if (IsNpc)
		{
			return false;
		}
		if (InSafeZone() && item != null && IsHostileItem(item) && !HasPlayerFlag(PlayerFlags.CombatZone))
		{
			return true;
		}
		return false;
	}

	public virtual void HeldEntityServerTick()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if (heldEntity != null)
		{
			heldEntity.ServerTick(this);
			if (heldEntity.canBeUsedWithShield && TryGetActiveShield(out var foundShield))
			{
				foundShield.ServerTick(this);
			}
		}
	}

	public void LightToggle(bool mask = true)
	{
		Item activeItem = GetActiveItem();
		if (activeItem != null)
		{
			BaseEntity heldEntity = activeItem.GetHeldEntity();
			if (heldEntity != null)
			{
				HeldEntity component = heldEntity.GetComponent<HeldEntity>();
				if ((bool)component)
				{
					component.SendMessage("SetLightsOn", mask && !component.LightsOn(), SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		foreach (Item item in inventory.containerWear.itemList)
		{
			ItemModWearable component2 = item.info.GetComponent<ItemModWearable>();
			if ((bool)component2 && component2.emissive)
			{
				LightToggle(item, mask);
			}
		}
		if (isMounted)
		{
			GetMounted().LightToggle(this);
		}
	}

	public void LightToggleItem(ulong itemUID)
	{
		Item item = inventory.FindItemByUID(new ItemId(itemUID));
		if (item != null)
		{
			LightToggle(item, mask: true);
		}
	}

	public void LightToggleEntity(ulong itemUID)
	{
		HeldEntity heldEntity = GetActiveItem()?.GetHeldEntity()?.GetComponent<HeldEntity>();
		if (heldEntity == null)
		{
			return;
		}
		if (heldEntity.net.ID.Value == itemUID)
		{
			heldEntity.SetLightsOn(!heldEntity.HasFlag(Flags.Reserved5));
		}
		else
		{
			if (!(heldEntity is BaseProjectile baseProjectile))
			{
				return;
			}
			foreach (BaseEntity child in baseProjectile.children)
			{
				if (child.net.ID.Value == itemUID && child is ProjectileWeaponMod projectileWeaponMod)
				{
					bool flag = !projectileWeaponMod.HasFlag(Flags.On);
					using (FlagsUpdateScope flagsUpdateScope = projectileWeaponMod.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
					{
						flagsUpdateScope.Set(Flags.On, flag);
					}
					if (projectileWeaponMod.isLight)
					{
						heldEntity.SetLightsOn(flag);
					}
				}
			}
		}
	}

	public void LightToggle(Item item, bool mask)
	{
		if (item != null)
		{
			item.SetFlag(Item.Flag.IsOn, mask && !item.HasFlag(Item.Flag.IsOn));
			item.MarkDirty();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	private void ReqLightToggle(RPCMessage msg)
	{
		ulong itemUID = msg.read.UInt64();
		LightToggleItem(itemUID);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ReqLightToggleEntity(RPCMessage msg)
	{
		ulong itemUID = msg.read.UInt64();
		LightToggleEntity(itemUID);
	}

	public void ClearDeathMarker(bool sendToClient = false)
	{
		if (!IsNpc)
		{
			if (ServerCurrentDeathNote != null)
			{
				Facepunch.Pool.Free(ref State.deathMarker);
			}
			DirtyPlayerState();
			if (sendToClient)
			{
				SendMarkersToClient();
			}
		}
	}

	public void Server_LogDeathMarker(Vector3 position)
	{
		if (!IsNpc)
		{
			if (ServerCurrentDeathNote == null)
			{
				ServerCurrentDeathNote = Facepunch.Pool.Get<MapNote>();
				ServerCurrentDeathNote.noteType = 0;
			}
			ServerCurrentDeathNote.worldPosition = position;
			ClientRPC(RpcTarget.Player("Client_AddNewDeathMarker", this), ServerCurrentDeathNote);
			DirtyPlayerState();
		}
	}

	[RPC_Server.CallsPerSecond(8uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void Server_AddMarker(RPCMessage msg)
	{
		MapNote mapNote = msg.read.Proto<MapNote>();
		if (Interface.CallHook("OnMapMarkerAdd", this, mapNote) != null || !CanUseMapMarkers)
		{
			return;
		}
		if (State.pointsOfInterest == null)
		{
			State.pointsOfInterest = Facepunch.Pool.Get<List<MapNote>>();
		}
		if (State.pointsOfInterest.Count >= ConVar.Server.maximumMapMarkers)
		{
			msg.player.ShowToast(GameTip.Styles.Blue_Short, MarkerLimitPhrase, false, ConVar.Server.maximumMapMarkers.ToString());
			return;
		}
		if (mapNote.label == "auto-name")
		{
			int num = FindUnusedNumberName();
			if (num != -1)
			{
				mapNote.label = num.ToString();
			}
		}
		ValidateMapNote(mapNote);
		if (mapNote.colourIndex == -1)
		{
			mapNote.colourIndex = FindUnusedPointOfInterestColour();
		}
		State.pointsOfInterest.Add(mapNote);
		DirtyPlayerState();
		SendMarkersToClient();
		TeamUpdate();
		Interface.CallHook("OnMapMarkerAdded", this, mapNote);
	}

	private int FindUnusedNumberName(int maxToCheck = 100)
	{
		List<MapNote> pointsOfInterest = State.pointsOfInterest;
		for (int i = 1; i < maxToCheck; i++)
		{
			bool flag = false;
			foreach (MapNote item in pointsOfInterest)
			{
				if (item.label == i.ToString())
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return -1;
	}

	private int FindUnusedPointOfInterestColour()
	{
		if (State.pointsOfInterest == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (HasColour(num))
			{
				num++;
			}
		}
		return num;
		bool HasColour(int index)
		{
			foreach (MapNote item in State.pointsOfInterest)
			{
				if (item.colourIndex == index)
				{
					return true;
				}
			}
			return false;
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_UpdateMarker(RPCMessage msg)
	{
		if (State.pointsOfInterest == null)
		{
			State.pointsOfInterest = Facepunch.Pool.Get<List<MapNote>>();
		}
		int num = msg.read.Int32();
		if (State.pointsOfInterest.Count <= num)
		{
			return;
		}
		using MapNote mapNote = msg.read.Proto<MapNote>();
		ValidateMapNote(mapNote);
		mapNote.CopyTo(State.pointsOfInterest[num]);
		DirtyPlayerState();
		SendMarkersToClient();
		TeamUpdate();
	}

	private void ValidateMapNote(MapNote n)
	{
		if (n.label != null)
		{
			n.label = Facepunch.Extend.StringExtensions.Truncate(n.label, 10).ToUpperInvariant();
		}
	}

	[RPC_Server.CallsPerSecond(10uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void Server_RemovePointOfInterest(RPCMessage msg)
	{
		int num = msg.read.Int32();
		if (State.pointsOfInterest != null && State.pointsOfInterest.Count > num && num >= 0 && Interface.CallHook("OnMapMarkerRemove", this, State.pointsOfInterest, num) == null)
		{
			State.pointsOfInterest[num].Dispose();
			State.pointsOfInterest.RemoveAt(num);
			DirtyPlayerState();
			SendMarkersToClient();
			TeamUpdate();
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	public void Server_RequestMarkers(RPCMessage msg)
	{
		SendMarkersToClient();
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_ClearMapMarkers(RPCMessage msg)
	{
		if (Interface.CallHook("OnMapMarkersClear", this, State.pointsOfInterest) != null)
		{
			return;
		}
		ServerCurrentDeathNote?.Dispose();
		ServerCurrentDeathNote = null;
		if (State.pointsOfInterest != null)
		{
			foreach (MapNote item in State.pointsOfInterest)
			{
				item?.Dispose();
			}
			State.pointsOfInterest.Clear();
		}
		DirtyPlayerState();
		TeamUpdate();
		Interface.CallHook("OnMapMarkersCleared", this);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(8uL)]
	[RPC_Server]
	public void Server_ClearPointsOfInterest(RPCMessage msg)
	{
		if (State.pointsOfInterest != null)
		{
			foreach (MapNote item in State.pointsOfInterest)
			{
				item?.Dispose();
			}
			State.pointsOfInterest.Clear();
		}
		DirtyPlayerState();
		TeamUpdate();
	}

	public void SendMarkersToClient()
	{
		using MapNoteList mapNoteList = Facepunch.Pool.Get<MapNoteList>();
		mapNoteList.notes = Facepunch.Pool.Get<List<MapNote>>();
		if (ServerCurrentDeathNote != null)
		{
			mapNoteList.notes.Add(ServerCurrentDeathNote);
		}
		if (State.pointsOfInterest != null)
		{
			mapNoteList.notes.AddRange(State.pointsOfInterest);
		}
		Interface.CallHook("OnPlayerMarkersSend", this, mapNoteList);
		ClientRPC(RpcTarget.Player("Client_ReceiveMarkers", this), mapNoteList);
		mapNoteList.notes.Clear();
	}

	public bool HasAttemptedMission(uint missionID)
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			if (acceptedMissions[i].missionID == missionID)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMissionActive()
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			if (acceptedMissions[i].IsActive())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCompletedMission(uint missionID)
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[i];
			if (missionInstance.missionID == missionID && missionInstance.status == BaseMission.MissionStatus.Completed)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFailedMission(uint missionID)
	{
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[i];
			if (missionInstance.missionID == missionID && missionInstance.status == BaseMission.MissionStatus.Failed)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanAcceptMission(BaseMission mission)
	{
		using (TimeWarning.New("BasePlayer-Mission.CanAcceptMission"))
		{
			if (HasActiveMission())
			{
				return false;
			}
			if (mission.prerequisiteMissions != null && mission.prerequisiteMissions.Length != 0)
			{
				BaseMission.MissionDependancy[] prerequisiteMissions = mission.prerequisiteMissions;
				foreach (BaseMission.MissionDependancy missionDependancy in prerequisiteMissions)
				{
					bool flag = false;
					for (int j = 0; j < acceptedMissions.Count; j++)
					{
						BaseMission.MissionInstance missionInstance = acceptedMissions[j];
						if (missionInstance.missionID == missionDependancy.missionID && missionInstance.status == missionDependancy.desiredStatus)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			uint id = mission.id;
			if (mission.isRepeatable)
			{
				bool num = HasCompletedMission(id);
				bool flag2 = HasFailedMission(id);
				if (num && mission.repeatDelaySecondsSuccess <= -1)
				{
					return false;
				}
				if (flag2 && mission.repeatDelaySecondsFailed <= -1)
				{
					return false;
				}
				for (int k = 0; k < acceptedMissions.Count; k++)
				{
					BaseMission.MissionInstance missionInstance2 = acceptedMissions[k];
					if (missionInstance2.missionID == id && missionInstance2.endTimeUtcSeconds != long.MinValue)
					{
						float num2 = 0f;
						if (missionInstance2.status == BaseMission.MissionStatus.Completed)
						{
							num2 = mission.repeatDelaySecondsSuccess;
						}
						else if (missionInstance2.status == BaseMission.MissionStatus.Failed)
						{
							num2 = mission.repeatDelaySecondsFailed;
						}
						if ((float)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - missionInstance2.endTimeUtcSeconds) * ConVar.Time.missiontimerscale < num2)
						{
							return false;
						}
					}
				}
			}
			else if (HasCompletedMission(id))
			{
				return false;
			}
			return true;
		}
	}

	public bool Server_CanAcceptMission(IMissionProvider provider, uint missionId)
	{
		BaseMission fromID = MissionManifest.GetFromID(missionId);
		if (fromID == null)
		{
			Debug.LogError($"Mission ID {missionId} not found in manifest");
			return false;
		}
		return Server_CanAcceptMission(provider.ProviderID(), fromID);
	}

	public bool Server_CanAcceptMission(IMissionProvider provider, BaseMission mission)
	{
		return Server_CanAcceptMission(provider.ProviderID(), mission);
	}

	public bool Server_CanAcceptMission(NetworkableId providerNetId, BaseMission mission)
	{
		mission.Server_UpdateMissionValidState(providerNetId, out var isValid);
		if (isValid)
		{
			return CanAcceptMission(mission);
		}
		return false;
	}

	private void PrepareMissionsForTutorial()
	{
		using PooledList<int> pooledList = Facepunch.Pool.Get<PooledList<int>>();
		if (acceptedMissions.Count > 0)
		{
			for (int num = acceptedMissions.Count - 1; num >= 0; num--)
			{
				BaseMission.MissionInstance missionInstance = acceptedMissions[num];
				if (missionInstance != null)
				{
					if (missionInstance.IsActive())
					{
						missionInstance.GetMission().MissionFailed(missionInstance, this, BaseMission.MissionFailReason.ResetPlayerState, saveImmediately: false);
					}
					if (missionInstance.GetMission() is TutorialMission)
					{
						pooledList.Add(num);
					}
				}
			}
		}
		for (int i = 0; i < pooledList.Count; i++)
		{
			int index = pooledList[i];
			BaseMission.MissionInstance obj = acceptedMissions[index];
			Facepunch.Pool.Free(ref obj);
			acceptedMissions.RemoveAt(index);
		}
		SetActiveMissionIndex(-1);
		MissionsDirty(saveImmediately: true);
	}

	public void AbandonActiveMission()
	{
		if (TryGetActiveMissionInstance(out var instance))
		{
			instance.GetMission().MissionFailed(instance, this, BaseMission.MissionFailReason.Abandon);
		}
	}

	public void ServerThinkMissions(float delta)
	{
		if (timeSinceServerMissionThink < 1f)
		{
			timeSinceServerMissionThink += delta;
			return;
		}
		try
		{
			for (int i = 0; i < acceptedMissions.Count; i++)
			{
				BaseMission.MissionInstance missionInstance = acceptedMissions[i];
				try
				{
					missionInstance.ServerThink(this, timeSinceServerMissionThink);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
		finally
		{
		}
		timeSinceServerMissionThink = 0f;
	}

	private static void ServerThinkMissionsParallel(in PlayerServerStates.ReadOnly playerStates, float delta)
	{
		using (TimeWarning.New("ServerThinkMissionsParallel"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].ServerThinkMissions(delta);
			}
		}
	}

	public void MissionsDirty(bool saveImmediately = false)
	{
		if (BaseMission.missionsenabled)
		{
			_missionsDirty = true;
			if (saveImmediately)
			{
				SaveMissionsIfDirty();
			}
		}
	}

	public void SaveMissionsIfDirty()
	{
		if (_missionsDirty && BaseMission.missionsenabled)
		{
			UpdatePlayerStateMissionsData();
			SendNetworkUpdate();
			_missionsDirty = false;
		}
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, uint identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			UintIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, uint identifier, float amount, Vector3 worldPos)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			UintIdentifier = identifier,
			WorldPosition = worldPos
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, int identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			IntIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, NetworkableId identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			NetworkIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, string identifier, float amount)
	{
		ProcessMissionEvent(type, new BaseMission.MissionEventPayload
		{
			StringIdentifier = identifier
		}, amount);
	}

	public void ProcessMissionEvent(BaseMission.MissionEventType type, BaseMission.MissionEventPayload payload, float amount)
	{
		if (BaseMission.missionsenabled && acceptedMissions != null)
		{
			for (int i = 0; i < acceptedMissions.Count; i++)
			{
				acceptedMissions[i].ProcessMissionEvent(this, type, payload, amount);
			}
		}
	}

	public void RegisterFollowupMission(BaseMission targetMission, IMissionProvider provider)
	{
		followupMission = targetMission;
		followupMissionProvider = provider;
		if (followupMission != null && followupMissionProvider != null)
		{
			Invoke(actionAssignFollowUpMission, 1.5f);
		}
	}

	private void AssignFollowUpMission()
	{
		if (followupMission != null && followupMissionProvider != null)
		{
			BaseMission.AssignMission(this, followupMissionProvider, followupMission);
		}
		followupMission = null;
		followupMissionProvider = null;
	}

	private void UpdatePlayerStateMissionsData()
	{
		State.missions?.Dispose();
		State.missions = Facepunch.Pool.Get<Missions>();
		State.missions.missions = Facepunch.Pool.Get<List<MissionInstance>>();
		int num = GetActiveMissionIndex();
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance = acceptedMissions[i];
			BaseMission mission = null;
			bool flag = missionInstance != null && MissionManifest.TryGetFromID(missionInstance.missionID, out mission);
			bool flag2 = missionInstance != null && missionInstance.status != 0 && missionInstance.status != BaseMission.MissionStatus.Pending;
			if (missionInstance == null || !flag || !flag2)
			{
				if (missionInstance == null)
				{
					Debug.LogError($"Null mission instance at index {i} on player {base.name}", this);
				}
				else
				{
					if (!flag)
					{
						Debug.LogError($"Failed to find a mission for instance ID {missionInstance.missionID} at index {i} on player {base.name}", this);
					}
					if (!flag2)
					{
						Debug.LogError($"Mission for instance ID {missionInstance.missionID} at index {i} on player {base.name} has invalid status: {missionInstance.status}", this);
					}
				}
				if (num != -1 && i == num)
				{
					num = -1;
				}
				continue;
			}
			MissionInstance missionInstance2 = Facepunch.Pool.Get<MissionInstance>();
			missionInstance2.missionID = missionInstance.missionID;
			missionInstance2.missionStatus = (uint)missionInstance.status;
			MissionInstanceData missionInstanceData = Facepunch.Pool.Get<MissionInstanceData>();
			missionInstanceData.providerID = missionInstance.providerID;
			missionInstanceData.startTimeUtcSeconds = missionInstance.startTimeUtcSeconds;
			missionInstanceData.endTimeUtcSeconds = missionInstance.endTimeUtcSeconds;
			missionInstanceData.hasDispensedRewards = missionInstance.hasDispensedRewards;
			missionInstanceData.missionPoints = Facepunch.Pool.Get<List<ProtoBuf.MissionPoint>>();
			foreach (KeyValuePair<string, Vector3> missionPoint2 in missionInstance.missionPoints)
			{
				ProtoBuf.MissionPoint missionPoint = Facepunch.Pool.Get<ProtoBuf.MissionPoint>();
				missionPoint.identifier = missionPoint2.Key;
				missionPoint.location = missionPoint2.Value;
				missionInstanceData.missionPoints.Add(missionPoint);
			}
			missionInstanceData.objectiveStatuses = Facepunch.Pool.Get<List<ObjectiveStatus>>();
			int count = missionInstance.objectiveStatuses.Count;
			int num2 = mission.objectives.Length;
			if (count != num2)
			{
				Debug.LogError($"Mission instance for mission {mission.name} contains data for {count} objectives but mission has {num2} objectives", mission);
			}
			for (int j = 0; j < count; j++)
			{
				BaseMission.MissionInstance.ObjectiveStatus objectiveStatus = missionInstance.objectiveStatuses[j];
				ObjectiveStatus objectiveStatus2 = Facepunch.Pool.Get<ObjectiveStatus>();
				objectiveStatus2.softCompleted = objectiveStatus.softCompleted;
				objectiveStatus2.blockReset = objectiveStatus.blockReset;
				objectiveStatus2.completed = objectiveStatus.completed;
				objectiveStatus2.failed = objectiveStatus.failed;
				objectiveStatus2.started = objectiveStatus.started;
				objectiveStatus2.progressCurrent = objectiveStatus.progressCurrent;
				objectiveStatus2.progressTarget = objectiveStatus.progressTarget;
				objectiveStatus2.worldLocation = objectiveStatus.worldLocation;
				missionInstanceData.objectiveStatuses.Add(objectiveStatus2);
			}
			missionInstanceData.missionEntities = Facepunch.Pool.Get<List<ProtoBuf.MissionEntity>>();
			foreach (KeyValuePair<string, MissionEntity> spawnedMissionEntity in missionInstance.spawnedMissionEntities)
			{
				BaseEntity baseEntity = ((spawnedMissionEntity.Value != null) ? spawnedMissionEntity.Value.GetEntity() : null);
				if (baseEntity.IsValid())
				{
					ProtoBuf.MissionEntity missionEntity = Facepunch.Pool.Get<ProtoBuf.MissionEntity>();
					missionEntity.identifier = spawnedMissionEntity.Key;
					missionEntity.entityID = baseEntity.net.ID;
					missionInstanceData.missionEntities.Add(missionEntity);
				}
			}
			missionInstanceData.persistentMissionEntities = Facepunch.Pool.Get<List<PersistentMissionEntityData>>();
			for (int k = 0; k < missionInstance.persistentMissionEntities.Count; k++)
			{
				BaseEntity baseEntity2 = missionInstance.persistentMissionEntities[k];
				if (baseEntity2.IsValid())
				{
					PersistentMissionEntityData persistentMissionEntityData = Facepunch.Pool.Get<PersistentMissionEntityData>();
					persistentMissionEntityData.entityID = baseEntity2.net.ID;
					missionInstanceData.persistentMissionEntities.Add(persistentMissionEntityData);
				}
			}
			missionInstance2.instanceData = missionInstanceData;
			State.missions.missions.Add(missionInstance2);
		}
		State.missions.activeMission = num;
		DirtyPlayerState();
	}

	public void WipeMissions(bool saveImmediately)
	{
		for (int num = acceptedMissions.Count - 1; num >= 0; num--)
		{
			BaseMission.MissionInstance obj = acceptedMissions[num];
			if (obj != null)
			{
				obj.GetMission().MissionFailed(obj, this, BaseMission.MissionFailReason.ResetPlayerState, saveImmediately: false);
				Facepunch.Pool.Free(ref obj);
			}
		}
		acceptedMissions.Clear();
		SetActiveMissionIndex(-1);
		MissionsDirty(saveImmediately);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	private void Server_RequestValidMissionsUpdate(RPCMessage _)
	{
		if (IsNpc)
		{
			Debug.LogError(base.name + " is a NPC, cannot proceed", this);
		}
		else
		{
			BaseMission.PlayerRequestedValidStatesUpdate(this);
		}
	}

	public void Server_SendValidMissionStates()
	{
		using MissionAcceptStatesList missionAcceptStatesList = Facepunch.Pool.Get<MissionAcceptStatesList>();
		missionAcceptStatesList.missionAcceptStates = Facepunch.Pool.Get<List<MissionAcceptState>>();
		foreach (KeyValuePair<BaseMission.MissionIdentifierData, BaseMission.MissionValidStateData> server_missionInstanceValidState in BaseMission.server_missionInstanceValidStates)
		{
			MissionAcceptState missionAcceptState = Facepunch.Pool.Get<MissionAcceptState>();
			missionAcceptState.providerNetId = server_missionInstanceValidState.Key.missionProviderNetId;
			missionAcceptState.missionID = server_missionInstanceValidState.Key.mission.id;
			missionAcceptState.canAccept = server_missionInstanceValidState.Value.isValid;
			missionAcceptStatesList.missionAcceptStates.Add(missionAcceptState);
		}
		ClientRPC(RpcTarget.Player("Client_ReceiveValidMissionStates", this), missionAcceptStatesList);
	}

	public void Server_SendCanAcceptMissionsFromProvider(IMissionProvider missionProvider)
	{
		if (IsNpc)
		{
			Debug.LogError(base.name + " is a NPC, cannot proceed", this);
			return;
		}
		using MissionAcceptStatesList missionAcceptStatesList = Facepunch.Pool.Get<MissionAcceptStatesList>();
		missionAcceptStatesList.missionAcceptStates = Facepunch.Pool.Get<List<MissionAcceptState>>();
		BufferList<BaseMission> allMissions = missionProvider.GetAllMissions();
		int count = allMissions.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				BaseMission baseMission = allMissions[i];
				MissionAcceptState missionAcceptState = Facepunch.Pool.Get<MissionAcceptState>();
				missionAcceptState.providerNetId = missionProvider.ProviderID();
				missionAcceptState.missionID = baseMission.id;
				missionAcceptState.canAccept = Server_CanAcceptMission(missionProvider, baseMission);
				missionAcceptStatesList.missionAcceptStates.Add(missionAcceptState);
			}
			ClientRPC(RpcTarget.Player("Client_ReceiveMissionStatesForProvider", this), missionAcceptStatesList);
		}
	}

	public void Server_SendMissionFailed(uint missionID, BaseMission.MissionFailReason reason)
	{
		if (IsNpc)
		{
			Debug.LogError(base.name + " is a NPC, cannot proceed", this);
		}
		else
		{
			ClientRPC(RpcTarget.Player("Client_ReceiveMissionFailed", this), missionID, (int)reason);
		}
	}

	public void SetActiveMissionIndex(int index)
	{
		_activeMissionIndex = index;
		if (IsInTutorial && GetCurrentTutorialIsland() != null)
		{
			GetCurrentTutorialIsland().OnPlayerStartedMission(this);
		}
	}

	public int GetActiveMissionIndex()
	{
		return _activeMissionIndex;
	}

	public bool HasActiveMission()
	{
		bool flag = GetActiveMissionIndex() != -1;
		bool flag2 = false;
		BaseMission.MissionInstance missionInstance = null;
		for (int i = 0; i < acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance2 = acceptedMissions[i];
			if (missionInstance2.IsActive())
			{
				missionInstance = missionInstance2;
				flag2 = true;
				break;
			}
		}
		if (flag != flag2)
		{
			string arg = ((missionInstance == null) ? "null" : $"ID: {missionInstance.missionID.ToString()}, mission: {missionInstance.GetMission().name}, status: {missionInstance.status}");
			Debug.LogWarning($"Discrepancy between active mission index {GetActiveMissionIndex()} and active mission instance {arg}");
		}
		return flag || flag2;
	}

	public BaseMission.MissionInstance GetActiveMissionInstance()
	{
		int activeMissionIndex = GetActiveMissionIndex();
		if (activeMissionIndex >= 0 && activeMissionIndex < acceptedMissions.Count)
		{
			return acceptedMissions[activeMissionIndex];
		}
		return null;
	}

	public bool TryGetActiveMissionInstance(out BaseMission.MissionInstance instance)
	{
		instance = GetActiveMissionInstance();
		return instance != null;
	}

	private void LoadMissions(Missions loadedMissions)
	{
		if (acceptedMissions.Count > 0)
		{
			for (int num = acceptedMissions.Count - 1; num >= 0; num--)
			{
				BaseMission.MissionInstance obj = acceptedMissions[num];
				if (obj != null)
				{
					Facepunch.Pool.Free(ref obj);
				}
			}
		}
		acceptedMissions.Clear();
		bool flag = true;
		if (loadedMissions != null && loadedMissions.missions != null && loadedMissions.missions.Count > 0)
		{
			for (int i = 0; i < loadedMissions.missions.Count; i++)
			{
				MissionInstance missionInstance = loadedMissions.missions[i];
				if (!MissionManifest.TryGetFromID(missionInstance.missionID, out var mission))
				{
					flag = false;
					continue;
				}
				BaseMission.MissionInstance missionInstance2 = Facepunch.Pool.Get<BaseMission.MissionInstance>();
				missionInstance2.missionID = missionInstance.missionID;
				missionInstance2.status = (BaseMission.MissionStatus)missionInstance.missionStatus;
				MissionInstanceData instanceData = missionInstance.instanceData;
				if (instanceData != null)
				{
					missionInstance2.providerID = instanceData.providerID;
					missionInstance2.startTimeUtcSeconds = instanceData.startTimeUtcSeconds;
					missionInstance2.endTimeUtcSeconds = instanceData.endTimeUtcSeconds;
					missionInstance2.hasDispensedRewards = instanceData.hasDispensedRewards;
					if (base.isServer && instanceData.missionPoints != null)
					{
						for (int j = 0; j < instanceData.missionPoints.Count; j++)
						{
							ProtoBuf.MissionPoint missionPoint = instanceData.missionPoints[j];
							string identifier = missionPoint.identifier;
							Vector3 location = missionPoint.location;
							missionInstance2.missionPoints.Add(identifier, location);
							if (missionInstance2.IsActive() && mission.TryGetPositionGenerator(identifier, out var positionGenerator) && positionGenerator.positionsAreExclusive)
							{
								BaseMission.AddPositionBlocker(missionInstance2, location);
							}
						}
					}
					int count = instanceData.objectiveStatuses.Count;
					int num2 = mission.objectives.Length;
					if (base.isServer && count != num2)
					{
						Debug.LogError($"Loaded mission instance data for mission {mission.name} contains data for {count} objectives but mission has {num2} objectives. Loaded mission points: {GenerateMissionPointsDataDebug(instanceData)}", mission);
					}
					for (int k = 0; k < count; k++)
					{
						ObjectiveStatus objectiveStatus = instanceData.objectiveStatuses[k];
						BaseMission.MissionInstance.ObjectiveStatus objectiveStatus2 = Facepunch.Pool.Get<BaseMission.MissionInstance.ObjectiveStatus>();
						objectiveStatus2.started = objectiveStatus.started;
						objectiveStatus2.softCompleted = objectiveStatus.softCompleted;
						objectiveStatus2.blockReset = objectiveStatus.blockReset;
						objectiveStatus2.completed = objectiveStatus.completed;
						objectiveStatus2.failed = objectiveStatus.failed;
						objectiveStatus2.progressTarget = objectiveStatus.progressTarget;
						objectiveStatus2.progressCurrent = objectiveStatus.progressCurrent;
						objectiveStatus2.worldLocation = objectiveStatus.worldLocation;
						missionInstance2.objectiveStatuses.Add(objectiveStatus2);
					}
					if (base.isServer)
					{
						if (instanceData.missionEntities != null)
						{
							missionInstance2.spawnedMissionEntities.Clear();
							BaseMission mission2 = missionInstance2.GetMission();
							for (int l = 0; l < instanceData.missionEntities.Count; l++)
							{
								ProtoBuf.MissionEntity missionEntity = instanceData.missionEntities[l];
								MissionEntity missionEntity2 = null;
								if (BaseNetworkable.serverEntities.TryGetEntity(missionEntity.entityID, out var entity))
								{
									missionEntity2 = (entity.gameObject.TryGetComponent<MissionEntity>(out var component) ? component : entity.gameObject.AddComponent<MissionEntity>());
									BaseMission.MissionEntityEntry missionEntityEntry = ((mission2 != null) ? ((IReadOnlyCollection<BaseMission.MissionEntityEntry>)(object)mission2.spawnMissionEntityDefinitions).FindWith((BaseMission.MissionEntityEntry ed) => ed.identifier, missionEntity.identifier) : null);
									missionEntity2.Setup(this, missionInstance2, missionEntity.identifier, missionEntityEntry?.cleanupOnMissionSuccess ?? true, missionEntityEntry?.cleanupOnMissionFailed ?? true);
								}
								missionInstance2.spawnedMissionEntities.Add(missionEntity.identifier, missionEntity2);
							}
						}
						if (instanceData.persistentMissionEntities != null)
						{
							missionInstance2.persistentMissionEntities.Clear();
							for (int m = 0; m < instanceData.persistentMissionEntities.Count; m++)
							{
								PersistentMissionEntityData persistentMissionEntityData = instanceData.persistentMissionEntities[m];
								if (BaseNetworkable.serverEntities.TryGetEntity(persistentMissionEntityData.entityID, out var entity2))
								{
									missionInstance2.persistentMissionEntities.TryAdd(entity2);
								}
							}
						}
					}
				}
				acceptedMissions.Add(missionInstance2);
			}
		}
		else
		{
			flag = false;
		}
		SetActiveMissionIndex(flag ? loadedMissions.activeMission : (-1));
		if (base.isServer && TryGetActiveMissionInstance(out var instance))
		{
			instance.PostServerLoad(this);
		}
	}

	public bool HasSpaceForMissionRewards(BaseMission.MissionInstance missionInstance, bool showToastOnFailure = false)
	{
		if (!missionInstance.TryGetTotalRequiredRewardItemSlots(out var requiredSlots))
		{
			if (showToastOnFailure)
			{
				ShowToast(GameTip.Styles.Red_Normal, FailedToCheckRewardsSpace, false);
			}
			return false;
		}
		if (!inventory.HasEmptySlots(requiredSlots))
		{
			if (showToastOnFailure)
			{
				ShowToast(GameTip.Styles.Red_Normal, NoSpaceInInventoryPhrase, false);
			}
			return false;
		}
		return true;
	}

	private string GenerateMissionPointsDataDebug(MissionInstanceData missionInstanceData)
	{
		string text = string.Empty;
		if (missionInstanceData.missionPoints != null)
		{
			for (int i = 0; i < missionInstanceData.missionPoints.Count; i++)
			{
				text = text + "\n" + missionInstanceData.missionPoints[i].identifier + ": " + missionInstanceData.missionPoints[i].location;
			}
		}
		return text;
	}

	private void UpdateModelState(ModelState ms)
	{
		if (!IsDead() && !IsSpectating() && !isInvisible)
		{
			ms.sleeping = IsSleeping();
			ms.mounted = isMounted;
			ms.ragdolling = IsRagdolling();
			ms.relaxed = IsRelaxed();
			ms.crawling = IsCrawling();
			ms.loading = IsLoadingAfterTransfer();
		}
	}

	public void SendModelState(bool force = false)
	{
		if (force || lastModelState == null || HasModelStateChanged())
		{
			if (lastModelState == null)
			{
				lastModelState = modelState.Copy();
			}
			else
			{
				modelState.CopyTo(lastModelState);
			}
			if (!base.limitNetworking && Interface.CallHook("OnSendModelState", this) == null)
			{
				ClientRPC(RpcTarget.NetworkGroup("OnModelState"), modelState);
			}
		}
	}

	private bool HasModelStateChanged()
	{
		if (lastModelState == null)
		{
			return true;
		}
		return !ModelState.Equal(lastModelState, modelState);
	}

	public BaseMountable GetMounted()
	{
		return mounted.Get(base.isServer) as BaseMountable;
	}

	public BaseVehicle GetMountedVehicle()
	{
		BaseMountable baseMountable = GetMounted();
		if (!baseMountable.IsValid())
		{
			return null;
		}
		return baseMountable.VehicleParent();
	}

	public void MarkSwapSeat()
	{
		nextSeatSwapTime = UnityEngine.Time.time + 0.75f;
	}

	public bool SwapSeatCooldown()
	{
		return UnityEngine.Time.time < nextSeatSwapTime;
	}

	public bool CanMountMountablesNow()
	{
		if (!IsDead())
		{
			return !IsWounded();
		}
		return false;
	}

	public void SetMounted(BaseMountable mount)
	{
		mounted.Set(mount);
		RefreshColliderSize(forced: true);
		if (ActivePlayerInd != -1)
		{
			PlayerStates.IsMounted[ActivePlayerInd] = mount.IsValid();
			PlayerStates.Mountables[ActivePlayerInd] = mount;
		}
	}

	public void EnsureDismounted()
	{
		if (isMounted)
		{
			GetMounted().DismountPlayer(this);
		}
	}

	public virtual void DismountObject()
	{
		SetMounted(null);
		SendNetworkUpdate();
		PauseSpeedHackDetection(5f);
		PauseTickDistanceDetection(5f);
	}

	public void HandleMountedOnLoad()
	{
		if (!mounted.IsValid(base.isServer))
		{
			return;
		}
		BaseMountable baseMountable = mounted.Get(base.isServer) as BaseMountable;
		if (baseMountable != null)
		{
			baseMountable.MountPlayer(this);
			if (!AllowSleeperMounting(baseMountable))
			{
				baseMountable.DismountPlayer(this);
			}
		}
		else
		{
			SetMounted(null);
		}
	}

	public bool AllowSleeperMounting(BaseMountable mountable)
	{
		if (mountable.allowSleeperMounting)
		{
			return true;
		}
		if (!IsLoadingAfterTransfer())
		{
			return IsTransferProtected();
		}
		return true;
	}

	public bool IsBlockedFromLootingByMountable()
	{
		return GetMounted()?.blockLooting ?? false;
	}

	public PlayerSecondaryData SaveSecondaryData()
	{
		PlayerSecondaryData playerSecondaryData = Facepunch.Pool.Get<PlayerSecondaryData>();
		playerSecondaryData.userId = userID;
		PlayerState playerState = State.Copy();
		if (playerState.pointsOfInterest != null)
		{
			Facepunch.Pool.Free(ref playerState.pointsOfInterest, freeElements: true);
		}
		if (playerState.pings != null)
		{
			Facepunch.Pool.Free(ref playerState.pings, freeElements: true);
		}
		playerState.deathMarker?.Dispose();
		playerState.deathMarker = null;
		playerState.missions?.Dispose();
		playerState.missions = null;
		playerState.numberOfTimesReported = 0;
		playerSecondaryData.playerState = playerState;
		if (currentTeam != 0L)
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(currentTeam);
			if (playerTeam != null)
			{
				playerSecondaryData.teamId = playerTeam.teamID;
				playerSecondaryData.isTeamLeader = playerTeam.teamLeader == (ulong)userID;
			}
		}
		playerSecondaryData.relationships = Facepunch.Pool.Get<List<PlayerSecondaryData.RelationshipData>>();
		foreach (RelationshipManager.PlayerRelationshipInfo value in RelationshipManager.ServerInstance.GetRelationships(userID).relations.Values)
		{
			PlayerSecondaryData.RelationshipData relationshipData = Facepunch.Pool.Get<PlayerSecondaryData.RelationshipData>();
			relationshipData.info = value.ToProto();
			relationshipData.mugshotData = GetPoolableMugshotData(value);
			playerSecondaryData.relationships.Add(relationshipData);
		}
		return playerSecondaryData;
		ArraySegment<byte> GetPoolableMugshotData(RelationshipManager.PlayerRelationshipInfo relationshipInfo)
		{
			if (relationshipInfo.mugshotCrc == 0)
			{
				return default(ArraySegment<byte>);
			}
			try
			{
				uint steamIdHash = RelationshipManager.GetSteamIdHash(userID, relationshipInfo.player);
				byte[] array = FileStorage.server.Get(relationshipInfo.mugshotCrc, FileStorage.Type.jpg, RelationshipManager.ServerInstance.net.ID, steamIdHash);
				if (array == null)
				{
					return default(ArraySegment<byte>);
				}
				byte[] array2 = BufferStream.Shared.ArrayPool.Rent(array.Length);
				new Span<byte>(array).CopyTo(array2);
				return new ArraySegment<byte>(array2, 0, array.Length);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				return default(ArraySegment<byte>);
			}
		}
	}

	public void LoadSecondaryData(PlayerSecondaryData data)
	{
		if (data == null)
		{
			return;
		}
		if (data.userId != (ulong)userID)
		{
			Debug.LogError($"Attempted to load secondary data with an incorrect userID! Expected {data.userId} but player has {userID.Get()}, not loading it.");
			return;
		}
		if (data.playerState != null)
		{
			State.unHostileTimestamp = data.playerState.unHostileTimestamp;
			DirtyPlayerState();
		}
		if (data.relationships == null)
		{
			return;
		}
		RelationshipManager.PlayerRelationships relationships = RelationshipManager.ServerInstance.GetRelationships(userID);
		relationships.ClearRelations();
		foreach (PlayerSecondaryData.RelationshipData relationship in data.relationships)
		{
			if (relationship.mugshotData.Count > 0)
			{
				try
				{
					byte[] array = new byte[relationship.mugshotData.Count];
					relationship.mugshotData.AsSpan().CopyTo(array);
					uint steamIdHash = RelationshipManager.GetSteamIdHash(userID, relationship.info.playerID);
					uint num = FileStorage.server.Store(array, FileStorage.Type.jpg, RelationshipManager.ServerInstance.net.ID, steamIdHash);
					if (num != relationship.info.mugshotCrc)
					{
						Debug.LogWarning($"Mugshot data for {userID.Get()}->{relationship.info.playerID} had a CRC mismatch, updating it");
						relationship.info.mugshotCrc = num;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			relationships.relations.Add(relationship.info.playerID, RelationshipManager.PlayerRelationshipInfo.FromProto(relationship.info));
		}
		RelationshipManager.ServerInstance.MarkRelationshipsDirtyFor(this);
	}

	public override void DisableTransferProtection()
	{
		BaseVehicle vehicleParent = GetVehicleParent();
		if (vehicleParent != null && vehicleParent.IsTransferProtected())
		{
			vehicleParent.DisableTransferProtection();
		}
		BaseMountable baseMountable = GetMounted();
		if (baseMountable != null && baseMountable.IsTransferProtected())
		{
			baseMountable.DisableTransferProtection();
		}
		base.DisableTransferProtection();
	}

	public void KickAfterServerTransfer()
	{
		if (IsConnected)
		{
			Kick("Redirecting to another zone...");
		}
		Kill();
	}

	public void Server_UpdatePaintballColor(int newColor)
	{
		if (PaintballColorLookup.instance == null)
		{
			Debug.LogError("Failed to retrieve PaintballColorLookup instance");
			return;
		}
		server_paintballColor = Mathf.Clamp(newColor, 0, PaintballColorLookup.instance.GetColorsCount() - 1);
		ItemDefinition paintballGunItemDefinition = PaintballColorLookup.instance.paintballGunItemDefinition;
		ItemDefinition overallsItemDefinition = PaintballColorLookup.instance.overallsItemDefinition;
		if (inventory != null)
		{
			if (TryGetActiveItem(out var item) && item.info == paintballGunItemDefinition)
			{
				if (item.instanceData == null)
				{
					item.instanceData = new ProtoBuf.Item.InstanceData();
					item.instanceData.ShouldPool = false;
				}
				item.instanceData.dataInt = server_paintballColor;
				item.MarkDirty();
			}
			if (inventory.containerWear != null)
			{
				using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
				inventory.containerWear.FindItemsByItemID(pooledList, overallsItemDefinition.itemid);
				for (int i = 0; i < pooledList.Count; i++)
				{
					Item item2 = pooledList[i];
					if (item2.instanceData == null)
					{
						item2.instanceData = new ProtoBuf.Item.InstanceData();
						item2.instanceData.ShouldPool = false;
					}
					item2.instanceData.dataInt = server_paintballColor;
					item2.MarkDirty();
				}
			}
		}
		Server_SendPaintballColorUpdate();
	}

	private void Server_SendPaintballColorUpdate()
	{
		ClientRPC(RpcTarget.Player("Client_ReceivePaintballColorUpdate", this), server_paintballColor);
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	private void RequestParachuteDeploy(RPCMessage msg)
	{
		RequestParachuteDeploy();
	}

	public void RequestParachuteDeploy()
	{
		if (isMounted || !CheckParachuteClearance())
		{
			return;
		}
		Item slot = inventory.containerWear.GetSlot(7);
		if (slot == null || !(slot.conditionNormalized > 0f) || slot.isBroken || !slot.info.TryGetComponent<ItemModParachute>(out var component))
		{
			return;
		}
		Parachute parachute = GameManager.server.CreateEntity(component.ParachuteVehiclePrefab.resourcePath, base.transform.position, eyes.rotation) as Parachute;
		if (parachute != null)
		{
			parachute.skinID = slot.skin;
			parachute.Spawn();
			parachute.SetHealth(parachute.MaxHealth() * slot.conditionNormalized);
			parachute.AttemptMount(this);
			if (isMounted)
			{
				slot.Remove();
				ItemManager.DoRemoves();
				SendNetworkUpdate();
			}
			else
			{
				parachute.Kill();
			}
		}
	}

	public bool CheckParachuteClearance()
	{
		Vector3 position = base.transform.position;
		if (!WaterLevel.Test(position - Vector3.up * 5f, waves: false, volumes: true, this) && !GamePhysics.Trace(new Ray(position, -Vector3.up), 1f, out var _, 6f, 1218674945, QueryTriggerInteraction.UseGlobal, this))
		{
			return !GamePhysics.CheckSphere(position + Vector3.up * 3.5f, 2f, 1218543873);
		}
		return false;
	}

	public bool HasValidParachuteEquipped()
	{
		if (inventory == null || inventory.containerWear == null)
		{
			return false;
		}
		Item slot = inventory.containerWear.GetSlot(7);
		if (slot != null && slot.conditionNormalized > 0f && !slot.isBroken && slot.info.TryGetComponent<ItemModParachute>(out var _))
		{
			return true;
		}
		return false;
	}

	public void ClearClientPetLink()
	{
		ClientRPC(RpcTarget.Player("CLIENT_SetPetPrefabID", this), 0u, 0uL);
	}

	public void SendClientPetLink()
	{
		if (PetEntity == null && BasePet.ActivePetByOwnerID.TryGetValue(userID, out var value) && value.Brain != null)
		{
			value.Brain.SetOwningPlayer(this);
		}
		ClientRPC(RpcTarget.Player("CLIENT_SetPetPrefabID", this), (PetEntity != null) ? PetEntity.prefabID : 0u, (PetEntity != null) ? PetEntity.net.ID : default(NetworkableId));
		if (PetEntity != null)
		{
			SendClientPetStateIndex();
		}
	}

	public void SendClientPetStateIndex()
	{
		BasePet basePet = PetEntity as BasePet;
		if (!(basePet == null))
		{
			ClientRPC(RpcTarget.Player("CLIENT_SetPetPetLoadedStateIndex", this), basePet.Brain.LoadedDesignIndex());
		}
	}

	[RPC_Server]
	private void IssuePetCommand(RPCMessage msg)
	{
		ParsePetCommand(msg, raycast: false);
	}

	[RPC_Server]
	private void IssuePetCommandRaycast(RPCMessage msg)
	{
		ParsePetCommand(msg, raycast: true);
	}

	private void ParsePetCommand(RPCMessage msg, bool raycast)
	{
		if (UnityEngine.Time.time - lastPetCommandIssuedTime <= 1f)
		{
			return;
		}
		lastPetCommandIssuedTime = UnityEngine.Time.time;
		if (!(msg.player == null) && Pet != null && Pet.IsOwnedBy(msg.player))
		{
			int cmd = msg.read.Int32();
			int param = msg.read.Int32();
			if (raycast)
			{
				Ray value = msg.read.Ray();
				Pet.IssuePetCommand((PetCommandType)cmd, param, value);
			}
			else
			{
				Pet.IssuePetCommand((PetCommandType)cmd, param, null);
			}
		}
	}

	public bool CanPing(bool disregardHeldEntity = false)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(base.isServer);
		if (activeGameMode != null && !activeGameMode.allowPings)
		{
			return false;
		}
		if ((disregardHeldEntity || GetHeldEntity() is Binocular || (isMounted && GetMounted() is ComputerStation computerStation && computerStation.AllowPings()) || (GetHeldEntity() is BaseProjectile baseProjectile && baseProjectile.AllowsPingUsage())) && IsAlive() && !IsWounded())
		{
			return !IsSpectating();
		}
		return false;
	}

	public static PingStyle GetPingStyle(PingType t)
	{
		PingStyle pingStyle = default(PingStyle);
		return t switch
		{
			PingType.Hostile => HostileMarker, 
			PingType.GoTo => GoToMarker, 
			PingType.Dollar => DollarMarker, 
			PingType.Loot => LootMarker, 
			PingType.Node => NodeMarker, 
			PingType.Gun => GunMarker, 
			PingType.Build => BuildMarker, 
			_ => pingStyle, 
		};
	}

	private void ApplyPingStyle(MapNote note, PingType type)
	{
		PingStyle pingStyle = GetPingStyle(type);
		note.colourIndex = pingStyle.ColourIndex;
		note.icon = pingStyle.IconIndex;
	}

	[RPC_Server.InputValidation(new Type[]
	{
		typeof(Vector3),
		typeof(int),
		typeof(bool)
	})]
	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(3uL)]
	private void Server_AddPing(RPCMessage msg)
	{
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		if (ConVar.Server.maximumPings == 0 || !CanPing())
		{
			return;
		}
		Vector3 vector = msg.read.Vector3();
		PingType pingType = (PingType)Mathf.Clamp(msg.read.Int32(), 0, 6);
		bool wasViaWheel = msg.read.Bit();
		PingStyle pingStyle = GetPingStyle(pingType);
		foreach (MapNote ping in State.pings)
		{
			if (ping.icon == pingStyle.IconIndex && (ping.worldPosition - vector).sqrMagnitude < 0.75f)
			{
				return;
			}
		}
		if (State.pings.Count >= ConVar.Server.maximumPings)
		{
			State.pings.RemoveAt(0);
		}
		MapNote mapNote = Facepunch.Pool.Get<MapNote>();
		mapNote.worldPosition = vector;
		mapNote.isPing = true;
		mapNote.timeRemaining = (mapNote.totalDuration = ConVar.Server.pingDuration);
		ApplyPingStyle(mapNote, pingType);
		State.pings.Add(mapNote);
		DirtyPlayerState();
		SendPingsToClient();
		TeamUpdate(fullTeamUpdate: true);
		Facepunch.Rust.Analytics.Azure.OnPlayerPinged(this, pingType, wasViaWheel);
	}

	public void AddPingAtLocation(PingType type, Vector3 location, float time, NetworkableId associatedId)
	{
		if (State.pings != null)
		{
			PingStyle pingStyle = GetPingStyle(type);
			foreach (MapNote ping in State.pings)
			{
				if (ping.icon == pingStyle.IconIndex && Vector3.Distance(location, ping.worldPosition) < 0.25f)
				{
					return;
				}
			}
		}
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		MapNote mapNote = Facepunch.Pool.Get<MapNote>();
		mapNote.worldPosition = location;
		mapNote.isPing = true;
		mapNote.timeRemaining = (mapNote.totalDuration = time);
		mapNote.associatedId = associatedId;
		ApplyPingStyle(mapNote, type);
		State.pings.Add(mapNote);
		DirtyPlayerState();
		SendPingsToClient();
		TeamUpdate(fullTeamUpdate: false);
	}

	public void RemovePingAtLocation(PingType type, Vector3 location, float tolerance, NetworkableId associatedId)
	{
		if (State.pings == null)
		{
			return;
		}
		PingStyle pingStyle = GetPingStyle(type);
		for (int i = 0; i < State.pings.Count; i++)
		{
			MapNote mapNote = State.pings[i];
			if (mapNote.icon == pingStyle.IconIndex && Vector3.Distance(location, mapNote.worldPosition) < tolerance)
			{
				State.pings.RemoveAt(i);
				DirtyPlayerState();
				SendPingsToClient();
				TeamUpdate(fullTeamUpdate: false);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(3uL)]
	private void Server_RemovePing(RPCMessage msg)
	{
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		int num = msg.read.Int32();
		if (num >= 0 && num < State.pings.Count)
		{
			State.pings.RemoveAt(num);
			DirtyPlayerState();
			SendPingsToClient();
			TeamUpdate(fullTeamUpdate: true);
		}
	}

	public void SendPingsToClient()
	{
		using MapNoteList mapNoteList = Facepunch.Pool.Get<MapNoteList>();
		mapNoteList.notes = Facepunch.Pool.Get<List<MapNote>>();
		mapNoteList.notes.AddRange(State.pings);
		Interface.CallHook("OnPlayerPingsSend", this, mapNoteList);
		ClientRPC(RpcTarget.Player("Client_ReceivePings", this), mapNoteList);
		mapNoteList.notes.Clear();
	}

	public void TickPings()
	{
		if ((float)lastTick < 0.5f)
		{
			return;
		}
		TimeSince timeSince = lastTick;
		lastTick = 0f;
		UpdateResourcePings();
		if (State.pings == null)
		{
			return;
		}
		List<MapNote> obj = Facepunch.Pool.Get<List<MapNote>>();
		foreach (MapNote ping in State.pings)
		{
			ping.timeRemaining -= timeSince;
			if (ping.timeRemaining <= 0f)
			{
				obj.Add(ping);
			}
		}
		int count = obj.Count;
		foreach (MapNote item in obj)
		{
			if (State.pings.Contains(item))
			{
				State.pings.Remove(item);
			}
		}
		Facepunch.Pool.Free(ref obj, freeElements: false);
		if (count > 0)
		{
			DirtyPlayerState();
			SendPingsToClient();
			TeamUpdate(fullTeamUpdate: true);
		}
	}

	public void RegisterPingedEntity(BaseEntity entity, PingType type)
	{
		if (!pingedEntities.Contains((entity.net.ID, type)))
		{
			pingedEntities.Add((entity.net.ID, type));
		}
	}

	public void DeregisterPingedEntitiesOfType(PingType type)
	{
		if (pingedEntities.Count <= 0)
		{
			return;
		}
		for (int num = pingedEntities.Count - 1; num >= 0; num--)
		{
			if (pingedEntities[num].pingType == type)
			{
				pingedEntities.RemoveAt(num);
			}
		}
	}

	public void DeregisterPingedEntity(NetworkableId id, PingType type)
	{
		if (!pingedEntities.Contains((id, type)))
		{
			return;
		}
		pingedEntities.Remove((id, type));
		for (int i = 0; i < State.pings.Count; i++)
		{
			if (State.pings[i].associatedId == id)
			{
				State.pings.RemoveAt(i);
				break;
			}
		}
		DirtyPlayerState();
		SendPingsToClient();
	}

	public void EnableResourcePings(ItemDefinition forItem, PingType pingType)
	{
		if (!tutorialDesiredResource.Contains((forItem, pingType)))
		{
			tutorialDesiredResource.Add((forItem, pingType));
		}
	}

	private void UpdateResourcePings()
	{
		if (State == null || (float)lastResourcePingUpdate < 3f || !IsInTutorial)
		{
			return;
		}
		lastResourcePingUpdate = 0f;
		if (State.pings == null)
		{
			State.pings = new List<MapNote>();
		}
		using (Facepunch.Pool.Get<PooledList<BaseEntity>>())
		{
			using PooledList<(BaseEntity, PingType)> pooledList2 = Facepunch.Pool.Get<PooledList<(BaseEntity, PingType)>>();
			using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
			foreach (var item2 in tutorialDesiredResource)
			{
				pooledList.Clear();
				if (net.group.networkables != null)
				{
					foreach (Networkable networkable in net.group.networkables)
					{
						BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(networkable.ID) as BaseEntity;
						if (baseEntity != null && Distance(baseEntity) < 128f && baseEntity.isServer)
						{
							if (baseEntity.TryGetComponent<ResourceDispenser>(out var component) && component.HasItemToDispense(item2.item))
							{
								pooledList.Add(baseEntity);
							}
							else if (baseEntity is CollectibleEntity collectibleEntity && collectibleEntity.HasItem(item2.item))
							{
								pooledList.Add(baseEntity);
							}
							else if (baseEntity is StorageContainer { inventory: not null } storageContainer && storageContainer.inventory.HasItem(item2.item))
							{
								pooledList.Add(baseEntity);
							}
						}
					}
				}
				if (pooledList.Count <= 0)
				{
					continue;
				}
				float num = float.MaxValue;
				BaseEntity baseEntity2 = null;
				foreach (BaseEntity item3 in pooledList)
				{
					float num2 = Distance(item3);
					if (num2 < num)
					{
						num = num2;
						baseEntity2 = item3;
					}
				}
				if (baseEntity2 != null)
				{
					pooledList2.Add((baseEntity2, item2.pingType));
				}
			}
			using PooledList<(NetworkableId, PingType)> pooledList3 = Facepunch.Pool.Get<PooledList<(NetworkableId, PingType)>>();
			foreach (var pingedEntity in pingedEntities)
			{
				BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(pingedEntity.id);
				if (baseNetworkable != null && !baseNetworkable.IsDestroyed)
				{
					pooledList2.Add((baseNetworkable as BaseEntity, pingedEntity.pingType));
				}
				else
				{
					pooledList3.Add(pingedEntity);
				}
			}
			foreach (var item4 in pooledList3)
			{
				pingedEntities.Remove(item4);
			}
			using PooledList<MapNote> pooledList4 = Facepunch.Pool.Get<PooledList<MapNote>>();
			foreach (MapNote ping in State.pings)
			{
				if (ping.associatedId.Value == 0L)
				{
					continue;
				}
				bool flag = false;
				foreach (var item5 in pooledList2)
				{
					if (ping.associatedId == item5.Item1.net.ID)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					BaseNetworkable baseNetworkable2 = BaseNetworkable.serverEntities.Find(ping.associatedId);
					if (baseNetworkable2 != null && baseNetworkable2 is IEntityPingSource entityPingSource && entityPingSource.IsPingValid(ping))
					{
						flag = true;
					}
				}
				if (!flag)
				{
					pooledList4.Add(ping);
				}
			}
			bool flag2 = pooledList4.Count > 0;
			foreach (MapNote item6 in pooledList4)
			{
				if (State.pings.Contains(item6))
				{
					State.pings.Remove(item6);
				}
			}
			foreach (var item7 in pooledList2)
			{
				if (HasPingForEntity(item7.Item1))
				{
					continue;
				}
				PingType item = item7.Item2;
				foreach (var pingedEntity2 in pingedEntities)
				{
					if (pingedEntity2.id == item7.Item1.net.ID)
					{
						item = pingedEntity2.pingType;
					}
				}
				State.pings.Add(CreatePingForEntity(item7.Item1, item));
				flag2 = true;
			}
			if (flag2)
			{
				DirtyPlayerState();
				SendPingsToClient();
			}
		}
	}

	private MapNote CreatePingForEntity(BaseEntity baseEntity, PingType type)
	{
		MapNote mapNote = Facepunch.Pool.Get<MapNote>();
		mapNote.worldPosition = baseEntity.transform.position;
		mapNote.isPing = true;
		mapNote.timeRemaining = (mapNote.totalDuration = 30f);
		mapNote.associatedId = baseEntity.net.ID;
		ApplyPingStyle(mapNote, type);
		return mapNote;
	}

	private bool HasPingForEntity(BaseEntity ent)
	{
		return HasPingForEntity(ent.net.ID);
	}

	private bool HasPingForEntity(NetworkableId id)
	{
		foreach (MapNote ping in State.pings)
		{
			if (ping.associatedId == id)
			{
				return true;
			}
		}
		return false;
	}

	public void DisableResourcePings(ItemDefinition forItem, PingType type)
	{
		if (tutorialDesiredResource.Contains((forItem, type)))
		{
			tutorialDesiredResource.Remove((forItem, type));
		}
		if (tutorialDesiredResource.Count == 0)
		{
			UpdateResourcePings();
		}
	}

	private void ClearAllPings()
	{
		if (State != null && State.pings != null)
		{
			State.pings.Clear();
		}
		tutorialDesiredResource.Clear();
		pingedEntities.Clear();
	}

	public void DirtyPlayerState()
	{
		_playerStateDirty = true;
	}

	public void SavePlayerState()
	{
		if (_playerStateDirty)
		{
			_playerStateDirty = false;
			State.protocol = 288;
			State.seed = World.Seed;
			State.saveCreatedTime = Epoch.FromDateTime(SaveRestore.SaveCreatedTime);
			SingletonComponent<ServerMgr>.Instance.playerStateManager.Save(userID);
		}
	}

	public void ResetPlayerState()
	{
		SingletonComponent<ServerMgr>.Instance.playerStateManager.Reset(userID);
		ClientRPC(RpcTarget.Player("SetHostileLength", this), 0f);
		SendMarkersToClient();
		WipeMissions(saveImmediately: true);
		if (modifiers != null)
		{
			modifiers.RemoveAll();
		}
		DirtyPlayerState();
		SavePlayerState();
		AdventCalendar.playerRewardHistory.Remove(userID);
	}

	public static void RecordToastToPlayOnReconnect(GameTip.Styles style, Translate.Phrase phrase, ulong playerId)
	{
		if (FindByID(playerId) != null)
		{
			return;
		}
		PlayerState playerState = SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(playerId);
		if (playerState != null)
		{
			if (playerState.toastOnReconnect == null)
			{
				playerState.toastOnReconnect = Facepunch.Pool.Get<PooledList<ReconnectToast>>();
			}
			ReconnectToast reconnectToast = Facepunch.Pool.Get<ReconnectToast>();
			reconnectToast.phrase = phrase.token;
			reconnectToast.type = (int)style;
			playerState.toastOnReconnect.Add(reconnectToast);
		}
		SingletonComponent<ServerMgr>.Instance.playerStateManager.Save(playerId);
	}

	public bool IsSleeping()
	{
		return HasPlayerFlag(PlayerFlags.Sleeping);
	}

	public bool IsSpectating()
	{
		return HasPlayerFlag(PlayerFlags.Spectating);
	}

	public bool IsRelaxed()
	{
		return HasPlayerFlag(PlayerFlags.Relaxed);
	}

	public bool IsServerFalling()
	{
		return HasPlayerFlag(PlayerFlags.ServerFall);
	}

	public bool IsLoadingAfterTransfer()
	{
		return HasPlayerFlag(PlayerFlags.LoadingAfterTransfer);
	}

	public bool CanBuild()
	{
		return CanBuild(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildBlockedByMonument()
	{
		return ConstructionErrors.IsBuildBlockedByMonument(playerCollider.transform.position);
	}

	public bool CanBuild(bool cached, float cacheDuration = 1f)
	{
		if (IsBuildingBlockedByVehicle(cached, cacheDuration))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(cached, cacheDuration))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached, cacheDuration);
		if (buildingPrivilege == null)
		{
			return true;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool CanBuild(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		return CanBuild(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool CanBuild(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		OBB obb = new OBB(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return true;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool CanBuild(OBB obb)
	{
		return CanBuild(obb, PrivilegeCacheDefaultValue());
	}

	public bool CanBuild(OBB obb, bool cached)
	{
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return true;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingBlocked()
	{
		return IsBuildingBlocked(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlocked(bool cached)
	{
		if (IsBuildingBlockedByVehicle(cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		return !buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingBlocked(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		return IsBuildingBlocked(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlocked(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		OBB obb = new OBB(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		return !buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingBlocked(OBB obb)
	{
		return IsBuildingBlocked(obb, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlocked(OBB obb, bool cached)
	{
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		return !buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingAuthed()
	{
		return IsBuildingAuthed(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingAuthed(bool cached, float cacheDuration = 1f)
	{
		if (IsBuildingBlockedByVehicle(cached, cacheDuration))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(cached, cacheDuration))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached, cacheDuration);
		if (buildingPrivilege == null)
		{
			return false;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingAuthed(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		return IsBuildingAuthed(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingAuthed(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		OBB obb = new OBB(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool IsBuildingAuthed(OBB obb)
	{
		return IsBuildingAuthed(obb, PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingAuthed(OBB obb, bool cached)
	{
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		return buildingPrivilege.CanBuild(this);
	}

	public bool CanPlaceBuildingPrivilege()
	{
		return CanPlaceBuildingPrivilege(PrivilegeCacheDefaultValue());
	}

	public bool CanPlaceBuildingPrivilege(bool cached)
	{
		if (IsBuildingBlockedByVehicle(cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(cached))
		{
			return false;
		}
		return GetBuildingPrivilege(cached) == null;
	}

	public bool CanPlaceBuildingPrivilege(Vector3 position, Quaternion rotation, Bounds bounds, BuildingPrivlidge exclude = null)
	{
		return CanPlaceBuildingPrivilege(position, rotation, bounds, PrivilegeCacheDefaultValue(), exclude);
	}

	public bool CanPlaceBuildingPrivilege(Vector3 position, Quaternion rotation, Bounds bounds, bool cached, BuildingPrivlidge exclude = null)
	{
		OBB obb = new OBB(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		return GetBuildingPrivilege(obb, cached, 1f, exclude) == null;
	}

	public bool CanPlaceBuildingPrivilege(OBB obb)
	{
		return CanPlaceBuildingPrivilege(obb, PrivilegeCacheDefaultValue());
	}

	public bool CanPlaceBuildingPrivilege(OBB obb, bool cached)
	{
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return false;
		}
		return GetBuildingPrivilege(obb, cached) == null;
	}

	public bool IsNearEnemyBase()
	{
		return IsNearEnemyBase(PrivilegeCacheDefaultValue());
	}

	public bool IsNearEnemyBase(bool cached)
	{
		if (IsBuildingBlockedByVehicle(cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		if (!buildingPrivilege.IsAuthed(this))
		{
			return buildingPrivilege.AnyAuthed();
		}
		return false;
	}

	public bool IsNearEnemyBase(Vector3 position, Quaternion rotation, Bounds bounds)
	{
		return IsNearEnemyBase(position, rotation, bounds, PrivilegeCacheDefaultValue());
	}

	public bool IsNearEnemyBase(Vector3 position, Quaternion rotation, Bounds bounds, bool cached)
	{
		OBB obb = new OBB(position, rotation, bounds);
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		if (!buildingPrivilege.IsAuthed(this))
		{
			return buildingPrivilege.AnyAuthed();
		}
		return false;
	}

	public bool IsNearEnemyBase(OBB obb)
	{
		return IsNearEnemyBase(obb, PrivilegeCacheDefaultValue());
	}

	public bool IsNearEnemyBase(OBB obb, bool cached)
	{
		if (IsBuildingBlockedByVehicle(obb, cached))
		{
			return true;
		}
		if (IsBuildingBlockedByEntity(obb, cached))
		{
			return true;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege(obb, cached);
		if (buildingPrivilege == null)
		{
			return false;
		}
		if (!buildingPrivilege.IsAuthed(this))
		{
			return buildingPrivilege.AnyAuthed();
		}
		return false;
	}

	public bool IsBuildingBlockedByVehicle()
	{
		return IsBuildingBlockedByVehicle(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlockedByVehicle(bool cached, float cacheDuration = 1f)
	{
		return IsBuildingBlockedByVehicle(WorldSpaceBounds(), cached);
	}

	public BaseEntity GetVehicleBuildingPrivilege(bool cached, float cacheDuration = 1f)
	{
		return GetVehicleBuildingPrivilege(WorldSpaceBounds(), cached, cacheDuration);
	}

	public BaseEntity GetVehicleBuildingPrivilege(OBB obb, bool cached, float cacheDuration = 1f)
	{
		if (cached && BaseEntity.IsCacheValid(cachedVehicleBuildingPrivilegeTime, cacheDuration, cachedVehicleBuildingPrivilegePosition, obb.position))
		{
			return cachedVehicleBuildingPrivilege;
		}
		cachedVehicleBuildingPrivilege = null;
		cachedVehicleBuildingPrivilegeBlocked = false;
		BoatBuildingStation forPlayer = BoatBuildingStation.GetForPlayer(this);
		if (forPlayer != null)
		{
			cachedVehicleBuildingPrivilege = forPlayer;
			cachedVehicleBuildingPrivilegeTime = UnityEngine.Time.time;
			cachedVehicleBuildingPrivilegePosition = obb.position;
			cachedVehicleBuildingPrivilegeBlocked = !forPlayer.CanPlayerBuild(this);
			return cachedVehicleBuildingPrivilege;
		}
		List<BaseVehicle> obj = Facepunch.Pool.Get<List<BaseVehicle>>();
		Vis.Entities(obb.position, 2f + obb.extents.magnitude, obj, 134217728);
		for (int i = 0; i < obj.Count; i++)
		{
			BaseVehicle baseVehicle = obj[i];
			if (baseVehicle.isServer == base.isServer && !baseVehicle.IsDead() && !(obb.Distance(baseVehicle.WorldSpaceBounds()) > 2f) && baseVehicle.HasBuildingPrivilege)
			{
				VehiclePrivilege childPrivilege = baseVehicle.GetChildPrivilege();
				if (childPrivilege != null)
				{
					cachedVehicleBuildingPrivilege = childPrivilege;
					cachedVehicleBuildingPrivilegeBlocked = !childPrivilege.IsAuthed(this);
				}
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		cachedVehicleBuildingPrivilegeTime = UnityEngine.Time.time;
		cachedVehicleBuildingPrivilegePosition = obb.position;
		return cachedVehicleBuildingPrivilege;
	}

	private bool IsBuildingBlockedByVehicle(OBB obb, bool cached, float cacheDuration = 1f)
	{
		if (cached && BaseEntity.IsCacheValid(cachedVehicleBuildingPrivilegeTime, cacheDuration, cachedVehicleBuildingPrivilegePosition, obb.position))
		{
			if (cachedVehicleBuildingPrivilege != null)
			{
				return cachedVehicleBuildingPrivilegeBlocked;
			}
			return false;
		}
		if (GetVehicleBuildingPrivilege(obb, cached, cacheDuration) != null)
		{
			return cachedVehicleBuildingPrivilegeBlocked;
		}
		return false;
	}

	public bool IsBuildingBlockedByEntity()
	{
		return IsBuildingBlockedByEntity(PrivilegeCacheDefaultValue());
	}

	public bool IsBuildingBlockedByEntity(bool cached, float cacheDuration = 1f)
	{
		return IsBuildingBlockedByEntity(WorldSpaceBounds(), cached, cacheDuration);
	}

	private bool IsBuildingBlockedByEntity(OBB obb, bool cached, float cacheDuration = 1f)
	{
		if (cached && BaseEntity.IsCacheValid(cachedEntityBuildingPrivilegeTime, cacheDuration, cachedEntityBuildingPrivilegePosition, obb.position))
		{
			if (cachedEntityBuildingPrivilege != null)
			{
				return cachedEntityBuildingPrivilegeBlocked;
			}
			return false;
		}
		List<BaseEntity> obj = Facepunch.Pool.Get<List<BaseEntity>>();
		Vis.Entities(obb.position, 3f + obb.extents.magnitude, obj, 2097152);
		cachedEntityBuildingPrivilege = null;
		cachedEntityBuildingPrivilegeBlocked = false;
		for (int i = 0; i < obj.Count; i++)
		{
			BaseEntity baseEntity = obj[i];
			if (baseEntity.isServer != base.isServer || obb.Distance(baseEntity.WorldSpaceBounds()) > 3f)
			{
				continue;
			}
			EntityPrivilege entityBuildingPrivilege = baseEntity.GetEntityBuildingPrivilege();
			if (!(entityBuildingPrivilege == null))
			{
				cachedEntityBuildingPrivilege = baseEntity;
				if (!entityBuildingPrivilege.IsAuthed(this))
				{
					cachedEntityBuildingPrivilegeBlocked = true;
					break;
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		cachedEntityBuildingPrivilegeTime = UnityEngine.Time.time;
		cachedEntityBuildingPrivilegePosition = obb.position;
		return cachedEntityBuildingPrivilegeBlocked;
	}

	public bool HasPrivilegeFromOther()
	{
		return HasPrivilegeFromOther(PrivilegeCacheDefaultValue());
	}

	public bool HasPrivilegeFromOther(bool cached)
	{
		if (IsBuildingBlockedByVehicle(WorldSpaceBounds(), cached))
		{
			return false;
		}
		if (IsBuildingBlockedByEntity(WorldSpaceBounds(), cached))
		{
			return false;
		}
		if (!(cachedVehicleBuildingPrivilege != null))
		{
			return cachedEntityBuildingPrivilege != null;
		}
		return true;
	}

	public void SetMortarCooldown(float duration)
	{
		mortarCooldown = duration;
		if (base.isServer)
		{
			SendNetworkUpdate();
		}
	}

	public bool HasMortarCooldown()
	{
		return (float)mortarCooldown > 0f;
	}

	private static bool LineOfSightBidirectional(Vector3 p0, Vector3 p1, int lineOfSightLayerMask, BaseEntity ignoreEntity = null)
	{
		if (!GamePhysics.LineOfSight(p0, p1, lineOfSightLayerMask, ignoreEntity))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1, p0, lineOfSightLayerMask, ignoreEntity))
		{
			return false;
		}
		return true;
	}

	private static bool LineOfSightBasic(Vector3 p0_curProjectilePos, Vector3 p1_hitRaycastStartPos, Vector3 p2_closestRayPos, Vector3 p3_worldHitPos, FiredProjectile firedProjectile, int lineOfSightLayerMask)
	{
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (ConVar.AntiHack.projectile_backtracking > 0f)
		{
			vector = (p1_hitRaycastStartPos - p0_curProjectilePos).normalized * ConVar.AntiHack.projectile_backtracking;
			vector2 = (p2_closestRayPos - p1_hitRaycastStartPos).normalized * ConVar.AntiHack.projectile_backtracking;
		}
		if (!LineOfSightBidirectional(p0_curProjectilePos - vector, p1_hitRaycastStartPos + vector, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		if (!LineOfSightBidirectional(p1_hitRaycastStartPos - vector2, p2_closestRayPos, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		if (!LineOfSightBidirectional(p2_closestRayPos, p3_worldHitPos, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		return true;
	}

	private static bool LineOfSightDetailed(Vector3 p0_curProjectilePos, FiredProjectile firedProjectile, int lineOfSightLayerMask)
	{
		List<Vector3> simulatedPositions = firedProjectile.simulatedPositions;
		for (int i = 1; i < simulatedPositions.Count; i++)
		{
			if (!GamePhysics.LineOfSight(simulatedPositions[i - 1], simulatedPositions[i], lineOfSightLayerMask, firedProjectile.lastEntityHit))
			{
				return false;
			}
		}
		if (simulatedPositions.Count >= 1 && !GamePhysics.LineOfSight(simulatedPositions[simulatedPositions.Count - 1], p0_curProjectilePos, lineOfSightLayerMask, firedProjectile.lastEntityHit))
		{
			return false;
		}
		return true;
	}

	private static bool LineOfSightPlayer(Vector3 p0_worldHitPos, Vector3 p1_posOnPlayer, int lineOfSightLayerMask, float padding)
	{
		if (!GamePhysics.LineOfSight(p0_worldHitPos, p1_posOnPlayer, lineOfSightLayerMask, 0f, padding))
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(p1_posOnPlayer, p0_worldHitPos, lineOfSightLayerMask, padding, 0f))
		{
			return false;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void OnProjectileAttack(RPCMessage msg)
	{
		using PlayerProjectileAttack playerProjectileAttack = msg.read.Proto<PlayerProjectileAttack>();
		if (playerProjectileAttack == null)
		{
			return;
		}
		PlayerAttack playerAttack = playerProjectileAttack.playerAttack;
		HitInfo hitInfo = Facepunch.Pool.Get<HitInfo>();
		try
		{
			hitInfo.LoadFromAttack(playerAttack.attack, serverSide: true);
			hitInfo.Initiator = this;
			hitInfo.ProjectileID = playerAttack.projectileID;
			hitInfo.ProjectileDistance = playerProjectileAttack.hitDistance;
			hitInfo.ProjectileVelocity = playerProjectileAttack.hitVelocity;
			hitInfo.ProjectileTravelTime = playerProjectileAttack.travelTime;
			hitInfo.Predicted = msg.connection;
			if (hitInfo.IsNaNOrInfinity() || float.IsNaN(playerProjectileAttack.travelTime) || float.IsInfinity(playerProjectileAttack.travelTime))
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Contains NaN ({playerAttack.projectileID})");
				stats.combat.LogInvalid(hitInfo, "projectile_nan");
				return;
			}
			if (!firedProjectiles.TryGetValue(playerAttack.projectileID, out var firedProjectile))
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Missing ID ({playerAttack.projectileID})", logToAnalytics: false);
				stats.combat.LogInvalid(hitInfo, "projectile_invalid");
				return;
			}
			hitInfo.ProjectileHits = firedProjectile.hits;
			hitInfo.ProjectileIntegrity = firedProjectile.integrity;
			hitInfo.ProjectileTrajectoryMismatch = firedProjectile.trajectoryMismatch;
			if (firedProjectile.integrity <= 0f)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Integrity is zero ({playerAttack.projectileID})");
				Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
				stats.combat.LogInvalid(hitInfo, "projectile_integrity");
				return;
			}
			if (firedProjectile.firedTime < UnityEngine.Time.realtimeSinceStartup - 8f)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Lifetime is zero ({playerAttack.projectileID})");
				Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
				stats.combat.LogInvalid(hitInfo, "projectile_lifetime");
				return;
			}
			if (firedProjectile.ricochets > 0)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile attack is ricochet ({playerAttack.projectileID})");
				Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
				stats.combat.LogInvalid(hitInfo, "projectile_ricochet_attack");
				return;
			}
			if (playerProjectileAttack.hitVelocity == Vector3.zero)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile hitVelocity is zero ({playerAttack.projectileID})");
				Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
				stats.combat.LogInvalid(hitInfo, "projectile_zero_hit_velocity");
				return;
			}
			hitInfo.Weapon = firedProjectile.weaponSource;
			hitInfo.WeaponPrefab = firedProjectile.weaponPrefab;
			hitInfo.ProjectilePrefab = firedProjectile.projectilePrefab;
			hitInfo.damageProperties = firedProjectile.projectilePrefab.damageProperties;
			Vector3 position = firedProjectile.position;
			Vector3 initialPositionOffset = firedProjectile.initialPositionOffset;
			Vector3 positionOffset = firedProjectile.positionOffset;
			Vector3 velocity = firedProjectile.velocity;
			float partialTime = firedProjectile.partialTime;
			float travelTime = firedProjectile.travelTime;
			float num = Mathf.Clamp(playerProjectileAttack.travelTime, firedProjectile.travelTime, 8f);
			Vector3 gravity = UnityEngine.Physics.gravity * firedProjectile.projectilePrefab.gravityModifier;
			float drag = firedProjectile.projectilePrefab.drag;
			BaseEntity hitEntity = hitInfo.HitEntity;
			BasePlayer hitPlayer = hitEntity as BasePlayer;
			bool flag = hitPlayer != null;
			bool flag2 = flag && hitPlayer.IsSleeping();
			bool flag3 = flag && hitPlayer.IsWounded();
			bool flag4 = flag && hitPlayer.isMounted;
			bool flag5 = flag && hitPlayer.HasParent();
			bool flag6 = hitEntity != null;
			bool flag7 = flag6 && hitEntity.IsNpc;
			bool flag8 = hitInfo.HitMaterial == Projectile.WaterMaterialID();
			bool valid;
			float entityDeltaTime;
			if (firedProjectile.protection > 0)
			{
				valid = true;
				float num2 = 1f + ConVar.AntiHack.projectile_forgiveness;
				float num3 = 1f - ConVar.AntiHack.projectile_forgiveness;
				float projectile_clientframes = ConVar.AntiHack.projectile_clientframes;
				float projectile_serverframes = ConVar.AntiHack.projectile_serverframes;
				float num4 = Mathx.Decrement(firedProjectile.firedTime);
				float num5 = Mathf.Clamp(Mathx.Increment(UnityEngine.Time.realtimeSinceStartup) - num4, 0f, 8f);
				float num6 = num;
				float num7 = Mathf.Abs(num5 - num6);
				firedProjectile.desyncLifeTime = num7;
				float num8 = Mathf.Min(num5, num6);
				float num9 = projectile_clientframes / 60f;
				float num10 = projectile_serverframes * Mathx.Max(UnityEngine.Time.deltaTime, UnityEngine.Time.smoothDeltaTime, UnityEngine.Time.fixedDeltaTime);
				float num11 = (desyncTimeClamped + num8 + num9 + num10) * num2;
				entityDeltaTime = ((firedProjectile.protection >= 6) ? ((desyncTimeClamped + num9 + num10) * num2) : num11);
				_ = desyncTimeClamped;
				float num12 = Vector3.Distance(firedProjectile.initialPosition, hitInfo.HitPositionWorld);
				int num13 = 1075904512;
				if (ConVar.AntiHack.projectile_terraincheck)
				{
					num13 |= 0x800000;
				}
				if (ConVar.AntiHack.projectile_vehiclecheck)
				{
					num13 |= 0x8000000;
				}
				if (ConVar.AntiHack.projectile_defaultcheck)
				{
					num13 |= 1;
				}
				if (ConVar.AntiHack.projectile_deployedcheck)
				{
					num13 |= 0x100;
				}
				if (flag6 && net.group != null && hitEntity.net != null && hitEntity.net.group != null && !net.subscriber.IsSubscribed(hitEntity.net.group))
				{
					AntiHack.Log(this, AntiHackType.ProjectileHack, "Entity out of network range");
					stats.combat.LogInvalid(hitInfo, "projectile_network_range");
					valid = false;
				}
				if (flag && hitInfo.boneArea == (HitArea)(-1))
				{
					string arg = hitInfo.ProjectilePrefab.name;
					string arg2 = (flag6 ? hitEntity.ShortPrefabName : "world");
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Bone is invalid ({arg} on {arg2} bone {hitInfo.HitBone})");
					stats.combat.LogInvalid(hitInfo, "projectile_bone");
					valid = false;
				}
				if (flag8)
				{
					if (flag6)
					{
						string text = hitInfo.ProjectilePrefab.name;
						string text2 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, "Projectile water hit on entity (" + text + " on " + text2 + ")");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "water_entity");
						valid = false;
					}
					if (!WaterLevel.Test(hitInfo.HitPositionWorld - 0.5f * Vector3.up, waves: true, volumes: true, this))
					{
						string text3 = hitInfo.ProjectilePrefab.name;
						string text4 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, "Projectile water level (" + text3 + " on " + text4 + ")");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "water_level");
						valid = false;
					}
				}
				if (firedProjectile.protection >= 2)
				{
					bool flag9 = flag && !flag7 && !flag2 && !flag3;
					if (firedProjectile.protection >= 6 && flag9)
					{
						if (flag5 || flag4)
						{
							if (flag5 && ConVar.AntiHack.parenthistory && hitPlayer.tickHistory.ParentCount > 0)
							{
								VerifyParentedPlayerDistance();
							}
							else
							{
								VerifyEntityDistance();
							}
						}
						else
						{
							VerifyPlayerDistance();
						}
					}
					else if (flag6)
					{
						VerifyEntityDistance();
					}
				}
				if (firedProjectile.protection >= 1)
				{
					float num14 = (flag6 ? (hitEntity.AntiHackVelocity() + hitEntity.GetParentVelocity().magnitude) : 0f);
					float num15 = (flag6 ? (entityDeltaTime * num14) : 0f);
					float magnitude = firedProjectile.initialVelocity.magnitude;
					float num16 = hitInfo.ProjectilePrefab.initialDistance + num11 * magnitude;
					float num17 = hitInfo.ProjectileDistance + 1f + positionOffset.magnitude + num15 + estimatedVelocity.magnitude;
					if (num12 > num16)
					{
						string text5 = hitInfo.ProjectilePrefab.name;
						string text6 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile too fast ({text5} on {text6} with {num12}m > {num16}m in {num11}s)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "projectile_maxspeed");
						valid = false;
					}
					if (num12 > num17)
					{
						string text7 = hitInfo.ProjectilePrefab.name;
						string text8 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile too far away ({text7} on {text8} with {num12}m > {num17}m in {num11}s)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "projectile_distance");
						valid = false;
					}
					if (num7 > ConVar.AntiHack.projectile_desync)
					{
						string text9 = hitInfo.ProjectilePrefab.name;
						string text10 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile desync ({text9} on {text10} with {num7}s > {ConVar.AntiHack.projectile_desync}s)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "projectile_desync");
						valid = false;
					}
				}
				if (firedProjectile.protection >= 4)
				{
					float num18 = 0f;
					if (flag6)
					{
						float num19 = hitEntity.GetParentVelocity().magnitude;
						if (hitEntity is ILargeVehicleForProjectiles)
						{
							num19 += hitEntity.AntiHackVelocity();
						}
						num18 = entityDeltaTime * num19;
					}
					SimulateProjectile(ref position, ref velocity, ref partialTime, num - travelTime, gravity, drag, out var prevPosition, out var prevVelocity);
					Line line = new Line(prevPosition - prevVelocity, prevPosition);
					Line line2 = new Line(prevPosition, position);
					Line line3 = new Line(position, position + velocity);
					float num20 = Mathx.Min(line.Distance(hitInfo.PointStart), line2.Distance(hitInfo.PointStart), line3.Distance(hitInfo.PointStart));
					float num21 = Mathx.Min(line.Distance(hitInfo.HitPositionWorld), line2.Distance(hitInfo.HitPositionWorld), line3.Distance(hitInfo.HitPositionWorld));
					float num22 = (firedProjectile.startPointMismatch = Mathf.Max(num20 - initialPositionOffset.magnitude - num18, 0f));
					float num23 = (firedProjectile.endPointMismatch = Mathf.Max(num21 - initialPositionOffset.magnitude - num18, 0f));
					if (num22 > ConVar.AntiHack.projectile_trajectory)
					{
						string text11 = firedProjectile.projectilePrefab.name;
						string text12 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Start position trajectory ({text11} on {text12} with {num22}m > {ConVar.AntiHack.projectile_trajectory}m)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "trajectory_start");
						valid = false;
					}
					if (num23 > ConVar.AntiHack.projectile_trajectory)
					{
						string text13 = firedProjectile.projectilePrefab.name;
						string text14 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"End position trajectory ({text13} on {text14} with {num23}m > {ConVar.AntiHack.projectile_trajectory}m)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "trajectory_end");
						valid = false;
					}
					if (hitInfo.ProjectileTrajectoryMismatch > ConVar.AntiHack.projectile_trajectory_update)
					{
						string text15 = firedProjectile.projectilePrefab.name;
						string text16 = (flag6 ? hitEntity.ShortPrefabName : "world");
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"Update position trajectory ({text15} on {text16} with {hitInfo.ProjectileTrajectoryMismatch}m > {ConVar.AntiHack.projectile_trajectory_update}m)");
						Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
						stats.combat.LogInvalid(hitInfo, "trajectory_update_total");
						valid = false;
					}
					hitInfo.ProjectileVelocity = velocity;
					if (playerProjectileAttack.hitVelocity != Vector3.zero && velocity != Vector3.zero)
					{
						float num24 = Vector3.Angle(playerProjectileAttack.hitVelocity, velocity);
						float num25 = playerProjectileAttack.hitVelocity.magnitude / velocity.magnitude;
						if (num24 > ConVar.AntiHack.projectile_anglechange)
						{
							string text17 = firedProjectile.projectilePrefab.name;
							string text18 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(this, AntiHackType.ProjectileHack, $"Trajectory angle change ({text17} on {text18} with {num24}deg > {ConVar.AntiHack.projectile_anglechange}deg)");
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, "angle_change");
							valid = false;
						}
						if (num25 > ConVar.AntiHack.projectile_velocitychange)
						{
							string text19 = firedProjectile.projectilePrefab.name;
							string text20 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(this, AntiHackType.ProjectileHack, $"Trajectory velocity change ({text19} on {text20} with {num25} > {ConVar.AntiHack.projectile_velocitychange})");
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, "velocity_change");
							valid = false;
						}
					}
				}
				if (firedProjectile.protection >= 3)
				{
					if (firedProjectile.simulatedPositions.Count > ConVar.AntiHack.projectile_update_limit)
					{
						AntiHack.Log(this, AntiHackType.ProjectileHack, $"projectile_update_limit exceeded on attack ({firedProjectile.simulatedPositions.Count} > {ConVar.AntiHack.projectile_update_limit})");
						stats.combat.LogInvalid(hitInfo, "projectile_update_limit");
						valid = false;
					}
					if (valid)
					{
						Vector3 position2 = firedProjectile.position;
						Vector3 pointStart = hitInfo.PointStart;
						Vector3 hitPositionWorld = hitInfo.HitPositionWorld;
						if (!flag8)
						{
							hitPositionWorld -= hitInfo.ProjectileVelocity.normalized * 0.001f;
						}
						Vector3 vector = hitInfo.PositionOnRay(hitPositionWorld);
						bool flag10 = LineOfSightBasic(position2, pointStart, vector, hitPositionWorld, firedProjectile, num13);
						bool flag11 = true;
						if (flag10)
						{
							flag11 = LineOfSightDetailed(position2, firedProjectile, num13);
						}
						bool flag12 = flag10 && flag11;
						string text21 = (flag6 ? hitEntity.Categorize() : "world");
						string value = string.Empty;
						switch (text21)
						{
						case "player":
							value = (flag12 ? "hit_player_direct_los" : "hit_player_indirect_los");
							break;
						case "building":
							value = (flag12 ? "hit_building_direct_los" : "hit_building_indirect_los");
							break;
						case "entity":
							value = (flag12 ? "hit_entity_direct_los" : "hit_entity_indirect_los");
							break;
						}
						if (!string.IsNullOrEmpty(value))
						{
							stats.Add(value, 1, Stats.Server);
						}
						if (!flag12 && flag6)
						{
							string text22 = hitInfo.ProjectilePrefab.name;
							string shortPrefabName = hitEntity.ShortPrefabName;
							string text23 = ((!flag10) ? "projectile_los" : "projectile_los_detailed");
							AntiHack.Log(this, AntiHackType.ProjectileHack, $"Line of sight {text23} ({text22} on {shortPrefabName}) {position2} {pointStart} {vector} {hitPositionWorld}");
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, text23);
						}
						if (!flag12)
						{
							valid = false;
						}
					}
					if (valid && flag && !flag7)
					{
						Vector3 hitPositionWorld2 = hitInfo.HitPositionWorld;
						Vector3 position3 = hitPlayer.eyes.position;
						Vector3 vector2 = hitPlayer.CenterPoint();
						float projectile_losforgiveness = ConVar.AntiHack.projectile_losforgiveness;
						bool flag13 = LineOfSightPlayer(hitPositionWorld2, position3, num13, projectile_losforgiveness);
						if (!flag13)
						{
							flag13 = LineOfSightPlayer(hitPositionWorld2, vector2, num13, projectile_losforgiveness);
						}
						if (!flag13)
						{
							string text24 = hitInfo.ProjectilePrefab.name;
							string shortPrefabName2 = hitEntity.ShortPrefabName;
							AntiHack.Log(this, AntiHackType.ProjectileHack, $"Line of sight player ({text24} on {shortPrefabName2}) {hitPositionWorld2} {position3} or {hitPositionWorld2} {vector2}");
							Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
							stats.combat.LogInvalid(hitInfo, "projectile_los_player");
							valid = false;
						}
					}
				}
				if (!valid)
				{
					AntiHack.AddViolation(this, AntiHackType.ProjectileHack, ConVar.AntiHack.projectile_penalty);
					return;
				}
			}
			firedProjectile.position = hitInfo.HitPositionWorld;
			firedProjectile.velocity = velocity.normalized * playerProjectileAttack.hitVelocity.magnitude;
			firedProjectile.travelTime = num;
			firedProjectile.partialTime = partialTime;
			firedProjectile.hits++;
			firedProjectile.lastEntityHit = hitEntity;
			firedProjectile.simulatedPositions.Clear();
			firedProjectile.simulatedPositions.Add(position);
			hitInfo.ProjectilePrefab.CalculateDamage(hitInfo, firedProjectile.projectileModifier, firedProjectile.integrity);
			if (flag8)
			{
				if (hitInfo.ProjectilePrefab.waterIntegrityLoss > 0f)
				{
					firedProjectile.integrity = Mathf.Clamp01(firedProjectile.integrity - hitInfo.ProjectilePrefab.waterIntegrityLoss);
				}
			}
			else if (hitInfo.ProjectilePrefab.penetrationPower <= 0f || !flag6)
			{
				firedProjectile.integrity = 0f;
			}
			else
			{
				float num26 = hitEntity.PenetrationResistance(hitInfo) / hitInfo.ProjectilePrefab.penetrationPower;
				firedProjectile.integrity = Mathf.Clamp01(firedProjectile.integrity - num26);
			}
			if (flag6)
			{
				stats.Add(firedProjectile.itemMod.category + "_hit_" + hitEntity.Categorize(), 1);
			}
			if (Interface.CallHook("OnPlayerAttack", this, hitInfo) != null)
			{
				return;
			}
			if (firedProjectile.integrity <= 0f)
			{
				if (hitInfo.ProjectilePrefab.remainInWorld)
				{
					CreateWorldProjectile(hitInfo, firedProjectile.itemDef, firedProjectile.itemMod, hitInfo.ProjectilePrefab, firedProjectile.pickupItem);
				}
				if (firedProjectile.hits <= ConVar.AntiHack.projectile_impactspawndepth)
				{
					firedProjectile.itemMod.ServerProjectileHit(hitInfo);
				}
			}
			else if (firedProjectile.hits == ConVar.AntiHack.projectile_impactspawndepth)
			{
				firedProjectile.itemMod.ServerProjectileHit(hitInfo);
			}
			firedProjectiles[playerAttack.projectileID] = firedProjectile;
			if (flag6)
			{
				if (firedProjectile.hits <= ConVar.AntiHack.projectile_damagedepth)
				{
					hitEntity.OnAttacked(hitInfo);
					firedProjectile.itemMod.ServerProjectileHitEntity(hitInfo);
				}
				else
				{
					stats.combat.LogInvalid(hitInfo, "ricochet");
				}
			}
			Projectile.CustomEffectData clientEffectData = firedProjectile.projectilePrefab.clientEffectData;
			bool playDefaultHitEffects = firedProjectile.projectilePrefab.playDefaultHitEffects;
			GameObjectRef clientEffectPrefab = firedProjectile.projectilePrefab.clientEffectPrefab;
			if (!clientEffectData.UseCustomEffect || playDefaultHitEffects)
			{
				Effect.server.ImpactEffect(hitInfo);
			}
			if (clientEffectData.UseCustomEffect)
			{
				string text25 = null;
				if (clientEffectPrefab != null && clientEffectPrefab.isValid)
				{
					text25 = clientEffectPrefab.resourcePath;
				}
				if (text25 != null)
				{
					Effect.server.ImpactEffect(hitInfo, text25);
				}
			}
			hitInfo.DoHitEffects = hitInfo.ProjectilePrefab.doHitEffects;
			SingletonComponent<NpcNoiseManager>.Instance.OnProjectileHit(this, hitInfo);
			void VerifyEntityDistance()
			{
				float num29 = hitEntity.AntiHackVelocity() + hitEntity.GetParentVelocity().magnitude;
				float num30 = hitEntity.AntiHackPadding() + entityDeltaTime * num29;
				float num31 = (firedProjectile.entityDistance = hitEntity.Distance(hitInfo.HitPositionWorld));
				if (num31 > num30)
				{
					string text27 = hitInfo.ProjectilePrefab.name;
					string shortPrefabName4 = hitEntity.ShortPrefabName;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Entity too far away ({text27} on {shortPrefabName4} with {num31}m > {num30}m in {entityDeltaTime}s)");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "entity_distance");
					valid = false;
				}
			}
			void VerifyParentedPlayerDistance()
			{
				float num32 = hitPlayer.AntiHackPadding() + ConVar.AntiHack.tickhistoryforgiveness;
				float num33 = (firedProjectile.entityDistance = hitPlayer.TickHistoryDistanceParented(hitInfo.HitPositionWorld));
				if (num33 > num32)
				{
					string text28 = hitInfo.ProjectilePrefab.name;
					string shortPrefabName5 = hitPlayer.ShortPrefabName;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Parented player too far away ({text28} on {shortPrefabName5} with {num33}m > {num32}m in {entityDeltaTime}s)");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "player_distance");
					valid = false;
				}
			}
			void VerifyPlayerDistance()
			{
				float num27 = hitPlayer.AntiHackPadding() + ConVar.AntiHack.tickhistoryforgiveness;
				float num28 = (firedProjectile.entityDistance = hitPlayer.tickHistory.Distance(hitPlayer, hitInfo.HitPositionWorld));
				if (num28 > num27)
				{
					string text26 = hitInfo.ProjectilePrefab.name;
					string shortPrefabName3 = hitPlayer.ShortPrefabName;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Player too far away ({text26} on {shortPrefabName3} with {num28}m > {num27}m in {entityDeltaTime}s)");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(firedProjectile);
					stats.combat.LogInvalid(hitInfo, "player_distance");
					valid = false;
				}
			}
		}
		finally
		{
			if (hitInfo != null)
			{
				((IDisposable)hitInfo).Dispose();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	public void OnProjectileRicochet(RPCMessage msg)
	{
		using PlayerProjectileRicochet playerProjectileRicochet = msg.read.Proto<PlayerProjectileRicochet>();
		if (playerProjectileRicochet != null)
		{
			FiredProjectile value;
			if (playerProjectileRicochet.hitPosition.IsNaNOrInfinity() || playerProjectileRicochet.inVelocity.IsNaNOrInfinity() || playerProjectileRicochet.outVelocity.IsNaNOrInfinity() || playerProjectileRicochet.hitNormal.IsNaNOrInfinity() || float.IsNaN(playerProjectileRicochet.travelTime) || float.IsInfinity(playerProjectileRicochet.travelTime))
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Contains NaN ({playerProjectileRicochet.projectileID})");
			}
			else if (!firedProjectiles.TryGetValue(playerProjectileRicochet.projectileID, out value))
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Missing ID ({playerProjectileRicochet.projectileID})", logToAnalytics: false);
			}
			else if (value.firedTime < UnityEngine.Time.realtimeSinceStartup - 8f)
			{
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Lifetime is zero ({playerProjectileRicochet.projectileID})");
			}
			else if (Interface.CallHook("OnProjectileRicochet", this, playerProjectileRicochet) == null)
			{
				value.ricochets++;
				firedProjectiles[playerProjectileRicochet.projectileID] = value;
			}
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server]
	public void OnProjectileUpdate(RPCMessage msg)
	{
		using PlayerProjectileUpdate playerProjectileUpdate = msg.read.Proto<PlayerProjectileUpdate>();
		if (playerProjectileUpdate == null)
		{
			return;
		}
		if (playerProjectileUpdate.curPosition.IsNaNOrInfinity() || playerProjectileUpdate.curVelocity.IsNaNOrInfinity() || float.IsNaN(playerProjectileUpdate.travelTime) || float.IsInfinity(playerProjectileUpdate.travelTime))
		{
			AntiHack.Log(this, AntiHackType.ProjectileHack, $"Contains NaN ({playerProjectileUpdate.projectileID})");
			return;
		}
		if (!firedProjectiles.TryGetValue(playerProjectileUpdate.projectileID, out var value))
		{
			AntiHack.Log(this, AntiHackType.ProjectileHack, $"Missing ID ({playerProjectileUpdate.projectileID})", logToAnalytics: false);
			return;
		}
		if (value.firedTime < UnityEngine.Time.realtimeSinceStartup - 8f)
		{
			AntiHack.Log(this, AntiHackType.ProjectileHack, $"Lifetime is zero ({playerProjectileUpdate.projectileID})");
			Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
			return;
		}
		if (value.ricochets > 0)
		{
			AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile update is ricochet ({playerProjectileUpdate.projectileID})");
			Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
			return;
		}
		Vector3 position = value.position;
		Vector3 positionOffset = value.positionOffset;
		Vector3 velocity = value.velocity;
		float num = value.trajectoryMismatch;
		float partialTime = value.partialTime;
		float travelTime = value.travelTime;
		float num2 = Mathf.Clamp(playerProjectileUpdate.travelTime, value.travelTime, 8f);
		Vector3 vector = UnityEngine.Physics.gravity * value.projectilePrefab.gravityModifier;
		float drag = value.projectilePrefab.drag;
		if (value.protection > 0)
		{
			float num3 = 1f - ConVar.AntiHack.projectile_forgiveness;
			float num4 = 1f + ConVar.AntiHack.projectile_forgiveness;
			float projectile_clientframes = ConVar.AntiHack.projectile_clientframes;
			float projectile_serverframes = ConVar.AntiHack.projectile_serverframes;
			float num5 = Mathx.Decrement(value.firedTime);
			float num6 = Mathf.Clamp(Mathx.Increment(UnityEngine.Time.realtimeSinceStartup) - num5, 0f, 8f);
			float num7 = num2;
			float num8 = (value.desyncLifeTime = Mathf.Abs(num6 - num7));
			float num9 = Mathf.Min(num6, num7);
			float num10 = projectile_clientframes / 60f;
			float num11 = projectile_serverframes * Mathx.Max(UnityEngine.Time.deltaTime, UnityEngine.Time.smoothDeltaTime, UnityEngine.Time.fixedDeltaTime);
			float num12 = (num9 + desyncTimeClamped + num10 + num11) * num4;
			float num13 = Mathf.Max(0f, (num9 - desyncTimeClamped - num10 - num11) * num3);
			int num14 = 1075904512;
			if (ConVar.AntiHack.projectile_terraincheck)
			{
				num14 |= 0x800000;
			}
			if (ConVar.AntiHack.projectile_vehiclecheck)
			{
				num14 |= 0x8000000;
			}
			if (value.protection >= 1)
			{
				float num15 = value.projectilePrefab.initialDistance + num12 * value.initialVelocity.magnitude;
				float num16 = Vector3.Distance(value.initialPosition, playerProjectileUpdate.curPosition);
				if (num16 > num15)
				{
					string text = value.projectilePrefab.name;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile distance ({text} with {num16}m > {num15}m in {num12}s)");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
					return;
				}
				if (num8 > ConVar.AntiHack.projectile_desync)
				{
					string arg = value.projectilePrefab.name;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile desync ({arg} with {num8}s > {ConVar.AntiHack.projectile_desync}s)");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
					return;
				}
				Vector3 curVelocity = playerProjectileUpdate.curVelocity;
				Vector3 initialVelocity = value.initialVelocity;
				Vector3 vector2 = ((value.hits == 0) ? initialVelocity : value.velocity);
				float num17 = drag * (1f / 32f);
				Vector3 vector3 = vector * (1f / 32f);
				int num18 = Mathf.FloorToInt(num13 / (1f / 32f));
				int num19 = Mathf.CeilToInt(num12 / (1f / 32f));
				for (int i = 0; i < num18; i++)
				{
					initialVelocity += vector3;
					initialVelocity -= initialVelocity * num17;
					vector2 += vector3;
					vector2 -= vector2 * num17;
				}
				float magnitude = curVelocity.magnitude;
				float num20 = initialVelocity.magnitude;
				float num21 = vector2.magnitude;
				for (int j = num18; j < num19; j++)
				{
					initialVelocity += vector3;
					initialVelocity -= initialVelocity * num17;
					vector2 += vector3;
					vector2 -= vector2 * num17;
					num21 = Mathf.Min(num21, vector2.magnitude);
					num20 = Mathf.Max(num20, initialVelocity.magnitude);
				}
				if (magnitude < num21 * num3)
				{
					string arg2 = value.projectilePrefab.name;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile velocity too low ({arg2} with {magnitude} < {num21})");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
					return;
				}
				if (magnitude > num20 * num4)
				{
					string arg3 = value.projectilePrefab.name;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Projectile velocity too high ({arg3} with {magnitude} > {num20})");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
					return;
				}
			}
			if (value.protection >= 3)
			{
				Vector3 position2 = value.position;
				Vector3 curPosition = playerProjectileUpdate.curPosition;
				Vector3 vector4 = Vector3.zero;
				if (ConVar.AntiHack.projectile_backtracking > 0f)
				{
					vector4 = (curPosition - position2).normalized * ConVar.AntiHack.projectile_backtracking;
				}
				if (!GamePhysics.LineOfSight(position2 - vector4, curPosition + vector4, num14, value.lastEntityHit))
				{
					string arg4 = value.projectilePrefab.name;
					AntiHack.Log(this, AntiHackType.ProjectileHack, $"Line of sight ({arg4} on update) {position2} {curPosition}");
					Facepunch.Rust.Analytics.Azure.OnProjectileHackViolation(value);
					return;
				}
			}
			if (value.protection >= 4)
			{
				SimulateProjectile(ref position, ref velocity, ref partialTime, num2 - travelTime, vector, drag, out var prevPosition, out var prevVelocity);
				value.simulatedPositions.Add(position);
				Line line = new Line(prevPosition - prevVelocity, prevPosition);
				Line line2 = new Line(prevPosition, position);
				Line line3 = new Line(position, position + velocity);
				float num22 = Mathx.Min(line.Distance(playerProjectileUpdate.curPosition), line2.Distance(playerProjectileUpdate.curPosition), line3.Distance(playerProjectileUpdate.curPosition));
				num += Mathf.Max(num22 - positionOffset.magnitude, 0f);
			}
			if (value.protection >= 5)
			{
				if (value.inheritedVelocity != Vector3.zero)
				{
					Vector3 curVelocity2 = value.inheritedVelocity + velocity;
					Vector3 curVelocity3 = playerProjectileUpdate.curVelocity;
					if (curVelocity3.magnitude > 2f * curVelocity2.magnitude || curVelocity3.magnitude < 0.5f * curVelocity2.magnitude)
					{
						playerProjectileUpdate.curVelocity = curVelocity2;
					}
					value.inheritedVelocity = Vector3.zero;
				}
				else
				{
					playerProjectileUpdate.curVelocity = velocity;
				}
			}
		}
		value.updates.Add(new FiredProjectileUpdate
		{
			OldPosition = value.position,
			NewPosition = playerProjectileUpdate.curPosition,
			OldVelocity = value.velocity,
			NewVelocity = playerProjectileUpdate.curVelocity,
			Mismatch = num,
			PartialTime = partialTime
		});
		value.position = playerProjectileUpdate.curPosition;
		value.velocity = playerProjectileUpdate.curVelocity;
		value.travelTime = playerProjectileUpdate.travelTime;
		value.partialTime = partialTime;
		value.trajectoryMismatch = num;
		value.positionOffset = default(Vector3);
		firedProjectiles[playerProjectileUpdate.projectileID] = value;
	}

	private void SimulateProjectile(ref Vector3 position, ref Vector3 velocity, ref float partialTime, float travelTime, Vector3 gravity, float drag, out Vector3 prevPosition, out Vector3 prevVelocity)
	{
		float num = 1f / 32f;
		prevPosition = position;
		prevVelocity = velocity;
		if (partialTime > Mathf.Epsilon)
		{
			float num2 = num - partialTime;
			if (travelTime < num2)
			{
				prevPosition = position;
				prevVelocity = velocity;
				position += velocity * travelTime;
				partialTime += travelTime;
				return;
			}
			prevPosition = position;
			prevVelocity = velocity;
			position += velocity * num2;
			velocity += gravity * num;
			velocity -= velocity * (drag * num);
			travelTime -= num2;
		}
		int num3 = Mathf.FloorToInt(travelTime / num);
		for (int i = 0; i < num3; i++)
		{
			prevPosition = position;
			prevVelocity = velocity;
			position += velocity * num;
			velocity += gravity * num;
			velocity -= velocity * (drag * num);
		}
		partialTime = travelTime - num * (float)num3;
		if (partialTime > Mathf.Epsilon)
		{
			prevPosition = position;
			prevVelocity = velocity;
			position += velocity * partialTime;
		}
	}

	protected virtual void CreateWorldProjectile(HitInfo info, ItemDefinition itemDef, ItemModProjectile itemMod, Projectile projectilePrefab, Item recycleItem)
	{
		if (Interface.CallHook("CanCreateWorldProjectile", info, itemDef) != null)
		{
			return;
		}
		Vector3 projectileVelocity = info.ProjectileVelocity;
		Item item = ((recycleItem != null) ? recycleItem : ItemManager.Create(itemDef, 1, 0uL, isServerSide: true, 0uL));
		if (Interface.CallHook("OnWorldProjectileCreate", info, item) != null)
		{
			return;
		}
		BaseEntity baseEntity = null;
		if (!info.DidHit)
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(projectileVelocity.normalized));
			baseEntity.Kill(DestroyMode.Gib);
			return;
		}
		if (projectilePrefab.breakProbability > 0f && UnityEngine.Random.value <= projectilePrefab.breakProbability)
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(projectileVelocity.normalized));
			baseEntity.Kill(DestroyMode.Gib);
			return;
		}
		if (projectilePrefab.conditionLoss > 0f)
		{
			item.LoseCondition(projectilePrefab.conditionLoss * 100f);
			if (item.isBroken)
			{
				baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(projectileVelocity.normalized));
				baseEntity.Kill(DestroyMode.Gib);
				return;
			}
		}
		if (projectilePrefab.stickProbability > 0f && UnityEngine.Random.value <= projectilePrefab.stickProbability)
		{
			baseEntity = ((info.HitEntity == null) ? item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(projectileVelocity.normalized)) : ((info.HitBone != 0) ? item.CreateWorldObject(info.HitPositionLocal, Quaternion.LookRotation(info.HitNormalLocal * -1f), info.HitEntity, info.HitBone) : item.CreateWorldObject(info.HitPositionLocal, Quaternion.LookRotation(info.HitEntity.transform.InverseTransformDirection(projectileVelocity.normalized)), info.HitEntity)));
			DroppedItem droppedItem = baseEntity as DroppedItem;
			if (droppedItem != null)
			{
				droppedItem.StickIn();
			}
			else
			{
				baseEntity.GetComponent<Rigidbody>().isKinematic = true;
			}
		}
		else
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(projectileVelocity.normalized));
			Rigidbody component = baseEntity.GetComponent<Rigidbody>();
			component.AddForce(projectileVelocity.normalized * 200f);
			component.WakeUp();
		}
	}

	public void CleanupExpiredProjectiles()
	{
		foreach (KeyValuePair<int, FiredProjectile> item in firedProjectiles.Where((KeyValuePair<int, FiredProjectile> x) => x.Value.firedTime < UnityEngine.Time.realtimeSinceStartup - 8f - 1f).ToList())
		{
			Facepunch.Rust.Analytics.Azure.OnFiredProjectileRemoved(this, item.Value);
			firedProjectiles.Remove(item.Key);
			FiredProjectile obj = item.Value;
			Facepunch.Pool.Free(ref obj);
		}
	}

	public bool HasFiredProjectile(int id)
	{
		return firedProjectiles.ContainsKey(id);
	}

	public void NoteFiredProjectile(int projectileid, Vector3 startPos, Vector3 startVel, AttackEntity attackEnt, ItemDefinition firedItemDef, Guid projectileGroupId, Vector3 positionOffset, Item pickupItem = null)
	{
		BaseProjectile baseProjectile = attackEnt as BaseProjectile;
		ItemModProjectile component = firedItemDef.GetComponent<ItemModProjectile>();
		Projectile component2 = component.GetOverrideProjectile(baseProjectile).Get().GetComponent<Projectile>();
		if (startPos.IsNaNOrInfinity() || startVel.IsNaNOrInfinity())
		{
			string text = component2.name;
			AntiHack.Log(this, AntiHackType.ProjectileHack, "Contains NaN (" + text + ")");
			stats.combat.LogInvalid(this, baseProjectile, "projectile_nan");
			return;
		}
		int projectile_protection = ConVar.AntiHack.projectile_protection;
		Vector3 inheritedVelocity = ((attackEnt != null) ? attackEnt.GetInheritedVelocity(this, startVel.normalized) : Vector3.zero);
		if (projectile_protection >= 1)
		{
			float num = 1f - ConVar.AntiHack.projectile_forgiveness;
			float num2 = 1f + ConVar.AntiHack.projectile_forgiveness;
			float magnitude = startVel.magnitude;
			float num3 = component.GetMinVelocity();
			float num4 = component.GetMaxVelocity();
			BaseProjectile baseProjectile2 = attackEnt as BaseProjectile;
			if ((bool)baseProjectile2)
			{
				num3 *= baseProjectile2.GetProjectileVelocityScale();
				num4 *= baseProjectile2.GetProjectileVelocityScale(getMax: true);
			}
			num3 *= num;
			num4 *= num2;
			if (magnitude < num3)
			{
				string arg = component2.name;
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Velocity ({arg} with {magnitude} < {num3})");
				stats.combat.LogInvalid(this, baseProjectile, "projectile_minvelocity");
				return;
			}
			if (magnitude > num4)
			{
				string arg2 = component2.name;
				AntiHack.Log(this, AntiHackType.ProjectileHack, $"Velocity ({arg2} with {magnitude} > {num4})");
				stats.combat.LogInvalid(this, baseProjectile, "projectile_maxvelocity");
				return;
			}
		}
		FiredProjectile firedProjectile = Facepunch.Pool.Get<FiredProjectile>();
		firedProjectile.itemDef = firedItemDef;
		firedProjectile.itemMod = component;
		firedProjectile.projectilePrefab = component2;
		firedProjectile.firedTime = UnityEngine.Time.realtimeSinceStartup;
		firedProjectile.travelTime = 0f;
		firedProjectile.weaponSource = attackEnt;
		firedProjectile.weaponPrefab = ((attackEnt == null) ? null : GameManager.server.FindPrefab(StringPool.Get(attackEnt.prefabID)).GetComponent<AttackEntity>());
		firedProjectile.projectileModifier = ((baseProjectile == null) ? Projectile.Modifier.Default : baseProjectile.GetProjectileModifier());
		firedProjectile.pickupItem = pickupItem;
		firedProjectile.integrity = 1f;
		firedProjectile.position = startPos;
		firedProjectile.initialPositionOffset = positionOffset;
		firedProjectile.positionOffset = positionOffset;
		firedProjectile.velocity = startVel;
		firedProjectile.initialPosition = startPos;
		firedProjectile.initialVelocity = startVel;
		firedProjectile.inheritedVelocity = inheritedVelocity;
		firedProjectile.protection = projectile_protection;
		firedProjectile.ricochets = 0;
		firedProjectile.hits = 0;
		firedProjectile.id = projectileid;
		firedProjectile.attacker = this;
		firedProjectile.simulatedPositions.Add(startPos);
		firedProjectiles.Add(projectileid, firedProjectile);
		Facepunch.Rust.Analytics.Azure.OnFiredProjectile(this, firedProjectile, projectileGroupId);
	}

	public void ServerNoteFiredProjectile(int projectileid, Vector3 startPos, Vector3 startVel, AttackEntity attackEnt, ItemDefinition firedItemDef, Item pickupItem = null)
	{
		BaseProjectile baseProjectile = attackEnt as BaseProjectile;
		ItemModProjectile component = firedItemDef.GetComponent<ItemModProjectile>();
		Projectile component2 = component.GetOverrideProjectile(baseProjectile).Get().GetComponent<Projectile>();
		int protection = 0;
		Vector3 zero = Vector3.zero;
		if (!startPos.IsNaNOrInfinity() && !startVel.IsNaNOrInfinity())
		{
			FiredProjectile firedProjectile = Facepunch.Pool.Get<FiredProjectile>();
			firedProjectile.itemDef = firedItemDef;
			firedProjectile.itemMod = component;
			firedProjectile.projectilePrefab = component2;
			firedProjectile.firedTime = UnityEngine.Time.realtimeSinceStartup;
			firedProjectile.travelTime = 0f;
			firedProjectile.weaponSource = attackEnt;
			firedProjectile.weaponPrefab = ((attackEnt == null) ? null : GameManager.server.FindPrefab(StringPool.Get(attackEnt.prefabID)).GetComponent<AttackEntity>());
			firedProjectile.projectileModifier = ((baseProjectile == null) ? Projectile.Modifier.Default : baseProjectile.GetProjectileModifier());
			firedProjectile.pickupItem = pickupItem;
			firedProjectile.integrity = 1f;
			firedProjectile.trajectoryMismatch = 0f;
			firedProjectile.position = startPos;
			firedProjectile.positionOffset = Vector3.zero;
			firedProjectile.velocity = startVel;
			firedProjectile.initialPosition = startPos;
			firedProjectile.initialVelocity = startVel;
			firedProjectile.inheritedVelocity = zero;
			firedProjectile.protection = protection;
			firedProjectile.ricochets = 0;
			firedProjectile.hits = 0;
			firedProjectile.id = projectileid;
			firedProjectile.attacker = this;
			firedProjectiles.Add(projectileid, firedProjectile);
		}
	}

	public void ApplyRadiation(float radsAmount, bool protection = true)
	{
		if (IsAlive() && !IsSleeping() && !InSafeZone())
		{
			float num = 0f;
			num = (protection ? Radiation.GetRadiationAfterProtection(radsAmount, RadiationProtection()) : Mathf.Max(0f, radsAmount));
			metabolism.ApplyChange(MetabolismAttribute.Type.Radiation, num, 0f);
		}
	}

	public void PlayerInventoryRadioactivityChange(float radAmount, bool hasRads)
	{
		if (!Radiation.water_inventory_damage)
		{
			return;
		}
		if (inflictInventoryRadsAction == null)
		{
			inflictInventoryRadsAction = InflictRadsFromInventory;
		}
		inventoryRads = radAmount;
		if (!hasRads || radAmount < 2500f)
		{
			if (IsInvoking(inflictInventoryRadsAction))
			{
				CancelInvoke(inflictInventoryRadsAction);
			}
		}
		else if (!IsInvoking(inflictInventoryRadsAction))
		{
			InvokeRepeating(inflictInventoryRadsAction, 1f, 1f);
		}
	}

	private void InflictRadsFromInventory()
	{
		if (Radiation.water_inventory_damage)
		{
			float num = inventoryRads * Radiation.MaterialToRadsRatio;
			num *= 0.05f;
			ApplyRadiation(num);
		}
	}

	public void RadioactiveLootCheck(List<ItemContainer> containerRefs)
	{
		radiationCheckContainers.Clear();
		radiationCheckContainers.AddRange(containerRefs);
		HasOpenedLoot();
	}

	private void HasOpenedLoot()
	{
		if (Radiation.water_loot_damage)
		{
			hasOpenedLoot = true;
			CheckRadsInContainer();
			InflictRadsFromContainer();
			if (inflictRadsAction == null)
			{
				inflictRadsAction = InflictRadsFromContainer;
			}
			if (checkRadsAction == null)
			{
				checkRadsAction = CheckRadsInContainer;
			}
			if (!IsInvoking(checkRadsAction))
			{
				InvokeRepeating(checkRadsAction, 1f, 2500f);
			}
			if (!IsInvoking(inflictRadsAction))
			{
				InvokeRepeating(inflictRadsAction, 1f, 1f);
			}
		}
	}

	public void HasClosedLoot()
	{
		if (IsInvoking(inflictRadsAction))
		{
			CancelInvoke(inflictRadsAction);
		}
		hasOpenedLoot = false;
	}

	private void InflictRadsFromContainer()
	{
		if (!Radiation.water_loot_damage)
		{
			return;
		}
		if (!hasOpenedLoot)
		{
			if (IsInvoking(checkRadsAction))
			{
				CancelInvoke(checkRadsAction);
			}
			if (IsInvoking(inflictRadsAction))
			{
				CancelInvoke(inflictRadsAction);
			}
		}
		else
		{
			ApplyRadiation(containerRads);
		}
	}

	private void CheckRadsInContainer()
	{
		if (!hasOpenedLoot)
		{
			return;
		}
		containerRads = 0f;
		foreach (ItemContainer radiationCheckContainer in radiationCheckContainers)
		{
			containerRads += radiationCheckContainer.GetRadioactiveMaterialInContainer() * Radiation.MaterialToRadsRatio;
		}
		containerRads *= 0.05f;
	}

	public bool IsRagdolling()
	{
		return HasPlayerFlag(PlayerFlags.Ragdolling);
	}

	protected virtual bool AllowRagdoll()
	{
		return true;
	}

	public void Ragdoll(Vector3 velocityOverride = default(Vector3), bool matchPlayerGravity = true, bool flailInAir = false, bool dieOnImpact = false, BaseEntity initiator = null)
	{
		if (!ConVar.Physics.allowplayertempragdoll)
		{
			EnsureDismounted();
		}
		else if (!UsedAdminCheat() && AllowRagdoll())
		{
			BaseRagdoll baseRagdoll = CreateRagdoll(base.transform.position, base.transform.rotation, velocityOverride, matchPlayerGravity, flailInAir, dieOnImpact, initiator);
			EnsureDismounted();
			baseRagdoll.AttemptMount(this, doMountChecks: false);
			if (mounted.Get(serverside: true) is BaseRagdoll)
			{
				SetPlayerFlag(PlayerFlags.Ragdolling, b: true);
			}
			SendNetworkUpdateImmediate();
		}
	}

	private BaseRagdoll CreateRagdoll(Vector3 position, Quaternion rotation, Vector3 velocityOverride, bool matchPlayerGravity, bool flailInAir, bool dieOnImpact, BaseEntity initiator)
	{
		BaseRagdoll baseRagdoll = GameManager.server.CreateEntity("assets/prefabs/player/player_temp_ragdoll.prefab") as BaseRagdoll;
		baseRagdoll.transform.SetPositionAndRotation(position, rotation);
		Ragdoll component = baseRagdoll.GetComponent<Ragdoll>();
		if (component != null)
		{
			component.simOnServer = true;
		}
		baseRagdoll.InitFromPlayer(this, velocityOverride, matchPlayerGravity, flailInAir, dieOnImpact, initiator);
		baseRagdoll.Spawn();
		BaseMountable baseMountable = GetMounted();
		if ((bool)baseMountable)
		{
			baseRagdoll.gameObject.SetIgnoreCollisions(baseMountable.gameObject, ignore: true);
		}
		return baseRagdoll;
	}

	public override bool CanUseNetworkCache(Network.Connection connection)
	{
		if (net == null)
		{
			return true;
		}
		if (connection.authLevel != 0)
		{
			return false;
		}
		if (net.connection != connection)
		{
			return true;
		}
		return false;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		HandleMountedOnLoad();
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			OcclusionInitGroup(canBeInAGroup: true);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		BasePlayer basePlayer2 = ((info.forConnection != null && info.forConnection.player is BasePlayer basePlayer) ? basePlayer : null);
		bool flag = basePlayer2 != null;
		bool flag2 = net != null && net.connection == info.forConnection;
		bool flag3 = !info.forDisk && flag && basePlayer2.IsAdmin;
		bool flag4 = flag && playersRecordingClientDemos.Contains(basePlayer2);
		info.msg.basePlayer = Facepunch.Pool.Get<ProtoBuf.BasePlayer>();
		info.msg.basePlayer.userid = userID;
		info.msg.basePlayer.name = displayName;
		info.msg.basePlayer.playerFlags = (int)playerFlags;
		info.msg.basePlayer.currentTeam = ((info.forDisk || flag2 || flag3) ? currentTeam : ((ulong)((currentTeam != 0L) ? (-1) : 0)));
		info.msg.basePlayer.heldEntity = svActiveItemID;
		info.msg.basePlayer.reputation = reputation;
		if (!info.forDisk && currentGesture != null && currentGesture.animationType == GestureConfig.AnimationType.Loop)
		{
			info.msg.basePlayer.loopingGesture = currentGesture.gestureId;
		}
		if (IsConnected && (IsAdmin || IsDeveloper))
		{
			info.msg.basePlayer.skinCol = net.connection.info.GetFloat("global.skincol", -1f);
			info.msg.basePlayer.skinTex = net.connection.info.GetFloat("global.skintex", -1f);
			info.msg.basePlayer.skinMesh = net.connection.info.GetFloat("global.skinmesh", -1f);
		}
		else
		{
			info.msg.basePlayer.skinCol = -1f;
			info.msg.basePlayer.skinTex = -1f;
			info.msg.basePlayer.skinMesh = -1f;
		}
		info.msg.basePlayer.underwear = GetUnderwearSkin(info.cachedTime.Time);
		info.msg.basePlayer.paintballColor = server_paintballColor;
		if (info.forDisk || flag2 || flag4)
		{
			info.msg.basePlayer.metabolism = metabolism.Save();
			info.msg.basePlayer.modifiers = null;
			if (modifiers != null)
			{
				info.msg.basePlayer.modifiers = modifiers.Save(info.forDisk);
			}
		}
		if (!info.forDisk && !flag2)
		{
			info.msg.basePlayer.playerFlags &= -5;
			info.msg.basePlayer.playerFlags &= -129;
			if (info.msg.baseCombat != null && !flag3)
			{
				info.msg.baseCombat.health = 100f;
				info.msg.basePlayer.playerFlags &= -33;
			}
		}
		info.msg.basePlayer.inventory = inventory.Save(info.forDisk || flag2);
		ModelState ms = modelState.Copy();
		UpdateModelState(ms);
		info.msg.basePlayer.modelState = ms;
		if (info.forDisk)
		{
			BaseEntity baseEntity = mounted.Get(base.isServer);
			if (baseEntity.IsValid())
			{
				if (baseEntity.enableSaving)
				{
					info.msg.basePlayer.mounted = mounted.uid;
				}
				else
				{
					BaseVehicle mountedVehicle = GetMountedVehicle();
					if (mountedVehicle.IsValid() && mountedVehicle.enableSaving)
					{
						info.msg.basePlayer.mounted = mountedVehicle.net.ID;
					}
				}
			}
			info.msg.basePlayer.respawnId = respawnId;
		}
		else
		{
			info.msg.basePlayer.mounted = mounted.uid;
		}
		if (flag2)
		{
			if (cachedPersistantPlayer != null)
			{
				info.msg.basePlayer.persistantData = cachedPersistantPlayer.Copy();
			}
			if (!info.forDisk)
			{
				PlayerState cached = SingletonComponent<ServerMgr>.Instance.playerStateManager.GetCached(userID);
				if (cached != null && cached.missions != null)
				{
					info.msg.basePlayer.missions?.Dispose();
					info.msg.basePlayer.missions = cached.missions.Copy();
				}
			}
		}
		info.msg.basePlayer.bagCount = SleepingBag.GetSleepingBagCount(userID);
		info.msg.basePlayer.shelterCount = LegacyShelter.GetShelterCount(userID);
		info.msg.basePlayer.bbsCount = BoatBuildingStation.GetBBSCount(userID);
		info.msg.basePlayer.mortarCooldown = mortarCooldown.LeftFrom(info.cachedTime.Time);
		if (info.forDisk)
		{
			info.msg.basePlayer.loadingTimeout = timeUntilLoadingExpires;
			info.msg.basePlayer.currentLife = lifeStory;
			info.msg.basePlayer.previousLife = previousLifeStory;
		}
		if (!info.forDisk)
		{
			info.msg.basePlayer.clanId = clanId;
		}
		if (info.forDisk && inventory.crafting != null)
		{
			info.msg.basePlayer.itemCrafter = inventory.crafting.Save();
		}
		if (info.forDisk && !IsBot)
		{
			SavePlayerState();
		}
		info.msg.basePlayer.tutorialAllowance = (int)CurrentTutorialAllowance;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.basePlayer == null)
		{
			return;
		}
		ProtoBuf.BasePlayer basePlayer = info.msg.basePlayer;
		if (info.fromDisk && IsBot && (ulong)userID != basePlayer.userid)
		{
			freeBotIds.Add(userID);
		}
		ulong num = userID;
		userID = basePlayer.userid;
		UserIDString = userID.Get().ToString();
		if (basePlayer.name != null)
		{
			displayName = basePlayer.name;
		}
		_ = playerFlags;
		playerFlags = (PlayerFlags)basePlayer.playerFlags;
		if ((ulong)userID != num)
		{
			UpdateGender();
		}
		currentTeam = basePlayer.currentTeam;
		reputation = basePlayer.reputation;
		if (basePlayer.modifiers != null && modifiers != null)
		{
			modifiers.Load(basePlayer.modifiers, info.fromDisk);
		}
		if (basePlayer.metabolism != null)
		{
			metabolism.Load(basePlayer.metabolism);
		}
		if (basePlayer.inventory != null)
		{
			inventory.Load(basePlayer.inventory);
		}
		if (basePlayer.modelState != null)
		{
			if (modelState != null)
			{
				modelState.ResetToPool();
				modelState = null;
			}
			modelState = basePlayer.modelState;
			basePlayer.modelState = null;
		}
		if (info.fromDisk)
		{
			timeUntilLoadingExpires = info.msg.basePlayer.loadingTimeout;
			if ((float)timeUntilLoadingExpires > 0f)
			{
				float time = Mathf.Clamp(timeUntilLoadingExpires, 0f, Nexus.loadingTimeout);
				Invoke(RemoveLoadingPlayerFlag, time);
			}
			lifeStory = info.msg.basePlayer.currentLife;
			if (lifeStory != null)
			{
				lifeStory.ShouldPool = false;
			}
			previousLifeStory = info.msg.basePlayer.previousLife;
			if (previousLifeStory != null)
			{
				previousLifeStory.ShouldPool = false;
			}
			SetPlayerFlag(PlayerFlags.Sleeping, b: false);
			StartSleeping();
			SetPlayerFlag(PlayerFlags.Connected, b: false);
			if (lifeStory == null && IsAlive())
			{
				LifeStoryStart();
			}
			mounted.uid = info.msg.basePlayer.mounted;
			if (IsWounded())
			{
				Die();
			}
			respawnId = info.msg.basePlayer.respawnId;
			if (info.msg.basePlayer.itemCrafter?.queue != null)
			{
				inventory.crafting.Load(info.msg.basePlayer.itemCrafter);
			}
			server_paintballColor = info.msg.basePlayer.paintballColor;
		}
		if (!info.fromDisk)
		{
			clanId = info.msg.basePlayer.clanId;
		}
		CurrentTutorialAllowance = (TutorialItemAllowance)info.msg.basePlayer.tutorialAllowance;
		mortarCooldown = info.msg.basePlayer.mortarCooldown;
	}

	internal void LifeStoryStart()
	{
		if (lifeStory != null)
		{
			lifeStory = null;
		}
		lifeStory = new PlayerLifeStory
		{
			ShouldPool = false,
			wipeId = SaveRestore.WipeId
		};
		lifeStory.timeBorn = (uint)Epoch.Current;
		hasSentPresenceState = false;
	}

	public void LifeStoryEnd()
	{
		SingletonComponent<ServerMgr>.Instance.persistance.AddLifeStory(userID, lifeStory);
		if (lifeStory != null)
		{
			Facepunch.Rust.Analytics.Azure.OnPlayerLifeStoryEnd(this, lifeStory);
		}
		previousLifeStory = lifeStory;
		lifeStory = null;
	}

	internal void LifeStoryUpdate(float deltaTime, float moveSpeed)
	{
		if (lifeStory != null)
		{
			lifeStory.secondsAlive += deltaTime;
			nextTimeCategoryUpdate -= deltaTime * ((moveSpeed > 0.1f) ? 1f : 0.25f);
			if (nextTimeCategoryUpdate <= 0f && !waitingForLifeStoryUpdate)
			{
				nextTimeCategoryUpdate = 7f + 7f * UnityEngine.Random.Range(0.2f, 1f);
				waitingForLifeStoryUpdate = true;
				lifeStoryQueue.Add(this);
			}
			if (LifeStoryInWilderness)
			{
				lifeStory.secondsWilderness += deltaTime;
			}
			if (LifeStoryInMonument)
			{
				lifeStory.secondsInMonument += deltaTime;
			}
			if (LifeStoryInBase)
			{
				lifeStory.secondsInBase += deltaTime;
			}
			if (LifeStoryFlying)
			{
				lifeStory.secondsFlying += deltaTime;
			}
			if (LifeStoryBoating)
			{
				lifeStory.secondsBoating += deltaTime;
			}
			if (LifeStorySwimming)
			{
				lifeStory.secondsSwimming += deltaTime;
			}
			if (LifeStoryDriving)
			{
				lifeStory.secondsDriving += deltaTime;
			}
			if (IsSleeping())
			{
				lifeStory.secondsSleeping += deltaTime;
			}
			else if (IsRunning())
			{
				lifeStory.metersRun += moveSpeed * deltaTime;
			}
			else
			{
				lifeStory.metersWalked += moveSpeed * deltaTime;
			}
		}
	}

	private static void LifeStoryUpdate(in PlayerServerStates.ReadOnly playerStates, float deltaTime)
	{
		using (TimeWarning.New("LifeStoryUpdate"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			ReadOnlySpan<ModelState.Flag> readOnlySpan = playerStates.PlayerModelStateFlags;
			ReadOnlySpan<BasePlayer> readOnlySpan2 = objects;
			for (int i = 0; i < readOnlySpan2.Length; i++)
			{
				BasePlayer basePlayer = readOnlySpan2[i];
				bool flag = (readOnlySpan[basePlayer.ActivePlayerInd] & ModelState.Flag.OnGround) != 0;
				basePlayer.LifeStoryUpdate(deltaTime, flag ? basePlayer.estimatedSpeed : 0f);
			}
		}
	}

	public void UpdateTimeCategory()
	{
		using (TimeWarning.New("UpdateTimeCategory"))
		{
			waitingForLifeStoryUpdate = false;
			int num = currentTimeCategory;
			currentTimeCategory = 1;
			if (IsBuildingAuthed(cached: true, 45f))
			{
				currentTimeCategory = 4;
			}
			Vector3 position = base.transform.position;
			if (TerrainMeta.TopologyMap != null && ((uint)TerrainMeta.TopologyMap.GetTopology(position) & 0x400u) != 0 && TerrainMeta.Path != null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.shouldDisplayOnMap && monument.IsInBounds(position))
					{
						currentTimeCategory = 2;
						break;
					}
				}
			}
			if (IsSwimming())
			{
				currentTimeCategory |= 32;
			}
			if (isMounted)
			{
				BaseMountable baseMountable = GetMounted();
				if (baseMountable.mountTimeStatType == BaseMountable.MountStatType.Boating)
				{
					currentTimeCategory |= 16;
				}
				else if (baseMountable.mountTimeStatType == BaseMountable.MountStatType.Flying)
				{
					currentTimeCategory |= 8;
				}
				else if (baseMountable.mountTimeStatType == BaseMountable.MountStatType.Driving)
				{
					currentTimeCategory |= 64;
				}
			}
			else if (HasParent() && GetParentEntity() is BaseMountable baseMountable2)
			{
				if (baseMountable2.mountTimeStatType == BaseMountable.MountStatType.Boating)
				{
					currentTimeCategory |= 16;
				}
				else if (baseMountable2.mountTimeStatType == BaseMountable.MountStatType.Flying)
				{
					currentTimeCategory |= 8;
				}
				else if (baseMountable2.mountTimeStatType == BaseMountable.MountStatType.Driving)
				{
					currentTimeCategory |= 64;
				}
			}
			if (num != currentTimeCategory || !hasSentPresenceState)
			{
				LifeStoryInWilderness = (1 & currentTimeCategory) != 0;
				LifeStoryInMonument = (2 & currentTimeCategory) != 0;
				LifeStoryInBase = (4 & currentTimeCategory) != 0;
				LifeStoryFlying = (8 & currentTimeCategory) != 0;
				LifeStoryBoating = (0x10 & currentTimeCategory) != 0;
				LifeStorySwimming = (0x20 & currentTimeCategory) != 0;
				LifeStoryDriving = (0x40 & currentTimeCategory) != 0;
				ClientRPC(RpcTarget.Player("UpdateRichPresenceState", this), currentTimeCategory);
				hasSentPresenceState = true;
			}
		}
	}

	public void LifeStoryShotFired(BaseEntity withWeapon)
	{
		if (lifeStory == null)
		{
			return;
		}
		if (lifeStory.weaponStats == null)
		{
			lifeStory.weaponStats = Facepunch.Pool.Get<List<PlayerLifeStory.WeaponStats>>();
		}
		foreach (PlayerLifeStory.WeaponStats weaponStat in lifeStory.weaponStats)
		{
			if (weaponStat.weaponName == withWeapon.ShortPrefabName)
			{
				weaponStat.shotsFired++;
				return;
			}
		}
		PlayerLifeStory.WeaponStats weaponStats = Facepunch.Pool.Get<PlayerLifeStory.WeaponStats>();
		weaponStats.weaponName = withWeapon.ShortPrefabName;
		weaponStats.shotsFired++;
		lifeStory.weaponStats.Add(weaponStats);
	}

	public void LifeStoryShotHit(BaseEntity withWeapon)
	{
		if (lifeStory == null || withWeapon == null)
		{
			return;
		}
		if (lifeStory.weaponStats == null)
		{
			lifeStory.weaponStats = Facepunch.Pool.Get<List<PlayerLifeStory.WeaponStats>>();
		}
		foreach (PlayerLifeStory.WeaponStats weaponStat in lifeStory.weaponStats)
		{
			if (weaponStat.weaponName == withWeapon.ShortPrefabName)
			{
				weaponStat.shotsHit++;
				return;
			}
		}
		PlayerLifeStory.WeaponStats weaponStats = Facepunch.Pool.Get<PlayerLifeStory.WeaponStats>();
		weaponStats.weaponName = withWeapon.ShortPrefabName;
		weaponStats.shotsHit++;
		lifeStory.weaponStats.Add(weaponStats);
	}

	public void LifeStoryKill(BaseCombatEntity killed)
	{
		if (lifeStory != null)
		{
			if (killed is ScientistNPC || killed is ScientistNPC2)
			{
				lifeStory.killedScientists++;
			}
			else if (killed is BasePlayer)
			{
				lifeStory.killedPlayers++;
			}
			else if (killed is BaseAnimalNPC || killed is BaseNPC2 { IsAnimal: not false } || killed is SnakeHazard)
			{
				lifeStory.killedAnimals++;
			}
		}
	}

	public void LifeStoryGenericStat(string key, int value)
	{
		if (lifeStory == null)
		{
			return;
		}
		if (lifeStory.genericStats == null)
		{
			lifeStory.genericStats = Facepunch.Pool.Get<List<PlayerLifeStory.GenericStat>>();
		}
		foreach (PlayerLifeStory.GenericStat genericStat2 in lifeStory.genericStats)
		{
			if (genericStat2.key == key)
			{
				genericStat2.value += value;
				return;
			}
		}
		PlayerLifeStory.GenericStat genericStat = Facepunch.Pool.Get<PlayerLifeStory.GenericStat>();
		genericStat.key = key;
		genericStat.value = value;
		lifeStory.genericStats.Add(genericStat);
	}

	public void LifeStoryHurt(float amount)
	{
		if (lifeStory != null)
		{
			lifeStory.totalDamageTaken += amount;
		}
	}

	public void LifeStoryHeal(float amount)
	{
		if (lifeStory != null)
		{
			lifeStory.totalHealing += amount;
		}
	}

	public void SetOverrideDeathBlow(PlayerLifeStory.DeathInfo info)
	{
		cachedOverrideDeathInfo = info;
	}

	internal void LifeStoryLogDeath(in DeathBlow deathBlow, DamageType lastDamage)
	{
		if (lifeStory == null)
		{
			return;
		}
		lifeStory.timeDied = (uint)Epoch.Current;
		PlayerLifeStory.DeathInfo deathInfo = cachedOverrideDeathInfo ?? Facepunch.Pool.Get<PlayerLifeStory.DeathInfo>();
		deathInfo.lastDamageType = (int)lastDamage;
		cachedOverrideDeathInfo = null;
		if (deathBlow.IsValid)
		{
			if (deathBlow.Initiator != null)
			{
				deathBlow.Initiator.AttackerInfo(deathInfo);
				deathInfo.attackerDistance = Distance(deathBlow.Initiator);
			}
			if (deathBlow.WeaponPrefab != null)
			{
				deathInfo.inflictorName = deathBlow.WeaponPrefab.ShortPrefabName;
			}
			if (deathBlow.HitBone != 0)
			{
				deathInfo.hitBone = StringPool.Get(deathBlow.HitBone);
			}
			else
			{
				deathInfo.hitBone = "";
			}
		}
		else if (base.SecondsSinceAttacked <= 60f && lastAttacker != null)
		{
			lastAttacker.AttackerInfo(deathInfo);
		}
		lifeStory.deathInfo = deathInfo;
	}

	internal override void OnParentRemoved()
	{
		if (IsNpc)
		{
			base.OnParentRemoved();
		}
		else
		{
			SetParent(null, worldPositionStays: true, sendImmediate: true);
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		bool flag = ActivePlayerInd == -1;
		bool flag2 = false;
		if (oldParent != null)
		{
			TransformState(oldParent.transform.localToWorldMatrix);
			flag2 = flag && oldParent.syncPosition && (oldParent.net?.group?.isGlobal).GetValueOrDefault();
		}
		bool flag3 = false;
		if (newParent != null)
		{
			TransformState(newParent.transform.worldToLocalMatrix);
			flag3 = flag && newParent.syncPosition && (newParent.net?.group?.isGlobal).GetValueOrDefault();
		}
		if (flag && PositionTickRate < 0f)
		{
			bool flag4 = base.NetworkPosTickCallback != null && SingletonComponent<InvokeHandler>.Instance.IsInvoking(base.NetworkPosTickCallback);
			if (flag3 && !flag4)
			{
				if (base.NetworkPosTickCallback == null)
				{
					Action action2 = (base.NetworkPosTickCallback = base.NetworkPositionTick);
				}
				InvokeRandomized(base.NetworkPosTickCallback, base.PositionTickRate, base.PositionTickRate - PositionTickRate * 0.05f, base.PositionTickRate * 0.05f);
			}
			else if (flag2 && flag4)
			{
				CancelInvoke(base.NetworkPosTickCallback);
			}
		}
		tickHistory.Reset();
		if (newParent != null && ConVar.AntiHack.parenthistory)
		{
			tickHistory.AddPoint(newParent.transform.InverseTransformPoint(base.transform.position), tickHistoryCapacity);
			tickHistory.AddParentPoint(newParent.transform.position, tickHistoryCapacity);
		}
	}

	private void TransformState(Matrix4x4 matrix)
	{
		if (ActivePlayerInd != -1)
		{
			PlayerStates.TickCache.TransformEntries(ActivePlayerInd, in matrix);
		}
		tickHistory.TransformEntries(matrix);
		if (eyes != null)
		{
			Vector3 euler = new Vector3(0f, matrix.rotation.eulerAngles.y, 0f);
			eyes.bodyRotation = Quaternion.Euler(euler) * eyes.bodyRotation;
		}
	}

	public void RecordParentPosition(int limit)
	{
		if (ConVar.AntiHack.parenthistory)
		{
			Transform parent = base.transform.parent;
			if (!(parent == null))
			{
				tickHistory.AddParentPoint(parent.position, limit);
			}
		}
	}

	public float TickHistoryDistanceParented(Vector3 point)
	{
		return tickHistory.DistanceParented(this, point);
	}

	public bool CanSuicide()
	{
		if (IsAdmin || IsDeveloper)
		{
			return true;
		}
		return UnityEngine.Time.realtimeSinceStartup > nextSuicideTime;
	}

	public void MarkSuicide()
	{
		nextSuicideTime = UnityEngine.Time.realtimeSinceStartup + 60f;
	}

	public bool CanRespawn()
	{
		return UnityEngine.Time.realtimeSinceStartup > nextRespawnTime;
	}

	public void MarkRespawn(float nextSpawnDelay = 5f)
	{
		nextRespawnTime = UnityEngine.Time.realtimeSinceStartup + nextSpawnDelay;
	}

	public void MovePosition(Vector3 newPos, bool forceUpdateTriggers = true)
	{
		base.transform.position = newPos;
		if (ActivePlayerInd != -1)
		{
			BaseEntity baseEntity = parentEntity.Get(base.isServer);
			Vector3 point = ((baseEntity != null) ? baseEntity.transform.InverseTransformPoint(newPos) : newPos);
			PlayerStates.TickCache.Reset(this, point);
		}
		ticksPerSecond.Increment();
		tickHistory.AddPoint(newPos, tickHistoryCapacity);
		RecordParentPosition(tickHistoryCapacity);
		NetworkPositionTick();
		if ((!IsNpc || !isMounted) && forceUpdateTriggers)
		{
			ForceUpdateTriggers();
		}
	}

	public void OverrideViewAngles(Vector3 newAng)
	{
		viewAngles = newAng;
	}

	public override void ServerInit()
	{
		stats = new PlayerStatistics(this);
		if ((ulong)userID == 0L)
		{
			if (!CollectionEx.IsEmpty(freeBotIds))
			{
				userID = freeBotIds[freeBotIds.Count - 1];
				freeBotIds.RemoveAt(freeBotIds.Count - 1);
			}
			else if (botIdCounter < 10000000)
			{
				userID = botIdCounter++;
			}
			else
			{
				userID = (ulong)UnityEngine.Random.Range(0, 10000000);
				Debug.LogError("Exhausted all bot user IDs! This can cause unexpected issues");
			}
			UserIDString = userID.Get().ToString();
			displayName = UserIDString;
			bots.Add(this);
			botColliderWorkQueue.Add(this);
		}
		EnablePlayerCollider();
		SetPlayerRigidbodyState(!IsSleeping());
		base.ServerInit();
		eyes.bodyRotation = base.transform.rotation;
		if (Query.Server != null)
		{
			Query.Server.AddPlayer(this);
		}
		UpdateGender();
		inventory.ServerInit(this);
		metabolism.ServerInit(this);
		metabolism.MarkNeedsFullSnapshot();
		if (modifiers != null)
		{
			modifiers.ServerInit(this);
		}
		if (recentWaveTargets != null)
		{
			recentWaveTargets.Clear();
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Query.Server.RemovePlayer(this);
		if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
		{
			FreeUnoccludedSubscribers();
		}
		lastPlayerVisibility.Clear();
		if ((bool)inventory)
		{
			inventory.DoDestroy();
		}
		sleepingPlayerList.Remove(this);
		sleepingPlayerLookup.Remove(userID);
		if (IsBot)
		{
			bots.Remove(this);
			botColliderWorkQueue.Remove(this);
			freeBotIds.Add(userID);
		}
		SavePlayerState();
		if (cachedPersistantPlayer != null)
		{
			cachedPersistantPlayer.Dispose();
			cachedPersistantPlayer = null;
		}
	}

	private static void AddToPlayerCache(BasePlayer player, Network.Connection c, ref PlayerServerStates playerStates)
	{
		Debug.Assert(player.ActivePlayerInd == -1, "Player already in PlayerCache!");
		StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
		player.ActivePlayerInd = playerCache.Add(player);
		int count = playerCache.Count;
		Transform transform = player.transform;
		transform.GetPositionAndRotation(out var position, out var rotation);
		NativeArrayEx.Expand(ref playerStates.PlayerLocalPos, count);
		playerStates.PlayerLocalPos[player.ActivePlayerInd] = transform.localPosition;
		NativeArrayEx.Expand(ref playerStates.PlayerPos, count);
		playerStates.PlayerPos[player.ActivePlayerInd] = position;
		NativeArrayEx.Expand(ref playerStates.LastFramePlayerPos, count);
		playerStates.LastFramePlayerPos[player.ActivePlayerInd] = Vector3.zero;
		NativeArrayEx.Expand(ref playerStates.PlayerLocalRots, count);
		playerStates.PlayerLocalRots[player.ActivePlayerInd] = transform.localRotation;
		NativeArrayEx.Expand(ref playerStates.PlayerRots, count);
		playerStates.PlayerRots[player.ActivePlayerInd] = rotation;
		NativeArrayEx.Expand(ref playerStates.WaterInfos, count);
		NativeArrayEx.Expand(ref playerStates.WaterFactors, count);
		NativeArrayEx.Expand(ref playerStates.CachedStates, count);
		playerStates.TickCache.Expand(count);
		playerStates.PlayerTransformsAccess.Add(transform);
		NativeArrayEx.Expand(ref playerStates.PlayerModelStateFlags, count);
		NativeArrayEx.Expand(ref playerStates.PlayerModelStateDucking, count);
		if (player.modelState != null)
		{
			playerStates.PlayerModelStateFlags[player.ActivePlayerInd] = (ModelState.Flag)player.modelState.flags;
			playerStates.PlayerModelStateDucking[player.ActivePlayerInd] = player.modelState.ducking;
		}
		else
		{
			playerStates.PlayerModelStateFlags[player.ActivePlayerInd] = ModelState.Flag.OnGround;
			playerStates.PlayerModelStateDucking[player.ActivePlayerInd] = 0f;
		}
		NativeArrayEx.Expand(ref playerStates.IsMounted, count);
		playerStates.IsMounted[player.ActivePlayerInd] = false;
		if (playerStates.Mountables.Capacity < count)
		{
			playerStates.Mountables.Resize(count);
		}
		playerStates.Mountables[player.ActivePlayerInd] = null;
		NativeArrayEx.Expand(ref playerStates.TickDeltaTime, count);
		playerStates.TickDeltaTime[player.ActivePlayerInd] = 0f;
		NativeArrayEx.Expand(ref playerStates.TickNeedsFinalizing, count);
		playerStates.TickNeedsFinalizing[player.ActivePlayerInd] = false;
		if (EACServer.CanSendAnalytics)
		{
			NativeArrayEx.Expand(ref EACTickStates, count * (int)Player.clientTickRate);
		}
		if (EACServer.ValidInterface)
		{
			NativeArrayEx.Expand(ref ClientHandles, count);
			ClientHandles[player.ActivePlayerInd] = EACServer.GetClient(c);
		}
		AntiHack.OnPlayerAddedToCache(player, playerCache, player.ActivePlayerInd);
	}

	private static void RemoveFromPlayerCache(BasePlayer player, ref PlayerServerStates playerStates)
	{
		Debug.Assert(player.ActivePlayerInd != -1, "Player not in the PlayerCache!");
		StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
		int activePlayerInd = player.ActivePlayerInd;
		int indexForSyncRemove = playerCache.GetIndexForSyncRemove(activePlayerInd);
		playerCache.RemoveAtSwapback(activePlayerInd, invalidateStableIndex: true);
		player.ActivePlayerInd = -1;
		playerStates.PlayerTransformsAccess.RemoveAtSwapBack(indexForSyncRemove);
		int count = playerCache.Count;
		if (indexForSyncRemove != count)
		{
			Debug.Assert(indexForSyncRemove < count, "Unexpected swap indices, expecting to swap from end to earlier in range!");
			playerCache.Objects[indexForSyncRemove].ActivePlayerInd = indexForSyncRemove;
			playerStates.PlayerLocalPos[indexForSyncRemove] = playerStates.PlayerLocalPos[count];
			playerStates.PlayerPos[indexForSyncRemove] = playerStates.PlayerPos[count];
			playerStates.LastFramePlayerPos[indexForSyncRemove] = playerStates.LastFramePlayerPos[count];
			playerStates.PlayerLocalRots[indexForSyncRemove] = playerStates.PlayerLocalRots[count];
			playerStates.PlayerRots[indexForSyncRemove] = playerStates.PlayerRots[count];
			playerStates.WaterInfos[indexForSyncRemove] = playerStates.WaterInfos[count];
			playerStates.WaterFactors[indexForSyncRemove] = playerStates.WaterFactors[count];
			playerStates.CachedStates[indexForSyncRemove] = playerStates.CachedStates[count];
			playerStates.TickCache.MovePlayer(count, indexForSyncRemove);
			playerStates.PlayerModelStateFlags[indexForSyncRemove] = playerStates.PlayerModelStateFlags[count];
			playerStates.PlayerModelStateDucking[indexForSyncRemove] = playerStates.PlayerModelStateDucking[count];
			playerStates.IsMounted[indexForSyncRemove] = playerStates.IsMounted[count];
			playerStates.Mountables[indexForSyncRemove] = playerStates.Mountables[count];
			playerStates.Mountables[count] = null;
			playerStates.TickDeltaTime[indexForSyncRemove] = playerStates.TickDeltaTime[count];
			playerStates.TickNeedsFinalizing[indexForSyncRemove] = playerStates.TickNeedsFinalizing[count];
			if (EACServer.CanSendAnalytics)
			{
				for (int i = 0; i < (int)Player.clientTickRate; i++)
				{
					EACTickStates[indexForSyncRemove * (int)Player.clientTickRate + i] = EACTickStates[count * (int)Player.clientTickRate + i];
				}
			}
			if (EACServer.ValidInterface)
			{
				ClientHandles[indexForSyncRemove] = ClientHandles[count];
			}
		}
		AntiHack.OnPlayerRemovedFromCache(player, count, indexForSyncRemove);
	}

	internal static void ServerUpdateParallel(float deltaTime, in PlayerServerStates playerStates)
	{
		if (!Network.Net.sv.IsConnected())
		{
			return;
		}
		using (TimeWarning.New("ServerUpdateParallel"))
		{
			CachePlayerTransforms(in playerStates);
			StableObjectArray<BasePlayer> playerCache = playerStates.PlayerCache;
			PlayerServerStates.ReadOnly playerStates2 = playerStates.AsReadOnly();
			LifeStoryUpdate(in playerStates2, deltaTime);
			using NativeList<int> toUpdate = new NativeList<int>(playerCache.Count, Allocator.Temp);
			FinalizeTickParallel(in playerStates, deltaTime, toUpdate);
			if (BaseMission.missionsenabled)
			{
				playerStates2 = playerStates.AsReadOnly();
				ServerThinkMissionsParallel(in playerStates2, deltaTime);
			}
			if (ConVar.AntiHack.terrain_protection > 0)
			{
				playerStates2 = playerStates.AsReadOnly();
				AntiHack.ValidateAgainstTerrain(in playerStates2);
			}
			float serverTickInterval = Player.serverTickInterval;
			playerStates2 = playerStates.AsReadOnly();
			ConnectedPlayersUpdate(in playerStates2, toUpdate.AsReadOnly(), deltaTime, serverTickInterval);
			playerStates2 = playerStates.AsReadOnly();
			ServerUpdatePlayerTickMisc(in playerStates2, toUpdate.AsReadOnly());
			playerStates2 = playerStates.AsReadOnly();
			ServerUpdatePlayerMutes(in playerStates2);
			ServerEnforceViolations(in playerStates);
			ServerKickIdlePlayers(in playerStates);
			ServerKickUnresponsivePlayers(in playerStates);
		}
	}

	private static void CachePlayerTransforms(in PlayerServerStates playerStates)
	{
		RecacheTransforms recacheTransforms = default(RecacheTransforms);
		recacheTransforms.LocalPos = playerStates.PlayerLocalPos;
		recacheTransforms.Pos = playerStates.PlayerPos;
		recacheTransforms.LocalRots = playerStates.PlayerLocalRots;
		recacheTransforms.Rots = playerStates.PlayerRots;
		RecacheTransforms jobData = recacheTransforms;
		IJobParallelForTransformExtensions.RunReadOnlyByRef(ref jobData, playerStates.PlayerTransformsAccess);
	}

	private static void ServerUpdatePlayerMutes(in PlayerServerStates.ReadOnly playerStates)
	{
		using (TimeWarning.New("ServerUpdatePlayerMutes"))
		{
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			long num = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				BasePlayer basePlayer = objects[i];
				if (basePlayer.HasPlayerFlag(PlayerFlags.ChatMute) && realtimeSinceStartup > basePlayer.nextMuteCheckTime)
				{
					basePlayer.nextMuteCheckTime = realtimeSinceStartup + 60f;
					if (basePlayer.State.chatMuteExpiryTimestamp > 0.0 && (double)num > basePlayer.State.chatMuteExpiryTimestamp)
					{
						basePlayer.State.chatMuted = false;
						basePlayer.State.chatMuteExpiryTimestamp = 0.0;
						basePlayer.SetPlayerFlag(PlayerFlags.ChatMute, b: false);
						basePlayer.ChatMessage("You have been unmuted");
					}
				}
			}
		}
	}

	private static void ServerEnforceViolations(in PlayerServerStates playerStates)
	{
		if (ConVar.AntiHack.enforcementlevel <= 0)
		{
			return;
		}
		using (TimeWarning.New("ServerEnforceViolations"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				if (AntiHack.EnforceViolations(objects[i]))
				{
					num--;
					i--;
				}
			}
		}
	}

	private static void ServerKickIdlePlayers(in PlayerServerStates playerStates)
	{
		if (ConVar.Server.idlekick <= 0 || ((SingletonComponent<ServerMgr>.Instance.AvailableSlots > 0 || ConVar.Server.idlekickmode != 1) && ConVar.Server.idlekickmode != 2))
		{
			return;
		}
		using (TimeWarning.New("ServerKickIdlePlayers"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				BasePlayer basePlayer = objects[i];
				if (!(basePlayer.IdleTime < (float)(ConVar.Server.idlekick * 60)) && (!basePlayer.IsAdmin || ConVar.Server.idlekickadmins != 0) && (!basePlayer.IsDeveloper || ConVar.Server.idlekickadmins != 0))
				{
					basePlayer.Kick($"Idle for {ConVar.Server.idlekick} minutes");
					num--;
					i--;
				}
			}
		}
	}

	private static void ServerKickUnresponsivePlayers(in PlayerServerStates playerStates)
	{
		using (TimeWarning.New("ServerKickUnresponsivePlayers"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			int num = objects.Length;
			for (int i = 0; i < num; i++)
			{
				BasePlayer basePlayer = objects[i];
				if (!basePlayer.IsReceivingSnapshot && basePlayer.IsAlive() && basePlayer.timeSinceLastTick > (float)ConVar.Server.playertimeout)
				{
					basePlayer.lastTickTime = 0f;
					basePlayer.Kick("Unresponsive");
					num--;
					i--;
				}
			}
		}
	}

	private static void ServerUpdatePlayerTickMisc(in PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly indices)
	{
		using (TimeWarning.New("ServerUpdatePlayerTickMisc"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			foreach (int item in indices)
			{
				BasePlayer basePlayer = objects[item];
				if (!basePlayer.IsNpc)
				{
					using (TimeWarning.New("TickPings"))
					{
						basePlayer.TickPings();
					}
				}
				using (TimeWarning.New("HeldEntityServerCycle"))
				{
					basePlayer.HeldEntityServerTick();
				}
			}
		}
	}

	private static void GatherPlayersToUpdate(in PlayerServerStates playerStates, float deltaTime, NativeList<int> indices)
	{
		using (TimeWarning.New("GatherPlayersToUpdate"))
		{
			double realtimeSinceStartupAsDouble = UnityEngine.Time.realtimeSinceStartupAsDouble;
			float serverTickInterval = Player.serverTickInterval;
			float maxdesync = ConVar.AntiHack.maxdesync;
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				BasePlayer basePlayer = objects[i];
				basePlayer.desyncTimeRaw = Mathf.Max(basePlayer.timeSinceLastTick - deltaTime, 0f);
				basePlayer.desyncTimeClamped = Mathf.Min(basePlayer.desyncTimeRaw, maxdesync);
				if (!(realtimeSinceStartupAsDouble < basePlayer.lastPlayerTick + (double)serverTickInterval))
				{
					if (basePlayer.lastPlayerTick < realtimeSinceStartupAsDouble - (double)(serverTickInterval * 100f))
					{
						basePlayer.lastPlayerTick = realtimeSinceStartupAsDouble - (double)UnityEngine.Random.Range(0f, serverTickInterval);
					}
					while (basePlayer.lastPlayerTick < realtimeSinceStartupAsDouble)
					{
						basePlayer.lastPlayerTick += serverTickInterval;
					}
					indices.AddNoResize(basePlayer.ActivePlayerInd);
				}
			}
		}
	}

	private void ServerUpdateBots(float deltaTime)
	{
		RefreshColliderSize(forced: false);
	}

	private static void ConnectedPlayersUpdate(in PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly indices, float deltaTime, float tickDeltaTime)
	{
		using (TimeWarning.New("ConnectedPlayersUpdate"))
		{
			SendEntityUpdates(playerStates.PlayerCache.UnsafeObjects, indices);
			using NativeList<int> nativeList = new NativeList<int>(indices.Length, Allocator.Temp);
			using NativeList<int> nativeList2 = new NativeList<int>(indices.Length, Allocator.Temp);
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			foreach (int item in indices)
			{
				BasePlayer basePlayer = objects[item];
				if (basePlayer.IsReceivingSnapshot)
				{
					if (basePlayer.SnapshotQueue.Length == 0 && EACServer.IsAuthenticated(basePlayer.net.connection))
					{
						basePlayer.EnterGame();
					}
					continue;
				}
				nativeList.AddNoResize(item);
				if (basePlayer.IsAlive())
				{
					nativeList2.AddNoResize(item);
				}
			}
			UpdateMetabolism(objects, nativeList2.AsReadOnly(), tickDeltaTime);
			UpdateModifiers(objects, nativeList2.AsReadOnly());
			UpdateHostility(objects, nativeList2.AsReadOnly(), tickDeltaTime);
			UpdateHeavyLandingAnims(objects, nativeList2.AsReadOnly());
			UpdateConnectedStates(objects, nativeList.AsReadOnly(), deltaTime);
			RefreshColliderSizes(objects, nativeList.AsReadOnly(), playerStates.CachedStates);
			SendModelStates(objects, nativeList.AsReadOnly());
		}
		static void RefreshColliderSizes(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices, NativeArray<CachedState>.ReadOnly cachedStates)
		{
			using (TimeWarning.New("RefreshColliderSizes"))
			{
				foreach (int item2 in indices)
				{
					BasePlayer obj = players[item2];
					bool isSwimming = cachedStates[item2].IsSwimming;
					obj.RefreshColliderSize(forced: false, isSwimming);
				}
			}
		}
		static void SendModelStates(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices)
		{
			using (TimeWarning.New("SendModelStates"))
			{
				foreach (int item3 in indices)
				{
					players[item3].SendModelState();
				}
			}
		}
		static void UpdateConnectedStates(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices, float deltaTime)
		{
			using (TimeWarning.New("UpdateConnectedStates"))
			{
				foreach (int item4 in indices)
				{
					BasePlayer basePlayer2 = players[item4];
					if (basePlayer2.stallProtectionTime > 0f)
					{
						basePlayer2.stallProtectionTime -= deltaTime;
					}
					int num = (int)basePlayer2.net.connection.GetSecondsConnected();
					int num2 = num - basePlayer2.secondsConnected;
					if (num2 > 0)
					{
						basePlayer2.stats.Add("time", num2, Stats.Server);
						basePlayer2.secondsConnected = num;
					}
					if (basePlayer2.IsLoadingAfterTransfer())
					{
						Debug.LogWarning("Force removing loading flag for player (sanity check failed)", basePlayer2);
						basePlayer2.SetPlayerFlag(PlayerFlags.LoadingAfterTransfer, b: false);
					}
					if (basePlayer2.State != null)
					{
						basePlayer2.SetPlayerFlag(PlayerFlags.ChatMute, basePlayer2.State.chatMuted);
					}
				}
			}
		}
		static void UpdateHeavyLandingAnims(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices)
		{
			using (TimeWarning.New("UpdateHeavyLandingAnims"))
			{
				foreach (int item5 in indices)
				{
					BasePlayer basePlayer3 = players[item5];
					if (basePlayer3.PlayHeavyLandingAnimation && !basePlayer3.modelState.mounted && basePlayer3.modelState.onground && Parachute.LandingAnimations)
					{
						basePlayer3.Server_StartGesture(GestureCollection.HeavyLandingId);
						basePlayer3.PlayHeavyLandingAnimation = false;
					}
				}
			}
		}
		static void UpdateHostility(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices, float tickDeltaTime)
		{
			using (TimeWarning.New("UpdateHostility"))
			{
				foreach (int item6 in indices)
				{
					BasePlayer basePlayer4 = players[item6];
					if (basePlayer4.InSafeZone() || basePlayer4.InHostileWarningZone())
					{
						float num3 = 0f;
						HeldEntity heldEntity = basePlayer4.GetHeldEntity();
						if ((bool)heldEntity && heldEntity.hostile)
						{
							num3 = tickDeltaTime;
						}
						if (num3 == 0f)
						{
							basePlayer4.MarkWeaponDrawnDuration(0f);
						}
						else
						{
							basePlayer4.AddWeaponDrawnDuration(num3);
						}
						if (basePlayer4.weaponDrawnDuration >= 8f)
						{
							basePlayer4.MarkHostileFor(30f);
						}
					}
					else
					{
						basePlayer4.MarkWeaponDrawnDuration(0f);
					}
				}
			}
		}
		static void UpdateMetabolism(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices, float tickDeltaTime)
		{
			using (TimeWarning.New("UpdateMetabolism"))
			{
				foreach (int item7 in indices)
				{
					BasePlayer basePlayer6 = players[item7];
					basePlayer6.metabolism.ServerUpdate(basePlayer6, tickDeltaTime);
				}
			}
		}
		static void UpdateModifiers(ReadOnlySpan<BasePlayer> players, NativeArray<int>.ReadOnly indices)
		{
			using (TimeWarning.New("UpdateModifiers"))
			{
				foreach (int item8 in indices)
				{
					BasePlayer basePlayer5 = players[item8];
					if (basePlayer5.modifiers != null)
					{
						basePlayer5.modifiers.ServerUpdate(basePlayer5);
					}
				}
			}
		}
	}

	public static void UpdateSubscriptions(in PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly indices, float currTime)
	{
		using (TimeWarning.New("UpdateSubscriptions"))
		{
			BufferList<Networkable> obj = Facepunch.Pool.Get<BufferList<Networkable>>();
			NativeList<int> nativeList = new NativeList<int>(indices.Length, Allocator.TempJob);
			NativeList<int> nativeList2 = new NativeList<int>(indices.Length, Allocator.TempJob);
			NativeList<int> nativeList3 = new NativeList<int>(indices.Length, Allocator.TempJob);
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			foreach (int item in indices)
			{
				BasePlayer basePlayer = objects[item];
				Debug.Assert(basePlayer.IsConnected);
				if (basePlayer.net.ShouldUpdateSubscriptions)
				{
					if (basePlayer.IsReceivingSnapshot)
					{
						obj.Add(basePlayer.net);
						nativeList.AddNoResize(int.MaxValue);
						nativeList2.AddNoResize(int.MaxValue);
						nativeList3.AddNoResize(item);
					}
					else if (currTime > basePlayer.lastSubscriptionTick + ConVar.Server.entitybatchtime)
					{
						obj.Add(basePlayer.net);
						nativeList.AddNoResize(ConVar.Server.entitybatchsize);
						nativeList2.AddNoResize(ConVar.Server.entitybatchsize * 2);
						nativeList3.AddNoResize(item);
						basePlayer.lastSubscriptionTick = currTime;
					}
				}
			}
			if (obj.Count > 0)
			{
				Networkable.UpdateSubscriptions(obj, nativeList2.AsArray(), nativeList.AsArray());
			}
			for (int i = 0; i < obj.Count; i++)
			{
				int index = nativeList3[i];
				bool updateSubscriptions = nativeList2[i] == int.MinValue || nativeList[i] == int.MinValue;
				objects[index].net.SetUpdateSubscriptions(updateSubscriptions);
			}
			nativeList3.Dispose();
			nativeList2.Dispose();
			nativeList.Dispose();
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
	}

	internal void EnterGame()
	{
		SetPlayerFlag(PlayerFlags.ReceivingSnapshot, b: false);
		bool flag = false;
		if (IsLoadingAfterTransfer())
		{
			SetPlayerFlag(PlayerFlags.LoadingAfterTransfer, b: false);
			EndSleeping();
			flag = true;
		}
		if (flag)
		{
			SendNetworkUpdateImmediate();
		}
		ClientRPC(RpcTarget.Player("FinishLoading", this));
		Invoke(DelayedTeamUpdate, 1f);
		if (PlayerStateEx.IsSaveStale(State))
		{
			State.protocol = 288;
			State.seed = World.Seed;
			State.saveCreatedTime = Epoch.FromDateTime(SaveRestore.SaveCreatedTime);
			Debug.Log("PlayerState was from old protocol or different seed, or not from a loaded save. Clearing player state");
			WipeMissions(saveImmediately: true);
			OnFogOfWarStale();
			if (State.toastOnReconnect != null)
			{
				State.toastOnReconnect.Clear();
			}
		}
		else
		{
			LoadMissions(State.missions);
			MissionsDirty(saveImmediately: true);
		}
		BaseMission.PlayerRequestedValidStatesUpdate(this);
		double num = State.unHostileTimestamp - TimeEx.currentTimestamp;
		if (num > 0.0)
		{
			ClientRPC(RpcTarget.Player("SetHostileLength", this), (float)num);
		}
		if (IsTransferProtected() && base.TransferProtectionRemaining > 0f)
		{
			ClientRPC(RpcTarget.Player("SetTransferProtectionDuration", this), base.TransferProtectionRemaining);
		}
		if ((ConVar.Server.deepSeaFogofwar || ConVar.Server.fogofwar) && !hasSentFogOfWar)
		{
			if (State.fogImageNetId != net.ID && State.fogImageNetId.Value != 0L)
			{
				FileStorage.server.ReassignEntityId(State.fogImageNetId, net.ID);
			}
			State.fogImageNetId = net.ID;
			hasSentFogOfWar = true;
			SendFogImagesToClient();
		}
		if (modifiers != null)
		{
			modifiers.ResetTicking();
		}
		if (net != null)
		{
			EACServer.OnFinishLoading(net.connection);
		}
		Debug.Log($"{this} has spawned");
		if ((Demo.recordlistmode == 0) ? Demo.recordlist.Contains(UserIDString) : (!Demo.recordlist.Contains(UserIDString)))
		{
			StartServerDemoRecording();
		}
		SendClientPetLink();
		ClientRPC(RpcTarget.Player("ForceViewAnglesTo", this), base.transform.forward);
		HandleTutorialOnGameEnter();
	}

	private void HandleTutorialOnGameEnter()
	{
		bool isInTutorial = IsInTutorial;
		BaseMission.MissionInstance instance;
		bool flag = TryGetActiveMissionInstance(out instance) && instance.GetMission() is TutorialMission;
		bool flag2 = !isInTutorial && flag;
		if (!flag2 && isInTutorial && TutorialIsland.RestoreOrCreateIslandForPlayer(this, triggerAnalytics: false) == null)
		{
			flag2 = true;
		}
		if (flag2)
		{
			ClearTutorial();
			Hurt(999999f);
			ClearTutorial_PostDeath();
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ClientKeepConnectionAlive(RPCMessage msg)
	{
		lastTickTime = UnityEngine.Time.time;
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void ClientLoadingComplete(RPCMessage msg)
	{
		TryDisableTransferProtectionOnLoaded();
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void Server_OnClientDemoRecordingStateChanged(RPCMessage msg)
	{
		if (net == null || net.connection == null || !(net.connection.player is BasePlayer basePlayer))
		{
			return;
		}
		if (!basePlayer.IsAdmin)
		{
			playersRecordingClientDemos.Remove(basePlayer);
			return;
		}
		bool flag = msg.read.Bool();
		bool flag2 = playersRecordingClientDemos.Contains(basePlayer);
		if (flag != flag2)
		{
			if (flag)
			{
				playersRecordingClientDemos.TryAdd(basePlayer);
				basePlayer.SendCompleteSnapshot();
			}
			else
			{
				playersRecordingClientDemos.Remove(basePlayer);
			}
		}
	}

	public void PlayerInit(Network.Connection c)
	{
		using (TimeWarning.New("PlayerInit", 10))
		{
			CancelInvoke(base.KillMessage);
			CancelInvoke(OfflineMetabolism);
			SetPlayerFlag(PlayerFlags.Connected, b: true);
			activePlayerList.Add(this);
			activePlayerLookup[c.userid] = this;
			AddToPlayerCache(this, c, ref PlayerStates);
			bots.Remove(this);
			botColliderWorkQueue.Remove(this);
			userID = c.userid;
			UserIDString = userID.Get().ToString();
			displayName = c.username;
			c.player = this;
			secondsConnected = 0;
			currentTeam = RelationshipManager.ServerInstance.FindPlayersTeam(userID)?.teamID ?? 0;
			SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerName(userID, displayName);
			cachedPersistantPlayer = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerInfo(userID);
			UpdateGender();
			Vector3 position = base.transform.position;
			PlayerStates.TickCache.Reset(this, position);
			tickHistory.Reset(position);
			eyeHistory.Clear();
			lastTickTime = 0f;
			lastInputTime = 0f;
			SetPlayerFlag(PlayerFlags.ReceivingSnapshot, b: true);
			lastSentActiveWorkbenchId = default(NetworkableId);
			stats.Init();
			InvokeRandomized(StatSave, UnityEngine.Random.Range(5f, 10f), 30f, UnityEngine.Random.Range(0f, 6f));
			previousLifeStory = SingletonComponent<ServerMgr>.Instance.persistance.GetLastLifeStory(userID);
			if (previousLifeStory != null && previousLifeStory.wipeId != SaveRestore.WipeId)
			{
				previousLifeStory = null;
			}
			SetPlayerFlag(PlayerFlags.IsAdmin, c.authLevel != 0);
			SetPlayerFlag(PlayerFlags.IsDeveloper, DeveloperList.IsDeveloper(this));
			if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
			{
				OcclusionInitGroup(canBeInAGroup: false);
			}
			if (IsDead() && net.SwitchGroup(BaseNetworkable.LimboNetworkGroup))
			{
				SendNetworkGroupChange();
			}
			net.OnConnected(c);
			net.StartSubscriber();
			_ = State;
			SendAsSnapshot(net.connection);
			GlobalNetworkHandler.server.StartSendingSnapshot(this);
			ClientRPC(RpcTarget.Player("StartLoading", this));
			if ((bool)BaseGameMode.GetActiveGameMode(serverside: true))
			{
				BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerConnected(this);
			}
			if (net != null)
			{
				EACServer.OnStartLoading(net.connection);
			}
			Interface.CallHook("IOnPlayerConnected", this);
			if (IsAdmin)
			{
				if (ConVar.AntiHack.noclip_protection <= 0)
				{
					ChatMessage("antihack.noclip_protection is disabled!");
				}
				if (ConVar.AntiHack.speedhack_protection <= 0)
				{
					ChatMessage("antihack.speedhack_protection is disabled!");
				}
				if (ConVar.AntiHack.flyhack_protection <= 0)
				{
					ChatMessage("antihack.flyhack_protection is disabled!");
				}
				if (ConVar.AntiHack.projectile_protection <= 0)
				{
					ChatMessage("antihack.projectile_protection is disabled!");
				}
				if (ConVar.AntiHack.melee_protection <= 0)
				{
					ChatMessage("antihack.melee_protection is disabled!");
				}
				if (ConVar.AntiHack.eye_protection <= 0)
				{
					ChatMessage("antihack.eye_protection is disabled!");
				}
				Command("debug.setcreative_ui", IsInCreativeMode);
				Command("debug.setinvis_ui", isInvisible);
				if (isInvisible)
				{
					invisPlayers.Add(this);
				}
			}
			inventory.crafting.SendToOwner();
			if (TerrainMeta.Path != null && TerrainMeta.Path.OceanPatrolFar != null)
			{
				SendCargoPatrolPath();
			}
			if (currentTeam == 0L && RelationshipManager.ServerInstance.HasPendingInvite(userID, out var foundTeamID) && RelationshipManager.ServerInstance.GetTeamLeaderInfo(foundTeamID, out var leaderDisplayName, out var leaderID))
			{
				ClientRPC(RpcTarget.Player("CLIENT_PendingInvite", this), leaderDisplayName, leaderID, foundTeamID);
			}
			requestingReputationUpdate = true;
		}
	}

	public void StatSave()
	{
		if (stats != null)
		{
			stats.Save();
		}
	}

	public void SendDeathInformation()
	{
		ClientRPC(RpcTarget.Player("OnDied", this));
	}

	public void SendRespawnOptions()
	{
		if (NexusServer.Started && ZoneController.Instance.CanRespawnAcrossZones(this))
		{
			CollectExternalAndSend();
			return;
		}
		List<RespawnInformation.SpawnOptions> list = Facepunch.Pool.Get<List<RespawnInformation.SpawnOptions>>();
		GetRespawnOptionsForPlayer(list, userID);
		Interface.CallHook("OnRespawnInformationGiven", this, list);
		SendToPlayer(list, loading: false);
		async void CollectExternalAndSend()
		{
			List<RespawnInformation.SpawnOptions> list2 = Facepunch.Pool.Get<List<RespawnInformation.SpawnOptions>>();
			GetRespawnOptionsForPlayer(list2, userID);
			List<RespawnInformation.SpawnOptions> allSpawnOptions = Facepunch.Pool.Get<List<RespawnInformation.SpawnOptions>>();
			foreach (RespawnInformation.SpawnOptions item in list2)
			{
				allSpawnOptions.Add(item.Copy());
			}
			SendToPlayer(list2, loading: true);
			try
			{
				Request request = Facepunch.Pool.Get<Request>();
				request.spawnOptions = Facepunch.Pool.Get<SpawnOptionsRequest>();
				request.spawnOptions.userId = userID;
				using (NexusRpcResult nexusRpcResult = await NexusServer.BroadcastRpc(request, 10f))
				{
					foreach (KeyValuePair<string, Response> response in nexusRpcResult.Responses)
					{
						string key = response.Key;
						SpawnOptionsResponse spawnOptions2 = response.Value.spawnOptions;
						if (spawnOptions2 != null && spawnOptions2.spawnOptions.Count != 0)
						{
							foreach (RespawnInformation.SpawnOptions spawnOption in spawnOptions2.spawnOptions)
							{
								RespawnInformation.SpawnOptions spawnOptions3 = spawnOption.Copy();
								spawnOptions3.nexusZone = key;
								allSpawnOptions.Add(spawnOptions3);
							}
						}
					}
				}
				SendToPlayer(allSpawnOptions, loading: false);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		void SendToPlayer(List<RespawnInformation.SpawnOptions> spawnOptions, bool loading)
		{
			using RespawnInformation respawnInformation = Facepunch.Pool.Get<RespawnInformation>();
			respawnInformation.spawnOptions = spawnOptions;
			respawnInformation.loading = loading;
			if (LegacyShelter.max_shelters == LegacyShelter.FpShelterDefault && LegacyShelter.SheltersPerPlayer.ContainsKey(userID) && LegacyShelter.SheltersPerPlayer[userID].Count > 0)
			{
				respawnInformation.shelterPositions = Facepunch.Pool.Get<List<Vector3>>();
				foreach (LegacyShelter item2 in LegacyShelter.SheltersPerPlayer[userID])
				{
					respawnInformation.shelterPositions.Add(item2.transform.position);
				}
			}
			if (IsDead())
			{
				respawnInformation.previousLife = previousLifeStory;
				if (!ConVar.Server.skipDeathScreenFade)
				{
					respawnInformation.fadeIn = previousLifeStory != null && previousLifeStory.timeDied > Epoch.Current - 5;
				}
				else
				{
					respawnInformation.fadeIn = false;
				}
			}
			ClientRPC(RpcTarget.Player("OnRespawnInformation", this), respawnInformation);
		}
	}

	public static void GetRespawnOptionsForPlayer(List<RespawnInformation.SpawnOptions> spawnOptions, ulong userID)
	{
		BasePlayer basePlayer = FindByID(userID);
		using PooledList<SleepingBag> pooledList = SleepingBag.FindForPlayer(userID);
		foreach (SleepingBag item in pooledList)
		{
			if ((!(item is StaticRespawnArea staticRespawnArea) || staticRespawnArea.IsAuthed(userID)) && (!(basePlayer != null) || basePlayer.IsInTutorial == item.IsTutorialBag))
			{
				RespawnInformation.SpawnOptions spawnOptions2 = Facepunch.Pool.Get<RespawnInformation.SpawnOptions>();
				spawnOptions2.id = item.net.ID;
				spawnOptions2.name = item.niceName;
				spawnOptions2.worldPosition = item.transform.position;
				spawnOptions2.type = (item.isStatic ? RespawnInformation.SpawnOptions.RespawnType.Static : item.RespawnType);
				spawnOptions2.unlockSeconds = item.GetUnlockSeconds(userID);
				spawnOptions2.respawnState = item.GetRespawnState(userID);
				spawnOptions2.mobile = item.IsMobile();
				spawnOptions2.corpse = item.HasFlag(Flags.Reserved14);
				spawnOptions2.deepSea = item.IsInsideDeepSea();
				spawnOptions2.showOnCompass = item.showOnCompass;
				spawnOptions2.favourite = item.favourite;
				spawnOptions.Add(spawnOptions2);
			}
		}
	}

	public bool HasRespawnOptions()
	{
		List<RespawnInformation.SpawnOptions> obj = Facepunch.Pool.Get<List<RespawnInformation.SpawnOptions>>();
		GetRespawnOptionsForPlayer(obj, userID);
		bool result = obj.Count > 0;
		Facepunch.Pool.Free(ref obj, freeElements: true);
		return result;
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	[RPC_Server.FromOwner]
	private void RequestRespawnInformation(RPCMessage msg)
	{
		SendRespawnOptions();
	}

	public void ScheduledDeath()
	{
		foreach (TriggerSafeZone allSafeZone in TriggerSafeZone.allSafeZones)
		{
			if (allSafeZone.triggerCollider.bounds.Intersects(WorldSpaceBounds().ToBounds()) && !(allSafeZone.Apartment == null))
			{
				ApartmentRoom playerApartment = allSafeZone.Apartment.GetPlayerApartment(this);
				if (!(playerApartment == null) && playerApartment.IsInsideRoom(this))
				{
					return;
				}
			}
		}
		PlayerLifeStory.DeathInfo deathInfo = Facepunch.Pool.Get<PlayerLifeStory.DeathInfo>();
		deathInfo.attackerName = "safezone";
		SetOverrideDeathBlow(deathInfo);
		Hurt(999f, DamageType.Suicide, null, useProtection: false);
	}

	public virtual void StartSleeping()
	{
		if (IsSleeping())
		{
			return;
		}
		Interface.CallHook("OnPlayerSleep", this);
		if (IsRestrained)
		{
			inventory.SetLockedByRestraint(flag: false);
		}
		bool flag = InSafeZone();
		float num = 1f;
		if (!flag && Rust.Application.isLoadingSave)
		{
			flag = TriggerSafeZone.IsBoundsInsideSafeZone(WorldSpaceBounds());
			if (flag)
			{
				num = 4f;
			}
		}
		if (flag && !IsInvoking(ScheduledDeath))
		{
			Invoke(ScheduledDeath, NPCAutoTurret.sleeperhostiledelay * num);
		}
		BaseMountable baseMountable = GetMounted();
		if (baseMountable != null && !AllowSleeperMounting(baseMountable))
		{
			EnsureDismounted();
		}
		SetPlayerFlag(PlayerFlags.Sleeping, b: true);
		sleepStartTime = UnityEngine.Time.time;
		sleepingPlayerList.TryAdd(this);
		sleepingPlayerLookup[userID] = this;
		bots.Remove(this);
		botColliderWorkQueue.Remove(this);
		CancelInvoke(InventoryUpdate);
		CancelInvoke(TeamUpdate);
		CancelInvoke(UpdateClanLastSeen);
		inventory.loot.Clear();
		inventory.containerMain.OnChanged();
		inventory.containerBelt.OnChanged();
		inventory.containerWear.OnChanged();
		EnablePlayerCollider();
		if (!IsLoadingAfterTransfer())
		{
			RemovePlayerRigidbody();
			TurnOffAllLights();
		}
		SetServerFall(wantsOn: true);
		RunOfflineMetabolism(state: true);
		EndActiveConversation();
	}

	private void TurnOffAllLights()
	{
		LightToggle(mask: false);
		HeldEntity heldEntity = GetHeldEntity();
		if (heldEntity != null)
		{
			ToggleableLightWeapon component = heldEntity.GetComponent<ToggleableLightWeapon>();
			if (component != null)
			{
				component.SetIsOn(isOn: false);
			}
		}
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (IsSleeping() || IsIncapacitated())
		{
			Invoke(DelayedServerFall, 0.05f);
		}
	}

	private void DelayedServerFall()
	{
		SetServerFall(wantsOn: true);
	}

	public void SetServerFall(bool wantsOn)
	{
		if (wantsOn && ConVar.Server.playerserverfall)
		{
			if (!IsInvoking(ServerFall))
			{
				SetPlayerFlag(PlayerFlags.ServerFall, b: true);
				lastFallTime = UnityEngine.Time.time - fallTickRate;
				InvokeRandomized(ServerFall, 0f, fallTickRate, fallTickRate * 0.1f);
				fallVelocity = estimatedVelocity.y;
			}
		}
		else
		{
			CancelInvoke(ServerFall);
			SetPlayerFlag(PlayerFlags.ServerFall, b: false);
		}
	}

	public void ServerFall()
	{
		if (IsDead() || HasParent() || (!IsIncapacitated() && !IsSleeping()))
		{
			SetServerFall(wantsOn: false);
			return;
		}
		float num = UnityEngine.Time.time - lastFallTime;
		lastFallTime = UnityEngine.Time.time;
		float radius = GetRadius();
		float num2 = GetHeight(ducked: true) * 0.5f;
		float num3 = 2.5f;
		float num4 = 0.5f;
		fallVelocity += UnityEngine.Physics.gravity.y * num3 * num4 * num;
		float num5 = Mathf.Abs(fallVelocity * num);
		Vector3 vector = base.transform.position + Vector3.up * (radius + num2);
		Vector3 position = base.transform.position;
		Vector3 position2 = base.transform.position;
		int layerMask = 1537286401;
		layerMask = GamePhysics.HandleIgnoreCollision(vector, layerMask);
		layerMask = GamePhysics.HandleIgnoreCollision(vector + Vector3.down * (num5 + num2), layerMask);
		if (UnityEngine.Physics.SphereCast(vector, radius, Vector3.down, out var hitInfo, num5 + num2, layerMask, QueryTriggerInteraction.Ignore))
		{
			SetServerFall(wantsOn: false);
			if (hitInfo.distance > num2)
			{
				position2 += Vector3.down * (hitInfo.distance - num2);
			}
			ApplyFallDamageFromVelocity(fallVelocity);
			UpdateEstimatedVelocity(position2, position2, num);
			fallVelocity = 0f;
		}
		else if (UnityEngine.Physics.Raycast(vector, Vector3.down, out hitInfo, num5 + radius + num2, layerMask, QueryTriggerInteraction.Ignore))
		{
			SetServerFall(wantsOn: false);
			if (hitInfo.distance > num2 - radius)
			{
				position2 += Vector3.down * (hitInfo.distance - num2 - radius);
			}
			ApplyFallDamageFromVelocity(fallVelocity);
			UpdateEstimatedVelocity(position2, position2, num);
			fallVelocity = 0f;
		}
		else
		{
			position2 += Vector3.down * num5;
			UpdateEstimatedVelocity(position, position2, num);
			if (WaterLevel.Test(position2, waves: true, volumes: true, this) || AntiHack.TestInsideTerrain(position2))
			{
				SetServerFall(wantsOn: false);
			}
		}
		MovePosition(position2, forceUpdateTriggers: false);
	}

	public void RunOfflineMetabolism(bool state)
	{
		if (state)
		{
			InvokeRandomized(OfflineMetabolism, ConVar.Server.metabolismtick, ConVar.Server.metabolismtick, ConVar.Server.metabolismtick / 10f);
		}
		else
		{
			CancelInvoke(OfflineMetabolism);
		}
	}

	private void OfflineMetabolism()
	{
		if (!base.IsDestroyed)
		{
			inventory.containerWear.OnCycle(ConVar.Server.metabolismtick);
			metabolism.ServerUpdate(this, ConVar.Server.metabolismtick);
		}
	}

	public void DelayedRigidbodyDisable()
	{
		RemovePlayerRigidbody();
	}

	public virtual void EndSleeping()
	{
		if (IsSleeping() && Interface.CallHook("OnPlayerSleepEnd", this) == null)
		{
			if (IsRestrained)
			{
				inventory.SetLockedByRestraint(flag: true);
			}
			SetPlayerFlag(PlayerFlags.Sleeping, b: false);
			sleepStartTime = -1f;
			sleepingPlayerList.Remove(this);
			sleepingPlayerLookup.Remove(userID);
			if ((ulong)userID < 10000000 && !bots.Contains(this))
			{
				bots.Add(this);
				botColliderWorkQueue.Add(this);
			}
			CancelInvoke(ScheduledDeath);
			InvokeRepeating(InventoryUpdate, 1f, 0.1f * UnityEngine.Random.Range(0.99f, 1.01f));
			if (RelationshipManager.TeamsEnabled())
			{
				InvokeRandomized(TeamUpdate, 1f, 4f, 1f);
			}
			InvokeRandomized(UpdateClanLastSeen, 300f, 300f, 60f);
			EnablePlayerCollider();
			RefreshColliderSize(forced: true);
			AddPlayerRigidbody();
			SetServerFall(wantsOn: false);
			RunOfflineMetabolism(state: false);
			if (HasParent())
			{
				SetParent(null, worldPositionStays: true);
				RemoveFromTriggers();
				ForceUpdateTriggers();
			}
			inventory.containerMain.OnChanged();
			inventory.containerBelt.OnChanged();
			inventory.containerWear.OnChanged();
			Interface.CallHook("OnPlayerSleepEnded", this);
			EACServer.LogPlayerSpawn(this);
			if (TotalPingCount > 0)
			{
				SendPingsToClient();
			}
			if (TutorialIsland.ShouldPlayerBeAskedToStartTutorial(this))
			{
				ClientRPC(RpcTarget.Player("PromptToStartTutorial", this));
			}
			if (AntiHack.TestNoClipping(this, base.transform.position, base.transform.position, NoClipRadius(ConVar.AntiHack.noclip_margin), ConVar.AntiHack.noclip_backtracking, out var _))
			{
				ForceCastNoClip();
			}
			if (State.toastOnReconnect != null && State.toastOnReconnect.Count > 0)
			{
				Invoke(ProcessReconnectToast, 2f);
			}
		}
	}

	private void ProcessReconnectToast()
	{
		if (State.toastOnReconnect != null && State.toastOnReconnect.Count != 0)
		{
			ReconnectToast reconnectToast = State.toastOnReconnect[0];
			State.toastOnReconnect.RemoveAt(0);
			ShowToast((GameTip.Styles)reconnectToast.type, new Translate.Phrase(reconnectToast.phrase), false);
			if (State.toastOnReconnect.Count > 0)
			{
				Invoke(ProcessReconnectToast, 10f);
			}
		}
	}

	public virtual void EndLooting()
	{
		if ((bool)inventory.loot)
		{
			inventory.loot.Clear();
		}
	}

	public virtual void OnDisconnected()
	{
		startTutorialCooldown = 0f;
		stats.Save(forceSteamSave: true);
		EndLooting();
		ClearDesigningAIEntity();
		Server_CancelGesture();
		if (IsAlive() || IsSleeping())
		{
			UpdateActiveItem(default(ItemId));
			StartSleeping();
		}
		else
		{
			Invoke(base.KillMessage, 0f);
		}
		if (isInvisible)
		{
			invisPlayers.Remove(this);
		}
		activePlayerList.Remove(this);
		activePlayerLookup.Remove(userID);
		if (ActivePlayerInd != -1)
		{
			RemoveFromPlayerCache(this, ref PlayerStates);
		}
		SetPlayerFlag(PlayerFlags.Connected, b: false);
		StopServerDemoRecording();
		playersRecordingClientDemos.Remove(this);
		if (net != null)
		{
			if (ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion())
			{
				OcclusionOnDisconnect();
			}
			net.OnDisconnected();
		}
		RefreshColliderSize(forced: true);
		if ((bool)BaseGameMode.GetActiveGameMode(serverside: true))
		{
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerDisconnected(this);
		}
		BaseMission.PlayerDisconnected(this);
		ClanManager serverInstance = ClanManager.ServerInstance;
		if (clanId != 0L && serverInstance != null)
		{
			serverInstance.ClanMemberConnectionsChanged(clanId);
		}
		hasSentFogOfWar = false;
		UpdateClanLastSeen();
		DropSpectators();
		EndActiveConversation();
	}

	private void InventoryUpdate()
	{
		if (IsConnected && !IsDead())
		{
			inventory.ServerUpdate(0.1f);
		}
	}

	public void ApplyFallDamageFromVelocity(float velocity)
	{
		if (IsGod())
		{
			return;
		}
		float num = Mathf.InverseLerp(-15f, -100f, velocity);
		if (num != 0f && Interface.CallHook("OnPlayerLand", this, num) == null)
		{
			float num2 = ((modifiers != null) ? Mathf.Clamp01(1f - modifiers.GetValue(Modifier.ModifierType.Clotting)) : 1f);
			metabolism.bleeding.Add(num * 0.5f * num2);
			float num3 = num * 500f;
			Facepunch.Rust.Analytics.Azure.OnFallDamage(this, velocity, num3);
			Hurt(num3, DamageType.Fall);
			if (num3 > 20f && fallDamageEffect.isValid && !isInvisible)
			{
				Effect.server.Run(fallDamageEffect.resourcePath, base.transform.position, Vector3.zero);
			}
			Interface.CallHook("OnPlayerLanded", this, num);
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void OnPlayerLanded(RPCMessage msg)
	{
		float num = msg.read.Float();
		if (!float.IsNaN(num) && !float.IsInfinity(num))
		{
			ApplyFallDamageFromVelocity(num);
			fallVelocity = 0f;
		}
	}

	public void SendSubscribedGroupsSnapshot()
	{
		using (TimeWarning.New("SendSubscribedGroupsSnapshot"))
		{
			foreach (Group item in net.subscriber.subscribed)
			{
				if (item.ID != 0)
				{
					EnterVisibility(item);
				}
			}
		}
	}

	public override void OnNetworkGroupLeave(Group group)
	{
		base.OnNetworkGroupLeave(group);
		LeaveVisibility(group);
	}

	private void LeaveVisibility(Group group)
	{
		ServerMgr.OnLeaveVisibility(net.connection, group);
	}

	public override void OnNetworkGroupEnter(Group group)
	{
		base.OnNetworkGroupEnter(group);
		EnterVisibility(group);
	}

	private void EnterVisibility(Group group)
	{
		ServerMgr.OnEnterVisibility(net.connection, group);
		SendSnapshots(group.networkables);
	}

	public void CheckDeathCondition(HitInfo info = null)
	{
		Assert.IsTrue(base.isServer, "CheckDeathCondition called on client!");
		if (!IsSpectating() && !IsDead() && metabolism.ShouldDie())
		{
			Die(info);
		}
	}

	public virtual BaseCorpse CreateCorpse(PlayerFlags flagsOnDeath, Vector3 posOnDeath, Quaternion rotOnDeath, List<TriggerBase> triggersOnDeath, bool forceServerSide = false)
	{
		if (Interface.CallHook("OnPlayerCorpseSpawn", this) != null)
		{
			return null;
		}
		using (TimeWarning.New("Create corpse"))
		{
			string strCorpsePrefab = ((!(ConVar.Physics.serversideragdolls || forceServerSide)) ? "assets/prefabs/player/player_corpse.prefab" : "assets/prefabs/player/player_corpse_new.prefab");
			bool flag = false;
			if (ConVar.Global.cinematicGingerbreadCorpses)
			{
				foreach (Item item in inventory.containerWear.itemList)
				{
					if (item != null && item.info.TryGetComponent<ItemCorpseOverride>(out var component))
					{
						strCorpsePrefab = ((GetFloatBasedOnUserID(userID, 4332uL) > 0.5f) ? component.FemaleCorpse.resourcePath : component.MaleCorpse.resourcePath);
						flag = component.BlockWearableCopy;
						break;
					}
				}
			}
			PlayerCorpse playerCorpse = DropCorpse(strCorpsePrefab, posOnDeath, rotOnDeath, flagsOnDeath, modelState) as PlayerCorpse;
			if ((bool)playerCorpse)
			{
				using (FlagsUpdateScope flagsUpdateScope = playerCorpse.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
				{
					flagsUpdateScope.Set(Flags.Reserved5, HasPlayerFlag(PlayerFlags.DisplaySash));
				}
				if (!flag)
				{
					playerCorpse.TakeFrom(this, inventory.containerMain, inventory.containerWear, inventory.containerBelt);
				}
				playerCorpse.playerName = displayName;
				playerCorpse.streamerName = RandomUsernames.Get(userID);
				playerCorpse.playerSteamID = userID;
				playerCorpse.underwearSkin = GetUnderwearSkin(UnityEngine.Time.time);
				if (!CollectionEx.IsNullOrEmpty(triggersOnDeath))
				{
					foreach (TriggerBase item2 in triggersOnDeath)
					{
						if (item2 is TriggerParent triggerParent)
						{
							triggerParent.ForceParentEarly(playerCorpse);
						}
					}
				}
				playerCorpse.Spawn();
				playerCorpse.TakeChildren(this);
				ResourceDispenser component2 = playerCorpse.GetComponent<ResourceDispenser>();
				int num = 2;
				if (lifeStory != null)
				{
					num += Mathf.Clamp(Mathf.FloorToInt(lifeStory.secondsAlive / 180f), 0, 20);
				}
				component2.containedItems.Add(new ItemAmount(ItemManager.FindItemDefinition("fat.animal"), num));
				Interface.CallHook("OnPlayerCorpseSpawned", this, playerCorpse);
				return playerCorpse;
			}
		}
		return null;
		static float GetFloatBasedOnUserID(ulong steamid, ulong seed)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)(seed + steamid));
			float result = UnityEngine.Random.Range(0f, 1f);
			UnityEngine.Random.state = state;
			return result;
		}
	}

	public override void OnDied(HitInfo info)
	{
		PlayerFlags flagsOnDeath = playerFlags;
		Vector3 position = base.transform.position;
		List<TriggerBase> obj = Facepunch.Pool.Get<List<TriggerBase>>();
		if (triggers != null)
		{
			foreach (TriggerBase trigger in triggers)
			{
				if (trigger != null)
				{
					obj.Add(trigger);
				}
			}
		}
		BaseMountable baseMountable = GetMounted();
		Vector3 vector = Vector3.zero;
		Quaternion rotOnDeath;
		if (baseMountable.IsValid())
		{
			rotOnDeath = baseMountable.mountAnchor.rotation;
			vector = baseMountable.GetMountRagdollVelocity(this);
		}
		else
		{
			rotOnDeath = Quaternion.Euler(base.transform.eulerAngles.x, eyes.bodyRotation.eulerAngles.y, base.transform.eulerAngles.z);
		}
		RemoveReceiveTickListenersOnDeath();
		EnsureDismounted();
		EndSleeping();
		EndLooting();
		stats.Add("deaths", 1, Stats.All);
		if (info != null && info.InitiatorPlayer != null && !info.InitiatorPlayer.IsNpc && !IsNpc)
		{
			RelationshipManager.ServerInstance.SetSeen(info.InitiatorPlayer, this);
			RelationshipManager.ServerInstance.SetSeen(this, info.InitiatorPlayer);
			RelationshipManager.ServerInstance.SetRelationship(this, info.InitiatorPlayer, RelationshipManager.RelationshipType.Enemy);
			HandleClanPlayerKilled(info.InitiatorPlayer);
		}
		if ((bool)BaseGameMode.GetActiveGameMode(serverside: true))
		{
			BasePlayer instigator = info?.InitiatorPlayer;
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerDeath(instigator, this, info);
		}
		inventory.DropBackpackOnDeath(wounded: false);
		DisablePlayerCollider();
		RemovePlayerRigidbody();
		List<BasePlayer> obj2 = Facepunch.Pool.Get<List<BasePlayer>>();
		if (IsIncapacitated())
		{
			foreach (BasePlayer activePlayer in activePlayerList)
			{
				if (activePlayer != null && activePlayer.inventory != null && activePlayer.inventory.loot != null && activePlayer.inventory.loot.entitySource == this)
				{
					obj2.Add(activePlayer);
				}
			}
		}
		bool flag = IsWounded();
		StopWounded();
		if (inventory.crafting != null)
		{
			inventory.crafting.CancelAll();
		}
		EACServer.LogPlayerDespawn(this);
		bool flag2 = eyes.HeadRay().direction.y > 0.8f;
		bool flag3 = false;
		if (flag2)
		{
			Vector3 direction = -eyes.MovementForward();
			if (GamePhysics.Trace(new Ray(eyes.position, direction), 0f, out var _, 1f, 2097152))
			{
				flag3 = true;
			}
		}
		if (!wantsSpectate)
		{
			BaseCorpse baseCorpse = CreateCorpse(flagsOnDeath, position, rotOnDeath, obj, flag2 && flag3);
			if (baseCorpse != null)
			{
				if (baseCorpse.CorpseIsRagdoll && baseMountable != null)
				{
					BaseVehicle baseVehicle = baseMountable.VehicleParent();
					if (baseVehicle != null && baseVehicle.mountedPlayerRagdolls == BaseVehicle.RagdollMode.FallThrough)
					{
						baseCorpse.gameObject.SetIgnoreCollisions(baseVehicle.gameObject, ignore: true);
					}
				}
				if (info != null)
				{
					Rigidbody component = baseCorpse.GetComponent<Rigidbody>();
					if (component != null)
					{
						float num = (baseCorpse.CorpseIsRagdoll ? 5f : 1f);
						Vector3 vector2 = (info.attackNormal + Vector3.up * 0.5f).normalized * num;
						component.AddForce(vector2 + vector, ForceMode.VelocityChange);
					}
				}
				if (baseCorpse is PlayerCorpse { containers: not null } playerCorpse)
				{
					foreach (BasePlayer item in obj2)
					{
						if (item == null)
						{
							continue;
						}
						item.inventory.loot.StartLootingEntity(playerCorpse);
						ItemContainer[] containers = playerCorpse.containers;
						foreach (ItemContainer itemContainer in containers)
						{
							if (itemContainer != null)
							{
								item.inventory.loot.AddContainer(itemContainer);
							}
						}
						item.inventory.loot.SendImmediate();
					}
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		inventory.Strip();
		DeathBlow deathBlow;
		if (flag && lastDamage == DamageType.Suicide && cachedNonSuicideHit.IsValid)
		{
			deathBlow = cachedNonSuicideHit;
			DeathBlow.Reset(ref cachedNonSuicideHit);
			lastDamage = info.damageTypes.GetMajorityDamageType();
		}
		else
		{
			DeathBlow.From(info, out deathBlow);
		}
		if (lastDamage == DamageType.Fall)
		{
			stats.Add("death_fall", 1);
		}
		string text = "";
		string text2 = "";
		if (info != null)
		{
			if ((bool)info.Initiator)
			{
				if (info.Initiator == this)
				{
					text = ToString() + " was killed by " + lastDamage.ToString() + " at " + base.transform.position;
					text2 = "You died: killed by " + lastDamage;
					if (lastDamage == DamageType.Suicide)
					{
						stats.Add("death_suicide", 1, Stats.All);
					}
					else
					{
						stats.Add("death_selfinflicted", 1);
					}
				}
				else if (info.Initiator is BasePlayer)
				{
					BasePlayer basePlayer = info.Initiator.ToPlayer();
					text = ToString() + " was killed by " + basePlayer.ToString() + " at " + base.transform.position;
					text2 = "You died: killed by " + basePlayer.displayName + " (" + basePlayer.userID.Get() + ")";
					basePlayer.stats.Add("kill_player", 1, Stats.All);
					basePlayer.LifeStoryKill(this);
					OnKilledByPlayer(basePlayer);
					if (lastDamage == DamageType.Fun_Water)
					{
						basePlayer.GiveAchievement("SUMMER_LIQUIDATOR");
						LiquidWeapon liquidWeapon = basePlayer.GetHeldEntity() as LiquidWeapon;
						if (liquidWeapon != null && liquidWeapon.RequiresPumping && liquidWeapon.PressureFraction <= liquidWeapon.MinimumPressureFraction)
						{
							basePlayer.GiveAchievement("SUMMER_NO_PRESSURE");
						}
					}
					else if (Rust.GameInfo.HasAchievements && lastDamage == DamageType.Explosion && info.WeaponPrefab != null && info.WeaponPrefab.ShortPrefabName.Contains("mlrs") && basePlayer != null)
					{
						basePlayer.stats.Add("mlrs_kills", 1, Stats.All);
						basePlayer.stats.Save(forceSteamSave: true);
					}
					else if (info.WeaponPrefab != null && info.WeaponPrefab.ShortPrefabName.Contains("50cal") && basePlayer != null && basePlayer.IsNonNpcPlayer() && basePlayer.GetMountedVehicle() is PTBoat)
					{
						basePlayer.GiveAchievement("STOLEN_PTBOAT_KILL");
					}
					Facepunch.Rust.Analytics.Azure.OnPlayerDeath(this, basePlayer);
				}
				else
				{
					text = ToString() + " was killed by " + info.Initiator.ShortPrefabName + " (" + info.Initiator.Categorize() + ") at " + base.transform.position;
					text2 = "You died: killed by " + info.Initiator.Categorize();
					stats.Add("death_" + info.Initiator.Categorize(), 1);
				}
			}
			else if (lastDamage == DamageType.Fall)
			{
				text = ToString() + " was killed by fall at " + base.transform.position;
				text2 = "You died: killed by fall";
			}
			else
			{
				text = ToString() + " was killed by " + info.damageTypes.GetMajorityDamageType().ToString() + " at " + base.transform.position;
				text2 = "You died: " + info.damageTypes.GetMajorityDamageType();
			}
		}
		else
		{
			text = ToString() + " died (" + lastDamage.ToString() + ")";
			text2 = "You died: " + lastDamage;
		}
		using (TimeWarning.New("LogMessage"))
		{
			DebugEx.Log(text);
			ConsoleMessage(text2);
		}
		if (net.connection == null && info?.Initiator != null && info.Initiator != this)
		{
			CompanionServer.Util.SendDeathNotification(this, info.Initiator);
		}
		EndActiveConversation();
		SendNetworkUpdateImmediate();
		LifeStoryLogDeath(in deathBlow, lastDamage);
		Server_LogDeathMarker(base.transform.position);
		LifeStoryEnd();
		if (net.connection == null)
		{
			Invoke(base.KillMessage, 0f);
		}
		else
		{
			SendRespawnOptions();
			SendDeathInformation();
			stats.Save();
		}
		PlayerInjureState = GetInjureState();
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public void RespawnAt(Vector3 position, Quaternion rotation, BaseEntity spawnPointEntity = null)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode && !activeGameMode.CanPlayerRespawn(this))
		{
			return;
		}
		SetPlayerFlag(PlayerFlags.Wounded, b: false);
		SetPlayerFlag(PlayerFlags.Incapacitated, b: false);
		SetPlayerFlag(PlayerFlags.ReceivingSnapshot, b: true);
		SetPlayerFlag(PlayerFlags.DisplaySash, b: false);
		respawnId = Guid.NewGuid().ToString("N");
		ServerPerformance.spawns++;
		Vector3 position2 = base.transform.position;
		SetParent(null, worldPositionStays: true);
		base.transform.SetPositionAndRotation(position, rotation);
		if (ActivePlayerInd != -1)
		{
			PlayerStates.TickCache.Reset(this, position);
		}
		tickHistory.Reset(position);
		eyeHistory.Clear();
		ForceUpdateTriggers();
		estimatedVelocity = Vector3.zero;
		estimatedSpeed = 0f;
		estimatedSpeed2D = 0f;
		lastTickTime = 0f;
		lastStallTime = 0f;
		StopWounded();
		ResetWoundingVars();
		StopSpectating();
		UpdateNetworkGroup();
		EnablePlayerCollider();
		RemovePlayerRigidbody();
		StartSleeping();
		LifeStoryStart();
		metabolism.Reset();
		metabolism.MarkNeedsFullSnapshot();
		if (modifiers != null)
		{
			if (Player.keepteaondeath)
			{
				modifiers.RemoveAllExceptFromSource(Modifier.ModifierSource.Tea);
			}
			else
			{
				modifiers.RemoveAll();
			}
		}
		InitializeHealth(StartHealth(), StartMaxHealth());
		bool flag = false;
		if (ConVar.Server.respawnWithLoadout)
		{
			string infoString = GetInfoString("client.respawnloadout", string.Empty);
			if (!string.IsNullOrEmpty(infoString) && Inventory.LoadLoadout(infoString, out var so))
			{
				so.LoadItemsOnTo(this);
				flag = true;
			}
		}
		if (!flag)
		{
			inventory.GiveDefaultItems();
		}
		SendNetworkUpdateImmediate();
		ClientRPC(RpcTarget.Player("StartLoading", this));
		if (DeepSea.enabled && PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			bool num = DeepSeaManager.IsInsideDeepSea(position2);
			bool flag2 = DeepSeaManager.IsInsideDeepSea(position);
			if (num != flag2)
			{
				PointEntity<DeepSeaManager>.ServerInstance.ClientRPC(RpcTarget.Player("CLIENT_PlayerEnterOrLeaveDeepSea", this), flag2);
			}
		}
		Facepunch.Rust.Analytics.Azure.OnPlayerRespawned(this, spawnPointEntity);
		if ((bool)activeGameMode)
		{
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerRespawn(this);
		}
		if (IsConnected)
		{
			EACServer.OnStartLoading(net.connection);
		}
		Interface.CallHook("OnPlayerRespawned", this);
		ProcessMissionEvent(BaseMission.MissionEventType.RESPAWN, 0, 0f);
		PlayerInjureState = GetInjureState();
	}

	public void Respawn()
	{
		SpawnPoint spawnPoint = ServerMgr.FindSpawnPoint(this, 0uL);
		if (ConVar.Server.respawnAtDeathPosition && ServerCurrentDeathNote != null)
		{
			spawnPoint.pos = ServerCurrentDeathNote.worldPosition;
		}
		object obj = Interface.CallHook("OnPlayerRespawn", this, spawnPoint);
		if (obj is SpawnPoint)
		{
			spawnPoint = (SpawnPoint)obj;
		}
		RespawnAt(spawnPoint.pos, spawnPoint.rot);
	}

	public bool IsImmortalTo(HitInfo info)
	{
		if (IsGod())
		{
			return true;
		}
		if (WoundingCausingImmortality(info))
		{
			return true;
		}
		BaseVehicle mountedVehicle = GetMountedVehicle();
		if (mountedVehicle != null && mountedVehicle.ignoreDamageFromOutside)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (initiatorPlayer != null && initiatorPlayer.GetMountedVehicle() != mountedVehicle)
			{
				return true;
			}
		}
		if (IsInTutorial)
		{
			_ = info.InitiatorPlayer != this;
			return false;
		}
		return false;
	}

	public float TimeAlive()
	{
		return lifeStory.secondsAlive;
	}

	public override void Hurt(HitInfo info)
	{
		if (IsDead() || IsTransferProtected() || (IsImmortalTo(info) && info.damageTypes.Total() >= 0f) || Interface.CallHook("IOnBasePlayerHurt", this, info) != null)
		{
			return;
		}
		bool wasWounded = IsWounded();
		if (ConVar.Server.pve && !IsNpc && (bool)info.Initiator && info.Initiator is BasePlayer && info.Initiator != this)
		{
			(info.Initiator as BasePlayer).Hurt(info.damageTypes.Total(), DamageType.Generic);
			return;
		}
		if (info.damageTypes.Has(DamageType.Fun_Water))
		{
			bool flag = true;
			Item activeItem = GetActiveItem();
			if (activeItem != null && (activeItem.info.shortname == "gun.water" || activeItem.info.shortname == "pistol.water"))
			{
				float value = metabolism.wetness.value;
				metabolism.wetness.Add(ConVar.Server.funWaterWetnessGain);
				bool flag2 = metabolism.wetness.value >= ConVar.Server.funWaterDamageThreshold;
				flag = !flag2;
				if (info.InitiatorPlayer != null)
				{
					if (flag2 && value < ConVar.Server.funWaterDamageThreshold)
					{
						info.InitiatorPlayer.GiveAchievement("SUMMER_SOAKED");
					}
					if (metabolism.radiation_level.Fraction() > 0.2f && !string.IsNullOrEmpty("SUMMER_RADICAL"))
					{
						info.InitiatorPlayer.GiveAchievement("SUMMER_RADICAL");
					}
				}
			}
			if (flag)
			{
				info.damageTypes.Scale(DamageType.Fun_Water, 0f);
			}
		}
		if (info.damageTypes.Has(DamageType.BeeSting))
		{
			float num = Mathf.Abs(timeSinceLastStung - UnityEngine.Time.time);
			float num2 = 1f;
			if (num < 2f)
			{
				num2 = Mathf.Lerp(0.2f, 0.05f, Mathf.Exp((0f - num) * 1.5f));
			}
			else
			{
				num2 = 1f;
				timeSinceLastStung = UnityEngine.Time.time;
			}
			info.damageTypes.ScaleAll(num2);
			if (baseProtection.Get(DamageType.BeeSting) > 0f)
			{
				info.damageTypes.ScaleAll(0f);
			}
		}
		if (info.damageTypes.Get(DamageType.Drowned) > 5f && drownEffect.isValid)
		{
			Effect.server.Run(drownEffect.resourcePath, this, StringPool.Get("head"), Vector3.zero, Vector3.zero);
		}
		if (modifiers != null)
		{
			if (info.damageTypes.Has(DamageType.Radiation))
			{
				info.damageTypes.Scale(DamageType.Radiation, 1f - Mathf.Clamp01(modifiers.GetValue(Modifier.ModifierType.Radiation_Resistance)));
			}
			if (info.damageTypes.Has(DamageType.RadiationExposure))
			{
				info.damageTypes.Scale(DamageType.RadiationExposure, 1f - Mathf.Clamp01(modifiers.GetValue(Modifier.ModifierType.Radiation_Exposure_Resistance)));
			}
		}
		metabolism.pending_health.Subtract(info.damageTypes.Total() * 10f);
		BasePlayer initiatorPlayer = info.InitiatorPlayer;
		if ((bool)initiatorPlayer && initiatorPlayer != this)
		{
			if (initiatorPlayer.InSafeZone() || InSafeZone())
			{
				initiatorPlayer.MarkHostileFor(300f);
			}
			if (!initiatorPlayer.InSafeCombatZone() && initiatorPlayer.InSafeZone() && !initiatorPlayer.IsNpc)
			{
				info.damageTypes.ScaleAll(0f);
				return;
			}
			if (initiatorPlayer.IsNpc && initiatorPlayer.Family == BaseNpc.AiStatistics.FamilyEnum.Murderer && info.damageTypes.Get(DamageType.Explosion) > 0f)
			{
				info.damageTypes.ScaleAll(Halloween.scarecrow_beancan_vs_player_dmg_modifier);
			}
		}
		if (initiatorPlayer != null && !initiatorPlayer.IsNpc && !IsNpc)
		{
			float num3 = 1f / Mathf.Max(float.MinValue, ConVar.Server.pvp_ttk_global);
			float num4 = 1f / Mathf.Max(float.MinValue, ConVar.Server.pvp_ttk_bullet);
			float num5 = 1f / Mathf.Max(float.MinValue, ConVar.Server.pvp_ttk_melee);
			if (num3 != 1f)
			{
				info.damageTypes.ScaleAll(num3);
			}
			if (num4 != 1f)
			{
				info.damageTypes.Scale(DamageType.Bullet, num4);
			}
			if (num5 != 1f)
			{
				info.damageTypes.Scale(DamageType.Slash, num5);
				info.damageTypes.Scale(DamageType.Blunt, num5);
				info.damageTypes.Scale(DamageType.Stab, num5);
			}
		}
		base.Hurt(info);
		if ((bool)BaseGameMode.GetActiveGameMode(serverside: true))
		{
			BasePlayer instigator = info?.InitiatorPlayer;
			BaseGameMode.GetActiveGameMode(serverside: true).OnPlayerHurt(instigator, this, info);
		}
		if (IsRestrained && info.damageTypes.GetMajorityDamageType().InterruptsRestraintMinigame())
		{
			Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
			if (handcuffs != null)
			{
				handcuffs.InterruptUnlockMiniGame(wasPushedOrDamaged: true);
			}
		}
		EACServer.LogPlayerTakeDamage(this, info, wasWounded);
		PlayerInjureState = GetInjureState();
		metabolism.SendChanges();
		if (info.PointStart != Vector3.zero && (info.damageTypes.Total() >= 0f || IsGod()))
		{
			int arg = (int)info.damageTypes.GetMajorityDamageType();
			if (info.Weapon != null && info.damageTypes.Has(DamageType.Bullet))
			{
				BaseProjectile component = info.Weapon.GetComponent<BaseProjectile>();
				if (component != null && component.IsSilenced())
				{
					arg = 12;
				}
			}
			ClientRPC(RpcTarget.PlayerAndSpectators("DirectionalDamage", this), info.PointStart, arg, Mathf.CeilToInt(info.damageTypes.Total()));
			if (info.damageTypes.Has(DamageType.BeeSting) && UnityEngine.Time.time > timeSinceLastStungRPC + 2f)
			{
				ClientRPC(RpcTarget.Player("OnStungByBees", this));
				timeSinceLastStungRPC = UnityEngine.Time.time;
			}
		}
		DeathBlow.From(info, out cachedNonSuicideHit);
	}

	public override void Heal(float amount)
	{
		if (IsCrawling())
		{
			float num = base.health;
			base.Heal(amount);
			healingWhileCrawling += base.health - num;
		}
		else
		{
			base.Heal(amount);
		}
		ProcessMissionEvent(BaseMission.MissionEventType.HEAL, 0, amount);
	}

	public static BasePlayer FindBot(ulong userId)
	{
		foreach (BasePlayer bot in bots)
		{
			if ((ulong)bot.userID == userId)
			{
				return bot;
			}
		}
		return FindBotClosestMatch(userId.ToString());
	}

	public static BasePlayer FindBotClosestMatch(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (BasePlayer bot in bots)
		{
			if (bot.displayName.Contains(name))
			{
				return bot;
			}
		}
		return null;
	}

	public static BasePlayer FindByID(ulong userID)
	{
		using (TimeWarning.New("BasePlayer.FindByID"))
		{
			activePlayerLookup.TryGetValue(userID, out var value);
			return value;
		}
	}

	public static bool TryFindByID(ulong userID, out BasePlayer basePlayer)
	{
		basePlayer = FindByID(userID);
		return basePlayer != null;
	}

	public static BasePlayer FindSleeping(ulong userID)
	{
		using (TimeWarning.New("BasePlayer.FindSleeping"))
		{
			sleepingPlayerLookup.TryGetValue(userID, out var value);
			return value;
		}
	}

	public static BasePlayer FindAwakeOrSleepingByID(ulong userID)
	{
		if (userID == 0L)
		{
			return null;
		}
		BasePlayer basePlayer = FindByID(userID);
		if (!(basePlayer != null))
		{
			return FindSleeping(userID);
		}
		return basePlayer;
	}

	public static bool TryFindAwakeOrSleepingByID(ulong userID, out BasePlayer basePlayer)
	{
		basePlayer = FindAwakeOrSleepingByID(userID);
		return basePlayer != null;
	}

	private void ResetArgumentArray(object[] targetArray)
	{
		for (int i = 0; i < targetArray.Length; i++)
		{
			targetArray[i] = null;
		}
	}

	public void Command(string strCommand)
	{
		Command(strCommand, noParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0)
	{
		singleParameterCommandArgs[0] = arg0;
		Command(strCommand, singleParameterCommandArgs);
		ResetArgumentArray(singleParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0, object arg1)
	{
		doubleParameterCommandArgs[0] = arg0;
		doubleParameterCommandArgs[1] = arg1;
		Command(strCommand, doubleParameterCommandArgs);
		ResetArgumentArray(doubleParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0, object arg1, object arg2)
	{
		tripleParameterCommandArgs[0] = arg0;
		tripleParameterCommandArgs[1] = arg1;
		tripleParameterCommandArgs[2] = arg2;
		Command(strCommand, tripleParameterCommandArgs);
		ResetArgumentArray(tripleParameterCommandArgs);
	}

	public void Command(string strCommand, object arg0, object arg1, object arg2, object arg3)
	{
		quadParameterCommandArgs[0] = arg0;
		quadParameterCommandArgs[1] = arg1;
		quadParameterCommandArgs[2] = arg2;
		quadParameterCommandArgs[3] = arg3;
		Command(strCommand, quadParameterCommandArgs);
		ResetArgumentArray(quadParameterCommandArgs);
	}

	public void Command(string strCommand, params object[] arguments)
	{
		if (IsBot)
		{
			BotCommand(strCommand, arguments);
		}
		if (net.connection != null)
		{
			ConsoleNetwork.SendClientCommand(net.connection, strCommand, arguments);
		}
	}

	private void BotCommand(string strCommand, params object[] arguments)
	{
		ConsoleSystem.Option server = ConsoleSystem.Option.Server;
		server.Connection = new Network.Connection
		{
			player = this
		};
		SetPlayerFlag(PlayerFlags.IsDeveloper, b: true);
		SetPlayerFlag(PlayerFlags.IsAdmin, b: true);
		ConsoleSystem.Run(server, strCommand, arguments);
	}

	public override void OnInvalidPosition()
	{
		if (!IsDead())
		{
			Die();
		}
	}

	public static BasePlayer FindByNameOrIP(string strNameOrIDOrIP, IEnumerable<BasePlayer> list)
	{
		BasePlayer basePlayer = list.FirstOrDefault((BasePlayer x) => x.displayName.StartsWith(strNameOrIDOrIP, StringComparison.CurrentCultureIgnoreCase));
		if ((bool)basePlayer)
		{
			return basePlayer;
		}
		BasePlayer basePlayer2 = list.FirstOrDefault((BasePlayer x) => x.net != null && x.net.connection != null && x.net.connection.ipaddress == strNameOrIDOrIP);
		if ((bool)basePlayer2)
		{
			return basePlayer2;
		}
		return null;
	}

	public static BasePlayer Find(string strNameOrIDOrIP)
	{
		if (ulong.TryParse(strNameOrIDOrIP, out var result))
		{
			BasePlayer basePlayer = FindByID(result);
			if (basePlayer != null)
			{
				return basePlayer;
			}
		}
		return FindByNameOrIP(strNameOrIDOrIP, activePlayerList);
	}

	public static BasePlayer FindSleeping(string strNameOrIDOrIP)
	{
		if (ulong.TryParse(strNameOrIDOrIP, out var result))
		{
			BasePlayer basePlayer = FindSleeping(result);
			if (basePlayer != null)
			{
				return basePlayer;
			}
		}
		return FindByNameOrIP(strNameOrIDOrIP, sleepingPlayerList);
	}

	public static BasePlayer FindAwakeOrSleeping(string strNameOrIDOrIP)
	{
		if (ulong.TryParse(strNameOrIDOrIP, out var result))
		{
			BasePlayer basePlayer = FindByID(result);
			if (basePlayer != null)
			{
				return basePlayer;
			}
			BasePlayer basePlayer2 = FindSleeping(result);
			if (basePlayer2 != null)
			{
				return basePlayer2;
			}
		}
		return FindByNameOrIP(strNameOrIDOrIP, allPlayerList);
	}

	public void SendConsoleCommand(string command, params object[] obj)
	{
		ConsoleNetwork.SendClientCommand(net.connection, command, obj);
	}

	public void UpdateRadiation(float fAmount)
	{
		metabolism.radiation_level.Increase(fAmount);
	}

	public override float RadiationExposureFraction()
	{
		float num = Mathf.Clamp(baseProtection.amounts[17], -1f, Radiation.MaxExposureProtection);
		return 1f - num;
	}

	public override float RadiationProtection()
	{
		return Mathf.Clamp(baseProtection.amounts[17], -1f, Radiation.MaxExposureProtection) * 100f;
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		if (Interface.CallHook("OnPlayerHealthChange", this, oldvalue, newvalue) != null)
		{
			return;
		}
		base.OnHealthChanged(oldvalue, newvalue);
		if (base.isServer)
		{
			if (oldvalue > newvalue)
			{
				LifeStoryHurt(oldvalue - newvalue);
			}
			else
			{
				LifeStoryHeal(newvalue - oldvalue);
			}
			metabolism.isDirty = true;
		}
	}

	public void SV_ClothingChanged()
	{
		UpdateProtectionFromClothing();
		UpdateMoveSpeedFromClothing();
	}

	public bool IsNoob()
	{
		return !HasPlayerFlag(PlayerFlags.DisplaySash);
	}

	public bool HasHostileItem()
	{
		using (TimeWarning.New("BasePlayer.HasHostileItem"))
		{
			foreach (Item item in inventory.containerBelt.itemList)
			{
				if (IsHostileItem(item))
				{
					return true;
				}
			}
			foreach (Item item2 in inventory.containerMain.itemList)
			{
				if (IsHostileItem(item2))
				{
					return true;
				}
			}
			return false;
		}
	}

	public override void GiveItem(Item item, GiveItemReason reason = GiveItemReason.Generic, GiveItemOptions options = GiveItemOptions.None)
	{
		if (reason == GiveItemReason.ResourceHarvested)
		{
			stats.Add(item.info.HarvestStatKey, item.amount, (Stats)6);
		}
		if (reason == GiveItemReason.ResourceHarvested || reason == GiveItemReason.Crafted)
		{
			ProcessMissionEvent(BaseMission.MissionEventType.HARVEST, item.info.itemid, item.amount);
		}
		int amount = item.amount;
		if (inventory.GiveItem(item, null, options))
		{
			bool infoBool = GetInfoBool("global.streamermode", defaultVal: false);
			string text = item.GetName(infoBool);
			if (!string.IsNullOrEmpty(text))
			{
				Command("note.inv", item.info.itemid, amount, text, (int)reason);
			}
			else
			{
				Command("note.inv", item.info.itemid, amount, string.Empty, (int)reason);
			}
		}
		else
		{
			item.Drop(inventory.containerMain.dropPosition, inventory.containerMain.dropVelocity);
		}
	}

	public override void AttackerInfo(PlayerLifeStory.DeathInfo info)
	{
		info.attackerName = displayName;
		info.attackerSteamID = userID;
	}

	public void InvalidateWorkbenchCache()
	{
		nextCheckTime = 0f;
	}

	public Workbench GetCachedCraftLevelWorkbench()
	{
		return _cachedWorkbench;
	}

	public void SendActiveWorkbenchIfChanged()
	{
		NetworkableId networkableId = ((_cachedWorkbench != null && _cachedWorkbench.net != null) ? _cachedWorkbench.net.ID : default(NetworkableId));
		if (!(networkableId == lastSentActiveWorkbenchId))
		{
			lastSentActiveWorkbenchId = networkableId;
			ClientRPC(RpcTarget.Player("RPC_SetActiveWorkbench", this), networkableId);
		}
	}

	public virtual bool ShouldDropActiveItem()
	{
		object obj = Interface.CallHook("CanDropActiveItem", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	public override void Die(HitInfo info = null)
	{
		using (TimeWarning.New("Player.Die"))
		{
			if (!IsDead())
			{
				Handcuffs restraintItem = Belt.GetRestraintItem();
				if (restraintItem != null)
				{
					restraintItem.HeldWhenOwnerDied(this);
				}
				if (InGesture)
				{
					Server_CancelGesture();
				}
				if (Belt != null && ShouldDropActiveItem())
				{
					Vector3 vector = new Vector3(UnityEngine.Random.Range(-2f, 2f), 0.2f, UnityEngine.Random.Range(-2f, 2f));
					Belt.DropActive(GetDropPosition(), GetInheritedDropVelocity() + vector.normalized * 3f);
				}
				if (!WoundInsteadOfDying(info) && Interface.CallHook("OnPlayerDeath", this, info) == null)
				{
					SleepingBag.OnPlayerDeath(this);
					base.Die(info);
				}
			}
		}
	}

	public void Kick(string reason, bool reserveSlot = true)
	{
		if (IsConnected)
		{
			net.connection.canReserveSlot = reserveSlot;
			Network.Net.sv.Kick(net.connection, reason);
			Interface.CallHook("OnPlayerKicked", this, reason, reserveSlot);
		}
	}

	public override Vector3 GetDropPosition()
	{
		return eyes.position;
	}

	public override Vector3 GetDropVelocity()
	{
		return GetInheritedDropVelocity() + eyes.BodyForward() * 4f + Vector3Ex.Range(-0.5f, 0.5f);
	}

	public override void ApplyInheritedVelocity(Vector3 velocity)
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity != null)
		{
			ClientRPC(RpcTarget.Player("SetInheritedVelocity", this), baseEntity.transform.InverseTransformDirection(velocity), baseEntity.net.ID);
		}
		else
		{
			ClientRPC(RpcTarget.Player("SetInheritedVelocity", this), velocity, default(NetworkableId));
		}
		PauseSpeedHackDetection();
	}

	public virtual void SetInfo(string key, string val)
	{
		if (IsConnected)
		{
			Interface.CallHook("OnPlayerSetInfo", net.connection, key, val);
			net.connection.info.Set(key, val);
		}
	}

	public virtual int GetInfoInt(string key, int defaultVal)
	{
		if (!IsConnected)
		{
			return defaultVal;
		}
		return net.connection.info.GetInt(key, defaultVal);
	}

	public virtual bool GetInfoBool(string key, bool defaultVal)
	{
		if (!IsConnected)
		{
			return defaultVal;
		}
		return net.connection.info.GetBool(key, defaultVal);
	}

	public virtual string GetInfoString(string key, string defaultVal)
	{
		if (!IsConnected)
		{
			return defaultVal;
		}
		return net.connection.info.GetString(key, defaultVal);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(1uL)]
	public void PerformanceReport(RPCMessage msg)
	{
		string text = msg.read.String();
		using PerformanceReport performanceReport = msg.read.Proto<PerformanceReport>();
		if (performanceReport.user_id != UserIDString)
		{
			DebugEx.Log($"Client performance report from {this} has incorrect user_id ({UserIDString})");
			return;
		}
		switch (text)
		{
		case "json":
			DebugEx.Log(ConvertPerfReportToJSON(performanceReport));
			break;
		case "legacy":
		{
			string text2 = (performanceReport.memory_managed_heap + "MB").PadRight(9);
			string text3 = (performanceReport.memory_system + "MB").PadRight(9);
			string text4 = (performanceReport.fps.ToString("0") + "FPS").PadRight(8);
			string text5 = NumberExtensions.FormatSeconds(performanceReport.fps).PadRight(9);
			string text6 = UserIDString.PadRight(20);
			string text7 = performanceReport.streamer_mode.ToString().PadRight(7);
			DebugEx.Log(text2 + text3 + text4 + text5 + text7 + text6 + displayName);
			break;
		}
		case "none":
			break;
		case "rcon":
			RCon.Broadcast(RCon.LogType.ClientPerf, ConvertPerfReportToJSON(performanceReport));
			break;
		default:
			Debug.LogError("Unknown PerformanceReport format '" + text + "'");
			break;
		}
	}

	private string ConvertPerfReportToJSON(PerformanceReport report)
	{
		ClientPerformanceReport clientPerformanceReport = default(ClientPerformanceReport);
		clientPerformanceReport.request_id = report.request_id;
		clientPerformanceReport.user_id = report.user_id;
		clientPerformanceReport.fps_average = report.fps_average;
		clientPerformanceReport.fps = report.fps;
		clientPerformanceReport.frame_id = report.frame_id;
		clientPerformanceReport.frame_time = report.frame_time;
		clientPerformanceReport.frame_time_average = report.frame_time_average;
		clientPerformanceReport.memory_system = report.memory_system;
		clientPerformanceReport.memory_collections = report.memory_collections;
		clientPerformanceReport.memory_managed_heap = report.memory_managed_heap;
		clientPerformanceReport.realtime_since_startup = report.realtime_since_startup;
		clientPerformanceReport.streamer_mode = report.streamer_mode;
		clientPerformanceReport.ping = report.ping;
		clientPerformanceReport.tasks_invokes = report.tasks_invokes;
		clientPerformanceReport.tasks_load_balancer = report.tasks_load_balancer;
		clientPerformanceReport.workshop_skins_queued = report.workshop_skins_queued;
		return JsonConvert.SerializeObject(clientPerformanceReport);
	}

	public override bool ShouldNetworkTo(BasePlayer player)
	{
		object obj = Interface.CallHook("CanNetworkTo", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		bool flag = ShouldNetworkToSkipOcclusion(player);
		if (flag && ServerOcclusion.OcclusionEnabled && SupportsServerOcclusion() && player.SupportsServerOcclusion() && this != player)
		{
			flag = OcclusionGetCachedVisibility(player);
		}
		return flag;
	}

	public bool ShouldNetworkToSkipOcclusion(BasePlayer player)
	{
		if (player == this)
		{
			return true;
		}
		if (IsSpectating())
		{
			return false;
		}
		if (isInvisible)
		{
			return player.IsSpectating();
		}
		if (player.OcclusionShouldSeeAllPlayers())
		{
			return true;
		}
		return base.ShouldNetworkTo(player);
	}

	internal bool GiveAchievement(string name, bool allowTutorial = false)
	{
		if (Rust.GameInfo.HasAchievements && (!IsInTutorial || allowTutorial))
		{
			ClientRPC(RpcTarget.Player("RecieveAchievement", this), name);
			return true;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	public async void OnPlayerReported(RPCMessage msg)
	{
		try
		{
			string text = msg.read.String();
			string text2 = msg.read.StringMultiLine();
			string message = ((text2 != null && text2.Length > 1400) ? text2.Substring(0, 1400) : text2);
			string text3 = msg.read.String();
			string targetId = msg.read.String();
			string text4 = msg.read.String();
			DebugEx.Log($"[PlayerReport] {this} reported {text4}[{targetId}] - \"{text}\"");
			RCon.Broadcast(RCon.LogType.Report, new
			{
				PlayerId = UserIDString,
				PlayerName = displayName,
				TargetId = targetId,
				TargetName = text4,
				Subject = text,
				Message = message,
				Type = text3
			});
			Interface.CallHook("OnPlayerReported", this, text4, targetId, text, text2, text3);
			if (!string.IsNullOrEmpty(ConVar.Server.reportsServerEndpoint))
			{
				ReportType type = ReportType.Abuse;
				if (text3.Equals("cheat"))
				{
					type = ReportType.Cheat;
				}
				if (text3.Equals("break_server_rules"))
				{
					type = ReportType.BreakingServerRules;
				}
				Facepunch.Models.Feedback feedback = default(Facepunch.Models.Feedback);
				feedback.Subject = text;
				feedback.Message = message;
				feedback.TargetReportType = text3;
				feedback.TargetId = targetId;
				feedback.TargetName = text4;
				feedback.Type = type;
				Facepunch.Models.Feedback feedback2 = feedback;
				DebugEx.Log("[OnPlayerReported to endpoint] " + await Facepunch.Feedback.ServerReport(ConVar.Server.reportsServerEndpoint, userID, ConVar.Server.reportsServerEndpointKey, feedback2));
			}
			BasePlayer basePlayer = FindAwakeOrSleeping(targetId);
			if (basePlayer != null)
			{
				basePlayer.State.numberOfTimesReported++;
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[OnPlayerReported] Exception occurred when sending F7 report to endpoint: " + ex.Message);
			Debug.LogException(ex);
		}
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public async void OnFeedbackReport(RPCMessage msg)
	{
		try
		{
			string text = msg.read.String();
			string text2 = msg.read.StringMultiLine();
			string text3 = ((text2 != null && text2.Length > 1400) ? text2.Substring(0, 1400) : text2);
			ReportType reportType = (ReportType)Mathf.Clamp(msg.read.Int32(), 0, 6);
			if (ConVar.Server.printReportsToConsole)
			{
				DebugEx.Log($"[FeedbackReport] {this} reported {reportType} - \"{text}\" \"{text3}\"");
				RCon.Broadcast(RCon.LogType.Report, new
				{
					PlayerId = UserIDString,
					PlayerName = displayName,
					Subject = text,
					Message = text3,
					Type = reportType
				});
			}
			Interface.CallHook("OnFeedbackReported", this, text, text2, reportType);
			if (!string.IsNullOrEmpty(ConVar.Server.reportsServerEndpoint))
			{
				string image = msg.read.StringMultiLine(60000);
				Facepunch.Models.Feedback feedback = default(Facepunch.Models.Feedback);
				feedback.Type = reportType;
				feedback.Message = text3;
				feedback.Subject = text;
				Facepunch.Models.Feedback feedback2 = feedback;
				feedback2.AppInfo.Image = image;
				DebugEx.Log("[OnFeedbackReport to endpoint] " + await Facepunch.Feedback.ServerReport(ConVar.Server.reportsServerEndpoint, userID, ConVar.Server.reportsServerEndpointKey, feedback2));
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[OnFeedbackReport] Exception occurred when sending F7 report to endpoint: " + ex.Message);
			Debug.LogException(ex);
		}
	}

	public void StartServerDemoRecording()
	{
		if (net != null && net.connection != null && !net.connection.IsRecording)
		{
			string text = $"demos/{UserIDString}/{DateTime.Now:yyyy-MM-dd-hhmmss}.dem";
			if (Interface.CallHook("OnDemoRecordingStart", text, this) == null)
			{
				Debug.Log(ToString() + " recording started: " + text);
				net.connection.StartRecording(text, new Demo.Header
				{
					version = Demo.Version,
					level = SceneManager.GetActiveScene().name,
					levelSeed = World.Seed,
					levelSize = World.Size,
					checksum = World.Checksum,
					localclient = userID,
					position = eyes.position,
					rotation = eyes.HeadForward(),
					levelUrl = World.Url,
					recordedTime = DateTime.Now.ToBinary()
				});
				SendCompleteSnapshot();
				InvokeRepeating(actionMonitorServerDemoRecording, 10f, 10f);
				Interface.CallHook("OnDemoRecordingStarted", text, this);
			}
		}
	}

	public void SendGlobalSnapshot()
	{
		using (TimeWarning.New("SendGlobalSnapshot", 10))
		{
			EnterVisibility(BaseNetworkable.GlobalNetworkGroup);
		}
	}

	public void SendCompleteSnapshot()
	{
		SendNetworkUpdateImmediate();
		SendGlobalSnapshot();
		SendSubscribedGroupsSnapshot();
		SendEntityUpdate();
		TreeManager.SendSnapshot(this);
		ServerMgr.SendReplicatedVars(net.connection);
	}

	public void StopServerDemoRecording()
	{
		if (net != null && net.connection != null && net.connection.IsRecording && Interface.CallHook("OnDemoRecordingStop", net.connection.recordFilename, this) == null)
		{
			Debug.Log(ToString() + " recording stopped: " + net.connection.RecordFilename);
			net.connection.StopRecording();
			CancelInvoke(actionMonitorServerDemoRecording);
			Interface.CallHook("OnDemoRecordingStopped", net.connection.recordFilename, this);
		}
	}

	public void MonitorServerDemoRecording()
	{
		if (net != null && net.connection != null && net.connection.IsRecording && (net.connection.RecordTimeElapsed.TotalSeconds >= (double)Demo.splitseconds || (float)net.connection.RecordFilesize >= Demo.splitmegabytes * 1024f * 1024f))
		{
			StopServerDemoRecording();
			StartServerDemoRecording();
		}
	}

	public void InvalidateCachedPeristantPlayer()
	{
		cachedPersistantPlayer = null;
	}

	public bool IsPlayerVisibleToUs(BasePlayer otherPlayer, Vector3 fromOffset, int layerMask)
	{
		if (otherPlayer == null)
		{
			return false;
		}
		Vector3 vector = (isMounted ? eyes.worldMountedPosition : (IsDucked() ? eyes.worldCrouchedPosition : ((!IsCrawling()) ? eyes.worldStandingPosition : eyes.worldCrawlingPosition)));
		vector += fromOffset;
		if (!otherPlayer.IsVisibleSpecificLayers(vector, otherPlayer.CenterPoint(), layerMask) && !otherPlayer.IsVisibleSpecificLayers(vector, otherPlayer.transform.position, layerMask) && !otherPlayer.IsVisibleSpecificLayers(vector, otherPlayer.eyes.position, layerMask))
		{
			return false;
		}
		if (!IsVisibleSpecificLayers(otherPlayer.CenterPoint(), vector, layerMask) && !IsVisibleSpecificLayers(otherPlayer.transform.position, vector, layerMask) && !IsVisibleSpecificLayers(otherPlayer.eyes.position, vector, layerMask))
		{
			return false;
		}
		return true;
	}

	protected virtual void OnKilledByPlayer(BasePlayer p)
	{
	}

	public override void OnKilled()
	{
		CancelInvoke(OfflineMetabolism);
		base.OnKilled();
	}

	public int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		if (container.HasFlag(ItemContainer.Flag.Clothing))
		{
			if (item.IsBackpack())
			{
				return 7;
			}
			if (!item.info.isWearable)
			{
				return -1;
			}
			foreach (Item item2 in container.itemList)
			{
				if (!item2.info.ItemModWearable.CanExistWith(item.info.ItemModWearable) && item2.position == 7 == item.IsBackpack())
				{
					return item2.position;
				}
			}
		}
		return -1;
	}

	public ItemContainerId GetIdealContainer(BasePlayer looter, Item item, ItemMoveModifier modifier)
	{
		bool flag = (modifier & ItemMoveModifier.Alt) != ItemMoveModifier.Alt && looter.inventory.loot.containers.Count > 0;
		ItemContainer parent = item.parent;
		BaseEntity baseEntity = parent?.GetEntityOwner();
		Item activeItem = looter.GetActiveItem();
		Item backpackWithInventory = inventory.GetBackpackWithInventory();
		bool flag2 = backpackWithInventory != null && backpackWithInventory == item.parentItem;
		bool flag3 = false;
		if ((modifier & ItemMoveModifier.BackpackOpen) == ItemMoveModifier.BackpackOpen && looter == this && backpackWithInventory != null)
		{
			if (backpackWithInventory.contents.HasSpaceFor(item))
			{
				if (!flag)
				{
					if (item.parentItem == null || !item.parentItem.IsBackpack() || item.parentItem.parent != inventory.containerWear)
					{
						return backpackWithInventory.contents.uid;
					}
				}
				else if (inventory.loot.FindItem(item.uid) != null && !inventory.containerMain.HasSpaceFor(item))
				{
					return backpackWithInventory.contents.uid;
				}
			}
			else
			{
				flag3 = true;
			}
		}
		if (activeItem != null && !flag3 && !flag && activeItem.contents != null && activeItem.contents != item.parent && activeItem.contents.capacity > 0 && activeItem.contents.CanAcceptItem(looter, item, -1) == ItemContainer.CanAcceptResult.CanAccept)
		{
			return activeItem.contents.uid;
		}
		if (item.info.isWearable && item.info.ItemModWearable.equipOnRightClick && item.parent != inventory.containerWear && !flag && !flag2)
		{
			if (flag3)
			{
				if (baseEntity != this)
				{
					if (!inventory.containerMain.IsFull())
					{
						return inventory.containerMain.uid;
					}
					if (!inventory.containerWear.IsFull())
					{
						return inventory.containerWear.uid;
					}
				}
				return ItemContainerId.Invalid;
			}
			if (backpackWithInventory == null || item.parent != backpackWithInventory.contents)
			{
				return inventory.containerWear.uid;
			}
		}
		if (parent == inventory.containerMain)
		{
			if (flag)
			{
				return default(ItemContainerId);
			}
			return inventory.containerBelt.uid;
		}
		if (parent == inventory.containerWear)
		{
			return inventory.containerMain.uid;
		}
		if (parent == inventory.containerBelt)
		{
			return inventory.containerMain.uid;
		}
		return default(ItemContainerId);
	}

	private BaseVehicle GetVehicleParent()
	{
		BaseVehicle mountedVehicle = GetMountedVehicle();
		if (mountedVehicle != null)
		{
			return mountedVehicle;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity != null && baseEntity is BaseVehicle result)
		{
			return result;
		}
		return null;
	}

	private void TryDisableTransferProtectionOnLoaded()
	{
		if (IsTransferProtected())
		{
			BaseVehicle vehicleParent = GetVehicleParent();
			if (!(vehicleParent != null) || vehicleParent.ShouldDisableTransferProtectionOnLoad(this))
			{
				DisableTransferProtection();
			}
		}
	}

	private void RemoveLoadingPlayerFlag()
	{
		if (IsLoadingAfterTransfer())
		{
			SetPlayerFlag(PlayerFlags.LoadingAfterTransfer, b: false);
			if (IsSleeping())
			{
				SetPlayerFlag(PlayerFlags.Sleeping, b: false);
				StartSleeping();
			}
		}
	}

	public bool InNoRespawnZone()
	{
		bool flag = false;
		Vector3 position = base.transform.position;
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerNoRespawnZone triggerNoRespawnZone = triggers[i] as TriggerNoRespawnZone;
				if (!(triggerNoRespawnZone == null))
				{
					flag = triggerNoRespawnZone.InNoRespawnZone(position, checkRadius: false);
					if (flag)
					{
						break;
					}
				}
			}
		}
		return flag;
	}

	private void SendCargoPatrolPath()
	{
		if (!BaseBoat.generate_paths)
		{
			return;
		}
		if (cachedOceanPaths == null)
		{
			cachedOceanPaths = Facepunch.Pool.Get<OceanPaths>();
			cachedOceanPaths.cargoPatrolPath = TerrainMeta.Path.OceanPatrolFar;
			cachedOceanPaths.harborApproaches = new List<VectorList>();
			for (int i = 0; i < CargoShip.TotalAvailableHarborDockingPaths; i++)
			{
				VectorList vectorList = new VectorList();
				vectorList.vectorPoints = CargoShip.GetCargoApproachPath(i);
				cachedOceanPaths.harborApproaches.Add(vectorList);
			}
		}
		ClientRPC(RpcTarget.Player("ReceiveCargoPatrolPath", this), cachedOceanPaths);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void RPC_ReqDoRestrainedPush(RPCMessage rpc)
	{
		if (IsSleeping() || IsDead() || !IsRestrained)
		{
			return;
		}
		BasePlayer player = rpc.player;
		if (player == null || player == this)
		{
			return;
		}
		Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
		if (handcuffs != null)
		{
			handcuffs.InterruptUnlockMiniGame(wasPushedOrDamaged: true);
			handcuffs.RepairOnPush();
		}
		if (isMounted)
		{
			BaseMountable baseMountable = GetMounted();
			if (baseMountable != null)
			{
				baseMountable.DismountPlayer(this);
				return;
			}
		}
		Vector3 force = player.eyes.BodyForward() * 10f;
		force.y = 0f;
		force += Vector3.up * 3f;
		DoPush(force, isRestrained: true);
		Hurt(Handcuffs.restrainedPushDamage, DamageType.Generic, player, useProtection: false);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_ReqRemoveCuffs(RPCMessage rpc)
	{
		if (IsDead() || !IsRestrained)
		{
			return;
		}
		BasePlayer player = rpc.player;
		if (!(player == null) && !(player == this))
		{
			Handcuffs handcuffs = GetHeldEntity() as Handcuffs;
			if (handcuffs != null)
			{
				handcuffs.UnlockAndReturnToPlayer(player);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	private void RPC_ReqRemoveHood(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!(player == null) && !(player == this))
		{
			RemoveAndReturnPrisonerHood(player);
		}
	}

	private void RemoveAndReturnPrisonerHood(BasePlayer returnToPlayer)
	{
		if (!(returnToPlayer == null) && !IsDead() && IsRestrained)
		{
			Item equippedPrisonerHoodItem = inventory.GetEquippedPrisonerHoodItem();
			if (equippedPrisonerHoodItem != null)
			{
				bool isLocked = inventory.containerWear.IsLocked();
				inventory.containerWear.SetLocked(isLocked: false);
				returnToPlayer.GiveItem(equippedPrisonerHoodItem);
				inventory.containerWear.SetLocked(isLocked);
			}
		}
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.IsVisible(3f)]
	private void RPC_ReqEquipHood(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!(player == null))
		{
			EquipPrisonerHood(player);
		}
	}

	private void EquipPrisonerHood(BasePlayer placingPlayer)
	{
		if (placingPlayer == null || IsDead() || !IsRestrained || inventory == null || inventory.GetEquippedPrisonerHoodItem() != null)
		{
			return;
		}
		Item usableHoodItem = placingPlayer.inventory.GetUsableHoodItem();
		if (usableHoodItem == null)
		{
			return;
		}
		inventory.SetLockedByRestraint(flag: false);
		if (!usableHoodItem.MoveToContainer(inventory.containerBelt))
		{
			Item slot = inventory.containerBelt.GetSlot(0);
			if (slot != null && slot == Belt.GetRestraintItem()?.GetItem())
			{
				slot = inventory.containerBelt.GetSlot(1);
			}
			if (slot != null)
			{
				if (!slot.MoveToContainer(inventory.containerMain))
				{
					slot.DropAndTossUpwards(base.transform.position);
				}
				usableHoodItem.MoveToContainer(inventory.containerBelt);
			}
		}
		inventory.SetLockedByRestraint(flag: true);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	private void RPC_ReqForceMountNearest(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if (!(player == null))
		{
			ForceRestrainedMountNearest(player);
		}
	}

	private void ForceRestrainedMountNearest(BasePlayer forcingPlayer)
	{
		if (forcingPlayer == null || isMounted || !IsRestrained || IsDead() || IsSleeping() || IsWounded())
		{
			return;
		}
		List<BaseMountable> obj = Facepunch.Pool.Get<List<BaseMountable>>();
		Vis.Entities(base.transform.position, 2f, obj);
		obj.Sort((BaseMountable a, BaseMountable b) => (base.transform.position - a.transform.position).sqrMagnitude.CompareTo((base.transform.position - b.transform.position).sqrMagnitude));
		foreach (BaseMountable item in obj)
		{
			if (item.isClient || !item.AllowForceMountWhenRestrained || item.VehicleParent() != null || !item.DirectlyMountable() || item.Distance(eyes.position) > 3f || !GamePhysics.LineOfSight(eyes.center, eyes.position, 1218519041) || (!item.IsVisible(eyes.HeadRay(), 1218519041, 3f) && !item.IsVisible(eyes.position, 3f)))
			{
				continue;
			}
			bool flag = false;
			ModularCar modularCar = item as ModularCar;
			if (modularCar != null && modularCar.CarLock.HasALock)
			{
				flag = !modularCar.CarLock.HasLockPermission(this);
				if (modularCar.CarLock.HasLockPermission(forcingPlayer))
				{
					modularCar.CarLock.TryAddPlayer(userID);
				}
			}
			item.AttemptMount(this);
			if (modularCar != null && modularCar.CarLock.HasALock && flag)
			{
				modularCar.CarLock.TryRemovePlayer(userID);
			}
			if (isMounted)
			{
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_ReqForceSwapSeat(RPCMessage rpc)
	{
		if (!isMounted || !IsRestrained || IsDead() || IsSleeping() || IsWounded() || rpc.player == null)
		{
			return;
		}
		BasePlayer player = rpc.player;
		BaseMountable baseMountable = GetMounted();
		if (baseMountable == null)
		{
			return;
		}
		BaseVehicle baseVehicle = baseMountable.GetComponent<BaseVehicle>();
		if (baseVehicle == null)
		{
			baseVehicle = baseMountable.VehicleParent();
		}
		if (baseVehicle == null)
		{
			return;
		}
		bool flag = false;
		ModularCar modularCar = baseVehicle as ModularCar;
		if (modularCar != null && modularCar.CarLock.HasALock)
		{
			flag = !modularCar.CarLock.HasLockPermission(this);
			if (modularCar.CarLock.HasLockPermission(player))
			{
				modularCar.CarLock.TryAddPlayer(userID);
			}
		}
		baseVehicle.SwapSeats(this, 0, forcingRestrainedPlayer: true);
		if (modularCar != null && modularCar.CarLock.HasALock && flag)
		{
			modularCar.CarLock.TryRemovePlayer(userID);
		}
	}

	public PlayerInventory.CanMoveFromResponse CanMoveFrom(BasePlayer player, Item item)
	{
		if (IsRestrainedOrSurrendering)
		{
			ItemContainer itemContainer = item?.parent;
			if (itemContainer == null)
			{
				return PlayerInventory.CanMoveFromResponse.Success();
			}
			if (itemContainer.IsLocked())
			{
				return PlayerInventory.CanMoveFromResponse.Failure(PlayerInventoryErrors.InventoryLockedError);
			}
			if (itemContainer == inventory.containerBelt && item.IsOn() && item.info.GetComponent<ItemModRestraint>() != null)
			{
				return PlayerInventory.CanMoveFromResponse.Failure(TakingRestraintItemError);
			}
		}
		return PlayerInventory.CanMoveFromResponse.Success();
	}

	public void GetAllInventories(List<ItemContainer> list)
	{
		list.Add(inventory.containerMain);
		list.Add(inventory.containerBelt);
		list.Add(inventory.containerWear);
	}

	public void DoPush(Vector3 force, bool isRestrained = false)
	{
		AddTempSpeedHackBudget(5f, 2f);
		PauseTickDistanceDetection(2f);
		ClientRPC(RpcTarget.Player(isRestrained ? "RPC_DoRestrainedPush" : "RPC_DoPush", this), force);
	}

	public override bool SupportsServerOcclusion()
	{
		if (!IsNpc && !IsBot)
		{
			return !RustRelayFakePlayer.IsFakePlayer(this);
		}
		return false;
	}

	public void OnEnterDeepSea()
	{
		Facepunch.Rust.Analytics.Azure.OnDeepSeaTraverse(this, entering: true, PointEntity<DeepSeaManager>.ServerInstance.TimeToWipe);
	}

	public void OnExitDeepSea()
	{
		Facepunch.Rust.Analytics.Azure.OnDeepSeaTraverse(this, entering: false, PointEntity<DeepSeaManager>.ServerInstance.TimeToWipe);
	}

	public override bool EnterTrigger(TriggerBase trigger)
	{
		if (trigger is TriggerLadder)
		{
			onLadderCount++;
		}
		return base.EnterTrigger(trigger);
	}

	public override void LeaveTrigger(TriggerBase trigger)
	{
		if (trigger is TriggerLadder)
		{
			onLadderCount--;
		}
		base.LeaveTrigger(trigger);
	}

	public void FreeUnoccludedSubscribers()
	{
		if (unoccludedSubscribers != null)
		{
			Facepunch.Pool.FreeUnmanaged(ref unoccludedSubscribers);
		}
	}

	protected override bool OcclusionLeavePlayersGroup(BaseNetworkable other)
	{
		bool result = base.OcclusionLeavePlayersGroup(other);
		if (other is BasePlayer basePlayer)
		{
			lastPlayerVisibility.Remove(basePlayer.net.ID.Value);
		}
		return result;
	}

	private static void ServerUpdateOcclusionParallel(in PlayerServerStates.ReadOnly playerStates, float networkTime)
	{
		ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
		if (objects.Length == 0)
		{
			OcclusionFrameCache.Clear();
			return;
		}
		OcclusionFrameCache.Clear();
		RecalculateOcclusionPositions(objects, playerStates.PlayerPos);
		int num = objects.Length * 8;
		BufferList<OcclusionPlayerPair> obj = Facepunch.Pool.Get<BufferList<OcclusionPlayerPair>>();
		if (obj.Capacity < num)
		{
			obj.Resize(num);
		}
		BufferList<OcclusionPlayerPair> obj2 = Facepunch.Pool.Get<BufferList<OcclusionPlayerPair>>();
		if (ConVar.Server.UsePlayerUpdateJobs >= 4)
		{
			GatherPairsParallel(playerStates.PlayerCache, playerStates.PlayerPos, obj, obj2, networkTime, DeepSea.enabled);
		}
		else
		{
			GatherPairs(objects, obj, obj2, networkTime);
		}
		BufferList<OcclusionPlayerPair> obj3 = Facepunch.Pool.Get<BufferList<OcclusionPlayerPair>>();
		if (obj.Count > 0)
		{
			using (TimeWarning.New("Run Occlusion Checks"))
			{
				NativeArray<bool> results = new NativeArray<bool>(obj.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				OcclusionLineOfSight(obj.ContentReadOnlySpan(), results);
				for (int i = 0; i < obj.Count; i++)
				{
					OcclusionPlayerPair element = obj[i];
					if (results[i])
					{
						if (element.from.IsConnected)
						{
							element.from.unoccludedSubscribers.Add(element.to.net.connection);
						}
						OcclusionFrameCache.Add((element.from.net.ID.Value, element.to.net.ID.Value));
						obj2.Add(element);
					}
					else
					{
						obj3.Add(element);
					}
				}
				results.Dispose();
			}
		}
		if (obj2.Count + obj3.Count > 0)
		{
			OcclusionSendUpdates(obj2.ContentReadOnlySpan(), obj3.ContentReadOnlySpan(), networkTime);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		Facepunch.Pool.FreeUnmanaged(ref obj3);
		static void RecalculateOcclusionPositions(ReadOnlySpan<BasePlayer> players, NativeArray<Vector3>.ReadOnly playerPos)
		{
			using (TimeWarning.New("Recalculate player grid positions"))
			{
				ReadOnlySpan<BasePlayer> readOnlySpan = players;
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					BasePlayer basePlayer = readOnlySpan[j];
					Vector3 position = playerPos[basePlayer.ActivePlayerInd] + PlayerEyes.EyeOffset;
					basePlayer.SubGrid = ServerOcclusion.GetSubGrid(position);
					basePlayer.Chunk = ServerOcclusion.GetGrid(position);
					basePlayer.OcclusionResetUnoccludedSubscribers();
				}
			}
		}
	}

	public static void GatherPairs(ReadOnlySpan<BasePlayer> players, BufferList<OcclusionPlayerPair> pairsToCheck, BufferList<OcclusionPlayerPair> pairsFound, float networkTime)
	{
		using (TimeWarning.New("Gather Occlusion Pairs For Checking"))
		{
			ReadOnlySpan<BasePlayer> readOnlySpan = players;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				BasePlayer basePlayer = readOnlySpan[i];
				basePlayer.unoccludedSubscribers.Add(basePlayer.net.connection);
				if (basePlayer.IsSpectating())
				{
					if (!(basePlayer.net.SubStrategy is SpectatorSubStrategy spectatorSubStrategy))
					{
						continue;
					}
					ServerOcclusion.Group value = null;
					if (spectatorSubStrategy.SpectatedPlayer != null)
					{
						value = spectatorSubStrategy.SpectatedPlayer.OcclusionGroup;
					}
					else if (spectatorSubStrategy.LastGroup != null)
					{
						ServerOcclusion.Occludees.TryGetValue(spectatorSubStrategy.LastGroup, out value);
					}
					if (value == null)
					{
						continue;
					}
					foreach (BaseNetworkable item in value)
					{
						BasePlayer basePlayer2 = item as BasePlayer;
						if (!(basePlayer2 == null) && !(basePlayer == basePlayer2))
						{
							if (basePlayer2.IsConnected)
							{
								basePlayer2.unoccludedSubscribers.Add(basePlayer.net.connection);
							}
							OcclusionFrameCache.Add((basePlayer2.net.ID.Value, basePlayer.net.ID.Value));
						}
					}
					continue;
				}
				ServerOcclusion.Group group = basePlayer.OcclusionGroup;
				if (group == null || group.Count <= 1)
				{
					continue;
				}
				foreach (BaseNetworkable item2 in group)
				{
					BasePlayer basePlayer3 = item2 as BasePlayer;
					if (basePlayer3 == null || basePlayer == basePlayer3)
					{
						continue;
					}
					bool flag = true;
					bool flag2 = ConVar.AntiHack.server_occlusion_disable_sleeper_los;
					if (basePlayer3.IsConnected)
					{
						flag = CustomShouldNetworkTo(basePlayer3, basePlayer);
						flag2 = false;
						if (flag)
						{
							flag2 = CustomShouldSkipServerOcclusion(basePlayer3, basePlayer);
						}
					}
					if (!flag)
					{
						continue;
					}
					OcclusionLastSeenStatus occlusionLastSeenStatus = basePlayer.OcclusionGetRecentlySeen(basePlayer3, networkTime);
					OcclusionPlayerPair occlusionPlayerPair = default(OcclusionPlayerPair);
					occlusionPlayerPair.from = basePlayer3;
					occlusionPlayerPair.to = basePlayer;
					occlusionPlayerPair.lastSeenStatus = occlusionLastSeenStatus;
					OcclusionPlayerPair element = occlusionPlayerPair;
					if (occlusionLastSeenStatus == OcclusionLastSeenStatus.Valid)
					{
						if (element.from.IsConnected)
						{
							element.from.unoccludedSubscribers.Add(element.to.net.connection);
						}
						OcclusionFrameCache.Add((element.from.net.ID.Value, element.to.net.ID.Value));
					}
					else if (flag2)
					{
						if (element.from.IsConnected)
						{
							element.from.unoccludedSubscribers.Add(element.to.net.connection);
						}
						pairsFound.Add(element);
						OcclusionFrameCache.Add((element.from.net.ID.Value, element.to.net.ID.Value));
					}
					else
					{
						pairsToCheck.Add(element);
					}
				}
			}
		}
	}

	private static bool CustomShouldNetworkTo(BasePlayer from, BasePlayer to)
	{
		if (from.IsSpectating())
		{
			return false;
		}
		if (from.isInvisible)
		{
			return to.IsSpectating();
		}
		if (from.limitNetworking)
		{
			BaseEntity baseEntity = from.GetParentEntity();
			if (baseEntity == null)
			{
				return false;
			}
			if (baseEntity != to)
			{
				return false;
			}
		}
		if (from.ShouldInheritNetworkGroup())
		{
			BaseEntity baseEntity2 = from.GetParentEntity();
			if (baseEntity2 != null)
			{
				return baseEntity2.ShouldNetworkTo(to);
			}
		}
		return true;
	}

	private static bool CustomShouldSkipServerOcclusion(BasePlayer from, BasePlayer to)
	{
		if (from.SubGrid.Equals(default(ServerOcclusion.SubGrid)) || to.SubGrid.Equals(default(ServerOcclusion.SubGrid)))
		{
			return true;
		}
		if (from.SubGrid.GetDistance(to.SubGrid) < ServerOcclusion.MinOcclusionDistance)
		{
			return true;
		}
		if (from.ShouldSkipServerOcclusion(to))
		{
			return true;
		}
		return false;
	}

	private static bool CustomShouldSkipServerOcclusionParallel(BasePlayer from, BasePlayer to, bool observerShouldSkipOcclusion)
	{
		if (from.SubGrid.Equals(default(ServerOcclusion.SubGrid)) || to.SubGrid.Equals(default(ServerOcclusion.SubGrid)))
		{
			return true;
		}
		if (from.SubGrid.GetDistance(to.SubGrid) < ServerOcclusion.MinOcclusionDistance)
		{
			return true;
		}
		return observerShouldSkipOcclusion;
	}

	public static void GatherPairsParallel(StableObjectArray<BasePlayer> playerCache, NativeArray<Vector3>.ReadOnly playerPoses, BufferList<OcclusionPlayerPair> pairsToCheck, BufferList<OcclusionPlayerPair> pairsFound, float networkTime, bool deepSeaEnabled)
	{
		using (TimeWarning.New("Gather Occlusion Pairs For Checking (Parallel)"))
		{
			int length = playerCache.Objects.Length;
			int num = Mathf.Max(1, ConVar.Server.OcclusionGatherBatchPlayerCount);
			int num2 = (length + num - 1) / num;
			BufferList<OcclusionPairWorkerBuffers> obj = Facepunch.Pool.Get<BufferList<OcclusionPairWorkerBuffers>>();
			for (int i = 0; i < num2; i++)
			{
				obj.Add(Facepunch.Pool.Get<OcclusionPairWorkerBuffers>());
			}
			using (TimeWarning.New("UniTask Accumulation"))
			{
				using PooledList<UniTask> pooledList = Facepunch.Pool.Get<PooledList<UniTask>>();
				for (int j = 0; j < num2; j++)
				{
					int num3 = j * num;
					int count = Math.Min(num, length - num3);
					OcclusionPairWorkerBuffers buffers = obj[j];
					pooledList.Add(GatherOcclusionPairsChunk(playerCache, playerPoses, num3, count, networkTime, deepSeaEnabled, buffers));
				}
				ThreadUtils.WaitForTasks(pooledList);
			}
			using (TimeWarning.New("Merge Occlusion Pairs"))
			{
				for (int k = 0; k < num2; k++)
				{
					OcclusionPairWorkerBuffers occlusionPairWorkerBuffers = obj[k];
					pairsToCheck.AddSpan(occlusionPairWorkerBuffers.ToCheck.ContentReadOnlySpan());
					pairsFound.AddSpan(occlusionPairWorkerBuffers.Found.ContentReadOnlySpan());
					BufferList<(BasePlayer, BasePlayer)> subAdds = occlusionPairWorkerBuffers.SubAdds;
					for (int l = 0; l < subAdds.Count; l++)
					{
						var (basePlayer, basePlayer2) = subAdds[l];
						basePlayer.unoccludedSubscribers.Add(basePlayer2.net.connection);
					}
					BufferList<(ulong, ulong)> cacheAdds = occlusionPairWorkerBuffers.CacheAdds;
					for (int m = 0; m < cacheAdds.Count; m++)
					{
						var (item, item2) = cacheAdds[m];
						OcclusionFrameCache.Add((item, item2));
					}
				}
			}
			for (int n = 0; n < num2; n++)
			{
				OcclusionPairWorkerBuffers obj2 = obj[n];
				Facepunch.Pool.Free(ref obj2);
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
	}

	private static async UniTask GatherOcclusionPairsChunk(StableObjectArray<BasePlayer> playerCache, NativeArray<Vector3>.ReadOnly observerPositions, int start, int count, float networkTime, bool deepSeaEnabled, OcclusionPairWorkerBuffers buffers)
	{
		await UniTask.SwitchToThreadPool();
		BufferList<OcclusionPlayerPair> toCheck = buffers.ToCheck;
		BufferList<OcclusionPlayerPair> found = buffers.Found;
		BufferList<(BasePlayer, BasePlayer)> subAdds = buffers.SubAdds;
		BufferList<(ulong, ulong)> cacheAdds = buffers.CacheAdds;
		using (TimeWarning.New("GatherOcclusionPairsChunk"))
		{
			BasePlayer[] unsafeObjects = playerCache.UnsafeObjects;
			for (int i = start; i < start + count; i++)
			{
				BasePlayer basePlayer = unsafeObjects[i];
				subAdds.Add((basePlayer, basePlayer));
				if (basePlayer.IsSpectating())
				{
					if (!(basePlayer.net.SubStrategy is SpectatorSubStrategy spectatorSubStrategy))
					{
						continue;
					}
					ServerOcclusion.Group value = null;
					if (spectatorSubStrategy.SpectatedPlayer != null)
					{
						value = spectatorSubStrategy.SpectatedPlayer.OcclusionGroup;
					}
					else if (spectatorSubStrategy.LastGroup != null)
					{
						ServerOcclusion.Occludees.TryGetValue(spectatorSubStrategy.LastGroup, out value);
					}
					if (value == null)
					{
						continue;
					}
					foreach (BaseNetworkable item in value)
					{
						BasePlayer basePlayer2 = item as BasePlayer;
						if (!(basePlayer2 == null) && !(basePlayer == basePlayer2))
						{
							if (basePlayer2.IsConnected)
							{
								subAdds.Add((basePlayer2, basePlayer));
							}
							cacheAdds.Add((basePlayer2.net.ID.Value, basePlayer.net.ID.Value));
						}
					}
					continue;
				}
				ServerOcclusion.Group group = basePlayer.OcclusionGroup;
				if (group == null || group.Count <= 1)
				{
					continue;
				}
				bool observerShouldSkipOcclusion = basePlayer.ComputeObserverShouldSkipOcclusion(observerPositions[basePlayer.ActivePlayerInd], deepSeaEnabled);
				foreach (BaseNetworkable item2 in group)
				{
					BasePlayer basePlayer3 = item2 as BasePlayer;
					if (basePlayer3 == null || basePlayer == basePlayer3)
					{
						continue;
					}
					bool flag = true;
					bool flag2 = ConVar.AntiHack.server_occlusion_disable_sleeper_los;
					if (basePlayer3.IsConnected)
					{
						flag = CustomShouldNetworkTo(basePlayer3, basePlayer);
						flag2 = false;
						if (flag)
						{
							flag2 = CustomShouldSkipServerOcclusionParallel(basePlayer3, basePlayer, observerShouldSkipOcclusion);
						}
					}
					if (!flag)
					{
						continue;
					}
					OcclusionLastSeenStatus occlusionLastSeenStatus = basePlayer.OcclusionGetRecentlySeen(basePlayer3, networkTime);
					OcclusionPlayerPair occlusionPlayerPair = default(OcclusionPlayerPair);
					occlusionPlayerPair.from = basePlayer3;
					occlusionPlayerPair.to = basePlayer;
					occlusionPlayerPair.lastSeenStatus = occlusionLastSeenStatus;
					OcclusionPlayerPair element = occlusionPlayerPair;
					if (occlusionLastSeenStatus == OcclusionLastSeenStatus.Valid)
					{
						if (element.from.IsConnected)
						{
							subAdds.Add((element.from, element.to));
						}
						cacheAdds.Add((basePlayer3.net.ID.Value, basePlayer.net.ID.Value));
					}
					else if (flag2)
					{
						if (element.from.IsConnected)
						{
							subAdds.Add((element.from, element.to));
						}
						found.Add(element);
						cacheAdds.Add((basePlayer3.net.ID.Value, basePlayer.net.ID.Value));
					}
					else
					{
						toCheck.Add(element);
					}
				}
			}
		}
	}

	private bool ShouldSkipServerOcclusion(BasePlayer player)
	{
		return player.ComputeObserverShouldSkipOcclusion(player.transform.position, DeepSea.enabled);
	}

	public void OcclusionResetUnoccludedSubscribers()
	{
		if (unoccludedSubscribers == null)
		{
			unoccludedSubscribers = Facepunch.Pool.Get<List<Network.Connection>>();
		}
		else
		{
			unoccludedSubscribers.Clear();
		}
	}

	private bool ComputeObserverShouldSkipOcclusion(Vector3 pos, bool deepSeaEnabled)
	{
		bool server_occlusion_disable_los = ConVar.AntiHack.server_occlusion_disable_los;
		bool flag = GetMounted() is ComputerStation;
		bool flag2 = OcclusionShouldSeeAllPlayers();
		bool flag3 = deepSeaEnabled && DeepSeaManager.IsInsideDeepSea(pos);
		return server_occlusion_disable_los || flag || flag2 || flag3;
	}

	public bool OcclusionLineOfSight(BasePlayer player)
	{
		ServerOcclusion.SubGrid subGrid = player.SubGrid;
		if (SubGrid.GetDistance(subGrid) < ServerOcclusion.MinOcclusionDistance)
		{
			return true;
		}
		if (SubGrid.Equals(default(ServerOcclusion.SubGrid)) || subGrid.Equals(default(ServerOcclusion.SubGrid)))
		{
			return true;
		}
		if (ConVar.AntiHack.server_occlusion_caching)
		{
			using (TimeWarning.New("OcclusionCache"))
			{
				if (ServerOcclusion.GetCachedVisibility(SubGrid, subGrid, out var result))
				{
					return result;
				}
			}
		}
		using (TimeWarning.New("CalculatePathBetweenGrids"))
		{
			ServerOcclusion.CalculatePathBetweenGrids(SubGrid, subGrid, out var pathBlocked);
			if (ConVar.AntiHack.server_occlusion_caching)
			{
				ServerOcclusion.CacheVisibility(SubGrid, subGrid, !pathBlocked);
			}
			return !pathBlocked;
		}
	}

	public static void OcclusionLineOfSight(ReadOnlySpan<OcclusionPlayerPair> pairsToCheck, NativeArray<bool> results)
	{
		NativeList<(ServerOcclusion.SubGrid, ServerOcclusion.SubGrid)> nativeList = new NativeList<(ServerOcclusion.SubGrid, ServerOcclusion.SubGrid)>(pairsToCheck.Length, Allocator.TempJob);
		NativeList<int> nativeList2 = new NativeList<int>(pairsToCheck.Length, Allocator.Temp);
		NativeHashMap<long, int> nativeHashMap = new NativeHashMap<long, int>(pairsToCheck.Length, Allocator.Temp);
		NativeList<(int, int)> nativeList3 = new NativeList<(int, int)>(pairsToCheck.Length, Allocator.Temp);
		for (int i = 0; i < pairsToCheck.Length; i++)
		{
			BasePlayer from = pairsToCheck[i].from;
			BasePlayer to = pairsToCheck[i].to;
			if (ConVar.AntiHack.server_occlusion_caching && ServerOcclusion.GetCachedVisibility(from.SubGrid, to.SubGrid, out var value))
			{
				results[i] = value;
				continue;
			}
			int num = from.SubGrid.GetIndex();
			int num2 = to.SubGrid.GetIndex();
			if (num > num2)
			{
				int num3 = num2;
				int num4 = num;
				num = num3;
				num2 = num4;
			}
			long key = ((long)num << 32) + num2;
			if (nativeHashMap.TryGetValue(key, out var item))
			{
				nativeList3.AddNoResize((i, item));
				continue;
			}
			nativeHashMap.Add(key, i);
			nativeList.AddNoResize((from.SubGrid, to.SubGrid));
			nativeList2.AddNoResize(i);
		}
		nativeHashMap.Dispose();
		NativeArray<bool> pathsBlocked = new NativeArray<bool>(nativeList.Length, Allocator.TempJob);
		ServerOcclusion.CalculatePathsBetweenGridsJob(nativeList.AsReadOnly(), pathsBlocked).Complete();
		if (ConVar.AntiHack.server_occlusion_caching)
		{
			for (int j = 0; j < nativeList.Length; j++)
			{
				ServerOcclusion.SubGrid item2 = nativeList[j].Item1;
				ServerOcclusion.SubGrid item3 = nativeList[j].Item2;
				bool flag = pathsBlocked[j];
				ServerOcclusion.CacheVisibility(item2, item3, !flag);
			}
		}
		for (int k = 0; k < nativeList.Length; k++)
		{
			int index = nativeList2[k];
			bool flag2 = pathsBlocked[k];
			results[index] = !flag2;
		}
		nativeList.Dispose();
		pathsBlocked.Dispose();
		nativeList2.Dispose();
		foreach (var item6 in nativeList3)
		{
			int item4 = item6.Item1;
			int item5 = item6.Item2;
			bool value2 = results[item5];
			results[item4] = value2;
		}
		nativeList3.Dispose();
	}

	private static void OcclusionSendUpdates(ReadOnlySpan<OcclusionPlayerPair> pairsFound, ReadOnlySpan<OcclusionPlayerPair> pairsLost, float networkTime)
	{
		BufferList<(BaseEntity, BasePlayer)> obj = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
		OcclusionGatherFoundPairsToSend(pairsFound, obj, networkTime);
		BufferList<(BaseEntity, BasePlayer)> obj2 = Facepunch.Pool.Get<BufferList<(BaseEntity, BasePlayer)>>();
		OcclusionGatherLostPairsToSend(pairsLost, obj2);
		using PooledList<UniTask> tasks = Facepunch.Pool.Get<PooledList<UniTask>>();
		SendEntityDestroyMessages(obj2, tasks);
		SendEntitySnapshotsWithChildren(obj.ContentReadOnlySpan(), tasks);
		ThreadUtils.WaitForTasks(tasks);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
	}

	private static void OcclusionGatherFoundPairsToSend(ReadOnlySpan<OcclusionPlayerPair> pairsFound, BufferList<(BaseEntity, BasePlayer)> toSendPairs, float networkTime)
	{
		ReadOnlySpan<OcclusionPlayerPair> readOnlySpan = pairsFound;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			OcclusionPlayerPair occlusionPlayerPair = readOnlySpan[i];
			occlusionPlayerPair.to.lastPlayerVisibility[occlusionPlayerPair.from.net.ID.Value] = networkTime;
			if (occlusionPlayerPair.lastSeenStatus == OcclusionLastSeenStatus.None)
			{
				toSendPairs.Add((occlusionPlayerPair.from, occlusionPlayerPair.to));
			}
		}
	}

	private static void OcclusionGatherLostPairsToSend(ReadOnlySpan<OcclusionPlayerPair> pairsLost, BufferList<(BaseEntity, BasePlayer)> toSendPairs)
	{
		ReadOnlySpan<OcclusionPlayerPair> readOnlySpan = pairsLost;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			OcclusionPlayerPair occlusionPlayerPair = readOnlySpan[i];
			if (occlusionPlayerPair.lastSeenStatus == OcclusionLastSeenStatus.Expired)
			{
				occlusionPlayerPair.to.lastPlayerVisibility.Remove(occlusionPlayerPair.from.net.ID.Value);
				toSendPairs.Add((occlusionPlayerPair.from, occlusionPlayerPair.to));
			}
		}
	}

	private OcclusionLastSeenStatus OcclusionGetRecentlySeen(BasePlayer player, float networkTime)
	{
		ulong value = player.net.ID.Value;
		if (lastPlayerVisibility.TryGetValue(value, out var value2))
		{
			if (networkTime - value2 < ServerOcclusion.OcclusionPollRate)
			{
				return OcclusionLastSeenStatus.Valid;
			}
			return OcclusionLastSeenStatus.Expired;
		}
		return OcclusionLastSeenStatus.None;
	}

	private bool OcclusionShouldSeeAllPlayers()
	{
		if (IsSpectating())
		{
			return true;
		}
		if (isInvisible)
		{
			return true;
		}
		if (ConVar.AntiHack.server_occlusion_admin_bypass && (IsAdmin || IsDeveloper))
		{
			return true;
		}
		return false;
	}

	public void OcclusionMakeSubscribersForget()
	{
		ulong value = net.ID.Value;
		foreach (Network.Connection subscriber in net.group.subscribers)
		{
			(subscriber.player as BasePlayer).lastPlayerVisibility.Remove(value);
		}
	}

	public bool OcclusionGetCachedVisibility(BaseEntity ent)
	{
		Debug.Assert(ent.SupportsServerOcclusion());
		return OcclusionFrameCache.Contains((net.ID.Value, ent.net.ID.Value));
	}

	public ReadOnlySpan<BasePlayer> GetSpectators()
	{
		if (IsBeingSpectated)
		{
			return (net.SubStrategy as SpectatedSubStrategy).GetSpectators();
		}
		return default(ReadOnlySpan<BasePlayer>);
	}

	public void SetSpectateTeamInfo(bool state)
	{
		IsSpectatingTeamInfo = state;
	}

	private void Tick_Spectator()
	{
		int num = 0;
		if (serverInput.WasJustPressed(BUTTON.JUMP))
		{
			num++;
		}
		if (serverInput.WasJustPressed(BUTTON.DUCK))
		{
			num--;
		}
		if (num != 0)
		{
			SpectateOffset += num;
			using (TimeWarning.New("UpdateSpectateTarget"))
			{
				UpdateSpectateTarget(spectateFilter);
			}
		}
		if (!((float)lastSpectateTeamInfoUpdate > 0.5f) || !IsSpectatingTeamInfo)
		{
			return;
		}
		lastSpectateTeamInfoUpdate = 0f;
		using SpectateTeamInfo spectateTeamInfo = Facepunch.Pool.Get<SpectateTeamInfo>();
		spectateTeamInfo.teams = Facepunch.Pool.Get<List<SpectateTeam>>();
		spectateTeamInfo.teams.Clear();
		foreach (KeyValuePair<ulong, RelationshipManager.PlayerTeam> team in RelationshipManager.ServerInstance.teams)
		{
			SpectateTeam spectateTeam = Facepunch.Pool.Get<SpectateTeam>();
			spectateTeam.teamId = team.Key;
			spectateTeam.teamMembers = Facepunch.Pool.Get<List<PlayerTeam.TeamMember>>();
			spectateTeam.teamMembers.Clear();
			foreach (ulong member in team.Value.members)
			{
				PlayerTeam.TeamMember teamMember = Facepunch.Pool.Get<PlayerTeam.TeamMember>();
				teamMember.userID = member;
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				teamMember.displayName = ((basePlayer != null) ? basePlayer.displayName : (SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member) ?? "DEAD"));
				teamMember.healthFraction = ((basePlayer != null && basePlayer.IsAlive()) ? basePlayer.healthFraction : 0f);
				teamMember.position = ((basePlayer != null) ? basePlayer.transform.position : Vector3.zero);
				teamMember.online = basePlayer != null && !basePlayer.IsSleeping();
				teamMember.wounded = basePlayer != null && basePlayer.IsWounded();
				spectateTeam.teamMembers.Add(teamMember);
			}
			spectateTeamInfo.teams.Add(spectateTeam);
		}
		ClientRPC(RpcTarget.Player("ReceiveSpectateTeamInfo", this), spectateTeamInfo);
	}

	public void UpdateSpectateTarget(string strName, bool invalidateIfNone = false, bool announce = true)
	{
		if (Interface.CallHook("CanSpectateTarget", this, strName) != null)
		{
			return;
		}
		BasePlayer basePlayer = this;
		bool checkName;
		using (TimeWarning.New("BasePlayer.UpdateSpectateTarget"))
		{
			spectateFilter = strName;
			checkName = !string.IsNullOrWhiteSpace(strName);
			using PooledList<BasePlayer> pooledList = Facepunch.Pool.Get<PooledList<BasePlayer>>();
			int i = 0;
			for (int count = activePlayerList.Count; i < count; i++)
			{
				BasePlayer basePlayer2 = activePlayerList[i];
				if (IsPlayerEligible(basePlayer2))
				{
					pooledList.Add(basePlayer2);
				}
			}
			if (pooledList.Count > 0)
			{
				pooledList.Sort(DisplayNameComparison);
			}
			if (net.connection.info.GetBool("global.spectatebots"))
			{
				using PooledList<BasePlayer> pooledList2 = Facepunch.Pool.Get<PooledList<BasePlayer>>();
				int j = 0;
				for (int count2 = bots.Count; j < count2; j++)
				{
					BasePlayer basePlayer3 = bots[j];
					if (IsPlayerEligible(basePlayer3))
					{
						pooledList2.Add(basePlayer3);
					}
				}
				if (pooledList2.Count > 0)
				{
					pooledList2.Sort(DisplayNameComparison);
					pooledList.AddRange(pooledList2);
				}
			}
			int count3 = pooledList.Count;
			if (count3 == 0)
			{
				if (announce)
				{
					ChatMessage("No valid spectate targets for filter " + spectateFilter + "!");
				}
				if (invalidateIfNone)
				{
					SpectatePlayer(null, announce);
				}
			}
			else
			{
				BasePlayer target = pooledList[SpectateOffset % count3];
				SpectatePlayer(target, announce);
			}
		}
		bool IsPlayerEligible(BasePlayer player)
		{
			if (player == this)
			{
				return false;
			}
			if (player == null)
			{
				return false;
			}
			if (player.IsNpc)
			{
				return false;
			}
			if (player.IsNpc || player.IsSpectating() || player.IsDead() || player.IsSleeping())
			{
				return false;
			}
			if (checkName)
			{
				if (!player.displayName.Contains(spectateFilter, CompareOptions.IgnoreCase))
				{
					return player.UserIDString.Contains(spectateFilter);
				}
				return true;
			}
			return true;
		}
	}

	public void UpdateSpectateTarget(ulong id)
	{
		foreach (BasePlayer activePlayer in activePlayerList)
		{
			if (activePlayer != null && (ulong)activePlayer.userID == id)
			{
				spectateFilter = string.Empty;
				SpectatePlayer(activePlayer);
				break;
			}
		}
	}

	private void DropSpectators()
	{
		ISubscriberStrategy subStrategy = net.SubStrategy;
		if (!(subStrategy is SpectatedSubStrategy spectatedSubStrategy))
		{
			if (subStrategy is SpectatorSubStrategy)
			{
				StopSpectating();
			}
			return;
		}
		ReadOnlySpan<BasePlayer> spectators = spectatedSubStrategy.GetSpectators();
		for (int num = spectators.Length - 1; num >= 0; num--)
		{
			spectators[num].SpectatePlayer(null);
		}
	}

	private void SpectatePlayer(BasePlayer target, bool announce = true)
	{
		if (target == this)
		{
			return;
		}
		if ((bool)spectatingTarget)
		{
			SpectatedSubStrategy obj = spectatingTarget.net.SubStrategy as SpectatedSubStrategy;
			if (obj.RemoveSpectator(this))
			{
				Facepunch.Pool.Free(ref obj);
				spectatingTarget.net.SubStrategy = Network.Server.DefaultSubscriberStrategy;
			}
		}
		if (target != null)
		{
			if (announce)
			{
				ChatMessage("Spectating: " + target.displayName + ". SteamID: " + target.UserIDString);
			}
			if (target.net.SubStrategy is SpectatedSubStrategy spectatedSubStrategy)
			{
				spectatedSubStrategy.AddSpectator(this);
			}
			else
			{
				SpectatedSubStrategy spectatedSubStrategy2 = Facepunch.Pool.Get<SpectatedSubStrategy>();
				spectatedSubStrategy2.AddSpectator(this);
				target.net.SubStrategy = spectatedSubStrategy2;
			}
			using (TimeWarning.New("SendEntitySnapshot"))
			{
				if (ServerOcclusion.OcclusionEnabled)
				{
					OcclusionFrameCache.Add((target.net.ID.Value, net.ID.Value));
				}
				SendEntitySnapshot(target);
			}
			ClientRPC(RpcTarget.Player("SpectateTarget", this), target.net.ID);
		}
		else
		{
			ClientRPC(RpcTarget.Player("SpectateTarget", this), default(NetworkableId));
		}
		SpectatorSubStrategy spectatorSubStrategy = net.SubStrategy as SpectatorSubStrategy;
		if (spectatorSubStrategy == null)
		{
			net.SubStrategy = Facepunch.Pool.Get<SpectatorSubStrategy>();
			spectatorSubStrategy = net.SubStrategy as SpectatorSubStrategy;
		}
		spectatorSubStrategy.SpectatedPlayer = target;
		if (target == null && spectatingTarget != null)
		{
			spectatorSubStrategy.LastGroup = spectatingTarget.net.group;
		}
		spectatingTarget = target;
		if (spectatingTarget != null && !net.subscriber.IsSubscribed(spectatingTarget.net.group))
		{
			ClearEntityQueue();
			SendEntitySnapshot(this);
			net.InvalidateSubscriptions(2);
		}
		PostSetSpectatePlayer(target);
	}

	private void PostSetSpectatePlayer(BasePlayer player)
	{
		if (!(player == null) && player.metabolism != null)
		{
			player.metabolism.ForceSendChangesToSpectators();
		}
	}

	public void StartSpectating()
	{
		if (!IsSpectating() && Interface.CallHook("OnPlayerSpectate", this, spectateFilter) == null)
		{
			SetPlayerFlag(PlayerFlags.Spectating, b: true);
			UnityEngine.TransformEx.SetLayerRecursive(base.gameObject, 10);
			CancelInvoke(InventoryUpdate);
			ChatMessage("Becoming Spectator");
			UpdateSpectateTarget(spectateFilter, invalidateIfNone: true, announce: false);
			Query.Server.RemovePlayer(this);
		}
	}

	public void StopSpectating()
	{
		if (!IsSpectating() || Interface.CallHook("OnPlayerSpectateEnd", this, spectateFilter) != null)
		{
			return;
		}
		if ((bool)spectatingTarget)
		{
			SpectatedSubStrategy obj = spectatingTarget.net.SubStrategy as SpectatedSubStrategy;
			if (obj.RemoveSpectator(this))
			{
				Facepunch.Pool.Free(ref obj);
				spectatingTarget.net.SubStrategy = Network.Server.DefaultSubscriberStrategy;
			}
		}
		spectatingTarget = null;
		SpectatorSubStrategy obj2 = net.SubStrategy as SpectatorSubStrategy;
		Facepunch.Pool.Free(ref obj2);
		net.SubStrategy = Network.Server.DefaultSubscriberStrategy;
		SetPlayerFlag(PlayerFlags.Spectating, b: false);
		UnityEngine.TransformEx.SetLayerRecursive(base.gameObject, 17);
		Query.Server.RemovePlayer(this);
		Query.Server.AddPlayer(this);
	}

	public void Teleport(BasePlayer player)
	{
		Teleport(player.transform.position);
	}

	public void Teleport(string strName, bool playersOnly)
	{
		BaseEntity[] array = Util.FindTargets(strName, playersOnly);
		if (array != null && array.Length != 0)
		{
			BaseEntity baseEntity = array[UnityEngine.Random.Range(0, array.Length)];
			Teleport(baseEntity.transform.position);
		}
	}

	public void TeleportToNearestTargetEntity(string entityName, int index)
	{
		BaseEntity[] array = Util.FindTargets(entityName, onlyPlayers: false);
		if (array == null || array.Length == 0)
		{
			return;
		}
		using PooledList<BaseEntity> pooledList = Facepunch.Pool.Get<PooledList<BaseEntity>>();
		pooledList.AddRange(array.OrderBy((BaseEntity entity) => Vector3.SqrMagnitude(entity.transform.position - base.transform.position)));
		Vector3 zero = Vector3.zero;
		if (index <= 0)
		{
			zero = pooledList[0].transform.position;
		}
		else if (index >= array.Length)
		{
			zero = pooledList[pooledList.Count - 1].transform.position;
		}
		else
		{
			zero = pooledList[index].transform.position;
		}
		Teleport(zero);
	}

	public void Teleport(Vector3 position)
	{
		MovePosition(position);
		ClientRPC(RpcTarget.Player("ForcePositionTo", this), position);
	}

	public void CopyRotation(BasePlayer player)
	{
		viewAngles = player.viewAngles;
		SendNetworkUpdate_Position();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	[RPC_Server.FromOwner]
	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	private void UpdateSpectatePositionFromDebugCamera(RPCMessage msg)
	{
		if (IsSpectating() && ConVar.Global.updateNetworkPositionWithDebugCameraWhileSpectating)
		{
			Vector3 position = msg.read.Vector3();
			base.transform.position = position;
			SetParent(null);
		}
	}

	[RPC_Server]
	private void NotifyDebugCameraEnded(RPCMessage msg)
	{
		if (IsSpectating() && ConVar.Global.updateNetworkPositionWithDebugCameraWhileSpectating)
		{
			UpdateSpectateTarget(spectateFilter);
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (IsSleeping())
		{
			return false;
		}
		if (!IsAlive())
		{
			return false;
		}
		if (InSafeZone())
		{
			return false;
		}
		if (splashType == null || splashType.shortname == null)
		{
			return false;
		}
		if (!(splashType == WaterTypes.RadioactiveWaterItemDef) && !(splashType == WaterTypes.WaterItemDef))
		{
			return splashType == WaterTypes.SaltWaterItemDef;
		}
		return true;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		CheckWaterRadiation(splashType, amount);
		CheckWater(splashType, amount);
		return amount;
	}

	public int DoSplashFunWater(ItemDefinition splashType, int amount)
	{
		CheckWaterRadiation(splashType, amount);
		return amount;
	}

	private void CheckWaterRadiation(ItemDefinition splashType, int amount)
	{
		if (splashType == WaterTypes.RadioactiveWaterItemDef)
		{
			float a = (float)amount * Radiation.MaterialToRadsRatio;
			a = Mathf.Max(a, 0.5f);
			ApplyRadiation(a);
		}
	}

	private void CheckWater(ItemDefinition splashType, int amount)
	{
		if (splashType == WaterTypes.WaterItemDef || splashType == WaterTypes.SaltWaterItemDef)
		{
			float a = (float)amount * 0.01f;
			a = Mathf.Max(a, 5f);
			timeSinceLastWaterSplash = 0f;
			if (!(baseProtection.amounts[4] > 0f))
			{
				metabolism.wetness.Add(a);
			}
		}
	}

	public void AddNeabyStash(StashContainer newStash)
	{
		if (newStash == null)
		{
			return;
		}
		foreach (NearbyStash nearbyStash in nearbyStashes)
		{
			if (nearbyStash.Entity == newStash)
			{
				return;
			}
		}
		if (nearbyStashes.Count == 0)
		{
			InvokeRepeating(CheckStashRevealInvoke, 0f, StashContainer.PlayerDetectionTickRate);
		}
		nearbyStashes.Add(new NearbyStash(newStash));
	}

	public void RemoveNearbyStash(StashContainer stash)
	{
		for (int i = 0; i < nearbyStashes.Count; i++)
		{
			if (!(nearbyStashes[i].Entity != stash))
			{
				nearbyStashes.RemoveAt(i);
				break;
			}
		}
		if (nearbyStashes.Count == 0)
		{
			CancelInvoke(CheckStashRevealInvoke);
		}
	}

	private void CheckStashRevealInvoke()
	{
		for (int i = 0; i < nearbyStashes.Count; i++)
		{
			NearbyStash nearbyStash = nearbyStashes[i];
			if (nearbyStash.Entity == null || nearbyStash.Entity.IsDestroyed)
			{
				nearbyStashes.RemoveAt(i);
			}
			else if (nearbyStash.Entity.IsHidden() && nearbyStash.Entity.PlayerInRange(this))
			{
				nearbyStash.LookingAtTime += StashContainer.PlayerDetectionTickRate;
				if (nearbyStash.LookingAtTime >= nearbyStash.Entity.uncoverTime)
				{
					if (Interface.CallHook("CanSeeStash", this, nearbyStash.Entity) != null)
					{
						break;
					}
					nearbyStash.Entity.SetHidden(isHidden: false);
					Facepunch.Rust.Analytics.Azure.OnStashRevealed(this, nearbyStash.Entity);
					Interface.CallHook("OnStashExposed", nearbyStash.Entity, this);
				}
			}
			else
			{
				nearbyStash.LookingAtTime = 0f;
			}
		}
	}

	public override float GetThreatLevel()
	{
		EnsureUpdated();
		return cachedThreatLevel;
	}

	public void EnsureUpdated()
	{
		if (UnityEngine.Time.realtimeSinceStartup - lastUpdateTime < 30f)
		{
			return;
		}
		lastUpdateTime = UnityEngine.Time.realtimeSinceStartup;
		cachedThreatLevel = 0f;
		if (IsSleeping() || Interface.CallHook("OnThreatLevelUpdate", this) != null)
		{
			return;
		}
		if (inventory.containerWear.itemList.Count > 2)
		{
			cachedThreatLevel += 1f;
		}
		foreach (Item item in inventory.containerBelt.itemList)
		{
			BaseEntity heldEntity = item.GetHeldEntity();
			if ((bool)heldEntity && heldEntity is BaseProjectile && !(heldEntity is BowWeapon))
			{
				cachedThreatLevel += 2f;
				break;
			}
		}
	}

	public override bool IsHostile()
	{
		object obj = Interface.CallHook("CanEntityBeHostile", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return State.unHostileTimestamp > TimeEx.currentTimestamp;
	}

	public virtual float GetHostileDuration()
	{
		return Mathf.Clamp((float)(State.unHostileTimestamp - TimeEx.currentTimestamp), 0f, float.PositiveInfinity);
	}

	public void SetHostilePauseTime(float duration = 2f)
	{
		hostilePauseTime = UnityEngine.Time.realtimeSinceStartup + duration;
	}

	private bool IsHostilePaused()
	{
		return UnityEngine.Time.realtimeSinceStartup < hostilePauseTime;
	}

	public override void MarkHostileFor(float duration = 60f)
	{
		if (Interface.CallHook("OnEntityMarkHostile", this, duration) == null && !IsHostilePaused() && !InSafeCombatZone())
		{
			duration = Mathf.Max(duration, (float)(State.unHostileTimestamp - TimeEx.currentTimestamp));
			SetHostileDuration(duration);
		}
	}

	public void SetHostileDuration(float duration)
	{
		duration = Mathf.Max(duration, 0f);
		State.unHostileTimestamp = TimeEx.currentTimestamp + (double)duration;
		DirtyPlayerState();
		ClientRPC(RpcTarget.Player("SetHostileLength", this), duration);
	}

	public void MarkWeaponDrawnDuration(float newDuration)
	{
		float f = weaponDrawnDuration;
		weaponDrawnDuration = newDuration;
		if (Mathf.FloorToInt(newDuration) != Mathf.FloorToInt(f))
		{
			ClientRPC(RpcTarget.Player("SetWeaponDrawnDuration", this), weaponDrawnDuration);
		}
	}

	public void AddWeaponDrawnDuration(float duration)
	{
		if (InSafeCombatZone() || HasPlayerFlag(PlayerFlags.CombatZone))
		{
			timeLastInCombatZone = 0f;
			MarkWeaponDrawnDuration(0f);
		}
		else if (!((float)timeLastInCombatZone < 1f))
		{
			MarkWeaponDrawnDuration(weaponDrawnDuration + duration);
		}
	}

	public void OnReceivedTick(NetRead read)
	{
		using (TimeWarning.New("OnReceiveTickFromStream"))
		{
			PlayerTick playerTick;
			using (TimeWarning.New("PlayerTick.Deserialize"))
			{
				playerTick = read.ProtoDelta(lastReceivedTick);
			}
			using (TimeWarning.New("RecordPacket"))
			{
				net.connection.RecordPacket(15, playerTick);
			}
			using (TimeWarning.New("PlayerTick.Copy"))
			{
				lastReceivedTick?.Dispose();
				lastReceivedTick = playerTick.Copy();
			}
			using (TimeWarning.New("OnReceiveTick"))
			{
				OnReceiveTick(playerTick, wasStalled);
			}
			lastTickTime = UnityEngine.Time.time;
			rawTicksPerSecond.Increment();
			playerTick.Dispose();
		}
	}

	public void OnReceivedVoice(ReadOnlySpan<byte> data)
	{
		NetWrite netWrite = Network.Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.VoiceData);
		netWrite.EntityID(net.ID);
		netWrite.BytesWithSize(data);
		float num = 0f;
		if (HasPlayerFlag(PlayerFlags.VoiceRangeBoost))
		{
			num = Voice.voiceRangeBoostAmount;
		}
		List<Network.Connection> connectionsWithin = BaseNetworkable.GetConnectionsWithin(base.transform.position, 100f + num, includeInvisPlayers: true);
		ComputerStation.AddRemoteVoiceListeners(connectionsWithin, base.transform.position, 100f + num);
		netWrite.Send(new SendInfo(connectionsWithin)
		{
			priority = Priority.Immediate
		});
		if (activeTelephone != null)
		{
			activeTelephone.OnReceivedVoiceFromUser(data);
		}
		if (SingletonComponent<NpcNoiseManager>.Instance != null)
		{
			SingletonComponent<NpcNoiseManager>.Instance.OnVoiceChat(this);
		}
	}

	public void ResetInputIdleTime()
	{
		lastInputTime = UnityEngine.Time.time;
	}

	internal void EACStateUpdate(in CachedState cachedState, in EACTickState tickState)
	{
		if ((cachedState.PlayerFlags & PlayerFlags.ReceivingSnapshot) == 0)
		{
			EACServer.LogPlayerTick(net, tickState);
		}
	}

	public void AddReceiveTickListener(IReceivePlayerTickListener listener)
	{
		if (receiveTickListeners != null && !receiveTickListeners.Contains(listener))
		{
			receiveTickListeners.Add(listener);
		}
	}

	public void RemoveReceiveTickListener(IReceivePlayerTickListener listener)
	{
		receiveTickListeners.Remove(listener);
	}

	private void OnReceiveTick(PlayerTick msg, bool wasPlayerStalled)
	{
		if (msg.inputState != null)
		{
			serverInput.Flip(msg.inputState);
		}
		if (Interface.CallHook("OnPlayerTick", this, msg, wasPlayerStalled) != null)
		{
			return;
		}
		if (serverInput.current.buttons != serverInput.previous.buttons)
		{
			ResetInputIdleTime();
		}
		if (Interface.CallHook("OnPlayerInput", this, serverInput) != null || IsReceivingSnapshot || IsTransferProtected())
		{
			return;
		}
		if (IsSpectating())
		{
			using (TimeWarning.New("Tick_Spectator"))
			{
				Tick_Spectator();
				return;
			}
		}
		if (IsDead())
		{
			return;
		}
		if (IsSleeping())
		{
			if (serverInput.WasJustPressed(BUTTON.FIRE_PRIMARY) || serverInput.WasJustPressed(BUTTON.FIRE_SECONDARY) || serverInput.WasJustPressed(BUTTON.JUMP) || serverInput.WasJustPressed(BUTTON.DUCK))
			{
				EndSleeping();
				SendNetworkUpdateImmediate();
			}
			UpdateActiveItem(default(ItemId));
			return;
		}
		if (IsRestrained && restraintItemId.HasValue && restraintItemId.HasValue)
		{
			UpdateActiveItem(restraintItemId.Value);
		}
		else if (!Belt.CanHoldItem())
		{
			UpdateActiveItem(default(ItemId));
		}
		else
		{
			UpdateActiveItem(msg.activeItem);
		}
		UpdateModelStateFromTick(msg);
		if (float.IsNaN(modelState.ducking) || float.IsInfinity(modelState.ducking))
		{
			Kick("Kicked: invalid modelstate");
			return;
		}
		modelState.ducking = Mathf.Clamp01(modelState.ducking);
		if (IsIncapacitated())
		{
			return;
		}
		ForwardReceiveTickToListeners(msg);
		if (isMounted)
		{
			GetMounted().PlayerServerInput(serverInput, this);
		}
		UpdatePositionFromTick(msg, wasPlayerStalled);
		UpdateRotationFromTick(msg);
		if (TryGetActiveMissionInstance(out var instance) && instance.status == BaseMission.MissionStatus.Active && instance.NeedsPlayerInput())
		{
			ProcessMissionEvent(BaseMission.MissionEventType.PLAYER_TICK, net.ID, 0f);
		}
		if (TutorialIsland.EnforceTrespassChecks && !IsAdmin && !IsNpc && net != null && net.group != null)
		{
			if (net.group.restricted)
			{
				bool flag = false;
				if (!IsInTutorial)
				{
					flag = true;
				}
				else
				{
					TutorialIsland currentTutorialIsland = GetCurrentTutorialIsland();
					if (currentTutorialIsland == null || currentTutorialIsland.net.group != net.group)
					{
						flag = true;
					}
				}
				if (flag)
				{
					tutorialKickTime += UnityEngine.Time.deltaTime;
					if (tutorialKickTime > 3f)
					{
						Debug.LogWarning($"Killing player {displayName}/{userID.Get()} as they are on a tutorial island that doesn't belong them");
						Hurt(999f);
						tutorialKickTime = 0f;
					}
				}
				else
				{
					tutorialKickTime = 0f;
				}
			}
			else if (IsInTutorial && !net.group.restricted)
			{
				bool flag2 = false;
				TutorialIsland currentTutorialIsland2 = GetCurrentTutorialIsland();
				if (currentTutorialIsland2 == null || currentTutorialIsland2.net.group != net.group)
				{
					flag2 = true;
				}
				if (flag2)
				{
					tutorialKickTime += UnityEngine.Time.deltaTime;
					if (tutorialKickTime > 3f)
					{
						Debug.LogWarning($"Killing player {displayName}/{userID.Get()} as they are no longer on a tutorial island and are marked as being in a tutorial");
						Hurt(999f);
						tutorialKickTime = 0f;
					}
				}
				else
				{
					tutorialKickTime = 0f;
				}
			}
		}
		if (ActivePlayerInd != -1 && EACServer.CanSendAnalytics)
		{
			CollectEACTick(msg);
		}
	}

	private void CollectEACTick(PlayerTick tick)
	{
		Vector3 position = tick.position;
		ModelState modelState = modelStateTick ?? this.modelState;
		Vector3 vector = position + GetOffset(modelState.ducked);
		Vector3 vector2 = eyes.PositionWithOverride(position);
		Quaternion quaternion = eyes.parentRotation * Quaternion.Euler(tickViewAngles);
		LogPlayerTickOptions logPlayerTickOptions = default(LogPlayerTickOptions);
		logPlayerTickOptions.PlayerHandle = ClientHandles[ActivePlayerInd];
		logPlayerTickOptions.PlayerPosition = new Vec3f
		{
			x = vector.x,
			y = vector.y,
			z = vector.z
		};
		logPlayerTickOptions.PlayerViewPosition = new Vec3f
		{
			x = vector2.x,
			y = vector2.y,
			z = vector2.z
		};
		logPlayerTickOptions.PlayerViewRotation = new Quat
		{
			w = quaternion.w,
			x = quaternion.x,
			y = quaternion.y,
			z = quaternion.z
		};
		logPlayerTickOptions.PlayerHealth = base.health;
		LogPlayerTickOptions tickOptions = logPlayerTickOptions;
		if (modelState.ducked)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Crouching;
		}
		if (isMounted)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Mounted;
		}
		if (modelState.crawling)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Prone;
		}
		if (modelState.waterLevel >= 0.75f)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Swimming;
		}
		if (!modelState.onground)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Falling;
		}
		if (modelState.onLadder)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.OnLadder;
		}
		if (modelState.flying)
		{
			tickOptions.PlayerMovementState |= AntiCheatCommonPlayerMovementState.Flying;
		}
		int num = Mathf.Min(lastEACTickIndex++, (int)Player.clientTickRate - 1);
		int index = ActivePlayerInd * (int)Player.clientTickRate + num;
		EACTickState value = new EACTickState
		{
			TickOptions = tickOptions
		};
		DateTime unixEpoch = DateTime.UnixEpoch;
		value.Timestamp = unixEpoch.Ticks;
		EACTickStates[index] = value;
	}

	private void RemoveReceiveTickListenersOnDeath()
	{
		for (int num = receiveTickListeners.Count - 1; num >= 0; num--)
		{
			IReceivePlayerTickListener receivePlayerTickListener = receiveTickListeners[num];
			if (receivePlayerTickListener == null)
			{
				receiveTickListeners.RemoveAt(num);
			}
			else if (receivePlayerTickListener.ShouldRemoveOnPlayerDeath())
			{
				receiveTickListeners.Remove(receivePlayerTickListener);
			}
		}
	}

	private void ForwardReceiveTickToListeners(PlayerTick msg)
	{
		if (receiveTickListeners == null)
		{
			return;
		}
		for (int num = receiveTickListeners.Count - 1; num >= 0; num--)
		{
			IReceivePlayerTickListener receivePlayerTickListener = receiveTickListeners[num];
			if (receivePlayerTickListener == null)
			{
				receiveTickListeners.RemoveAt(num);
			}
			else
			{
				receivePlayerTickListener.OnReceivePlayerTick(this, msg);
			}
		}
	}

	public void ApplyStallProtection(float time)
	{
		stallProtectionTime = Mathf.Max(time, stallProtectionTime);
	}

	public void UpdateActiveItem(ItemId itemID)
	{
		Assert.IsTrue(base.isServer, "Realm should be server!");
		if (svActiveItemID == itemID)
		{
			return;
		}
		if (equippingBlocked)
		{
			itemID = default(ItemId);
		}
		Item item = inventory.containerBelt.FindItemByUID(itemID);
		if (IsItemHoldRestricted(item))
		{
			itemID = default(ItemId);
		}
		Item activeItem = GetActiveItem();
		if (Interface.CallHook("OnActiveItemChange", this, activeItem, itemID) != null)
		{
			return;
		}
		svActiveItemID = default(ItemId);
		if (activeItem != null)
		{
			HeldEntity heldEntity = activeItem.GetHeldEntity() as HeldEntity;
			if (heldEntity != null)
			{
				heldEntity.SetHeld(bHeld: false);
			}
		}
		svActiveItemID = itemID;
		if (ConVar.AntiHack.hotbar_network_mode != 1)
		{
			SendNetworkUpdate();
		}
		Item activeItem2 = GetActiveItem();
		if (ConVar.AntiHack.hotbar_network_mode == 1)
		{
			if (inventory.containerBelt.dirty)
			{
				inventory.SendUpdatedInventory(PlayerInventory.Type.Belt, inventory.containerBelt, bSendInventoryToEveryone: true);
			}
			else
			{
				inventory.SendUpdatedInventoryInternal(PlayerInventory.Type.Belt, inventory.containerBelt, PlayerInventory.NetworkInventoryMode.EveryoneButLocal);
			}
		}
		if (activeItem2 != null)
		{
			HeldEntity heldEntity2 = activeItem2.GetHeldEntity() as HeldEntity;
			if (heldEntity2 != null)
			{
				heldEntity2.SetHeld(bHeld: true);
			}
			NotifyGesturesNewItemEquipped();
		}
		if (ConVar.AntiHack.hotbar_network_mode == 1)
		{
			SendNetworkUpdate();
		}
		inventory.UpdatedVisibleHolsteredItems();
		Interface.CallHook("OnActiveItemChanged", this, activeItem, activeItem2);
	}

	internal void UpdateModelStateFromTick(PlayerTick tick)
	{
		if (tick.modelState != null && !ModelState.Equal(modelStateTick, tick.modelState))
		{
			if (modelStateTick != null)
			{
				modelStateTick.ResetToPool();
			}
			modelStateTick = tick.modelState;
			tick.modelState = null;
			PlayerStates.TickNeedsFinalizing[ActivePlayerInd] = true;
		}
	}

	internal void UpdatePositionFromTick(PlayerTick tick, bool wasPlayerStalled)
	{
		if (tick.position.IsNaNOrInfinity() || tick.eyePos.IsNaNOrInfinity())
		{
			Kick("Kicked: Invalid Position");
		}
		else
		{
			if (tick.parentID != parentEntity.uid)
			{
				return;
			}
			ref AntiHack.PlayerState reference = ref ((Span<AntiHack.PlayerState>)AntiHack.PlayerStates)[ActivePlayerInd];
			float num = PlayerStates.TickDeltaTime[ActivePlayerInd];
			reference.TickDistancePausetime = Mathf.Max(0f, reference.TickDistancePausetime - num);
			if (isMounted || (modelState != null && modelState.mounted) || (modelStateTick != null && modelStateTick.mounted) || (IsWounded() && IsRestrained))
			{
				return;
			}
			if (wasPlayerStalled)
			{
				Vector3 endPoint = TickInterpolatorCache.GetEndPoint(PlayerStates.TickCache.ReadOnly, ActivePlayerInd);
				float num2 = Vector3.Distance(tick.position, endPoint);
				if (num2 > 0.01f)
				{
					AntiHack.ResetTimer(this);
				}
				if (num2 > 0.5f)
				{
					ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", this), endPoint, parentEntity.uid);
				}
				return;
			}
			if (!AntiHack.ShouldIgnore(this))
			{
				Vector3 endPoint2 = TickInterpolatorCache.GetEndPoint(PlayerStates.TickCache.ReadOnly, ActivePlayerInd);
				float num3 = Vector3.Distance(tick.position, endPoint2);
				float tick_max_distance = ConVar.AntiHack.tick_max_distance;
				float f = ((ConVar.AntiHack.flyhack_protection <= 0 || AntiHack.PlayerFlyhackStates[ActivePlayerInd].IsInAir || RecentlyInAir()) ? ConVar.AntiHack.tick_max_distance_falling : tick_max_distance);
				float f2 = (HasParent() ? ConVar.AntiHack.tick_max_distance_parented : tick_max_distance);
				float f3 = ((AntiHack.PlayerStates[ActivePlayerInd].TickDistancePausetime > 0f) ? ConVar.AntiHack.tick_distance_forgiveness : tick_max_distance);
				float num4 = Mathx.Max(tick_max_distance, f, f2, f3);
				if (num3 > num4)
				{
					AntiHack.Log(this, AntiHackType.Ticks, $"moved too far between ticks: {num3} units. Max dist: {num4}");
					AntiHack.ResetTimer(this);
					ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", this), endPoint2, parentEntity.uid);
					return;
				}
			}
			PlayerStates.TickCache.AddTick(this, tick.position);
			PlayerStates.TickNeedsFinalizing[ActivePlayerInd] = true;
		}
	}

	internal void UpdateRotationFromTick(PlayerTick tick)
	{
		if (tick.inputState != null)
		{
			if (tick.inputState.aimAngles.IsNaNOrInfinity())
			{
				Kick("Kicked: Invalid Rotation");
				return;
			}
			if (tick.inputState.mouseDelta.IsNaNOrInfinity())
			{
				Kick("Kicked: Invalid Rotation");
				return;
			}
			tickMouseDelta = tick.inputState.mouseDelta;
			tickViewAngles = tick.inputState.aimAngles;
			PlayerStates.TickNeedsFinalizing[ActivePlayerInd] = true;
		}
	}

	public void UpdateEstimatedVelocity(Vector3 lastPos, Vector3 currentPos, float deltaTime)
	{
		estimatedVelocity = (currentPos - lastPos) / deltaTime;
		estimatedSpeed = estimatedVelocity.magnitude;
		estimatedSpeed2D = estimatedVelocity.Magnitude2D();
		if (estimatedSpeed < 0.01f)
		{
			estimatedSpeed = 0f;
		}
		if (estimatedSpeed2D < 0.01f)
		{
			estimatedSpeed2D = 0f;
		}
	}

	private void CheckModelState(in PlayerServerStates playerStates)
	{
		using (TimeWarning.New("ModelState"))
		{
			if (modelStateTick == null)
			{
				return;
			}
			if (modelStateTick.inheritedVelocity != Vector3.zero && FindTrigger<TriggerForce>() == null)
			{
				modelStateTick.inheritedVelocity = Vector3.zero;
			}
			if (modelState != null)
			{
				if (ConVar.AntiHack.modelstate && TriggeredAntiHack())
				{
					modelStateTick.ducked = modelState.ducked;
				}
				modelState.ResetToPool();
				modelState = null;
			}
			modelState = modelStateTick;
			modelStateTick = null;
			UpdateModelState(modelState);
			((Span<ModelState.Flag>)playerStates.PlayerModelStateFlags)[ActivePlayerInd] = (ModelState.Flag)modelState.flags;
			((Span<float>)playerStates.PlayerModelStateDucking)[ActivePlayerInd] = modelState.ducking;
		}
	}

	public static void InitInternalState(int initCap = 32)
	{
		DisposeInternalState();
		PlayerStates.Init(initCap);
		WaterLevel.InitInternalState(initCap);
		AntiHack.InitInternalState(initCap);
	}

	public static void DisposeInternalState()
	{
		PlayerStates.SafeDispose();
		NativeArrayEx.SafeDispose(ref EACTickStates);
		NativeArrayEx.SafeDispose(ref ClientHandles);
		WaterLevel.DisposeInternalState();
		AntiHack.DisposeInternalState();
	}

	private static void FinalizeTickParallel(in PlayerServerStates playerStates, float deltaTime, NativeList<int> toUpdate)
	{
		using (TimeWarning.New("FinalizeTickParallel"))
		{
			StableObjectArray<BasePlayer> playerCache2 = playerStates.PlayerCache;
			NativeList<int> indices = new NativeList<int>(playerCache2.Count, Allocator.TempJob);
			GatherPlayersToFinalize(in playerStates, deltaTime, indices);
			ReadOnlySpan<BasePlayer> objects = playerCache2.Objects;
			_ = playerStates.TickCache.ReadOnly;
			ServerPreFinalize(in playerStates, indices.AsReadOnly());
			ServerCachePlayerInfo(in playerStates, indices.AsReadOnly(), recachePosDependentOnly: false);
			NativeArray<PositionChange> nativeArray = new NativeArray<PositionChange>(objects.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeList<int> toValidate = new NativeList<int>(indices.Length, Allocator.TempJob);
			BasePlayerJobs.GatherPosToValidateJob gatherPosToValidateJob = default(BasePlayerJobs.GatherPosToValidateJob);
			gatherPosToValidateJob.Changes = nativeArray;
			gatherPosToValidateJob.ToValidate = toValidate;
			gatherPosToValidateJob.TickCache = playerStates.TickCache.ReadOnly;
			gatherPosToValidateJob.Indices = indices.AsReadOnly();
			BasePlayerJobs.GatherPosToValidateJob jobData = gatherPosToValidateJob;
			IJobExtensions.RunByRef(ref jobData);
			PlayerServerStates.ReadOnly playerStates2 = playerStates.AsReadOnly();
			AntiHack.ValidateMoves(in playerStates2, toValidate.AsReadOnly(), nativeArray);
			NativeList<int> indicesToGather = new NativeList<int>(toValidate.Length, Allocator.TempJob);
			playerStates2 = playerStates.AsReadOnly();
			GatherPlayersPosChanged(in playerStates2, toValidate.AsReadOnly(), nativeArray.AsReadOnly(), indicesToGather);
			toValidate.Dispose();
			if (!indicesToGather.IsEmpty)
			{
				using (TimeWarning.New("RecachingPlayerState"))
				{
					CachePlayerTransforms(in playerStates);
					ServerCachePlayerInfo(in playerStates, indicesToGather.AsReadOnly(), recachePosDependentOnly: true);
				}
			}
			indicesToGather.Dispose();
			NativeList<int> toBroadcastIndices = new NativeList<int>(indices.Length, Allocator.Temp);
			NativeList<int> validIndices = new NativeList<int>(indices.Length, Allocator.Temp);
			ServerFinalizePlayers(in playerStates, nativeArray.AsReadOnly(), indices.AsReadOnly(), toBroadcastIndices, validIndices);
			indices.Dispose();
			GatherPlayersToUpdate(in playerStates, deltaTime, toUpdate);
			playerStates2 = playerStates.AsReadOnly();
			UpdateSubscriptions(in playerStates2, toUpdate.AsReadOnly(), UnityEngine.Time.realtimeSinceStartup);
			float time = UnityEngine.Time.time;
			using PooledList<UniTask> pooledList = Facepunch.Pool.Get<PooledList<UniTask>>();
			if (EACServer.CanSendAnalytics)
			{
				pooledList.Add(UpdateEAC(playerCache2, validIndices.AsReadOnly(), playerStates.CachedStates.AsReadOnly(), EACTickStates.AsReadOnly(), nativeArray.AsReadOnly()));
			}
			if (Facepunch.Rust.Analytics.GameplayTickAnalyticsConVar)
			{
				pooledList.Add(UpdateAnalytics(playerCache2, validIndices.AsReadOnly(), playerStates.CachedStates.AsReadOnly(), playerStates.PlayerPos.AsReadOnly(), playerStates.IsMounted.AsReadOnly()));
			}
			if (ServerOcclusion.OcclusionEnabled)
			{
				playerStates2 = playerStates.AsReadOnly();
				ServerUpdateOcclusionParallel(in playerStates2, time);
			}
			playerStates2 = playerStates.AsReadOnly();
			NetworkPositionTick(in playerStates2, toBroadcastIndices.AsReadOnly(), time);
			ThreadUtils.WaitForTasks(pooledList);
			toBroadcastIndices.Dispose();
			validIndices.Dispose();
			nativeArray.Dispose();
			if (EACServer.CanSendAnalytics)
			{
				FillJobUnsafe<EACTickState> fillJobUnsafe = default(FillJobUnsafe<EACTickState>);
				fillJobUnsafe.Value = default(EACTickState);
				fillJobUnsafe.Values = EACTickStates;
				FillJobUnsafe<EACTickState> jobData2 = fillJobUnsafe;
				IJobExtensions.RunByRef(ref jobData2);
			}
		}
		static async UniTask UpdateAnalytics(StableObjectArray<BasePlayer> playerCache, NativeArray<int>.ReadOnly toBroadcast, NativeArray<CachedState>.ReadOnly cachedStates, NativeArray<Vector3>.ReadOnly playerPos, NativeArray<bool>.ReadOnly isMounted)
		{
			await UniTask.SwitchToThreadPool();
			using (TimeWarning.New("UpdateAnalytics"))
			{
				foreach (int item in toBroadcast)
				{
					BasePlayer player = playerCache.Objects[item];
					Vector3 pos = playerPos[item];
					CachedState tickState = cachedStates[item];
					Facepunch.Rust.Analytics.Azure.OnPlayerTick(player, pos, in tickState, isMounted[item]);
				}
			}
		}
		static async UniTask UpdateEAC(StableObjectArray<BasePlayer> playerCache, NativeArray<int>.ReadOnly validPlayers, NativeArray<CachedState>.ReadOnly cachedStates, NativeArray<EACTickState>.ReadOnly tickStates, NativeArray<PositionChange>.ReadOnly positionChanges)
		{
			await UniTask.SwitchToThreadPool();
			using (TimeWarning.New("EACStateUpdateJob"))
			{
				foreach (int item2 in validPlayers)
				{
					BasePlayer basePlayer = playerCache.Objects[item2];
					basePlayer.lastEACTickIndex = 0;
					if (positionChanges[item2] != PositionChange.Invalid)
					{
						CachedState cachedState = cachedStates[item2];
						for (int i = 0; i < (int)Player.clientTickRate; i++)
						{
							EACTickState tickState2 = tickStates[item2 * (int)Player.clientTickRate + i];
							if (tickState2.Timestamp == 0L)
							{
								break;
							}
							basePlayer.EACStateUpdate(in cachedState, in tickState2);
						}
					}
				}
			}
		}
	}

	private static void GatherPlayersToFinalize(in PlayerServerStates playerStates, float deltaTime, NativeList<int> indices)
	{
		using (TimeWarning.New("GatherPlayersToFinalize"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<float> span = playerStates.TickDeltaTime;
			Span<bool> span2 = playerStates.TickNeedsFinalizing;
			ReadOnlySpan<BasePlayer> readOnlySpan = objects;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				BasePlayer basePlayer = readOnlySpan[i];
				span[basePlayer.ActivePlayerInd] += deltaTime;
				if (!basePlayer.IsReceivingSnapshot && span2[basePlayer.ActivePlayerInd])
				{
					indices.AddNoResize(basePlayer.ActivePlayerInd);
					span2[basePlayer.ActivePlayerInd] = false;
				}
			}
		}
	}

	private static void GatherPlayersPosChanged(in PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly indicesToCheck, NativeArray<PositionChange>.ReadOnly posChanges, NativeList<int> indicesToGather)
	{
		using (TimeWarning.New("GatherPlayersPosChanged"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			TickInterpolatorCache.ReadOnlyState tickCache = playerStates.TickCache;
			foreach (int item in indicesToCheck)
			{
				if (posChanges[item] == PositionChange.Valid)
				{
					indicesToGather.AddNoResize(item);
					BasePlayer basePlayer = objects[item];
					Vector3 endPoint = TickInterpolatorCache.GetEndPoint(tickCache, item);
					basePlayer.transform.localPosition = endPoint;
					basePlayer.ticksPerSecond.Increment();
					basePlayer.tickHistory.AddPoint(endPoint, basePlayer.tickHistoryCapacity);
					basePlayer.RecordParentPosition(basePlayer.tickHistoryCapacity);
					AntiHack.FadeViolations(basePlayer, playerStates.TickDeltaTime[item]);
				}
			}
		}
	}

	private static void ServerCachePlayerInfo(in PlayerServerStates playerStates, NativeArray<int>.ReadOnly indices, bool recachePosDependentOnly)
	{
		using (TimeWarning.New("ServerCachePlayerInfo"))
		{
			using NativeList<int> foundDiff = new NativeList<int>(indices.Length, Allocator.TempJob);
			NativeArray<Vector3>.ReadOnly readOnly = playerStates.PlayerPos.AsReadOnly();
			NativeArray<Quaternion>.ReadOnly readOnly2 = playerStates.PlayerRots.AsReadOnly();
			DiffVec3Indirect diffVec3Indirect = default(DiffVec3Indirect);
			diffVec3Indirect.FoundDiff = foundDiff;
			diffVec3Indirect.A = playerStates.LastFramePlayerPos.AsReadOnly();
			diffVec3Indirect.B = readOnly;
			diffVec3Indirect.Indices = indices;
			DiffVec3Indirect jobData = diffVec3Indirect;
			IJobExtensions.RunByRef(ref jobData);
			CopyIndirect<Vector3> copyIndirect = default(CopyIndirect<Vector3>);
			copyIndirect.From = readOnly;
			copyIndirect.To = playerStates.LastFramePlayerPos;
			copyIndirect.Indices = foundDiff.AsReadOnly();
			CopyIndirect<Vector3> jobData2 = copyIndirect;
			IJobExtensions.RunByRef(ref jobData2);
			GetWaterFactors(in playerStates, foundDiff.AsReadOnly());
			BasePlayerJobs.UpdateWaterCache updateWaterCache = default(BasePlayerJobs.UpdateWaterCache);
			updateWaterCache.States = playerStates.CachedStates;
			updateWaterCache.Factors = playerStates.WaterFactors.AsReadOnly();
			updateWaterCache.Infos = playerStates.WaterInfos.AsReadOnly();
			updateWaterCache.Indices = foundDiff.AsReadOnly();
			BasePlayerJobs.UpdateWaterCache jobData3 = updateWaterCache;
			IJobExtensions.RunByRef(ref jobData3);
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<CachedState> span = playerStates.CachedStates;
			NativeArray<ModelState.Flag>.ReadOnly readOnly3 = playerStates.PlayerModelStateFlags.AsReadOnly();
			NativeArray<bool>.ReadOnly readOnly4 = playerStates.IsMounted.AsReadOnly();
			ReadOnlySpan<BaseMountable> readOnlySpan = playerStates.Mountables.Buffer;
			foreach (int item in foundDiff)
			{
				BasePlayer basePlayer = objects[item];
				ref CachedState reference = ref span[item];
				reference.EyePos = basePlayer.eyes.GetPos(readOnly[item], readOnly2[item], readOnly4[item], readOnlySpan[item]);
				bool ducked = (readOnly3[item] & ModelState.Flag.Ducked) != 0;
				reference.Center = basePlayer.GetCenter(ducked, readOnly[item]);
				reference.MovementModify = basePlayer.GetMovementModify();
				reference.IsOnLadder = basePlayer.onLadderCount > 0;
			}
			if (recachePosDependentOnly)
			{
				return;
			}
			foreach (int item2 in indices)
			{
				BasePlayer basePlayer2 = objects[item2];
				ref CachedState reference2 = ref span[item2];
				reference2.EyeRot = basePlayer2.eyes.rotation;
				reference2.PlayerFlags = basePlayer2.playerFlags;
				reference2.ModifiersMovementMultiplier = basePlayer2.GetModifiersMovementMultiplier();
				reference2.ClothingMoveSpeedReduction = basePlayer2.clothingMoveSpeedReduction;
				reference2.ClothingWaterSpeedBonus = basePlayer2.clothingWaterSpeedBonus;
				reference2.WeaponMoveSpeedScale = basePlayer2.weaponMoveSpeedScale;
			}
		}
	}

	private static void ServerPreFinalize(in PlayerServerStates playerStates, NativeArray<int>.ReadOnly indices)
	{
		using (TimeWarning.New("ServerPreFinalize"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			TickInterpolatorCache.ReadOnlyState readOnly = playerStates.TickCache.ReadOnly;
			foreach (int item in indices)
			{
				BasePlayer obj = objects[item];
				obj.rawTickCount = obj.rawTicksPerSecond.Calculate();
				obj.CheckModelState(in playerStates);
				obj.UpdateEstimatedVelocity(TickInterpolatorCache.GetStartPoint(readOnly, item), TickInterpolatorCache.GetEndPoint(readOnly, item), playerStates.TickDeltaTime[item]);
			}
		}
	}

	private static void ServerFinalizePlayers(in PlayerServerStates playerStates, NativeArray<PositionChange>.ReadOnly posChanges, NativeArray<int>.ReadOnly finalizeIndices, NativeList<int> toBroadcastIndices, NativeList<int> validIndices)
	{
		using (TimeWarning.New("ServerFinalizePlayers"))
		{
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			Span<CachedState> span = playerStates.CachedStates;
			TickInterpolatorCache tickCache = playerStates.TickCache;
			NativeArray<Vector3>.ReadOnly readOnly = playerStates.PlayerLocalPos.AsReadOnly();
			Span<float> span2 = playerStates.TickDeltaTime;
			foreach (int item in finalizeIndices)
			{
				BasePlayer basePlayer = objects[item];
				if (basePlayer.IsRealNull())
				{
					continue;
				}
				ref CachedState reference = ref span[item];
				Vector3 vector = readOnly[item];
				PositionChange num = posChanges[item];
				bool flag = num == PositionChange.Valid;
				if (num == PositionChange.Invalid && ConVar.AntiHack.forceposition)
				{
					basePlayer.ClientRPC(RpcTarget.Player("ForcePositionToParentOffset", basePlayer), vector, basePlayer.parentEntity.uid);
				}
				tickCache.Reset(basePlayer, vector);
				if (basePlayer.tickViewAngles != basePlayer.viewAngles)
				{
					basePlayer.viewAngles = basePlayer.tickViewAngles;
					if (!basePlayer.isMounted || !basePlayer.GetMounted().isMobile)
					{
						basePlayer.transform.rotation = Quaternion.identity;
					}
					basePlayer.transform.hasChanged = true;
					flag = true;
				}
				if (basePlayer.modelState != null)
				{
					basePlayer.modelState.waterLevel = reference.WaterFactor;
				}
				span2[item] = 0f;
				using (TimeWarning.New("AntiHack.EnforceViolations"))
				{
					AntiHack.ValidateEyeHistory(basePlayer);
				}
				if (flag)
				{
					basePlayer.eyes.NetworkUpdate(Quaternion.Euler(basePlayer.viewAngles));
					reference.EyePos = basePlayer.eyes.position;
					reference.EyeRot = basePlayer.eyes.rotation;
					reference.Center = basePlayer.GetCenter();
					toBroadcastIndices.AddNoResize(item);
					basePlayer.InvalidateNetworkCache();
				}
				validIndices.AddNoResize(item);
			}
		}
	}

	private static void NetworkPositionTick(in PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly toUpdate, float networkTime)
	{
		using (TimeWarning.New("NetworkPositionTick"))
		{
			NativeList<int> nativeList = new NativeList<int>(toUpdate.Length, Allocator.Temp);
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			foreach (int item in toUpdate)
			{
				BasePlayer basePlayer = objects[item];
				basePlayer.transform.hasChanged = false;
				if (Query.Server != null)
				{
					Query.Server.Move(basePlayer);
				}
				SingletonComponent<NpcFireManager>.Instance.Move(basePlayer);
				if (basePlayer.net != null)
				{
					if (!basePlayer.globalBroadcast && !ValidBounds.Test(basePlayer, playerStates.PlayerPos[item]))
					{
						basePlayer.OnInvalidPosition();
						continue;
					}
					basePlayer.TryScheduleUpdateNetworkGroup();
					nativeList.AddNoResize(item);
				}
			}
			SendNetworkPositions(in playerStates, nativeList.AsReadOnly(), networkTime);
			nativeList.Dispose();
		}
	}

	private static void SendNetworkPositions(in PlayerServerStates.ReadOnly playerStates, NativeArray<int>.ReadOnly indices, float networkTime)
	{
		if (Rust.Application.isLoading || Rust.Application.isLoadingSave)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkPositions"))
		{
			List<Network.Connection> obj = Facepunch.Pool.Get<List<Network.Connection>>();
			List<Network.Connection> obj2 = Facepunch.Pool.Get<List<Network.Connection>>();
			Network.Connection activeFakeConnection = RustRelay.ActiveFakeConnection;
			ReadOnlySpan<BasePlayer> objects = playerStates.PlayerCache.Objects;
			foreach (int item in indices)
			{
				BasePlayer basePlayer = objects[item];
				if (basePlayer.IsDestroyed || !basePlayer.isSpawned)
				{
					continue;
				}
				List<Network.Connection> list;
				if (ServerOcclusion.OcclusionEnabled)
				{
					list = basePlayer.unoccludedSubscribers;
				}
				else
				{
					list = basePlayer.GetSubscribers();
					if (list == null)
					{
						continue;
					}
				}
				if (list.Count > 0 && ConVar.AntiHack.stall_position_restrictions)
				{
					obj2.Clear();
					foreach (Network.Connection item2 in list)
					{
						if (!(item2.player as BasePlayer).isStalled)
						{
							obj2.Add(item2);
						}
					}
					list = obj2;
				}
				if (activeFakeConnection != null)
				{
					if (list.Count == 0)
					{
						list.Add(activeFakeConnection);
					}
					else if (list[0] != activeFakeConnection)
					{
						list.Add(list[0]);
						list[0] = activeFakeConnection;
					}
				}
				if (list.Count > 0)
				{
					SendPos(basePlayer, playerStates.PlayerLocalPos[item], basePlayer.viewAngles, networkTime, list);
				}
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
			Facepunch.Pool.FreeUnmanaged(ref obj2);
		}
		static void SendPos(BasePlayer player, Vector3 networkPos, Vector3 networkRotEuler, float networkTime, List<Network.Connection> dest)
		{
			player.LogEntry(RustLog.EntryType.Network, 3, "SendNetworkPositions");
			NetWrite netWrite = Network.Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityPosition);
			netWrite.EntityID(player.net.ID);
			netWrite.Vector3(in networkPos);
			netWrite.Vector3(in networkRotEuler);
			netWrite.Float(networkTime);
			NetworkableId uid = player.parentEntity.uid;
			if (uid.IsValid)
			{
				netWrite.EntityID(uid);
			}
			SendInfo info = new SendInfo(dest)
			{
				method = SendMethod.ReliableUnordered,
				priority = Priority.Immediate
			};
			netWrite.Send(info);
		}
	}

	public bool IsCraftingTutorialBlocked(ItemDefinition def, out bool forceUnlock)
	{
		forceUnlock = false;
		if (!IsInTutorial)
		{
			return false;
		}
		if (def.tutorialAllowance == TutorialItemAllowance.None)
		{
			return true;
		}
		bool num = CurrentTutorialAllowance >= def.tutorialAllowance;
		if (num && def.Blueprint != null && !def.Blueprint.defaultBlueprint)
		{
			forceUnlock = true;
		}
		return !num;
	}

	public bool CanModifyCraftAmountDuringTutorial()
	{
		if (IsInTutorial)
		{
			return CurrentTutorialAllowance >= TutorialItemAllowance.Level4_Spear_Fire;
		}
		return false;
	}

	public TutorialIsland GetCurrentTutorialIsland()
	{
		if (!IsInTutorial)
		{
			return null;
		}
		foreach (TutorialIsland tutorial in TutorialIsland.GetTutorialList(base.isServer))
		{
			if (tutorial.ForPlayer.Get(base.isServer) == this)
			{
				return tutorial;
			}
		}
		return null;
	}

	public void ClearTutorial()
	{
		SetPlayerFlag(PlayerFlags.IsInTutorial, b: false);
		SleepingBag.ClearTutorialBagsForPlayer(userID);
	}

	public void ClearTutorial_PostDeath()
	{
		ClearAllPings();
		ClearDeathMarker();
		PrepareMissionsForTutorial();
		SendPingsToClient();
		SendMarkersToClient();
	}

	public void OnStartedTutorial()
	{
		ClearAllPings();
		PrepareMissionsForTutorial();
	}

	public void SetTutorialAllowance(TutorialItemAllowance newAllowance)
	{
		if (newAllowance >= CurrentTutorialAllowance)
		{
			CurrentTutorialAllowance = newAllowance;
			SendNetworkUpdate();
		}
	}

	public void Server_FailActiveTutorialMission()
	{
		if (IsInTutorial && TryGetActiveMissionInstance(out var instance) && instance.GetMission() is TutorialMission)
		{
			AbandonActiveMission();
		}
	}

	[RPC_Server]
	private void StartTutorial(RPCMessage msg)
	{
		if (!(msg.player != this))
		{
			StartTutorial(triggerAnalytics: true);
		}
	}

	public void StartTutorial(bool triggerAnalytics)
	{
		if (ConVar.Server.tutorialEnabled)
		{
			if (!TutorialIsland.HasAvailableTutorialIsland)
			{
				ShowToast(GameTip.Styles.Red_Normal, TutorialIsland.NoTutorialIslandsAvailablePhrase, false);
			}
			else if (startTutorialCooldown > UnityEngine.Time.realtimeSinceStartup)
			{
				int num = Mathf.CeilToInt(startTutorialCooldown - UnityEngine.Time.realtimeSinceStartup);
				ShowToast(GameTip.Styles.Red_Normal, TutorialIsland.TutorialIslandStartCooldown, false, num.ToString());
			}
			else
			{
				startTutorialCooldown = UnityEngine.Time.realtimeSinceStartup + (float)Debugging.tutorial_start_cooldown;
				Hurt(99999f);
				Respawn();
				TutorialIsland.RestoreOrCreateIslandForPlayer(this, triggerAnalytics);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	private void PlayerRequestedTutorialStart(RPCMessage msg)
	{
		if (ConVar.Server.tutorialEnabled)
		{
			if (!TutorialIsland.HasAvailableTutorialIsland)
			{
				ShowToast(GameTip.Styles.Red_Normal, TutorialIsland.NoTutorialIslandsAvailablePhrase, false);
			}
			else
			{
				ClientRPC(RpcTarget.Player("PromptToStartTutorial", this));
			}
		}
	}

	public void UpdateGender()
	{
		isFemale = Underwear.IsFemale(this);
	}

	public uint GetUnderwearSkin(float time)
	{
		uint infoInt = (uint)GetInfoInt("client.underwearskin", 0);
		if (infoInt != lastValidUnderwearSkin && time > nextUnderwearValidationTime)
		{
			UnderwearManifest underwearManifest = UnderwearManifest.Get();
			nextUnderwearValidationTime = time + 0.2f;
			Underwear underwear = underwearManifest.GetUnderwear(infoInt);
			if (underwear == null)
			{
				lastValidUnderwearSkin = 0u;
			}
			else if (Underwear.Validate(underwear, this))
			{
				lastValidUnderwearSkin = infoInt;
			}
		}
		return lastValidUnderwearSkin;
	}

	[RPC_Server]
	public void ServerRPC_UnderwearChange(RPCMessage msg)
	{
		if (!(msg.player != this))
		{
			uint num = lastValidUnderwearSkin;
			uint underwearSkin = GetUnderwearSkin(UnityEngine.Time.time);
			if (num != underwearSkin)
			{
				SendNetworkUpdate();
			}
		}
	}

	public static int CompareByDisplayName(BasePlayer a, BasePlayer b)
	{
		return string.Compare(a.displayName, b.displayName, StringComparison.Ordinal);
	}

	public static void Server_SendWorldNotificationToAllActivePlayers(WorldNotificationConfig.NotificationType notificationType, Vector3 worldPosition)
	{
		if (!WorldNotificationConfig.instance.TryGetDataForMonumentType(notificationType, out var data))
		{
			Debug.LogError($"Failed to find notification data for monument type {notificationType}");
			return;
		}
		bool flag = PointEntity<DeepSeaManager>.ServerInstance != null;
		bool isEventInDeepSea = flag && DeepSeaManager.IsInsideDeepSea(worldPosition);
		for (int i = 0; i < activePlayerList.Count; i++)
		{
			activePlayerList[i].Server_SendWorldNotification(notificationType, worldPosition, data, flag, isEventInDeepSea);
		}
	}

	private void Server_SendWorldNotification(WorldNotificationConfig.NotificationType notificationType, Vector3 worldPosition, WorldNotificationConfig.Data notificationData, bool isDeepSeaManagerValid, bool isEventInDeepSea)
	{
		if (IsPlayerValidForNotification() && (isDeepSeaManagerValid && DeepSeaManager.IsInsideDeepSea(base.transform.position)) == isEventInDeepSea)
		{
			ClientRPC(RpcTarget.Player("Client_DoWorldNotification", this), (int)notificationType, worldPosition);
		}
	}

	public void Server_SendWorldNotification(WorldNotificationConfig.NotificationType notificationType, Vector3 worldPosition)
	{
		if (!IsPlayerValidForNotification())
		{
			return;
		}
		if (!WorldNotificationConfig.instance.TryGetDataForMonumentType(notificationType, out var _))
		{
			Debug.LogError($"Failed to find notification data for monument type {notificationType}");
			return;
		}
		bool num = PointEntity<DeepSeaManager>.ServerInstance != null;
		bool flag = num && DeepSeaManager.IsInsideDeepSea(base.transform.position);
		bool flag2 = num && DeepSeaManager.IsInsideDeepSea(worldPosition);
		if (flag == flag2)
		{
			ClientRPC(RpcTarget.Player("Client_DoWorldNotification", this), (int)notificationType, worldPosition);
		}
	}

	private bool IsPlayerValidForNotification()
	{
		if (!IsNpc && IsConnected && !IsSleeping())
		{
			return !IsInTutorial;
		}
		return false;
	}

	public bool IsWounded()
	{
		return HasPlayerFlag(PlayerFlags.Wounded);
	}

	public bool IsCrawling()
	{
		return IsCrawling(playerFlags);
	}

	public static bool IsCrawling(PlayerFlags flags)
	{
		if (HasPlayerFlag(flags, PlayerFlags.Wounded))
		{
			return !HasPlayerFlag(flags, PlayerFlags.Incapacitated);
		}
		return false;
	}

	public bool IsIncapacitated()
	{
		return HasPlayerFlag(PlayerFlags.Incapacitated);
	}

	public bool WoundInsteadOfDying(HitInfo info)
	{
		if (!EligibleForWounding(info))
		{
			return false;
		}
		BecomeWounded(info);
		return true;
	}

	public void ResetWoundingVars()
	{
		CancelInvoke(WoundingTick);
		woundedDuration = 0f;
		lastWoundedStartTime = float.NegativeInfinity;
		healingWhileCrawling = 0f;
		woundedByFallDamage = false;
	}

	public virtual bool EligibleForWounding(HitInfo info)
	{
		object obj = Interface.CallHook("CanBeWounded", this, info);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!ConVar.Server.woundingenabled)
		{
			return false;
		}
		if (IsWounded())
		{
			return false;
		}
		if (IsSleeping())
		{
			return false;
		}
		if (isMounted)
		{
			return false;
		}
		if (info == null)
		{
			return false;
		}
		if (!IsWounded() && UnityEngine.Time.realtimeSinceStartup - lastWoundedStartTime < ConVar.Server.rewounddelay)
		{
			return false;
		}
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode && !activeGameMode.allowWounding)
		{
			return false;
		}
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				if (triggers[i] is IHurtTrigger)
				{
					return false;
				}
			}
		}
		if (info.WeaponPrefab is BaseMelee)
		{
			return true;
		}
		if (info.WeaponPrefab is BaseProjectile)
		{
			return !info.isHeadshot;
		}
		switch (info.damageTypes.GetMajorityDamageType())
		{
		case DamageType.Suicide:
			return false;
		case DamageType.Fall:
			return true;
		case DamageType.Bite:
			return true;
		case DamageType.Bleeding:
			return true;
		case DamageType.Hunger:
			return true;
		case DamageType.Thirst:
			return true;
		case DamageType.Poison:
			return true;
		default:
		{
			if (BaseNetworkableEx.Is<BaseNPC2>(info.Initiator, out var castedUnityObject) && !castedUnityObject.IsAnimal)
			{
				return true;
			}
			return false;
		}
		}
	}

	public void BecomeWounded(HitInfo info)
	{
		if (IsWounded() || Interface.CallHook("OnPlayerWound", this, info) != null)
		{
			return;
		}
		bool flag = info != null && info.damageTypes.GetMajorityDamageType() == DamageType.Fall;
		if (IsCrawling())
		{
			woundedByFallDamage |= flag;
			GoToIncapacitated(info);
			return;
		}
		woundedByFallDamage = flag;
		if (flag || !ConVar.Server.crawlingenabled)
		{
			GoToIncapacitated(info);
		}
		else
		{
			GoToCrawling(info);
		}
	}

	public void StopWounded(BasePlayer source = null)
	{
		if (IsWounded())
		{
			RecoverFromWounded();
			CancelInvoke(WoundingTick);
			EACServer.LogPlayerRevive(source, this);
			PlayerInjureState = GetInjureState();
		}
	}

	public void ProlongWounding(float delay)
	{
		if (!IsRestrained)
		{
			woundedDuration = Mathf.Max(woundedDuration, Mathf.Min(TimeSinceWoundedStarted + delay, woundedDuration + delay));
			SendWoundedInformation(woundedDuration);
		}
	}

	public void SendWoundedInformation(float timeLeft)
	{
		float recoveryChance = GetRecoveryChance();
		ClientRPC(RpcTarget.Player("CLIENT_GetWoundedInformation", this), recoveryChance, timeLeft, woundedDuration);
	}

	public float GetRecoveryChance()
	{
		float num = (IsIncapacitated() ? ConVar.Server.incapacitatedrecoverchance : ConVar.Server.woundedrecoverchance);
		float num2 = Mathf.Lerp(t: (metabolism.hydration.Fraction() + metabolism.calories.Fraction()) / 2f, a: 0f, b: ConVar.Server.woundedmaxfoodandwaterbonus);
		float result = Mathf.Clamp01(num + num2);
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("largemedkit");
		if (inventory.containerBelt.FindItemByItemID(itemDefinition.itemid) != null && !woundedByFallDamage)
		{
			return 1f;
		}
		return result;
	}

	public void WoundingTick()
	{
		using (TimeWarning.New("WoundingTick"))
		{
			if (IsDead())
			{
				return;
			}
			if (!Player.woundforever && TimeSinceWoundedStarted >= woundedDuration)
			{
				float num = (IsIncapacitated() ? ConVar.Server.incapacitatedrecoverchance : ConVar.Server.woundedrecoverchance);
				float num2 = Mathf.Lerp(t: (metabolism.hydration.Fraction() + metabolism.calories.Fraction()) / 2f, a: 0f, b: ConVar.Server.woundedmaxfoodandwaterbonus);
				float num3 = Mathf.Clamp01(num + num2);
				if (UnityEngine.Random.value < num3)
				{
					RecoverFromWounded();
					return;
				}
				if (woundedByFallDamage)
				{
					Die();
					return;
				}
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition("largemedkit");
				Item item = inventory.containerBelt.FindItemByItemID(itemDefinition.itemid);
				if (item != null)
				{
					item.UseItem();
					RecoverFromWounded();
				}
				else
				{
					Die();
				}
			}
			else
			{
				if (IsSwimming() && IsCrawling())
				{
					GoToIncapacitated(null);
				}
				Invoke(WoundingTick, 1f);
			}
		}
	}

	public void GoToCrawling(HitInfo info)
	{
		base.health = UnityEngine.Random.Range(ConVar.Server.crawlingminimumhealth, ConVar.Server.crawlingmaximumhealth);
		metabolism.bleeding.value = 0f;
		healingWhileCrawling = 0f;
		WoundedStartSharedCode(info);
		StartWoundedTick(40, 50);
		SendWoundedInformation(woundedDuration);
		SendNetworkUpdateImmediate();
		PlayerInjureState = GetInjureState();
		RefreshColliderSize(forced: true);
	}

	public void GoToIncapacitated(HitInfo info)
	{
		if (!IsWounded())
		{
			WoundedStartSharedCode(info);
		}
		base.health = UnityEngine.Random.Range(2f, 6f);
		metabolism.bleeding.value = 0f;
		healingWhileCrawling = 0f;
		SetPlayerFlag(PlayerFlags.Incapacitated, b: true);
		SetServerFall(wantsOn: true);
		StartWoundedTick(10, 25);
		SendWoundedInformation(woundedDuration);
		SendNetworkUpdateImmediate();
		PlayerInjureState = GetInjureState();
		RefreshColliderSize(forced: true);
	}

	public void WoundedStartSharedCode(HitInfo info)
	{
		stats.Add("wounded", 1, (Stats)5);
		SetPlayerFlag(PlayerFlags.Wounded, b: true);
		if ((bool)BaseGameMode.GetActiveGameMode(base.isServer))
		{
			BaseGameMode.GetActiveGameMode(base.isServer).OnPlayerWounded(info.InitiatorPlayer, this, info);
		}
		inventory.DropBackpackOnDeath(wounded: true);
	}

	public void StartWoundedTick(int minTime, int maxTime)
	{
		woundedDuration = UnityEngine.Random.Range(minTime, maxTime + 1);
		ApplyWoundedStartTime();
		Invoke(WoundingTick, 1f);
	}

	public void ApplyWoundedStartTime()
	{
		lastWoundedStartTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public void RecoverFromWounded()
	{
		if (Interface.CallHook("OnPlayerRecover", this) == null)
		{
			if (IsCrawling())
			{
				base.health = UnityEngine.Random.Range(2f, 6f) + healingWhileCrawling;
			}
			healingWhileCrawling = 0f;
			SetPlayerFlag(PlayerFlags.Wounded, b: false);
			SetPlayerFlag(PlayerFlags.Incapacitated, b: false);
			if ((bool)BaseGameMode.GetActiveGameMode(base.isServer))
			{
				BaseGameMode.GetActiveGameMode(base.isServer).OnPlayerRevived(null, this);
			}
			Interface.CallHook("OnPlayerRecovered", this);
			RefreshColliderSize(forced: true);
		}
	}

	public bool WoundingCausingImmortality(HitInfo info)
	{
		if (!IsWounded())
		{
			return false;
		}
		if (TimeSinceWoundedStarted > 0.25f)
		{
			return false;
		}
		if (info != null && info.damageTypes.GetMajorityDamageType() == DamageType.Fall)
		{
			return false;
		}
		return true;
	}

	public InjureState GetInjureState()
	{
		if (IsDead())
		{
			return InjureState.Dead;
		}
		if (IsIncapacitated())
		{
			return InjureState.Incapacitated;
		}
		if (IsCrawling())
		{
			return InjureState.Crawling;
		}
		return InjureState.Normal;
	}

	public virtual void OnMedicalToolApplied(BasePlayer fromPlayer, ItemDefinition itemDef, ItemModConsumable consumable, MedicalTool medicalToolEntity, bool canRevive)
	{
		if (fromPlayer != this && IsWounded() && canRevive)
		{
			if (Interface.CallHook("OnPlayerRevive", fromPlayer, this) != null)
			{
				return;
			}
			StopWounded(fromPlayer);
		}
		foreach (ItemModConsumable.ConsumableEffect effect in consumable.effects)
		{
			if (effect.type == MetabolismAttribute.Type.Health)
			{
				base.health += effect.amount;
				ProcessMissionEvent(BaseMission.MissionEventType.HEAL, medicalToolEntity.prefabID, effect.amount);
			}
			else
			{
				metabolism.ApplyChange(effect.type, effect.amount, effect.time);
			}
		}
	}

	public override BasePlayer ToPlayer()
	{
		return this;
	}

	public static string SanitizePlayerNameString(string playerName, ulong userId)
	{
		playerName = playerName.ToPrintable(32).EscapeRichText().Trim();
		if (string.IsNullOrWhiteSpace(playerName))
		{
			playerName = userId.ToString();
		}
		return playerName;
	}

	public bool IsGod()
	{
		if (base.isServer && (IsAdmin || IsDeveloper) && IsConnected && net.connection != null && net.connection.info.GetBool("global.god"))
		{
			return true;
		}
		return false;
	}

	public override Quaternion GetNetworkRotation()
	{
		if (base.isServer)
		{
			return Quaternion.Euler(viewAngles);
		}
		return Quaternion.identity;
	}

	public bool CanInteract()
	{
		return CanInteract(usableWhileCrawling: false);
	}

	public bool CanInteract(bool usableWhileCrawling)
	{
		if (IsTransferProtected())
		{
			return false;
		}
		bool flag = CurrentGestureIsSurrendering;
		if (!flag && IsRestrained)
		{
			Handcuffs restraintItem = Belt.GetRestraintItem();
			flag = restraintItem != null && restraintItem.BlockUse;
		}
		if (!IsDead() && !IsSleeping() && !IsSpectating() && (usableWhileCrawling ? (!IsIncapacitated()) : (!IsWounded())) && !HasActiveTelephone)
		{
			return !flag;
		}
		return false;
	}

	public override float StartHealth()
	{
		return UnityEngine.Random.Range(50f, 60f);
	}

	public override float StartMaxHealth()
	{
		return 100f;
	}

	public override float MaxHealth()
	{
		if (maxHealthOverride > 0f)
		{
			return maxHealthOverride;
		}
		return _maxHealth * (1f + ((modifiers != null) ? modifiers.GetValue(Modifier.ModifierType.Max_Health) : 0f));
	}

	public override float AntiHackVelocity()
	{
		if (IsSleeping())
		{
			return 0f;
		}
		if (isMounted)
		{
			return GetMounted().AntiHackVelocity();
		}
		return GetMaxSpeed();
	}

	public override float AntiHackPadding()
	{
		if (isMounted)
		{
			return GetMounted().AntiHackPadding();
		}
		if (IsSleeping())
		{
			return 0.6f;
		}
		return base.AntiHackPadding();
	}

	public override OBB WorldSpaceBounds()
	{
		if (IsSleeping())
		{
			Vector3 center = bounds.center;
			Vector3 size = bounds.size;
			center.y /= 2f;
			size.y /= 2f;
			return new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, new Bounds(center, size));
		}
		return base.WorldSpaceBounds();
	}

	public Vector3 GetMountVelocity()
	{
		BaseMountable baseMountable = GetMounted();
		if (!(baseMountable != null))
		{
			return Vector3.zero;
		}
		return baseMountable.GetWorldVelocity();
	}

	public override Vector3 GetInheritedProjectileVelocity(Vector3 direction)
	{
		BaseMountable baseMountable = GetMounted();
		if (!baseMountable)
		{
			return base.GetInheritedProjectileVelocity(direction);
		}
		return baseMountable.GetInheritedProjectileVelocity(direction);
	}

	public override Vector3 GetInheritedThrowVelocity(Vector3 direction)
	{
		BaseMountable baseMountable = GetMounted();
		if (!baseMountable)
		{
			return base.GetInheritedThrowVelocity(direction);
		}
		return baseMountable.GetInheritedThrowVelocity(direction);
	}

	public override Vector3 GetInheritedDropVelocity()
	{
		BaseMountable baseMountable = GetMounted();
		if (!baseMountable)
		{
			return base.GetInheritedDropVelocity();
		}
		return baseMountable.GetInheritedDropVelocity();
	}

	public override void PreInitShared()
	{
		base.PreInitShared();
		cachedProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		baseProtection = ScriptableObject.CreateInstance<ProtectionProperties>();
		protectionAgainstNPCs = ScriptableObject.CreateInstance<ProtectionProperties>();
		inventoryValue.Set(GetComponent<PlayerInventory>());
		blueprints = GetComponent<PlayerBlueprints>();
		metabolism = GetComponent<PlayerMetabolism>();
		modifiers = GetComponent<PlayerModifiers>();
		colliderValue.Set(GetComponent<CapsuleCollider>());
		eyesValue.Set(GetComponent<PlayerEyes>());
		playerColliderStanding = new CapsuleColliderInfo(playerCollider.height, playerCollider.radius, playerCollider.center);
		playerColliderDucked = new CapsuleColliderInfo(1.5f, playerCollider.radius, Vector3.up * 0.75f);
		playerColliderCrawling = new CapsuleColliderInfo(playerCollider.radius, playerCollider.radius, Vector3.up * playerCollider.radius);
		playerColliderLyingDown = new CapsuleColliderInfo(0f, playerCollider.radius - 0.1f, Vector3.up * (playerCollider.radius - 0.1f));
		Belt = new PlayerBelt(this);
	}

	public override void DestroyShared()
	{
		RustNavigation.RemoveDrawViewer(this);
		UnityEngine.Object.Destroy(cachedProtection);
		UnityEngine.Object.Destroy(baseProtection);
		base.DestroyShared();
	}

	public override void ResetState()
	{
		base.ResetState();
		if (eyesValue != null)
		{
			eyesValue.Dispose();
			eyesValue = null;
		}
		if (inventoryValue != null)
		{
			inventoryValue.Dispose();
			inventoryValue = null;
		}
		if (colliderValue != null)
		{
			colliderValue.Dispose();
			colliderValue = null;
		}
	}

	public override bool InSafeZone()
	{
		if (base.isServer)
		{
			return base.InSafeZone();
		}
		return false;
	}

	public bool IsInNoRespawnZone()
	{
		if (base.isServer)
		{
			return InNoRespawnZone();
		}
		return false;
	}

	public bool IsOnATugboat()
	{
		if (GetMountedVehicle() is Tugboat)
		{
			return true;
		}
		if (GetParentEntity() is Tugboat)
		{
			return true;
		}
		return false;
	}

	public bool IsInAHelicopter()
	{
		if (GetMountedVehicle() is BaseHelicopter)
		{
			return true;
		}
		if (GetParentEntity() is BaseHelicopter)
		{
			return true;
		}
		return false;
	}

	public static void ServerCycle(float deltaTime)
	{
		CleanNulls(activePlayerList, ref PlayerStates);
		ServerUpdateParallel(deltaTime, in PlayerStates);
		try
		{
			using (TimeWarning.New("BasePlayer.BotColliderWorkQueue"))
			{
				botColliderWorkQueue.RunList(botColliderFrameBudgetMs);
			}
		}
		catch (Exception exception)
		{
			Debug.LogWarning("Server Exception: BasePlayer.BotColliderWorkQueue");
			Debug.LogException(exception);
		}
		static void CleanNulls(ListHashSet<BasePlayer> players, ref PlayerServerStates playerStates)
		{
			using (TimeWarning.New("CleanNulls"))
			{
				for (int i = 0; i < activePlayerList.Values.Count; i++)
				{
					BasePlayer basePlayer = activePlayerList[i];
					if (basePlayer == null)
					{
						activePlayerList.RemoveAt(i--);
						RemoveFromPlayerCache(basePlayer, ref playerStates);
					}
				}
			}
		}
	}

	private bool ManuallyCheckSafezone()
	{
		if (!base.isServer)
		{
			return false;
		}
		if (BaseGameMode.TryGetActiveGameMode(base.isServer, out var gameMode) && !gameMode.safeZone)
		{
			return false;
		}
		List<Collider> obj = Facepunch.Pool.Get<List<Collider>>();
		Vis.Colliders(base.transform.position, 0f, obj);
		bool result = false;
		foreach (Collider item in obj)
		{
			if (item.GetComponent<TriggerSafeZone>() != null)
			{
				result = true;
				continue;
			}
			TriggerSafeZoneOverride component = item.GetComponent<TriggerSafeZoneOverride>();
			if (!(component != null) || !component.IsCombatActive)
			{
				continue;
			}
			result = false;
			break;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return result;
	}

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (InSafeCombatZone())
		{
			if (!ApartmentRoom.ArePlayersInsideSameHostileRoom(baseEntity, this))
			{
				return false;
			}
		}
		else if ((!Player.adminsafezonelooting || !baseEntity.IsAdmin) && (baseEntity.InSafeZone() || InSafeZone() || ManuallyCheckSafezone()) && (ulong)baseEntity.userID != (ulong)userID)
		{
			return false;
		}
		if (RelationshipManager.ServerInstance != null)
		{
			if ((IsSleeping() || IsIncapacitated()) && !RelationshipManager.ServerInstance.HasRelations(baseEntity.userID, userID))
			{
				RelationshipManager.ServerInstance.SetRelationship(baseEntity, this, RelationshipManager.RelationshipType.Acquaintance);
			}
			RelationshipManager.ServerInstance.SetSeen(baseEntity, this);
		}
		if (IsCrawling())
		{
			GoToIncapacitated(null);
		}
		if (inventory.crafting != null)
		{
			inventory.crafting.CancelAll();
		}
		return base.OnStartBeingLooted(baseEntity);
	}

	public Bounds GetBounds(bool ducked)
	{
		return new Bounds(base.transform.position + GetOffset(ducked), GetSize(ducked));
	}

	public Bounds GetBounds()
	{
		return GetBounds(modelState.ducked);
	}

	public Vector3 GetCenter(bool ducked)
	{
		return GetCenter(ducked, base.transform.position);
	}

	public Vector3 GetCenter(bool ducked, Vector3 pos)
	{
		return pos + GetOffset(ducked);
	}

	public Vector3 GetOcclusionOffset()
	{
		return base.transform.position + PlayerEyes.EyeOffset;
	}

	public Vector3 GetCenter()
	{
		return GetCenter(modelState.ducked);
	}

	public static Vector3 GetOffset(bool ducked)
	{
		if (ducked)
		{
			return new Vector3(0f, 0.55f, 0f);
		}
		return new Vector3(0f, 0.9f, 0f);
	}

	public Vector3 GetOffset()
	{
		return GetOffset(modelState.ducked);
	}

	public static Vector3 GetSize(bool ducked)
	{
		if (ducked)
		{
			return new Vector3(1f, 1.1f, 1f);
		}
		return new Vector3(1f, 1.8f, 1f);
	}

	public Vector3 GetSize()
	{
		return GetSize(modelState.ducked);
	}

	public static float GetHeight(bool ducked)
	{
		if (ducked)
		{
			return 1.1f;
		}
		return 1.8f;
	}

	public float GetHeight()
	{
		return GetHeight(modelState.ducked);
	}

	public static float GetRadius()
	{
		return 0.5f;
	}

	public static float GetJumpHeight()
	{
		return 1.5f;
	}

	public override Vector3 TriggerPoint()
	{
		return base.transform.position + NoClipOffset();
	}

	public static Vector3 NoClipOffset()
	{
		return new Vector3(0f, GetHeight(ducked: true) - GetRadius(), 0f);
	}

	public static float NoClipRadius(float margin)
	{
		return GetRadius() - margin;
	}

	public float MaxDeployDistance(Item item)
	{
		return 8f;
	}

	public float GetMinSpeed()
	{
		return GetSpeed(0f, 0f, 1f);
	}

	public float GetMaxSpeed()
	{
		return GetSpeed(1f, 0f, 0f);
	}

	public float GetSpeed(float running, float ducking, float crawling)
	{
		return GetSpeed(running, ducking, crawling, IsSwimming());
	}

	public float GetSpeed(bool includeMovementModify, float running, float ducking, float crawling)
	{
		return GetSpeed(running, ducking, crawling, IsSwimming(), includeMovementModify);
	}

	public float GetSpeed(float running, float ducking, float crawling, bool isSwimming, bool includeMovementModify = true)
	{
		float num = 1f;
		MovementModify movementModify = GetMovementModify();
		num -= clothingMoveSpeedReduction;
		if (isSwimming)
		{
			num += clothingWaterSpeedBonus;
		}
		if (crawling > 0f)
		{
			return Mathf.Lerp(2.8f, 0.72f, crawling) * num * GetModifiersMovementMultiplier();
		}
		float num2 = Mathf.Lerp(Mathf.Lerp(2.8f, 5.5f, running), 1.7f, ducking) * num * weaponMoveSpeedScale * GetModifiersMovementMultiplier();
		if (!includeMovementModify)
		{
			return num2;
		}
		if (!isSwimming)
		{
			return Mathf.Lerp(num2, 0f, Mathf.Max(movementModify.drag, clothingMoveSpeedReduction));
		}
		return num2;
	}

	private float GetModifiersMovementMultiplier()
	{
		float num = ((modifiers != null) ? modifiers.GetValue(Modifier.ModifierType.MoveSpeed) : 0f);
		return 1f + num;
	}

	public override void OnAttacked(HitInfo info)
	{
		if (Interface.CallHook("IOnBasePlayerAttacked", this, info) != null)
		{
			return;
		}
		float oldHealth = base.health;
		if (base.isServer)
		{
			if (InSafeCombatZone())
			{
				if (!ApartmentRoom.ArePlayersInsideSameHostileRoom(info.InitiatorPlayer, this) && info.Initiator != null)
				{
					info.damageTypes.ScaleAll(0f);
				}
			}
			else if (InSafeZone() && !IsHostile() && info.Initiator != null && info.Initiator != this)
			{
				info.damageTypes.ScaleAll(0f);
			}
		}
		if (base.isServer)
		{
			HitArea boneArea = info.boneArea;
			if (boneArea != (HitArea)(-1))
			{
				List<Item> obj = Facepunch.Pool.Get<List<Item>>();
				obj.AddRange(inventory.containerWear.itemList);
				for (int i = 0; i < obj.Count; i++)
				{
					Item item = obj[i];
					if (item != null)
					{
						ItemModWearable component = item.info.GetComponent<ItemModWearable>();
						if (!(component == null) && component.ProtectsArea(boneArea))
						{
							item.OnAttacked(info);
						}
					}
				}
				Facepunch.Pool.Free(ref obj, freeElements: false);
				inventory.ServerUpdate(0f);
			}
		}
		base.OnAttacked(info);
		if (base.isServer && base.isServer && info.hasDamage)
		{
			if (!info.damageTypes.Has(DamageType.Bleeding) && info.damageTypes.IsBleedCausing() && !IsWounded() && !IsImmortalTo(info) && !info.damageTypes.Has(DamageType.BeeSting))
			{
				float num = ((modifiers != null) ? Mathf.Clamp01(1f - modifiers.GetValue(Modifier.ModifierType.Clotting)) : 1f);
				metabolism.bleeding.Add(info.damageTypes.Total() * 0.2f * num);
			}
			if (isMounted)
			{
				GetMounted().MounteeTookDamage(this, info);
			}
			CheckDeathCondition(info);
			if (net != null && net.connection != null)
			{
				ClientRPC(RpcTarget.Player("TakeDamageHit", this));
			}
			string text = StringPool.Get(info.HitBone);
			bool flag = Vector3.Dot((info.PointEnd - info.PointStart).normalized, eyes.BodyForward()) > 0.4f;
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if ((bool)initiatorPlayer && !info.damageTypes.IsMeleeType())
			{
				initiatorPlayer.LifeStoryShotHit(info.Weapon);
			}
			if (info.isHeadshot)
			{
				if (flag)
				{
					SignalBroadcast(Signal.Flinch_RearHead, string.Empty);
				}
				else
				{
					SignalBroadcast(Signal.Flinch_Head, string.Empty);
				}
				if (!initiatorPlayer || !initiatorPlayer.limitNetworking)
				{
					Effect.server.Run("assets/bundled/prefabs/fx/headshot.prefab", this, 0u, new Vector3(0f, 2f, 0f), Vector3.zero, (initiatorPlayer != null) ? initiatorPlayer.net.connection : null);
				}
				if ((bool)initiatorPlayer)
				{
					initiatorPlayer.stats.Add("headshot", 1, (Stats)5);
					if (initiatorPlayer.IsBeingSpectated)
					{
						ReadOnlySpan<BasePlayer> spectators = initiatorPlayer.GetSpectators();
						for (int j = 0; j < spectators.Length; j++)
						{
							BasePlayer basePlayer = spectators[j];
							basePlayer.ClientRPC(RpcTarget.Player("SpectatedPlayerHeadshot", basePlayer));
						}
					}
				}
			}
			else if (flag)
			{
				SignalBroadcast(Signal.Flinch_RearTorso, string.Empty);
			}
			else if (text == "spine" || text == "spine2")
			{
				SignalBroadcast(Signal.Flinch_Stomach, string.Empty);
			}
			else
			{
				SignalBroadcast(Signal.Flinch_Chest, string.Empty);
			}
		}
		if (stats != null)
		{
			if (IsWounded())
			{
				stats.combat.LogAttack(info, "wounded", oldHealth);
			}
			else if (IsDead())
			{
				stats.combat.LogAttack(info, "killed", oldHealth);
			}
			else
			{
				stats.combat.LogAttack(info, "", oldHealth);
			}
		}
		if (ConVar.Global.cinematicGingerbreadCorpses)
		{
			info.HitMaterial = ConVar.Global.GingerbreadMaterialID();
		}
	}

	public void EnablePlayerCollider()
	{
		if (!playerCollider.enabled && Interface.CallHook("OnPlayerColliderEnable", this, playerCollider) == null && !(base.isServer & isInvisible))
		{
			RefreshColliderSize(forced: true);
			playerCollider.enabled = true;
		}
	}

	public void DisablePlayerCollider()
	{
		if (playerCollider.enabled)
		{
			RemoveFromTriggers();
			playerCollider.enabled = false;
		}
	}

	public Bounds GetColliderBounds()
	{
		if (playerCollider == null)
		{
			return default(Bounds);
		}
		return playerCollider.bounds;
	}

	private void RefreshColliderSize(bool forced, bool? isSwimmingCached = null)
	{
		if (!(playerCollider == null) && (forced || (playerCollider.enabled && !(UnityEngine.Time.time < nextColliderRefreshTime))))
		{
			nextColliderRefreshTime = UnityEngine.Time.time + 0.25f + UnityEngine.Random.Range(-0.05f, 0.05f);
			BaseMountable baseMountable = GetMounted();
			CapsuleColliderInfo capsuleColliderInfo = ((baseMountable != null && baseMountable.IsValid()) ? ((!baseMountable.modifiesPlayerCollider) ? playerColliderStanding : baseMountable.customPlayerCollider) : ((!IsIncapacitated() && !IsSleeping()) ? (IsCrawling() ? playerColliderCrawling : ((!modelState.ducked && !(isSwimmingCached.HasValue ? isSwimmingCached.Value : IsSwimming())) ? playerColliderStanding : playerColliderDucked)) : playerColliderLyingDown));
			if (playerCollider.height != capsuleColliderInfo.height || playerCollider.radius != capsuleColliderInfo.radius || playerCollider.center != capsuleColliderInfo.center)
			{
				playerCollider.height = capsuleColliderInfo.height;
				playerCollider.radius = capsuleColliderInfo.radius;
				playerCollider.center = capsuleColliderInfo.center;
			}
		}
	}

	private void SetPlayerRigidbodyState(bool isEnabled)
	{
		if (isEnabled)
		{
			AddPlayerRigidbody();
		}
		else
		{
			RemovePlayerRigidbody();
		}
	}

	public void AddPlayerRigidbody()
	{
		if (playerRigidbody == null)
		{
			playerRigidbody = base.gameObject.GetComponent<Rigidbody>();
		}
		if (playerRigidbody == null)
		{
			playerRigidbody = base.gameObject.AddComponent<Rigidbody>();
			playerRigidbody.useGravity = false;
			playerRigidbody.isKinematic = true;
			playerRigidbody.mass = 1f;
			playerRigidbody.interpolation = RigidbodyInterpolation.None;
			playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}
	}

	public void RemovePlayerRigidbody()
	{
		if (playerRigidbody == null)
		{
			playerRigidbody = base.gameObject.GetComponent<Rigidbody>();
		}
		if (playerRigidbody != null)
		{
			RemoveFromTriggers();
			UnityEngine.Object.DestroyImmediate(playerRigidbody);
			playerRigidbody = null;
		}
	}

	public bool IsEnsnared()
	{
		if (triggers == null)
		{
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is TriggerEnsnare)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAttacking()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if (heldEntity == null)
		{
			return false;
		}
		AttackEntity attackEntity = heldEntity as AttackEntity;
		if (attackEntity == null)
		{
			return false;
		}
		return attackEntity.NextAttackTime - UnityEngine.Time.time > attackEntity.repeatDelay - 1f;
	}

	public bool CanAttack()
	{
		HeldEntity heldEntity = GetHeldEntity();
		if (heldEntity == null)
		{
			return false;
		}
		bool flag = IsSwimming();
		bool flag2 = heldEntity.CanBeUsedInWater();
		if (modelState.onLadder)
		{
			return false;
		}
		if (modelState.blocking)
		{
			return false;
		}
		if (!flag && !modelState.onground)
		{
			return false;
		}
		if (flag && !flag2)
		{
			return false;
		}
		if (IsEnsnared())
		{
			return false;
		}
		return true;
	}

	public bool OnLadder()
	{
		if (modelState.onLadder && !IsWounded())
		{
			return FindTrigger<TriggerLadder>();
		}
		return false;
	}

	public bool IsSwimming()
	{
		return IsSwimming(WaterFactor());
	}

	public static bool IsSwimming(float waterFactor)
	{
		return waterFactor >= 0.65f;
	}

	public bool IsHeadUnderwater()
	{
		return WaterFactor() > 0.75f;
	}

	public virtual bool IsOnGround()
	{
		return modelState.onground;
	}

	public bool IsRunning()
	{
		if (modelState != null)
		{
			return modelState.sprinting;
		}
		return false;
	}

	public bool IsDucked()
	{
		if (modelState != null)
		{
			return IsDucked(modelState.ducking);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDucked(float factor)
	{
		return factor > 0.5f;
	}

	public void ShowToast(GameTip.Styles style, Translate.Phrase phrase, bool overlay = false, params string[] arguments)
	{
		if (base.isServer)
		{
			SendConsoleCommand("gametip.showtoast_translated", (int)style, phrase.token, phrase.english, overlay, arguments);
		}
	}

	public void ShowBlockedByEntityToast(BaseEntity ent, Translate.Phrase fallbackError = null)
	{
		if (!(ent == null))
		{
			ClientRPC(RpcTarget.Player("CLIENT_ShowBlockedByToast", this), ent.net.ID, fallbackError.token, fallbackError.english);
		}
	}

	public void ChatMessage(string msg)
	{
		if (base.isServer && Interface.CallHook("OnMessagePlayer", msg, this) == null)
		{
			SendConsoleCommand("chat.add", 2, 0, msg);
		}
	}

	public void ConsoleMessage(string msg)
	{
		if (base.isServer)
		{
			SendConsoleCommand("echo " + msg);
		}
	}

	public override float PenetrationResistance(HitInfo info)
	{
		return 100f;
	}

	public override void ScaleDamage(HitInfo info)
	{
		if (isMounted)
		{
			GetMounted().ScaleDamageForPlayer(this, info);
		}
		if (info.UseProtection || info.UseProtectionForNPCs)
		{
			HitArea boneArea = info.boneArea;
			if (info.UseProtectionForNPCs)
			{
				info.damageTypes.Total();
				protectionAgainstNPCs.Scale(info.damageTypes);
			}
			else if (boneArea != (HitArea)(-1))
			{
				cachedProtection.Clear();
				cachedProtection.Add(inventory.containerWear.itemList, boneArea);
				cachedProtection.Multiply(DamageType.Arrow, ConVar.Server.arrowarmor);
				cachedProtection.Multiply(DamageType.Bullet, ConVar.Server.bulletarmor);
				cachedProtection.Multiply(DamageType.Slash, ConVar.Server.meleearmor);
				cachedProtection.Multiply(DamageType.Blunt, ConVar.Server.meleearmor);
				cachedProtection.Multiply(DamageType.Stab, ConVar.Server.meleearmor);
				cachedProtection.Multiply(DamageType.Bleeding, ConVar.Server.bleedingarmor);
				cachedProtection.Scale(info.damageTypes);
			}
			else
			{
				baseProtection.Scale(info.damageTypes);
			}
		}
		if ((bool)info.damageProperties)
		{
			info.damageProperties.ScaleDamage(info);
		}
		if (!IsNpc && info.InitiatorPlayer != null && !info.InitiatorPlayer.IsNpc)
		{
			info.damageTypes.Scale(DamageType.Bullet, ConVar.Server.pvpBulletDamageMultiplier);
		}
		if (IsNpc && info.InitiatorPlayer != null && !info.InitiatorPlayer.IsNpc)
		{
			info.damageTypes.Total();
			info.damageTypes.Scale(DamageType.Bullet, ConVar.Server.pveBulletDamageMultiplier);
		}
	}

	public void ResetWeaponMoveSpeedScale()
	{
		weaponMoveSpeedScale = 1f;
	}

	private void UpdateMoveSpeedFromClothing()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		bool flag = false;
		bool flag2 = false;
		float num4 = 0f;
		eggVision = 0f;
		base.Weight = 0f;
		foreach (Item item in inventory.containerWear.itemList)
		{
			ItemModWearable component = item.info.GetComponent<ItemModWearable>();
			if ((bool)component)
			{
				if (component.blocksAiming)
				{
					flag = true;
				}
				if (component.blocksEquipping)
				{
					flag2 = true;
				}
				num4 += component.accuracyBonus;
				eggVision += component.eggVision;
				base.Weight += component.weight;
				float num5 = 0f;
				float num6 = 0f;
				if (item.info.TryGetComponent<ItemModContainerArmorSlot>(out var component2))
				{
					num6 = component2.TotalSpeedReduction(item);
				}
				if (component.movementProperties != null)
				{
					num5 = component.movementProperties.speedReduction;
					num3 += component.movementProperties.waterSpeedBonus;
				}
				float num7 = num5 + num6;
				num = Mathf.Max(num, num7);
				num2 += num7;
			}
		}
		clothingAccuracyBonus = num4;
		clothingMoveSpeedReduction = Mathf.Max(num2, num);
		clothingBlocksAiming = flag;
		clothingWaterSpeedBonus = num3;
		equippingBlocked = flag2;
		if (base.isServer && equippingBlocked)
		{
			UpdateActiveItem(default(ItemId));
		}
		if (base.isServer && isMounted)
		{
			BaseVehicle mountedVehicle = GetMountedVehicle();
			if (mountedVehicle != null)
			{
				mountedVehicle.OnMountedPlayerWeightChanged(this);
			}
		}
	}

	public virtual void UpdateProtectionFromClothing()
	{
		baseProtection.Clear();
		baseProtection.Add(inventory.containerWear.itemList);
		float num = 1f / 6f;
		for (int i = 0; i < baseProtection.amounts.Length; i++)
		{
			switch (i)
			{
			case 22:
				baseProtection.amounts[i] = 1f;
				break;
			default:
				baseProtection.amounts[i] *= num;
				break;
			case 17:
			case 25:
				break;
			}
		}
		float value = baseProtection.amounts[17];
		baseProtection.amounts[17] = Mathf.Clamp(value, -1f, Radiation.MaxExposureProtection);
		if (!IsNpc)
		{
			baseProtection.amounts[16] = Mathf.Clamp(baseProtection.amounts[16], 0f, ConVar.Server.max_explosive_protection);
		}
		protectionAgainstNPCs.Clear();
		protectionAgainstNPCs.Add(inventory.containerWear.itemList, HitArea.Head);
		protectionAgainstNPCs.Add(inventory.containerWear.itemList, HitArea.Chest, 1.5f);
		protectionAgainstNPCs.Add(inventory.containerWear.itemList, HitArea.Leg, 0.5f);
		for (int j = 0; j < protectionAgainstNPCs.amounts.Length; j++)
		{
			protectionAgainstNPCs.amounts[j] /= 3f;
		}
	}

	public override string Categorize()
	{
		return "player";
	}

	public override string ToString()
	{
		if (_name == null)
		{
			if (base.isServer)
			{
				_name = $"{displayName}[{userID.Get()}]";
			}
			else
			{
				_name = base.ShortPrefabName;
			}
		}
		return _name;
	}

	public string GetDebugStatus()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("Entity: {0}\n", ToString());
		stringBuilder.AppendFormat("Name: {0}\n", displayName);
		stringBuilder.AppendFormat("SteamID: {0}\n", userID.Get());
		foreach (PlayerFlags value in Enum.GetValues(typeof(PlayerFlags)))
		{
			stringBuilder.AppendFormat("{1}: {0}\n", HasPlayerFlag(value), value);
		}
		return stringBuilder.ToString();
	}

	public override Item GetItem(ItemId itemId)
	{
		if (inventory == null)
		{
			return null;
		}
		return inventory.FindItemByUID(itemId);
	}

	public override float WaterFactor()
	{
		WaterLevel.WaterInfo info;
		return WaterFactor(out info);
	}

	public float WaterFactor(out WaterLevel.WaterInfo info)
	{
		if (GetMounted().IsValid())
		{
			return GetMounted().WaterFactorForPlayer(this, out info);
		}
		return GetUnmountedWaterFactor(out info);
	}

	public float GetUnmountedWaterFactor(out WaterLevel.WaterInfo info)
	{
		if (GetParentEntity() != null && GetParentEntity().BlocksWaterFor(this))
		{
			info = default(WaterLevel.WaterInfo);
			return 0f;
		}
		Vector3 vector = playerCollider.transform.TransformPoint(playerCollider.center);
		float radius = playerCollider.radius;
		float num = ((playerCollider.height <= 2f * radius || IsSleeping()) ? 0f : (playerCollider.height * 0.5f - radius));
		Vector3 start = vector - playerCollider.transform.up * num;
		Vector3 end = vector + playerCollider.transform.up * num;
		info = WaterLevel.GetWaterInfo(start, end, radius, waves: true, volumes: true, this);
		return WaterLevel.Factor(in info, start, end, radius);
	}

	public static void GetWaterFactors(in PlayerServerStates playerStates, NativeArray<int>.ReadOnly indices)
	{
		GetWaterFactors(playerStates.PlayerCache.UnsafeObjects, playerStates.PlayerPos.AsReadOnly(), playerStates.PlayerRots.AsReadOnly(), playerStates.IsMounted.AsReadOnly(), playerStates.Mountables.Buffer, indices, playerStates.WaterInfos, playerStates.WaterFactors);
	}

	public static void GetWaterFactors(BasePlayer[] playerCache, NativeArray<Vector3>.ReadOnly posi, NativeArray<Quaternion>.ReadOnly rots, NativeArray<int>.ReadOnly indices, NativeArray<WaterLevel.WaterInfo> infos, NativeArray<float> factors)
	{
		NativeArray<bool> source = new NativeArray<bool>(playerCache.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		try
		{
			BufferList<BaseMountable> obj = Facepunch.Pool.Get<BufferList<BaseMountable>>();
			if (obj.Capacity < playerCache.Length)
			{
				obj.Resize(playerCache.Length);
			}
			Span<bool> span = source;
			foreach (int item in indices)
			{
				BaseMountable baseMountable = playerCache[item].GetMounted();
				span[item] = (object)baseMountable != null;
				obj[item] = baseMountable;
			}
			GetWaterFactors(playerCache, posi, rots, source.AsReadOnly(), obj.Buffer, indices, infos, factors);
			Facepunch.Pool.FreeUnmanaged(ref obj);
		}
		finally
		{
			((IDisposable)source).Dispose();
		}
	}

	public static void GetWaterFactors(BasePlayer[] playerCache, NativeArray<Vector3>.ReadOnly posi, NativeArray<Quaternion>.ReadOnly rots, NativeArray<bool>.ReadOnly isMounted, ReadOnlySpan<BaseMountable> mountables, NativeArray<int>.ReadOnly indices, NativeArray<WaterLevel.WaterInfo> infos, NativeArray<float> factors)
	{
		using (TimeWarning.New("GetWaterFactors"))
		{
			ReadOnlySpan<BasePlayer> readOnlySpan = playerCache;
			NativeList<int> nativeList = new NativeList<int>(indices.Length, Allocator.TempJob);
			foreach (int item in indices)
			{
				BasePlayer basePlayer = readOnlySpan[item];
				if (isMounted[item])
				{
					factors[item] = mountables[item].WaterFactorForPlayer(basePlayer, out var info);
					infos[item] = info;
					continue;
				}
				BaseEntity baseEntity = basePlayer.GetParentEntity();
				if ((object)baseEntity != null && baseEntity.BlocksWaterFor(basePlayer))
				{
					infos[item] = default(WaterLevel.WaterInfo);
					factors[item] = 0f;
				}
				else
				{
					nativeList.AddNoResize(item);
				}
			}
			if (!nativeList.IsEmpty)
			{
				NativeArray<Vector3> starts = new NativeArray<Vector3>(readOnlySpan.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				NativeArray<Vector3> ends = new NativeArray<Vector3>(readOnlySpan.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				NativeArray<float> radii = new NativeArray<float>(readOnlySpan.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				NativeArray<int>.ReadOnly indices2 = nativeList.AsReadOnly();
				foreach (int item2 in indices2)
				{
					CapsuleCollider capsuleCollider = readOnlySpan[item2].playerCollider;
					starts[item2] = capsuleCollider.center;
					ends[item2] = new Vector2(capsuleCollider.radius, capsuleCollider.height);
				}
				GetWaterFactorsParamsJobIndirect getWaterFactorsParamsJobIndirect = default(GetWaterFactorsParamsJobIndirect);
				getWaterFactorsParamsJobIndirect.Starts = starts;
				getWaterFactorsParamsJobIndirect.Ends = ends;
				getWaterFactorsParamsJobIndirect.Radii = radii;
				getWaterFactorsParamsJobIndirect.Pos = posi;
				getWaterFactorsParamsJobIndirect.Rots = rots;
				getWaterFactorsParamsJobIndirect.Indices = indices2;
				GetWaterFactorsParamsJobIndirect jobData = getWaterFactorsParamsJobIndirect;
				IJobExtensions.RunByRef(ref jobData);
				WaterLevel.GetWaterInfos(starts.AsReadOnly(), ends.AsReadOnly(), radii.AsReadOnly(), new ReadOnlySpan<BaseEntity>(playerCache), indices2, waves: true, volumes: true, infos);
				CalcWaterFactorsJobIndirect calcWaterFactorsJobIndirect = default(CalcWaterFactorsJobIndirect);
				calcWaterFactorsJobIndirect.Factors = factors;
				calcWaterFactorsJobIndirect.Indices = indices2;
				calcWaterFactorsJobIndirect.Infos = infos.AsReadOnly();
				calcWaterFactorsJobIndirect.Starts = starts.AsReadOnly();
				calcWaterFactorsJobIndirect.Ends = ends.AsReadOnly();
				calcWaterFactorsJobIndirect.Radii = radii.AsReadOnly();
				CalcWaterFactorsJobIndirect jobData2 = calcWaterFactorsJobIndirect;
				IJobExtensions.RunByRef(ref jobData2);
				starts.Dispose();
				ends.Dispose();
				radii.Dispose();
			}
			nativeList.Dispose();
		}
	}

	public override float AirFactor()
	{
		float num = ((WaterFactor() >= 1f) ? 0f : 1f);
		BaseMountable baseMountable = GetMounted();
		if (baseMountable.IsValid() && baseMountable.BlocksWaterFor(this))
		{
			float num2 = baseMountable.AirFactor();
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public float GetOxygenTime(out ItemModGiveOxygen.AirSupplyType airSupplyType)
	{
		BaseVehicle mountedVehicle = GetMountedVehicle();
		if (mountedVehicle.IsValid() && mountedVehicle is IAirSupply airSupply)
		{
			float airTimeRemaining = airSupply.GetAirTimeRemaining(null);
			if (airTimeRemaining > 0f)
			{
				airSupplyType = airSupply.AirType;
				return airTimeRemaining;
			}
		}
		foreach (Item item in inventory.containerWear.itemList)
		{
			IAirSupply componentInChildren = item.info.GetComponentInChildren<IAirSupply>();
			if (componentInChildren != null)
			{
				float airTimeRemaining2 = componentInChildren.GetAirTimeRemaining(item);
				if (airTimeRemaining2 > 0f)
				{
					airSupplyType = componentInChildren.AirType;
					return airTimeRemaining2;
				}
			}
		}
		airSupplyType = ItemModGiveOxygen.AirSupplyType.Lungs;
		if (metabolism.oxygen.value > 0.5f)
		{
			float num = Mathf.InverseLerp(0.5f, 1f, metabolism.oxygen.value);
			return 5f * num;
		}
		return 0f;
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	public static bool AnyPlayersVisibleToEntity(Vector3 pos, float radius, BaseEntity source, Vector3 entityEyePos, bool ignorePlayersWithPriv = false)
	{
		List<RaycastHit> obj = Facepunch.Pool.Get<List<RaycastHit>>();
		List<BasePlayer> obj2 = Facepunch.Pool.Get<List<BasePlayer>>();
		Vis.Entities(pos, radius, obj2, 131072);
		bool flag = false;
		foreach (BasePlayer item in obj2)
		{
			if (item.IsSleeping() || !item.IsAlive() || (item.IsBuildingAuthed() && ignorePlayersWithPriv))
			{
				continue;
			}
			obj.Clear();
			GamePhysics.TraceAll(new Ray(item.eyes.position, (entityEyePos - item.eyes.position).normalized), 0f, obj, 9f, 1218519297);
			for (int i = 0; i < obj.Count; i++)
			{
				BaseEntity entity = RaycastHitEx.GetEntity(obj[i]);
				if (entity != null && (entity == source || entity.EqualNetID(source)))
				{
					flag = true;
					break;
				}
				if (!(entity != null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
		return flag;
	}

	public bool IsStandingOnEntity(BaseEntity standingOn, int layerMask)
	{
		BaseEntity standingOnEntity = GetStandingOnEntity(layerMask);
		if (standingOnEntity == null)
		{
			return false;
		}
		if (standingOnEntity.EqualNetID(standingOn))
		{
			return true;
		}
		BaseEntity baseEntity = standingOnEntity.GetParentEntity();
		if (baseEntity != null && baseEntity.EqualNetID(standingOn))
		{
			return true;
		}
		return false;
	}

	public BaseEntity GetStandingOnEntity(int layerMask)
	{
		if (!IsOnGround())
		{
			return null;
		}
		if (UnityEngine.Physics.SphereCast(base.transform.position + Vector3.up * (0.25f + GetRadius()), GetRadius() * 0.95f, Vector3.down, out var hitInfo, 4f, layerMask))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (entity != null)
			{
				return entity;
			}
		}
		return null;
	}

	public void SetActiveTelephone(PhoneController t)
	{
		activeTelephone = t;
		Interface.CallHook("OnActiveTelephoneUpdated", this, t);
	}

	public void ClearDesigningAIEntity()
	{
		if (IsDesigningAI)
		{
			designingAIEntity.GetComponent<global::IAIDesign>()?.StopDesigning();
		}
		designingAIEntity = null;
	}

	public static bool IsBotId(ulong id)
	{
		return id < 10000000;
	}

	public static void ReserveBotIds(List<ulong> usedIds)
	{
		usedIds.Sort();
		freeBotIds.Clear();
		ulong num = 1uL;
		foreach (ulong usedId in usedIds)
		{
			for (; num != usedId; num++)
			{
				freeBotIds.Add(num);
			}
			num = usedId + 1;
		}
		botIdCounter = num;
	}
}

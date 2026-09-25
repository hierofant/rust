#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Development.Attributes;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PlayerBoat : BaseBoat, Anchor.IAnchorable, TriggerHurtNotChild.IHurtTriggerUser, IReceiveDeepSeaNotifications, ILargeVehicleForProjectiles, IPlannerReparentChildrenToMe
{
	public struct DragByAngle
	{
		public float[] dragByDirectionOfTravel;

		public int directionIncrements;

		public PlayerBoat boat;

		private const int substeps = 16;

		private const int hitsPerRay = 24;

		public void Init(int angleIncrements)
		{
			using (TimeWarning.New("DragByAngle.Init"))
			{
				directionIncrements = angleIncrements;
				dragByDirectionOfTravel = new float[360 / directionIncrements];
				List<BoatBuildingBlock> obj = Facepunch.Pool.Get<List<BoatBuildingBlock>>();
				foreach (BoatBuildingBlock item in boat.BoatBuildingBlocks.Cached)
				{
					if ((bool)item && item.Hull)
					{
						obj.Add(item);
					}
				}
				if (obj.Count == 0)
				{
					Facepunch.Pool.FreeUnmanaged(ref obj);
					return;
				}
				Bounds bounds2 = new Bounds(obj[0].transform.localPosition, obj[0].bounds.size);
				for (int i = 1; i < obj.Count; i++)
				{
					obj[i].transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
					new OBB(localPosition, localRotation, obj[i].bounds).BoundsEncapsulate(ref bounds2);
				}
				OBB bounds = new OBB(boat.transform, bounds2);
				Vector3 forward = boat.transform.forward;
				JobHandle job = default(JobHandle);
				List<NativeList<SpherecastCommand>> obj2 = Facepunch.Pool.Get<List<NativeList<SpherecastCommand>>>();
				List<NativeArray<RaycastHit>> obj3 = Facepunch.Pool.Get<List<NativeArray<RaycastHit>>>();
				List<Plane> obj4 = Facepunch.Pool.Get<List<Plane>>();
				List<float> obj5 = Facepunch.Pool.Get<List<float>>();
				for (int j = 0; j < dragByDirectionOfTravel.Length; j++)
				{
					Vector3 vector = -(Quaternion.Euler(0f, j * directionIncrements, 0f) * forward);
					float num = ExtentInDirXZ(-vector);
					Vector3 vector2 = bounds.position + (-vector * (num + 5f)).WithY(bounds.position.y);
					Plane plane = new Plane(vector, vector2);
					job = JobHandle.CombineDependencies(job, SchedulePlaneProjection(plane, vector2, bounds, out var commands, out var hits, out var step));
					obj2.Add(commands);
					obj3.Add(hits);
					obj4.Add(plane);
					obj5.Add(step);
				}
				job.Complete();
				job = default(JobHandle);
				for (int k = 0; k < obj2.Count; k++)
				{
					GamePhysics.VerifySpheres(obj3[k], obj2[k].AsArray(), 24);
					job = JobHandle.CombineDependencies(job, GamePhysics.SortDeferred(obj3[k], obj2[k].Length, 24));
				}
				job.Complete();
				for (int l = 0; l < obj2.Count; l++)
				{
					dragByDirectionOfTravel[l] = GetDragFromHits(obj2[l].AsArray(), obj3[l], obj4[l], obj5[l]);
					obj2[l].Dispose();
					obj3[l].Dispose();
				}
				Facepunch.Pool.FreeUnmanaged(ref obj);
				Facepunch.Pool.FreeUnmanaged(ref obj2);
				Facepunch.Pool.FreeUnmanaged(ref obj3);
				Facepunch.Pool.FreeUnmanaged(ref obj4);
				Facepunch.Pool.FreeUnmanaged(ref obj5);
				float ExtentInDirXZ(Vector3 dir)
				{
					return Mathf.Abs(Vector3.Dot(dir, bounds.right.XZ().normalized)) * bounds.extents.x + Mathf.Abs(Vector3.Dot(dir, bounds.forward.XZ().normalized)) * bounds.extents.z;
				}
			}
		}

		private JobHandle SchedulePlaneProjection(Plane plane, Vector3 planePoint, OBB worldBounds, out NativeList<SpherecastCommand> commands, out NativeArray<RaycastHit> hits, out float step)
		{
			using (TimeWarning.New("SchedulePlaneProjection"))
			{
				Vector3 normal = plane.normal;
				Vector3 normalized = Vector3.Cross(Vector3.up, normal).normalized;
				float num = float.MaxValue;
				float num2 = float.MinValue;
				for (int i = -1; i <= 1; i += 2)
				{
					for (int j = -1; j <= 1; j += 2)
					{
						for (int k = -1; k <= 1; k += 2)
						{
							Vector3 vector = Vector3.Scale(new Vector3(i, j, k), worldBounds.extents);
							float num3 = Vector3.Dot(worldBounds.position + worldBounds.rotation * vector - worldBounds.position, normalized);
							if (num3 < num)
							{
								num = num3;
							}
							if (num3 > num2)
							{
								num2 = num3;
							}
						}
					}
				}
				commands = new NativeList<SpherecastCommand>(16, Allocator.TempJob);
				QueryParameters queryParameters = new QueryParameters(134217728, hitMultipleFaces: false, QueryTriggerInteraction.Ignore);
				float num4 = num2 - num;
				step = num4 / 16f;
				int num5 = 0;
				for (float num6 = num; num6 <= num2; num6 += step)
				{
					if (num5 >= 16)
					{
						break;
					}
					SpherecastCommand value = new SpherecastCommand(planePoint + normalized * num6, step * 0.5f, normal, queryParameters, worldBounds.extents.Max() * 3f);
					num5++;
					commands.Add(in value);
				}
				hits = new NativeArray<RaycastHit>(commands.Length * 24, Allocator.TempJob);
				return SpherecastCommand.ScheduleBatch(commands, hits, 4, 24);
			}
		}

		private float GetDragFromHits(NativeArray<SpherecastCommand> commands, NativeArray<RaycastHit> hits, Plane plane, float step)
		{
			using (TimeWarning.New("GetDragFromHits"))
			{
				float num = 0f;
				for (int i = 0; i < commands.Length; i++)
				{
					int num2 = i * 24;
					for (int j = num2; j < num2 + 24; j++)
					{
						RaycastHit hit = hits[j];
						if (RaycastHitEx.GetEntity(hit) is BoatBuildingBlock { Hull: not false } boatBuildingBlock && boatBuildingBlock.GetParentEntity() == boat)
						{
							float num3 = Vector3.Dot(hit.normal, -plane.normal) * step;
							num += num3;
							break;
						}
					}
				}
				return Mathf.Lerp(DragByAngle_MinDrag, DragByAngle_MaxDrag, Mathf.Pow(Mathf.InverseLerp(DragByAngle_MinContrib, DragByAngle_MaxContrib, num), DragByAngle_Exponent));
			}
		}

		public float GetDrag(Transform t, Rigidbody r)
		{
			if (dragByDirectionOfTravel == null)
			{
				return 1f;
			}
			Matrix4x4 localToWorldMatrix = t.localToWorldMatrix;
			Vector3 from = localToWorldMatrix.MultiplyVector(Vector3.forward);
			Vector3 axis = localToWorldMatrix.MultiplyVector(Vector3.up);
			Vector3 linearVelocity = r.linearVelocity;
			float num = Vector3.SignedAngle(from, linearVelocity, axis);
			if (num < 0f)
			{
				num += 360f;
			}
			int num2 = Mathf.FloorToInt(num / (float)directionIncrements) % dragByDirectionOfTravel.Length;
			int num3 = Mathf.CeilToInt(num / (float)directionIncrements) % dragByDirectionOfTravel.Length;
			bool flag = num2 > num3;
			float t2 = Mathf.InverseLerp(num2 * directionIncrements, (float)(num3 * directionIncrements) + (flag ? 360f : 0f), num);
			return Mathf.Lerp(dragByDirectionOfTravel[num2], dragByDirectionOfTravel[num3], t2);
		}

		public void BuildDragDebugTextTable(TextTable table)
		{
			table.AddColumn("Angle");
			table.AddColumn("Drag");
			for (int i = 0; i < dragByDirectionOfTravel.Length; i++)
			{
				int num = i * directionIncrements;
				float num2 = dragByDirectionOfTravel[i];
				table.AddRow(num.ToString(), num2.ToString());
			}
		}

		public void DDrawForPlayer(BasePlayer p)
		{
			if ((bool)boat)
			{
				Vector3 vector = boat.transform.position + Vector3.up * 2f;
				Vector3 forward = boat.transform.forward;
				for (int i = 0; i < dragByDirectionOfTravel.Length; i++)
				{
					int num = i * directionIncrements;
					float num2 = dragByDirectionOfTravel[i];
					Vector3 vector2 = Quaternion.Euler(0f, num, 0f) * forward;
					Vector3 vector3 = vector + vector2 * 5f;
					UnityEngine.DDraw.Arrow(p, vector, vector3, Color.yellow, 30f, 0.5f, distanceFade: true, zTest: false);
					UnityEngine.DDraw.Text(p, vector3 + Vector3.up * 1f, $"{num}°: {num2:F4}", Color.yellow, 30f);
				}
			}
		}
	}

	[Header("Effects")]
	public Transform boatRear;

	public ParticleSystemContainer wakeEffect;

	[ServerVar(Help = "(Generated) Duration in seconds after entering the deep sea zone that a player boat has before its engine is powered down")]
	public static float DeepSeaTransitionPowerDownGraceDuration = 10f;

	[ServerVar(Help = "(Generated) When enabled, player boat engines are powered down when no players are aboard; prevents runaway unmanned boats")]
	public static bool PowerdownOnNoPlayers = true;

	[ServerVar(Help = "When enabled, deployables on boats send immediate network updates when orphaned during edit mode to prevent looping sounds from being killed")]
	public static bool OrphanSendImmediate = true;

	[ServerVar(Help = "(Generated) Interval in seconds between checks to determine whether any players are still aboard the boat")]
	public static float AboardPlayerCheckInterval = 2f;

	[ServerVar(Help = "(Generated) Time in seconds after a boat is anchored before it becomes eligible for shore drift; default 21600s (6 hours)")]
	public static float AnchoredDriftDelaySeconds = 21600f;

	[ServerVar(Help = "0 - 1")]
	public static float SailPositionInfluence = 0.1f;

	[ServerVar(Help = "0 - 1")]
	public static float EnginePositionInfluences = 0.05f;

	[ServerVar(Help = "(Generated) Maximum angle in degrees from vertical at which building blocks can be placed on a player boat; default 30 degrees")]
	public static float PlacementUpThreshold = 30f;

	[ServerVar]
	[Help("How long until player boat corpses despawn")]
	public static float corpseseconds = 1800f;

	[ServerVar(Help = "How long before a boat loses all its health while outside")]
	public static float decayminutes = 720f;

	[ServerVar(Help = "How long until decay begins after the boat was last used")]
	public static float decaystartdelayminutes = 1440f;

	public static float CannonHitSlowdownMultiplier = 0f;

	private bool cachedPlayersAboard;

	private float approxTimestampNoPlayersAboard = float.NegativeInfinity;

	private List<TriggerParent> parentingTriggers = new List<TriggerParent>();

	private TimeUntil nextBeachedBoatCheck;

	private TimeSince timeBeached;

	private bool isBeached;

	private TimeCachedValue<float> desiredDrag;

	private TimeCachedValue<float> adjustedVelocityMax;

	private float calculatedMass;

	private TimeSince timeSinceLastUsed;

	private TimeSince timeSinceLastCannonAttack;

	private const float DECAY_TICK_TIME = 60f;

	private List<BaseEntity> entitiesToDestroyOnDeath = new List<BaseEntity>();

	private List<IOEntity> ioEntitiesToDisconnectOnDeath = new List<IOEntity>();

	private float powerDownGraceExpireTimestamp;

	public const string ACHIEVEMENT_HIT_BY_CANNON_NAME = "BOAT_CANNON_HIT";

	private DragByAngle dragByAngle;

	[ServerVar]
	public static float DragByAngle_MinDrag = 0.65f;

	[ServerVar]
	public static float DragByAngle_MaxDrag = 2.2f;

	[ServerVar]
	public static float DragByAngle_MinContrib = 5f;

	[ServerVar]
	public static float DragByAngle_MaxContrib = 50f;

	[ServerVar]
	public static float DragByAngle_Exponent = 0.4f;

	public const Flags Flag_DestructibleWreck = Flags.Reserved18;

	public static Translate.Phrase tooFastToEditErrorPhrase = new Translate.Phrase("playerboat_too_fast_to_edit", "The boat is moving too fast to edit.");

	public static Translate.Phrase recentlyDamagedCantEditPhrase = new Translate.Phrase("playerboat_recently_damaged_cant_edit", "The boat has recently been damaged and can't be edited.");

	public static Translate.Phrase onDeployEditCoolDownPhrase = new Translate.Phrase("playerboat_cooldown_phrase", "Deploy & Edit is on cooldown.");

	public static Translate.Phrase invalidDeployLocationPhrase = new Translate.Phrase("playerboat_invalid_deploy_location_phrase", "Unable to deploy & edit in this location.");

	public ItemDefinition BoatBuildingStationItem;

	public GameObjectRef BoatBuildingStationPrefab;

	public List<BaseEntity> ChildrenToDestroyOnDeath;

	public GameObjectRef sinkEffect;

	[HideInInspector]
	public List<ItemAmount> DynamicBuildCost = new List<ItemAmount>();

	[ReplicatedVar]
	public static bool EditEnabled = true;

	[ReplicatedVar]
	public static bool FinishEditingEnabled = true;

	[ReplicatedVar]
	public static bool HammerRepairEnabled = true;

	[ReplicatedVar]
	public static int MaxBlockCount = 180;

	[ReplicatedVar]
	public static int MaxDeployableCount = 120;

	[ReplicatedVar]
	public static bool DestructibleWrecksEnabled = true;

	[ReplicatedVar]
	public static bool UseDestructibleWreckStability = true;

	[Header("Player Boat")]
	public float MaxEditVelocity = 2f;

	public float DeathSinkRate = 0.05f;

	public AnimationCurve AimSwayVelocityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve AimSwayClarityClampVelocityCurve = AnimationCurve.Linear(0f, 0f, 15f, 1f);

	public AnimationCurve CannonAttackSlowdownCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public const Flags Flag_Dying = Flags.Broken;

	[ReplicatedVar]
	public static float VelocityMax = 15f;

	public CachedBoatParts<BoatBuildingBlock> BoatBuildingBlocks = new CachedBoatParts<BoatBuildingBlock>();

	public CachedBoatParts<BaseEntity> Deployables = new CachedBoatParts<BaseEntity>();

	private CachedBoatParts<Sail> Sails = new CachedBoatParts<Sail>();

	private CachedBoatParts<SmallEngine> Engines = new CachedBoatParts<SmallEngine>();

	private CachedBoatParts<Anchor> Anchors = new CachedBoatParts<Anchor>();

	private CachedBoatParts<SteeringWheel> SteeringWheels = new CachedBoatParts<SteeringWheel>();

	public ProtectionProperties ParentedBoatDeployableProtection;

	public PlayerBoatSounds playerBoatSounds;

	public Vector3 lastEditLocalPos;

	public Vector3 lastEditLocalRot;

	[ServerVar(Help = "(Generated) When enabled, draws debug visualisations for player boat state including drift target, shore direction, and power zones")]
	public static bool DebugVis { get; set; }

	protected override bool AllowKinematicDrift => true;

	public float boatSpawnTime { get; set; }

	public float TotalThrust => SailThrust + EngineThrust;

	public float SailThrust
	{
		get
		{
			float num = 0f;
			foreach (Sail item in Sails.Cached)
			{
				if (!(item == null))
				{
					num += item.CurrentThrust;
				}
			}
			return num;
		}
	}

	public float EngineThrust
	{
		get
		{
			float num = 0f;
			foreach (SmallEngine item in Engines.Cached)
			{
				if (!(item == null))
				{
					num += item.CurrentThrust;
				}
			}
			return num;
		}
	}

	public DragByAngle Tests_DragByAngle => dragByAngle;

	public bool KilledForEditMode { get; set; }

	public bool Anchored { get; private set; }

	public bool IsDying => HasFlag(Flags.Broken);

	public bool IsDestructibleWreck
	{
		get
		{
			if (DestructibleWrecksEnabled)
			{
				return HasFlag(Flags.Reserved18);
			}
			return false;
		}
	}

	public override VehiclePrivilege GetChildPrivilege()
	{
		foreach (SteeringWheel item in SteeringWheels.Cached)
		{
			if (!(item == null))
			{
				return item.Privilege;
			}
		}
		return null;
	}

	public override bool IsAuthedForBuilding(BasePlayer player)
	{
		return IsPlayerAuthed(player, authedIfNoPriveOrLock: true);
	}

	public static bool IsPlayerAuthedOnChildEntity(BaseEntity entity, BasePlayer player, bool authedIfNoPrivOrLock)
	{
		if (entity == null)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		PlayerBoat parentPlayerBoat = GetParentPlayerBoat(entity);
		if (parentPlayerBoat != null)
		{
			return parentPlayerBoat.IsPlayerAuthed(player, authedIfNoPrivOrLock);
		}
		return false;
	}

	public bool IsPlayerAuthed(BasePlayer player, bool authedIfNoPriveOrLock)
	{
		if (player == null)
		{
			return false;
		}
		PlayerBoatPrivilege playerBoatPrivilege = null;
		bool flag = false;
		SteeringWheel steeringWheel = GetSteeringWheel();
		if (steeringWheel != null)
		{
			playerBoatPrivilege = steeringWheel.Privilege;
			flag = steeringWheel.BoatLock.HasALock;
		}
		if (playerBoatPrivilege == null || !flag)
		{
			return authedIfNoPriveOrLock;
		}
		return playerBoatPrivilege.IsAuthed(player);
	}

	public void Init(List<BoatBuildingBlock> blocks, List<BaseEntity> ents, Vector3 halfExtents, bool loading)
	{
		rigidBody.isKinematic = true;
		bool autoSyncTransforms = UnityEngine.Physics.autoSyncTransforms;
		try
		{
			UnityEngine.Physics.autoSyncTransforms = false;
			if (!loading)
			{
				ParentChildBlocks(blocks);
				ParentChildEntities(ents);
			}
			ParentBlockTriggers(blocks);
		}
		finally
		{
			if (autoSyncTransforms)
			{
				UnityEngine.Physics.SyncTransforms();
			}
			UnityEngine.Physics.autoSyncTransforms = autoSyncTransforms;
		}
		rigidBody.isKinematic = false;
		Vector3 dimensions = halfExtents * 2f;
		dimensions.y += 6f;
		SetDimensions(dimensions);
		CalculateHealth(loading);
		CalculateRepairCost();
		CalculateMass();
		CalculateBuoyancy();
		SwitchToVehicle(loading);
		SetShouldTriggersParentSwimmers(shouldParentSwimmers: true);
		SetTriggersMovementMask(Axis.XZ);
		if (loading)
		{
			OnEntitiesAddedToBoat();
			rigidBody.isKinematic = false;
		}
		OnAnchoredChanged();
		boatSpawnTime = UnityEngine.Time.time;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		desiredDrag = new TimeCachedValue<float>
		{
			refreshCooldown = 1f,
			refreshRandomRange = 0.5f,
			updateValue = CalculateDesiredDrag
		};
		adjustedVelocityMax = new TimeCachedValue<float>
		{
			refreshCooldown = 1.2f,
			refreshRandomRange = 0.2f,
			updateValue = CalculateAdjustedMaxVelocity
		};
		InvokeRandomized(CheckForPlayersAboard, 0f, AboardPlayerCheckInterval, AboardPlayerCheckInterval * 0.1f);
		dragByAngle.boat = this;
		Invoke(BakeDragAngles, 0f);
		ResetTimeSinceUsed();
		InvokeRandomized(BoatDecay, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
		Invoke(StartDeployAndEditCoolDown, 0f);
		timeSinceLastCannonAttack = 100f;
	}

	private void StartDeployAndEditCoolDown()
	{
		BoatBuildingStation.StartGlobalEditFinishCoolDown();
		if (IsInvoking(ClearDeployAndEditCoolDown))
		{
			CancelInvoke(ClearDeployAndEditCoolDown);
		}
		SetFlagLocal(Flags.Busy, b: true);
		SendNetworkUpdateImmediate();
		Invoke(ClearDeployAndEditCoolDown, BoatBuildingStation.EditFinishUseInterval);
	}

	private void ClearDeployAndEditCoolDown()
	{
		SetFlagLocal(Flags.Busy, b: false);
		SendNetworkUpdateImmediate();
	}

	private void BakeDragAngles()
	{
		dragByAngle.Init(45);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.playerBoat = Facepunch.Pool.Get<ProtoBuf.PlayerBoat>();
		info.msg.playerBoat.size = bounds.size;
		info.msg.playerBoat.lastEditLocalPos = lastEditLocalPos;
		info.msg.playerBoat.lastEditLocalRot = lastEditLocalRot;
		if (info.forDisk)
		{
			info.msg.playerBoat.timeSinceLastUsed = timeSinceLastUsed.PassedSince(info.cachedTime.Time);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (!base.isServer)
		{
			return;
		}
		if (base.health <= 0f || HasFlag(Flags.Broken))
		{
			Kill();
			return;
		}
		BoatBuildingStation.GetBoatBlocksOBBExtents(BoatBuildingBlocks.Cached, base.transform.forward, out var _, out var halfExtents, out var _);
		if (!IsDying)
		{
			base.transform.position = base.transform.position.WithY(Env.oceanlevel);
			base.transform.localEulerAngles = new Vector3(0f, base.transform.localEulerAngles.y, 0f);
		}
		Init(BoatBuildingBlocks.Cached, null, halfExtents, loading: true);
	}

	public void ResetTimeSinceUsed()
	{
		timeSinceLastUsed = 0f;
	}

	public bool VerifyDeployablePlacement(BasePlayer player)
	{
		if (MaxDeployableCount > 0 && Deployables.Cached.Count >= MaxDeployableCount)
		{
			player.ShowToast(GameTip.Styles.Error, BoatBuildingStation.invalidTooManyDeployablesPhrase, false);
			return false;
		}
		if (!HasFlag(Flags.Broken) && !IsWithinDegreesOfUp(PlacementUpThreshold))
		{
			player.ShowToast(GameTip.Styles.Error, BoatBuildingStation.invalidIllegalPlacement, false);
			return false;
		}
		return true;
	}

	private void CheckForPlayersAboard()
	{
		bool flag = cachedPlayersAboard;
		cachedPlayersAboard = AnyPlayersOnBoat();
		if (UnityEngine.Time.time < powerDownGraceExpireTimestamp)
		{
			cachedPlayersAboard = flag;
		}
		if (flag != cachedPlayersAboard)
		{
			if (cachedPlayersAboard)
			{
				OnPlayersAboard();
			}
			else
			{
				OnNoPlayersAboard();
			}
		}
	}

	private void OnPlayersAboard()
	{
		approxTimestampNoPlayersAboard = 0f;
	}

	private void OnNoPlayersAboard()
	{
		PowerDown();
		approxTimestampNoPlayersAboard = UnityEngine.Time.realtimeSinceStartup;
	}

	public void PowerDown(bool force = false)
	{
		if (PowerdownOnNoPlayers || force)
		{
			SetAllSailsOpen(flag: false);
			SetAllEnginesOn(flag: false);
			desiredDrag.ForceNextRun();
			ResetSteering();
		}
	}

	public void OnCreatedAtBBS(BoatBuildingStation bbs)
	{
		lastEditLocalPos = Quaternion.Inverse(base.transform.rotation) * (bbs.transform.position - base.transform.position);
		lastEditLocalRot = (Quaternion.Inverse(base.transform.rotation) * bbs.transform.rotation).eulerAngles;
	}

	public bool DeployAndEdit(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (!CanDeployAndEdit(player, out var outPosition, out var outRotation, checkForCoolDown: true, checkForItem: true, checkDeploy: true, sendErrorToasts: true))
		{
			return false;
		}
		BoatBuildingStation obj = GameManager.server.CreateEntity(BoatBuildingStationPrefab.resourcePath, outPosition, outRotation) as BoatBuildingStation;
		obj.Spawn();
		obj.SendNetworkUpdate();
		obj.OnPlaced(player);
		Deployable component = BoatBuildingStationPrefab.Get().GetComponent<Deployable>();
		if (component != null && component.placeEffect.isValid)
		{
			Effect.server.Run(component.placeEffect.resourcePath, outPosition, Vector3.up);
		}
		player.inventory.Take(null, BoatBuildingStationItem.itemid, 1);
		player.Command("note.inv", BoatBuildingStationItem.itemid, -1);
		return true;
	}

	public void ParentChildBlocks(List<BoatBuildingBlock> blocks)
	{
		foreach (BoatBuildingBlock block in blocks)
		{
			block.SetParent(this, worldPositionStays: true);
		}
	}

	public void ParentBlockTriggers(List<BoatBuildingBlock> blocks)
	{
		parentingTriggers.Clear();
		List<Transform> obj = Facepunch.Pool.Get<List<Transform>>();
		foreach (BoatBuildingBlock block in blocks)
		{
			if (!block.ProvidesParentingTrigger)
			{
				continue;
			}
			List<TriggerParent> collection = block.SetTriggerParent(this);
			parentingTriggers.AddRange(collection);
			Transform[] dismountPoints = block.DismountPoints;
			if (dismountPoints == null || dismountPoints.Length == 0)
			{
				continue;
			}
			for (int i = 0; i < block.DismountPoints.Length; i++)
			{
				if ((bool)block.DismountPoints[i])
				{
					obj.Add(block.DismountPoints[i]);
				}
			}
		}
		dismountPositions = obj.ToArray();
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private void SetShouldTriggersParentSwimmers(bool shouldParentSwimmers)
	{
		foreach (TriggerParent parentingTrigger in parentingTriggers)
		{
			parentingTrigger.parentSwimmers = shouldParentSwimmers;
		}
	}

	private void SetTriggersMovementMask(Axis movementMask)
	{
		foreach (TriggerParent parentingTrigger in parentingTriggers)
		{
			parentingTrigger.TriggerMovementMask = movementMask;
		}
	}

	public void ParentChildEntities(List<BaseEntity> ents)
	{
		foreach (BaseEntity ent in ents)
		{
			ent.SetParent(this, worldPositionStays: true);
		}
		OnEntitiesAddedToBoat();
	}

	private void OnEntitiesAddedToBoat()
	{
		foreach (BaseEntity child in children)
		{
			if (child is global::IBoatBuildingPiece boatBuildingPiece)
			{
				boatBuildingPiece.OnAddedToBoat(this);
			}
		}
		ListenServerColliderFix();
	}

	public void DistributeHealthAcrossBlocks()
	{
		List<BoatBuildingBlock> cached = BoatBuildingBlocks.Cached;
		float num = 0f;
		float num2 = 0f;
		float num3 = base.health;
		foreach (BoatBuildingBlock item in cached)
		{
			item.SendNetworkUpdateOnHealthChanged = false;
			item.DecayTouch();
			if (!(item.damageTaken <= 0f))
			{
				num += item.damageTaken;
				float num4 = Mathf.Max(item.health - item.damageTaken, 1f);
				float num5 = Mathf.Abs(item.health - num4);
				item.health = num4;
				num2 += num5;
			}
		}
		float num6 = 0f;
		foreach (BoatBuildingBlock item2 in cached)
		{
			if (num2 >= num)
			{
				num6 += item2.health;
				continue;
			}
			float num7 = Mathf.Min(item2.health - 1f, num - num2);
			item2.health -= num7;
			num6 += item2.health;
			num2 += num7;
		}
		float num8 = Mathf.Clamp(num3 - num6, 0f, num3);
		foreach (BoatBuildingBlock item3 in cached)
		{
			float num9 = Mathf.Min(num8, item3.MaxHealth() - item3.health);
			if (num9 > 0f)
			{
				item3.health += num9;
				num8 -= num9;
			}
			item3.SendNetworkUpdateOnHealthChanged = true;
		}
	}

	public void OrphanChildEntities(bool boatBlocksOnly = false)
	{
		for (int num = children.Count - 1; num >= 0; num--)
		{
			BaseEntity baseEntity = children[num];
			if (!boatBlocksOnly || !(baseEntity as BoatBuildingBlock == null))
			{
				bool sendImmediate = baseEntity is BasePlayer || baseEntity is PartyBalloon || (OrphanSendImmediate && !(baseEntity is BoatBuildingBlock));
				baseEntity.SetParent(null, worldPositionStays: true, sendImmediate);
			}
		}
	}

	[ServerVar(ClientAdmin = true, Help = "(Generated) Prints drag force debug data based on the angle between the boat heading and player look direction; admin-only")]
	public static void LookAtDragByAngle(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer)
		{
			BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 5f);
			if (baseNetworkable == null || !(baseNetworkable.GetRootParentEntity() is PlayerBoat playerBoat))
			{
				arg.ReplyWith("Not looking at boat");
				return;
			}
			TextTable obj = Facepunch.Pool.Get<TextTable>();
			playerBoat.dragByAngle.BuildDragDebugTextTable(obj);
			playerBoat.dragByAngle.DDrawForPlayer(basePlayer);
			arg.ReplyWith(obj.ToString());
			Facepunch.Pool.Free(ref obj);
		}
	}

	private float CalculateAdjustedMaxVelocity()
	{
		using (TimeWarning.New("CalculateAdjustedMaxVelocity"))
		{
			float num = float.MaxValue;
			NativeArray<Vector3> nativeArray = new NativeArray<Vector3>(3, Allocator.TempJob);
			NativeArray<int> nativeArray2 = new NativeArray<int>(3, Allocator.TempJob);
			NativeArray<float> results = new NativeArray<float>(3, Allocator.TempJob);
			for (int i = 0; i < 3; i++)
			{
				nativeArray2[i] = i;
				Vector3 pos = (nativeArray[i] = planeFitPoints[i].position);
				num = Mathf.Min(num, WaterLevel.GetOverallWaterDepth(pos, waves: false, volumes: false));
			}
			TerrainMeta.Texturing.GetCoarseDistancesToShoreIndirect(nativeArray.AsReadOnly(), nativeArray2.AsReadOnly(), results);
			float num2 = float.MaxValue;
			for (int j = 0; j < results.Length; j++)
			{
				num2 = Mathf.Min(num2, results[j]);
			}
			nativeArray.Dispose(default(JobHandle));
			nativeArray2.Dispose(default(JobHandle));
			results.Dispose(default(JobHandle));
			float a = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(20f, 50f, num2));
			float b = Mathf.Lerp(0.3f, 1f, Mathf.InverseLerp(3f, 7f, num));
			return VelocityMax * Mathf.Min(a, b);
		}
	}

	private float CalculateDesiredDrag()
	{
		using (TimeWarning.New("CalculateDesiredDrag"))
		{
			if (!(Mathf.Abs(TotalThrust) > Mathf.Epsilon))
			{
				return 1.5f;
			}
			return dragByAngle.GetDrag(base.transform, rigidBody);
		}
	}

	public float CurrentThrust()
	{
		if (Anchored)
		{
			return 0f;
		}
		return TotalThrust;
	}

	public void CalculateHealth(bool loading)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!(item == null))
			{
				num2 += item.MaxHealth();
				num += item.health;
			}
		}
		OverrideMaxHealth(num2);
		if (!loading)
		{
			SetHealth(num);
		}
	}

	public void CalculateRepairCost()
	{
		if (BoatBuildingBlocks.Cached == null || BoatBuildingBlocks.Cached.Count == 0)
		{
			return;
		}
		DynamicBuildCost.Clear();
		int num = 0;
		int num2 = 0;
		List<ItemAmount> list = null;
		List<ItemAmount> list2 = null;
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (item == null)
			{
				continue;
			}
			if (item.Hull)
			{
				if (list == null)
				{
					list = item.BuildCost().Items;
				}
				num++;
			}
			else
			{
				if (list2 == null)
				{
					list2 = item.BuildCost().Items;
				}
				num2++;
			}
		}
		AddBlockCosts(list, num, ref DynamicBuildCost);
		AddBlockCosts(list2, num2, ref DynamicBuildCost);
	}

	private void AddBlockCosts(List<ItemAmount> costs, int blockCount, ref List<ItemAmount> dynamicBuildCost)
	{
		if (costs == null || blockCount <= 0)
		{
			return;
		}
		foreach (ItemAmount cost in costs)
		{
			if (cost == null || cost.itemDef == null)
			{
				continue;
			}
			float num = cost.amount * (float)blockCount;
			bool flag = false;
			for (int i = 0; i < dynamicBuildCost.Count; i++)
			{
				ItemAmount itemAmount = dynamicBuildCost[i];
				if (itemAmount.itemDef == cost.itemDef)
				{
					itemAmount.amount += num;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				dynamicBuildCost.Add(new ItemAmount(cost.itemDef, num));
			}
		}
	}

	public override float RepairCostFraction()
	{
		return 1f;
	}

	public void CalculateMass()
	{
		float num = 0f;
		int num2 = 0;
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!(item == null))
			{
				num += item.ContributingMass;
				num2++;
			}
		}
		num = Mathf.Max(num, 1000f);
		rigidBody.mass = num;
		calculatedMass = num;
	}

	public void CalculateBuoyancy()
	{
		buoyancy.scaleForceWithMass = true;
		Vector3 forward = base.transform.forward;
		Vector3 right = base.transform.right;
		Vector3 position = base.transform.position;
		float num = bounds.size.x / 2f;
		float num2 = bounds.size.z / 2f;
		List<Vector3> obj = Facepunch.Pool.Get<List<Vector3>>();
		obj.Add(position + forward * num2);
		obj.Add(position + forward * num2 + right * num);
		obj.Add(position + forward * num2 + -right * num);
		obj.Add(position);
		obj.Add(position + right * num);
		obj.Add(position + -right * num);
		obj.Add(position + -forward * num2);
		obj.Add(position + -forward * num2 + right * num);
		obj.Add(position + -forward * num2 + -right * num);
		buoyancy.SetBuoyancyPointLocations(obj);
		buoyancy.SavePointData(forced: true);
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Transform[] array = planeFitPoints;
		if (array != null && array.Length >= 3)
		{
			planeFitPoints[0].position = position + forward * num2;
			planeFitPoints[1].position = position + -forward * num2 + right * num;
			planeFitPoints[2].position = position + -forward * num2 + -right * num;
		}
	}

	public override bool EngineOn()
	{
		if (!(CurrentThrust() > 0f))
		{
			return !RudderInDeadzone();
		}
		return true;
	}

	private bool RudderInDeadzone()
	{
		return Mathf.Abs(steering) < 0.02f;
	}

	public override bool EngineOnEligible()
	{
		if (EngineOn() && !IsFlipped())
		{
			return base.healthFraction > 0f;
		}
		return false;
	}

	public override void VehicleFixedUpdate()
	{
		using (TimeWarning.New("PlayerBoat.VehicleFixedUpdate"))
		{
			UpdateBuoyancy();
			bool flag = rigidBody.linearVelocity.magnitude > AntiHackVelocity();
			if (EngineOn() && !Anchored)
			{
				bool reversing = false;
				if (!flag)
				{
					CalcSailsForces(out var accumForce, out var accumTorque);
					CalcEnginesForces(out var accumForce2, out var accumTorque2);
					Vector3 accumulatedForce = accumForce + accumForce2;
					Vector3 vector = accumTorque + accumTorque2;
					float num = 13f;
					float num2 = 1f;
					if (accumulatedForce.magnitude > 0.1f)
					{
						float mass = rigidBody.mass;
						Vector3 vector2 = accumulatedForce / mass;
						Vector3 vector3 = accumulatedForce;
						if (vector2.sqrMagnitude > num * num)
						{
							accumulatedForce = accumulatedForce.normalized * (mass * num);
						}
						num2 = accumulatedForce.sqrMagnitude / vector3.sqrMagnitude;
						HandleCannonAttackSlowdown(ref accumulatedForce);
						rigidBody.AddForce(accumulatedForce, ForceMode.Force);
						if (Vector3.Dot(accumulatedForce, base.transform.forward) <= 0f)
						{
							reversing = true;
						}
					}
					if (vector != Vector3.zero)
					{
						rigidBody.AddTorque(vector * num2, ForceMode.Force);
					}
				}
				RudderTorque(reversing);
				gasPedal = 0f;
			}
			base.VehicleFixedUpdate();
			HandleBeachedPushing();
			rigidBody.linearDamping = desiredDrag.Get(force: false);
			if (flag)
			{
				rigidBody.linearVelocity = rigidBody.linearVelocity.normalized * AntiHackVelocity();
			}
		}
	}

	public override bool AnyMounted()
	{
		return HasDriver();
	}

	private void HandleCannonAttackSlowdown(ref Vector3 accumulatedForce)
	{
		float b = CannonAttackSlowdownCurve.Evaluate(timeSinceLastCannonAttack);
		b = Mathf.Lerp(1f, b, CannonHitSlowdownMultiplier);
		accumulatedForce *= b;
	}

	public override void DismountAllPlayers()
	{
		base.DismountAllPlayers();
		List<SteeringWheel> cached = SteeringWheels.Cached;
		for (int i = 0; i < cached.Count; i++)
		{
			if ((bool)cached[i])
			{
				cached[i].DismountAllPlayers();
			}
		}
	}

	private void UpdateBuoyancy()
	{
		if (IsDying)
		{
			buoyancy.buoyancyScale = Mathf.Lerp(buoyancy.buoyancyScale, 0f, UnityEngine.Time.fixedDeltaTime * DeathSinkRate);
		}
	}

	private void HandleBeachedPushing()
	{
		using (TimeWarning.New("PlayerBoat.HandleBeachedPushing"))
		{
			if (Anchored)
			{
				isBeached = false;
				return;
			}
			if ((float)nextBeachedBoatCheck <= 0f)
			{
				bool flag = isBeached;
				isBeached = false;
				Transform transform = null;
				float num = float.MaxValue;
				Transform[] array = planeFitPoints;
				foreach (Transform transform2 in array)
				{
					Vector3 position = transform2.position;
					float coarseDistanceToShore = TerrainTexturing.Instance.GetCoarseDistanceToShore(position);
					if (coarseDistanceToShore < num)
					{
						num = coarseDistanceToShore;
						transform = transform2;
					}
				}
				if (transform != null)
				{
					float overallWaterDepth = WaterLevel.GetOverallWaterDepth(transform.position, waves: false, volumes: false, this);
					if (num < 10f || overallWaterDepth < 3.5f)
					{
						isBeached = true;
					}
				}
				if (!isBeached)
				{
					nextBeachedBoatCheck = 4f;
					return;
				}
				if (isBeached && !flag)
				{
					timeBeached = 0f;
				}
			}
			if (isBeached && (float)timeBeached > 10f)
			{
				float t = Mathf.InverseLerp(10f, 13f, timeBeached);
				float num2 = Mathf.Lerp(0f, 3f, t);
				Transform[] array = planeFitPoints;
				for (int i = 0; i < array.Length; i++)
				{
					Vector3 position2 = array[i].position;
					Vector3 vector = -TerrainTexturing.Instance.GetCoarseVectorToShore(position2).shoreDir * (num2 * (1f / (float)planeFitPoints.Length)) + Vector3.up * (num2 * 0.75f);
					rigidBody.AddForceAtPosition(vector * rigidBody.mass, position2, ForceMode.Force);
				}
			}
		}
	}

	public override bool BuoyancySleep(bool inWater)
	{
		if (isBeached)
		{
			return false;
		}
		SetToKinematic();
		return true;
	}

	public override bool BuoyancyWake()
	{
		SetToNonKinematic();
		return true;
	}

	private void CalcSailsForces(out Vector3 accumForce, out Vector3 accumTorque)
	{
		using (TimeWarning.New("ApplySailsForces"))
		{
			if (!buoyancy.InWater)
			{
				accumForce = Vector3.zero;
				accumTorque = Vector3.zero;
				return;
			}
			List<Sail> cached = Sails.Cached;
			Vector3 vector = rigidBody.centerOfMass + rigidBody.position;
			accumForce = Vector3.zero;
			accumTorque = Vector3.zero;
			foreach (Sail item in cached)
			{
				if (!(item.CurrentThrust < Mathf.Epsilon))
				{
					Vector3 vector2 = Vector3.Lerp(vector, item.ThrustPosition, SailPositionInfluence);
					Vector3 vector3 = item.Direction * item.CurrentThrust;
					accumForce += vector3;
					accumTorque += Vector3.Cross(vector2 - vector, vector3);
				}
			}
		}
	}

	private void RudderTorque(bool reversing)
	{
		if (!RudderInDeadzone() && !Anchored)
		{
			float num = (reversing ? 1f : (-1f)) * steering * steeringScale;
			float num2 = Mathf.Clamp(Vector3.Dot(rigidBody.linearVelocity, base.transform.forward) / AntiHackVelocity(), 0.6f, 1f);
			if (rigidBody.linearVelocity.sqrMagnitude < 0.1f)
			{
				num2 = 0.3f;
			}
			float num3 = num * num2 * 3f;
			rigidBody.AddRelativeTorque(Vector3.up * num3, ForceMode.Acceleration);
		}
	}

	private void CalcEnginesForces(out Vector3 accumForce, out Vector3 accumTorque)
	{
		using (TimeWarning.New("ApplyEngineForces"))
		{
			accumForce = Vector3.zero;
			accumTorque = Vector3.zero;
			if (!buoyancy.InWater)
			{
				return;
			}
			List<SmallEngine> cached = Engines.Cached;
			Vector3 vector = rigidBody.centerOfMass + rigidBody.position;
			Vector3 forward = base.transform.forward;
			Plane plane = new Plane(forward, vector);
			Vector3 vector2 = Quaternion.AngleAxis(steering * 30f, Vector3.up) * forward;
			Vector3 vector3 = Quaternion.AngleAxis((0f - steering) * 30f, Vector3.up) * forward;
			foreach (SmallEngine item in cached)
			{
				float num = item.CurrentThrust;
				if (!(num < Mathf.Epsilon) && item.TryUseFuel())
				{
					Vector3 vector4 = Vector3.Lerp(vector, item.ThrustPosition, EnginePositionInfluences);
					if (item.InReverse)
					{
						num *= item.ReverseMod;
					}
					Vector3 vector5 = (plane.GetSide(vector4) ? vector3 : vector2) * num;
					accumForce += vector5;
					accumTorque += Vector3.Cross(vector4 - vector, vector5);
				}
			}
		}
	}

	public void SwitchToVehicle(bool loading)
	{
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!(item == null))
			{
				item.SwitchToVehicle(loading);
			}
		}
	}

	public void SwitchToConstruction()
	{
		foreach (BoatBuildingBlock item in BoatBuildingBlocks.Cached)
		{
			if (!(item == null) && item.ProvidesParentingTrigger)
			{
				item.ResetTriggerParent();
			}
		}
	}

	public override bool AnyPlayersOnBoat()
	{
		if (base.AnyPlayersOnBoat())
		{
			return true;
		}
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		GetPlayersOnBoat(obj);
		bool num = obj.Count > 0;
		Facepunch.Pool.FreeUnmanaged(ref obj);
		if (!num)
		{
			return base.AnyPlayersOnBoat();
		}
		return true;
	}

	[PoolAnalyzerNonCaching]
	public override void GetPlayersOnBoat(List<BasePlayer> players)
	{
		if (players == null)
		{
			return;
		}
		players.Clear();
		base.GetPlayersOnBoat(players);
		foreach (TriggerParent parentingTrigger in parentingTriggers)
		{
			if (parentingTrigger == null || !parentingTrigger.HasAnyEntityContents)
			{
				continue;
			}
			foreach (BaseEntity entityContent in parentingTrigger.entityContents)
			{
				BasePlayer basePlayer = entityContent.ToPlayer();
				if (basePlayer != null && !players.Contains(basePlayer))
				{
					players.Add(basePlayer);
				}
			}
		}
		foreach (BaseEntity child in children)
		{
			if (!(child is SmallRamp) && !(child is Plank) && !(child is BaseLadder))
			{
				continue;
			}
			foreach (BaseEntity child2 in child.children)
			{
				BasePlayer basePlayer2 = child2.ToPlayer();
				if (basePlayer2 != null && !players.Contains(basePlayer2))
				{
					players.Add(basePlayer2);
				}
			}
		}
	}

	public void SetAllSailsOpen(bool flag)
	{
		foreach (Sail item in Sails.Cached)
		{
			if (flag)
			{
				item.Lower(null);
			}
			else
			{
				item.Raise(null);
			}
		}
	}

	public void SetAllEnginesOn(bool flag)
	{
		foreach (SmallEngine item in Engines.Cached)
		{
			if (flag)
			{
				item.TurnOn();
			}
			else
			{
				item.TurnOff();
			}
		}
	}

	public void ResetSteering()
	{
		foreach (SteeringWheel item in SteeringWheels.Cached)
		{
			if (!(item == null))
			{
				item.ResetSteering();
			}
		}
	}

	public void OnAnchoredChanged()
	{
		bool anchored = Anchored;
		Anchored = false;
		foreach (Anchor item in Anchors.Cached)
		{
			if (item.Lowered)
			{
				Anchored = true;
				break;
			}
		}
		buoyancy.FlowForceDisabled = Anchored;
		if (Anchored != anchored)
		{
			SetDriftDelayAmount(Anchored ? AnchoredDriftDelaySeconds : 0f);
		}
		if (Anchored)
		{
			rigidBody.mass = 100000f;
		}
		else
		{
			rigidBody.mass = calculatedMass;
		}
	}

	private void DebugDrawDimensions()
	{
	}

	[ServerVar(Help = "(Generated) Opens all sails on the player boat directly in front of the calling admin player; admin-only dev command")]
	public static void SetSailsOpen(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin)
		{
			return;
		}
		List<Sail> obj = Facepunch.Pool.Get<List<Sail>>();
		Vis.Entities(basePlayer.transform.position, arg.GetFloat(1), obj);
		bool @bool = arg.GetBool(0);
		foreach (Sail item in obj)
		{
			if (item.isServer)
			{
				if (@bool)
				{
					item.Lower(basePlayer);
				}
				else
				{
					item.Raise(basePlayer);
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Instantly kills the player boat directly in front of the calling admin player; admin-only dev command")]
	public static void Sink(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer.IsAdmin)
		{
			return;
		}
		using PooledList<ConstructionSkin> pooledList = Facepunch.Pool.Get<PooledList<ConstructionSkin>>();
		Vis.Components(basePlayer.transform.position, arg.GetFloat(1, 5f), pooledList, 134217728);
		using PooledHashSet<PlayerBoat> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<PlayerBoat>>();
		foreach (ConstructionSkin item in pooledList)
		{
			PlayerBoat componentInParent = item.GetComponentInParent<PlayerBoat>();
			if ((bool)componentInParent && !componentInParent.IsClient && pooledHashSet.Add(componentInParent))
			{
				componentInParent.SetHealth(0f);
				componentInParent.Die();
			}
		}
	}

	public override void OnDied(HitInfo info)
	{
		if (!IsDying)
		{
			PowerDown(force: true);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Broken, b: true);
			}
			buoyancy.buoyancyScale *= 0.6f;
			SetShouldTriggersParentSwimmers(shouldParentSwimmers: false);
			SetTriggersMovementMask(Axis.XYZ);
			Invoke(DestroyFlaggedItemsOnDeath, 20f);
			Invoke(EnableDestructibleWreck, 20f);
			repair.enabled = false;
			Invoke(DismountAllPlayers, 10f);
			EnterCorpseState();
			if (sinkEffect.isValid)
			{
				Effect.server.Run(sinkEffect.resourcePath, this);
			}
		}
	}

	private void EnableDestructibleWreck()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved18, b: true);
	}

	private void DestroyFlaggedItemsOnDeath()
	{
		if (entitiesToDestroyOnDeath != null)
		{
			for (int num = entitiesToDestroyOnDeath.Count - 1; num >= 0; num--)
			{
				BaseEntity baseEntity = entitiesToDestroyOnDeath[num];
				if (!(baseEntity == null))
				{
					baseEntity.Kill(DestroyMode.Gib);
				}
			}
		}
		if (ioEntitiesToDisconnectOnDeath == null)
		{
			return;
		}
		for (int num2 = ioEntitiesToDisconnectOnDeath.Count - 1; num2 >= 0; num2--)
		{
			IOEntity iOEntity = ioEntitiesToDisconnectOnDeath[num2];
			if (!(iOEntity == null))
			{
				iOEntity.DisconnectAll();
			}
		}
	}

	protected void EnterCorpseState()
	{
		Invoke(ActualDeath, corpseseconds);
	}

	public void ActualDeath()
	{
		Kill(DestroyMode.Gib);
	}

	public void OnBoatDeployableHurt(BaseEntity deployable, HitInfo info)
	{
		TakeDamage(deployable, ParentedBoatDeployableProtection, info);
	}

	public void OnBuildingBlockHurt(BoatBuildingBlock block, HitInfo info)
	{
		TakeDamage(block, block.baseProtection, info);
	}

	private void TakeDamage(BaseEntity damagedEntity, ProtectionProperties protectionProperties, HitInfo hitInfo)
	{
		if (DeepSeaManager.IsInsideDeepSea(this) && InSafeZone())
		{
			return;
		}
		float num = base.health;
		ProtectionProperties protectionProperties2 = baseProtection;
		baseProtection = protectionProperties;
		Hurt(hitInfo);
		baseProtection = protectionProperties2;
		if (damagedEntity is BoatBuildingBlock boatBuildingBlock)
		{
			float amount = base.health - num;
			boatBuildingBlock.RecordDamageTaken(amount);
		}
		if (hitInfo.damageTypes.Has(DamageType.Cannon))
		{
			timeSinceLastCannonAttack = 0f;
			if (Rust.GameInfo.HasAchievements && (bool)hitInfo.InitiatorPlayer)
			{
				hitInfo.InitiatorPlayer.GiveAchievement("BOAT_CANNON_HIT");
			}
		}
	}

	public override BasePlayer GetPlayerDamageInitiator()
	{
		if (HasDriver())
		{
			foreach (SteeringWheel item in SteeringWheels.Cached)
			{
				BasePlayer mounted = item.GetMounted();
				if ((bool)mounted)
				{
					return mounted;
				}
			}
		}
		else if (cachedPlayersAboard)
		{
			foreach (TriggerParent parentingTrigger in parentingTriggers)
			{
				if (!parentingTrigger || !parentingTrigger.HasAnyEntityContents)
				{
					continue;
				}
				foreach (BaseEntity entityContent in parentingTrigger.entityContents)
				{
					if (entityContent.ToPlayer() != null)
					{
						return entityContent as BasePlayer;
					}
				}
			}
		}
		return BasePlayer.FindByID(base.OwnerID);
	}

	public float GetDamageMultiplier(BaseEntity ent)
	{
		return Mathf.Max(1f, Mathf.Abs(GetSpeed()) * 4f);
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
	}

	protected override float GetPushActionForce()
	{
		return Mathf.Min(rigidBody.mass, 1000f) * 5f;
	}

	public override bool AllowInitChildSupports()
	{
		return true;
	}

	private void BoatDecay()
	{
		BaseBoat.WaterVehicleDecay(this, 60f, timeSinceLastUsed, decayminutes, decayminutes, decaystartdelayminutes, preventDecayIndoors);
	}

	void IReceiveDeepSeaNotifications.OnEnterDeepSea()
	{
		TriggerPowerDownGracePeriod();
	}

	void IReceiveDeepSeaNotifications.OnExitDeepSea()
	{
		TriggerPowerDownGracePeriod();
	}

	private void TriggerPowerDownGracePeriod()
	{
		powerDownGraceExpireTimestamp = UnityEngine.Time.time + DeepSeaTransitionPowerDownGraceDuration;
	}

	[ServerVar(Help = "(Generated) Kills all player boats that have more building blocks than the given threshold; used for server cleanup")]
	public static void kill_all_above_block_count(ConsoleSystem.Arg arg)
	{
		if (arg.Args == null || arg.Args.Length == 0)
		{
			arg.ReplyWith("Must provide a minimum block count.");
			return;
		}
		int @int = arg.GetInt(0);
		int num = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!(playerBoat == null) && playerBoat.BoatBuildingBlocks != null && playerBoat.BoatBuildingBlocks.Cached.Count >= @int)
			{
				playerBoat.Kill();
				num++;
			}
		}
		arg.ReplyWith($"Killed {num} player boats");
	}

	[ServerVar(Help = "(Generated) Kills all player boats that have more deployed entities than the given threshold; used for server cleanup")]
	public static void kill_all_above_deployable_count(ConsoleSystem.Arg arg)
	{
		if (arg.Args == null || arg.Args.Length == 0)
		{
			arg.ReplyWith("Must provide a minimum deployable count.");
			return;
		}
		int @int = arg.GetInt(0);
		int num = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!(playerBoat == null) && playerBoat.Deployables != null && playerBoat.Deployables.Cached.Count >= @int)
			{
				playerBoat.Kill();
				num++;
			}
		}
		arg.ReplyWith($"Killed {num} player boats");
	}

	[ServerVar(Help = "(Generated) Prints statistics about all player boats on the server including block counts, deployable counts, and resource totals")]
	public static void print_stats(ConsoleSystem.Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BOATS:");
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!(playerBoat == null) && playerBoat.BoatBuildingBlocks != null)
			{
				stringBuilder.AppendLine($"{playerBoat.BoatBuildingBlocks.Cached.Count} blocks / " + $"{playerBoat.Deployables.Cached.Count} deployables. Alive: " + $"{UnityEngine.Time.time - playerBoat.boatSpawnTime}s. Pos: " + $"{playerBoat.transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Prints a list of boats with non-convex collider deployables. Not a fast command. Use sparingly when needed.")]
	public static void print_nonconvex(ConsoleSystem.Arg arg)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("BOATS WITH NON CONVEX MESH COLLIDERS:");
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!(playerBoat == null) && playerBoat.Deployables != null && playerBoat.HasNonConvexColliders())
			{
				stringBuilder.AppendLine($"{playerBoat.BoatBuildingBlocks.Cached.Count} blocks / " + $"{playerBoat.Deployables.Cached.Count} deployables. Alive: " + $"{UnityEngine.Time.time - playerBoat.boatSpawnTime}s. Pos: " + $"{playerBoat.transform.position}");
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Kills any entities deployed on boats with non-convex colliders. Not a fast command. Use sparingly when needed.")]
	public static void kill_nonconvex_deployables(ConsoleSystem.Arg arg)
	{
		new StringBuilder();
		int num = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		foreach (PlayerBoat playerBoat in array)
		{
			if (!(playerBoat == null) && playerBoat.Deployables != null && playerBoat.DestroyNonConvexColliderDeployables())
			{
				num++;
			}
		}
		arg.ReplyWith("Killed convex entities on " + num + " boats.");
	}

	[ServerVar(Help = "Kills any IO entities deployed on boats. Not a fast command. Use sparingly when needed.")]
	public static void kill_io_deployables(ConsoleSystem.Arg arg)
	{
		new StringBuilder();
		int num = 0;
		PlayerBoat[] array = Util.FindAll<PlayerBoat>();
		PlayerBoat[] array2 = array;
		foreach (PlayerBoat playerBoat in array2)
		{
			if (playerBoat == null || playerBoat.Deployables == null)
			{
				continue;
			}
			for (int num2 = playerBoat.Deployables.Cached.Count - 1; num2 >= 0; num2--)
			{
				BaseEntity baseEntity = playerBoat.Deployables.Cached[num2];
				if (!(baseEntity == null) && !(baseEntity.gameObject == null) && baseEntity is IOEntity iOEntity && !(iOEntity is Signage))
				{
					baseEntity.Kill();
					num++;
				}
			}
		}
		arg.ReplyWith($"Killed {num} IO entities on {array.Length} boats.");
	}

	public bool HasNonConvexColliders()
	{
		foreach (BaseEntity item in Deployables.Cached)
		{
			if (item == null || item.gameObject == null)
			{
				continue;
			}
			MeshCollider[] componentsInChildren = item.gameObject.GetComponentsInChildren<MeshCollider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!componentsInChildren[i].convex)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool DestroyNonConvexColliderDeployables()
	{
		int num = 0;
		for (int num2 = Deployables.Cached.Count - 1; num2 >= 0; num2--)
		{
			BaseEntity baseEntity = Deployables.Cached[num2];
			if (!(baseEntity == null) && !(baseEntity.gameObject == null) && !(baseEntity is global::IBoatBuildingPiece))
			{
				MeshCollider[] componentsInChildren = baseEntity.gameObject.GetComponentsInChildren<MeshCollider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (!componentsInChildren[i].convex)
					{
						num++;
						baseEntity.Kill();
						break;
					}
				}
			}
		}
		return num > 0;
	}

	public bool IsWithinDegreesOfUp(float maxDegrees)
	{
		float num = Vector3.Dot(base.transform.up, Vector3.up);
		float num2 = Mathf.Cos(maxDegrees * (MathF.PI / 180f));
		return num >= num2;
	}

	public void Teleport(Vector3 position)
	{
		base.transform.position = position;
		Rigidbody component = GetComponent<Rigidbody>();
		if (component != null)
		{
			component.velocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
		SendNetworkUpdateImmediate();
		UpdateNetworkGroup();
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		GetMountedPlayers(obj);
		foreach (BasePlayer item in obj)
		{
			item.Teleport(item.transform.position);
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public SteeringWheel GetSteeringWheel()
	{
		foreach (SteeringWheel item in SteeringWheels.Cached)
		{
			if (!(item == null))
			{
				return item;
			}
		}
		return null;
	}

	public override bool ForceChildFullStability()
	{
		if (IsDestructibleWreck)
		{
			return !UseDestructibleWreckStability;
		}
		return true;
	}

	public override float AntiHackVelocity()
	{
		if (base.isServer)
		{
			return adjustedVelocityMax.Get(force: false);
		}
		return VelocityMax;
	}

	public static bool IsChildOfInteractablePlayerBoat(BaseEntity entity)
	{
		PlayerBoat parentPlayerBoat = GetParentPlayerBoat(entity);
		if (parentPlayerBoat == null)
		{
			return false;
		}
		return !parentPlayerBoat.IsDying;
	}

	public static bool IsChildOfFinishedPlayerBoat(BaseEntity entity)
	{
		if (GetParentPlayerBoat(entity) == null)
		{
			return false;
		}
		return true;
	}

	public static bool HasPermissionToPickup(BasePlayer player, BaseEntity entity, out bool parentIsBoat)
	{
		parentIsBoat = false;
		if (player == null)
		{
			return false;
		}
		PlayerBoat parentPlayerBoat = GetParentPlayerBoat(entity);
		if (parentPlayerBoat == null)
		{
			return false;
		}
		parentIsBoat = true;
		return parentPlayerBoat.IsAuthedForBuilding(player);
	}

	public static PlayerBoat GetParentPlayerBoat(BaseEntity entity, bool includeEntityItself = false)
	{
		if (entity == null)
		{
			return null;
		}
		if (includeEntityItself && entity is PlayerBoat)
		{
			return entity as PlayerBoat;
		}
		BaseEntity baseEntity = entity.GetParentEntity();
		while (baseEntity != null)
		{
			if (baseEntity is PlayerBoat result)
			{
				return result;
			}
			entity = baseEntity;
			baseEntity = entity.GetParentEntity();
		}
		return null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		lastEditLocalPos = info.msg.playerBoat.lastEditLocalPos;
		lastEditLocalRot = info.msg.playerBoat.lastEditLocalRot;
		if (base.isServer)
		{
			rigidBody.isKinematic = true;
			if (info.fromDisk)
			{
				timeSinceLastUsed = info.msg.playerBoat.timeSinceLastUsed;
			}
		}
	}

	public void SetDimensions(Vector3 size)
	{
		bounds = new Bounds(new Vector3(0f, size.y / 8f, 0f), size);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool ForceDeployableSetParent()
	{
		return true;
	}

	public void OnSubChildAdded(BaseEntity subChild)
	{
		ListenServerColliderFix();
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		CacheChild(child);
		if (base.isServer)
		{
			if (ShouldDestroyOnDeath(child))
			{
				entitiesToDestroyOnDeath.Add(child);
			}
			if (child is IOEntity item)
			{
				ioEntitiesToDisconnectOnDeath.Add(item);
			}
		}
		if (!(child is BasePlayer))
		{
			ListenServerColliderFix();
		}
	}

	protected override void OnChildRemoved(BaseEntity child)
	{
		base.OnChildRemoved(child);
		if (base.isServer && KilledForEditMode)
		{
			return;
		}
		UnCacheChild(child);
		if (base.isServer)
		{
			if (ShouldDestroyOnDeath(child))
			{
				entitiesToDestroyOnDeath.Remove(child);
			}
			if (child is IOEntity item)
			{
				ioEntitiesToDisconnectOnDeath.Remove(item);
			}
		}
	}

	private bool ShouldDestroyOnDeath(BaseEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity is BoatBuildingBlock)
		{
			return false;
		}
		if (entity is Door)
		{
			return true;
		}
		if (entity is SimpleBuildingBlock)
		{
			return true;
		}
		if (ChildrenToDestroyOnDeath != null)
		{
			foreach (BaseEntity item in ChildrenToDestroyOnDeath)
			{
				if (!(item == null) && item.prefabID == entity.prefabID)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ListenServerColliderFix()
	{
	}

	private void CacheChild(BaseEntity child)
	{
		AddIf<Sail>(child as Sail, Sails.Cached);
		AddIf<SmallEngine>(child as SmallEngine, Engines.Cached);
		AddIf<BoatBuildingBlock>(child as BoatBuildingBlock, BoatBuildingBlocks.Cached);
		AddIf<Anchor>(child as Anchor, Anchors.Cached);
		AddIf<SteeringWheel>(child as SteeringWheel, SteeringWheels.Cached);
		if (!(child is BoatBuildingBlock) && !(child is BasePlayer) && !(child is DroppedItem))
		{
			AddIf<BaseEntity>(child, Deployables.Cached);
		}
		static bool AddIf<T>(T value, List<T> list) where T : BaseEntity
		{
			if (value != null && !list.Contains(value))
			{
				list.Add(value);
				return true;
			}
			return false;
		}
	}

	private void UnCacheChild(BaseEntity child)
	{
		RemoveIf<Sail>(child as Sail, Sails.Cached);
		RemoveIf<SmallEngine>(child as SmallEngine, Engines.Cached);
		RemoveIf<BoatBuildingBlock>(child as BoatBuildingBlock, BoatBuildingBlocks.Cached);
		RemoveIf<Anchor>(child as Anchor, Anchors.Cached);
		RemoveIf<SteeringWheel>(child as SteeringWheel, SteeringWheels.Cached);
		if (!(child is BoatBuildingBlock) && !(child is BasePlayer) && !(child is DroppedItem))
		{
			RemoveIf<BaseEntity>(child, Deployables.Cached);
		}
		static void RemoveIf<T>(T value, List<T> list) where T : BaseEntity
		{
			if (value != null)
			{
				list.Remove(value);
			}
		}
	}

	protected override bool IgnoreChildEntitiesForDismountClipChecks()
	{
		return true;
	}

	protected override bool DismountCheckSkipVehicles()
	{
		return false;
	}

	public bool CanStartEditing(BasePlayer player, bool sendErrorToasts)
	{
		object obj = Interface.CallHook("CanEditPlayerBoat", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (IsDying)
		{
			return false;
		}
		if (base.isServer && rigidBody.linearVelocity.magnitude >= MaxEditVelocity)
		{
			if (sendErrorToasts)
			{
				player.ShowToast(GameTip.Styles.Error, tooFastToEditErrorPhrase, false);
			}
			return false;
		}
		if (IsOnDamagedCoolDown())
		{
			if (base.isServer && sendErrorToasts)
			{
				player.ShowToast(GameTip.Styles.Error, recentlyDamagedCantEditPhrase, false);
			}
			return false;
		}
		if (!IsPlayerAuthed(player, authedIfNoPriveOrLock: true))
		{
			return false;
		}
		return true;
	}

	public bool IsOnDamagedCoolDown()
	{
		if (base.isServer)
		{
			if (base.SecondsSinceAttacked <= GetDamageRepairCooldown())
			{
				return UnityEngine.Time.time - boatSpawnTime > GetDamageRepairCooldown();
			}
			return false;
		}
		return false;
	}

	public override Vector3 GetDismountCheckStart(BasePlayer player)
	{
		List<BoatBuildingBlock> cached = BoatBuildingBlocks.Cached;
		if (cached == null || cached.Count == 0)
		{
			return base.GetDismountCheckStart(player);
		}
		TriggerLadder triggerLadder = player.FindTrigger<TriggerLadder>();
		if ((bool)triggerLadder)
		{
			return GameObjectEx.ToBaseEntity(triggerLadder.gameObject).CenterPoint();
		}
		Vector3 vector = player.TriggerPoint();
		int num = -1;
		float num2 = float.MaxValue;
		for (int i = 0; i < cached.Count; i++)
		{
			BoatBuildingBlock boatBuildingBlock = cached[i];
			if (boatBuildingBlock == null)
			{
				continue;
			}
			Transform[] dismountPoints = boatBuildingBlock.DismountPoints;
			if (dismountPoints == null || dismountPoints.Length != 0)
			{
				float num3 = Vector3.SqrMagnitude(vector - boatBuildingBlock.GetDismountCheckStart());
				if (num3 < num2)
				{
					num = i;
					num2 = num3;
				}
			}
		}
		Debug.Assert(num != -1);
		return cached[num].GetDismountCheckStart();
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		if (IsOn())
		{
			return false;
		}
		if (Anchored)
		{
			return false;
		}
		if (pusher.OnLadder())
		{
			return false;
		}
		if (pusher.GetParentEntity() == this)
		{
			return false;
		}
		if (!IsFlipped() && pusher.IsStandingOnEntity(this, 1218652417))
		{
			return false;
		}
		if (pusher.IsBuildingBlockedByVehicle())
		{
			return false;
		}
		if (IsDying)
		{
			return false;
		}
		if (!pusher.isMounted)
		{
			return base.healthFraction > 0f;
		}
		return false;
	}

	public bool IsOnDeployAndEditCoolDown()
	{
		if (!IsBusy())
		{
			return BoatBuildingStation.IsOnGlobalEditFinishCoolDown();
		}
		return true;
	}

	public bool CanDeployAndEdit(BasePlayer player, out Vector3 outPosition, out Quaternion outRotation, bool checkForCoolDown, bool checkForItem, bool checkDeploy, bool sendErrorToasts)
	{
		outPosition = Vector3.zero;
		outRotation = Quaternion.identity;
		if (!EditEnabled)
		{
			return false;
		}
		if (checkForCoolDown && IsOnDeployAndEditCoolDown())
		{
			if (sendErrorToasts && player != null)
			{
				player.ShowToast(GameTip.Styles.Error, onDeployEditCoolDownPhrase, false);
			}
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (player.GetParentEntity() != this)
		{
			return false;
		}
		if (!player.CanInteract())
		{
			return false;
		}
		if (DeepSeaManager.IsInsideDeepSea(base.transform.position))
		{
			return false;
		}
		if (checkForItem && !PlayerHasBBSItem(player))
		{
			return false;
		}
		if (!CanStartEditing(player, sendErrorToasts))
		{
			return false;
		}
		if (checkDeploy)
		{
			GetDeployAndEditPositionRotation(out outPosition, out outRotation);
			if (!ContainerCorpse.IsValidPointForEntity(BoatBuildingStationPrefab.resourceID, outPosition, outRotation, this, -1, ignoreChildrenOfEntity: true))
			{
				if (sendErrorToasts && player != null)
				{
					player.ShowToast(GameTip.Styles.Error, invalidDeployLocationPhrase, false);
				}
				return false;
			}
		}
		return true;
	}

	public bool PlayerHasBBSItem(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		return player.inventory.GetAmount(BoatBuildingStationItem) > 0;
	}

	public void GetDeployAndEditPositionRotation(out Vector3 outPosition, out Quaternion outRotation)
	{
		Vector3 vector = base.transform.position.WithY(Env.oceanlevel);
		Quaternion quaternion = Quaternion.Euler(base.transform.eulerAngles.WithXZ(0f, 0f));
		outPosition = vector + quaternion * lastEditLocalPos;
		outRotation = quaternion * Quaternion.Euler(lastEditLocalRot);
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (!base.isClient)
		{
			ProcessCollision(collision);
		}
	}

	private void ProcessCollision(Collision collision)
	{
		if (!base.isClient && collision != null && !(collision.gameObject == null) && !(collision.gameObject == null))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(collision.gameObject);
			if (Interface.CallHook("OnPlayerBoatCollide", this, baseEntity, collision) == null && baseEntity != null && !baseEntity.isClient && baseEntity is IDestroyableOnPlayerBoatCollision destroyableOnPlayerBoatCollision && destroyableOnPlayerBoatCollision.ShouldBeDestroyedBy(this))
			{
				baseEntity.Kill(DestroyMode.Gib);
			}
		}
	}

	public override EntityBuildCost BuildCost()
	{
		return new EntityBuildCost(DynamicBuildCost);
	}
}

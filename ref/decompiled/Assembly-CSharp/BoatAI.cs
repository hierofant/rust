using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using Rust;
using Rust.Ai;
using Rust.Assertions;
using Rust.Safety;
using Unity.Collections;
using UnityEngine;

public class BoatAI : BaseEntity
{
	private struct FeelerResult
	{
		public Vector3 direction;

		public RaycastHit? hit;
	}

	public struct Context
	{
		public float[] InterestMap;

		public float[] DangerMap;

		public Context(int resolution)
		{
			InterestMap = new float[resolution];
			DangerMap = new float[resolution];
		}
	}

	public abstract class BoatState
	{
		public abstract void Enter(BoatAI boatAI);

		public abstract void Update(Context ctx, BoatAI boatAI, float delta);

		public abstract void Exit(BoatAI boatAI);

		public abstract string GetStateName();
	}

	public class IdleState : BoatState
	{
		public override void Enter(BoatAI boatAI)
		{
			boatAI.StartEngine(boatAI.Boat);
			boatAI.SwitchState(boatAI._wanderState);
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Idle";
		}
	}

	public class WaitState : BoatState
	{
		private const float MAX_WAIT_TIME = 60f;

		private const float MIN_WAIT_TIME = 10f;

		private TimeSince _timeSinceEntered;

		private float _timeToWait;

		public override void Enter(BoatAI boatAI)
		{
			_timeSinceEntered = 0f;
			_timeToWait = UnityEngine.Random.Range(10f, 60f);
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			if ((float)_timeSinceEntered >= _timeToWait)
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Wait";
		}
	}

	public class WanderState : BoatState
	{
		private float _wanderAngle;

		private float _targetAngle;

		private TimeSince _timeSinceLastAngleChange;

		private Vector3 _macroTarget;

		private TimeSince _timeSinceNewTarget;

		private float _nextMacroInterval;

		private bool _isInDeepsea;

		private TimeSince _timeSinceEntered;

		private float _timeToWait;

		public override void Enter(BoatAI boatAI)
		{
			_timeSinceEntered = 0f;
			_timeToWait = UnityEngine.Random.Range(120f, 320f);
			_wanderAngle = UnityEngine.Random.Range(-5f, 5f);
			_targetAngle = _wanderAngle;
			_timeSinceLastAngleChange = 0f;
			_nextMacroInterval = UnityEngine.Random.Range(60f, 150f);
			_timeSinceNewTarget = _nextMacroInterval + 1f;
			if (PointEntity<DeepSeaManager>.ServerInstance != null)
			{
				_isInDeepsea = DeepSeaManager.IsInsideDeepSea(boatAI.transform.position);
			}
		}

		private void CheckLeaveState(BoatAI boatAI)
		{
			if ((float)_timeSinceEntered >= _timeToWait)
			{
				boatAI.SwitchState(boatAI._waitState);
			}
			if (boatAI.HasProtectionArea)
			{
				boatAI.SwitchState(boatAI._orbitState);
			}
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			CheckLeaveState(boatAI);
			boatAI.MaintainGroupCohesion(ctx);
			if ((float)_timeSinceNewTarget > _nextMacroInterval)
			{
				PickNewMacroTarget(boatAI);
			}
			if ((float)_timeSinceLastAngleChange > 0.5f)
			{
				_targetAngle = UnityEngine.Random.Range(-5f, 5f);
				_timeSinceLastAngleChange = 0f;
			}
			_wanderAngle = Mathf.Lerp(_wanderAngle, _targetAngle, delta * 0.5f);
			Vector3 vector = Quaternion.AngleAxis(_wanderAngle, Vector3.up) * boatAI._boat.transform.forward;
			Vector3 zero = Vector3.zero;
			if (_macroTarget != Vector3.zero)
			{
				zero = _macroTarget - boatAI.transform.position;
				zero.y = 0f;
				if (zero.sqrMagnitude > 100f)
				{
					zero.Normalize();
					boatAI.AddContextInterest(ctx, zero, 0.3f);
				}
			}
			boatAI.AddContextInterest(ctx, vector, 0.15f, 2);
			if (_isInDeepsea && PointEntity<DeepSeaManager>.ServerInstance != null)
			{
				Vector3 vector2 = DeepSeaManager.DeepSeaBounds.center - boatAI.transform.position;
				vector2.y = 0f;
				if (vector2.magnitude > 3000f)
				{
					Vector3 normalized = vector2.normalized;
					Vector3 worldDirection = Vector3.Lerp(vector, normalized, 0.75f);
					boatAI.AddContextInterest(ctx, worldDirection, 0.5f, 0);
				}
			}
			if (boatAI.HasProtectionArea)
			{
				Vector3 vector3 = boatAI.ProtectionCenter - boatAI.transform.position;
				vector3.y = 0f;
				if (vector3.magnitude > 20f)
				{
					Vector3 normalized2 = vector3.normalized;
					Vector3 worldDirection2 = Vector3.Lerp(vector, normalized2, 0.75f);
					boatAI.AddContextInterest(ctx, worldDirection2, 0.5f, 0);
				}
			}
			if (_macroTarget != Vector3.zero && Vector3Ex.Distance2D(boatAI.transform.position, _macroTarget) < 25f)
			{
				PickNewMacroTarget(boatAI);
			}
		}

		private void PickNewMacroTarget(BoatAI boatAI)
		{
			_timeSinceNewTarget = 0f;
			_nextMacroInterval = UnityEngine.Random.Range(60f, 150f);
			if (boatAI.HasProtectionArea)
			{
				Vector3 protectionCenter = boatAI.ProtectionCenter;
				_macroTarget = protectionCenter + new Vector3(UnityEngine.Random.Range(0f - boatAI.ProtectionRadius, boatAI.ProtectionRadius), 0f, UnityEngine.Random.Range(0f - boatAI.ProtectionRadius, boatAI.ProtectionRadius));
				return;
			}
			if (PointEntity<DeepSeaManager>.ServerInstance == null)
			{
				_macroTarget = boatAI.transform.position + boatAI._boat.transform.forward * 100f;
				return;
			}
			Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
			Vector3 min = deepSeaBounds.min;
			Vector3 max = deepSeaBounds.max;
			_macroTarget = new Vector3(UnityEngine.Random.Range(min.x, max.x), boatAI.transform.position.y, UnityEngine.Random.Range(min.z, max.z));
		}

		public override void Exit(BoatAI boatAI)
		{
			_macroTarget = Vector3.zero;
		}

		public override string GetStateName()
		{
			return "Wander";
		}
	}

	public class SeekState : BoatState
	{
		public override void Enter(BoatAI boatAI)
		{
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			if (!(boatAI._boat == null))
			{
				IAITarget activeTarget = boatAI.ActiveTarget;
				if (activeTarget == null)
				{
					boatAI.SwitchState(boatAI._wanderState);
				}
				else if (!activeTarget.Position.HasValue)
				{
					boatAI.ClearCurrentTarget();
					boatAI.SwitchState(boatAI._wanderState);
				}
				else if (activeTarget.IsReached(boatAI))
				{
					boatAI.ClearCurrentTarget();
					boatAI.SwitchState(boatAI._wanderState);
				}
				else
				{
					Vector3 normalized = (activeTarget.Position.Value - boatAI._boat.transform.position).normalized;
					normalized.y = 0f;
					boatAI.AddContextInterest(ctx, normalized, 1f, 5);
				}
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Seek";
		}
	}

	public class DriveByState : BoatState
	{
		private bool _beenClose;

		private const float PASS_DISTANCE = 25f;

		private const float AIM_AHEAD = 15f;

		public override void Enter(BoatAI boatAI)
		{
			_beenClose = false;
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			if (boatAI._boat == null)
			{
				return;
			}
			if (!(boatAI.ActiveTarget is PlayerTarget { Position: var position } playerTarget))
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
			else
			{
				if (!position.HasValue)
				{
					return;
				}
				Vector3 position2 = playerTarget.Player.transform.position;
				Vector3 vector = position2 - boatAI._boat.transform.position;
				Vector3 normalized = vector.normalized;
				Vector3 vector2 = position2 + normalized * 15f;
				Vector3 normalized2 = Vector3.Cross(Vector3.up, normalized).normalized;
				Vector3 vector3 = ((Vector3.Dot(boatAI._boat.transform.forward, normalized2) > 0f) ? normalized2 : (-normalized2)) * 25f;
				Vector3 vector4 = vector2 + vector3;
				Vector3 normalized3 = (vector4 - boatAI._boat.transform.position).normalized;
				normalized3.y = 0f;
				boatAI.AddContextInterest(ctx, normalized3, 1f, 2);
				float num = Vector3Ex.Distance2D(boatAI._boat.transform.position, vector4);
				if (_beenClose)
				{
					if (Vector3.Dot(boatAI._boat.transform.forward, vector.normalized) < 0f)
					{
						boatAI.SwitchState(boatAI._seekState);
					}
					else if (num > 20f)
					{
						boatAI.SwitchState(boatAI._seekState);
					}
				}
				else if (num < 10f)
				{
					_beenClose = true;
				}
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "DriveBy";
		}
	}

	public class RamState : BoatState
	{
		private TimeSince _timeSinceStartedRam;

		private const float RAM_DURATION = 20f;

		public override void Enter(BoatAI boatAI)
		{
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			if (boatAI._boat == null)
			{
				return;
			}
			PlayerTarget playerTarget = boatAI.ActiveTarget as PlayerTarget;
			bool flag = playerTarget != null && playerTarget.IsValid(boatAI) && playerTarget.Position.HasValue && Mathf.Abs(playerTarget.Position.Value.y - boatAI._boat.transform.position.y) > 15f;
			if (playerTarget == null || flag)
			{
				if (PRINT_DEBUGS)
				{
					Debug.Log("Leaving ram state");
				}
				boatAI.SwitchState(boatAI._seekState);
			}
			else if (playerTarget.Position.HasValue)
			{
				Vector3 rhs = playerTarget.Position.Value - boatAI._boat.transform.position;
				Vector3 normalized = rhs.normalized;
				normalized.y = 0f;
				boatAI.AddContextInterest(ctx, normalized, 1f, 0);
				if (Vector3Ex.Distance2D(boatAI._boat.transform.position, playerTarget.Position.Value) < 2f && Vector3.Dot(boatAI._boat.transform.forward, rhs) < 0f)
				{
					boatAI.SwitchState(boatAI._seekState);
				}
			}
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Ram";
		}
	}

	public class OrbitState : BoatState
	{
		private const float TAU = MathF.PI * 2f;

		private const float CIRCLE_RESOLUTION = 45f;

		private const float ORBIT_RADIUS = 30f;

		private const float ORBIT_STEP = 1f / 45f;

		private float orbitPercent;

		private Vector3 currentOrbitTargetPoint;

		public override void Enter(BoatAI boatAI)
		{
			orbitPercent = 0f;
			currentOrbitTargetPoint = GetRandomPointOnCircle(boatAI.ProtectionCenter, 80f);
		}

		public override void Update(Context ctx, BoatAI boatAI, float delta)
		{
			if (boatAI._boat == null)
			{
				return;
			}
			if (!boatAI.HasProtectionArea)
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
			if (Vector3.Distance(boatAI.transform.position, currentOrbitTargetPoint) < 60f)
			{
				orbitPercent += 1f / 45f;
				if (orbitPercent > 1f)
				{
					orbitPercent -= 1f;
				}
				float radAngle = orbitPercent * (MathF.PI * 2f);
				currentOrbitTargetPoint = GetPointOnCircle(boatAI.ProtectionCenter, 80f, radAngle);
			}
			Vector3 normalized = (currentOrbitTargetPoint - boatAI.transform.position).normalized;
			boatAI.AddContextInterest(ctx, normalized, 0.5f, 2);
		}

		private Vector3 GetRandomPointOnCircle(Vector3 centre, float radius)
		{
			float radAngle = UnityEngine.Random.Range(0f, MathF.PI * 2f);
			return GetPointOnCircle(centre, radius, radAngle);
		}

		private Vector3 GetPointOnCircle(Vector3 centre, float radius, float radAngle)
		{
			float x = Mathf.Cos(radAngle) * radius;
			float z = Mathf.Sin(radAngle) * radius;
			return centre + new Vector3(x, 0f, z);
		}

		public override void Exit(BoatAI boatAI)
		{
		}

		public override string GetStateName()
		{
			return "Orbit";
		}
	}

	public enum AILoadMode
	{
		LoadAi,
		KillAi,
		KillBoat
	}

	private class InputProvider : IAiInputProvider
	{
		private BoatAI _boatAI;

		public InputProvider(BoatAI boatAI)
		{
			_boatAI = boatAI;
		}

		public void OnAdd(BaseVehicle vehicle)
		{
			_boatAI.OnAdd(vehicle);
			vehicle.BeenAttacked += _boatAI.BoatAttacked;
			vehicle.Died += _boatAI.BoatDied;
			vehicle.OnDismountAll += _boatAI.KillAllRemainingScientists;
			_boatAI.OnAttached();
		}

		public void OnTick(BaseVehicle vehicle, float delta, ref float steering, ref float gasPedal)
		{
			BoatWorkQueue.Add(new BoatAIInstruction
			{
				AI = _boatAI,
				Delta = delta
			});
		}

		public void OnRemove(BaseVehicle vehicle)
		{
			_boatAI.OnRemove(vehicle);
			vehicle.BeenAttacked -= _boatAI.BoatAttacked;
			vehicle.Died -= _boatAI.BoatDied;
			vehicle.OnDismountAll -= _boatAI.KillAllRemainingScientists;
		}

		public void OnTick(BaseVehicle vehicle, float delta)
		{
		}
	}

	public struct BoatAIInstruction : IEquatable<BoatAIInstruction>
	{
		public BoatAI AI;

		public float Delta;

		public bool Equals(BoatAIInstruction other)
		{
			return (object)AI == other.AI;
		}

		public override bool Equals(object obj)
		{
			if (obj is BoatAIInstruction other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (!(AI != null))
			{
				return 0;
			}
			return AI.GetHashCode();
		}
	}

	public class BoatAIWorkQueue : ObjectWorkQueue<BoatAIInstruction>
	{
		protected override void RunJob(BoatAIInstruction instruction)
		{
			if (ShouldAdd(instruction))
			{
				instruction.AI.OnTick(instruction.AI.Boat, instruction.Delta, ref instruction.AI._boat.steering, ref instruction.AI._boat.gasPedal);
			}
		}

		protected override bool ShouldAdd(BoatAIInstruction instruction)
		{
			if (base.ShouldAdd(instruction))
			{
				return instruction.AI.IsValid();
			}
			return false;
		}

		protected override bool IsValidToRun(BoatAIInstruction entity)
		{
			return true;
		}
	}

	private float? _closestObstacle;

	private FeelerResult[] feelers = new FeelerResult[8];

	private TimeSince timeSinceAvoidanceUpdate;

	private const int MAX_HITS_PER_TRACE = 8;

	private static readonly List<Vector3> _contextMap = new List<Vector3>();

	private Context _bufferContext;

	private int _lastBestIndex;

	private const float LEAVE_WANDER_TIME_MIN = 120f;

	private const float LEAVE_WANDER_TIME_MAX = 320f;

	private const float SMALL_WANDER_CHANGE_INTERVAL = 0.5f;

	private const float WANDER_ANGLE_CHANGE = 5f;

	private const float MACRO_TARGET_INTERVAL_MIN = 60f;

	private const float MACRO_TARGET_INTERVAL_MAX = 150f;

	private const float MACRO_TARGET_PULL_STRENGTH = 0.3f;

	private const float DEEPSEA_CENTER_PULL_START = 3000f;

	private const float DEEPSEA_CENTER_PULL_STRENGTH = 0.75f;

	private const float PROTECTION_AREA_CENTER_PULL_START = 20f;

	private const float PROTECTION_AREA_PULL_STRENGTH = 0.75f;

	private const float GROUP_COHESION_RADIUS = 30f;

	private const float GROUP_PULL_STRENGTH = 0.25f;

	private const int CONTEXT_RESOLUTION = 8;

	[SerializeField]
	[Header("Boat AI - Scientists")]
	private bool _autoFillWithScientists;

	[SerializeField]
	private GameObjectRef _scientistPrefab;

	private HumanNPC _driverNpc;

	private Dictionary<HumanNPC, MountedWeaponSeat> _turretNpcs = new Dictionary<HumanNPC, MountedWeaponSeat>();

	private bool _hasSpawnedScientists;

	private bool _hasKilledScientists;

	private List<AiMountedWeaponController> _mountedWeaponControllers;

	private BoatState _currentState;

	private IdleState _idleState;

	private WanderState _wanderState;

	private WaitState _waitState;

	private SeekState _seekState;

	private DriveByState _driveByState;

	private RamState _ramState;

	private OrbitState _orbitState;

	public const string DeepSeaRHIBPath = "assets/content/vehicles/boats/rhib/rhib.deepsea.prefab";

	public const string DeepSeaPTBoatPath = "assets/content/vehicles/boats/ptboat/ptboat.deepsea.prefab";

	[SerializeField]
	[Header("Boat AI")]
	private BaseBoat _boat;

	[Header("Boat AI - General")]
	[SerializeField]
	private bool _autoInit;

	[SerializeField]
	private bool _autoPursue;

	[SerializeField]
	private float _thinkTime = 5f;

	[SerializeField]
	private float _searchRange = 50f;

	[Header("Boat AI - Collision Avoidance")]
	[SerializeField]
	private float _awarenessAngle;

	[SerializeField]
	private float _awarenessDistance;

	[Header("Boat AI - Debug")]
	[SerializeField]
	private Transform _debugMoveTo;

	[ServerVar(Help = "(Generated) When enabled, draws DDraw visualisations of boat AI steering, avoidance, and pathfinding state")]
	public static bool DRAW_DEBUGS = false;

	[ServerVar(Help = "(Generated) When enabled, logs verbose boat AI decision-making output to the server console each AI tick")]
	public static bool PRINT_DEBUGS = false;

	[ServerVar(Help = "Distance players need to be to start syncing mounted seats")]
	public static float enable_mount_sync_distance = 750f;

	[ServerVar(Help = "How often to update the avoidance cache. Lower number means a more accurate cache at the expensive of performance.", ShowInAdminUI = true)]
	public static float avoidance_update_interval = 0.8f;

	[ServerVar(Help = "How long per frame to spend on boat ai", Saved = true, ShowInAdminUI = true)]
	public static float boat_ai_frame_budget_ms = 0.3f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "(Generated) Maximum speed as a fraction of the boat's top speed that AI-controlled boats will use; default 0.9; saved and shown in admin UI")]
	public static float max_speed_percentage = 0.9f;

	private int _driveDirection = 1;

	private float _driveLockTimer;

	private float _stuckTimer;

	private Vector3 _lastPos;

	[ServerVar(Help = "(Generated) When enabled, boat AI entities can enter a sleep state when no players are nearby; reduce CPU usage for idle boats")]
	public static bool allow_sleeping = false;

	[ServerVar(Help = "(Generated) Number of seconds a boat AI will wait without player interaction before entering sleep mode; default 30s")]
	public static float seconds_until_sleep = 30f;

	private const float AI_SPAWN_DELAY = 5f;

	private InputProvider _provider;

	private TimeSince _timeSinceThought;

	private TimeSince _timeSinceSleepy;

	private TimeSince _timeSinceSpawned;

	private bool _isSleeping;

	private bool _setupRan;

	private ScientistBoatOilrigManager _oilrigManager;

	public static BoatAIWorkQueue BoatWorkQueue = new BoatAIWorkQueue();

	private int __sync_GroupId;

	private NetworkableId __sync_BoatID;

	private int __sync_LoadModeSync;

	public bool HasProtectionArea => ProtectionRadius > 0f;

	public float SearchRange => _searchRange;

	public MotorRowboat Boat { get; private set; }

	public IAITarget ActiveTarget { get; set; }

	public float PursuitTargetAcquireTime { get; set; }

	public bool InGroup => GroupId != -1;

	[Sync(Autosave = true)]
	public int GroupId
	{
		[CompilerGenerated]
		get
		{
			return __sync_GroupId;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_GroupId, value))
			{
				__sync_GroupId = value;
				byte nameID = __GetWeaverID("GroupId");
				QueueSyncVar(nameID);
			}
		}
	}

	public Vector3 ProtectionCenter { get; set; }

	public float ProtectionRadius { get; set; }

	[Sync(Autosave = true)]
	private NetworkableId BoatID
	{
		[CompilerGenerated]
		get
		{
			return __sync_BoatID;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_BoatID, value))
			{
				__sync_BoatID = value;
				byte nameID = __GetWeaverID("BoatID");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	private int LoadModeSync
	{
		[CompilerGenerated]
		get
		{
			return __sync_LoadModeSync;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_LoadModeSync, value))
			{
				__sync_LoadModeSync = value;
				byte nameID = __GetWeaverID("LoadModeSync");
				QueueSyncVar(nameID);
			}
		}
	}

	public AILoadMode LoadMode
	{
		get
		{
			return (AILoadMode)LoadModeSync;
		}
		set
		{
			LoadModeSync = (int)value;
		}
	}

	private void FillRaycastCommands(NativeArray<RaycastCommand> allRaycasts)
	{
		Vector3 position = _boat.transform.position;
		QueryParameters queryParameters = new QueryParameters(1218781441);
		for (int i = 0; i < _contextMap.Count; i++)
		{
			Vector3 direction = _contextMap[i];
			allRaycasts[i] = new RaycastCommand(position, direction, queryParameters, _awarenessDistance);
		}
	}

	private void ProcessHits(NativeArray<RaycastHit> hits, in FeelerResult[] results, int rayCount)
	{
		Assert.That(results.Length == rayCount);
		_closestObstacle = null;
		for (int i = 0; i < rayCount; i++)
		{
			int num = i * 8;
			RaycastHit? hit = null;
			for (int j = 0; j < 8; j++)
			{
				RaycastHit raycastHit = hits[num + j];
				if (!(raycastHit.collider == null) && (!ColliderEx.IsOnLayer(raycastHit.collider, Rust.Layer.Trigger) || raycastHit.collider.CompareTag("BoatAIAvoid")))
				{
					BaseEntity entity = RaycastHitEx.GetEntity(raycastHit);
					if (!(entity == this) && !(raycastHit.collider.transform.root == _boat.transform) && (!Check.EntityValid(entity) || !Check.EntityIsClient(entity)) && (!hit.HasValue || raycastHit.distance < hit.Value.distance))
					{
						hit = raycastHit;
					}
				}
			}
			results[i] = new FeelerResult
			{
				direction = _contextMap[i],
				hit = hit
			};
			if (hit.HasValue && (!_closestObstacle.HasValue || hit.Value.distance < _closestObstacle.Value))
			{
				_closestObstacle = hit.Value.distance;
			}
		}
	}

	private void AvoidObstacles(Context ctx)
	{
		using (TimeWarning.New("BoatAI.AvoidObstacles"))
		{
			if ((float)timeSinceAvoidanceUpdate >= avoidance_update_interval + UnityEngine.Random.Range(-0.1f, 0.1f))
			{
				timeSinceAvoidanceUpdate = 0f;
				_closestObstacle = null;
				int count = _contextMap.Count;
				if (feelers == null || feelers.Length != count)
				{
					feelers = new FeelerResult[count];
				}
				using NativeArray<RaycastCommand> nativeArray = new NativeArray<RaycastCommand>(count, Allocator.TempJob);
				FillRaycastCommands(nativeArray);
				using NativeArray<RaycastHit> hits = new NativeArray<RaycastHit>(count * 8, Allocator.TempJob);
				GamePhysics.TraceRaysUnordered(nativeArray, hits, 8, traceWater: false);
				ProcessHits(hits, in feelers, count);
			}
			FeelerResult[] array = feelers;
			for (int i = 0; i < array.Length; i++)
			{
				FeelerResult feelerResult = array[i];
				if (feelerResult.hit.HasValue)
				{
					float strength = Mathf.Clamp01(1f - feelerResult.hit.Value.distance / _awarenessDistance);
					AddContextDanger(ctx, feelerResult.direction, strength, 0);
				}
			}
		}
	}

	public void MoveTo(Vector3 pos, float stopRadius = 80f, Action onArrived = null)
	{
		SetMoveCommand(new PointTarget(pos, stopRadius));
	}

	public void MoveTo(Transform t, float stopRadius = 3f)
	{
		SetMoveCommand(new TransformTarget(t, stopRadius));
	}

	private void SetMoveCommand(IAITarget t)
	{
		ActiveTarget = t;
		EnableScientistBrains();
		RefreshSleeping();
		if (t is PlayerTarget playerTarget)
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log("Found target - " + playerTarget.Player.displayName);
			}
			GiveMountedWeaponsExtraTarget(playerTarget.Player);
			if (playerTarget.StayClose && UnityEngine.Random.value >= 0.8f)
			{
				SwitchState(_ramState);
				return;
			}
		}
		SwitchState(_seekState);
	}

	private bool HasValidPursuit()
	{
		if (ActiveTarget != null)
		{
			return ActiveTarget.IsValid(this);
		}
		return false;
	}

	public void SetProtectionArea(Vector3 center, float radius)
	{
		ProtectionCenter = center;
		ProtectionRadius = radius;
	}

	private void ClearCurrentTarget()
	{
		if (PRINT_DEBUGS)
		{
			Debug.Log("attempt to release target");
		}
		if (ActiveTarget != null && ActiveTarget is PlayerTarget playerTarget)
		{
			BoatAICoordination.ReleaseClaim(this, playerTarget.Player);
		}
		_timeSinceSleepy = 0f;
		ActiveTarget = null;
		DisableScientistBrains();
		GiveMountedWeaponsExtraTarget(null);
	}

	private void ResetContext(Context ctx)
	{
		for (int i = 0; i < ctx.InterestMap.Length; i++)
		{
			ctx.InterestMap[i] = 0f;
			ctx.DangerMap[i] = 0f;
		}
	}

	private void InitialiseContextMaps()
	{
		if (_contextMap.Count != 8)
		{
			_contextMap.Clear();
			float num = 45f;
			for (int i = 0; i < 8; i++)
			{
				Vector3 item = Quaternion.AngleAxis((float)i * num, Vector3.up) * Vector3.forward;
				_contextMap.Add(item);
			}
		}
		_bufferContext = new Context(8);
	}

	private int GetContextIndex(Vector3 worldDirection)
	{
		Vector3 normalized = worldDirection.normalized;
		normalized.y = 0f;
		int result = 0;
		float num = float.NegativeInfinity;
		for (int i = 0; i < _contextMap.Count; i++)
		{
			float num2 = Vector3.Dot(normalized, _contextMap[i]);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	private void BlurContext(Context ctx, int iterations = 1)
	{
		int num = ctx.InterestMap.Length;
		BufferList<float> obj = Facepunch.Pool.Get<BufferList<float>>();
		BufferList<float> obj2 = Facepunch.Pool.Get<BufferList<float>>();
		obj.Clear();
		obj2.Clear();
		for (int i = 0; i < num; i++)
		{
			obj.Add(0f);
			obj2.Add(0f);
		}
		for (int j = 0; j < iterations; j++)
		{
			for (int k = 0; k < num; k++)
			{
				int num2 = (k - 1 + num) % num;
				int num3 = (k + 1) % num;
				obj[k] = (ctx.InterestMap[num2] + ctx.InterestMap[k] + ctx.InterestMap[num3]) / 3f;
				obj2[k] = (ctx.DangerMap[num2] + ctx.DangerMap[k] + ctx.DangerMap[num3]) / 3f;
			}
			for (int l = 0; l < num; l++)
			{
				ctx.InterestMap[l] = obj[l];
				ctx.DangerMap[l] = obj2[l];
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		Facepunch.Pool.FreeUnmanaged(ref obj2);
	}

	private Vector3 GetContextDirection(int index)
	{
		return _contextMap[index];
	}

	private Vector3 GetBestContextDirection()
	{
		using (TimeWarning.New("BoatAI.Context.GetBestContextDirection"))
		{
			if (_contextMap.Count == 0)
			{
				return base.transform.forward;
			}
			BlurContext(_bufferContext);
			int num = -1;
			float num2 = float.MinValue;
			for (int i = 0; i < _bufferContext.InterestMap.Length; i++)
			{
				float num3 = _bufferContext.InterestMap[i];
				float num4 = _bufferContext.DangerMap[i];
				float num5 = num3 - num4;
				if ((double)num5 > (double)num2 + 0.05)
				{
					num2 = num5;
					num = i;
				}
			}
			if (num == -1 || num2 <= 0.01f)
			{
				return Vector3.zero;
			}
			_ = _lastBestIndex;
			_lastBestIndex = num;
			return _contextMap[num];
		}
	}

	private void AddContextInterest(Context context, Vector3 worldDirection, float strength, int spread = 1)
	{
		int contextIndex = GetContextIndex(worldDirection);
		for (int i = -spread; i <= spread; i++)
		{
			int num = (contextIndex + i + context.InterestMap.Length) % context.InterestMap.Length;
			if (num >= 0 && num < context.InterestMap.Length)
			{
				float num2 = 1f - (float)Mathf.Abs(i) / (float)(spread + 1);
				float num3 = strength * num2;
				context.InterestMap[num] += num3;
			}
		}
	}

	private void AddContextDanger(Context context, Vector3 worldDirection, float strength, int spread = 1)
	{
		int contextIndex = GetContextIndex(worldDirection);
		for (int i = -spread; i <= spread; i++)
		{
			int num = (contextIndex + i + context.DangerMap.Length) % context.DangerMap.Length;
			if (num >= 0 && num < context.DangerMap.Length)
			{
				float num2 = 1f - (float)Mathf.Abs(i) / (float)(spread + 1);
				float num3 = strength * num2;
				context.DangerMap[num] += num3;
			}
		}
	}

	private IEnumerator SpawnAllScientists()
	{
		yield return new WaitForSeconds(1f);
		int num = 0;
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if (mountable is RHIBDriver || mountable is MountedWeaponSeat)
			{
				SpawnScientist(mountable);
				continue;
			}
			if (num <= 0)
			{
				SpawnScientist(mountable);
			}
			else if (UnityEngine.Random.value <= 0.5f)
			{
				SpawnScientist(mountable);
			}
			num++;
		}
		yield return new WaitForEndOfFrame();
		CacheImportantScientists();
		_hasSpawnedScientists = true;
		DisableScientistBrains();
		_mountedWeaponControllers = new List<AiMountedWeaponController>();
		_mountedWeaponControllers = (from seat in _turretNpcs.Values
			select seat.MountedWeaponGameObject.GetComponent<AiMountedWeaponController>() into controller
			where controller != null
			select controller).ToList();
	}

	public void GiveMountedWeaponsExtraTarget(BasePlayer ply)
	{
		if (_mountedWeaponControllers == null)
		{
			return;
		}
		foreach (AiMountedWeaponController mountedWeaponController in _mountedWeaponControllers)
		{
			mountedWeaponController.SetExtraTarget(ply);
		}
	}

	public bool IsScientistDriverDead()
	{
		if (!_hasSpawnedScientists)
		{
			return false;
		}
		if (!(_driverNpc == null))
		{
			if (_driverNpc != null)
			{
				return !_driverNpc.IsAlive();
			}
			return false;
		}
		return true;
	}

	private void CacheImportantScientists()
	{
		foreach (BaseVehicle.MountPointInfo allMountPoint in _boat.allMountPoints)
		{
			BasePlayer mounted = allMountPoint.mountable.GetMounted();
			if (allMountPoint.mountable is RHIBDriver)
			{
				mounted.inventory.containerBelt.Clear();
				mounted.inventory.containerMain.Clear();
				_driverNpc = mounted as HumanNPC;
			}
			if (allMountPoint.mountable is MountedWeaponSeat)
			{
				mounted.inventory.containerBelt.Clear();
				mounted.inventory.containerMain.Clear();
				_turretNpcs.Add(mounted as HumanNPC, allMountPoint.mountable as MountedWeaponSeat);
			}
		}
	}

	private void KillAllRemainingScientists()
	{
		KillAllRemainingScientists(skipLoot: false);
	}

	private void KillAllRemainingScientists(bool skipLoot = false)
	{
		if (_hasSpawnedScientists && !_hasKilledScientists)
		{
			foreach (BaseVehicle.MountPointInfo mountPoint in _boat.mountPoints)
			{
				if (mountPoint.mountable.GetMounted() is HumanNPC { IsDestroyed: false } humanNPC)
				{
					if (skipLoot)
					{
						humanNPC.Kill();
					}
					else
					{
						humanNPC.Die();
					}
				}
			}
		}
		_hasKilledScientists = true;
	}

	private void SpawnScientist(BaseMountable mountable)
	{
		if (BaseNetworkableEx.Is<HumanNPC>(_scientistPrefab.GetEntity(), out var _))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(_scientistPrefab.resourcePath, _boat.mountAnchor.position, Quaternion.identity);
			baseEntity.Spawn();
			HumanNPC humanNPC = baseEntity as HumanNPC;
			mountable.AttemptMount(humanNPC, doMountChecks: false);
			if (humanNPC.Brain != null && humanNPC.Brain.Senses != null)
			{
				humanNPC.Brain.Senses.ignoreTutorialPlayers = true;
			}
		}
	}

	private void EnableScientistBrains()
	{
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BaseMountable mountable = _boat.mountPoints[i].mountable;
			if (mountable == null)
			{
				Debug.LogError("Mountable was null, skipping enabling boat scientist brain");
				continue;
			}
			BasePlayer mounted = mountable.GetMounted();
			if (mounted != null && mounted is HumanNPC humanNPC && humanNPC != null)
			{
				humanNPC.Brain.SetThinkMode(AIThinkMode.Interval);
			}
		}
	}

	private void DisableScientistBrains()
	{
		for (int i = 0; i < _boat.mountPoints.Count; i++)
		{
			BasePlayer mounted = _boat.mountPoints[i].mountable.GetMounted();
			if (mounted != null && mounted is HumanNPC humanNPC && humanNPC != null)
			{
				humanNPC.Brain.SetThinkMode(AIThinkMode.None);
			}
		}
	}

	private void SwitchState(BoatState newState)
	{
		_currentState?.Exit(this);
		if (newState != null)
		{
			_currentState = newState;
			_currentState.Enter(this);
		}
	}

	private void ExitState()
	{
		if (_currentState != null)
		{
			_currentState.Exit(this);
			_currentState = null;
		}
	}

	private bool IsInState(BoatState state)
	{
		return _currentState == state;
	}

	private void SetupStateCache()
	{
		if (_idleState == null)
		{
			_idleState = new IdleState();
		}
		if (_wanderState == null)
		{
			_wanderState = new WanderState();
		}
		if (_waitState == null)
		{
			_waitState = new WaitState();
		}
		if (_seekState == null)
		{
			_seekState = new SeekState();
		}
		if (_driveByState == null)
		{
			_driveByState = new DriveByState();
		}
		if (_ramState == null)
		{
			_ramState = new RamState();
		}
		if (_orbitState == null)
		{
			_orbitState = new OrbitState();
		}
	}

	private float GetSteerToTarget(Vector3 targetPosition)
	{
		Vector3 vector = _boat.transform.InverseTransformPoint(targetPosition);
		float num = Mathf.Clamp(0f - vector.x, -1f, 1f);
		if (vector.z < 0f)
		{
			num = ((num >= 0f) ? 1f : (-1f));
		}
		return Mathf.Clamp(num, -1f, 1f);
	}

	private void MaintainGroupCohesion(Context ctx)
	{
		if (!InGroup)
		{
			return;
		}
		ListHashSet<BoatAI> groupMembers = BoatAICoordination.GetGroupMembers(GroupId);
		if (groupMembers == null || groupMembers.Count <= 1)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		int num = 0;
		foreach (BoatAI value in groupMembers.Values)
		{
			if (!(value == null) && !(value == this))
			{
				zero += value.transform.position;
				num++;
			}
		}
		if (num != 0)
		{
			zero /= (float)num;
			if (Vector3Ex.Distance2D(base.transform.position, zero) > 30f)
			{
				Vector3 normalized = (zero - base.transform.position).normalized;
				normalized.y = 0f;
				AddContextInterest(ctx, normalized, 0.25f);
			}
		}
	}

	private void StartEngine(MotorRowboat boat)
	{
		if (!boat.EngineOn())
		{
			boat.EngineToggle(wantsOn: true);
		}
	}

	private void StopEngine(MotorRowboat boat)
	{
		if (!(boat == null) && boat.EngineOn())
		{
			boat.EngineToggle(wantsOn: false);
		}
	}

	public bool IsPlayerTargetValid(BasePlayer ply)
	{
		if (ply == null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning("[BoatAI] Invalid target: player was null");
			}
			return false;
		}
		if (ply.isClient)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning($"[BoatAI] Invalid target: {ply} is client-side");
			}
			return false;
		}
		if (SimpleAIMemory.PlayerIgnoreList.Contains(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning("[BoatAI] Invalid target: " + ply.displayName + " is in ignore list");
			}
			return false;
		}
		if (!Check.IsValidAttackTarget(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning("[BoatAI] Invalid target: " + ply.displayName + " not valid attack target");
			}
			return false;
		}
		if (Check.SimplyOnTerrainAt(ply.transform.position))
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning("[BoatAI] Invalid target: " + ply.displayName + " standing on terrain (excluded)");
			}
			return false;
		}
		return true;
	}

	public bool HasLineOfSightToPlayer(BasePlayer ply)
	{
		return GamePhysics.LineOfSight(_boat.transform.position, ply.eyes.position, 153092352);
	}

	public bool IsPlayerInRange(BasePlayer ply, float range)
	{
		return Vector3.SqrMagnitude(ply.transform.position - _boat.transform.position) <= range * range;
	}

	public bool IsSameAsActiveTarget(BasePlayer ply)
	{
		if (!(ActiveTarget is PlayerTarget playerTarget))
		{
			return false;
		}
		return (ulong)playerTarget.Player.userID == (ulong)ply.userID;
	}

	public static void SpawnBoatGroup(Vector2 pos, Quaternion rot, HashSet<RHIB> ActiveRHIBS = null, bool registerWithDeepSea = false, bool spawnsPT = true)
	{
		if (!AI.scientist_spawners_enabled)
		{
			return;
		}
		using PooledHashSet<RHIB> pooledHashSet = Facepunch.Pool.Get<PooledHashSet<RHIB>>();
		Vector3 vector = new Vector3(pos.x, 0f, pos.y);
		Vector3 vector2 = rot * Vector3.forward;
		Vector3 vector3 = rot * Vector3.right;
		Vector3[] obj = new Vector3[3]
		{
			vector2 * 10f,
			-vector2 * 5f - vector3 * 7.5f,
			-vector2 * 5f + vector3 * 7.5f
		};
		int nextGroupId = BoatAICoordination.GetNextGroupId();
		bool flag = false;
		Vector3[] array = obj;
		foreach (Vector3 vector4 in array)
		{
			Vector3 vector5 = vector;
			vector5.y = 0f;
			RHIB rHIB = SpawnEntityAt((spawnsPT && !flag) ? "assets/content/vehicles/boats/ptboat/ptboat.deepsea.prefab" : "assets/content/vehicles/boats/rhib/rhib.deepsea.prefab", vector5 + vector4, rot) as RHIB;
			if (rHIB != null)
			{
				ActiveRHIBS?.Add(rHIB);
			}
			if (rHIB != null)
			{
				pooledHashSet.Add(rHIB);
			}
			BoatAICoordination.AddToGroup(rHIB.GetComponentInChildren<BoatAI>(), nextGroupId);
			LootFill component = rHIB.GetComponent<LootFill>();
			if (component != null)
			{
				component.FillLoot();
			}
			flag = true;
		}
		if (registerWithDeepSea && PointEntity<DeepSeaManager>.ServerInstance != null)
		{
			PointEntity<DeepSeaManager>.ServerInstance.RegisterRHIBs(pooledHashSet);
		}
	}

	private static BaseEntity SpawnEntityAt(string prefabPath, Vector3 position, Quaternion rotation)
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabPath, position, rotation);
		if (baseEntity == null)
		{
			return null;
		}
		baseEntity.Spawn();
		baseEntity.UpdateNetworkGroup();
		return baseEntity;
	}

	public static bool FindBoatSpawnPositionInRadius(Vector2 centre, float radius, out Vector2 position)
	{
		position = default(Vector2);
		int layerMask = 1218652417;
		int i = 0;
		float f = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		for (; i < 10; i++)
		{
			Vector2 vector = centre + new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * radius;
			if (!GamePhysics.CheckSphere(new Vector3(vector.x, 0f, vector.y), 4f, layerMask))
			{
				position = vector;
				return true;
			}
			f = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		}
		return false;
	}

	private void SetupAI(BaseBoat boat)
	{
		if (!(boat == null) && !_setupRan)
		{
			Invoke(delegate
			{
				SetupInternal(boat);
			}, 1f);
		}
	}

	private void SetupInternal(BaseBoat boat)
	{
		_setupRan = true;
		_boat = boat;
		Boat = boat as MotorRowboat;
		_lastPos = _boat.transform.position;
		_timeSinceSpawned = 0f;
		_timeSinceSleepy = seconds_until_sleep;
		BoatAICoordination.Register(this);
		InitialiseContextMaps();
		SetupStateCache();
		_provider = new InputProvider(this);
		_boat.AddAIDriver(_provider);
		BoatID = _boat.net.ID;
		if ((bool)_debugMoveTo)
		{
			_autoPursue = false;
			SetMoveCommand(new TransformTarget(_debugMoveTo));
		}
		Invoke(NightCheck, 1f);
		InvokeRandomized(NightCheck, 0f, 30f, 0.05f);
		InvokeRandomized(TargetCheck, 0f, 5f, 0.1f);
		SwitchState(_idleState);
		RefreshSleeping();
	}

	private void TargetCheck()
	{
		using (TimeWarning.New("BoatAi.TargetCheck"))
		{
			if (ActiveTarget != null && !ActiveTarget.IsValid(this))
			{
				ClearCurrentTarget();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		if (!base.isClient && (!info.forDisk || LoadMode != AILoadMode.KillAi))
		{
			base.Save(info);
		}
	}

	public void OnTargetClaimAvailable(BasePlayer ply)
	{
		if (Check.EntityValid(ply) && ActiveTarget is PlayerTarget playerTarget && ply.userID.Get() == playerTarget.Player.userID.Get())
		{
			PursuePlayer(ply);
		}
	}

	public void OnGroupChanged(int groupId)
	{
		GroupId = groupId;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (_autoInit)
		{
			SetupAI(_boat);
		}
	}

	public override void OnKilled()
	{
		RemoveAI();
		base.OnKilled();
	}

	public void SetOilRigManager(ScientistBoatOilrigManager manager)
	{
		_oilrigManager = manager;
	}

	private void OnAttached()
	{
		if (_autoFillWithScientists)
		{
			StartCoroutine(SpawnAllScientists());
		}
	}

	private void RefreshSleeping()
	{
		_isSleeping = allow_sleeping && ActiveTarget == null && (float)_timeSinceSleepy > seconds_until_sleep;
		if (_isSleeping && UnityEngine.Random.Range(0f, 1f) < 0.01f)
		{
			_isSleeping = false;
			SwitchState(_wanderState);
			_timeSinceSleepy = 0f;
		}
	}

	private void NightCheck()
	{
		bool flag = TOD_Sky.Instance != null && (TOD_Sky.Instance.Cycle.Hour > 19f || TOD_Sky.Instance.Cycle.Hour < 8f);
		if (_boat.HasFlag(Flags.Reserved5) != flag)
		{
			using (FlagsUpdateScope flagsUpdateScope = _boat.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, flag);
			}
		}
	}

	private void RemoveAI()
	{
		if (!(_boat == null))
		{
			StopEngine(Boat);
			LoadMode = AILoadMode.KillAi;
			if (ActiveTarget != null)
			{
				ClearCurrentTarget();
			}
			ExitState();
			_boat.RemoveAIDriver(runCallbacks: false);
			BoatAICoordination.Unregister(this);
			if (InGroup)
			{
				BoatAICoordination.RemoveFromGroup(this, GroupId);
			}
			if (_oilrigManager != null)
			{
				_oilrigManager.AIDestroyed(_boat as RHIB);
			}
			EnableScientistBrains();
			Boat = null;
			Kill(DestroyMode.None, callOnKilled: false);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		BaseBoat baseBoat = BaseNetworkable.serverEntities.Find(BoatID) as BaseBoat;
		if (!(baseBoat != null))
		{
			return;
		}
		if (LoadMode == AILoadMode.LoadAi)
		{
			SetupAI(baseBoat);
		}
		if (LoadMode == AILoadMode.KillAi)
		{
			KillAllRemainingScientists(skipLoot: true);
			Kill();
		}
		if (LoadMode == AILoadMode.KillBoat)
		{
			if (baseBoat is RHIB rHIB)
			{
				rHIB.AdminKillNoLoot(killMountedNPCs: false);
			}
			else
			{
				baseBoat.Kill();
			}
			KillAllRemainingScientists(skipLoot: true);
			Kill();
		}
	}

	public void OnAdd(BaseVehicle vehicle)
	{
		SetupAI(vehicle as BaseBoat);
	}

	public void OnTick(BaseVehicle vehicle, float delta, ref float steering, ref float gasPedal)
	{
		if (_boat == null)
		{
			return;
		}
		using (TimeWarning.New("BoatAI.OnTick"))
		{
			if (IsScientistDriverDead())
			{
				RemoveAI();
				return;
			}
			gasPedal = 0f;
			if ((float)_timeSinceSpawned < 5f)
			{
				return;
			}
			if ((_isSleeping || !BaseNetworkable.HasCloseConnections(base.transform.position, enable_mount_sync_distance)) && !HasValidPursuit())
			{
				_boat.DisableMountedSyncForAllSeats();
				return;
			}
			_boat.EnableMountedSyncForAllSeats();
			if (!AI.move)
			{
				return;
			}
			ResetContext(_bufferContext);
			RunGlobalMethods();
			if (!Boat.IsOn())
			{
				StartEngine(Boat);
			}
			using (TimeWarning.New("BoatAI.OnTick.CurrentState.Update"))
			{
				_currentState?.Update(_bufferContext, this, delta);
				Vector3 bestContextDirection = GetBestContextDirection();
				Vector3 targetPosition = _boat.transform.position + bestContextDirection.normalized * 10f;
				float steerToTarget = GetSteerToTarget(targetPosition);
				steering = steerToTarget;
				float num = Vector3.Dot(_boat.transform.forward, bestContextDirection.normalized);
				float magnitude = bestContextDirection.magnitude;
				Vector3 position = _boat.transform.position;
				Vector3 vector = position - _lastPos;
				_lastPos = position;
				vector.y = 0f;
				float num2 = vector.magnitude / Mathf.Max(delta, 0.001f);
				if (bestContextDirection.sqrMagnitude > Mathf.Epsilon && num2 < 0.6f)
				{
					_stuckTimer += delta;
				}
				else
				{
					_stuckTimer = 0f;
				}
				bool flag = _stuckTimer >= 1f;
				if (_driveLockTimer <= 0f)
				{
					if (_driveDirection == 1 && flag && num < -0.4f)
					{
						_driveDirection = -1;
						_driveLockTimer = 0.4f;
					}
					else if (_driveDirection == -1 && num > -0.15f)
					{
						_driveDirection = 1;
						_driveLockTimer = 0.4f;
					}
				}
				else
				{
					_driveLockTimer -= delta;
				}
				if (bestContextDirection == Vector3.zero)
				{
					gasPedal = 0f;
				}
				else
				{
					float value = magnitude * (float)_driveDirection;
					gasPedal = Mathf.Clamp(value, 0f - max_speed_percentage, max_speed_percentage);
				}
				if (_driveDirection < 0)
				{
					steering *= -1f;
				}
				if (ActiveTarget != null && Vector3Ex.Distance2D(_boat.transform.position, ActiveTarget.Position.Value) < 5f)
				{
					gasPedal = 0f;
				}
				if (!DRAW_DEBUGS)
				{
					return;
				}
				UnityEngine.DDraw.BroadcastText(base.transform.position + Vector3.up * 5f, _currentState?.GetStateName() ?? "No State", Color.white, 0.05f, distanceFade: true, zTest: true);
				UnityEngine.DDraw.BroadcastText(base.transform.position + Vector3.up * 3.5f, $"Speed {gasPedal}", Color.white, 0.05f, distanceFade: true, zTest: true);
				UnityEngine.DDraw.BroadcastText(base.transform.position + Vector3.up * 0f, $"GROUP {GroupId}", Color.white, 0.05f, distanceFade: true, zTest: true);
				UnityEngine.DDraw.BroadcastLine(_boat.transform.position, _boat.transform.position + bestContextDirection, Color.green, 0.05f, distanceFade: false, zTest: false);
				UnityEngine.DDraw.BroadcastSphere(_boat.transform.position + bestContextDirection, 0.5f, Color.green, 0.05f, distanceFade: false, zTest: false);
				if (ActiveTarget != null && ActiveTarget.IsValid(this))
				{
					Vector3 normalized = (ActiveTarget.Position.Value - _boat.transform.position).normalized;
					normalized.y = 0f;
					UnityEngine.DDraw.BroadcastLine(_boat.transform.position, _boat.transform.position + normalized * 10f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
					UnityEngine.DDraw.BroadcastLine(_boat.transform.position, ActiveTarget.Position.Value, Color.white, 0.05f, distanceFade: false);
				}
				if (ActiveTarget != null && ActiveTarget.IsValid(this) && ActiveTarget.Position.HasValue)
				{
					if (ActiveTarget is PlayerTarget playerTarget)
					{
						UnityEngine.DDraw.BroadcastText(base.transform.position + Vector3.up * 2f, $"STAY CLOSE {playerTarget.StayClose}", Color.white, 0.05f, distanceFade: true, zTest: true);
					}
					UnityEngine.DDraw.BroadcastSphere(ActiveTarget.Position.Value, 0.5f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
				}
				UnityEngine.DDraw.BroadcastLine(_boat.transform.position, _boat.transform.position + bestContextDirection * 5f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
				UnityEngine.DDraw.BroadcastSphere(_boat.transform.position + bestContextDirection * 5f, 0.5f, Color.magenta, 0.05f, distanceFade: false, zTest: false);
				for (int i = 0; i < _contextMap.Count; i++)
				{
					float num3 = _bufferContext.DangerMap[i];
					float num4 = _bufferContext.InterestMap[i];
					UnityEngine.DDraw.BroadcastText(_boat.transform.position + _contextMap[i] * 5f, string.Format("INDEX {0}, Dgr {1}, Int {2}", i, num3.ToString("F2"), num4.ToString("F2")), Color.black, 0.05f, distanceFade: false);
				}
			}
		}
	}

	public void OnRemove(BaseVehicle vehicle)
	{
		Kill();
	}

	private void RunGlobalMethods()
	{
		using (TimeWarning.New("BoatAI.GlobalMethods"))
		{
			EnsureHasFuel();
			Boat.DriverHeartbeat();
			AvoidObstacles(_bufferContext);
			RunThink();
		}
	}

	private void RunThink()
	{
		if ((float)_timeSinceThought < _thinkTime)
		{
			return;
		}
		using (TimeWarning.New("BoatAI.RunThink"))
		{
			_timeSinceThought = 0f;
			if (_autoPursue && (!HasValidPursuit() || !(ActiveTarget is PlayerTarget)))
			{
				BasePlayer basePlayer = FindClosestPlayerTarget();
				if (basePlayer != null)
				{
					PursuePlayer(basePlayer);
				}
			}
		}
	}

	private void EnsureHasFuel()
	{
		IFuelSystem fuelSystem = Boat.GetFuelSystem();
		int fuelAmount = fuelSystem.GetFuelAmount();
		if (fuelAmount < 50)
		{
			int amount = 50 - fuelAmount;
			fuelSystem.AddFuel(amount);
		}
	}

	private void PursuePlayer(BasePlayer ply, bool applyToGroup = true)
	{
		if (ply == null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning(base.name + ": PursuePlayer called with null player");
			}
			return;
		}
		if (_boat == null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning(base.name + ": PursuePlayer called before Setup() \ufffd _boat is null");
			}
			return;
		}
		if (_seekState == null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning(base.name + ": PursuePlayer called before SetupStateCache()");
			}
			return;
		}
		if (!IsPlayerTargetValid(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log("Player isnt valid");
			}
			return;
		}
		if (!IsPlayerInRange(ply, _searchRange))
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log("Player isnt in range");
			}
			return;
		}
		if (ActiveTarget is PlayerTarget playerTarget)
		{
			BoatAICoordination.ReleaseClaim(this, playerTarget.Player);
		}
		bool stayClose = BoatAICoordination.TryClaimTarget(this, ply);
		PlayerTarget moveCommand = new PlayerTarget(ply, UnityEngine.Time.time, _boat.transform)
		{
			StayClose = stayClose
		};
		SetMoveCommand(moveCommand);
		PursuitTargetAcquireTime = UnityEngine.Time.time;
		if (!(InGroup && applyToGroup))
		{
			return;
		}
		ListHashSet<BoatAI> groupMembers = BoatAICoordination.GetGroupMembers(GroupId);
		if (groupMembers == null)
		{
			return;
		}
		foreach (BoatAI item in groupMembers)
		{
			if (!(item == null) && !(item == this))
			{
				item.PursuePlayer(ply, applyToGroup: false);
			}
		}
	}

	private BasePlayer FindClosestPlayerTarget()
	{
		using (TimeWarning.New("BoatAI.FindTarget"))
		{
			using PooledList<BasePlayer> pooledList = Facepunch.Pool.Get<PooledList<BasePlayer>>();
			Query.Server.GetPlayersInSphere(_boat.transform.position, _searchRange, pooledList);
			BasePlayer result = null;
			float num = float.MaxValue;
			foreach (BasePlayer item in pooledList)
			{
				if (IsPlayerTargetValid(item) && IsPlayerInRange(item, _searchRange) && !(item.transform.position.y < -5f) && !BoatAICoordination.IsTargetClaimedByAnotherGroup(this, item))
				{
					float num2 = Vector3.SqrMagnitude(item.transform.position - _boat.transform.position);
					if (num2 < num)
					{
						num = num2;
						result = item;
					}
				}
			}
			return result;
		}
	}

	private void BoatAttacked(HitInfo info)
	{
		if (info.InitiatorPlayer != null)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (IsPlayerTargetValid(initiatorPlayer))
			{
				PursuePlayer(info.InitiatorPlayer);
			}
		}
	}

	private void BoatDied()
	{
		if (_hasSpawnedScientists)
		{
			KillAllRemainingScientists();
		}
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		switch (id)
		{
		case 0:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: GroupId for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_GroupId);
			return true;
		case 1:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: BoatID for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_BoatID);
			return true;
		case 2:
			if (ConVar.Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log("SyncVar Writing: LoadModeSync for " + iD.ToString());
			}
			SyncVarNetWrite(writer, __sync_LoadModeSync);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_GroupId;
				int _sync_GroupId = reader.Int32();
				__sync_GroupId = _sync_GroupId;
			}
			catch (Exception exception2)
			{
				Debug.LogException(exception2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_BoatID;
				NetworkableId _sync_BoatID = reader.EntityID();
				__sync_BoatID = _sync_BoatID;
			}
			catch (Exception exception3)
			{
				Debug.LogException(exception3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_LoadModeSync;
				int _sync_LoadModeSync = reader.Int32();
				__sync_LoadModeSync = _sync_LoadModeSync;
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"GroupId" => 0, 
			"BoatID" => 1, 
			"LoadModeSync" => 2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
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
		__sync_GroupId = 0;
		__sync_BoatID = default(NetworkableId);
		__sync_LoadModeSync = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}
}

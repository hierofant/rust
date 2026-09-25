using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Rust.Ai.Gen2.Nav;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.Gen2;

public class RustNavMeshAgent : EntityComponent<BaseEntity>, IServerComponent
{
	public enum Speeds
	{
		Sneak,
		Walk,
		Jog,
		Run,
		Sprint,
		FullSprint
	}

	public bool letUnityMoveAgentIfPossible = true;

	private static ListHashSet<RustNavMeshAgent> enabledComponents = new ListHashSet<RustNavMeshAgent>();

	private static RustNavMeshPath tempPath = new RustNavMeshPath();

	private static RustNavMeshPath scratchPath = new RustNavMeshPath();

	private RustNavMeshPath CurPathNS = new RustNavMeshPath();

	private NavMeshAgent _agent;

	private NavVector3 previousPositionNS;

	private IntPtr corridor;

	private bool followingPath;

	private readonly List<NavVector3> corners = new List<NavVector3>();

	private bool lastCornerIsEnd;

	private bool cornersDirty = true;

	private const int CorridorValidityLookahead = 16;

	private const float CorridorOptimizationRange = 20f;

	private const float CornerRepullDistance = 0.25f;

	private const float SteeringCornerRepullMoveDistance = 0.35f;

	private const float SteeringCornerRepullMaxInterval = 0.25f;

	private NavVector3 lastCornerPullPositionNS;

	private float lastCornerPullTime;

	private int lastSeenTileVersion = -1;

	private NavVector3 pendingOptimizeTargetNS;

	private bool hasPendingOptimize;

	private RustNavmesh boundNavmesh;

	[NonSerialized]
	public readonly List<NavVector3> lastValidPath = new List<NavVector3>();

	private bool isScientist;

	private SenseComponent senses;

	[SerializeField]
	[Header("Base fields")]
	private int _agentTypeID;

	[SerializeField]
	private float _baseOffset;

	[SerializeField]
	private float _desiredSpeed;

	[SerializeField]
	private float _angularSpeed;

	[SerializeField]
	public ResettableFloat _acceleration = new ResettableFloat(10f);

	[SerializeField]
	private float _stoppingDistance;

	[SerializeField]
	private bool _autoBraking;

	[SerializeField]
	private float _height;

	[SerializeField]
	private int _avoidancePriority;

	[SerializeField]
	private int _areaMask;

	[SerializeField]
	private bool _updatePosition = true;

	[SerializeField]
	private bool _updateRotation = true;

	private ObstacleAvoidanceType _obstacleAvoidanceType;

	private bool _isStopped;

	private NavVector3 _velocityNS = NavVector3.zero;

	private NavVector3 _nextPositionNS = NavVector3.zero;

	private const float FindClosestEdgeMaxRadius = 10f;

	private const float MovingTargetPatchMaxDistance = 3f;

	private const float MovingTargetFullReplanInterval = 1f;

	private float lastFullPlanTime = float.NegativeInfinity;

	public Vector3? overrideDirectionWS;

	[Header("Doors")]
	public bool canOpenDoors;

	private HashSet<object> pausingSources = new HashSet<object>();

	[Header("Movement speed")]
	public float sneakSpeed = 0.6f;

	public float walkSpeed = 0.89f;

	public float jogSpeed = 2.45f;

	public float runSpeed = 4.4f;

	public float sprintSpeed = 6f;

	public float fullSprintSpeed = 9f;

	public ResettableFloat deceleration = new ResettableFloat(2f);

	public float emergencyDeceleration = 10f;

	private float currentSpeed;

	private float dampVelocity;

	private float _agentTypeRadius = -1f;

	[Header("Steering")]
	public bool canSteer = true;

	public float maxTurnRadius = 2f;

	[NonSerialized]
	public float currentDeviation;

	private NavVector3 cachedSteeringForwardNS;

	private bool hasCachedSteeringForward;

	[Header("Swimming")]
	public bool canSwim;

	public float swimSpeed = 0.6f;

	public float swimSprintSpeed = 0.89f;

	public ResettableFloat desiredSwimDepth = new ResettableFloat(0.7f);

	[Header("Terrain Preferences")]
	public TerrainTopology.Enum preferedTopology = TerrainTopology.Enum.Field | TerrainTopology.Enum.Forest | TerrainTopology.Enum.Forestside | TerrainTopology.Enum.Lakeside | TerrainTopology.Enum.Mainland;

	public TerrainBiome.Enum preferedBiome = TerrainBiome.Enum.Arid | TerrainBiome.Enum.Temperate | TerrainBiome.Enum.Tundra | TerrainBiome.Enum.Arctic;

	public IndependantNavmesh independantNavmesh { get; private set; }

	public bool HasValidIndependantNavmesh
	{
		get
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (independantNavmesh != null && independantNavmesh.Navmesh != null)
			{
				return independantNavmesh.Navmesh.IsBuilt();
			}
			return false;
		}
	}

	public bool IsNavMeshBuilt
	{
		get
		{
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return false;
			}
			if (boundNavmesh == null || !boundNavmesh.IsValid())
			{
				TryBindNavmesh();
			}
			if (boundNavmesh != null)
			{
				return boundNavmesh.IsBuilt();
			}
			return false;
		}
	}

	public NavVector3 forward => WorldToNavDirection(base.baseEntity.transform.forward);

	public NavVector3 right => WorldToNavDirection(base.baseEntity.transform.right);

	public NavVector3 up => WorldToNavDirection(base.baseEntity.transform.up);

	public OffMeshLinkData currentOffMeshLinkData
	{
		get
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.currentOffMeshLinkData;
			}
			return default(OffMeshLinkData);
		}
	}

	public int agentTypeID
	{
		get
		{
			return _agentTypeID;
		}
		set
		{
			_agentTypeID = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.agentTypeID = value;
			}
		}
	}

	public int areaMask
	{
		get
		{
			return _areaMask;
		}
		set
		{
			_areaMask = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.areaMask = value;
			}
		}
	}

	public float speed
	{
		get
		{
			return _desiredSpeed;
		}
		set
		{
			_desiredSpeed = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.speed = value;
			}
		}
	}

	public float angularSpeed
	{
		get
		{
			return _angularSpeed;
		}
		set
		{
			_angularSpeed = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.angularSpeed = value;
			}
		}
	}

	public float acceleration
	{
		get
		{
			return _acceleration.Value;
		}
		set
		{
			_acceleration.Value = value;
			if (letUnityMoveAgentIfPossible)
			{
				deceleration.Value = value;
			}
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.acceleration = value;
			}
		}
	}

	public bool updatePosition
	{
		get
		{
			return _updatePosition;
		}
		set
		{
			_updatePosition = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && letUnityMoveAgentIfPossible)
			{
				agent.updatePosition = value;
			}
		}
	}

	public bool updateRotation
	{
		get
		{
			return _updateRotation;
		}
		set
		{
			_updateRotation = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && letUnityMoveAgentIfPossible)
			{
				agent.updateRotation = value;
			}
		}
	}

	public float stoppingDistance
	{
		get
		{
			return _stoppingDistance;
		}
		set
		{
			_stoppingDistance = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.stoppingDistance = value;
			}
		}
	}

	public float height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.height = value;
			}
		}
	}

	public ObstacleAvoidanceType obstacleAvoidanceType
	{
		get
		{
			return _obstacleAvoidanceType;
		}
		set
		{
			_obstacleAvoidanceType = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.obstacleAvoidanceType = value;
			}
		}
	}

	public int avoidancePriority
	{
		get
		{
			return _avoidancePriority;
		}
		set
		{
			_avoidancePriority = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.avoidancePriority = value;
			}
		}
	}

	public bool isStopped
	{
		get
		{
			return _isStopped;
		}
		set
		{
			_isStopped = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent) && letUnityMoveAgentIfPossible)
			{
				agent.isStopped = value;
			}
		}
	}

	public bool autoBraking
	{
		get
		{
			return _autoBraking;
		}
		set
		{
			_autoBraking = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.autoBraking = value;
			}
		}
	}

	public float baseOffset
	{
		get
		{
			return _baseOffset;
		}
		set
		{
			_baseOffset = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.baseOffset = value;
			}
		}
	}

	public Vector3 nextPositionWS => NavToWorldSpace(nextPosition);

	public NavVector3 nextPosition
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (TryGetAgent(out var agent))
				{
					return new NavVector3(agent.nextPosition);
				}
				if (AI.logIssues)
				{
					Debug.LogError("RustNavMeshAgent.nextPosition called with useUnityNavmesh enabled, but no NavMeshAgent was found on the entity.");
				}
				return _nextPositionNS;
			}
			if (currentPolyRef != 0L)
			{
				return _nextPositionNS;
			}
			return WorldToNavSpace(base.transform.position);
		}
	}

	public NavMeshPathStatus pathStatus
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavMeshPathStatus.PathInvalid;
				}
				return agent.pathStatus;
			}
			if (!hasPath)
			{
				return NavMeshPathStatus.PathInvalid;
			}
			return CurPathNS.status;
		}
	}

	public bool isOnOffMeshLink
	{
		get
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.isOnOffMeshLink;
			}
			return false;
		}
	}

	public bool pathPending
	{
		get
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				return agent.pathPending;
			}
			return false;
		}
	}

	public NavVector3 steeringTarget
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.steeringTarget);
			}
			if (corners.Count <= 0)
			{
				return _nextPositionNS;
			}
			return corners[0];
		}
	}

	public NavVector3 velocity
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.velocity);
			}
			return _velocityNS;
		}
		set
		{
			_velocityNS = value;
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.velocity = value.Value;
			}
			if (!AI.useUnityNavmesh && AI.logIssues)
			{
				RustNavigation.LogError("Setting velocity on RustNavMeshAgent has no effect when not using Unity NavMesh. Use Move() to move the agent instead.");
			}
		}
	}

	public Vector3 desiredVelocityWS => NavToWorldDirection(desiredVelocity);

	public NavVector3 desiredVelocity
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.desiredVelocity);
			}
			return velocity;
		}
	}

	public bool hasPath
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				return agent.hasPath;
			}
			return followingPath;
		}
	}

	public Vector3 destinationWS => NavToWorldSpace(destination);

	public NavVector3 destination
	{
		get
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return NavVector3.zero;
				}
				return new NavVector3(agent.destination);
			}
			if (!hasPath)
			{
				return _nextPositionNS;
			}
			return CurPathNS.GetDestinationNS();
		}
		set
		{
			if (AI.useUnityNavmesh)
			{
				if (TryGetAgent(out var agent))
				{
					agent.destination = value.Value;
				}
			}
			else
			{
				SetDestination(value);
			}
		}
	}

	public float remainingDistance
	{
		get
		{
			using (TimeWarning.New("RustNavMeshAgent.remainingDistance"))
			{
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent))
					{
						return 0f;
					}
					return agent.remainingDistance;
				}
				if (!hasPath)
				{
					return 0f;
				}
				if (corners.Count > 0)
				{
					return RemainingDistanceAlongCorners();
				}
				return CurPathNS.GetPathLength();
			}
		}
	}

	public bool isOnNavMesh
	{
		get
		{
			using (TimeWarning.New("RustNavMeshAgent.isOnNavMesh"))
			{
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent))
					{
						return false;
					}
					return agent.isOnNavMesh;
				}
				if (currentPolyRef == 0L)
				{
					TryBindNavmesh();
				}
				NavHit hitNS;
				return SamplePosition(_nextPositionNS, out hitNS, 2f, debugDraw: false);
			}
		}
	}

	private ulong currentPolyRef
	{
		get
		{
			if (!(corridor != IntPtr.Zero))
			{
				return 0uL;
			}
			return RecastWrapper.CorridorGetFirstPoly(corridor);
		}
	}

	public bool IsJumping
	{
		get
		{
			return base.baseEntity.HasFlag(BaseEntity.Flags.Reserved2);
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved2, value);
		}
	}

	public bool IsPaused
	{
		get
		{
			if (pausingSources.Count == 0)
			{
				return false;
			}
			bool wasPaused = pausingSources.Count > 0;
			bool flag;
			do
			{
				flag = false;
				foreach (object pausingSource in pausingSources)
				{
					if (ObjectEx.IsUnityNull(pausingSource))
					{
						if (AI.logIssues)
						{
							RustNavigation.LogError("Removing null pausing source from " + base.baseEntity.name + ".");
						}
						pausingSources.Remove(pausingSource);
						flag = true;
						break;
					}
				}
			}
			while (flag);
			OnChange(wasPaused);
			return pausingSources.Count > 0;
		}
	}

	private float agentTypeRadius
	{
		get
		{
			if (_agentTypeRadius < 0f)
			{
				float agentRadius = NavMesh.GetSettingsByID(_agentTypeID).agentRadius;
				_agentTypeRadius = ((agentRadius > 0f) ? agentRadius : 0.5f);
			}
			return _agentTypeRadius;
		}
	}

	public bool IsSprinting => currentSpeed >= sprintSpeed;

	public bool IsSwimming
	{
		get
		{
			return base.baseEntity.HasFlag(BaseEntity.Flags.Reserved1);
		}
		set
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = base.baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Reserved1, value);
		}
	}

	private bool TryGetAgent(out NavMeshAgent agent)
	{
		agent = null;
		if (!RustNavigation.EnsureUnityNavmesh())
		{
			return false;
		}
		if (_agent == null)
		{
			_agent = base.gameObject.GetComponent<NavMeshAgent>();
		}
		if (_agent == null)
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("Entity " + (base.baseEntity?.name ?? "unknown entity") + " has RustNavMeshAgent but no NavMeshAgent component. This component will not function correctly without a NavMeshAgent.");
			}
			return false;
		}
		agent = _agent;
		return true;
	}

	public NavVector3 WorldToNavSpace(Vector3 positionWS)
	{
		if (AI.useUnityNavmesh)
		{
			return new NavVector3(base.baseEntity.WorldToNavMeshSpace.MultiplyPoint3x4(positionWS));
		}
		if (independantNavmesh == null)
		{
			return new NavVector3(positionWS);
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformPointFromWorldSpaceToNavSpace(positionWS);
		}
		return new NavVector3(positionWS);
	}

	public Vector3 NavToWorldSpace(NavVector3 positionNS)
	{
		if (AI.useUnityNavmesh)
		{
			return base.baseEntity.NavMeshToWorldSpace.MultiplyPoint3x4(positionNS.Value);
		}
		if (independantNavmesh == null)
		{
			return positionNS.Value;
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformPointFromNavSpaceToWorldSpace(positionNS);
		}
		return positionNS.Value;
	}

	public Vector3 NavToWorldDirection(NavVector3 directionNS)
	{
		if (AI.useUnityNavmesh)
		{
			return base.baseEntity.NavMeshToWorldSpace.MultiplyVector(directionNS.Value);
		}
		if (independantNavmesh == null)
		{
			return directionNS.Value;
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformDirectionFromNavSpaceToWorldSpace(directionNS);
		}
		return directionNS.Value;
	}

	public NavVector3 WorldToNavDirection(Vector3 directionWS)
	{
		if (AI.useUnityNavmesh)
		{
			return new NavVector3(base.baseEntity.WorldToNavMeshSpace.MultiplyVector(directionWS));
		}
		if (independantNavmesh == null)
		{
			return new NavVector3(directionWS);
		}
		if (HasValidIndependantNavmesh)
		{
			return independantNavmesh.TransformDirectionFromWorldSpaceToNavSpace(directionWS);
		}
		return new NavVector3(directionWS);
	}

	public void ApplySerializedSettingsToAgent(NavMeshAgent agent)
	{
		agent.baseOffset = _baseOffset;
		agent.agentTypeID = agentTypeID;
		agent.speed = _desiredSpeed;
		agent.angularSpeed = _angularSpeed;
		agent.acceleration = _acceleration.Value;
		agent.autoBraking = _autoBraking;
		agent.stoppingDistance = _stoppingDistance;
		agent.height = _height;
		agent.obstacleAvoidanceType = _obstacleAvoidanceType;
		agent.avoidancePriority = _avoidancePriority;
		agent.areaMask = _areaMask;
		agent.updatePosition = _updatePosition;
		agent.updateRotation = _updateRotation;
		agent.enabled = false;
	}

	private bool SamplePositionInternal(NavVector3 positionNS, out NavHit hitNS, float maxDistance, out ulong nearestPolyRef)
	{
		using (TimeWarning.New("RustNavMeshAgent.SamplePositionInternal"))
		{
			hitNS = default(NavHit);
			nearestPolyRef = 0uL;
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (!NavMesh.SamplePosition(positionNS.Value, out var hit, maxDistance, agent.areaMask))
				{
					return false;
				}
				hitNS = NavHit.FromUnity(in hit);
				return true;
			}
			if (boundNavmesh != null)
			{
				return boundNavmesh.SamplePositionPoly(positionNS, out hitNS, maxDistance * Vector3.one, out nearestPolyRef);
			}
			return false;
		}
	}

	public bool SamplePosition(Vector3 positionWS, out NavMeshHit hitWS, float maxDistance, bool debugDraw = true)
	{
		hitWS = default(NavMeshHit);
		if (!SamplePosition(WorldToNavSpace(positionWS), out var hitNS, maxDistance, debugDraw))
		{
			return false;
		}
		hitWS = hitNS.ToUnity();
		hitWS.position = NavToWorldSpace(hitNS.position);
		hitWS.normal = NavToWorldDirection(hitNS.normal);
		return true;
	}

	public bool SamplePosition(NavVector3 positionNS, out NavHit hitNS, float maxDistance, bool debugDraw = true)
	{
		ulong nearestPolyRef;
		return SamplePositionPoly(positionNS, out hitNS, maxDistance, out nearestPolyRef, debugDraw);
	}

	public bool SamplePositionPoly(NavVector3 positionNS, out NavHit hitNS, float maxDistance, out ulong nearestPolyRef, bool debugDraw = true)
	{
		using (TimeWarning.New("RustNavMeshAgent.SamplePosition"))
		{
			bool num = SamplePositionInternal(positionNS, out hitNS, maxDistance, out nearestPolyRef);
			float num2 = 1f;
			if (num && maxDistance > num2 && Mathf.Abs(hitNS.position.y - positionNS.y) > num2 && SampleGroundPositionWithPhysics(positionNS, out var hitInfoNS, 3.5f) && SamplePositionInternal(hitInfoNS.point, out var hitNS2, num2, out nearestPolyRef))
			{
				hitNS = hitNS2;
			}
			if (!num)
			{
				return false;
			}
			return true;
		}
	}

	public bool Raycast(NavVector3 startNS, NavVector3 endNS, out NavHit hitNS)
	{
		ulong startRef = 0uL;
		return Raycast(ref startRef, startNS, endNS, out hitNS);
	}

	private bool Raycast(ref ulong startRef, NavVector3 startNS, NavVector3 endNS, out NavHit hitNS)
	{
		using (TimeWarning.New("RustNavMeshAgent.Raycast"))
		{
			hitNS = default(NavHit);
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (!NavMesh.Raycast(startNS.Value, endNS.Value, out var hit, agent.areaMask))
				{
					return false;
				}
				hitNS = NavHit.FromUnity(in hit);
				return true;
			}
			if (boundNavmesh == null)
			{
				if (AI.logIssues)
				{
					RustNavigation.LogError(string.Format("No navmesh bound, cannot raycast for {0} from {1} to {2}", base.baseEntity?.name ?? "unknown entity", startNS, endNS));
				}
				return false;
			}
			return boundNavmesh.Raycast(ref startRef, startNS, endNS, out hitNS);
		}
	}

	public bool CalculatePath(NavVector3 startNS, NavVector3 endNS, RustNavMeshPath pathNS)
	{
		ulong startRef = 0uL;
		return CalculatePath(ref startRef, startNS, endNS, pathNS);
	}

	private bool CalculatePath(ref ulong startRef, NavVector3 startNS, NavVector3 endNS, RustNavMeshPath pathNS)
	{
		using (TimeWarning.New("RustNavMeshAgent.CalculatePath"))
		{
			pathNS.Reset();
			bool flag = false;
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				flag = RustNavMeshHelpers.CalculatePath(startNS.Value, endNS.Value, agent.areaMask, pathNS);
			}
			else if (boundNavmesh != null)
			{
				flag = boundNavmesh.CalculatePath(ref startRef, startNS, endNS, pathNS);
			}
			else
			{
				flag = false;
				if (AI.logIssues)
				{
					RustNavigation.LogError(string.Format("No navmesh bound, cannot calculate path for {0} from {1} to {2}", base.baseEntity?.name ?? "unknown entity", startNS, endNS));
				}
			}
			if (flag)
			{
				_ = pathNS.status;
			}
			return flag;
		}
	}

	public bool CanReach(Vector3 locationWS, bool updateLastValidPath = false)
	{
		return CanReach(WorldToNavSpace(locationWS), updateLastValidPath);
	}

	public bool CanReach(NavVector3 locationNS, bool updateLastValidPath = false)
	{
		using (TimeWarning.New("RustNavMeshAgent.CanReach"))
		{
			if (!IsPositionOnNavmesh(locationNS, out var hitNS, 1f))
			{
				return false;
			}
			if (!CalculatePath(hitNS.position, tempPath))
			{
				return false;
			}
			bool result = tempPath.status == NavMeshPathStatus.PathComplete;
			if (updateLastValidPath)
			{
				lastValidPath.Clear();
				lastValidPath.AddRange(tempPath.corners);
			}
			return result;
		}
	}

	public bool SetDestinationWithParams(Vector3 targetPositionWS, bool autoBraking = true, Speeds? gait = null, float? acceleration = null, float? deceleration = null, float? deviation = null, float? swimDepth = null)
	{
		return SetDestinationWithParams(WorldToNavSpace(targetPositionWS), autoBraking, gait, acceleration, deceleration, deviation, swimDepth);
	}

	public bool SetDestinationWithParams(NavVector3 targetPositionNS, bool autoBraking = true, Speeds? gait = null, float? acceleration = null, float? deceleration = null, float? deviation = null, float? swimDepth = null)
	{
		if (!SetDestination(targetPositionNS))
		{
			return false;
		}
		this.autoBraking = autoBraking;
		if (gait.HasValue)
		{
			SetGait(gait.Value);
		}
		if (acceleration.HasValue)
		{
			this.acceleration = acceleration.Value;
		}
		if (deceleration.HasValue)
		{
			this.deceleration.Value = deceleration.Value;
		}
		if (deviation.HasValue)
		{
			currentDeviation = deviation.Value;
		}
		if (swimDepth.HasValue)
		{
			desiredSwimDepth.Value = swimDepth.Value;
		}
		return true;
	}

	private void OnEnable()
	{
		enabledComponents.TryAdd(this);
		isScientist = BaseNetworkableEx.Is<ScientistNPC2>(base.baseEntity, out var _);
		TryGetComponent<SenseComponent>(out senses);
		if (AI.useUnityNavmesh)
		{
			_nextPositionNS = WorldToNavSpace(base.transform.position);
			if (SamplePositionPoly(_nextPositionNS, out var hitNS, 10f, out var _))
			{
				_nextPositionNS = hitNS.position;
			}
		}
		else
		{
			if (corridor == IntPtr.Zero)
			{
				corridor = RecastWrapper.CreateCorridor();
			}
			TryBindNavmesh();
		}
		previousPositionNS = _nextPositionNS;
		TrySyncWorldPosWithNavPos();
	}

	internal void TryBindNavmesh()
	{
		lastSeenTileVersion = -1;
		independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(base.transform.position);
		if (HasValidIndependantNavmesh)
		{
			boundNavmesh = independantNavmesh.Navmesh;
		}
		else
		{
			boundNavmesh = (RustNavigation.Instance.IsDefaultNavmeshBuilt() ? RustNavigation.Instance.DefaultNavmesh : null);
		}
		_nextPositionNS = WorldToNavSpace(base.transform.position);
		if (SamplePositionPoly(_nextPositionNS, out var hitNS, 10f, out var nearestPolyRef))
		{
			_nextPositionNS = hitNS.position;
		}
		AnchorCorridor(nearestPolyRef, _nextPositionNS);
		previousPositionNS = _nextPositionNS;
	}

	private void OnDestroy()
	{
		if (corridor != IntPtr.Zero)
		{
			RecastWrapper.FreeCorridor(corridor);
			corridor = IntPtr.Zero;
		}
	}

	private void RefreshCorners()
	{
		cornersDirty = false;
		lastCornerPullPositionNS = _nextPositionNS;
		lastCornerPullTime = UnityEngine.Time.time;
		boundNavmesh.CorridorFindCorners(corridor, corners, 256, out lastCornerIsEnd);
	}

	private bool ShouldRepullSteeringCorners()
	{
		if (corners.Count == 0)
		{
			return true;
		}
		float num = _nextPositionNS.x - corners[0].x;
		float num2 = _nextPositionNS.z - corners[0].z;
		if (num * num + num2 * num2 <= 0.0625f)
		{
			return true;
		}
		float num3 = _nextPositionNS.x - lastCornerPullPositionNS.x;
		num2 = _nextPositionNS.z - lastCornerPullPositionNS.z;
		if (num3 * num3 + num2 * num2 >= 0.122499995f)
		{
			return true;
		}
		return UnityEngine.Time.time - lastCornerPullTime >= 0.25f;
	}

	private float RemainingDistanceAlongCorners()
	{
		using (TimeWarning.New("Tick.RemainingAlongCorners"))
		{
			float num = 0f;
			NavVector3 aNS = _nextPositionNS;
			foreach (NavVector3 corner in corners)
			{
				num += NavVector3.Distance(aNS, corner);
				aNS = corner;
			}
			return num;
		}
	}

	private void OnDisable()
	{
		enabledComponents.Remove(this);
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			agent.enabled = false;
		}
		if (!AI.useUnityNavmesh)
		{
			ResetPath();
		}
	}

	public static void RebindAgentsAfterNavmeshSwap()
	{
		int count = enabledComponents.Count;
		for (int i = 0; i < count; i++)
		{
			RustNavMeshAgent rustNavMeshAgent = enabledComponents[i];
			if (!ObjectEx.IsUnityNull(rustNavMeshAgent) && (rustNavMeshAgent.boundNavmesh == null || !rustNavMeshAgent.boundNavmesh.IsValid()))
			{
				rustNavMeshAgent.TryBindNavmesh();
				rustNavMeshAgent.ResetPath();
			}
		}
	}

	public static void TickEnabledComponents()
	{
		float deltaTime = UnityEngine.Time.deltaTime;
		for (int num = enabledComponents.Count - 1; num >= 0; num--)
		{
			RustNavMeshAgent rustNavMeshAgent = enabledComponents[num];
			if (ObjectEx.IsUnityNull(rustNavMeshAgent) || !rustNavMeshAgent.baseEntity.IsValid())
			{
				enabledComponents.RemoveAt(num);
			}
			else
			{
				rustNavMeshAgent.Tick(deltaTime);
			}
		}
	}

	private void Tick(float deltaTime)
	{
		try
		{
			using (TimeWarning.New("RustNavMeshAgent.Tick"))
			{
				if (AI.useUnityNavmesh && TryGetAgent(out var agent))
				{
					if (letUnityMoveAgentIfPossible)
					{
						return;
					}
					if (agent.enabled && agent.isOnNavMesh)
					{
						agent.isStopped = true;
					}
					agent.updatePosition = false;
					agent.updateRotation = false;
				}
				if (updateRotation && (overrideDirectionWS.HasValue || (isScientist && senses != null)))
				{
					using (TimeWarning.New("Tick.Rotation"))
					{
						hasCachedSteeringForward = false;
						if (overrideDirectionWS.HasValue)
						{
							base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(overrideDirectionWS.Value.NormalizeXZ()), _angularSpeed * deltaTime);
						}
						else
						{
							base.transform.rotation = Quaternion.LookRotation((senses.GetEyeTransform().rotation * Vector3.forward).NormalizeXZ());
						}
					}
				}
				if (!hasPath || IsPaused || _isStopped)
				{
					if (currentSpeed == 0f && !(previousPositionNS != _nextPositionNS))
					{
						return;
					}
					using (TimeWarning.New("Tick.NoPath"))
					{
						currentSpeed = Mathf.SmoothDamp(currentSpeed, (_nextPositionNS - previousPositionNS).magnitude / deltaTime, ref dampVelocity, 1f, 9999f, UnityEngine.Time.smoothDeltaTime * 10f);
						if (currentSpeed < 0.005f)
						{
							currentSpeed = 0f;
						}
						return;
					}
				}
				bool flag = !letUnityMoveAgentIfPossible && canSteer;
				float num = ((flag && _autoBraking) ? maxTurnRadius : _stoppingDistance);
				NavVector3 navVector;
				float num2;
				if (AI.useUnityNavmesh)
				{
					if (!TryGetAgent(out var agent2))
					{
						return;
					}
					navVector = new NavVector3(agent2.steeringTarget);
					num2 = remainingDistance;
					if (num2 <= num)
					{
						ResetPath();
						return;
					}
				}
				else
				{
					if (boundNavmesh == null || corridor == IntPtr.Zero)
					{
						ResetPath();
						return;
					}
					if (lastSeenTileVersion != boundNavmesh.TileChangeVersion)
					{
						lastSeenTileVersion = boundNavmesh.TileChangeVersion;
						if (!boundNavmesh.CorridorIsValid(corridor, 16))
						{
							using (TimeWarning.New("Tick.Replan"))
							{
								ulong startRef = currentPolyRef;
								if (!boundNavmesh.CalculatePath(ref startRef, _nextPositionNS, CurPathNS.GetDestinationNS(), tempPath) || tempPath.status != 0 || !SetPath(tempPath))
								{
									ResetPath();
									return;
								}
							}
						}
					}
					if (cornersDirty || (flag && ShouldRepullSteeringCorners()))
					{
						RefreshCorners();
					}
					if (corners.Count == 0)
					{
						ResetPath();
						return;
					}
					navVector = corners[0];
					num2 = RemainingDistanceAlongCorners();
					if (lastCornerIsEnd && num2 <= num)
					{
						ResetPath();
						return;
					}
					if (flag)
					{
						pendingOptimizeTargetNS = corners[Mathf.Min(1, corners.Count - 1)];
						hasPendingOptimize = true;
					}
					else
					{
						float num3 = _nextPositionNS.x - navVector.x;
						float num4 = _nextPositionNS.z - navVector.z;
						if (num3 * num3 + num4 * num4 <= 0.0625f)
						{
							cornersDirty = true;
						}
					}
				}
				bool num5 = !AI.useUnityNavmesh;
				NavVector3 navVector2 = navVector - _nextPositionNS;
				if (num5)
				{
					navVector2 = navVector2.Flat();
				}
				NavVector3 normalized = navVector2.normalized;
				AdjustCurrentSpeedFromDesiredSpeed(_nextPositionNS, _autoBraking, num2, num, deltaTime);
				NavVector3 navVector3 = normalized * (currentSpeed * deltaTime);
				if (!num5 || (lastCornerIsEnd && corners.Count == 1))
				{
					navVector3 = NavVector3.ClampMagnitude(navVector3, navVector2.magnitude);
				}
				MoveInternal(navVector3, num2);
				_velocityNS = navVector3 / deltaTime;
			}
		}
		finally
		{
			previousPositionNS = _nextPositionNS;
		}
	}

	public void ActivateCurrentOffMeshLink(bool activated)
	{
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			agent.ActivateCurrentOffMeshLink(activated);
		}
	}

	public void CompleteOffMeshLink()
	{
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			agent.CompleteOffMeshLink();
		}
	}

	public bool FindClosestEdge(out NavHit hitNS)
	{
		if (AI.useUnityNavmesh && TryGetAgent(out var agent))
		{
			NavMeshHit hit;
			bool result = agent.FindClosestEdge(out hit);
			hitNS = NavHit.FromUnity(in hit);
			return result;
		}
		hitNS = default(NavHit);
		if (boundNavmesh != null)
		{
			ulong startRef = currentPolyRef;
			return boundNavmesh.FindDistanceToWall(ref startRef, _nextPositionNS, 10f, out hitNS);
		}
		return false;
	}

	public bool FindClosestEdge(NavVector3 positionNS, out NavHit hitNS)
	{
		if (AI.useUnityNavmesh)
		{
			hitNS = default(NavHit);
			if (!TryGetAgent(out var agent))
			{
				return false;
			}
			if (!NavMesh.FindClosestEdge(positionNS.Value, out var hit, agent.areaMask))
			{
				return false;
			}
			hitNS = NavHit.FromUnity(in hit);
			return true;
		}
		hitNS = default(NavHit);
		ulong startRef = 0uL;
		if (boundNavmesh != null)
		{
			return boundNavmesh.FindDistanceToWall(ref startRef, positionNS, 10f, out hitNS);
		}
		return false;
	}

	public bool SampleConnectedPositions(float maxRadius, float minRadius, int count, List<NavVector3> resultsNS, float angleOffset = -1f)
	{
		if (AI.useUnityNavmesh || boundNavmesh == null)
		{
			return false;
		}
		if (angleOffset < 0f)
		{
			angleOffset = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		}
		ulong startRef = currentPolyRef;
		return boundNavmesh.FindDonutPointsInCircle(ref startRef, _nextPositionNS, maxRadius, minRadius, angleOffset, count, resultsNS);
	}

	public bool SampleConnectedPositions(NavVector3 centerNS, float maxRadius, float minRadius, int count, List<NavVector3> resultsNS, float angleOffset = -1f)
	{
		if (AI.useUnityNavmesh || boundNavmesh == null)
		{
			return false;
		}
		if (angleOffset < 0f)
		{
			angleOffset = UnityEngine.Random.Range(0f, MathF.PI * 2f);
		}
		ulong startRef = 0uL;
		if (currentPolyRef != 0L && NavVector3.Distance(centerNS, _nextPositionNS) <= maxRadius)
		{
			startRef = currentPolyRef;
		}
		return boundNavmesh.FindDonutPointsInCircle(ref startRef, centerNS, maxRadius, minRadius, angleOffset, count, resultsNS);
	}

	public bool SetDestination(Vector3 targetPositionWS)
	{
		return SetDestination(WorldToNavSpace(targetPositionWS));
	}

	public bool SetDestination(NavVector3 targetPositionNS)
	{
		using (TimeWarning.New("RustNavMeshAgent.SetDestination"))
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (!TryEnableAgent(agent))
				{
					return false;
				}
				if (letUnityMoveAgentIfPossible)
				{
					return agent.SetDestination(targetPositionNS.Value);
				}
			}
			if (hasPath && CurPathNS.GetDestinationNS() == targetPositionNS)
			{
				return true;
			}
			if (TryPatchMovingDestination(targetPositionNS))
			{
				return true;
			}
			if (!CalculatePath(targetPositionNS, tempPath) || tempPath.status != 0)
			{
				return false;
			}
			if (!SetPath(tempPath))
			{
				return false;
			}
			return true;
		}
	}

	private bool TryPatchMovingDestination(NavVector3 targetPositionNS)
	{
		if (AI.useUnityNavmesh)
		{
			return false;
		}
		if (corridor == IntPtr.Zero || boundNavmesh == null)
		{
			return false;
		}
		if (UnityEngine.Time.time - lastFullPlanTime >= 1f)
		{
			return false;
		}
		bool flag = followingPath;
		if (flag)
		{
			if (CurPathNS.status != 0 || CurPathNS.corners.Count == 0)
			{
				return false;
			}
			if (NavVector3.Distance(CurPathNS.GetDestinationNS(), targetPositionNS) > 3f)
			{
				return false;
			}
		}
		else
		{
			ulong num = currentPolyRef;
			if (num == 0L)
			{
				return false;
			}
			if (NavVector3.Distance(_nextPositionNS, targetPositionNS) > 3f)
			{
				return false;
			}
			AnchorCorridor(num, _nextPositionNS);
		}
		if (!boundNavmesh.CorridorMoveTargetPosition(corridor, targetPositionNS, out var resultTargetNS))
		{
			return false;
		}
		NavVector3 navVector = resultTargetNS - targetPositionNS;
		if (Mathf.Abs(navVector.x) > 0.5f || Mathf.Abs(navVector.y) > 2f || Mathf.Abs(navVector.z) > 0.5f)
		{
			return false;
		}
		if (flag)
		{
			CurPathNS.corners[CurPathNS.corners.Count - 1] = resultTargetNS;
		}
		else
		{
			CurPathNS.corners.Clear();
			CurPathNS.corners.Add(_nextPositionNS);
			CurPathNS.corners.Add(resultTargetNS);
			CurPathNS.polyRefCount = 0;
			CurPathNS.status = NavMeshPathStatus.PathComplete;
		}
		followingPath = true;
		cornersDirty = true;
		_isStopped = false;
		return true;
	}

	public void TryEnableInternalUnityAgent()
	{
		if (AI.useUnityNavmesh && base.enabled && TryGetAgent(out var agent))
		{
			TryEnableAgent(agent);
		}
	}

	private bool TryEnableAgent(NavMeshAgent Agent)
	{
		if (Agent.enabled)
		{
			return Agent.isOnNavMesh;
		}
		Agent.enabled = true;
		if (!Agent.isOnNavMesh)
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError($"{base.baseEntity} is not on navmesh at {base.baseEntity.transform.position} in {MapHelper.PositionToString(base.baseEntity.transform.position)}");
			}
			return false;
		}
		return true;
	}

	public bool SetPath(RustNavMeshPath pathNS)
	{
		using (TimeWarning.New("RustNavMeshAgent.SetPath"))
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (pathNS.unityPath == null)
				{
					if (AI.logIssues)
					{
						RustNavigation.LogError("Trying to set a path on a RustNavMeshAgent with useUnityNavmesh enabled, but the provided path doesn't have a Unity NavMeshPath.");
					}
					return false;
				}
				if (!TryEnableAgent(agent))
				{
					return false;
				}
				return agent.SetPath(pathNS.unityPath);
			}
			if (pathNS.status == NavMeshPathStatus.PathInvalid)
			{
				ResetPath();
				return false;
			}
			if (pathNS.polyRefCount == 0 || pathNS.polyRefs[0] != currentPolyRef)
			{
				ulong startRef = currentPolyRef;
				if (boundNavmesh == null || !boundNavmesh.CalculatePath(ref startRef, _nextPositionNS, pathNS.GetDestinationNS(), scratchPath) || scratchPath.status != 0)
				{
					ResetPath();
					return false;
				}
				pathNS = scratchPath;
			}
			CurPathNS.CopyFrom(pathNS);
			IntPtr intPtr = corridor;
			ulong[] polyRefs = CurPathNS.polyRefs;
			int polyRefCount = CurPathNS.polyRefCount;
			NavVector3 destinationNS = CurPathNS.GetDestinationNS();
			RecastWrapper.CorridorSetPath(intPtr, polyRefs, polyRefCount, in destinationNS.Value);
			followingPath = true;
			cornersDirty = true;
			_isStopped = false;
			lastFullPlanTime = UnityEngine.Time.time;
			lastValidPath.Clear();
			lastValidPath.AddRange(CurPathNS.corners);
			return true;
		}
	}

	private void AnchorCorridor(ulong polyRef, NavVector3 positionNS)
	{
		if (corridor != IntPtr.Zero)
		{
			RecastWrapper.CorridorReset(corridor, polyRef, in positionNS.Value);
		}
	}

	public void Move(NavVector3 deltaNS)
	{
		MoveInternal(deltaNS, null);
	}

	private void MoveInternal(NavVector3 deltaNS, float? remainingDistanceHint)
	{
		using (TimeWarning.New("RustNavMeshAgent.Move"))
		{
			if (AI.useUnityNavmesh && letUnityMoveAgentIfPossible)
			{
				if (TryGetAgent(out var agent) && TryEnableAgent(agent))
				{
					agent.Move(deltaNS.Value);
				}
				return;
			}
			if (!letUnityMoveAgentIfPossible && canSteer && !IsPaused)
			{
				deltaNS = AdjustMovementForSteering(deltaNS, remainingDistanceHint);
			}
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent2) || !TryEnableAgent(agent2))
				{
					return;
				}
				agent2.Move(deltaNS.Value);
				_nextPositionNS = new NavVector3(agent2.nextPosition);
			}
			else
			{
				if (boundNavmesh == null || !boundNavmesh.IsValid())
				{
					TryBindNavmesh();
				}
				if (boundNavmesh != null && corridor != IntPtr.Zero)
				{
					if (!boundNavmesh.IsValidPolyRef(currentPolyRef))
					{
						TryBindNavmesh();
						ResetPath();
						return;
					}
					bool flag;
					NavVector3 resultPosNS;
					if (hasPendingOptimize)
					{
						hasPendingOptimize = false;
						flag = boundNavmesh.CorridorOptimizeAndMove(corridor, pendingOptimizeTargetNS, 20f, _nextPositionNS + deltaNS, out resultPosNS);
					}
					else
					{
						flag = boundNavmesh.CorridorMove(corridor, _nextPositionNS + deltaNS, out resultPosNS, out var _);
					}
					if (flag)
					{
						_nextPositionNS = resultPosNS;
					}
					else if (followingPath)
					{
						ResetPath();
					}
				}
			}
			Quaternion? newRotation = null;
			NavVector3 directionNS = deltaNS.Flat();
			if (_updateRotation && !IsPaused && !overrideDirectionWS.HasValue && directionNS.magnitude > 0.001f)
			{
				using (TimeWarning.New("Move.RotationWrite"))
				{
					Vector3 vector = NavToWorldDirection(directionNS);
					if (!letUnityMoveAgentIfPossible && canSteer)
					{
						newRotation = Quaternion.LookRotation(vector);
						cachedSteeringForwardNS = directionNS.NormalizeXZ();
						hasCachedSteeringForward = true;
					}
					else
					{
						newRotation = Quaternion.RotateTowards(base.transform.rotation, Quaternion.LookRotation(vector), _angularSpeed * UnityEngine.Time.deltaTime);
						hasCachedSteeringForward = false;
					}
				}
			}
			else
			{
				hasCachedSteeringForward = false;
			}
			SyncWorldPosWithNavPos(newRotation);
			TryOpenDoors();
		}
	}

	private bool TrySyncWorldPosWithNavPos()
	{
		return SyncWorldPosWithNavPos(null);
	}

	private bool SyncWorldPosWithNavPos(Quaternion? newRotation)
	{
		using (TimeWarning.New("RustNavMeshAgent.TrySyncWorldPosWithNavPos"))
		{
			if (!_updatePosition)
			{
				if (newRotation.HasValue)
				{
					base.transform.rotation = newRotation.Value;
				}
				return false;
			}
			Vector3 position = (canSwim ? CalculateSwimmingWorldPosition() : NavToWorldSpace(_nextPositionNS + NavVector3.up * _baseOffset));
			if (newRotation.HasValue)
			{
				base.transform.SetPositionAndRotation(position, newRotation.Value);
			}
			else
			{
				base.transform.position = position;
			}
			return true;
		}
	}

	public bool Warp(Vector3 newPositionWS)
	{
		return WarpToWorldPosition(newPositionWS);
	}

	public bool Warp(NavVector3 newPositionNS)
	{
		using (TimeWarning.New("RustNavMeshAgent.Warp"))
		{
			if (AI.useUnityNavmesh)
			{
				if (!TryGetAgent(out var agent))
				{
					return false;
				}
				if (!TryEnableAgent(agent))
				{
					return false;
				}
				return agent.Warp(newPositionNS.Value);
			}
			if (!SamplePositionPoly(newPositionNS, out var hitNS, 1f, out var nearestPolyRef))
			{
				return false;
			}
			_nextPositionNS = hitNS.position;
			AnchorCorridor(nearestPolyRef, _nextPositionNS);
			ResetPath();
			TrySyncWorldPosWithNavPos();
			return true;
		}
	}

	public bool WarpToWorldPosition(Vector3 newPositionWS)
	{
		if (!AI.useUnityNavmesh)
		{
			independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(newPositionWS);
			if (HasValidIndependantNavmesh)
			{
				boundNavmesh = independantNavmesh.Navmesh;
			}
			else
			{
				boundNavmesh = (RustNavigation.Instance.IsDefaultNavmeshBuilt() ? RustNavigation.Instance.DefaultNavmesh : null);
			}
		}
		return Warp(WorldToNavSpace(newPositionWS));
	}

	private void EnsureAnchorValid()
	{
		if (AI.useUnityNavmesh || corridor == IntPtr.Zero)
		{
			return;
		}
		if (boundNavmesh == null || !boundNavmesh.IsValid())
		{
			TryBindNavmesh();
			return;
		}
		ulong num = currentPolyRef;
		if ((num == 0L || !boundNavmesh.IsValidPolyRef(num)) && SamplePositionPoly(_nextPositionNS, out var hitNS, 2.5f, out var nearestPolyRef, debugDraw: false) && nearestPolyRef != 0L)
		{
			_nextPositionNS = hitNS.position;
			AnchorCorridor(nearestPolyRef, _nextPositionNS);
			ResetPath();
		}
	}

	public bool CalculatePath(Vector3 targetPositionWS, RustNavMeshPath pathNS)
	{
		return CalculatePath(WorldToNavSpace(targetPositionWS), pathNS);
	}

	public bool CalculatePath(NavVector3 targetPositionNS, RustNavMeshPath pathNS)
	{
		EnsureAnchorValid();
		ulong startRef = currentPolyRef;
		return CalculatePath(ref startRef, _nextPositionNS, targetPositionNS, pathNS);
	}

	public bool Raycast(Vector3 targetPositionWS, out NavMeshHit hitWS)
	{
		hitWS = default(NavMeshHit);
		if (!Raycast(WorldToNavSpace(targetPositionWS), out var hitNS))
		{
			return false;
		}
		hitWS = hitNS.ToUnity();
		hitWS.position = NavToWorldSpace(hitNS.position);
		hitWS.normal = NavToWorldDirection(hitNS.normal);
		return true;
	}

	public bool Raycast(NavVector3 targetPositionNS, out NavHit hitNS)
	{
		ulong startRef = currentPolyRef;
		return Raycast(ref startRef, _nextPositionNS, targetPositionNS, out hitNS);
	}

	public bool IsPositionOnNavmesh(Vector3 positionWS, out NavMeshHit hitWS)
	{
		hitWS = default(NavMeshHit);
		if (!IsPositionOnNavmesh(WorldToNavSpace(positionWS), out var hitNS))
		{
			return false;
		}
		hitWS = hitNS.ToUnity();
		hitWS.position = NavToWorldSpace(hitNS.position);
		hitWS.normal = NavToWorldDirection(hitNS.normal);
		return true;
	}

	public bool IsPositionOnNavmesh(NavVector3 positionNS, out NavHit hitNS, float maxDistance = 2f)
	{
		return SamplePosition(positionNS, out hitNS, maxDistance);
	}

	public void ResetPath()
	{
		using (TimeWarning.New("RustNavMeshAgent.ResetPath"))
		{
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.ResetPath();
			}
			CurPathNS.Reset();
			followingPath = false;
			corners.Clear();
			cornersDirty = true;
			lastCornerIsEnd = false;
			hasPendingOptimize = false;
			AnchorCorridor(currentPolyRef, _nextPositionNS);
			autoBraking = true;
			_desiredSpeed = 0f;
			if (!letUnityMoveAgentIfPossible)
			{
				_acceleration.Reset();
				deceleration.Reset();
			}
			currentDeviation = 0f;
		}
	}

	public bool TryOpenDoors()
	{
		if (!canOpenDoors)
		{
			return false;
		}
		using (TimeWarning.New("RustNavMeshAgent.TryOpenDoors"))
		{
			bool flag = false;
			using PooledList<NPCDoorTriggerBox> pooledList = Facepunch.Pool.Get<PooledList<NPCDoorTriggerBox>>();
			NPCDoorTriggerBox.AllDoors.GetNeighboors(base.baseEntity.transform.position, pooledList);
			foreach (NPCDoorTriggerBox item in pooledList)
			{
				flag |= item.TryOpenDoorFor(base.baseEntity);
			}
			return flag;
		}
	}

	public bool Pause(object source)
	{
		bool wasPaused = pausingSources.Count > 0;
		bool result = pausingSources.Add(source);
		OnChange(wasPaused);
		return result;
	}

	public bool Unpause(object source)
	{
		bool wasPaused = pausingSources.Count > 0;
		bool result = pausingSources.Remove(source);
		OnChange(wasPaused);
		return result;
	}

	private void OnChange(bool wasPaused)
	{
		if (!wasPaused && pausingSources.Count > 0)
		{
			ResetPath();
			if (AI.useUnityNavmesh && TryGetAgent(out var agent))
			{
				agent.enabled = false;
			}
		}
		else if (wasPaused && pausingSources.Count == 0)
		{
			bool flag = Warp(WorldToNavSpace(base.transform.position));
			if (AI.logIssues && !flag)
			{
				Debug.LogError($"Failed to reproject {base.baseEntity.name} to current position {base.transform.position} after unpausing.", this);
			}
		}
	}

	private static float GetBrakingDistance(float speed, float brakingDeceleration)
	{
		float num = speed / Mathf.Max(brakingDeceleration, 0.001f);
		return 0.5f * brakingDeceleration * num * num;
	}

	public float GetSpeedForGait(Speeds gait)
	{
		return gait switch
		{
			Speeds.Sneak => sneakSpeed, 
			Speeds.Walk => walkSpeed, 
			Speeds.Jog => jogSpeed, 
			Speeds.Run => runSpeed, 
			Speeds.Sprint => sprintSpeed, 
			Speeds.FullSprint => fullSprintSpeed, 
			_ => walkSpeed, 
		};
	}

	public void SetGait(Speeds gait)
	{
		speed = GetSpeedForGait(gait);
	}

	public void SetSpeedRatio(float ratio, Speeds minSpeed = Speeds.Sneak, Speeds maxSpeed = Speeds.Sprint, int offset = 0)
	{
		int num = Mathf.FloorToInt(Mathf.Lerp((float)minSpeed, (float)maxSpeed, ratio));
		num = Mathf.Clamp(num + offset, (int)minSpeed, (int)maxSpeed);
		SetGait((Speeds)num);
	}

	public void AdjustCurrentSpeedFromDesiredSpeed(NavVector3 position, bool shouldStopAtDestination, float remainingDistance, float stoppingDistance, float dt)
	{
		using (TimeWarning.New("Tick.SpeedAdjust"))
		{
			float num = AdjustDesiredSpeedWhenSwimming(_desiredSpeed, sprintSpeed);
			if (letUnityMoveAgentIfPossible)
			{
				float num2 = 2f * agentTypeRadius;
				if (shouldStopAtDestination && remainingDistance < num2 && remainingDistance * num < num2 * currentSpeed)
				{
					currentSpeed = Mathf.Max(0f, currentSpeed - dt * currentSpeed * currentSpeed / (2f * Mathf.Max(remainingDistance, 0.001f)));
					return;
				}
				shouldStopAtDestination = false;
			}
			if (shouldStopAtDestination && remainingDistance - stoppingDistance < GetBrakingDistance(currentSpeed, deceleration.Value))
			{
				currentSpeed = Mathf.Max(1f, currentSpeed - deceleration.Value * dt);
			}
			else if (currentSpeed > num)
			{
				float num3 = (currentSpeed - num) / deceleration.Value;
				float num4 = ((currentSpeed > walkSpeed && num3 > 1f) ? emergencyDeceleration : deceleration.Value);
				currentSpeed = Mathf.Max(num, currentSpeed - num4 * dt);
			}
			else if (currentSpeed < num)
			{
				currentSpeed = Mathf.Min(num, currentSpeed + _acceleration.Value * dt);
			}
		}
	}

	public NavVector3 AdjustMovementForSteering(NavVector3 deltaNS)
	{
		return AdjustMovementForSteering(deltaNS, null);
	}

	public NavVector3 AdjustMovementForSteering(NavVector3 deltaNS, float? remainingDistanceHint)
	{
		using (TimeWarning.New("AdjustMovementForSteering"))
		{
			NavVector3 targetNS = deltaNS;
			float num = remainingDistanceHint ?? remainingDistance;
			if (currentDeviation != 0f && Mathf.Abs(num - NavVector3.Distance(_nextPositionNS, CurPathNS.GetDestinationNS())) < 5f)
			{
				targetNS = Quaternion.AngleAxis(currentDeviation, Vector3.up) * deltaNS;
			}
			float num2 = (_autoBraking ? Mathx.RemapValClamped(num, maxTurnRadius * 2f, 0f, maxTurnRadius, 0.001f) : maxTurnRadius);
			float num3 = currentSpeed / num2;
			return RotateTowardsFlat(hasCachedSteeringForward ? cachedSteeringForwardNS : forward, targetNS, num3 * UnityEngine.Time.deltaTime) * targetNS.magnitude;
		}
	}

	private static NavVector3 RotateTowardsFlat(NavVector3 currentNS, NavVector3 targetNS, float maxRadiansDelta)
	{
		float x = currentNS.x;
		float z = currentNS.z;
		float x2 = targetNS.x;
		float z2 = targetNS.z;
		float num = Mathf.Sqrt(x * x + z * z);
		float num2 = Mathf.Sqrt(x2 * x2 + z2 * z2);
		if (num < 1E-06f || num2 < 1E-06f)
		{
			return targetNS.NormalizeXZ();
		}
		x /= num;
		z /= num;
		x2 /= num2;
		z2 /= num2;
		if (x * x2 + z * z2 >= Mathf.Cos(maxRadiansDelta))
		{
			return new NavVector3(new Vector3(x2, 0f, z2));
		}
		float f = ((x * z2 - z * x2 >= 0f) ? maxRadiansDelta : (0f - maxRadiansDelta));
		float num3 = Mathf.Cos(f);
		float num4 = Mathf.Sin(f);
		return new NavVector3(new Vector3(x * num3 - z * num4, 0f, x * num4 + z * num3));
	}

	public Vector3 CalculateSwimmingWorldPosition()
	{
		using (TimeWarning.New("RustNavMeshAgent.CalculateSwimmingWorldPosition"))
		{
			Vector3 vector = NavToWorldSpace(_nextPositionNS);
			WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(vector, waves: false, volumes: false);
			if (IsSwimming = waterInfo.currentDepth > desiredSwimDepth.Value)
			{
				vector.y = base.baseEntity.transform.position.y;
				vector.y = Mathf.MoveTowards(vector.y, waterInfo.surfaceLevel - desiredSwimDepth.Value, 1f * UnityEngine.Time.deltaTime);
				vector.y = Mathf.Max(vector.y, waterInfo.terrainHeight);
			}
			return vector;
		}
	}

	public float AdjustDesiredSpeedWhenSwimming(float desiredGroundSpeed, float groundSprintSpeed)
	{
		if (!IsSwimming || desiredGroundSpeed <= 0f)
		{
			return desiredGroundSpeed;
		}
		if (!(desiredGroundSpeed < groundSprintSpeed))
		{
			return swimSprintSpeed;
		}
		return swimSpeed;
	}

	public bool IsPositionOnFavoredTerrain(NavVector3 positionNS)
	{
		return IsPositionOnFavoredTerrain(NavToWorldSpace(positionNS));
	}

	public bool IsPositionOnFavoredTerrain(Vector3 positionWS)
	{
		using (TimeWarning.New("IsPositionOnFavoredTerrain"))
		{
			return IsPositionAtTopologyRequirement(positionWS, preferedTopology) && IsPositionABiomeRequirement(positionWS, preferedBiome);
		}
	}

	public bool IsPositionAtTopologyRequirement(NavVector3 positionNS, TerrainTopology.Enum topologyRequirement)
	{
		return IsPositionAtTopologyRequirement(NavToWorldSpace(positionNS), topologyRequirement);
	}

	public bool IsPositionAtTopologyRequirement(Vector3 positionWS, TerrainTopology.Enum topologyRequirement)
	{
		using (TimeWarning.New("IsPositionAtTopologyRequirement"))
		{
			if (TerrainMeta.TopologyMap == null)
			{
				return false;
			}
			TerrainTopology.Enum topology = (TerrainTopology.Enum)TerrainMeta.TopologyMap.GetTopology(positionWS);
			if ((topologyRequirement & topology) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsPositionABiomeRequirement(NavVector3 positionNS, TerrainBiome.Enum biomeRequirement)
	{
		return IsPositionABiomeRequirement(NavToWorldSpace(positionNS), biomeRequirement);
	}

	public bool IsPositionABiomeRequirement(Vector3 positionWS, TerrainBiome.Enum biomeRequirement)
	{
		using (TimeWarning.New("IsPositionABiomeRequirement"))
		{
			if (biomeRequirement == (TerrainBiome.Enum)0)
			{
				return true;
			}
			if (TerrainMeta.BiomeMap == null)
			{
				return false;
			}
			TerrainBiome.Enum biomeMaxType = (TerrainBiome.Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(positionWS);
			if ((biomeRequirement & biomeMaxType) == 0)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsInWater(NavVector3 positionNS)
	{
		return IsInWater(NavToWorldSpace(positionNS));
	}

	public bool IsInWater(Vector3 positionWS)
	{
		using (TimeWarning.New("IsInWater"))
		{
			if (base.baseEntity.GetParentEntity() != null)
			{
				return false;
			}
			if (WaterLevel.GetWaterDepth(positionWS, waves: false, volumes: false) >= 0.3f)
			{
				return true;
			}
			return false;
		}
	}

	public bool SampleGroundPositionWithPhysics(Vector3 positionWS, out NavGroundHit hitInfoNS, float maxDistance = 2f, float radius = 0f, int layerMask = 1503731969)
	{
		return SampleGroundPositionWithPhysics(WorldToNavSpace(positionWS), out hitInfoNS, maxDistance, radius, layerMask);
	}

	public bool SampleGroundPositionWithPhysics(NavVector3 positionNS, out NavGroundHit hitInfoNS, float maxDistance = 2f, float radius = 0f, int layerMask = 1503731969)
	{
		using (TimeWarning.New("SampleGroundPositionWithPhysics"))
		{
			Vector3 origin = NavToWorldSpace(positionNS) + Vector3.up * radius * 1.5f;
			float maxDistance2 = maxDistance + radius * 1.5f;
			RaycastHit hitInfo;
			bool num = GamePhysics.TraceRealm(GamePhysics.Realm.Server, new Ray(origin, Vector3.down), radius, out hitInfo, maxDistance2, layerMask, QueryTriggerInteraction.Ignore);
			hitInfoNS = new NavGroundHit
			{
				distance = hitInfo.distance,
				collider = hitInfo.collider,
				rawHitWS = hitInfo
			};
			if (!num)
			{
				hitInfoNS.point = positionNS;
				hitInfoNS.normal = NavVector3.up;
				return false;
			}
			hitInfoNS.point = WorldToNavSpace(hitInfo.point);
			hitInfoNS.normal = WorldToNavDirection(hitInfo.normal);
			if (radius > 0f && hitInfoNS.distance <= 0f)
			{
				hitInfoNS.point = positionNS;
			}
			return true;
		}
	}
}

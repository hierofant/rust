using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using UnityEngine;
using UnityEngine.Serialization;

public class Construction : PrefabAttribute
{
	public struct Target
	{
		public bool valid;

		public Ray ray;

		public BaseEntity entity;

		public Socket_Base socket;

		public bool onTerrain;

		public Vector3 position;

		public Vector3 normal;

		public Vector3 rotation;

		public BasePlayer player;

		public bool buildingBlocked;

		public bool isHoldingShift;

		public int snappingMode;

		public bool isSnapped;

		public Vector3 snappedPosition;

		public Vector3 snappedRotation;

		public bool shouldParent;

		public Quaternion GetWorldRotation(bool female)
		{
			Quaternion quaternion = socket.rotation;
			if (socket.male && socket.female && female)
			{
				quaternion = socket.rotation * Quaternion.Euler(180f, 0f, 180f);
			}
			return entity.transform.rotation * quaternion;
		}

		public Vector3 GetWorldPosition()
		{
			return entity.transform.localToWorldMatrix.MultiplyPoint3x4(socket.position);
		}
	}

	public struct Placement
	{
		public Vector3 position;

		public Quaternion rotation;

		public bool isPopulated;

		public bool shouldParent;

		public bool parentPassed;

		public readonly bool isHoldingShift;

		public Transform transform;

		public BaseEntity ignoredEntity;

		public Placement(Target target)
		{
			isHoldingShift = target.isHoldingShift;
			position = Vector3.zero;
			rotation = Quaternion.identity;
			isPopulated = true;
			transform = null;
			ignoredEntity = null;
			shouldParent = target.shouldParent;
			parentPassed = false;
			if (target.entity != null)
			{
				transform = target.entity.transform;
			}
		}

		public bool ShouldIgnoreCollider(Collider col)
		{
			if (ignoredEntity == null || col == null)
			{
				return false;
			}
			return ShouldIgnoreEntity(GameObjectEx.ToBaseEntity(col));
		}

		public bool ShouldIgnoreEntity(BaseEntity ent)
		{
			if (ignoredEntity == null || ent == null)
			{
				return false;
			}
			if (ent == ignoredEntity)
			{
				return true;
			}
			return false;
		}
	}

	public class Grade
	{
		public BuildingGrade grade;

		public float maxHealth;

		public List<ItemAmount> costToBuild;

		public PhysicsMaterial physicMaterial => grade.physicMaterial;

		public ProtectionProperties damageProtecton => grade.damageProtecton;
	}

	public static Translate.Phrase lastPlacementError = string.Empty;

	public static bool lastPlacementErrorIsDetailed;

	public static string lastPlacementErrorDebug;

	public static BuildingBlock lastBuildingBlockError;

	public BaseEntity.Menu.Option info;

	public bool canBypassBuildingPermission;

	public bool showBuildingBlockedPreview = true;

	[InspectorName("Can Bypass Road Checks")]
	public bool canPlaceOnRoads;

	[FormerlySerializedAs("canRotate")]
	public bool canRotateBeforePlacement;

	[FormerlySerializedAs("canRotate")]
	public bool canRotateAfterPlacement;

	public bool canBeDeployedInDeepSeaWater;

	public bool checkVolumeOnRotate;

	public bool checkVolumeOnUpgrade;

	public bool canPlaceAtMaxDistance;

	public bool placeOnWater;

	public bool overridePlacementLayer;

	public LayerMask overridedPlacementLayer;

	public LayerMask additionalPlacementLayer;

	public Vector3 rotationAmount = new Vector3(0f, 90f, 0f);

	public Vector3 applyStartingRotation = Vector3.zero;

	public Transform deployOffset;

	public bool enforceLineOfSightCheckAgainstParentEntity;

	[Tooltip("Axis Snapping for IO Entities.")]
	public bool canSnap;

	[Tooltip("Force placement to be at a specific height.")]
	public bool forceY;

	[Tooltip("Height value to force the placement at.")]
	public float forceYValue;

	[Tooltip("Force placement height to the ocean level. Overrides forceY.")]
	public bool forceYToOceanLevel;

	[Tooltip("Optional Y offset to apply to the ocean level height placement.")]
	public float OceanLevelYOffset;

	public float holdToPlaceDuration;

	public bool canFloodFillSockets;

	[Space]
	public bool alternativeLOSChecks;

	public Vector3[] alternativeLOSPositions;

	public bool canUseLastValidPosition = true;

	[Range(0f, 10f)]
	public float healthMultiplier = 1f;

	[Range(0f, 10f)]
	public float costMultiplier = 1f;

	[Range(1f, 50f)]
	public float maxplaceDistance = 4f;

	[Range(0f, 10f)]
	public float minPlaceDistance = 1f;

	public UnityEngine.Mesh guideMesh;

	public Material[] guideMeshMaterial;

	public GameObjectRef placeEffect;

	[Space]
	public bool hideBuildingPlannerViewmodel;

	[NonSerialized]
	public Socket_Base[] allSockets;

	[NonSerialized]
	public BuildingProximity[] allProximities;

	[NonSerialized]
	public ConstructionGrade defaultGrade;

	[NonSerialized]
	public SocketHandle socketHandle;

	[NonSerialized]
	public Bounds bounds;

	[NonSerialized]
	public bool isBuildingPrivilege;

	[NonSerialized]
	public bool isSleepingBag;

	[NonSerialized]
	public ConstructionGrade[] grades;

	[NonSerialized]
	public Deployable deployable;

	[NonSerialized]
	public ConstructionPlaceholder placeholder;

	[ReplicatedVar]
	public static bool alternativeLOSChecks_enabled = true;

	public bool UpdatePlacement(Transform transform, Construction common, ref Target target)
	{
		if (common.deployable == null && !TargetIsInteractable(target))
		{
			return false;
		}
		if (common.forceY)
		{
			transform.position = transform.position.WithY(forceYValue);
		}
		if (common.forceYToOceanLevel)
		{
			transform.position = transform.position.WithY(Env.oceanlevel + common.OceanLevelYOffset);
		}
		if (!target.valid)
		{
			if (common.placeOnWater)
			{
				lastPlacementError = ConstructionErrors.WantsWater;
			}
			return false;
		}
		if (!common.canBypassBuildingPermission && !target.player.CanBuild(isServer))
		{
			lastPlacementError = ConstructionErrors.NoPermission;
			return false;
		}
		if (!canBeDeployedInDeepSeaWater && DeepSea.block_building && DeepSeaManager.IsInsideDeepSea(transform.position))
		{
			if (target.entity == null && !canPlaceAtMaxDistance)
			{
				lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
				return false;
			}
			BoatBuildingStation forPlayer = BoatBuildingStation.GetForPlayer(target.player);
			if (forPlayer == null || !forPlayer.IsInsideBuildArea(target.player.transform.position))
			{
				VehiclePrivilege vehiclePrivilege = target.player.GetVehicleBuildingPrivilege(cached: true) as VehiclePrivilege;
				if (vehiclePrivilege == null)
				{
					lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
					return false;
				}
				if (!common.canBypassBuildingPermission)
				{
					if (!vehiclePrivilege.IsAuthed(target.player))
					{
						lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
						return false;
					}
					BaseVehicle parentVehicle = vehiclePrivilege.ParentVehicle;
					if (parentVehicle == null)
					{
						lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
						return false;
					}
					BaseVehicle baseVehicle = target.entity?.GetComponentInParent<BaseVehicle>();
					if (baseVehicle == null || baseVehicle != parentVehicle || !target.player.IsStandingOnEntity(parentVehicle, 134217728))
					{
						lastPlacementError = ConstructionErrors.CannotBuildInTheDeepSea;
						return false;
					}
				}
			}
		}
		List<Socket_Base> obj = Facepunch.Pool.Get<List<Socket_Base>>();
		common.FindMaleSockets(target, obj);
		foreach (Socket_Base item in obj)
		{
			Placement placement = default(Placement);
			if (target.entity != null && target.socket != null && target.entity.IsOccupied(target.socket))
			{
				continue;
			}
			if (!placement.isPopulated)
			{
				placement = item.DoPlacement(target);
			}
			if (target.player != null && target.player.IsInTutorial)
			{
				TutorialIsland currentTutorialIsland = target.player.GetCurrentTutorialIsland();
				if (currentTutorialIsland != null && !currentTutorialIsland.CheckPlacement(common, target, ref placement))
				{
					placement = default(Placement);
				}
			}
			if (!placement.isPopulated)
			{
				continue;
			}
			if (target.player.IsInCreativeMode && Creative.freePlacement)
			{
				transform.SetPositionAndRotation(placement.position, placement.rotation);
				return true;
			}
			if (!item.CheckSocketMods(ref placement))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				continue;
			}
			if (!IOEntity.allow_on_boats && target.entity != null)
			{
				BaseEntity baseEntity = GameManager.server.FindPrefab(common.prefabID)?.GetComponent<BaseEntity>();
				if ((baseEntity is IOEntity iOEntity && !(iOEntity is Signage)) || baseEntity is ElectricOven)
				{
					if (!(target.entity is BoatBuildingBlock))
					{
						BaseEntity entity = target.entity;
						if (!(entity is PlayerBoat) && !(entity is Tugboat) && !target.entity.HasParentBoat(out var _))
						{
							goto IL_03bb;
						}
					}
					transform.position = placement.position;
					transform.rotation = placement.rotation;
					lastPlacementError = ConstructionErrors.CannotBuildOnBoat;
					continue;
				}
			}
			goto IL_03bb;
			IL_03bb:
			if (!TestPlacingThroughRock(ref placement, target))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.ThroughRock;
				continue;
			}
			if (common.HasAlternativeLOSChecks())
			{
				if (target.socket == null && !TestPlacingThroughWall(ref placement, transform, common, target))
				{
					transform.position = placement.position;
					transform.rotation = placement.rotation;
					lastPlacementError = ConstructionErrors.ThroughWalls;
					lastPlacementErrorDebug = "Placing through walls";
					continue;
				}
				if (!Planner.HasLineOfSight(ref placement, common, target))
				{
					transform.position = placement.position;
					transform.rotation = placement.rotation;
					lastPlacementError = ConstructionErrors.LineOfSightBlocked;
					continue;
				}
			}
			else if (!TestPlacingThroughWall(ref placement, transform, common, target))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.ThroughWalls;
				lastPlacementErrorDebug = "Placing through walls";
				continue;
			}
			if (!TestPlacingCloseToRoad(ref placement, target, common))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.TooCloseToRoad;
				continue;
			}
			if (target.entity is Door && target.socket == null)
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.CantDeployOnDoor;
				continue;
			}
			if (Vector3.Distance(placement.position, target.player.eyes.position) > common.maxplaceDistance + 1f)
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.TooFarAway;
				continue;
			}
			DeployVolume[] volumes = PrefabAttribute.server.FindAll<DeployVolume>(prefabID);
			if (DeployVolume.Check(placement.position, placement.rotation, volumes))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				if (DeployVolume.LastDeployHit != null)
				{
					lastPlacementErrorDebug = DeployVolume.LastDeployHit.name;
					string blockedByErrorFromCollider = ConstructionErrors.GetBlockedByErrorFromCollider(DeployVolume.LastDeployHit, target.player);
					if (!string.IsNullOrEmpty(blockedByErrorFromCollider))
					{
						lastPlacementError = blockedByErrorFromCollider;
						lastPlacementErrorIsDetailed = true;
						continue;
					}
				}
				lastPlacementError = ConstructionErrors.NotEnoughSpace;
				continue;
			}
			if (BuildingProximity.Check(target.player, this, placement.position, placement.rotation))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				continue;
			}
			if (common.isBuildingPrivilege && !target.player.CanPlaceBuildingPrivilege(placement.position, placement.rotation, common.bounds))
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.StackPrivilege;
				continue;
			}
			bool flag = target.player.IsBuildingBlocked(placement.position, placement.rotation, common.bounds, cached: false);
			if (!common.canBypassBuildingPermission && flag)
			{
				transform.position = placement.position;
				transform.rotation = placement.rotation;
				lastPlacementError = ConstructionErrors.NoPermission;
				continue;
			}
			target.buildingBlocked = flag;
			transform.SetPositionAndRotation(placement.position, placement.rotation);
			if ((!(target.player != null) || !target.player.IsInCreativeMode || !Creative.bypassHoldToPlaceDuration) && common.holdToPlaceDuration > 0f && target.player != null && isServer && target.player.GetHeldEntity() is Planner planner && (Vector3.Distance(target.player.transform.position, planner.serverStartDurationPlacementPosition) > 1f || Mathf.Abs((float)planner.serverStartDurationPlacementTime - common.holdToPlaceDuration) > 0.5f))
			{
				break;
			}
			Facepunch.Pool.FreeUnmanaged(ref obj);
			return true;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return false;
	}

	private bool TestPlacingThroughRock(ref Placement placement, Target target)
	{
		OBB oBB = new OBB(placement.position, Vector3.one, placement.rotation, bounds);
		Vector3 center = target.player.GetCenter(ducked: true);
		Vector3 origin = target.ray.origin;
		if (UnityEngine.Physics.Linecast(center, origin, 65536, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		RaycastHit hit;
		Vector3 end = (oBB.Trace(target.ray, out hit) ? hit.point : oBB.ClosestPoint(origin));
		if (UnityEngine.Physics.Linecast(origin, end, 65536, QueryTriggerInteraction.Ignore))
		{
			return false;
		}
		return true;
	}

	private static bool TestPlacingThroughWall(ref Placement placement, Transform transform, Construction common, Target target)
	{
		Vector3 position = placement.position;
		if (common.deployOffset != null)
		{
			position += placement.rotation * common.deployOffset.localPosition;
		}
		Vector3 vector = position - target.ray.origin;
		if (!UnityEngine.Physics.Raycast(target.ray.origin, vector.normalized, out var hitInfo, vector.magnitude, 2097152))
		{
			return true;
		}
		StabilityEntity stabilityEntity = RaycastHitEx.GetEntity(hitInfo) as StabilityEntity;
		if (!common.enforceLineOfSightCheckAgainstParentEntity && stabilityEntity != null && target.entity == stabilityEntity)
		{
			return true;
		}
		if (vector.magnitude - hitInfo.distance < 0.2f)
		{
			return true;
		}
		transform.SetPositionAndRotation(hitInfo.point, placement.rotation);
		return false;
	}

	private bool TestPlacingCloseToRoad(ref Placement placement, Target target, Construction construction)
	{
		if (construction.canPlaceOnRoads)
		{
			return true;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		TerrainTopologyMap topologyMap = TerrainMeta.TopologyMap;
		if (heightMap == null)
		{
			return true;
		}
		if (topologyMap == null)
		{
			return true;
		}
		OBB oBB = new OBB(placement.position, Vector3.one, placement.rotation, bounds);
		float num = Mathf.Abs(heightMap.GetHeight(oBB.position) - oBB.position.y);
		if (num > 9f)
		{
			return true;
		}
		float radius = Mathf.Lerp(3f, 0f, num / 9f);
		Vector3 position = oBB.position;
		Vector3 point = oBB.GetPoint(-1f, 0f, -1f);
		Vector3 point2 = oBB.GetPoint(-1f, 0f, 1f);
		Vector3 point3 = oBB.GetPoint(1f, 0f, -1f);
		Vector3 point4 = oBB.GetPoint(1f, 0f, 1f);
		int topology = topologyMap.GetTopology(position, radius);
		int topology2 = topologyMap.GetTopology(point, radius);
		int topology3 = topologyMap.GetTopology(point2, radius);
		int topology4 = topologyMap.GetTopology(point3, radius);
		int topology5 = topologyMap.GetTopology(point4, radius);
		if (((topology | topology2 | topology3 | topology4 | topology5) & 0x80800) == 0)
		{
			return true;
		}
		return false;
	}

	public virtual bool ShowAsNeutral(Target target)
	{
		return target.buildingBlocked;
	}

	public bool TargetIsInteractable(Target target)
	{
		if (target.entity == null)
		{
			return true;
		}
		if (target.entity is BuildingBlock buildingBlock)
		{
			return buildingBlock.Interactable();
		}
		return true;
	}

	public BaseEntity CreateConstruction(Target target, bool bNeedsValidPlacement = false)
	{
		GameObject gameObject = GameManager.server.CreatePrefab(fullName, Vector3.zero, Quaternion.identity, active: false);
		bool flag = UpdatePlacement(gameObject.transform, this, ref target);
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(gameObject);
		if (bNeedsValidPlacement && !flag)
		{
			if (baseEntity.IsValid())
			{
				baseEntity.Kill();
			}
			else
			{
				GameManager.Destroy(gameObject);
			}
			return null;
		}
		DecayEntity decayEntity = baseEntity as DecayEntity;
		if ((bool)decayEntity)
		{
			decayEntity.AttachToBuilding(target.entity as DecayEntity);
		}
		return baseEntity;
	}

	public bool HasMaleSockets(Target target)
	{
		Socket_Base[] array = allSockets;
		foreach (Socket_Base socket_Base in array)
		{
			if (socket_Base.male && !socket_Base.maleDummy && socket_Base.TestTarget(target))
			{
				return true;
			}
		}
		return false;
	}

	public void FindMaleSockets(Target target, List<Socket_Base> sockets)
	{
		Socket_Base[] array = allSockets;
		foreach (Socket_Base socket_Base in array)
		{
			if (socket_Base.male && !socket_Base.maleDummy && socket_Base.TestTarget(target))
			{
				sockets.Add(socket_Base);
			}
		}
	}

	public ConstructionGrade GetGrade(BuildingGrade.Enum iGrade, ulong iSkin)
	{
		ConstructionGrade[] array = grades;
		foreach (ConstructionGrade constructionGrade in array)
		{
			if (constructionGrade.gradeBase.type == iGrade && constructionGrade.gradeBase.skin == iSkin)
			{
				return constructionGrade;
			}
		}
		return defaultGrade;
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		isBuildingPrivilege = rootObj.GetComponent<BuildingPrivlidge>();
		isSleepingBag = rootObj.GetComponent<SleepingBag>();
		bounds = rootObj.GetComponent<BaseEntity>().bounds;
		deployable = GetComponent<Deployable>();
		placeholder = GetComponentInChildren<ConstructionPlaceholder>();
		allSockets = GetComponentsInChildren<Socket_Base>(includeInactive: true);
		allProximities = GetComponentsInChildren<BuildingProximity>(includeInactive: true);
		socketHandle = GetComponentsInChildren<SocketHandle>(includeInactive: true).FirstOrDefault();
		grades = rootObj.GetComponents<ConstructionGrade>();
		ConstructionGrade[] array = grades;
		foreach (ConstructionGrade constructionGrade in array)
		{
			if (!(constructionGrade == null))
			{
				constructionGrade.construction = this;
				if (!(defaultGrade != null))
				{
					defaultGrade = constructionGrade;
				}
			}
		}
	}

	protected override Type GetIndexedType()
	{
		return typeof(Construction);
	}

	public bool HasAlternativeLOSChecks()
	{
		if (alternativeLOSChecks_enabled && alternativeLOSChecks && alternativeLOSPositions != null)
		{
			return alternativeLOSPositions.Length != 0;
		}
		return false;
	}

	private bool CanDeployOnTugboat(Socket_Base baseSocket)
	{
		SocketMod[] socketMods = baseSocket.socketMods;
		foreach (SocketMod socketMod in socketMods)
		{
			if (socketMod is SocketMod_AreaCheck socketMod_AreaCheck)
			{
				if (socketMod_AreaCheck.wantsInside && ((int)socketMod_AreaCheck.layerMask & 0x8000000) > 0)
				{
					return true;
				}
			}
			else if (socketMod is SocketMod_SphereCheck { wantsCollide: not false } socketMod_SphereCheck && ((int)socketMod_SphereCheck.layerMask & 0x8000000) > 0)
			{
				return true;
			}
		}
		return false;
	}
}

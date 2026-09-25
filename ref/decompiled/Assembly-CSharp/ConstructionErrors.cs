using System.Collections.Generic;
using Facepunch;
using Rust.Ai.Gen2;
using UnityEngine;

public static class ConstructionErrors
{
	public static readonly Translate.Phrase NoPermission = new Translate.Phrase("error_buildpermission", "You don't have permission to build here");

	public static readonly Translate.Phrase StackPrivilege = new Translate.Phrase("error_stackprivilege", "Cannot stack building privileges");

	public static readonly Translate.Phrase CantBuildWhileMoving = new Translate.Phrase("error_whilemoving", "You can't build this while moving");

	public static readonly Translate.Phrase ThroughRock = new Translate.Phrase("error_throughrock", "Placing through rock");

	public static readonly Translate.Phrase ThroughWalls = new Translate.Phrase("error_throughwalls", "Placing through walls");

	public static readonly Translate.Phrase InsideObjects = new Translate.Phrase("error_insideobjects", "Can't deploy inside objects");

	public static readonly Translate.Phrase TooCloseToRoad = new Translate.Phrase("error_tooclosetoroad", "Placing too close to road");

	public static readonly Translate.Phrase TooFarAway = new Translate.Phrase("error_toofar", "Too far away");

	public static readonly Translate.Phrase BlockedBy = new Translate.Phrase("error_blockedby", "Blocked by {0}");

	public static readonly Translate.Phrase BlockedByPlayer = new Translate.Phrase("error_blockedbyplayer", "Blocked by Player {0}");

	public static readonly Translate.Phrase BlockedByVehicle = new Translate.Phrase("error_blockedbyvehicle", "Blocked by Vehicle");

	public static readonly Translate.Phrase TooCloseTo = new Translate.Phrase("error_toocloseto", "Too close to {0}");

	public static readonly Translate.Phrase TooCloseToMonument = new Translate.Phrase("error_tooclosetomonument", "Cannot build this close to {0}");

	public static readonly Translate.Phrase BlockedByTree = new Translate.Phrase("error_blockedbytree", "Blocked by tree");

	public static readonly Translate.Phrase BlockedByTerrain = new Translate.Phrase("error_blockedbyterrain", "Blocked by terrain");

	public static readonly Translate.Phrase BlockedByPiping = new Translate.Phrase("error_blockedbypiping", "Blocked by industrial piping");

	public static readonly Translate.Phrase SkinNotOwned = new Translate.Phrase("error_skinnotowned", "Skin not owned");

	public static readonly Translate.Phrase CannotBuildInThisArea = new Translate.Phrase("error_cannotbuildarea", "Cannot build in this area");

	public static readonly Translate.Phrase CannotBuildInTheDeepSea = new Translate.Phrase("error_cannotbuilddeepsea", "Cannot build in the deep sea");

	public static readonly Translate.Phrase CannotBuildOnBoat = new Translate.Phrase("error_cannotbuildboat", "Cannot be deployed on boats");

	public static readonly Translate.Phrase BoatBuildingEntityLimit = new Translate.Phrase("error_boatbuilding_entity_limit", "Cannot deploy any more of this item.");

	public static readonly Translate.Phrase NotEnoughSpace = new Translate.Phrase("error_notenoughspace", "Not enough space");

	public static readonly Translate.Phrase NotStableEnough = new Translate.Phrase("error_notstableenough", "Not stable enough");

	public static readonly Translate.Phrase MustPlaceOnConstruction = new Translate.Phrase("error_wantsconstruction", "Must be placed on a construction");

	public static readonly Translate.Phrase MustPlaceOnBoat = new Translate.Phrase("error_wantsboat", "Must be placed on a player boat in edit mode");

	public static readonly Translate.Phrase CannotPlaceOnBoat = new Translate.Phrase("error_no_boat", "Cannot be placed on boats");

	public static readonly Translate.Phrase RequiresHull = new Translate.Phrase("error_requiresHull", "Must be placed on the boat hull");

	public static readonly Translate.Phrase CantPlaceOnNetting = new Translate.Phrase("error_placement_no_netting", "Can't be placed on boat building netting");

	public static readonly Translate.Phrase MustPlaceOnNetting = new Translate.Phrase("error_placement_needs_netting", "Must be placed on boat building netting");

	public static readonly Translate.Phrase CantPlaceOnConstruction = new Translate.Phrase("error_doesnotwantconstruction", "Cannot be placed on constructions");

	public static readonly Translate.Phrase CantPlaceOnMonument = new Translate.Phrase("error_cantplaceonmonument", "Cannot be placed on monument");

	public static readonly Translate.Phrase NotInTerrain = new Translate.Phrase("error_notinterrain", "Not in terrain");

	public static readonly Translate.Phrase MustPlaceOnRoad = new Translate.Phrase("error_placement_needs_road", "Must be placed on road");

	public static readonly Translate.Phrase CantPlaceOnRoad = new Translate.Phrase("error_placement_no_road", "Cannot be placed on road");

	public static readonly Translate.Phrase InvalidAreaVehicleLarge = new Translate.Phrase("error_invalidarea_vehiclelarge", "Cannot deploy near a large vehicle");

	public static readonly Translate.Phrase InvalidAngle = new Translate.Phrase("error_invalidangle", "Invalid angle");

	public static readonly Translate.Phrase InvalidEntity = new Translate.Phrase("error_invalidentitycheck", "Invalid entity");

	public static readonly Translate.Phrase InvalidEntityType = new Translate.Phrase("error_invalidentitytype", "Invalid entity type");

	public static readonly Translate.Phrase WantsWater = new Translate.Phrase("error_inwater_wants", "Must be placed in water");

	public static readonly Translate.Phrase WantsWaterBody = new Translate.Phrase("error_inwater_wants_body", "Must be placed in a body of water");

	public static readonly Translate.Phrase InWater = new Translate.Phrase("error_inwater", "Can't be placed in water");

	public static readonly Translate.Phrase TooDeep = new Translate.Phrase("error_toodeep", "Water is too deep");

	public static readonly Translate.Phrase TooShallow = new Translate.Phrase("error_shallow", "Water is too shallow");

	public static readonly Translate.Phrase CouldntFindConstruction = new Translate.Phrase("error_counlndfindconstruction", "Couldn't find construction");

	public static readonly Translate.Phrase CouldntFindEntity = new Translate.Phrase("error_counlndfindentity", "Couldn't find entity");

	public static readonly Translate.Phrase CouldntFindSocket = new Translate.Phrase("error_counlndfindsocket", "Couldn't find socket");

	public static readonly Translate.Phrase Antihack = new Translate.Phrase("error_antihack", "Anti hack!");

	public static readonly Translate.Phrase AntihackWithReason = new Translate.Phrase("error_antihack_reason", "Anti hack! ({0})");

	public static readonly Translate.Phrase CantDeployOnDoor = new Translate.Phrase("error_cantdeployondoor", "Can't deploy on door");

	public static readonly Translate.Phrase DeployableMismatch = new Translate.Phrase("error_deployablemismatch", "Deployable mismatch!");

	public static readonly Translate.Phrase LineOfSightBlocked = new Translate.Phrase("error_lineofsightblocked", "Line of sight blocked");

	public static readonly Translate.Phrase ParentTooFar = new Translate.Phrase("error_parenttoofar", "Parent too far away");

	public static readonly Translate.Phrase SocketOccupied = new Translate.Phrase("error_sockectoccupied", "Target socket is occupied");

	public static readonly Translate.Phrase SocketNotFemale = new Translate.Phrase("error_socketnotfemale", "Target socket is not female");

	public static readonly Translate.Phrase WantsInside = new Translate.Phrase("error_wantsinside", "Must be placed inside your base");

	public static readonly Translate.Phrase WantsOutside = new Translate.Phrase("error_wantsoutside", "Can't be placed inside a base");

	public static readonly Translate.Phrase PlayerName = new Translate.Phrase("error_name_player", "Player {0}");

	public static readonly Translate.Phrase HorseName = new Translate.Phrase("error_name_horse", "Horse");

	public static readonly Translate.Phrase ModularCarName = new Translate.Phrase("error_name_modularcar", "Modular Car");

	public static readonly Translate.Phrase TreeName = new Translate.Phrase("error_name_tree", "Tree");

	public static readonly Translate.Phrase DebrisName = new Translate.Phrase("error_name_debris", "Debris");

	public static readonly Translate.Phrase OreName = new Translate.Phrase("error_name_ore", "Ore");

	public static readonly Translate.Phrase DroppedItemName = new Translate.Phrase("error_dropped_item", "Dropped item");

	public static readonly Translate.Phrase CannotAttachToUnauthorized = new Translate.Phrase("error_cannotattachtounauth", "Cannot attach to unauthorized building");

	public static readonly Translate.Phrase CannotConnectTwoBuildings = new Translate.Phrase("error_connecttwobuildings", "Cannot connect two buildings with cupboards");

	public static readonly Translate.Phrase CantUpgradeRecentlyDamaged = new Translate.Phrase("error_upgraderecentlydamaged", "Recently damaged, upgradable in {0} seconds");

	public static readonly Translate.Phrase CantRotateAnymore = new Translate.Phrase("grade_rotationblocked", "Can't rotate this block anymore");

	public static readonly Translate.Phrase CantDemolishAnymore = new Translate.Phrase("grade_demolishblocked", "Can't demolish this block anymore");

	public static string GetTranslatedNameFromEntity(BaseEntity entity, BasePlayer fromPlayer = null)
	{
		if (entity is ModularCar || entity is BaseVehicleModule)
		{
			return ModularCarName.translated;
		}
		if (entity is BaseVehicleSeat && entity.parentEntity.Get(serverside: false) is RidableHorse)
		{
			return HorseName.translated;
		}
		if (entity is BaseNPC2 baseNPC)
		{
			return baseNPC.displayName;
		}
		if (entity is RidableHorse || entity is HorseSaddle)
		{
			return HorseName.translated;
		}
		if (entity is HumanNPC humanNPC)
		{
			return humanNPC.displayName;
		}
		if (entity is BasePlayer { displayName: var arg } basePlayer)
		{
			if (fromPlayer != null)
			{
				arg = NameHelper.GetPlayerNameStreamSafe(fromPlayer, basePlayer);
			}
			return string.Format(PlayerName.translated, arg);
		}
		if (entity is BuildingBlock buildingBlock)
		{
			return PrefabAttribute.server.Find<Construction>(buildingBlock.prefabID).info.name.translated;
		}
		if (entity is DebrisEntity)
		{
			return DebrisName.translated;
		}
		if (entity is TreeEntity)
		{
			return TreeName.translated;
		}
		if (entity is OreResourceEntity)
		{
			return OreName.translated;
		}
		if (entity is DroppedItem)
		{
			return DroppedItemName.translated;
		}
		SprayCan.GetItemDefinitionForEntity(entity, out var def);
		if (def != null)
		{
			return def.displayName.translated;
		}
		return string.Empty;
	}

	public static string GetBlockedByErrorFromEntity(BaseEntity entity, BasePlayer fromPlayer = null)
	{
		string translatedNameFromEntity = GetTranslatedNameFromEntity(entity, fromPlayer);
		if (!string.IsNullOrEmpty(translatedNameFromEntity))
		{
			return string.Format(BlockedBy.translated, translatedNameFromEntity);
		}
		return null;
	}

	public static string GetBlockedByErrorFromCollider(Collider col, BasePlayer fromPlayer = null)
	{
		if (col == null)
		{
			return null;
		}
		PreventBuildingMonumentTag preventBuildingMonumentTag = GetPreventBuildingMonumentTag(col);
		if (preventBuildingMonumentTag != null)
		{
			return string.Format(TooCloseToMonument.translated, preventBuildingMonumentTag.GetAttachedMonument().displayPhrase.translated);
		}
		if (col.TryGetComponent<ColliderInfo_Pipe>(out var _))
		{
			return BlockedByPiping.translated;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(col);
		if (baseEntity != null)
		{
			return GetBlockedByErrorFromEntity(baseEntity, fromPlayer);
		}
		if (col is TerrainCollider)
		{
			return BlockedByTerrain.translated;
		}
		return null;
	}

	public static bool IsBuildBlockedByMonument(Vector3 pos)
	{
		List<Collider> obj = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(pos, 0.1f, obj, 536870912, QueryTriggerInteraction.Collide);
		foreach (Collider item in obj)
		{
			if (item.TryGetComponent<PreventBuildingMonumentTag>(out var component) && component.GetAttachedMonument() != null)
			{
				Pool.FreeUnmanaged(ref obj);
				return true;
			}
		}
		Pool.FreeUnmanaged(ref obj);
		return false;
	}

	public static PreventBuildingMonumentTag GetPreventBuildingMonumentTag(Collider col)
	{
		if (col != null && col.TryGetComponent<PreventBuildingMonumentTag>(out var component) && component.GetAttachedMonument() != null && !component.gameObject.HasCustomTag(GameObjectTag.BlockPlacement))
		{
			return component;
		}
		return null;
	}

	public static void Log(BasePlayer player, string message)
	{
		if (!(player == null) && !string.IsNullOrEmpty(message) && player.isServer && player.net.connection != null && player.net.connection.info.GetBool("client.errortoasts_debug"))
		{
			player.ChatMessage(message);
		}
	}
}

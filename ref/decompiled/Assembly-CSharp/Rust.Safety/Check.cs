using UnityEngine;

namespace Rust.Safety;

public static class Check
{
	public static bool EntityValid(BaseEntity entity)
	{
		if ((bool)entity)
		{
			return entity.IsValid();
		}
		return false;
	}

	public static bool EntityIsServer(BaseEntity entity)
	{
		if (EntityValid(entity))
		{
			return entity.isServer;
		}
		return false;
	}

	public static bool EntityIsClient(BaseEntity entity)
	{
		if (EntityValid(entity))
		{
			return entity.isClient;
		}
		return false;
	}

	public static bool EntitySamePrefabs(BaseEntity entity1, BaseEntity entity2)
	{
		return entity1.prefabID == entity2.prefabID;
	}

	public static bool RefValid<T>(ResourceRef<T> gRef) where T : Object
	{
		return gRef?.isValid ?? false;
	}

	public static bool InBuildingPrivilegeArea(BasePlayer ply)
	{
		if (!(ply.GetBuildingPrivilege() != null))
		{
			return ply.HasPrivilegeFromOther();
		}
		return true;
	}

	public static bool InBuildingPrivilegeArea(BasePlayer ply, bool useCache, float cacheDuration = 1f)
	{
		if (!(ply.GetBuildingPrivilege(useCache, cacheDuration) != null))
		{
			return ply.HasPrivilegeFromOther(useCache);
		}
		return true;
	}

	public static bool HasTCBuildingPrivilege(BasePlayer ply)
	{
		return ply.IsBuildingAuthed();
	}

	public static bool HasTCBuildingPrivilege(BasePlayer ply, bool useCache, float cacheDuration = 1f)
	{
		return ply.IsBuildingAuthed(useCache, cacheDuration);
	}

	public static bool HasOtherBuildingPrivilege(BasePlayer ply)
	{
		return ply.HasPrivilegeFromOther();
	}

	public static bool HasOtherBuildingPrivilege(BasePlayer ply, bool useCache)
	{
		return ply.HasPrivilegeFromOther(useCache);
	}

	public static bool HasBuildingPrivilege(BasePlayer ply)
	{
		if (!ply.IsBuildingAuthed())
		{
			return ply.HasPrivilegeFromOther();
		}
		return true;
	}

	public static bool HasBuildingPrivilege(BasePlayer ply, bool useCache, float cacheDuration = 1f)
	{
		if (!ply.IsBuildingAuthed(useCache, cacheDuration))
		{
			return ply.HasPrivilegeFromOther(useCache);
		}
		return true;
	}

	public static bool IsAuthorisedToBuild(BasePlayer ply)
	{
		if (!InBuildingPrivilegeArea(ply))
		{
			return false;
		}
		return ply.CanBuild();
	}

	public static bool IsAuthorisedToBuild(BasePlayer ply, bool useCache, float cacheDuration = 1f)
	{
		if (!InBuildingPrivilegeArea(ply, useCache))
		{
			return false;
		}
		return ply.CanBuild(useCache, cacheDuration);
	}

	public static bool IsValidAttackTarget(BasePlayer ply)
	{
		if (ply == null)
		{
			return false;
		}
		if (!ply.IsAlive())
		{
			return false;
		}
		if (ply.InSafeZone())
		{
			return false;
		}
		if (ply.IsInTutorial)
		{
			return false;
		}
		if (ply.IsSleeping())
		{
			return false;
		}
		if (ply.isInvisible)
		{
			return false;
		}
		if (ply.IsNpc || ply.IsBot || ply is HumanNPC)
		{
			return false;
		}
		return true;
	}

	public static bool SimplyOnTerrainAt(Vector3 point)
	{
		float height = TerrainMeta.HeightMap.GetHeight(point);
		float oceanLevel = WaterSystem.OceanLevel;
		return height > oceanLevel;
	}

	public static bool IsValidWeapon(Item item, bool checkCanUseTurret)
	{
		ItemDefinition info = item.info;
		if (item.isBroken)
		{
			return false;
		}
		ItemModEntity component = info.GetComponent<ItemModEntity>();
		if (component == null)
		{
			return false;
		}
		HeldEntity component2 = component.entityPrefab.Get().GetComponent<HeldEntity>();
		if (component2 == null)
		{
			return false;
		}
		if (checkCanUseTurret && !component2.IsUsableByTurret)
		{
			return false;
		}
		return true;
	}
}

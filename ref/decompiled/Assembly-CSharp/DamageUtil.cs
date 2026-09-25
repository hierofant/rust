using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public static class DamageUtil
{
	public static void RadiusDamage(BaseEntity attackingPlayer, BaseEntity weaponPrefab, Vector3 pos, float minradius, float radius, List<DamageTypeEntry> damage, int layers, bool useLineOfSight, bool ignoreAI = false, bool ignoreAttackingPlayer = false, bool extendedLineOfSight = false, List<DamageTypeEntry> playerDamage = null, bool removeWallpaper = false, bool includeBoatBuildingPieces = true, BaseEntity ignoreEntity = null)
	{
		using (TimeWarning.New("DamageUtil.RadiusDamage"))
		{
			List<HitInfo> obj = Pool.Get<List<HitInfo>>();
			List<BaseEntity> obj2 = Pool.Get<List<BaseEntity>>();
			List<BaseEntity> obj3 = Pool.Get<List<BaseEntity>>();
			List<BaseEntity> obj4 = Pool.Get<List<BaseEntity>>();
			BaseEntity baseEntity = null;
			Vis.Entities(pos, radius, obj4, layers);
			for (int i = 0; i < obj4.Count; i++)
			{
				BaseEntity baseEntity2 = obj4[i];
				if (!baseEntity2.isServer || ignoreEntity == baseEntity2 || obj2.Contains(baseEntity2) || (!includeBoatBuildingPieces && (baseEntity2 is BoatBuildingBlock || baseEntity2 is global::IBoatBuildingPiece)))
				{
					continue;
				}
				baseEntity = baseEntity2.GetParentEntity();
				bool flag = ShouldSingleHitSharedParentEntity(baseEntity2, baseEntity);
				if ((flag && obj3.Contains(baseEntity)) || (ignoreAI && IsIgnoredAI(baseEntity2)))
				{
					continue;
				}
				Vector3 vector = baseEntity2.ClosestPoint(pos);
				float num = Mathf.Clamp01((Vector3.Distance(vector, pos) - minradius) / (radius - minradius));
				if (num > 1f)
				{
					continue;
				}
				float amount = 1f - num;
				if (flag)
				{
					amount = 0.85f;
				}
				if (removeWallpaper && baseEntity2 is BuildingBlock buildingBlock)
				{
					buildingBlock.RemoveWallpaper(0);
					buildingBlock.RemoveWallpaper(1);
				}
				if ((extendedLineOfSight && !GamePhysics.LineOfSight(baseEntity2.CenterPoint(), pos, 1218519041, baseEntity2)) || (useLineOfSight && !baseEntity2.IsVisible(pos)))
				{
					continue;
				}
				if (useLineOfSight && baseEntity2 is BasePlayer basePlayer && basePlayer.IsDucked())
				{
					Bounds colliderBounds = basePlayer.GetColliderBounds();
					if (colliderBounds.max.y - vector.y < 0.1f && !GamePhysics.LineOfSight(pos, colliderBounds.center.WithY(colliderBounds.max.y - 0.1f), 1218519041, baseEntity2))
					{
						continue;
					}
				}
				HitInfo hitInfo = new HitInfo();
				hitInfo.Initiator = attackingPlayer;
				hitInfo.WeaponPrefab = weaponPrefab;
				if (playerDamage != null && playerDamage.Count > 0 && baseEntity2 is BasePlayer)
				{
					hitInfo.damageTypes.Add(playerDamage);
				}
				else
				{
					hitInfo.damageTypes.Add(damage);
				}
				hitInfo.damageTypes.ScaleAll(amount);
				hitInfo.HitPositionWorld = vector;
				hitInfo.HitNormalWorld = (pos - vector).normalized;
				hitInfo.PointStart = pos;
				hitInfo.PointEnd = hitInfo.HitPositionWorld;
				obj.Add(hitInfo);
				obj2.Add(baseEntity2);
				if (flag)
				{
					obj3.Add(baseEntity);
				}
			}
			for (int j = 0; j < obj2.Count; j++)
			{
				BaseEntity baseEntity3 = obj2[j];
				HitInfo info = obj[j];
				if (!ignoreAttackingPlayer || !(attackingPlayer != null) || !baseEntity3.EqualNetID(attackingPlayer))
				{
					baseEntity3.OnAttacked(info);
				}
			}
			Pool.FreeUnmanaged(ref obj);
			Pool.FreeUnmanaged(ref obj2);
			Pool.FreeUnmanaged(ref obj3);
			Pool.FreeUnmanaged(ref obj4);
		}
	}

	public static bool ShouldSingleHitSharedParentEntity(BaseEntity hitEnt, BaseEntity parentEntity)
	{
		if (parentEntity == null)
		{
			return false;
		}
		if (parentEntity is PlayerBoat)
		{
			if (!(hitEnt is BoatBuildingBlock))
			{
				return hitEnt is global::IBoatBuildingPiece;
			}
			return true;
		}
		return false;
	}

	private static bool IsIgnoredAI(BaseEntity ent)
	{
		return ent is ScientistNPC;
	}
}

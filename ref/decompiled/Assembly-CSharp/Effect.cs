using System.Collections.Generic;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;

public class Effect : EffectData
{
	public enum Type : uint
	{
		Generic = 0u,
		Projectile = 1u,
		PaintballSplat = 3u
	}

	public static class client
	{
		public static void Run(Type fxtype, BaseEntity ent, uint boneID = 0u, Vector3 posLocal = default(Vector3), Vector3 normLocal = default(Vector3))
		{
		}

		public static void Run(string strName, BaseEntity ent, uint boneID = 0u, Vector3 posLocal = default(Vector3), Vector3 normLocal = default(Vector3), Type effectType = Type.Generic, int number = 0, bool ignoreMaxSpawnDistance = false)
		{
			string.IsNullOrEmpty(strName);
		}

		private static void RescaleEffectIfNeeded(BaseEntity ent, ref uint boneID, ref Vector3 posLocal, ref Vector3 normLocal)
		{
			if (boneID != 0 && ent != null && ent.model != null)
			{
				Transform transform = ent.model.FindBone(boneID);
				if (transform != null && transform.localScale != Vector3.one)
				{
					normLocal = transform.TransformDirection(normLocal);
					normLocal = ent.transform.InverseTransformDirection(normLocal).normalized;
					posLocal = transform.TransformPoint(posLocal);
					posLocal = ent.transform.InverseTransformPoint(posLocal);
					boneID = 0u;
				}
			}
		}

		public static void Run(Type fxtype, Vector3 posWorld = default(Vector3), Vector3 normWorld = default(Vector3), Vector3 up = default(Vector3))
		{
		}

		public static void Run(string strName, Vector3 posWorld = default(Vector3), Vector3 normWorld = default(Vector3), Vector3 up = default(Vector3), Type overrideType = Type.Generic, int number = 0, bool ignoreMaxSpawnDistance = false)
		{
			string.IsNullOrEmpty(strName);
		}

		public static void Run(string strName, GameObject obj)
		{
			string.IsNullOrEmpty(strName);
		}

		public static void DoAdditiveImpactEffect(HitInfo info, string effectName)
		{
			if (info.HitEntity.IsValid())
			{
				Run(effectName, info.HitEntity, info.HitBone, info.HitPositionLocal + info.HitNormalLocal * 0.1f, info.HitNormalLocal);
			}
			else
			{
				Run(effectName, info.HitPositionWorld + info.HitNormalWorld * 0.1f, info.HitNormalWorld);
			}
		}

		private static bool CanPlayImpactEffect(HitInfo info)
		{
			if (TerrainMeta.WaterMap != null && info.HitMaterial != Projectile.WaterMaterialID() && info.HitMaterial != Projectile.FleshMaterialID() && WaterLevel.Test(info.HitPositionWorld, waves: false, volumes: false))
			{
				return false;
			}
			return true;
		}

		private static void HandleAdditiveEffects(HitInfo info)
		{
			if (info.damageTypes.Has(DamageType.Explosion))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab");
			}
			if (info.damageTypes.Has(DamageType.Heat))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/fire.prefab");
			}
		}

		public static void ImpactEffect(HitInfo info, string customEffect = null)
		{
			if (!info.DoHitEffects || !CanPlayImpactEffect(info))
			{
				return;
			}
			string materialName = StringPool.Get(info.HitMaterial);
			int number = 0;
			Type type = Type.Generic;
			DamageType damageType;
			if (info.WeaponPrefab is AttackEntity attackEntity)
			{
				damageType = attackEntity.GetDamageTypeForEffect(info);
				number = attackEntity.GetImpactEffectNumberValue(info);
				type = attackEntity.GetEffectType(info);
			}
			else
			{
				damageType = info.damageTypes.GetMajorityDamageType();
			}
			string strName = customEffect ?? EffectDictionary.GetParticle(damageType, materialName);
			string decal = EffectDictionary.GetDecal(damageType, materialName);
			bool ignoreMaxSpawnDistance = false;
			if (info.HitEntity.IsValid())
			{
				if (customEffect == null)
				{
					GameObjectRef impactEffect = info.HitEntity.GetImpactEffect(info);
					if (impactEffect.isValid)
					{
						strName = impactEffect.resourcePath;
					}
				}
				Run(strName, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, type, number, ignoreMaxSpawnDistance);
				if (info.DoDecals)
				{
					Run(decal, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, type, number, ignoreMaxSpawnDistance);
				}
			}
			else
			{
				Run(strName, info.HitPositionWorld, info.HitNormalWorld, default(Vector3), type, number, ignoreMaxSpawnDistance);
				Run(decal, info.HitPositionWorld, info.HitNormalWorld, default(Vector3), type, number, ignoreMaxSpawnDistance);
			}
			if (info.WeaponPrefab is BaseMelee baseMelee)
			{
				string strikeEffectPath = baseMelee.GetStrikeEffectPath(materialName);
				if (info.HitEntity.IsValid())
				{
					Run(strikeEffectPath, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal);
				}
				else
				{
					Run(strikeEffectPath, info.HitPositionWorld, info.HitNormalWorld);
				}
			}
			HandleAdditiveEffects(info);
		}
	}

	public static class server
	{
		public static void Run(Type fxtype, BaseEntity ent, uint boneID = 0u, Vector3 posLocal = default(Vector3), Vector3 normLocal = default(Vector3), Connection sourceConnection = null, bool broadcast = false, List<Connection> targets = null, int number = 0)
		{
			reusableInstance.Init(fxtype, ent, boneID, posLocal, normLocal, sourceConnection);
			reusableInstance.broadcast = broadcast;
			reusableInstance.targets = targets;
			reusableInstance.number = number;
			EffectNetwork.Send(reusableInstance);
		}

		public static void Run(string strName, BaseEntity ent, uint boneID = 0u, Vector3 posLocal = default(Vector3), Vector3 normLocal = default(Vector3), Connection sourceConnection = null, bool broadcast = false, List<Connection> targets = null, int number = 0, Type type = Type.Generic)
		{
			if (!string.IsNullOrEmpty(strName))
			{
				reusableInstance.Init(type, ent, boneID, posLocal, normLocal, sourceConnection);
				reusableInstance.pooledString = strName;
				reusableInstance.broadcast = broadcast;
				reusableInstance.targets = targets;
				reusableInstance.number = number;
				EffectNetwork.Send(reusableInstance);
			}
		}

		public static void Run(Type fxtype, Vector3 posWorld = default(Vector3), Vector3 normWorld = default(Vector3), Connection sourceConnection = null, bool broadcast = false, List<Connection> targets = null, int number = 0)
		{
			reusableInstance.Init(fxtype, posWorld, normWorld, sourceConnection);
			reusableInstance.broadcast = broadcast;
			reusableInstance.targets = targets;
			reusableInstance.number = number;
			EffectNetwork.Send(reusableInstance);
		}

		public static void Run(string strName, Vector3 posWorld = default(Vector3), Vector3 normWorld = default(Vector3), Connection sourceConnection = null, bool broadcast = false, List<Connection> targets = null, int number = 0, Type type = Type.Generic)
		{
			if (!string.IsNullOrEmpty(strName))
			{
				reusableInstance.Init(type, posWorld, normWorld, sourceConnection);
				reusableInstance.pooledString = strName;
				reusableInstance.broadcast = broadcast;
				reusableInstance.targets = targets;
				reusableInstance.number = number;
				EffectNetwork.Send(reusableInstance);
			}
		}

		public static void DoAdditiveImpactEffect(HitInfo info, string effectName)
		{
			if (info.HitEntity.IsValid())
			{
				Run(effectName, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted);
			}
			else
			{
				Run(effectName, info.HitPositionWorld, info.HitNormalWorld, info.Predicted);
			}
		}

		private static bool CanPlayImpactEffect(HitInfo info)
		{
			if (TerrainMeta.WaterMap != null && info.HitMaterial != Projectile.WaterMaterialID() && info.HitMaterial != Projectile.FleshMaterialID() && WaterLevel.Test(info.HitPositionWorld, waves: false, volumes: false))
			{
				return false;
			}
			return true;
		}

		private static void HandleAdditiveEffects(HitInfo info)
		{
			if (info.damageTypes.Has(DamageType.Explosion))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab");
			}
			if (info.damageTypes.Has(DamageType.Heat))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/fire.prefab");
			}
		}

		private static void HandleWeaponEffects(HitInfo info, string materialName)
		{
			if (!info.WeaponPrefab)
			{
				return;
			}
			BaseMelee baseMelee = info.WeaponPrefab as BaseMelee;
			if (baseMelee != null)
			{
				string strikeEffectPath = baseMelee.GetStrikeEffectPath(materialName);
				if (info.HitEntity.IsValid())
				{
					Run(strikeEffectPath, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted);
				}
				else
				{
					Run(strikeEffectPath, info.HitPositionWorld, info.HitNormalWorld, info.Predicted);
				}
			}
		}

		private static bool IsLegalPlacement(HitInfo info)
		{
			Bounds bounds = info.HitEntity.bounds;
			float num = info.HitEntity.AntiHackPadding();
			bounds.extents += new Vector3(num, num, num);
			if (!bounds.Contains(info.HitPositionLocal))
			{
				BasePlayer initiatorPlayer = info.InitiatorPlayer;
				if (initiatorPlayer != null && initiatorPlayer.GetType() == typeof(BasePlayer))
				{
					float num2 = Mathf.Sqrt(bounds.SqrDistance(info.HitPositionLocal));
					if (num2 > ConVar.AntiHack.impact_effect_distance_forgiveness)
					{
						AntiHack.Log(initiatorPlayer, AntiHackType.EffectHack, $"Tried to run an impact effect outside of entity '{info.HitEntity.ShortPrefabName}' bounds by {num2}m");
					}
				}
				return false;
			}
			return true;
		}

		public static void ImpactEffect(HitInfo info, string customEffect = null)
		{
			if (Interface.CallHook("OnImpactEffectCreate", info, customEffect) != null || ((bool)info.InitiatorPlayer && info.InitiatorPlayer.limitNetworking) || !info.DoHitEffects || !CanPlayImpactEffect(info))
			{
				return;
			}
			string materialName = StringPool.Get(info.HitMaterial);
			int number = 0;
			Type type = Type.Generic;
			DamageType damageType;
			if (info.WeaponPrefab is AttackEntity attackEntity)
			{
				damageType = attackEntity.GetDamageTypeForEffect(info);
				number = info.Weapon.GetImpactEffectNumberValue(info);
				type = info.Weapon.GetEffectType(info);
			}
			else
			{
				damageType = info.damageTypes.GetMajorityDamageType();
			}
			string strName = customEffect ?? EffectDictionary.GetParticle(damageType, materialName);
			string decal = EffectDictionary.GetDecal(damageType, materialName);
			if (info.HitEntity.IsValid())
			{
				if (customEffect == null)
				{
					GameObjectRef impactEffect = info.HitEntity.GetImpactEffect(info);
					if (impactEffect.isValid)
					{
						strName = impactEffect.resourcePath;
					}
				}
				if (!IsLegalPlacement(info))
				{
					return;
				}
				Run(strName, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted, broadcast: false, null, number, type);
				Run(decal, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted, broadcast: false, null, number, type);
			}
			else
			{
				Run(strName, info.HitPositionWorld, info.HitNormalWorld, info.Predicted, broadcast: false, null, number, type);
				Run(decal, info.HitPositionWorld, info.HitNormalWorld, info.Predicted, broadcast: false, null, number, type);
			}
			HandleWeaponEffects(info, materialName);
			HandleAdditiveEffects(info);
		}
	}

	public Vector3 upDir;

	public Vector3 worldPos;

	public Vector3 worldNrm;

	public bool attached;

	public Transform transform;

	public GameObject gameObject;

	public string pooledString;

	public bool broadcast;

	public List<Connection> targets;

	private static Effect reusableInstance = new Effect();

	public Effect()
	{
	}

	public Effect(string effectName, Vector3 posWorld, Vector3 normWorld, Connection sourceConnection = null)
	{
		Init(Type.Generic, posWorld, normWorld, sourceConnection);
		pooledString = effectName;
	}

	public Effect(string effectName, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal, Connection sourceConnection = null)
	{
		Init(Type.Generic, ent, boneID, posLocal, normLocal, sourceConnection);
		pooledString = effectName;
	}

	public void Init(Type fxtype, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal, Connection sourceConnection = null)
	{
		Clear();
		type = (uint)fxtype;
		attached = true;
		origin = posLocal;
		normal = normLocal;
		if (ent != null && !ent.IsValid())
		{
			Debug.LogWarning("Effect.Init - invalid entity");
		}
		entity = (ent.IsValid() ? ent.net.ID : default(NetworkableId));
		source = sourceConnection?.userid ?? 0;
		bone = boneID;
	}

	public void Init(Type fxtype, Vector3 posWorld, Vector3 normWorld, Connection sourceConnection = null)
	{
		Clear();
		type = (uint)fxtype;
		attached = false;
		origin = (worldPos = posWorld);
		normal = (worldNrm = normWorld);
		source = sourceConnection?.userid ?? 0;
	}

	public void InitWithSourceEntity(Type fxtype, BaseEntity sourceEntity, Vector3 posWorld, Vector3 normWorld, Connection sourceConnection = null)
	{
		Init(fxtype, posWorld, normWorld, sourceConnection);
		base.sourceEntity = (sourceEntity.IsValid() ? sourceEntity.net.ID : default(NetworkableId));
	}

	public void Clear()
	{
		type = 0u;
		pooledstringid = 0u;
		number = 0;
		origin = default(Vector3);
		normal = default(Vector3);
		scale = 0f;
		entity = default(NetworkableId);
		bone = 0u;
		source = 0uL;
		distanceOverride = 0f;
		ignoreMaxSpawnDistance = false;
		sourceEntity = default(NetworkableId);
		upDir = Vector3.zero;
		worldPos = Vector3.zero;
		worldNrm = Vector3.zero;
		attached = false;
		transform = null;
		gameObject = null;
		pooledString = null;
		broadcast = false;
		targets = null;
	}
}

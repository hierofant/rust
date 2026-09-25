using System;
using Rust;
using UnityEngine;

public class ItemModProjectile : MonoBehaviour
{
	[Serializable]
	public struct SkinOverride
	{
		public GameObjectRef SkinnedWeapon;

		public GameObjectRef OverrideProjectile;
	}

	public GameObjectRef projectileObject = new GameObjectRef();

	public ItemModProjectileMod[] mods;

	public AmmoTypes ammoType;

	public AmmoSubTypes ammoSubType;

	public int numProjectiles = 1;

	public float projectileSpread;

	public float projectileVelocity = 100f;

	public float projectileVelocitySpread;

	public bool useCurve;

	public AnimationCurve spreadScalar;

	public GameObjectRef attackEffectOverride;

	public float barrelConditionLoss;

	public string category = "bullet";

	public SkinOverride[] SkinOverrides = new SkinOverride[0];

	public float GetRandomVelocity()
	{
		return projectileVelocity + UnityEngine.Random.Range(0f - projectileVelocitySpread, projectileVelocitySpread);
	}

	public float GetSpreadScalar()
	{
		if (useCurve)
		{
			return spreadScalar.Evaluate(UnityEngine.Random.Range(0f, 1f));
		}
		return 1f;
	}

	public float GetIndexedSpreadScalar(int shotIndex, int maxShots)
	{
		float num = 0f;
		if (shotIndex != -1)
		{
			float num2 = 1f / (float)maxShots;
			num = (float)shotIndex * num2;
		}
		else
		{
			num = UnityEngine.Random.Range(0f, 1f);
		}
		return spreadScalar.Evaluate(num);
	}

	public float GetAverageVelocity()
	{
		return projectileVelocity;
	}

	public float GetMinVelocity()
	{
		return projectileVelocity - projectileVelocitySpread;
	}

	public float GetMaxVelocity()
	{
		return projectileVelocity + projectileVelocitySpread;
	}

	public bool IsAmmo(AmmoTypes ammo)
	{
		return (ammoType & ammo) != 0;
	}

	public virtual void ServerProjectileHit(HitInfo info)
	{
		if (mods == null)
		{
			return;
		}
		ItemModProjectileMod[] array = mods;
		foreach (ItemModProjectileMod itemModProjectileMod in array)
		{
			if (!(itemModProjectileMod == null))
			{
				itemModProjectileMod.ServerProjectileHit(info);
			}
		}
	}

	public virtual void ServerProjectileHitEntity(HitInfo hitInfo)
	{
		if (mods == null)
		{
			return;
		}
		ItemModProjectileMod[] array = mods;
		foreach (ItemModProjectileMod itemModProjectileMod in array)
		{
			if (!(itemModProjectileMod == null))
			{
				itemModProjectileMod.ServerProjectileHitEntity(hitInfo);
			}
		}
	}

	public GameObjectRef GetOverrideProjectile(BaseEntity firingFrom)
	{
		SkinOverride[] skinOverrides = SkinOverrides;
		for (int i = 0; i < skinOverrides.Length; i++)
		{
			SkinOverride skinOverride = skinOverrides[i];
			if (skinOverride.SkinnedWeapon.resourceID == firingFrom.prefabID)
			{
				return skinOverride.OverrideProjectile;
			}
		}
		return projectileObject;
	}
}

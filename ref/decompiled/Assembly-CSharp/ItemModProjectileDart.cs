using System.Collections.Generic;
using UnityEngine;

public class ItemModProjectileDart : ItemModProjectileMod
{
	public List<ModifierDefintion> modifiers;

	public List<ItemModConsumable.ConsumableEffect> effects;

	public float TurretEffectScale = 0.5f;

	public float TurretDurationScale = 0.5f;

	public override void ServerProjectileHitEntity(HitInfo info)
	{
		base.ServerProjectileHitEntity(info);
		BasePlayer basePlayer = info.HitEntity as BasePlayer;
		if (basePlayer == null)
		{
			return;
		}
		float effectScale = 1f;
		float durationScale = 1f;
		if (info.Initiator != null && info.Initiator is AutoTurret)
		{
			effectScale = TurretEffectScale;
			durationScale = TurretDurationScale;
		}
		PlayerModifiers.AddToPlayer(basePlayer, modifiers, effectScale, durationScale);
		if (effects == null)
		{
			return;
		}
		foreach (ItemModConsumable.ConsumableEffect effect in effects)
		{
			if (!(Mathf.Clamp01(basePlayer.healthFraction + basePlayer.metabolism.pending_health.Fraction()) > effect.onlyIfHealthLessThan))
			{
				if (effect.type == MetabolismAttribute.Type.Health)
				{
					basePlayer.health += effect.amount;
				}
				else
				{
					basePlayer.metabolism.ApplyChange(effect.type, effect.amount, effect.time);
				}
			}
		}
	}
}

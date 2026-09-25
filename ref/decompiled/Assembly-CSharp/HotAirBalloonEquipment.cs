using UnityEngine;

public class HotAirBalloonEquipment : BaseCombatEntity
{
	[SerializeField]
	private DamageRenderer damageRenderer;

	public HealthThresholdToggle healthThresholdToggle;

	[HideInInspector]
	public float DelayNextUpgradeOnRemoveDuration;

	private EntityRef<HotAirBalloon> hotAirBalloon;

	public override float Health()
	{
		if (GetParentEntity() is HotAirBalloon hotAirBalloon)
		{
			return hotAirBalloon.Health();
		}
		return base.Health();
	}

	public override float MaxHealth()
	{
		if (GetParentEntity() is HotAirBalloon hotAirBalloon)
		{
			return hotAirBalloon.MaxHealth();
		}
		return base.MaxHealth();
	}

	public virtual void Added(HotAirBalloon hab, bool fromSave)
	{
		hotAirBalloon.Set(hab);
	}

	public virtual void Removed(HotAirBalloon hab)
	{
		hotAirBalloon.Set(null);
	}

	public override void DoRepair(BasePlayer player)
	{
		HotAirBalloon hotAirBalloon = this.hotAirBalloon.Get(serverside: true);
		if (hotAirBalloon.IsValid())
		{
			hotAirBalloon.DoRepair(player);
		}
	}
}

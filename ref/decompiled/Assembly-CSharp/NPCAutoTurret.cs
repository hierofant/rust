using UnityEngine;

public class NPCAutoTurret : AutoTurret
{
	public Transform centerMuzzle;

	public Transform muzzleLeft;

	public Transform muzzleRight;

	public bool useSleeperHostile = true;

	private bool useLeftMuzzle;

	[ReplicatedVar(Help = "How many seconds until a sleeping player is considered hostile")]
	public static float sleeperhostiledelay = 1200f;

	[ServerVar(Help = "If an npc turret is firing at a sleeping player and the player is blocked, still apply damage")]
	public static bool forceDamageBlockedSleepers = true;

	private Matrix4x4 toCenterMuzzleFromPitch;

	public override void ServerInit()
	{
		base.ServerInit();
		SetOnline();
		SetPeacekeepermode(isOn: true);
		toCenterMuzzleFromPitch = gun_pitch.worldToLocalMatrix * centerMuzzle.localToWorldMatrix;
	}

	public virtual bool HasAmmo()
	{
		return true;
	}

	public override bool CheckPeekers()
	{
		return false;
	}

	public override float TargetScanRate()
	{
		return 1.25f;
	}

	public override bool InFiringArc(BaseCombatEntity potentialtarget)
	{
		return true;
	}

	public override float GetMaxAngleForEngagement()
	{
		return 15f;
	}

	public override bool HasFallbackWeapon()
	{
		return true;
	}

	public override Matrix4x4 GetCenterMuzzle()
	{
		if (base.isServer)
		{
			return base.GetCenterMuzzle() * toCenterMuzzleFromPitch;
		}
		return centerMuzzle.localToWorldMatrix;
	}

	public override void FireGun(Vector3 targetPos, float aimCone, Transform muzzleToUse = null, BaseCombatEntity target = null)
	{
		muzzleToUse = muzzleRight;
		float a = ((target != null) ? target.health : 0f);
		base.FireGun(targetPos, aimCone, muzzleToUse, target);
		float b = ((target != null) ? target.health : 0f);
		if (forceDamageBlockedSleepers && target != null && target is BasePlayer basePlayer && basePlayer.IsSleeping() && Mathf.Approximately(a, b))
		{
			target.Hurt(5f);
		}
	}

	public override bool Ignore(BasePlayer player)
	{
		if (!(player is ScientistNPC))
		{
			return player is BanditGuard;
		}
		return true;
	}

	public override bool IsEntityHostile(BaseCombatEntity ent)
	{
		BasePlayer basePlayer = ent as BasePlayer;
		if (basePlayer != null)
		{
			if (basePlayer.IsNpc)
			{
				if (basePlayer is ScientistNPC || basePlayer is BanditGuard)
				{
					return false;
				}
				if (basePlayer is NPCShopKeeper)
				{
					return false;
				}
				if (basePlayer is BasePet)
				{
					return base.IsEntityHostile(basePlayer);
				}
				return true;
			}
			if (basePlayer.IsSleeping() && useSleeperHostile && basePlayer.secondsSleeping >= sleeperhostiledelay)
			{
				return true;
			}
		}
		return base.IsEntityHostile(ent);
	}

	protected override bool ShouldApplyInterference()
	{
		return false;
	}
}

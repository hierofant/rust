using Rust;
using UnityEngine;

public class BatteringRamHead : BaseCombatEntity
{
	public float damagedHealthThreshold = 100f;

	public float brokenHealthThreshold = 20f;

	public Collider serverCollider;

	[HideInInspector]
	public BatteringRam batteringRamOwner;

	[HideInInspector]
	public DamageRenderer damageRenderer;

	public const Flags Flags_DamagedLow = Flags.Reserved6;

	public const Flags Flags_DamagedMid = Flags.Reserved7;

	public const Flags Flags_DamagedHeavy = Flags.Reserved8;

	public bool CanBeUsed()
	{
		return base.health > brokenHealthThreshold;
	}

	public override void OnRepair()
	{
		base.OnRepair();
		UpdateBrokenFlag();
	}

	public override void OnRepairFinished(BasePlayer player)
	{
		base.OnRepairFinished(player);
		UpdateBrokenFlag();
	}

	public override void OnHealthChanged(float oldvalue, float newvalue)
	{
		base.OnHealthChanged(oldvalue, newvalue);
		UpdateDamageFlags();
	}

	private void UpdateDamageFlags()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		float num = base.healthFraction;
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
		flagsUpdateScope.Set(Flags.Reserved7, b: false);
		flagsUpdateScope.Set(Flags.Reserved8, b: false);
		if (num <= 0.1f)
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
		else if (num <= 0.5f)
		{
			flagsUpdateScope.Set(Flags.Reserved7, b: true);
		}
		else if (num <= 0.75f)
		{
			flagsUpdateScope.Set(Flags.Reserved6, b: true);
		}
	}

	private void UpdateBrokenFlag()
	{
		if (CanBeUsed())
		{
			using (FlagsUpdateScope flagsUpdateScope = batteringRamOwner.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Broken, b: false);
			}
			using FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope2.Set(Flags.Broken, b: false);
		}
	}

	public override float GetDamageRepairCooldown()
	{
		return 0f;
	}

	public void TakeDamage(float damage)
	{
		damage = Mathf.Min(damage, base.health - 10f);
		Hurt(damage, DamageType.Blunt, this, useProtection: false);
		if (!CanBeUsed())
		{
			using (FlagsUpdateScope flagsUpdateScope = batteringRamOwner.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Broken, b: true);
			}
			using FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope2.Set(Flags.Broken, b: true);
		}
	}

	[ServerVar(Help = "(Generated) Deals the specified amount of damage to the battering ram head entity nearest to the calling admin player; admin-only")]
	public static void hurt(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || !basePlayer.IsAdmin)
		{
			return;
		}
		float @float = arg.GetFloat(0);
		BatteringRamHead[] array = Util.FindAll<BatteringRamHead>();
		foreach (BatteringRamHead batteringRamHead in array)
		{
			if (batteringRamHead.isServer && Vector3.Distance(batteringRamHead.transform.position, basePlayer.transform.position) <= 10f)
			{
				batteringRamHead.TakeDamage(@float);
			}
		}
	}
}

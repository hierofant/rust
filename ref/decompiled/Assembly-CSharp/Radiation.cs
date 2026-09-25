using UnityEngine;

public static class Radiation
{
	public enum Tier
	{
		MINIMAL,
		LOW,
		MEDIUM,
		HIGH,
		NONE
	}

	[ServerVar(Help = "(Generated) When enabled, radiation-contaminated water damages loot containers over time when exposed to sufficient radiation")]
	public static bool water_loot_damage = true;

	[ServerVar(Help = "(Generated) When enabled, radiation-contaminated water damages items in a player's inventory over time when in a radiation zone")]
	public static bool water_inventory_damage = true;

	public static float MaterialToRadsRatio = 0.0044f;

	[ServerVar(Help = "(Generated) Multiplier converting radiation material value to effective radiation damage rate; lower values reduce radiation intensity globally")]
	public static float materialToRadsRatio
	{
		get
		{
			return MaterialToRadsRatio;
		}
		set
		{
			MaterialToRadsRatio = value;
		}
	}

	public static float MaxExposureProtection => 0.5f;

	public static float GetRadiation(Tier tier)
	{
		return tier switch
		{
			Tier.NONE => 0f, 
			Tier.MINIMAL => 2f, 
			Tier.LOW => 10f, 
			Tier.MEDIUM => 25f, 
			Tier.HIGH => 51f, 
			_ => 1f, 
		};
	}

	public static float GetRadiationAfterProtection(float radiationAmount, float radiationProtection)
	{
		return Mathf.Clamp(radiationAmount - radiationProtection, 0f, radiationAmount);
	}
}

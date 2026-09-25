using UnityEngine;

public class ItemModWorkbenchReinforced : ItemModWorkbenchUpgrade
{
	[Tooltip("Flat HP to add for Workbench Level 1.")]
	[Header("Reinforced - Per-Bench Health Bonus")]
	public float healthBonusLevel1 = 500f;

	[Tooltip("Flat HP to add for Workbench Level 2.")]
	public float healthBonusLevel2 = 500f;

	[Tooltip("Flat HP to add for Workbench Level 3.")]
	public float healthBonusLevel3 = 500f;

	[Tooltip("Flat HP to add for the IO/Engineering bench.")]
	public float healthBonusIOBench = 500f;

	[Header("Reinforced - Explosive Resistance")]
	[Range(0f, 1f)]
	[Tooltip("Fraction of explosive damage to absorb (0-1).")]
	public float explosiveResistance = 0.5f;

	public float GetHealthBonusForWorkbench(int workbenchLevel, bool ioBench)
	{
		if (ioBench)
		{
			return healthBonusIOBench;
		}
		return workbenchLevel switch
		{
			1 => healthBonusLevel1, 
			2 => healthBonusLevel2, 
			3 => healthBonusLevel3, 
			_ => healthBonusLevel1, 
		};
	}

	public override float GetExplosiveDamageReduction()
	{
		return explosiveResistance;
	}
}

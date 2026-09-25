using UnityEngine;

public class ItemModWorkbenchSalvage : ItemModWorkbenchUpgrade
{
	[Range(0f, 1f)]
	[Tooltip("Fraction to reduce tech-tree unlock cost by (e.g. 0.2 = 20% cheaper).")]
	[Header("Salvage")]
	public float costReduction = 0.2f;

	public override float GetTechTreeCostMultiplier()
	{
		return 1f - costReduction;
	}
}

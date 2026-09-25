using UnityEngine;

public class ItemModWorkbenchPrototype : ItemModWorkbenchUpgrade
{
	[Tooltip("Multiplier applied to scrap cost when bypassing the tech tree path (2 = double).")]
	[Header("Prototype")]
	public float costMultiplier = 2f;

	[Range(0f, 1f)]
	[Tooltip("Probability (0-1) that the unlock fails and consumes resources without granting the blueprint.")]
	public float failChance = 0.1f;

	public override bool CanBypassTechTreePath()
	{
		return true;
	}

	public override float GetTechTreeFailChance()
	{
		return failChance;
	}

	public override float GetBypassCostMultiplier()
	{
		return costMultiplier;
	}
}

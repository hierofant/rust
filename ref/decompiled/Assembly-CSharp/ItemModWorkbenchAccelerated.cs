using UnityEngine;

public class ItemModWorkbenchAccelerated : ItemModWorkbenchUpgrade
{
	[Range(0f, 1f)]
	[Tooltip("Speed bonus multiplier gained per item already crafted in this batch (e.g. 0.25 = 25% faster per item).")]
	[Header("Accelerated Crafting")]
	public float speedBonusPerItem = 0.25f;

	[Range(0.1f, 1f)]
	[Tooltip("Maximum speed multiplier this upgrade can reach (e.g. 0.25 = 4x faster at cap).")]
	public float maxSpeedMultiplier = 0.25f;

	public override float GetCraftSpeedMultiplier(ItemCraftTask task)
	{
		if (task == null || (task.amount <= 1 && task.numCrafted == 0))
		{
			return 1f;
		}
		return Mathf.Clamp(1f - speedBonusPerItem * (float)task.numCrafted, maxSpeedMultiplier, 1f);
	}
}

using System.Collections.Generic;

public interface ISimpleUpgradable
{
	bool UpgradingEnabled();

	bool CanUpgrade(BasePlayer player, ItemDefinition upgradeItem);

	bool CostIsItem();

	void DoUpgrade(BasePlayer player, ItemDefinition upgradeItem);

	List<ItemDefinition> GetUpgradeItems();
}

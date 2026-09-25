public interface ILootContainer
{
	BaseEntity GetEntity();

	ItemContainer GetInventory();

	void SpawnLoot();

	void PopulateLoot();

	float GetLootCountdownTimeRemaining();
}

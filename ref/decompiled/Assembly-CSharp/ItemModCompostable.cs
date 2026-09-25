using ConVar;

public class ItemModCompostable : ItemMod
{
	public float TotalFertilizerProduced = 0.2f;

	public float BaitValue = 1f;

	public int MaxBaitStack;

	public override void OnItemCreated(Item itemcreated)
	{
		if (TotalFertilizerProduced > 0f)
		{
			itemcreated.cookTimeLeft = Server.composterUpdateInterval;
			ItemModCookable.SubscribeCycleCooking(itemcreated);
		}
	}
}

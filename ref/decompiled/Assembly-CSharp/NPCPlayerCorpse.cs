using ConVar;

public class NPCPlayerCorpse : PlayerCorpse
{
	private bool lootEnabled;

	public override float GetRemovalTime()
	{
		return Server.npccorpsedespawn;
	}

	public override bool CanLoot(BasePlayer player)
	{
		if (lootEnabled)
		{
			return base.CanLoot(player);
		}
		return false;
	}

	public void SetLootableIn(float when)
	{
		Invoke(EnableLooting, when);
	}

	public void EnableLooting()
	{
		lootEnabled = true;
	}

	protected override bool CanLootContainer(ItemContainer c, int index)
	{
		if (index == 1 || index == 2)
		{
			return false;
		}
		return base.CanLootContainer(c, index);
	}

	protected override void PreDropItems()
	{
		base.PreDropItems();
		if (containers != null && containers.Length >= 2)
		{
			containers[1].Clear();
			ItemManager.DoRemoves();
		}
	}
}

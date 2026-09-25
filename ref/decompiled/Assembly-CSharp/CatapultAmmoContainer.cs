using System;

public class CatapultAmmoContainer : StorageContainer
{
	[NonSerialized]
	public Catapult catapult;

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		catapult.UpdateLoadedAmmo(item, added);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (catapult != null)
		{
			return catapult.CanBeLooted(player);
		}
		return false;
	}
}

using Facepunch;

public class Gate : Door
{
	public override bool CanBeRedirectSwapped(BasePlayer player)
	{
		using PooledList<BaseEntity> pooledList = Pool.Get<PooledList<BaseEntity>>();
		Vis.Entities(WorldSpaceBounds(), pooledList, -2145386240);
		foreach (BaseEntity item in pooledList)
		{
			if (!(item == null) && !item.isClient && !(item == this) && !(item is BuildingBlock) && !(item is SimpleBuildingBlock) && !(item is Door) && !(item is BaseOven) && !(item is Barricade))
			{
				if (!string.IsNullOrEmpty(ConstructionErrors.GetTranslatedNameFromEntity(item)))
				{
					SprayCan.LastReskinError = ConstructionErrors.BlockedBy;
					SprayCan.LastReskinErrorEntity = item;
				}
				else
				{
					SprayCan.LastReskinError = SprayCan.BlockedBySomething;
				}
				return false;
			}
		}
		return base.CanBeRedirectSwapped(player);
	}
}

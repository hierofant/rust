public static class BaseEntityEx
{
	public static bool IsValidEntityReference<T>(this T obj) where T : class
	{
		return obj as BaseEntity != null;
	}

	public static bool HasEntityInParents(this BaseEntity ent, BaseEntity toFind)
	{
		if (ent == null || toFind == null)
		{
			return false;
		}
		if ((object)ent == toFind)
		{
			return true;
		}
		NetworkableId otherID = ((toFind.net != null) ? toFind.net.ID : NetworkableId.EmptyId);
		if (otherID.IsValid)
		{
			if (ent.EqualNetID(otherID))
			{
				return true;
			}
			BaseEntity parentEntity = ent.GetParentEntity();
			while (parentEntity != null)
			{
				if ((object)parentEntity == toFind || parentEntity.EqualNetID(otherID))
				{
					return true;
				}
				parentEntity = parentEntity.GetParentEntity();
			}
			return false;
		}
		BaseEntity parentEntity2 = ent.GetParentEntity();
		while (parentEntity2 != null)
		{
			if ((object)parentEntity2 == toFind)
			{
				return true;
			}
			parentEntity2 = parentEntity2.GetParentEntity();
		}
		return false;
	}

	public static bool HasColorData(this BaseEntity entity)
	{
		return EntityColorSwapLookup.instance.EntityHasColorData(entity);
	}

	public static bool TryGetColorDataset(this BaseEntity entity, out EntityColorSwapLookup.ColorDataset colorDataset)
	{
		return EntityColorSwapLookup.instance.TryGetEntityColorDataset(entity, out colorDataset);
	}
}

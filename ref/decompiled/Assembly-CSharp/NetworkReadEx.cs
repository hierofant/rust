using Network;

public static class NetworkReadEx
{
	public static BasePlayer Player(this NetRead read)
	{
		return BasePlayer.FindByID(read.UInt64());
	}

	public static BaseEntity Entity(this NetRead read)
	{
		NetworkableId uid = read.EntityID();
		return BaseNetworkable.serverEntities.Find(uid) as BaseEntity;
	}

	public static EntityRef EntityRef(this NetRead read)
	{
		EntityRef result = default(EntityRef);
		result.uid = read.EntityID();
		return result;
	}

	public static EntityRef<T> EntityRef<T>(this NetRead read) where T : BaseEntity
	{
		return new EntityRef<T>(read.EntityID());
	}
}

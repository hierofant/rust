public class PointEntity : BaseEntity
{
}
public class PointEntity<T> : PointEntity where T : PointEntity<T>
{
	public static T ServerInstance;

	public override void PreInitShared()
	{
		base.PreInitShared();
		if (base.isServer)
		{
			ServerInstance = this as T;
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			ServerInstance = null;
		}
	}
}

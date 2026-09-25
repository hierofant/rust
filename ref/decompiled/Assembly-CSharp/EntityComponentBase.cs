using Network;

public class EntityComponentBase : BaseMonoBehaviour
{
	public virtual BaseEntity GetBaseEntity()
	{
		return null;
	}

	public virtual void InitShared()
	{
	}

	public virtual void ResetState()
	{
	}

	public virtual void DestroyShared()
	{
	}

	public virtual void SaveComponent(BaseNetworkable.SaveInfo info)
	{
	}

	public virtual void LoadComponent(BaseNetworkable.LoadInfo info)
	{
	}

	public virtual void OnAttacked(HitInfo hitInfo)
	{
	}

	public virtual void OnChildAdded(BaseEntity child)
	{
	}

	public virtual void OnChildRemoved(BaseEntity child)
	{
	}

	public virtual void Hurt(HitInfo hitInfo)
	{
	}

	public virtual void ServerInitPostNetworkGroupAssign()
	{
	}

	public virtual void OnEntityDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
	}

	public virtual bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		return false;
	}

	public virtual bool OnDied(HitInfo info)
	{
		return true;
	}
}

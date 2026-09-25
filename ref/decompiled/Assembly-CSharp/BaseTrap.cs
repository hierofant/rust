using UnityEngine;

public class BaseTrap : DecayEntity
{
	public virtual void ObjectEntered(GameObject obj)
	{
	}

	public virtual void Arm()
	{
		SetFlagLocal(Flags.On, b: true);
		SendNetworkUpdate();
	}

	public virtual void OnEmpty()
	{
	}
}

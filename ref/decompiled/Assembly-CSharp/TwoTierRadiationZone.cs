using UnityEngine;

public abstract class TwoTierRadiationZone : MonoBehaviour
{
	public TriggerRadiation InnerRadiation;

	public TriggerRadiation OuterRadiation;

	public virtual void Apply(float inner, float outer)
	{
	}

	public virtual void Apply(Bounds inner, Bounds outer)
	{
	}

	public void SetRadiationLevel(float inner, float outer)
	{
		InnerRadiation.RadiationAmountOverride = inner;
		OuterRadiation.RadiationAmountOverride = outer;
	}

	public void SetBypassArmor(bool state)
	{
		InnerRadiation.BypassArmor = state;
		OuterRadiation.BypassArmor = state;
	}

	public void SetIgnoreAboveGroundPlayers(bool state)
	{
		InnerRadiation.IgnoreAboveGroundPlayers = state;
		OuterRadiation.IgnoreAboveGroundPlayers = state;
	}

	public bool HasPlayersInRange()
	{
		if (!CheckRadZone(InnerRadiation))
		{
			return CheckRadZone(OuterRadiation);
		}
		return true;
		static bool CheckRadZone(TriggerRadiation r)
		{
			if (r != null && r.entityContents != null)
			{
				foreach (BaseEntity entityContent in r.entityContents)
				{
					if (entityContent is BasePlayer { IsNpc: false })
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}

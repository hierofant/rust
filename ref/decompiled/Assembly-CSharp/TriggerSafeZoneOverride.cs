using System.Collections.Generic;
using ConVar;
using UnityEngine;

public class TriggerSafeZoneOverride : TriggerBase, IServerComponent
{
	public static List<TriggerSafeZoneOverride> allHostileZones = new List<TriggerSafeZoneOverride>();

	public Collider triggerCollider { get; private set; }

	public ApartmentRoom Apartment { get; set; }

	public bool IsCombatActive
	{
		get
		{
			if (!ApartmentCommands.allowcombatoutsideofbreakin && Apartment != null)
			{
				return Apartment.IsBreakInActive();
			}
			return true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		triggerCollider = GetComponent<Collider>();
		base.InterestLayers = (int)base.InterestLayers | 0x200;
		Apartment = GetComponentInParent<ApartmentRoom>();
	}

	protected void OnEnable()
	{
		allHostileZones.Add(this);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		allHostileZones.Remove(this);
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		if (Apartment != null && Apartment.isServer && ent is BasePlayer { IsBot: false, isServer: not false } basePlayer)
		{
			Apartment.OnPlayerEnterCombatZone(basePlayer);
		}
	}
}

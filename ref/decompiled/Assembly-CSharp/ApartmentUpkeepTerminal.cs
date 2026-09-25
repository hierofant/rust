using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ApartmentUpkeepTerminal : StorageContainer
{
	public GameObject assignDialog;

	public ApartmentRoom Apartment { get; set; }

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.apartmentUpkeep = Pool.Get<ProtoBuf.ApartmentUpkeepTerminal>();
			info.msg.apartmentUpkeep.apartmentId = ((Apartment != null) ? Apartment.net.ID : default(NetworkableId));
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (base.inventory != null && Apartment != null)
		{
			base.inventory.maxStackSize = Mathf.RoundToInt(Apartment.GetDailyUpkeepCost() * 3f);
		}
		if (Apartment != null)
		{
			Apartment.SendNetworkUpdate();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}
}

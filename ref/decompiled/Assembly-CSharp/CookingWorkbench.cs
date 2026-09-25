using Rust;
using UnityEngine;

public class CookingWorkbench : MixingTable
{
	public GameObjectRef SubOvenPrefab;

	public Transform SubOvenPosition;

	[Tooltip("The recipes that will set the OvenCooking flag (to play the oven effects)")]
	public ItemDefinition[] ovenCookingFlagItems;

	public const Flags OvenCooking = Flags.Reserved9;

	public const Flags MixingTea = Flags.Reserved10;

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!base.isServer || (next & Flags.On) == Flags.On == ((old & Flags.On) == Flags.On))
		{
			return;
		}
		string text = ((currentRecipe != null) ? currentRecipe.ProducedItem.shortname : currentProductionItem?.shortname);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		bool flag = GetChildBbq().IsOn();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		ItemDefinition[] array = ovenCookingFlagItems;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].shortname == text)
			{
				flagsUpdateScope.Set(Flags.Reserved9, (next & Flags.On) == Flags.On || flag);
				return;
			}
		}
		flagsUpdateScope.Set(Flags.Reserved10, (next & Flags.On) == Flags.On);
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		SpawnOven();
	}

	private void SpawnOven()
	{
		if (!(GetChildBbq() != null) && SubOvenPrefab.isValid)
		{
			BaseEntity baseEntity = base.gameManager.CreateEntity(SubOvenPrefab.resourcePath, SubOvenPosition.position, SubOvenPosition.rotation);
			baseEntity.SetParent(this, worldPositionStays: true);
			baseEntity.transform.localPosition = SubOvenPosition.localPosition;
			baseEntity.transform.localRotation = SubOvenPosition.localRotation;
			baseEntity.OwnerID = base.OwnerID;
			baseEntity.Spawn();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			SpawnOven();
		}
	}

	private CookingWorkbenchBbq GetChildBbq()
	{
		foreach (BaseEntity child in children)
		{
			if (child is CookingWorkbenchBbq result)
			{
				return result;
			}
		}
		return null;
	}

	protected override bool CanCompletePickup(BasePlayer player)
	{
		CookingWorkbenchBbq childBbq = GetChildBbq();
		if (childBbq != null && childBbq.inventory != null && !childBbq.inventory.IsEmpty())
		{
			pickupErrorToFormat = (format: PickupErrors.ItemInventoryMustBeEmpty, arg0: pickup.itemTarget.displayName);
			return false;
		}
		return base.CanCompletePickup(player);
	}

	internal override void DoServerDestroy()
	{
		CookingWorkbenchBbq childBbq = GetChildBbq();
		if (childBbq != null)
		{
			childBbq.DropItems();
			childBbq.Kill();
		}
		base.DoServerDestroy();
	}
}

using Facepunch.Rust;
using UnityEngine;

public class ItemModCookable : ItemMod
{
	[ItemSelector]
	public ItemDefinition becomeOnCooked;

	public float cookTime = 30f;

	public float amountOfBecome = 1f;

	public int lowTemp;

	public int highTemp;

	public bool setCookingFlag;

	public void OnValidate()
	{
		if (amountOfBecome < 1f)
		{
			amountOfBecome = 1f;
		}
		if (becomeOnCooked == null)
		{
			Debug.LogWarning("[ItemModCookable] becomeOnCooked is unset! [" + base.name + "]", base.gameObject);
		}
	}

	public static void CycleCooking(Item item, float delta)
	{
		CookableItemInfo cookableItemInfo = item.info.ItemModCookable;
		if (item.GetEntityOwner() is Composter)
		{
			cookableItemInfo = item.info.ItemModCompostable;
		}
		if (cookableItemInfo == null)
		{
			return;
		}
		using (TimeWarning.New("ItemModCookable:CycleCooking"))
		{
			if (!cookableItemInfo.CanBeCookedByAtTemperature(item.temperature) || item.cookTimeLeft < 0f)
			{
				if (cookableItemInfo.setCookingFlag && item.HasFlag(Item.Flag.Cooking))
				{
					item.SetFlag(Item.Flag.Cooking, b: false);
					item.MarkDirty();
				}
				return;
			}
			if (cookableItemInfo.setCookingFlag && !item.HasFlag(Item.Flag.Cooking))
			{
				item.SetFlag(Item.Flag.Cooking, b: true);
				item.MarkDirty();
			}
			int num = Mathf.FloorToInt(item.cookTimeLeft / 5f);
			item.cookTimeLeft -= delta;
			if (item.cookTimeLeft > 0f)
			{
				int num2 = Mathf.FloorToInt(item.cookTimeLeft / 5f);
				if (num != num2)
				{
					item.MarkDirty();
				}
				return;
			}
			float num3 = item.cookTimeLeft * -1f;
			int a = 1 + Mathf.FloorToInt(num3 / cookableItemInfo.cookTime);
			item.cookTimeLeft = cookableItemInfo.cookTime - num3 % cookableItemInfo.cookTime;
			BaseOven baseOven = item.GetEntityOwner() as BaseOven;
			a = Mathf.Min(a, item.amount);
			if (item.amount > a)
			{
				item.amount -= a;
				item.MarkDirty();
			}
			else
			{
				item.Remove();
			}
			Analytics.Azure.AddPendingItems(baseOven, item.info.shortname, a, "smelt");
			if (cookableItemInfo.becomeOnCooked == null)
			{
				return;
			}
			float num4 = cookableItemInfo.amountOfBecome * (float)a;
			int num5 = Mathf.FloorToInt(num4);
			if (num4 != (float)num5)
			{
				float num6 = num4 - (float)num5;
				if (Random.value < num6)
				{
					num5++;
				}
			}
			if (num5 == 0)
			{
				return;
			}
			bool flag = false;
			foreach (Item item3 in item.parent.itemList)
			{
				if (cookableItemInfo.becomeOnCooked == item3.info && item3.amount + num5 < cookableItemInfo.becomeOnCooked.stackable)
				{
					item3.amount += num5;
					item3.MarkDirty();
					flag = true;
					break;
				}
			}
			Analytics.Azure.AddPendingItems(baseOven, cookableItemInfo.becomeOnCooked.shortname, num5, "smelt", consumed: false);
			if (item.parent.entityOwner != null && item.parent.entityOwner.net.group.restricted)
			{
				TutorialIsland closestTutorialIsland = TutorialIsland.GetClosestTutorialIsland(item.parent.entityOwner.transform.position, 50f);
				if (closestTutorialIsland != null)
				{
					BasePlayer basePlayer = closestTutorialIsland.ForPlayer.Get(serverside: true);
					if (basePlayer != null)
					{
						basePlayer.ProcessMissionEvent(BaseMission.MissionEventType.COOK, new BaseMission.MissionEventPayload
						{
							IntIdentifier = cookableItemInfo.becomeOnCooked.itemid,
							WorldPosition = item.parent.entityOwner.transform.position,
							NetworkIdentifier = item.parent.entityOwner.net.ID
						}, num5);
					}
				}
			}
			if (flag)
			{
				return;
			}
			Item item2 = ItemManager.Create(cookableItemInfo.becomeOnCooked, num5, 0uL, isServerSide: true, 0uL);
			if (item2 != null && !item2.MoveToContainer(item.parent) && !item2.MoveToContainer(item.parent))
			{
				item2.Drop(item.parent.dropPosition, item.parent.dropVelocity);
				if ((bool)item.parent.entityOwner && baseOven != null)
				{
					baseOven.OvenFull();
				}
			}
		}
	}

	public override void OnItemCreated(Item itemcreated)
	{
		itemcreated.cookTimeLeft = cookTime;
		SubscribeCycleCooking(itemcreated);
	}

	public static void SubscribeCycleCooking(Item item)
	{
		item.onCycle -= CycleCooking;
		item.onCycle += CycleCooking;
	}
}

using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Oxide.Core;
using UnityEngine;

public class NPCApartmentSecurity : NPCTalking
{
	public override void OnConversationAction(BasePlayer player, string msg)
	{
		base.OnConversationAction(player, msg);
		if (msg == "PaidDoor")
		{
			OnPaidToll(player, base.transform.position);
		}
		else if (msg == "PaidKey")
		{
			OnPurchaseKey(player, base.transform.position);
		}
	}

	public static void OnPaidToll(BasePlayer player, Vector3 position, bool doPayment = true)
	{
		if (doPayment)
		{
			ItemDefinition itemDefinition = ItemManager.FindItemDefinition("scrap");
			int num = 50;
			if (player.inventory.GetAmount(itemDefinition) < num)
			{
				player.ChatMessage($"You need {num} scrap");
				return;
			}
			player.inventory.Take(null, itemDefinition.itemid, num);
		}
		List<TimerSwitch> obj = Facepunch.Pool.Get<List<TimerSwitch>>();
		Vis.Entities(position, 10f, obj);
		TimerSwitch timerSwitch = obj.OrderByDescending((TimerSwitch x) => Vector3.Distance(position, player.transform.position)).FirstOrDefault((TimerSwitch x) => !x.isClient);
		if (timerSwitch != null)
		{
			timerSwitch.timerLength = ApartmentCommands.apartmentsecurityaccesstime;
			timerSwitch.SendNetworkUpdateImmediate();
			timerSwitch.SwitchPressed();
			timerSwitch.SendNetworkUpdate();
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	public static void OnPurchaseKey(BasePlayer player, Vector3 position)
	{
		if (Interface.CallHook("OnApartmentMasterKeyPurchase", player) != null)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("scrap");
		int masterkeyprice = ApartmentCommands.masterkeyprice;
		if (player.inventory.GetAmount(itemDefinition) < masterkeyprice)
		{
			return;
		}
		ItemDefinition masterKey = ItemManager.Items.MasterKey;
		if (!(masterKey == null))
		{
			player.inventory.Take(null, itemDefinition.itemid, masterkeyprice);
			Item item = ItemManager.Create(masterKey, 1, 0uL, isServerSide: true, 0uL);
			if (item != null)
			{
				item.AddItemOwnership(player, ItemOwnershipPhrases.VendorSale);
				player.GiveItem(item, GiveItemReason.PickedUp);
				Interface.CallHook("OnApartmentMasterKeyPurchased", player, item);
			}
		}
	}

	public bool Conversation_CanAffordMasterKey(BasePlayer player)
	{
		object obj = Interface.CallHook("CanAffordApartmentMasterKey", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		ItemDefinition definition = ItemManager.FindItemDefinition("scrap");
		return player.inventory.GetAmount(definition) >= ApartmentCommands.masterkeyprice;
	}
}

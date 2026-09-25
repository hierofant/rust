using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch;
using Newtonsoft.Json;
using Rust.UI.MainMenu;
using Steamworks;
using UnityEngine;

namespace ConVar;

[Factory("inventory")]
public class Inventory : ConsoleSystem
{
	public class LoadoutFileInfo
	{
		[JsonProperty("fileName")]
		public string fileName;

		[JsonProperty("displayName")]
		public string displayName;

		[JsonProperty("createdTimestamp")]
		public long createdTimestamp;

		[JsonProperty("itemCount")]
		public int itemCount;

		[JsonProperty("belt")]
		public SavedLoadout.SavedItem[] belt;

		[JsonProperty("wear")]
		public SavedLoadout.SavedItem[] wear;
	}

	public class SavedLoadout
	{
		public struct SavedItem
		{
			[JsonProperty("id")]
			public int id;

			[JsonProperty("amount")]
			public int amount;

			[JsonProperty("skin")]
			public ulong skin;

			[JsonProperty("containedItems")]
			public int[] containedItems;

			[JsonProperty("blueprintTarget")]
			public int blueprintTarget;
		}

		[JsonProperty("belt")]
		public SavedItem[] belt;

		[JsonProperty("wear")]
		public SavedItem[] wear;

		[JsonProperty("main")]
		public SavedItem[] main;

		[JsonProperty("backpack")]
		public SavedItem[] backpack;

		[JsonProperty("heldItemIndex")]
		public int heldItemIndex;

		public SavedLoadout()
		{
		}

		public SavedLoadout(BasePlayer player)
		{
			belt = SaveItems(player.inventory.containerBelt);
			wear = SaveItems(player.inventory.containerWear);
			main = SaveItems(player.inventory.containerMain);
			Item backpackWithInventory = player.inventory.GetBackpackWithInventory();
			if (backpackWithInventory != null)
			{
				backpack = SaveItems(backpackWithInventory.contents);
			}
			heldItemIndex = GetSlotIndex(player);
		}

		public SavedLoadout(PlayerInventoryProperties properties)
		{
			belt = SaveItems(properties.belt);
			wear = SaveItems(properties.wear);
			main = SaveItems(properties.main);
			heldItemIndex = 0;
		}

		private static SavedItem[] SaveItems(ItemContainer itemContainer)
		{
			List<SavedItem> list = new List<SavedItem>();
			for (int i = 0; i < itemContainer.capacity; i++)
			{
				Item slot = itemContainer.GetSlot(i);
				if (slot == null)
				{
					continue;
				}
				SavedItem savedItem = default(SavedItem);
				savedItem.id = slot.info.itemid;
				savedItem.amount = slot.amount;
				savedItem.skin = slot.skin;
				savedItem.blueprintTarget = slot.blueprintTarget;
				SavedItem item = savedItem;
				if (slot.contents != null && slot.contents.itemList != null)
				{
					List<int> list2 = new List<int>();
					foreach (Item item2 in slot.contents.itemList)
					{
						list2.Add(item2.info.itemid);
					}
					item.containedItems = list2.ToArray();
				}
				list.Add(item);
			}
			return list.ToArray();
		}

		private static SavedItem[] SaveItems(List<PlayerInventoryProperties.ItemAmountSkinned> items)
		{
			List<SavedItem> list = new List<SavedItem>();
			foreach (PlayerInventoryProperties.ItemAmountSkinned item2 in items)
			{
				SavedItem savedItem = default(SavedItem);
				savedItem.id = item2.itemid;
				savedItem.amount = (int)item2.amount;
				savedItem.skin = item2.skinOverride;
				SavedItem item = savedItem;
				if (item2.blueprint)
				{
					item.blueprintTarget = item.id;
					item.id = ItemManager.blueprintBaseDef.itemid;
				}
				list.Add(item);
			}
			return list.ToArray();
		}

		public void LoadItemsOnTo(BasePlayer player)
		{
			player.inventory.containerMain.Clear();
			player.inventory.containerBelt.Clear();
			player.inventory.containerWear.Clear();
			ItemManager.DoRemoves();
			LoadItems(belt, player.inventory.containerBelt);
			LoadItems(wear, player.inventory.containerWear);
			LoadItems(main, player.inventory.containerMain);
			if (backpack != null && backpack.Length != 0)
			{
				Item backpackWithInventory = player.inventory.GetBackpackWithInventory();
				if (backpackWithInventory != null)
				{
					backpackWithInventory.contents.Clear();
					LoadItems(backpack, backpackWithInventory.contents);
				}
			}
			EquipItemInSlot(player, heldItemIndex);
			player.inventory.SendSnapshot();
			void LoadItems(SavedItem[] items, ItemContainer container)
			{
				foreach (SavedItem item in items)
				{
					player.inventory.GiveItem(LoadItem(item), container);
				}
			}
		}

		private Item LoadItem(SavedItem item)
		{
			Item item2 = ItemManager.CreateByItemID(item.id, item.amount, item.skin, 0uL);
			if (item.blueprintTarget != 0)
			{
				item2.blueprintTarget = item.blueprintTarget;
			}
			if (item.containedItems != null && item.containedItems.Length != 0)
			{
				int[] containedItems = item.containedItems;
				foreach (int itemID in containedItems)
				{
					item2.contents.AddItem(ItemManager.FindItemDefinition(itemID), 1, 0uL);
				}
			}
			return item2;
		}
	}

	[ReplicatedVar(Help = "Disables all attire limitations, so NPC clothing and invalid overlaps can be equipped")]
	public static bool disableAttireLimitations;

	public const string LoadoutDirectory = "loadouts";

	[ServerVar(Help = "(Generated) When enabled, ownership tracking is applied to stackable items; disabled by default due to performance cost; servers can enable for full ownership auditing")]
	public static bool stackable_item_ownership;

	[ServerUserVar(Name = "lighttoggle")]
	public static void lighttoggle_sv(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer && !basePlayer.IsDead() && !basePlayer.IsSleeping() && !basePlayer.InGesture)
		{
			basePlayer.LightToggle();
		}
	}

	[ServerUserVar]
	public static void endloot(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((bool)basePlayer && !basePlayer.IsDead() && !basePlayer.IsSleeping())
		{
			basePlayer.inventory.loot.Clear();
		}
	}

	[ServerVar(Help = "{item} {amount} {condition} {skin} {container} {slot}")]
	public static void give(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		bool flag = arg.HasArg("--silent", remove: true);
		Item item = ItemManager.CreateByPartialName(arg.GetString(0), 1, arg.GetULong(3, 0uL));
		if (item == null)
		{
			arg.ReplyWith("Invalid Item!");
			return;
		}
		int num = (item.amount = arg.GetInt(1, 1));
		float @float = arg.GetFloat(2, 1f);
		item.conditionNormalized = @float;
		item.OnVirginSpawn();
		item.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		string @string = arg.GetString(4);
		int num2 = arg.GetInt(5, -1);
		ItemContainer itemContainer = null;
		switch (@string)
		{
		case "0":
		case "main":
			itemContainer = basePlayer.inventory.containerMain;
			break;
		case "1":
		case "belt":
			itemContainer = basePlayer.inventory.containerBelt;
			break;
		case "2":
		case "wear":
			itemContainer = basePlayer.inventory.containerWear;
			break;
		}
		if (itemContainer == null)
		{
			if (!basePlayer.inventory.GiveItem(item))
			{
				item.Remove();
				arg.ReplyWith("Couldn't give item (inventory full?)");
				return;
			}
		}
		else
		{
			if (num2 != -1)
			{
				Item slot = itemContainer.GetSlot(num2);
				if (slot != null && slot.contents != null)
				{
					itemContainer = slot.contents;
					num2 = -1;
				}
			}
			if (!item.MoveToContainer(itemContainer, num2))
			{
				item.Remove();
				arg.ReplyWith("Couldn't give item (inventory full?)");
				return;
			}
		}
		if (!flag)
		{
			basePlayer.Command("note.inv", item.info.itemid, num);
		}
		Debug.Log("giving " + UI_Utils.StreamerModeSanitizeShort(basePlayer.displayName) + " " + num + " x " + item.info.displayName.english);
		if (basePlayer.IsDeveloper)
		{
			if (!flag)
			{
				basePlayer.ChatMessage("you silently gave yourself " + num + " x " + item.info.displayName.english);
			}
		}
		else
		{
			Chat.BroadcastPlayerAction(basePlayer, "gave themselves " + num + " x " + item.info.displayName.english);
		}
	}

	[ServerVar(Help = "{item}")]
	public static void giveequip(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		bool flag = arg.HasArg("--silent", remove: true);
		Item item = ItemManager.CreateByPartialName(arg.GetString(0), 1, arg.GetULong(3, 0uL));
		if (item == null)
		{
			arg.ReplyWith("Invalid Item!");
			return;
		}
		int num = (item.amount = arg.GetInt(1, 1));
		float @float = arg.GetFloat(2, 1f);
		item.conditionNormalized = @float;
		item.OnVirginSpawn();
		item.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		int iTargetPos = -1;
		if (!item.MoveToContainer(basePlayer.inventory.containerMain, iTargetPos))
		{
			item.Remove();
			arg.ReplyWith("Couldn't give item (inventory full?)");
			return;
		}
		if (!item.MoveToContainer(basePlayer.inventory.containerWear, iTargetPos))
		{
			item.Remove();
			arg.ReplyWith("Couldn't give item (inventory full?)");
			return;
		}
		if (!flag)
		{
			basePlayer.Command("note.inv", item.info.itemid, num);
		}
		Debug.Log("giving " + basePlayer.displayName + " " + num + " x " + item.info.displayName.english);
		if (basePlayer.IsDeveloper)
		{
			if (!flag)
			{
				basePlayer.ChatMessage("you silently gave yourself " + num + " x " + item.info.displayName.english);
			}
		}
		else
		{
			Chat.BroadcastPlayerAction(basePlayer, "gave themselves " + num + " x " + item.info.displayName.english);
		}
	}

	[ServerVar(Help = "(Generated) Resets all blueprints for the specified player (by name/Steam ID) back to the default unlocked set, removing any learned recipes")]
	public static void resetbp(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayer(arg, 0);
		if (basePlayer == null)
		{
			if (arg.HasArgs())
			{
				arg.ReplyWith("Can't find player");
				return;
			}
			basePlayer = ArgEx.Player(arg);
		}
		basePlayer.blueprints.Reset();
	}

	[ServerVar(Help = "(Generated) Unlocks every craftable blueprint for the specified player (by name/Steam ID), giving them access to all recipes immediately")]
	public static void unlockall(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayer(arg, 0);
		if (basePlayer == null)
		{
			if (arg.HasArgs())
			{
				arg.ReplyWith("Can't find player");
				return;
			}
			basePlayer = ArgEx.Player(arg);
		}
		basePlayer.blueprints.UnlockAll();
	}

	[ServerVar(Help = "(Generated) Gives the specified item (by partial name) to every currently connected player on the server; broadcasts who issued the command in chat")]
	public static void giveall(Arg arg)
	{
		Item item = null;
		string username = "SERVER";
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer != null)
		{
			username = basePlayer.displayName;
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			item = ItemManager.CreateByPartialName(arg.GetString(0), 1, 0uL);
			if (item == null)
			{
				arg.ReplyWith("Invalid Item!");
				return;
			}
			int num = (item.amount = arg.GetInt(1, 1));
			item.OnVirginSpawn();
			item.SetItemOwnership(username, ItemOwnershipPhrases.SpawnedPhrase);
			if (!activePlayer.inventory.GiveItem(item))
			{
				item.Remove();
				arg.ReplyWith("Couldn't give item (inventory full?)");
				continue;
			}
			activePlayer.Command("note.inv", item.info.itemid, num);
			Debug.Log(" [ServerVar] giving " + activePlayer.displayName + " " + item.amount + " x " + item.info.displayName.english);
		}
		if (item != null)
		{
			Chat.BroadcastPlayerAction(basePlayer, "gave everyone " + item.amount + " x " + item.info.displayName.english);
		}
	}

	[ServerVar(Help = "{item} {player} {amount} {skin}")]
	public static void giveto(Arg arg)
	{
		BasePlayer basePlayer = BasePlayer.Find(arg.GetString(0));
		if (basePlayer == null)
		{
			arg.ReplyWith("Couldn't find player!");
			return;
		}
		Item item = ItemManager.CreateByPartialName(arg.GetString(1), 1, arg.GetULong(3, 0uL));
		if (item == null)
		{
			arg.ReplyWith("Invalid Item!");
			return;
		}
		int num = (item.amount = arg.GetInt(2, 1));
		item.OnVirginSpawn();
		item.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		if (!basePlayer.inventory.GiveItem(item))
		{
			item.Remove();
			arg.ReplyWith("Couldn't give item (inventory full?)");
			return;
		}
		basePlayer.Command("note.inv", item.info.itemid, num);
		Debug.Log(" [ServerVar] giving " + basePlayer.displayName + " " + num + " x " + item.info.displayName.english);
		Chat.BroadcastPlayerAction(ArgEx.Player(arg), " gave ", basePlayer, " " + num + " x " + item.info.displayName.english);
	}

	[ServerVar(Help = "{itemid} {amount}")]
	public static void giveid(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		Item item = ItemManager.CreateByItemID(arg.GetInt(0), 1, 0uL, 0uL);
		if (item == null)
		{
			arg.ReplyWith("Invalid Item!");
			return;
		}
		int num = (item.amount = arg.GetInt(1, 1));
		item.OnVirginSpawn();
		item.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		if (!basePlayer.inventory.GiveItem(item))
		{
			item.Remove();
			arg.ReplyWith("Couldn't give item (inventory full?)");
			return;
		}
		basePlayer.Command("note.inv", item.info.itemid, num);
		Debug.Log(" [ServerVar] giving " + UI_Utils.StreamerModeSanitizeShort(basePlayer.displayName) + " " + num + " x " + item.info.displayName.english);
		if (basePlayer.IsDeveloper)
		{
			basePlayer.ChatMessage("you silently gave yourself " + num + " x " + item.info.displayName.english);
		}
		else
		{
			Chat.BroadcastPlayerAction(basePlayer, "gave themselves " + num + " x " + item.info.displayName.english);
		}
	}

	[ServerVar(Help = "{itemid} {amount}")]
	public static void givearm(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		Item item = ItemManager.CreateByItemID(arg.GetInt(0), 1, 0uL, 0uL);
		if (item == null)
		{
			arg.ReplyWith("Invalid Item!");
			return;
		}
		int num = (item.amount = arg.GetInt(1, 1));
		item.OnVirginSpawn();
		item.SetItemOwnership(basePlayer, ItemOwnershipPhrases.SpawnedPhrase);
		if (!basePlayer.inventory.GiveItem(item, basePlayer.inventory.containerBelt))
		{
			item.Remove();
			arg.ReplyWith("Couldn't give item (inventory full?)");
			return;
		}
		basePlayer.Command("note.inv", item.info.itemid, num);
		Debug.Log(" [ServerVar] giving " + UI_Utils.StreamerModeSanitizeShort(basePlayer.displayName) + " " + item.amount + " x " + item.info.displayName.english);
		if (basePlayer.IsDeveloper)
		{
			basePlayer.ChatMessage("you silently gave yourself " + item.amount + " x " + item.info.displayName.english);
		}
		else
		{
			Chat.BroadcastPlayerAction(basePlayer, "gave themselves " + item.amount + " x " + item.info.displayName.english);
		}
	}

	[ServerVar(Help = "Set worn items to have maximum armor slots supported")]
	public static void setwornarmorslots(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		int @int = arg.GetInt(0, 4);
		foreach (Item item in basePlayer.inventory.containerWear.itemList)
		{
			ItemModContainerArmorSlot component = item.info.GetComponent<ItemModContainerArmorSlot>();
			if (component != null)
			{
				component.SetSlotAmount(item, Mathf.Min(@int, component.MaxSlots));
			}
		}
	}

	[ServerVar(Help = "(Generated) Checks whether the calling player has the specified item ID with the given skin ID in their inventory; used to validate pipette/skin matching server-side")]
	public static void pipetteid(Arg arg)
	{
		BasePlayer ply = ArgEx.Player(arg);
		int itemId = arg.GetInt(0);
		using PooledList<Item> pooledList = Facepunch.Pool.Get<PooledList<Item>>();
		ply.inventory.FindItemsByItemID(pooledList, itemId);
		ulong skinId = arg.GetULong(1, 1uL);
		bool flag = false;
		foreach (Item item in pooledList)
		{
			if (item.skin == skinId)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemId);
			ply.Command($"give {itemDefinition.shortname} 1 1 {skinId}");
		}
		InvokeHandler.Invoke(ply, delegate
		{
			ply.Command($"inventory.selectitem {itemId} {skinId}");
		}, 0.2f);
	}

	[ServerVar(Help = "Copies the players inventory to the player in front of them")]
	public static void copyTo(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic) || basePlayer == null)
		{
			return;
		}
		BasePlayer basePlayer2 = null;
		if (arg.HasArgs() && arg.GetString(0).ToLower() != "true")
		{
			basePlayer2 = ArgEx.GetPlayer(arg, 0);
			if (basePlayer2 == null)
			{
				uint uInt = arg.GetUInt(0);
				basePlayer2 = BasePlayer.FindByID(uInt);
				if (basePlayer2 == null)
				{
					basePlayer2 = BasePlayer.FindBot(uInt);
				}
			}
		}
		else
		{
			basePlayer2 = RelationshipManager.GetLookingAtPlayer(basePlayer);
		}
		if (!(basePlayer2 == null))
		{
			copyTo(basePlayer, basePlayer2);
		}
	}

	public static void copyTo(BasePlayer from, BasePlayer toply)
	{
		toply.inventory.containerBelt.Clear();
		toply.inventory.containerWear.Clear();
		int num = 0;
		foreach (Item item4 in from.inventory.containerBelt.itemList)
		{
			toply.inventory.containerBelt.AddItem(item4.info, item4.amount, item4.skin);
			if (item4.contents != null && !CopyArmorSlots(item4, toply.inventory.containerBelt.itemList[num], toply))
			{
				Item item = toply.inventory.containerBelt.itemList[num];
				foreach (Item item5 in item4.contents.itemList)
				{
					item.contents.AddItem(item5.info, item5.amount, item5.skin);
				}
			}
			num++;
		}
		foreach (Item item6 in from.inventory.containerWear.itemList)
		{
			toply.inventory.containerWear.AddItem(item6.info, item6.amount, item6.skin);
			if (item6.contents == null)
			{
				continue;
			}
			List<Item> itemList = toply.inventory.containerWear.itemList;
			Item item2 = itemList[itemList.Count - 1];
			if (item6.IsBackpack())
			{
				if (item2 == null)
				{
					continue;
				}
				foreach (Item item7 in item6.contents.itemList)
				{
					item2.contents.AddItem(item7.info, item7.amount, item7.skin);
					if (item7.contents == null)
					{
						continue;
					}
					List<Item> itemList2 = item2.contents.itemList;
					Item item3 = itemList2[itemList2.Count - 1];
					if (CopyArmorSlots(item7, item3, toply))
					{
						continue;
					}
					foreach (Item item8 in item7.contents.itemList)
					{
						item3.contents.AddItem(item8.info, item8.amount, item8.skin);
					}
				}
			}
			else
			{
				CopyArmorSlots(item6, item2, toply);
			}
		}
		if (from.IsDeveloper)
		{
			from.ChatMessage("you silently copied items to " + NameHelper.GetPlayerNameStreamSafe(from, toply));
		}
		else
		{
			Chat.BroadcastPlayerAction(from, " copied their inventory to ", toply, "");
		}
	}

	private static bool CopyArmorSlots(Item sourceItem, Item destItem, BasePlayer player)
	{
		if (sourceItem == null)
		{
			return false;
		}
		if (sourceItem.contents == null)
		{
			return false;
		}
		if (destItem == null)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (!sourceItem.info.TryGetComponent<ItemModContainerArmorSlot>(out var _))
		{
			return false;
		}
		int capacity = sourceItem.contents.capacity;
		if (capacity == 0)
		{
			return false;
		}
		destItem.info.GetComponent<ItemModContainerArmorSlot>().CreateAtCapacity(capacity, destItem);
		foreach (Item item in sourceItem.contents.itemList)
		{
			destItem.contents.AddItem(item.info, item.amount, item.skin);
		}
		return true;
	}

	[ServerVar(Help = "Deploys a loadout to players in a radius eg. inventory.deployLoadoutInRange testloadout 30")]
	public static void deployLoadoutInRange(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic) || basePlayer == null)
		{
			return;
		}
		string @string = arg.GetString(0);
		if (!LoadLoadout(@string, out var so))
		{
			arg.ReplyWith("Can't find loadout: " + @string);
			return;
		}
		float @float = arg.GetFloat(1);
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		global::Vis.Entities(basePlayer.transform.position, @float, obj, 131072);
		int num = 0;
		foreach (BasePlayer item in obj)
		{
			if (!(item == basePlayer) && !item.isClient)
			{
				so.LoadItemsOnTo(item);
				num++;
			}
		}
		arg.ReplyWith($"Applied loadout {@string} to {num} players");
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "Deploys the given loadout to a target player. eg. inventory.deployLoadout testloadout jim")]
	public static void deployLoadout(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic))
		{
			string @string = arg.GetString(0);
			BasePlayer basePlayer2 = (string.IsNullOrEmpty(arg.GetString(1)) ? null : ArgEx.GetPlayerOrSleeperOrBot(arg, 1));
			if (basePlayer2 == null)
			{
				basePlayer2 = basePlayer;
			}
			SavedLoadout so;
			if (basePlayer2 == null)
			{
				arg.ReplyWith("Could not find player " + arg.GetString(1) + " and no local player available");
			}
			else if (LoadLoadout(@string, out so))
			{
				so.LoadItemsOnTo(basePlayer2);
				arg.ReplyWith("Deployed loadout " + @string + " to " + basePlayer2.displayName);
			}
			else
			{
				arg.ReplyWith("Could not find loadout " + @string);
			}
		}
	}

	[ServerVar(Help = "Clears the inventory of a target player. eg. inventory.clearInventory jim. Can take container names as arguments: --belt --wear --backpack")]
	public static void clearInventory(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic))
		{
			return;
		}
		arg.TryRemoveKeyBindEventArgs();
		BasePlayer basePlayer2 = basePlayer;
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		if (arg.Args == null || arg.Args.Length == 0)
		{
			flag = true;
		}
		else
		{
			int num = 0;
			basePlayer2 = ArgEx.GetPlayerOrSleeperOrBot(arg, 0);
			if (basePlayer2 == null)
			{
				switch (arg.GetString(0).ToLower())
				{
				case "--main":
				case "--belt":
				case "--wear":
				case "--backpack":
					break;
				default:
					arg.ReplyWith("Could not find player '" + arg.GetString(0) + "'");
					return;
				}
				basePlayer2 = basePlayer;
				num = 0;
			}
			else
			{
				num = 1;
			}
			if (num == 1 && arg.Args.Length == 1)
			{
				flag = true;
			}
			else
			{
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				for (int i = num; i < arg.Args.Length; i++)
				{
					switch (arg.GetString(i).ToLower())
					{
					case "--main":
						flag2 = true;
						break;
					case "--belt":
						flag3 = true;
						break;
					case "--wear":
						flag4 = true;
						break;
					case "--backpack":
						flag5 = true;
						break;
					}
				}
				if (flag2)
				{
					basePlayer2.inventory.containerMain.Clear();
					stringBuilder.AppendLine("Cleared " + basePlayer2.displayName + "'s main inventory");
				}
				if (flag3)
				{
					basePlayer2.inventory.containerBelt.Clear();
					stringBuilder.AppendLine("Cleared " + basePlayer2.displayName + "'s belt");
				}
				if (flag4)
				{
					basePlayer2.inventory.containerWear.Clear();
					stringBuilder.AppendLine("Cleared " + basePlayer2.displayName + "'s clothings");
				}
				if (flag5 && basePlayer2.inventory.GetContainer(PlayerInventory.Type.BackpackContents) != null)
				{
					basePlayer2.inventory.GetContainer(PlayerInventory.Type.BackpackContents).Clear();
					stringBuilder.AppendLine("Cleared " + basePlayer2.displayName + "'s backpack");
				}
			}
		}
		if (flag)
		{
			basePlayer2.inventory.containerMain.Clear();
			basePlayer2.inventory.containerBelt.Clear();
			basePlayer2.inventory.containerWear.Clear();
			basePlayer2.inventory.GetContainer(PlayerInventory.Type.BackpackContents)?.Clear();
			stringBuilder.AppendLine(basePlayer2.displayName + "'s whole inventory cleared");
		}
		basePlayer2.SV_ClothingChanged();
		arg.ReplyWith(stringBuilder.ToString());
		ItemManager.DoRemoves();
	}

	private static string GetLoadoutPath(string loadoutName)
	{
		return Server.GetServerFolder("loadouts") + "/" + loadoutName + ".ldt";
	}

	private static void BroadcastLoadoutListDirty()
	{
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && activePlayer.net?.connection != null && (activePlayer.IsAdmin || activePlayer.IsDeveloper))
			{
				ConsoleNetwork.SendClientCommand(activePlayer.net.connection, "inventory.LoadoutUI_MarkDirty");
			}
		}
	}

	[ServerVar(Help = "Saves the current equipped loadout of the calling player. eg. inventory.saveLoadout loaduoutname")]
	public static void saveloadout(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && (basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic))
		{
			string @string = arg.GetString(0);
			string contents = JsonConvert.SerializeObject(new SavedLoadout(basePlayer), Formatting.Indented);
			string loadoutPath = GetLoadoutPath(@string);
			File.WriteAllText(loadoutPath, contents);
			arg.ReplyWith("Saved loadout to " + loadoutPath);
			BroadcastLoadoutListDirty();
		}
	}

	public static bool LoadLoadout(string name, out SavedLoadout so)
	{
		PlayerInventoryProperties inventoryConfig = PlayerInventoryProperties.GetInventoryConfig(name);
		if (inventoryConfig != null)
		{
			Debug.Log("Found builtin config!");
			so = new SavedLoadout(inventoryConfig);
			return true;
		}
		so = new SavedLoadout();
		string loadoutPath = GetLoadoutPath(name);
		if (!File.Exists(loadoutPath))
		{
			return false;
		}
		so = JsonConvert.DeserializeObject<SavedLoadout>(File.ReadAllText(loadoutPath));
		if (so == null)
		{
			return false;
		}
		return true;
	}

	[ServerVar(Help = "Prints all saved inventory loadouts")]
	public static void listloadouts(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic))
		{
			return;
		}
		string serverFolder = Server.GetServerFolder("loadouts");
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in Directory.EnumerateFiles(serverFolder))
		{
			stringBuilder.AppendLine(item);
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Deletes a named admin loadout file from the server; requires admin or developer permissions; loadout name is passed as the first argument")]
	public static void deleteLoadout(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic))
		{
			return;
		}
		string @string = arg.GetString(0);
		if (!string.IsNullOrEmpty(@string))
		{
			string loadoutPath = GetLoadoutPath(@string);
			if (File.Exists(loadoutPath))
			{
				File.Delete(loadoutPath);
				arg.ReplyWith("Deleted loadout " + @string);
				BroadcastLoadoutListDirty();
			}
			else
			{
				arg.ReplyWith("Could not find loadout " + @string);
			}
		}
	}

	[ServerVar(Help = "(Generated) Reads the list of saved admin loadouts from disk and sends it to the requesting player's client as a JSON array for display in the loadout UI")]
	public static void LoadoutUI_RequestLoadoutList(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || (!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic))
		{
			return;
		}
		string serverFolder = Server.GetServerFolder("loadouts");
		if (!Directory.Exists(serverFolder))
		{
			ConsoleNetwork.SendClientCommand(arg.Connection, "inventory.LoadoutUI_ReceiveLoadoutList", "[]");
			return;
		}
		FileInfo[] files = new DirectoryInfo(serverFolder).GetFiles("*.ldt");
		List<LoadoutFileInfo> obj = Facepunch.Pool.Get<List<LoadoutFileInfo>>();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			fileInfo.Refresh();
			if (fileInfo.Exists)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
				LoadoutFileInfo loadoutFileInfo = new LoadoutFileInfo
				{
					fileName = fileInfo.Name,
					displayName = fileNameWithoutExtension,
					createdTimestamp = new DateTimeOffset(fileInfo.LastWriteTime).ToUnixTimeSeconds()
				};
				if (LoadLoadout(fileNameWithoutExtension, out var so))
				{
					loadoutFileInfo.belt = so.belt;
					loadoutFileInfo.wear = so.wear;
					loadoutFileInfo.itemCount = CountItems(so.belt) + CountItems(so.wear) + CountItems(so.main) + CountItems(so.backpack);
				}
				obj.Add(loadoutFileInfo);
			}
		}
		string text = JsonConvert.SerializeObject(obj);
		ConsoleNetwork.SendClientCommand(arg.Connection, "inventory.LoadoutUI_ReceiveLoadoutList", text);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	private static int CountItems(SavedLoadout.SavedItem[] items)
	{
		if (items == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			SavedLoadout.SavedItem savedItem = items[i];
			num += ((savedItem.amount <= 1) ? 1 : savedItem.amount);
		}
		return num;
	}

	[ServerVar(Help = "(Generated) Prints the names of all Steam inventory item definitions currently loaded from the Steam backend; useful for verifying skin/item definition state")]
	[ClientVar(Help = "(Generated) Prints the names of all Steam inventory item definitions currently loaded from the Steam backend; useful for verifying skin/item definition state")]
	public static void defs(Arg arg)
	{
		if (Steamworks.SteamInventory.Definitions == null)
		{
			arg.ReplyWith("no definitions");
			return;
		}
		if (Steamworks.SteamInventory.Definitions.Length == 0)
		{
			arg.ReplyWith("0 definitions");
			return;
		}
		string[] obj = Steamworks.SteamInventory.Definitions.Select((InventoryDef x) => x.Name).ToArray();
		arg.ReplyWith(obj);
	}

	[ClientVar(Help = "(Generated) Forces a reload of all Steam inventory item definitions from the Steam backend; use if definitions appear stale or missing after a store update")]
	[ServerVar(Help = "(Generated) Forces a reload of all Steam inventory item definitions from the Steam backend; use if definitions appear stale or missing after a store update")]
	public static void reloaddefs(Arg arg)
	{
		Steamworks.SteamInventory.LoadItemDefinitions();
	}

	[ServerVar(Help = "(Generated) Equips the belt/hotbar slot at the given index for the player the admin is currently looking at; requires admin or developer permissions")]
	public static void equipslottarget(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((basePlayer.IsAdmin || basePlayer.IsDeveloper || Server.cinematic) && !(basePlayer == null))
		{
			BasePlayer lookingAtPlayer = RelationshipManager.GetLookingAtPlayer(basePlayer);
			if (!(lookingAtPlayer == null))
			{
				int @int = arg.GetInt(0);
				EquipItemInSlot(lookingAtPlayer, @int);
				arg.ReplyWith($"Equipped slot {@int} on player {lookingAtPlayer.displayName}");
			}
		}
	}

	[ServerVar(Help = "(Generated) Equips the belt/hotbar item at the given slot index for the calling admin or a named target player; requires admin or developer permissions")]
	public static void equipslot(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((!basePlayer.IsAdmin && !basePlayer.IsDeveloper && !Server.cinematic) || basePlayer == null)
		{
			return;
		}
		BasePlayer basePlayer2 = null;
		if (arg.HasArgs(2))
		{
			basePlayer2 = ArgEx.GetPlayer(arg, 1);
			if (basePlayer2 == null)
			{
				uint uInt = arg.GetUInt(1);
				basePlayer2 = BasePlayer.FindByID(uInt);
				if (basePlayer2 == null)
				{
					basePlayer2 = BasePlayer.FindBot(uInt);
				}
			}
		}
		if (!(basePlayer2 == null))
		{
			int @int = arg.GetInt(0);
			EquipItemInSlot(basePlayer2, @int);
			Debug.Log($"Equipped slot {@int} on player {basePlayer2.displayName}");
		}
	}

	public static void EquipItemInSlot(BasePlayer player, int slot)
	{
		ItemId itemID = default(ItemId);
		for (int i = 0; i < player.inventory.containerBelt.itemList.Count; i++)
		{
			if (player.inventory.containerBelt.itemList[i] != null && i == slot)
			{
				itemID = player.inventory.containerBelt.itemList[i].uid;
				break;
			}
		}
		player.UpdateActiveItem(itemID);
	}

	private static int GetSlotIndex(BasePlayer player)
	{
		if (player.GetActiveItem() == null)
		{
			return -1;
		}
		ItemId uid = player.GetActiveItem().uid;
		for (int i = 0; i < player.inventory.containerBelt.itemList.Count; i++)
		{
			if (player.inventory.containerBelt.itemList[i] != null && player.inventory.containerBelt.itemList[i].uid == uid)
			{
				return i;
			}
		}
		return -1;
	}

	[ServerVar(Help = "(Generated) Gives the calling player a blueprint item for the specified item (by partial name); broadcasts the gift in chat unless the recipient is a developer")]
	public static void giveBp(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!basePlayer)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindDefinitionByPartialName(arg.GetString(0));
		if (itemDefinition == null)
		{
			arg.ReplyWith("Could not find item: " + arg.GetString(0));
			return;
		}
		if (itemDefinition.Blueprint == null)
		{
			arg.ReplyWith(itemDefinition.shortname + " has no blueprint!");
			return;
		}
		Item item = ItemManager.Create(ItemManager.blueprintBaseDef, 1, 0uL, isServerSide: true, 0uL);
		item.blueprintTarget = itemDefinition.itemid;
		item.OnVirginSpawn();
		if (!basePlayer.inventory.GiveItem(item))
		{
			item.Remove();
			arg.ReplyWith("Couldn't give item (inventory full?)");
			return;
		}
		basePlayer.Command("note.inv", item.info.itemid, 1);
		Debug.Log("giving " + basePlayer.displayName + " 1 x " + item.blueprintTargetDef.shortname + " blueprint");
		if (basePlayer.IsDeveloper)
		{
			basePlayer.ChatMessage("you silently gave yourself 1 x " + item.blueprintTargetDef.shortname + " blueprint");
		}
		else
		{
			Chat.BroadcastPlayerAction(basePlayer, "gave themselves 1 x " + item.blueprintTargetDef.shortname + " blueprint");
		}
	}

	[ServerVar(Help = "(Generated) Sets the custom icon image ID on the item currently held by the calling player; the image ID refers to a server-side stored image used for custom item icons")]
	public static void set_item_image(Arg arg)
	{
		Item activeItem = ArgEx.Player(arg).GetActiveItem();
		if (activeItem == null)
		{
			arg.ReplyWith("No active item");
			return;
		}
		if (!arg.HasArgs())
		{
			arg.ReplyWith("No image id provided");
			return;
		}
		uint num = (activeItem.iconImageId = arg.GetUInt(0));
		activeItem.MarkDirty();
		arg.ReplyWith($"Set image id to {num}");
	}

	[ServerVar(Help = "Add ownership to item")]
	public static void addownership(Arg args)
	{
		Item activeItem = ArgEx.Player(args).GetActiveItem();
		if (activeItem == null)
		{
			args.ReplyWith("You must be holding an item to use this command");
			return;
		}
		if (!args.HasArgs(3))
		{
			args.ReplyWith("Usage: addownership {username} {reason} {amount}");
			return;
		}
		string @string = args.GetString(0);
		string string2 = args.GetString(1);
		int @int = args.GetInt(2, 1);
		activeItem.AddItemOwnership(@string, string2, @int);
		args.ReplyWith($"Added '{@int}' ownership to item");
	}

	[ServerVar(Help = "Reduce ownership to item to allow new ownership to be added")]
	public static void reduceownership(Arg args)
	{
		Item activeItem = ArgEx.Player(args).GetActiveItem();
		if (activeItem == null)
		{
			args.ReplyWith("You must be holding an item to use this command");
			return;
		}
		if (!args.HasArgs())
		{
			args.ReplyWith("Usage: reduceownership {amount}");
			return;
		}
		int @int = args.GetInt(0, 1);
		activeItem.ReduceItemOwnership(@int);
		args.ReplyWith($"Reduced ownership of item by '{@int}'");
	}

	[ServerVar(Help = "Reduce ownership to item to allow new ownership to be added")]
	public static void convertownership(Arg args)
	{
		Item activeItem = ArgEx.Player(args).GetActiveItem();
		if (activeItem == null)
		{
			args.ReplyWith("You must be holding an item to use this command");
			return;
		}
		if (!args.HasArgs(3))
		{
			args.ReplyWith("Usage: convertownership {username} {reason} {amount}");
			return;
		}
		string @string = args.GetString(0);
		string string2 = args.GetString(1);
		int @int = args.GetInt(2, 1);
		activeItem.ReduceItemOwnership(@int);
		activeItem.AddItemOwnership(@string, string2, @int);
		args.ReplyWith($"Converted '{@int}' ownership of item to '{@string}'");
	}
}

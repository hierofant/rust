using System.Linq;
using System.Text;
using UnityEngine;

namespace ConVar;

[Factory("apartment")]
public class ApartmentCommands : ConsoleSystem
{
	[ReplicatedVar(Name = "breakinseconds", Help = "How long a player needs to hold the break in interaction on an apartment door with a master key")]
	public static float breakinseconds = 30f;

	[ReplicatedVar(Name = "apartmentevictiondelay", Help = "How long should we wait before evicting a player from their apartment if they don't pay rent?")]
	public static float apartmentevictiondelay = 86400f;

	[ReplicatedVar(Help = "Should an invisible blocker prevent guests from entering apartments that they don't have access to?")]
	public static bool apartmentinvisibleblocker = true;

	[ReplicatedVar(Help = "Should combat be allowed inside apartment rooms outside of the break-in period?")]
	public static bool allowcombatoutsideofbreakin = true;

	[ReplicatedVar(Help = "How much scrap the apartment security NPC charges for a master key")]
	public static int masterkeyprice = 1000;

	[ReplicatedVar(Name = "adminapartmentbypass", Help = "Should admins be able to bypass apartment authorization checks?")]
	public static bool adminapartmentbypass = false;

	[ServerVar(Help = "How many hours of scrap upkeep does the apartments spawn with (so players don't see 'Eviction' vital right after renting an apartment")]
	public static float apartmentfreerenthours = 4f;

	[ServerVar(Name = "intruderauthseconds", Help = "How long a player stays authorized on an apartment room after breaking in with a master key")]
	public static float intruderauthseconds = 300f;

	[ServerVar(Name = "rentscaling", Help = "Should the rent scale based on the items you have stored inside your apartment?")]
	public static float rentscaling = 0f;

	[ServerVar(Name = "npcsecuritydooropentime", Help = "How long should the apartment security NPC keep the door open for after being paid?")]
	public static float apartmentsecurityaccesstime = 300f;

	[ServerVar(Name = "adminapartmentnoclip", Help = "Should admins be able to noclip in apartments?")]
	public static bool adminapartmentnoclip = true;

	[ServerVar(Name = "printitemtax", Help = "Print out a list of all items that apartments will tax")]
	public static void PrintItemTax(Arg arg)
	{
		TextTable textTable = new TextTable();
		textTable.AddColumns("Item", "Tax Per Stack", "Stacksize");
		foreach (ItemDefinition item in from x in ItemManager.itemList
			where x.ApartmentTaxPerStack > 0f
			orderby x.ApartmentTaxPerStack descending
			select x)
		{
			textTable.AddRow(item.shortname, item.ApartmentTaxPerStack.ToString("0.##"), item.stackable.ToString());
		}
		arg.ReplyWith(textTable.ToString());
	}

	private static ApartmentBuilding GetApartmentBuilding()
	{
		return BaseNetworkable.serverEntities.OfType<ApartmentBuilding>().FirstOrDefault();
	}

	[ServerVar(Name = "rentroom")]
	public static void RentApartment(Arg arg)
	{
		string @string = arg.GetString(0);
		BasePlayer player = ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(@string);
		if (apartmentRoom == null)
		{
			arg.ReplyWith("No room found with number '" + @string + "'");
		}
		else if (apartmentBuilding.GetPlayerApartment(player) != null)
		{
			arg.ReplyWith("You already have an apartment!");
		}
		else
		{
			apartmentBuilding.GiveRoomToPlayer(player, apartmentRoom);
		}
	}

	[ServerVar(Name = "fakerentroom")]
	public static void fakerentroom(Arg arg)
	{
		string @string = arg.GetString(0);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(@string);
		if (apartmentRoom == null)
		{
			arg.ReplyWith("No room found with number '" + @string + "'");
			return;
		}
		if (apartmentRoom.IsCurrentlyRented())
		{
			arg.ReplyWith("Room '" + apartmentRoom.RoomNumber + "' is already rented");
			return;
		}
		BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", apartmentRoom.TeleportAnchor.transform.position, Quaternion.identity) as BasePlayer;
		basePlayer.Spawn();
		apartmentBuilding.GiveRoomToPlayer(basePlayer, apartmentRoom);
		arg.ReplyWith("Rented room '" + apartmentRoom.RoomNumber + "' to fake player");
	}

	[ServerVar(Name = "rentallrooms")]
	public static void RentAllRooms(Arg arg)
	{
		ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (!room.IsCurrentlyRented())
			{
				BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", room.TeleportAnchor.transform.position, Quaternion.identity) as BasePlayer;
				basePlayer.Spawn();
				apartmentBuilding.GiveRoomToPlayer(basePlayer, room);
			}
		}
		arg.ReplyWith("Rented every single room out");
	}

	[ServerVar(Name = "rentallroomsoftype")]
	public static void RentAllRoomsOfType(Arg arg)
	{
		ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		int @int = arg.GetInt(0);
		if (@int < 1 || @int > 3)
		{
			arg.ReplyWith($"Failed to get a room type from arg {@int}");
			return;
		}
		ApartmentSize apartmentSize = (ApartmentSize)@int;
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (!room.IsCurrentlyRented() && room.Size == apartmentSize)
			{
				BasePlayer basePlayer = GameManager.server.CreateEntity("assets/prefabs/player/player.prefab", room.TeleportAnchor.transform.position, Quaternion.identity) as BasePlayer;
				basePlayer.Spawn();
				apartmentBuilding.GiveRoomToPlayer(basePlayer, room);
			}
		}
		arg.ReplyWith($"Rented every single room of type {apartmentSize} out");
	}

	[ServerVar(Name = "checkoutroom")]
	public static void CheckoutRoom(Arg arg)
	{
		string @string = arg.GetString(0);
		BasePlayer player = ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		if (!string.IsNullOrEmpty(@string))
		{
			ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(@string);
			if (apartmentRoom == null)
			{
				arg.ReplyWith("No room found with number '" + @string + "'");
				return;
			}
			apartmentBuilding.Checkout(apartmentRoom);
			arg.ReplyWith("You checked out room '" + apartmentRoom.RoomNumber + "'");
			return;
		}
		ApartmentRoom playerApartment = apartmentBuilding.GetPlayerApartment(player);
		if (playerApartment == null)
		{
			arg.ReplyWith("You don't have an apartment room to checkout from!");
		}
		else if (apartmentBuilding.TryCheckout(player))
		{
			arg.ReplyWith("You have checked out of room '" + playerApartment.RoomNumber + "'");
		}
		else
		{
			arg.ReplyWith($"Failed to checkout of room '{playerApartment}'");
		}
	}

	[ServerVar(Help = "Checkout every room in the apartment complex")]
	public static void checkoutallrooms(Arg arg)
	{
		ArgEx.Player(arg);
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		foreach (ApartmentRoom room in apartmentBuilding.Rooms)
		{
			if (room.IsCurrentlyRented())
			{
				apartmentBuilding.Checkout(room);
			}
		}
		arg.ReplyWith("Checked out every room in the apartment complex");
	}

	[ServerVar(Help = "Test triggering the apartment security door")]
	public static void testapartmentsecuritydoor(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		NPCApartmentSecurity.OnPaidToll(basePlayer, basePlayer.transform.position, doPayment: false);
	}

	[ServerVar(Help = "Test triggering the scheduled death in safezones")]
	public static void scheduleddeath(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be ran from client");
		}
		else
		{
			basePlayer.ScheduledDeath();
		}
	}

	[ServerVar(Help = "Print list of furniture inside your room")]
	public static void printapartmentfurniture(Arg arg)
	{
		ApartmentBuilding apartmentBuilding = GetApartmentBuilding();
		if (apartmentBuilding == null)
		{
			arg.ReplyWith("No apartment building found");
			return;
		}
		string text = arg.GetString(0);
		BasePlayer player = ArgEx.Player(arg);
		if (string.IsNullOrEmpty(text))
		{
			ApartmentRoom playerApartment = apartmentBuilding.GetPlayerApartment(player);
			if (playerApartment == null)
			{
				arg.ReplyWith("Either provide a room numer or own a room!");
				return;
			}
			text = playerApartment.RoomNumber;
		}
		ApartmentRoom apartmentRoom = apartmentBuilding.FindByRoomNumber(text);
		if (apartmentRoom == null)
		{
			arg.ReplyWith("No room found with number '" + text + "'");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Room: " + text);
		TextTable textTable = new TextTable();
		textTable.AddColumns("Prefab", "ID", "Storage");
		foreach (BaseEntity item in apartmentRoom.Furniture.OrderBy((BaseEntity x) => x.ShortPrefabName))
		{
			string text2 = "";
			if (item is IItemContainerEntity itemContainerEntity)
			{
				text2 = $" {itemContainerEntity.inventory.itemList.Count}/{itemContainerEntity.inventory.capacity}";
			}
			textTable.AddRow(item.ShortPrefabName, item.net.ID.ToString(), text2);
		}
		stringBuilder.AppendLine(textTable.ToString());
		arg.ReplyWith(stringBuilder.ToString());
	}
}

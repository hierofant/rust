using UnityEngine;

namespace ConVar;

[Factory("craft")]
public class Craft : ConsoleSystem
{
	[ServerVar(Help = "(Generated) When enabled, all crafting completes instantly with no time delay; useful for testing crafting recipes or quickly equipping items in development")]
	public static bool instant;

	[ServerUserVar]
	public static void add(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer || basePlayer.IsDead())
		{
			return;
		}
		int @int = args.GetInt(0);
		int int2 = args.GetInt(1, 1);
		int num = (int)args.GetUInt64(2, 0uL);
		bool @bool = args.GetBool(3);
		int num2 = (int)args.GetUInt64(4, 0uL);
		if (int2 < 1)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(@int);
		if (itemDefinition == null)
		{
			args.ReplyWith("Item not found");
			return;
		}
		ItemBlueprint itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
		if (!itemBlueprint)
		{
			args.ReplyWith("Blueprint not found");
			return;
		}
		if (!itemBlueprint.userCraftable)
		{
			args.ReplyWith("Item is not craftable");
			return;
		}
		if (!basePlayer.blueprints.CanCraft(@int, num, basePlayer))
		{
			num = 0;
			if (0 == 0 && !basePlayer.blueprints.CanCraft(@int, num, basePlayer))
			{
				args.ReplyWith("You can't craft this item");
				return;
			}
			args.ReplyWith("You don't have permission to use this skin, so crafting unskinned");
		}
		bool flag = ItemSkinDirectory.FindByInventoryDefinitionId(num2).invItem is AccessoryItem;
		if (num2 != 0 && (!flag || !basePlayer.blueprints.CheckSkinOwnership(num2, basePlayer)))
		{
			args.ReplyWith("You don't have permission to use that attachment, removing...");
			num2 = 0;
		}
		int num3 = int2;
		int num4 = int2;
		if (@bool)
		{
			num3 = Mathf.Min(int2, 5);
			num4 = 1;
		}
		for (int num5 = num3; num5 >= num4; num5--)
		{
			if (basePlayer.inventory.crafting.CraftItem(itemBlueprint, basePlayer, null, num5, num, null, free: false, num2))
			{
				return;
			}
		}
		args.ReplyWith("Couldn't craft!");
	}

	[ServerUserVar]
	public static void canceltask(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsDead())
		{
			int @int = args.GetInt(0);
			if (!basePlayer.inventory.crafting.CancelTask(@int))
			{
				args.ReplyWith("Couldn't cancel task!");
			}
		}
	}

	[ServerUserVar]
	public static void cancel(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsDead())
		{
			int @int = args.GetInt(0);
			basePlayer.inventory.crafting.CancelBlueprint(@int);
		}
	}

	[ServerUserVar]
	public static void fasttracktask(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsDead())
		{
			int @int = args.GetInt(0);
			if (!basePlayer.inventory.crafting.FastTrackTask(@int))
			{
				args.ReplyWith("Couldn't fast track task!");
			}
		}
	}
}

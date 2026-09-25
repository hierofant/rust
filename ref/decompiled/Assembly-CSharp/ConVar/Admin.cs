using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Math;
using Network;
using Newtonsoft.Json;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Scripting;

namespace ConVar;

[Factory("global")]
public class Admin : ConsoleSystem
{
	private enum ChangeGradeMode
	{
		Upgrade,
		Downgrade
	}

	[JsonModel]
	[Preserve]
	public struct PlayerInfo
	{
		public string SteamID;

		public string OwnerSteamID;

		public string DisplayName;

		public int Ping;

		public string Address;

		public ulong EntityId;

		public int ConnectedSeconds;

		public float ViolationLevel;

		public float CurrentLevel;

		public float Health;

		public Vector3 Position;

		public bool IsMuted;

		public ulong TeamID;
	}

	[JsonModel]
	[Preserve]
	public struct PlayerIDInfo
	{
		public string SteamID;

		public string OwnerSteamID;

		public string DisplayName;

		public string Address;

		public ulong EntityId;
	}

	[JsonModel]
	[Preserve]
	public struct ServerInfoOutput
	{
		public string Hostname;

		public int MaxPlayers;

		public int Players;

		public int Queued;

		public int Joining;

		public int ReservedSlots;

		public int EntityCount;

		public string GameTime;

		public int Uptime;

		public string Map;

		public float Framerate;

		public int Memory;

		public int MemoryUsageSystem;

		public int Collections;

		public int NetworkIn;

		public int NetworkOut;

		public bool Restarting;

		public string SaveCreatedTime;

		public int Version;

		public string Protocol;
	}

	[Preserve]
	[JsonModel]
	public struct ServerConvarInfo
	{
		public string FullName;

		public string Value;

		public string Help;
	}

	[Preserve]
	[JsonModel]
	public struct ServerUGCInfo
	{
		public ulong entityId;

		public uint[] crcs;

		public UGCType contentType;

		public uint entityPrefabID;

		public string shortPrefabName;

		public ulong[] playerIds;

		public string contentString;

		public ServerUGCInfo(IUGCBrowserEntity fromEntity)
		{
			entityId = fromEntity.UgcEntity.net.ID.Value;
			crcs = fromEntity.GetContentCRCs;
			contentType = fromEntity.ContentType;
			entityPrefabID = fromEntity.UgcEntity.prefabID;
			shortPrefabName = fromEntity.UgcEntity.ShortPrefabName;
			playerIds = fromEntity.EditingHistory.ToArray();
			contentString = fromEntity.ContentString;
		}
	}

	private struct EntityAssociation
	{
		public BaseEntity TargetEntity;

		public EntityAssociationType AssociationType;
	}

	private enum EntityAssociationType
	{
		Owner,
		Auth,
		LockGuest
	}

	[ReplicatedVar(Help = "Controls whether the in-game admin UI is displayed to admins")]
	public static bool allowAdminUI = true;

	[ServerVar(Help = "Include bots in the admin UI player list (debugging purpose only)")]
	public static bool showBotsInPlayerList = false;

	[ServerVar(Help = "Print out currently connected clients")]
	public static void status(Arg arg)
	{
		string @string = arg.GetString(0);
		if (@string == "--json")
		{
			@string = arg.GetString(1);
		}
		bool flag = arg.HasArg("--json");
		string text = string.Empty;
		if (!flag && @string.Length == 0)
		{
			text = text + "hostname: " + Server.hostname + "\n";
			text = text + "version : " + 2633 + " secure (secure mode enabled, connected to Steam3)\n";
			text = text + "map     : " + Server.level + "\n";
			text += $"players : {BasePlayer.activePlayerList.Count()} ({Server.maxplayers} max) ({SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued} queued) ({SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining} joining)\n\n";
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.AddColumn("id");
		textTable.AddColumn("name");
		textTable.AddColumn("ping");
		textTable.AddColumn("connected");
		textTable.AddColumn("addr");
		textTable.AddColumn("owner");
		textTable.AddColumn("violation");
		textTable.AddColumn("kicks");
		textTable.AddColumn("entityId");
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			try
			{
				if (!activePlayer.IsValid())
				{
					continue;
				}
				string userIDString = activePlayer.UserIDString;
				if (activePlayer.net.connection == null)
				{
					textTable.AddRow(userIDString, "NO CONNECTION");
					continue;
				}
				string text2 = activePlayer.net.connection.ownerid.ToString();
				string text3 = activePlayer.displayName.QuoteSafe();
				string text4 = Network.Net.sv.GetAveragePing(activePlayer.net.connection).ToString();
				string text5 = activePlayer.net.connection.ipaddress;
				string text6 = activePlayer.net.ID.Value.ToString();
				string text7 = activePlayer.ViolationLevel.ToString("0.0");
				string text8 = activePlayer.GetAntiHackKicks().ToString();
				if (!arg.IsAdmin && !arg.IsRcon)
				{
					text5 = "xx.xxx.xx.xxx";
				}
				string text9 = activePlayer.net.connection.GetSecondsConnected() + "s";
				if (@string.Length <= 0 || text3.Contains(@string, CompareOptions.IgnoreCase) || userIDString.Contains(@string) || text2.Contains(@string) || text5.Contains(@string))
				{
					textTable.AddRow(userIDString, text3, text4, text9, text5, (text2 == userIDString) ? string.Empty : text2, text7, text8, text6);
				}
			}
			catch (Exception ex)
			{
				textTable.AddRow(activePlayer.UserIDString, ex.Message.QuoteSafe());
			}
		}
		if (flag)
		{
			arg.ReplyWith(textTable.ToJson());
		}
		else
		{
			arg.ReplyWith(text + textTable.ToString());
		}
	}

	[ServerVar(Help = "Print out stats of currently connected clients")]
	public static void stats(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		using (TextTable textTable = Facepunch.Pool.Get<TextTable>())
		{
			textTable.ShouldPadColumns = !flag;
			textTable.AddColumn("id");
			textTable.AddColumn("name");
			textTable.AddColumn("time");
			textTable.AddColumn("kills");
			textTable.AddColumn("deaths");
			textTable.AddColumn("suicides");
			textTable.AddColumn("player");
			textTable.AddColumn("building");
			textTable.AddColumn("entity");
			ulong uInt = arg.GetUInt64(0, 0uL);
			if (uInt == 0L)
			{
				string @string = arg.GetString(0);
				foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
				{
					try
					{
						if (activePlayer.IsValid())
						{
							string text = activePlayer.displayName.QuoteSafe();
							if (@string.Length <= 0 || text.Contains(@string, CompareOptions.IgnoreCase))
							{
								addRow(activePlayer.userID, text, textTable);
							}
						}
					}
					catch (Exception ex)
					{
						textTable.AddRow(activePlayer.UserIDString, ex.Message.QuoteSafe());
					}
				}
			}
			else
			{
				string name2 = "N/A";
				BasePlayer basePlayer = BasePlayer.FindByID(uInt);
				if ((bool)basePlayer)
				{
					name2 = basePlayer.displayName.QuoteSafe();
				}
				addRow(uInt, name2, textTable);
			}
			arg.ReplyWith(flag ? textTable.ToJson() : textTable.ToString());
		}
		static void addRow(ulong id, string name, TextTable table)
		{
			ServerStatistics.Storage storage = ServerStatistics.Get(id);
			string text2 = TimeSpanEx.ToShortString(TimeSpan.FromSeconds(storage.Get("time")));
			string text3 = storage.Get("kill_player").ToString();
			string text4 = (storage.Get("deaths") - storage.Get("death_suicide")).ToString();
			string text5 = storage.Get("death_suicide").ToString();
			string text6 = storage.Get("hit_player_direct_los").ToString();
			string text7 = storage.Get("hit_player_indirect_los").ToString();
			string text8 = storage.Get("hit_building_direct_los").ToString();
			string text9 = storage.Get("hit_building_indirect_los").ToString();
			string text10 = storage.Get("hit_entity_direct_los").ToString();
			string text11 = storage.Get("hit_entity_indirect_los").ToString();
			table.AddRow(id.ToString(), name, text2, text3, text4, text5, text6 + " / " + text7, text8 + " / " + text9, text10 + " / " + text11);
		}
	}

	[ServerVar(Help = "fillinventory <optional: category> - Fills your inventory with random items, can also specify a category (ammunition, weapon etc.)")]
	public static void fillInventory(Arg arg, string category)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		bool flag = !string.IsNullOrEmpty(category);
		ItemCategory result = ItemCategory.Weapon;
		if (flag && (category.IsNumeric() || !Enum.TryParse<ItemCategory>(category, ignoreCase: true, out result)))
		{
			arg.ReplyWith("'" + category + "' is not a valid item category!");
			return;
		}
		FillContainerInternal(basePlayer.inventory.containerBelt, flag, result, basePlayer);
		FillContainerInternal(basePlayer.inventory.containerMain, flag, result, basePlayer);
	}

	[ServerVar(Help = "fillcontainer <optional: category> - Fills the container you are looking at with random items, can also specify a category (ammunition, weapon etc.)")]
	public static void fillContainer(Arg arg, string category)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		bool flag = !string.IsNullOrEmpty(category);
		ItemCategory result = ItemCategory.Weapon;
		if (flag && (category.IsNumeric() || !Enum.TryParse<ItemCategory>(category, ignoreCase: true, out result)))
		{
			arg.ReplyWith("'" + category + "' is not a valid item category!");
			return;
		}
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 6f, 1084293377);
		if (baseNetworkable is IItemContainerEntity itemContainerEntity)
		{
			if (FillContainerInternal(itemContainerEntity.inventory, flag, result, basePlayer))
			{
				arg.ReplyWith($"Filled {baseNetworkable}.");
			}
			else
			{
				arg.ReplyWith($"Tried to fill {baseNetworkable}, but it couldn't accept some or all of the items.");
			}
		}
		else
		{
			arg.ReplyWith("Not looking at a container.");
		}
	}

	[ServerVar(Help = "fillcontainer_radius <radius> <optional: category> - Fills containers with random items within a radius, can also specify a category")]
	public static void fillContainer_radius(Arg arg, int radius, string category)
	{
		BasePlayer ply = ArgEx.Player(arg);
		if (ply == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		if (radius == 0)
		{
			arg.ReplyWith("Usage: fillcontainer_radius <radius> <optional: category>");
			return;
		}
		radius = Math.Min(radius, 50);
		bool useCategory = !string.IsNullOrEmpty(category);
		ItemCategory parsedCategory = ItemCategory.Weapon;
		if (useCategory && (category.IsNumeric() || !Enum.TryParse<ItemCategory>(category, ignoreCase: true, out parsedCategory)))
		{
			arg.ReplyWith("'" + category + "' is not a valid item category!");
			return;
		}
		int foundAmount = 0;
		StringBuilder sb = new StringBuilder();
		RunInRadius(radius, ply, delegate(BaseCombatEntity entity)
		{
			if (entity.isServer && entity is IItemContainerEntity itemContainerEntity)
			{
				if (FillContainerInternal(itemContainerEntity.inventory, useCategory, parsedCategory, ply))
				{
					sb.AppendLine($"Filled {entity}.");
				}
				else
				{
					sb.AppendLine($"Tried to fill {entity}, but it couldn't accept some or all of the items.");
				}
				foundAmount++;
			}
		}, null, 1084293377);
		if (foundAmount == 0)
		{
			sb.AppendLine("Didn't find any containers in this radius.");
		}
		arg.ReplyWith(sb.ToString());
	}

	private static bool FillContainerInternal(ItemContainer container, bool useCategory, ItemCategory category, BasePlayer ply)
	{
		List<ItemDefinition> list = ItemManager.itemList.Where((ItemDefinition def) => !def.hidden && (!useCategory || def.category == category) && def.itemType != ItemContainer.ContentsType.Liquid).ToList();
		container.Clear();
		bool flag = false;
		int capacity = container.capacity;
		for (int i = 0; i < capacity; i++)
		{
			ItemDefinition random = list.GetRandom();
			Item item = ItemManager.CreateByItemID(random.itemid, random.stackable, 0uL, 0uL);
			item.OnVirginSpawn();
			item.SetItemOwnership(ply, ItemOwnershipPhrases.SpawnedPhrase);
			if (!item.MoveToContainer(container))
			{
				flag = true;
				item.Remove();
			}
		}
		return !flag;
	}

	[ServerVar(Help = "clearcontainer: Removes all items inside the container you're looking at")]
	public static void clearContainer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, basePlayer.eyes.HeadRay(), 0f, 6f, 1084293377);
		if (baseNetworkable is IItemContainerEntity itemContainerEntity)
		{
			arg.ReplyWith($"Cleared {baseNetworkable}.");
			itemContainerEntity.inventory.Clear();
		}
		else
		{
			arg.ReplyWith("Not looking at a container.");
		}
	}

	[ServerVar(Help = "clearcontainer_radius <radius>: Removes all items inside a container within a radius")]
	public static void clearContainer_radius(Arg arg, int radius)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player!");
			return;
		}
		if (radius == 0)
		{
			arg.ReplyWith("Usage: clearContainer_radius <radius>");
			return;
		}
		int foundAmount = 0;
		StringBuilder sb = new StringBuilder();
		RunInRadius(radius, basePlayer, delegate(BaseCombatEntity entity)
		{
			if (entity.isServer && entity is IItemContainerEntity itemContainerEntity)
			{
				sb.AppendLine($"Emptied {entity}.");
				itemContainerEntity.inventory.Clear();
			}
			foundAmount++;
		}, null, 1084293377);
		if (foundAmount == 0)
		{
			arg.ReplyWith("Didn't find any containers in this radius");
		}
		else
		{
			arg.ReplyWith(sb.ToString());
		}
	}

	[ServerVar(Help = "upgrade_radius 'grade' 'radius'")]
	public static void upgrade_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'upgrade_radius {grade} {radius}'");
		}
		else
		{
			SkinRadiusInternal(arg, changeAnyGrade: true);
		}
	}

	[ServerVar(Help = "<grade>")]
	public static void upgrade_looking(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'upgrade_looking {grade}'");
		}
		else
		{
			SkinRaycastInternal(arg, changeAnyGrade: true);
		}
	}

	[ServerVar(Help = "skin_radius 'skin' 'radius'")]
	public static void skin_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'skin_radius {skin} {radius}'");
		}
		else
		{
			SkinRadiusInternal(arg, changeAnyGrade: false);
		}
	}

	[ServerVar(Help = "<skin>")]
	public static void skin_looking(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'skin_looking <skin>'");
		}
		else
		{
			SkinRaycastInternal(arg, changeAnyGrade: false);
		}
	}

	[ServerVar(Help = "<name/id> <radius> | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random")]
	public static void add_wallpaper_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'add_wallpaper_radius {skin} {radius}' | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random");
		}
		else
		{
			wallpaper_radius_internal(arg, addIfMissing: true);
		}
	}

	[ServerVar(Help = "<name/id> <radius> | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random")]
	public static void change_wallpaper_radius(Arg arg)
	{
		if (!arg.HasArgs(2))
		{
			arg.ReplyWith("Format is 'change_wallpaper_radius {skin} {radius}' | Use print_wallpaper_skins for a list | 0 -> default, -1 -> random");
		}
		else
		{
			wallpaper_radius_internal(arg, addIfMissing: false);
		}
	}

	[ServerVar(Help = "clear_wallpaper_radius <radius>")]
	public static void clear_wallpaper_radius(Arg arg)
	{
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is 'clear_wallpaper_radius {radius}'");
			return;
		}
		RunInRadius(arg.GetFloat(0), ArgEx.Player(arg), delegate(BuildingBlock block)
		{
			if (block.HasWallpaper())
			{
				block.RemoveWallpaper(0);
				block.RemoveWallpaper(1);
			}
		}, null, 136314880);
	}

	public static BuildingGrade FindBuildingSkin(string name, out string error)
	{
		BuildingGrade buildingGrade = null;
		error = null;
		IEnumerable<BuildingGrade> source = from x in PrefabAttribute.server.FindAll<ConstructionGrade>(2194854973u)
			select x.gradeBase;
		switch (name)
		{
		case "twig":
		case "0":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "twigs");
			break;
		case "wood":
		case "1":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "wood");
			break;
		case "stone":
		case "2":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "stone");
			break;
		case "metal":
		case "sheetmetal":
		case "3":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "metal");
			break;
		case "hqm":
		case "armored":
		case "armoured":
		case "4":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "toptier");
			break;
		case "adobe":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "adobe");
			break;
		case "shipping":
		case "shippingcontainer":
		case "container":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "shipping_container");
			break;
		case "brutal":
		case "brutalist":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "brutalist");
			break;
		case "brick":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "brick");
			break;
		case "jungle":
		case "jungleruin":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "jungle");
			break;
		case "crypt":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "crypt");
			break;
		case "frontier":
		case "legacy":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "frontier");
			break;
		case "gingerbread":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "gingerbread");
			break;
		case "space":
		case "spacestation":
			buildingGrade = source.FirstOrDefault((BuildingGrade x) => x.name == "space_station");
			break;
		default:
			error = "Valid skins are:\ntwig\nwood | frontier | gingerbread\nstone | adobe | brick | brutalist | jungle | crypt\nmetal | shipping\nhqm | space";
			return null;
		}
		if (buildingGrade == null)
		{
			error = "Unable to find skin object for '" + name + "'";
		}
		return buildingGrade;
	}

	private static IEnumerable<BuildingBlock> SearchRadius(Vector3 position, float radius)
	{
		List<BuildingBlock> list = new List<BuildingBlock>();
		global::Vis.Entities(position, radius, list, 2097152);
		return list;
	}

	private static IEnumerable<BuildingBlock> SearchLookingAt(Vector3 position, Vector3 direction, float maxDistance)
	{
		BuildingBlock buildingBlock = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, new Ray(position, direction), 0f, maxDistance, 10485760, QueryTriggerInteraction.Ignore) as BuildingBlock;
		if (buildingBlock == null)
		{
			return Array.Empty<BuildingBlock>();
		}
		return buildingBlock.GetBuilding()?.buildingBlocks;
	}

	private static void SkinRadiusInternal(Arg arg, bool changeAnyGrade)
	{
		IEnumerable<BuildingBlock> blocks = SearchRadius(ArgEx.Player(arg).transform.position, arg.GetFloat(1));
		ApplySkinInternal(arg, changeAnyGrade, blocks);
	}

	private static void SkinRaycastInternal(Arg arg, bool changeAnyGrade)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		IEnumerable<BuildingBlock> blocks = SearchLookingAt(basePlayer.eyes.position, basePlayer.eyes.BodyForward(), 100f);
		ApplySkinInternal(arg, changeAnyGrade, blocks);
	}

	private static void ApplySkinInternal(Arg arg, bool changeAnyGrade, IEnumerable<BuildingBlock> blocks)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("This must be called from the client");
			return;
		}
		arg.GetFloat(1);
		string @string = arg.GetString(0);
		string error;
		BuildingGrade buildingGrade = FindBuildingSkin(@string, out error);
		if (buildingGrade == null)
		{
			arg.ReplyWith(error);
			return;
		}
		if (!buildingGrade.enabledInStandalone)
		{
			arg.ReplyWith("Skin " + @string + " is not enabled in standalone yet");
			return;
		}
		if (blocks == null || blocks.Count() == 0)
		{
			arg.ReplyWith("No building blocks found");
			return;
		}
		uint shippingContainerBlockColourForPlayer = BuildingBlock.GetShippingContainerBlockColourForPlayer(basePlayer);
		foreach (BuildingBlock block in blocks)
		{
			if (!block.isClient && (block.grade == buildingGrade.type || changeAnyGrade))
			{
				block.ChangeGradeAndSkin(buildingGrade.type, buildingGrade.skin, playEffect: false, updateSkin: true, shippingContainerBlockColourForPlayer);
			}
		}
	}

	private static void wallpaper_radius_internal(Arg arg, bool addIfMissing)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("This must be called from the client");
			return;
		}
		float @float = arg.GetFloat(1);
		string @string = arg.GetString(0);
		int skinIdParsed = -1;
		if (!int.TryParse(@string, out skinIdParsed))
		{
			skinIdParsed = -1;
		}
		bool flag = false;
		string foundSkinName = "";
		foreach (ItemSkinDirectory.Skin item in WallpaperSettings.WallpaperItemDef.skins.Concat(WallpaperSettings.FlooringItemDef.skins).Concat(WallpaperSettings.CeilingItemDef.skins))
		{
			if (skinIdParsed != -1 && item.id == skinIdParsed)
			{
				flag = true;
				foundSkinName = item.invItem.displayName.english.Trim();
				break;
			}
			if (skinIdParsed == -1 && (item.invItem.displayName.english.Contains(@string, StringComparison.InvariantCultureIgnoreCase) || item.invItem.name.Contains(@string, StringComparison.InvariantCultureIgnoreCase)))
			{
				flag = true;
				foundSkinName = item.invItem.displayName.english.Trim();
				skinIdParsed = item.id;
				break;
			}
		}
		if (skinIdParsed == 0)
		{
			flag = true;
		}
		if (!flag && skinIdParsed != -1)
		{
			arg.ReplyWith("Invalid skin");
			return;
		}
		RunInRadius(@float, basePlayer, delegate(BuildingBlock block)
		{
			bool flag2 = block.HasWallpaper();
			bool flag3 = flag2;
			if (addIfMissing && !flag2)
			{
				flag3 = WallpaperPlanner.Settings.CanUseWallpaper(block);
			}
			if (block.HasWallpaper() || flag3)
			{
				if (skinIdParsed == -1)
				{
					arg.ReplyWith("Applying random wallpaper");
					for (int i = 0; i < 2; i++)
					{
						ItemDefinition wallpaperItem = WallpaperPlanner.Settings.GetWallpaperItem(block, i);
						if (wallpaperItem != null)
						{
							int id = ArrayEx.GetRandom(wallpaperItem.skins).id;
							block.SetWallpaper((ulong)id, i);
						}
					}
				}
				else if (skinIdParsed == 0)
				{
					arg.ReplyWith("Applying default wallpaper");
					block.SetWallpaper(0uL);
					block.SetWallpaper(0uL, 1);
				}
				else
				{
					arg.ReplyWith("Applying '" + foundSkinName + "' wallpaper to compatible blocks");
					for (int j = 0; j < 2; j++)
					{
						ItemDefinition wallpaperItem2 = WallpaperPlanner.Settings.GetWallpaperItem(block, j);
						if (wallpaperItem2 != null && wallpaperItem2.skins.Any((ItemSkinDirectory.Skin x) => x.id == skinIdParsed))
						{
							block.SetWallpaper((ulong)skinIdParsed, j);
						}
					}
				}
				block.CheckWallpaper();
			}
		}, null, 136314880);
	}

	[ServerVar(Help = "Lists all wallpaper skins")]
	public static void print_wallpaper_skins(Arg arg)
	{
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumns("Id", "Type", "Name");
		ItemSkinDirectory.Skin[] skins = WallpaperSettings.WallpaperItemDef.skins;
		for (int i = 0; i < skins.Length; i++)
		{
			ItemSkinDirectory.Skin skin = skins[i];
			string[] array = new string[3];
			int id = skin.id;
			array[0] = id.ToString();
			array[1] = "Wall";
			array[2] = skin.invItem.displayName.english.Trim();
			textTable.AddRow(array);
		}
		skins = WallpaperSettings.FlooringItemDef.skins;
		for (int i = 0; i < skins.Length; i++)
		{
			ItemSkinDirectory.Skin skin2 = skins[i];
			string[] array2 = new string[3];
			int id = skin2.id;
			array2[0] = id.ToString();
			array2[1] = "Floor";
			array2[2] = skin2.invItem.displayName.english.Trim();
			textTable.AddRow(array2);
		}
		skins = WallpaperSettings.CeilingItemDef.skins;
		for (int i = 0; i < skins.Length; i++)
		{
			ItemSkinDirectory.Skin skin3 = skins[i];
			string[] array3 = new string[3];
			int id = skin3.id;
			array3[0] = id.ToString();
			array3[1] = "Ceiling";
			array3[2] = skin3.invItem.displayName.english.Trim();
			textTable.AddRow(array3);
		}
		arg.ReplyWith(textTable.ToString());
	}

	[ServerVar(Help = "<gene string> - Applies the given genes (e.g. \"YYYGGG\") to the clone/seed in your hands")]
	public static void applygenes(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called from a player");
			return;
		}
		Item activeItem = basePlayer.GetActiveItem();
		if (activeItem == null || !activeItem.info.TryGetComponent<ItemModDeployable>(out var component) || component.entityPrefab.Get().GetComponent<GrowableEntity>() == null)
		{
			arg.ReplyWith("Not holding a growable item");
			return;
		}
		string text = arg.GetString(0, "YYYGGG").ToUpper();
		if (text.Length != 6 || text.Any((char x) => !"XGHWY".Contains(x)))
		{
			arg.ReplyWith("Invalid gene string");
			return;
		}
		if (activeItem.instanceData == null)
		{
			activeItem.instanceData = new ProtoBuf.Item.InstanceData
			{
				ShouldPool = false,
				dataInt = GrowableGeneEncoding.EncodeGeneStringToInt(text)
			};
		}
		else
		{
			activeItem.instanceData.dataInt = GrowableGeneEncoding.EncodeGeneStringToInt(text);
		}
		activeItem.MarkDirty();
		arg.ReplyWith("Applied genes to the held item");
	}

	[ServerVar(Help = "Kills all bee swarms")]
	public static void killbees(Arg arg)
	{
		int num = 0;
		BeeSwarmMaster[] array = BaseEntity.Util.FindAll<BeeSwarmMaster>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AdminKill();
			num++;
		}
		BeeSwarmAI[] array2 = BaseEntity.Util.FindAll<BeeSwarmAI>();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].AdminKill();
			num++;
		}
		arg.ReplyWith($"Killed {num} bee swarms");
	}

	[ServerVar(Help = "(Generated) Deals 1000 damage to the specified player (by name/SteamID/bot) killing them immediately; useful for testing death logic without console kill commands")]
	public static void killplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!basePlayer)
		{
			basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
		}
		if (!basePlayer)
		{
			arg.ReplyWith("Player not found");
		}
		else
		{
			basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
		}
	}

	[ServerVar(Help = "(Generated) Deals lethal damage to every non-NPC player currently connected to the server; reports the number of players killed")]
	public static void killallplayers(Arg arg)
	{
		BasePlayer[] array = BaseEntity.Util.FindAll<BasePlayer>();
		int num = 0;
		BasePlayer[] array2 = array;
		foreach (BasePlayer basePlayer in array2)
		{
			if (!basePlayer.IsNpc)
			{
				basePlayer.Hurt(1000f, DamageType.Suicide, basePlayer, useProtection: false);
				num++;
			}
		}
		arg.ReplyWith($"Killed {num} players");
	}

	[ServerVar(Help = "(Generated) Puts the specified player into the wounded/downed state immediately without killing them; useful for testing the crawl/revive mechanics")]
	public static void injureplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!basePlayer)
		{
			basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
		}
		if (!basePlayer)
		{
			arg.ReplyWith("Player not found");
		}
		else
		{
			Global.InjurePlayer(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Recovers the specified player from the wounded state, standing them back up at minimum health")]
	public static void recoverplayer(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!basePlayer)
		{
			basePlayer = BasePlayer.FindBotClosestMatch(arg.GetString(0));
		}
		if (!basePlayer)
		{
			arg.ReplyWith("Player not found");
		}
		else
		{
			Global.RecoverPlayer(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Kicks the specified player from the server with an optional reason; broadcasts the kick to chat and places them through the queue on reconnect")]
	public static void kick(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (!player || player.net == null || player.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		string @string = arg.GetString(1, "no reason given");
		arg.ReplyWith("Kicked: " + player.displayName);
		Chat.Broadcast("Kicking " + player.displayName + " (" + @string + ")", "SERVER", "#eee", 0uL);
		player.Kick("Kicked: " + arg.GetString(1, "No Reason Given"), reserveSlot: false);
	}

	[ServerVar(Help = "(Generated) Silently kicks the specified player without broadcasting to chat; the kick is logged to RCON only")]
	public static void skick(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (!player || player.net == null || player.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		string @string = arg.GetString(1, "no reason given");
		arg.ReplyWith("Kicked: " + player.displayName);
		Chat.ChatEntry ce = default(Chat.ChatEntry);
		ce.Channel = Chat.ChatChannel.Server;
		ce.Message = "(SILENT) Kicking " + player.displayName + " (" + @string + ")";
		ce.UserId = "0";
		ce.Username = "SERVER";
		ce.Color = "#eee";
		ce.Time = Epoch.Current;
		Chat.Record(ce);
		player.Kick("Kicked: " + arg.GetString(1, "No Reason Given"), reserveSlot: false);
	}

	[ServerVar(Help = "(Generated) Kicks all currently connected players from the server with an optional reason; useful for forcing a restart or clearing the server")]
	public static void kickall(Arg arg)
	{
		BasePlayer[] array = BasePlayer.activePlayerList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kick("Kicked: " + arg.GetString(0, "No Reason Given"));
		}
	}

	[ServerVar(Help = "ban <player> <reason> [optional duration]")]
	public static void ban(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (!player || player.net == null || player.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		ServerUsers.User user = ServerUsers.Get(player.userID);
		if (user != null && user.group == ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith($"User {player.userID.Get()} is already banned");
			return;
		}
		string @string = arg.GetString(1, "No Reason Given");
		if (TryGetBanExpiry(arg, 2, out var expiry, out var durationSuffix))
		{
			ServerUsers.Set(player.userID, ServerUsers.UserGroup.Banned, player.displayName, @string, expiry);
			string text = "";
			if (player.IsConnected && player.net.connection.ownerid != 0L && player.net.connection.ownerid != player.net.connection.userid)
			{
				text += $" and also banned ownerid {player.net.connection.ownerid}";
				ServerUsers.Set(player.net.connection.ownerid, ServerUsers.UserGroup.Banned, player.displayName, arg.GetString(1, $"Family share owner of {player.net.connection.userid}"), -1L);
			}
			ServerUsers.Save();
			arg.ReplyWith($"Kickbanned User{durationSuffix}: {player.userID.Get()} - {player.displayName}{text}");
			Chat.Broadcast("Kickbanning " + player.displayName + durationSuffix + " (" + @string + ")", "SERVER", "#eee", 0uL);
			Network.Net.sv.Kick(player.net.connection, "Banned" + durationSuffix + ": " + @string);
		}
	}

	[ServerVar(Help = "(Generated) Adds the specified Steam64 ID as a server moderator with optional name and reason; grants admin flag to the player if connected")]
	public static void moderatorid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string @string = arg.GetString(1, "unnamed");
		string string2 = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && user.group == ServerUsers.UserGroup.Moderator)
		{
			arg.ReplyWith("User " + uInt + " is already a Moderator");
			return;
		}
		ServerUsers.Set(uInt, ServerUsers.UserGroup.Moderator, @string, string2, -1L);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if (basePlayer != null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: true);
			basePlayer.net.connection.authLevel = 1u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Added moderator " + @string + ", steamid " + uInt);
	}

	[ServerVar(Help = "(Generated) Adds the specified Steam64 ID as a server owner (auth level 2) with optional name and reason; requires the caller to also be auth level 2")]
	public static void ownerid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string @string = arg.GetString(1, "unnamed");
		string string2 = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		if (arg.Connection != null && arg.Connection.authLevel < 2)
		{
			arg.ReplyWith("Moderators cannot run ownerid");
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && user.group == ServerUsers.UserGroup.Owner)
		{
			arg.ReplyWith("User " + uInt + " is already an Owner");
			return;
		}
		ServerUsers.Set(uInt, ServerUsers.UserGroup.Owner, @string, string2, -1L);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if (basePlayer != null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: true);
			basePlayer.net.connection.authLevel = 2u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Added owner " + @string + ", steamid " + uInt);
	}

	[ServerVar(Help = "(Generated) Removes moderator status from the specified Steam64 ID; removes admin flag from the player if currently connected")]
	public static void removemoderator(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user == null || user.group != ServerUsers.UserGroup.Moderator)
		{
			arg.ReplyWith("User " + uInt + " isn't a moderator");
			return;
		}
		ServerUsers.Remove(uInt);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if (basePlayer != null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: false);
			basePlayer.net.connection.authLevel = 0u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Removed Moderator: " + uInt);
	}

	[ServerVar(Help = "(Generated) Removes owner status from the specified Steam64 ID; removes admin flag from the player if currently connected")]
	public static void removeowner(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user == null || user.group != ServerUsers.UserGroup.Owner)
		{
			arg.ReplyWith("User " + uInt + " isn't an owner");
			return;
		}
		ServerUsers.Remove(uInt);
		ServerUsers.Save();
		BasePlayer basePlayer = BasePlayer.FindByID(uInt);
		if (basePlayer != null)
		{
			basePlayer.SetPlayerFlag(BasePlayer.PlayerFlags.IsAdmin, b: false);
			basePlayer.net.connection.authLevel = 0u;
			basePlayer.SendNetworkUpdate();
		}
		arg.ReplyWith("Removed Owner: " + uInt);
	}

	[ServerVar(Help = "banid <steamid> <username> <reason> [optional duration]")]
	public static void banid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string text = arg.GetString(1, "unnamed");
		string @string = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && user.group == ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith("User " + uInt + " is already banned");
		}
		else
		{
			if (!TryGetBanExpiry(arg, 3, out var expiry, out var durationSuffix))
			{
				return;
			}
			string text2 = "";
			BasePlayer basePlayer = BasePlayer.FindByID(uInt);
			if (basePlayer != null && basePlayer.IsConnected)
			{
				text = basePlayer.displayName;
				if (basePlayer.IsConnected && basePlayer.net.connection.ownerid != 0L && basePlayer.net.connection.ownerid != basePlayer.net.connection.userid)
				{
					text2 += $" and also banned ownerid {basePlayer.net.connection.ownerid}";
					ServerUsers.Set(basePlayer.net.connection.ownerid, ServerUsers.UserGroup.Banned, basePlayer.displayName, arg.GetString(1, $"Family share owner of {basePlayer.net.connection.userid}"), expiry);
				}
				Chat.Broadcast("Kickbanning " + basePlayer.displayName + durationSuffix + " (" + @string + ")", "SERVER", "#eee", 0uL);
				Network.Net.sv.Kick(basePlayer.net.connection, "Banned" + durationSuffix + ": " + @string);
			}
			ServerUsers.Set(uInt, ServerUsers.UserGroup.Banned, text, @string, expiry);
			arg.ReplyWith($"Banned User{durationSuffix}: {uInt} - \"{text}\" for \"{@string}\"{text2}");
		}
	}

	private static bool TryGetBanExpiry(Arg arg, int n, out long expiry, out string durationSuffix)
	{
		expiry = arg.GetTimestamp(n, -1L);
		durationSuffix = null;
		int current = Epoch.Current;
		if (expiry > 0 && expiry <= current)
		{
			arg.ReplyWith("Expiry time is in the past");
			return false;
		}
		durationSuffix = ((expiry > 0) ? (" for " + (expiry - current).FormatSecondsLong()) : "");
		return true;
	}

	[ServerVar(Help = "(Generated) Removes the ban for the specified Steam64 ID from the server banlist, allowing the player to reconnect")]
	public static void unban(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith($"This doesn't appear to be a 64bit steamid: {uInt}");
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user == null || user.group != ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith($"User {uInt} isn't banned");
			return;
		}
		ServerUsers.Remove(uInt);
		arg.ReplyWith("Unbanned User: " + uInt);
	}

	[ServerVar(Help = "(Generated) Moves the specified Steam64 ID to the front of the connection queue so they connect immediately on next join")]
	public static void skipqueue(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
		}
		else
		{
			SingletonComponent<ServerMgr>.Instance.connectionQueue.SkipQueue(uInt);
		}
	}

	[ServerVar(Help = "Adds skip queue permissions to a SteamID")]
	public static void skipqueueid(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		string @string = arg.GetString(1, "unnamed");
		string string2 = arg.GetString(2, "no reason");
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && (user.group == ServerUsers.UserGroup.Owner || user.group == ServerUsers.UserGroup.Moderator || user.group == ServerUsers.UserGroup.SkipQueue))
		{
			arg.ReplyWith($"User {uInt} will already skip the queue ({user.group})");
			return;
		}
		if (user != null && user.group == ServerUsers.UserGroup.Banned)
		{
			arg.ReplyWith($"User {uInt} is banned");
			return;
		}
		ServerUsers.Set(uInt, ServerUsers.UserGroup.SkipQueue, @string, string2, -1L);
		arg.ReplyWith($"Added skip queue permission for {@string} ({uInt})");
	}

	[ServerVar(Help = "Removes skip queue permission from a SteamID")]
	public static void removeskipqueue(Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		if (uInt < 70000000000000000L)
		{
			arg.ReplyWith("This doesn't appear to be a 64bit steamid: " + uInt);
			return;
		}
		ServerUsers.User user = ServerUsers.Get(uInt);
		if (user != null && (user.group == ServerUsers.UserGroup.Owner || user.group == ServerUsers.UserGroup.Moderator))
		{
			arg.ReplyWith($"User is a {user.group}, cannot remove skip queue permission with this command");
			return;
		}
		if (user == null || user.group != ServerUsers.UserGroup.SkipQueue)
		{
			arg.ReplyWith("User does not have skip queue permission");
			return;
		}
		ServerUsers.Remove(uInt);
		arg.ReplyWith("Removed skip queue permission: " + uInt);
	}

	[ServerVar(Help = "Print out currently connected clients etc")]
	public static void players(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.ResizeColumns(5);
		textTable.AddColumn("id");
		textTable.AddColumn("name");
		textTable.AddColumn("ping");
		textTable.AddColumn("updt");
		textTable.AddColumn("dist");
		textTable.AddColumn("enId");
		textTable.ResizeRows(BasePlayer.activePlayerList.Count);
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			string userIDString = activePlayer.UserIDString;
			textTable.AddValue(userIDString);
			string text = activePlayer.displayName;
			if (text.Length >= 14)
			{
				text = text.Substring(0, 14) + "..";
			}
			textTable.AddValue(text);
			int averagePing = Network.Net.sv.GetAveragePing(activePlayer.net.connection);
			textTable.AddValue(averagePing);
			int queuedUpdateCount = activePlayer.GetQueuedUpdateCount(BasePlayer.NetworkQueue.Update);
			textTable.AddValue(queuedUpdateCount);
			int queuedUpdateCount2 = activePlayer.GetQueuedUpdateCount(BasePlayer.NetworkQueue.UpdateDistance);
			textTable.AddValue(queuedUpdateCount2);
			ulong value = activePlayer.net.ID.Value;
			textTable.AddValue(value);
		}
		arg.ReplyWith(flag ? textTable.ToJson(stringify: false) : textTable.ToString());
	}

	[ServerVar(Help = "Sends a message in chat")]
	public static void say(Arg arg)
	{
		Chat.Broadcast((string)arg.FullString, "SERVER", "#eee", 0uL);
	}

	[ServerVar(Help = "Show user info for players on server.")]
	public static void users(Arg arg)
	{
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			text = text + activePlayer.userID.Get() + ":\"" + activePlayer.displayName + "\"\n";
			num++;
		}
		text = text + num + "users\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for players on server.")]
	public static void sleepingusers(Arg arg)
	{
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
		{
			text += $"{sleepingPlayer.userID.Get()}:{sleepingPlayer.displayName}\n";
			num++;
		}
		text += $"{num} sleeping users\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for sleeping players on server in range of the player.")]
	public static void sleepingusersinrange(Arg arg)
	{
		BasePlayer fromPlayer = ArgEx.Player(arg);
		if (fromPlayer == null)
		{
			return;
		}
		if (fromPlayer.IsSpectating() && fromPlayer.SpectatingTarget != null)
		{
			fromPlayer = fromPlayer.SpectatingTarget;
		}
		float range = arg.GetFloat(0);
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		foreach (BasePlayer sleepingPlayer in BasePlayer.sleepingPlayerList)
		{
			obj.Add(sleepingPlayer);
		}
		obj.RemoveAll((BasePlayer p) => p.Distance2D(fromPlayer) > range);
		obj.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D(fromPlayer) < basePlayer.Distance2D(fromPlayer))) ? 1 : (-1));
		foreach (BasePlayer item in obj)
		{
			text += $"{item.userID.Get()}:{item.displayName}:{item.Distance2D(fromPlayer)}m\n";
			num++;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		text += $"{num} sleeping users within {range}m\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for players on server in range of the player.")]
	public static void usersinrange(Arg arg)
	{
		BasePlayer fromPlayer = ArgEx.Player(arg);
		if (fromPlayer == null)
		{
			return;
		}
		if (fromPlayer.IsSpectating() && fromPlayer.SpectatingTarget != null)
		{
			fromPlayer = fromPlayer.SpectatingTarget;
		}
		float range = arg.GetFloat(0);
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			obj.Add(activePlayer);
		}
		obj.RemoveAll((BasePlayer p) => p.Distance2D(fromPlayer) > range);
		obj.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D(fromPlayer) < basePlayer.Distance2D(fromPlayer))) ? 1 : (-1));
		foreach (BasePlayer item in obj)
		{
			text += $"{item.userID.Get()}:{item.displayName}:{item.Distance2D(fromPlayer)}m\n";
			num++;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		text += $"{num} users within {range}m\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "Show user info for players on server in range of the supplied player (eg. Jim 50)")]
	public static void usersinrangeofplayer(Arg arg)
	{
		BasePlayer targetPlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (targetPlayer == null)
		{
			return;
		}
		float range = arg.GetFloat(1);
		string text = "<slot:userid:\"name\">\n";
		int num = 0;
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			obj.Add(activePlayer);
		}
		obj.RemoveAll((BasePlayer p) => p.Distance2D(targetPlayer) > range);
		obj.Sort((BasePlayer player, BasePlayer basePlayer) => (!(player.Distance2D(targetPlayer) < basePlayer.Distance2D(targetPlayer))) ? 1 : (-1));
		foreach (BasePlayer item in obj)
		{
			text += $"{item.userID.Get()}:{item.displayName}:{item.Distance2D(targetPlayer)}m\n";
			num++;
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		text += $"{num} users within {range}m of {targetPlayer.displayName}\n";
		arg.ReplyWith(text);
	}

	[ServerVar(Help = "List of banned users (sourceds compat)")]
	public static void banlist(Arg arg)
	{
		arg.ReplyWith(ServerUsers.BanListString());
	}

	[ServerVar(Help = "List of banned users - shows reasons and usernames")]
	public static void banlistex(Arg arg)
	{
		arg.ReplyWith(ServerUsers.BanListStringEx());
	}

	[ServerVar(Help = "List of banned users, by ID (sourceds compat)")]
	public static void listid(Arg arg)
	{
		arg.ReplyWith(ServerUsers.BanListString(bHeader: true));
	}

	[ServerVar(Help = "(Generated) Mutes the specified connected player preventing them from using chat; optionally accepts a mute expiry timestamp for temporary mutes")]
	public static void mute(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		long timestamp = arg.GetTimestamp(1, 0L);
		if (timestamp > 0)
		{
			playerOrSleeper.State.chatMuteExpiryTimestamp = timestamp;
			string text = (timestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds()).FormatSecondsLong();
			playerOrSleeper.ChatMessage("You have been muted for " + text);
		}
		else
		{
			playerOrSleeper.State.chatMuteExpiryTimestamp = 0.0;
			playerOrSleeper.ChatMessage("You have been permanently muted");
		}
		playerOrSleeper.State.chatMuted = true;
		playerOrSleeper.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, b: true);
	}

	[ServerVar(Help = "(Generated) Removes the chat mute from the specified connected player, allowing them to send messages again")]
	public static void unmute(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		if (!playerOrSleeper || playerOrSleeper.net == null || playerOrSleeper.net.connection == null)
		{
			arg.ReplyWith("Player not found");
			return;
		}
		playerOrSleeper.State.chatMuted = false;
		playerOrSleeper.State.chatMuteExpiryTimestamp = 0.0;
		playerOrSleeper.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, b: false);
		playerOrSleeper.ChatMessage("You have been unmuted");
	}

	[ServerVar(Help = "Print a list of currently muted players")]
	public static void mutelist(Arg arg)
	{
		var obj = from x in BasePlayer.allPlayerList
			where x.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute)
			select new
			{
				SteamId = x.UserIDString,
				Name = x.displayName
			};
		arg.ReplyWith(obj);
	}

	[ServerVar(Help = "(Generated) Requests a performance report from every connected client; supports legacy and JSON formats; used for monitoring client frame rates and memory usage")]
	public static void clientperf(Arg arg)
	{
		string @string = arg.GetString(0, "legacy");
		int @int = arg.GetInt(1, UnityEngine.Random.Range(int.MinValue, int.MaxValue));
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			activePlayer.ClientRPC(RpcTarget.Player("GetPerformanceReport", activePlayer), @string, @int);
		}
	}

	[ServerVar(Help = "Get information about all the cars in the world")]
	public static void carstats(Arg arg)
	{
		HashSet<ModularCar> allCarsList = ModularCar.allCarsList;
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumn("id");
		textTable.AddColumn("sockets");
		textTable.AddColumn("modules");
		textTable.AddColumn("complete");
		textTable.AddColumn("engine");
		textTable.AddColumn("health");
		textTable.AddColumn("location");
		int count = allCarsList.Count;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (ModularCar item in allCarsList)
		{
			string text = item.net.ID.ToString();
			string text2 = item.TotalSockets.ToString();
			string text3 = item.NumAttachedModules.ToString();
			string text4;
			if (item.IsComplete())
			{
				text4 = "Complete";
				num++;
			}
			else
			{
				text4 = "Partial";
			}
			string text5;
			if (item.HasAnyWorkingEngines())
			{
				text5 = "Working";
				num2++;
			}
			else
			{
				text5 = "Broken";
			}
			string text6 = ((item.TotalMaxHealth() != 0f) ? $"{item.TotalHealth() / item.TotalMaxHealth():0%}" : "0");
			string text7;
			if (item.IsOutside())
			{
				text7 = "Outside";
			}
			else
			{
				text7 = "Inside";
				num3++;
			}
			textTable.AddRow(text, text2, text3, text4, text5, text6, text7);
		}
		string text8 = "";
		text8 = ((count != 1) ? (text8 + $"\nThe world contains {count} modular cars.") : (text8 + "\nThe world contains 1 modular car."));
		text8 = ((num != 1) ? (text8 + $"\n{num} ({(float)num / (float)count:0%}) are in a completed state.") : (text8 + $"\n1 ({1f / (float)count:0%}) is in a completed state."));
		text8 = ((num2 != 1) ? (text8 + $"\n{num2} ({(float)num2 / (float)count:0%}) are driveable.") : (text8 + $"\n1 ({1f / (float)count:0%}) is driveable."));
		arg.ReplyWith(string.Concat(str1: (num3 != 1) ? (text8 + $"\n{num3} ({(float)num3 / (float)count:0%}) are sheltered indoors.") : (text8 + $"\n1 ({1f / (float)count:0%}) is sheltered indoors."), str0: textTable.ToString()));
	}

	[ServerVar(Help = "(Generated) Prints a table of all members in the team of the specified player showing Steam ID, username, online status, and whether they are team leader; supports --json")]
	public static string teaminfo(Arg arg)
	{
		ulong num = ArgEx.GetPlayerOrSleeper(arg, 0)?.userID ?? ((EncryptedValue<ulong>)0uL);
		if (num == 0L)
		{
			num = arg.GetULong(0, 0uL);
		}
		if (!SingletonComponent<ServerMgr>.Instance.persistance.DoesPlayerExist(num))
		{
			return "Player not found";
		}
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(num);
		if (playerTeam == null)
		{
			return "Player is not in a team";
		}
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.ResizeColumns(4);
		textTable.AddColumn("steamID");
		textTable.AddColumn("username");
		textTable.AddColumn("online");
		textTable.AddColumn("leader");
		textTable.ResizeRows(playerTeam.members.Count);
		foreach (ulong memberId in playerTeam.members)
		{
			bool flag2 = Network.Net.sv.connections.FirstOrDefault((Connection c) => c.connected && c.userid == memberId) != null;
			textTable.AddValue(memberId);
			textTable.AddValue(GetPlayerName(memberId));
			textTable.AddValue(flag2 ? "x" : "");
			textTable.AddValue((memberId == playerTeam.teamLeader) ? "x" : "");
		}
		return flag ? textTable.ToJson() : $"ID: {playerTeam.teamID}\n\n{textTable}";
	}

	[ServerVar(Help = "(Generated) Authorises the specified player (or caller if none given) to all tool cupboards within the given radius around them")]
	public static void authradius(Arg arg)
	{
		float @float = arg.GetFloat(0, -1f);
		if (@float < 0f)
		{
			arg.ReplyWith("Format is 'authradius {radius} [user]'");
			return;
		}
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		obj.Add(ArgEx.GetPlayer(arg, 1) ?? ArgEx.Player(arg));
		SetAuthInRadius(obj[0], obj, @float, auth: true);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Authorises multiple specified players to all tool cupboards within the given radius around the calling admin")]
	public static void authradius_multi(Arg arg)
	{
		float @float = arg.GetFloat(0, -1f);
		if (@float < 0f)
		{
			arg.ReplyWith("Format is 'authradius {radius} [user, user, ...]'");
		}
		else
		{
			SetAuthInRadius(ArgEx.Player(arg), ArgEx.GetPlayerArgs(arg, 1), @float, auth: true);
		}
	}

	[ServerVar(Help = "(Generated) Finds all players within playerRadius of the caller, then authorises each of them to TCs within authRadius of themselves")]
	public static void authradius_radius(Arg arg)
	{
		run_authradius_radius(arg, authFlag: true);
	}

	[ServerVar(Help = "(Generated) Finds all players within playerRadius of the caller, then deauthorises each of them from TCs within authRadius of themselves")]
	public static void deauthradius_radius(Arg arg)
	{
		run_authradius_radius(arg, authFlag: false);
	}

	private static void run_authradius_radius(Arg arg, bool authFlag)
	{
		float @float = arg.GetFloat(0, -1f);
		float float2 = arg.GetFloat(1, -1f);
		if (@float < 0f || float2 < 0f)
		{
			arg.ReplyWith("Format is 'authradius_radius {playerRadius, authRadius }'");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		List<BasePlayer> obj = Facepunch.Pool.Get<List<BasePlayer>>();
		global::Vis.Entities(basePlayer.transform.position, @float, obj, 131072);
		for (int num = obj.Count - 1; num >= 0; num--)
		{
			BasePlayer basePlayer2 = obj[num];
			if (basePlayer2 == null)
			{
				obj.RemoveAt(num);
			}
			else if (basePlayer2.isClient || Vector3.Distance(basePlayer2.transform.position, basePlayer.transform.position) > @float)
			{
				obj.Remove(basePlayer2);
			}
		}
		SetAuthInRadius(basePlayer, obj, float2, authFlag);
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Removes authorisation for the specified player (or caller) from all tool cupboards within the given radius")]
	public static void deauthradius(Arg arg)
	{
		float @float = arg.GetFloat(0, -1f);
		if (@float < 0f)
		{
			arg.ReplyWith("Format is 'deauthradius {radius} [user]'");
			return;
		}
		List<BasePlayer> list = new List<BasePlayer>();
		list.Add(ArgEx.GetPlayer(arg, 1) ?? ArgEx.Player(arg));
		SetAuthInRadius(list[0], list, @float, auth: false);
	}

	[ServerVar(Help = "(Generated) Removes authorisation for multiple specified players from all tool cupboards within the given radius around the calling admin")]
	public static void deauthradius_multi(Arg arg)
	{
		float @float = arg.GetFloat(0, -1f);
		if (@float < 0f)
		{
			arg.ReplyWith("Format is 'deauthradius {radius} [user, user, ...]'");
		}
		else
		{
			SetAuthInRadius(ArgEx.Player(arg), ArgEx.GetPlayerArgs(arg, 1), @float, auth: false);
		}
	}

	private static void SetAuthInRadius(BasePlayer radiusTargetPlayer, List<BasePlayer> players, float radius, bool auth)
	{
		if (players == null)
		{
			return;
		}
		if (players.Count == 0)
		{
			players.Add(radiusTargetPlayer);
		}
		List<BaseEntity> list = new List<BaseEntity>();
		global::Vis.Entities(radiusTargetPlayer.transform.position, radius, list);
		int num = 0;
		foreach (BaseEntity item in list)
		{
			if (!item.isServer)
			{
				continue;
			}
			bool flag = true;
			foreach (BasePlayer player in players)
			{
				bool flag2 = SetUserAuthorized(item, player.userID, auth);
				if (!flag2)
				{
					flag2 = SetUserAuthorized(item.GetSlot(BaseEntity.Slot.Lock), player.userID, auth);
				}
				if (flag)
				{
					num += (flag2 ? 1 : 0);
					flag = false;
				}
			}
		}
		Debug.Log("Set auth: " + auth + " on " + players.Count + " players, for " + num + " entities.");
	}

	public static bool SetUserAuthorized(BaseEntity entity, ulong userId, bool state)
	{
		if (entity == null)
		{
			return false;
		}
		if (entity is CodeLock codeLock)
		{
			if (state)
			{
				codeLock.whitelistPlayers.Add(userId);
			}
			else
			{
				codeLock.whitelistPlayers.Remove(userId);
				codeLock.guestPlayers.Remove(userId);
			}
			codeLock.SendNetworkUpdate();
		}
		else if (entity is AutoTurret autoTurret)
		{
			if (state)
			{
				autoTurret.authorizedPlayers.Add(userId);
			}
			else
			{
				autoTurret.authorizedPlayers.Remove(userId);
			}
			autoTurret.SendNetworkUpdate();
		}
		else if (entity is BuildingPrivlidge buildingPrivlidge)
		{
			if (state)
			{
				buildingPrivlidge.authorizedPlayers.Add(userId);
				buildingPrivlidge.recentGroupMembers.Remove(userId);
			}
			else
			{
				buildingPrivlidge.authorizedPlayers.Remove(userId);
				buildingPrivlidge.recentGroupMembers[userId] = (uint)Epoch.Current;
			}
			buildingPrivlidge.RecalculateGroupUpkeep();
			if (entity.GetSlot(BaseEntity.Slot.Lock).IsValid())
			{
				SetUserAuthorized(entity.GetSlot(BaseEntity.Slot.Lock), userId, state);
			}
			buildingPrivlidge.SendNetworkUpdate();
		}
		else if (entity is Tugboat tugboat)
		{
			VehiclePrivilege componentInChildren = tugboat.GetComponentInChildren<VehiclePrivilege>();
			if (componentInChildren != null)
			{
				if (state)
				{
					componentInChildren.authorizedPlayers.Add(userId);
				}
				else
				{
					componentInChildren.authorizedPlayers.Remove(userId);
				}
				componentInChildren.SendNetworkUpdate();
			}
		}
		else if (entity is PlayerBoat playerBoat)
		{
			PlayerBoatPrivilege privilege = playerBoat.GetSteeringWheel().Privilege;
			if (privilege != null)
			{
				if (state)
				{
					privilege.authorizedPlayers.Add(userId);
				}
				else
				{
					privilege.authorizedPlayers.Remove(userId);
				}
				privilege.SendNetworkUpdate();
			}
		}
		else
		{
			if (!(entity is ModularCar modularCar))
			{
				return false;
			}
			if (state)
			{
				modularCar.CarLock.TryAddPlayer(userId);
			}
			else
			{
				modularCar.CarLock.TryRemovePlayer(userId);
			}
			modularCar.SendNetworkUpdate();
		}
		return true;
	}

	[ServerVar(Help = "(Generated) Runs an admin command (kill, lock, unlock, etc.) on a specific entity by network ID; blocks operation on players and point entities")]
	public static void entid(Arg arg)
	{
		BaseEntity baseEntity = BaseNetworkable.serverEntities.Find(ArgEx.GetEntityID(arg, 1)) as BaseEntity;
		if (baseEntity == null || baseEntity is BasePlayer || baseEntity is PointEntity)
		{
			return;
		}
		string @string = arg.GetString(0);
		if (ArgEx.Player(arg) != null)
		{
			Debug.Log($"[ENTCMD] {ArgEx.Player(arg).displayName}/{ArgEx.Player(arg).userID.Get()} used *{@string}* on ent [{baseEntity.name}/{baseEntity.net.ID}] at position {baseEntity.transform.position}");
		}
		switch (@string)
		{
		case "kill":
			baseEntity.AdminKill();
			break;
		case "lock":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope2 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope2.Set(BaseEntity.Flags.Locked, b: true);
			break;
		}
		case "unlock":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Locked, b: false);
			break;
		}
		case "open":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope6 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope6.Set(BaseEntity.Flags.Open, b: true);
			break;
		}
		case "close":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope5 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope5.Set(BaseEntity.Flags.Open, b: false);
			break;
		}
		case "debug":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope4 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope4.Set(BaseEntity.Flags.Debugging, b: true);
			break;
		}
		case "undebug":
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope3 = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope3.Set(BaseEntity.Flags.Debugging, b: false);
			break;
		}
		case "who":
			arg.ReplyWith(baseEntity.Admin_Who());
			break;
		case "auth":
			arg.ReplyWith(AuthList(baseEntity));
			break;
		case "upgrade":
			arg.ReplyWith(ChangeGrade(baseEntity, arg.GetInt(2, 1), 0, BuildingGrade.Enum.None, 0uL, arg.GetFloat(3)));
			break;
		case "downgrade":
			arg.ReplyWith(ChangeGrade(baseEntity, 0, arg.GetInt(2, 1), BuildingGrade.Enum.None, 0uL, arg.GetFloat(3)));
			break;
		case "setgrade":
		{
			string error;
			BuildingGrade buildingGrade = FindBuildingSkin(arg.GetString(2), out error);
			arg.ReplyWith(ChangeGrade(baseEntity, 0, 0, buildingGrade.type, buildingGrade.skin, arg.GetFloat(3)));
			break;
		}
		case "repair":
			RunInRadius(arg.GetFloat(2), baseEntity, delegate(BaseCombatEntity entity)
			{
				if (entity.repair.enabled)
				{
					entity.SetHealth(entity.MaxHealth());
				}
			});
			break;
		case "maxhp":
		{
			if (!(baseEntity is BaseCombatEntity baseCombatEntity))
			{
				arg.ReplyWith("Entity doesn't support max health!");
				break;
			}
			float @float = arg.GetFloat(2);
			baseCombatEntity.OverrideMaxHealth(@float);
			if (@float <= 0f)
			{
				arg.ReplyWith($"Removed max health override from {baseEntity}");
			}
			else
			{
				arg.ReplyWith($"Set max health to {@float}");
			}
			break;
		}
		case "dronetax":
		{
			List<MarketTerminal> list = new List<MarketTerminal>();
			if (baseEntity is Marketplace marketplace)
			{
				list.AddRange(from x in marketplace.terminalEntities
					select x.Get(serverside: true) into x
					where x != null
					select x);
			}
			else
			{
				if (!(baseEntity is MarketTerminal item))
				{
					arg.ReplyWith("Entity is not a market terminal!");
					break;
				}
				list.Add(item);
			}
			{
				foreach (MarketTerminal item2 in list)
				{
					string string2 = arg.GetString(2);
					if (int.TryParse(string2, out var result) && result > 0)
					{
						item2.deliveryFeeAmount = result;
						item2.SendNetworkUpdate();
						arg.ReplyWith($"Set drone tax to '{result}'");
						continue;
					}
					ItemDefinition itemDefinition = ItemManager.FindDefinitionByPartialName(string2);
					if (itemDefinition != null)
					{
						item2.deliveryFeeCurrency = itemDefinition;
						item2.SendNetworkUpdate();
						arg.ReplyWith("Set drone tax item to '" + itemDefinition.shortname + "'");
					}
					else
					{
						arg.ReplyWith("'" + string2 + "' is not a tax amount or valid item!");
					}
				}
				break;
			}
		}
		case "image":
		{
			if (!(baseEntity is ISignage signage))
			{
				arg.ReplyWith("Entity is not a sign");
				break;
			}
			uint[] textureCRCs = signage.GetTextureCRCs();
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"{textureCRCs.Length} Image CRCs");
			uint[] array = textureCRCs;
			foreach (uint num in array)
			{
				stringBuilder.AppendLine(num.ToString());
			}
			arg.ReplyWith(stringBuilder.ToString());
			break;
		}
		case "scale":
		{
			string string3 = arg.GetString(2);
			if (string.IsNullOrEmpty(string3))
			{
				arg.ReplyWith($"Scale: {baseEntity.transform.localScale}");
				break;
			}
			if (string3 == "default")
			{
				baseEntity.networkEntityScale = false;
				baseEntity.transform.localScale = Vector3.one;
				baseEntity.SendNetworkUpdate();
				arg.ReplyWith("Reset scale");
				break;
			}
			Vector3 one = Vector3.one;
			if (float.TryParse(string3, out var result2))
			{
				one = new Vector3(result2, result2, result2);
			}
			else
			{
				one = Vector3Ex.Parse(string3);
				if (one == Vector3.zero)
				{
					arg.ReplyWith(string3 + " is not a valid scale");
					break;
				}
			}
			baseEntity.networkEntityScale = true;
			baseEntity.transform.localScale = one;
			baseEntity.SendNetworkUpdate();
			arg.ReplyWith($"Set scale to {baseEntity.transform.localScale}");
			break;
		}
		case "settime":
		{
			int @int = arg.GetInt(2, -1);
			if (@int == -1)
			{
				arg.ReplyWith("Time not provided");
			}
			else if (baseEntity is WipeLaptopEntity wipeLaptopEntity)
			{
				wipeLaptopEntity.SetTimeLeft(@int);
				arg.ReplyWith($"Set time left to {@int}");
			}
			else
			{
				arg.ReplyWith("Not looking at a laptop");
			}
			break;
		}
		default:
			arg.ReplyWith("Unknown command");
			break;
		}
	}

	private static string AuthList(BaseEntity ent)
	{
		List<ulong> list;
		if (!(ent is BuildingPrivlidge buildingPrivlidge))
		{
			if (!(ent is AutoTurret autoTurret))
			{
				if (ent is CodeLock codeLock)
				{
					return CodeLockAuthList(codeLock);
				}
				if (!(ent is KeyLock keyLock))
				{
					if (!(ent is LegacyShelter legacyShelter))
					{
						if (ent is BaseVehicleModule vehicleModule)
						{
							return CodeLockAuthList(vehicleModule);
						}
						if (!(ent is Tugboat tugboat))
						{
							if (!(ent is SteeringWheel steeringWheel))
							{
								return "Entity has no auth list";
							}
							list = new List<ulong>();
							PlayerBoatPrivilege componentInChildren = steeringWheel.GetComponentInChildren<PlayerBoatPrivilege>();
							if (componentInChildren != null)
							{
								foreach (ulong authorizedPlayer in componentInChildren.authorizedPlayers)
								{
									list.Add(authorizedPlayer);
								}
							}
						}
						else
						{
							list = new List<ulong>();
							VehiclePrivilege componentInChildren2 = tugboat.GetComponentInChildren<VehiclePrivilege>();
							if (componentInChildren2 != null)
							{
								foreach (ulong authorizedPlayer2 in componentInChildren2.authorizedPlayers)
								{
									list.Add(authorizedPlayer2);
								}
							}
						}
					}
					else
					{
						list = new List<ulong> { legacyShelter.OwnerID };
					}
				}
				else
				{
					list = new List<ulong> { keyLock.OwnerID };
				}
			}
			else
			{
				list = new List<ulong>();
				foreach (ulong authorizedPlayer3 in autoTurret.authorizedPlayers)
				{
					list.Add(authorizedPlayer3);
				}
			}
		}
		else
		{
			list = new List<ulong>();
			foreach (ulong authorizedPlayer4 in buildingPrivlidge.authorizedPlayers)
			{
				list.Add(authorizedPlayer4);
			}
		}
		if (list == null || list.Count == 0)
		{
			return "Nobody is authed to this entity";
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumn("steamID");
		textTable.AddColumn("username");
		foreach (ulong item in list)
		{
			textTable.AddRow(item.ToString(), GetPlayerName(item));
		}
		return textTable.ToString();
	}

	private static string CodeLockAuthList(CodeLock codeLock)
	{
		if (codeLock.whitelistPlayers.Count == 0 && codeLock.guestPlayers.Count == 0)
		{
			return "Nobody is authed to this entity";
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumn("steamID");
		textTable.AddColumn("username");
		textTable.AddColumn("isGuest");
		foreach (ulong whitelistPlayer in codeLock.whitelistPlayers)
		{
			textTable.AddRow(whitelistPlayer.ToString(), GetPlayerName(whitelistPlayer), "");
		}
		foreach (ulong guestPlayer in codeLock.guestPlayers)
		{
			textTable.AddRow(guestPlayer.ToString(), GetPlayerName(guestPlayer), "x");
		}
		return textTable.ToString();
	}

	private static string CodeLockAuthList(BaseVehicleModule vehicleModule)
	{
		if (!vehicleModule.IsOnAVehicle)
		{
			return "Nobody is authed to this entity";
		}
		ModularCar modularCar = vehicleModule.Vehicle as ModularCar;
		if (modularCar == null || !modularCar.IsLockable || modularCar.CarLock.WhitelistPlayers.Count == 0)
		{
			return "Nobody is authed to this entity";
		}
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.AddColumn("steamID");
		textTable.AddColumn("username");
		foreach (ulong whitelistPlayer in modularCar.CarLock.WhitelistPlayers)
		{
			textTable.AddRow(whitelistPlayer.ToString(), GetPlayerName(whitelistPlayer));
		}
		return textTable.ToString();
	}

	public static string GetPlayerName(ulong steamId)
	{
		BasePlayer basePlayer = BasePlayer.allPlayerList.FirstOrDefault((BasePlayer p) => (ulong)p.userID == steamId);
		string text;
		if (!(basePlayer != null))
		{
			text = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(steamId);
			if (text == null)
			{
				return "[unknown]";
			}
		}
		else
		{
			text = basePlayer.displayName;
		}
		return text;
	}

	public static string ChangeGrade(BaseEntity entity, int increaseBy = 0, int decreaseBy = 0, BuildingGrade.Enum targetGrade = BuildingGrade.Enum.None, ulong skin = 0uL, float radius = 0f)
	{
		if (entity as BuildingBlock == null)
		{
			return $"'{entity}' is not a building block";
		}
		int total = 0;
		RunInRadius(radius, entity, delegate(BuildingBlock block)
		{
			BuildingGrade.Enum grade = block.grade;
			if (targetGrade > BuildingGrade.Enum.None && targetGrade < BuildingGrade.Enum.Count)
			{
				grade = targetGrade;
			}
			else
			{
				grade = (BuildingGrade.Enum)Mathf.Min((int)(grade + increaseBy), 4);
				grade = (BuildingGrade.Enum)Mathf.Max((int)(grade - decreaseBy), 0);
			}
			if (grade != block.grade)
			{
				block.ChangeGradeAndSkin(targetGrade, skin);
				total++;
			}
		});
		return $"Upgraded/downgraded '{total}' building block(s)";
	}

	private static bool RunInRadius<T>(float radius, BaseEntity initial, Action<T> callback, Func<T, bool> filter = null, int layerMask = 2097152) where T : BaseEntity
	{
		List<T> obj = Facepunch.Pool.Get<List<T>>();
		radius = Mathf.Clamp(radius, 0f, 200f);
		if (radius > 0f)
		{
			global::Vis.Entities(initial.transform.position, radius, obj, layerMask);
		}
		else if (initial is T item)
		{
			obj.Add(item);
		}
		foreach (T item2 in obj)
		{
			if (!item2.isClient)
			{
				try
				{
					callback(item2);
				}
				catch (Exception arg)
				{
					Debug.LogError($"Exception while running callback in radius: {arg}");
					Facepunch.Pool.FreeUnmanaged(ref obj);
					return false;
				}
			}
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		return true;
	}

	[ServerVar(Help = "Get a list of players")]
	public static PlayerInfo[] playerlist(Arg arg)
	{
		bool showAddress = arg.Connection == null || arg.Connection.authLevel >= 2;
		List<PlayerInfo> list = BasePlayer.activePlayerList.Select(delegate(BasePlayer x)
		{
			PlayerInfo result = default(PlayerInfo);
			result.SteamID = x.UserIDString;
			result.OwnerSteamID = x.OwnerID.ToString();
			result.DisplayName = x.displayName;
			result.Ping = Network.Net.sv.GetAveragePing(x.net.connection);
			result.Address = (showAddress ? x.net.connection.ipaddress : string.Empty);
			result.EntityId = x.net.ID.Value;
			result.ConnectedSeconds = (int)x.net.connection.GetSecondsConnected();
			result.ViolationLevel = x.ViolationLevel;
			result.Health = x.Health();
			result.Position = x.transform.position;
			result.IsMuted = x.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute);
			result.TeamID = x.currentTeam;
			return result;
		}).ToList();
		if (showBotsInPlayerList)
		{
			foreach (BasePlayer bot in BasePlayer.bots)
			{
				if (!(bot == null) && !bot.IsDestroyed)
				{
					list.Add(new PlayerInfo
					{
						SteamID = bot.UserIDString,
						OwnerSteamID = bot.OwnerID.ToString(),
						DisplayName = bot.displayName,
						Ping = 0,
						Address = string.Empty,
						EntityId = ((bot.net != null) ? bot.net.ID.Value : 0),
						ConnectedSeconds = 0,
						ViolationLevel = bot.ViolationLevel,
						Health = bot.Health(),
						Position = bot.transform.position,
						IsMuted = false,
						TeamID = bot.currentTeam
					});
				}
			}
		}
		return list.ToArray();
	}

	[ServerVar(Help = "Get a list of player's IDs")]
	public static PlayerIDInfo[] playerlistids(Arg arg)
	{
		bool showAddress = arg.Connection == null || arg.Connection.authLevel >= 2;
		return BasePlayer.activePlayerList.Select(delegate(BasePlayer x)
		{
			PlayerIDInfo result = default(PlayerIDInfo);
			result.SteamID = x.UserIDString;
			result.OwnerSteamID = x.OwnerID.ToString();
			result.DisplayName = x.displayName;
			result.Address = (showAddress ? x.net.connection.ipaddress : string.Empty);
			result.EntityId = x.net.ID.Value;
			return result;
		}).ToArray();
	}

	[ServerVar(Help = "List of banned users")]
	public static ServerUsers.User[] Bans()
	{
		return ServerUsers.GetAll(ServerUsers.UserGroup.Banned).ToArray();
	}

	[ServerVar(Help = "Get a list of information about the server")]
	public static ServerInfoOutput ServerInfo()
	{
		ServerInfoOutput result = default(ServerInfoOutput);
		result.Hostname = Server.hostname;
		result.MaxPlayers = Server.maxplayers;
		result.Players = BasePlayer.activePlayerList.Count;
		result.Queued = SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued;
		result.Joining = SingletonComponent<ServerMgr>.Instance.connectionQueue.Joining;
		result.ReservedSlots = SingletonComponent<ServerMgr>.Instance.connectionQueue.ReservedCount;
		result.EntityCount = BaseNetworkable.serverEntities.Count;
		result.GameTime = ((TOD_Sky.Instance != null) ? TOD_Sky.Instance.Cycle.DateTime.ToString() : DateTime.UtcNow.ToString());
		result.Uptime = (int)UnityEngine.Time.realtimeSinceStartup;
		result.Map = Server.level;
		result.Framerate = Performance.report.frameRate;
		result.Memory = (int)Performance.report.memoryAllocations;
		result.MemoryUsageSystem = (int)Performance.report.memoryUsageSystem;
		result.Collections = (int)Performance.report.memoryCollections;
		result.NetworkIn = (int)((Network.Net.sv != null) ? Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesReceived_LastSecond) : 0);
		result.NetworkOut = (int)((Network.Net.sv != null) ? Network.Net.sv.GetStat(null, BaseNetwork.StatTypeLong.BytesSent_LastSecond) : 0);
		result.Restarting = SingletonComponent<ServerMgr>.Instance.Restarting;
		result.SaveCreatedTime = SaveRestore.SaveCreatedTime.ToString();
		result.Version = 2633;
		result.Protocol = Protocol.printable;
		return result;
	}

	[ServerVar(Help = "Get information about this build")]
	public static BuildInfo BuildInfo()
	{
		return Facepunch.BuildInfo.Current;
	}

	[ServerVar(Help = "(Generated) Triggers a full refresh of the admin UI by requesting the player list, server info, convars, and UGC list all at once")]
	public static void AdminUI_FullRefresh(Arg arg)
	{
		AdminUI_RequestPlayerList(arg);
		AdminUI_RequestServerInfo(arg);
		AdminUI_RequestServerConvars(arg);
		AdminUI_RequestUGCList(arg);
	}

	[ServerVar(Help = "(Generated) Server-side handler that serialises and sends the current player list to the requesting admin client for display in the admin UI")]
	public static void AdminUI_RequestPlayerList(Arg arg)
	{
		if (allowAdminUI)
		{
			ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceivePlayerList", JsonConvert.SerializeObject(playerlist(arg)));
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that serialises and sends current server info (name, players, FPS, etc.) to the requesting admin client")]
	public static void AdminUI_RequestServerInfo(Arg arg)
	{
		if (allowAdminUI)
		{
			ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveServerInfo", JsonConvert.SerializeObject(ServerInfo()));
		}
	}

	[ServerVar(Help = "(Generated) Server-side handler that collects all ServerAdmin+ShowInAdminUI convars and sends them to the admin client for editing via the admin UI")]
	public static void AdminUI_RequestServerConvars(Arg arg)
	{
		if (!allowAdminUI)
		{
			return;
		}
		List<ServerConvarInfo> obj = Facepunch.Pool.Get<List<ServerConvarInfo>>();
		Command[] all = Index.All;
		foreach (Command command in all)
		{
			if (command.Server && command.Variable && command.ServerAdmin && command.ShowInAdminUI && !command.RconOnly)
			{
				obj.Add(new ServerConvarInfo
				{
					FullName = command.FullName,
					Value = command.GetOveride?.Invoke(),
					Help = command.Description
				});
			}
		}
		ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveCommands", JsonConvert.SerializeObject(obj));
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Server-side handler that scans all entities for UGC content (images, patterns, vending names) and sends a serialised list to the admin client")]
	public static void AdminUI_RequestUGCList(Arg arg)
	{
		if (!allowAdminUI)
		{
			return;
		}
		List<ServerUGCInfo> obj = Facepunch.Pool.Get<List<ServerUGCInfo>>();
		uint[] array = null;
		ulong[] array2 = null;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (ObjectEx.IsUnityNull(serverEntity))
			{
				continue;
			}
			array = null;
			array2 = null;
			UGCType uGCType = UGCType.ImageJpg;
			string text = string.Empty;
			if (serverEntity.TryGetComponent<IUGCBrowserEntity>(out var component))
			{
				if (component.UgcEntity == null)
				{
					continue;
				}
				array = component.GetContentCRCs;
				array2 = component.EditingHistory.ToArray();
				uGCType = component.ContentType;
				text = component.ContentString;
			}
			bool flag = false;
			if (array != null)
			{
				uint[] array3 = array;
				for (int i = 0; i < array3.Length; i++)
				{
					if (array3[i] != 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (uGCType == UGCType.PatternBoomer)
			{
				flag = true;
				PatternFirework patternFirework = component as PatternFirework;
				if (patternFirework != null && patternFirework.Design == null)
				{
					flag = false;
				}
			}
			if (uGCType == UGCType.VendingMachine && !string.IsNullOrEmpty(text))
			{
				flag = true;
			}
			if (flag)
			{
				obj.Add(new ServerUGCInfo
				{
					entityId = serverEntity.net.ID.Value,
					crcs = array,
					contentType = uGCType,
					entityPrefabID = serverEntity.prefabID,
					shortPrefabName = serverEntity.ShortPrefabName,
					playerIds = array2,
					contentString = text
				});
			}
		}
		ConsoleNetwork.SendClientCommand(arg.Connection, "AdminUI_ReceiveUGCList", JsonConvert.SerializeObject(obj));
		Facepunch.Pool.FreeUnmanaged(ref obj);
	}

	[ServerVar(Help = "(Generated) Server-side handler that retrieves a specific UGC data blob by CRC, entity ID, and type and sends it to the requesting admin client")]
	public static void AdminUI_RequestUGCContent(Arg arg)
	{
		if (allowAdminUI && !(ArgEx.Player(arg) == null))
		{
			uint uInt = arg.GetUInt(0);
			NetworkableId entityID = ArgEx.GetEntityID(arg, 1);
			FileStorage.Type @int = (FileStorage.Type)arg.GetInt(2);
			uint uInt2 = arg.GetUInt(3);
			byte[] array = FileStorage.server.Get(uInt, @int, entityID, uInt2);
			if (array != null)
			{
				SendInfo sendInfo = new SendInfo(arg.Connection);
				sendInfo.channel = 2;
				sendInfo.method = SendMethod.Reliable;
				SendInfo sendInfo2 = sendInfo;
				ArgEx.Player(arg).ClientRPC(RpcTarget.SendInfo("AdminReceivedUGC", sendInfo2), uInt, (uint)array.Length, array, uInt2, (byte)@int);
			}
		}
	}

	[ServerVar(Help = "(Generated) Clears all UGC content (images, patterns) from the entity with the given network ID and notifies the IUGCBrowserEntity component")]
	public static void AdminUI_DeleteUGCContent(Arg arg)
	{
		if (!allowAdminUI)
		{
			return;
		}
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
		if (baseNetworkable != null)
		{
			FileStorage.server.RemoveAllByEntity(entityID);
			if (baseNetworkable.TryGetComponent<IUGCBrowserEntity>(out var component))
			{
				component.ClearContent();
			}
		}
	}

	[ServerVar(Help = "(Generated) Sends the firework pattern design data for the specified pattern firework entity to the requesting admin client")]
	public static void AdminUI_RequestFireworkPattern(Arg arg)
	{
		if (allowAdminUI)
		{
			NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
			if (baseNetworkable != null && baseNetworkable is PatternFirework { Design: not null } patternFirework)
			{
				SendInfo sendInfo = new SendInfo(arg.Connection);
				sendInfo.channel = 2;
				sendInfo.method = SendMethod.Reliable;
				SendInfo sendInfo2 = sendInfo;
				ArgEx.Player(arg).ClientRPC(RpcTarget.SendInfo("AdminReceivedPatternFirework", sendInfo2), entityID, patternFirework.Design.ToProtoBytes());
			}
		}
	}

	[ServerVar(Help = "(Generated) Clears all UGC content from a single entity by network ID; reports success or failure")]
	public static void clearugcentity(Arg arg)
	{
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
		if (baseNetworkable != null && baseNetworkable.TryGetComponent<IUGCBrowserEntity>(out var component))
		{
			component.ClearContent();
			arg.ReplyWith($"Cleared content on {baseNetworkable.ShortPrefabName}/{entityID}");
		}
		else
		{
			arg.ReplyWith($"Could not find UGC entity with id {entityID}");
		}
	}

	[ServerVar(Help = "(Generated) Clears UGC content from all entities within the given radius of a world position; reports how many entities were cleared")]
	public static void clearugcentitiesinrange(Arg arg)
	{
		Vector3 vector = arg.GetVector3(0);
		float @float = arg.GetFloat(1);
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity.TryGetComponent<IUGCBrowserEntity>(out var component) && Vector3.Distance(serverEntity.transform.position, vector) <= @float)
			{
				component.ClearContent();
				num++;
			}
		}
		arg.ReplyWith($"Cleared {num} UGC entities within {@float}m of {vector}");
	}

	[ServerVar(Help = "(Generated) Clears the custom name UGC from all vending machines whose content string contains the given search text (case/symbol insensitive)")]
	public static void clearVendingMachineNamesContaining(Arg arg)
	{
		string @string = arg.GetString(0);
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity.TryGetComponent<IUGCBrowserEntity>(out var component) && component.ContentType == UGCType.VendingMachine && component.ContentString.Contains(@string, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols))
			{
				component.ClearContent();
				num++;
			}
		}
		arg.ReplyWith($"Cleared {num} vending machines containing {@string}");
	}

	[ServerVar(Help = "(Generated) Clears UGC content from all entities that have the specified player (by name or Steam ID) in their editing history")]
	public static void clearUGCByPlayer(Arg arg)
	{
		BasePlayer playerOrSleeper = ArgEx.GetPlayerOrSleeper(arg, 0);
		ulong num = ((playerOrSleeper == null) ? arg.GetULong(0, 0uL) : playerOrSleeper.userID.Get());
		int num2 = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			if (serverEntity.TryGetComponent<IUGCBrowserEntity>(out var component) && component.EditingHistory.Contains(num))
			{
				component.ClearContent();
				num2++;
			}
		}
		arg.ReplyWith($"Cleared {num2} UGC entities modified by {((playerOrSleeper != null) ? playerOrSleeper.displayName : ((object)num))}");
	}

	[ServerVar(Help = "(Generated) Returns a JSON object containing the UGC info (CRCs, type, player history) for the entity with the given network ID")]
	public static void getugcinfo(Arg arg)
	{
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(entityID);
		if (baseNetworkable != null && baseNetworkable.TryGetComponent<IUGCBrowserEntity>(out var component) && component.UgcEntity != null)
		{
			ServerUGCInfo serverUGCInfo = new ServerUGCInfo(component);
			arg.ReplyWith(JsonConvert.SerializeObject(serverUGCInfo));
		}
		else
		{
			arg.ReplyWith($"Invalid entity id: {entityID}");
		}
	}

	[ServerVar(Help = "Returns all entities that the provided player is authed to (TC's, locks, etc), supports --json")]
	public static void authcount(Arg arg)
	{
		ulong num = ArgEx.GetPlayerOrSleeper(arg, 0)?.userID ?? ((EncryptedValue<ulong>)0uL);
		if (num == 0L)
		{
			num = arg.GetULong(0, 0uL);
		}
		if (!SingletonComponent<ServerMgr>.Instance.persistance.DoesPlayerExist(num))
		{
			arg.ReplyWith("Please provide a valid player, unable to find '" + arg.GetString(0) + "'");
			return;
		}
		string playerName = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(num);
		string text = arg.GetString(1);
		if (text == "--json")
		{
			text = string.Empty;
		}
		List<EntityAssociation> obj = Facepunch.Pool.Get<List<EntityAssociation>>();
		FindEntityAssociationsForPlayer(num, useOwnerId: false, useAuth: true, text, obj);
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.AddColumns("Prefab name", "Position", "ID", "Type");
		foreach (EntityAssociation item in obj)
		{
			textTable.AddRow(item.TargetEntity.ShortPrefabName, item.TargetEntity.transform.position.ToString(), item.TargetEntity.net.ID.ToString(), item.AssociationType.ToString());
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		if (flag)
		{
			arg.ReplyWith(textTable.ToJson());
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Found entities " + playerName + " is authed to");
		stringBuilder.AppendLine(textTable.ToString());
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "Returns all entities that the provided player has placed, supports --json")]
	public static void entcount(Arg arg)
	{
		ulong num = ArgEx.GetPlayerOrSleeper(arg, 0)?.userID ?? ((EncryptedValue<ulong>)0uL);
		if (num == 0L)
		{
			num = arg.GetULong(0, 0uL);
		}
		if (!SingletonComponent<ServerMgr>.Instance.persistance.DoesPlayerExist(num))
		{
			arg.ReplyWith("Please provide a valid player, unable to find '" + arg.GetString(0) + "'");
			return;
		}
		string playerName = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(num);
		string text = arg.GetString(1);
		if (text == "--json")
		{
			text = string.Empty;
		}
		List<EntityAssociation> obj = Facepunch.Pool.Get<List<EntityAssociation>>();
		FindEntityAssociationsForPlayer(num, useOwnerId: true, useAuth: false, text, obj);
		bool flag = arg.HasArg("--json");
		using TextTable textTable = Facepunch.Pool.Get<TextTable>();
		textTable.ShouldPadColumns = !flag;
		textTable.AddColumns("Prefab name", "Position", "ID");
		foreach (EntityAssociation item in obj)
		{
			textTable.AddRow(item.TargetEntity.ShortPrefabName, item.TargetEntity.transform.position.ToString(), item.TargetEntity.net.ID.ToString());
		}
		Facepunch.Pool.FreeUnmanaged(ref obj);
		if (flag)
		{
			arg.ReplyWith(textTable.ToJson());
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Found entities associated with " + playerName);
		stringBuilder.AppendLine(textTable.ToString());
		arg.ReplyWith(stringBuilder.ToString());
	}

	private static void FindEntityAssociationsForPlayer(ulong steamId, bool useOwnerId, bool useAuth, string filter, List<EntityAssociation> results)
	{
		results.Clear();
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			EntityAssociationType entityAssociationType = EntityAssociationType.Owner;
			if (!(serverEntity is BaseEntity baseEntity))
			{
				continue;
			}
			bool flag = false;
			if (useOwnerId && baseEntity.OwnerID == steamId)
			{
				flag = true;
			}
			if (useAuth && !flag)
			{
				if (!flag && baseEntity is BuildingPrivlidge buildingPrivlidge && buildingPrivlidge.IsAuthed(steamId))
				{
					flag = true;
				}
				if (!flag && baseEntity is SimplePrivilege simplePrivilege && simplePrivilege.IsAuthed(steamId))
				{
					flag = true;
				}
				if (!flag && baseEntity is KeyLock keyLock && keyLock.OwnerID == steamId)
				{
					flag = true;
				}
				else if (baseEntity is CodeLock codeLock)
				{
					if (codeLock.whitelistPlayers.Contains(steamId))
					{
						flag = true;
					}
					else if (codeLock.guestPlayers.Contains(steamId))
					{
						flag = true;
						entityAssociationType = EntityAssociationType.LockGuest;
					}
				}
				if (!flag && baseEntity is ModularCar { IsLockable: not false } modularCar && modularCar.CarLock.HasLockPermission(steamId))
				{
					flag = true;
				}
				if (!flag && baseEntity is AutoTurret autoTurret && autoTurret.IsAuthed(steamId))
				{
					flag = true;
				}
				if (flag && entityAssociationType == EntityAssociationType.Owner)
				{
					entityAssociationType = EntityAssociationType.Auth;
				}
			}
			if (flag && !string.IsNullOrEmpty(filter) && !serverEntity.ShortPrefabName.Contains(filter, CompareOptions.IgnoreCase))
			{
				flag = false;
			}
			if (flag)
			{
				results.Add(new EntityAssociation
				{
					TargetEntity = baseEntity,
					AssociationType = entityAssociationType
				});
			}
		}
	}
}

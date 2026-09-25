using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace ConVar;

[Factory("territory")]
public class Territory : ConsoleSystem
{
	private class TerritoryState
	{
		public bool exists;

		public float hexSize;

		public float offsetX;

		public float offsetZ;

		public int gridWidth;

		public int gridHeight;

		public int totalCells;

		public int ownedCells;

		public List<FactionState> factions;
	}

	private class FactionState
	{
		public int index;

		public string name;

		public string color;

		public int cells;

		public float share;

		public int regions;

		public int largestRegion;
	}

	[ClientVar(Help = "Preview every territory grid tile on the map, owned or not. Requires the grid to exist on the server")]
	public static bool preview = false;

	private static float _hexsize = 50f;

	private static float _offsetx;

	private static float _offsetz;

	private const string PrefabPath = "assets/bundled/prefabs/twitchevents/territory_zones.prefab";

	[ServerVar(Help = "Hex cell size in metres (centre to corner). Changing it rebuilds the grid and wipes ownership")]
	public static float hexsize
	{
		get
		{
			return _hexsize;
		}
		set
		{
			_hexsize = value;
			PointEntity<TerritoryZoneController>.ServerInstance?.EnsureGrid(_hexsize);
		}
	}

	[ServerVar(Help = "Shifts the whole territory grid east/west in metres. Painted cells move with it")]
	public static float offsetx
	{
		get
		{
			return _offsetx;
		}
		set
		{
			_offsetx = value;
			PointEntity<TerritoryZoneController>.ServerInstance?.SetGridOffset(new Vector2(_offsetx, _offsetz));
		}
	}

	[ServerVar(Help = "Shifts the whole territory grid north/south in metres. Painted cells move with it")]
	public static float offsetz
	{
		get
		{
			return _offsetz;
		}
		set
		{
			_offsetz = value;
			PointEntity<TerritoryZoneController>.ServerInstance?.SetGridOffset(new Vector2(_offsetx, _offsetz));
		}
	}

	public static void SyncFromController(TerritoryZoneController controller)
	{
		_hexsize = controller.HexSize;
		_offsetx = controller.GridOffset.x;
		_offsetz = controller.GridOffset.y;
	}

	private static TerritoryZoneController GetOrCreate()
	{
		TerritoryZoneController territoryZoneController = PointEntity<TerritoryZoneController>.ServerInstance;
		if (territoryZoneController == null)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/bundled/prefabs/twitchevents/territory_zones.prefab", Vector3.zero);
			if (baseEntity == null)
			{
				return null;
			}
			baseEntity.Spawn();
			territoryZoneController = baseEntity as TerritoryZoneController;
		}
		if (territoryZoneController != null)
		{
			territoryZoneController.EnsureGrid(hexsize);
			territoryZoneController.SetGridOffset(new Vector2(_offsetx, _offsetz));
		}
		return territoryZoneController;
	}

	private static int ParseFaction(TerritoryZoneController controller, string arg)
	{
		if (string.Equals(arg, "none", StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}
		int num = controller.FindFaction(arg);
		if (num >= 0)
		{
			return num;
		}
		if (!int.TryParse(arg, out var result))
		{
			return -1;
		}
		return result;
	}

	[ServerVar(Help = "Create a faction, or recolour an existing one: territory.createfaction <name> <html colour, e.g. #FF0000 or #FF0000B0>")]
	public static void createfaction(Arg arg)
	{
		TerritoryZoneController orCreate = GetOrCreate();
		if (!(orCreate == null))
		{
			string @string = arg.GetString(0);
			if (string.IsNullOrEmpty(@string) || int.TryParse(@string, out var _))
			{
				arg.ReplyWith("Faction name must be a non-numeric string");
				return;
			}
			if (!ColorUtility.TryParseHtmlString(arg.GetString(1), out var color))
			{
				arg.ReplyWith("Couldn't parse colour");
				return;
			}
			int num = orCreate.CreateFaction(@string, color);
			arg.ReplyWith((num >= 0) ? $"Faction '{@string}' is index {num}" : $"No free faction slots (max {31})");
		}
	}

	[ServerVar(Help = "Returns a JSON report of the territory grid: dimensions, offsets and a per-faction breakdown (cells held, share of claimed land, region counts) sorted by holdings")]
	public static void state(Arg arg)
	{
		TerritoryZoneController serverInstance = PointEntity<TerritoryZoneController>.ServerInstance;
		if (serverInstance == null || serverInstance.CellFactions == null)
		{
			arg.ReplyWith(JsonConvert.SerializeObject(new TerritoryState
			{
				exists = false
			}));
			return;
		}
		int[] array = new int[32];
		int num = 0;
		byte[] cellFactions = serverInstance.CellFactions;
		foreach (byte b in cellFactions)
		{
			if (b != 0)
			{
				array[b]++;
				num++;
			}
		}
		List<FactionState> list = new List<FactionState>();
		for (int j = 1; j < 32; j++)
		{
			if (array[j] != 0 || !string.IsNullOrEmpty(serverInstance.FactionNames[j]))
			{
				int largestRegion;
				int regions = serverInstance.CountRegions(j, out largestRegion);
				list.Add(new FactionState
				{
					index = j,
					name = serverInstance.FactionNames[j],
					color = "#" + ColorUtility.ToHtmlStringRGBA((Color)serverInstance.GetFactionColor(j)),
					cells = array[j],
					share = ((num > 0) ? ((float)array[j] / (float)num) : 0f),
					regions = regions,
					largestRegion = largestRegion
				});
			}
		}
		TerritoryState value = new TerritoryState
		{
			exists = true,
			hexSize = serverInstance.HexSize,
			offsetX = serverInstance.GridOffset.x,
			offsetZ = serverInstance.GridOffset.y,
			gridWidth = HexGridLayout.Width(serverInstance.HexSize),
			gridHeight = HexGridLayout.Height(serverInstance.HexSize),
			totalCells = serverInstance.CellFactions.Length,
			ownedCells = num,
			factions = list.OrderByDescending((FactionState f) => f.cells).ToList()
		};
		arg.ReplyWith(JsonConvert.SerializeObject(value, Formatting.Indented));
	}

	[ServerVar(Help = "List created factions and their colours")]
	public static void factions(Arg arg)
	{
		TerritoryZoneController serverInstance = PointEntity<TerritoryZoneController>.ServerInstance;
		if (serverInstance == null)
		{
			arg.ReplyWith("No territory grid");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i < serverInstance.FactionNames.Length; i++)
		{
			if (!string.IsNullOrEmpty(serverInstance.FactionNames[i]))
			{
				stringBuilder.AppendLine($"{i}: {serverInstance.FactionNames[i]} #{ColorUtility.ToHtmlStringRGBA((Color)serverInstance.GetFactionColor(i))}");
			}
		}
		arg.ReplyWith((stringBuilder.Length > 0) ? stringBuilder.ToString() : "No factions created");
	}

	[ServerVar(Help = "Set the hex cell at your position: territory.sethere <faction name|none>")]
	public static void sethere(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			arg.ReplyWith("Must be called by a player");
			return;
		}
		TerritoryZoneController orCreate = GetOrCreate();
		if (!(orCreate == null))
		{
			int num = ParseFaction(orCreate, arg.GetString(0));
			if (num < 0)
			{
				arg.ReplyWith("Unknown faction '" + arg.GetString(0) + "' - create it with territory.createfaction");
			}
			else
			{
				arg.ReplyWith(orCreate.SetCellAt(basePlayer.transform.position, num) ? $"Set cell {HexGridLayout.WorldToCell(basePlayer.transform.position, orCreate.HexSize, orCreate.GridOffset)} to faction {num}" : "Failed - cell out of range or invalid faction");
			}
		}
	}

	[ServerVar(Help = "Set the hex cell containing a world position: territory.setat <x> <z> <faction name|none>")]
	public static void setat(Arg arg)
	{
		TerritoryZoneController orCreate = GetOrCreate();
		if (!(orCreate == null))
		{
			Vector3 worldPos = new Vector3(arg.GetFloat(0), 0f, arg.GetFloat(1));
			int num = ParseFaction(orCreate, arg.GetString(2));
			if (num < 0)
			{
				arg.ReplyWith("Unknown faction '" + arg.GetString(2) + "' - create it with territory.createfaction");
			}
			else
			{
				arg.ReplyWith(orCreate.SetCellAt(worldPos, num) ? $"Set cell {HexGridLayout.WorldToCell(worldPos, orCreate.HexSize, orCreate.GridOffset)} to faction {num}" : "Failed - cell out of range or invalid faction");
			}
		}
	}

	[ServerVar(Help = "Set a hex cell by index: territory.setcell <cell> <faction name|none>")]
	public static void setcell(Arg arg)
	{
		TerritoryZoneController orCreate = GetOrCreate();
		if (!(orCreate == null))
		{
			int @int = arg.GetInt(0);
			int num = ParseFaction(orCreate, arg.GetString(1));
			if (num < 0)
			{
				arg.ReplyWith("Unknown faction '" + arg.GetString(1) + "' - create it with territory.createfaction");
			}
			else
			{
				arg.ReplyWith(orCreate.SetCell(@int, num) ? $"Set cell {@int} to faction {num}" : "Failed - cell out of range or invalid faction");
			}
		}
	}

	[ServerVar(Help = "Fill every hex cell: territory.fill <faction name|none>")]
	public static void fill(Arg arg)
	{
		TerritoryZoneController orCreate = GetOrCreate();
		if (!(orCreate == null))
		{
			int num = ParseFaction(orCreate, arg.GetString(0));
			if (num < 0)
			{
				arg.ReplyWith("Unknown faction '" + arg.GetString(0) + "' - create it with territory.createfaction");
			}
			else
			{
				orCreate.FillAll(num);
			}
		}
	}

	[ServerVar(Help = "Clear all territory ownership")]
	public static void clear(Arg arg)
	{
		GetOrCreate()?.FillAll(0);
	}

	[ServerVar(Help = "Remove the territory grid entirely")]
	public static void destroy(Arg arg)
	{
		TerritoryZoneController serverInstance = PointEntity<TerritoryZoneController>.ServerInstance;
		if (serverInstance != null)
		{
			serverInstance.Kill();
		}
	}
}

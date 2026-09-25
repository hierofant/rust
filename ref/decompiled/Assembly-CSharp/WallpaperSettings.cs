using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Wallpaper/Wallpaper Settings")]
public class WallpaperSettings : BaseScriptableObject
{
	public enum Category
	{
		Wall = 1,
		Floor = 2,
		Ceiling = 3,
		First = 1,
		Last = 3
	}

	[Flags]
	public enum Side
	{
		Soft = 1,
		Hard = 2
	}

	[Serializable]
	public struct WallpaperSetting
	{
		public GameObjectRef WallpaperPrefab;

		public BuildingBlock BuildingBlock;

		public Side side;
	}

	public WallpaperSetting[] Wallpapers;

	public WallpaperSetting[] Floorings;

	public WallpaperSetting[] Ceilings;

	public ItemAmount PlacementPrice;

	private static ItemDefinition _wallpaperItem;

	private static ItemDefinition _flooringItem;

	private static ItemDefinition _ceilingItem;

	public static ItemDefinition WallpaperItemDef
	{
		get
		{
			if (_wallpaperItem == null)
			{
				_wallpaperItem = ItemManager.FindItemDefinition("wallpaper.wall");
			}
			return _wallpaperItem;
		}
	}

	public static ItemDefinition FlooringItemDef
	{
		get
		{
			if (_flooringItem == null)
			{
				_flooringItem = ItemManager.FindItemDefinition("wallpaper.flooring");
			}
			return _flooringItem;
		}
	}

	public static ItemDefinition CeilingItemDef
	{
		get
		{
			if (_ceilingItem == null)
			{
				_ceilingItem = ItemManager.FindItemDefinition("wallpaper.ceiling");
			}
			return _ceilingItem;
		}
	}

	private (WallpaperSetting setting, Category category)? GetMatchingSetting(BuildingBlock buildingBlock, int side)
	{
		if (buildingBlock == null)
		{
			return null;
		}
		Side side2 = (Side)(side + 1);
		WallpaperSetting[] wallpapers = Wallpapers;
		for (int i = 0; i < wallpapers.Length; i++)
		{
			WallpaperSetting item = wallpapers[i];
			if (item.BuildingBlock != null && item.BuildingBlock.prefabID == buildingBlock.prefabID && (item.side & side2) != 0)
			{
				return (item, Category.Wall);
			}
		}
		wallpapers = Ceilings;
		for (int i = 0; i < wallpapers.Length; i++)
		{
			WallpaperSetting item2 = wallpapers[i];
			if (item2.BuildingBlock != null && item2.BuildingBlock.prefabID == buildingBlock.prefabID && (item2.side & side2) != 0)
			{
				return (item2, Category.Ceiling);
			}
		}
		wallpapers = Floorings;
		for (int i = 0; i < wallpapers.Length; i++)
		{
			WallpaperSetting item3 = wallpapers[i];
			if (item3.BuildingBlock != null && item3.BuildingBlock.prefabID == buildingBlock.prefabID && (item3.side & side2) != 0)
			{
				return (item3, Category.Floor);
			}
		}
		return null;
	}

	public Category GetCategory(BuildingBlock buildingBlock, int side)
	{
		(WallpaperSetting, Category)? matchingSetting = GetMatchingSetting(buildingBlock, side);
		if (!matchingSetting.HasValue)
		{
			return (Category)0;
		}
		return matchingSetting.Value.Item2;
	}

	public GameObjectRef GetWallpaperPrefab(BuildingBlock buildingBlock, int side)
	{
		(WallpaperSetting, Category)? matchingSetting = GetMatchingSetting(buildingBlock, side);
		if (!matchingSetting.HasValue)
		{
			return null;
		}
		return matchingSetting.Value.Item1.WallpaperPrefab;
	}

	public Construction GetConstruction(BuildingBlock buildingBlock, int side)
	{
		GameObjectRef wallpaperPrefab = GetWallpaperPrefab(buildingBlock, side);
		if (wallpaperPrefab == null)
		{
			return null;
		}
		if (buildingBlock.isServer)
		{
			return PrefabAttribute.server.Find<Construction>(wallpaperPrefab.resourceID);
		}
		return null;
	}

	public Deployable GetDeployable(BuildingBlock buildingBlock)
	{
		GameObjectRef wallpaperPrefab = GetWallpaperPrefab(buildingBlock, 0);
		if (wallpaperPrefab == null)
		{
			return null;
		}
		if (buildingBlock.isServer)
		{
			return PrefabAttribute.server.Find<Deployable>(wallpaperPrefab.resourceID);
		}
		return null;
	}

	public bool CanUseWallpaper(BuildingBlock buildingBlock)
	{
		if (GetWallpaperPrefab(buildingBlock, 0) == null)
		{
			return GetWallpaperPrefab(buildingBlock, 1) != null;
		}
		return true;
	}

	public static ItemDefinition GetItemDefForCategory(Category category)
	{
		return category switch
		{
			Category.Wall => WallpaperItemDef, 
			Category.Floor => FlooringItemDef, 
			Category.Ceiling => CeilingItemDef, 
			_ => null, 
		};
	}

	public ItemDefinition GetWallpaperItem(BuildingBlock buildingBlock, int side)
	{
		(WallpaperSetting, Category)? matchingSetting = GetMatchingSetting(buildingBlock, side);
		if (!matchingSetting.HasValue)
		{
			return null;
		}
		return GetItemDefForCategory(matchingSetting.Value.Item2);
	}

	public int GetSideThatMustBeInside(BuildingBlock buildingBlock)
	{
		string prefabName = buildingBlock.PrefabName;
		if (prefabName.Contains("floor"))
		{
			return 1;
		}
		if (prefabName.Contains("wall"))
		{
			return 1;
		}
		return -1;
	}
}

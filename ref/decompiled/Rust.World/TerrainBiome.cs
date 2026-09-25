using System;
using System.Collections.Generic;

public static class TerrainBiome
{
	[Flags]
	public enum Enum
	{
		Arid = 1,
		Temperate = 2,
		Tundra = 4,
		Arctic = 8,
		Jungle = 0x10,
		DeepSea = 0x20
	}

	public const int COUNT = 5;

	public const int EVERYTHING = -1;

	public const int NOTHING = 0;

	public const int ARID = 1;

	public const int TEMPERATE = 2;

	public const int TUNDRA = 4;

	public const int ARCTIC = 8;

	public const int JUNGLE = 16;

	public const int DEEPSEA = 32;

	public const int ARID_IDX = 0;

	public const int TEMPERATE_IDX = 1;

	public const int TUNDRA_IDX = 2;

	public const int ARCTIC_IDX = 3;

	public const int JUNGLE_IDX = 4;

	public const int DEEPSEA_IDX = 5;

	private static Dictionary<int, int> type2index = new Dictionary<int, int>
	{
		{ 1, 0 },
		{ 2, 1 },
		{ 4, 2 },
		{ 8, 3 },
		{ 16, 4 },
		{ 32, 5 }
	};

	public static int TypeToIndex(int id)
	{
		return type2index[id];
	}

	public static int IndexToType(int idx)
	{
		return 1 << idx;
	}
}

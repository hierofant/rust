using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ServerOcclusionJobs;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Algorithm
{
	public static bool Trace(int3 from, int3 to, in GridDefinition gridDef, int blockedGridThreshold, int neighbourThreshold, bool useNeighbourThresholds)
	{
		int num = 0;
		int neighboursChecked = 0;
		int num2 = from.x;
		int num3 = from.y;
		int num4 = from.z;
		int x = to.x;
		int y = to.y;
		int z = to.z;
		int num5 = x - from.x;
		int num6 = y - from.y;
		int num7 = z - from.z;
		int num8 = Mathf.Abs(num5);
		int num9 = Mathf.Abs(num6);
		int num10 = Mathf.Abs(num7);
		int num11 = num8 << 1;
		int num12 = num9 << 1;
		int num13 = num10 << 1;
		int num14 = ((num5 >= 0) ? 1 : (-1));
		int num15 = ((num6 >= 0) ? 1 : (-1));
		int num16 = ((num7 >= 0) ? 1 : (-1));
		int3 nStep = -math.int3(num14, num15, num16);
		if (num8 >= num9 && num8 >= num10)
		{
			int num17 = num12 - num8;
			int num18 = num13 - num8;
			for (int i = 0; i < num8; i++)
			{
				if (!AddToGridArea(new int3(num2, num3, num4), in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold) && ++num > blockedGridThreshold)
				{
					return true;
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num11;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num11;
				}
				num17 += num12;
				num18 += num13;
				num2 += num14;
			}
		}
		else if (num9 >= num8 && num9 >= num10)
		{
			int num17 = num11 - num9;
			int num18 = num13 - num9;
			for (int j = 0; j < num9; j++)
			{
				if (!AddToGridArea(new int3(num2, num3, num4), in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold) && ++num > blockedGridThreshold)
				{
					return true;
				}
				if (num17 > 0)
				{
					num2 += num14;
					num17 -= num12;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num12;
				}
				num17 += num11;
				num18 += num13;
				num3 += num15;
			}
		}
		else
		{
			int num17 = num12 - num10;
			int num18 = num11 - num10;
			for (int k = 0; k < num10; k++)
			{
				if (!AddToGridArea(new int3(num2, num3, num4), in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold) && ++num > blockedGridThreshold)
				{
					return true;
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num13;
				}
				if (num18 > 0)
				{
					num2 += num14;
					num18 -= num13;
				}
				num17 += num12;
				num18 += num11;
				num4 += num16;
			}
		}
		return false;
	}

	public static bool Gather(int3 from, int3 to, in GridDefinition gridDef, int blockedGridThreshold, int neighbourThreshold, bool useNeighbourThresholds, NativeList<(int3, Color)> cells)
	{
		int num = 0;
		int neighboursChecked = 0;
		int num2 = from.x;
		int num3 = from.y;
		int num4 = from.z;
		int x = to.x;
		int y = to.y;
		int z = to.z;
		int num5 = x - from.x;
		int num6 = y - from.y;
		int num7 = z - from.z;
		int num8 = Mathf.Abs(num5);
		int num9 = Mathf.Abs(num6);
		int num10 = Mathf.Abs(num7);
		int num11 = num8 << 1;
		int num12 = num9 << 1;
		int num13 = num10 << 1;
		int num14 = ((num5 >= 0) ? 1 : (-1));
		int num15 = ((num6 >= 0) ? 1 : (-1));
		int num16 = ((num7 >= 0) ? 1 : (-1));
		int3 nStep = -math.int3(num14, num15, num16);
		if (num8 >= num9 && num8 >= num10)
		{
			int num17 = num12 - num8;
			int num18 = num13 - num8;
			for (int i = 0; i < num8; i++)
			{
				int3 @int = new int3(num2, num3, num4);
				if (!AddToGridArea(@int, in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold, cells))
				{
					(int3, Color) value = (@int, Color.red);
					cells.Add(in value);
					if (++num > blockedGridThreshold)
					{
						return true;
					}
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num11;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num11;
				}
				num17 += num12;
				num18 += num13;
				num2 += num14;
			}
		}
		else if (num9 >= num8 && num9 >= num10)
		{
			int num17 = num11 - num9;
			int num18 = num13 - num9;
			for (int j = 0; j < num9; j++)
			{
				int3 int2 = new int3(num2, num3, num4);
				if (!AddToGridArea(int2, in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold, cells))
				{
					(int3, Color) value = (int2, Color.red);
					cells.Add(in value);
					if (++num > blockedGridThreshold)
					{
						return true;
					}
				}
				if (num17 > 0)
				{
					num2 += num14;
					num17 -= num12;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num12;
				}
				num17 += num11;
				num18 += num13;
				num3 += num15;
			}
		}
		else
		{
			int num17 = num12 - num10;
			int num18 = num11 - num10;
			for (int k = 0; k < num10; k++)
			{
				int3 int3 = new int3(num2, num3, num4);
				if (!AddToGridArea(int3, in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold, cells))
				{
					(int3, Color) value = (int3, Color.red);
					cells.Add(in value);
					if (++num > blockedGridThreshold)
					{
						return true;
					}
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num13;
				}
				if (num18 > 0)
				{
					num2 += num14;
					num18 -= num13;
				}
				num17 += num12;
				num18 += num11;
				num4 += num16;
			}
		}
		return false;
	}

	private static bool NeighbourBlockedOneAxis(in GridDefinition grid, int3 cell, int xDir, int yDir, int zDir)
	{
		if (xDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z + 1), new int3(cell.x, cell.y, cell.z + 1), new int3(cell.x + xDir, cell.y, cell.z + 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z - 1), new int3(cell.x, cell.y, cell.z - 1), new int3(cell.x + xDir, cell.y, cell.z - 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y + 1, cell.z), new int3(cell.x, cell.y + 1, cell.z), new int3(cell.x + xDir, cell.y + 1, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - 1, cell.z), new int3(cell.x, cell.y - 1, cell.z), new int3(cell.x + xDir, cell.y - 1, cell.z)))
			{
				return false;
			}
		}
		else if (yDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - 1, cell.y - yDir, cell.z), new int3(cell.x - 1, cell.y, cell.z), new int3(cell.x - 1, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x + 1, cell.y - yDir, cell.z), new int3(cell.x + 1, cell.y, cell.z), new int3(cell.x + 1, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z - 1), new int3(cell.x, cell.y, cell.z - 1), new int3(cell.x, cell.y + yDir, cell.z - 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z + 1), new int3(cell.x, cell.y, cell.z + 1), new int3(cell.x, cell.y + yDir, cell.z + 1)))
			{
				return false;
			}
		}
		else
		{
			if (!AreBlocked(in grid, new int3(cell.x - 1, cell.y, cell.z - zDir), new int3(cell.x - 1, cell.y, cell.z), new int3(cell.x - 1, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x + 1, cell.y, cell.z - zDir), new int3(cell.x + 1, cell.y, cell.z), new int3(cell.x + 1, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - 1, cell.z - zDir), new int3(cell.x, cell.y - 1, cell.z), new int3(cell.x, cell.y - 1, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y + 1, cell.z - zDir), new int3(cell.x, cell.y + 1, cell.z), new int3(cell.x, cell.y + 1, cell.z + zDir)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool NeighbourBlockedTwoAxis(in GridDefinition grid, int3 cell, int xDir, int yDir, int zDir)
	{
		if (xDir != 0 && zDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z), new int3(cell.x - xDir, cell.y, cell.z + zDir), new int3(cell.x, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y + 1, cell.z - zDir), new int3(cell.x, cell.y + 1, cell.z), new int3(cell.x + xDir, cell.y + 1, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - 1, cell.z - zDir), new int3(cell.x, cell.y - 1, cell.z), new int3(cell.x + xDir, cell.y - 1, cell.z + zDir)))
			{
				return false;
			}
		}
		else if (xDir != 0 && yDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z), new int3(cell.x - xDir, cell.y + yDir, cell.z), new int3(cell.x, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - yDir, cell.z + 1), new int3(cell.x, cell.y, cell.z + 1), new int3(cell.x + xDir, cell.y + yDir, cell.z + 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z), new int3(cell.x + xDir, cell.y - yDir, cell.z), new int3(cell.x + xDir, cell.y, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - yDir, cell.z - 1), new int3(cell.x, cell.y, cell.z - 1), new int3(cell.x + xDir, cell.y + yDir, cell.z - 1)))
			{
				return false;
			}
		}
		else
		{
			if (!AreBlocked(in grid, new int3(cell.x, cell.y, cell.z - zDir), new int3(cell.x, cell.y + yDir, cell.z - zDir), new int3(cell.x, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x + 1, cell.y - yDir, cell.z - zDir), new int3(cell.x + 1, cell.y, cell.z), new int3(cell.x + 1, cell.y + yDir, cell.z - zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z), new int3(cell.x, cell.y - yDir, cell.z + zDir), new int3(cell.x, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - 1, cell.y - yDir, cell.z - zDir), new int3(cell.x - 1, cell.y, cell.z), new int3(cell.x - 1, cell.y + yDir, cell.z - zDir)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool NeighbourBlockedThreeAxis(in GridDefinition grid, int3 cell, int xDir, int yDir, int zDir)
	{
		if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z), new int3(cell.x - xDir, cell.y, cell.z + zDir), new int3(cell.x, cell.y, cell.z + zDir)))
		{
			return false;
		}
		if (!AreBlocked(in grid, new int3(cell.x, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z)))
		{
			return false;
		}
		if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z - zDir), new int3(cell.x - xDir, cell.y + yDir, cell.z - zDir), new int3(cell.x, cell.y + yDir, cell.z)))
		{
			return false;
		}
		if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z), new int3(cell.x + xDir, cell.y - yDir, cell.z + zDir), new int3(cell.x + xDir, cell.y, cell.z + zDir)))
		{
			return false;
		}
		return true;
	}

	private static bool AreBlocked(in GridDefinition grid, int3 p1, int3 p2, int3 p3)
	{
		if (grid.IsValidSubGrid(p1) && grid.IsBlocked(p1))
		{
			return true;
		}
		if (grid.IsValidSubGrid(p2) && grid.IsBlocked(p2))
		{
			return true;
		}
		if (grid.IsValidSubGrid(p3) && grid.IsBlocked(p3))
		{
			return true;
		}
		return false;
	}

	private static bool AddNeighbours(int3 cell, in GridDefinition grid, int3 nStep)
	{
		return (((nStep.x != 0) ? 1 : 0) + ((nStep.y != 0) ? 1 : 0) + ((nStep.z != 0) ? 1 : 0)) switch
		{
			1 => !NeighbourBlockedOneAxis(in grid, cell, -nStep.x, -nStep.y, -nStep.z), 
			2 => !NeighbourBlockedTwoAxis(in grid, cell, -nStep.x, -nStep.y, -nStep.z), 
			3 => !NeighbourBlockedThreeAxis(in grid, cell, -nStep.x, -nStep.y, -nStep.z), 
			_ => true, 
		};
	}

	private static bool AddToGridArea(int3 cell, in GridDefinition grid, int3 nStep, ref int neighboursChecked, bool useNeighbourThresholds, int neighbourThreshold)
	{
		if (!grid.IsBlocked(cell))
		{
			return true;
		}
		if (!useNeighbourThresholds || ++neighboursChecked <= neighbourThreshold)
		{
			return AddNeighbours(cell, in grid, nStep);
		}
		return false;
	}

	private static bool AddToGridArea(int3 cell, in GridDefinition grid, int3 nStep, ref int neighboursChecked, bool useNeighbourThresholds, int neighbourThreshold, NativeList<(int3, Color)> cells)
	{
		if (!grid.IsBlocked(cell))
		{
			(int3, Color) value = (cell, Color.green);
			cells.Add(in value);
			return true;
		}
		if ((!useNeighbourThresholds || ++neighboursChecked <= neighbourThreshold) && AddNeighbours(cell, in grid, nStep))
		{
			(int3, Color) value = (cell, Color.yellow);
			cells.Add(in value);
			return true;
		}
		return false;
	}
}

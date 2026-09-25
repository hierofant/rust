using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace ServerOcclusionJobs;

public struct GridDefinition
{
	[NativeDisableContainerSafetyRestriction]
	public NativeArray<NativeBitArray>.ReadOnly OcclusionSubGridBlocked;

	public int3 ChunkCount;

	public int3 SubChunkCount;

	public bool IsValidGrid(int3 p)
	{
		if (p.x < 0 || p.y < 0 || p.z < 0)
		{
			return false;
		}
		if (p.x >= ChunkCount.x || p.y >= ChunkCount.y || p.z >= ChunkCount.z)
		{
			return false;
		}
		return true;
	}

	public bool IsValidSubGrid(int3 p)
	{
		if (p.x < 0 || p.y < 0 || p.z < 0)
		{
			return false;
		}
		if (p.x >= SubChunkCount.x || p.y >= SubChunkCount.y || p.z >= SubChunkCount.z)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetGridIndex(int x, int y, int z)
	{
		return z * ChunkCount.x * ChunkCount.y + y * ChunkCount.z + x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsBlocked(int3 p)
	{
		return IsBlocked(p.x, p.y, p.z);
	}

	private bool IsBlocked(int x, int y, int z)
	{
		int result;
		int x2 = Math.DivRem(x, 8, out result);
		int result2;
		int y2 = Math.DivRem(y, 8, out result2);
		int result3;
		int z2 = Math.DivRem(z, 8, out result3);
		int gridIndex = GetGridIndex(x2, y2, z2);
		NativeBitArray nativeBitArray = (IsValidGrid(math.int3(x2, y2, z2)) ? OcclusionSubGridBlocked[gridIndex] : default(NativeBitArray));
		int pos = result3 * 8 * 8 + result2 * 8 + result;
		if (nativeBitArray.IsCreated)
		{
			return nativeBitArray.IsSet(pos);
		}
		return false;
	}
}

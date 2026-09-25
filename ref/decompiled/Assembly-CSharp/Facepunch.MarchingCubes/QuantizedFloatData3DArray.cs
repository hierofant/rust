using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

[GenerateTestsForBurstCompatibility]
public struct QuantizedFloatData3DArray : IDisposable
{
	public const float MaxValue = 2.5f;

	public const float InvMaxValue = 0.4f;

	public const float MaxSmoothing = 0.625f;

	public NativeArray<byte> FlatArray;

	public int3 Origin;

	public int3 Bounds;

	private int _widthHeight;

	public int Width => Bounds.x;

	public int Height => Bounds.y;

	public int Depth => Bounds.z;

	public int WidthHeight => _widthHeight;

	public int NumCells => FlatArray.Length;

	public bool IsCreated => FlatArray.IsCreated;

	public float this[int x, int y, int z]
	{
		get
		{
			return Sample(ToIndex(x, y, z));
		}
		set
		{
			FlatArray[ToIndex(x, y, z)] = Compress(value);
		}
	}

	public void Init(int3 origin, int3 bounds, Allocator allocator = Allocator.Persistent)
	{
		Origin = origin;
		Bounds = bounds;
		_widthHeight = Width * Height;
		FlatArray = new NativeArray<byte>(Bounds.x * Bounds.y * Bounds.z, allocator);
	}

	public static int3 MipBounds(int3 bounds, int level)
	{
		int num = 1 << level;
		return math.max(2, (bounds + (num - 1)) / num);
	}

	public void Clear()
	{
		if (FlatArray.IsCreated)
		{
			NativeArrayEx.MemClear(in FlatArray);
		}
	}

	public void ToLocalIntBounds(in Bounds worldFloatBounds, out int3 min, out int3 max)
	{
		min = math.max(0, (int3)math.floor((float3)worldFloatBounds.min - (float3)Origin));
		max = math.min(Bounds - 1, (int3)math.ceil((float3)worldFloatBounds.max - (float3)Origin));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ToIndex(int x, int y, int z)
	{
		return x + y * Width + z * WidthHeight;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ToIndex(int3 c)
	{
		return ToIndex(c.x, c.y, c.z);
	}

	public byte GetByte(int x, int y, int z)
	{
		return FlatArray[ToIndex(x, y, z)];
	}

	public void SetByte(int x, int y, int z, byte b)
	{
		FlatArray[ToIndex(x, y, z)] = b;
	}

	public float Sample(int3 p)
	{
		return (int)FlatArray[ToIndex(p.x, p.y, p.z)];
	}

	public float Sample(int flatIndex)
	{
		return (int)FlatArray[flatIndex];
	}

	public byte Compress(float f)
	{
		return (byte)Mathf.Clamp((int)((f * 0.4f * 0.5f + 0.5f) * 255f), 0, 255);
	}

	public void Dispose()
	{
		NativeArrayEx.SafeDispose(ref FlatArray);
	}
}

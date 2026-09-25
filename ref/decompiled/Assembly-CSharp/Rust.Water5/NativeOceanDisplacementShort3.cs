using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Rust.Water5;

internal struct NativeOceanDisplacementShort3 : IDisposable
{
	public readonly struct ReadOnly
	{
		private readonly NativeArray<OceanDisplacementShort3>.ReadOnly oceanDisplacementShort3S;

		private readonly int spectrumCount;

		private readonly int frameCount;

		public OceanDisplacementShort3 this[int x, int y, int z]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return oceanDisplacementShort3S[z * spectrumCount * frameCount + y * spectrumCount + x];
			}
		}

		public ReadOnly(NativeArray<OceanDisplacementShort3>.ReadOnly oceanDisplacementShort3s, int spectrumCount, int frameCount)
		{
			oceanDisplacementShort3S = oceanDisplacementShort3s;
			this.spectrumCount = spectrumCount;
			this.frameCount = frameCount;
		}
	}

	[NativeDisableParallelForRestriction]
	private NativeArray<OceanDisplacementShort3> _arr;

	private int spectrumCount;

	private int frameCount;

	public OceanDisplacementShort3 this[int x, int y, int z]
	{
		get
		{
			return _arr[z * spectrumCount * frameCount + y * spectrumCount + x];
		}
		set
		{
			_arr[z * spectrumCount * frameCount + y * spectrumCount + x] = value;
		}
	}

	public int Length => _arr.Length;

	public static Rust.Water5.NativeOceanDisplacementShort3 Create(int x, int y, int z)
	{
		Rust.Water5.NativeOceanDisplacementShort3 result = default(Rust.Water5.NativeOceanDisplacementShort3);
		result._arr = new NativeArray<OceanDisplacementShort3>(x * y * z, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		result.spectrumCount = x;
		result.frameCount = y;
		return result;
	}

	public static Rust.Water5.NativeOceanDisplacementShort3 Create(OceanDisplacementShort3[,,] simData)
	{
		Rust.Water5.NativeOceanDisplacementShort3 result = default(Rust.Water5.NativeOceanDisplacementShort3);
		result._arr = new NativeArray<OceanDisplacementShort3>(simData.Length, Allocator.Persistent);
		result.spectrumCount = simData.GetLength(0);
		result.frameCount = simData.GetLength(1);
		for (int i = 0; i < result.spectrumCount; i++)
		{
			for (int j = 0; j < result.frameCount; j++)
			{
				for (int k = 0; k < simData.GetLength(2); k++)
				{
					result._arr[i * result.spectrumCount + j * result.frameCount + k] = simData[i, j, k];
				}
			}
		}
		return result;
	}

	public unsafe OceanDisplacementShort3* GetUnsafePtr()
	{
		if (!_arr.IsCreated)
		{
			return null;
		}
		return (OceanDisplacementShort3*)_arr.GetUnsafePtr();
	}

	public NativeArray<OceanDisplacementShort3>.ReadOnly GetNativeRawReadOnly()
	{
		return _arr.AsReadOnly();
	}

	public NativeArray<OceanDisplacementShort3> GetNativeRaw()
	{
		return _arr;
	}

	public void Dispose()
	{
		_arr.Dispose();
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(_arr.AsReadOnly(), spectrumCount, frameCount);
	}
}

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Facepunch.MarchingCubes;

[BurstCompile(FloatMode = FloatMode.Fast)]
internal struct DownsampleJob : IJobParallelForBatch
{
	[global::Unity.Collections.ReadOnly]
	public QuantizedFloatData3DArray src;

	[NativeDisableContainerSafetyRestriction]
	public QuantizedFloatData3DArray dst;

	public void Execute(int startIndex, int count)
	{
		int3 y = src.Bounds - new int3(1);
		for (int i = startIndex; i < startIndex + count; i++)
		{
			int3 @int = new int3(i % dst.Width, i % dst.WidthHeight / dst.Width, i / dst.WidthHeight) * 2;
			int3 int2 = math.min(@int + new int3(1), y);
			int num = src.FlatArray[src.ToIndex(@int.x, @int.y, @int.z)] + src.FlatArray[src.ToIndex(int2.x, @int.y, @int.z)] + src.FlatArray[src.ToIndex(@int.x, int2.y, @int.z)] + src.FlatArray[src.ToIndex(int2.x, int2.y, @int.z)] + src.FlatArray[src.ToIndex(@int.x, @int.y, int2.z)] + src.FlatArray[src.ToIndex(int2.x, @int.y, int2.z)] + src.FlatArray[src.ToIndex(@int.x, int2.y, int2.z)] + src.FlatArray[src.ToIndex(int2.x, int2.y, int2.z)];
			dst.FlatArray[i] = (byte)(num >> 3);
		}
	}
}

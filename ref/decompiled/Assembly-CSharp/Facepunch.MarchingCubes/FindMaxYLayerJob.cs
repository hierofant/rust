using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Facepunch.MarchingCubes;

[BurstCompile(FloatMode = FloatMode.Fast)]
public struct FindMaxYLayerJob : IJob
{
	public QuantizedFloatData3DArray data;

	public float iso;

	public NativeReference<int> maxYLayer;

	public void Execute()
	{
		maxYLayer.Value = GetMaxY();
	}

	private int GetMaxY()
	{
		for (int num = data.Height - 1; num > 0; num--)
		{
			for (int i = 0; i < data.Depth; i++)
			{
				for (int j = 0; j < data.Width; j++)
				{
					if (data[j, num, i] <= iso)
					{
						return num;
					}
				}
			}
		}
		return 0;
	}
}

using System.Threading.Tasks;
using Unity.Collections;

public class GenerateOceanTopology : ProceduralComponent
{
	public override void Process(uint seed)
	{
		TerrainTopologyMap topologymap = TerrainMeta.TopologyMap;
		TerrainHeightMap heightmap = TerrainMeta.HeightMap;
		NativeArray<int> dst = topologymap.dst;
		int res = topologymap.res;
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				float normX2 = topologymap.Coordinate(i);
				float normZ2 = topologymap.Coordinate(z);
				int num = dst[z * res + i];
				if (heightmap.GetHeight01(normX2, normZ2) > 0.5f)
				{
					dst[z * res + i] = num & -129;
				}
				else if (((uint)num & 0x4000u) != 0)
				{
					dst[z * res + i] = num | 0x80;
				}
			}
		});
		ImageProcessing.FloodFill2D(0, 0, res, res, (int x, int z) => (dst[z * res + x] & 0x80) != 0, delegate(int x, int z)
		{
			dst[z * res + x] &= -129;
		});
		ImageProcessing.FloodFill2D(0, 0, res, res, delegate(int x, int z)
		{
			if (((uint)dst[z * res + x] & 0x810080u) != 0)
			{
				return false;
			}
			float normX = topologymap.Coordinate(x);
			float normZ = topologymap.Coordinate(z);
			return heightmap.GetHeight01(normX, normZ) <= 0.5f;
		}, delegate(int x, int z)
		{
			dst[z * res + x] |= 128;
		});
	}
}

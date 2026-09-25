using Unity.Collections;

namespace TerrainTopologyMapJobs;

public static class TerrainTopologyMapJobUtil
{
	public static int GetTopologyRadius(NativeArray<int>.ReadOnly src, int res, float terrainOneOverSizeX, float radius, float normX, float normZ)
	{
		float num = terrainOneOverSizeX * radius;
		int x_mid = Index(normX, res);
		int z_mid = Index(normZ, res);
		int x_min = Index(normX - num, res);
		int x_max = Index(normX + num, res);
		int z_min = Index(normZ - num, res);
		int z_max = Index(normZ + num, res);
		return GetTopologyRadius(src, res, radius, x_min, x_mid, x_max, z_min, z_mid, z_max);
	}

	private static int Index(float normalized, int res)
	{
		int num = (int)(normalized * (float)res);
		if (num >= 0)
		{
			if (num <= res - 1)
			{
				return num;
			}
			return res - 1;
		}
		return 0;
	}

	internal static int GetTopologyRadius(NativeArray<int>.ReadOnly src, int res, float radius, int x_min, int x_mid, int x_max, int z_min, int z_mid, int z_max)
	{
		int num = 0;
		float num2 = radius * radius;
		for (int i = z_min; i <= z_max; i++)
		{
			int num3 = i - z_mid;
			int num4 = num3 * num3;
			for (int j = x_min; j <= x_max; j++)
			{
				int num5 = j - x_mid;
				if (!((float)(num5 * num5 + num4) > num2))
				{
					num |= src[i * res + j];
				}
			}
		}
		return num;
	}
}

using Unity.Collections;
using UnityEngine;

namespace TerrainHeightMapJobs;

public struct HeightMapData
{
	public Bounds DeepSeaBounds;

	public NativeArray<short>.ReadOnly Data;

	public NativeArray<short>.ReadOnly DeepSeaData;

	public Vector3 TerrainPos;

	public float TerrainScale;

	public Vector2 TerrainOneOverSize;

	public int Res;

	public float NormY;

	public static float GetHeight01(Vector2 uv, NativeArray<short>.ReadOnly data, int res)
	{
		int num = res - 1;
		float num2 = uv.x * (float)num;
		float num3 = uv.y * (float)num;
		int num4 = (int)num2;
		int num5 = (int)num3;
		float num6 = Mathf.Clamp01(num2 - (float)num4);
		float num7 = Mathf.Clamp01(num3 - (float)num5);
		num4 = ((num4 >= 0) ? num4 : 0);
		num5 = ((num5 >= 0) ? num5 : 0);
		num4 = ((num4 <= num) ? num4 : num);
		num5 = ((num5 <= num) ? num5 : num);
		int num8 = ((num2 < (float)num) ? 1 : 0);
		int num9 = ((num3 < (float)num) ? res : 0);
		int num10 = num5 * res + num4;
		int index = num10 + num8;
		int num11 = num10 + num9;
		int index2 = num11 + num8;
		float height = GetHeight01(num10, data);
		float height2 = GetHeight01(index, data);
		float height3 = GetHeight01(num11, data);
		float height4 = GetHeight01(index2, data);
		float num12 = (height2 - height) * num6 + height;
		return ((height4 - height3) * num6 + height3 - num12) * num7 + num12;
	}

	public static float GetHeight01(int x, int z, NativeArray<short>.ReadOnly data, int res)
	{
		return BitUtility.Short2Float(data[z * res + x]);
	}

	public static float GetHeight01(int index, NativeArray<short>.ReadOnly data)
	{
		return BitUtility.Short2Float(data[index]);
	}

	public static Vector3 GetNormal(Vector2 uv, float normY, NativeArray<short>.ReadOnly data, int res)
	{
		int num = res - 1;
		float num2 = uv.x * (float)num;
		float num3 = uv.y * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		Vector3 normal = GetNormal(num4, num5, normY, data, res);
		Vector3 normal2 = GetNormal(x, num5, normY, data, res);
		Vector3 normal3 = GetNormal(num4, z, normY, data, res);
		Vector3 normal4 = GetNormal(x, z, normY, data, res);
		float t = num2 - (float)num4;
		float t2 = num3 - (float)num5;
		Vector3 a = Vector3.Slerp(normal, normal2, t);
		Vector3 b = Vector3.Slerp(normal3, normal4, t);
		return Vector3.Slerp(a, b, t2).normalized;
	}

	public static Vector3 GetNormal(int x, int z, float normY, NativeArray<short>.ReadOnly data, int res)
	{
		int max = res - 1;
		int x2 = Mathf.Clamp(x - 1, 0, max);
		int z2 = Mathf.Clamp(z - 1, 0, max);
		int x3 = Mathf.Clamp(x + 1, 0, max);
		int z3 = Mathf.Clamp(z + 1, 0, max);
		float height = GetHeight01(x2, z2, data, res);
		float height2 = GetHeight01(x3, z2, data, res);
		float height3 = GetHeight01(x2, z3, data, res);
		float num = (height2 - height) * 0.5f;
		float num2 = (height3 - height) * 0.5f;
		return new Vector3(0f - num, normY, 0f - num2).normalized;
	}
}

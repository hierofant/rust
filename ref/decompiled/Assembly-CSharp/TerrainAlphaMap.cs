using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Serialization;

public class TerrainAlphaMap : TerrainMap<byte>
{
	public struct AlphaSampler
	{
		private unsafe byte* data;

		private int res;

		private int len;

		private float scaleX;

		private float baseX;

		private float scaleZ;

		private float baseZ;

		private unsafe byte* rowT;

		private unsafe byte* rowB;

		private float wz;

		public unsafe static AlphaSampler Create(NativeArray<byte> alphas, int res, Vector3 mapPos, Vector3 oneOverSize)
		{
			int num = res - 1;
			float num2 = oneOverSize.x * (float)num;
			float num3 = oneOverSize.z * (float)num;
			AlphaSampler result = default(AlphaSampler);
			result.data = (byte*)alphas.GetUnsafeReadOnlyPtr();
			result.res = res;
			result.len = num;
			result.scaleX = num2;
			result.baseX = (0f - mapPos.x) * num2;
			result.scaleZ = num3;
			result.baseZ = (0f - mapPos.z) * num3;
			return result;
		}

		public unsafe void BeginRow(float worldZ)
		{
			float num = worldZ * scaleZ + baseZ;
			int num2 = Mathf.Clamp((int)num, 0, len);
			int num3 = Mathf.Min(num2 + 1, len);
			wz = Mathf.Clamp01(num - (float)num2);
			rowT = data + num2 * res;
			rowB = data + num3 * res;
		}

		public unsafe float SampleRow(float worldX)
		{
			float num = worldX * scaleX + baseX;
			int num2 = Mathf.Clamp((int)num, 0, len);
			int num3 = Mathf.Min(num2 + 1, len);
			float num4 = Mathf.Clamp01(num - (float)num2);
			float num5 = BitUtility.Byte2Float(rowT[num2]);
			float num6 = BitUtility.Byte2Float(rowT[num3]);
			float num7 = BitUtility.Byte2Float(rowB[num2]);
			float num8 = BitUtility.Byte2Float(rowB[num3]);
			float num9 = (num6 - num5) * num4 + num5;
			return ((num8 - num7) * num4 + num7 - num9) * wz + num9;
		}
	}

	[FormerlySerializedAs("ColorTexture")]
	public Texture2D AlphaTexture;

	private bool _generatedAlphaTexture;

	public override void Setup()
	{
		res = terrainData.alphamapResolution;
		InitArrays(res * res);
		for (int i = 0; i < res; i++)
		{
			for (int j = 0; j < res; j++)
			{
				dst[i * res + j] = byte.MaxValue;
			}
		}
		if (!(AlphaTexture != null))
		{
			return;
		}
		if (AlphaTexture.width == AlphaTexture.height && AlphaTexture.width == res)
		{
			Color32[] pixels = AlphaTexture.GetPixels32();
			int k = 0;
			int num = 0;
			for (; k < res; k++)
			{
				int num2 = 0;
				while (num2 < res)
				{
					dst[k * res + num2] = pixels[num].a;
					num2++;
					num++;
				}
			}
		}
		else
		{
			Debug.LogError("Invalid alpha texture: " + AlphaTexture.name);
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		if (_generatedAlphaTexture && AlphaTexture != null)
		{
			UnityEngine.Object.Destroy(AlphaTexture);
			AlphaTexture = null;
		}
	}

	public void GenerateTextures()
	{
		AlphaTexture = new Texture2D(res, res, TextureFormat.Alpha8, mipChain: false, linear: true);
		AlphaTexture.name = "AlphaTexture";
		AlphaTexture.wrapMode = TextureWrapMode.Clamp;
		AlphaTexture.SetPixelData(src, 0);
		_generatedAlphaTexture = Application.isPlaying;
	}

	public void ApplyTextures()
	{
		AlphaTexture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		AlphaTexture.Compress(highQuality: false);
		AlphaTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public float GetAlpha(Vector3 worldPos)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetAlpha(normX, normZ);
	}

	public float GetAlpha(float normX, float normZ)
	{
		int num = res - 1;
		float num2 = normX * (float)num;
		float num3 = normZ * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		float a = Mathf.Lerp(GetAlpha(num4, num5), GetAlpha(x, num5), num2 - (float)num4);
		float b = Mathf.Lerp(GetAlpha(num4, z), GetAlpha(x, z), num2 - (float)num4);
		return Mathf.Lerp(a, b, num3 - (float)num5);
	}

	public float GetAlpha(int x, int z)
	{
		return BitUtility.Byte2Float(src[z * res + x]);
	}

	public float GetAlphaFloat(int x, int z)
	{
		return GetAlpha(x, z);
	}

	public void SetAlpha(Vector3 worldPos, float a)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetAlpha(normX, normZ, a);
	}

	public void SetAlpha(float normX, float normZ, float a)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetAlpha(x, z, a);
	}

	public void SetAlpha(int x, int z, float a)
	{
		dst[z * res + x] = BitUtility.Float2Byte(a);
	}

	public void SetAlpha(int x, int z, float a, float opacity)
	{
		SetAlpha(x, z, Mathf.Lerp(GetAlpha(x, z), a, opacity));
	}

	public void SetAlpha(Vector3 worldPos, float a, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetAlpha(normX, normZ, a, opacity, radius, fade);
	}

	public void SetAlpha(float normX, float normZ, float a, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			lerp *= opacity;
			if (lerp > 0f)
			{
				SetAlpha(x, z, a, lerp);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public AlphaSampler CreateSampler()
	{
		return AlphaSampler.Create(src, res, TerrainMeta.Position, TerrainMeta.OneOverSize);
	}
}

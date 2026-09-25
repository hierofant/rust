namespace UnityEngine;

public static class TextureEx
{
	private static Color32[] buffer = new Color32[8192];

	public static void Clear(this Texture2D tex, Color32 color)
	{
		if (tex == null)
		{
			return;
		}
		if (tex.width > buffer.Length)
		{
			Debug.LogError("Trying to clear texture that is too big: " + tex.width);
			return;
		}
		for (int i = 0; i < tex.width; i++)
		{
			buffer[i] = color;
		}
		for (int j = 0; j < tex.height; j++)
		{
			tex.SetPixels32(0, j, tex.width, 1, buffer);
		}
		tex.Apply();
	}

	public static int GetSizeInBytes(this Texture texture)
	{
		int num = texture.width;
		int num2 = texture.height;
		if (texture is Texture2D)
		{
			Texture2D obj = texture as Texture2D;
			int bitsPerPixel = GetBitsPerPixel(obj.format);
			int mipmapCount = obj.mipmapCount;
			int i = 1;
			int num3 = 0;
			int loadedMipmapLevel = obj.loadedMipmapLevel;
			for (; i <= mipmapCount; i++)
			{
				if (i >= loadedMipmapLevel)
				{
					num3 += num * num2 * bitsPerPixel / 8;
				}
				num /= 2;
				num2 /= 2;
			}
			return num3;
		}
		if (texture is Texture2DArray)
		{
			Texture2DArray obj2 = texture as Texture2DArray;
			int bitsPerPixel2 = GetBitsPerPixel(obj2.format);
			int num4 = 10;
			int j = 1;
			int num5 = 0;
			int depth = obj2.depth;
			for (; j <= num4; j++)
			{
				num5 += num * num2 * bitsPerPixel2 / 8;
				num /= 2;
				num2 /= 2;
			}
			return num5 * depth;
		}
		if (texture is Cubemap)
		{
			int bitsPerPixel3 = GetBitsPerPixel((texture as Cubemap).format);
			int num6 = num * num2 * bitsPerPixel3 / 8;
			int num7 = 6;
			return num6 * num7;
		}
		if (texture is RenderTexture)
		{
			RenderTexture renderTexture = texture as RenderTexture;
			int bitsPerPixel4 = GetBitsPerPixel(renderTexture.format, renderTexture.depth);
			int mipmapCount2 = renderTexture.mipmapCount;
			int k = 1;
			int num8 = 0;
			for (; k <= mipmapCount2; k++)
			{
				num8 += num * num2 * bitsPerPixel4 / 8;
				num /= 2;
				num2 /= 2;
			}
			return num8;
		}
		return 0;
	}

	public static int GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
			return 8;
		case TextureFormat.ARGB4444:
			return 16;
		case TextureFormat.RGBA4444:
			return 16;
		case TextureFormat.RGB24:
			return 24;
		case TextureFormat.RGBA32:
			return 32;
		case TextureFormat.ARGB32:
			return 32;
		case TextureFormat.RGB565:
			return 16;
		case TextureFormat.DXT1:
		case TextureFormat.DXT1Crunched:
			return 4;
		case TextureFormat.DXT5:
		case TextureFormat.BC7:
		case TextureFormat.DXT5Crunched:
			return 8;
		case TextureFormat.PVRTC_RGB2:
			return 2;
		case TextureFormat.PVRTC_RGBA2:
			return 2;
		case TextureFormat.PVRTC_RGB4:
			return 4;
		case TextureFormat.PVRTC_RGBA4:
			return 4;
		case TextureFormat.ETC_RGB4:
			return 4;
		case TextureFormat.ETC2_RGBA8:
			return 8;
		case TextureFormat.BGRA32:
			return 32;
		default:
			return 0;
		}
	}

	public static int GetBitsPerPixel(RenderTextureFormat format, int depthBits)
	{
		switch (format)
		{
		case RenderTextureFormat.ARGBFloat:
		case RenderTextureFormat.ARGBInt:
			return 128;
		case RenderTextureFormat.ARGBHalf:
		case RenderTextureFormat.DefaultHDR:
		case RenderTextureFormat.ARGB64:
		case RenderTextureFormat.RGFloat:
		case RenderTextureFormat.RGInt:
		case RenderTextureFormat.RGBAUShort:
		case RenderTextureFormat.BGRA10101010_XR:
			return 64;
		default:
			return 32;
		case RenderTextureFormat.RGB565:
		case RenderTextureFormat.ARGB4444:
		case RenderTextureFormat.ARGB1555:
		case RenderTextureFormat.RHalf:
		case RenderTextureFormat.RG16:
		case RenderTextureFormat.R16:
			return 16;
		case RenderTextureFormat.R8:
			return 8;
		case RenderTextureFormat.Depth:
		case RenderTextureFormat.Shadowmap:
			return depthBits;
		}
	}
}

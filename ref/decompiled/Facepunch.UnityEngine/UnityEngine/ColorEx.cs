namespace UnityEngine;

public static class ColorEx
{
	private const byte k_MaxByteForOverexposedColor = 191;

	public static string ToHex(this Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	public static string ToString(Color color)
	{
		return $"{color.r} {color.g} {color.b} {color.a}";
	}

	public static Color Parse(string str)
	{
		string[] array = str.Split(' ');
		if (array.Length == 3 && float.TryParse(array[0], out var result) && float.TryParse(array[1], out var result2) && float.TryParse(array[2], out var result3))
		{
			return new Color(result, result2, result3);
		}
		if (array.Length == 4 && float.TryParse(array[0], out result) && float.TryParse(array[1], out result2) && float.TryParse(array[2], out result3) && float.TryParse(array[3], out var result4))
		{
			return new Color(result, result2, result3, result4);
		}
		return Color.white;
	}

	public static Color WithAlpha(this Color color, float a)
	{
		return new Color(color.r, color.g, color.b, a);
	}

	public static int ToInt32(this Color32 color)
	{
		return (color.r << 24) | (color.g << 16) | (color.b << 8) | color.a;
	}

	public static Color32 FromInt32(int value)
	{
		return new Color32((byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value);
	}

	public static Vector4 ToVector4(this Color color)
	{
		return new Vector4(color.r, color.g, color.b, color.a);
	}

	public static float GetHDRIntensity(this Color color)
	{
		float maxColorComponent = color.maxColorComponent;
		float num = 191f / maxColorComponent;
		if (maxColorComponent != 0f && (!(maxColorComponent <= 1f) || !(maxColorComponent >= 0.003921569f)))
		{
			return Mathf.Log(255f / num) / Mathf.Log(2f);
		}
		return 1f;
	}

	public static Color WithHDRIntensity(this Color color, float hdrIntensity)
	{
		float num = 0.7490196f * Mathf.Pow(2f, hdrIntensity);
		return new Color(color.r * num, color.g * num, color.b * num);
	}
}

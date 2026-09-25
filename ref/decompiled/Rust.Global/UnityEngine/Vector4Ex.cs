namespace UnityEngine;

public static class Vector4Ex
{
	public static Vector4 Parse(string p)
	{
		string[] array = p.Split(' ');
		if (array.Length == 4 && float.TryParse(array[0], out var result) && float.TryParse(array[1], out var result2) && float.TryParse(array[2], out var result3) && float.TryParse(array[3], out var result4))
		{
			return new Vector4(result, result2, result3, result4);
		}
		return Vector4.zero;
	}
}

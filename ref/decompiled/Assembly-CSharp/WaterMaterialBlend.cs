using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Rust/Water/Material Blend")]
public class WaterMaterialBlend : ScriptableObject
{
	[Serializable]
	public struct BlendState
	{
		public Color Albedo;

		public Color Specular;

		public float Smoothness;

		public float NormalStrength;

		public Color WaterColor;

		public Vector4 ColorExtinction;

		public float ScatterCoefficient;

		public Color SubSurfaceColour;

		public static BlendState Default = new BlendState
		{
			Albedo = Color.white,
			Specular = Color.white,
			Smoothness = 0.5f,
			NormalStrength = 0.5f,
			WaterColor = Color.white,
			ColorExtinction = Vector4.zero,
			ScatterCoefficient = 0.5f,
			SubSurfaceColour = Color.white
		};

		public static BlendState Blend(BlendState a, BlendState b, float t)
		{
			if (t <= 0.001f)
			{
				return a;
			}
			if (t >= 0.999f)
			{
				return b;
			}
			BlendState result = default(BlendState);
			result.Albedo = Color.Lerp(a.Albedo, b.Albedo, t);
			result.Specular = Color.Lerp(a.Specular, b.Specular, t);
			result.Smoothness = Mathf.Lerp(a.Smoothness, b.Smoothness, t);
			result.NormalStrength = Mathf.Lerp(a.NormalStrength, b.NormalStrength, t);
			result.WaterColor = Color.Lerp(a.WaterColor, b.WaterColor, t);
			result.ColorExtinction = Vector4.Lerp((Vector4)a.WaterColor, (Vector4)b.WaterColor, t);
			result.ScatterCoefficient = Mathf.Lerp(a.ScatterCoefficient, b.ScatterCoefficient, t);
			result.SubSurfaceColour = Color.Lerp(a.SubSurfaceColour, b.SubSurfaceColour, t);
			return result;
		}
	}

	public BlendState MaterialA = BlendState.Default;

	public BlendState MaterialB = BlendState.Default;
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rust.Json;

public static class UnityJsonConverters
{
	public static JsonConverter[] CreateAll()
	{
		return new JsonConverter[24]
		{
			new Vector2Converter(),
			new Vector3Converter(),
			new Vector4Converter(),
			new Vector2IntConverter(),
			new Vector3IntConverter(),
			new QuaternionConverter(),
			new Matrix4x4Converter(),
			new ColorConverter(),
			new Color32Converter(),
			new RectConverter(),
			new RectIntConverter(),
			new RectOffsetConverter(),
			new BoundsConverter(),
			new BoundsIntConverter(),
			new LayerMaskConverter(),
			new RayConverter(),
			new Ray2DConverter(),
			new PlaneConverter(),
			new PoseConverter(),
			new Hash128Converter(),
			new ResolutionConverter(),
			new KeyframeConverter(),
			new AnimationCurveConverter(),
			new GradientConverter()
		};
	}

	internal static float F(JToken token, string name, float fallback = 0f)
	{
		return ((float?)token?[name]) ?? fallback;
	}

	internal static int I(JToken token, string name, int fallback = 0)
	{
		return ((int?)token?[name]) ?? fallback;
	}
}

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class GradientConverter : JsonConverter<Gradient>
{
	public override void WriteJson(JsonWriter writer, Gradient value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("colorKeys");
		writer.WriteStartArray();
		GradientColorKey[] colorKeys = value.colorKeys;
		for (int i = 0; i < colorKeys.Length; i++)
		{
			GradientColorKey gradientColorKey = colorKeys[i];
			writer.WriteStartObject();
			writer.WritePropertyName("color");
			serializer.Serialize(writer, gradientColorKey.color);
			writer.WritePropertyName("time");
			writer.WriteValue(gradientColorKey.time);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WritePropertyName("alphaKeys");
		writer.WriteStartArray();
		GradientAlphaKey[] alphaKeys = value.alphaKeys;
		for (int i = 0; i < alphaKeys.Length; i++)
		{
			GradientAlphaKey gradientAlphaKey = alphaKeys[i];
			writer.WriteStartObject();
			writer.WritePropertyName("alpha");
			writer.WriteValue(gradientAlphaKey.alpha);
			writer.WritePropertyName("time");
			writer.WriteValue(gradientAlphaKey.time);
			writer.WriteEndObject();
		}
		writer.WriteEndArray();
		writer.WritePropertyName("mode");
		writer.WriteValue((int)value.mode);
		writer.WriteEndObject();
	}

	public override Gradient ReadJson(JsonReader reader, Type objectType, Gradient existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject jObject = JObject.Load(reader);
		GradientColorKey[] array = Array.Empty<GradientColorKey>();
		if (jObject["colorKeys"] is JArray jArray)
		{
			array = new GradientColorKey[jArray.Count];
			for (int i = 0; i < jArray.Count; i++)
			{
				array[i] = new GradientColorKey(jArray[i]["color"]?.ToObject<Color>(serializer) ?? Color.white, UnityJsonConverters.F(jArray[i], "time"));
			}
		}
		GradientAlphaKey[] array2 = Array.Empty<GradientAlphaKey>();
		if (jObject["alphaKeys"] is JArray jArray2)
		{
			array2 = new GradientAlphaKey[jArray2.Count];
			for (int j = 0; j < jArray2.Count; j++)
			{
				array2[j] = new GradientAlphaKey(UnityJsonConverters.F(jArray2[j], "alpha", 1f), UnityJsonConverters.F(jArray2[j], "time"));
			}
		}
		Gradient gradient = new Gradient();
		gradient.mode = (GradientMode)UnityJsonConverters.I(jObject, "mode");
		gradient.SetKeys(array, array2);
		return gradient;
	}
}

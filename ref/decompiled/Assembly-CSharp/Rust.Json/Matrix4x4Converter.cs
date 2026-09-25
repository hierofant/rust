using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Matrix4x4Converter : JsonConverter<Matrix4x4>
{
	public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				writer.WritePropertyName($"m{i}{j}");
				writer.WriteValue(value[i, j]);
			}
		}
		writer.WriteEndObject();
	}

	public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		Matrix4x4 identity = Matrix4x4.identity;
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			for (int i = 0; i < 16 && i < jArray.Count; i++)
			{
				identity[i / 4, i % 4] = (float)jArray[i];
			}
			return identity;
		}
		JObject token = JObject.Load(reader);
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < 4; k++)
			{
				identity[j, k] = UnityJsonConverters.F(token, $"m{j}{k}", identity[j, k]);
			}
		}
		return identity;
	}
}

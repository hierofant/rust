using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector2IntConverter : JsonConverter<Vector2Int>
{
	public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WriteEndObject();
	}

	public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Vector2Int((int)jArray[0], (int)jArray[1]);
		}
		JObject token = JObject.Load(reader);
		return new Vector2Int(UnityJsonConverters.I(token, "x"), UnityJsonConverters.I(token, "y"));
	}
}

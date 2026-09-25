using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector2Converter : JsonConverter<Vector2>
{
	public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WriteEndObject();
	}

	public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Vector2((float)jArray[0], (float)jArray[1]);
		}
		JObject token = JObject.Load(reader);
		return new Vector2(UnityJsonConverters.F(token, "x"), UnityJsonConverters.F(token, "y"));
	}
}

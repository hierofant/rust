using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class ColorConverter : JsonConverter<Color>
{
	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("r");
		writer.WriteValue(value.r);
		writer.WritePropertyName("g");
		writer.WriteValue(value.g);
		writer.WritePropertyName("b");
		writer.WriteValue(value.b);
		writer.WritePropertyName("a");
		writer.WriteValue(value.a);
		writer.WriteEndObject();
	}

	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Color((float)jArray[0], (float)jArray[1], (float)jArray[2], (jArray.Count > 3) ? ((float)jArray[3]) : 1f);
		}
		JObject token = JObject.Load(reader);
		return new Color(UnityJsonConverters.F(token, "r"), UnityJsonConverters.F(token, "g"), UnityJsonConverters.F(token, "b"), UnityJsonConverters.F(token, "a", 1f));
	}
}

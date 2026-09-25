using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class RectOffsetConverter : JsonConverter<RectOffset>
{
	public override void WriteJson(JsonWriter writer, RectOffset value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("left");
		writer.WriteValue(value.left);
		writer.WritePropertyName("right");
		writer.WriteValue(value.right);
		writer.WritePropertyName("top");
		writer.WriteValue(value.top);
		writer.WritePropertyName("bottom");
		writer.WriteValue(value.bottom);
		writer.WriteEndObject();
	}

	public override RectOffset ReadJson(JsonReader reader, Type objectType, RectOffset existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject token = JObject.Load(reader);
		return new RectOffset(UnityJsonConverters.I(token, "left"), UnityJsonConverters.I(token, "right"), UnityJsonConverters.I(token, "top"), UnityJsonConverters.I(token, "bottom"));
	}
}

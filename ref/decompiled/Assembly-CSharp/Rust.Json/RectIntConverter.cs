using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class RectIntConverter : JsonConverter<RectInt>
{
	public override void WriteJson(JsonWriter writer, RectInt value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("width");
		writer.WriteValue(value.width);
		writer.WritePropertyName("height");
		writer.WriteValue(value.height);
		writer.WriteEndObject();
	}

	public override RectInt ReadJson(JsonReader reader, Type objectType, RectInt existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject token = JObject.Load(reader);
		return new RectInt(UnityJsonConverters.I(token, "x"), UnityJsonConverters.I(token, "y"), UnityJsonConverters.I(token, "width"), UnityJsonConverters.I(token, "height"));
	}
}

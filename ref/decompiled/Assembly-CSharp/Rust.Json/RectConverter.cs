using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class RectConverter : JsonConverter<Rect>
{
	public override void WriteJson(JsonWriter writer, Rect value, JsonSerializer serializer)
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

	public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Rect((float)jArray[0], (float)jArray[1], (float)jArray[2], (float)jArray[3]);
		}
		JObject token = JObject.Load(reader);
		return new Rect(UnityJsonConverters.F(token, "x"), UnityJsonConverters.F(token, "y"), UnityJsonConverters.F(token, "width"), UnityJsonConverters.F(token, "height"));
	}
}

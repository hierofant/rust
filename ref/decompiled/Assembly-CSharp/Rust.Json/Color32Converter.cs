using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Color32Converter : JsonConverter<Color32>
{
	public override void WriteJson(JsonWriter writer, Color32 value, JsonSerializer serializer)
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

	public override Color32 ReadJson(JsonReader reader, Type objectType, Color32 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Color32((byte)(int)jArray[0], (byte)(int)jArray[1], (byte)(int)jArray[2], (jArray.Count > 3) ? ((byte)(int)jArray[3]) : byte.MaxValue);
		}
		JObject token = JObject.Load(reader);
		return new Color32((byte)UnityJsonConverters.I(token, "r"), (byte)UnityJsonConverters.I(token, "g"), (byte)UnityJsonConverters.I(token, "b"), (byte)UnityJsonConverters.I(token, "a", 255));
	}
}

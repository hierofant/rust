using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector4Converter : JsonConverter<Vector4>
{
	public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WritePropertyName("w");
		writer.WriteValue(value.w);
		writer.WriteEndObject();
	}

	public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Vector4((float)jArray[0], (float)jArray[1], (float)jArray[2], (float)jArray[3]);
		}
		JObject token = JObject.Load(reader);
		return new Vector4(UnityJsonConverters.F(token, "x"), UnityJsonConverters.F(token, "y"), UnityJsonConverters.F(token, "z"), UnityJsonConverters.F(token, "w"));
	}
}

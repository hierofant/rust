using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector3Converter : JsonConverter<Vector3>
{
	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WriteEndObject();
	}

	public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Vector3((float)jArray[0], (float)jArray[1], (float)jArray[2]);
		}
		JObject token = JObject.Load(reader);
		return new Vector3(UnityJsonConverters.F(token, "x"), UnityJsonConverters.F(token, "y"), UnityJsonConverters.F(token, "z"));
	}
}

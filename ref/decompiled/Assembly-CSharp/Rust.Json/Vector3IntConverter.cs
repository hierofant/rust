using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Vector3IntConverter : JsonConverter<Vector3Int>
{
	public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
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

	public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Vector3Int((int)jArray[0], (int)jArray[1], (int)jArray[2]);
		}
		JObject token = JObject.Load(reader);
		return new Vector3Int(UnityJsonConverters.I(token, "x"), UnityJsonConverters.I(token, "y"), UnityJsonConverters.I(token, "z"));
	}
}

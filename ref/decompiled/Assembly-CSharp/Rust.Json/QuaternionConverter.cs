using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class QuaternionConverter : JsonConverter<Quaternion>
{
	public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
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

	public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.StartArray)
		{
			JArray jArray = JArray.Load(reader);
			return new Quaternion((float)jArray[0], (float)jArray[1], (float)jArray[2], (float)jArray[3]);
		}
		JObject jObject = JObject.Load(reader);
		JToken jToken = jObject["eulerAngles"];
		if (jToken != null)
		{
			return Quaternion.Euler(UnityJsonConverters.F(jToken, "x"), UnityJsonConverters.F(jToken, "y"), UnityJsonConverters.F(jToken, "z"));
		}
		return new Quaternion(UnityJsonConverters.F(jObject, "x"), UnityJsonConverters.F(jObject, "y"), UnityJsonConverters.F(jObject, "z"), UnityJsonConverters.F(jObject, "w", 1f));
	}
}

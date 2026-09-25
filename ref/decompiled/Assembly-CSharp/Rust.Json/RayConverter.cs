using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class RayConverter : JsonConverter<Ray>
{
	public override void WriteJson(JsonWriter writer, Ray value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("origin");
		serializer.Serialize(writer, value.origin);
		writer.WritePropertyName("direction");
		serializer.Serialize(writer, value.direction);
		writer.WriteEndObject();
	}

	public override Ray ReadJson(JsonReader reader, Type objectType, Ray existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Vector3 origin = jObject["origin"]?.ToObject<Vector3>(serializer) ?? Vector3.zero;
		Vector3 direction = jObject["direction"]?.ToObject<Vector3>(serializer) ?? Vector3.forward;
		return new Ray(origin, direction);
	}
}

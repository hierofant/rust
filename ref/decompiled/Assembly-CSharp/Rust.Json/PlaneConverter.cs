using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class PlaneConverter : JsonConverter<Plane>
{
	public override void WriteJson(JsonWriter writer, Plane value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("normal");
		serializer.Serialize(writer, value.normal);
		writer.WritePropertyName("distance");
		writer.WriteValue(value.distance);
		writer.WriteEndObject();
	}

	public override Plane ReadJson(JsonReader reader, Type objectType, Plane existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		return new Plane(jObject["normal"]?.ToObject<Vector3>(serializer) ?? Vector3.up, UnityJsonConverters.F(jObject, "distance"));
	}
}

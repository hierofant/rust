using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class PoseConverter : JsonConverter<Pose>
{
	public override void WriteJson(JsonWriter writer, Pose value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("position");
		serializer.Serialize(writer, value.position);
		writer.WritePropertyName("rotation");
		serializer.Serialize(writer, value.rotation);
		writer.WriteEndObject();
	}

	public override Pose ReadJson(JsonReader reader, Type objectType, Pose existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Vector3 position = jObject["position"]?.ToObject<Vector3>(serializer) ?? Vector3.zero;
		Quaternion rotation = jObject["rotation"]?.ToObject<Quaternion>(serializer) ?? Quaternion.identity;
		return new Pose(position, rotation);
	}
}

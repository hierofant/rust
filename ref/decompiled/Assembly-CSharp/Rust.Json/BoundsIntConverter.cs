using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class BoundsIntConverter : JsonConverter<BoundsInt>
{
	public override void WriteJson(JsonWriter writer, BoundsInt value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("position");
		serializer.Serialize(writer, value.position);
		writer.WritePropertyName("size");
		serializer.Serialize(writer, value.size);
		writer.WriteEndObject();
	}

	public override BoundsInt ReadJson(JsonReader reader, Type objectType, BoundsInt existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Vector3Int position = jObject["position"]?.ToObject<Vector3Int>(serializer) ?? Vector3Int.zero;
		Vector3Int size = jObject["size"]?.ToObject<Vector3Int>(serializer) ?? Vector3Int.zero;
		return new BoundsInt(position, size);
	}
}

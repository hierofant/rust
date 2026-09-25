using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class Ray2DConverter : JsonConverter<Ray2D>
{
	public override void WriteJson(JsonWriter writer, Ray2D value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("origin");
		serializer.Serialize(writer, value.origin);
		writer.WritePropertyName("direction");
		serializer.Serialize(writer, value.direction);
		writer.WriteEndObject();
	}

	public override Ray2D ReadJson(JsonReader reader, Type objectType, Ray2D existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject jObject = JObject.Load(reader);
		Vector2 origin = jObject["origin"]?.ToObject<Vector2>(serializer) ?? Vector2.zero;
		Vector2 direction = jObject["direction"]?.ToObject<Vector2>(serializer) ?? Vector2.up;
		return new Ray2D(origin, direction);
	}
}

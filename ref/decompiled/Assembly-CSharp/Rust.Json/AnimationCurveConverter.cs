using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class AnimationCurveConverter : JsonConverter<AnimationCurve>
{
	public override void WriteJson(JsonWriter writer, AnimationCurve value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("keys");
		serializer.Serialize(writer, value.keys);
		writer.WritePropertyName("preWrapMode");
		writer.WriteValue((int)value.preWrapMode);
		writer.WritePropertyName("postWrapMode");
		writer.WriteValue((int)value.postWrapMode);
		writer.WriteEndObject();
	}

	public override AnimationCurve ReadJson(JsonReader reader, Type objectType, AnimationCurve existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JObject jObject = JObject.Load(reader);
		return new AnimationCurve(jObject["keys"]?.ToObject<Keyframe[]>(serializer) ?? Array.Empty<Keyframe>())
		{
			preWrapMode = (WrapMode)UnityJsonConverters.I(jObject, "preWrapMode", 8),
			postWrapMode = (WrapMode)UnityJsonConverters.I(jObject, "postWrapMode", 8)
		};
	}
}

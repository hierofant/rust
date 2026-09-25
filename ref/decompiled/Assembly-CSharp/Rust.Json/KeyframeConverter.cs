using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class KeyframeConverter : JsonConverter<Keyframe>
{
	public override void WriteJson(JsonWriter writer, Keyframe value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("time");
		writer.WriteValue(value.time);
		writer.WritePropertyName("value");
		writer.WriteValue(value.value);
		writer.WritePropertyName("inTangent");
		writer.WriteValue(value.inTangent);
		writer.WritePropertyName("outTangent");
		writer.WriteValue(value.outTangent);
		writer.WritePropertyName("inWeight");
		writer.WriteValue(value.inWeight);
		writer.WritePropertyName("outWeight");
		writer.WriteValue(value.outWeight);
		writer.WritePropertyName("weightedMode");
		writer.WriteValue((int)value.weightedMode);
		writer.WriteEndObject();
	}

	public override Keyframe ReadJson(JsonReader reader, Type objectType, Keyframe existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject token = JObject.Load(reader);
		Keyframe result = new Keyframe(UnityJsonConverters.F(token, "time"), UnityJsonConverters.F(token, "value"), UnityJsonConverters.F(token, "inTangent"), UnityJsonConverters.F(token, "outTangent"), UnityJsonConverters.F(token, "inWeight"), UnityJsonConverters.F(token, "outWeight"));
		result.weightedMode = (WeightedMode)UnityJsonConverters.I(token, "weightedMode");
		return result;
	}
}

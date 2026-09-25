using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class ResolutionConverter : JsonConverter<Resolution>
{
	public override void WriteJson(JsonWriter writer, Resolution value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("width");
		writer.WriteValue(value.width);
		writer.WritePropertyName("height");
		writer.WriteValue(value.height);
		writer.WritePropertyName("refreshRate");
		writer.WriteValue(value.refreshRateRatio.value);
		writer.WriteEndObject();
	}

	public override Resolution ReadJson(JsonReader reader, Type objectType, Resolution existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		JObject token = JObject.Load(reader);
		Resolution result = default(Resolution);
		result.width = UnityJsonConverters.I(token, "width");
		result.height = UnityJsonConverters.I(token, "height");
		return result;
	}
}

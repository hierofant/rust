using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Rust.Json;

public class LayerMaskConverter : JsonConverter<LayerMask>
{
	public override void WriteJson(JsonWriter writer, LayerMask value, JsonSerializer serializer)
	{
		writer.WriteValue(value.value);
	}

	public override LayerMask ReadJson(JsonReader reader, Type objectType, LayerMask existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return reader.TokenType switch
		{
			JsonToken.Integer => Convert.ToInt32(reader.Value), 
			JsonToken.String => LayerMask.GetMask((string)reader.Value), 
			JsonToken.StartObject => UnityJsonConverters.I(JObject.Load(reader), "value"), 
			_ => default(LayerMask), 
		};
	}
}

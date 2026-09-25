using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Rust.Json;

public class Hash128Converter : JsonConverter<Hash128>
{
	public override void WriteJson(JsonWriter writer, Hash128 value, JsonSerializer serializer)
	{
		writer.WriteValue(value.ToString());
	}

	public override Hash128 ReadJson(JsonReader reader, Type objectType, Hash128 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.String)
		{
			return default(Hash128);
		}
		return Hash128.Parse((string)reader.Value);
	}
}

using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Rust.Json;

[Preserve]
public static class JsonSettingsBootstrap
{
	private static bool registered;

	[Preserve]
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void RegisterUnityConverters()
	{
		if (registered)
		{
			return;
		}
		registered = true;
		Func<JsonSerializerSettings> previous = JsonConvert.DefaultSettings;
		JsonConvert.DefaultSettings = delegate
		{
			JsonSerializerSettings jsonSerializerSettings = previous?.Invoke() ?? new JsonSerializerSettings();
			JsonConverter[] array = UnityJsonConverters.CreateAll();
			foreach (JsonConverter item in array)
			{
				jsonSerializerSettings.Converters.Add(item);
			}
			jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			return jsonSerializerSettings;
		};
	}
}

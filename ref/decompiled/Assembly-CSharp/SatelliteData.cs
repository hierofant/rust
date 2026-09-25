using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConVar;
using UnityEngine;

public class SatelliteData
{
	public enum SatelliteSize
	{
		Small,
		Medium,
		Large
	}

	public class ThrusterInfo
	{
		public int labelIndex;

		public ThrusterEffect effect;

		public string label => ThrusterLabels[labelIndex];
	}

	public enum ThrusterEffect
	{
		Left,
		Right,
		Forward,
		Backward,
		Tighten,
		Widen,
		RotateCW,
		RotateCCW
	}

	public int prefixIndex;

	public int nameNumber;

	public int payloadIndex;

	public float mass;

	public int fuel;

	public SatelliteSize size;

	public List<ThrusterInfo> thrusters;

	private static readonly string[] Prefixes = new string[6] { "COBALT", "RUSTSAT", "SCRAP", "RADTOWN", "OUTPOST", "BANDIT" };

	private static readonly Translate.Phrase[] Payloads = new Translate.Phrase[10]
	{
		new Translate.Phrase("satellite.payload.comms_relay", "COMMUNICATIONS RELAY"),
		new Translate.Phrase("satellite.payload.surveillance", "SURVEILLANCE PLATFORM"),
		new Translate.Phrase("satellite.payload.solar_observatory", "SOLAR OBSERVATORY"),
		new Translate.Phrase("satellite.payload.weather_station", "WEATHER STATION"),
		new Translate.Phrase("satellite.payload.nav_beacon", "NAVIGATION BEACON"),
		new Translate.Phrase("satellite.payload.sigint", "SIGNALS INTELLIGENCE"),
		new Translate.Phrase("satellite.payload.research_lab", "RESEARCH LABORATORY"),
		new Translate.Phrase("satellite.payload.early_warning", "EARLY WARNING SENSOR"),
		new Translate.Phrase("satellite.payload.fuel_depot", "ORBITAL FUEL DEPOT"),
		new Translate.Phrase("satellite.payload.telescope", "SPACE TELESCOPE")
	};

	private static readonly string[] ThrusterLabels = new string[8] { "7-4A", "2-8B", "5-1C", "9-3D", "1-6E", "4-2F", "8-5G", "3-9H" };

	private const int SeedSpreadPrime = 7919;

	private const float SmallSizeChance = 0.4f;

	private const float MediumSizeCumulative = 0.8f;

	private static readonly Translate.Phrase SizeSmallPhrase = new Translate.Phrase("satellite.size.small", "SMALL");

	private static readonly Translate.Phrase SizeMediumPhrase = new Translate.Phrase("satellite.size.medium", "MEDIUM");

	private static readonly Translate.Phrase SizeLargePhrase = new Translate.Phrase("satellite.size.large", "LARGE");

	public string name => $"{Prefixes[prefixIndex]}-{nameNumber}";

	public string payload => Payloads[payloadIndex].translated;

	public static List<SatelliteData> GenerateList(int count, int baseSeed)
	{
		List<SatelliteData> list = new List<SatelliteData>(count);
		for (int i = 0; i < count; i++)
		{
			int seed = baseSeed + i * 7919;
			list.Add(Generate(seed));
		}
		return list;
	}

	public static SatelliteData Generate(int seed)
	{
		System.Random random = new System.Random(seed);
		SatelliteData satelliteData = new SatelliteData();
		satelliteData.prefixIndex = random.Next(Prefixes.Length);
		satelliteData.nameNumber = random.Next(100, 9999);
		satelliteData.payloadIndex = random.Next(Payloads.Length);
		float num = (float)random.NextDouble();
		if (num < 0.4f)
		{
			satelliteData.size = SatelliteSize.Small;
			satelliteData.mass = 500f + (float)random.NextDouble() * 1000f;
		}
		else if (num < 0.8f)
		{
			satelliteData.size = SatelliteSize.Medium;
			satelliteData.mass = 1500f + (float)random.NextDouble() * 1500f;
		}
		else
		{
			satelliteData.size = SatelliteSize.Large;
			satelliteData.mass = 3000f + (float)random.NextDouble() * 2000f;
		}
		satelliteData.mass = Mathf.Round(satelliteData.mass);
		float t = (float)random.NextDouble();
		satelliteData.fuel = Mathf.RoundToInt(satelliteData.size switch
		{
			SatelliteSize.Small => Mathf.Lerp(Satellite.fuel_small_min, Satellite.fuel_small_max, t), 
			SatelliteSize.Medium => Mathf.Lerp(Satellite.fuel_medium_min, Satellite.fuel_medium_max, t), 
			SatelliteSize.Large => Mathf.Lerp(Satellite.fuel_large_min, Satellite.fuel_large_max, t), 
			_ => 18f, 
		});
		satelliteData.fuel = Mathf.Clamp(satelliteData.fuel, Satellite.fuel_min, Satellite.fuel_max);
		ThrusterEffect[] array = (ThrusterEffect[])Enum.GetValues(typeof(ThrusterEffect));
		int num2 = array.Length;
		satelliteData.thrusters = new List<ThrusterInfo>(num2);
		for (int num3 = num2 - 1; num3 > 0; num3--)
		{
			int num4 = random.Next(num3 + 1);
			ref ThrusterEffect reference = ref array[num3];
			ref ThrusterEffect reference2 = ref array[num4];
			ThrusterEffect thrusterEffect = array[num4];
			ThrusterEffect thrusterEffect2 = array[num3];
			reference = thrusterEffect;
			reference2 = thrusterEffect2;
		}
		for (int i = 0; i < num2; i++)
		{
			int num5 = random.Next(i + 1);
			int labelIndex = i;
			if (num5 != i)
			{
				labelIndex = satelliteData.thrusters[num5].labelIndex;
				satelliteData.thrusters[num5].labelIndex = i;
			}
			satelliteData.thrusters.Add(new ThrusterInfo
			{
				labelIndex = labelIndex,
				effect = array[i]
			});
		}
		return satelliteData;
	}

	public static string SerializeList(List<SatelliteData> satellites)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < satellites.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(';');
			}
			SatelliteData satelliteData = satellites[i];
			stringBuilder.Append(satelliteData.prefixIndex).Append('|').Append(satelliteData.nameNumber)
				.Append('|')
				.Append(satelliteData.payloadIndex)
				.Append('|')
				.Append(satelliteData.mass.ToString("F0", CultureInfo.InvariantCulture))
				.Append('|')
				.Append(satelliteData.fuel)
				.Append('|')
				.Append((int)satelliteData.size)
				.Append('|');
			for (int j = 0; j < satelliteData.thrusters.Count; j++)
			{
				if (j > 0)
				{
					stringBuilder.Append(',');
				}
				stringBuilder.Append(satelliteData.thrusters[j].labelIndex);
			}
		}
		return stringBuilder.ToString();
	}

	public static List<SatelliteData> DeserializeList(string data)
	{
		List<SatelliteData> list = new List<SatelliteData>();
		if (string.IsNullOrEmpty(data))
		{
			return list;
		}
		string[] array = data.Split(';');
		foreach (string text in array)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			string[] array2 = text.Split('|');
			if (array2.Length < 7)
			{
				continue;
			}
			SatelliteData satelliteData = new SatelliteData();
			if (int.TryParse(array2[0], out var result))
			{
				satelliteData.prefixIndex = result;
			}
			if (int.TryParse(array2[1], out var result2))
			{
				satelliteData.nameNumber = result2;
			}
			if (int.TryParse(array2[2], out var result3))
			{
				satelliteData.payloadIndex = result3;
			}
			if (float.TryParse(array2[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var result4))
			{
				satelliteData.mass = result4;
			}
			if (int.TryParse(array2[4], out var result5))
			{
				satelliteData.fuel = result5;
			}
			if (int.TryParse(array2[5], out var result6))
			{
				satelliteData.size = (SatelliteSize)result6;
			}
			satelliteData.thrusters = new List<ThrusterInfo>();
			string[] array3 = array2[6].Split(',');
			for (int j = 0; j < array3.Length; j++)
			{
				if (int.TryParse(array3[j], out var result7))
				{
					satelliteData.thrusters.Add(new ThrusterInfo
					{
						labelIndex = result7
					});
				}
			}
			list.Add(satelliteData);
		}
		return list;
	}

	public string GetSizeLabel()
	{
		return size switch
		{
			SatelliteSize.Small => SizeSmallPhrase.translated, 
			SatelliteSize.Medium => SizeMediumPhrase.translated, 
			SatelliteSize.Large => SizeLargePhrase.translated, 
			_ => "UNKNOWN", 
		};
	}

	public static string GetThrusterLabel(int index)
	{
		return ThrusterLabels[index];
	}

	public static string GetShortEffectLabel(ThrusterEffect effect)
	{
		return effect switch
		{
			ThrusterEffect.Forward => "N", 
			ThrusterEffect.Backward => "S", 
			ThrusterEffect.Right => "E", 
			ThrusterEffect.Left => "W", 
			ThrusterEffect.RotateCW => "RCW", 
			ThrusterEffect.RotateCCW => "RCCW", 
			ThrusterEffect.Tighten => "IN", 
			ThrusterEffect.Widen => "OUT", 
			_ => effect.ToString(), 
		};
	}
}

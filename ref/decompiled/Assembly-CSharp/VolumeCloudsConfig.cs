using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Volume Clouds/Cloud Config")]
public class VolumeCloudsConfig : ScriptableObject
{
	public VolumeCloudsWeatherLayerConfig WeatherLayer = new VolumeCloudsWeatherLayerConfig();

	[Range(0f, 1f)]
	[Header("Detail")]
	public float Billows = 1f;

	[Range(0f, 3f)]
	public float BillowsGamma = 2f;

	[Range(0f, 2f)]
	public float BillowsFrequencyCurve = 0.45f;

	[Range(0f, 1f)]
	public float Wisps = 1f;

	[Range(0f, 3f)]
	public float WispsGamma = 2f;

	[Range(0f, 2f)]
	public float WispsFrequencyCurve = 1f;

	[Range(0f, 2f)]
	public float DetailTypeDensityCurve = 0.25f;

	[Range(0.001f, 1f)]
	public float DetailTypeHeightTransition = 0.3f;

	[Range(0f, 2f)]
	public float CurlNoiseScale = 0.2f;

	public float CurlNoiseStrength = 50f;

	[Header("Density")]
	[Range(0f, 1f)]
	public float DensityScale = 0.05f;

	[Range(0f, 1f)]
	public float DensityCurve = 0.4f;

	[Range(0f, 1f)]
	public float WispsDensitySoftening = 0.3f;

	[Header("Multiple Scattering")]
	public float MSIntensity = 1f;

	[Range(0f, 1f)]
	public float MSAbsorption = 0.5f;

	[Range(0f, 2f)]
	public float MSDepthFalloff = 1f;

	[Range(0f, 1f)]
	public float MSMinDepth = 0.01f;

	[Header("Direct Scattering")]
	public Gradient SunColorScale;

	[Range(-1f, 1f)]
	public float SunEccentricity1 = 0.6f;

	[Range(-1f, 1f)]
	public float SunEccentricity2 = 0.2f;

	public Gradient MoonColorScale;

	[Range(-1f, 1f)]
	public float MoonEccentricity1 = 0.9f;

	[Range(-1f, 1f)]
	public float MoonEccentricity2 = 0.2f;

	[Range(0f, 2f)]
	public float MoonSaturation = 1f;

	[Range(0f, 3f)]
	public float DirectScatterBrightness = 1f;

	[Range(0f, 3f)]
	public float DirectScatterContrast = 1f;

	[Range(0f, 2f)]
	public float Absorption = 1f;

	[Header("Ambient Scattering")]
	public Gradient AmbientColorScale;

	[Range(0f, 1f)]
	public float AmbientSaturation = 0.6f;

	[Range(0f, 2f)]
	public float AmbientScatteringFalloff = 1f;

	[Range(0f, 3f)]
	public float AmbientScatterBrightness = 1f;

	[Range(0f, 3f)]
	public float AmbientScatterContrast = 1f;

	[Header("Other")]
	[Range(0f, 1f)]
	public float CoverageScale = 1f;

	[Range(0f, 1f)]
	public float CloudTypeTop = 1f;

	[Range(0f, 1f)]
	public float CloudTypeBottom;

	[Range(0f, 1f)]
	public float VerticalProfileTopEnd = 1f;

	[Range(0f, 1f)]
	public float VerticalProfileTopStart;

	[Range(0f, 1f)]
	public float VerticalProfileBottomStart = 1f;

	[Range(0f, 1f)]
	public float VerticalProfileBottomEnd;

	public float WindShear;

	public Vector2 WindVector = new Vector2(0f, 1f);

	[Range(0f, 1f)]
	public float ShadowDensityScale = 1f;

	[Range(0f, 1f)]
	public float AtmosphereShadowDensityScale = 1f;

	[Min(0f)]
	public float HazeDensity = 1f;

	[Min(0f)]
	public float HazeHeightFalloff = 0.035f;

	[Range(0f, 1f)]
	public float HorizonBuffer;

	public float EvalSunColorScale { get; set; }

	public float EvalMoonColorScale { get; set; }

	public float EvalAmbientColorScale { get; set; }

	public void CopyWeatherGen(VolumeCloudsConfig copy)
	{
		WeatherLayer.CopyFrom(copy.WeatherLayer);
	}

	public void CopyFrom(VolumeCloudsConfig copy)
	{
		CopyWeatherGen(copy);
		Billows = copy.Billows;
		BillowsGamma = copy.BillowsGamma;
		BillowsFrequencyCurve = copy.BillowsFrequencyCurve;
		Wisps = copy.Wisps;
		WispsGamma = copy.WispsGamma;
		WispsFrequencyCurve = copy.WispsFrequencyCurve;
		DetailTypeDensityCurve = copy.DetailTypeDensityCurve;
		DetailTypeHeightTransition = copy.DetailTypeHeightTransition;
		CurlNoiseScale = copy.CurlNoiseScale;
		CurlNoiseStrength = copy.CurlNoiseStrength;
		DensityScale = copy.DensityScale;
		DensityCurve = copy.DensityCurve;
		WispsDensitySoftening = copy.WispsDensitySoftening;
		SunEccentricity1 = copy.SunEccentricity1;
		SunEccentricity2 = copy.SunEccentricity2;
		AmbientScatteringFalloff = copy.AmbientScatteringFalloff;
		Absorption = copy.Absorption;
		SunColorScale.SetKeys(copy.SunColorScale.colorKeys, copy.SunColorScale.alphaKeys);
		AmbientColorScale.SetKeys(copy.AmbientColorScale.colorKeys, copy.AmbientColorScale.alphaKeys);
		AmbientSaturation = copy.AmbientSaturation;
		CoverageScale = copy.CoverageScale;
		MSIntensity = copy.MSIntensity;
		MSAbsorption = copy.MSAbsorption;
		MSDepthFalloff = copy.MSDepthFalloff;
		MSMinDepth = copy.MSMinDepth;
		CloudTypeTop = copy.CloudTypeTop;
		CloudTypeBottom = copy.CloudTypeBottom;
		MoonColorScale.SetKeys(copy.MoonColorScale.colorKeys, copy.MoonColorScale.alphaKeys);
		MoonEccentricity1 = copy.MoonEccentricity1;
		MoonEccentricity2 = copy.MoonEccentricity2;
		MoonSaturation = copy.MoonSaturation;
		VerticalProfileTopEnd = copy.VerticalProfileTopEnd;
		VerticalProfileTopStart = copy.VerticalProfileTopStart;
		VerticalProfileBottomStart = copy.VerticalProfileBottomStart;
		VerticalProfileBottomEnd = copy.VerticalProfileBottomEnd;
		DirectScatterBrightness = copy.DirectScatterBrightness;
		DirectScatterContrast = copy.DirectScatterContrast;
		AmbientScatterBrightness = copy.AmbientScatterBrightness;
		AmbientScatterContrast = copy.AmbientScatterContrast;
		WindShear = copy.WindShear;
		WindVector = copy.WindVector;
		ShadowDensityScale = copy.ShadowDensityScale;
		AtmosphereShadowDensityScale = copy.AtmosphereShadowDensityScale;
		HazeDensity = copy.HazeDensity;
		HazeHeightFalloff = copy.HazeHeightFalloff;
		HorizonBuffer = copy.HorizonBuffer;
		EvalSunColorScale = copy.EvalSunColorScale;
		EvalMoonColorScale = copy.EvalMoonColorScale;
		EvalAmbientColorScale = copy.EvalAmbientColorScale;
	}

	public void Lerp(VolumeCloudsConfig a, VolumeCloudsConfig b, float t)
	{
		Billows = Mathf.Lerp(a.Billows, b.Billows, t);
		BillowsGamma = Mathf.Lerp(a.BillowsGamma, b.BillowsGamma, t);
		BillowsFrequencyCurve = Mathf.Lerp(a.BillowsFrequencyCurve, b.BillowsFrequencyCurve, t);
		Wisps = Mathf.Lerp(a.Wisps, b.Wisps, t);
		WispsGamma = Mathf.Lerp(a.WispsGamma, b.WispsGamma, t);
		WispsFrequencyCurve = Mathf.Lerp(a.WispsFrequencyCurve, b.WispsFrequencyCurve, t);
		DetailTypeDensityCurve = Mathf.Lerp(a.DetailTypeDensityCurve, b.DetailTypeDensityCurve, t);
		DetailTypeHeightTransition = Mathf.Lerp(a.DetailTypeHeightTransition, b.DetailTypeHeightTransition, t);
		CurlNoiseScale = Mathf.Lerp(a.CurlNoiseScale, b.CurlNoiseScale, t);
		CurlNoiseStrength = Mathf.Lerp(a.CurlNoiseStrength, b.CurlNoiseStrength, t);
		DensityScale = Mathf.Lerp(a.DensityScale, b.DensityScale, t);
		DensityCurve = Mathf.Lerp(a.DensityCurve, b.DensityCurve, t);
		WispsDensitySoftening = Mathf.Lerp(a.WispsDensitySoftening, b.WispsDensitySoftening, t);
		MSIntensity = Mathf.Lerp(a.MSIntensity, b.MSIntensity, t);
		MSAbsorption = Mathf.Lerp(a.MSAbsorption, b.MSAbsorption, t);
		MSDepthFalloff = Mathf.Lerp(a.MSDepthFalloff, b.MSDepthFalloff, t);
		MSMinDepth = Mathf.Lerp(a.MSMinDepth, b.MSMinDepth, t);
		SunEccentricity1 = Mathf.Lerp(a.SunEccentricity1, b.SunEccentricity1, t);
		SunEccentricity2 = Mathf.Lerp(a.SunEccentricity2, b.SunEccentricity2, t);
		EvalAmbientColorScale = Mathf.Lerp(a.EvalAmbientColorScale, b.EvalAmbientColorScale, t);
		EvalSunColorScale = Mathf.Lerp(a.EvalSunColorScale, b.EvalSunColorScale, t);
		EvalMoonColorScale = Mathf.Lerp(a.EvalMoonColorScale, b.EvalMoonColorScale, t);
		AmbientSaturation = Mathf.Lerp(a.AmbientSaturation, b.AmbientSaturation, t);
		AmbientScatteringFalloff = Mathf.Lerp(a.AmbientScatteringFalloff, b.AmbientScatteringFalloff, t);
		Absorption = Mathf.Lerp(a.Absorption, b.Absorption, t);
		CoverageScale = Mathf.Lerp(a.CoverageScale, b.CoverageScale, t);
		CloudTypeTop = Mathf.Lerp(a.CloudTypeTop, b.CloudTypeTop, t);
		CloudTypeBottom = Mathf.Lerp(a.CloudTypeBottom, b.CloudTypeBottom, t);
		MoonEccentricity1 = Mathf.Lerp(a.MoonEccentricity1, b.MoonEccentricity1, t);
		MoonEccentricity2 = Mathf.Lerp(a.MoonEccentricity2, b.MoonEccentricity2, t);
		MoonSaturation = Mathf.Lerp(a.MoonSaturation, b.MoonSaturation, t);
		DirectScatterBrightness = Mathf.Lerp(a.DirectScatterBrightness, b.DirectScatterBrightness, t);
		DirectScatterContrast = Mathf.Lerp(a.DirectScatterContrast, b.DirectScatterContrast, t);
		AmbientScatterBrightness = Mathf.Lerp(a.AmbientScatterBrightness, b.AmbientScatterBrightness, t);
		AmbientScatterContrast = Mathf.Lerp(a.AmbientScatterContrast, b.AmbientScatterContrast, t);
		VerticalProfileTopStart = Mathf.Lerp(a.VerticalProfileTopStart, b.VerticalProfileTopStart, t);
		VerticalProfileTopEnd = Mathf.Lerp(a.VerticalProfileTopEnd, b.VerticalProfileTopEnd, t);
		VerticalProfileBottomStart = Mathf.Lerp(a.VerticalProfileBottomStart, b.VerticalProfileBottomStart, t);
		VerticalProfileBottomEnd = Mathf.Lerp(a.VerticalProfileBottomEnd, b.VerticalProfileBottomEnd, t);
		WindShear = Mathf.Lerp(a.WindShear, b.WindShear, t);
		WindVector = Vector2.Lerp(a.WindVector, b.WindVector, t);
		ShadowDensityScale = Mathf.Lerp(a.ShadowDensityScale, b.ShadowDensityScale, t);
		AtmosphereShadowDensityScale = Mathf.Lerp(a.AtmosphereShadowDensityScale, b.AtmosphereShadowDensityScale, t);
		HazeDensity = Mathf.Lerp(a.HazeDensity, b.HazeDensity, t);
		HazeHeightFalloff = Mathf.Lerp(a.HazeHeightFalloff, b.HazeHeightFalloff, t);
		HorizonBuffer = Mathf.Lerp(a.HorizonBuffer, b.HorizonBuffer, t);
	}
}

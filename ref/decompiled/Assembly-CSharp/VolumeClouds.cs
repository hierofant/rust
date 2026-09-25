using UnityEngine;

[ExecuteInEditMode]
public class VolumeClouds : SingletonComponent<VolumeClouds>
{
	public struct NoiseOffsets
	{
		public Vector2 CoverageBase;

		public Vector2 CoverageDetailPerlin;

		public Vector2 CoverageDetailWorley;

		public static NoiseOffsets Default(VolumeCloudsWeatherLayerConfig cfg)
		{
			NoiseOffsets result = default(NoiseOffsets);
			result.CoverageBase.x = cfg.CoverageBase.Offset.x;
			result.CoverageBase.y = cfg.CoverageBase.Offset.y;
			result.CoverageDetailPerlin.x = cfg.CoverageDetailPerlin.Offset.x;
			result.CoverageDetailPerlin.y = cfg.CoverageDetailPerlin.Offset.y;
			result.CoverageDetailWorley.x = cfg.CoverageDetailWorley.Offset.x;
			result.CoverageDetailWorley.y = cfg.CoverageDetailWorley.Offset.y;
			return result;
		}

		private static Vector2 RandVector(ref uint seed)
		{
			float x = SeedRandom.Range(++seed, -100f, 100f);
			float y = SeedRandom.Range(++seed, -100f, 100f);
			return new Vector2(x, y);
		}

		public static NoiseOffsets Random(uint seed)
		{
			NoiseOffsets result = default(NoiseOffsets);
			result.CoverageBase = RandVector(ref seed);
			result.CoverageDetailPerlin = RandVector(ref seed);
			result.CoverageDetailWorley = RandVector(ref seed);
			return result;
		}
	}

	public enum Quality
	{
		Off,
		Low,
		Medium,
		High,
		Ultra,
		COUNT
	}

	public bool UseRandomOffsets = true;

	public VolumeCloudsConfig DefaultConfig;

	public VolumeCloudsCirrusConfig DefaultCirrusConfig;

	public VolumeCloudsConfig DeepSeaPortalConfig;

	public float DeepSeaPortalLerp;

	private static readonly int PID_DetailNoise = Shader.PropertyToID("_DetailNoise");

	private static readonly int PID_DetailScale = Shader.PropertyToID("_DetailScale");

	private static readonly int PID_Billows = Shader.PropertyToID("_Billows");

	private static readonly int PID_BillowsGamma = Shader.PropertyToID("_BillowsGamma");

	private static readonly int PID_BillowsFrequencyCurve = Shader.PropertyToID("_BillowsFrequencyCurve");

	private static readonly int PID_Wisps = Shader.PropertyToID("_Wisps");

	private static readonly int PID_WispsGamma = Shader.PropertyToID("_WispsGamma");

	private static readonly int PID_WispsFrequencyCurve = Shader.PropertyToID("_WispsFrequencyCurve");

	private static readonly int PID_DetailTypeDensityCurve = Shader.PropertyToID("_DetailTypeDensityCurve");

	private static readonly int PID_DetailTypeHeightTransition = Shader.PropertyToID("_DetailTypeHeightTransition");

	private static readonly int PID_WeatherMap = Shader.PropertyToID("_VolumeCloudsWeatherMap");

	private static readonly int PID_WeatherScale = Shader.PropertyToID("_VolumeCloudsWeatherMapScale");

	private static readonly int PID_Wind = Shader.PropertyToID("_VolumeCloudsWindVector");

	private static readonly int PID_Coverage = Shader.PropertyToID("_Coverage");

	private static readonly int PID_DensityScale = Shader.PropertyToID("_DensityScale");

	private static readonly int PID_DensityCurve = Shader.PropertyToID("_DensityCurve");

	private static readonly int PID_WispsDensitySoftening = Shader.PropertyToID("_WispsDensitySoftening");

	private static readonly int PID_MSIntensity = Shader.PropertyToID("_MSIntensity");

	private static readonly int PID_MSAbsorption = Shader.PropertyToID("_MSAbsorption");

	private static readonly int PID_MSMinDepth = Shader.PropertyToID("_MSMinDepth");

	private static readonly int PID_MSDepthFalloff = Shader.PropertyToID("_MSDepthFalloff");

	private static readonly int PID_Eccentricity = Shader.PropertyToID("_Eccentricity");

	private static readonly int PID_AmbientScatteringFalloff = Shader.PropertyToID("_AmbientScatteringFalloff");

	private static readonly int PID_ScatterBrightnessContrast = Shader.PropertyToID("_ScatterBrightnessContrast");

	private static readonly int PID_Absorption = Shader.PropertyToID("_Absorption");

	private static readonly int PID_CloudTypeTop = Shader.PropertyToID("_CloudTypeTop");

	private static readonly int PID_CloudTypeBottom = Shader.PropertyToID("_CloudTypeBottom");

	private static readonly int PID_MipDistance = Shader.PropertyToID("_MipDistance");

	private static readonly int PID_BlueNoise = Shader.PropertyToID("_BlueNoise");

	private static readonly int PID_CirrusOpaqueness = Shader.PropertyToID("_CirrusOpaqueness");

	private static readonly int PID_CirrusAbsorption = Shader.PropertyToID("_CirrusAbsorption");

	private static readonly int PID_CirrusEccentricity = Shader.PropertyToID("_CirrusEccentricity");

	private static readonly int PID_CirrusWeatherMap = Shader.PropertyToID("_CirrusWeatherMap");

	private static readonly int PID_CirrusWeatherMapScale = Shader.PropertyToID("_CirrusWeatherMapScale");

	private static readonly int PID_OutputTexture = Shader.PropertyToID("_OutputImage");

	private static readonly int PID_OutputImageSize = Shader.PropertyToID("_OutputImageSize");

	private static readonly int PID_CamPos = Shader.PropertyToID("_CamPos");

	private static readonly int PID_VolumeCloudTex = Shader.PropertyToID("_VolumeCloudTex");

	private static readonly int PID_VolumeCloudReflTex = Shader.PropertyToID("_VolumeCloudReflTex");

	private static readonly int PID_CloudTexSize = Shader.PropertyToID("_CloudTexSize");

	private static readonly int PID_FrameCount = Shader.PropertyToID("_FrameCount");

	private static readonly int PID_CamProjInv = Shader.PropertyToID("_CamProjInv");

	private static readonly int PID_StepCount = Shader.PropertyToID("_StepCount");

	private static readonly int PID_StepCountLow = Shader.PropertyToID("_StepCountLow");

	private static readonly int PID_VerticalProfileTopOffset = Shader.PropertyToID("_VerticalProfileTopOffset");

	private static readonly int PID_VerticalProfileTopScale = Shader.PropertyToID("_VerticalProfileTopScale");

	private static readonly int PID_VerticalProfileBottomOffset = Shader.PropertyToID("_VerticalProfileBottomOffset");

	private static readonly int PID_VerticalProfileBottomScale = Shader.PropertyToID("_VerticalProfileBottomScale");

	private static readonly int PID_WeatherScrollOffset = Shader.PropertyToID("_VolumeCloudsWeatherScrollOffset");

	private static readonly int PID_BlueNoiseOffset = Shader.PropertyToID("_BlueNoiseOffset");

	private static readonly int PID_RenderType = Shader.PropertyToID("_RenderType");

	private static readonly int PID_NumRadialLayers = Shader.PropertyToID("_NumRadialLayers");

	private static readonly int PID_LerpT = Shader.PropertyToID("_LerpT");

	private static readonly int PID_LerpSourceImage = Shader.PropertyToID("_LerpSourceImage");

	private static readonly int PID_LerpTargetImage = Shader.PropertyToID("_LerpTargetImage");

	private static readonly int PID_DepthBuffer = Shader.PropertyToID("_CloudDepthBuffer");

	private static readonly int PID_CamInvViewMatrix = Shader.PropertyToID("_CamInvViewMatrix");

	private static readonly int PID_PrevImage = Shader.PropertyToID("_PrevOutputImage");

	private static readonly int PID_LowResBuffer = Shader.PropertyToID("_LowResBuffer");

	private static readonly int PID_SizeDivisor = Shader.PropertyToID("_SizeDivisor");

	private static readonly int PID_ReprojectionMatrix = Shader.PropertyToID("_ReprojectionMatrix");

	private static readonly int PID_FirstFrame = Shader.PropertyToID("_FirstFrame");

	private static readonly int PID_CurrBlockOffsetX = Shader.PropertyToID("_CurrBlockOffsetX");

	private static readonly int PID_CurrBlockOffsetY = Shader.PropertyToID("_CurrBlockOffsetY");

	private static readonly int PID_ShadowInvViewProjMatrix = Shader.PropertyToID("_ShadowInvViewProjMatrix");

	private static readonly int PID_CloudShadowViewProj = Shader.PropertyToID("_CloudShadowViewProj");

	private static readonly int PID_CloudShadowMap = Shader.PropertyToID("_CloudShadowMap");

	private static readonly int PID_CloudShadowNearFar = Shader.PropertyToID("_CloudShadowNearFar");

	private static readonly int PID_SunColorScale = Shader.PropertyToID("_VolumeCloudsSunColorScale");

	private static readonly int PID_MoonColorScale = Shader.PropertyToID("_VolumeCloudsMoonColorScale");

	private static readonly int PID_AmbientColorScale = Shader.PropertyToID("_VolumeCloudsAmbientColorScale");

	private static readonly int PID_AmbientSaturation = Shader.PropertyToID("_VolumeCloudsAmbientSaturation");

	private static readonly int PID_MoonSaturation = Shader.PropertyToID("_VolumeCloudsMoonSaturation");

	private static readonly int PID_VolumeCloudsGroundRadius = Shader.PropertyToID("_VolumeCloudsGroundRadius");

	private static readonly int PID_CloudAmbientLight = Shader.PropertyToID("_CloudAmbientLight");

	private static readonly int PID_WindShear = Shader.PropertyToID("_WindShear");

	private static readonly int PID_ShadowDensityScale = Shader.PropertyToID("_VolumeCloudsShadowDensityScale");

	private static readonly int PID_AtmosphereShadowDensityScale = Shader.PropertyToID("_VolumeCloudsAtmosphereShadowDensityScale");

	private static readonly int PID_CloudHazeDensity = Shader.PropertyToID("_CloudHazeDensity");

	private static readonly int PID_CloudHazeHeightFalloff = Shader.PropertyToID("_CloudHazeHeightFalloff");

	private static readonly int PID_CloudHazeRampStartDistance = Shader.PropertyToID("_CloudHazeRampStartDistance");

	private static readonly int PID_CloudHazeRampEndDistance = Shader.PropertyToID("_CloudHazeRampEndDistance");

	private static readonly int PID_TODLerpValue = Shader.PropertyToID("_TOD_LerpValue");

	private static readonly int PID_HorizonBuffer = Shader.PropertyToID("_HorizonBuffer");

	private static readonly int PID_ApplySunContrast = Shader.PropertyToID("_VCloudsSunContrast");

	private static readonly int PID_ApplySunFogginess = Shader.PropertyToID("_VCloudsSunFogginess");

	private static readonly int PID_DetailScrollOffset = Shader.PropertyToID("_DetailScrollOffset");

	private static readonly int PID_LerpedWeatherTextureCopy = Shader.PropertyToID("_LerpedWeatherTextureCopy");

	private int[,] PID_InstabilityParams;

	private int[,] PID_CoverageBaseParams;

	private int[,] PID_CoverageDetailParams;

	private int[,] PID_CoverageDetailWParams;

	private int[,] PID_RadialLayerParams;

	private int[,] PID_CurlNoiseParams;

	public ComputeShader WeatherGenShader;

	[Header("Shaders/Materials")]
	public ComputeShader MarchShader;

	public ComputeShader UpscaleShader;

	public ComputeShader ShadowMapShader;

	public ComputeShader AmbientShader;

	public ComputeShader AtmosphericScatteringShader;

	public Quality QualityLevel = Quality.High;

	private bool _cloudsEnabled;

	[Header("Textures")]
	public Texture CurlNoise;

	public Texture DetailNoise;

	public Texture BlueNoise;

	public float WeatherTextureScale = 0.15f;

	public float CirrusWeatherTextureScale = 0.15f;

	public float DetailNoiseScale = 1.3f;

	public float MipDistance = 25000f;

	public float GroundRadius = 60000f;

	public bool SunColorApplyContrast;

	public bool SunColorApplyFogginess;

	public Vector2i AtmosphericScatteringResolution = new Vector2i(128, 128);

	public bool CloudsShouldBeEnabled
	{
		get
		{
			if (QualityLevel != 0)
			{
				return base.enabled;
			}
			return false;
		}
	}

	public bool CloudsEnabled => _cloudsEnabled;

	private void FetchShaderPropertyIDs()
	{
		char[] array = new char[4] { 'A', 'B', 'C', 'D' };
		PID_InstabilityParams = new int[4, 3];
		PID_CoverageBaseParams = new int[4, 3];
		PID_CoverageDetailParams = new int[4, 3];
		PID_CoverageDetailWParams = new int[4, 2];
		PID_RadialLayerParams = new int[3, 2];
		PID_CurlNoiseParams = new int[4, 1];
		for (int i = 0; i < 4; i++)
		{
			PID_InstabilityParams[i, 0] = Shader.PropertyToID($"_InstabilityParams{array[i]}0");
			PID_InstabilityParams[i, 1] = Shader.PropertyToID($"_InstabilityParams{array[i]}1");
			PID_InstabilityParams[i, 2] = Shader.PropertyToID($"_InstabilityParams{array[i]}2");
			PID_CoverageBaseParams[i, 0] = Shader.PropertyToID($"_CoverageBaseParams{array[i]}0");
			PID_CoverageBaseParams[i, 1] = Shader.PropertyToID($"_CoverageBaseParams{array[i]}1");
			PID_CoverageBaseParams[i, 2] = Shader.PropertyToID($"_CoverageBaseParams{array[i]}2");
			PID_CoverageDetailParams[i, 0] = Shader.PropertyToID($"_CoverageDetailParams{array[i]}0");
			PID_CoverageDetailParams[i, 1] = Shader.PropertyToID($"_CoverageDetailParams{array[i]}1");
			PID_CoverageDetailParams[i, 2] = Shader.PropertyToID($"_CoverageDetailParams{array[i]}2");
			PID_CoverageDetailWParams[i, 0] = Shader.PropertyToID($"_CoverageDetailWParams{array[i]}0");
			PID_CoverageDetailWParams[i, 1] = Shader.PropertyToID($"_CoverageDetailWParams{array[i]}1");
			PID_CurlNoiseParams[i, 0] = Shader.PropertyToID($"_CurlParams{array[i]}0");
		}
		for (int j = 0; j < 3; j++)
		{
			PID_RadialLayerParams[j, 0] = Shader.PropertyToID($"_RadialLayerParams{array[j]}0");
			PID_RadialLayerParams[j, 1] = Shader.PropertyToID($"_RadialLayerParams{array[j]}1");
		}
	}
}

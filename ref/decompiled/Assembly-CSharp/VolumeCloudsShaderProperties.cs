using UnityEngine;

public static class VolumeCloudsShaderProperties
{
	public static readonly int HasVClouds = Shader.PropertyToID("_HasVClouds");

	public static readonly int DetailNoise = Shader.PropertyToID("_DetailNoise");

	public static readonly int DetailScale = Shader.PropertyToID("_DetailScale");

	public static readonly int Billows = Shader.PropertyToID("_Billows");

	public static readonly int BillowsGamma = Shader.PropertyToID("_BillowsGamma");

	public static readonly int BillowsFrequencyCurve = Shader.PropertyToID("_BillowsFrequencyCurve");

	public static readonly int Wisps = Shader.PropertyToID("_Wisps");

	public static readonly int WispsGamma = Shader.PropertyToID("_WispsGamma");

	public static readonly int WispsFrequencyCurve = Shader.PropertyToID("_WispsFrequencyCurve");

	public static readonly int DetailTypeDensityCurve = Shader.PropertyToID("_DetailTypeDensityCurve");

	public static readonly int DetailTypeHeightTransition = Shader.PropertyToID("_DetailTypeHeightTransition");

	public static readonly int WeatherMap = Shader.PropertyToID("_VolumeCloudsWeatherMap");

	public static readonly int WeatherScale = Shader.PropertyToID("_VolumeCloudsWeatherMapScale");

	public static readonly int Wind = Shader.PropertyToID("_VolumeCloudsWindVector");

	public static readonly int Coverage = Shader.PropertyToID("_Coverage");

	public static readonly int DensityScale = Shader.PropertyToID("_DensityScale");

	public static readonly int DensityCurve = Shader.PropertyToID("_DensityCurve");

	public static readonly int WispsDensitySoftening = Shader.PropertyToID("_WispsDensitySoftening");

	public static readonly int MSIntensity = Shader.PropertyToID("_MSIntensity");

	public static readonly int MSAbsorption = Shader.PropertyToID("_MSAbsorption");

	public static readonly int MSMinDepth = Shader.PropertyToID("_MSMinDepth");

	public static readonly int MSDepthFalloff = Shader.PropertyToID("_MSDepthFalloff");

	public static readonly int Eccentricity = Shader.PropertyToID("_Eccentricity");

	public static readonly int AmbientScatteringFalloff = Shader.PropertyToID("_AmbientScatteringFalloff");

	public static readonly int ScatterBrightnessContrast = Shader.PropertyToID("_ScatterBrightnessContrast");

	public static readonly int Absorption = Shader.PropertyToID("_Absorption");

	public static readonly int CloudTypeTop = Shader.PropertyToID("_CloudTypeTop");

	public static readonly int CloudTypeBottom = Shader.PropertyToID("_CloudTypeBottom");

	public static readonly int MipDistance = Shader.PropertyToID("_MipDistance");

	public static readonly int BlueNoise = Shader.PropertyToID("_BlueNoise");

	public static readonly int CirrusOpaqueness = Shader.PropertyToID("_CirrusOpaqueness");

	public static readonly int CirrusAbsorption = Shader.PropertyToID("_CirrusAbsorption");

	public static readonly int CirrusEccentricity = Shader.PropertyToID("_CirrusEccentricity");

	public static readonly int CirrusWeatherMap = Shader.PropertyToID("_CirrusWeatherMap");

	public static readonly int CirrusWeatherMapScale = Shader.PropertyToID("_CirrusWeatherMapScale");

	public static readonly int OutputTexture = Shader.PropertyToID("_OutputImage");

	public static readonly int OutputImageSize = Shader.PropertyToID("_OutputImageSize");

	public static readonly int CamPos = Shader.PropertyToID("_CamPos");

	public static readonly int VolumeCloudTex = Shader.PropertyToID("_VolumeCloudTex");

	public static readonly int VolumeCloudReflTex = Shader.PropertyToID("_VolumeCloudReflTex");

	public static readonly int CloudTexSize = Shader.PropertyToID("_CloudTexSize");

	public static readonly int FrameCount = Shader.PropertyToID("_FrameCount");

	public static readonly int CamProjInv = Shader.PropertyToID("_CamProjInv");

	public static readonly int StepCount = Shader.PropertyToID("_StepCount");

	public static readonly int StepCountLow = Shader.PropertyToID("_StepCountLow");

	public static readonly int VerticalProfileTopOffset = Shader.PropertyToID("_VerticalProfileTopOffset");

	public static readonly int VerticalProfileTopScale = Shader.PropertyToID("_VerticalProfileTopScale");

	public static readonly int VerticalProfileBottomOffset = Shader.PropertyToID("_VerticalProfileBottomOffset");

	public static readonly int VerticalProfileBottomScale = Shader.PropertyToID("_VerticalProfileBottomScale");

	public static readonly int WeatherScrollOffset = Shader.PropertyToID("_VolumeCloudsWeatherScrollOffset");

	public static readonly int BlueNoiseOffset = Shader.PropertyToID("_BlueNoiseOffset");

	public static readonly int RenderType = Shader.PropertyToID("_RenderType");

	public static readonly int NumRadialLayers = Shader.PropertyToID("_NumRadialLayers");

	public static readonly int LerpT = Shader.PropertyToID("_LerpT");

	public static readonly int LerpSourceImage = Shader.PropertyToID("_LerpSourceImage");

	public static readonly int LerpTargetImage = Shader.PropertyToID("_LerpTargetImage");

	public static readonly int DepthBuffer = Shader.PropertyToID("_CloudDepthBuffer");

	public static readonly int CamInvViewMatrix = Shader.PropertyToID("_CamInvViewMatrix");

	public static readonly int PrevImage = Shader.PropertyToID("_PrevOutputImage");

	public static readonly int LowResBuffer = Shader.PropertyToID("_LowResBuffer");

	public static readonly int SizeDivisor = Shader.PropertyToID("_SizeDivisor");

	public static readonly int ReprojectionMatrix = Shader.PropertyToID("_ReprojectionMatrix");

	public static readonly int FirstFrame = Shader.PropertyToID("_FirstFrame");

	public static readonly int CurrBlockOffsetX = Shader.PropertyToID("_CurrBlockOffsetX");

	public static readonly int CurrBlockOffsetY = Shader.PropertyToID("_CurrBlockOffsetY");

	public static readonly int ShadowInvViewProjMatrix = Shader.PropertyToID("_ShadowInvViewProjMatrix");

	public static readonly int CloudShadowViewProj = Shader.PropertyToID("_CloudShadowViewProj");

	public static readonly int CloudShadowMap = Shader.PropertyToID("_CloudShadowMap");

	public static readonly int CloudShadowNearFar = Shader.PropertyToID("_CloudShadowNearFar");

	public static readonly int SunColorScale = Shader.PropertyToID("_VolumeCloudsSunColorScale");

	public static readonly int MoonColorScale = Shader.PropertyToID("_VolumeCloudsMoonColorScale");

	public static readonly int AmbientColorScale = Shader.PropertyToID("_VolumeCloudsAmbientColorScale");

	public static readonly int AmbientSaturation = Shader.PropertyToID("_VolumeCloudsAmbientSaturation");

	public static readonly int MoonSaturation = Shader.PropertyToID("_VolumeCloudsMoonSaturation");

	public static readonly int VolumeCloudsGroundRadius = Shader.PropertyToID("_VolumeCloudsGroundRadius");

	public static readonly int CloudAmbientLight = Shader.PropertyToID("_CloudAmbientLight");

	public static readonly int WindShear = Shader.PropertyToID("_WindShear");

	public static readonly int ShadowDensityScale = Shader.PropertyToID("_VolumeCloudsShadowDensityScale");

	public static readonly int AtmosphereShadowDensityScale = Shader.PropertyToID("_VolumeCloudsAtmosphereShadowDensityScale");

	public static readonly int CloudHazeDensity = Shader.PropertyToID("_CloudHazeDensity");

	public static readonly int CloudHazeHeightFalloff = Shader.PropertyToID("_CloudHazeHeightFalloff");

	public static readonly int CloudHazeRampStartDistance = Shader.PropertyToID("_CloudHazeRampStartDistance");

	public static readonly int CloudHazeRampEndDistance = Shader.PropertyToID("_CloudHazeRampEndDistance");

	public static readonly int TODLerpValue = Shader.PropertyToID("_TOD_LerpValue");

	public static readonly int HorizonBuffer = Shader.PropertyToID("_HorizonBuffer");

	public static readonly int ApplySunContrast = Shader.PropertyToID("_VCloudsSunContrast");

	public static readonly int ApplySunFogginess = Shader.PropertyToID("_VCloudsSunFogginess");

	public static readonly int DetailScrollOffset = Shader.PropertyToID("_DetailScrollOffset");

	public static readonly int LerpedWeatherTextureCopy = Shader.PropertyToID("_LerpedWeatherTextureCopy");

	public static readonly int FarFieldAtmosphere = Shader.PropertyToID("_FarFieldAtmosphere");

	public static int[,] InstabilityParams;

	public static int[,] CoverageBaseParams;

	public static int[,] CoverageDetailParams;

	public static int[,] CoverageDetailWParams;

	public static int[,] RadialLayerParams;

	public static int[,] CurlNoiseParams;

	private static bool _shaderPropsFetched = false;

	public static void FetchShaderPropertyIDs()
	{
		if (!_shaderPropsFetched)
		{
			_shaderPropsFetched = true;
			char[] array = new char[4] { 'A', 'B', 'C', 'D' };
			InstabilityParams = new int[4, 3];
			CoverageBaseParams = new int[4, 3];
			CoverageDetailParams = new int[4, 3];
			CoverageDetailWParams = new int[4, 2];
			RadialLayerParams = new int[3, 2];
			CurlNoiseParams = new int[4, 1];
			for (int i = 0; i < 4; i++)
			{
				InstabilityParams[i, 0] = Shader.PropertyToID($"_InstabilityParams{array[i]}0");
				InstabilityParams[i, 1] = Shader.PropertyToID($"_InstabilityParams{array[i]}1");
				InstabilityParams[i, 2] = Shader.PropertyToID($"_InstabilityParams{array[i]}2");
				CoverageBaseParams[i, 0] = Shader.PropertyToID($"_CoverageBaseParams{array[i]}0");
				CoverageBaseParams[i, 1] = Shader.PropertyToID($"_CoverageBaseParams{array[i]}1");
				CoverageBaseParams[i, 2] = Shader.PropertyToID($"_CoverageBaseParams{array[i]}2");
				CoverageDetailParams[i, 0] = Shader.PropertyToID($"_CoverageDetailParams{array[i]}0");
				CoverageDetailParams[i, 1] = Shader.PropertyToID($"_CoverageDetailParams{array[i]}1");
				CoverageDetailParams[i, 2] = Shader.PropertyToID($"_CoverageDetailParams{array[i]}2");
				CoverageDetailWParams[i, 0] = Shader.PropertyToID($"_CoverageDetailWParams{array[i]}0");
				CoverageDetailWParams[i, 1] = Shader.PropertyToID($"_CoverageDetailWParams{array[i]}1");
				CurlNoiseParams[i, 0] = Shader.PropertyToID($"_CurlParams{array[i]}0");
			}
			for (int j = 0; j < 3; j++)
			{
				RadialLayerParams[j, 0] = Shader.PropertyToID($"_RadialLayerParams{array[j]}0");
				RadialLayerParams[j, 1] = Shader.PropertyToID($"_RadialLayerParams{array[j]}1");
			}
		}
	}
}

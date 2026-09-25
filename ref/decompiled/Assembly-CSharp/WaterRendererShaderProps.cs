using UnityEngine;

public static class WaterRendererShaderProps
{
	public static readonly int PreviousVP = Shader.PropertyToID("_PreviousVP");

	public static readonly int NonJitteredVP = Shader.PropertyToID("_NonJitteredVP");

	public static readonly int NonJitteredInvVP = Shader.PropertyToID("_NonJitteredInvVP");

	public static readonly int CausticsTex = Shader.PropertyToID("_CausticsTex");

	public static readonly int CausticsSpeed = Shader.PropertyToID("_CausticsSpeed");

	public static readonly int CausticsScale = Shader.PropertyToID("_CausticsScale");

	public static readonly int CausticsBrightness = Shader.PropertyToID("_CausticsBrightness");

	public static readonly int WaterCaustics_LightDirection = Shader.PropertyToID("_WaterCaustics_LightDirection");

	public static readonly int Water_CausticsTex = Shader.PropertyToID("_Water_CausticsTex");

	public static readonly int Water_OceanVFace = Shader.PropertyToID("_Water_OceanVFace");

	public static readonly int Water_CameraView = Shader.PropertyToID("_Water_CameraView");

	public static readonly int Water_CameraViewProj = Shader.PropertyToID("_Water_CameraViewProj");

	public static readonly int Water_CameraShoreVector = Shader.PropertyToID("_Water_CameraShoreVector");

	public static readonly int WaterSSR_CameraProj = Shader.PropertyToID("_WaterSSR_CameraProj");

	public static readonly int FrustumNearCorners = Shader.PropertyToID("_FrustumNearCorners");

	public static readonly int FrustumRayCorners = Shader.PropertyToID("_FrustumRayCorners");

	public static readonly int WaterSSR_WorldNormTexture = Shader.PropertyToID("_WaterSSR_WorldNormTexture");

	public static readonly int WaterSSR_ReflectionTexture = Shader.PropertyToID("_WaterSSR_ReflectionTexture");

	public static readonly int WaterSurfaceTexture = Shader.PropertyToID("_WaterSurfaceTexture");

	public static readonly int WaterSurfaceMaskTexture = Shader.PropertyToID("_WaterSurfaceMaskTexture");

	public static readonly int WaterPreFogBackgroundTexture = Shader.PropertyToID("_WaterPreFogBackgroundTexture");

	public static readonly int WaterMotionTexture = Shader.PropertyToID("_WaterMotionTexture");

	public static readonly int Water_CullingVolumeArray = Shader.PropertyToID("_Water_CullingVolumeArray");

	public static readonly int Water_CullingVolumeCount = Shader.PropertyToID("_Water_CullingVolumeCount");

	public static readonly int WaterSurfaceWaterColor = Shader.PropertyToID("_WaterSurfaceWaterColor");

	public static readonly int WaterSurfaceAlbedo = Shader.PropertyToID("_WaterSurfaceAlbedo");

	public static readonly int BackgroundColorTexture = Shader.PropertyToID("_BackgroundColorTexture");

	public static readonly int UnderwaterColorTexture = Shader.PropertyToID("_UnderwaterColorTexture");

	public static readonly int UnderwaterFogEnabled = Shader.PropertyToID("_UnderwaterFogEnabled");

	public static readonly int WorldSpaceLightPos0 = Shader.PropertyToID("_WorldSpaceLightPos0");

	public static readonly int LightColor0 = Shader.PropertyToID("_LightColor0");

	public static readonly int UnderwaterMaskZBuffer = Shader.PropertyToID("_UnderwaterMaskZBuffer");

	public static readonly int SurfaceMaskTexCopy = Shader.PropertyToID("_SurfaceMaskTexCopy");

	public static readonly int OceanSurfaceMaskTex = Shader.PropertyToID("_OceanSurfaceMaskTex");

	public static readonly int Terrain_ShoreVector = Shader.PropertyToID("Terrain_ShoreVector");

	public static readonly int Terrain_HeightSlope = Shader.PropertyToID("Terrain_HeightSlope");

	public static readonly int PreFogCopy = Shader.PropertyToID("_PreFogCopy");

	public static readonly int[] BiomeRiverOverride = new int[5]
	{
		Shader.PropertyToID("_AridColorOverride"),
		Shader.PropertyToID("_TemperateColorOverride"),
		Shader.PropertyToID("_TundraColorOverride"),
		Shader.PropertyToID("_ArcticColorOverride"),
		Shader.PropertyToID("_JungleColorOverride")
	};

	public static readonly int[] BiomeRiverWaterColor = new int[5]
	{
		Shader.PropertyToID("_AridWaterColor"),
		Shader.PropertyToID("_TemperateWaterColor"),
		Shader.PropertyToID("_TundraWaterColor"),
		Shader.PropertyToID("_ArcticWaterColor"),
		Shader.PropertyToID("_JungleWaterColor")
	};

	public static readonly int[] BiomeRiverColorExtinction = new int[5]
	{
		Shader.PropertyToID("_AridExtinction"),
		Shader.PropertyToID("_TemperateExtinction"),
		Shader.PropertyToID("_TundraExtinction"),
		Shader.PropertyToID("_ArcticExtinction"),
		Shader.PropertyToID("_JungleExtinction")
	};

	public static readonly int[] BiomeRiverScatterCoefficient = new int[5]
	{
		Shader.PropertyToID("_AridScatterCoefficient"),
		Shader.PropertyToID("_TemperateScatterCoefficient"),
		Shader.PropertyToID("_TundraScatterCoefficient"),
		Shader.PropertyToID("_ArcticScatterCoefficient"),
		Shader.PropertyToID("_JungleScatterCoefficient")
	};

	public static readonly int[] BiomeRiverAlbedo = new int[5]
	{
		Shader.PropertyToID("_AridColor"),
		Shader.PropertyToID("_TemperateColor"),
		Shader.PropertyToID("_TundraColor"),
		Shader.PropertyToID("_ArcticColor"),
		Shader.PropertyToID("_JungleColor")
	};

	public static readonly int WaterColor = Shader.PropertyToID("_WaterColor");

	public static readonly int ColorExtinction = Shader.PropertyToID("_ColorExtinction");

	public static readonly int ScatterCoefficient = Shader.PropertyToID("_ScatterCoefficient");

	public static readonly int WaterAlbedo = Shader.PropertyToID("_Color");

	public static readonly int UnderwaterExtinction = Shader.PropertyToID("_UnderwaterExtinction");

	public static readonly int UnderwaterScatterCoefficient = Shader.PropertyToID("_UnderwaterScatterCoefficient");

	public static readonly int ScreenSpaceDLightMultTexture = Shader.PropertyToID("_ScreenSpaceDLightMultTexture");

	public static readonly int WaterInteraction_WorldOffset = Shader.PropertyToID("_WaterInteraction_WorldOffset");

	public static readonly int WaterInteractionMap_Desc = Shader.PropertyToID("_WaterInteractionMap_Desc");

	public static readonly int InteractionTex = Shader.PropertyToID("_InteractionTex");

	public static readonly int InteractionTexture = Shader.PropertyToID("_InteractionTexture");

	public static readonly int InteractionTextureSize = Shader.PropertyToID("_InteractionTextureSize");

	public static readonly int InteractionInstanceBuffer = Shader.PropertyToID("_InteractionInstanceBuffer");

	public static readonly int WaterInteractionMap = Shader.PropertyToID("_WaterInteractionMap");
}

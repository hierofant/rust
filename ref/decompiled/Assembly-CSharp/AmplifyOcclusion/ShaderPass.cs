namespace AmplifyOcclusion;

internal static class ShaderPass
{
	public const int CombineDownsampledOcclusionDepth = 16;

	public const int CombineEmission = 17;

	public const int CombineEmissionLog = 18;

	public const int BlurHorizontal1 = 0;

	public const int BlurVertical1 = 1;

	public const int BlurHorizontal2 = 2;

	public const int BlurVertical2 = 3;

	public const int BlurHorizontal3 = 4;

	public const int BlurVertical3 = 5;

	public const int BlurHorizontal4 = 6;

	public const int BlurVertical4 = 7;

	public const int ApplyDebug = 0;

	public const int ApplyDebugTemporal = 1;

	public const int ApplyDeferred = 5;

	public const int ApplyDeferredTemporal = 6;

	public const int ApplyDeferredLog = 10;

	public const int ApplyDeferredLogTemporal = 11;

	public const int ApplyPostEffect = 15;

	public const int ApplyPostEffectTemporal = 16;

	public const int ApplyPostEffectTemporalMultiply = 20;

	public const int OcclusionLow_None = 0;

	public const int OcclusionLow_Camera = 1;

	public const int OcclusionLow_GBuffer = 2;

	public const int OcclusionLow_GBufferOctaEncoded = 3;
}

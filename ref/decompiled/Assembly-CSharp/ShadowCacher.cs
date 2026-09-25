using System;
using System.Collections;
using ConVar;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[Serializable]
public class ShadowCacher
{
	private const int CACHE_TEXTURE_RESOLUTION = 256;

	private const float CACHE_TEXTURE_FRAGMENT_SIZE = 0.00390625f;

	private const GraphicsFormat CACHE_TEXTURE_FORMAT = GraphicsFormat.R32G32B32A32_SFloat;

	private static readonly int tempShadowMapCacheId = Shader.PropertyToID("_TempShadowMapCache");

	private static readonly int tempGaussianBlurId = Shader.PropertyToID("_TempGaussianBlur");

	private static readonly int blurDirectionId = Shader.PropertyToID("_BlurDirection");

	private static readonly int shadowMapTextureId = Shader.PropertyToID("_ShadowMapTexture");

	private static readonly int cubemapFaceId = Shader.PropertyToID("_CubemapFace");

	private static readonly int originalLightCookieId = Shader.PropertyToID("_OriginalLightCookie");

	private static readonly int shadowNearPlaneId = Shader.PropertyToID("_ShadowNearPlane");

	private static bool staticResourcesLoaded;

	private static Material copyShadowMapMat;

	private static Material gaussianBlurMat;

	private static Cubemap defaultWhiteCubemap;

	private static UnityEngine.Texture defaultSpotLightCookie;

	private static RenderTexturePool spotLightRtPool;

	private static RenderTexturePool pointLightRtPool;

	[SerializeField]
	private float refreshDistanceDelta = 0.01f;

	private Light light;

	private LightLOD lightLod;

	private CommandBuffer shadowCopyCommandBuffer;

	private RenderTexture cachedShadowMap;

	private bool pendingCapture;

	private UnityEngine.Texture originalCookieTexture;

	private Coroutine captureRoutine;

	private Vector3 lastRefreshPosition;

	private float shadowCacheRefreshTimer;

	private bool isInitialized;

	private float currentShadowFrameRate;

	private RenderTexturePool renderTexturePool;

	public void Initialize(Light light, LightLOD lightLod)
	{
		if (isInitialized)
		{
			return;
		}
		this.light = light;
		this.lightLod = lightLod;
		if (!staticResourcesLoaded)
		{
			Shader shader = Shader.Find("Hidden/ShadowCacheCopy");
			if (shader == null)
			{
				Debug.LogError("Failed to find the copy shader for shadow caching!", lightLod);
			}
			else
			{
				copyShadowMapMat = new Material(shader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			Shader shader2 = Shader.Find("Hidden/ShadowCacheGaussianBlur");
			if (shader2 == null)
			{
				Debug.LogError("Failed to find the Gaussian Blur shader for shadow caching!", lightLod);
			}
			else
			{
				gaussianBlurMat = new Material(shader2)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
			defaultWhiteCubemap = Resources.Load<Cubemap>("ShadowCaching/WhiteCubemap");
			if (defaultWhiteCubemap == null)
			{
				Debug.LogError("Failed to load default white cubemap texture for shadow caching!", lightLod);
			}
			defaultSpotLightCookie = Resources.Load<UnityEngine.Texture>("ShadowCaching/DefaultUnityPointLightCookie");
			if (defaultSpotLightCookie == null)
			{
				Debug.LogError("Failed to load default spot light cookie texture for shadow caching!", lightLod);
			}
			RebuildShadowMapPools(ConVar.Graphics.shadowlights);
			staticResourcesLoaded = true;
		}
		originalCookieTexture = light.cookie;
		if (originalCookieTexture == null)
		{
			originalCookieTexture = light.type switch
			{
				LightType.Spot => defaultSpotLightCookie, 
				LightType.Point => defaultWhiteCubemap, 
				_ => Texture2D.whiteTexture, 
			};
		}
		renderTexturePool = light.type switch
		{
			LightType.Spot => spotLightRtPool, 
			LightType.Point => pointLightRtPool, 
			_ => null, 
		};
		cachedShadowMap = renderTexturePool?.GetInstance();
		if (cachedShadowMap == null)
		{
			Debug.LogError("Failed to get cached shadow map render texture from pool! Initialization cancelled.", lightLod);
			return;
		}
		InitializeCommandBuffers();
		Refresh();
		SetEnabledFlag(enabled: true, light);
		isInitialized = true;
	}

	private void InitializeCommandBuffers()
	{
		shadowCopyCommandBuffer = new CommandBuffer();
		shadowCopyCommandBuffer.name = "Shadow Cache Copy";
		shadowCopyCommandBuffer.SetGlobalTexture(shadowMapTextureId, BuiltinRenderTextureType.CurrentActive);
		shadowCopyCommandBuffer.SetGlobalTexture(originalLightCookieId, originalCookieTexture);
		shadowCopyCommandBuffer.SetGlobalFloat(shadowNearPlaneId, light.shadowNearPlane);
		if (light.type == LightType.Point)
		{
			shadowCopyCommandBuffer.GetTemporaryRTArray(tempShadowMapCacheId, 256, 256, 6, 0, FilterMode.Point, GraphicsFormat.R32G32B32A32_SFloat);
			shadowCopyCommandBuffer.GetTemporaryRTArray(tempGaussianBlurId, 256, 256, 6, 0, FilterMode.Bilinear, GraphicsFormat.R32G32B32A32_SFloat);
			for (int i = 0; i < 6; i++)
			{
				shadowCopyCommandBuffer.SetGlobalInt(cubemapFaceId, i);
				shadowCopyCommandBuffer.Blit((UnityEngine.Texture)null, tempShadowMapCacheId, copyShadowMapMat, 0, i);
				shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, new Vector2(0.00390625f, 0f));
				shadowCopyCommandBuffer.Blit(tempShadowMapCacheId, tempGaussianBlurId, gaussianBlurMat, 0, i);
				shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, new Vector2(0f, 0.00390625f));
				shadowCopyCommandBuffer.Blit(tempGaussianBlurId, tempShadowMapCacheId, gaussianBlurMat, 0, i);
				shadowCopyCommandBuffer.CopyTexture(tempShadowMapCacheId, i, 0, cachedShadowMap, i, 0);
			}
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempShadowMapCacheId);
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempGaussianBlurId);
		}
		if (light.type == LightType.Spot)
		{
			shadowCopyCommandBuffer.GetTemporaryRT(tempShadowMapCacheId, 256, 256, 0, FilterMode.Point, GraphicsFormat.R32G32B32A32_SFloat);
			shadowCopyCommandBuffer.GetTemporaryRT(tempGaussianBlurId, 256, 256, 0, FilterMode.Point, GraphicsFormat.R32G32B32A32_SFloat);
			shadowCopyCommandBuffer.Blit(null, tempShadowMapCacheId, copyShadowMapMat, 1);
			shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, new Vector2(0.00390625f, 0f));
			shadowCopyCommandBuffer.Blit(tempShadowMapCacheId, tempGaussianBlurId, gaussianBlurMat, 1);
			shadowCopyCommandBuffer.SetGlobalVector(blurDirectionId, new Vector2(0f, 0.00390625f));
			shadowCopyCommandBuffer.Blit(tempGaussianBlurId, tempShadowMapCacheId, gaussianBlurMat, 1);
			shadowCopyCommandBuffer.CopyTexture(tempShadowMapCacheId, cachedShadowMap);
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempShadowMapCacheId);
			shadowCopyCommandBuffer.ReleaseTemporaryRT(tempGaussianBlurId);
		}
	}

	public void SetEnabledFlag(bool enabled, Light lightComponent)
	{
		if (!(lightComponent == null))
		{
			lightComponent.shadowStrength = (enabled ? 1f : 0f);
		}
	}

	public static void RebuildShadowMapPools(int maxShadowLights)
	{
		int capacity = Mathf.Max(0, maxShadowLights) * 3;
		int capacity2 = Mathf.Max(0, maxShadowLights);
		spotLightRtPool = new RenderTexturePool(256, 256, GraphicsFormat.R32G32B32A32_SFloat, TextureDimension.Tex2D, FilterMode.Bilinear, capacity);
		pointLightRtPool = new RenderTexturePool(256, 256, GraphicsFormat.R32G32B32A32_SFloat, TextureDimension.Cube, FilterMode.Bilinear, capacity2);
	}

	private bool HasLightMoved()
	{
		if (refreshDistanceDelta > 0f)
		{
			return (lightLod.transform.position - lastRefreshPosition).sqrMagnitude >= refreshDistanceDelta * refreshDistanceDelta;
		}
		return false;
	}

	private void Refresh()
	{
		if (!pendingCapture)
		{
			light.shadows = LightShadows.Soft;
			light.AddCommandBuffer(LightEvent.AfterShadowMap, shadowCopyCommandBuffer);
			pendingCapture = true;
			captureRoutine = lightLod.StartCoroutine(FinishCaptureAtEndOfFrame());
		}
	}

	private IEnumerator FinishCaptureAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		FinishCapture();
	}

	private void FinishCapture()
	{
		light.shadows = LightShadows.None;
		light.cookie = cachedShadowMap;
		if (shadowCopyCommandBuffer != null)
		{
			light.RemoveCommandBuffer(LightEvent.AfterShadowMap, shadowCopyCommandBuffer);
		}
		pendingCapture = false;
	}

	public void Release()
	{
		if (!isInitialized)
		{
			return;
		}
		isInitialized = false;
		pendingCapture = false;
		if (lightLod != null && captureRoutine != null)
		{
			lightLod.StopCoroutine(captureRoutine);
		}
		renderTexturePool.ReleaseInstance(cachedShadowMap);
		cachedShadowMap = null;
		if (light != null)
		{
			light.shadows = LightShadows.None;
			if (originalCookieTexture == defaultSpotLightCookie || originalCookieTexture == defaultWhiteCubemap)
			{
				originalCookieTexture = null;
			}
			light.cookie = originalCookieTexture;
			if (shadowCopyCommandBuffer != null)
			{
				light.RemoveCommandBuffer(LightEvent.AfterShadowMap, shadowCopyCommandBuffer);
			}
		}
		shadowCopyCommandBuffer?.Release();
		shadowCopyCommandBuffer = null;
		SetEnabledFlag(enabled: false, light);
	}
}

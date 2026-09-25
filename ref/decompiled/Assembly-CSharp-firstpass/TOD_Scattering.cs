using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[ImageEffectAllowedInSceneView]
[AddComponentMenu("Time of Day/Camera Atmospheric Scattering")]
public class TOD_Scattering : TOD_ImageEffect
{
	public Shader ScatteringShader;

	public Shader ScreenClearShader;

	public Shader SkyMaskShader;

	public Texture2D DitheringTexture;

	[Tooltip("Whether to render atmosphere and fog in a single pass or two separate passes. Disable when using anti-aliasing in forward rendering or when your manual reflection scripts need the sky dome to be present before the image effects are rendered.")]
	public bool SinglePass = true;

	[Header("Fog")]
	[Tooltip("How quickly the fog thickens with increasing distance.")]
	[Range(0f, 1f)]
	public float GlobalDensity = 0.01f;

	[Tooltip("How quickly the fog falls off with increasing altitude.")]
	[Range(0f, 1f)]
	public float HeightFalloff = 0.01f;

	[Tooltip("The distance the fog starts at.")]
	public float StartDistance;

	[Tooltip("The height where the fog reaches its maximum density.")]
	public float ZeroLevel;

	[Header("Blur")]
	[Tooltip("The scattering resolution.")]
	public ResolutionType Resolution = ResolutionType.Normal;

	[Tooltip("The number of blur iterations to be performed.")]
	[TOD_Range(0f, 4f)]
	public int BlurIterations = 2;

	[Tooltip("The radius to blur filter applied to the directional scattering.")]
	[TOD_Min(0f)]
	public float BlurRadius = 2f;

	[Tooltip("The maximum radius of the directional scattering.")]
	[TOD_Min(0f)]
	public float MaxRadius = 1f;

	private Material scatteringMaterial;

	private Material screenClearMaterial;

	private Material skyMaskMaterial;

	private RenderTexture skyMaskTexture;

	protected void OnEnable()
	{
		if (!ScatteringShader)
		{
			ScatteringShader = Shader.Find("Hidden/Time of Day/Scattering");
		}
		if (!ScreenClearShader)
		{
			ScreenClearShader = Shader.Find("Hidden/Time of Day/Screen Clear");
		}
		if (!SkyMaskShader)
		{
			SkyMaskShader = Shader.Find("Hidden/Time of Day/Sky Mask");
		}
		scatteringMaterial = CreateMaterial(ScatteringShader);
		screenClearMaterial = CreateMaterial(ScreenClearShader);
		skyMaskMaterial = CreateMaterial(SkyMaskShader);
	}

	protected void OnDisable()
	{
		if ((bool)scatteringMaterial)
		{
			Object.DestroyImmediate(scatteringMaterial);
		}
		if ((bool)screenClearMaterial)
		{
			Object.DestroyImmediate(screenClearMaterial);
		}
		if ((bool)skyMaskMaterial)
		{
			Object.DestroyImmediate(skyMaskMaterial);
		}
		if (skyMaskTexture != null)
		{
			skyMaskTexture.Release();
			skyMaskTexture = null;
		}
	}

	protected void OnPreCull()
	{
		if (SinglePass && (bool)sky && sky.Initialized)
		{
			sky.Components.AtmosphereRenderer.enabled = false;
		}
	}

	protected void OnPostRender()
	{
		if (SinglePass && (bool)sky && sky.Initialized)
		{
			sky.Components.AtmosphereRenderer.enabled = true;
		}
	}

	private void MakeSkyMaskTexture(RenderTexture source)
	{
		Vector3i skyMaskSize = GetSkyMaskSize(source, Resolution);
		int x = skyMaskSize.x;
		int y = skyMaskSize.y;
		int z = skyMaskSize.z;
		skyMaskTexture = new RenderTexture(x, y, z);
		skyMaskTexture.wrapMode = TextureWrapMode.Clamp;
		skyMaskTexture.filterMode = FilterMode.Bilinear;
		skyMaskTexture.Create();
		Shader.SetGlobalTexture("_TOD_SkyMask", skyMaskTexture);
	}

	private void ResizeSkyMask(RenderTexture source)
	{
		if (skyMaskTexture == null)
		{
			MakeSkyMaskTexture(source);
			return;
		}
		Vector3i skyMaskSize = GetSkyMaskSize(source, Resolution);
		if (skyMaskSize.x != skyMaskTexture.width || skyMaskSize.y != skyMaskTexture.height)
		{
			skyMaskTexture.Release();
			skyMaskTexture.width = skyMaskSize.x;
			skyMaskTexture.height = skyMaskSize.y;
			skyMaskTexture.Create();
		}
	}

	[ImageEffectOpaque]
	protected void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!CheckSupport(needDepth: true))
		{
			Graphics.Blit(source, destination);
			return;
		}
		ResizeSkyMask(source);
		sky.Components.Scattering = this;
		Vector3 lightPos = cam.WorldToViewportPoint(sky.Components.SunTransform.position);
		RenderTexture skyMask = GetSkyMask(source, skyMaskMaterial, screenClearMaterial, Resolution, lightPos, BlurIterations, BlurRadius, MaxRadius);
		scatteringMaterial.SetMatrix("_FrustumCornersWS", FrustumCorners());
		scatteringMaterial.SetTexture("_SkyMask", skyMask);
		if (SinglePass)
		{
			scatteringMaterial.EnableKeyword("TOD_SCATTERING_SINGLE_PASS");
		}
		else
		{
			scatteringMaterial.DisableKeyword("TOD_SCATTERING_SINGLE_PASS");
		}
		Shader.SetGlobalTexture("TOD_BayerTexture", DitheringTexture);
		Shader.SetGlobalVector("TOD_ScatterDensity", new Vector4(HeightFalloff, ZeroLevel, GlobalDensity, StartDistance));
		Graphics.Blit(skyMask, skyMaskTexture);
		Graphics.Blit(source, destination, scatteringMaterial);
		RenderTexture.ReleaseTemporary(skyMask);
	}
}

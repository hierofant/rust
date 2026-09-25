using AmplifyOcclusion;
using Rust.RenderPipeline.Runtime;
using UnityEngine;

[CreateAssetMenu(menuName = "Rendering/RustRendererFeatures/AmbientOcclusion")]
public class SSAORendererFeature : RustRendererFeature
{
	[Header("Ambient Occlusion")]
	[Tooltip("How the occlusion result is composited into the frame.")]
	public ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per occlusion pass.")]
	public SampleCountLevel SampleCount = SampleCountLevel.Medium;

	[Tooltip("Source used for per-pixel normals.")]
	public PerPixelNormalSource PerPixelNormals = PerPixelNormalSource.Camera;

	[Tooltip("Final applied intensity of the occlusion effect.")]
	[Range(0f, 1f)]
	public float Intensity = 1f;

	[Tooltip("Tint colour blended with the occlusion shadow.")]
	public Color Tint = Color.black;

	[Range(0f, 32f)]
	[Tooltip("World-space radius of the occlusion kernel.")]
	public float Radius = 2f;

	[Tooltip("Power exponent attenuation of the occlusion.")]
	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	[Tooltip("Initial occlusion contribution offset (reduces self-occlusion / acne).")]
	[Range(0f, 0.99f)]
	public float Bias = 0.05f;

	[Tooltip("Controls thickness-based occlusion contribution.")]
	[Range(0f, 1f)]
	public float Thickness = 1f;

	[Tooltip("Compute occlusion and blur at half resolution.")]
	public bool Downsample = true;

	[Header("Distance Fade")]
	[Tooltip("Fade the effect out at a distance.")]
	public bool FadeEnabled;

	[Tooltip("Distance (Unity units) where fading begins.")]
	public float FadeStart = 100f;

	[Tooltip("Length of the fade transition zone.")]
	public float FadeLength = 50f;

	[Range(0f, 1f)]
	public float FadeToIntensity;

	public Color FadeToTint = Color.black;

	[Range(0f, 32f)]
	public float FadeToRadius = 2f;

	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	[Range(0f, 1f)]
	public float FadeToThickness = 1f;

	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	[Tooltip("Blur kernel radius in screen pixels (1–4).")]
	[Range(1f, 4f)]
	public int BlurRadius = 3;

	[Tooltip("Number of blur passes.")]
	[Range(1f, 4f)]
	public int BlurPasses = 1;

	[Range(0f, 20f)]
	[Tooltip("0 = blurred / 1 = sharpened.")]
	public float BlurSharpness = 10f;

	[Header("Temporal Filter")]
	[Tooltip("Accumulate occlusion over multiple frames to reduce noise.")]
	public bool FilterEnabled = true;

	[Tooltip("Accumulation decay. 0 = fast update (more flicker). 1 = slow update (ghosting).")]
	[Range(0f, 1f)]
	public float FilterBlending = 0.5f;

	[Tooltip("Motion-discard sensitivity. 0 = reuse more. 1 = discard more.")]
	[Range(0f, 1f)]
	public float FilterResponse = 0.5f;

	[Header("Shaders")]
	public Shader occlusionShader;

	public Shader blurShader;

	public Shader occlusionApplyShader;

	public override void Create()
	{
	}

	public override void AddRenderPasses(RustRenderer renderer)
	{
	}
}

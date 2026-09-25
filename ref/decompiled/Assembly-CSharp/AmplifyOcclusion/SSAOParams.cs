using UnityEngine;

namespace AmplifyOcclusion;

public struct SSAOParams
{
	public ApplicationMethod applyMethod;

	public SampleCountLevel sampleCount;

	public PerPixelNormalSource perPixelNormals;

	public float intensity;

	public Color tint;

	public float radius;

	public float powerExponent;

	public float bias;

	public float thickness;

	public bool downsample;

	public bool fadeEnabled;

	public float fadeStart;

	public float fadeLength;

	public float fadeToIntensity;

	public Color fadeToTint;

	public float fadeToRadius;

	public float fadeToPowerExponent;

	public float fadeToThickness;

	public bool blurEnabled;

	public int blurRadius;

	public int blurPasses;

	public float blurSharpness;

	public bool filterEnabled;

	public float filterBlending;

	public float filterResponse;

	public bool useMotionVectors;

	public bool temporalDilation;

	public bool temporalDirections;

	public bool temporalOffsets;
}

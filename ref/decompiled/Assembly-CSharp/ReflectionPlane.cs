using UnityEngine;

public class ReflectionPlane : DecayEntity
{
	private static readonly int _ColorTexID = Shader.PropertyToID("_ColorTex");

	private static readonly int _DepthTexID = Shader.PropertyToID("_DepthTex");

	private static readonly int _ReflectionLerpID = Shader.PropertyToID("_ReflectionLerp");

	[Header("Reflection Plane")]
	public LayerMask layerMask = -1;

	public float nearClip;

	public float farClip;

	public Material reflectionMaterial;

	public Renderer reflectionRenderer;

	public float maxDistance;

	public float fadeTime = 0.25f;
}

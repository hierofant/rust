using Rust.RenderPipeline.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeCloudsCameraContext : RustRendererFeatureCameraContext
{
	public int currUpdate;

	public bool firstFrame = true;

	public Matrix4x4 lastViewProj = Matrix4x4.identity;

	public Matrix4x4 currViewProj = Matrix4x4.identity;

	public Matrix4x4 projMat = Matrix4x4.identity;

	public Matrix4x4 inverseViewMat = Matrix4x4.identity;

	public Matrix4x4 projToWorld = Matrix4x4.identity;

	public RenderTexture upscaledOutputRT;

	public RTHandle upscaledOutputRTHandle;

	public int outputSizeX;

	public int outputSizeY;

	public int lowResSizeX;

	public int lowResSizeY;

	public int sizeDivisor;
}
